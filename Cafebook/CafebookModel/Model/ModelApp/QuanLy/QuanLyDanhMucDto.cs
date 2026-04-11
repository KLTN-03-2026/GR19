using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyDanhMucGridDto { public int IdDanhMuc { get; set; } public string TenDanhMuc { get; set; } = string.Empty; public int SoLuongSanPham { get; set; } }
    public class QuanLyDanhMucSaveDto { public string TenDanhMuc { get; set; } = string.Empty; }
}