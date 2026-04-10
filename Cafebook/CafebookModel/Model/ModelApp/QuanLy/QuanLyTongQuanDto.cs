using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyTongQuanDto
    {
        public decimal TongDoanhThuHomNay { get; set; }
        public int TongDonHangHomNay { get; set; }
        public string SanPhamBanChayHomNay { get; set; } = string.Empty;

        // Dữ liệu cho biểu đồ 30 ngày
        public List<QuanLyTongQuanChartPoint> DoanhThu30Ngay { get; set; } = new List<QuanLyTongQuanChartPoint>();
    }

    public class QuanLyTongQuanChartPoint
    {
        public DateTime Ngay { get; set; }
        public decimal TongTien { get; set; }
    }
}