// File: CafebookModel/Model/ModelApp/QuanLy/QuanLyTongQuanDto.cs
using System;
using System.Collections.Generic;

namespace CafebookModel.Model.ModelApp.QuanLy
{
    public class QuanLyTongQuanDto
    {
        public decimal TongDoanhThuHomNay { get; set; }
        public string SanPhamBanChayHomNay { get; set; } = string.Empty;
        public int SoLuongBanChayHomNay { get; set; }

        public List<QuanLyTongQuanChartPoint> DoanhThu30Ngay { get; set; } = new List<QuanLyTongQuanChartPoint>();

        public List<QuanLyTongQuanPieChartPoint> CoCauDoanhThu { get; set; } = new List<QuanLyTongQuanPieChartPoint>();

        public List<QuanLyTongQuanBarChartPoint> Top5SanPham { get; set; } = new List<QuanLyTongQuanBarChartPoint>();
    }

    public class QuanLyTongQuanChartPoint
    {
        public DateTime Ngay { get; set; }
        public decimal TongTien { get; set; }
    }

    public class QuanLyTongQuanPieChartPoint
    {
        public string TenDanhMuc { get; set; } = string.Empty;
        public decimal GiaTri { get; set; }
    }

    public class QuanLyTongQuanBarChartPoint
    {
        public string TenSanPham { get; set; } = string.Empty;
        public int SoLuong { get; set; }
    }
}