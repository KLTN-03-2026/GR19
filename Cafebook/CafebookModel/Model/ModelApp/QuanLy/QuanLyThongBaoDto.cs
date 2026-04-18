using System;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyThongBaoGridDto
    {
        public int IdThongBao { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public DateTime ThoiGianTao { get; set; }
        public string LoaiThongBao { get; set; } = string.Empty;
        public bool DaXem { get; set; }
        public string TenNhanVienTao { get; set; } = string.Empty;
        public bool IsSystemAlert => !(LoaiThongBao == "ThongBaoNhanVien" || LoaiThongBao == "ThongBaoQuanLy" || LoaiThongBao == "ThongBaoToanNhanVien");
    }

    public class QuanLyThongBaoSaveDto
    {
        public string NoiDung { get; set; } = string.Empty;
        public string LoaiThongBao { get; set; } = string.Empty;
        public int? IdNhanVienTao { get; set; }
        public bool DaXem { get; set; }
    }
}