USE [CafebookDB]
GO

INSERT INTO [dbo].[KhachHang] 
([hoTen], [soDienThoai], [email], [diaChi], [diemTichLuy], [tenDangNhap], [matKhau], [ngayTao], [BiKhoa], [AnhDaiDien], [taiKhoanTam], [DaXoa], [lyDoKhoa], [thoiGianMoKhoa])
VALUES
-- ====================================================================================
-- NHÓM 1: KHÁCH HÀNG THÂN THIẾT / VIP (Điểm tích lũy cao) - 20 Dòng
-- ====================================================================================
(N'Nguyễn Văn An', N'0901234001', N'nva01@gmail.com', N'01 Lê Lợi, TP. Đà Nẵng', 2500, N'nguyenvanan', N'123456', '2025-01-10', 0, NULL, 0, 0, NULL, NULL),
(N'Trần Thị Bích', N'0901234002', N'tranbich@gmail.com', N'02 Hùng Vương, TP. Đà Nẵng', 3200, N'tranbich', N'123456', '2025-01-12', 0, NULL, 0, 0, NULL, NULL),
(N'Lê Hoàng Cường', N'0901234003', N'lecuong@gmail.com', N'03 Nguyễn Văn Linh, TP. Đà Nẵng', 1850, N'hoangcuong', N'123456', '2025-01-15', 0, NULL, 0, 0, NULL, NULL),
(N'Phạm Dung', N'0901234004', N'phamdung@gmail.com', N'04 Phan Châu Trinh, TP. Đà Nẵng', 4100, N'phamdung', N'123456', '2025-01-20', 0, NULL, 0, 0, NULL, NULL),
(N'Hoàng Văn Ân', N'0901234005', N'hoangan@gmail.com', N'05 Điện Biên Phủ, TP. Đà Nẵng', 1200, N'hoangan12', N'123456', '2025-02-05', 0, NULL, 0, 0, NULL, NULL),
(N'Đỗ Bích Chi', N'0901234006', N'dobichchi@gmail.com', N'06 Tôn Đức Thắng, TP. Đà Nẵng', 2150, N'dobichchi', N'123456', '2025-02-10', 0, NULL, 0, 0, NULL, NULL),
(N'Ngô Bảo', N'0901234007', N'ngobao@gmail.com', N'07 Nguyễn Hữu Thọ, TP. Đà Nẵng', 3050, N'ngobao', N'123456', '2025-02-14', 0, NULL, 0, 0, NULL, NULL),
(N'Vũ Đức Đam', N'0901234008', N'vudam@gmail.com', N'08 Lê Duẩn, TP. Đà Nẵng', 1900, N'vudam', N'123456', '2025-02-28', 0, NULL, 0, 0, NULL, NULL),
(N'Đinh Khắc Ân', N'0901234009', N'dinhkhacan@gmail.com', N'09 Ông Ích Khiêm, TP. Đà Nẵng', 2700, N'khacan', N'123456', '2025-03-01', 0, NULL, 0, 0, NULL, NULL),
(N'Bùi Lan Hương', N'0901234010', N'builanhuong@gmail.com', N'10 Trần Phú, TP. Đà Nẵng', 4500, N'lanhuongbui', N'123456', '2025-03-10', 0, NULL, 0, 0, NULL, NULL),
(N'Trương Nam', N'0901234011', N'truongnam@gmail.com', N'11 Ngô Quyền, TP. Đà Nẵng', 1100, N'truongnam', N'123456', '2025-03-15', 0, NULL, 0, 0, NULL, NULL),
(N'Lý Quang', N'0901234012', N'lyquang@gmail.com', N'12 Bạch Đằng, TP. Đà Nẵng', 3300, N'lyquang', N'123456', '2025-04-01', 0, NULL, 0, 0, NULL, NULL),
(N'Huỳnh Tú', N'0901234013', N'huynhtu@gmail.com', N'13 Nguyễn Tất Thành, TP. Đà Nẵng', 2650, N'huynhtu', N'123456', '2025-04-12', 0, NULL, 0, 0, NULL, NULL),
(N'Trần Bằng', N'0901234014', N'tranbang@gmail.com', N'14 Hà Huy Tập, TP. Đà Nẵng', 1800, N'tranbang', N'123456', '2025-05-02', 0, NULL, 0, 0, NULL, NULL),
(N'Nguyễn Diệu', N'0901234015', N'nguyendieu@gmail.com', N'15 Hoàng Diệu, TP. Đà Nẵng', 2950, N'nguyendieu', N'123456', '2025-05-20', 0, NULL, 0, 0, NULL, NULL),
(N'Lê Tuấn', N'0901234016', N'letuan@gmail.com', N'16 Đống Đa, TP. Đà Nẵng', 1450, N'letuan', N'123456', '2025-06-05', 0, NULL, 0, 0, NULL, NULL),
(N'Phạm Hùng', N'0901234017', N'phamhung@gmail.com', N'17 Lương Nhữ Hộc, TP. Đà Nẵng', 3800, N'phamhung', N'123456', '2025-06-18', 0, NULL, 0, 0, NULL, NULL),
(N'Ngô Mai', N'0901234018', N'ngomai@gmail.com', N'18 Hải Phòng, TP. Đà Nẵng', 2100, N'ngomai', N'123456', '2025-07-01', 0, NULL, 0, 0, NULL, NULL),
(N'Vũ Tùng', N'0901234019', N'vutung@gmail.com', N'19 Trưng Nữ Vương, TP. Đà Nẵng', 3150, N'vutung', N'123456', '2025-07-15', 0, NULL, 0, 0, NULL, NULL),
(N'Đinh Hạnh', N'0901234020', N'dinhhanh@gmail.com', N'20 Núi Thành, TP. Đà Nẵng', 1700, N'dinhhanh', N'123456', '2025-08-08', 0, NULL, 0, 0, NULL, NULL),

-- ====================================================================================
-- NHÓM 2: KHÁCH HÀNG MỚI / THƯỜNG (Điểm tích lũy thấp hoặc = 0) - 20 Dòng
-- ====================================================================================
(N'Bùi Phương', N'0902234021', N'buiphuong@gmail.com', N'21 Tiểu La, TP. Đà Nẵng', 10, N'buiphuong', N'123456', '2026-03-01', 0, NULL, 0, 0, NULL, NULL),
(N'Trương Linh', N'0902234022', N'truonglinh@gmail.com', N'22 Lê Đại Hành, TP. Đà Nẵng', 50, N'truonglinh', N'123456', '2026-03-05', 0, NULL, 0, 0, NULL, NULL),
(N'Lý Mạc', N'0902234023', N'lymac@gmail.com', N'23 Nguyễn Trãi, TP. Đà Nẵng', 0, N'lymac', N'123456', '2026-03-10', 0, NULL, 0, 0, NULL, NULL),
(N'Huỳnh Như', N'0902234024', N'huynhnhu@gmail.com', N'24 Cẩm Lệ, TP. Đà Nẵng', 120, N'huynhnhu', N'123456', '2026-03-15', 0, NULL, 0, 0, NULL, NULL),
(N'Trần Tiến', N'0902234025', N'trantien@gmail.com', N'25 Liên Chiểu, TP. Đà Nẵng', 0, N'trantien', N'123456', '2026-03-20', 0, NULL, 0, 0, NULL, NULL),
(N'Nguyễn Trọng', N'0902234026', N'nguyentrong@gmail.com', N'26 Hòa Vang, TP. Đà Nẵng', 80, N'nguyentrong', N'123456', '2026-03-22', 0, NULL, 0, 0, NULL, NULL),
(N'Lê Quyên', N'0902234027', N'lequyen@gmail.com', N'27 Ngũ Hành Sơn, TP. Đà Nẵng', 30, N'lequyen', N'123456', '2026-03-25', 0, NULL, 0, 0, NULL, NULL),
(N'Phạm Thu', N'0902234028', N'phamthu@gmail.com', N'28 Sơn Trà, TP. Đà Nẵng', 0, N'phamthu', N'123456', '2026-04-01', 0, NULL, 0, 0, NULL, NULL),
(N'Ngô Bảo Châu', N'0902234029', N'ngobaochau@gmail.com', N'29 Hải Châu, TP. Đà Nẵng', 150, N'ngobaochau', N'123456', '2026-04-05', 0, NULL, 0, 0, NULL, NULL),
(N'Vũ Cường', N'0902234030', N'vucuong@gmail.com', N'30 Hòa Khánh, TP. Đà Nẵng', 0, N'vucuong', N'123456', '2026-04-08', 0, NULL, 0, 0, NULL, NULL),
(N'Đinh Yến', N'0902234031', N'dinhyen@gmail.com', NULL, 40, N'dinhyen', N'123456', '2026-04-10', 0, NULL, 0, 0, NULL, NULL),
(N'Bùi Tuấn', N'0902234032', N'buituan@gmail.com', NULL, 0, N'buituan', N'123456', '2026-04-12', 0, NULL, 0, 0, NULL, NULL),
(N'Trương Hảo', N'0902234033', N'truonghao@gmail.com', NULL, 25, N'truonghao', N'123456', '2026-04-15', 0, NULL, 0, 0, NULL, NULL),
(N'Lý Phát', N'0902234034', N'lyphat@gmail.com', N'34 Ngô Thì Nhậm, TP. Đà Nẵng', 0, N'lyphat', N'123456', '2026-04-18', 0, NULL, 0, 0, NULL, NULL),
(N'Huỳnh Quang', N'0902234035', N'huynhquang@gmail.com', N'35 Lý Thái Tổ, TP. Đà Nẵng', 110, N'huynhquang', N'123456', '2026-04-20', 0, NULL, 0, 0, NULL, NULL),
(N'Trần Minh', N'0902234036', N'tranminh@gmail.com', NULL, 0, N'tranminh', N'123456', '2026-04-22', 0, NULL, 0, 0, NULL, NULL),
(N'Nguyễn Vũ', N'0902234037', N'nguyenvu@gmail.com', N'37 Tôn Thất Thuyết, TP. Đà Nẵng', 90, N'nguyenvu', N'123456', '2026-04-24', 0, NULL, 0, 0, NULL, NULL),
(N'Lê Thảo', N'0902234038', N'lethao@gmail.com', NULL, 0, N'lethao', N'123456', '2026-04-25', 0, NULL, 0, 0, NULL, NULL),
(N'Phạm Nhi', N'0902234039', N'phamnhi@gmail.com', N'39 Lạc Long Quân, TP. Đà Nẵng', 15, N'phamnhi', N'123456', '2026-04-26', 0, NULL, 0, 0, NULL, NULL),
(N'Ngô Khoa', N'0902234040', N'ngokhoa@gmail.com', NULL, 0, N'ngokhoa', N'123456', '2026-04-28', 0, NULL, 0, 0, NULL, NULL),

