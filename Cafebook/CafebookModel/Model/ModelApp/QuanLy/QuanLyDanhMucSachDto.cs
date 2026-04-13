using System.ComponentModel.DataAnnotations;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyDanhMucSachItemDto
    {
        public int Id { get; set; }
        public string Ten { get; set; } = string.Empty;
        public string? MoTa { get; set; }
    }

    public class QuanLyDanhMucSachSaveDto
    {
        [Required]
        public string Ten { get; set; } = string.Empty;
        public string? MoTa { get; set; }
    }
}