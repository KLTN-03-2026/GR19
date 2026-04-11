using System;
namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLySuCoBanDto
    {
        public int IdThongBao { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public DateTime ThoiGianTao { get; set; }
        public string TenNhanVienTao { get; set; } = string.Empty;
        public bool DaXem { get; set; }
    }
}