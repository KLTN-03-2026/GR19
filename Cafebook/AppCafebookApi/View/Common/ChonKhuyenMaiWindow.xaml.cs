using CafebookModel.Model.ModelApp.NhanVien;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AppCafebookApi.Services; // Thêm để dùng AppConfigManager & AuthService

namespace AppCafebookApi.View.common
{
    public partial class ChonKhuyenMaiWindow : Window
    {
        private readonly int _idHoaDon;
        private readonly int? _currentSelectedId;
        private static readonly HttpClient _httpClient;
        private List<KhuyenMaiHienThiGoiMonDto> _allKms = new List<KhuyenMaiHienThiGoiMonDto>();

        public int? SelectedId { get; private set; }

        // ======================================================
        // NÂNG CẤP 1: DYNAMIC URL (Tuyệt đối không hardcode)
        // ======================================================
        static ChonKhuyenMaiWindow()
        {
            _httpClient = new HttpClient();
            string? apiUrl = AppConfigManager.GetApiServerUrl();
            if (!string.IsNullOrWhiteSpace(apiUrl))
            {
                _httpClient.BaseAddress = new Uri(apiUrl);
            }
        }

        public ChonKhuyenMaiWindow(int idHoaDon, int? currentSelectedId)
        {
            InitializeComponent();
            _idHoaDon = idHoaDon;
            _currentSelectedId = currentSelectedId;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Chặn crash nếu chưa có API
            if (_httpClient.BaseAddress == null)
            {
                MessageBox.Show("Hệ thống chưa được cấu hình URL Server.", "Thiếu cấu hình");
                this.Close();
                return;
            }

            // Gắn Token
            if (AuthService.CurrentUser != null && !string.IsNullOrEmpty(AuthService.AuthToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
            }

            this.IsEnabled = false;
            try
            {
                var response = await _httpClient.GetAsync($"api/app/nhanvien/goimon/khuyenmai-available/{_idHoaDon}");
                if (response.IsSuccessStatusCode)
                {
                    _allKms = await response.Content.ReadFromJsonAsync<List<KhuyenMaiHienThiGoiMonDto>>() ?? new List<KhuyenMaiHienThiGoiMonDto>();

                    _allKms.Insert(0, new KhuyenMaiHienThiGoiMonDto
                    {
                        IdKhuyenMai = 0,
                        TenChuongTrinh = "-- Không áp dụng --",
                        MaKhuyenMai = "",
                        DieuKienApDung = "Chọn mục này để gỡ bỏ khuyến mãi hiện tại.",
                        IsEligible = true
                    });

                    lvKhuyenMai.ItemsSource = _allKms;

                    if (_currentSelectedId.HasValue) lvKhuyenMai.SelectedValue = _currentSelectedId.Value;
                    else lvKhuyenMai.SelectedValue = 0;
                }
                else
                {
                    MessageBox.Show(await response.Content.ReadAsStringAsync(), "Lỗi tải Khuyến mãi");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi API");
                this.Close();
            }
            this.IsEnabled = true;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lvKhuyenMai == null || _allKms == null) return;

            string filter = txtSearch.Text.ToLower().Trim();
            if (string.IsNullOrEmpty(filter) || filter == "nhập mã hoặc tên km...")
            {
                lvKhuyenMai.ItemsSource = _allKms;
                return;
            }

            var filteredList = _allKms.Where(k =>
                k.TenChuongTrinh.ToLower().Contains(filter) ||
                (k.MaKhuyenMai ?? "").ToLower().Contains(filter) ||
                k.IdKhuyenMai == 0
            ).ToList();

            lvKhuyenMai.ItemsSource = filteredList;
        }

        private void BtnApDung_Click(object sender, RoutedEventArgs e)
        {
            if (lvKhuyenMai.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn một khuyến mãi.", "Chưa chọn");
                return;
            }

            var selectedKm = (KhuyenMaiHienThiGoiMonDto)lvKhuyenMai.SelectedItem;

            if (!selectedKm.IsEligible)
            {
                MessageBox.Show($"Không thể áp dụng khuyến mãi này.\nLý do: {selectedKm.IneligibilityReason}", "Không đủ điều kiện", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.SelectedId = selectedKm.IdKhuyenMai;
            this.DialogResult = true;
            this.Close();
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}