-- ====================================================================================
-- NHÓM 3: KHÁCH HÀNG VÃNG LAI / TÀI KHOẢN TẠM (Cấp Dummy Data cho cột UNIQUE) - 20 Dòng
-- ====================================================================================
(N'Khách Lẻ 01', N'0903234041', N'kl01@cafebook.local', NULL, 0, N'kl_tmp_01', N'123456', '2026-04-01', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 02', N'0903234042', N'kl02@cafebook.local', NULL, 0, N'kl_tmp_02', N'123456', '2026-04-02', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 03', N'0903234043', N'kl03@cafebook.local', NULL, 0, N'kl_tmp_03', N'123456', '2026-04-05', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 04', N'0903234044', N'kl04@cafebook.local', NULL, 0, N'kl_tmp_04', N'123456', '2026-04-06', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 05', N'0903234045', N'kl05@cafebook.local', NULL, 0, N'kl_tmp_05', N'123456', '2026-04-08', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 06', N'0903234046', N'kl06@cafebook.local', NULL, 0, N'kl_tmp_06', N'123456', '2026-04-10', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 07', N'0903234047', N'kl07@cafebook.local', NULL, 0, N'kl_tmp_07', N'123456', '2026-04-11', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 08', N'0903234048', N'kl08@cafebook.local', NULL, 0, N'kl_tmp_08', N'123456', '2026-04-12', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 09', N'0903234049', N'kl09@cafebook.local', NULL, 0, N'kl_tmp_09', N'123456', '2026-04-14', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 10', N'0903234050', N'kl10@cafebook.local', NULL, 0, N'kl_tmp_10', N'123456', '2026-04-15', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 11', N'0903234051', N'kl11@cafebook.local', NULL, 0, N'kl_tmp_11', N'123456', '2026-04-16', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 12', N'0903234052', N'kl12@cafebook.local', NULL, 0, N'kl_tmp_12', N'123456', '2026-04-18', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 13', N'0903234053', N'kl13@cafebook.local', NULL, 0, N'kl_tmp_13', N'123456', '2026-04-20', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 14', N'0903234054', N'kl14@cafebook.local', NULL, 0, N'kl_tmp_14', N'123456', '2026-04-21', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 15', N'0903234055', N'kl15@cafebook.local', NULL, 0, N'kl_tmp_15', N'123456', '2026-04-22', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 16', N'0903234056', N'kl16@cafebook.local', NULL, 0, N'kl_tmp_16', N'123456', '2026-04-24', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 17', N'0903234057', N'kl17@cafebook.local', NULL, 0, N'kl_tmp_17', N'123456', '2026-04-25', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 18', N'0903234058', N'kl18@cafebook.local', NULL, 0, N'kl_tmp_18', N'123456', '2026-04-26', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 19', N'0903234059', N'kl19@cafebook.local', NULL, 0, N'kl_tmp_19', N'123456', '2026-04-28', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 20', N'0903234060', N'kl20@cafebook.local', NULL, 0, N'kl_tmp_20', N'123456', '2026-04-29', 0, NULL, 1, 0, NULL, NULL),

-- ====================================================================================
-- NHÓM 4: KHÁCH HÀNG BỊ KHÓA TÀI KHOẢN (BiKhoa = 1) - 20 Dòng
-- ====================================================================================
(N'Vũ Phương', N'0904234061', N'vuphuong@gmail.com', NULL, 150, N'vuphuong', N'123456', '2025-05-10', 1, NULL, 0, 0, N'Hủy đơn hàng liên tục 5 lần', '2026-05-15'),
(N'Đinh Tài', N'0904234062', N'dinhtai@gmail.com', NULL, 200, N'dinhtai', N'123456', '2025-06-11', 1, NULL, 0, 0, N'Spam hệ thống đánh giá', NULL),
(N'Bùi Tuấn', N'0904234063', N'buituan2@gmail.com', NULL, 50, N'buituan2', N'123456', '2025-07-12', 1, NULL, 0, 0, N'Boom hàng COD', '2026-10-01'),
(N'Trương Vi', N'0904234064', N'truongvi@gmail.com', NULL, 0, N'truongvi', N'123456', '2025-08-13', 1, NULL, 0, 0, N'Tài khoản clone gian lận khuyến mãi', NULL),
(N'Lý Xuyên', N'0904234065', N'lyxuyen@gmail.com', NULL, 300, N'lyxuyen', N'123456', '2025-09-14', 1, NULL, 0, 0, N'Cố tình trả sách rách bìa nhiều lần', '2026-12-31'),
(N'Huỳnh Yến', N'0904234066', N'huynhyen@gmail.com', NULL, 10, N'huynhyen', N'123456', '2025-10-15', 1, NULL, 0, 0, N'Spam tin nhắn AI chatbot', '2026-05-20'),
(N'Trần Cường', N'0904234067', N'trancuong@gmail.com', NULL, 80, N'trancuong', N'123456', '2025-11-16', 1, NULL, 0, 0, N'Boom hàng', NULL),
(N'Nguyễn Diễm', N'0904234068', N'nguyendiem@gmail.com', NULL, 25, N'nguyendiem', N'123456', '2025-12-17', 1, NULL, 0, 0, N'Dùng lời lẽ không chuẩn mực với Shipper', '2026-06-01'),
(N'Lê Hà', N'0904234069', N'leha@gmail.com', NULL, 0, N'leha', N'123456', '2026-01-18', 1, NULL, 0, 0, N'Gian lận điểm tích lũy', NULL),
(N'Phạm Kiên', N'0904234070', N'phamkien@gmail.com', NULL, 400, N'phamkien', N'123456', '2026-02-19', 1, NULL, 0, 0, N'Vi phạm chính sách đặt bàn (Không đến nhiều lần)', '2026-05-01'),
(N'Ngô Lâm', N'0904234071', N'ngolam@gmail.com', NULL, 0, N'ngolam', N'123456', '2026-03-20', 1, NULL, 0, 0, N'Hủy đơn hàng liên tục', '2026-05-25'),
(N'Vũ Ninh', N'0904234072', N'vuninh@gmail.com', NULL, 60, N'vuninh', N'123456', '2026-04-01', 1, NULL, 0, 0, N'Đánh giá ảo', NULL),
(N'Đinh Oanh', N'0904234073', N'dinhoanh@gmail.com', NULL, 15, N'dinhoanh', N'123456', '2026-04-05', 1, NULL, 0, 0, N'Boom hàng COD', '2026-07-01'),
(N'Bùi Quốc', N'0904234074', N'buiquoc@gmail.com', NULL, 0, N'buiquoc', N'123456', '2026-04-10', 1, NULL, 0, 0, N'Clone account', NULL),
(N'Trương Sĩ', N'0904234075', N'truongsi@gmail.com', NULL, 20, N'truongsi', N'123456', '2026-04-12', 1, NULL, 0, 0, N'Spam tin nhắn web', '2026-05-15'),
(N'Lý Thuận', N'0904234076', N'lythuan@gmail.com', NULL, 110, N'lythuan', N'123456', '2026-04-15', 1, NULL, 0, 0, N'Nợ tiền thuê sách quá hạn 3 tháng', NULL),
(N'Huỳnh Uyên', N'0904234077', N'huynhuyen@gmail.com', NULL, 0, N'huynhuyen', N'123456', '2026-04-18', 1, NULL, 0, 0, N'Gian lận khuyến mãi', NULL),
(N'Trần Vỹ', N'0904234078', N'tranvy@gmail.com', NULL, 90, N'tranvy', N'123456', '2026-04-20', 1, NULL, 0, 0, N'Boom hàng', '2026-08-01'),
(N'Nguyễn Xuân', N'0904234079', N'nguyenxuan@gmail.com', NULL, 5, N'nguyenxuan', N'123456', '2026-04-25', 1, NULL, 0, 0, N'Bom hàng liên tục', NULL),
(N'Lê Yến', N'0904234080', N'leyen@gmail.com', NULL, 0, N'leyen', N'123456', '2026-04-28', 1, NULL, 0, 0, N'Spam hệ thống', '2026-05-30'),

-- ====================================================================================
-- NHÓM 5: KHÁCH HÀNG ĐÃ XÓA (Soft delete - DaXoa = 1) - 20 Dòng
-- ====================================================================================
(N'Phạm Anh', N'0905234081', N'phamanh@gmail.com', NULL, 0, N'phamanh', N'123456', '2025-01-05', 0, NULL, 0, 1, NULL, NULL),
(N'Ngô Bình', N'0905234082', N'ngobinh@gmail.com', NULL, 0, N'ngobinh', N'123456', '2025-02-10', 0, NULL, 0, 1, NULL, NULL),
(N'Vũ Chiến', N'0905234083', N'vuchien@gmail.com', NULL, 50, N'vuchien', N'123456', '2025-03-15', 0, NULL, 0, 1, NULL, NULL),
(N'Đinh Đạo', N'0905234084', N'dinhdao@gmail.com', NULL, 0, N'dinhdao', N'123456', '2025-04-20', 0, NULL, 0, 1, NULL, NULL),
(N'Bùi Giang', N'0905234085', N'buigiang@gmail.com', NULL, 120, N'buigiang', N'123456', '2025-05-25', 0, NULL, 0, 1, NULL, NULL),
(N'Trương Hiếu', N'0905234086', N'truonghieu@gmail.com', NULL, 0, N'truonghieu', N'123456', '2025-06-30', 0, NULL, 0, 1, NULL, NULL),
(N'Lý Khanh', N'0905234087', N'lykhanh@gmail.com', NULL, 0, N'lykhanh', N'123456', '2025-07-05', 0, NULL, 0, 1, NULL, NULL),
(N'Huỳnh Long', N'0905234088', N'huynhlong@gmail.com', NULL, 10, N'huynhlong', N'123456', '2025-08-10', 0, NULL, 0, 1, NULL, NULL),
(N'Trần Mẫn', N'0905234089', N'tranman@gmail.com', NULL, 0, N'tranman', N'123456', '2025-09-15', 0, NULL, 0, 1, NULL, NULL),
(N'Nguyễn Ngọc', N'0905234090', N'nguyenngoc@gmail.com', NULL, 40, N'nguyenngoc', N'123456', '2025-10-20', 0, NULL, 0, 1, NULL, NULL),
(N'Lê Phúc', N'0905234091', N'lephuc@gmail.com', NULL, 0, N'lephuc', N'123456', '2025-11-25', 0, NULL, 0, 1, NULL, NULL),
(N'Phạm Quyết', N'0905234092', N'phamquyet@gmail.com', NULL, 0, N'phamquyet', N'123456', '2025-12-30', 0, NULL, 0, 1, NULL, NULL),
(N'Ngô Sơn', N'0905234093', N'ngoson@gmail.com', NULL, 80, N'ngoson', N'123456', '2026-01-04', 0, NULL, 0, 1, NULL, NULL),
(N'Vũ Tuyết', N'0905234094', N'vutuyet@gmail.com', NULL, 0, N'vutuyet', N'123456', '2026-02-09', 0, NULL, 0, 1, NULL, NULL),
(N'Đinh Uẩn', N'0905234095', N'dinhuan@gmail.com', NULL, 0, N'dinhuan', N'123456', '2026-03-14', 0, NULL, 0, 1, NULL, NULL),
(N'Bùi Vương', N'0905234096', N'buivuong@gmail.com', NULL, 30, N'buivuong', N'123456', '2026-04-01', 0, NULL, 0, 1, NULL, NULL),
(N'Trương Xuyến', N'0905234097', N'truongxuyen@gmail.com', NULL, 0, N'truongxuyen', N'123456', '2026-04-10', 0, NULL, 0, 1, NULL, NULL),
(N'Lý Yến', N'0905234098', N'lyyen@gmail.com', NULL, 0, N'lyyen', N'123456', '2026-04-15', 0, NULL, 0, 1, NULL, NULL),
(N'Huỳnh Ân', N'0905234099', N'huynhan@gmail.com', NULL, 15, N'huynhan', N'123456', '2026-04-20', 0, NULL, 0, 1, NULL, NULL),
(N'Trần Bằng', N'0905234100', N'tranbang2@gmail.com', NULL, 0, N'tranbang2', N'123456', '2026-04-25', 0, NULL, 0, 1, NULL, NULL);
GO
USE [CafebookDB]
GO

INSERT INTO [dbo].[KhachHang] 
([hoTen], [soDienThoai], [email], [diaChi], [diemTichLuy], [tenDangNhap], [matKhau], [ngayTao], [BiKhoa], [AnhDaiDien], [taiKhoanTam], [DaXoa], [lyDoKhoa], [thoiGianMoKhoa])
VALUES
-- ====================================================================================
-- NHÓM 1: KHÁCH HÀNG THÂN THIẾT / VIP (101 - 120)
-- ====================================================================================
(N'Đoàn Văn Nam', N'0906234101', N'vip101@gmail.com', N'101 Lê Lợi, TP. Đà Nẵng', 3500, N'vipuser101', N'123456', '2025-01-02', 0, NULL, 0, 0, NULL, NULL),
(N'Hồ Thị Nga', N'0906234102', N'vip102@gmail.com', N'102 Hùng Vương, TP. Đà Nẵng', 2800, N'vipuser102', N'123456', '2025-01-15', 0, NULL, 0, 0, NULL, NULL),
(N'Mai Tuấn Anh', N'0906234103', N'vip103@gmail.com', N'103 Nguyễn Văn Linh, TP. Đà Nẵng', 4200, N'vipuser103', N'123456', '2025-02-05', 0, NULL, 0, 0, NULL, NULL),
(N'Cao Bảo Ngọc', N'0906234104', N'vip104@gmail.com', N'104 Phan Châu Trinh, TP. Đà Nẵng', 5100, N'vipuser104', N'123456', '2025-02-18', 0, NULL, 0, 0, NULL, NULL),
(N'Phan Nhật', N'0906234105', N'vip105@gmail.com', N'105 Điện Biên Phủ, TP. Đà Nẵng', 3100, N'vipuser105', N'123456', '2025-03-10', 0, NULL, 0, 0, NULL, NULL),
(N'Tôn Thất Minh', N'0906234106', N'vip106@gmail.com', N'106 Tôn Đức Thắng, TP. Đà Nẵng', 2250, N'vipuser106', N'123456', '2025-03-22', 0, NULL, 0, 0, NULL, NULL),
(N'Dương Cẩm', N'0906234107', N'vip107@gmail.com', N'107 Nguyễn Hữu Thọ, TP. Đà Nẵng', 3750, N'vipuser107', N'123456', '2025-04-14', 0, NULL, 0, 0, NULL, NULL),
(N'Quách Hùng', N'0906234108', N'vip108@gmail.com', N'108 Lê Duẩn, TP. Đà Nẵng', 4800, N'vipuser108', N'123456', '2025-05-01', 0, NULL, 0, 0, NULL, NULL),
(N'Tiêu Viêm', N'0906234109', N'vip109@gmail.com', N'109 Ông Ích Khiêm, TP. Đà Nẵng', 2900, N'vipuser109', N'123456', '2025-05-15', 0, NULL, 0, 0, NULL, NULL),
(N'Châu Khải', N'0906234110', N'vip110@gmail.com', N'110 Trần Phú, TP. Đà Nẵng', 3300, N'vipuser110', N'123456', '2025-06-10', 0, NULL, 0, 0, NULL, NULL),
(N'Lại Thế Thắng', N'0906234111', N'vip111@gmail.com', N'111 Ngô Quyền, TP. Đà Nẵng', 1950, N'vipuser111', N'123456', '2025-06-25', 0, NULL, 0, 0, NULL, NULL),
(N'Mạc Sầu', N'0906234112', N'vip112@gmail.com', N'112 Bạch Đằng, TP. Đà Nẵng', 4400, N'vipuser112', N'123456', '2025-07-08', 0, NULL, 0, 0, NULL, NULL),
(N'Kỷ Hiểu Phù', N'0906234113', N'vip113@gmail.com', N'113 Nguyễn Tất Thành, TP. Đà Nẵng', 2700, N'vipuser113', N'123456', '2025-08-12', 0, NULL, 0, 0, NULL, NULL),
(N'Văn Khang', N'0906234114', N'vip114@gmail.com', N'114 Hà Huy Tập, TP. Đà Nẵng', 3600, N'vipuser114', N'123456', '2025-09-05', 0, NULL, 0, 0, NULL, NULL),
(N'Đổng Trác', N'0906234115', N'vip115@gmail.com', N'115 Hoàng Diệu, TP. Đà Nẵng', 2100, N'vipuser115', N'123456', '2025-10-18', 0, NULL, 0, 0, NULL, NULL),
(N'Thạch Hạo', N'0906234116', N'vip116@gmail.com', N'116 Đống Đa, TP. Đà Nẵng', 3200, N'vipuser116', N'123456', '2025-11-20', 0, NULL, 0, 0, NULL, NULL),
(N'Bạch Thiển', N'0906234117', N'vip117@gmail.com', N'117 Lương Nhữ Hộc, TP. Đà Nẵng', 4150, N'vipuser117', N'123456', '2025-12-05', 0, NULL, 0, 0, NULL, NULL),
(N'Hà Anh', N'0906234118', N'vip118@gmail.com', N'118 Hải Phòng, TP. Đà Nẵng', 1800, N'vipuser118', N'123456', '2026-01-15', 0, NULL, 0, 0, NULL, NULL),
(N'Tô Lâm', N'0906234119', N'vip119@gmail.com', N'119 Trưng Nữ Vương, TP. Đà Nẵng', 2450, N'vipuser119', N'123456', '2026-02-10', 0, NULL, 0, 0, NULL, NULL),
(N'Cù Trọng', N'0906234120', N'vip120@gmail.com', N'120 Núi Thành, TP. Đà Nẵng', 3900, N'vipuser120', N'123456', '2026-03-01', 0, NULL, 0, 0, NULL, NULL),

-- ====================================================================================
-- NHÓM 2: KHÁCH HÀNG MỚI / THƯỜNG (121 - 140)
-- ====================================================================================
(N'Chu Cường', N'0906234121', N'norm121@gmail.com', N'121 Tiểu La, TP. Đà Nẵng', 15, N'normuser121', N'123456', '2026-03-10', 0, NULL, 0, 0, NULL, NULL),
(N'Đào Thu', N'0906234122', N'norm122@gmail.com', N'122 Lê Đại Hành, TP. Đà Nẵng', 80, N'normuser122', N'123456', '2026-03-12', 0, NULL, 0, 0, NULL, NULL),
(N'Lưu Trọng', N'0906234123', N'norm123@gmail.com', N'123 Nguyễn Trãi, TP. Đà Nẵng', 0, N'normuser123', N'123456', '2026-03-15', 0, NULL, 0, 0, NULL, NULL),
(N'Khổng Minh', N'0906234124', N'norm124@gmail.com', N'124 Cẩm Lệ, TP. Đà Nẵng', 150, N'normuser124', N'123456', '2026-03-18', 0, NULL, 0, 0, NULL, NULL),
(N'Diệp Phàm', N'0906234125', N'norm125@gmail.com', N'125 Liên Chiểu, TP. Đà Nẵng', 0, N'normuser125', N'123456', '2026-03-20', 0, NULL, 0, 0, NULL, NULL),
(N'Sử Đại', N'0906234126', N'norm126@gmail.com', N'126 Hòa Vang, TP. Đà Nẵng', 65, N'normuser126', N'123456', '2026-03-22', 0, NULL, 0, 0, NULL, NULL),
(N'Cấn Oanh', N'0906234127', N'norm127@gmail.com', N'127 Ngũ Hành Sơn, TP. Đà Nẵng', 30, N'normuser127', N'123456', '2026-03-25', 0, NULL, 0, 0, NULL, NULL),
(N'La Thành', N'0906234128', N'norm128@gmail.com', N'128 Sơn Trà, TP. Đà Nẵng', 0, N'normuser128', N'123456', '2026-03-28', 0, NULL, 0, 0, NULL, NULL),
(N'Từ Lăng', N'0906234129', N'norm129@gmail.com', N'129 Hải Châu, TP. Đà Nẵng', 120, N'normuser129', N'123456', '2026-04-01', 0, NULL, 0, 0, NULL, NULL),
(N'Kiều Phong', N'0906234130', N'norm130@gmail.com', N'130 Hòa Khánh, TP. Đà Nẵng', 0, N'normuser130', N'123456', '2026-04-05', 0, NULL, 0, 0, NULL, NULL),
(N'Bàng Thống', N'0906234131', N'norm131@gmail.com', NULL, 45, N'normuser131', N'123456', '2026-04-08', 0, NULL, 0, 0, NULL, NULL),
(N'Phương Tôn', N'0906234132', N'norm132@gmail.com', NULL, 0, N'normuser132', N'123456', '2026-04-10', 0, NULL, 0, 0, NULL, NULL),
(N'Uông Thành', N'0906234133', N'norm133@gmail.com', NULL, 85, N'normuser133', N'123456', '2026-04-12', 0, NULL, 0, 0, NULL, NULL),
(N'Trầm Tuấn', N'0906234134', N'norm134@gmail.com', N'134 Ngô Thì Nhậm, TP. Đà Nẵng', 0, N'normuser134', N'123456', '2026-04-14', 0, NULL, 0, 0, NULL, NULL),
(N'Lạc Thanh', N'0906234135', N'norm135@gmail.com', N'135 Lý Thái Tổ, TP. Đà Nẵng', 115, N'normuser135', N'123456', '2026-04-16', 0, NULL, 0, 0, NULL, NULL),
(N'Ân Tú', N'0906234136', N'norm136@gmail.com', NULL, 0, N'normuser136', N'123456', '2026-04-18', 0, NULL, 0, 0, NULL, NULL),
(N'Thiệu Phong', N'0906234137', N'norm137@gmail.com', N'137 Tôn Thất Thuyết, TP. Đà Nẵng', 95, N'normuser137', N'123456', '2026-04-20', 0, NULL, 0, 0, NULL, NULL),
(N'Ninh Khang', N'0906234138', N'norm138@gmail.com', NULL, 0, N'normuser138', N'123456', '2026-04-22', 0, NULL, 0, 0, NULL, NULL),
(N'Tưởng Hân', N'0906234139', N'norm139@gmail.com', N'139 Lạc Long Quân, TP. Đà Nẵng', 20, N'normuser139', N'123456', '2026-04-25', 0, NULL, 0, 0, NULL, NULL),
(N'Giang Nam', N'0906234140', N'norm140@gmail.com', NULL, 0, N'normuser140', N'123456', '2026-04-28', 0, NULL, 0, 0, NULL, NULL),

-- ====================================================================================
-- NHÓM 3: KHÁCH HÀNG VÃNG LAI / TÀI KHOẢN TẠM (141 - 160)
-- ====================================================================================
(N'Khách Lẻ 141', N'0906234141', N'kl141@cafebook.local', NULL, 0, N'guest_tmp_141', N'123456', '2026-04-01', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 142', N'0906234142', N'kl142@cafebook.local', NULL, 0, N'guest_tmp_142', N'123456', '2026-04-02', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 143', N'0906234143', N'kl143@cafebook.local', NULL, 0, N'guest_tmp_143', N'123456', '2026-04-03', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 144', N'0906234144', N'kl144@cafebook.local', NULL, 0, N'guest_tmp_144', N'123456', '2026-04-04', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 145', N'0906234145', N'kl145@cafebook.local', NULL, 0, N'guest_tmp_145', N'123456', '2026-04-05', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 146', N'0906234146', N'kl146@cafebook.local', NULL, 0, N'guest_tmp_146', N'123456', '2026-04-06', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 147', N'0906234147', N'kl147@cafebook.local', NULL, 0, N'guest_tmp_147', N'123456', '2026-04-07', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 148', N'0906234148', N'kl148@cafebook.local', NULL, 0, N'guest_tmp_148', N'123456', '2026-04-08', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 149', N'0906234149', N'kl149@cafebook.local', NULL, 0, N'guest_tmp_149', N'123456', '2026-04-09', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 150', N'0906234150', N'kl150@cafebook.local', NULL, 0, N'guest_tmp_150', N'123456', '2026-04-10', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 151', N'0906234151', N'kl151@cafebook.local', NULL, 0, N'guest_tmp_151', N'123456', '2026-04-11', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 152', N'0906234152', N'kl152@cafebook.local', NULL, 0, N'guest_tmp_152', N'123456', '2026-04-12', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 153', N'0906234153', N'kl153@cafebook.local', NULL, 0, N'guest_tmp_153', N'123456', '2026-04-13', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 154', N'0906234154', N'kl154@cafebook.local', NULL, 0, N'guest_tmp_154', N'123456', '2026-04-14', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 155', N'0906234155', N'kl155@cafebook.local', NULL, 0, N'guest_tmp_155', N'123456', '2026-04-15', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 156', N'0906234156', N'kl156@cafebook.local', NULL, 0, N'guest_tmp_156', N'123456', '2026-04-16', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 157', N'0906234157', N'kl157@cafebook.local', NULL, 0, N'guest_tmp_157', N'123456', '2026-04-17', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 158', N'0906234158', N'kl158@cafebook.local', NULL, 0, N'guest_tmp_158', N'123456', '2026-04-18', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 159', N'0906234159', N'kl159@cafebook.local', NULL, 0, N'guest_tmp_159', N'123456', '2026-04-19', 0, NULL, 1, 0, NULL, NULL),
(N'Khách Lẻ 160', N'0906234160', N'kl160@cafebook.local', NULL, 0, N'guest_tmp_160', N'123456', '2026-04-20', 0, NULL, 1, 0, NULL, NULL),

-- ====================================================================================
-- NHÓM 4: KHÁCH HÀNG BỊ KHÓA TÀI KHOẢN (161 - 180)
-- ====================================================================================
(N'Viên Thuật', N'0906234161', N'ban161@gmail.com', NULL, 40, N'banuser161', N'123456', '2025-01-10', 1, NULL, 0, 0, N'Cố tình phá hoại thiết bị quán', '2026-10-15'),
(N'Trương Cáp', N'0906234162', N'ban162@gmail.com', NULL, 120, N'banuser162', N'123456', '2025-02-11', 1, NULL, 0, 0, N'Xúc phạm nhân viên', NULL),
(N'Nhan Lương', N'0906234163', N'ban163@gmail.com', NULL, 20, N'banuser163', N'123456', '2025-03-12', 1, NULL, 0, 0, N'Boom hàng COD 3 lần liên tiếp', '2026-12-01'),
(N'Văn Xú', N'0906234164', N'ban164@gmail.com', NULL, 0, N'banuser164', N'123456', '2025-04-13', 1, NULL, 0, 0, N'Đánh giá tiêu cực sai sự thật', NULL),
(N'Hoàng Tổ', N'0906234165', N'ban165@gmail.com', NULL, 250, N'banuser165', N'123456', '2025-05-14', 1, NULL, 0, 0, N'Không hoàn trả sách thuê quá 6 tháng', '2027-01-01'),
(N'Hoa Hùng', N'0906234166', N'ban166@gmail.com', NULL, 5, N'banuser166', N'123456', '2025-06-15', 1, NULL, 0, 0, N'Spam đặt bàn ảo', '2026-06-20'),
(N'Hạ Hầu Đôn', N'0906234167', N'ban167@gmail.com', NULL, 110, N'banuser167', N'123456', '2025-07-16', 1, NULL, 0, 0, N'Buff điểm tích lũy trái phép', NULL),
(N'Hạ Hầu Uyên', N'0906234168', N'ban168@gmail.com', NULL, 30, N'banuser168', N'123456', '2025-08-17', 1, NULL, 0, 0, N'Làm đổ nước lên kệ sách nhưng không đền', '2026-08-01'),
(N'Tào Hồng', N'0906234169', N'ban169@gmail.com', NULL, 0, N'banuser169', N'123456', '2025-09-18', 1, NULL, 0, 0, N'Tạo nhiều account nhận khuyến mãi tân thủ', NULL),
(N'Tào Nhân', N'0906234170', N'ban170@gmail.com', NULL, 310, N'banuser170', N'123456', '2025-10-19', 1, NULL, 0, 0, N'Quấy rối khách hàng khác tại quán', '2026-07-01'),
(N'Lý Điển', N'0906234171', N'ban171@gmail.com', NULL, 0, N'banuser171', N'123456', '2025-11-20', 1, NULL, 0, 0, N'Đặt hàng số lượng lớn rồi không nhận', '2026-09-25'),
(N'Nhạc Tiến', N'0906234172', N'ban172@gmail.com', NULL, 40, N'banuser172', N'123456', '2025-12-01', 1, NULL, 0, 0, N'Tạo feedback ảo trên website', NULL),
(N'Vu Cấm', N'0906234173', N'ban173@gmail.com', NULL, 25, N'banuser173', N'123456', '2026-01-05', 1, NULL, 0, 0, N'Boom đơn', '2026-11-01'),
(N'Trình Dục', N'0906234174', N'ban174@gmail.com', NULL, 0, N'banuser174', N'123456', '2026-02-10', 1, NULL, 0, 0, N'Spam tin nhắn cho CSKH', NULL),
(N'Bàng Đức', N'0906234175', N'ban175@gmail.com', NULL, 15, N'banuser175', N'123456', '2026-02-12', 1, NULL, 0, 0, N'Chửi bới trên kênh chat', '2026-06-15'),
(N'Hứa Chử', N'0906234176', N'ban176@gmail.com', NULL, 80, N'banuser176', N'123456', '2026-03-15', 1, NULL, 0, 0, N'Nợ tiền sách quá hạn', NULL),
(N'Điển Vi', N'0906234177', N'ban177@gmail.com', NULL, 0, N'banuser177', N'123456', '2026-03-18', 1, NULL, 0, 0, N'Tài khoản giả mạo', NULL),
(N'Từ Hoảng', N'0906234178', N'ban178@gmail.com', NULL, 50, N'banuser178', N'123456', '2026-04-20', 1, NULL, 0, 0, N'Boom hàng', '2026-09-01'),
(N'Quách Gia', N'0906234179', N'ban179@gmail.com', NULL, 10, N'banuser179', N'123456', '2026-04-25', 1, NULL, 0, 0, N'Tấn công DDoS vào web quán', NULL),
(N'Tuân Úc', N'0906234180', N'ban180@gmail.com', NULL, 0, N'banuser180', N'123456', '2026-04-28', 1, NULL, 0, 0, N'Gian lận voucher', '2026-08-30'),

-- ====================================================================================
-- NHÓM 5: KHÁCH HÀNG ĐÃ XÓA (Soft delete 181 - 200)
-- ====================================================================================
(N'Lữ Mông', N'0906234181', N'del181@gmail.com', NULL, 0, N'deluser181', N'123456', '2025-01-05', 0, NULL, 0, 1, NULL, NULL),
(N'Lục Tốn', N'0906234182', N'del182@gmail.com', NULL, 0, N'deluser182', N'123456', '2025-02-10', 0, NULL, 0, 1, NULL, NULL),
(N'Cam Ninh', N'0906234183', N'del183@gmail.com', NULL, 70, N'deluser183', N'123456', '2025-03-15', 0, NULL, 0, 1, NULL, NULL),
(N'Chu Du', N'0906234184', N'del184@gmail.com', NULL, 0, N'deluser184', N'123456', '2025-04-20', 0, NULL, 0, 1, NULL, NULL),
(N'Hoàng Cái', N'0906234185', N'del185@gmail.com', NULL, 140, N'deluser185', N'123456', '2025-05-25', 0, NULL, 0, 1, NULL, NULL),
(N'Trình Phổ', N'0906234186', N'del186@gmail.com', NULL, 0, N'deluser186', N'123456', '2025-06-30', 0, NULL, 0, 1, NULL, NULL),
(N'Hàn Đương', N'0906234187', N'del187@gmail.com', NULL, 0, N'deluser187', N'123456', '2025-07-05', 0, NULL, 0, 1, NULL, NULL),
(N'Tổ Mậu', N'0906234188', N'del188@gmail.com', NULL, 15, N'deluser188', N'123456', '2025-08-10', 0, NULL, 0, 1, NULL, NULL),
(N'Tôn Kiên', N'0906234189', N'del189@gmail.com', NULL, 0, N'deluser189', N'123456', '2025-09-15', 0, NULL, 0, 1, NULL, NULL),
(N'Tôn Sách', N'0906234190', N'del190@gmail.com', NULL, 60, N'deluser190', N'123456', '2025-10-20', 0, NULL, 0, 1, NULL, NULL),
(N'Tôn Quyền', N'0906234191', N'del191@gmail.com', NULL, 0, N'deluser191', N'123456', '2025-11-25', 0, NULL, 0, 1, NULL, NULL),
(N'Lỗ Túc', N'0906234192', N'del192@gmail.com', NULL, 0, N'deluser192', N'123456', '2025-12-30', 0, NULL, 0, 1, NULL, NULL),
(N'Đinh Phụng', N'0906234193', N'del193@gmail.com', NULL, 90, N'deluser193', N'123456', '2026-01-04', 0, NULL, 0, 1, NULL, NULL),
(N'Từ Thịnh', N'0906234194', N'del194@gmail.com', NULL, 0, N'deluser194', N'123456', '2026-02-09', 0, NULL, 0, 1, NULL, NULL),
(N'Tưởng Khâm', N'0906234195', N'del195@gmail.com', NULL, 0, N'deluser195', N'123456', '2026-03-14', 0, NULL, 0, 1, NULL, NULL),
(N'Phan Chương', N'0906234196', N'del196@gmail.com', NULL, 40, N'deluser196', N'123456', '2026-04-01', 0, NULL, 0, 1, NULL, NULL),
(N'Chu Thái', N'0906234197', N'del197@gmail.com', NULL, 0, N'deluser197', N'123456', '2026-04-10', 0, NULL, 0, 1, NULL, NULL),
(N'Trần Vũ', N'0906234198', N'del198@gmail.com', NULL, 0, N'deluser198', N'123456', '2026-04-15', 0, NULL, 0, 1, NULL, NULL),
(N'Lăng Thống', N'0906234199', N'del199@gmail.com', NULL, 25, N'deluser199', N'123456', '2026-04-20', 0, NULL, 0, 1, NULL, NULL),
(N'Đổng Tập', N'0906234200', N'del200@gmail.com', NULL, 0, N'deluser200', N'123456', '2026-04-25', 0, NULL, 0, 1, NULL, NULL);
GO

USE [CafebookDB]
GO

SET NOCOUNT ON;
PRINT N'Bắt đầu chèn 1000 khách hàng...';

-- Bắt đầu từ 201 để không trùng với 200 dữ liệu đã chèn trước đó
DECLARE @i INT = 201; 
DECLARE @max INT = 1200; 

-- Khai báo các biến lưu trữ dữ liệu
DECLARE @hoTen NVARCHAR(255);
DECLARE @soDienThoai NVARCHAR(20);
DECLARE @email NVARCHAR(100);
DECLARE @diaChi NVARCHAR(500);
DECLARE @diemTichLuy INT;
DECLARE @tenDangNhap NVARCHAR(100);
DECLARE @matKhau NVARCHAR(255) = N'123456';
DECLARE @ngayTao DATETIME;
DECLARE @BiKhoa BIT;
DECLARE @AnhDaiDien NVARCHAR(MAX) = NULL;
DECLARE @taiKhoanTam BIT;
DECLARE @DaXoa BIT;
DECLARE @lyDoKhoa NVARCHAR(500);
DECLARE @thoiGianMoKhoa DATETIME;

WHILE @i <= @max
BEGIN
    -- 1. Tự động sinh dữ liệu ĐỘC NHẤT (Unique) cho mỗi vòng lặp
    -- VD: 0910000201, khachhang201@cafebook.local, user201
    SET @soDienThoai = '091' + RIGHT('0000000' + CAST(@i AS VARCHAR(7)), 7);
    SET @email = 'khachhang' + CAST(@i AS VARCHAR(10)) + '@cafebook.local';
    SET @tenDangNhap = 'user' + CAST(@i AS VARCHAR(10));
    
    -- Ngày tạo rải rác ngẫu nhiên trong 365 ngày qua
    SET @ngayTao = DATEADD(DAY, -(@i % 365), GETDATE());
    SET @diaChi = CAST(@i AS NVARCHAR) + N' Nguyễn Văn Linh, Q. Hải Châu, TP. Đà Nẵng';
    
    -- Đặt lại giá trị mặc định cho các cờ (flag)
    SET @BiKhoa = 0;
    SET @taiKhoanTam = 0;
    SET @DaXoa = 0;
    SET @lyDoKhoa = NULL;
    SET @thoiGianMoKhoa = NULL;

    -- 2. Phân loại 1000 khách hàng ra 5 nhóm đều nhau (Mỗi nhóm 200 người)
    DECLARE @mod INT = @i % 5;

    IF @mod = 0 -- NHÓM 1: VIP
    BEGIN
        SET @hoTen = N'Khách VIP ' + CAST(@i AS NVARCHAR);
        SET @diemTichLuy = 1000 + (@i * 2); -- Điểm cao
    END
    ELSE IF @mod = 1 -- NHÓM 2: THƯỜNG
    BEGIN
        SET @hoTen = N'Khách Thường ' + CAST(@i AS NVARCHAR);
        SET @diemTichLuy = @i % 150; -- Điểm thấp (0 - 149)
    END
    ELSE IF @mod = 2 -- NHÓM 3: VÃNG LAI (Tài khoản tạm)
    BEGIN
        SET @hoTen = N'Khách Lẻ ' + CAST(@i AS NVARCHAR);
        SET @diemTichLuy = 0;
        SET @taiKhoanTam = 1;
        SET @tenDangNhap = 'guest_tmp_' + CAST(@i AS VARCHAR);
    END
    ELSE IF @mod = 3 -- NHÓM 4: BỊ KHÓA
    BEGIN
        SET @hoTen = N'Khách Vi Phạm ' + CAST(@i AS NVARCHAR);
        SET @diemTichLuy = @i % 50;
        SET @BiKhoa = 1;
        SET @lyDoKhoa = N'Hệ thống tự động khóa tài khoản vi phạm #' + CAST(@i AS NVARCHAR);
        
        -- Một nửa khóa có thời hạn, một nửa khóa vĩnh viễn
        IF (@i % 2 = 0)
            SET @thoiGianMoKhoa = DATEADD(MONTH, 2, GETDATE());
        ELSE
            SET @thoiGianMoKhoa = NULL; 
    END
    ELSE IF @mod = 4 -- NHÓM 5: ĐÃ XÓA (Soft Delete)
    BEGIN
        SET @hoTen = N'Khách Cũ ' + CAST(@i AS NVARCHAR);
        SET @diemTichLuy = 0;
        SET @DaXoa = 1;
    END

    -- 3. Thực thi chèn vào Database
    INSERT INTO [dbo].[KhachHang] 
    ([hoTen], [soDienThoai], [email], [diaChi], [diemTichLuy], [tenDangNhap], [matKhau], [ngayTao], [BiKhoa], [AnhDaiDien], [taiKhoanTam], [DaXoa], [lyDoKhoa], [thoiGianMoKhoa])
    VALUES
    (@hoTen, @soDienThoai, @email, @diaChi, @diemTichLuy, @tenDangNhap, @matKhau, @ngayTao, @BiKhoa, @AnhDaiDien, @taiKhoanTam, @DaXoa, @lyDoKhoa, @thoiGianMoKhoa);

    -- Tăng biến đếm
    SET @i = @i + 1;
END

PRINT N'Đã chèn thành công 1000 khách hàng!';
GO

USE [CafebookDB]
GO

-----------------------------------------------------------
-- 1. CHÈN DỮ LIỆU BẢNG: Thể Loại (Bắt đầu từ ID 101)
-----------------------------------------------------------
SET IDENTITY_INSERT [dbo].[TheLoai] ON;
INSERT INTO [dbo].[TheLoai] ([idTheLoai], [tenTheLoai], [MoTa]) VALUES 
(101, N'Tiểu thuyết', N'Các tác phẩm văn học hư cấu dài, có cốt truyện.'),
(102, N'Kỹ năng sống', N'Sách phát triển bản thân, giao tiếp, tâm lý.'),
(103, N'Kinh doanh', N'Sách về quản lý, khởi nghiệp, đầu tư.'),
(104, N'Lịch sử', N'Sách về các sự kiện và nhân vật lịch sử.'),
(105, N'Khoa học viễn tưởng', N'Văn học giả tưởng dựa trên các ý tưởng khoa học.');
SET IDENTITY_INSERT [dbo].[TheLoai] OFF;
GO

-----------------------------------------------------------
-- 2. CHÈN DỮ LIỆU BẢNG: Tác Giả (Bắt đầu từ ID 101)
-----------------------------------------------------------
SET IDENTITY_INSERT [dbo].[TacGia] ON;
INSERT INTO [dbo].[TacGia] ([idTacGia], [tenTacGia], [gioiThieu]) VALUES 
(101, N'Nguyễn Nhật Ánh', N'Nhà văn nổi tiếng của Việt Nam, chuyên viết cho tuổi thơ.'),
(102, N'Dale Carnegie', N'Nhà văn và nhà thuyết trình người Mỹ.'),
(103, N'Paulo Coelho', N'Tiểu thuyết gia người Brazil.'),
(104, N'Yuval Noah Harari', N'Nhà sử học người Israel.'),
(105, N'Robert Kiyosaki', N'Tác giả bộ sách Dạy Con Làm Giàu.'),
(106, N'J.K. Rowling', N'Tác giả bộ truyện Harry Potter.'),
(107, N'Nam Cao', N'Nhà văn hiện thực xuất sắc của Việt Nam.'),
(108, N'George R.R. Martin', N'Tác giả Trò chơi vương quyền.');
SET IDENTITY_INSERT [dbo].[TacGia] OFF;
GO

-----------------------------------------------------------
-- 3. CHÈN DỮ LIỆU BẢNG: Nhà Xuất Bản (Bắt đầu từ ID 101)
-----------------------------------------------------------
SET IDENTITY_INSERT [dbo].[NhaXuatBan] ON;
INSERT INTO [dbo].[NhaXuatBan] ([idNhaXuatBan], [tenNhaXuatBan], [MoTa]) VALUES 
(101, N'NXB Trẻ', N'Nhà xuất bản Trẻ TP.HCM.'),
(102, N'NXB Kim Đồng', N'Nhà xuất bản dành cho thiếu nhi.'),
(103, N'NXB Tổng hợp TP.HCM', N'Nhà xuất bản uy tín tại TP.HCM.'),
(104, N'NXB Hội Nhà văn', N'Nhà xuất bản của Hội Nhà văn Việt Nam.'),
(105, N'NXB Phụ Nữ', N'Nhà xuất bản dành cho phụ nữ và gia đình.');
SET IDENTITY_INSERT [dbo].[NhaXuatBan] OFF;
GO

-----------------------------------------------------------
-- 4. CHÈN DỮ LIỆU BẢNG: Sách (100 đầu sách, ID 101-200)
-----------------------------------------------------------
SET IDENTITY_INSERT [dbo].[Sach] ON;
INSERT INTO [dbo].[Sach] ([idSach], [tenSach], [namXuatBan], [moTa], [soLuongTong], [soLuongHienCo], [AnhBia], [GiaBia], [ViTri]) VALUES 
(101, N'Đắc Nhân Tâm', 2020, N'Nghệ thuật thu phục lòng người.', 20, 20, NULL, 86000.00, N'Kệ A1'),
(102, N'Nhà Lãnh Đạo Không Chức Danh', 2018, N'Sách phát triển bản thân.', 15, 12, NULL, 75000.00, N'Kệ A2'),
(103, N'Tôi Thấy Hoa Vàng Trên Cỏ Xanh', 2015, N'Truyện dài của Nguyễn Nhật Ánh.', 30, 25, NULL, 90000.00, N'Kệ B1'),
(104, N'Mắt Biếc', 2016, N'Câu chuyện tình yêu tuổi học trò.', 25, 20, NULL, 85000.00, N'Kệ B1'),
(105, N'Cho Tôi Xin Một Vé Đi Tuổi Thơ', 2010, N'Hành trình về tuổi thơ.', 40, 38, NULL, 70000.00, N'Kệ B2'),
(106, N'Nhà Giả Kim', 2019, N'Hành trình theo đuổi ước mơ.', 50, 45, NULL, 65000.00, N'Kệ A3'),
(107, N'Sapiens: Lược Sử Loài Người', 2018, N'Sách tóm tắt lịch sử tiến hóa.', 20, 15, NULL, 150000.00, N'Kệ C1'),
(108, N'Cha Giàu Cha Nghèo', 2017, N'Sách tư duy tài chính.', 35, 30, NULL, 95000.00, N'Kệ D1'),
(109, N'Harry Potter và Hòn Đá Phù Thủy', 2000, N'Tập 1 bộ truyện Harry Potter.', 10, 8, NULL, 120000.00, N'Kệ E1'),
(110, N'Chí Phèo', 2012, N'Tuyển tập truyện ngắn Nam Cao.', 20, 18, NULL, 55000.00, N'Kệ B3'),
(111, N'Sách Văn Học Ký Sự 11', 2021, N'Mô tả sách văn học', 10, 10, NULL, 60000.00, N'Kệ F1'),
(112, N'Sách Văn Học Ký Sự 12', 2021, N'Mô tả sách văn học', 10, 10, NULL, 60000.00, N'Kệ F1'),
(113, N'Sách Văn Học Ký Sự 13', 2021, N'Mô tả sách văn học', 10, 10, NULL, 60000.00, N'Kệ F1'),
(114, N'Sách Văn Học Ký Sự 14', 2021, N'Mô tả sách văn học', 10, 10, NULL, 60000.00, N'Kệ F1'),
(115, N'Sách Văn Học Ký Sự 15', 2021, N'Mô tả sách văn học', 10, 10, NULL, 60000.00, N'Kệ F1'),
(116, N'Sách Phát Triển Bản Thân 16', 2022, N'Mô tả sách kỹ năng', 15, 15, NULL, 75000.00, N'Kệ F2'),
(117, N'Sách Phát Triển Bản Thân 17', 2022, N'Mô tả sách kỹ năng', 15, 15, NULL, 75000.00, N'Kệ F2'),
(118, N'Sách Phát Triển Bản Thân 18', 2022, N'Mô tả sách kỹ năng', 15, 15, NULL, 75000.00, N'Kệ F2'),
(119, N'Sách Phát Triển Bản Thân 19', 2022, N'Mô tả sách kỹ năng', 15, 15, NULL, 75000.00, N'Kệ F2'),
(120, N'Sách Phát Triển Bản Thân 20', 2022, N'Mô tả sách kỹ năng', 15, 15, NULL, 75000.00, N'Kệ F2'),
(121, N'Lịch Sử Thế Giới Cổ Đại 21', 2019, N'Kiến thức lịch sử', 8, 8, NULL, 110000.00, N'Kệ F3'),
(122, N'Lịch Sử Thế Giới Cổ Đại 22', 2019, N'Kiến thức lịch sử', 8, 8, NULL, 110000.00, N'Kệ F3'),
(123, N'Lịch Sử Thế Giới Cổ Đại 23', 2019, N'Kiến thức lịch sử', 8, 8, NULL, 110000.00, N'Kệ F3'),
(124, N'Lịch Sử Thế Giới Cổ Đại 24', 2019, N'Kiến thức lịch sử', 8, 8, NULL, 110000.00, N'Kệ F3'),
(125, N'Lịch Sử Thế Giới Cổ Đại 25', 2019, N'Kiến thức lịch sử', 8, 8, NULL, 110000.00, N'Kệ F3'),
(126, N'Kinh Doanh Thời 4.0 Tập 26', 2023, N'Mô tả sách kinh doanh', 20, 20, NULL, 130000.00, N'Kệ F4'),
(127, N'Kinh Doanh Thời 4.0 Tập 27', 2023, N'Mô tả sách kinh doanh', 20, 20, NULL, 130000.00, N'Kệ F4'),
(128, N'Kinh Doanh Thời 4.0 Tập 28', 2023, N'Mô tả sách kinh doanh', 20, 20, NULL, 130000.00, N'Kệ F4'),
(129, N'Kinh Doanh Thời 4.0 Tập 29', 2023, N'Mô tả sách kinh doanh', 20, 20, NULL, 130000.00, N'Kệ F4'),
(130, N'Kinh Doanh Thời 4.0 Tập 30', 2023, N'Mô tả sách kinh doanh', 20, 20, NULL, 130000.00, N'Kệ F4'),
(131, N'Tác phẩm Kinh Điển 31', 2005, N'Văn học', 5, 5, NULL, 50000.00, N'Kệ G1'),
(132, N'Tác phẩm Kinh Điển 32', 2005, N'Văn học', 5, 5, NULL, 50000.00, N'Kệ G1'),
(133, N'Tác phẩm Kinh Điển 33', 2005, N'Văn học', 5, 5, NULL, 50000.00, N'Kệ G1'),
(134, N'Tác phẩm Kinh Điển 34', 2005, N'Văn học', 5, 5, NULL, 50000.00, N'Kệ G1'),
(135, N'Tác phẩm Kinh Điển 35', 2005, N'Văn học', 5, 5, NULL, 50000.00, N'Kệ G1'),
(136, N'Tác phẩm Kinh Điển 36', 2005, N'Văn học', 5, 5, NULL, 50000.00, N'Kệ G1'),
(137, N'Tác phẩm Kinh Điển 37', 2005, N'Văn học', 5, 5, NULL, 50000.00, N'Kệ G1'),
(138, N'Tác phẩm Kinh Điển 38', 2005, N'Văn học', 5, 5, NULL, 50000.00, N'Kệ G1'),
(139, N'Tác phẩm Kinh Điển 39', 2005, N'Văn học', 5, 5, NULL, 50000.00, N'Kệ G1'),
(140, N'Tác phẩm Kinh Điển 40', 2005, N'Văn học', 5, 5, NULL, 50000.00, N'Kệ G1'),
(141, N'Cẩm nang vào bếp 41', 2020, N'Nấu ăn', 12, 12, NULL, 65000.00, N'Kệ G2'),
(142, N'Cẩm nang vào bếp 42', 2020, N'Nấu ăn', 12, 12, NULL, 65000.00, N'Kệ G2'),
(143, N'Cẩm nang vào bếp 43', 2020, N'Nấu ăn', 12, 12, NULL, 65000.00, N'Kệ G2'),
(144, N'Cẩm nang vào bếp 44', 2020, N'Nấu ăn', 12, 12, NULL, 65000.00, N'Kệ G2'),
(145, N'Cẩm nang vào bếp 45', 2020, N'Nấu ăn', 12, 12, NULL, 65000.00, N'Kệ G2'),
(146, N'Cẩm nang vào bếp 46', 2020, N'Nấu ăn', 12, 12, NULL, 65000.00, N'Kệ G2'),
(147, N'Cẩm nang vào bếp 47', 2020, N'Nấu ăn', 12, 12, NULL, 65000.00, N'Kệ G2'),
(148, N'Cẩm nang vào bếp 48', 2020, N'Nấu ăn', 12, 12, NULL, 65000.00, N'Kệ G2'),
(149, N'Cẩm nang vào bếp 49', 2020, N'Nấu ăn', 12, 12, NULL, 65000.00, N'Kệ G2'),
(150, N'Cẩm nang vào bếp 50', 2020, N'Nấu ăn', 12, 12, NULL, 65000.00, N'Kệ G2'),
(151, N'Khoa học vũ trụ 51', 2018, N'Vũ trụ', 7, 7, NULL, 99000.00, N'Kệ G3'),
(152, N'Khoa học vũ trụ 52', 2018, N'Vũ trụ', 7, 7, NULL, 99000.00, N'Kệ G3'),
(153, N'Khoa học vũ trụ 53', 2018, N'Vũ trụ', 7, 7, NULL, 99000.00, N'Kệ G3'),
(154, N'Khoa học vũ trụ 54', 2018, N'Vũ trụ', 7, 7, NULL, 99000.00, N'Kệ G3'),
(155, N'Khoa học vũ trụ 55', 2018, N'Vũ trụ', 7, 7, NULL, 99000.00, N'Kệ G3'),
(156, N'Khoa học vũ trụ 56', 2018, N'Vũ trụ', 7, 7, NULL, 99000.00, N'Kệ G3'),
(157, N'Khoa học vũ trụ 57', 2018, N'Vũ trụ', 7, 7, NULL, 99000.00, N'Kệ G3'),
(158, N'Khoa học vũ trụ 58', 2018, N'Vũ trụ', 7, 7, NULL, 99000.00, N'Kệ G3'),
(159, N'Khoa học vũ trụ 59', 2018, N'Vũ trụ', 7, 7, NULL, 99000.00, N'Kệ G3'),
(160, N'Khoa học vũ trụ 60', 2018, N'Vũ trụ', 7, 7, NULL, 99000.00, N'Kệ G3'),
(161, N'Hành trình tâm linh 61', 2021, N'Tâm linh', 15, 15, NULL, 80000.00, N'Kệ H1'),
(162, N'Hành trình tâm linh 62', 2021, N'Tâm linh', 15, 15, NULL, 80000.00, N'Kệ H1'),
(163, N'Hành trình tâm linh 63', 2021, N'Tâm linh', 15, 15, NULL, 80000.00, N'Kệ H1'),
(164, N'Hành trình tâm linh 64', 2021, N'Tâm linh', 15, 15, NULL, 80000.00, N'Kệ H1'),
(165, N'Hành trình tâm linh 65', 2021, N'Tâm linh', 15, 15, NULL, 80000.00, N'Kệ H1'),
(166, N'Hành trình tâm linh 66', 2021, N'Tâm linh', 15, 15, NULL, 80000.00, N'Kệ H1'),
(167, N'Hành trình tâm linh 67', 2021, N'Tâm linh', 15, 15, NULL, 80000.00, N'Kệ H1'),
(168, N'Hành trình tâm linh 68', 2021, N'Tâm linh', 15, 15, NULL, 80000.00, N'Kệ H1'),
(169, N'Hành trình tâm linh 69', 2021, N'Tâm linh', 15, 15, NULL, 80000.00, N'Kệ H1'),
(170, N'Hành trình tâm linh 70', 2021, N'Tâm linh', 15, 15, NULL, 80000.00, N'Kệ H1'),
(171, N'Mật mã Da Vinci 71', 2008, N'Tiểu thuyết trinh thám', 22, 22, NULL, 120000.00, N'Kệ H2'),
(172, N'Mật mã Da Vinci 72', 2008, N'Tiểu thuyết trinh thám', 22, 22, NULL, 120000.00, N'Kệ H2'),
(173, N'Mật mã Da Vinci 73', 2008, N'Tiểu thuyết trinh thám', 22, 22, NULL, 120000.00, N'Kệ H2'),
(174, N'Mật mã Da Vinci 74', 2008, N'Tiểu thuyết trinh thám', 22, 22, NULL, 120000.00, N'Kệ H2'),
(175, N'Mật mã Da Vinci 75', 2008, N'Tiểu thuyết trinh thám', 22, 22, NULL, 120000.00, N'Kệ H2'),
(176, N'Mật mã Da Vinci 76', 2008, N'Tiểu thuyết trinh thám', 22, 22, NULL, 120000.00, N'Kệ H2'),
(177, N'Mật mã Da Vinci 77', 2008, N'Tiểu thuyết trinh thám', 22, 22, NULL, 120000.00, N'Kệ H2'),
(178, N'Mật mã Da Vinci 78', 2008, N'Tiểu thuyết trinh thám', 22, 22, NULL, 120000.00, N'Kệ H2'),
(179, N'Mật mã Da Vinci 79', 2008, N'Tiểu thuyết trinh thám', 22, 22, NULL, 120000.00, N'Kệ H2'),
(180, N'Mật mã Da Vinci 80', 2008, N'Tiểu thuyết trinh thám', 22, 22, NULL, 120000.00, N'Kệ H2'),
(181, N'Nghệ thuật Sống 81', 2022, N'Tâm lý học', 18, 18, NULL, 70000.00, N'Kệ I1'),
(182, N'Nghệ thuật Sống 82', 2022, N'Tâm lý học', 18, 18, NULL, 70000.00, N'Kệ I1'),
(183, N'Nghệ thuật Sống 83', 2022, N'Tâm lý học', 18, 18, NULL, 70000.00, N'Kệ I1'),
(184, N'Nghệ thuật Sống 84', 2022, N'Tâm lý học', 18, 18, NULL, 70000.00, N'Kệ I1'),
(185, N'Nghệ thuật Sống 85', 2022, N'Tâm lý học', 18, 18, NULL, 70000.00, N'Kệ I1'),
(186, N'Nghệ thuật Sống 86', 2022, N'Tâm lý học', 18, 18, NULL, 70000.00, N'Kệ I1'),
(187, N'Nghệ thuật Sống 87', 2022, N'Tâm lý học', 18, 18, NULL, 70000.00, N'Kệ I1'),
(188, N'Nghệ thuật Sống 88', 2022, N'Tâm lý học', 18, 18, NULL, 70000.00, N'Kệ I1'),
(189, N'Nghệ thuật Sống 89', 2022, N'Tâm lý học', 18, 18, NULL, 70000.00, N'Kệ I1'),
(190, N'Nghệ thuật Sống 90', 2022, N'Tâm lý học', 18, 18, NULL, 70000.00, N'Kệ I1'),
(191, N'Hành Trang Tuổi Trẻ 91', 2023, N'Định hướng', 25, 25, NULL, 68000.00, N'Kệ I2'),
(192, N'Hành Trang Tuổi Trẻ 92', 2023, N'Định hướng', 25, 25, NULL, 68000.00, N'Kệ I2'),
(193, N'Hành Trang Tuổi Trẻ 93', 2023, N'Định hướng', 25, 25, NULL, 68000.00, N'Kệ I2'),
(194, N'Hành Trang Tuổi Trẻ 94', 2023, N'Định hướng', 25, 25, NULL, 68000.00, N'Kệ I2'),
(195, N'Hành Trang Tuổi Trẻ 95', 2023, N'Định hướng', 25, 25, NULL, 68000.00, N'Kệ I2'),
(196, N'Hành Trang Tuổi Trẻ 96', 2023, N'Định hướng', 25, 25, NULL, 68000.00, N'Kệ I2'),
(197, N'Hành Trang Tuổi Trẻ 97', 2023, N'Định hướng', 25, 25, NULL, 68000.00, N'Kệ I2'),
(198, N'Hành Trang Tuổi Trẻ 98', 2023, N'Định hướng', 25, 25, NULL, 68000.00, N'Kệ I2'),
(199, N'Hành Trang Tuổi Trẻ 99', 2023, N'Định hướng', 25, 25, NULL, 68000.00, N'Kệ I2'),
(200, N'Hành Trang Tuổi Trẻ 100', 2023, N'Định hướng', 25, 25, NULL, 68000.00, N'Kệ I2');
SET IDENTITY_INSERT [dbo].[Sach] OFF;
GO

-----------------------------------------------------------
-- 5. CHÈN BẢNG LIÊN KẾT: Sach_TacGia (Dùng ID từ 101)
-----------------------------------------------------------
INSERT INTO [dbo].[Sach_TacGia] ([idSach], [idTacGia]) VALUES 
(101, 102), (102, 102), (103, 101), (104, 101), (105, 101),
(106, 103), (107, 104), (108, 105), (109, 106), (110, 107),
(111, 107), (112, 107), (113, 107), (114, 107), (115, 107),
(116, 102), (117, 102), (118, 102), (119, 102), (120, 102),
(121, 104), (122, 104), (123, 104), (124, 104), (125, 104),
(126, 105), (127, 105), (128, 105), (129, 105), (130, 105),
(131, 107), (132, 107), (133, 107), (134, 107), (135, 107),
(136, 107), (137, 107), (138, 107), (139, 107), (140, 107),
(141, 101), (142, 101), (143, 101), (144, 101), (145, 101),
(146, 101), (147, 101), (148, 101), (149, 101), (150, 101),
(151, 104), (152, 104), (153, 104), (154, 104), (155, 104),
(156, 104), (157, 104), (158, 104), (159, 104), (160, 104),
(161, 103), (162, 103), (163, 103), (164, 103), (165, 103),
(166, 103), (167, 103), (168, 103), (169, 103), (170, 103),
(171, 108), (172, 108), (173, 108), (174, 108), (175, 108),
(176, 108), (177, 108), (178, 108), (179, 108), (180, 108),
(181, 102), (182, 102), (183, 102), (184, 102), (185, 102),
(186, 102), (187, 102), (188, 102), (189, 102), (190, 102),
(191, 101), (192, 101), (193, 101), (194, 101), (195, 101),
(196, 101), (197, 101), (198, 101), (199, 101), (200, 101);
GO

-----------------------------------------------------------
-- 6. CHÈN BẢNG LIÊN KẾT: Sach_NhaXuatBan (Dùng ID từ 101)
-----------------------------------------------------------
INSERT INTO [dbo].[Sach_NhaXuatBan] ([idSach], [idNhaXuatBan]) VALUES 
(101, 103), (102, 103), (103, 101), (104, 101), (105, 101),
(106, 104), (107, 103), (108, 101), (109, 101), (110, 104),
(111, 104), (112, 104), (113, 104), (114, 104), (115, 104),
(116, 103), (117, 103), (118, 103), (119, 103), (120, 103),
(121, 103), (122, 103), (123, 103), (124, 103), (125, 103),
(126, 101), (127, 101), (128, 101), (129, 101), (130, 101),
(131, 104), (132, 104), (133, 104), (134, 104), (135, 104),
(136, 104), (137, 104), (138, 104), (139, 104), (140, 104),
(141, 105), (142, 105), (143, 105), (144, 105), (145, 105),
(146, 105), (147, 105), (148, 105), (149, 105), (150, 105),
(151, 103), (152, 103), (153, 103), (154, 103), (155, 103),
(156, 103), (157, 103), (158, 103), (159, 103), (160, 103),
(161, 101), (162, 101), (163, 101), (164, 101), (165, 101),
(166, 101), (167, 101), (168, 101), (169, 101), (170, 101),
(171, 103), (172, 103), (173, 103), (174, 103), (175, 103),
(176, 103), (177, 103), (178, 103), (179, 103), (180, 103),
(181, 101), (182, 101), (183, 101), (184, 101), (185, 101),
(186, 101), (187, 101), (188, 101), (189, 101), (190, 101),
(191, 102), (192, 102), (193, 102), (194, 102), (195, 102),
(196, 102), (197, 102), (198, 102), (199, 102), (200, 102);
GO

-----------------------------------------------------------
-- 7. CHÈN BẢNG LIÊN KẾT: Sach_TheLoai (Dùng ID từ 101)
-----------------------------------------------------------
INSERT INTO [dbo].[Sach_TheLoai] ([idSach], [idTheLoai]) VALUES 
(101, 102), (102, 102), (103, 101), (104, 101), (105, 101),
(106, 101), (107, 104), (108, 103), (109, 105), (110, 101),
(111, 101), (112, 101), (113, 101), (114, 101), (115, 101),
(116, 102), (117, 102), (118, 102), (119, 102), (120, 102),
(121, 104), (122, 104), (123, 104), (124, 104), (125, 104),
(126, 103), (127, 103), (128, 103), (129, 103), (130, 103),
(131, 101), (132, 101), (133, 101), (134, 101), (135, 101),
(136, 101), (137, 101), (138, 101), (139, 101), (140, 101),
(141, 102), (142, 102), (143, 102), (144, 102), (145, 102),
(146, 102), (147, 102), (148, 102), (149, 102), (150, 102),
(151, 105), (152, 105), (153, 105), (154, 105), (155, 105),
(156, 105), (157, 105), (158, 105), (159, 105), (160, 105),
(161, 102), (162, 102), (163, 102), (164, 102), (165, 102),
(166, 102), (167, 102), (168, 102), (169, 102), (170, 102),
(171, 101), (172, 101), (173, 101), (174, 101), (175, 101),
(176, 101), (177, 101), (178, 101), (179, 101), (180, 101),
(181, 102), (182, 102), (183, 102), (184, 102), (185, 102),
(186, 102), (187, 102), (188, 102), (189, 102), (190, 102),
(191, 102), (192, 102), (193, 102), (194, 102), (195, 102),
(196, 102), (197, 102), (198, 102), (199, 102), (200, 102);
GO

USE [CafebookDB]
GO

SET NOCOUNT ON;

-- Khai báo số lượng sách cần tạo
DECLARE @TotalBooks INT = 3000;
DECLARE @Counter INT = 1;

-- Khai báo các biến lưu trữ dữ liệu ngẫu nhiên
DECLARE @RandomTacGia INT;
DECLARE @RandomNXB INT;
DECLARE @RandomTheLoai INT;
DECLARE @NewSachID INT;

DECLARE @TenSach NVARCHAR(500);
DECLARE @NamXB INT;
DECLARE @SoLuong INT;
DECLARE @GiaBia DECIMAL(18,2);
DECLARE @ViTri NVARCHAR(100);

PRINT N'Đang tiến hành tạo 3000 sách ngẫu nhiên...';

-- Đưa vào Transaction để tăng tốc độ chạy và đảm bảo an toàn dữ liệu
BEGIN TRANSACTION;

WHILE @Counter <= @TotalBooks
BEGIN
    -- 1. Sinh dữ liệu ngẫu nhiên cho Sách
    -- Tên sách ngẫu nhiên để không bị trùng lặp
    SET @TenSach = N'Đầu Sách Ngẫu Nhiên Số ' + CAST(ABS(CHECKSUM(NEWID())) % 99999 AS NVARCHAR) + N' - ' + CAST(@Counter AS NVARCHAR);
    
    -- Năm xuất bản từ 2000 đến 2024
    SET @NamXB = 2000 + (ABS(CHECKSUM(NEWID())) % 25);
    
    -- Số lượng từ 10 đến 59 cuốn
    SET @SoLuong = ABS(CHECKSUM(NEWID())) % 50 + 10; 
    
    -- Giá bìa từ 50.000đ đến 249.000đ
    SET @GiaBia = (ABS(CHECKSUM(NEWID())) % 200 + 50) * 1000; 
    
    -- Vị trí kệ sách từ Kệ A đến Kệ H, số từ 1 đến 5
    SET @ViTri = N'Kệ ' + CHAR(65 + ABS(CHECKSUM(NEWID())) % 8) + CAST((ABS(CHECKSUM(NEWID())) % 5 + 1) AS NVARCHAR);

    -- 2. Lấy ID ngẫu nhiên TỪ NHỮNG DỮ LIỆU ĐÃ CÓ TRONG BẢNG
    SELECT TOP 1 @RandomTacGia = idTacGia FROM TacGia ORDER BY NEWID();
    SELECT TOP 1 @RandomNXB = idNhaXuatBan FROM NhaXuatBan ORDER BY NEWID();
    SELECT TOP 1 @RandomTheLoai = idTheLoai FROM TheLoai ORDER BY NEWID();

    -- 3. Chèn vào bảng Sach (không chèn idSach vì hệ thống tự tăng IDENTITY)
    INSERT INTO [dbo].[Sach] ([tenSach], [namXuatBan], [moTa], [soLuongTong], [soLuongHienCo], [GiaBia], [ViTri])
    VALUES (@TenSach, @NamXB, N'Đây là mô tả tự động sinh ra cho cuốn: ' + @TenSach, @SoLuong, @SoLuong, @GiaBia, @ViTri);

    -- Lấy ID của cuốn sách vừa được thêm vào
    SET @NewSachID = SCOPE_IDENTITY();

    -- 4. Chèn vào các bảng liên kết (Chỉ chèn nếu tìm thấy ID tham chiếu hợp lệ)
    IF @RandomTacGia IS NOT NULL
        INSERT INTO [dbo].[Sach_TacGia] ([idSach], [idTacGia]) VALUES (@NewSachID, @RandomTacGia);
        
    IF @RandomNXB IS NOT NULL
        INSERT INTO [dbo].[Sach_NhaXuatBan] ([idSach], [idNhaXuatBan]) VALUES (@NewSachID, @RandomNXB);
        
    IF @RandomTheLoai IS NOT NULL
        INSERT INTO [dbo].[Sach_TheLoai] ([idSach], [idTheLoai]) VALUES (@NewSachID, @RandomTheLoai);

    -- Tăng biến đếm
    SET @Counter = @Counter + 1;
END

-- Xác nhận lưu thay đổi
COMMIT TRANSACTION;

PRINT N'Hoàn tất! Đã chèn thành công ' + CAST(@TotalBooks AS NVARCHAR) + N' đầu sách vào hệ thống.';
GO

USE [CafebookDB]
GO

SET NOCOUNT ON;

-- Khai báo số lượng sách cần tạo thêm
DECLARE @TotalBooks INT = 5000;
DECLARE @Counter INT = 1;

-- Khai báo các biến lưu trữ dữ liệu ngẫu nhiên
DECLARE @RandomTacGia INT;
DECLARE @RandomNXB INT;
DECLARE @RandomTheLoai INT;
DECLARE @NewSachID INT;

DECLARE @TenSach NVARCHAR(500);
DECLARE @NamXB INT;
DECLARE @SoLuong INT;
DECLARE @GiaBia DECIMAL(18,2);
DECLARE @ViTri NVARCHAR(100);

PRINT N'Đang tiến hành tạo thêm 5000 sách ngẫu nhiên...';

-- Đưa vào Transaction để tăng tốc độ chạy và đảm bảo an toàn dữ liệu
BEGIN TRANSACTION;

WHILE @Counter <= @TotalBooks
BEGIN
    -- 1. Sinh dữ liệu ngẫu nhiên cho Sách
    -- Đổi tiền tố để dễ phân biệt với lô 3000 cuốn trước đó
    SET @TenSach = N'Sách Bổ Sung 5K - ' + CAST(ABS(CHECKSUM(NEWID())) % 999999 AS NVARCHAR) + N' - ' + CAST(@Counter AS NVARCHAR);
    
    -- Năm xuất bản từ 2000 đến 2024
    SET @NamXB = 2000 + (ABS(CHECKSUM(NEWID())) % 25);
    
    -- Số lượng từ 10 đến 59 cuốn
    SET @SoLuong = ABS(CHECKSUM(NEWID())) % 50 + 10; 
    
    -- Giá bìa từ 50.000đ đến 249.000đ
    SET @GiaBia = (ABS(CHECKSUM(NEWID())) % 200 + 50) * 1000; 
    
    -- Vị trí kệ sách từ Kệ A đến Kệ H, số từ 1 đến 5
    SET @ViTri = N'Kệ ' + CHAR(65 + ABS(CHECKSUM(NEWID())) % 8) + CAST((ABS(CHECKSUM(NEWID())) % 5 + 1) AS NVARCHAR);

    -- 2. Lấy ID ngẫu nhiên TỪ NHỮNG DỮ LIỆU ĐÃ CÓ TRONG BẢNG
    SELECT TOP 1 @RandomTacGia = idTacGia FROM TacGia ORDER BY NEWID();
    SELECT TOP 1 @RandomNXB = idNhaXuatBan FROM NhaXuatBan ORDER BY NEWID();
    SELECT TOP 1 @RandomTheLoai = idTheLoai FROM TheLoai ORDER BY NEWID();

    -- 3. Chèn vào bảng Sach (không chèn idSach vì hệ thống tự tăng IDENTITY)
    INSERT INTO [dbo].[Sach] ([tenSach], [namXuatBan], [moTa], [soLuongTong], [soLuongHienCo], [GiaBia], [ViTri])
    VALUES (@TenSach, @NamXB, N'Đây là mô tả tự động sinh ra cho cuốn (Lô 5000): ' + @TenSach, @SoLuong, @SoLuong, @GiaBia, @ViTri);

    -- Lấy ID của cuốn sách vừa được thêm vào
    SET @NewSachID = SCOPE_IDENTITY();

    -- 4. Chèn vào các bảng liên kết (Chỉ chèn nếu tìm thấy ID tham chiếu hợp lệ)
    IF @RandomTacGia IS NOT NULL
        INSERT INTO [dbo].[Sach_TacGia] ([idSach], [idTacGia]) VALUES (@NewSachID, @RandomTacGia);
        
    IF @RandomNXB IS NOT NULL
        INSERT INTO [dbo].[Sach_NhaXuatBan] ([idSach], [idNhaXuatBan]) VALUES (@NewSachID, @RandomNXB);
        
    IF @RandomTheLoai IS NOT NULL
        INSERT INTO [dbo].[Sach_TheLoai] ([idSach], [idTheLoai]) VALUES (@NewSachID, @RandomTheLoai);

    -- Tăng biến đếm
    SET @Counter = @Counter + 1;
END

-- Xác nhận lưu thay đổi
COMMIT TRANSACTION;

PRINT N'Hoàn tất! Đã chèn thành công ' + CAST(@TotalBooks AS NVARCHAR) + N' đầu sách mới vào hệ thống.';
GO

USE [CafebookDB]
GO

SET NOCOUNT ON;
PRINT N'Bắt đầu chèn thêm 1800 khách hàng (để tổng đạt 3000)...';

-- Bắt đầu từ 1201 để nối tiếp với 1200 khách hàng đã có
DECLARE @i INT = 1201; 
DECLARE @max INT = 3000; 

-- Khai báo các biến lưu trữ dữ liệu
DECLARE @hoTen NVARCHAR(255);
DECLARE @soDienThoai NVARCHAR(20);
DECLARE @email NVARCHAR(100);
DECLARE @diaChi NVARCHAR(500);
DECLARE @diemTichLuy INT;
DECLARE @tenDangNhap NVARCHAR(100);
DECLARE @matKhau NVARCHAR(255) = N'123456';
DECLARE @ngayTao DATETIME;
DECLARE @BiKhoa BIT;
DECLARE @AnhDaiDien NVARCHAR(MAX) = NULL;
DECLARE @taiKhoanTam BIT;
DECLARE @DaXoa BIT;
DECLARE @lyDoKhoa NVARCHAR(500);
DECLARE @thoiGianMoKhoa DATETIME;

-- Bọc trong Transaction để tăng tốc độ Insert
BEGIN TRANSACTION;

WHILE @i <= @max
BEGIN
    -- 1. Tự động sinh dữ liệu ĐỘC NHẤT
    SET @soDienThoai = '091' + RIGHT('0000000' + CAST(@i AS VARCHAR(7)), 7);
    SET @email = 'khachhang' + CAST(@i AS VARCHAR(10)) + '@cafebook.local';
    SET @tenDangNhap = 'user' + CAST(@i AS VARCHAR(10));
    
    -- Ngày tạo rải rác ngẫu nhiên trong 365 ngày qua
    SET @ngayTao = DATEADD(DAY, -(@i % 365), GETDATE());
    SET @diaChi = CAST(@i AS NVARCHAR) + N' Nguyễn Văn Linh, Q. Hải Châu, TP. Đà Nẵng';
    
    -- Đặt lại giá trị mặc định cho các cờ (flag)
    SET @BiKhoa = 0;
    SET @taiKhoanTam = 0;
    SET @DaXoa = 0;
    SET @lyDoKhoa = NULL;
    SET @thoiGianMoKhoa = NULL;

    -- 2. Phân loại khách hàng vào 5 nhóm
    DECLARE @mod INT = @i % 5;

    IF @mod = 0 -- NHÓM 1: VIP
    BEGIN
        SET @hoTen = N'Khách VIP ' + CAST(@i AS NVARCHAR);
        SET @diemTichLuy = 1000 + (@i * 2);
    END
    ELSE IF @mod = 1 -- NHÓM 2: THƯỜNG
    BEGIN
        SET @hoTen = N'Khách Thường ' + CAST(@i AS NVARCHAR);
        SET @diemTichLuy = @i % 150;
    END
    ELSE IF @mod = 2 -- NHÓM 3: VÃNG LAI (Tài khoản tạm)
    BEGIN
        SET @hoTen = N'Khách Lẻ ' + CAST(@i AS NVARCHAR);
        SET @diemTichLuy = 0;
        SET @taiKhoanTam = 1;
        SET @tenDangNhap = 'guest_tmp_' + CAST(@i AS VARCHAR);
    END
    ELSE IF @mod = 3 -- NHÓM 4: BỊ KHÓA
    BEGIN
        SET @hoTen = N'Khách Vi Phạm ' + CAST(@i AS NVARCHAR);
        SET @diemTichLuy = @i % 50;
        SET @BiKhoa = 1;
        SET @lyDoKhoa = N'Hệ thống tự động khóa tài khoản vi phạm #' + CAST(@i AS NVARCHAR);
        
        -- Một nửa khóa có thời hạn, một nửa khóa vĩnh viễn
        IF (@i % 2 = 0)
            SET @thoiGianMoKhoa = DATEADD(MONTH, 2, GETDATE());
        ELSE
            SET @thoiGianMoKhoa = NULL; 
    END
    ELSE IF @mod = 4 -- NHÓM 5: ĐÃ XÓA (Soft Delete)
    BEGIN
        SET @hoTen = N'Khách Cũ ' + CAST(@i AS NVARCHAR);
        SET @diemTichLuy = 0;
        SET @DaXoa = 1;
    END

    -- 3. Thực thi chèn vào Database
    INSERT INTO [dbo].[KhachHang] 
    ([hoTen], [soDienThoai], [email], [diaChi], [diemTichLuy], [tenDangNhap], [matKhau], [ngayTao], [BiKhoa], [AnhDaiDien], [taiKhoanTam], [DaXoa], [lyDoKhoa], [thoiGianMoKhoa])
    VALUES
    (@hoTen, @soDienThoai, @email, @diaChi, @diemTichLuy, @tenDangNhap, @matKhau, @ngayTao, @BiKhoa, @AnhDaiDien, @taiKhoanTam, @DaXoa, @lyDoKhoa, @thoiGianMoKhoa);

    -- Tăng biến đếm
    SET @i = @i + 1;
END

COMMIT TRANSACTION;

PRINT N'Đã chèn thành công! Cơ sở dữ liệu hiện đã có đủ 3000 khách hàng.';
GO

USE [CafebookDB]
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;

PRINT N'0. DỌN DẸP DỮ LIỆU CŨ (TỪ ID 101) ĐỂ TRÁNH LỖI...';
DELETE FROM [dbo].[DinhLuong] WHERE [idSanPham] >= 101;
DELETE FROM [dbo].[ChiTietHoaDon] WHERE [idSanPham] >= 101;
DELETE FROM [dbo].[SanPham] WHERE [idSanPham] >= 101;
DELETE FROM [dbo].[DonViChuyenDoi] WHERE [idChuyenDoi] >= 101 OR [idChuyenDoi] = 150;
DELETE FROM [dbo].[NguyenLieu] WHERE [idNguyenLieu] >= 101;
DELETE FROM [dbo].[DanhMuc] WHERE [idDanhMuc] >= 4;

PRINT N'1. CHÈN DANH MỤC SẢN PHẨM MỚI...';
SET IDENTITY_INSERT [dbo].[DanhMuc] ON;
INSERT INTO [dbo].[DanhMuc] ([idDanhMuc], [tenDanhMuc], [idDanhMucCha]) VALUES 
(4, N'Nước ép & Sinh tố', NULL),
(5, N'Đồ ăn vặt', NULL),
(6, N'Ăn nhẹ (Bánh mì/Sandwich)', NULL);
SET IDENTITY_INSERT [dbo].[DanhMuc] OFF;

PRINT N'2. CHÈN 37 NGUYÊN LIỆU MỚI (Bắt đầu từ ID 101)...';
SET IDENTITY_INSERT [dbo].[NguyenLieu] ON;
INSERT INTO [dbo].[NguyenLieu] ([idNguyenLieu], [tenNguyenLieu], [donViTinh], [tonKho], [TonKhoToiThieu]) VALUES 
(101, N'Trà Hoa Cúc Sấy', N'kg', 5.0, 1.0),
(102, N'Trà Lavender', N'kg', 3.0, 0.5),
(103, N'Trà Xanh Lài', N'kg', 10.0, 2.0),
(104, N'Trà Oolong', N'kg', 10.0, 2.0),
(105, N'Cam Tươi', N'kg', 20.0, 5.0),
(106, N'Thơm (Dứa)', N'kg', 15.0, 3.0),
(107, N'Táo Xanh/Đỏ', N'kg', 15.0, 3.0),
(108, N'Cà Rốt', N'kg', 10.0, 2.0),
(109, N'Dâu Tây', N'kg', 5.0, 1.0),
(110, N'Bơ', N'kg', 10.0, 2.0),
(111, N'Việt Quất', N'kg', 5.0, 1.0),
(112, N'Xoài Chín', N'kg', 15.0, 3.0),
(113, N'Syrup Đường', N'lít', 10.0, 2.0),
(114, N'Syrup Vanilla', N'lít', 5.0, 1.0),
(115, N'Syrup Caramel', N'lít', 5.0, 1.0),
(116, N'Bột Cacao', N'kg', 5.0, 1.0),
(117, N'Bột Matcha', N'kg', 3.0, 0.5),
(118, N'Kem béo (Richs)', N'lít', 10.0, 2.0),
(119, N'Bánh Croissant Đông Lạnh', N'cái', 100, 20),
(120, N'Cốt bánh Tiramisu (Làm sẵn)', N'phần', 50, 10),
(121, N'Cốt bánh Mousse Trà Xanh', N'phần', 30, 5),
(122, N'Cốt bánh Mousse Chanh Dây', N'phần', 30, 5),
(123, N'Cốt bánh Cheesecake', N'phần', 40, 10),
(124, N'Bánh Quy Bơ/Hạnh Nhân', N'kg', 10.0, 2.0),
(125, N'Hạt Điều Rang', N'kg', 15.0, 2.0),
(126, N'Trái Cây Sấy Thập Cẩm', N'kg', 10.0, 2.0),
(127, N'Bánh Mì Cắt Lát (Sandwich)', N'bịch', 20, 5),
(128, N'Thịt Gà Xông Khói', N'kg', 5.0, 1.0),
(129, N'Phô Mai Lát', N'kg', 5.0, 1.0),
(130, N'Chocolate Đen 70%', N'kg', 5.0, 1.0),
(131, N'Sữa Chua Không Đường', N'hộp', 50, 10),
(132, N'Đào Ngâm', N'hộp', 20, 5),
(133, N'Vải Ngâm', N'hộp', 20, 5),
(134, N'Bánh Su Kem', N'cái', 100, 20),
(135, N'Hạt Macca Úc', N'kg', 10.0, 2.0),
(136, N'Bánh Mì Ngọt Nho Khô', N'cái', 30, 5),
(137, N'Bánh Flan Caramen', N'hộp', 50, 10);
SET IDENTITY_INSERT [dbo].[NguyenLieu] OFF;

PRINT N'3. BỔ SUNG ĐƠN VỊ CHUYỂN ĐỔI (Đảm bảo quy đổi chính xác)...';
SET IDENTITY_INSERT [dbo].[DonViChuyenDoi] ON;
INSERT INTO [dbo].[DonViChuyenDoi] ([idChuyenDoi], [idNguyenLieu], [TenDonVi], [GiaTriQuyDoi], [LaDonViCoBan]) VALUES 
(150, 3, N'gram', 1000.0, 0), -- Bổ sung gram cho Cà phê Robusta (cũ)
(101, 101, N'gram', 1000.0, 0),
(102, 102, N'gram', 1000.0, 0),
(103, 103, N'gram', 1000.0, 0),
(104, 104, N'gram', 1000.0, 0),
(105, 105, N'gram', 1000.0, 0),
(106, 106, N'gram', 1000.0, 0),
(107, 107, N'gram', 1000.0, 0),
(108, 108, N'gram', 1000.0, 0),
(109, 109, N'gram', 1000.0, 0),
(110, 110, N'gram', 1000.0, 0),
(111, 111, N'gram', 1000.0, 0),
(112, 112, N'gram', 1000.0, 0),
(113, 113, N'ml', 1000.0, 0),
(114, 114, N'ml', 1000.0, 0),
(115, 115, N'ml', 1000.0, 0),
(116, 116, N'gram', 1000.0, 0),
(117, 117, N'gram', 1000.0, 0),
(118, 118, N'ml', 1000.0, 0),
(119, 124, N'gram', 1000.0, 0),
(120, 125, N'gram', 1000.0, 0),
(121, 126, N'gram', 1000.0, 0),
(122, 127, N'lát', 20.0, 0), -- 1 bịch sandwich có 20 lát
(123, 128, N'gram', 1000.0, 0),
(124, 129, N'gram', 1000.0, 0),
(125, 130, N'gram', 1000.0, 0),
(126, 135, N'gram', 1000.0, 0);
SET IDENTITY_INSERT [dbo].[DonViChuyenDoi] OFF;

PRINT N'4. CHÈN 50 SẢN PHẨM MỚI (Đồ uống & Ăn nhẹ)...';
SET IDENTITY_INSERT [dbo].[SanPham] ON;
INSERT INTO [dbo].[SanPham] ([idSanPham], [tenSanPham], [idDanhMuc], [giaBan], [moTa], [trangThaiKinhDoanh], [NhomIn]) VALUES 
-- Cà phê
(101, N'Espresso Nóng', 1, 35000, N'Cà phê chiết xuất nguyên chất.', 1, N'Pha Chế'),
(102, N'Americano Nóng', 1, 40000, N'Espresso kết hợp nước nóng thanh nhẹ.', 1, N'Pha Chế'),
(103, N'Americano Đá', 1, 45000, N'Americano giải khát, mát lạnh.', 1, N'Pha Chế'),
(104, N'Latte Nóng', 1, 55000, N'Cà phê Espresso và sữa tươi béo ngậy.', 1, N'Pha Chế'),
(105, N'Latte Đá', 1, 55000, N'Latte thêm đá.', 1, N'Pha Chế'),
(106, N'Cappuccino Nóng', 1, 55000, N'Cà phê sữa Ý với bọt sữa dày mịn.', 1, N'Pha Chế'),
(107, N'Mocha Nóng', 1, 60000, N'Espresso, sữa nóng và bột cacao.', 1, N'Pha Chế'),
(108, N'Caramel Macchiato', 1, 60000, N'Sữa, Espresso và sốt Caramel.', 1, N'Pha Chế'),
(109, N'Cà Phê Đen Đá', 1, 30000, N'Cà phê pha phin truyền thống.', 1, N'Pha Chế'),
(110, N'Cà Phê Sữa Đá', 1, 35000, N'Cà phê phin và sữa đặc.', 1, N'Pha Chế'),
(111, N'Bạc Xỉu Đá', 1, 40000, N'Nhiều sữa, ít cà phê, rất dễ uống.', 1, N'Pha Chế'),
(112, N'Cà Phê Muối', 1, 45000, N'Vị đắng, ngọt, mặn hòa quyện.', 1, N'Pha Chế'),

-- Trà 
(113, N'Trà Hoa Cúc Mật Ong', 2, 45000, N'Trà hoa cúc thanh nhiệt, an thần.', 1, N'Pha Chế'),
(114, N'Trà Lavender Nóng', 2, 50000, N'Trà hoa oải hương giảm căng thẳng.', 1, N'Pha Chế'),
(115, N'Trà Xanh Lài Nóng', 2, 40000, N'Trà xanh ướp hoa lài truyền thống.', 1, N'Pha Chế'),
(116, N'Trà Oolong Sen', 2, 45000, N'Oolong êm dịu, thoang thoảng hương sen.', 1, N'Pha Chế'),
(117, N'Trà Đào Cam Sả', 2, 55000, N'Trà thanh mát, chua ngọt tự nhiên.', 1, N'Pha Chế'),
(118, N'Trà Vải', 2, 50000, N'Trà đen kết hợp vải ngâm dịu ngọt.', 1, N'Pha Chế'),
(119, N'Trà Dâu Tây Nhiệt Đới', 2, 55000, N'Trà trái cây dâu tây tươi mát.', 1, N'Pha Chế'),
(120, N'Trà Đen Macchiato', 2, 50000, N'Trà đen nguyên bản kem cheese béo.', 1, N'Pha Chế'),

-- Nước ép & Sinh tố
(121, N'Nước Ép Cam Tươi', 4, 50000, N'Cam ép nguyên chất 100%.', 1, N'Pha Chế'),
(122, N'Nước Ép Thơm', 4, 45000, N'Ép dứa thanh mát, tốt cho tiêu hóa.', 1, N'Pha Chế'),
(123, N'Nước Ép Táo', 4, 50000, N'Nước ép táo xanh thanh lọc cơ thể.', 1, N'Pha Chế'),
(124, N'Nước Ép Cà Rốt Cam', 4, 55000, N'Vitamin A dồi dào, sáng mắt đọc sách.', 1, N'Pha Chế'),
(125, N'Sinh Tố Bơ', 4, 60000, N'Sinh tố bơ Đắk Lắk béo ngậy.', 1, N'Pha Chế'),
(126, N'Sinh Tố Dâu Tây', 4, 60000, N'Sinh tố dâu tươi chua ngọt dế uống.', 1, N'Pha Chế'),
(127, N'Sinh Tố Việt Quất', 4, 65000, N'Sinh tố việt quất chua nhẹ, lạ miệng.', 1, N'Pha Chế'),
(128, N'Sinh Tố Xoài', 4, 55000, N'Sinh tố xoài cát ngọt lịm thơm lừng.', 1, N'Pha Chế'),
(129, N'Matcha Đá Xay', 4, 65000, N'Matcha Nhật xay kem béo.', 1, N'Pha Chế'),

-- Signature
(130, N'Trà Hoa Hồng Táo Đỏ', 2, 65000, N'Món signature thảo mộc thanh dưỡng.', 1, N'Pha Chế'),
(131, N'Cà Phê Mơ Màng', 1, 65000, N'Espresso kết hợp sữa yến mạch và sả.', 1, N'Pha Chế'),

-- Bánh & Ăn Vặt
(132, N'Bánh Croissant Bơ (Sừng bò)', 3, 40000, N'Bánh nướng bơ Pháp thơm nhẹ.', 1, N'Bếp'),
(133, N'Bánh Tiramisu', 3, 50000, N'Bánh mềm mịn, hương cà phê nhẹ nhàng.', 1, N'Bếp'),
(134, N'Mousse Trà Xanh', 3, 50000, N'Bánh mousse matcha thanh mát.', 1, N'Bếp'),
(135, N'Mousse Chanh Dây', 3, 50000, N'Mousse vị chua thanh, không ngán.', 1, N'Bếp'),
(136, N'Cheesecake Truyền Thống', 3, 55000, N'Phô mai béo mịn nướng mềm.', 1, N'Bếp'),
(137, N'Bánh Su Kem Choux', 3, 35000, N'Vỏ mỏng, nhân kem lạnh ngọt dịu.', 1, N'Bếp'),
(138, N'Bánh Quy Hạnh Nhân', 5, 30000, N'Hũ bánh quy bơ hạnh nhân giòn rụm.', 1, N'Bếp'),
(139, N'Hạt Điều Rang Nguyên Vị', 5, 45000, N'Hạt điều rang sấy khô, nhâm nhi vui miệng.', 1, N'Bếp'),
(140, N'Hạt Macca Úc', 5, 60000, N'Hạt macca béo ngậy, nứt vỏ sẵn.', 1, N'Bếp'),
(141, N'Trái Cây Sấy Dẻo', 5, 40000, N'Xoài, mít, dâu sấy dẻo ít đường.', 1, N'Bếp'),
(142, N'Trái Cây Sấy Giòn', 5, 35000, N'Khoai lang, chuối, mít sấy khô.', 1, N'Bếp'),
(143, N'Chocolate Đen 70% Cacao', 5, 50000, N'Tỉnh táo tinh thần, tốt cho não bộ.', 1, N'Bếp'),

-- Ăn Nhẹ
(144, N'Bánh Mì Bơ Tỏi Nướng Giòn', 6, 45000, N'Bánh mì nướng bơ tỏi thơm lừng.', 1, N'Bếp'),
(145, N'Sandwich Gà Xông Khói', 6, 55000, N'Sandwich ăn nhẹ, không mùi nồng.', 1, N'Bếp'),
(146, N'Sandwich Phô Mai Nướng', 6, 50000, N'Sandwich nướng kẹp phô mai tan chảy.', 1, N'Bếp'),
(147, N'Bánh Mì Ngọt Nho Khô', 6, 30000, N'Bánh mì mềm nhân nho khô dịu ngọt.', 1, N'Bếp'),
(148, N'Sandwich Bơ Đậu Phộng Mứt', 6, 40000, N'Món ăn ngọt nạp năng lượng nhanh.', 1, N'Bếp'),
(149, N'Salad Trái Cây Sữa Chua', 6, 55000, N'Nhẹ bụng, thanh mát, đẹp da.', 1, N'Bếp'),
(150, N'Bánh Flan Mềm', 3, 25000, N'Bánh flan caramen truyền thống mềm tan.', 1, N'Bếp');
SET IDENTITY_INSERT [dbo].[SanPham] OFF;


PRINT N'5. THIẾT LẬP ĐỊNH LƯỢNG CHO 100% CÁC MÓN (Cả 50 món)...';
INSERT INTO [dbo].[DinhLuong] ([idSanPham], [idNguyenLieu], [SoLuongSuDung], [idDonViSuDung]) VALUES 
-- 101. Espresso: 20g Arabica
(101, 2, 20.0, 1),
-- 102, 103. Americano: 20g Arabica
(102, 2, 20.0, 1), (103, 2, 20.0, 1),
-- 104, 105. Latte (Nóng/Đá): 20g Arabica + 150ml Sữa tươi
(104, 2, 20.0, 1), (104, 4, 150.0, 6),
(105, 2, 20.0, 1), (105, 4, 150.0, 6),
-- 106. Cappuccino: 20g Arabica + 100ml Sữa tươi + 10g Cacao
(106, 2, 20.0, 1), (106, 4, 100.0, 6), (106, 116, 10.0, 116),
-- 107. Mocha: 20g Arabica + 120ml Sữa tươi + 15g Cacao
(107, 2, 20.0, 1), (107, 4, 120.0, 6), (107, 116, 15.0, 116),
-- 108. Caramel Macchiato: 20g Arabica + 150ml Sữa tươi + 15ml Syrup Caramel
(108, 2, 20.0, 1), (108, 4, 150.0, 6), (108, 115, 15.0, 115),
-- 109. CF Đen: 25g Robusta + 20g Đường
(109, 3, 25.0, 150), (109, 7, 20.0, 9),
-- 110. CF Sữa Đá: 25g Robusta + 30ml Sữa đặc
(110, 3, 25.0, 150), (110, 6, 30.0, 10),
-- 111. Bạc xỉu: 10g Robusta + 40ml Sữa đặc + 60ml Sữa tươi
(111, 3, 10.0, 150), (111, 6, 40.0, 10), (111, 4, 60.0, 6),
-- 112. CF Muối: 25g Robusta + 20ml Sữa đặc + 30ml Kem béo
(112, 3, 25.0, 150), (112, 6, 20.0, 10), (112, 118, 30.0, 118),
-- 113. Trà Hoa Cúc Mật Ong: 10g Cúc sấy + 20ml Syrup
(113, 101, 10.0, 101), (113, 113, 20.0, 113),
-- 114. Trà Lavender: 10g Lavender + 20ml Syrup
(114, 102, 10.0, 102), (114, 113, 20.0, 113),
-- 115. Trà Xanh Lài: 15g Trà Lài + 20ml Syrup
(115, 103, 15.0, 103), (115, 113, 20.0, 113),
-- 116. Trà Oolong Sen: 15g Trà Oolong + 20ml Syrup
(116, 104, 15.0, 104), (116, 113, 20.0, 113),
-- 117. Trà Đào Cam Sả: 1 túi Lipton + 30ml Syrup + 0.1 hộp Đào + 50g Cam tươi
(117, 5, 1.0, 4), (117, 113, 30.0, 113), (117, 105, 50.0, 105), (117, 132, 0.1, NULL),
-- 118. Trà Vải: 1 túi Lipton + 30ml Syrup + 0.1 hộp Vải
(118, 5, 1.0, 4), (118, 113, 30.0, 113), (118, 133, 0.1, NULL),
-- 119. Trà Dâu Tây: 15g Trà lài + 50g Dâu + 30ml Syrup
(119, 103, 15.0, 103), (119, 109, 50.0, 109), (119, 113, 30.0, 113),
-- 120. Trà Đen Macchiato: 1 túi Lipton + 30ml Kem béo + 20ml Syrup
(120, 5, 1.0, 4), (120, 118, 30.0, 118), (120, 113, 20.0, 113),
-- 121. Ép Cam: 400g Cam + 20ml Syrup
(121, 105, 400.0, 105), (121, 113, 20.0, 113),
-- 122. Ép Thơm: 400g Thơm + 20ml Syrup
(122, 106, 400.0, 106), (122, 113, 20.0, 113),
-- 123. Ép Táo: 400g Táo + 20ml Syrup
(123, 107, 400.0, 107), (123, 113, 20.0, 113),
-- 124. Ép Cà Rốt Cam: 200g Cà rốt + 200g Cam + 20ml Syrup
(124, 108, 200.0, 108), (124, 105, 200.0, 105), (124, 113, 20.0, 113),
-- 125. Sinh Tố Bơ: 150g Bơ + 40ml Sữa đặc + 40ml Sữa tươi
(125, 110, 150.0, 110), (125, 6, 40.0, 10), (125, 4, 40.0, 6),
-- 126. Sinh Tố Dâu Tây: 150g Dâu + 40ml Sữa đặc + 40ml Sữa tươi
(126, 109, 150.0, 109), (126, 6, 40.0, 10), (126, 4, 40.0, 6),
-- 127. Sinh Tố Việt Quất: 150g Việt quất + 40ml Sữa đặc + 40ml Sữa tươi
(127, 111, 150.0, 111), (127, 6, 40.0, 10), (127, 4, 40.0, 6),
-- 128. Sinh Tố Xoài: 150g Xoài + 40ml Sữa đặc + 40ml Sữa tươi
(128, 112, 150.0, 112), (128, 6, 40.0, 10), (128, 4, 40.0, 6),
-- 129. Matcha Đá Xay: 15g Matcha + 60ml Sữa tươi + 30ml Sữa đặc + 30ml Kem béo
(129, 117, 15.0, 117), (129, 4, 60.0, 6), (129, 6, 30.0, 10), (129, 118, 30.0, 118),
-- 130. Trà Hoa Hồng Táo Đỏ: 10g Táo đỏ + 10g Trà cúc + 20ml Syrup
(130, 107, 10.0, 107), (130, 101, 10.0, 101), (130, 113, 20.0, 113),
-- 131. CF Mơ Màng: 20g Arabica + 100ml Sữa tươi + 10ml Syrup Vanilla
(131, 2, 20.0, 1), (131, 4, 100.0, 6), (131, 114, 10.0, 114),
-- 132. Croissant: 1 cái
(132, 119, 1.0, NULL),
-- 133. Tiramisu: 1 phần cốt bánh + 5g Cacao rắc bề mặt
(133, 120, 1.0, NULL), (133, 116, 5.0, 116),
-- 134. Mousse Trà Xanh: 1 phần cốt bánh + 2g Matcha rắc bề mặt
(134, 121, 1.0, NULL), (134, 117, 2.0, 117),
-- 135. Mousse Chanh Dây: 1 phần cốt bánh
(135, 122, 1.0, NULL),
-- 136. Cheesecake: 1 phần cốt bánh
(136, 123, 1.0, NULL),
-- 137. Bánh Su Kem: 3 cái
(137, 134, 3.0, NULL),
-- 138. Bánh Quy Hạnh Nhân: 100g 
(138, 124, 100.0, 119),
-- 139. Hạt Điều Rang: 100g
(139, 125, 100.0, 120),
-- 140. Hạt Macca: 100g
(140, 135, 100.0, 126),
-- 141, 142. Trái Cây Sấy Dẻo/Giòn: 100g
(141, 126, 100.0, 121),
(142, 126, 100.0, 121),
-- 143. Chocolate Đen: 50g
(143, 130, 50.0, 125),
-- 144. Bánh Mì Bơ Tỏi: 3 lát bánh mì + 20g Bơ
(144, 127, 3.0, 122), (144, 110, 20.0, 110),
-- 145. Sandwich Gà: 2 lát bánh mì + 50g Thịt Gà + 20g Phô mai
(145, 127, 2.0, 122), (145, 128, 50.0, 123), (145, 129, 20.0, 124),
-- 146. Sandwich Phô Mai: 2 lát bánh mì + 40g Phô mai
(146, 127, 2.0, 122), (146, 129, 40.0, 124),
-- 147. Bánh Mì Ngọt Nho Khô: 1 cái
(147, 136, 1.0, NULL),
-- 148. Sandwich Bơ Đậu Phộng: 2 lát bánh mì + 30g Bơ
(148, 127, 2.0, 122), (148, 110, 30.0, 110),
-- 149. Salad Sữa Chua: 1 hộp Sữa chua + 50g Táo + 50g Dâu + 50g Xoài
(149, 131, 1.0, NULL), (149, 107, 50.0, 107), (149, 109, 50.0, 109), (149, 112, 50.0, 112),
-- 150. Bánh Flan Mềm: 1 hộp
(150, 137, 1.0, NULL);

COMMIT TRANSACTION;
PRINT N'ĐÃ HOÀN TẤT! Cả 50 món đều đã có công thức định lượng đầy đủ!';
GO

USE [CafebookDB]
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;

PRINT N'-------------------------------------------------------';
PRINT N'BẮT ĐẦU TẠO ĐƠN HÀNG, GIAO DỊCH, CHI TIẾT & ĐÁNH GIÁ...';
PRINT N'-------------------------------------------------------';

DECLARE @TongSoDon INT = 100; -- SỐ LƯỢNG HÓA ĐƠN BẠN MUỐN TẠO (Có thể sửa thành 500, 1000...)
DECLARE @i INT = 1;

-- Biến chung
DECLARE @NewHD_ID INT;
DECLARE @IdKhachHang INT;
DECLARE @IdNhanVien INT;
DECLARE @ThoiGianTao DATETIME;
DECLARE @ThoiGianThanhToan DATETIME;
DECLARE @TrangThai NVARCHAR(50);
DECLARE @LoaiHoaDon NVARCHAR(50);
DECLARE @PhuongThucThanhToan NVARCHAR(50);
DECLARE @IdBan INT;

-- Biến sinh Ngày từ 20/03/2026 đến Hiện tại
DECLARE @StartDate DATETIME = '2026-03-20 07:00:00';
DECLARE @EndDate DATETIME = GETDATE();
DECLARE @RandomSeconds INT;

-- VÒNG LẶP TẠO HÓA ĐƠN
WHILE @i <= @TongSoDon
BEGIN
    -- 1. SINH DỮ LIỆU NGẪU NHIÊN CƠ BẢN
    -- Lấy khách hàng và nhân viên ngẫu nhiên
    SELECT TOP 1 @IdKhachHang = idKhachHang FROM KhachHang WHERE taiKhoanTam = 0 ORDER BY NEWID();
    SELECT TOP 1 @IdNhanVien = idNhanVien FROM NhanVien ORDER BY NEWID();
    
    -- Ngày tạo ngẫu nhiên trong khoảng thời gian đã cho
    SET @RandomSeconds = ABS(CHECKSUM(NEWID())) % DATEDIFF(SECOND, @StartDate, @EndDate);
    SET @ThoiGianTao = DATEADD(SECOND, @RandomSeconds, @StartDate);
    
    -- Quyết định loại đơn: 70% Tại quán, 30% Giao hàng
    IF (ABS(CHECKSUM(NEWID())) % 100 < 70) 
    BEGIN
        SET @LoaiHoaDon = N'Tại quán';
        SELECT TOP 1 @IdBan = idBan FROM Ban ORDER BY NEWID();
        SET @TrangThai = N'Đã thanh toán';
        SET @ThoiGianThanhToan = DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 60 + 15, @ThoiGianTao);
        
        -- Tại quán thường thanh toán bằng Tiền mặt hoặc Chuyển khoản/Momo
        IF (ABS(CHECKSUM(NEWID())) % 2 = 0) SET @PhuongThucThanhToan = N'Tiền mặt';
        ELSE SET @PhuongThucThanhToan = N'Chuyển khoản';
    END
    ELSE 
    BEGIN
        SET @LoaiHoaDon = N'Giao hàng';
        SET @IdBan = NULL;
        
        -- Đơn giao hàng: 60% Hoàn thành, 20% Đang giao, 10% Chờ xác nhận, 10% Đã hủy
        DECLARE @TThaiGH INT = ABS(CHECKSUM(NEWID())) % 100;
        IF @TThaiGH < 60
        BEGIN
            SET @TrangThai = N'Đã thanh toán';
            SET @ThoiGianThanhToan = DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 120 + 30, @ThoiGianTao);
            SET @PhuongThucThanhToan = (CASE WHEN (ABS(CHECKSUM(NEWID())) % 2 = 0) THEN N'COD' ELSE N'VNPAY' END);
        END
        ELSE IF @TThaiGH < 80
        BEGIN
            SET @TrangThai = N'Đang giao hàng';
            SET @ThoiGianThanhToan = NULL;
            SET @PhuongThucThanhToan = N'COD';
        END
        ELSE IF @TThaiGH < 90
        BEGIN
            SET @TrangThai = N'Chờ xác nhận';
            SET @ThoiGianThanhToan = NULL;
            SET @PhuongThucThanhToan = N'COD';
        END
        ELSE
        BEGIN
            SET @TrangThai = N'Đã hủy';
            SET @ThoiGianThanhToan = NULL;
            SET @PhuongThucThanhToan = N'COD';
        END
    END

    -- 2. TẠO HÓA ĐƠN
    INSERT INTO [dbo].[HoaDon] 
    ([idBan], [idNhanVien], [idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia], [TongPhuThu], [phuongThucThanhToan], [LoaiHoaDon])
    VALUES 
    (@IdBan, @IdNhanVien, @IdKhachHang, @ThoiGianTao, @ThoiGianThanhToan, @TrangThai, 0, 0, 0, @PhuongThucThanhToan, @LoaiHoaDon);
    
    SET @NewHD_ID = SCOPE_IDENTITY();

    -- 3. TẠO CHI TIẾT HÓA ĐƠN (MUA MÓN)
    -- Mỗi hóa đơn mua ngẫu nhiên từ 1 đến 5 món
    DECLARE @SoMon INT = ABS(CHECKSUM(NEWID())) % 5 + 1;
    DECLARE @j INT = 1;
    DECLARE @TongTienHD DECIMAL(18,2) = 0;

    WHILE @j <= @SoMon
    BEGIN
        DECLARE @IdSP INT;
        DECLARE @GiaBan DECIMAL(18,2);
        DECLARE @SoLuong INT = ABS(CHECKSUM(NEWID())) % 3 + 1; -- Mua 1-3 ly/món

        -- Lấy SP ngẫu nhiên
        SELECT TOP 1 @IdSP = idSanPham, @GiaBan = giaBan FROM SanPham ORDER BY NEWID();

        -- Tránh trùng món trong cùng 1 hóa đơn
        IF NOT EXISTS (SELECT 1 FROM ChiTietHoaDon WHERE idHoaDon = @NewHD_ID AND idSanPham = @IdSP)
        BEGIN
            INSERT INTO [dbo].[ChiTietHoaDon] ([idHoaDon], [idSanPham], [soLuong], [donGia])
            VALUES (@NewHD_ID, @IdSP, @SoLuong, @GiaBan);
            
            SET @TongTienHD = @TongTienHD + (@SoLuong * @GiaBan);
            SET @j = @j + 1;
        END
    END

    -- 4. CẬP NHẬT TỔNG TIỀN VÀO HÓA ĐƠN
    UPDATE HoaDon SET tongTienGoc = @TongTienHD WHERE idHoaDon = @NewHD_ID;

    -- 5. TẠO GIAO DỊCH THANH TOÁN (Nếu Đã thanh toán)
    IF @TrangThai = N'Đã thanh toán'
    BEGIN
        DECLARE @MaGD NVARCHAR(100) = N'TRANS_' + CAST(@NewHD_ID AS NVARCHAR) + '_' + CAST(ABS(CHECKSUM(NEWID())) AS NVARCHAR);
        INSERT INTO [dbo].[GiaoDichThanhToan] ([idHoaDon], [MaGiaoDichNgoai], [CongThanhToan], [SoTien], [ThoiGianGiaoDich], [TrangThai])
        VALUES (@NewHD_ID, @MaGD, @PhuongThucThanhToan, @TongTienHD, @ThoiGianThanhToan, N'Thành công');
    END

    -- 6. TẠO ĐÁNH GIÁ (Chỉ tạo nếu là đơn ONLINE và ĐÃ THANH TOÁN / GIAO XONG)
    IF @LoaiHoaDon = N'Giao hàng' AND @TrangThai = N'Đã thanh toán'
    BEGIN
        -- Đánh giá ngẫu nhiên 3 - 5 sao
        DECLARE @SoSao INT = ABS(CHECKSUM(NEWID())) % 3 + 3; 
        DECLARE @BinhLuan NVARCHAR(MAX);

        IF @SoSao = 5 SET @BinhLuan = N'Đồ uống ngon, giao hàng siêu nhanh. Sẽ ủng hộ quán lâu dài!';
        ELSE IF @SoSao = 4 SET @BinhLuan = N'Món ăn khá ổn, đóng gói cẩn thận. Giao hàng đúng giờ.';
        ELSE SET @BinhLuan = N'Tạm được, nhưng trà hơi ngọt so với khẩu vị của mình.';

        -- Lấy 1 sản phẩm bất kỳ trong hóa đơn này để đánh giá
        DECLARE @IdSPDanhGia INT;
        SELECT TOP 1 @IdSPDanhGia = idSanPham FROM ChiTietHoaDon WHERE idHoaDon = @NewHD_ID ORDER BY NEWID();

        INSERT INTO [dbo].[DanhGia] ([idKhachHang], [idSanPham], [idHoaDon], [SoSao], [BinhLuan], [NgayTao], [TrangThai])
        VALUES (@IdKhachHang, @IdSPDanhGia, @NewHD_ID, @SoSao, @BinhLuan, DATEADD(HOUR, 2, @ThoiGianThanhToan), N'Hiển thị');
    END

    -- Chuyển sang đơn tiếp theo
    SET @i = @i + 1;
