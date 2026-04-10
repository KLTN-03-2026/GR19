using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CafebookModel.Model.ModelEntities
{
    [Table("CaLamViec")]
    public class CaLamViec
    {
        [Key]
        public int IdCa { get; set; }

        [Required]
        [StringLength(100)]
        public string TenCa { get; set; } = string.Empty;

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public virtual ICollection<LichLamViec> LichLamViecs { get; set; } = new List<LichLamViec>();

        // BỔ SUNG LIÊN KẾT ĐẾN BẢNG NHU CẦU
        public virtual ICollection<NhuCauCaLam> NhuCauCaLams { get; set; } = new List<NhuCauCaLam>();
    }
}