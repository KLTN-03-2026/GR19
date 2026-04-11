namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyDinhLuongSPDto { public int IdSanPham { get; set; } public string TenSanPham { get; set; } = string.Empty; public string TenDanhMuc { get; set; } = string.Empty; }
    public class QuanLyDinhLuongNLDto { public int IdNguyenLieu { get; set; } public string TenNguyenLieu { get; set; } = string.Empty; public decimal SoLuongSuDung { get; set; } public int IdDonViSuDung { get; set; } public string TenDonVi { get; set; } = string.Empty; }
    public class QuanLyDinhLuongSaveDto { public int IdNguyenLieu { get; set; } public decimal SoLuongSuDung { get; set; } public int IdDonViSuDung { get; set; } }
    public class LookupDinhLuongDto { public int Id { get; set; } public string Ten { get; set; } = string.Empty; }
}