using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyNhapKhoView : Page
    {
        //private static readonly HttpClient httpClient;
        private List<QuanLyNhapKhoGridDto> _phieuNhapList = new();
        private List<LookupNhapKhoDto> _nccList = new();
        private List<LookupNhapKhoDto> _nlList = new();
        private ObservableCollection<QuanLyChiTietNhapKhoDto> _chiTietList = new();

        private string? _fileDinhKemBase64 = null;
        private string? _tenFileDinhKem = null;
        private string? _urlFileDinhKemHienTai = null;

        private bool _isViewing = false;

        //static QuanLyNhapKhoView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLyNhapKhoView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("QL_NHAP_KHO")) { MessageBox.Show("Từ chối truy cập!"); this.NavigationService?.GoBack(); return; }

            ApplyPermissions();
            if (FindName("dgChiTiet") is DataGrid dg) dg.ItemsSource = _chiTietList;
            await LoadMasterDataAsync();
        }

        private void ApplyPermissions()
        {
            bool canEdit = AuthService.CoQuyen("QL_NHAP_KHO");
            if (FindName("btnLamMoiForm") is Button b1) b1.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnThemNL") is Button b3) b3.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadMasterDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var ncc = await ApiClient.Instance.GetFromJsonAsync<List<LookupNhapKhoDto>>("api/app/quanly-nhapkho/lookup-ncc");
                if (ncc != null && FindName("cmbNhaCungCap") is ComboBox cb1)
                {
                    _nccList = ncc; cb1.ItemsSource = _nccList;
                    // Set up filter for ComboBox
                    ICollectionView view1 = CollectionViewSource.GetDefaultView(cb1.ItemsSource);
                    view1.Filter = item => string.IsNullOrEmpty(cb1.Text) || ((LookupNhapKhoDto)item).Ten.IndexOf(cb1.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                var nl = await ApiClient.Instance.GetFromJsonAsync<List<LookupNhapKhoDto>>("api/app/quanly-nhapkho/lookup-nl");
                if (nl != null && FindName("cmbNguyenLieu") is ComboBox cb2)
                {
                    _nlList = nl; cb2.ItemsSource = _nlList;
                    // Set up filter for ComboBox
                    ICollectionView view2 = CollectionViewSource.GetDefaultView(cb2.ItemsSource);
                    view2.Filter = item => string.IsNullOrEmpty(cb2.Text) || ((LookupNhapKhoDto)item).Ten.IndexOf(cb2.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                await LoadPhieuNhapAsync();
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        // ==========================================
        // CÁC HÀM XỬ LÝ TÌM KIẾM TRONG COMBOBOX
        // ==========================================
        private void CmbNhaCungCap_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is ComboBox cmb)
            {
                CollectionViewSource.GetDefaultView(cmb.ItemsSource).Refresh();
                cmb.IsDropDownOpen = true;
            }
        }

        private void CmbNguyenLieu_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is ComboBox cmb)
            {
                CollectionViewSource.GetDefaultView(cmb.ItemsSource).Refresh();
                cmb.IsDropDownOpen = true;
            }
        }

        private async Task LoadPhieuNhapAsync()
        {
            try
            {
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLyNhapKhoGridDto>>("api/app/quanly-nhapkho");
                if (res != null) { _phieuNhapList = res; FilterData(); }
            }
            catch { }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => FilterData();

        private void FilterData()
        {
            if (!(FindName("dgPhieuNhap") is DataGrid dg)) return;
            string k = (FindName("txtSearch") as TextBox)?.Text.ToLower() ?? "";
            dg.ItemsSource = string.IsNullOrEmpty(k) ? _phieuNhapList : _phieuNhapList.Where(x => x.TenNhaCungCap.ToLower().Contains(k) || x.IdPhieuNhap.ToString().Contains(k)).ToList();
        }

        private async void DgPhieuNhap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgPhieuNhap") is DataGrid dg && dg.SelectedItem is QuanLyNhapKhoGridDto item)
            {
                _isViewing = true;
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblTitle") is TextBlock title) title.Text = $"Chi tiết Phiếu Nhập #{item.IdPhieuNhap}";

                if (FindName("btnLuu") is Button btnLuu) btnLuu.Visibility = Visibility.Collapsed;
                if (FindName("btnXuatFile") is Button btnXuat) btnXuat.Visibility = Visibility.Visible; // Hiển thị nút xuất

                if (FindName("btnThemNL") is Button btnA) btnA.IsEnabled = false;
                if (FindName("txtGiamGia") is TextBox txtG) txtG.IsReadOnly = true;

                try
                {
                    var detail = await ApiClient.Instance.GetFromJsonAsync<QuanLyNhapKhoDetailDto>($"api/app/quanly-nhapkho/{item.IdPhieuNhap}");
                    if (detail != null)
                    {
                        if (FindName("cmbNhaCungCap") is ComboBox c1) c1.SelectedValue = detail.IdNhaCungCap;
                        if (FindName("txtGhiChu") is TextBox t1) t1.Text = detail.GhiChu;
                        if (FindName("txtGiamGia") is TextBox t2) t2.Text = detail.GiamGia.ToString("N0");

                        _urlFileDinhKemHienTai = detail.HoaDonDinhKem;
                        if (!string.IsNullOrEmpty(_urlFileDinhKemHienTai))
                        {
                            if (FindName("txtTenFileDinhKem") is TextBlock txtF) txtF.Text = "Đã có chứng từ";
                            if (FindName("btnXemDinhKem") is Button btnX) btnX.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            if (FindName("txtTenFileDinhKem") is TextBlock txtF) txtF.Text = "Không có chứng từ";
                            if (FindName("btnXemDinhKem") is Button btnX) btnX.Visibility = Visibility.Collapsed;
                        }
                        if (FindName("btnDinhKem") is Button b) b.Visibility = Visibility.Collapsed; // Ẩn nút đính kèm khi xem

                        _chiTietList.Clear();
                        foreach (var ct in detail.ChiTiet) _chiTietList.Add(ct);
                        CalculateTotals();
                    }
                }
                catch { }
            }
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_NHAP_KHO")) return;
            _isViewing = false;
            _chiTietList.Clear();

            _fileDinhKemBase64 = null;
            _tenFileDinhKem = null;
            _urlFileDinhKemHienTai = null;

            if (FindName("dgPhieuNhap") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblTitle") is TextBlock title) title.Text = "Tạo Phiếu Nhập Mới";

            if (FindName("btnLuu") is Button btnLuu) btnLuu.Visibility = Visibility.Visible;
            if (FindName("btnXuatFile") is Button btnXuat) btnXuat.Visibility = Visibility.Collapsed; // Ẩn nút xuất

            if (FindName("btnThemNL") is Button btnA) btnA.IsEnabled = true;
            if (FindName("txtGiamGia") is TextBox txtG) { txtG.IsReadOnly = false; txtG.Text = "0"; }

            if (FindName("cmbNhaCungCap") is ComboBox c1) c1.SelectedItem = null;
            if (FindName("txtGhiChu") is TextBox t1) t1.Text = "";
            if (FindName("cmbNguyenLieu") is ComboBox c2) c2.SelectedItem = null;
            if (FindName("txtSoLuong") is TextBox t2) t2.Text = "";
            if (FindName("txtDonGia") is TextBox t3) t3.Text = "";

            if (FindName("txtTenFileDinhKem") is TextBlock txtTenFile) txtTenFile.Text = "Chưa có";
            if (FindName("btnDinhKem") is Button btnDk) btnDk.Visibility = Visibility.Visible;
            if (FindName("btnXemDinhKem") is Button btnXem) btnXem.Visibility = Visibility.Collapsed;

            CalculateTotals();
        }

        private void BtnThemNL_Click(object sender, RoutedEventArgs e)
        {
            if (_isViewing) return;
            int idNl = (FindName("cmbNguyenLieu") as ComboBox)?.SelectedValue as int? ?? 0;
            string tenNl = (FindName("cmbNguyenLieu") as ComboBox)?.Text ?? "";
            decimal.TryParse((FindName("txtSoLuong") as TextBox)?.Text, out decimal sl);
            decimal.TryParse((FindName("txtDonGia") as TextBox)?.Text, out decimal gia);

            if (idNl == 0 || sl <= 0 || gia < 0) { MessageBox.Show("Vui lòng nhập Nguyên liệu, Số lượng (>0) và Đơn giá hợp lệ."); return; }

            var exist = _chiTietList.FirstOrDefault(x => x.IdNguyenLieu == idNl);
            if (exist != null) { exist.SoLuong += sl; exist.DonGiaNhap = gia; }
            else _chiTietList.Add(new QuanLyChiTietNhapKhoDto { IdNguyenLieu = idNl, TenNguyenLieu = tenNl, SoLuong = sl, DonGiaNhap = gia });

            if (FindName("cmbNguyenLieu") is ComboBox c2) c2.SelectedItem = null;
            if (FindName("txtSoLuong") is TextBox t2) t2.Text = "";
            if (FindName("txtDonGia") is TextBox t3) t3.Text = "";

            if (FindName("dgChiTiet") is DataGrid dg) dg.Items.Refresh();
            CalculateTotals();
        }

        private void BtnXoaNL_Click(object sender, RoutedEventArgs e)
        {
            if (_isViewing) return;
            if (sender is Button btn && btn.DataContext is QuanLyChiTietNhapKhoDto item)
            {
                _chiTietList.Remove(item); CalculateTotals();
            }
        }

        private void TxtGiamGia_TextChanged(object sender, TextChangedEventArgs e) => CalculateTotals();

        private void CalculateTotals()
        {
            decimal tienHang = _chiTietList.Sum(x => x.ThanhTien);
            decimal.TryParse((FindName("txtGiamGia") as TextBox)?.Text, out decimal giamGia);
            decimal tongCong = tienHang - giamGia;

            if (FindName("txtTienHang") is TextBlock t1) t1.Text = tienHang.ToString("N0");
            if (FindName("txtTongCong") is TextBlock t2) t2.Text = tongCong.ToString("N0");
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("QL_NHAP_KHO") || _isViewing) return;
            if (!_chiTietList.Any()) { MessageBox.Show("Phiếu nhập chưa có nguyên liệu nào!"); return; }

            int idNcc = (FindName("cmbNhaCungCap") as ComboBox)?.SelectedValue as int? ?? 0;
            decimal.TryParse((FindName("txtGiamGia") as TextBox)?.Text, out decimal giamGia);

            var dto = new QuanLyNhapKhoSaveDto
            {
                IdNhaCungCap = idNcc,
                GiamGia = giamGia,
                GhiChu = (FindName("txtGhiChu") as TextBox)?.Text,
                FileDinhKemBase64 = _fileDinhKemBase64, // Gắn Base64
                TenFileDinhKem = _tenFileDinhKem,       // Gắn Tên File
                ChiTiet = _chiTietList.Select(x => new QuanLyChiTietNhapKhoSaveDto { IdNguyenLieu = x.IdNguyenLieu, SoLuong = x.SoLuong, DonGiaNhap = x.DonGiaNhap }).ToList()
            };

            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = await ApiClient.Instance.PostAsJsonAsync("api/app/quanly-nhapkho", dto);
                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Tạo phiếu nhập thành công và đã cập nhật tồn kho!");
                    BtnLamMoiForm_Click(this, new RoutedEventArgs());
                    await LoadPhieuNhapAsync();
                }
                else MessageBox.Show(await res.Content.ReadAsStringAsync());
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        // ==========================================
        // CÁC HÀM XUẤT FILE HOÁ ĐƠN (XML/PNG/JPG)
        // ==========================================
        private void BtnXuatFile_Click(object sender, RoutedEventArgs e)
        {
            if (FindName("formChiTiet") is not FrameworkElement targetElement) return;

            SaveFileDialog sfd = new SaveFileDialog
            {
                Title = "Lưu Phiếu Nhập Kho",
                Filter = "PNG Image|*.png|JPEG Image|*.jpg|XML File|*.xml",
                FileName = $"PhieuNhapKho_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (sfd.ShowDialog() == true)
            {
                string ext = Path.GetExtension(sfd.FileName).ToLower();
                try
                {
                    if (ext == ".xml")
                    {
                        ExportToXml(sfd.FileName);
                    }
                    else
                    {
                        // Ẩn tạm nút để hình ảnh hoá đơn đẹp hơn (không dính các nút bấm)
                        if (FindName("btnXuatFile") is Button btnXuat) btnXuat.Visibility = Visibility.Collapsed;

                        ExportToImage(targetElement, sfd.FileName, ext);

                        // Mở lại nút
                        if (FindName("btnXuatFile") is Button btnXuat2) btnXuat2.Visibility = Visibility.Visible;
                    }
                    MessageBox.Show("Lưu file phiếu nhập thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file: " + ex.Message);
                }
            }
        }

        private void ExportToImage(FrameworkElement element, string fileName, string extension)
        {
            double dpi = 96d;
            Rect bounds = VisualTreeHelper.GetDescendantBounds(element);

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, PixelFormats.Default);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(element);
                dc.DrawRectangle(new SolidColorBrush(Colors.White), null, new Rect(new Point(), bounds.Size)); // Background trắng
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }
            rtb.Render(dv);

            BitmapEncoder encoder = extension == ".jpg" ? new JpegBitmapEncoder() : new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                encoder.Save(fs);
            }
        }

        private void ExportToXml(string fileName)
        {
            var ncc = (FindName("cmbNhaCungCap") as ComboBox)?.Text ?? "Khách lẻ";
            var tienHang = (FindName("txtTienHang") as TextBlock)?.Text ?? "0";
            var giamGia = (FindName("txtGiamGia") as TextBox)?.Text ?? "0";
            var tongCong = (FindName("txtTongCong") as TextBlock)?.Text ?? "0";
            var idPhieu = (FindName("lblTitle") as TextBlock)?.Text ?? "";

            var xml = new System.Xml.Linq.XElement("PhieuNhapKho",
                new System.Xml.Linq.XElement("ThongTin",
                    new System.Xml.Linq.XAttribute("MaPhieu", idPhieu),
                    new System.Xml.Linq.XAttribute("NgayXuat", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
                ),
                new System.Xml.Linq.XElement("NhaCungCap", ncc),
                new System.Xml.Linq.XElement("ThanhToan",
                    new System.Xml.Linq.XElement("TienHang", tienHang),
                    new System.Xml.Linq.XElement("GiamGia", giamGia),
                    new System.Xml.Linq.XElement("TongTien", tongCong)
                ),
                new System.Xml.Linq.XElement("ChiTiet",
                    _chiTietList.Select(x => new System.Xml.Linq.XElement("NguyenLieu",
                        new System.Xml.Linq.XAttribute("Ten", x.TenNguyenLieu),
                        new System.Xml.Linq.XAttribute("SoLuong", x.SoLuong),
                        new System.Xml.Linq.XAttribute("DonGiaNhap", x.DonGiaNhap),
                        new System.Xml.Linq.XAttribute("ThanhTien", x.ThanhTien)
                    ))
                )
            );
            xml.Save(fileName);
        }

        private void BtnDinhKem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Chọn Hóa Đơn Nhà Cung Cấp",
                Filter = "Tất cả hỗ trợ|*.png;*.jpg;*.jpeg;*.xml|Hình ảnh|*.png;*.jpg;*.jpeg|XML File|*.xml",
                Multiselect = false
            };

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    FileInfo fi = new FileInfo(ofd.FileName);
                    if (fi.Length > 5 * 1024 * 1024) // Giới hạn file 5MB để tránh lag DB
                    {
                        MessageBox.Show("Vui lòng chọn file hóa đơn dưới 5MB!");
                        return;
                    }

                    _tenFileDinhKem = fi.Name;
                    byte[] fileBytes = File.ReadAllBytes(ofd.FileName);
                    _fileDinhKemBase64 = Convert.ToBase64String(fileBytes);

                    if (FindName("txtTenFileDinhKem") is TextBlock txtF) txtF.Text = _tenFileDinhKem;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể đọc file: " + ex.Message);
                }
            }
        }

        private void BtnXemDinhKem_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_urlFileDinhKemHienTai))
            {
                try
                {
                    // Mở link đính kèm bằng trình duyệt mặc định hoặc ứng dụng phù hợp của Windows
                    string fullUrl = $"{AppConfigManager.GetApiServerUrl()}{_urlFileDinhKemHienTai}";
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = fullUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể mở file đính kèm: " + ex.Message);
                }
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}