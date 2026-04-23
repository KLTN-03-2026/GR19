using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.IO;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using AppCafebookApi.Services;
using CafebookModel.Utils;
using CafebookModel.Model.ModelApp.QuanLy;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLySachView : Page
    {
        //private static readonly HttpClient httpClient;
        private List<QuanLySachGridDto> _allSachList = new();
        private QuanLySachDetailDto? _selectedSach = null;
        private string? _currentAnhBiaFilePath = null;
        private bool _deleteImageRequest = false;

        // Lưu trữ danh mục để phục vụ Auto-suggest
        private List<QuanLySachFilterLookupDto> _lookupTacGia = new();
        private List<QuanLySachFilterLookupDto> _lookupTheLoai = new();
        private List<QuanLySachFilterLookupDto> _lookupNXB = new();

        //static QuanLySachView() { httpClient = new HttpClient { BaseAddress = new Uri(AppConfigManager.GetApiServerUrl() ?? "http://localhost") }; }

        public QuanLySachView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            bool hasAnyQuyen = AuthService.CoQuyen("FULL_QL", "QL_SACH", "QL_DANH_MUC_SACH", "QL_LICH_SU_THUE_SACH");
            if (!hasAnyQuyen)
            {
                MessageBox.Show("Bạn không có quyền truy cập module này!", "Từ chối");
                this.NavigationService?.GoBack();
                return;
            }

            ApplyPermissions();

            if (AuthService.CoQuyen("FULL_QL", "QL_SACH"))
            {
                await LoadLookupsAsync();
                await LoadSachAsync();
            }
        }

        private void ApplyPermissions()
        {
            bool hasQlSach = AuthService.CoQuyen("FULL_QL", "QL_SACH");
            if (FindName("GridDuLieuSach") is Grid g) g.Visibility = hasQlSach ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("txtThongBaoKhongCoQuyen") is Border b) b.Visibility = hasQlSach ? Visibility.Collapsed : Visibility.Visible;

            if (FindName("btnNavDanhMuc") is Button bNav1) bNav1.Visibility = AuthService.CoQuyen("FULL_QL", "QL_DANH_MUC_SACH") ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnNavLichSu") is Button bNav2) bNav2.Visibility = AuthService.CoQuyen("FULL_QL", "QL_LICH_SU_THUE_SACH") ? Visibility.Visible : Visibility.Collapsed;

            if (FindName("btnLamMoiForm") is Button b1) b1.Visibility = hasQlSach ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnLuu") is Button b2) b2.Visibility = hasQlSach ? Visibility.Visible : Visibility.Collapsed;
            if (FindName("btnXoa") is Button b3) b3.Visibility = hasQlSach ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task LoadLookupsAsync()
        {
            try
            {
                var tl = await ApiClient.Instance.GetFromJsonAsync<List<QuanLySachFilterLookupDto>>("api/app/quanly-sach/lookup/theloai");
                var tg = await ApiClient.Instance.GetFromJsonAsync<List<QuanLySachFilterLookupDto>>("api/app/quanly-sach/lookup/tacgia");
                var nxb = await ApiClient.Instance.GetFromJsonAsync<List<QuanLySachFilterLookupDto>>("api/app/quanly-sach/lookup/nxb");

                if (tl != null)
                {
                    _lookupTheLoai = tl;
                    if (FindName("cmbFilterTheLoai") is ComboBox c1) { var filterTl = new List<QuanLySachFilterLookupDto> { new QuanLySachFilterLookupDto { Ten = "Tất cả" } }; filterTl.AddRange(tl); c1.ItemsSource = filterTl; c1.SelectedIndex = 0; }
                }
                if (tg != null)
                {
                    _lookupTacGia = tg;
                    if (FindName("cmbFilterTacGia") is ComboBox c2) { var filterTg = new List<QuanLySachFilterLookupDto> { new QuanLySachFilterLookupDto { Ten = "Tất cả" } }; filterTg.AddRange(tg); c2.ItemsSource = filterTg; c2.SelectedIndex = 0; }
                }
                if (nxb != null) _lookupNXB = nxb;
            }
            catch { }
        }

        private async Task LoadSachAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var res = await ApiClient.Instance.GetFromJsonAsync<List<QuanLySachGridDto>>("api/app/quanly-sach");
                if (res != null) { _allSachList = res; FilterData(); }
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private void Filters_Changed(object sender, RoutedEventArgs e) => FilterData();
        private void Filters_Changed(object sender, TextChangedEventArgs e) => FilterData();
        private void Filters_Changed(object sender, SelectionChangedEventArgs e) => FilterData();

        private void FilterData()
        {
            if (!(FindName("dgSach") is DataGrid dg)) return;
            var query = _allSachList.AsEnumerable();

            string k = (FindName("txtSearch") as TextBox)?.Text.ToLower() ?? "";
            if (!string.IsNullOrEmpty(k)) query = query.Where(x => x.TenSach.ToLower().Contains(k) || x.TenTacGia.ToLower().Contains(k));

            string tl = (FindName("cmbFilterTheLoai") as ComboBox)?.SelectedValue?.ToString() ?? "Tất cả";
            if (tl != "Tất cả") query = query.Where(x => x.TenTheLoai.Contains(tl));

            string tg = (FindName("cmbFilterTacGia") as ComboBox)?.SelectedValue?.ToString() ?? "Tất cả";
            if (tg != "Tất cả") query = query.Where(x => x.TenTacGia.Contains(tg));

            dg.ItemsSource = query.ToList();
        }

        private async void DgSach_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgSach") is DataGrid dg && dg.SelectedItem is QuanLySachGridDto item)
            {
                if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
                if (FindName("lblFormTitle") is TextBlock title) title.Text = "Chi tiết Sách";

                if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
                try
                {
                    var detail = await ApiClient.Instance.GetFromJsonAsync<QuanLySachDetailDto>($"api/app/quanly-sach/{item.IdSach}");
                    if (detail != null)
                    {
                        _selectedSach = detail;
                        _currentAnhBiaFilePath = null;
                        _deleteImageRequest = false;

                        if (FindName("txtTenSach") is TextBox t1) t1.Text = detail.TenSach;
                        if (FindName("txtTacGiaList") is TextBox t2) t2.Text = detail.DanhSachTacGia + (string.IsNullOrEmpty(detail.DanhSachTacGia) ? "" : ", ");
                        if (FindName("txtTheLoaiList") is TextBox t3) t3.Text = detail.DanhSachTheLoai + (string.IsNullOrEmpty(detail.DanhSachTheLoai) ? "" : ", ");
                        if (FindName("txtNXBList") is TextBox t4) t4.Text = detail.DanhSachNhaXuatBan + (string.IsNullOrEmpty(detail.DanhSachNhaXuatBan) ? "" : ", ");
                        if (FindName("txtViTri") is TextBox tV) tV.Text = detail.ViTri;
                        if (FindName("txtNamXuatBan") is TextBox t5) t5.Text = detail.NamXuatBan?.ToString();
                        if (FindName("txtGiaBia") is TextBox t6) t6.Text = detail.GiaBia?.ToString("0.##");
                        if (FindName("txtSoLuongTong") is TextBox t8) t8.Text = detail.SoLuongTong.ToString();
                        if (FindName("txtMoTa") is TextBox t9) t9.Text = detail.MoTa;

                        LoadImagePreview(detail.AnhBia);
                    }
                }
                finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
            }
        }

        private void BtnLamMoiForm_Click(object sender, RoutedEventArgs e)
        {
            _selectedSach = null;
            if (FindName("dgSach") is DataGrid dg) dg.SelectedItem = null;
            if (FindName("formChiTiet") is StackPanel form) form.IsEnabled = true;
            if (FindName("lblFormTitle") is TextBlock title) title.Text = "Thêm Sách Mới";

            _currentAnhBiaFilePath = null;
            _deleteImageRequest = false;
            LoadImagePreview(null);

            if (FindName("txtTenSach") is TextBox t1) t1.Text = "";
            if (FindName("txtTacGiaList") is TextBox t2) t2.Text = "";
            if (FindName("txtTheLoaiList") is TextBox t3) t3.Text = "";
            if (FindName("txtNXBList") is TextBox t4) t4.Text = "";
            if (FindName("txtViTri") is TextBox tV) tV.Text = "";
            if (FindName("txtNamXuatBan") is TextBox t5) t5.Text = "";
            if (FindName("txtGiaBia") is TextBox t6) t6.Text = "";
            if (FindName("txtSoLuongTong") is TextBox t8) t8.Text = "1";
            if (FindName("txtMoTa") is TextBox t9) t9.Text = "";
        }

        private async void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            string tieuDe = (FindName("txtTenSach") as TextBox)?.Text.Trim() ?? "";
            if (string.IsNullOrEmpty(tieuDe)) { MessageBox.Show("Vui lòng nhập Tên sách!"); return; }

            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(tieuDe), "TenSach");
            formData.Add(new StringContent((FindName("txtTacGiaList") as TextBox)?.Text.Trim() ?? ""), "DanhSachTacGia");
            formData.Add(new StringContent((FindName("txtTheLoaiList") as TextBox)?.Text.Trim() ?? ""), "DanhSachTheLoai");
            formData.Add(new StringContent((FindName("txtNXBList") as TextBox)?.Text.Trim() ?? ""), "DanhSachNhaXuatBan");
            formData.Add(new StringContent((FindName("txtViTri") as TextBox)?.Text.Trim() ?? ""), "ViTri");
            formData.Add(new StringContent((FindName("txtNamXuatBan") as TextBox)?.Text.Trim() ?? ""), "NamXuatBan");
            formData.Add(new StringContent((FindName("txtGiaBia") as TextBox)?.Text.Trim() ?? "0"), "GiaBia");
            formData.Add(new StringContent((FindName("txtSoLuongTong") as TextBox)?.Text.Trim() ?? "0"), "SoLuongTong");
            formData.Add(new StringContent((FindName("txtMoTa") as TextBox)?.Text.Trim() ?? ""), "MoTa");
            formData.Add(new StringContent(_deleteImageRequest.ToString()), "XoaAnhBia");

            if (!string.IsNullOrEmpty(_currentAnhBiaFilePath))
            {
                var fileContent = new ByteArrayContent(File.ReadAllBytes(_currentAnhBiaFilePath));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                formData.Add(fileContent, "AnhBiaUpload", Path.GetFileName(_currentAnhBiaFilePath));
            }

            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                HttpResponseMessage res = _selectedSach == null
                    ? await ApiClient.Instance.PostAsync("api/app/quanly-sach", formData)
                    : await ApiClient.Instance.PutAsync($"api/app/quanly-sach/{_selectedSach.IdSach}", formData);

                if (res.IsSuccessStatusCode) { MessageBox.Show("Lưu sách thành công!"); await LoadLookupsAsync(); await LoadSachAsync(); }
                else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
            }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSach == null) return;
            if (MessageBox.Show($"Bạn chắc chắn xóa sách '{_selectedSach.TenSach}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
                try
                {
                    var res = await ApiClient.Instance.DeleteAsync($"api/app/quanly-sach/{_selectedSach.IdSach}");
                    if (res.IsSuccessStatusCode) { MessageBox.Show("Xóa sách thành công!"); BtnLamMoiForm_Click(this, new RoutedEventArgs()); await LoadSachAsync(); }
                    else MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
                }
                finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
            }
        }

        #region LOGIC TÌM KIẾM ĐỀ XUẤT (AUTO-SUGGEST) TRONG TEXTBOX
        private void TxtAutoSuggest_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox txt) return;
            string tag = txt.Tag?.ToString() ?? "";
            string text = txt.Text;

            // Tìm chuỗi gõ hiện tại (Sau dấu phẩy cuối cùng)
            int lastCommaIndex = text.LastIndexOf(',');
            string currentWord = (lastCommaIndex == -1) ? text : text.Substring(lastCommaIndex + 1);
            currentWord = currentWord.Trim().ToLower();

            Popup? pop = null;
            ListBox? lst = null;
            IEnumerable<QuanLySachFilterLookupDto>? source = null;

            if (tag == "TacGia")
            {
                pop = FindName("popTacGia") as Popup; lst = FindName("lstTacGia") as ListBox; source = _lookupTacGia;
            }
            else if (tag == "TheLoai")
            {
                pop = FindName("popTheLoai") as Popup; lst = FindName("lstTheLoai") as ListBox; source = _lookupTheLoai;
            }
            else if (tag == "NXB")
            {
                pop = FindName("popNXB") as Popup; lst = FindName("lstNXB") as ListBox; source = _lookupNXB;
            }

            if (pop == null || lst == null || source == null) return;

            if (string.IsNullOrEmpty(currentWord))
            {
                pop.IsOpen = false;
                return;
            }

            var matches = source.Where(x => x.Ten.ToLower().Contains(currentWord)).ToList();
            if (matches.Any())
            {
                lst.SelectionChanged -= LstSuggest_SelectionChanged; // Tắt tạm event để không loop
                lst.ItemsSource = matches;
                lst.SelectionChanged += LstSuggest_SelectionChanged;
                pop.IsOpen = true;
            }
            else
            {
                pop.IsOpen = false;
            }
        }

        private void LstSuggest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox lst || lst.SelectedItem is not QuanLySachFilterLookupDto selected) return;
            string tag = lst.Tag?.ToString() ?? "";

            TextBox? txt = null; Popup? pop = null;

            if (tag == "TacGia") { txt = FindName("txtTacGiaList") as TextBox; pop = FindName("popTacGia") as Popup; }
            else if (tag == "TheLoai") { txt = FindName("txtTheLoaiList") as TextBox; pop = FindName("popTheLoai") as Popup; }
            else if (tag == "NXB") { txt = FindName("txtNXBList") as TextBox; pop = FindName("popNXB") as Popup; }

            if (txt == null || pop == null) return;

            string text = txt.Text;
            int lastCommaIndex = text.LastIndexOf(',');

            if (lastCommaIndex == -1)
            {
                txt.Text = selected.Ten + ", ";
            }
            else
            {
                string prefix = text.Substring(0, lastCommaIndex + 1);
                txt.Text = prefix + " " + selected.Ten + ", ";
            }

            pop.IsOpen = false;
            lst.SelectedItem = null;

            // Đưa con trỏ nháy về cuối
            txt.Focus();
            txt.CaretIndex = txt.Text.Length;
        }
        #endregion

        #region XỬ LÝ ẢNH
        private void BorderAnhBia_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png" };
            if (ofd.ShowDialog() == true)
            {
                _currentAnhBiaFilePath = ofd.FileName;
                _deleteImageRequest = false;
                if (FindName("AvatarPreview") is Image img) img.Source = new BitmapImage(new Uri(_currentAnhBiaFilePath));
                if (FindName("txtUploadHint") is TextBlock txt) txt.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnXoaAnh_Click(object sender, RoutedEventArgs e)
        {
            _currentAnhBiaFilePath = null;
            _deleteImageRequest = true;
            LoadImagePreview(null);
        }

        private void LoadImagePreview(string? urlOrPath)
        {
            if (FindName("AvatarPreview") is Image img)
            {
                if (string.IsNullOrEmpty(urlOrPath)) { img.Source = null; if (FindName("txtUploadHint") is TextBlock txt) txt.Visibility = Visibility.Visible; }
                else
                {
                    string fullUrl = urlOrPath.StartsWith("http") ? urlOrPath : $"{(AppConfigManager.GetApiServerUrl() ?? "http://localhost").TrimEnd('/')}/{urlOrPath.TrimStart('/')}";
                    img.Source = HinhAnhHelper.LoadImage(fullUrl, HinhAnhPaths.DefaultBookCover);
                    if (FindName("txtUploadHint") is TextBlock txt) txt.Visibility = Visibility.Collapsed;
                }
            }
        }
        #endregion

        #region ĐIỀU HƯỚNG
        private void BtnNavDanhMuc_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "QL_DANH_MUC_SACH"))
            { MessageBox.Show("Không có quyền truy cập Danh mục sách!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            this.NavigationService?.Navigate(new QuanLyDanhMucSachView());
        }
        private void BtnNavLichSu_Click(object sender, RoutedEventArgs e)
        {
            if (!AuthService.CoQuyen("FULL_QL", "QL_LICH_SU_THUE_SACH"))
            { MessageBox.Show("Không có quyền truy cập lịch sử thuê sách", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            this.NavigationService?.Navigate(new QuanLyLichSuThueSachView());
        }
        #endregion
    }
}