END

COMMIT TRANSACTION;
PRINT N'ĐÃ XONG! Mọi đơn hàng, giao dịch và đánh giá đã được thêm thành công.';
GO
USE [CafebookDB]
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;

PRINT N'-------------------------------------------------------';
PRINT N'BẮT ĐẦU CÀY ĐÁNH GIÁ CHO TẤT CẢ SẢN PHẨM...';
PRINT N'-------------------------------------------------------';

-- Tạo bảng tạm để chứa danh sách toàn bộ sản phẩm
DECLARE @ProductList TABLE (
    id INT IDENTITY(1,1), 
    idSanPham INT, 
    tenSanPham NVARCHAR(255), 
    giaBan DECIMAL(18,2)
);

-- Lấy tất cả sản phẩm đang kinh doanh đưa vào bảng tạm
INSERT INTO @ProductList (idSanPham, tenSanPham, giaBan)
SELECT idSanPham, tenSanPham, giaBan FROM SanPham WHERE trangThaiKinhDoanh = 1;

DECLARE @i INT = 1;
DECLARE @max INT = (SELECT MAX(id) FROM @ProductList);

-- Vòng lặp duyệt qua TỪNG SẢN PHẨM MỘT
WHILE @i <= @max
BEGIN
    DECLARE @IdSP INT, @TenSP NVARCHAR(255), @GiaBan DECIMAL(18,2);
    SELECT @IdSP = idSanPham, @TenSP = tenSanPham, @GiaBan = giaBan FROM @ProductList WHERE id = @i;
    
    -- Mỗi sản phẩm sẽ được tạo ngẫu nhiên từ 1 đến 3 đánh giá
    DECLARE @NumReviews INT = ABS(CHECKSUM(NEWID())) % 3 + 1;
    DECLARE @r INT = 1;
    
    WHILE @r <= @NumReviews
    BEGIN
        -- 1. Bốc ngẫu nhiên 1 Khách hàng (Tài khoản thật, không phải khách vãng lai)
        DECLARE @IdKH INT;
        SELECT TOP 1 @IdKH = idKhachHang FROM KhachHang WHERE taiKhoanTam = 0 ORDER BY NEWID();
        
        -- 2. Tạo một Hóa Đơn Online (Giao hàng) ngẫu nhiên trong 40 ngày qua
        DECLARE @ThoiGianTao DATETIME = DATEADD(DAY, - (ABS(CHECKSUM(NEWID())) % 40), GETDATE());
        DECLARE @ThoiGianTT DATETIME = DATEADD(MINUTE, 30, @ThoiGianTao);
        DECLARE @IdHD INT;
        
        INSERT INTO [dbo].[HoaDon] 
        ([idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia], [TongPhuThu], [phuongThucThanhToan], [LoaiHoaDon], [TrangThaiGiaoHang], [DiaChiGiaoHang])
        VALUES 
        (@IdKH, @ThoiGianTao, @ThoiGianTT, N'Đã thanh toán', @GiaBan, 0, 0, N'COD', N'Giao hàng', N'Hoàn thành', N'Địa chỉ mặc định');
        
        SET @IdHD = SCOPE_IDENTITY();
        
        -- 3. Mua đúng sản phẩm này vào Chi Tiết Hóa Đơn
        INSERT INTO [dbo].[ChiTietHoaDon] ([idHoaDon], [idSanPham], [soLuong], [donGia])
        VALUES (@IdHD, @IdSP, 1, @GiaBan);
        
        -- 4. Ghi nhận thanh toán thành công
        INSERT INTO [dbo].[GiaoDichThanhToan] ([idHoaDon], [MaGiaoDichNgoai], [CongThanhToan], [SoTien], [ThoiGianGiaoDich], [TrangThai])
        VALUES (@IdHD, 'COD_DG_' + CAST(@IdHD AS VARCHAR), N'COD', @GiaBan, @ThoiGianTT, N'Thành công');
        
        -- 5. Viết Đánh Giá cho sản phẩm
        DECLARE @SoSao INT = ABS(CHECKSUM(NEWID())) % 3 + 3; -- Random 3 đến 5 sao
        DECLARE @BinhLuan NVARCHAR(MAX);
        
        -- Tạo nội dung bình luận tự nhiên có chứa tên món
        IF @SoSao = 5 
            SET @BinhLuan = N'Sản phẩm ' + @TenSP + N' rất tuyệt vời, đúng gu của mình. Đóng gói rất cẩn thận, shipper thân thiện!';
        ELSE IF @SoSao = 4 
            SET @BinhLuan = N'Món ' + @TenSP + N' dùng khá ngon, hương vị đậm đà. Sẽ ủng hộ quán vào lần tới.';
        ELSE 
            SET @BinhLuan = N'Món ' + @TenSP + N' uống cũng tạm được, cá nhân mình thấy chưa đặc sắc lắm nhưng nhìn chung là sạch sẽ.';
        
        INSERT INTO [dbo].[DanhGia] ([idKhachHang], [idSanPham], [idHoaDon], [SoSao], [BinhLuan], [NgayTao], [TrangThai])
        VALUES (@IdKH, @IdSP, @IdHD, @SoSao, @BinhLuan, DATEADD(HOUR, 2, @ThoiGianTT), N'Hiển thị');
        
        SET @r = @r + 1;
    END
    
    SET @i = @i + 1;
