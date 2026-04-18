using System;
using System.Collections.Generic;

namespace CafebookModel.Model.Shared
{
    public class SharedThongBaoItemDto
    {
        public int IdThongBao { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public DateTime ThoiGianTao { get; set; }
        public string LoaiThongBao { get; set; } = string.Empty;
        public int? IdLienQuan { get; set; }
        public bool DaXem { get; set; }
        public string TenNhanVienTao { get; set; } = string.Empty;
    }

    public class SharedThongBaoResponseDto
    {
        public int UnreadCount { get; set; }
        public List<SharedThongBaoItemDto> Notifications { get; set; } = new();
    }
}