using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    // DTO hiển thị danh sách nhân viên bên cột trái
    public class PhanQuyen_NhanVienDto
    {
        public int IdNhanVien { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
    }

    // DTO hiển thị danh sách quyền bên cột phải
    public class PhanQuyen_QuyenDto
    {
        public string IdQuyen { get; set; } = string.Empty;
        public string TenQuyen { get; set; } = string.Empty;
        public string NhomQuyen { get; set; } = string.Empty;
    }

    // DTO dùng để gửi danh sách các quyền đã tick lên API để lưu
    public class PhanQuyen_SaveRequestDto
    {
        public List<string> SelectedQuyenIds { get; set; } = new List<string>();
    }
}