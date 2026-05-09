using AppCafebookApi.Services;
using CafebookModel.Model.ModelApp.NhanVien;
using System;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;

namespace AppCafebookApi.View.Common
{
    public partial class TemDanPreviewWindow : Window
    {
        private readonly CheBienItemDto _item;

        public TemDanPreviewWindow(CheBienItemDto item)
        {
            InitializeComponent();
            _item = item;
            LoadData(_item);
        }

        private void LoadData(CheBienItemDto item)
        {
            lblSoBan.Text = item.SoBan;
            lblThoiGian.Text = item.ThoiGianGoi.ToString("HH:mm");
            lblTenMon.Text = item.TenMon;
            lblSoLuong.Text = $"SL: {item.SoLuong}";

            if (!string.IsNullOrWhiteSpace(item.GhiChu))
            {
                lblGhiChu.Text = $"* {item.GhiChu}";
                lblGhiChu.Visibility = Visibility.Visible;
            }
            else
            {
                lblGhiChu.Visibility = Visibility.Collapsed;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = await ApiClient.Instance.GetFromJsonAsync<PhieuGoiMonPrintDto>($"api/app/nhanvien/goimon/print-data/{_item.IdHoaDon}");

                if (data != null && !string.IsNullOrEmpty(data.TenQuan))
                {
                    lblTenQuan.Text = data.TenQuan; 
                }
                else
                {
                    lblTenQuan.Text = "CAFEBOOK";
                }
            }
            catch
            {
                lblTenQuan.Text = "CAFEBOOK";
            }
        }

        private void BtnIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    btnIn.Visibility = Visibility.Collapsed;
                    btnDong.Visibility = Visibility.Collapsed;

                    printDialog.PrintVisual(PrintArea, $"Tem Dan - {lblTenMon.Text}");

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in tem: {ex.Message}", "Lỗi In", MessageBoxButton.OK, MessageBoxImage.Error);
                btnIn.Visibility = Visibility.Visible;
                btnDong.Visibility = Visibility.Visible;
            }
        }

        private void BtnDong_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}