END

COMMIT TRANSACTION;
PRINT N'ĐÃ XONG! Mọi sản phẩm trên hệ thống hiện tại đều đã có đánh giá đi kèm hóa đơn hợp lệ.';
GO

USE [CafebookDB]
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;

PRINT N'-------------------------------------------------------';
PRINT N'BẮT ĐẦU THÊM PHẢN HỒI ĐÁNH GIÁ (REPLY) TỪ NHÂN VIÊN...';
PRINT N'-------------------------------------------------------';

INSERT INTO [dbo].[PhanHoiDanhGia] ([idDanhGia], [idNhanVien], [NoiDung], [NgayTao])
SELECT 
    dg.idDanhGia,
    ((dg.idDanhGia % 3) + 1) AS idNhanVien, -- Xoay vòng chia đều cho nhân viên ID 1, 2, 3
    CASE 
        WHEN dg.SoSao = 5 THEN 
            CASE (dg.idDanhGia % 3)
                WHEN 0 THEN N'Cảm ơn bạn đã ủng hộ Cafebook! Rất mong được phục vụ bạn vào những lần tới ạ. 🥰'
                WHEN 1 THEN N'Dạ, quán rất vui vì bạn đã hài lòng với sản phẩm. Chúc bạn một ngày thật tuyệt vời!'
                ELSE N'Cảm ơn đánh giá 5 sao của bạn! Đây là động lực lớn để team Cafebook tiếp tục phát triển. ❤️'
            END
        WHEN dg.SoSao = 4 THEN 
            CASE (dg.idDanhGia % 3)
                WHEN 0 THEN N'Cảm ơn bạn đã để lại đánh giá. Quán sẽ tiếp tục nâng cao chất lượng phục vụ để bạn có trải nghiệm tốt hơn nha!'
                WHEN 1 THEN N'Dạ cảm ơn phản hồi của bạn. Cafebook sẽ ghi nhận để ngày càng hoàn thiện hơn ạ. 😊'
                ELSE N'Cảm ơn bạn đã tin tưởng và chọn Cafebook. Hẹn gặp lại bạn vào một ngày gần nhất!'
            END
        ELSE 
            CASE (dg.idDanhGia % 3)
                WHEN 0 THEN N'Cảm ơn bạn đã góp ý chân thành. Quán đã ghi nhận và sẽ xem xét điều chỉnh lại để món ăn/đồ uống hợp khẩu vị hơn ạ.'
                WHEN 1 THEN N'Dạ xin lỗi vì sản phẩm chưa hoàn toàn làm bạn hài lòng. Quán sẽ cố gắng cải thiện trong thời gian tới!'
                ELSE N'Cảm ơn phản hồi của bạn. Bộ phận pha chế/bếp đã nhận được thông tin và sẽ rút kinh nghiệm ạ. Mong bạn vẫn ủng hộ quán nha.'
            END
    END AS NoiDung,
    -- Thời gian trả lời cộng thêm từ 30 đến 299 phút để trông tự nhiên
    DATEADD(MINUTE, (dg.idDanhGia % 270) + 30, dg.NgayTao) AS NgayTao 
