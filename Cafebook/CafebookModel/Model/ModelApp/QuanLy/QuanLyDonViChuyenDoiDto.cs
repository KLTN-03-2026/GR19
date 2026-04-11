using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyDonViChuyenDoiGridDto
    {
        public int IdChuyenDoi { get; set; }
        public int IdNguyenLieu { get; set; }
        public string TenNguyenLieu { get; set; } = string.Empty;
        public string TenDonVi { get; set; } = string.Empty;
        public decimal GiaTriQuyDoi { get; set; }
        public bool LaDonViCoBan { get; set; }
    }

    public class QuanLyDonViChuyenDoiSaveDto
    {
        public int IdNguyenLieu { get; set; }
        [Required] public string TenDonVi { get; set; } = string.Empty;
        public decimal GiaTriQuyDoi { get; set; }
        public bool LaDonViCoBan { get; set; }
    }

    public class LookupNguyenLieuDvtDto
    {
        public int Id { get; set; }
        public string Ten { get; set; } = string.Empty;
    }
}