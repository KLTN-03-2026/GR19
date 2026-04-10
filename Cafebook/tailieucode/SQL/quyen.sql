-- Xóa toàn bộ dữ liệu cũ để tránh trùng lặp khi chạy lại
DELETE FROM [dbo].[Quyen];
GO

-- CHÈN DỮ LIỆU CHUẨN ĐÃ ĐƯỢC CẬP NHẬT
-- Nhóm: Hệ thống & Phân quyền Tối cao
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'FULL_QL', N'Toàn quyền Quản lý', N'Hệ thống')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'FULL_NV', N'Toàn quyền Nhân viên', N'Hệ thống')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'CM_CAI_DAT', N'Cài đặt phần mềm', N'Hệ thống')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'CM_THONG_BAO', N'Xem thông báo hệ thống', N'Hệ thống')

-- Nhóm: Cá nhân
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_THONG_TIN', N'Xem Thông tin cá nhân', N'Cá nhân')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_LICH_LAM_VIEC', N'Xem Lịch làm việc cá nhân', N'Cá nhân')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_CHAM_CONG', N'Chấm công vào/ra ca', N'Cá nhân')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_PHIEU_LUONG', N'Xem Phiếu lương cá nhân', N'Cá nhân')

-- Nhóm: Vận hành POS (Dành cho Nhân viên)
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_SO_DO_BAN', N'Truy cập Sơ đồ bàn', N'Vận hành POS')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_DAT_BAN', N'Xử lý Đặt bàn', N'Vận hành POS')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_GOI_MON', N'Order & Gọi món', N'Vận hành POS')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_CHE_BIEN', N'Màn hình Bếp/Pha chế', N'Vận hành POS')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_GIAO_HANG', N'Màn hình Giao hàng', N'Vận hành POS')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_THANH_TOAN', N'Thanh toán hóa đơn', N'Vận hành POS')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_THUE_SACH', N'Xử lý Thuê/Trả sách', N'Vận hành POS')

-- Nhóm: Quản lý Tổng quan & Báo cáo (QUYỀN THÊM MỚI Ở ĐÂY)
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_TONG_QUAN', N'Xem Dashboard Tổng quan', N'Tổng quan & Báo cáo')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_BAO_CAO_DOANH_THU', N'Xem Báo cáo Doanh thu', N'Tổng quan & Báo cáo')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_BAO_CAO_TON_KHO_NL', N'Xem Báo cáo Kho Nguyên liệu', N'Tổng quan & Báo cáo')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_BAO_CAO_TON_KHO_SACH', N'Xem Báo cáo Tồn kho Sách', N'Tổng quan & Báo cáo')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_BAO_CAO_NHAN_SU', N'Xem Báo cáo Hiệu suất Nhân sự', N'Tổng quan & Báo cáo')

-- Nhóm: Quản lý Nhân sự
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_NHAN_VIEN', N'Quản lý Danh sách Nhân viên', N'Quản lý Nhân sự')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_PHAN_QUYEN', N'Quản lý Phân quyền', N'Quản lý Nhân sự')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_CAI_DAT_NHAN_SU', N'Cài đặt tham số Nhân sự', N'Quản lý Nhân sự')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_LICH_LAM_VIEC', N'Xếp Lịch làm việc', N'Quản lý Nhân sự')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_DON_XIN_NGHI', N'Duyệt Đơn xin nghỉ', N'Quản lý Nhân sự')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_LUONG', N'Quản lý Bảng lương', N'Quản lý Nhân sự')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_PHAT_LUONG', N'Quản lý Phát lương', N'Quản lý Nhân sự')

-- Nhóm: Tài chính & Giao dịch
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_DON_HANG', N'Quản lý Đơn hàng', N'Tài chính & Giao dịch')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_KHACH_HANG', N'Quản lý Khách hàng', N'Tài chính & Giao dịch')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_KHUYEN_MAI', N'Quản lý Khuyến mãi', N'Tài chính & Giao dịch')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_PHU_THU', N'Quản lý Phụ thu', N'Tài chính & Giao dịch')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_NGUOI_GIAO_HANG', N'Quản lý Shipper nội bộ', N'Tài chính & Giao dịch')

-- Nhóm: Quản lý Thực đơn & Bàn
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_BAN', N'Quản lý Sơ đồ bàn', N'Quản lý Thực đơn')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_SAN_PHAM', N'Quản lý Sản phẩm', N'Quản lý Thực đơn')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_NGUYEN_LIEU', N'Quản lý Nguyên liệu', N'Quản lý Thực đơn')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_DON_VI_CHUYEN_DOI', N'Quản lý Đơn vị chuyển đổi', N'Quản lý Thực đơn')

-- Nhóm: Quản lý Kho & Thư viện
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_TON_KHO', N'Xem Tồn kho', N'Quản lý Kho')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_NHAP_KHO', N'Quản lý Nhập kho', N'Quản lý Kho')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_KIEM_KHO', N'Quản lý Kiểm kho', N'Quản lý Kho')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_XUAT_HUY', N'Quản lý Xuất hủy', N'Quản lý Kho')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_NHA_CUNG_CAP', N'Quản lý Nhà cung cấp', N'Quản lý Kho')
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_SACH', N'Quản lý Thư viện Sách', N'Quản lý Thư viện')
GO