FROM [dbo].[DanhGia] dg
WHERE NOT EXISTS (
    -- Bỏ qua những đánh giá đã có người phản hồi
    SELECT 1 FROM [dbo].[PhanHoiDanhGia] ph WHERE ph.idDanhGia = dg.idDanhGia
);

COMMIT TRANSACTION;
PRINT N'ĐÃ XONG! Toàn bộ đánh giá trên hệ thống hiện tại đã được nhân viên chăm sóc khách hàng phản hồi.';
GO
USE [CafebookDB]
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;

PRINT N'-------------------------------------------------------';
PRINT N'BẮT ĐẦU TẠO DỮ LIỆU NHÂN SỰ TỪ THÁNG 3/2026 ĐẾN NAY...';
PRINT N'-------------------------------------------------------';

DECLARE @StartDate DATE = '2026-03-01';
DECLARE @EndDate DATE = GETDATE();

-- 1. DỌN DẸP DỮ LIỆU TRONG KHOẢNG THỜI GIAN NÀY ĐỂ KHÔNG BỊ TRÙNG LẶP
DELETE FROM BangChamCong WHERE idLichLamViec IN (SELECT idLichLamViec FROM LichLamViec WHERE ngayLam >= @StartDate AND ngayLam <= @EndDate);
DELETE FROM LichLamViec WHERE ngayLam >= @StartDate AND ngayLam <= @EndDate;
DELETE FROM NhuCauCaLam WHERE ngayLam >= @StartDate AND ngayLam <= @EndDate;

