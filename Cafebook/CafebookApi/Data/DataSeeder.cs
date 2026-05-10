using CafebookModel.Model.ModelEntities;
using Microsoft.EntityFrameworkCore;

namespace CafebookApi.Data
{
    public static class DataSeeder
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CafebookDbContext>();

            // (Tùy chọn) Đảm bảo database đã được tạo và apply các migration mới nhất
            await context.Database.MigrateAsync();

            // ==========================================
            // 1. SEED BẢNG VAI TRÒ
            // ==========================================
            if (!await context.VaiTros.AnyAsync())
            {
                context.VaiTros.AddRange(
                    new VaiTro { TenVaiTro = "Quản lý", MoTa = "Quản lý toàn bộ hệ thống" },
                    new VaiTro { TenVaiTro = "Nhân viên", MoTa = "Nhân viên thao tác nghiệp vụ" }
                );
                await context.SaveChangesAsync();
            }

            // ==========================================
            // 2. SEED BẢNG QUYỀN
            // ==========================================
            if (!await context.Quyens.AnyAsync())
            {
                context.Quyens.AddRange(
                    new Quyen { IdQuyen = "CM_CAI_DAT", TenQuyen = "Cài đặt phần mềm", NhomQuyen = "Hệ thống" },
                    new Quyen { IdQuyen = "CM_NHAT_KY_HE_THONG", TenQuyen = "Quản lý nhật ký hệ thống", NhomQuyen = "Hệ thống" },
                    new Quyen { IdQuyen = "CM_THONG_BAO", TenQuyen = "Xem thông báo hệ thống", NhomQuyen = "Hệ thống" },
                    new Quyen { IdQuyen = "FULL_ADMIN", TenQuyen = "Toàn quyền Hệ Thống", NhomQuyen = "Hệ thống" },
                    new Quyen { IdQuyen = "FULL_NV", TenQuyen = "Toàn quyền Nhân viên", NhomQuyen = "Hệ thống" },
                    new Quyen { IdQuyen = "FULL_QL", TenQuyen = "Toàn quyền Quản lý", NhomQuyen = "Hệ thống" },
                    new Quyen { IdQuyen = "NV_CHAM_CONG", TenQuyen = "Chấm công vào/ra ca", NhomQuyen = "Cá nhân" },
                    new Quyen { IdQuyen = "NV_CHE_BIEN", TenQuyen = "Màn hình Bếp/Pha chế", NhomQuyen = "Vận hành POS" },
                    new Quyen { IdQuyen = "NV_DANG_KY_CA_LAM", TenQuyen = "Đăng Ký Ca Làm", NhomQuyen = "Chức năng web" },
                    new Quyen { IdQuyen = "NV_DAT_BAN", TenQuyen = "Xử lý Đặt bàn", NhomQuyen = "Vận hành POS" },
                    new Quyen { IdQuyen = "NV_GIAO_HANG", TenQuyen = "Màn hình Giao hàng", NhomQuyen = "Vận hành POS" },
                    new Quyen { IdQuyen = "NV_GOI_MON", TenQuyen = "Order & Gọi món", NhomQuyen = "Vận hành POS" },
                    new Quyen { IdQuyen = "NV_HO_TRO_KH", TenQuyen = "Hỗ trợ khách hàng", NhomQuyen = "Chức năng web" },
                    new Quyen { IdQuyen = "NV_LICH_LAM_VIEC", TenQuyen = "Xem Lịch làm việc cá nhân", NhomQuyen = "Cá nhân" },
                    new Quyen { IdQuyen = "NV_PHAN_HOI", TenQuyen = "Phản hồi Góp ý & Đánh giá", NhomQuyen = "Chức năng web" },
                    new Quyen { IdQuyen = "NV_PHIEU_LUONG", TenQuyen = "Xem Phiếu lương cá nhân", NhomQuyen = "Cá nhân" },
                    new Quyen { IdQuyen = "NV_SHIP_HANG", TenQuyen = "Giao hàng đến khách", NhomQuyen = "Chức năng web" },
                    new Quyen { IdQuyen = "NV_SO_DO_BAN", TenQuyen = "Truy cập Sơ đồ bàn", NhomQuyen = "Vận hành POS" },
                    new Quyen { IdQuyen = "NV_THANH_TOAN", TenQuyen = "Thanh toán hóa đơn", NhomQuyen = "Vận hành POS" },
                    new Quyen { IdQuyen = "NV_THONG_TIN", TenQuyen = "Xem Thông tin cá nhân", NhomQuyen = "Cá nhân" },
                    new Quyen { IdQuyen = "NV_THUE_SACH", TenQuyen = "Xử lý Thuê/Trả sách", NhomQuyen = "Vận hành POS" },
                    new Quyen { IdQuyen = "QL_BAN", TenQuyen = "Quản lý thêm sửa xóa bàn", NhomQuyen = "Quản lý Bàn" },
                    new Quyen { IdQuyen = "QL_BAO_CAO_DOANH_THU", TenQuyen = "Xem Báo cáo Doanh thu", NhomQuyen = "Tổng quan & Báo cáo" },
                    new Quyen { IdQuyen = "QL_BAO_CAO_HIEU_SUAT_NHAN_SU", TenQuyen = "Xem Báo cáo Hiệu suất KPI", NhomQuyen = "Tổng quan & Báo cáo" },
                    new Quyen { IdQuyen = "QL_BAO_CAO_NHAN_SU", TenQuyen = "Xem Báo cáo Nhân sự", NhomQuyen = "Tổng quan & Báo cáo" },
                    new Quyen { IdQuyen = "QL_BAO_CAO_TON_KHO_NL", TenQuyen = "Xem Báo cáo Kho Nguyên liệu", NhomQuyen = "Tổng quan & Báo cáo" },
                    new Quyen { IdQuyen = "QL_BAO_CAO_TON_KHO_SACH", TenQuyen = "Xem Báo cáo Tồn kho Sách", NhomQuyen = "Tổng quan & Báo cáo" },
                    new Quyen { IdQuyen = "QL_CHAM_CONG", TenQuyen = "Quản lý chấm công nhân sự", NhomQuyen = "Quản lý lương" },
                    new Quyen { IdQuyen = "QL_DANH_MUC", TenQuyen = "Quản lý Danh mục sản phẩm", NhomQuyen = "Quản lý Sản phẩm" },
                    new Quyen { IdQuyen = "QL_DANH_MUC_SACH", TenQuyen = "Quản lý danh mục sách", NhomQuyen = "Quản lý Thư viện" },
                    new Quyen { IdQuyen = "QL_DE_XUAT", TenQuyen = "Quản lý đề xuất Sách & sản phẩm", NhomQuyen = "Hệ thống" },
                    new Quyen { IdQuyen = "QL_DINH_LUONG", TenQuyen = "Quản lý Định lượng nguyên liệu sản phẩm", NhomQuyen = "Quản lý Sản phẩm" },
                    new Quyen { IdQuyen = "QL_DON_HANG", TenQuyen = "Quản lý Đơn hàng", NhomQuyen = "Tài chính & Giao dịch" },
                    new Quyen { IdQuyen = "QL_DON_VI_CHUYEN_DOI", TenQuyen = "Quản lý Kho Đơn vị chuyển đổi", NhomQuyen = "Quản lý Kho" },
                    new Quyen { IdQuyen = "QL_DON_XIN_NGHI", TenQuyen = "Quản lý Đơn xin nghỉ", NhomQuyen = "Quản lý Nhân sự" },
                    new Quyen { IdQuyen = "QL_KHACH_HANG", TenQuyen = "Quản lý Khách hàng", NhomQuyen = "Quản lý Khách hàng KM" },
                    new Quyen { IdQuyen = "QL_KHU_VUC", TenQuyen = "Quản lý Khu vực bàn", NhomQuyen = "Quản lý Bàn" },
                    new Quyen { IdQuyen = "QL_KHUYEN_MAI", TenQuyen = "Quản lý Khuyến mãi", NhomQuyen = "Quản lý Khách hàng KM" },
                    new Quyen { IdQuyen = "QL_KIEM_KHO", TenQuyen = "Quản lý Kiểm kho", NhomQuyen = "Quản lý Kho" },
                    new Quyen { IdQuyen = "QL_LICH_LAM_VIEC", TenQuyen = "Quản lý lịch làm việc", NhomQuyen = "Quản lý Nhân sự" },
                    new Quyen { IdQuyen = "QL_LICH_SU_THUE_SACH", TenQuyen = "Quản lý lịch sử thuê sách", NhomQuyen = "Quản lý Thư viện" },
                    new Quyen { IdQuyen = "QL_LUONG", TenQuyen = "Quản lý Bảng lương", NhomQuyen = "Quản lý lương" },
                    new Quyen { IdQuyen = "QL_NGUOI_GIAO_HANG", TenQuyen = "Quản lý đơn vị vận chuyển", NhomQuyen = "Tài chính & Giao dịch" },
                    new Quyen { IdQuyen = "QL_NGUYEN_LIEU", TenQuyen = "Quản lý Kho Nguyên liệu", NhomQuyen = "Quản lý Kho" },
                    new Quyen { IdQuyen = "QL_NHA_CUNG_CAP", TenQuyen = "Quản lý Nhà cung cấp", NhomQuyen = "Quản lý Kho" },
                    new Quyen { IdQuyen = "QL_NHAN_VIEN", TenQuyen = "Quản lý Danh sách Nhân viên", NhomQuyen = "Quản lý Nhân sự" },
                    new Quyen { IdQuyen = "QL_NHAP_KHO", TenQuyen = "Quản lý Nhập kho", NhomQuyen = "Quản lý Kho" },
                    new Quyen { IdQuyen = "QL_PHAN_QUYEN", TenQuyen = "Quản lý Phân quyền", NhomQuyen = "Quản lý Nhân sự" },
                    new Quyen { IdQuyen = "QL_PHAT_LUONG", TenQuyen = "Quản lý Phát lương", NhomQuyen = "Quản lý lương" },
                    new Quyen { IdQuyen = "QL_PHU_THU", TenQuyen = "Quản lý Phụ thu", NhomQuyen = "Tài chính & Giao dịch" },
                    new Quyen { IdQuyen = "QL_SACH", TenQuyen = "Quản lý Thư viện Sách", NhomQuyen = "Quản lý Thư viện" },
                    new Quyen { IdQuyen = "QL_SAN_PHAM", TenQuyen = "Quản lý Sản phẩm CURD", NhomQuyen = "Quản lý Sản phẩm" },
                    new Quyen { IdQuyen = "QL_SU_CO_BAN", TenQuyen = "Quản lý Sự cố bàn", NhomQuyen = "Quản lý Bàn" },
                    new Quyen { IdQuyen = "QL_THONG_BAO", TenQuyen = "Quản lý thông báo", NhomQuyen = "Hệ thống" },
                    new Quyen { IdQuyen = "QL_THUONG_PHAT", TenQuyen = "Quản lý Thưởng phạt", NhomQuyen = "Quản lý lương" },
                    new Quyen { IdQuyen = "QL_TON_KHO", TenQuyen = "Xem Tồn Kho/ cảnh báo", NhomQuyen = "Quản lý Kho" },
                    new Quyen { IdQuyen = "QL_TONG_QUAN", TenQuyen = "Xem Dashboard Tổng quan", NhomQuyen = "Tổng quan & Báo cáo" },
                    new Quyen { IdQuyen = "QL_XUAT_HUY", TenQuyen = "Quản lý Xuất hủy", NhomQuyen = "Quản lý Kho" }
                );
                await context.SaveChangesAsync();
            }

