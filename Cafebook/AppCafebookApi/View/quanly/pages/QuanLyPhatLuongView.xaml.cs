using AppCafebookApi.Services;
using AppCafebookApi.View.Common;
using CafebookModel.Model.ModelApp.QuanLy;
using CafebookModel.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AppCafebookApi.View.quanly.pages
{
    public partial class QuanLyPhatLuongView : Page
    {
        private bool _isDataLoaded = false;
        private int _currentSelectedId = 0;

        public QuanLyPhatLuongView() { InitializeComponent(); }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isDataLoaded) return;
            if (!string.IsNullOrEmpty(AuthService.AuthToken)) ApiClient.Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);

            if (!AuthService.CoQuyen("FULL_ADMIN", "FULL_QL", "QL_PHAT_LUONG"))
            {
                MessageBox.Show("Bạn không có quyền truy cập module Phát lương!", "Từ chối", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.NavigationService?.GoBack(); return;
            }
            await Task.Delay(350);
            if (!this.IsLoaded) return;
            try
            {
                if (FindName("cmbNam") is ComboBox cNam && FindName("cmbThang") is ComboBox cThang)
                {
                    int currentYear = DateTime.Now.Year;
                    for (int i = currentYear - 2; i <= currentYear + 1; i++) cNam.Items.Add(i);
                    for (int i = 1; i <= 12; i++) cThang.Items.Add(i);
                    cNam.SelectedItem = currentYear;
                    cThang.SelectedItem = DateTime.Now.Month;
                }
                await LoadDataAsync();
                _isDataLoaded = true;
            }
            catch (Exception ex) { Console.WriteLine($"Lỗi: {ex.Message}"); }
        }

        private async Task LoadDataAsync()
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                int nam = (FindName("cmbNam") as ComboBox)?.SelectedItem as int? ?? DateTime.Now.Year;
                int thang = (FindName("cmbThang") as ComboBox)?.SelectedItem as int? ?? DateTime.Now.Month;

                var res = await ApiClient.Instance.GetFromJsonAsync<List<PhatLuongGridDto>>($"api/app/phatluong/danhsach?nam={nam}&thang={thang}");
                if (res != null && FindName("dgPhieuLuong") is DataGrid dg) dg.ItemsSource = res;

                _currentSelectedId = 0;
                if (FindName("lblHuongDan") is TextBlock lbl) lbl.Visibility = Visibility.Visible;
                if (FindName("scrollChiTiet") is ScrollViewer scroll) scroll.Visibility = Visibility.Collapsed;
                if (FindName("panelAction") is StackPanel panel) panel.Visibility = Visibility.Collapsed;
            }
            catch { }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnLoc_Click(object sender, RoutedEventArgs e) => await LoadDataAsync();

        private async void DgPhieuLuong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FindName("dgPhieuLuong") is DataGrid dg && dg.SelectedItem is PhatLuongGridDto item)
            {
                _currentSelectedId = item.IdPhieuLuong;
                await LoadChiTietPhieuAsync(_currentSelectedId);
            }
        }

        private async Task LoadChiTietPhieuAsync(int idPhieuLuong)
        {
            if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
            try
            {
                var detail = await ApiClient.Instance.GetFromJsonAsync<PhatLuongDetailDto>($"api/app/phatluong/chitiet/{idPhieuLuong}");
                if (detail != null)
                {
                    if (FindName("lblTenQuan") is TextBlock tq) tq.Text = detail.TenQuan.ToUpper();
                    if (FindName("lblDiaChiQuan") is TextBlock tdc) { tdc.Text = string.IsNullOrEmpty(detail.DiaChiQuan) ? "" : $"Địa chỉ: {detail.DiaChiQuan}"; tdc.Visibility = string.IsNullOrEmpty(detail.DiaChiQuan) ? Visibility.Collapsed : Visibility.Visible; }
                    if (FindName("lblSdtQuan") is TextBlock tsdt) { tsdt.Text = string.IsNullOrEmpty(detail.SoDienThoaiQuan) ? "" : $"Điện thoại: {detail.SoDienThoaiQuan}"; tsdt.Visibility = string.IsNullOrEmpty(detail.SoDienThoaiQuan) ? Visibility.Collapsed : Visibility.Visible; }
                    if (FindName("lblKyLuong") is TextBlock t1) t1.Text = $"Kỳ Lương: {detail.KyLuong}";
                    if (FindName("lblTenNhanVien") is TextBlock t2) t2.Text = detail.TenNhanVien;
                    if (FindName("lblNgayChot") is TextBlock t3) t3.Text = detail.NgayChot.ToString("dd/MM/yyyy HH:mm");
                    if (FindName("lblLuongCoBan") is TextBlock t4) t4.Text = detail.LuongCoBan.ToString("N0") + " đ";
                    if (FindName("lblTongGioLam") is TextBlock t5) t5.Text = detail.TongGioLam.ToString("0.##") + " h";
                    if (FindName("lblLuongGoc") is TextBlock t6) t6.Text = detail.LuongGoc.ToString("N0") + " đ";

                    if (FindName("dgThuongPhatChiTiet") is DataGrid dgC) dgC.ItemsSource = detail.DanhSachThuongPhat;

                    if (FindName("lblTongThuong") is TextBlock t7) t7.Text = detail.TienThuong.ToString("N0") + " đ";
                    if (FindName("lblTongKhauTru") is TextBlock t8) t8.Text = detail.KhauTru.ToString("N0") + " đ";
                    if (FindName("lblThucLanh") is TextBlock t9) t9.Text = detail.ThucLanh.ToString("N0") + " đ";

                    if (FindName("lblTrangThai") is TextBlock t10)
                    {
                        if (detail.TrangThai == "Đã phát")
                        {
                            t10.Text = "Trạng thái: ĐÃ THANH TOÁN LƯƠNG";
                            t10.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80));
                            if (FindName("btnXacNhanPhat") is Button bX) bX.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            t10.Text = "Trạng thái: CHƯA PHÁT";
                            t10.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54));
                            if (FindName("btnXacNhanPhat") is Button bX) bX.Visibility = Visibility.Visible;
                        }
                    }

                    if (FindName("lblHuongDan") is TextBlock lbl) lbl.Visibility = Visibility.Collapsed;
                    if (FindName("scrollChiTiet") is ScrollViewer scroll) scroll.Visibility = Visibility.Visible;
                    if (FindName("panelAction") is StackPanel panel) panel.Visibility = Visibility.Visible;
                }
            }
            catch { MessageBox.Show("Lỗi tải chi tiết phiếu lương!"); }
            finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
        }

        private async void BtnXacNhanPhat_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSelectedId == 0) return;

            if (MessageBox.Show("Xác nhận đã thanh toán Lương bằng Tiền mặt / Chuyển khoản cho nhân viên này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                if (FindName("LoadingOverlay") is Border l) l.Visibility = Visibility.Visible;
                try
                {
                    var payload = new XacNhanPhatDto { IdNguoiPhat = AuthService.CurrentUser?.IdNhanVien ?? 1 };
                    HttpResponseMessage res = await ApiClient.Instance.PutAsJsonAsync($"api/app/phatluong/xacnhan/{_currentSelectedId}", payload);

                    if (res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Phát lương thành công! Hệ thống sẽ mở phiếu để bạn in.", "Hoàn tất");

                        int idPhieuCanIn = _currentSelectedId;

                        await LoadDataAsync();

                        var printWindow = new PhieuLuongPreviewWindow(idPhieuCanIn);

                        printWindow.Owner = Window.GetWindow(this);

                        printWindow.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show($"Lỗi: {await res.Content.ReadAsStringAsync()}");
                    }
                }
                finally { if (FindName("LoadingOverlay") is Border l2) l2.Visibility = Visibility.Collapsed; }
            }
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    if (FindName("printArea") is FrameworkElement pArea)
                    {
                        pArea.Effect = null;
                        Thickness oldMargin = pArea.Margin;
                        pArea.Margin = new Thickness(0);

                        Size printSize = new Size(pArea.ActualWidth, pArea.ActualHeight);
                        pArea.Measure(printSize);
                        pArea.Arrange(new Rect(new Point(0, 0), printSize));
                        pArea.UpdateLayout();

                        printDialog.PrintVisual(pArea, "In Phiếu Lương Cafebook");

                        pArea.Margin = oldMargin;
                        pArea.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        pArea.Arrange(new Rect(new Point(0, 0), pArea.DesiredSize));
                        pArea.UpdateLayout();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi in ấn: {ex.Message}\nVui lòng kiểm tra lại kết nối với máy in.");
            }
        }

        private void BtnQuayLai_Click(object sender, RoutedEventArgs e) => this.NavigationService?.GoBack();
    }
}