-- 2. KHAI BÁO BẢNG PHÂN CÔNG GIẢ LẬP CHO CÁC CA (Đủ 6 ca)
-- Vai trò: 1 (Quản lý), 2 (Nhân viên)
-- Nhân viên: 1, 3 (Quản lý) | 2, 4, 7, 11 (Nhân viên)
DECLARE @PhanCong TABLE (IdCa INT, IdVaiTro INT, IdNhanVien INT);
INSERT INTO @PhanCong VALUES 
(1, 1, 1), (1, 2, 2), (1, 2, 4),    -- Ca 1 (FT-Sáng): QL Bảo Toàn, NV 2, 4
(2, 1, 3), (2, 2, 7), (2, 2, 11),   -- Ca 2 (FT-Chiều): QL Shushune, NV 7, 11
(3, 1, 1), (3, 2, 2), (3, 2, 7),    -- Ca 3 (FT-Tối): QL Bảo Toàn, NV 2, 7
(4, 2, 11),                         -- Ca 4 (PT-Trưa): NV 11
(5, 2, 4),                          -- Ca 5 (PT-Chiều): NV 4
(6, 2, 11);                         -- Ca 6 (PT-Tối): NV 11

-- 3. VÒNG LẶP DUYỆT QUA TỪNG NGÀY
DECLARE @CurrentDate DATE = @StartDate;