            // ==========================================
            // 3. SEED CÀI ĐẶT
            // ==========================================
            var defaultSettings = new List<CaiDat>
            {
                new CaiDat { TenCaiDat = "AI_Chat_API_Key", GiaTri = "lm-studio", MoTa = "API Key cho dịch vụ (OpenAI, Gemini...)" },
                new CaiDat { TenCaiDat = "AI_Chat_API_model", GiaTri = "google/gemma-4-e4b", MoTa = "Tên Model AI đang sử dụng" },
                new CaiDat { TenCaiDat = "AI_Chat_Endpoint", GiaTri = "http://127.0.0.1:1234/v1/chat/completions", MoTa = "Endpoint của dịch vụ AI Chat" },
                new CaiDat { TenCaiDat = "DiemTichLuy_DoiVND", GiaTri = "1000", MoTa = "1 điểm tích lũy bằng ... VND trừ vào hóa đơn" },
                new CaiDat { TenCaiDat = "DiemTichLuy_NhanVND", GiaTri = "50000", MoTa = "Mỗi ... VND trong hóa đơn được 1 điểm" },
                new CaiDat { TenCaiDat = "HR_ChuyenCan_SoGio", GiaTri = "400", MoTa = "Số giờ công tối thiểu trong tháng yêu cầu để đạt thưởng chuyên cần." },
                new CaiDat { TenCaiDat = "HR_ChuyenCan_TienThuong", GiaTri = "100000", MoTa = "Số tiền thưởng (VND) khi đạt chuyên cần." },
                new CaiDat { TenCaiDat = "HR_HeSoOT", GiaTri = "1", MoTa = "Hệ số lương khi làm tăng ca (Overtime) (ví dụ: 1.5)" },
                new CaiDat { TenCaiDat = "HR_PhatDiTre_Phut", GiaTri = "10", MoTa = "Số phút cho phép đi trễ. Vượt quá ngưỡng này bắt đầu tính phạt." },
                new CaiDat { TenCaiDat = "HR_PhatDiTreMoiLan", GiaTri = "5000", MoTa = "Tiền phạt đi trễ mỗi một lần" },
                new CaiDat { TenCaiDat = "HR_PhatRaSom_Phut", GiaTri = "10", MoTa = "Số phút cho phép ra ca sớm Vượt quá ngưỡng này bắt đầu tính phạt." },
                new CaiDat { TenCaiDat = "HR_PhatVeSomMoiLan", GiaTri = "6000", MoTa = "Tiền phạt về sóm mỗi một lần" },
                new CaiDat { TenCaiDat = "HR_TinhTangCa_Phut", GiaTri = "60", MoTa = "Số phút cho phép được tính là tăng ca của mỗi một ca" },
                new CaiDat { TenCaiDat = "HR_VaoCaSom_Phut", GiaTri = "30", MoTa = "Cho phép nhân viên vào ca sớm Phút" },
                new CaiDat { TenCaiDat = "LienHe_Email", GiaTri = "cafebook.hotro@gmail.com", MoTa = "Gmail của quán" },
                new CaiDat { TenCaiDat = "LienHe_Facebook", GiaTri = "https://www.facebook.com/lamtoan24/", MoTa = "Link Facebook quán" },
                new CaiDat { TenCaiDat = "LienHe_GoogleMapsEmbed", GiaTri = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3919.106598502801!2d106.7010418153489!3d10.80311546168051!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x317528a459b2184f%3A0x805d52140130f4d3!2zVHLGsOG7nW5nIMSQ4bqhaSBo4buNYyBIw7luZyBCw6BuZw!5e0!3m2!1svi!2s!4v1678888888888!5m2!1svi!2s", MoTa = "Link GoogleMapsEmbed Quán" },
                new CaiDat { TenCaiDat = "LienHe_Instagram", GiaTri = "https://instagram.com/lamtoan24", MoTa = "Link Instagram quán" },
                new CaiDat { TenCaiDat = "LienHe_Website", GiaTri = "https://cafebook.shushushu.id.vn", MoTa = "Website quán" },
                new CaiDat { TenCaiDat = "LienHe_X", GiaTri = "https://x.com/", MoTa = "Link X quán" },
                new CaiDat { TenCaiDat = "LienHe_Youtube", GiaTri = "https://www.youtube.com/@Shu.otaku.t", MoTa = "Link Youtube quán" },
                new CaiDat { TenCaiDat = "LienHe_Zalo", GiaTri = "https://id.zalo.me/account?continue=https%3A%2F%2Fchat.zalo.me%2F", MoTa = "Zalo Quán" },
                new CaiDat { TenCaiDat = "NganHang_ChuTaiKhoan", GiaTri = "Lam Chu Bao Toan", MoTa = "Tên chủ tài khoản ngân hàng" },
                new CaiDat { TenCaiDat = "NganHang_MaDinhDanhNganHang", GiaTri = "970422", MoTa = "Mã định Danh của ngân hàng thụ hưởng" },
                new CaiDat { TenCaiDat = "NganHang_SoTaiKhoan", GiaTri = "0376512695", MoTa = "Số tài khoản ngân hàng " },
                new CaiDat { TenCaiDat = "Sach_DiemPhieuThue", GiaTri = "1", MoTa = "Diểm nhận được trên 1 phiếu trả sách." },
                new CaiDat { TenCaiDat = "Sach_PhiThue", GiaTri = "10000", MoTa = "Phí dịch vụ thuê sách được trừ sau khi trả sách" },
                new CaiDat { TenCaiDat = "Sach_PhiTraTreMoiNgay", GiaTri = "5000", MoTa = "Số tiền (VND) phạt nếu khách trả sách trễ 1 ngày" },
                new CaiDat { TenCaiDat = "Sach_SoNgayMuonToiDa", GiaTri = "15", MoTa = "Số Ngày Mượn sách tối đa" },
                new CaiDat { TenCaiDat = "Smtp_EnableSsl", GiaTri = "true", MoTa = "Bật bảo mật SSL/TLS (true/false)" },
                new CaiDat { TenCaiDat = "Smtp_FromName", GiaTri = "Cafebook Hỗ Trợ", MoTa = "Tên người gửi hiển thị trong email khách hàng nhận" },
                new CaiDat { TenCaiDat = "Smtp_Host", GiaTri = "smtp.gmail.com", MoTa = "Máy chủ gửi mail SMTP" },
                new CaiDat { TenCaiDat = "Smtp_Password", GiaTri = "raja nenx mxhk vtvn", MoTa = "Mật khẩu ứng dụng (App Password) của Gmail" },
                new CaiDat { TenCaiDat = "Smtp_Port", GiaTri = "587", MoTa = "Cổng kết nối SMTP" },
                new CaiDat { TenCaiDat = "Smtp_Username", GiaTri = "cafebook.hotro@gmail.com", MoTa = "Tài khoản Gmail gửi hỗ trợ" },
                new CaiDat { TenCaiDat = "ThongTin_DiaChi", GiaTri = "08 Hà Văn Tín, P. Hòa Khánh Nam, Q. Liên Chiểu, TP. Đà Nẵng", MoTa = "Địa chỉ in trên hóa đơn" },
                new CaiDat { TenCaiDat = "ThongTin_GioDongCua", GiaTri = "22:00", MoTa = "Giờ đóng cửa quán" },
                new CaiDat { TenCaiDat = "ThongTin_GioiThieu", GiaTri = "Cafebook là không gian lý tưởng, kết hợp giữa niềm đam mê cà phê và tình yêu sách. Chúng tôi mang đến những hạt cà phê chất lượng cùng hàng ngàn đầu sách chọn lọc, tạo nên một ốc đảo bình yên cho tâm hồn bạn.", MoTa = "Giới thiệu được hiển thị ở trên web" },
                new CaiDat { TenCaiDat = "ThongTin_GioMoCua", GiaTri = "07:00", MoTa = "Giờ mở cửa quán" },
                new CaiDat { TenCaiDat = "ThongTin_SoDienThoai", GiaTri = "0376512695", MoTa = "Số Điện Thoại Liên Hệ" },
                new CaiDat { TenCaiDat = "ThongTin_TenQuan", GiaTri = "Cafe Sách Bookshuheheee", MoTa = "Tên quán hiển thị trên hóa đơn, trang web" },
                new CaiDat { TenCaiDat = "ThongTin_ThuMoCua", GiaTri = "2,3,4,5,6,7,8", MoTa = "Các thứ mở cửa trong tuần từ 2-8 cách nhau bởi dấu phẩy" },
                new CaiDat { TenCaiDat = "VNPay_HashSecret", GiaTri = "YK4I1AD53ANFTLC1CJIXNUSUJ45NLA2T", MoTa = "Chuỗi bí mật tạo checksum VNPay" },
                new CaiDat { TenCaiDat = "VNPay_TmnCode", GiaTri = "5KL790VC", MoTa = "Mã Terminal ID của VNPay" },
                new CaiDat { TenCaiDat = "VNPay_Url", GiaTri = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html", MoTa = "Đường dẫn thanh toán VNPay" },
                new CaiDat { TenCaiDat = "Wifi_MatKhau", GiaTri = "Shu.0311", MoTa = "Mật khẩu Wifi cho khách" }
            };

            var existingSettingNames = await context.CaiDats.Select(c => c.TenCaiDat).ToListAsync();
            var missingSettings = defaultSettings.Where(s => !existingSettingNames.Contains(s.TenCaiDat)).ToList();
            if (missingSettings.Any())
            {
                await context.CaiDats.AddRangeAsync(missingSettings);
                await context.SaveChangesAsync();
            }
        }
    }
}