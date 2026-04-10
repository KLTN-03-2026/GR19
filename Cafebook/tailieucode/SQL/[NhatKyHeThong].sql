USE [CafebookDB]; -- Đổi tên DB nếu cần
GO

CREATE TABLE [dbo].[NhatKyHeThong] (
    [IdNhatKy] INT IDENTITY(1,1) NOT NULL,
    [IdNhanVien] INT NULL, -- Ai làm? (Null nếu chưa đăng nhập hoặc hệ thống tự chạy)
    [HanhDong] NVARCHAR(50) NOT NULL, -- THEMMOI, CAPNHAT, XOA, DANGNHAP, DANGXUAT
    [BangBiAnhHuong] NVARCHAR(100) NOT NULL, -- Tên bảng (VD: SanPham, HoaDon)
    [KhoaChinh] NVARCHAR(100) NULL, -- ID của dòng bị tác động
    [DuLieuCu] NVARCHAR(MAX) NULL, -- Dữ liệu trước khi sửa (Lưu chuỗi JSON)
    [DuLieuMoi] NVARCHAR(MAX) NULL, -- Dữ liệu sau khi sửa (Lưu chuỗi JSON)
    [ThoiGian] DATETIME NOT NULL DEFAULT GETDATE(),
    [DiaChiIP] NVARCHAR(50) NULL,
    
    PRIMARY KEY CLUSTERED ([IdNhatKy] ASC),
    CONSTRAINT [FK_NhatKy_NhanVien] FOREIGN KEY ([IdNhanVien]) REFERENCES [dbo].[NhanVien] ([IdNhanVien]) ON DELETE SET NULL
);
GO