WHILE @CurrentDate <= @EndDate
BEGIN
    -- 3.1. Thêm Nhu Cầu Ca Làm (Đếm số lượng cần từ bảng phân công)
    INSERT INTO NhuCauCaLam (ngayLam, idCa, idVaiTro, soLuongCan, loaiYeuCau, ghiChu)
    SELECT @CurrentDate, IdCa, IdVaiTro, COUNT(*), N'Tất cả', N'Tạo tự động'
    FROM @PhanCong
    GROUP BY IdCa, IdVaiTro;

    -- 3.2. Đăng ký Lịch Làm Việc & Chấm công cho từng người
    DECLARE @IdCa INT, @IdNhanVien INT, @IdLichLamViec INT;
    DECLARE @GioBatDau TIME, @GioKetThuc TIME;
    DECLARE @DatetimeVao DATETIME, @DatetimeRa DATETIME;

    DECLARE cur CURSOR FOR SELECT IdCa, IdNhanVien FROM @PhanCong;
    OPEN cur;
    FETCH NEXT FROM cur INTO @IdCa, @IdNhanVien;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Thêm Lịch Làm Việc
        INSERT INTO LichLamViec (idNhanVien, idCa, ngayLam, trangThai, ghiChu)
        VALUES (@IdNhanVien, @IdCa, @CurrentDate, N'Đã duyệt', N'');
        
        SET @IdLichLamViec = SCOPE_IDENTITY();

        -- Lấy khung giờ của ca làm việc
        SELECT @GioBatDau = gioBatDau, @GioKetThuc = gioKetThuc FROM CaLamViec WHERE idCa = @IdCa;

        -- Ghép Ngày + Giờ để tạo ra Datetime chuẩn
        SET @DatetimeVao = DATEADD(MINUTE, DATEDIFF(MINUTE, '00:00:00', @GioBatDau), CAST(@CurrentDate AS DATETIME));
        SET @DatetimeRa = DATEADD(MINUTE, DATEDIFF(MINUTE, '00:00:00', @GioKetThuc), CAST(@CurrentDate AS DATETIME));

        -- Tạo ngẫu nhiên đi trễ / về sớm để dữ liệu trông thật
        -- Giờ vào: Sớm 10 phút đến trễ 5 phút
        SET @DatetimeVao = DATEADD(MINUTE, (ABS(CHECKSUM(NEWID())) % 16) - 10, @DatetimeVao);
        -- Giờ ra: Về sớm 5 phút đến tăng ca 15 phút
        SET @DatetimeRa = DATEADD(MINUTE, (ABS(CHECKSUM(NEWID())) % 21) - 5, @DatetimeRa);

        -- Nếu ca làm đi qua nửa đêm (ca tối), cộng thêm 1 ngày cho giờ Ra (Phòng hờ ca qua đêm)
        IF @DatetimeRa < @DatetimeVao 
            SET @DatetimeRa = DATEADD(DAY, 1, @DatetimeRa);

        -- Thêm Bảng Chấm Công
        INSERT INTO BangChamCong (idLichLamViec, gioVao, gioRa, ghiChuSua)
        VALUES (@IdLichLamViec, @DatetimeVao, @DatetimeRa, NULL);

        FETCH NEXT FROM cur INTO @IdCa, @IdNhanVien;
    END
    CLOSE cur;
    DEALLOCATE cur;

    -- Tăng lên 1 ngày
    SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
END

COMMIT TRANSACTION;
PRINT N'ĐÃ XONG! Toàn bộ Nhu cầu ca làm, Lịch làm việc và Bảng chấm công đã được chèn thành công.';
GO
USE [CafebookDB]
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;

PRINT N'-------------------------------------------------------';
PRINT N'BẮT ĐẦU THÊM CÁC LOẠI PHỤ THU VÀO HỆ THỐNG...';
PRINT N'-------------------------------------------------------';

-- Chèn dữ liệu vào bảng PhuThu (ID sẽ tự động tăng)
INSERT INTO [dbo].[PhuThu] ([TenPhuThu], [GiaTri], [LoaiGiaTri]) 
VALUES 
-- Nhóm phụ thu theo phần trăm (PhanTram)
(N'Phụ thu ngày Lễ / Tết', 15.00, N'PhanTram'),      -- Phụ thu 15% tổng bill
(N'Phụ thu cuối tuần (Thứ 7 & CN)', 5.00, N'PhanTram'), -- Phụ thu 5% tổng bill
(N'Phụ thu sau 22h00', 10.00, N'PhanTram'),             -- Phụ thu 10% phục vụ khuya

-- Nhóm phụ thu theo số tiền cố định (SoTien)
(N'Phụ thu bao bì mang đi (Takeaway)', 5000.00, N'SoTien'),   -- 5k tiền ly nhựa, túi
(N'Phụ thu phòng VIP / Phòng họp', 50000.00, N'SoTien'),      -- 50k phí thuê phòng riêng
(N'Phụ thu thức ăn mang từ ngoài vào', 30000.00, N'SoTien'),  -- 30k phí dịch vụ
(N'Phụ thu dọn dẹp đặc biệt', 50000.00, N'SoTien'),           -- 50k phí dọn dẹp khi khách làm bẩn/nôn trớ
(N'Phí giữ xe qua đêm', 10000.00, N'SoTien');                 -- 10k phí giữ xe

COMMIT TRANSACTION;
PRINT N'ĐÃ XONG! Các loại phụ thu mới đã được thêm thành công.';
GO
USE [CafebookDB]
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;

PRINT N'-------------------------------------------------------';
PRINT N'TẠO 15 PHIẾU XUẤT HỦY (Kèm Chi Tiết) - ĐÃ FIX LỖI NULL';
PRINT N'-------------------------------------------------------';
DECLARE @i INT = 1;

WHILE @i <= 15
BEGIN
    DECLARE @idPXH INT;
    DECLARE @ngayXuat DATETIME = DATEADD(DAY, - (ABS(CHECKSUM(NEWID())) % 60), GETDATE());
    DECLARE @LyDo NVARCHAR(500);

    -- Dùng phép chia lấy dư từ biến @i để đảm bảo chắc chắn có giá trị, không bao giờ NULL
    IF @i % 3 = 0 
        SET @LyDo = N'Hư hỏng do vận chuyển';
    ELSE IF @i % 3 = 1 
        SET @LyDo = N'Nguyên liệu hết hạn sử dụng';
    ELSE 
        SET @LyDo = N'Làm đổ/vỡ trong kho';

    -- Insert Phiếu Xuất Hủy
    INSERT INTO [dbo].[PhieuXuatHuy] (idNhanVienXuat, NgayXuatHuy, LyDoXuatHuy, TongGiaTriHuy)
    VALUES (1, @ngayXuat, @LyDo, 0);
    
    SET @idPXH = SCOPE_IDENTITY();

    -- Tạo Chi Tiết Xuất Hủy
    DECLARE @k INT = 1;
    DECLARE @soMonHuy INT = ABS(CHECKSUM(NEWID())) % 3 + 1; -- Hủy 1 đến 3 món ngẫu nhiên
    DECLARE @tongHuy DECIMAL(18,2) = 0;

    WHILE @k <= @soMonHuy
    BEGIN
         DECLARE @idNL2 INT;
         SELECT TOP 1 @idNL2 = idNguyenLieu FROM NguyenLieu ORDER BY NEWID();

         -- Tránh trùng lặp nguyên liệu trong cùng 1 phiếu
         IF NOT EXISTS (SELECT 1 FROM ChiTietXuatHuy WHERE idPhieuXuatHuy = @idPXH AND idNguyenLieu = @idNL2)
         BEGIN
             DECLARE @slHuy DECIMAL(18,2) = (ABS(CHECKSUM(NEWID())) % 5) + 1; -- Hủy từ 1 đến 5 đơn vị
             DECLARE @giaVon DECIMAL(18,2) = (ABS(CHECKSUM(NEWID())) % 50 + 10) * 1000;
             
             INSERT INTO [dbo].[ChiTietXuatHuy] (idPhieuXuatHuy, idNguyenLieu, SoLuong, DonGiaVon)
             VALUES (@idPXH, @idNL2, @slHuy, @giaVon);

             SET @tongHuy = @tongHuy + (@slHuy * @giaVon);
             SET @k = @k + 1;
         END
    END
    
    -- Cập nhật lại tổng tiền hủy cho khớp với chi tiết
    UPDATE [dbo].[PhieuXuatHuy] SET TongGiaTriHuy = @tongHuy WHERE idPhieuXuatHuy = @idPXH;

    SET @i = @i + 1;
