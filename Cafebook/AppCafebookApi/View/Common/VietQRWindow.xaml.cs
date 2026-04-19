using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AppCafebookApi.View.Common
{
    // ==========================================
    // 1. CÁC LỚP DTO CHỨA DỮ LIỆU TRẢ VỀ TỪ API
    // ==========================================
    public class VietQrResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public VietQrData? Data { get; set; }
    }

    public class VietQrData
    {
        [JsonPropertyName("qrCode")]
        public string QrCode { get; set; } = string.Empty;

        [JsonPropertyName("qrDataURL")]
        public string QrDataUrl { get; set; } = string.Empty;
    }

    // DTO cho API Danh sách ngân hàng
    public class BankListResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<BankData>? Data { get; set; }
    }

    public class BankData
    {
        [JsonPropertyName("bin")]
        public string Bin { get; set; } = string.Empty;

        [JsonPropertyName("shortName")]
        public string ShortName { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    // ==========================================
    // 2. LOGIC XỬ LÝ GIAO DIỆN VIETQR
    // ==========================================
    public partial class VietQRWindow : Window
    {
        private string _maNganHang;
        private string _soTaiKhoan;
        private string _chuTaiKhoan;
        private long _soTien;
        private string _noiDung;

        //private readonly string _clientId = "YOUR_CLIENT_ID_HERE";
        //private readonly string _apiKey = "YOUR_API_KEY_HERE";

        // Biến static để cache danh sách ngân hàng (tránh gọi API nhiều lần)
        private static Dictionary<string, string>? _cachedBanks = null;

        public VietQRWindow(string maNganHang, string soTaiKhoan, string chuTaiKhoan, decimal soTien, string noiDung)
        {
            InitializeComponent();

            _maNganHang = maNganHang;
            _soTaiKhoan = soTaiKhoan;
            _chuTaiKhoan = chuTaiKhoan;
            _soTien = Convert.ToInt64(soTien);
            _noiDung = RemoveDiacritics(noiDung);

            // Hiển thị tạm trong lúc chờ API tải tên ngân hàng
            lblNganHang.Text = $"Đang tải... ({_maNganHang})";
            lblSoTaiKhoan.Text = _soTaiKhoan;
            lblChuTaiKhoan.Text = _chuTaiKhoan.ToUpper();
            lblSoTien.Text = soTien.ToString("N0") + " đ";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Chạy song song 2 API: Lấy QR và Lấy Tên ngân hàng để tiết kiệm thời gian
            var taskGenerateQr = GenerateQrCodeAsync();
            var taskLoadBankName = LoadBankNameAsync();

            await Task.WhenAll(taskGenerateQr, taskLoadBankName);
        }

        private async Task LoadBankNameAsync()
        {
            string tenNganHang = await GetTenNganHangAsync(_maNganHang);
            lblNganHang.Text = $"{tenNganHang} ({_maNganHang})";
        }

        private async Task GenerateQrCodeAsync()
        {
            try
            {
                var requestData = new
                {
                    accountNo = _soTaiKhoan,
                    accountName = _chuTaiKhoan.ToUpper(),
                    acqId = Convert.ToInt32(_maNganHang),
                    amount = _soTien,
                    addInfo = _noiDung,
                    format = "text",
                    template = "qr_only" // Trả về ảnh QR cắt sát lề
                };

                using var client = new HttpClient();

                // client.DefaultRequestHeaders.Add("x-client-id", _clientId);
                // client.DefaultRequestHeaders.Add("x-api-key", _apiKey);

                var response = await client.PostAsJsonAsync("https://api.vietqr.io/v2/generate", requestData);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<VietQrResponse>();

                    if (result != null && result.Code == "00" && result.Data != null)
                    {
                        txtLoading.Visibility = Visibility.Collapsed;

                        string base64Data = result.Data.QrDataUrl;
                        if (base64Data.Contains(","))
                        {
                            base64Data = base64Data.Split(',')[1];
                        }

                        byte[] imageBytes = Convert.FromBase64String(base64Data);
                        using (var ms = new MemoryStream(imageBytes))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = ms;
                            bitmap.EndInit();

                            imgQR.Source = bitmap;
                        }
                    }
                    else
                    {
                        txtLoading.Text = "Lỗi: " + (result?.Desc ?? "Dữ liệu trả về trống");
                        txtLoading.Foreground = System.Windows.Media.Brushes.Red;
                    }
                }
                else
                {
                    txtLoading.Text = $"Lỗi kết nối API: {response.StatusCode}";
                    txtLoading.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                txtLoading.Text = "Lỗi hệ thống: " + ex.Message;
                txtLoading.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void BtnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        // ==========================================
        // HÀM HỖ TRỢ: Lấy tên ngân hàng động qua API
        // ==========================================
        private async Task<string> GetTenNganHangAsync(string bin)
        {
            // Nếu chưa có cache, gọi API lấy danh sách
            if (_cachedBanks == null)
            {
                try
                {
                    using var client = new HttpClient();
                    var response = await client.GetFromJsonAsync<BankListResponse>("https://api.vietqr.io/v2/banks");

                    if (response != null && response.Code == "00" && response.Data != null)
                    {
                        _cachedBanks = new Dictionary<string, string>();
                        foreach (var bank in response.Data)
                        {
                            if (!_cachedBanks.ContainsKey(bank.Bin))
                            {
                                _cachedBanks[bank.Bin] = bank.ShortName;
                            }
                        }
                    }
                }
                catch
                {
                    return "Ngân hàng (Chưa tải được tên)";
                }
            }

            // Nếu danh sách đã được tải, tìm tên trong từ điển cache
            if (_cachedBanks != null && _cachedBanks.TryGetValue(bin, out string? bankName))
            {
                return bankName;
            }

            return "Ngân hàng khác";
        }

        // ==========================================
        // HÀM HỖ TRỢ: Xóa dấu Tiếng Việt
        // ==========================================
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            text = text.ToLower();
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ", "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ", "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ", "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ", "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ", "ÍÌỊỈĨ",
                "đ", "Đ",
                "ýỳỵỷỹ", "ÝỲỴỶỸ"
            };
            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                    text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
            }
            return text;
        }
    }
}