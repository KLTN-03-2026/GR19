-- 1. NHÓM QUYỀN TỐI CAO (Đã có)
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES 
('FULL_NV', N'Toàn quyền Nhân viên', N'Hệ thống'),
('FULL_QL', N'Toàn quyền Quản lý', N'Hệ thống');

-- 2. NHÓM CHUNG (COMMON)
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES 
('CM_THONG_BAO', N'Xem thông báo hệ thống', N'Hệ thống'),
('CM_CAI_DAT', N'Cài đặt phần mềm', N'Hệ thống');

-- 3. NHÓM TỔNG QUAN (QUẢN LÝ)
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES 
('QL_TONG_QUAN', N'Xem Dashboard Tổng quan', N'Tổng quan');

-- 4. NHÓM QUẢN LÝ NHÂN SỰ
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES 
('QL_NHAN_VIEN', N'Quản lý Danh sách Nhân viên', N'Quản lý Nhân sự'),
('QL_PHAN_QUYEN', N'Quản lý Phân quyền', N'Quản lý Nhân sự'),
('QL_VAI_TRO', N'Quản lý Vai trò', N'Quản lý Nhân sự'),
('QL_DON_XIN_NGHI', N'Duyệt Đơn xin nghỉ', N'Quản lý Nhân sự'),
('QL_LICH_LAM_VIEC', N'Xếp Lịch làm việc', N'Quản lý Nhân sự'),
('QL_LUONG', N'Quản lý Bảng lương', N'Quản lý Nhân sự'),
('QL_PHAT_LUONG', N'Quản lý Phát lương', N'Quản lý Nhân sự'),
('QL_CAI_DAT_NHAN_SU', N'Cài đặt tham số Nhân sự', N'Quản lý Nhân sự'),
('QL_BAO_CAO_NHAN_SU', N'Xem Báo cáo Nhân sự', N'Quản lý Nhân sự');

-- 5. NHÓM QUẢN LÝ KHO & NGUYÊN LIỆU
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES 
('QL_TON_KHO', N'Xem Tồn kho', N'Quản lý Kho'),
('QL_NHAP_KHO', N'Quản lý Nhập kho', N'Quản lý Kho'),
('QL_XUAT_HUY', N'Quản lý Xuất hủy', N'Quản lý Kho'),
('QL_KIEM_KHO', N'Quản lý Kiểm kho', N'Quản lý Kho'),
('QL_NHA_CUNG_CAP', N'Quản lý Nhà cung cấp', N'Quản lý Kho');

-- 6. NHÓM QUẢN LÝ THỰC ĐƠN & BÀN
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES 
('QL_SAN_PHAM', N'Quản lý Sản phẩm', N'Quản lý Thực đơn'),
('QL_NGUYEN_LIEU', N'Quản lý Nguyên liệu', N'Quản lý Thực đơn'),
('QL_DON_VI_CHUYEN_DOI', N'Quản lý Đơn vị chuyển đổi', N'Quản lý Thực đơn'),
('QL_BAN', N'Quản lý Sơ đồ bàn', N'Quản lý Thực đơn');

-- 7. NHÓM TÀI CHÍNH & GIAO DỊCH
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES 
('QL_DON_HANG', N'Quản lý Đơn hàng', N'Tài chính & Giao dịch'),
('QL_KHACH_HANG', N'Quản lý Khách hàng', N'Tài chính & Giao dịch'),
('QL_KHUYEN_MAI', N'Quản lý Khuyến mãi', N'Tài chính & Giao dịch'),
('QL_PHU_THU', N'Quản lý Phụ thu', N'Tài chính & Giao dịch'),
('QL_NGUOI_GIAO_HANG', N'Quản lý Shipper nội bộ', N'Tài chính & Giao dịch');

-- 8. NHÓM QUẢN LÝ SÁCH
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES 
('QL_SACH', N'Quản lý Thư viện Sách', N'Quản lý Thư viện');

-- =========================================================
-- =================== MODULE NHÂN VIÊN ====================
-- =========================================================

-- 9. NHÓM CÁ NHÂN (NHÂN VIÊN)
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES 
('NV_THONG_TIN', N'Xem Thông tin cá nhân', N'Cá nhân'),
('NV_CHAM_CONG', N'Chấm công vào/ra ca', N'Cá nhân'),
('NV_LICH_LAM_VIEC', N'Xem Lịch làm việc cá nhân', N'Cá nhân'),
('NV_PHIEU_LUONG', N'Xem Phiếu lương cá nhân', N'Cá nhân');

-- 10. NHÓM VẬN HÀNH BÁN HÀNG (POS)
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES 
('NV_SO_DO_BAN', N'Truy cập Sơ đồ bàn', N'Vận hành POS'),
('NV_DAT_BAN', N'Xử lý Đặt bàn', N'Vận hành POS'),
('NV_GOI_MON', N'Order & Gọi món', N'Vận hành POS'),
('NV_THANH_TOAN', N'Thanh toán hóa đơn', N'Vận hành POS');

-- 11. NHÓM VẬN HÀNH KHÁC (BẾP, GIAO HÀNG, THƯ VIỆN)
INSERT INTO [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES 
('NV_CHE_BIEN', N'Màn hình Bếp/Pha chế', N'Vận hành POS'),
('NV_GIAO_HANG', N'Màn hình Giao hàng', N'Vận hành POS'),
('NV_THUE_SACH', N'Xử lý Thuê/Trả sách', N'Vận hành POS');