END

COMMIT TRANSACTION;
PRINT N'ĐÃ HOÀN TẤT! Đã chèn thành công 15 Phiếu Xuất Hủy mới và chi tiết đi kèm.';
GO
USE [CafebookDB]
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;

PRINT N'-------------------------------------------------------';
PRINT N'1. THÊM 5 KHU VỰC MỚI...';
PRINT N'-------------------------------------------------------';

-- Bảng tạm lưu lại ID của các khu vực mới vừa được tạo
DECLARE @NewAreas TABLE (idKhuVuc INT);

INSERT INTO [dbo].[KhuVuc] ([TenKhuVuc], [MoTa])
OUTPUT INSERTED.idKhuVuc INTO @NewAreas
VALUES 
(N'Sân Thượng', N'Khu vực ngoài trời thoáng mát, view đẹp'),
(N'Sân Vườn', N'Không gian xanh, nhiều cây cảnh thiên nhiên'),
(N'Phòng Đọc Sách', N'Khu vực yên tĩnh tuyệt đối, cấm làm ồn'),
(N'Phòng Họp VIP', N'Khu vực cách âm dành cho nhóm làm việc'),
(N'Khu Ban Công', N'Khu vực ngắm cảnh đường phố');

PRINT N'-------------------------------------------------------';
PRINT N'2. THÊM 50 BÀN VÀO CÁC KHU VỰC MỚI...';
PRINT N'-------------------------------------------------------';

DECLARE @i INT = 1;
DECLARE @TotalTables INT = 50;

WHILE @i <= @TotalTables
BEGIN
    -- Lấy ngẫu nhiên 1 ID khu vực TỪ NHỮNG KHU VỰC MỚI
    DECLARE @RandomKhuVuc INT;
    SELECT TOP 1 @RandomKhuVuc = idKhuVuc FROM @NewAreas ORDER BY NEWID();

    -- Đếm xem khu vực này đang có bao nhiêu bàn để đặt tên cho chuẩn (Ví dụ: KV3-B1, KV3-B2)
    DECLARE @CountBan INT;
    SELECT @CountBan = COUNT(*) + 1 FROM [dbo].[Ban] WHERE idKhuVuc = @RandomKhuVuc;
    
    DECLARE @SoBan NVARCHAR(50) = N'KV' + CAST(@RandomKhuVuc AS NVARCHAR) + N'-B' + CAST(@CountBan AS NVARCHAR);
    
    -- Số ghế ngẫu nhiên: 2, 4, 6, 8
    DECLARE @SoGhe INT = (ABS(CHECKSUM(NEWID())) % 4 + 1) * 2; 

    -- Trạng thái ngẫu nhiên: 60% Trống, 25% Có khách, 10% Đã đặt, 5% Bảo trì
    DECLARE @RandStatus INT = ABS(CHECKSUM(NEWID())) % 100;
    DECLARE @TrangThai NVARCHAR(50);
    DECLARE @GhiChu NVARCHAR(500) = NULL;

    IF @RandStatus < 60 
        SET @TrangThai = N'Trống';
    ELSE IF @RandStatus < 85 
        SET @TrangThai = N'Có khách';
    ELSE IF @RandStatus < 95 
        SET @TrangThai = N'Đã đặt';
    ELSE 
    BEGIN
        SET @TrangThai = N'Bảo trì';
        SET @GhiChu = CHOOSE(ABS(CHECKSUM(NEWID())) % 3 + 1, N'Bàn bị gãy chân', N'Ghế bị rách nệm', N'Chờ thợ mộc đến sửa');
    END

    -- Insert vào bảng Ban
    INSERT INTO [dbo].[Ban] ([soBan], [soGhe], [trangThai], [ghiChu], [idKhuVuc])
    VALUES (@SoBan, @SoGhe, @TrangThai, @GhiChu, @RandomKhuVuc);

    SET @i = @i + 1;
END

COMMIT TRANSACTION;
PRINT N'ĐÃ XONG! 5 khu vực mới và 50 bàn đã được rải đều vào hệ thống.';
GO
USE [CafebookDB]
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;

PRINT N'-------------------------------------------------------';
PRINT N'BẮT ĐẦU CHÈN 20 CHƯƠNG TRÌNH KHUYẾN MÃI (TỪ THÁNG 3 ĐẾN THÁNG 7/2026)';
PRINT N'-------------------------------------------------------';

INSERT INTO [dbo].[KhuyenMai] 
([maKhuyenMai], [tenChuongTrinh], [moTa], [loaiGiamGia], [giaTriGiam], [ngayBatDau], [ngayKetThuc], [dieuKienApDung], [soLuongConLai], [TrangThai], [GiamToiDa], [IdSanPhamApDung], [HoaDonToiThieu], [GioBatDau], [GioKetThuc], [NgayTrongTuan])
VALUES 
-- 1. Các chiến dịch theo tháng
(N'MARCH10', N'Chào tháng 3', N'Giảm 10% đón tháng 3 tươi đẹp', N'PhanTram', 10.00, '2026-03-01', '2026-03-31', N'Giảm 10% cho mọi đơn hàng', 500, N'Hoạt động', 30000.00, NULL, 0, NULL, NULL, NULL),
(N'SUMMERSTART', N'Chào hè rực rỡ', N'Giảm 15k cho đơn từ 100k', N'SoTien', 15000.00, '2026-05-01', '2026-05-31', N'Đơn tối thiểu 100k', 1000, N'Hoạt động', NULL, NULL, 100000.00, NULL, NULL, NULL),
(N'JULYPROMO', N'Vui hè tháng 7', N'Giảm 5% cho hóa đơn bất kỳ', N'PhanTram', 5.00, '2026-07-01', '2026-07-31', N'Không giới hạn điều kiện', 500, N'Hoạt động', 20000.00, NULL, 0, NULL, NULL, NULL),

-- 2. Các dịp lễ lớn
(N'WOMENDAY', N'Mùng 8/3 - Tôn vinh phái đẹp', N'Giảm 20% tổng hóa đơn', N'PhanTram', 20.00, '2026-03-01', '2026-03-10', N'Áp dụng toàn hệ thống', 300, N'Hoạt động', 50000.00, NULL, 50000.00, NULL, NULL, NULL),
(N'APRILFOOL', N'Cá tháng tư - Sale thật', N'Giảm 15k không dối lừa', N'SoTien', 15000.00, '2026-04-01', '2026-04-05', N'Đơn từ 80k', 200, N'Hoạt động', NULL, NULL, 80000.00, NULL, NULL, NULL),
(N'HOLIDAY304', N'Mừng đại lễ 30/4 - 1/5', N'Giảm 30% bung xõa lễ', N'PhanTram', 30.00, '2026-04-25', '2026-05-05', N'Giảm tối đa 100k', 1000, N'Hoạt động', 100000.00, NULL, 0, NULL, NULL, NULL),
(N'CHILDRENDAY', N'Quốc tế thiếu nhi 1/6', N'Giảm 20k cho đơn có trẻ em', N'SoTien', 20000.00, '2026-05-28', '2026-06-05', N'Đơn từ 100k', 300, N'Hoạt động', NULL, NULL, 100000.00, NULL, NULL, NULL),
(N'FAMILYDAY', N'Ngày gia đình VN 28/6', N'Giảm 15% cho nhóm từ 3 người', N'PhanTram', 15.00, '2026-06-20', '2026-06-30', N'Gắn kết gia đình', 200, N'Hoạt động', 50000.00, NULL, 150000.00, NULL, NULL, NULL),

-- 3. Khuyến mãi theo khung giờ (Happy Hour)
(N'MORNINGCF', N'Cà phê sáng tỉnh táo', N'Giảm 10k từ 7h - 9h sáng', N'SoTien', 10000.00, '2026-03-01', '2026-07-31', N'Uống cà phê sáng', 9999, N'Hoạt động', NULL, NULL, 30000.00, CAST('07:00:00' AS Time), CAST('09:00:00' AS Time), NULL),
(N'LUNCHBREAK', N'Nghỉ trưa nạp năng lượng', N'Giảm 15% từ 11h30 - 13h30', N'PhanTram', 15.00, '2026-03-01', '2026-07-31', N'Trưa mát mẻ', 9999, N'Hoạt động', 30000.00, NULL, 50000.00, CAST('11:30:00' AS Time), CAST('13:30:00' AS Time), NULL),
(N'NIGHTOWL', N'Cú đêm đọc sách', N'Giảm 20k từ 20h - 22h', N'SoTien', 20000.00, '2026-03-01', '2026-07-31', N'Đơn từ 100k', 9999, N'Hoạt động', NULL, NULL, 100000.00, CAST('20:00:00' AS Time), CAST('22:00:00' AS Time), NULL),

-- 4. Khuyến mãi theo thứ trong tuần
(N'HAPPYMONDAY', N'Thứ 2 vui vẻ', N'Giảm 15% đánh bay uể oải thứ 2', N'PhanTram', 15.00, '2026-03-01', '2026-07-31', N'Chỉ áp dụng thứ 2', 9999, N'Hoạt động', 40000.00, NULL, 0, NULL, NULL, N'2'),
(N'WEEKENDCHILL', N'Cuối tuần thư giãn', N'Giảm 10% T7 và CN', N'PhanTram', 10.00, '2026-03-01', '2026-07-31', N'Áp dụng cuối tuần', 9999, N'Hoạt động', 30000.00, NULL, 0, NULL, NULL, N'1,7'),

-- 5. Khuyến mãi giá trị hóa đơn (Upsell)
(N'BIGPARTY', N'Đi nhóm đông vui', N'Giảm 50k cho đơn từ 300k', N'SoTien', 50000.00, '2026-03-01', '2026-07-31', N'Đơn từ 300k', 500, N'Hoạt động', NULL, NULL, 300000.00, NULL, NULL, NULL),
(N'SUPERBIG', N'Bao trọn quán', N'Giảm 100k cho đơn từ 500k', N'SoTien', 100000.00, '2026-03-01', '2026-07-31', N'Đơn từ 500k', 200, N'Hoạt động', NULL, NULL, 500000.00, NULL, NULL, NULL),

-- 6. Khuyến mãi đối tượng / Dịch vụ
(N'STUDENT', N'Ưu đãi Học sinh - Sinh viên', N'Giảm 12% mọi lúc', N'PhanTram', 12.00, '2026-03-01', '2026-07-31', N'Thẻ HSSV', 1000, N'Hoạt động', 25000.00, NULL, 0, NULL, NULL, NULL),
(N'BOOKLOVER', N'Mọt sách xịn xò', N'Giảm 25k cho tín đồ mua mang về', N'SoTien', 25000.00, '2026-03-01', '2026-07-31', N'Đơn từ 120k', 800, N'Hoạt động', NULL, NULL, 120000.00, NULL, NULL, NULL),
(N'FREESHIP', N'Hỗ trợ giao hàng', N'Giảm 15k phí vận chuyển', N'SoTien', 15000.00, '2026-03-01', '2026-07-31', N'Cho đơn online trên 80k', 2000, N'Hoạt động', NULL, NULL, 80000.00, NULL, NULL, NULL),

-- 7. Khuyến mãi theo thời tiết mùa hè
(N'HOTDAY', N'Giải nhiệt mùa hè', N'Giảm 20% cho đồ uống lạnh', N'PhanTram', 20.00, '2026-05-15', '2026-07-31', N'Sale giải nhiệt', 1000, N'Hoạt động', 40000.00, NULL, 50000.00, NULL, NULL, NULL),
(N'RAINYDAY', N'Mưa buồn uống trà', N'Giảm 10k khi trời mưa rào', N'SoTien', 10000.00, '2026-06-01', '2026-07-31', N'Đơn tối thiểu 50k', 500, N'Hoạt động', NULL, NULL, 50000.00, NULL, NULL, NULL);

COMMIT TRANSACTION;
PRINT N'ĐÃ XONG! 20 chương trình khuyến mãi đã được cập nhật thành công.';
GO
USE [CafebookDB]
GO

SET NOCOUNT ON;

-- Cài đặt số lượng phiếu muốn sinh ra
DECLARE @SoLuongPhieu INT = 500; 
DECLARE @StartDate DATE = '2026-03-01';
DECLARE @EndDate DATE = GETDATE(); 
DECLARE @Counter INT = 1;

WHILE @Counter <= @SoLuongPhieu
BEGIN
    -- Lấy ngẫu nhiên Khách Hàng và Nhân Viên (Mặc định là 1 nếu bảng trống)
    DECLARE @RandomKhachHang INT = ISNULL((SELECT TOP 1 idKhachHang FROM KhachHang ORDER BY NEWID()), 1);
    DECLARE @RandomNhanVien INT = ISNULL((SELECT TOP 1 idNhanVien FROM NhanVien ORDER BY NEWID()), 1);

    -- Random Ngày Thuê
    DECLARE @DaysBetween INT = DATEDIFF(DAY, @StartDate, @EndDate);
    DECLARE @RandomDays INT = ABS(CHECKSUM(NEWID())) % (@DaysBetween + 1);
    DECLARE @NgayThue DATETIME = DATEADD(DAY, @RandomDays, @StartDate);
    
    -- Random giờ thuê từ 8h sáng đến 21h tối
    SET @NgayThue = DATEADD(HOUR, (ABS(CHECKSUM(NEWID())) % 14) + 8, @NgayThue);

    -- Random Trạng Thái (70% Đã trả, 30% Đang thuê)
    DECLARE @TrangThai NVARCHAR(50) = CASE WHEN ABS(CHECKSUM(NEWID())) % 100 < 70 THEN N'Đã trả' ELSE N'Đang thuê' END;
    
    -- 1. INSERT VÀO PhieuThueSach
    INSERT INTO PhieuThueSach (idKhachHang, idNhanVien, ngayThue, trangThai, tongTienCoc, PhuongThucThanhToan)
    VALUES (@RandomKhachHang, @RandomNhanVien, @NgayThue, @TrangThai, 0, CASE WHEN ABS(CHECKSUM(NEWID())) % 2 = 0 THEN N'Tiền mặt' ELSE N'Chuyển khoản' END);
    
    DECLARE @idPhieuThueSach INT = SCOPE_IDENTITY();
    
    -- Random số lượng sách mỗi phiếu (1 đến 3 cuốn)
    DECLARE @NumBooks INT = (ABS(CHECKSUM(NEWID())) % 3) + 1;
    DECLARE @BookCounter INT = 1;
    DECLARE @TongCoc DECIMAL(18,2) = 0;
    
    WHILE @BookCounter <= @NumBooks
    BEGIN
        -- Random Sách
        DECLARE @RandomSach INT = ISNULL((SELECT TOP 1 idSach FROM Sach ORDER BY NEWID()), 1);
        -- Tiền cọc sách từ 50,000 đến 90,000
        DECLARE @TienCoc DECIMAL(18,2) = (ABS(CHECKSUM(NEWID())) % 5 + 5) * 10000; 
        SET @TongCoc = @TongCoc + @TienCoc;
        
        -- Hẹn trả sau 14 ngày
        DECLARE @NgayHenTra DATETIME = DATEADD(DAY, 14, @NgayThue);
        
        -- Đảm bảo không trùng sách trong cùng 1 phiếu
        IF NOT EXISTS (SELECT 1 FROM ChiTietPhieuThue WHERE idPhieuThueSach = @idPhieuThueSach AND idSach = @RandomSach)
        BEGIN
            -- 2. INSERT VÀO ChiTietPhieuThue
            INSERT INTO ChiTietPhieuThue (idPhieuThueSach, idSach, ngayHenTra, DoMoiKhiThue, tienCoc)
            VALUES (@idPhieuThueSach, @RandomSach, @NgayHenTra, 100 - (ABS(CHECKSUM(NEWID())) % 20), @TienCoc);
            
            SET @BookCounter = @BookCounter + 1;
        END
    END

    -- Cập nhật tổng tiền cọc cho phiếu thuê
    UPDATE PhieuThueSach SET tongTienCoc = @TongCoc WHERE idPhieuThueSach = @idPhieuThueSach;

    -- Xử lý sinh dữ liệu Trả Sách nếu trạng thái là "Đã trả"
    IF @TrangThai = N'Đã trả'
    BEGIN
        -- Random ngày trả từ 1 đến 20 ngày sau ngày thuê
        DECLARE @NgayTra DATETIME = DATEADD(DAY, ABS(CHECKSUM(NEWID())) % 20 + 1, @NgayThue);
        IF @NgayTra > @EndDate SET @NgayTra = @EndDate;

        -- 3. INSERT VÀO PhieuTraSach (Ban đầu set tạm số liệu phạt = 0)
        INSERT INTO PhieuTraSach (IdPhieuThueSach, IdNhanVien, NgayTra, TongPhiThue, TongTienPhat, TongTienCocHoan, DiemTichLuy)
        VALUES (@idPhieuThueSach, @RandomNhanVien, @NgayTra, @NumBooks * 10000, 0, @TongCoc, @NumBooks);
        
        DECLARE @idPhieuTra INT = SCOPE_IDENTITY();
        DECLARE @TongPhat DECIMAL(18,2) = 0;

        -- Dùng Cursor nội bộ để duyệt qua từng chi tiết sách đã thuê
        DECLARE @CurSach INT;
        DECLARE @CurHenTra DATETIME;
        DECLARE curBooks CURSOR LOCAL FAST_FORWARD FOR 
            SELECT idSach, ngayHenTra FROM ChiTietPhieuThue WHERE idPhieuThueSach = @idPhieuThueSach;
        
        OPEN curBooks;
        FETCH NEXT FROM curBooks INTO @CurSach, @CurHenTra;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Tính phạt trễ hạn (5,000 VND / ngày trễ)
            DECLARE @PhatTre DECIMAL(18,2) = 0;
            IF @NgayTra > @CurHenTra 
                SET @PhatTre = DATEDIFF(DAY, @CurHenTra, @NgayTra) * 5000;
            
            -- Random phạt hư hỏng (Tỉ lệ 10% bị phạt 20,000 VND)
            DECLARE @PhatHuHong DECIMAL(18,2) = CASE WHEN ABS(CHECKSUM(NEWID())) % 10 = 0 THEN 20000 ELSE 0 END;
            
            -- Cập nhật ChiTietPhieuThue
            UPDATE ChiTietPhieuThue 
            SET ngayTraThucTe = @NgayTra, TienPhatTraTre = @PhatTre
            WHERE idPhieuThueSach = @idPhieuThueSach AND idSach = @CurSach;

            -- 4. INSERT VÀO ChiTietPhieuTra
            INSERT INTO ChiTietPhieuTra (IdPhieuTra, IdSach, TienPhat, TienPhatHuHong, DoMoiKhiTra, TinhTrangKhiTra)
            VALUES (@idPhieuTra, @CurSach, @PhatTre + @PhatHuHong, @PhatHuHong, 80, CASE WHEN @PhatHuHong > 0 THEN N'Hơi sờn góc' ELSE N'Bình thường' END);

            SET @TongPhat = @TongPhat + @PhatTre + @PhatHuHong;

            FETCH NEXT FROM curBooks INTO @CurSach, @CurHenTra;
        END
        
        CLOSE curBooks;
        DEALLOCATE curBooks;

        -- Cập nhật lại tổng tiền phạt và tiền cọc hoàn
        UPDATE PhieuTraSach 
        SET TongTienPhat = @TongPhat,
            TongTienCocHoan = CASE WHEN (@TongCoc - @TongPhat - TongPhiThue) < 0 THEN 0 ELSE (@TongCoc - @TongPhat - TongPhiThue) END
        WHERE IdPhieuTra = @idPhieuTra;
    END

    SET @Counter = @Counter + 1;
END

PRINT N'Hoàn tất chèn dữ liệu thuê/trả sách ngẫu nhiên!';
GO