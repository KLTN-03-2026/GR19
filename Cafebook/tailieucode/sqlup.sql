USE [db49191]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** TẠO BẢNG (TABLES) ******/

CREATE TABLE [dbo].[Ban](
	[idBan] [int] IDENTITY(1,1) NOT NULL,
	[soBan] [nvarchar](50) NOT NULL,
	[soGhe] [int] NOT NULL,
	[trangThai] [nvarchar](50) NOT NULL,
	[ghiChu] [nvarchar](500) NULL,
	[idKhuVuc] [int] NULL,
PRIMARY KEY CLUSTERED ([idBan] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[BangChamCong]([idChamCong] [int] IDENTITY(1,1) NOT NULL,
	[idLichLamViec][int] NOT NULL,
	[gioVao] [datetime] NULL,
	[gioRa][datetime] NULL,
	[soGioLam]  AS (datediff(minute,[gioVao],[gioRa])/(60.0)),
	[ghiChuSua] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([idChamCong] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[CaiDat](
	[tenCaiDat] [nvarchar](100) NOT NULL,
	[giaTri] [nvarchar](max) NOT NULL,
	[moTa][nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([tenCaiDat] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[CaLamViec](
	[idCa] [int] IDENTITY(1,1) NOT NULL,
	[tenCa][nvarchar](100) NOT NULL,
	[gioBatDau] [time](7) NOT NULL,
	[gioKetThuc] [time](7) NOT NULL,
PRIMARY KEY CLUSTERED ([idCa] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ChatLichSu](
	[idChat] [bigint] IDENTITY(1,1) NOT NULL,[idKhachHang] [int] NULL,
	[idNhanVien] [int] NULL,
	[NoiDungHoi] [ntext] NOT NULL,
	[NoiDungTraLoi] [ntext] NOT NULL,
	[ThoiGian] [datetime] NULL,[LoaiChat] [nvarchar](50) NULL,
	[LoaiTinNhan] [nvarchar](20) NULL,
	[IdThongBaoHoTro] [int] NULL,[GuestSessionId] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED ([idChat] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[ChiTietHoaDon](
	[idChiTietHoaDon] [int] IDENTITY(1,1) NOT NULL,
	[idHoaDon] [int] NOT NULL,
	[idSanPham] [int] NOT NULL,
	[soLuong] [int] NOT NULL,
	[donGia] [decimal](18, 2) NOT NULL,
	[thanhTien]  AS ([soLuong]*[donGia]),
	[ghiChu] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([idChiTietHoaDon] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ChiTietKiemKho](
	[idPhieuKiemKho] [int] NOT NULL,
	[idNguyenLieu] [int] NOT NULL,
	[TonKhoHeThong][decimal](18, 2) NOT NULL,
	[TonKhoThucTe] [decimal](18, 2) NOT NULL,
	[ChenhLech]  AS ([TonKhoThucTe]-[TonKhoHeThong]),
	[LyDoChenhLech] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED ([idPhieuKiemKho] ASC, [idNguyenLieu] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ChiTietNhapKho](
	[idPhieuNhapKho][int] NOT NULL,
	[idNguyenLieu] [int] NOT NULL,[soLuongNhap] [decimal](18, 2) NOT NULL,
	[donGiaNhap][decimal](18, 2) NOT NULL,
	[thanhTien]  AS ([soLuongNhap]*[donGiaNhap]),
PRIMARY KEY CLUSTERED ([idPhieuNhapKho] ASC, [idNguyenLieu] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ChiTietPhieuThue]([idPhieuThueSach] [int] NOT NULL,
	[idSach] [int] NOT NULL,[ngayHenTra] [datetime] NOT NULL,
	[ngayTraThucTe] [datetime] NULL,
	[tienCoc] [decimal](18, 2) NOT NULL,[TienPhatTraTre] [decimal](18, 2) NULL,[DoMoiKhiThue] [int] NULL,
	[GhiChuKhiThue] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([idPhieuThueSach] ASC, [idSach] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ChiTietPhieuTra]([IdPhieuTra] [int] NOT NULL,
	[IdSach] [int] NOT NULL,
	[TienPhat] [decimal](18, 2) NOT NULL,[TinhTrangKhiTra] [nvarchar](255) NULL,
	[TienPhatHuHong][decimal](18, 2) NULL,
	[DoMoiKhiTra] [int] NULL,
	[GhiChuKhiTra] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([IdPhieuTra] ASC, [IdSach] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ChiTietPhuThuHoaDon](
	[idHoaDon] [int] NOT NULL,
	[idPhuThu] [int] NOT NULL,
	[SoTien][decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED ([idHoaDon] ASC, [idPhuThu] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ChiTietXuatHuy](
	[idPhieuXuatHuy] [int] NOT NULL,
	[idNguyenLieu] [int] NOT NULL,
	[SoLuong] [decimal](18, 2) NOT NULL,
	[DonGiaVon] [decimal](18, 2) NOT NULL,
	[ThanhTien]  AS ([SoLuong]*[DonGiaVon]),
PRIMARY KEY CLUSTERED ([idPhieuXuatHuy] ASC, [idNguyenLieu] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[DanhGia](
	[idDanhGia] [int] IDENTITY(1,1) NOT NULL,
	[idKhachHang] [int] NOT NULL,[idSanPham] [int] NULL,
	[idHoaDon] [int] NOT NULL,[SoSao] [int] NOT NULL,
	[BinhLuan] [nvarchar](max) NULL,
	[HinhAnhURL] [nvarchar](max) NULL,
	[NgayTao][datetime] NOT NULL,
	[TrangThai] [nvarchar](50) NOT NULL,
 CONSTRAINT[PK_DanhGia] PRIMARY KEY CLUSTERED ([idDanhGia] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[DanhMuc](
	[idDanhMuc][int] IDENTITY(1,1) NOT NULL,
	[tenDanhMuc] [nvarchar](255) NOT NULL,
	[idDanhMucCha] [int] NULL,
PRIMARY KEY CLUSTERED ([idDanhMuc] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[DeXuatSach](
	[idSachGoc] [int] NOT NULL,
	[idSachDeXuat] [int] NOT NULL,
	[DoLienQuan] [float] NOT NULL,[LoaiDeXuat] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED ([idSachGoc] ASC, [idSachDeXuat] ASC, [LoaiDeXuat] ASC)
) ON[PRIMARY]
GO

CREATE TABLE [dbo].[DeXuatSanPham](
	[idSanPhamGoc][int] NOT NULL,
	[idSanPhamDeXuat] [int] NOT NULL,[DoLienQuan] [float] NOT NULL,
	[LoaiDeXuat] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED ([idSanPhamGoc] ASC,[idSanPhamDeXuat] ASC, [LoaiDeXuat] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[DinhLuong](
	[idSanPham] [int] NOT NULL,[idNguyenLieu] [int] NOT NULL,
	[SoLuongSuDung] [decimal](18, 2) NOT NULL,
	[idDonViSuDung] [int] NULL,
PRIMARY KEY CLUSTERED ([idSanPham] ASC, [idNguyenLieu] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[DonViChuyenDoi](
	[idChuyenDoi][int] IDENTITY(1,1) NOT NULL,
	[idNguyenLieu] [int] NOT NULL,
	[TenDonVi] [nvarchar](50) NOT NULL,[GiaTriQuyDoi] [decimal](18, 6) NOT NULL,[LaDonViCoBan] [bit] NOT NULL,
PRIMARY KEY CLUSTERED ([idChuyenDoi] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[DonXinNghi]([idDonXinNghi] [int] IDENTITY(1,1) NOT NULL,
	[idNhanVien][int] NOT NULL,
	[LoaiDon] [nvarchar](100) NOT NULL,
	[LyDo] [nvarchar](500) NOT NULL,
	[NgayBatDau][datetime] NOT NULL,
	[NgayKetThuc] [datetime] NOT NULL,[TrangThai] [nvarchar](50) NOT NULL,
	[idNguoiDuyet][int] NULL,
	[NgayDuyet] [datetime] NULL,[GhiChuPheDuyet] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED ([idDonXinNghi] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[GiaoDichThanhToan](
	[idGiaoDich] [int] IDENTITY(1,1) NOT NULL,[idHoaDon] [int] NOT NULL,
	[MaGiaoDichNgoai] [nvarchar](100) NOT NULL,
	[CongThanhToan] [nvarchar](50) NOT NULL,
	[SoTien] [decimal](18, 2) NOT NULL,[ThoiGianGiaoDich] [datetime] NULL,
	[TrangThai] [nvarchar](100) NOT NULL,
	[MaLoi] [nvarchar](50) NULL,[MoTaLoi] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([idGiaoDich] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[GopY](
	[IdGopY] [int] IDENTITY(1,1) NOT NULL,
	[HoTen] [nvarchar](255) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[NoiDung] [nvarchar](max) NOT NULL,
	[NgayTao][datetime] NOT NULL,
	[TrangThai] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED ([IdGopY] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[HoaDon](
	[idHoaDon] [int] IDENTITY(1,1) NOT NULL,
	[idBan] [int] NULL,[idNhanVien] [int] NULL,
	[idKhachHang] [int] NULL,[thoiGianTao] [datetime] NULL,
	[thoiGianThanhToan] [datetime] NULL,
	[trangThai] [nvarchar](50) NOT NULL,[tongTienGoc] [decimal](18, 2) NULL,
	[giamGia] [decimal](18, 2) NULL,
	[TongPhuThu] [decimal](18, 2) NULL,
	[thanhTien]  AS (([tongTienGoc]-[giamGia])+[TongPhuThu]),
	[phuongThucThanhToan] [nvarchar](50) NULL,
	[ghiChu] [nvarchar](max) NULL,[LoaiHoaDon] [nvarchar](50) NOT NULL,
	[TrangThaiGiaoHang] [nvarchar](100) NULL,
	[DiaChiGiaoHang] [nvarchar](500) NULL,
	[SoDienThoaiGiaoHang] [nvarchar](20) NULL,[idNguoiGiaoHang] [int] NULL,
PRIMARY KEY CLUSTERED ([idHoaDon] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[HoaDon_KhuyenMai](
	[idHoaDon] [int] NOT NULL,[idKhuyenMai] [int] NOT NULL,
PRIMARY KEY CLUSTERED ([idHoaDon] ASC,[idKhuyenMai] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[KhachHang](
	[idKhachHang] [int] IDENTITY(1,1) NOT NULL,[hoTen] [nvarchar](255) NOT NULL,
	[soDienThoai][nvarchar](20) NULL,
	[email] [nvarchar](100) NULL,[diaChi] [nvarchar](500) NULL,
	[diemTichLuy] [int] NULL,
	[tenDangNhap] [nvarchar](100) NULL,[matKhau] [nvarchar](255) NULL,
	[ngayTao] [datetime] NULL,
	[BiKhoa] [bit] NOT NULL,
	[AnhDaiDien] [nvarchar](max) NULL,
	[taiKhoanTam] [bit] NOT NULL,
	[DaXoa][bit] NOT NULL,
	[lyDoKhoa] [nvarchar](500) NULL,
	[thoiGianMoKhoa] [datetime] NULL,
PRIMARY KEY CLUSTERED ([idKhachHang] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[KhuVuc](
	[idKhuVuc] [int] IDENTITY(1,1) NOT NULL,
	[TenKhuVuc] [nvarchar](100) NOT NULL,[MoTa] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([idKhuVuc] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[KhuyenMai]([idKhuyenMai] [int] IDENTITY(1,1) NOT NULL,[maKhuyenMai] [nvarchar](50) NOT NULL,
	[tenChuongTrinh] [nvarchar](255) NOT NULL,
	[moTa] [ntext] NULL,[loaiGiamGia] [nvarchar](20) NOT NULL,
	[giaTriGiam] [decimal](18, 2) NOT NULL,
	[ngayBatDau] [datetime] NOT NULL,
	[ngayKetThuc] [datetime] NOT NULL,[dieuKienApDung] [nvarchar](500) NULL,
	[soLuongConLai] [int] NULL,
	[TrangThai] [nvarchar](50) NOT NULL,
	[GiamToiDa] [decimal](18, 2) NULL,
	[IdSanPhamApDung] [int] NULL,
	[HoaDonToiThieu] [decimal](18, 2) NULL,
	[GioBatDau] [time](7) NULL,
	[GioKetThuc] [time](7) NULL,
	[NgayTrongTuan] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED ([idKhuyenMai] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[LichLamViec](
	[idLichLamViec] [int] IDENTITY(1,1) NOT NULL,
	[idNhanVien] [int] NOT NULL,
	[idCa] [int] NOT NULL,
	[ngayLam][date] NOT NULL,
	[trangThai] [nvarchar](50) NOT NULL,[ghiChu] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED ([idLichLamViec] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[NguoiGiaoHang](
	[idNguoiGiaoHang] [int] IDENTITY(1,1) NOT NULL,
	[TenNguoiGiaoHang] [nvarchar](100) NOT NULL,[SoDienThoai] [nvarchar](20) NOT NULL,
	[TrangThai][nvarchar](50) NULL,
PRIMARY KEY CLUSTERED ([idNguoiGiaoHang] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[NguyenLieu]([idNguyenLieu] [int] IDENTITY(1,1) NOT NULL,
	[tenNguyenLieu] [nvarchar](255) NOT NULL,
	[donViTinh] [nvarchar](50) NOT NULL,
	[tonKho] [decimal](18, 2) NOT NULL,[TonKhoToiThieu] [decimal](18, 2) NOT NULL,
PRIMARY KEY CLUSTERED ([idNguyenLieu] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[NhaCungCap](
	[idNhaCungCap] [int] IDENTITY(1,1) NOT NULL,
	[tenNhaCungCap] [nvarchar](255) NOT NULL,[soDienThoai] [nvarchar](20) NULL,
	[diaChi] [nvarchar](500) NULL,
	[email] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED ([idNhaCungCap] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[NhanVien](
	[idNhanVien] [int] IDENTITY(1,1) NOT NULL,
	[hoTen] [nvarchar](255) NOT NULL,[soDienThoai] [nvarchar](20) NOT NULL,
	[email] [nvarchar](100) NULL,
	[diaChi] [nvarchar](500) NULL,[ngayVaoLam] [date] NOT NULL,
	[idVaiTro] [int] NOT NULL,
	[luongCoBan] [decimal](18, 2) NULL,[trangThaiLamViec] [nvarchar](50) NOT NULL,
	[tenDangNhap] [nvarchar](100) NOT NULL,
	[matKhau] [nvarchar](255) NOT NULL,
	[AnhDaiDien] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED ([idNhanVien] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE[dbo].[NhanVien_Quyen](
	[IdNhanVien] [int] NOT NULL,
	[IdQuyen] [nvarchar](100) NOT NULL,
 CONSTRAINT[PK_NhanVien_Quyen] PRIMARY KEY CLUSTERED ([IdNhanVien] ASC,[IdQuyen] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[NhatKyHeThong](
	[IdNhatKy] [int] IDENTITY(1,1) NOT NULL,[IdNhanVien] [int] NULL,
	[HanhDong] [nvarchar](50) NOT NULL,
	[BangBiAnhHuong] [nvarchar](100) NOT NULL,[KhoaChinh] [nvarchar](100) NULL,
	[DuLieuCu][nvarchar](max) NULL,
	[DuLieuMoi] [nvarchar](max) NULL,[ThoiGian] [datetime] NOT NULL,
	[DiaChiIP] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED ([IdNhatKy] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[NhatKyHuyMon](
	[idNhatKy] [int] IDENTITY(1,1) NOT NULL,
	[idHoaDon] [int] NOT NULL,[idSanPham] [int] NOT NULL,
	[SoLuongHuy] [int] NOT NULL,
	[LyDo] [nvarchar](255) NOT NULL,[idNhanVienHuy] [int] NOT NULL,
	[ThoiGianHuy] [datetime] NULL,
PRIMARY KEY CLUSTERED ([idNhatKy] ASC)
) ON [PRIMARY]
GO

CREATE TABLE[dbo].[NhaXuatBan](
	[idNhaXuatBan] [int] IDENTITY(1,1) NOT NULL,
	[tenNhaXuatBan] [nvarchar](255) NOT NULL,
	[MoTa] [ntext] NULL,
PRIMARY KEY CLUSTERED ([idNhaXuatBan] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[NhuCauCaLam](
	[idNhuCau] [int] IDENTITY(1,1) NOT NULL,
	[ngayLam] [date] NOT NULL,
	[idCa] [int] NOT NULL,
	[idVaiTro] [int] NOT NULL,[soLuongCan] [int] NOT NULL,
	[loaiYeuCau] [nvarchar](50) NULL,
	[ghiChu] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED ([idNhuCau] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PhanHoiDanhGia](
	[idPhanHoi] [int] IDENTITY(1,1) NOT NULL,
	[idDanhGia] [int] NOT NULL,
	[idNhanVien] [int] NOT NULL,
	[NoiDung] [nvarchar](max) NOT NULL,[NgayTao] [datetime] NOT NULL,
 CONSTRAINT [PK_PhanHoiDanhGia] PRIMARY KEY CLUSTERED ([idPhanHoi] ASC)
) ON [PRIMARY] TEXTIMAGE_ON[PRIMARY]
GO

CREATE TABLE [dbo].[PhieuDatBan](
	[idPhieuDatBan] [int] IDENTITY(1,1) NOT NULL,
	[idKhachHang] [int] NULL,
	[idBan] [int] NOT NULL,
	[hoTenKhach] [nvarchar](100) NULL,
	[sdtKhach] [nvarchar](20) NULL,
	[thoiGianDat] [datetime] NOT NULL,
	[soLuongKhach] [int] NOT NULL,
	[trangThai] [nvarchar](50) NOT NULL,
	[ghiChu] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([idPhieuDatBan] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PhieuKiemKho]([idPhieuKiemKho] [int] IDENTITY(1,1) NOT NULL,[idNhanVienKiem] [int] NOT NULL,
	[NgayKiem] [datetime] NULL,
	[GhiChu] [nvarchar](500) NULL,
	[TrangThai][nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED ([idPhieuKiemKho] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PhieuLuong]([idPhieuLuong] [int] IDENTITY(1,1) NOT NULL,
	[idNhanVien][int] NOT NULL,
	[thang] [int] NOT NULL,
	[nam][int] NOT NULL,
	[luongCoBan] [decimal](18, 2) NOT NULL,
	[tongGioLam] [decimal](18, 2) NOT NULL,[tienThuong] [decimal](18, 2) NULL,
	[khauTru] [decimal](18, 2) NULL,
	[thucLanh] [decimal](18, 2) NOT NULL,
	[ngayTao] [datetime] NULL,
	[trangThai][nvarchar](50) NOT NULL,
	[NgayPhatLuong] [datetime] NULL,[IdNguoiPhat] [int] NULL,
PRIMARY KEY CLUSTERED ([idPhieuLuong] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PhieuNhapKho]([idPhieuNhapKho] [int] IDENTITY(1,1) NOT NULL,[idNhaCungCap] [int] NULL,
	[idNhanVien] [int] NOT NULL,
	[ngayNhap] [datetime] NULL,
	[tongTien] [decimal](18, 2) NULL,
	[ghiChu] [nvarchar](500) NULL,[TrangThai] [nvarchar](50) NOT NULL,
	[HoaDonDinhKem][nvarchar](max) NULL,
PRIMARY KEY CLUSTERED ([idPhieuNhapKho] ASC)
) ON[PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[PhieuThueSach](
	[idPhieuThueSach] [int] IDENTITY(1,1) NOT NULL,[idKhachHang] [int] NOT NULL,
	[idNhanVien] [int] NULL,
	[ngayThue] [datetime] NULL,
	[trangThai] [nvarchar](50) NOT NULL,
	[tongTienCoc] [decimal](18, 2) NULL,
	[DiaChiGiaoHang] [nvarchar](500) NULL,[PhuongThucThanhToan] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED ([idPhieuThueSach] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PhieuThuongPhat](
	[idPhieuThuongPhat] [int] IDENTITY(1,1) NOT NULL,
	[idNhanVien] [int] NOT NULL,[idNguoiTao] [int] NOT NULL,
	[NgayTao] [datetime] NOT NULL,
	[SoTien] [decimal](18, 2) NOT NULL,
	[LyDo] [nvarchar](500) NOT NULL,
	[idPhieuLuong] [int] NULL,
PRIMARY KEY CLUSTERED ([idPhieuThuongPhat] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PhieuTraSach](
	[IdPhieuTra] [int] IDENTITY(1,1) NOT NULL,
	[IdPhieuThueSach] [int] NOT NULL,
	[IdNhanVien] [int] NOT NULL,
	[NgayTra] [datetime] NOT NULL,
	[TongPhiThue] [decimal](18, 2) NOT NULL,
	[TongTienPhat] [decimal](18, 2) NOT NULL,[TongTienCocHoan] [decimal](18, 2) NOT NULL,[DiemTichLuy] [int] NOT NULL,
PRIMARY KEY CLUSTERED ([IdPhieuTra] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PhieuXuatHuy]([idPhieuXuatHuy] [int] IDENTITY(1,1) NOT NULL,[idNhanVienXuat] [int] NOT NULL,
	[NgayXuatHuy][datetime] NULL,
	[LyDoXuatHuy] [nvarchar](500) NOT NULL,
	[TongGiaTriHuy] [decimal](18, 2) NULL,
PRIMARY KEY CLUSTERED ([idPhieuXuatHuy] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PhuThu](
	[idPhuThu] [int] IDENTITY(1,1) NOT NULL,
	[TenPhuThu] [nvarchar](100) NOT NULL,
	[GiaTri][decimal](18, 2) NOT NULL,
	[LoaiGiaTri] [nvarchar](20) NOT NULL,
PRIMARY KEY CLUSTERED ([idPhuThu] ASC)
) ON[PRIMARY]
GO

CREATE TABLE [dbo].[Quyen](
	[idQuyen] [nvarchar](100) NOT NULL,
	[TenQuyen] [nvarchar](255) NOT NULL,
	[NhomQuyen] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED ([idQuyen] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Sach]([idSach] [int] IDENTITY(1,1) NOT NULL,
	[tenSach] [nvarchar](500) NOT NULL,
	[namXuatBan] [int] NULL,[moTa] [ntext] NULL,
	[soLuongTong] [int] NOT NULL,[soLuongHienCo] [int] NOT NULL,
	[AnhBia] [nvarchar](max) NULL,
	[GiaBia] [decimal](18, 2) NULL,[ViTri] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED ([idSach] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[Sach_NhaXuatBan](
	[idSach] [int] NOT NULL,[idNhaXuatBan] [int] NOT NULL,
PRIMARY KEY CLUSTERED ([idSach] ASC,[idNhaXuatBan] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Sach_TacGia](
	[idSach] [int] NOT NULL,
	[idTacGia][int] NOT NULL,
PRIMARY KEY CLUSTERED ([idSach] ASC, [idTacGia] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Sach_TheLoai]([idSach] [int] NOT NULL,
	[idTheLoai] [int] NOT NULL,
PRIMARY KEY CLUSTERED ([idSach] ASC, [idTheLoai] ASC)
) ON[PRIMARY]
GO

CREATE TABLE [dbo].[SanPham](
	[idSanPham] [int] IDENTITY(1,1) NOT NULL,
	[tenSanPham] [nvarchar](255) NOT NULL,[idDanhMuc] [int] NOT NULL,
	[giaBan] [decimal](18, 2) NOT NULL,
	[moTa] [ntext] NULL,[trangThaiKinhDoanh] [bit] NOT NULL,
	[HinhAnh] [nvarchar](max) NULL,
	[NhomIn] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED ([idSanPham] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[TacGia](
	[idTacGia] [int] IDENTITY(1,1) NOT NULL,
	[tenTacGia] [nvarchar](255) NOT NULL,
	[gioiThieu] [ntext] NULL,
PRIMARY KEY CLUSTERED ([idTacGia] ASC)
) ON[PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[TheLoai]([idTheLoai] [int] IDENTITY(1,1) NOT NULL,[tenTheLoai] [nvarchar](255) NOT NULL,
	[MoTa] [ntext] NULL,
PRIMARY KEY CLUSTERED ([idTheLoai] ASC)
) ON [PRIMARY] TEXTIMAGE_ON[PRIMARY]
GO

CREATE TABLE [dbo].[ThongBao](
	[idThongBao] [int] IDENTITY(1,1) NOT NULL,
	[idNhanVienTao][int] NULL,
	[NoiDung] [nvarchar](500) NOT NULL,[ThoiGianTao] [datetime] NOT NULL,
	[LoaiThongBao] [nvarchar](50) NULL,
	[IdLienQuan] [int] NULL,
	[DaXem][bit] NOT NULL,
PRIMARY KEY CLUSTERED ([idThongBao] ASC)
) ON[PRIMARY]
GO

CREATE TABLE [dbo].[ThongBaoHoTro]([IdThongBao] [int] IDENTITY(1,1) NOT NULL,
	[IdKhachHang][int] NULL,
	[NoiDungYeuCau] [nvarchar](max) NULL,[ThoiGianTao] [datetime] NOT NULL,
	[TrangThai] [nvarchar](50) NOT NULL,
	[IdNhanVien] [int] NULL,[ThoiGianPhanHoi] [datetime] NULL,
	[GhiChu] [nvarchar](500) NULL,
	[GuestSessionId] [nvarchar](100) NULL,
 CONSTRAINT[PK_ThongBaoHoTro] PRIMARY KEY CLUSTERED ([IdThongBao] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[TrangThaiCheBien]([IdTrangThaiCheBien] [int] IDENTITY(1,1) NOT NULL,[IdChiTietHoaDon] [int] NOT NULL,
	[IdHoaDon] [int] NOT NULL,
	[IdSanPham] [int] NOT NULL,
	[TenMon] [nvarchar](255) NOT NULL,
	[SoBan] [nvarchar](50) NOT NULL,[SoLuong] [int] NOT NULL,
	[GhiChu] [nvarchar](500) NULL,
	[NhomIn] [nvarchar](50) NULL,
	[TrangThai] [nvarchar](50) NOT NULL,
	[ThoiGianGoi] [datetime] NOT NULL,[ThoiGianBatDau] [datetime] NULL,
	[ThoiGianHoanThanh][datetime] NULL,
PRIMARY KEY CLUSTERED ([IdTrangThaiCheBien] ASC)
) ON[PRIMARY]
GO

CREATE TABLE [dbo].[VaiTro](
	[idVaiTro] [int] IDENTITY(1,1) NOT NULL,
	[tenVaiTro] [nvarchar](100) NOT NULL,
	[moTa] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED ([idVaiTro] ASC)
) ON [PRIMARY]
GO


/****** CHÈN DỮ LIỆU (INSERT DATA) ******/

SET IDENTITY_INSERT [dbo].[Ban] ON 
INSERT [dbo].[Ban] ([idBan], [soBan], [soGhe], [trangThai], [ghiChu],[idKhuVuc]) VALUES (1, N'T1-B1', 5, N'Có khách', N'', 1)
INSERT [dbo].[Ban] ([idBan], [soBan],[soGhe], [trangThai], [ghiChu], [idKhuVuc]) VALUES (2, N'T1-B2', 4, N'Bảo trì', N'[Sự cố NV báo]: bàn gãy chân', 1)
INSERT [dbo].[Ban] ([idBan], [soBan],[soGhe], [trangThai], [ghiChu], [idKhuVuc]) VALUES (3, N'T2-B1', 6, N'Trống', N'', 2)
SET IDENTITY_INSERT [dbo].[Ban] OFF
GO

SET IDENTITY_INSERT [dbo].[BangChamCong] ON 
INSERT [dbo].[BangChamCong] ([idChamCong], [idLichLamViec],[gioVao], [gioRa], [ghiChuSua]) VALUES (1, 37, CAST(N'2026-04-20T07:15:00.000' AS DateTime), CAST(N'2026-04-20T11:55:00.000' AS DateTime), N'chỉnh lại do nhân viên quên out ca')
INSERT [dbo].[BangChamCong] ([idChamCong], [idLichLamViec], [gioVao], [gioRa], [ghiChuSua]) VALUES (2, 43, CAST(N'2026-04-21T07:00:00.000' AS DateTime), CAST(N'2026-04-21T11:30:00.000' AS DateTime), N'ok')
SET IDENTITY_INSERT [dbo].[BangChamCong] OFF
GO

INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'AI_Chat_API_Key', N'AIzaSyBlomT_JUMnIer7akPKWxZE1EW_XNMs--ca', N'API Key cho dịch vụ (OpenAI, Gemini...)')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'AI_Chat_Endpoint', N'https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent', N'Endpoint của dịch vụ AI Chat')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'DiemTichLuy_DoiVND', N'500', N'1 điểm tích lũy bằng ... VND trừ vào hóa đơn')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'DiemTichLuy_NhanVND', N'10000', N'Mỗi ... VND trong hóa đơn được 1 điểm')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'HR_ChuyenCan_SoGio', N'120', N'Số giờ công tối thiểu trong tháng yêu cầu để đạt thưởng chuyên cần.')
INSERT[dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'HR_ChuyenCan_TienThuong', N'100000', N'Số tiền thưởng (VND) khi đạt chuyên cần.')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'HR_HeSoOT', N'1.5', N'Hệ số lương khi làm tăng ca (Overtime) (ví dụ: 1.5)')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'HR_PhatDiTre_Phut', N'10', N'Số phút cho phép đi trễ. Vượt quá ngưỡng này bắt đầu tính phạt.')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'HR_PhatDiTreMoiLan', N'5000', N'Tiền phạt đi trễ mỗi một lần')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'HR_PhatRaSom_Phut', N'10', N'Số phút cho phép ra ca sớm Vượt quá ngưỡng này bắt đầu tính phạt.')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'HR_PhatVeSomMoiLan', N'6000', N'Tiền phạt về sóm mỗi một lần')
INSERT [dbo].[CaiDat] ([tenCaiDat],[giaTri], [moTa]) VALUES (N'HR_TinhTangCa_Phut', N'60', N'Số phút cho phép được tính là tăng ca của mỗi một ca')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'HR_VaoCaSom_Phut', N'30', N'Cho phép nhân viên vào ca sớm Phút')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'LienHe_Email', N'cafebook.hotro@gmail.com', N'Gmail của quán')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'LienHe_Facebook', N'https://www.facebook.com/lamtoan24/', N'Link Facebook quán (Đã sửa lỗi typo Facbook)')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri],[moTa]) VALUES (N'LienHe_GoogleMapsEmbed', N'https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3919.106598502801!2d106.7010418153489!3d10.80311546168051!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x317528a459b2184f%3A0x805d52140130f4d3!2zVHLGsOG7nW5nIMSQ4bqhaSBo4buNYyBIw7luZyBCw6BuZw!5e0!3m2!1svi!2s!4v1678888888888!5m2!1svi!2s', N'Link GoogleMapsEmbed Quán')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'LienHe_Instagram', N'https://instagram.com/lamtoan24', N'Link Instagram quán')
INSERT [dbo].[CaiDat] ([tenCaiDat],[giaTri], [moTa]) VALUES (N'LienHe_Website', N'https://cafebook.vn', N'Website quán')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'LienHe_X', N'https://x.com/', N'Link X quán')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri],[moTa]) VALUES (N'LienHe_Youtube', N'https://www.youtube.com/@Shu.otaku.t', N'Link Youtube quán')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'LienHe_Zalo', N'https://id.zalo.me/account?continue=https%3A%2F%2Fchat.zalo.me%2F', N'Zalo Quán')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'NganHang_ChuTaiKhoan', N'Lam Chu Bao Toan', N'Tên chủ tài khoản  ngân hàng')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'NganHang_MaDinhDanhNganHang', N'970422', N'Mã định Danh của ngân hàng thụ hưởng')
INSERT [dbo].[CaiDat] ([tenCaiDat],[giaTri], [moTa]) VALUES (N'NganHang_SoTaiKhoan', N'0376512695', N'Số tài khoản ngân hàng ')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'Sach_DiemPhieuThue', N'5', N'Diểm nhận được trên 1 phiếu trả sách.')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'Sach_PhatGiamDoMoi1Percent', N'2000', N'Số tiền phạt cho mỗi 1% độ mới bị sụt giảm')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'Sach_PhiThue', N'10000', N'Phí dịch vụ thuê sách được trừ sau khi trả sách')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'Sach_PhiTraTreMoiNgay', N'5000', N'Số tiền (VND) phạt nếu khách trả sách trễ 1 ngày')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri],[moTa]) VALUES (N'Sach_SoNgayMuonToiDa', N'30', N'Số Ngày Mượn sách tối đa')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'Smtp_EnableSsl', N'true', N'Bật bảo mật SSL/TLS (true/false)')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'Smtp_FromName', N'Cafebook Hỗ Trợ', N'Tên người gửi hiển thị trong email khách hàng nhận')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'Smtp_Host', N'smtp.gmail.com', N'Máy chủ gửi mail SMTP')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'Smtp_Password', N'raja nenx mxhk vtvn', N'Mật khẩu ứng dụng (App Password) của Gmail')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'Smtp_Port', N'587', N'Cổng kết nối SMTP')
INSERT[dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'Smtp_Username', N'cafebook.hotro@gmail.com', N'Tài khoản Gmail gửi hỗ trợ')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri],[moTa]) VALUES (N'ThongTin_DiaChi', N'08 Hà Văn Tín, P. Hòa Khánh Nam, Q. Liên Chiểu, TP. Đà Nẵng', N'Địa chỉ in trên hóa đơn')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri],[moTa]) VALUES (N'ThongTin_GioDongCua', N'22:00', N'Giờ đóng cửa quán')
INSERT [dbo].[CaiDat] ([tenCaiDat],[giaTri], [moTa]) VALUES (N'ThongTin_GioiThieu', N'Cafebook là không gian lý tưởng, kết hợp giữa niềm đam mê cà phê và tình yêu sách. Chúng tôi mang đến những hạt cà phê chất lượng cùng hàng ngàn đầu sách chọn lọc, tạo nên một ốc đảo bình yên cho tâm hồn bạn. tests', N'Giới thiệu được hiển thị ở trên web')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'ThongTin_GioMoCua', N'07:00', N'Giờ mở cửa quán')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'ThongTin_SoDienThoai', N'0376512695', N'Số Điện Thoại Liên Hệ')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'ThongTin_TenQuan', N'Cafe Sách Bookshuheheee', N'Tên quán hiển thị trên hóa đơn, trang web')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'ThongTin_ThuMoCua', N'2,3,4,5,6,7,8', N'Các thứ mở cửa trong tuần  từ  2-8 cách nhau bởi dấu phẩy'',"')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'VNPay_HashSecret', N'YK4I1AD53ANFTLC1CJIXNUSUJ45NLA2T', N'Chuỗi bí mật tạo checksum VNPay')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'VNPay_TmnCode', N'5KL790VC', N'Mã Terminal ID của VNPay')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'VNPay_Url', N' https://sandbox.vnpayment.vn/paymentv2/vpcpay.html', N'Đường dẫn thanh toán VNPay')
INSERT [dbo].[CaiDat] ([tenCaiDat], [giaTri], [moTa]) VALUES (N'Wifi_MatKhau', N'Shu.0311', N'Mật khẩu Wifi cho khách')
GO

SET IDENTITY_INSERT [dbo].[CaLamViec] ON 
INSERT [dbo].[CaLamViec] ([idCa], [tenCa], [gioBatDau],[gioKetThuc]) VALUES (1, N'FT-Sáng', CAST(N'07:00:00' AS Time), CAST(N'12:00:00' AS Time))
INSERT [dbo].[CaLamViec] ([idCa], [tenCa], [gioBatDau],[gioKetThuc]) VALUES (2, N'FT-Chiều', CAST(N'12:00:00' AS Time), CAST(N'17:00:00' AS Time))
INSERT [dbo].[CaLamViec] ([idCa], [tenCa], [gioBatDau],[gioKetThuc]) VALUES (3, N'FT-Tối', CAST(N'17:00:00' AS Time), CAST(N'22:00:00' AS Time))
INSERT [dbo].[CaLamViec] ([idCa], [tenCa], [gioBatDau], [gioKetThuc]) VALUES (4, N'PT-Trưa', CAST(N'09:00:00' AS Time), CAST(N'13:00:00' AS Time))
INSERT [dbo].[CaLamViec] ([idCa], [tenCa],[gioBatDau], [gioKetThuc]) VALUES (5, N'PT-Chều', CAST(N'13:00:00' AS Time), CAST(N'17:00:00' AS Time))
INSERT [dbo].[CaLamViec] ([idCa], [tenCa],[gioBatDau], [gioKetThuc]) VALUES (6, N'PT-Tối', CAST(N'17:00:00' AS Time), CAST(N'21:00:00' AS Time))
SET IDENTITY_INSERT [dbo].[CaLamViec] OFF
GO

SET IDENTITY_INSERT [dbo].[ChiTietHoaDon] ON 
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (1, 1, 1, 1, CAST(15000.00 AS Decimal(18, 2)), N'ds')
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (2, 2, 2, 1, CAST(20000.00 AS Decimal(18, 2)), N'tghd')
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (4, 3, 2, 1, CAST(20000.00 AS Decimal(18, 2)), N'a')
INSERT[dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon],[idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (5, 3, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (6, 4, 2, 3, CAST(20000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon],[idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (7, 4, 1, 5, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (8, 5, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia],[ghiChu]) VALUES (9, 6, 1, 2, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (10, 7, 1, 1, CAST(15000.00 AS Decimal(18, 2)), N'deb')
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (11, 8, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong],[donGia], [ghiChu]) VALUES (12, 9, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham],[soLuong], [donGia], [ghiChu]) VALUES (13, 10, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (14, 11, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (15, 12, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia],[ghiChu]) VALUES (16, 13, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong],[donGia], [ghiChu]) VALUES (17, 14, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT[dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong],[donGia], [ghiChu]) VALUES (18, 15, 1, 1, CAST(15000.00 AS Decimal(18, 2)), N'a')
INSERT[dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong],[donGia], [ghiChu]) VALUES (19, 18, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham],[soLuong], [donGia], [ghiChu]) VALUES (20, 18, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon],[idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (21, 18, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon],[idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (22, 19, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon],[idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (23, 20, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (24, 21, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (25, 22, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia],[ghiChu]) VALUES (26, 23, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong],[donGia], [ghiChu]) VALUES (27, 24, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT[dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong],[donGia], [ghiChu]) VALUES (28, 25, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham],[soLuong], [donGia], [ghiChu]) VALUES (29, 26, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon],[idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (30, 27, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon],[idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (31, 28, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon],[idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (32, 29, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (33, 30, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (34, 31, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong], [donGia],[ghiChu]) VALUES (35, 32, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong],[donGia], [ghiChu]) VALUES (36, 33, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT[dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham], [soLuong],[donGia], [ghiChu]) VALUES (37, 34, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon], [idSanPham],[soLuong], [donGia], [ghiChu]) VALUES (38, 35, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
INSERT [dbo].[ChiTietHoaDon] ([idChiTietHoaDon], [idHoaDon],[idSanPham], [soLuong], [donGia], [ghiChu]) VALUES (39, 36, 1, 1, CAST(15000.00 AS Decimal(18, 2)), NULL)
SET IDENTITY_INSERT [dbo].[ChiTietHoaDon] OFF
GO

INSERT[dbo].[ChiTietKiemKho] ([idPhieuKiemKho], [idNguyenLieu],[TonKhoHeThong], [TonKhoThucTe], [LyDoChenhLech]) VALUES (1, 2, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'')
INSERT [dbo].[ChiTietKiemKho] ([idPhieuKiemKho], [idNguyenLieu], [TonKhoHeThong],[TonKhoThucTe], [LyDoChenhLech]) VALUES (1, 3, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'')
INSERT [dbo].[ChiTietKiemKho] ([idPhieuKiemKho], [idNguyenLieu],[TonKhoHeThong], [TonKhoThucTe], [LyDoChenhLech]) VALUES (1, 4, CAST(5.00 AS Decimal(18, 2)), CAST(4.00 AS Decimal(18, 2)), N'đổ')
INSERT [dbo].[ChiTietKiemKho] ([idPhieuKiemKho], [idNguyenLieu], [TonKhoHeThong],[TonKhoThucTe], [LyDoChenhLech]) VALUES (1, 5, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'')
INSERT [dbo].[ChiTietKiemKho] ([idPhieuKiemKho], [idNguyenLieu], [TonKhoHeThong], [TonKhoThucTe],[LyDoChenhLech]) VALUES (1, 6, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'')
INSERT[dbo].[ChiTietKiemKho] ([idPhieuKiemKho], [idNguyenLieu],[TonKhoHeThong], [TonKhoThucTe], [LyDoChenhLech]) VALUES (1, 7, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'')
GO

INSERT [dbo].[ChiTietNhapKho] ([idPhieuNhapKho], [idNguyenLieu], [soLuongNhap], [donGiaNhap]) VALUES (1, 4, CAST(10.00 AS Decimal(18, 2)), CAST(25000.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietNhapKho] ([idPhieuNhapKho], [idNguyenLieu], [soLuongNhap],[donGiaNhap]) VALUES (2, 6, CAST(5.00 AS Decimal(18, 2)), CAST(25000.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietNhapKho] ([idPhieuNhapKho], [idNguyenLieu],[soLuongNhap], [donGiaNhap]) VALUES (3, 3, CAST(15.00 AS Decimal(18, 2)), CAST(100000.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietNhapKho] ([idPhieuNhapKho],[idNguyenLieu], [soLuongNhap], [donGiaNhap]) VALUES (4, 2, CAST(15.00 AS Decimal(18, 2)), CAST(85000.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietNhapKho] ([idPhieuNhapKho], [idNguyenLieu], [soLuongNhap], [donGiaNhap]) VALUES (5, 5, CAST(600.00 AS Decimal(18, 2)), CAST(1000.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietNhapKho] ([idPhieuNhapKho], [idNguyenLieu], [soLuongNhap],[donGiaNhap]) VALUES (8, 5, CAST(100.00 AS Decimal(18, 2)), CAST(1000.00 AS Decimal(18, 2)))
GO

INSERT[dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach],[ngayHenTra], [ngayTraThucTe], [tienCoc], [TienPhatTraTre], [DoMoiKhiThue], [GhiChuKhiThue]) VALUES (1, 2, CAST(N'2026-04-30T00:00:00.000' AS DateTime), CAST(N'2026-04-19T12:38:08.797' AS DateTime), CAST(350000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach],[ngayHenTra], [ngayTraThucTe], [tienCoc],[TienPhatTraTre], [DoMoiKhiThue], [GhiChuKhiThue]) VALUES (2, 1, CAST(N'2026-04-26T00:00:00.000' AS DateTime), CAST(N'2026-04-19T13:52:29.340' AS DateTime), CAST(100000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach], [ngayHenTra], [ngayTraThucTe], [tienCoc], [TienPhatTraTre], [DoMoiKhiThue], [GhiChuKhiThue]) VALUES (3, 4, CAST(N'2026-04-26T00:00:00.000' AS DateTime), CAST(N'2026-04-19T13:37:49.157' AS DateTime), CAST(300000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach], [ngayHenTra],[ngayTraThucTe], [tienCoc], [TienPhatTraTre], [DoMoiKhiThue],[GhiChuKhiThue]) VALUES (4, 3, CAST(N'2026-04-26T00:00:00.000' AS DateTime), CAST(N'2026-04-19T13:37:45.560' AS DateTime), CAST(99000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach],[ngayHenTra], [ngayTraThucTe], [tienCoc], [TienPhatTraTre],[DoMoiKhiThue], [GhiChuKhiThue]) VALUES (5, 4, CAST(N'2026-04-22T00:00:00.000' AS DateTime), CAST(N'2026-04-19T13:36:10.190' AS DateTime), CAST(300000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach],[idSach], [ngayHenTra], [ngayTraThucTe], [tienCoc],[TienPhatTraTre], [DoMoiKhiThue], [GhiChuKhiThue]) VALUES (6, 1, CAST(N'2026-04-26T00:00:00.000' AS DateTime), CAST(N'2026-04-19T13:37:41.600' AS DateTime), CAST(100000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach], [ngayHenTra], [ngayTraThucTe],[tienCoc], [TienPhatTraTre], [DoMoiKhiThue],[GhiChuKhiThue]) VALUES (7, 1, CAST(N'2026-04-26T00:00:00.000' AS DateTime), CAST(N'2026-04-19T13:37:37.163' AS DateTime), CAST(100000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach], [ngayHenTra],[ngayTraThucTe], [tienCoc], [TienPhatTraTre],[DoMoiKhiThue], [GhiChuKhiThue]) VALUES (8, 1, CAST(N'2026-04-26T00:00:00.000' AS DateTime), CAST(N'2026-04-19T13:47:58.930' AS DateTime), CAST(100000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach],[ngayHenTra], [ngayTraThucTe], [tienCoc],[TienPhatTraTre], [DoMoiKhiThue], [GhiChuKhiThue]) VALUES (9, 3, CAST(N'2026-04-26T00:00:00.000' AS DateTime), CAST(N'2026-04-19T13:50:08.187' AS DateTime), CAST(99000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach], [ngayHenTra], [ngayTraThucTe], [tienCoc],[TienPhatTraTre], [DoMoiKhiThue], [GhiChuKhiThue]) VALUES (10, 3, CAST(N'2026-04-20T00:00:00.000' AS DateTime), CAST(N'2026-04-19T13:52:06.747' AS DateTime), CAST(99000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach], [ngayHenTra],[ngayTraThucTe], [tienCoc], [TienPhatTraTre], [DoMoiKhiThue],[GhiChuKhiThue]) VALUES (11, 1, CAST(N'2026-04-26T00:00:00.000' AS DateTime), CAST(N'2026-04-19T14:14:35.150' AS DateTime), CAST(100000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach],[ngayHenTra], [ngayTraThucTe], [tienCoc], [TienPhatTraTre],[DoMoiKhiThue], [GhiChuKhiThue]) VALUES (12, 2, CAST(N'2026-04-26T00:00:00.000' AS DateTime), CAST(N'2026-04-19T14:14:27.633' AS DateTime), CAST(350000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach], [ngayHenTra], [ngayTraThucTe], [tienCoc],[TienPhatTraTre], [DoMoiKhiThue], [GhiChuKhiThue]) VALUES (13, 3, CAST(N'2026-04-26T00:00:00.000' AS DateTime), NULL, CAST(99000.00 AS Decimal(18, 2)), NULL, NULL, NULL)
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach], [ngayHenTra],[ngayTraThucTe], [tienCoc], [TienPhatTraTre],[DoMoiKhiThue], [GhiChuKhiThue]) VALUES (14, 1, CAST(N'2026-04-20T00:00:00.000' AS DateTime), NULL, CAST(100000.00 AS Decimal(18, 2)), NULL, NULL, NULL)
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach],[idSach], [ngayHenTra], [ngayTraThucTe], [tienCoc],[TienPhatTraTre], [DoMoiKhiThue], [GhiChuKhiThue]) VALUES (15, 1, CAST(N'2026-05-10T00:00:00.000' AS DateTime), CAST(N'2026-04-23T04:17:58.947' AS DateTime), CAST(100000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), 100, N'ok')
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach], [ngayHenTra],[ngayTraThucTe], [tienCoc], [TienPhatTraTre], [DoMoiKhiThue], [GhiChuKhiThue]) VALUES (16, 1, CAST(N'2026-05-23T00:00:00.000' AS DateTime), CAST(N'2026-04-23T04:33:48.650' AS DateTime), CAST(100000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), 100, N'20tyjhdfgdh')
INSERT [dbo].[ChiTietPhieuThue] ([idPhieuThueSach], [idSach], [ngayHenTra], [ngayTraThucTe],[tienCoc], [TienPhatTraTre], [DoMoiKhiThue],[GhiChuKhiThue]) VALUES (17, 3, CAST(N'2026-05-14T00:00:00.000' AS DateTime), CAST(N'2026-04-23T04:41:40.743' AS DateTime), CAST(99000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), 99, N'sdfsrf')
GO

INSERT [dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach],[TienPhat], [TinhTrangKhiTra], [TienPhatHuHong],[DoMoiKhiTra], [GhiChuKhiTra]) VALUES (1, 2, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL)
INSERT [dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach], [TienPhat],[TinhTrangKhiTra], [TienPhatHuHong], [DoMoiKhiTra], [GhiChuKhiTra]) VALUES (2, 4, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL)
INSERT [dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach],[TienPhat], [TinhTrangKhiTra], [TienPhatHuHong], [DoMoiKhiTra], [GhiChuKhiTra]) VALUES (3, 1, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL)
INSERT [dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach], [TienPhat], [TinhTrangKhiTra],[TienPhatHuHong], [DoMoiKhiTra], [GhiChuKhiTra]) VALUES (4, 1, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL)
INSERT[dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach], [TienPhat],[TinhTrangKhiTra], [TienPhatHuHong], [DoMoiKhiTra], [GhiChuKhiTra]) VALUES (5, 3, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL)
INSERT [dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach],[TienPhat], [TinhTrangKhiTra], [TienPhatHuHong], [DoMoiKhiTra], [GhiChuKhiTra]) VALUES (6, 4, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL)
INSERT [dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach], [TienPhat], [TinhTrangKhiTra],[TienPhatHuHong], [DoMoiKhiTra], [GhiChuKhiTra]) VALUES (7, 1, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL)
INSERT[dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach], [TienPhat],[TinhTrangKhiTra], [TienPhatHuHong], [DoMoiKhiTra], [GhiChuKhiTra]) VALUES (8, 3, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL)
INSERT [dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach],[TienPhat], [TinhTrangKhiTra], [TienPhatHuHong], [DoMoiKhiTra], [GhiChuKhiTra]) VALUES (9, 3, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL)
INSERT [dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach], [TienPhat], [TinhTrangKhiTra],[TienPhatHuHong], [DoMoiKhiTra], [GhiChuKhiTra]) VALUES (10, 1, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL)
INSERT [dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach],[TienPhat], [TinhTrangKhiTra], [TienPhatHuHong], [DoMoiKhiTra], [GhiChuKhiTra]) VALUES (11, 2, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL)
INSERT [dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach], [TienPhat],[TinhTrangKhiTra], [TienPhatHuHong], [DoMoiKhiTra], [GhiChuKhiTra]) VALUES (12, 1, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL)
INSERT [dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach], [TienPhat], [TinhTrangKhiTra],[TienPhatHuHong], [DoMoiKhiTra], [GhiChuKhiTra]) VALUES (13, 1, CAST(0.00 AS Decimal(18, 2)), NULL, CAST(0.00 AS Decimal(18, 2)), 100, NULL)
INSERT [dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach], [TienPhat], [TinhTrangKhiTra],[TienPhatHuHong], [DoMoiKhiTra], [GhiChuKhiTra]) VALUES (14, 1, CAST(0.00 AS Decimal(18, 2)), NULL, CAST(4000.00 AS Decimal(18, 2)), 98, N'xước bìa ')
INSERT [dbo].[ChiTietPhieuTra] ([IdPhieuTra], [IdSach],[TienPhat], [TinhTrangKhiTra], [TienPhatHuHong],[DoMoiKhiTra], [GhiChuKhiTra]) VALUES (15, 3, CAST(0.00 AS Decimal(18, 2)), NULL, CAST(8000.00 AS Decimal(18, 2)), 95, N'ửetfseasfdafs')
GO

INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon], [idPhuThu], [SoTien]) VALUES (1, 2, CAST(1500.00 AS Decimal(18, 2)))
INSERT[dbo].[ChiTietPhuThuHoaDon] ([idHoaDon], [idPhuThu],[SoTien]) VALUES (2, 2, CAST(2000.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon],[idPhuThu], [SoTien]) VALUES (3, 2, CAST(3500.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon], [idPhuThu], [SoTien]) VALUES (4, 2, CAST(13500.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon], [idPhuThu], [SoTien]) VALUES (6, 2, CAST(1500.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon],[idPhuThu], [SoTien]) VALUES (7, 2, CAST(750.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon], [idPhuThu], [SoTien]) VALUES (8, 2, CAST(750.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon], [idPhuThu], [SoTien]) VALUES (9, 2, CAST(750.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon], [idPhuThu],[SoTien]) VALUES (10, 1, CAST(1000.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon],[idPhuThu], [SoTien]) VALUES (10, 2, CAST(750.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon], [idPhuThu], [SoTien]) VALUES (11, 2, CAST(750.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon], [idPhuThu], [SoTien]) VALUES (12, 1, CAST(1000.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon],[idPhuThu], [SoTien]) VALUES (12, 2, CAST(750.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon], [idPhuThu], [SoTien]) VALUES (13, 1, CAST(1000.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon], [idPhuThu], [SoTien]) VALUES (13, 2, CAST(750.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon], [idPhuThu], [SoTien]) VALUES (14, 1, CAST(1000.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon], [idPhuThu], [SoTien]) VALUES (14, 2, CAST(750.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietPhuThuHoaDon] ([idHoaDon], [idPhuThu], [SoTien]) VALUES (15, 2, CAST(750.00 AS Decimal(18, 2)))
GO

INSERT [dbo].[ChiTietXuatHuy] ([idPhieuXuatHuy],[idNguyenLieu], [SoLuong], [DonGiaVon]) VALUES (1, 4, CAST(5.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
INSERT [dbo].[ChiTietXuatHuy] ([idPhieuXuatHuy], [idNguyenLieu], [SoLuong], [DonGiaVon]) VALUES (2, 6, CAST(1.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO

SET IDENTITY_INSERT [dbo].[DanhMuc] ON 
INSERT [dbo].[DanhMuc] ([idDanhMuc], [tenDanhMuc], [idDanhMucCha]) VALUES (1, N'Cafe', NULL)
INSERT [dbo].[DanhMuc] ([idDanhMuc], [tenDanhMuc], [idDanhMucCha]) VALUES (2, N'trà', NULL)
INSERT [dbo].[DanhMuc] ([idDanhMuc], [tenDanhMuc], [idDanhMucCha]) VALUES (3, N'bánh', NULL)
SET IDENTITY_INSERT [dbo].[DanhMuc] OFF
GO

INSERT [dbo].[DinhLuong] ([idSanPham], [idNguyenLieu], [SoLuongSuDung],[idDonViSuDung]) VALUES (1, 2, CAST(20.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[DinhLuong] ([idSanPham], [idNguyenLieu],[SoLuongSuDung], [idDonViSuDung]) VALUES (1, 6, CAST(10.00 AS Decimal(18, 2)), 6)
INSERT [dbo].[DinhLuong] ([idSanPham], [idNguyenLieu], [SoLuongSuDung],[idDonViSuDung]) VALUES (2, 4, CAST(200.00 AS Decimal(18, 2)), 6)
INSERT [dbo].[DinhLuong] ([idSanPham], [idNguyenLieu],[SoLuongSuDung], [idDonViSuDung]) VALUES (2, 5, CAST(1.00 AS Decimal(18, 2)), 4)
GO

SET IDENTITY_INSERT[dbo].[DonViChuyenDoi] ON 
INSERT [dbo].[DonViChuyenDoi] ([idChuyenDoi], [idNguyenLieu], [TenDonVi],[GiaTriQuyDoi], [LaDonViCoBan]) VALUES (1, 2, N'gram', CAST(1000.000000 AS Decimal(18, 6)), 0)
INSERT[dbo].[DonViChuyenDoi] ([idChuyenDoi], [idNguyenLieu], [TenDonVi],[GiaTriQuyDoi], [LaDonViCoBan]) VALUES (2, 2, N'kg', CAST(1.000000 AS Decimal(18, 6)), 1)
INSERT[dbo].[DonViChuyenDoi] ([idChuyenDoi], [idNguyenLieu], [TenDonVi],[GiaTriQuyDoi], [LaDonViCoBan]) VALUES (3, 3, N'kg', CAST(1.000000 AS Decimal(18, 6)), 1)
INSERT[dbo].[DonViChuyenDoi] ([idChuyenDoi], [idNguyenLieu], [TenDonVi],[GiaTriQuyDoi], [LaDonViCoBan]) VALUES (4, 5, N'túi', CAST(1.000000 AS Decimal(18, 6)), 1)
INSERT [dbo].[DonViChuyenDoi] ([idChuyenDoi], [idNguyenLieu],[TenDonVi], [GiaTriQuyDoi], [LaDonViCoBan]) VALUES (5, 4, N'lít', CAST(1.000000 AS Decimal(18, 6)), 1)
INSERT [dbo].[DonViChuyenDoi] ([idChuyenDoi], [idNguyenLieu], [TenDonVi], [GiaTriQuyDoi],[LaDonViCoBan]) VALUES (6, 4, N'ml', CAST(1000.000000 AS Decimal(18, 6)), 0)
INSERT [dbo].[DonViChuyenDoi] ([idChuyenDoi], [idNguyenLieu], [TenDonVi],[GiaTriQuyDoi], [LaDonViCoBan]) VALUES (7, 6, N'hộp', CAST(1.000000 AS Decimal(18, 6)), 1)
INSERT [dbo].[DonViChuyenDoi] ([idChuyenDoi],[idNguyenLieu], [TenDonVi], [GiaTriQuyDoi], [LaDonViCoBan]) VALUES (8, 7, N'kg', CAST(1.000000 AS Decimal(18, 6)), 1)
INSERT [dbo].[DonViChuyenDoi] ([idChuyenDoi], [idNguyenLieu], [TenDonVi], [GiaTriQuyDoi],[LaDonViCoBan]) VALUES (9, 7, N'gram', CAST(1000.000000 AS Decimal(18, 6)), 0)
INSERT [dbo].[DonViChuyenDoi] ([idChuyenDoi], [idNguyenLieu], [TenDonVi],[GiaTriQuyDoi], [LaDonViCoBan]) VALUES (10, 6, N'ml', CAST(1000.000000 AS Decimal(18, 6)), 0)
SET IDENTITY_INSERT [dbo].[DonViChuyenDoi] OFF
GO

SET IDENTITY_INSERT [dbo].[DonXinNghi] ON 
INSERT [dbo].[DonXinNghi] ([idDonXinNghi], [idNhanVien], [LoaiDon], [LyDo],[NgayBatDau], [NgayKetThuc], [TrangThai],[idNguoiDuyet], [NgayDuyet], [GhiChuPheDuyet]) VALUES (2, 3, N'Nghỉ có phép', N'sá', CAST(N'2026-04-09T00:16:39.237' AS DateTime), CAST(N'2026-04-09T00:16:39.240' AS DateTime), N'Đã duyệt', 1, CAST(N'2026-04-09T00:32:13.093' AS DateTime), N'a')
INSERT [dbo].[DonXinNghi] ([idDonXinNghi], [idNhanVien],[LoaiDon], [LyDo], [NgayBatDau], [NgayKetThuc],[TrangThai], [idNguoiDuyet], [NgayDuyet],[GhiChuPheDuyet]) VALUES (3, 2, N'Nghỉ ốm', N'ốm', CAST(N'2026-04-20T00:00:00.000' AS DateTime), CAST(N'2026-04-26T00:00:00.000' AS DateTime), N'Đã duyệt', 1, CAST(N'2026-04-19T19:50:27.167' AS DateTime), N'ok')
SET IDENTITY_INSERT [dbo].[DonXinNghi] OFF
GO

SET IDENTITY_INSERT [dbo].[GiaoDichThanhToan] ON 
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai], [CongThanhToan], [SoTien],[ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (1, 2, N'HD_2_150111', N'Tiền mặt', CAST(9000.00 AS Decimal(18, 2)), CAST(N'2026-04-18T15:01:11.670' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai], [CongThanhToan], [SoTien], [ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (2, 1, N'HD_1_164326', N'Tiền mặt', CAST(15000.00 AS Decimal(18, 2)), CAST(N'2026-04-18T16:43:26.077' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon],[MaGiaoDichNgoai], [CongThanhToan], [SoTien], [ThoiGianGiaoDich],[TrangThai], [MaLoi], [MoTaLoi]) VALUES (3, 3, N'HD_3_164457', N'Tiền mặt', CAST(35000.00 AS Decimal(18, 2)), CAST(N'2026-04-18T16:44:57.183' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai], [CongThanhToan], [SoTien],[ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (4, 4, N'HD_4_172055', N'Tiền mặt', CAST(133500.00 AS Decimal(18, 2)), CAST(N'2026-04-18T17:20:55.537' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai],[CongThanhToan], [SoTien], [ThoiGianGiaoDich], [TrangThai],[MaLoi], [MoTaLoi]) VALUES (5, 6, N'HD_6_182133', N'Tiền mặt', CAST(28500.00 AS Decimal(18, 2)), CAST(N'2026-04-18T18:21:33.570' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai], [CongThanhToan], [SoTien],[ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (6, 7, N'HD_7_193623', N'Ví điện tử', CAST(14250.00 AS Decimal(18, 2)), CAST(N'2026-04-18T19:36:22.923' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai],[CongThanhToan], [SoTien], [ThoiGianGiaoDich], [TrangThai],[MaLoi], [MoTaLoi]) VALUES (7, 8, N'HD_8_040903', N'Ví điện tử', CAST(14250.00 AS Decimal(18, 2)), CAST(N'2026-04-19T04:09:03.137' AS DateTime), N'Thành công', NULL, NULL)
INSERT[dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon],[MaGiaoDichNgoai], [CongThanhToan], [SoTien],[ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (8, 9, N'HD_9_053741', N'Chuyển khoản', CAST(14250.00 AS Decimal(18, 2)), CAST(N'2026-04-19T05:37:40.957' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon],[MaGiaoDichNgoai], [CongThanhToan], [SoTien], [ThoiGianGiaoDich],[TrangThai], [MaLoi], [MoTaLoi]) VALUES (9, 10, N'HD_10_055956', N'Chuyển khoản', CAST(8250.00 AS Decimal(18, 2)), CAST(N'2026-04-19T05:59:56.477' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich],[idHoaDon], [MaGiaoDichNgoai], [CongThanhToan], [SoTien],[ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (10, 11, N'HD_11_061716', N'Chuyển khoản', CAST(14250.00 AS Decimal(18, 2)), CAST(N'2026-04-19T06:17:16.257' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon],[MaGiaoDichNgoai], [CongThanhToan], [SoTien],[ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (11, 12, N'HD_12_064841', N'Ví điện tử', CAST(15250.00 AS Decimal(18, 2)), CAST(N'2026-04-19T06:48:41.633' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai], [CongThanhToan], [SoTien], [ThoiGianGiaoDich],[TrangThai], [MaLoi], [MoTaLoi]) VALUES (12, 13, N'HD_13_065635', N'Chuyển khoản', CAST(15250.00 AS Decimal(18, 2)), CAST(N'2026-04-19T06:56:35.083' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai],[CongThanhToan], [SoTien], [ThoiGianGiaoDich], [TrangThai],[MaLoi], [MoTaLoi]) VALUES (13, 14, N'HD_14_162953', N'Chuyển khoản', CAST(15250.00 AS Decimal(18, 2)), CAST(N'2026-04-19T16:29:52.950' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon],[MaGiaoDichNgoai], [CongThanhToan], [SoTien],[ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (14, 15, N'HD_15_024431', N'Chuyển khoản', CAST(7250.00 AS Decimal(18, 2)), CAST(N'2026-04-20T02:44:29.737' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon],[MaGiaoDichNgoai], [CongThanhToan], [SoTien], [ThoiGianGiaoDich],[TrangThai], [MaLoi], [MoTaLoi]) VALUES (15, 19, N'WEB_19_639125655652030602', N'COD', CAST(11750.00 AS Decimal(18, 2)), CAST(N'2026-04-23T18:26:05.203' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon],[MaGiaoDichNgoai], [CongThanhToan], [SoTien],[ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (16, 20, N'WEB_20_639125656870452678', N'VNPAY', CAST(11750.00 AS Decimal(18, 2)), CAST(N'2026-04-23T18:28:07.047' AS DateTime), N'Đang xử lý', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich],[idHoaDon], [MaGiaoDichNgoai], [CongThanhToan], [SoTien], [ThoiGianGiaoDich], [TrangThai], [MaLoi],[MoTaLoi]) VALUES (17, 21, N'WEB_21_639125663656377807', N'VNPAY', CAST(11750.00 AS Decimal(18, 2)), CAST(N'2026-04-23T18:39:25.640' AS DateTime), N'Đang xử lý', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai],[CongThanhToan], [SoTien], [ThoiGianGiaoDich], [TrangThai],[MaLoi], [MoTaLoi]) VALUES (18, 22, N'WEB_22_639125673099484793', N'VNPAY', CAST(18500.00 AS Decimal(18, 2)), CAST(N'2026-04-23T18:55:09.950' AS DateTime), N'Đang xử lý', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon],[MaGiaoDichNgoai], [CongThanhToan], [SoTien], [ThoiGianGiaoDich],[TrangThai], [MaLoi], [MoTaLoi]) VALUES (19, 23, N'WEB_23_639125676915662969', N'VNPAY', CAST(11750.00 AS Decimal(18, 2)), CAST(N'2026-04-23T19:01:31.567' AS DateTime), N'Thành công', NULL, NULL)
INSERT[dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon],[MaGiaoDichNgoai], [CongThanhToan], [SoTien],[ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (20, 24, N'WEB_24_639125678413741972', N'VNPAY', CAST(11750.00 AS Decimal(18, 2)), CAST(N'2026-04-23T19:04:01.373' AS DateTime), N'Thất bại', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich],[idHoaDon], [MaGiaoDichNgoai], [CongThanhToan],[SoTien], [ThoiGianGiaoDich], [TrangThai], [MaLoi],[MoTaLoi]) VALUES (21, 25, N'WEB_25_639125695196244737', N'VNPAY', CAST(11750.00 AS Decimal(18, 2)), CAST(N'2026-04-23T19:31:59.623' AS DateTime), N'Đã hủy', N'24', N'Khách hàng hủy giao dịch thanh toán.')
INSERT[dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon],[MaGiaoDichNgoai], [CongThanhToan], [SoTien],[ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (22, 26, N'WEB_26_639125698553290791', N'VNPAY', CAST(20000.00 AS Decimal(18, 2)), CAST(N'2026-04-23T19:37:35.330' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich],[idHoaDon], [MaGiaoDichNgoai], [CongThanhToan],[SoTien], [ThoiGianGiaoDich], [TrangThai], [MaLoi],[MoTaLoi]) VALUES (23, 27, N'WEB_27_639125710537328324', N'VNPAY', CAST(18500.00 AS Decimal(18, 2)), CAST(N'2026-04-23T19:57:33.733' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai],[CongThanhToan], [SoTien], [ThoiGianGiaoDich], [TrangThai],[MaLoi], [MoTaLoi]) VALUES (24, 28, N'WEB_28_639125730331088329', N'VNPAY', CAST(20000.00 AS Decimal(18, 2)), CAST(N'2026-04-23T20:30:33.110' AS DateTime), N'Đang xử lý', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon],[MaGiaoDichNgoai], [CongThanhToan], [SoTien], [ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (25, 29, N'WEB_29_639125739628483382', N'VNPAY', CAST(20000.00 AS Decimal(18, 2)), CAST(N'2026-04-23T20:46:02.850' AS DateTime), N'Đang xử lý', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai], [CongThanhToan], [SoTien],[ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (26, 30, N'WEB_30_639125742294133105', N'VNPAY', CAST(20000.00 AS Decimal(18, 2)), CAST(N'2026-04-23T20:50:29.413' AS DateTime), N'Đang xử lý', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai], [CongThanhToan], [SoTien],[ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (27, 31, N'WEB_31_639125742849577583', N'VNPAY', CAST(20000.00 AS Decimal(18, 2)), CAST(N'2026-04-23T20:51:24.957' AS DateTime), N'Đã hủy', N'24', N'Khách hàng hủy giao dịch thanh toán.')
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai], [CongThanhToan],[SoTien], [ThoiGianGiaoDich], [TrangThai], [MaLoi],[MoTaLoi]) VALUES (28, 32, N'WEB_32_639125744027846507', N'COD', CAST(20000.00 AS Decimal(18, 2)), CAST(N'2026-04-23T20:53:22.783' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai],[CongThanhToan], [SoTien], [ThoiGianGiaoDich], [TrangThai],[MaLoi], [MoTaLoi]) VALUES (29, 33, N'WEB_33_639125744343882792', N'COD', CAST(20000.00 AS Decimal(18, 2)), CAST(N'2026-04-23T20:53:54.387' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon], [MaGiaoDichNgoai],[CongThanhToan], [SoTien], [ThoiGianGiaoDich],[TrangThai], [MaLoi], [MoTaLoi]) VALUES (30, 34, N'WEB_34_639125744706306381', N'VNPAY', CAST(20000.00 AS Decimal(18, 2)), CAST(N'2026-04-23T20:54:30.630' AS DateTime), N'Thành công', NULL, NULL)
INSERT [dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon],[MaGiaoDichNgoai], [CongThanhToan], [SoTien],[ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (31, 35, N'WEB_35_639125748878506059', N'COD', CAST(20000.00 AS Decimal(18, 2)), CAST(N'2026-04-23T21:01:27.850' AS DateTime), N'Thành công', NULL, NULL)
INSERT[dbo].[GiaoDichThanhToan] ([idGiaoDich], [idHoaDon],[MaGiaoDichNgoai], [CongThanhToan], [SoTien],[ThoiGianGiaoDich], [TrangThai], [MaLoi], [MoTaLoi]) VALUES (32, 36, N'WEB_36_639125751918584202', N'COD', CAST(18500.00 AS Decimal(18, 2)), CAST(N'2026-04-23T21:06:31.860' AS DateTime), N'Thành công', NULL, NULL)
SET IDENTITY_INSERT [dbo].[GiaoDichThanhToan] OFF
GO

SET IDENTITY_INSERT [dbo].[GopY] ON 
INSERT [dbo].[GopY] ([IdGopY], [HoTen], [Email], [NoiDung], [NgayTao], [TrangThai]) VALUES (1, N'Lâm Chu Bảo Toàn', N'lamchubaotoan@gmail.com', N'mong thêm nhiền món mới', CAST(N'2026-04-22T00:21:35.013' AS DateTime), N'Chưa đọc')
INSERT [dbo].[GopY] ([IdGopY], [HoTen], [Email],[NoiDung], [NgayTao], [TrangThai]) VALUES (2, N'Lâm Chu Bảo Toàn', N'lamchubaotoan@gmail.com', N'thêm sách mới đi', CAST(N'2026-04-22T00:40:29.187' AS DateTime), N'Chưa đọc')
SET IDENTITY_INSERT [dbo].[GopY] OFF
GO

SET IDENTITY_INSERT [dbo].[HoaDon] ON 
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien], [idKhachHang],[thoiGianTao], [thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia],[TongPhuThu], [phuongThucThanhToan], [ghiChu],[LoaiHoaDon], [TrangThaiGiaoHang], [DiaChiGiaoHang],[SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (1, 3, 2, NULL, CAST(N'2026-04-18T11:29:05.163' AS DateTime), CAST(N'2026-04-18T16:43:26.077' AS DateTime), N'Đã thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(1500.00 AS Decimal(18, 2)), CAST(1500.00 AS Decimal(18, 2)), N'Tiền mặt', NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien],[idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai],[tongTienGoc], [giamGia], [TongPhuThu],[phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang],[DiaChiGiaoHang], [SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (2, 1, 2, 1, CAST(N'2026-04-18T14:42:09.627' AS DateTime), CAST(N'2026-04-18T15:01:11.670' AS DateTime), N'Đã thanh toán', CAST(20000.00 AS Decimal(18, 2)), CAST(13000.00 AS Decimal(18, 2)), CAST(2000.00 AS Decimal(18, 2)), N'Tiền mặt', NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien], [idKhachHang],[thoiGianTao], [thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia], [TongPhuThu], [phuongThucThanhToan],[ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang], [DiaChiGiaoHang], [SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (3, NULL, 2, 1, CAST(N'2026-04-18T16:44:04.003' AS DateTime), CAST(N'2026-04-18T16:44:57.183' AS DateTime), N'Đã thanh toán', CAST(35000.00 AS Decimal(18, 2)), CAST(3500.00 AS Decimal(18, 2)), CAST(3500.00 AS Decimal(18, 2)), N'Tiền mặt', NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan],[idNhanVien], [idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia], [TongPhuThu], [phuongThucThanhToan], [ghiChu], [LoaiHoaDon],[TrangThaiGiaoHang], [DiaChiGiaoHang], [SoDienThoaiGiaoHang],[idNguoiGiaoHang]) VALUES (4, 3, 2, 1, CAST(N'2026-04-18T16:56:21.197' AS DateTime), CAST(N'2026-04-18T17:20:55.537' AS DateTime), N'Đã thanh toán', CAST(135000.00 AS Decimal(18, 2)), CAST(15000.00 AS Decimal(18, 2)), CAST(13500.00 AS Decimal(18, 2)), N'Tiền mặt', NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan],[idNhanVien], [idKhachHang], [thoiGianTao], [thoiGianThanhToan],[trangThai], [tongTienGoc], [giamGia], [TongPhuThu],[phuongThucThanhToan], [ghiChu], [LoaiHoaDon],[TrangThaiGiaoHang], [DiaChiGiaoHang], [SoDienThoaiGiaoHang],[idNguoiGiaoHang]) VALUES (5, 3, 2, NULL, CAST(N'2026-04-18T17:21:22.163' AS DateTime), NULL, N'Đã hủy', CAST(15000.00 AS Decimal(18, 2)), CAST(1500.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien], [idKhachHang],[thoiGianTao], [thoiGianThanhToan], [trangThai],[tongTienGoc], [giamGia], [TongPhuThu], [phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang],[DiaChiGiaoHang], [SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (6, 3, 2, 1, CAST(N'2026-04-18T17:22:49.323' AS DateTime), CAST(N'2026-04-18T18:21:33.570' AS DateTime), N'Đã thanh toán', CAST(30000.00 AS Decimal(18, 2)), CAST(3000.00 AS Decimal(18, 2)), CAST(1500.00 AS Decimal(18, 2)), N'Tiền mặt', NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien], [idKhachHang], [thoiGianTao],[thoiGianThanhToan], [trangThai], [tongTienGoc],[giamGia], [TongPhuThu], [phuongThucThanhToan], [ghiChu],[LoaiHoaDon], [TrangThaiGiaoHang], [DiaChiGiaoHang],[SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (7, 3, 2, 1, CAST(N'2026-04-18T19:35:36.317' AS DateTime), CAST(N'2026-04-18T19:36:22.923' AS DateTime), N'Đã thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(1500.00 AS Decimal(18, 2)), CAST(750.00 AS Decimal(18, 2)), N'Ví điện tử', NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan],[idNhanVien], [idKhachHang], [thoiGianTao], [thoiGianThanhToan],[trangThai], [tongTienGoc], [giamGia], [TongPhuThu],[phuongThucThanhToan], [ghiChu], [LoaiHoaDon],[TrangThaiGiaoHang], [DiaChiGiaoHang], [SoDienThoaiGiaoHang],[idNguoiGiaoHang]) VALUES (8, 1, 2, 1, CAST(N'2026-04-19T04:08:21.450' AS DateTime), CAST(N'2026-04-19T04:09:03.137' AS DateTime), N'Đã thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(1500.00 AS Decimal(18, 2)), CAST(750.00 AS Decimal(18, 2)), N'Ví điện tử', NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien], [idKhachHang], [thoiGianTao],[thoiGianThanhToan], [trangThai], [tongTienGoc],[giamGia], [TongPhuThu], [phuongThucThanhToan], [ghiChu],[LoaiHoaDon], [TrangThaiGiaoHang], [DiaChiGiaoHang],[SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (9, 1, 2, NULL, CAST(N'2026-04-19T05:34:10.950' AS DateTime), CAST(N'2026-04-19T05:37:40.957' AS DateTime), N'Đã thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(1500.00 AS Decimal(18, 2)), CAST(750.00 AS Decimal(18, 2)), N'Chuyển khoản', NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan],[idNhanVien], [idKhachHang], [thoiGianTao],[thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia],[TongPhuThu], [phuongThucThanhToan], [ghiChu], [LoaiHoaDon],[TrangThaiGiaoHang], [DiaChiGiaoHang],[SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (10, 1, 2, 1, CAST(N'2026-04-19T05:50:13.713' AS DateTime), CAST(N'2026-04-19T05:59:56.477' AS DateTime), N'Đã thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(8500.00 AS Decimal(18, 2)), CAST(1750.00 AS Decimal(18, 2)), N'Chuyển khoản', NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien],[idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai],[tongTienGoc], [giamGia], [TongPhuThu],[phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang],[DiaChiGiaoHang], [SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (11, 1, 2, 1, CAST(N'2026-04-19T06:16:18.813' AS DateTime), CAST(N'2026-04-19T06:17:16.257' AS DateTime), N'Đã thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(1500.00 AS Decimal(18, 2)), CAST(750.00 AS Decimal(18, 2)), N'Chuyển khoản', NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien], [idKhachHang],[thoiGianTao], [thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia], [TongPhuThu], [phuongThucThanhToan],[ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang], [DiaChiGiaoHang], [SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (12, NULL, 2, NULL, CAST(N'2026-04-19T06:47:33.297' AS DateTime), CAST(N'2026-04-19T06:48:41.633' AS DateTime), N'Đã thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(1500.00 AS Decimal(18, 2)), CAST(1750.00 AS Decimal(18, 2)), N'Ví điện tử', NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan],[idNhanVien], [idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia], [TongPhuThu], [phuongThucThanhToan], [ghiChu], [LoaiHoaDon],[TrangThaiGiaoHang], [DiaChiGiaoHang], [SoDienThoaiGiaoHang],[idNguoiGiaoHang]) VALUES (13, NULL, 2, 1, CAST(N'2026-04-19T06:56:01.040' AS DateTime), CAST(N'2026-04-19T06:56:35.083' AS DateTime), N'Đã thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(1500.00 AS Decimal(18, 2)), CAST(1750.00 AS Decimal(18, 2)), N'Chuyển khoản', NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien],[idKhachHang], [thoiGianTao], [thoiGianThanhToan],[trangThai], [tongTienGoc], [giamGia], [TongPhuThu],[phuongThucThanhToan], [ghiChu], [LoaiHoaDon],[TrangThaiGiaoHang], [DiaChiGiaoHang], [SoDienThoaiGiaoHang],[idNguoiGiaoHang]) VALUES (14, 1, 2, 1, CAST(N'2026-04-19T16:27:19.787' AS DateTime), CAST(N'2026-04-19T16:29:52.950' AS DateTime), N'Đã thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(1500.00 AS Decimal(18, 2)), CAST(1750.00 AS Decimal(18, 2)), N'Chuyển khoản', NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT[dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien],[idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai],[tongTienGoc], [giamGia], [TongPhuThu],[phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang],[DiaChiGiaoHang], [SoDienThoaiGiaoHang],[idNguoiGiaoHang]) VALUES (15, 3, 2, 1, CAST(N'2026-04-20T02:43:33.847' AS DateTime), CAST(N'2026-04-20T02:44:29.737' AS DateTime), N'Đã thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(8500.00 AS Decimal(18, 2)), CAST(750.00 AS Decimal(18, 2)), N'Chuyển khoản', NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien], [idKhachHang],[thoiGianTao], [thoiGianThanhToan], [trangThai],[tongTienGoc], [giamGia], [TongPhuThu], [phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang],[DiaChiGiaoHang], [SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (18, 1, 2, NULL, CAST(N'2026-04-20T14:20:14.377' AS DateTime), NULL, N'Chưa thanh toán', CAST(30000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, N'Tại quán', NULL, NULL, NULL, NULL)
INSERT[dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien],[idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai],[tongTienGoc], [giamGia], [TongPhuThu],[phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang],[DiaChiGiaoHang], [SoDienThoaiGiaoHang],[idNguoiGiaoHang]) VALUES (19, NULL, NULL, 1, CAST(N'2026-04-23T18:26:04.410' AS DateTime), NULL, N'Chờ xác nhận', CAST(15000.00 AS Decimal(18, 2)), CAST(8250.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'COD', N'ádas', N'Giao hàng', N'Chờ xác nhận', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan],[idNhanVien], [idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia], [TongPhuThu], [phuongThucThanhToan], [ghiChu], [LoaiHoaDon],[TrangThaiGiaoHang], [DiaChiGiaoHang], [SoDienThoaiGiaoHang],[idNguoiGiaoHang]) VALUES (20, NULL, NULL, 1, CAST(N'2026-04-23T18:28:06.990' AS DateTime), NULL, N'Chờ thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(8250.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'VNPAY', NULL, N'Giao hàng', N'Chờ xác nhận', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien], [idKhachHang],[thoiGianTao], [thoiGianThanhToan], [trangThai],[tongTienGoc], [giamGia], [TongPhuThu], [phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang],[DiaChiGiaoHang], [SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (21, NULL, NULL, 1, CAST(N'2026-04-23T18:39:25.130' AS DateTime), NULL, N'Chờ thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(8250.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'VNPAY', NULL, N'Giao hàng', N'Chờ xác nhận', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien],[idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia], [TongPhuThu],[phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang], [DiaChiGiaoHang], [SoDienThoaiGiaoHang],[idNguoiGiaoHang]) VALUES (22, NULL, NULL, 1, CAST(N'2026-04-23T18:55:09.917' AS DateTime), NULL, N'Chờ thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(1500.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'VNPAY', N'FSDFFSD', N'Giao hàng', N'Chờ xác nhận', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien], [idKhachHang],[thoiGianTao], [thoiGianThanhToan], [trangThai],[tongTienGoc], [giamGia], [TongPhuThu], [phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang],[DiaChiGiaoHang], [SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (23, NULL, NULL, 1, CAST(N'2026-04-23T19:01:30.843' AS DateTime), NULL, N'Chờ xác nhận', CAST(15000.00 AS Decimal(18, 2)), CAST(8250.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'VNPAY', NULL, N'Giao hàng', N'Chờ xác nhận', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien],[idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia], [TongPhuThu],[phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang], [DiaChiGiaoHang], [SoDienThoaiGiaoHang],[idNguoiGiaoHang]) VALUES (24, NULL, NULL, 1, CAST(N'2026-04-23T19:04:01.320' AS DateTime), NULL, N'Đã hủy (Lỗi thanh toán)', CAST(15000.00 AS Decimal(18, 2)), CAST(8250.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'VNPAY', N'DSSADASD', N'Giao hàng', N'Chờ xác nhận', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien],[idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai],[tongTienGoc], [giamGia], [TongPhuThu],[phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang],[DiaChiGiaoHang], [SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (25, NULL, NULL, 1, CAST(N'2026-04-23T19:31:58.807' AS DateTime), NULL, N'Đã hủy', CAST(15000.00 AS Decimal(18, 2)), CAST(8250.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'VNPAY', NULL, N'Giao hàng', N'Đã hủy', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan],[idNhanVien], [idKhachHang], [thoiGianTao],[thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia],[TongPhuThu], [phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang], [DiaChiGiaoHang],[SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (26, NULL, NULL, 1, CAST(N'2026-04-23T19:37:35.300' AS DateTime), CAST(N'2026-04-23T19:38:06.180' AS DateTime), N'Đã thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'VNPAY', NULL, N'Giao hàng', N'Chờ xác nhận', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT[dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien],[idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai],[tongTienGoc], [giamGia], [TongPhuThu],[phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang],[DiaChiGiaoHang], [SoDienThoaiGiaoHang],[idNguoiGiaoHang]) VALUES (27, NULL, NULL, 1, CAST(N'2026-04-23T19:57:33.283' AS DateTime), CAST(N'2026-04-23T19:58:23.627' AS DateTime), N'Đã thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(1500.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'VNPAY', NULL, N'Giao hàng', N'Chờ xác nhận', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien], [idKhachHang],[thoiGianTao], [thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia], [TongPhuThu], [phuongThucThanhToan],[ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang], [DiaChiGiaoHang], [SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (28, NULL, NULL, 1, CAST(N'2026-04-23T20:30:32.650' AS DateTime), NULL, N'Chờ thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'VNPAY', NULL, N'Giao hàng', N'Chờ thanh toán', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan],[idNhanVien], [idKhachHang], [thoiGianTao], [thoiGianThanhToan],[trangThai], [tongTienGoc], [giamGia], [TongPhuThu],[phuongThucThanhToan], [ghiChu], [LoaiHoaDon],[TrangThaiGiaoHang], [DiaChiGiaoHang], [SoDienThoaiGiaoHang],[idNguoiGiaoHang]) VALUES (29, NULL, NULL, 1, CAST(N'2026-04-23T20:46:02.417' AS DateTime), NULL, N'Chờ thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'VNPAY', NULL, N'Giao hàng', N'Chờ thanh toán', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien], [idKhachHang], [thoiGianTao],[thoiGianThanhToan], [trangThai], [tongTienGoc],[giamGia], [TongPhuThu], [phuongThucThanhToan], [ghiChu],[LoaiHoaDon], [TrangThaiGiaoHang], [DiaChiGiaoHang],[SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (30, NULL, NULL, 1, CAST(N'2026-04-23T20:50:28.540' AS DateTime), NULL, N'Chờ thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'VNPAY', NULL, N'Giao hàng', N'Chờ thanh toán', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT[dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien],[idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai],[tongTienGoc], [giamGia], [TongPhuThu],[phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang],[DiaChiGiaoHang], [SoDienThoaiGiaoHang],[idNguoiGiaoHang]) VALUES (31, NULL, NULL, 1, CAST(N'2026-04-23T20:51:24.150' AS DateTime), NULL, N'Đã hủy', CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'VNPAY', NULL, N'Giao hàng', N'Đã hủy', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan],[idNhanVien], [idKhachHang], [thoiGianTao],[thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia],[TongPhuThu], [phuongThucThanhToan], [ghiChu], [LoaiHoaDon],[TrangThaiGiaoHang], [DiaChiGiaoHang],[SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (32, NULL, NULL, 1, CAST(N'2026-04-23T20:53:22.753' AS DateTime), NULL, N'Đã hủy', CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'COD', NULL, N'Giao hàng', N'Đã hủy', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien], [idKhachHang], [thoiGianTao],[thoiGianThanhToan], [trangThai], [tongTienGoc],[giamGia], [TongPhuThu], [phuongThucThanhToan], [ghiChu],[LoaiHoaDon], [TrangThaiGiaoHang], [DiaChiGiaoHang],[SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (33, NULL, NULL, 1, CAST(N'2026-04-23T20:53:54.357' AS DateTime), NULL, N'Chờ thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'COD', NULL, N'Giao hàng', N'Chờ xác nhận', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien],[idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai],[tongTienGoc], [giamGia], [TongPhuThu],[phuongThucThanhToan], [ghiChu], [LoaiHoaDon], [TrangThaiGiaoHang],[DiaChiGiaoHang], [SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (34, NULL, NULL, 1, CAST(N'2026-04-23T20:54:30.597' AS DateTime), CAST(N'2026-04-23T20:55:41.620' AS DateTime), N'Đã thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'VNPAY', NULL, N'Giao hàng', N'Chờ xác nhận', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan],[idNhanVien], [idKhachHang], [thoiGianTao], [thoiGianThanhToan], [trangThai], [tongTienGoc], [giamGia], [TongPhuThu], [phuongThucThanhToan], [ghiChu], [LoaiHoaDon],[TrangThaiGiaoHang], [DiaChiGiaoHang], [SoDienThoaiGiaoHang],[idNguoiGiaoHang]) VALUES (35, NULL, NULL, 1, CAST(N'2026-04-23T21:01:27.067' AS DateTime), NULL, N'Chờ thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'COD', NULL, N'Giao hàng', N'Chờ xác nhận', N'08 Hà Văn Tín', N'0376512695', NULL)
INSERT [dbo].[HoaDon] ([idHoaDon], [idBan], [idNhanVien], [idKhachHang], [thoiGianTao],[thoiGianThanhToan], [trangThai], [tongTienGoc],[giamGia], [TongPhuThu], [phuongThucThanhToan], [ghiChu],[LoaiHoaDon], [TrangThaiGiaoHang], [DiaChiGiaoHang],[SoDienThoaiGiaoHang], [idNguoiGiaoHang]) VALUES (36, NULL, NULL, 1, CAST(N'2026-04-23T21:06:30.923' AS DateTime), NULL, N'Chờ thanh toán', CAST(15000.00 AS Decimal(18, 2)), CAST(1500.00 AS Decimal(18, 2)), CAST(5000.00 AS Decimal(18, 2)), N'COD', NULL, N'Giao hàng', N'Chờ xác nhận', N'08 Hà Văn Tín', N'0376512695', NULL)
SET IDENTITY_INSERT [dbo].[HoaDon] OFF
GO

INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon], [idKhuyenMai]) VALUES (1, 1)
INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon], [idKhuyenMai]) VALUES (2, 1)
INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon], [idKhuyenMai]) VALUES (3, 1)
INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon], [idKhuyenMai]) VALUES (4, 2)
INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon],[idKhuyenMai]) VALUES (5, 1)
INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon], [idKhuyenMai]) VALUES (6, 1)
INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon], [idKhuyenMai]) VALUES (7, 1)
INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon], [idKhuyenMai]) VALUES (8, 1)
INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon],[idKhuyenMai]) VALUES (9, 1)
INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon], [idKhuyenMai]) VALUES (10, 1)
INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon], [idKhuyenMai]) VALUES (11, 1)
INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon], [idKhuyenMai]) VALUES (12, 1)
INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon], [idKhuyenMai]) VALUES (13, 1)
INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon], [idKhuyenMai]) VALUES (14, 1)
INSERT [dbo].[HoaDon_KhuyenMai] ([idHoaDon], [idKhuyenMai]) VALUES (15, 1)
GO

SET IDENTITY_INSERT [dbo].[KhachHang] ON 
INSERT [dbo].[KhachHang] ([idKhachHang], [hoTen], [soDienThoai], [email],[diaChi], [diemTichLuy], [tenDangNhap], [matKhau],[ngayTao], [BiKhoa], [AnhDaiDien], [taiKhoanTam], [DaXoa], [lyDoKhoa], [thoiGianMoKhoa]) VALUES (1, N'Lâm Chu Bảo Toàn', N'0376512695', N'lamchubaotoan@gmail.com', N'08 Hà Văn Tín', 69, N'sdrgsragsera', N'031124', CAST(N'2026-04-22T04:40:26.917' AS DateTime), 0, N'/images/avatars/avatarKH/1_lam-chu-bao-toan.jpg', 0, 0, NULL, NULL)
INSERT [dbo].[KhachHang] ([idKhachHang], [hoTen], [soDienThoai], [email],[diaChi], [diemTichLuy], [tenDangNhap], [matKhau],[ngayTao], [BiKhoa], [AnhDaiDien], [taiKhoanTam], [DaXoa], [lyDoKhoa], [thoiGianMoKhoa]) VALUES (2, N'Lâm Toàn', N'0812847885', N'hachorchogao@gmail.com', NULL, 5, N'0812847885', N'031124', CAST(N'2026-04-22T02:55:59.120' AS DateTime), 0, NULL, 1, 0, NULL, NULL)
INSERT [dbo].[KhachHang] ([idKhachHang], [hoTen], [soDienThoai], [email],[diaChi], [diemTichLuy], [tenDangNhap], [matKhau],[ngayTao], [BiKhoa], [AnhDaiDien], [taiKhoanTam], [DaXoa], [lyDoKhoa], [thoiGianMoKhoa]) VALUES (5, N'lâm', N'0344288579', N'nho.macarong@gmail.com', NULL, 15, N'0344288579', N'031124', CAST(N'2026-04-22T05:05:54.620' AS DateTime), 0, NULL, 0, 0, NULL, NULL)
INSERT [dbo].[KhachHang] ([idKhachHang], [hoTen],[soDienThoai], [email], [diaChi], [diemTichLuy],[tenDangNhap], [matKhau], [ngayTao], [BiKhoa], [AnhDaiDien],[taiKhoanTam], [DaXoa], [lyDoKhoa], [thoiGianMoKhoa]) VALUES (6, N'shu.tmusic0311@gmail.com', N'012345660', N'shu.tmusic0311@gmail.com', NULL, 0, N'shu.tmusic0311@gmail.com', N'031124', CAST(N'2026-04-22T03:30:37.663' AS DateTime), 0, NULL, 0, 0, NULL, NULL)
INSERT [dbo].[KhachHang] ([idKhachHang], [hoTen],[soDienThoai], [email], [diaChi], [diemTichLuy], [tenDangNhap], [matKhau], [ngayTao], [BiKhoa], [AnhDaiDien],[taiKhoanTam], [DaXoa], [lyDoKhoa], [thoiGianMoKhoa]) VALUES (7, N'shu.mihoyo.shu0311@gmail.com', N'0367858762', N'shu.mihoyo.shu0311@gmail.com', NULL, 0, N'shu.mihoyo.shu0311@gmail.com', N'031124', CAST(N'2026-04-22T03:05:32.740' AS DateTime), 0, NULL, 0, 1, NULL, NULL)
INSERT [dbo].[KhachHang] ([idKhachHang], [hoTen],[soDienThoai], [email], [diaChi], [diemTichLuy],[tenDangNhap], [matKhau], [ngayTao], [BiKhoa], [AnhDaiDien],[taiKhoanTam], [DaXoa], [lyDoKhoa], [thoiGianMoKhoa]) VALUES (9, N'yt.shu0311@gmail.com', N'0976423831', N'yt.shu0311@gmail.com', NULL, 0, N'yt.shu0311@gmail.com', N'031124', CAST(N'2026-04-22T03:44:52.673' AS DateTime), 0, NULL, 0, 0, NULL, NULL)
INSERT [dbo].[KhachHang] ([idKhachHang], [hoTen], [soDienThoai], [email],[diaChi], [diemTichLuy], [tenDangNhap], [matKhau],[ngayTao], [BiKhoa], [AnhDaiDien], [taiKhoanTam], [DaXoa], [lyDoKhoa], [thoiGianMoKhoa]) VALUES (10, N'lamcbaotoan@gmail.com', N'0977423831', N'lamcbaotoan@gmail.com', NULL, 0, N'lamcbaotoan@gmail.com', N'031124', CAST(N'2026-04-22T04:42:05.543' AS DateTime), 0, NULL, 0, 0, NULL, NULL)
SET IDENTITY_INSERT [dbo].[KhachHang] OFF
GO

SET IDENTITY_INSERT [dbo].[KhuVuc] ON 
INSERT [dbo].[KhuVuc] ([idKhuVuc], [TenKhuVuc], [MoTa]) VALUES (1, N'Tầng 1', N'Tầng 1 trước sảnh')
INSERT [dbo].[KhuVuc] ([idKhuVuc], [TenKhuVuc], [MoTa]) VALUES (2, N'Tầng 2', N'Tầng 2 phía trên tầng 1')
SET IDENTITY_INSERT [dbo].[KhuVuc] OFF
GO

SET IDENTITY_INSERT [dbo].[KhuyenMai] ON 
INSERT [dbo].[KhuyenMai] ([idKhuyenMai], [maKhuyenMai], [tenChuongTrinh],[moTa], [loaiGiamGia], [giaTriGiam], [ngayBatDau],[ngayKetThuc], [dieuKienApDung], [soLuongConLai],[TrangThai], [GiamToiDa], [IdSanPhamApDung], [HoaDonToiThieu],[GioBatDau], [GioKetThuc], [NgayTrongTuan]) VALUES (1, N'CAFEDEN', N'Cà Phê Đen (Nóng/Đá)', N'Cà Phê Đen (Nóng/Đá)', N'PhanTram', CAST(10.00 AS Decimal(18, 2)), CAST(N'2026-04-15T00:00:00.000' AS DateTime), CAST(N'2026-07-01T00:00:00.000' AS DateTime), N'Cà Phê Đen (Nóng/Đá)', 994, N'Hoạt động', CAST(5000.00 AS Decimal(18, 2)), 1, NULL, CAST(N'00:00:00' AS Time), CAST(N'23:59:00' AS Time), NULL)
INSERT [dbo].[KhuyenMai] ([idKhuyenMai], [maKhuyenMai], [tenChuongTrinh], [moTa],[loaiGiamGia], [giaTriGiam], [ngayBatDau], [ngayKetThuc],[dieuKienApDung], [soLuongConLai], [TrangThai], [GiamToiDa], [IdSanPhamApDung], [HoaDonToiThieu], [GioBatDau],[GioKetThuc], [NgayTrongTuan]) VALUES (2, N'GIAM15', N'Giảm 15k cho đơn từ 120k', N'Giảm 15k cho đơn từ 120k', N'SoTien', CAST(15000.00 AS Decimal(18, 2)), CAST(N'2026-04-18T00:00:00.000' AS DateTime), CAST(N'2026-07-15T00:00:00.000' AS DateTime), N'Giảm 15k cho đơn từ 120k', 100, N'Hoạt động', CAST(15000.00 AS Decimal(18, 2)), NULL, CAST(120000.00 AS Decimal(18, 2)), CAST(N'00:00:00' AS Time), CAST(N'23:59:00' AS Time), NULL)
INSERT [dbo].[KhuyenMai] ([idKhuyenMai], [maKhuyenMai],[tenChuongTrinh], [moTa], [loaiGiamGia], [giaTriGiam],[ngayBatDau], [ngayKetThuc], [dieuKienApDung],[soLuongConLai], [TrangThai], [GiamToiDa], [IdSanPhamApDung],[HoaDonToiThieu], [GioBatDau], [GioKetThuc], [NgayTrongTuan]) VALUES (3, N'THU2VUIVE', N'THỨ 2 VUI VẺ', N'Giảm 20% cho đơn từ 50K', N'PhanTram', CAST(20.00 AS Decimal(18, 2)), CAST(N'2026-04-18T00:00:00.000' AS DateTime), CAST(N'2026-07-18T00:00:00.000' AS DateTime), N'Giảm 20% cho đơn từ 50K', 100, N'Hoạt động', CAST(20000.00 AS Decimal(18, 2)), NULL, CAST(50000.00 AS Decimal(18, 2)), CAST(N'00:00:00' AS Time), CAST(N'23:59:00' AS Time), N'2')
INSERT [dbo].[KhuyenMai] ([idKhuyenMai], [maKhuyenMai], [tenChuongTrinh], [moTa],[loaiGiamGia], [giaTriGiam], [ngayBatDau], [ngayKetThuc],[dieuKienApDung], [soLuongConLai], [TrangThai], [GiamToiDa], [IdSanPhamApDung], [HoaDonToiThieu], [GioBatDau],[GioKetThuc], [NgayTrongTuan]) VALUES (4, N'HAPPYHOUR', N'Giờ vàng (14h-16h) giảm 20%', N'Giờ vàng (14h-16h) giảm 20%', N'PhanTram', CAST(20.00 AS Decimal(18, 2)), CAST(N'2026-04-18T00:00:00.000' AS DateTime), CAST(N'2026-07-18T00:00:00.000' AS DateTime), N'Giảm 20% Giảm tối đa 70k cho đơn tối thiểu 0đ', 100, N'Hoạt động', CAST(70000.00 AS Decimal(18, 2)), NULL, NULL, CAST(N'14:00:00' AS Time), CAST(N'16:00:00' AS Time), NULL)
SET IDENTITY_INSERT[dbo].[KhuyenMai] OFF
GO

SET IDENTITY_INSERT [dbo].[LichLamViec] ON 
INSERT [dbo].[LichLamViec] ([idLichLamViec],[idNhanVien], [idCa], [ngayLam], [trangThai], [ghiChu]) VALUES (33, 2, 1, CAST(N'2026-04-13' AS Date), N'Đã duyệt', N'')
INSERT [dbo].[LichLamViec] ([idLichLamViec],[idNhanVien], [idCa], [ngayLam], [trangThai], [ghiChu]) VALUES (34, 2, 6, CAST(N'2026-04-19' AS Date), N'Đã duyệt', N'')
INSERT [dbo].[LichLamViec] ([idLichLamViec], [idNhanVien], [idCa], [ngayLam],[trangThai], [ghiChu]) VALUES (37, 2, 1, CAST(N'2026-04-20' AS Date), N'Đã duyệt', N'')
INSERT[dbo].[LichLamViec] ([idLichLamViec], [idNhanVien], [idCa], [ngayLam], [trangThai], [ghiChu]) VALUES (40, 2, 2, CAST(N'2026-04-20' AS Date), N'Đã duyệt', N'')
INSERT [dbo].[LichLamViec] ([idLichLamViec], [idNhanVien], [idCa], [ngayLam], [trangThai], [ghiChu]) VALUES (41, 2, 3, CAST(N'2026-04-20' AS Date), N'Đã duyệt', N'')
INSERT [dbo].[LichLamViec] ([idLichLamViec], [idNhanVien], [idCa], [ngayLam], [trangThai],[ghiChu]) VALUES (43, 2, 1, CAST(N'2026-04-21' AS Date), N'Đã duyệt', N'')
INSERT [dbo].[LichLamViec] ([idLichLamViec], [idNhanVien], [idCa],[ngayLam], [trangThai], [ghiChu]) VALUES (44, 2, 2, CAST(N'2026-04-21' AS Date), N'Đã duyệt', N'')
INSERT [dbo].[LichLamViec] ([idLichLamViec],[idNhanVien], [idCa], [ngayLam], [trangThai], [ghiChu]) VALUES (45, 2, 3, CAST(N'2026-04-21' AS Date), N'Đã duyệt', N'')
SET IDENTITY_INSERT [dbo].[LichLamViec] OFF
GO

SET IDENTITY_INSERT [dbo].[NguoiGiaoHang] ON 
INSERT [dbo].[NguoiGiaoHang] ([idNguoiGiaoHang], [TenNguoiGiaoHang],[SoDienThoai], [TrangThai]) VALUES (1, N'nội bộ', N'0376512695', N'Sẵn sàng')
SET IDENTITY_INSERT [dbo].[NguoiGiaoHang] OFF
GO

SET IDENTITY_INSERT [dbo].[NguyenLieu] ON 
INSERT[dbo].[NguyenLieu] ([idNguyenLieu], [tenNguyenLieu], [donViTinh],[tonKho], [TonKhoToiThieu]) VALUES (2, N'Hạt Cà Phê Arabica (Nhập)', N'kg', CAST(14.64 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)))
INSERT [dbo].[NguyenLieu] ([idNguyenLieu], [tenNguyenLieu], [donViTinh], [tonKho], [TonKhoToiThieu]) VALUES (3, N'Hạt Cà Phê Robusta (VN)', N'kg', CAST(15.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)))
INSERT [dbo].[NguyenLieu] ([idNguyenLieu], [tenNguyenLieu], [donViTinh], [tonKho], [TonKhoToiThieu]) VALUES (4, N'Sữa tươi thanh trùng', N'lít', CAST(9.20 AS Decimal(18, 2)), CAST(5.00 AS Decimal(18, 2)))
INSERT [dbo].[NguyenLieu] ([idNguyenLieu], [tenNguyenLieu], [donViTinh], [tonKho], [TonKhoToiThieu]) VALUES (5, N'Trà túi lọc Lipton', N'túi', CAST(100.00 AS Decimal(18, 2)), CAST(20.00 AS Decimal(18, 2)))
INSERT[dbo].[NguyenLieu] ([idNguyenLieu], [tenNguyenLieu], [donViTinh],[tonKho], [TonKhoToiThieu]) VALUES (6, N'Sữa Đặc Ông Thọ', N'hộp', CAST(3.82 AS Decimal(18, 2)), CAST(2.00 AS Decimal(18, 2)))
INSERT [dbo].[NguyenLieu] ([idNguyenLieu], [tenNguyenLieu], [donViTinh], [tonKho], [TonKhoToiThieu]) VALUES (7, N'Đường cát trắng', N'kg', CAST(0.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)))
SET IDENTITY_INSERT [dbo].[NguyenLieu] OFF
GO

SET IDENTITY_INSERT [dbo].[NhaCungCap] ON 
INSERT [dbo].[NhaCungCap] ([idNhaCungCap],[tenNhaCungCap], [soDienThoai], [diaChi], [email]) VALUES (1, N'NCC Cà Phê Trung Nguyên', N'0911111111', N'1 Hùng Vương', N'0911111111@test.tets')
SET IDENTITY_INSERT [dbo].[NhaCungCap] OFF
GO

SET IDENTITY_INSERT [dbo].[NhanVien] ON 
INSERT[dbo].[NhanVien] ([idNhanVien], [hoTen],[soDienThoai], [email], [diaChi], [ngayVaoLam], [idVaiTro],[luongCoBan], [trangThaiLamViec], [tenDangNhap], [matKhau],[AnhDaiDien]) VALUES (1, N'Lâm Chu Bảo Toàn', N'0901111111', N'quanly@cafebook.vn', N'08 hà văn tín', CAST(N'2026-04-07' AS Date), 1, CAST(60000.00 AS Decimal(18, 2)), N'Đang làm việc', N'quanly', N'123456', N'/images/avatars/avatarNV/20260412034104_lam-chu-bao-toan.jpg')
INSERT [dbo].[NhanVien] ([idNhanVien], [hoTen],[soDienThoai], [email], [diaChi], [ngayVaoLam], [idVaiTro], [luongCoBan], [trangThaiLamViec], [tenDangNhap], [matKhau], [AnhDaiDien]) VALUES (2, N'Trần Thị Nhân Viên', N'0902222222', N'nhanvien@cafebook.vn', N'08 havantin', CAST(N'2026-04-07' AS Date), 2, CAST(22000.00 AS Decimal(18, 2)), N'Đang làm việc', N'nhanvien', N'123456', N'/images/avatars/avatarNV/20260419212150_tran-thi-nhan-vien.jpg')
INSERT[dbo].[NhanVien] ([idNhanVien], [hoTen], [soDienThoai], [email],[diaChi], [ngayVaoLam], [idVaiTro], [luongCoBan],[trangThaiLamViec], [tenDangNhap], [matKhau], [AnhDaiDien]) VALUES (3, N'shushune', N'0376512695', N'lamchubaotoan@gmail.com', N'08 ha van tin', CAST(N'2026-04-08' AS Date), 1, CAST(50000.00 AS Decimal(18, 2)), N'Đang làm việc', N'lamcbaotoan', N'123456', N'/images/avatars/avatarNV/20260412201631_shushune.jpg')
INSERT [dbo].[NhanVien] ([idNhanVien], [hoTen], [soDienThoai], [email],[diaChi], [ngayVaoLam], [idVaiTro], [luongCoBan],[trangThaiLamViec], [tenDangNhap], [matKhau], [AnhDaiDien]) VALUES (4, N'lâm nhân viên', N'012345679', N'lamnhanvien@cafebook.vn', N'08 ha van tin', CAST(N'2026-04-09' AS Date), 2, CAST(22000.00 AS Decimal(18, 2)), N'Đang làm việc', N'lamnhanvien', N'123456', N'/images/avatars/avatarNV/20260412205521_lam-nhan-vien.jpg')
INSERT [dbo].[NhanVien] ([idNhanVien], [hoTen], [soDienThoai],[email], [diaChi], [ngayVaoLam], [idVaiTro], [luongCoBan],[trangThaiLamViec], [tenDangNhap], [matKhau], [AnhDaiDien]) VALUES (7, N'lâm bếp', N'123456789', N'lambep@test.tesy', N'80havantin', CAST(N'2026-04-09' AS Date), 2, CAST(22000.00 AS Decimal(18, 2)), N'Đang làm việc', N'lambep', N'123456', N'/images/avatars/avatarNV/20260412201613_lam-bep.jpg')
INSERT [dbo].[NhanVien] ([idNhanVien], [hoTen], [soDienThoai], [email], [diaChi],[ngayVaoLam], [idVaiTro], [luongCoBan], [trangThaiLamViec], [tenDangNhap], [matKhau], [AnhDaiDien]) VALUES (11, N'lâm phục vụ', N'0123456785', N'lamphucvu@cafebook.vn', N'08havantin', CAST(N'2026-04-09' AS Date), 2, CAST(220000.00 AS Decimal(18, 2)), N'Đang làm việc', N'lamphucvu', N'123456', N'/images/avatars/avatarNV/20260412201542_lam-phuc-vu.jpg')
SET IDENTITY_INSERT [dbo].[NhanVien] OFF
GO

INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (1, N'FULL_QL')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (2, N'CM_THONG_BAO')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (2, N'NV_CHAM_CONG')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (2, N'NV_CHE_BIEN')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (2, N'NV_DAT_BAN')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien],[IdQuyen]) VALUES (2, N'NV_GIAO_HANG')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (2, N'NV_GOI_MON')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (2, N'NV_LICH_LAM_VIEC')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien],[IdQuyen]) VALUES (2, N'NV_PHIEU_LUONG')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (2, N'NV_SO_DO_BAN')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (2, N'NV_THANH_TOAN')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (2, N'NV_THONG_TIN')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (2, N'NV_THUE_SACH')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (3, N'CM_CAI_DAT')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (3, N'CM_NHAT_KY_HE_THONG')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (3, N'CM_THONG_BAO')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (3, N'QL_BAO_CAO_TON_KHO_NL')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien],[IdQuyen]) VALUES (3, N'QL_BAO_CAO_TON_KHO_SACH')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (3, N'QL_KHU_VUC')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (3, N'QL_KHUYEN_MAI')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (3, N'QL_NGUYEN_LIEU')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (3, N'QL_PHAN_QUYEN')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (3, N'QL_PHAT_LUONG')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (3, N'QL_PHU_THU')
INSERT[dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (3, N'QL_SACH')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (3, N'QL_TON_KHO')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (4, N'FULL_NV')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (7, N'NV_CHAM_CONG')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien],[IdQuyen]) VALUES (7, N'NV_CHE_BIEN')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (7, N'NV_LICH_LAM_VIEC')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (7, N'NV_PHIEU_LUONG')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (7, N'NV_THONG_TIN')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (11, N'NV_CHAM_CONG')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (11, N'NV_GOI_MON')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (11, N'NV_LICH_LAM_VIEC')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (11, N'NV_PHIEU_LUONG')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien],[IdQuyen]) VALUES (11, N'NV_SO_DO_BAN')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (11, N'NV_THANH_TOAN')
INSERT [dbo].[NhanVien_Quyen] ([IdNhanVien], [IdQuyen]) VALUES (11, N'NV_THONG_TIN')
GO

SET IDENTITY_INSERT [dbo].[NhatKyHeThong] ON 
INSERT[dbo].[NhatKyHeThong] ([IdNhatKy], [IdNhanVien],[HanhDong], [BangBiAnhHuong], [KhoaChinh], [DuLieuCu],[DuLieuMoi], [ThoiGian], [DiaChiIP]) VALUES (1, 1, N'ĐĂNG NHẬP', N'Hệ Thống', NULL, NULL, N'Đăng nhập thành công vào WPF App qua: quanly@cafebook.vn', CAST(N'2026-04-08T12:43:34.007' AS DateTime), N'::1')
INSERT [dbo].[NhatKyHeThong] ([IdNhatKy],[IdNhanVien], [HanhDong], [BangBiAnhHuong], [KhoaChinh],[DuLieuCu], [DuLieuMoi], [ThoiGian], [DiaChiIP]) VALUES (2, 1, N'ĐĂNG NHẬP', N'Hệ Thống', NULL, NULL, N'Đăng nhập thành công vào WPF App qua: quanly@cafebook.vn', CAST(N'2026-04-08T12:54:00.410' AS DateTime), N'::1')
INSERT [dbo].[NhatKyHeThong] ([IdNhatKy], [IdNhanVien], [HanhDong], [BangBiAnhHuong], [KhoaChinh], [DuLieuCu], [DuLieuMoi], [ThoiGian], [DiaChiIP]) VALUES (3, 1, N'ĐĂNG NHẬP', N'Hệ Thống', NULL, NULL, N'Đăng nhập thành công vào WPF App qua: quanly@cafebook.vn', CAST(N'2026-04-08T13:36:54.687' AS DateTime), N'::1')
INSERT [dbo].[NhatKyHeThong] ([IdNhatKy],[IdNhanVien], [HanhDong], [BangBiAnhHuong], [KhoaChinh],[DuLieuCu], [DuLieuMoi], [ThoiGian], [DiaChiIP]) VALUES (4, 1, N'ĐĂNG NHẬP', N'Hệ Thống', NULL, NULL, N'Đăng nhập thành công vào WPF App qua: quanly@cafebook.vn', CAST(N'2026-04-08T13:57:18.740' AS DateTime), N'::1')
INSERT [dbo].[NhatKyHeThong] ([IdNhatKy],[IdNhanVien], [HanhDong], [BangBiAnhHuong], [KhoaChinh],[DuLieuCu], [DuLieuMoi], [ThoiGian], [DiaChiIP]) VALUES (5, 1, N'ĐĂNG NHẬP', N'Hệ Thống', NULL, NULL, N'Đăng nhập thành công vào WPF App qua: quanly@cafebook.vn', CAST(N'2026-04-08T14:15:39.247' AS DateTime), N'::1')
SET IDENTITY_INSERT [dbo].[NhatKyHeThong] OFF
GO

SET IDENTITY_INSERT [dbo].[NhaXuatBan] ON 
INSERT [dbo].[NhaXuatBan] ([idNhaXuatBan], [tenNhaXuatBan],[MoTa]) VALUES (1, N'NXB Trẻ', N'Nhà xuất bản Trẻ (thành lập 24/3/1981, trụ sở tại TP.HCM) là đơn vị uy tín hàng đầu Việt Nam, trực thuộc Đoàn TNCS Hồ Chí Minh TP.HCM, chuyên xuất bản sách văn học, kỹ năng, thiếu nhi và văn hóa. NXB Trẻ tiên phong tuân thủ Công ước Berne (2003), nổi tiếng với các tác phẩm của Nguyễn Nhật Ánh, Harry Potter và chủ động hợp tác quốc tế, lan tỏa văn hóa đọc.')
INSERT [dbo].[NhaXuatBan] ([idNhaXuatBan], [tenNhaXuatBan], [MoTa]) VALUES (2, N'NXB Tri Thức', N'Nhà xuất bản (NXB) Tri thức là một đơn vị xuất bản uy tín tại Việt Nam, được thành lập vào tháng 9 năm 2005, trực thuộc Liên hiệp các Hội Khoa học và Kỹ thuật Việt Nam (VUSTA). NXB được biết đến với sứ mệnh phổ biến tri thức khoa học, tinh hoa nhân loại và các tư tưởng lớn.')
INSERT [dbo].[NhaXuatBan] ([idNhaXuatBan], [tenNhaXuatBan], [MoTa]) VALUES (3, N'NXB Tổng hợp TP.HCM', N'Nhà xuất bản Tổng hợp Thành phố Hồ Chí Minh (NXB Tổng hợp TP.HCM) là một đơn vị xuất bản uy tín, có lịch sử lâu đời, đóng vai trò quan trọng trong đời sống văn hóa, chính trị của Thành phố. Dưới đây là các thông tin mô tả chi tiết:')
SET IDENTITY_INSERT [dbo].[NhaXuatBan] OFF
GO

SET IDENTITY_INSERT [dbo].[NhuCauCaLam] ON 
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (174, CAST(N'2026-04-12' AS Date), 1, 1, 1, N'Tất cả', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (175, CAST(N'2026-04-12' AS Date), 1, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau],[ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau],[ghiChu]) VALUES (176, CAST(N'2026-04-12' AS Date), 4, 2, 1, N'Tất cả', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (177, CAST(N'2026-04-12' AS Date), 2, 1, 1, N'Tất cả', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa],[idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (178, CAST(N'2026-04-12' AS Date), 2, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT[dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (179, CAST(N'2026-04-12' AS Date), 5, 2, 1, N'Tất cả', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (180, CAST(N'2026-04-12' AS Date), 3, 1, 1, N'Full-time', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (181, CAST(N'2026-04-12' AS Date), 3, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (182, CAST(N'2026-04-12' AS Date), 6, 2, 1, N'Part-time', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau],[ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau],[ghiChu]) VALUES (184, CAST(N'2026-04-06' AS Date), 1, 1, 1, N'Tất cả', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (185, CAST(N'2026-04-06' AS Date), 1, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (186, CAST(N'2026-04-06' AS Date), 4, 2, 1, N'Tất cả', N'phục vụ')
INSERT[dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa],[idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (187, CAST(N'2026-04-06' AS Date), 2, 1, 1, N'Tất cả', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (188, CAST(N'2026-04-06' AS Date), 2, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa],[idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (189, CAST(N'2026-04-06' AS Date), 5, 2, 1, N'Tất cả', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (190, CAST(N'2026-04-06' AS Date), 3, 1, 1, N'Full-time', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (191, CAST(N'2026-04-06' AS Date), 3, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (192, CAST(N'2026-04-06' AS Date), 6, 2, 1, N'Part-time', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (193, CAST(N'2026-04-07' AS Date), 1, 1, 1, N'Tất cả', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau],[ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau],[ghiChu]) VALUES (194, CAST(N'2026-04-07' AS Date), 1, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau],[ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau],[ghiChu]) VALUES (195, CAST(N'2026-04-07' AS Date), 4, 2, 1, N'Tất cả', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (196, CAST(N'2026-04-07' AS Date), 2, 1, 1, N'Tất cả', N'quản lý')
INSERT[dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa],[idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (197, CAST(N'2026-04-07' AS Date), 2, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT[dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa],[idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (198, CAST(N'2026-04-07' AS Date), 5, 2, 1, N'Tất cả', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (199, CAST(N'2026-04-07' AS Date), 3, 1, 1, N'Full-time', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (200, CAST(N'2026-04-07' AS Date), 3, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (201, CAST(N'2026-04-07' AS Date), 6, 2, 1, N'Part-time', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (202, CAST(N'2026-04-08' AS Date), 1, 1, 1, N'Tất cả', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau],[ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (203, CAST(N'2026-04-08' AS Date), 1, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (204, CAST(N'2026-04-08' AS Date), 4, 2, 1, N'Tất cả', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau],[ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau],[ghiChu]) VALUES (205, CAST(N'2026-04-08' AS Date), 2, 1, 1, N'Tất cả', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (206, CAST(N'2026-04-08' AS Date), 2, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (207, CAST(N'2026-04-08' AS Date), 5, 2, 1, N'Tất cả', N'phục vụ')
INSERT[dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa],[idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (208, CAST(N'2026-04-08' AS Date), 3, 1, 1, N'Full-time', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (209, CAST(N'2026-04-08' AS Date), 3, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (210, CAST(N'2026-04-08' AS Date), 6, 2, 1, N'Part-time', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (211, CAST(N'2026-04-09' AS Date), 1, 1, 1, N'Tất cả', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (212, CAST(N'2026-04-09' AS Date), 1, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (213, CAST(N'2026-04-09' AS Date), 4, 2, 1, N'Tất cả', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (214, CAST(N'2026-04-09' AS Date), 2, 1, 1, N'Tất cả', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa],[idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (215, CAST(N'2026-04-09' AS Date), 2, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT[dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (216, CAST(N'2026-04-09' AS Date), 5, 2, 1, N'Tất cả', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (217, CAST(N'2026-04-09' AS Date), 3, 1, 1, N'Full-time', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (218, CAST(N'2026-04-09' AS Date), 3, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (219, CAST(N'2026-04-09' AS Date), 6, 2, 1, N'Part-time', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau],[ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau],[ghiChu]) VALUES (220, CAST(N'2026-04-10' AS Date), 1, 1, 1, N'Tất cả', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (221, CAST(N'2026-04-10' AS Date), 1, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (222, CAST(N'2026-04-10' AS Date), 4, 2, 1, N'Tất cả', N'phục vụ')
INSERT[dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa],[idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (223, CAST(N'2026-04-10' AS Date), 2, 1, 1, N'Tất cả', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (224, CAST(N'2026-04-10' AS Date), 2, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa],[idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (225, CAST(N'2026-04-10' AS Date), 5, 2, 1, N'Tất cả', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (226, CAST(N'2026-04-10' AS Date), 3, 1, 1, N'Full-time', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (227, CAST(N'2026-04-10' AS Date), 3, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (228, CAST(N'2026-04-10' AS Date), 6, 2, 1, N'Part-time', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (229, CAST(N'2026-04-11' AS Date), 1, 1, 1, N'Tất cả', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau],[ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau],[ghiChu]) VALUES (230, CAST(N'2026-04-11' AS Date), 1, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau],[ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau],[ghiChu]) VALUES (231, CAST(N'2026-04-11' AS Date), 4, 2, 1, N'Tất cả', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (232, CAST(N'2026-04-11' AS Date), 2, 1, 1, N'Tất cả', N'quản lý')
INSERT[dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa],[idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (233, CAST(N'2026-04-11' AS Date), 2, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT[dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (234, CAST(N'2026-04-11' AS Date), 5, 2, 1, N'Tất cả', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (235, CAST(N'2026-04-11' AS Date), 3, 1, 1, N'Full-time', N'quản lý')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (236, CAST(N'2026-04-11' AS Date), 3, 2, 3, N'Full-time', N'bếp, phục vụ, thu ngân')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (237, CAST(N'2026-04-11' AS Date), 6, 2, 1, N'Part-time', N'phục vụ')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau],[ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau],[ghiChu]) VALUES (238, CAST(N'2026-04-13' AS Date), 1, 2, 3, N'Tất cả', N'')
INSERT[dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa],[idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (239, CAST(N'2026-04-19' AS Date), 6, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (240, CAST(N'2026-04-20' AS Date), 1, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau],[ngayLam], [idCa], [idVaiTro], [soLuongCan], [loaiYeuCau],[ghiChu]) VALUES (248, CAST(N'2026-04-20' AS Date), 2, 2, 1, N'Tất cả', N'')
INSERT[dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (249, CAST(N'2026-04-20' AS Date), 3, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (250, CAST(N'2026-04-21' AS Date), 1, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (251, CAST(N'2026-04-21' AS Date), 2, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (252, CAST(N'2026-04-21' AS Date), 3, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (253, CAST(N'2026-04-22' AS Date), 1, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (254, CAST(N'2026-04-22' AS Date), 2, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (255, CAST(N'2026-04-22' AS Date), 3, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (256, CAST(N'2026-04-23' AS Date), 1, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (257, CAST(N'2026-04-23' AS Date), 2, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (258, CAST(N'2026-04-23' AS Date), 3, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (259, CAST(N'2026-04-24' AS Date), 1, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (260, CAST(N'2026-04-24' AS Date), 2, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (261, CAST(N'2026-04-24' AS Date), 3, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (262, CAST(N'2026-04-25' AS Date), 1, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (263, CAST(N'2026-04-25' AS Date), 2, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (264, CAST(N'2026-04-25' AS Date), 3, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro], [soLuongCan],[loaiYeuCau], [ghiChu]) VALUES (265, CAST(N'2026-04-26' AS Date), 1, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam],[idCa], [idVaiTro], [soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (266, CAST(N'2026-04-26' AS Date), 2, 2, 1, N'Tất cả', N'')
INSERT [dbo].[NhuCauCaLam] ([idNhuCau], [ngayLam], [idCa], [idVaiTro],[soLuongCan], [loaiYeuCau], [ghiChu]) VALUES (267, CAST(N'2026-04-26' AS Date), 3, 2, 1, N'Tất cả', N'')
SET IDENTITY_INSERT [dbo].[NhuCauCaLam] OFF
GO

SET IDENTITY_INSERT [dbo].[PhieuDatBan] ON 
INSERT [dbo].[PhieuDatBan] ([idPhieuDatBan], [idKhachHang], [idBan], [hoTenKhach], [sdtKhach], [thoiGianDat], [soLuongKhach], [trangThai],[ghiChu]) VALUES (1, 1, 1, N'Lâm Chu Bảo Toàn', N'0376512695', CAST(N'2026-04-18T21:00:00.000' AS DateTime), 1, N'Đã hủy', N'Tự động hủy do khách trễ 15p')
INSERT [dbo].[PhieuDatBan] ([idPhieuDatBan], [idKhachHang], [idBan],[hoTenKhach], [sdtKhach], [thoiGianDat], [soLuongKhach],[trangThai], [ghiChu]) VALUES (2, 2, 3, N'Lâm Toàn', N'0812847885', CAST(N'2026-04-18T21:30:00.000' AS DateTime), 1, N'Đã hủy', N'Tự động hủy do khách trễ 15p')
INSERT [dbo].[PhieuDatBan] ([idPhieuDatBan], [idKhachHang], [idBan], [hoTenKhach],[sdtKhach], [thoiGianDat], [soLuongKhach], [trangThai], [ghiChu]) VALUES (3, 5, 1, N'lâm', N'0344288579', CAST(N'2026-04-19T08:00:00.000' AS DateTime), 1, N'Đã hủy', N'Tự động hủy do khách trễ 15p')
INSERT [dbo].[PhieuDatBan] ([idPhieuDatBan], [idKhachHang], [idBan], [hoTenKhach],[sdtKhach], [thoiGianDat], [soLuongKhach], [trangThai], [ghiChu]) VALUES (4, 1, 1, N'Lâm Chu Bảo Toàn', N'0376512695', CAST(N'2026-04-19T10:59:00.000' AS DateTime), 1, N'Đã hủy', N'Tự động hủy do khách trễ 15p')
INSERT [dbo].[PhieuDatBan] ([idPhieuDatBan], [idKhachHang], [idBan], [hoTenKhach], [sdtKhach],[thoiGianDat], [soLuongKhach], [trangThai], [ghiChu]) VALUES (5, 1, 3, N'Lâm Chu Bảo Toàn', N'0376512695', CAST(N'2026-04-19T10:59:00.000' AS DateTime), 1, N'Đã hủy', N'Tự động hủy do khách trễ 15p')
INSERT [dbo].[PhieuDatBan] ([idPhieuDatBan], [idKhachHang], [idBan], [hoTenKhach],[sdtKhach], [thoiGianDat], [soLuongKhach], [trangThai], [ghiChu]) VALUES (6, 5, 1, N'lâm', N'0344288579', CAST(N'2026-04-19T13:59:00.000' AS DateTime), 1, N'Đã hủy', N'Tự động hủy do khách trễ 15p')
INSERT [dbo].[PhieuDatBan] ([idPhieuDatBan], [idKhachHang], [idBan], [hoTenKhach], [sdtKhach], [thoiGianDat], [soLuongKhach], [trangThai], [ghiChu]) VALUES (7, 1, 3, N'Lâm Chu Bảo Toàn', N'0376512695', CAST(N'2026-04-20T07:40:00.000' AS DateTime), 1, N'Đã hủy', N'a | Tự động hủy do trễ 15p')
INSERT [dbo].[PhieuDatBan] ([idPhieuDatBan], [idKhachHang], [idBan], [hoTenKhach], [sdtKhach],[thoiGianDat], [soLuongKhach], [trangThai], [ghiChu]) VALUES (8, 1, 1, N'Lâm Chu Bảo Toàn', N'0376512695', CAST(N'2026-04-22T07:30:00.000' AS DateTime), 2, N'Đã hủy', N'a | Tự động hủy do trễ 15p')
INSERT [dbo].[PhieuDatBan] ([idPhieuDatBan], [idKhachHang], [idBan], [hoTenKhach],[sdtKhach], [thoiGianDat], [soLuongKhach], [trangThai], [ghiChu]) VALUES (9, 1, 1, N'Lâm Chu Bảo Toàn', N'0376512695', CAST(N'2026-04-22T16:30:00.000' AS DateTime), 2, N'Đã hủy', N'Tự động hủy do khách trễ 15p')
INSERT [dbo].[PhieuDatBan] ([idPhieuDatBan], [idKhachHang], [idBan], [hoTenKhach], [sdtKhach],[thoiGianDat], [soLuongKhach], [trangThai], [ghiChu]) VALUES (10, 1, 1, N'Lâm Chu Bảo Toàn', N'0376512695', CAST(N'2026-04-24T12:00:00.000' AS DateTime), 5, N'Đã hủy', N'sai giờ')
INSERT [dbo].[PhieuDatBan] ([idPhieuDatBan], [idKhachHang], [idBan], [hoTenKhach], [sdtKhach], [thoiGianDat],[soLuongKhach], [trangThai], [ghiChu]) VALUES (11, 1, 1, N'Lâm Chu Bảo Toàn', N'0376512695', CAST(N'2026-04-23T07:00:00.000' AS DateTime), 2, N'Đã hủy', N'ẻtyhrdgreyert | dssedffsea')
INSERT [dbo].[PhieuDatBan] ([idPhieuDatBan], [idKhachHang],[idBan], [hoTenKhach], [sdtKhach], [thoiGianDat],[soLuongKhach], [trangThai], [ghiChu]) VALUES (12, 1, 1, N'Lâm Chu Bảo Toàn', N'0376512695', CAST(N'2026-04-23T07:00:00.000' AS DateTime), 2, N'Đã hủy', N'zdxfgkjlhzdxfbjklfsdk   tetwsstt')
INSERT [dbo].[PhieuDatBan] ([idPhieuDatBan], [idKhachHang], [idBan], [hoTenKhach], [sdtKhach], [thoiGianDat],[soLuongKhach], [trangThai], [ghiChu]) VALUES (13, 1, 1, N'Lâm Chu Bảo Toàn', N'0376512695', CAST(N'2026-04-23T07:00:00.000' AS DateTime), 2, N'Đã hủy', N'Hidhdudjdhhd')
SET IDENTITY_INSERT [dbo].[PhieuDatBan] OFF
GO

SET IDENTITY_INSERT [dbo].[PhieuKiemKho] ON 
INSERT [dbo].[PhieuKiemKho] ([idPhieuKiemKho], [idNhanVienKiem], [NgayKiem], [GhiChu],[TrangThai]) VALUES (1, 1, CAST(N'2026-04-12T00:06:04.970' AS DateTime), NULL, N'')
SET IDENTITY_INSERT [dbo].[PhieuKiemKho] OFF
GO

SET IDENTITY_INSERT [dbo].[PhieuNhapKho] ON 
INSERT [dbo].[PhieuNhapKho] ([idPhieuNhapKho],[idNhaCungCap], [idNhanVien], [ngayNhap], [tongTien],[ghiChu], [TrangThai], [HoaDonDinhKem]) VALUES (1, NULL, 1, CAST(N'2026-04-11T23:27:53.677' AS DateTime), CAST(235000.00 AS Decimal(18, 2)), N'', N'Hoàn thành', NULL)
INSERT [dbo].[PhieuNhapKho] ([idPhieuNhapKho], [idNhaCungCap], [idNhanVien], [ngayNhap],[tongTien], [ghiChu], [TrangThai], [HoaDonDinhKem]) VALUES (2, NULL, 1, CAST(N'2026-04-12T00:37:42.773' AS DateTime), CAST(125000.00 AS Decimal(18, 2)), N'tets', N'Hoàn thành', N'/images/BuildNhapKho/20260412003742_e9f0_phieunhapkho20260412000949.png')
INSERT [dbo].[PhieuNhapKho] ([idPhieuNhapKho], [idNhaCungCap],[idNhanVien], [ngayNhap], [tongTien], [ghiChu], [TrangThai],[HoaDonDinhKem]) VALUES (3, 1, 1, CAST(N'2026-04-12T01:28:25.920' AS DateTime), CAST(1500000.00 AS Decimal(18, 2)), N'test', N'Hoàn thành', N'/images/BuildNhapKho/20260412012825_b258_phieunhapkho20260412000949.png')
INSERT [dbo].[PhieuNhapKho] ([idPhieuNhapKho], [idNhaCungCap], [idNhanVien], [ngayNhap],[tongTien], [ghiChu], [TrangThai], [HoaDonDinhKem]) VALUES (4, 1, 1, CAST(N'2026-04-18T14:24:43.617' AS DateTime), CAST(1275000.00 AS Decimal(18, 2)), N'', N'Hoàn thành', N'/images/BuildNhapKho/20260418142443_d410_phieunhapkho20260412000949.png')
INSERT [dbo].[PhieuNhapKho] ([idPhieuNhapKho], [idNhaCungCap],[idNhanVien], [ngayNhap], [tongTien], [ghiChu], [TrangThai],[HoaDonDinhKem]) VALUES (5, 1, 1, CAST(N'2026-04-18T14:25:53.780' AS DateTime), CAST(600000.00 AS Decimal(18, 2)), N'', N'Hoàn thành', NULL)
INSERT [dbo].[PhieuNhapKho] ([idPhieuNhapKho],[idNhaCungCap], [idNhanVien], [ngayNhap], [tongTien],[ghiChu], [TrangThai], [HoaDonDinhKem]) VALUES (8, NULL, 1, CAST(N'2026-04-20T16:50:41.573' AS DateTime), CAST(100000.00 AS Decimal(18, 2)), N'', N'Hoàn thành', N'/images/BuildNhapKho/20260420165041_5ef8_phieunhapkho20260412000949.png')
SET IDENTITY_INSERT [dbo].[PhieuNhapKho] OFF
GO

SET IDENTITY_INSERT [dbo].[PhieuThueSach] ON 
INSERT[dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang], [idNhanVien],[ngayThue], [trangThai], [tongTienCoc], [DiaChiGiaoHang],[PhuongThucThanhToan]) VALUES (1, 1, 2, CAST(N'2026-04-19T07:33:31.010' AS DateTime), N'Đã Trả', CAST(350000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang], [idNhanVien], [ngayThue],[trangThai], [tongTienCoc], [DiaChiGiaoHang],[PhuongThucThanhToan]) VALUES (2, 5, 2, CAST(N'2026-04-19T07:34:54.680' AS DateTime), N'Đã Trả', CAST(100000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach],[idKhachHang], [idNhanVien], [ngayThue], [trangThai],[tongTienCoc], [DiaChiGiaoHang], [PhuongThucThanhToan]) VALUES (3, 2, 2, CAST(N'2026-04-19T08:02:27.283' AS DateTime), N'Đã Trả', CAST(300000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang], [idNhanVien],[ngayThue], [trangThai], [tongTienCoc], [DiaChiGiaoHang],[PhuongThucThanhToan]) VALUES (4, 1, 2, CAST(N'2026-04-19T12:39:07.617' AS DateTime), N'Đã Trả', CAST(99000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang], [idNhanVien], [ngayThue],[trangThai], [tongTienCoc], [DiaChiGiaoHang],[PhuongThucThanhToan]) VALUES (5, 5, 2, CAST(N'2026-04-19T12:42:09.907' AS DateTime), N'Đã Trả', CAST(300000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang],[idNhanVien], [ngayThue], [trangThai], [tongTienCoc],[DiaChiGiaoHang], [PhuongThucThanhToan]) VALUES (6, 1, 2, CAST(N'2026-04-19T13:07:50.940' AS DateTime), N'Đã Trả', CAST(100000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang], [idNhanVien], [ngayThue], [trangThai], [tongTienCoc], [DiaChiGiaoHang], [PhuongThucThanhToan]) VALUES (7, 1, 2, CAST(N'2026-04-19T13:37:01.013' AS DateTime), N'Đã Trả', CAST(100000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang], [idNhanVien],[ngayThue], [trangThai], [tongTienCoc], [DiaChiGiaoHang],[PhuongThucThanhToan]) VALUES (8, 1, 2, CAST(N'2026-04-19T13:38:49.343' AS DateTime), N'Đã Trả', CAST(100000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang], [idNhanVien], [ngayThue], [trangThai], [tongTienCoc], [DiaChiGiaoHang], [PhuongThucThanhToan]) VALUES (9, 1, 2, CAST(N'2026-04-19T13:48:19.677' AS DateTime), N'Đã Trả', CAST(99000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang],[idNhanVien], [ngayThue], [trangThai], [tongTienCoc],[DiaChiGiaoHang], [PhuongThucThanhToan]) VALUES (10, 1, 2, CAST(N'2026-04-19T13:49:24.043' AS DateTime), N'Đã Trả', CAST(99000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang], [idNhanVien], [ngayThue],[trangThai], [tongTienCoc], [DiaChiGiaoHang],[PhuongThucThanhToan]) VALUES (11, 1, 2, CAST(N'2026-04-19T14:12:47.160' AS DateTime), N'Đã Trả', CAST(100000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach],[idKhachHang], [idNhanVien], [ngayThue], [trangThai],[tongTienCoc], [DiaChiGiaoHang], [PhuongThucThanhToan]) VALUES (12, 5, 2, CAST(N'2026-04-19T14:13:34.660' AS DateTime), N'Đã Trả', CAST(350000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang],[idNhanVien], [ngayThue], [trangThai], [tongTienCoc],[DiaChiGiaoHang], [PhuongThucThanhToan]) VALUES (13, 2, 2, CAST(N'2026-04-19T14:13:51.063' AS DateTime), N'Đang Thuê', CAST(99000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang],[idNhanVien], [ngayThue], [trangThai], [tongTienCoc],[DiaChiGiaoHang], [PhuongThucThanhToan]) VALUES (14, 1, 2, CAST(N'2026-04-19T14:16:03.020' AS DateTime), N'Đang Thuê', CAST(100000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang], [idNhanVien],[ngayThue], [trangThai], [tongTienCoc], [DiaChiGiaoHang],[PhuongThucThanhToan]) VALUES (15, 1, 2, CAST(N'2026-04-23T04:17:15.090' AS DateTime), N'Đã Trả', CAST(100000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang], [idNhanVien], [ngayThue],[trangThai], [tongTienCoc], [DiaChiGiaoHang],[PhuongThucThanhToan]) VALUES (16, 1, 2, CAST(N'2026-04-23T04:18:39.250' AS DateTime), N'Đã Trả', CAST(100000.00 AS Decimal(18, 2)), NULL, NULL)
INSERT [dbo].[PhieuThueSach] ([idPhieuThueSach], [idKhachHang], [idNhanVien], [ngayThue], [trangThai], [tongTienCoc], [DiaChiGiaoHang], [PhuongThucThanhToan]) VALUES (17, 1, 2, CAST(N'2026-04-23T04:34:54.737' AS DateTime), N'Đã Trả', CAST(99000.00 AS Decimal(18, 2)), NULL, NULL)
SET IDENTITY_INSERT [dbo].[PhieuThueSach] OFF
GO

SET IDENTITY_INSERT[dbo].[PhieuThuongPhat] ON 
INSERT [dbo].[PhieuThuongPhat] ([idPhieuThuongPhat], [idNhanVien], [idNguoiTao],[NgayTao], [SoTien], [LyDo], [idPhieuLuong]) VALUES (5, 2, 1, CAST(N'2026-04-21T04:49:32.310' AS DateTime), CAST(100000.00 AS Decimal(18, 2)), N'thưởng lễ', NULL)
SET IDENTITY_INSERT [dbo].[PhieuThuongPhat] OFF
GO

SET IDENTITY_INSERT [dbo].[PhieuTraSach] ON 
INSERT [dbo].[PhieuTraSach] ([IdPhieuTra], [IdPhieuThueSach],[IdNhanVien], [NgayTra], [TongPhiThue], [TongTienPhat], [TongTienCocHoan], [DiemTichLuy]) VALUES (1, 1, 2, CAST(N'2026-04-19T12:38:08.797' AS DateTime), CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(350000.00 AS Decimal(18, 2)), 5)
INSERT [dbo].[PhieuTraSach] ([IdPhieuTra], [IdPhieuThueSach],[IdNhanVien], [NgayTra], [TongPhiThue], [TongTienPhat],[TongTienCocHoan], [DiemTichLuy]) VALUES (2, 5, 2, CAST(N'2026-04-19T13:36:10.190' AS DateTime), CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(300000.00 AS Decimal(18, 2)), 5)
INSERT [dbo].[PhieuTraSach] ([IdPhieuTra], [IdPhieuThueSach], [IdNhanVien],[NgayTra], [TongPhiThue], [TongTienPhat], [TongTienCocHoan],[DiemTichLuy]) VALUES (3, 7, 2, CAST(N'2026-04-19T13:37:37.163' AS DateTime), CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(100000.00 AS Decimal(18, 2)), 5)
INSERT [dbo].[PhieuTraSach] ([IdPhieuTra], [IdPhieuThueSach], [IdNhanVien], [NgayTra],[TongPhiThue], [TongTienPhat], [TongTienCocHoan],[DiemTichLuy]) VALUES (4, 6, 2, CAST(N'2026-04-19T13:37:41.600' AS DateTime), CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(100000.00 AS Decimal(18, 2)), 5)
INSERT [dbo].[PhieuTraSach] ([IdPhieuTra],[IdPhieuThueSach], [IdNhanVien], [NgayTra], [TongPhiThue],[TongTienPhat], [TongTienCocHoan], [DiemTichLuy]) VALUES (5, 4, 2, CAST(N'2026-04-19T13:37:45.560' AS DateTime), CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(99000.00 AS Decimal(18, 2)), 5)
INSERT[dbo].[PhieuTraSach] ([IdPhieuTra], [IdPhieuThueSach],[IdNhanVien], [NgayTra], [TongPhiThue], [TongTienPhat],[TongTienCocHoan], [DiemTichLuy]) VALUES (6, 3, 2, CAST(N'2026-04-19T13:37:49.157' AS DateTime), CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(300000.00 AS Decimal(18, 2)), 5)
INSERT [dbo].[PhieuTraSach] ([IdPhieuTra], [IdPhieuThueSach], [IdNhanVien],[NgayTra], [TongPhiThue], [TongTienPhat],[TongTienCocHoan], [DiemTichLuy]) VALUES (7, 8, 2, CAST(N'2026-04-19T13:47:58.930' AS DateTime), CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(100000.00 AS Decimal(18, 2)), 5)
INSERT [dbo].[PhieuTraSach] ([IdPhieuTra], [IdPhieuThueSach], [IdNhanVien], [NgayTra],[TongPhiThue], [TongTienPhat], [TongTienCocHoan],[DiemTichLuy]) VALUES (8, 9, 2, CAST(N'2026-04-19T13:50:08.187' AS DateTime), CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(99000.00 AS Decimal(18, 2)), 5)
INSERT [dbo].[PhieuTraSach] ([IdPhieuTra], [IdPhieuThueSach], [IdNhanVien], [NgayTra],[TongPhiThue], [TongTienPhat], [TongTienCocHoan],[DiemTichLuy]) VALUES (9, 10, 2, CAST(N'2026-04-19T13:52:06.747' AS DateTime), CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(99000.00 AS Decimal(18, 2)), 5)
INSERT [dbo].[PhieuTraSach] ([IdPhieuTra],[IdPhieuThueSach], [IdNhanVien], [NgayTra], [TongPhiThue],[TongTienPhat], [TongTienCocHoan], [DiemTichLuy]) VALUES (10, 2, 2, CAST(N'2026-04-19T13:52:29.340' AS DateTime), CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(100000.00 AS Decimal(18, 2)), 5)
INSERT [dbo].[PhieuTraSach] ([IdPhieuTra],[IdPhieuThueSach], [IdNhanVien], [NgayTra], [TongPhiThue],[TongTienPhat], [TongTienCocHoan], [DiemTichLuy]) VALUES (11, 12, 2, CAST(N'2026-04-19T14:14:27.633' AS DateTime), CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(350000.00 AS Decimal(18, 2)), 5)
INSERT [dbo].[PhieuTraSach] ([IdPhieuTra], [IdPhieuThueSach],[IdNhanVien], [NgayTra], [TongPhiThue], [TongTienPhat],[TongTienCocHoan], [DiemTichLuy]) VALUES (12, 11, 2, CAST(N'2026-04-19T14:14:35.150' AS DateTime), CAST(15000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(100000.00 AS Decimal(18, 2)), 5)
INSERT [dbo].[PhieuTraSach] ([IdPhieuTra], [IdPhieuThueSach],[IdNhanVien], [NgayTra], [TongPhiThue], [TongTienPhat],[TongTienCocHoan], [DiemTichLuy]) VALUES (13, 15, 2, CAST(N'2026-04-23T04:17:58.947' AS DateTime), CAST(10000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(100000.00 AS Decimal(18, 2)), 5)
INSERT [dbo].[PhieuTraSach] ([IdPhieuTra], [IdPhieuThueSach], [IdNhanVien],[NgayTra], [TongPhiThue], [TongTienPhat],[TongTienCocHoan], [DiemTichLuy]) VALUES (14, 16, 2, CAST(N'2026-04-23T04:33:48.650' AS DateTime), CAST(10000.00 AS Decimal(18, 2)), CAST(4000.00 AS Decimal(18, 2)), CAST(100000.00 AS Decimal(18, 2)), 5)
INSERT [dbo].[PhieuTraSach] ([IdPhieuTra], [IdPhieuThueSach], [IdNhanVien],[NgayTra], [TongPhiThue], [TongTienPhat],[TongTienCocHoan], [DiemTichLuy]) VALUES (15, 17, 2, CAST(N'2026-04-23T04:41:40.743' AS DateTime), CAST(10000.00 AS Decimal(18, 2)), CAST(8000.00 AS Decimal(18, 2)), CAST(99000.00 AS Decimal(18, 2)), 5)
SET IDENTITY_INSERT [dbo].[PhieuTraSach] OFF
GO

SET IDENTITY_INSERT [dbo].[PhieuXuatHuy] ON 
INSERT [dbo].[PhieuXuatHuy] ([idPhieuXuatHuy],[idNhanVienXuat], [NgayXuatHuy], [LyDoXuatHuy],[TongGiaTriHuy]) VALUES (1, 1, CAST(N'2026-04-11T23:30:48.070' AS DateTime), N'SỮA HỎNG', CAST(0.00 AS Decimal(18, 2)))
INSERT [dbo].[PhieuXuatHuy] ([idPhieuXuatHuy], [idNhanVienXuat],[NgayXuatHuy], [LyDoXuatHuy], [TongGiaTriHuy]) VALUES (2, 1, CAST(N'2026-04-12T01:23:58.567' AS DateTime), N'hết hạn sử dụng', CAST(0.00 AS Decimal(18, 2)))
SET IDENTITY_INSERT [dbo].[PhieuXuatHuy] OFF
GO

SET IDENTITY_INSERT [dbo].[PhuThu] ON 
INSERT [dbo].[PhuThu] ([idPhuThu], [TenPhuThu], [GiaTri],[LoaiGiaTri]) VALUES (1, N'Phí mang về', CAST(1000.00 AS Decimal(18, 2)), N'VNĐ')
INSERT [dbo].[PhuThu] ([idPhuThu], [TenPhuThu], [GiaTri], [LoaiGiaTri]) VALUES (2, N'VAT (5%)', CAST(5.00 AS Decimal(18, 2)), N'%')
INSERT [dbo].[PhuThu] ([idPhuThu], [TenPhuThu],[GiaTri], [LoaiGiaTri]) VALUES (3, N'Phí giao hàng Online', CAST(5000.00 AS Decimal(18, 2)), N'VNĐ')
SET IDENTITY_INSERT [dbo].[PhuThu] OFF
GO

INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'CM_CAI_DAT', N'Cài đặt phần mềm', N'Hệ thống')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'CM_NHAT_KY_HE_THONG', N'Quản lý nhật ký hệ thống ', N'Hệ thống')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'CM_THONG_BAO', N'Xem thông báo hệ thống', N'Hệ thống')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'FULL_NV', N'Toàn quyền Nhân viên', N'Hệ thống')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'FULL_QL', N'Toàn quyền Quản lý', N'Hệ thống')
INSERT[dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_CHAM_CONG', N'Chấm công vào/ra ca', N'Cá nhân')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_CHE_BIEN', N'Màn hình Bếp/Pha chế', N'Vận hành POS')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_DAT_BAN', N'Xử lý Đặt bàn', N'Vận hành POS')
INSERT[dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_GIAO_HANG', N'Màn hình Giao hàng', N'Vận hành POS')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_GOI_MON', N'Order & Gọi món', N'Vận hành POS')
INSERT[dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_LICH_LAM_VIEC', N'Xem Lịch làm việc cá nhân', N'Cá nhân')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_PHIEU_LUONG', N'Xem Phiếu lương cá nhân', N'Cá nhân')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_SO_DO_BAN', N'Truy cập Sơ đồ bàn', N'Vận hành POS')
INSERT[dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_THANH_TOAN', N'Thanh toán hóa đơn', N'Vận hành POS')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_THONG_TIN', N'Xem Thông tin cá nhân', N'Cá nhân')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'NV_THUE_SACH', N'Xử lý Thuê/Trả sách', N'Vận hành POS')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen],[NhomQuyen]) VALUES (N'QL_BAN', N'Quản lý thêm sửa xóa bàn', N'Quản lý Bàn ')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen],[NhomQuyen]) VALUES (N'QL_BAO_CAO_DOANH_THU', N'Xem Báo cáo Doanh thu', N'Tổng quan & Báo cáo')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_BAO_CAO_HIEU_SUAT_NHAN_SU', N'Xem Báo cáo Hiệu suất KPI', N'Tổng quan & Báo cáo')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen],[NhomQuyen]) VALUES (N'QL_BAO_CAO_NHAN_SU', N'Xem Báo cáo Nhân sự', N'Tổng quan & Báo cáo')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_BAO_CAO_TON_KHO_NL', N'Xem Báo cáo Kho Nguyên liệu', N'Tổng quan & Báo cáo')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen],[NhomQuyen]) VALUES (N'QL_BAO_CAO_TON_KHO_SACH', N'Xem Báo cáo Tồn kho Sách', N'Tổng quan & Báo cáo')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_CAI_DAT_NHAN_SU', N'Cài đặt tham số Nhân sự', N'Quản lý Nhân sự')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen],[NhomQuyen]) VALUES (N'QL_DANH_MUC', N'Quản lý Danh mục sản phẩm', N'Quản lý Sản phẩm')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_DANH_MUC_SACH', N'Quản lý danh mục sách', N'Quản lý Thư viện')
INSERT[dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_DINH_LUONG', N'Quản lý Định lượng nguyên liệu sản phẩm', N'Quản lý Sản phẩm')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen],[NhomQuyen]) VALUES (N'QL_DON_HANG', N'Quản lý Đơn hàng', N'Tài chính & Giao dịch')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen],[NhomQuyen]) VALUES (N'QL_DON_VI_CHUYEN_DOI', N'Quản lý Kho Đơn vị chuyển đổi', N'Quản lý Kho')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_DON_XIN_NGHI', N'Quản lý Đơn xin nghỉ', N'Quản lý Nhân sự')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_KHACH_HANG', N'Quản lý Khách hàng', N'Quản lý Khách hàng KM')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen],[NhomQuyen]) VALUES (N'QL_KHU_VUC', N'Quản lý Khu vực bàn', N'Quản lý Bàn ')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen],[NhomQuyen]) VALUES (N'QL_KHUYEN_MAI', N'Quản lý Khuyến mãi', N'Quản lý Khách hàng KM')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_KIEM_KHO', N'Quản lý Kiểm kho', N'Quản lý Kho')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_LICH_LAM_VIEC', N'Quản lý lịch  làm việc', N'Quản lý Nhân sự')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_LICH_SU_THUE_SACH', N'Quản lý lịch sử  thuê sách', N'Quản lý Thư viện')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_LUONG', N'Quản lý Bảng lương', N'Quản lý lương')
INSERT [dbo].[Quyen] ([idQuyen],[TenQuyen], [NhomQuyen]) VALUES (N'QL_NGUOI_GIAO_HANG', N'Quản lý đơn vị vận chuyển', N'Tài chính & Giao dịch')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_NGUYEN_LIEU', N'Quản lý Kho Nguyên liệu', N'Quản lý Kho')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_NHA_CUNG_CAP', N'Quản lý Nhà cung cấp', N'Quản lý Kho')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_NHAN_VIEN', N'Quản lý Danh sách Nhân viên', N'Quản lý Nhân sự')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_NHAP_KHO', N'Quản lý Nhập kho', N'Quản lý Kho')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_PHAN_QUYEN', N'Quản lý Phân quyền', N'Quản lý Nhân sự')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_PHAT_LUONG', N'Quản lý Phát lương', N'Quản lý lương')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_PHU_THU', N'Quản lý Phụ thu', N'Tài chính & Giao dịch')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_SACH', N'Quản lý Thư viện Sách', N'Quản lý Thư viện')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_SAN_PHAM', N'Quản lý Sản phẩm CURD', N'Quản lý Sản phẩm')
INSERT[dbo].[Quyen] ([idQuyen], [TenQuyen], [NhomQuyen]) VALUES (N'QL_SU_CO_BAN', N'Quản lý Sự cố bàn', N'Quản lý Bàn ')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen],[NhomQuyen]) VALUES (N'QL_THONG_BAO', N'Quản lý thông báo', N'Hệ thống')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen],[NhomQuyen]) VALUES (N'QL_TON_KHO', N'Xem Tồn Kho/ cảnh báo', N'Quản lý Kho')
INSERT [dbo].[Quyen] ([idQuyen],[TenQuyen], [NhomQuyen]) VALUES (N'QL_TONG_QUAN', N'Xem Dashboard Tổng quan', N'Tổng quan & Báo cáo')
INSERT [dbo].[Quyen] ([idQuyen], [TenQuyen],[NhomQuyen]) VALUES (N'QL_XUAT_HUY', N'Quản lý Xuất hủy', N'Quản lý Kho')
GO

SET IDENTITY_INSERT [dbo].[Sach] ON 
INSERT [dbo].[Sach] ([idSach], [tenSach], [namXuatBan], [moTa], [soLuongTong], [soLuongHienCo], [AnhBia], [GiaBia],[ViTri]) VALUES (1, N'Tôi thấy hoa vàng trên cỏ xanh', 2023, N'Cuốn sách là những mẩu chuyện nhỏ kể dưới góc nhìn của nhân vật Thiều về cuộc sống, tình anh em (với Tường), tình bạn (với Mận) và những câu chuyện về gia đình, làng quê nghèo. Tác phẩm mang đậm hoài niệm tuổi thơ, đan xen những ghen tị trẻ con và bài học về lòng nhân ái, sự vị tha.', 15, 14, N'/images/books/141c7cb0-93ac-4b88-a918-55c377c2fdbd_1_toi-thay-hoa-vang-tren-co-xanh.jpg', CAST(100000.00 AS Decimal(18, 2)), N'T1-K1')
INSERT [dbo].[Sach] ([idSach], [tenSach], [namXuatBan], [moTa], [soLuongTong],[soLuongHienCo], [AnhBia], [GiaBia], [ViTri]) VALUES (2, N'Sapiens: Lược sử loài người', 2014, N'Cuốn sách chia lịch sử Sapiens thành 4 phần chính dựa trên các cuộc cách mạng lớn:
Cách mạng Nhận thức (70.000 năm trước): Khi con người phát triển ngôn ngữ và tư duy.
Cách mạng Nông nghiệp (12.000 năm trước): Con người chuyển từ săn bắn hái lượm sang định cư.
Sự thống nhất của loài người: Sự hình thành tiền tệ, đế quốc và tôn giáo.
Cách mạng Khoa học (500 năm trước): Sự phát triển của kỹ thuật và hiểu biết, dẫn đến quyền năng tối thượng của Sapiens.', 5, 5, N'/images/books/1947d497-5b75-49c8-882a-fda98e36f787_4_sapiens-luoc-su-loai-nguoi.jpg', CAST(350000.00 AS Decimal(18, 2)), N'T1-K1')
INSERT [dbo].[Sach] ([idSach], [tenSach], [namXuatBan], [moTa], [soLuongTong], [soLuongHienCo],[AnhBia], [GiaBia], [ViTri]) VALUES (3, N'Đắc Nhân Tâm (Tên tiếng Anh: How to Win Friends and Influence People).', 2022, N'"Đắc Nhân Tâm" không chỉ là sách, mà là cẩm nang hướng dẫn cách cư xử, giao tiếp và tư duy để giành được thiện cảm, sự tin tưởng và ảnh hưởng tích cực đến người khác. Sách bao gồm các nguyên tắc vàng để xây dựng mối quan hệ vững chắc, cách thuyết phục người khác mà không gây ra sự phản kháng, và nghệ thuật lãnh đạo thông qua sự chân thành.', 6, 5, N'/images/books/3_ac-nhan-tam-ten-tieng-anh-how-to-win-friends-and-influence-people.jpg', CAST(99000.00 AS Decimal(18, 2)), N'T1-K1')
INSERT [dbo].[Sach] ([idSach],[tenSach], [namXuatBan], [moTa], [soLuongTong], [soLuongHienCo],[AnhBia], [GiaBia], [ViTri]) VALUES (4, N'Harry Potter và Hòn đá Phù thủy (Harry Potter and the Philosopher''s Stone)', 1997, N'Câu chuyện bắt đầu khi Harry Potter 11 tuổi, sống bất hạnh với dì dượng, nhận được thư nhập học trường phù thủy Hogwarts. Tại đây, cậu kết bạn với Ron, Hermione và khám phá bí mật về cái chết của cha mẹ, đồng thời đối đầu với những thế lực hắc ám để bảo vệ Hòn đá Phù thủy.', 4, 4, N'/images/books/4_harry-potter-va-hon-a-phu-thuy-harry-potter-and-the-philosophers-stone.jpg', CAST(300000.00 AS Decimal(18, 2)), N'T1-K1')
SET IDENTITY_INSERT [dbo].[Sach] OFF
GO

INSERT [dbo].[Sach_NhaXuatBan] ([idSach], [idNhaXuatBan]) VALUES (1, 1)
INSERT [dbo].[Sach_NhaXuatBan] ([idSach],[idNhaXuatBan]) VALUES (2, 2)
INSERT [dbo].[Sach_NhaXuatBan] ([idSach], [idNhaXuatBan]) VALUES (3, 3)
INSERT [dbo].[Sach_NhaXuatBan] ([idSach], [idNhaXuatBan]) VALUES (4, 1)
GO

INSERT [dbo].[Sach_TacGia] ([idSach], [idTacGia]) VALUES (1, 1)
INSERT [dbo].[Sach_TacGia] ([idSach],[idTacGia]) VALUES (2, 2)
INSERT [dbo].[Sach_TacGia] ([idSach], [idTacGia]) VALUES (3, 3)
INSERT [dbo].[Sach_TacGia] ([idSach], [idTacGia]) VALUES (4, 4)
GO

INSERT[dbo].[Sach_TheLoai] ([idSach], [idTheLoai]) VALUES (1, 1)
INSERT [dbo].[Sach_TheLoai] ([idSach], [idTheLoai]) VALUES (1, 2)
INSERT [dbo].[Sach_TheLoai] ([idSach],[idTheLoai]) VALUES (2, 3)
INSERT [dbo].[Sach_TheLoai] ([idSach], [idTheLoai]) VALUES (2, 4)
INSERT [dbo].[Sach_TheLoai] ([idSach], [idTheLoai]) VALUES (2, 5)
INSERT [dbo].[Sach_TheLoai] ([idSach], [idTheLoai]) VALUES (2, 6)
INSERT[dbo].[Sach_TheLoai] ([idSach], [idTheLoai]) VALUES (3, 7)
INSERT [dbo].[Sach_TheLoai] ([idSach], [idTheLoai]) VALUES (3, 8)
INSERT [dbo].[Sach_TheLoai] ([idSach],[idTheLoai]) VALUES (3, 9)
INSERT [dbo].[Sach_TheLoai] ([idSach], [idTheLoai]) VALUES (4, 2)
INSERT [dbo].[Sach_TheLoai] ([idSach], [idTheLoai]) VALUES (4, 10)
GO

SET IDENTITY_INSERT [dbo].[SanPham] ON 
INSERT [dbo].[SanPham] ([idSanPham], [tenSanPham],[idDanhMuc], [giaBan], [moTa], [trangThaiKinhDoanh],[HinhAnh], [NhomIn]) VALUES (1, N'Cafe đen', 1, CAST(15000.00 AS Decimal(18, 2)), N'cafe có đá', 1, N'/images/foods/cafe-en-639115174391118843.jpg', N'Pha chế')
INSERT [dbo].[SanPham] ([idSanPham],[tenSanPham], [idDanhMuc], [giaBan], [moTa], [trangThaiKinhDoanh],[HinhAnh], [NhomIn]) VALUES (2, N'trà sữa', 2, CAST(20000.00 AS Decimal(18, 2)), N'tà tữa tét', 1, N'/images/foods/tra-sua-639115178005448401.jpg', N'Pha chế')
SET IDENTITY_INSERT[dbo].[SanPham] OFF
GO

SET IDENTITY_INSERT [dbo].[TacGia] ON 
INSERT[dbo].[TacGia] ([idTacGia], [tenTacGia], [gioiThieu]) VALUES (1, N'Nguyễn Nhật Ánh', N'Nguyễn Nhật Ánh (sinh năm 1955 tại Quảng Nam) là nhà văn nổi tiếng bậc nhất Việt Nam viết cho thanh thiếu niên, nổi bật với phong cách văn xuôi giản dị, chân thành và giàu cảm xúc. Ông được ví như người kể chuyện tài tình, kết nối ký ức tuổi thơ qua các tác phẩm nhẹ nhàng, tinh tế và đằm thắm.')
INSERT [dbo].[TacGia] ([idTacGia], [tenTacGia], [gioiThieu]) VALUES (2, N'Yuval Noah Harari', N'Yuval Noah Harari (sinh năm 1976 tại Israel) là một nhà sử học, triết gia và nhà tư tưởng công chúng có ảnh hưởng lớn trên toàn cầu. Ông nổi tiếng với khả năng kết nối lịch sử, khoa học, triết học và công nghệ để giải thích các vấn đề đương đại.')
INSERT [dbo].[TacGia] ([idTacGia],[tenTacGia], [gioiThieu]) VALUES (3, N'Dale Carnegie', N'Dale Carnegie (1888–1955) là một nhà văn, nhà thuyết trình người Mỹ nổi tiếng thế giới, được mệnh danh là bậc thầy trong lĩnh vực phát triển năng lực cá nhân, nghệ thuật giao tiếp và thuyết phục.')
INSERT [dbo].[TacGia] ([idTacGia], [tenTacGia], [gioiThieu]) VALUES (4, N'J.K. Rowling', N'J.K. Rowling, tên thật là Joanne Rowling, là một nhà văn, nhà biên kịch và nhà từ thiện người Anh, nổi tiếng thế giới với tư cách là tác giả của loạt tiểu thuyết giả tưởng "Harry Potter".')
SET IDENTITY_INSERT [dbo].[TacGia] OFF
GO

SET IDENTITY_INSERT [dbo].[TheLoai] ON 
INSERT [dbo].[TheLoai] ([idTheLoai], [tenTheLoai], [MoTa]) VALUES (1, N'Truyện dài', N'Truyện dài là một thể loại văn xuôi tự sự có dung lượng lớn, số trang nhiều, thường được chia thành nhiều chương, hồi. Khác với truyện ngắn tập trung vào một tình huống duy nhất, truyện dài miêu tả hàng loạt sự kiện, bối cảnh phức tạp và quá trình phát triển tính cách của tuyến nhân vật rộng lớn trong một phạm vi không gian và thời gian tương đối dài.')
INSERT [dbo].[TheLoai] ([idTheLoai], [tenTheLoai], [MoTa]) VALUES (2, N'văn học thiếu nhi', N'Văn học thiếu nhi là các tác phẩm (truyện, thơ, tạp chí) được sáng tác dành riêng cho trẻ em hoặc viết về thiếu nhi, mang nội dung giáo dục tâm hồn, nhân cách qua lăng kính ngây thơ. Đặc điểm nổi bật là ngôn từ giản dị, dễ hiểu, giàu hình ảnh và hướng thiện. Các thể loại phổ biến bao gồm truyện cổ tích, ngụ ngôn, truyện đồng thoại, thơ thiếu nhi, và truyện phiêu lưu.')
INSERT [dbo].[TheLoai] ([idTheLoai], [tenTheLoai], [MoTa]) VALUES (3, N'Khoa học', N'Khoa học mô tả (Descriptive Science) là phương pháp nghiên cứu tập trung vào việc quan sát, ghi chép và phân loại các đặc điểm, hiện tượng tự nhiên hoặc xã hội để trả lời câu hỏi "cái gì", thay vì giải thích "tại sao". Đây là nền tảng cốt lõi trong các môn khoa học tự nhiên như sinh học (phân loại loài), địa lý, và quan sát thiên văn.')
INSERT [dbo].[TheLoai] ([idTheLoai], [tenTheLoai], [MoTa]) VALUES (4, N'Nhân chủng học', N'Nhân chủng học (Anthropology) là chuyên ngành khoa học xã hội nghiên cứu về con người, sự đa dạng của nhân loại từ góc độ sinh học, văn hóa, và xã hội, bao gồm cả quá khứ và hiện tại. Phạm vi của nhân chủng học rất rộng, thường được chia thành 4 phân ngành chính:')
INSERT [dbo].[TheLoai] ([idTheLoai], [tenTheLoai], [MoTa]) VALUES (5, N'Lịch sử', N'"Lịch sử" với tư cách là một thể loại (genre) văn học và nghệ thuật mô tả các sự kiện, nhân vật, và bối cảnh trong quá khứ, kết hợp giữa sự thật khách quan và hư cấu nghệ thuật.')
INSERT [dbo].[TheLoai] ([idTheLoai], [tenTheLoai],[MoTa]) VALUES (6, N'Khảo cổ học', N'Khảo cổ học là một ngành khoa học nghiên cứu về lịch sử, hoạt động của con người trong quá khứ thông qua các di tích, di vật, và hiện vật được tìm thấy. Ngành này đóng vai trò quan trọng trong việc tái dựng lại văn hóa, hành vi và sự phát triển của các xã hội cổ xưa.')
INSERT [dbo].[TheLoai] ([idTheLoai],[tenTheLoai], [MoTa]) VALUES (7, N'Sách Self-help', N'Sách self-help (sách tự lực/phát triển bản thân) là thể loại văn học hướng dẫn, truyền cảm hứng giúp độc giả cải thiện kỹ năng, tư duy, quản lý cảm xúc và giải quyết vấn đề cá nhân. Nội dung thường đúc kết kinh nghiệm, triết lý sống hoặc câu chuyện thành công, nhằm "ngọn hải đăng" dẫn đường giúp thay đổi bản thân.')
INSERT [dbo].[TheLoai] ([idTheLoai], [tenTheLoai], [MoTa]) VALUES (8, N'kỹ năng giao tiếp', N'Kỹ năng giao tiếp là khả năng sử dụng ngôn ngữ (lời nói, văn bản) và phi ngôn ngữ (cử chỉ, ánh mắt) để truyền đạt, tiếp nhận và phản hồi thông tin một cách hiệu quả, giúp tạo dựng mối quan hệ và đạt được mục đích tương tác.')
INSERT [dbo].[TheLoai] ([idTheLoai], [tenTheLoai], [MoTa]) VALUES (9, N'tâm lý học ứng dụng', N'Tâm lý học ứng dụng (Applied Psychology) là việc sử dụng các lý thuyết, phương pháp và phát hiện của tâm lý học khoa học để giải quyết các vấn đề thực tiễn trong cuộc sống, hành vi và trải nghiệm của con người. Thay vì chỉ nghiên cứu lý thuyết, lĩnh vực này tập trung vào việc áp dụng kiến thức vào các ngữ cảnh cụ thể.')
INSERT [dbo].[TheLoai] ([idTheLoai], [tenTheLoai], [MoTa]) VALUES (10, N'Tiểu thuyết giả tưởng', N'Tiểu thuyết giả tưởng (Speculative Fiction) là thể loại văn học bao gồm các câu chuyện hư cấu không dựa trên thực tế, tập trung vào những thế giới, sinh vật, hoặc hiện tượng kỳ ảo, siêu nhiên, tương lai, không có thật. Nó khác biệt với chủ nghĩa hiện thực thông thường bằng cách đặt ra các câu hỏi "nếu như" (what if).')
SET IDENTITY_INSERT [dbo].[TheLoai] OFF
GO

SET IDENTITY_INSERT [dbo].[ThongBao] ON 
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung],[ThoiGianTao], [LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (1, 1, N'hôm nay nghỉ nha TNV', CAST(N'2026-04-18T04:36:33.290' AS DateTime), N'ThongBaoNhanVien', NULL, 0)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao],[LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (2, 1, N'hôm nay nghỉ nha NV', CAST(N'2026-04-18T04:36:55.753' AS DateTime), N'ThongBaoToanNhanVien', NULL, 0)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (3, 1, N'hôm nay nghỉ nha QL', CAST(N'2026-04-18T04:37:15.273' AS DateTime), N'ThongBaoQuanLy', NULL, 0)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao],[NoiDung], [ThoiGianTao], [LoaiThongBao], [IdLienQuan],[DaXem]) VALUES (4, 2, N'Bàn T1-B2 vừa được báo cáo sự cố: gãy chân =))', CAST(N'2026-04-18T12:29:57.627' AS DateTime), N'SuCoBan', 2, 1)
INSERT [dbo].[ThongBao] ([idThongBao],[idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (5, 2, N'Phiếu gọi món mới cho[T2-B1].', CAST(N'2026-04-18T14:18:32.980' AS DateTime), N'PhieuGoiMon', 1, 1)
INSERT [dbo].[ThongBao] ([idThongBao],[idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (6, 2, N'Phiếu gọi món mới cho [T1-B1].', CAST(N'2026-04-18T14:36:20.407' AS DateTime), N'PhieuGoiMon', 1, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (7, 2, N'Phiếu gọi món mới cho[T1-B1].', CAST(N'2026-04-18T14:42:23.027' AS DateTime), N'PhieuGoiMon', 2, 1)
INSERT [dbo].[ThongBao] ([idThongBao],[idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (8, 2, N'Cảnh báo: Tồn kho ''Sữa tươi thanh trùng'' chỉ còn -199.996,00 lít.', CAST(N'2026-04-18T15:01:11.640' AS DateTime), N'CanhBaoKho', 4, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (9, 2, N'Cảnh báo: Tồn kho ''Trà túi lọc Lipton'' chỉ còn 4,00 túi.', CAST(N'2026-04-18T15:01:11.670' AS DateTime), N'CanhBaoKho', 5, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (10, 2, N'Cảnh báo: Tồn kho ''Trà túi lọc Lipton'' sắp hết. Hiện chỉ còn 3,00 túi.', CAST(N'2026-04-18T16:44:57.147' AS DateTime), N'CanhBaoKho', 5, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung],[ThoiGianTao], [LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (11, 2, N'Cảnh báo: Tồn kho ''Trà túi lọc Lipton'' sắp hết. Hiện chỉ còn 0,00 túi.', CAST(N'2026-04-18T17:20:55.497' AS DateTime), N'CanhBaoKho', 5, 1)
INSERT [dbo].[ThongBao] ([idThongBao],[idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (12, 2, N'Phiếu gọi món mới cho [T2-B1].', CAST(N'2026-04-18T17:22:20.087' AS DateTime), N'PhieuGoiMon', 5, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (13, 2, N'Phiếu gọi món mới cho[T2-B1].', CAST(N'2026-04-18T17:23:40.133' AS DateTime), N'PhieuGoiMon', 6, 1)
INSERT [dbo].[ThongBao] ([idThongBao],[idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (14, 2, N'Phiếu gọi món mới cho [T2-B1].', CAST(N'2026-04-18T18:13:00.627' AS DateTime), N'PhieuGoiMon', 6, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (15, 2, N'Phiếu gọi món mới cho[T2-B1].', CAST(N'2026-04-18T18:15:16.317' AS DateTime), N'PhieuGoiMon', 6, 1)
INSERT [dbo].[ThongBao] ([idThongBao],[idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (16, 2, N'Phiếu gọi món mới cho [T2-B1].', CAST(N'2026-04-18T18:20:24.997' AS DateTime), N'PhieuGoiMon', 6, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (17, 2, N'Phiếu gọi món mới cho[T1-B1].', CAST(N'2026-04-18T19:35:44.710' AS DateTime), N'PhieuGoiMon', 7, 1)
INSERT [dbo].[ThongBao] ([idThongBao],[idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (18, 2, N'Phiếu gọi món mới cho [T1-B1].', CAST(N'2026-04-18T19:35:51.960' AS DateTime), N'PhieuGoiMon', 7, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (19, 2, N'Phiếu gọi món mới cho[T1-B1].', CAST(N'2026-04-19T04:08:31.820' AS DateTime), N'PhieuGoiMon', 8, 1)
INSERT [dbo].[ThongBao] ([idThongBao],[idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (20, 2, N'Phiếu gọi món mới cho [T1-B1].', CAST(N'2026-04-19T05:34:20.700' AS DateTime), N'PhieuGoiMon', 9, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (21, 2, N'Phiếu gọi món mới cho[T1-B1].', CAST(N'2026-04-19T05:50:19.380' AS DateTime), N'PhieuGoiMon', 10, 1)
INSERT [dbo].[ThongBao] ([idThongBao],[idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (22, 2, N'Phiếu gọi món mới cho [T1-B1].', CAST(N'2026-04-19T06:16:27.080' AS DateTime), N'PhieuGoiMon', 11, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao],[LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (23, 2, N'Phiếu gọi món mới cho [Tại quán].', CAST(N'2026-04-19T06:47:57.843' AS DateTime), N'PhieuGoiMon', 12, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao],[LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (24, 2, N'Phiếu gọi món mới cho [Tại quán].', CAST(N'2026-04-19T06:56:15.720' AS DateTime), N'PhieuGoiMon', 13, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao],[LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (25, 2, N'Phiếu gọi món mới cho [T1-B1].', CAST(N'2026-04-19T16:27:25.663' AS DateTime), N'PhieuGoiMon', 14, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao],[LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (26, 2, N'Phiếu gọi món mới cho [T2-B1].', CAST(N'2026-04-20T02:44:02.620' AS DateTime), N'PhieuGoiMon', 15, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao],[LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (27, 2, N'Phiếu gọi món mới cho [T1-B1].', CAST(N'2026-04-20T05:30:01.093' AS DateTime), N'PhieuGoiMon', 16, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung],[ThoiGianTao], [LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (28, 2, N'Phiếu gọi món mới cho [T2-B1].', CAST(N'2026-04-20T14:15:14.653' AS DateTime), N'PhieuGoiMon', 16, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao],[NoiDung], [ThoiGianTao], [LoaiThongBao], [IdLienQuan],[DaXem]) VALUES (29, 2, N'Phiếu gọi món mới cho [T1-B1].', CAST(N'2026-04-20T14:20:20.570' AS DateTime), N'PhieuGoiMon', 18, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (30, 2, N'Bàn T2-B1 vừa được báo cáo sự cố: lỗi bàn ', CAST(N'2026-04-21T05:58:36.967' AS DateTime), N'SuCoBan', 3, 1)
INSERT[dbo].[ThongBao] ([idThongBao], [idNhanVienTao],[NoiDung], [ThoiGianTao], [LoaiThongBao], [IdLienQuan],[DaXem]) VALUES (31, 2, N'Bàn T2-B1 vừa được báo cáo sự cố: gãy chân', CAST(N'2026-04-21T06:13:50.577' AS DateTime), N'SuCoBan', 3, 1)
INSERT [dbo].[ThongBao] ([idThongBao],[idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (32, 2, N'Bàn T1-B2 vừa được báo cáo sự cố: bàn gãy chân
', CAST(N'2026-04-21T06:18:10.583' AS DateTime), N'SuCoBan', 2, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao],[LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (33, NULL, N'Đơn đặt bàn mới từ Web: Lâm Chu Bảo Toàn - Bàn T1-B1 lúc 07:30 22/04', CAST(N'2026-04-21T22:57:33.773' AS DateTime), N'DatBan', 0, 1)
INSERT [dbo].[ThongBao] ([idThongBao],[idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (34, NULL, N'Góp ý mới từ Lâm Chu Bảo Toàn: mong thêm nhiền món mới...', CAST(N'2026-04-22T00:21:35.840' AS DateTime), N'GopY', 1, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao],[LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (35, NULL, N'Góp ý mới từ Lâm Chu Bảo Toàn: thêm sách mới đi...', CAST(N'2026-04-22T00:40:29.570' AS DateTime), N'GopY', 2, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao],[LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (36, NULL, N'Đơn đặt bàn mới từ Web: Lâm Chu Bảo Toàn - Bàn T1-B1 lúc 16:30 22/04', CAST(N'2026-04-22T16:04:49.760' AS DateTime), N'DatBan', 0, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (37, NULL, N'Đơn đặt bàn mới từ Web: Lâm Chu Bảo Toàn - Bàn T1-B1 lúc 12:00 24/04', CAST(N'2026-04-22T22:08:47.587' AS DateTime), N'DatBan', 0, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao],[NoiDung], [ThoiGianTao], [LoaiThongBao], [IdLienQuan],[DaXem]) VALUES (38, NULL, N'Đơn đặt bàn mới từ Web: Lâm Chu Bảo Toàn - Bàn T1-B1 lúc 07:00 23/04', CAST(N'2026-04-23T01:39:03.010' AS DateTime), N'DatBan', 0, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung],[ThoiGianTao], [LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (39, NULL, N'Khách hàng Lâm Chu Bảo Toàn vừa hủy phiếu đặt T1-B1. Lý do: dssedffsea', CAST(N'2026-04-23T01:39:19.380' AS DateTime), N'HuyDatBan', 1, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (40, NULL, N'Đơn đặt bàn mới từ Web: Lâm Chu Bảo Toàn - Bàn T1-B1 lúc 07:00 23/04', CAST(N'2026-04-23T02:11:01.030' AS DateTime), N'DatBan', 0, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao],[NoiDung], [ThoiGianTao], [LoaiThongBao], [IdLienQuan],[DaXem]) VALUES (41, NULL, N'Khách hàng Lâm Chu Bảo Toàn vừa hủy phiếu đặt T1-B1. Lý do: zdxfgkjlhzdxfbjklfsdk   tetwsstt', CAST(N'2026-04-23T02:26:43.050' AS DateTime), N'HuyDatBan', 1, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao],[LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (42, NULL, N'Đơn đặt bàn mới từ Web: Lâm Chu Bảo Toàn - Bàn T1-B1 lúc 07:00 23/04', CAST(N'2026-04-23T02:27:37.827' AS DateTime), N'DatBan', 0, 1)
INSERT [dbo].[ThongBao] ([idThongBao],[idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (43, NULL, N'Khách hàng Lâm Chu Bảo Toàn vừa hủy phiếu đặt T1-B1. Lý do: Hidhdudjdhhd', CAST(N'2026-04-23T02:28:13.613' AS DateTime), N'HuyDatBan', 1, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (44, NULL, N'✅ ĐƠN HÀNG VNPAY ĐÃ THANH TOÁN (Mã đơn: #27)', CAST(N'2026-04-23T19:58:23.630' AS DateTime), N'DonHangMoi', 27, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao],[LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (45, NULL, N'🔔 CÓ ĐƠN HÀNG MỚI! (Mã đơn: #32 - Hình thức: Tiền mặt)', CAST(N'2026-04-23T20:53:22.787' AS DateTime), N'DonHangMoi', 32, 1)
INSERT [dbo].[ThongBao] ([idThongBao],[idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (46, NULL, N'🔔 CÓ ĐƠN HÀNG MỚI! (Mã đơn: #33 - Hình thức: Tiền mặt)', CAST(N'2026-04-23T20:53:54.390' AS DateTime), N'DonHangMoi', 33, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung],[ThoiGianTao], [LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (47, NULL, N'✅ ĐƠN HÀNG VNPAY ĐÃ THANH TOÁN (Mã đơn: #34)', CAST(N'2026-04-23T20:55:41.620' AS DateTime), N'DonHangMoi', 34, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung], [ThoiGianTao], [LoaiThongBao],[IdLienQuan], [DaXem]) VALUES (49, NULL, N'🔔 CÓ ĐƠN HÀNG MỚI! (Mã đơn: #35 - Hình thức: Tiền mặt)', CAST(N'2026-04-23T21:01:27.877' AS DateTime), N'DonHangMoi', 35, 1)
INSERT [dbo].[ThongBao] ([idThongBao], [idNhanVienTao], [NoiDung],[ThoiGianTao], [LoaiThongBao], [IdLienQuan], [DaXem]) VALUES (50, NULL, N'🔔 CÓ ĐƠN HÀNG MỚI! (Mã đơn: #36 - Hình thức: Tiền mặt)', CAST(N'2026-04-23T21:06:31.890' AS DateTime), N'DonHangMoi', 36, 1)
SET IDENTITY_INSERT [dbo].[ThongBao] OFF
GO

SET IDENTITY_INSERT [dbo].[TrangThaiCheBien] ON 
INSERT [dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien], [IdChiTietHoaDon],[IdHoaDon], [IdSanPham], [TenMon], [SoBan], [SoLuong], [GhiChu],[NhomIn], [TrangThai], [ThoiGianGoi], [ThoiGianBatDau],[ThoiGianHoanThanh]) VALUES (1, 1, 1, 1, N'Cafe đen', N'T1-B1', 1, N'ds', N'Pha chế', N'Hoàn thành', CAST(N'2026-04-18T14:36:20.157' AS DateTime), CAST(N'2026-04-18T19:06:52.717' AS DateTime), CAST(N'2026-04-18T19:06:53.973' AS DateTime))
INSERT [dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien],[IdChiTietHoaDon], [IdHoaDon], [IdSanPham], [TenMon], [SoBan],[SoLuong], [GhiChu], [NhomIn], [TrangThai], [ThoiGianGoi], [ThoiGianBatDau], [ThoiGianHoanThanh]) VALUES (2, 2, 2, 2, N'trà sữa', N'T1-B1', 1, N'tghd', N'Pha chế', N'Hoàn thành', CAST(N'2026-04-18T14:42:22.993' AS DateTime), CAST(N'2026-04-18T19:06:54.950' AS DateTime), CAST(N'2026-04-18T19:06:55.600' AS DateTime))
INSERT [dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien], [IdChiTietHoaDon], [IdHoaDon], [IdSanPham], [TenMon],[SoBan], [SoLuong], [GhiChu], [NhomIn], [TrangThai],[ThoiGianGoi], [ThoiGianBatDau], [ThoiGianHoanThanh]) VALUES (3, 8, 5, 1, N'Cafe đen', N'T2-B1', 1, NULL, N'Pha chế', N'Hoàn thành', CAST(N'2026-04-18T17:22:02.300' AS DateTime), CAST(N'2026-04-18T19:06:56.307' AS DateTime), CAST(N'2026-04-18T19:06:56.817' AS DateTime))
INSERT [dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien], [IdChiTietHoaDon], [IdHoaDon],[IdSanPham], [TenMon], [SoBan], [SoLuong], [GhiChu], [NhomIn], [TrangThai], [ThoiGianGoi], [ThoiGianBatDau],[ThoiGianHoanThanh]) VALUES (4, 9, 6, 1, N'Cafe đen', N'T2-B1', 2, NULL, N'Pha chế', N'Hoàn thành', CAST(N'2026-04-18T17:23:40.000' AS DateTime), CAST(N'2026-04-18T19:06:57.313' AS DateTime), CAST(N'2026-04-18T19:06:57.753' AS DateTime))
INSERT[dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien], [IdChiTietHoaDon], [IdHoaDon], [IdSanPham], [TenMon], [SoBan], [SoLuong],[GhiChu], [NhomIn], [TrangThai], [ThoiGianGoi],[ThoiGianBatDau], [ThoiGianHoanThanh]) VALUES (5, 10, 7, 1, N'Cafe đen', N'T2-B1', 1, N'deb', N'Pha chế', N'Hoàn thành', CAST(N'2026-04-18T19:35:44.607' AS DateTime), CAST(N'2026-04-18T19:36:38.363' AS DateTime), CAST(N'2026-04-18T19:36:41.417' AS DateTime))
INSERT [dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien], [IdChiTietHoaDon], [IdHoaDon], [IdSanPham], [TenMon],[SoBan], [SoLuong], [GhiChu], [NhomIn], [TrangThai],[ThoiGianGoi], [ThoiGianBatDau], [ThoiGianHoanThanh]) VALUES (6, 11, 8, 1, N'Cafe đen', N'T1-B1', 1, NULL, N'Pha chế', N'Hoàn thành', CAST(N'2026-04-19T04:08:31.717' AS DateTime), CAST(N'2026-04-19T04:09:07.850' AS DateTime), CAST(N'2026-04-19T04:09:09.280' AS DateTime))
INSERT [dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien], [IdChiTietHoaDon], [IdHoaDon],[IdSanPham], [TenMon], [SoBan], [SoLuong], [GhiChu],[NhomIn], [TrangThai], [ThoiGianGoi], [ThoiGianBatDau],[ThoiGianHoanThanh]) VALUES (7, 12, 9, 1, N'Cafe đen', N'T1-B1', 1, NULL, N'Pha chế', N'Hoàn thành', CAST(N'2026-04-19T05:34:20.607' AS DateTime), CAST(N'2026-04-19T05:37:52.427' AS DateTime), CAST(N'2026-04-19T05:37:53.090' AS DateTime))
INSERT [dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien], [IdChiTietHoaDon],[IdHoaDon], [IdSanPham], [TenMon], [SoBan], [SoLuong], [GhiChu], [NhomIn], [TrangThai], [ThoiGianGoi],[ThoiGianBatDau], [ThoiGianHoanThanh]) VALUES (8, 13, 10, 1, N'Cafe đen', N'T1-B1', 1, NULL, N'Pha chế', N'Hoàn thành', CAST(N'2026-04-19T05:50:19.277' AS DateTime), CAST(N'2026-04-19T06:00:10.473' AS DateTime), CAST(N'2026-04-19T06:00:10.687' AS DateTime))
INSERT [dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien], [IdChiTietHoaDon], [IdHoaDon], [IdSanPham], [TenMon], [SoBan], [SoLuong],[GhiChu], [NhomIn], [TrangThai], [ThoiGianGoi],[ThoiGianBatDau], [ThoiGianHoanThanh]) VALUES (9, 14, 11, 1, N'Cafe đen', N'T1-B1', 1, NULL, N'Pha chế', N'Hoàn thành', CAST(N'2026-04-19T06:16:26.927' AS DateTime), CAST(N'2026-04-19T06:18:03.573' AS DateTime), CAST(N'2026-04-19T06:18:04.090' AS DateTime))
INSERT [dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien], [IdChiTietHoaDon], [IdHoaDon], [IdSanPham], [TenMon],[SoBan], [SoLuong], [GhiChu], [NhomIn], [TrangThai],[ThoiGianGoi], [ThoiGianBatDau], [ThoiGianHoanThanh]) VALUES (10, 15, 12, 1, N'Cafe đen', N'Tại quán', 1, NULL, N'Pha chế', N'Hoàn thành', CAST(N'2026-04-19T06:47:57.730' AS DateTime), CAST(N'2026-04-19T08:04:38.300' AS DateTime), CAST(N'2026-04-19T08:04:39.143' AS DateTime))
INSERT [dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien], [IdChiTietHoaDon], [IdHoaDon],[IdSanPham], [TenMon], [SoBan], [SoLuong], [GhiChu], [NhomIn],[TrangThai], [ThoiGianGoi], [ThoiGianBatDau],[ThoiGianHoanThanh]) VALUES (11, 16, 13, 1, N'Cafe đen', N'Tại quán', 1, NULL, N'Pha chế', N'Hoàn thành', CAST(N'2026-04-19T06:56:15.613' AS DateTime), CAST(N'2026-04-19T08:04:39.667' AS DateTime), CAST(N'2026-04-19T08:04:40.050' AS DateTime))
INSERT[dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien], [IdChiTietHoaDon], [IdHoaDon],[IdSanPham], [TenMon], [SoBan], [SoLuong], [GhiChu], [NhomIn],[TrangThai], [ThoiGianGoi], [ThoiGianBatDau],[ThoiGianHoanThanh]) VALUES (12, 17, 14, 1, N'Cafe đen', N'T1-B1', 1, NULL, N'Pha chế', N'Hoàn thành', CAST(N'2026-04-19T16:27:25.577' AS DateTime), CAST(N'2026-04-19T16:30:31.187' AS DateTime), CAST(N'2026-04-19T16:30:31.623' AS DateTime))
INSERT [dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien],[IdChiTietHoaDon], [IdHoaDon], [IdSanPham], [TenMon], [SoBan], [SoLuong],[GhiChu], [NhomIn], [TrangThai], [ThoiGianGoi],[ThoiGianBatDau], [ThoiGianHoanThanh]) VALUES (13, 18, 15, 1, N'Cafe đen', N'T2-B1', 1, N'a', N'Pha chế', N'Hoàn thành', CAST(N'2026-04-20T02:44:02.517' AS DateTime), CAST(N'2026-04-20T03:28:13.837' AS DateTime), CAST(N'2026-04-20T03:28:14.103' AS DateTime))
INSERT [dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien], [IdChiTietHoaDon], [IdHoaDon], [IdSanPham],[TenMon], [SoBan], [SoLuong], [GhiChu], [NhomIn],[TrangThai], [ThoiGianGoi], [ThoiGianBatDau], [ThoiGianHoanThanh]) VALUES (14, 19, 18, 1, N'Cafe đen', N'T1-B1', 1, NULL, N'Pha chế', N'Hoàn thành', CAST(N'2026-04-20T05:30:00.960' AS DateTime), CAST(N'2026-04-20T14:00:34.210' AS DateTime), CAST(N'2026-04-20T14:00:35.320' AS DateTime))
INSERT [dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien], [IdChiTietHoaDon], [IdHoaDon],[IdSanPham], [TenMon], [SoBan], [SoLuong], [GhiChu],[NhomIn], [TrangThai], [ThoiGianGoi], [ThoiGianBatDau],[ThoiGianHoanThanh]) VALUES (15, 20, 18, 1, N'Cafe đen', N'T2-B1', 1, NULL, N'Pha chế', N'Hoàn thành', CAST(N'2026-04-20T14:15:14.353' AS DateTime), CAST(N'2026-04-20T14:19:54.940' AS DateTime), CAST(N'2026-04-20T14:19:55.307' AS DateTime))
INSERT [dbo].[TrangThaiCheBien] ([IdTrangThaiCheBien],[IdChiTietHoaDon], [IdHoaDon], [IdSanPham], [TenMon], [SoBan],[SoLuong], [GhiChu], [NhomIn], [TrangThai], [ThoiGianGoi],[ThoiGianBatDau], [ThoiGianHoanThanh]) VALUES (16, 21, 18, 1, N'Cafe đen', N'T1-B1', 1, NULL, N'Pha chế', N'Hoàn thành', CAST(N'2026-04-20T14:20:20.517' AS DateTime), CAST(N'2026-04-20T14:20:34.673' AS DateTime), CAST(N'2026-04-20T14:20:35.697' AS DateTime))
SET IDENTITY_INSERT [dbo].[TrangThaiCheBien] OFF
GO

SET IDENTITY_INSERT [dbo].[VaiTro] ON 
INSERT [dbo].[VaiTro] ([idVaiTro], [tenVaiTro], [moTa]) VALUES (1, N'Quản lý', N'Quản lý toàn bộ hệ thống')
INSERT [dbo].[VaiTro] ([idVaiTro], [tenVaiTro],[moTa]) VALUES (2, N'Nhân viên', N'Nhân viên thao tác nghiệp vụ')
SET IDENTITY_INSERT [dbo].[VaiTro] OFF
GO


/****** CHỈ MỤC & CÁC RÀNG BUỘC (INDEXES & CONSTRAINTS) ******/

SET ANSI_PADDING ON
GO

CREATE NONCLUSTERED INDEX[IX_GiaoDich_MaGiaoDichNgoai] ON [dbo].[GiaoDichThanhToan] ([MaGiaoDichNgoai] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

ALTER TABLE [dbo].[KhachHang] ADD UNIQUE NONCLUSTERED ([soDienThoai] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

ALTER TABLE [dbo].[KhachHang] ADD UNIQUE NONCLUSTERED ([tenDangNhap] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

ALTER TABLE [dbo].[KhachHang] ADD UNIQUE NONCLUSTERED ([email] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

ALTER TABLE [dbo].[KhuyenMai] ADD UNIQUE NONCLUSTERED ([maKhuyenMai] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

ALTER TABLE [dbo].[NguoiGiaoHang] ADD UNIQUE NONCLUSTERED ([SoDienThoai] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

ALTER TABLE [dbo].[NhanVien] ADD UNIQUE NONCLUSTERED ([soDienThoai] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

ALTER TABLE [dbo].[NhanVien] ADD UNIQUE NONCLUSTERED ([tenDangNhap] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

ALTER TABLE [dbo].[NhanVien] ADD UNIQUE NONCLUSTERED ([email] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [UQ_NhanVien_Email] ON [dbo].[NhanVien] ([email] ASC)
WHERE ([Email] IS NOT NULL AND [Email]<>'')
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
GO

ALTER TABLE [dbo].[PhieuLuong] ADD UNIQUE NONCLUSTERED ([idNhanVien] ASC, [thang] ASC, [nam] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX[IX_TrangThaiCheBien_TrangThai] ON [dbo].[TrangThaiCheBien] ([TrangThai] ASC, [NhomIn] ASC)
INCLUDE([ThoiGianGoi]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

ALTER TABLE [dbo].[VaiTro] ADD UNIQUE NONCLUSTERED ([tenVaiTro] ASC)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
GO

ALTER TABLE [dbo].[Ban] ADD  DEFAULT ((2)) FOR [soGhe]
GO
ALTER TABLE [dbo].[Ban] ADD  DEFAULT (N'Trống') FOR[trangThai]
GO
ALTER TABLE [dbo].[ChatLichSu] ADD  DEFAULT (getdate()) FOR[ThoiGian]
GO
ALTER TABLE [dbo].[ChiTietPhieuThue] ADD  DEFAULT ((0)) FOR [TienPhatTraTre]
GO
ALTER TABLE [dbo].[ChiTietPhieuThue] ADD  DEFAULT ((100)) FOR [DoMoiKhiThue]
GO
ALTER TABLE [dbo].[ChiTietPhieuTra] ADD  DEFAULT ((0)) FOR[TienPhatHuHong]
GO
ALTER TABLE [dbo].[DanhGia] ADD  DEFAULT (getdate()) FOR[NgayTao]
GO
ALTER TABLE [dbo].[DanhGia] ADD  DEFAULT (N'Hiển thị') FOR [TrangThai]
GO
ALTER TABLE [dbo].[DeXuatSach] ADD  DEFAULT ((0)) FOR [DoLienQuan]
GO
ALTER TABLE [dbo].[DeXuatSanPham] ADD  DEFAULT ((0)) FOR [DoLienQuan]
GO
ALTER TABLE [dbo].[DonViChuyenDoi] ADD  DEFAULT ((0)) FOR [LaDonViCoBan]
GO
ALTER TABLE [dbo].[DonXinNghi] ADD  DEFAULT (N'Chờ duyệt') FOR [TrangThai]
GO
ALTER TABLE [dbo].[GiaoDichThanhToan] ADD  DEFAULT (getdate()) FOR[ThoiGianGiaoDich]
GO
ALTER TABLE [dbo].[HoaDon] ADD  DEFAULT (getdate()) FOR [thoiGianTao]
GO
ALTER TABLE [dbo].[HoaDon] ADD  DEFAULT (N'Chưa thanh toán') FOR [trangThai]
GO
ALTER TABLE [dbo].[HoaDon] ADD  DEFAULT ((0)) FOR [tongTienGoc]
GO
ALTER TABLE [dbo].[HoaDon] ADD  DEFAULT ((0)) FOR [giamGia]
GO
ALTER TABLE [dbo].[HoaDon] ADD  DEFAULT ((0)) FOR [TongPhuThu]
GO
ALTER TABLE [dbo].[HoaDon] ADD  DEFAULT (N'Tại quán') FOR [LoaiHoaDon]
GO
ALTER TABLE [dbo].[KhachHang] ADD  DEFAULT ((0)) FOR [diemTichLuy]
GO
ALTER TABLE [dbo].[KhachHang] ADD  DEFAULT (getdate()) FOR [ngayTao]
GO
ALTER TABLE [dbo].[KhachHang] ADD  DEFAULT ((0)) FOR [BiKhoa]
GO
ALTER TABLE [dbo].[KhachHang] ADD  DEFAULT ((1)) FOR [taiKhoanTam]
GO
ALTER TABLE [dbo].[KhachHang] ADD  DEFAULT ((0)) FOR [DaXoa]
GO
ALTER TABLE [dbo].[KhuyenMai] ADD  DEFAULT (N'Hoạt động') FOR [TrangThai]
GO
ALTER TABLE[dbo].[LichLamViec] ADD  DEFAULT (N'Đã duyệt') FOR [trangThai]
GO
ALTER TABLE [dbo].[NguoiGiaoHang] ADD  DEFAULT (N'Sẵn sàng') FOR [TrangThai]
GO
ALTER TABLE [dbo].[NguyenLieu] ADD  DEFAULT ((0)) FOR [tonKho]
GO
ALTER TABLE [dbo].[NguyenLieu] ADD  DEFAULT ((0)) FOR [TonKhoToiThieu]
GO
ALTER TABLE [dbo].[NhanVien] ADD  DEFAULT ((0)) FOR [luongCoBan]
GO
ALTER TABLE [dbo].[NhanVien] ADD  DEFAULT (N'Đang làm việc') FOR [trangThaiLamViec]
GO
ALTER TABLE [dbo].[NhatKyHeThong] ADD  DEFAULT (getdate()) FOR [ThoiGian]
GO
ALTER TABLE [dbo].[NhatKyHuyMon] ADD  DEFAULT (getdate()) FOR [ThoiGianHuy]
GO
ALTER TABLE [dbo].[NhuCauCaLam] ADD  DEFAULT ((1)) FOR [soLuongCan]
GO
ALTER TABLE [dbo].[NhuCauCaLam] ADD  DEFAULT (N'Tất cả') FOR [loaiYeuCau]
GO
ALTER TABLE [dbo].[PhanHoiDanhGia] ADD  DEFAULT (getdate()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[PhieuDatBan] ADD  DEFAULT (N'Đã xác nhận') FOR [trangThai]
GO
ALTER TABLE [dbo].[PhieuKiemKho] ADD  DEFAULT (getdate()) FOR [NgayKiem]
GO
ALTER TABLE [dbo].[PhieuKiemKho] ADD  DEFAULT (N'Đang kiểm') FOR [TrangThai]
GO
ALTER TABLE [dbo].[PhieuLuong] ADD  DEFAULT ((0)) FOR [tienThuong]
GO
ALTER TABLE [dbo].[PhieuLuong] ADD  DEFAULT ((0)) FOR [khauTru]
GO
ALTER TABLE [dbo].[PhieuLuong] ADD  DEFAULT (getdate()) FOR [ngayTao]
GO
ALTER TABLE [dbo].[PhieuLuong] ADD  DEFAULT (N'Chưa thanh toán') FOR[trangThai]
GO
ALTER TABLE [dbo].[PhieuNhapKho] ADD  DEFAULT (getdate()) FOR [ngayNhap]
GO
ALTER TABLE [dbo].[PhieuNhapKho] ADD  DEFAULT ((0)) FOR [tongTien]
GO
ALTER TABLE [dbo].[PhieuNhapKho] ADD  DEFAULT (N'Đã hoàn thành') FOR [TrangThai]
GO
ALTER TABLE [dbo].[PhieuThueSach] ADD  DEFAULT (getdate()) FOR [ngayThue]
GO
ALTER TABLE [dbo].[PhieuThueSach] ADD  DEFAULT (N'Đang thuê') FOR [trangThai]
GO
ALTER TABLE [dbo].[PhieuThueSach] ADD  DEFAULT ((0)) FOR [tongTienCoc]
GO
ALTER TABLE [dbo].[PhieuThuongPhat] ADD  DEFAULT (getdate()) FOR [NgayTao]
GO
ALTER TABLE [dbo].[PhieuXuatHuy] ADD  DEFAULT (getdate()) FOR[NgayXuatHuy]
GO
ALTER TABLE [dbo].[PhieuXuatHuy] ADD  DEFAULT ((0)) FOR [TongGiaTriHuy]
GO
ALTER TABLE [dbo].[PhuThu] ADD  DEFAULT ('VND') FOR [LoaiGiaTri]
GO
ALTER TABLE [dbo].[Sach] ADD  DEFAULT ((1)) FOR [soLuongTong]
GO
ALTER TABLE [dbo].[Sach] ADD  DEFAULT ((1)) FOR[soLuongHienCo]
GO
ALTER TABLE [dbo].[Sach] ADD  DEFAULT ((0)) FOR[GiaBia]
GO
ALTER TABLE [dbo].[SanPham] ADD  DEFAULT ((1)) FOR[trangThaiKinhDoanh]
GO
ALTER TABLE [dbo].[ThongBao] ADD  DEFAULT (getdate()) FOR [ThoiGianTao]
GO
ALTER TABLE [dbo].[ThongBao] ADD  DEFAULT ((0)) FOR [DaXem]
GO
ALTER TABLE [dbo].[ThongBaoHoTro] ADD  DEFAULT (getdate()) FOR [ThoiGianTao]
GO
ALTER TABLE [dbo].[ThongBaoHoTro] ADD  DEFAULT (N'Chờ xử lý') FOR [TrangThai]
GO
ALTER TABLE [dbo].[TrangThaiCheBien] ADD  DEFAULT (N'Chờ làm') FOR [TrangThai]
GO
ALTER TABLE [dbo].[TrangThaiCheBien] ADD  DEFAULT (getdate()) FOR[ThoiGianGoi]
GO

/****** FOREIGN KEY CONSTRAINTS ******/

ALTER TABLE [dbo].[Ban]  WITH CHECK ADD  CONSTRAINT[FK_Ban_KhuVuc] FOREIGN KEY([idKhuVuc]) REFERENCES [dbo].[KhuVuc] ([idKhuVuc])
GO
ALTER TABLE [dbo].[Ban] CHECK CONSTRAINT[FK_Ban_KhuVuc]
GO
ALTER TABLE [dbo].[BangChamCong]  WITH CHECK ADD  CONSTRAINT[FK_ChamCong_LichLamViec] FOREIGN KEY([idLichLamViec]) REFERENCES[dbo].[LichLamViec] ([idLichLamViec]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[BangChamCong] CHECK CONSTRAINT [FK_ChamCong_LichLamViec]
GO
ALTER TABLE [dbo].[ChatLichSu]  WITH CHECK ADD  CONSTRAINT [FK_ChatLichSu_KhachHang] FOREIGN KEY([idKhachHang]) REFERENCES [dbo].[KhachHang] ([idKhachHang])
GO
ALTER TABLE[dbo].[ChatLichSu] CHECK CONSTRAINT [FK_ChatLichSu_KhachHang]
GO
ALTER TABLE [dbo].[ChatLichSu]  WITH CHECK ADD  CONSTRAINT [FK_ChatLichSu_NhanVien] FOREIGN KEY([idNhanVien]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[ChatLichSu] CHECK CONSTRAINT[FK_ChatLichSu_NhanVien]
GO
ALTER TABLE [dbo].[ChatLichSu]  WITH CHECK ADD  CONSTRAINT [FK_ChatLichSu_ThongBaoHoTro] FOREIGN KEY([IdThongBaoHoTro]) REFERENCES [dbo].[ThongBaoHoTro] ([IdThongBao])
GO
ALTER TABLE [dbo].[ChatLichSu] CHECK CONSTRAINT[FK_ChatLichSu_ThongBaoHoTro]
GO
ALTER TABLE [dbo].[ChiTietHoaDon]  WITH CHECK ADD  CONSTRAINT[FK_ChiTietHoaDon_HoaDon] FOREIGN KEY([idHoaDon]) REFERENCES [dbo].[HoaDon] ([idHoaDon]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ChiTietHoaDon] CHECK CONSTRAINT [FK_ChiTietHoaDon_HoaDon]
GO
ALTER TABLE [dbo].[ChiTietHoaDon]  WITH CHECK ADD  CONSTRAINT [FK_ChiTietHoaDon_SanPham] FOREIGN KEY([idSanPham]) REFERENCES [dbo].[SanPham] ([idSanPham])
GO
ALTER TABLE [dbo].[ChiTietHoaDon] CHECK CONSTRAINT [FK_ChiTietHoaDon_SanPham]
GO
ALTER TABLE [dbo].[ChiTietKiemKho]  WITH CHECK ADD  CONSTRAINT [FK_CTKK_NguyenLieu] FOREIGN KEY([idNguyenLieu]) REFERENCES [dbo].[NguyenLieu] ([idNguyenLieu])
GO
ALTER TABLE [dbo].[ChiTietKiemKho] CHECK CONSTRAINT [FK_CTKK_NguyenLieu]
GO
ALTER TABLE [dbo].[ChiTietKiemKho]  WITH CHECK ADD  CONSTRAINT[FK_CTKK_PhieuKiemKho] FOREIGN KEY([idPhieuKiemKho]) REFERENCES [dbo].[PhieuKiemKho] ([idPhieuKiemKho]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ChiTietKiemKho] CHECK CONSTRAINT [FK_CTKK_PhieuKiemKho]
GO
ALTER TABLE [dbo].[ChiTietNhapKho]  WITH CHECK ADD  CONSTRAINT [FK_ChiTietNhapKho_NguyenLieu] FOREIGN KEY([idNguyenLieu]) REFERENCES [dbo].[NguyenLieu] ([idNguyenLieu])
GO
ALTER TABLE [dbo].[ChiTietNhapKho] CHECK CONSTRAINT [FK_ChiTietNhapKho_NguyenLieu]
GO
ALTER TABLE [dbo].[ChiTietNhapKho]  WITH CHECK ADD  CONSTRAINT[FK_ChiTietNhapKho_PhieuNhapKho] FOREIGN KEY([idPhieuNhapKho]) REFERENCES [dbo].[PhieuNhapKho] ([idPhieuNhapKho]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ChiTietNhapKho] CHECK CONSTRAINT [FK_ChiTietNhapKho_PhieuNhapKho]
GO
ALTER TABLE [dbo].[ChiTietPhieuThue]  WITH CHECK ADD  CONSTRAINT[FK_ChiTietThue_Phieu] FOREIGN KEY([idPhieuThueSach]) REFERENCES [dbo].[PhieuThueSach] ([idPhieuThueSach]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ChiTietPhieuThue] CHECK CONSTRAINT [FK_ChiTietThue_Phieu]
GO
ALTER TABLE [dbo].[ChiTietPhieuThue]  WITH CHECK ADD  CONSTRAINT[FK_ChiTietThue_Sach] FOREIGN KEY([idSach]) REFERENCES [dbo].[Sach] ([idSach])
GO
ALTER TABLE [dbo].[ChiTietPhieuThue] CHECK CONSTRAINT[FK_ChiTietThue_Sach]
GO
ALTER TABLE [dbo].[ChiTietPhieuTra]  WITH CHECK ADD FOREIGN KEY([IdPhieuTra]) REFERENCES [dbo].[PhieuTraSach] ([IdPhieuTra])
GO
ALTER TABLE [dbo].[ChiTietPhieuTra]  WITH CHECK ADD FOREIGN KEY([IdSach]) REFERENCES [dbo].[Sach] ([idSach])
GO
ALTER TABLE [dbo].[ChiTietPhuThuHoaDon]  WITH CHECK ADD  CONSTRAINT [FK_CTPT_HoaDon] FOREIGN KEY([idHoaDon]) REFERENCES[dbo].[HoaDon] ([idHoaDon]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ChiTietPhuThuHoaDon] CHECK CONSTRAINT [FK_CTPT_HoaDon]
GO
ALTER TABLE [dbo].[ChiTietPhuThuHoaDon]  WITH CHECK ADD  CONSTRAINT[FK_CTPT_PhuThu] FOREIGN KEY([idPhuThu]) REFERENCES [dbo].[PhuThu] ([idPhuThu])
GO
ALTER TABLE [dbo].[ChiTietPhuThuHoaDon] CHECK CONSTRAINT [FK_CTPT_PhuThu]
GO
ALTER TABLE [dbo].[ChiTietXuatHuy]  WITH NOCHECK ADD  CONSTRAINT[FK_CTXH_NguyenLieu] FOREIGN KEY([idNguyenLieu]) REFERENCES [dbo].[NguyenLieu] ([idNguyenLieu])
GO
ALTER TABLE [dbo].[ChiTietXuatHuy] CHECK CONSTRAINT [FK_CTXH_NguyenLieu]
GO
ALTER TABLE [dbo].[ChiTietXuatHuy]  WITH NOCHECK ADD  CONSTRAINT[FK_CTXH_PhieuXuatHuy] FOREIGN KEY([idPhieuXuatHuy]) REFERENCES [dbo].[PhieuXuatHuy] ([idPhieuXuatHuy]) ON DELETE CASCADE
GO
ALTER TABLE[dbo].[ChiTietXuatHuy] CHECK CONSTRAINT[FK_CTXH_PhieuXuatHuy]
GO
ALTER TABLE [dbo].[DanhGia]  WITH CHECK ADD  CONSTRAINT[FK_DanhGia_HoaDon] FOREIGN KEY([idHoaDon]) REFERENCES [dbo].[HoaDon] ([idHoaDon])
GO
ALTER TABLE [dbo].[DanhGia] CHECK CONSTRAINT [FK_DanhGia_HoaDon]
GO
ALTER TABLE [dbo].[DanhGia]  WITH CHECK ADD  CONSTRAINT[FK_DanhGia_KhachHang] FOREIGN KEY([idKhachHang]) REFERENCES [dbo].[KhachHang] ([idKhachHang])
GO
ALTER TABLE [dbo].[DanhGia] CHECK CONSTRAINT[FK_DanhGia_KhachHang]
GO
ALTER TABLE [dbo].[DanhGia]  WITH CHECK ADD  CONSTRAINT[FK_DanhGia_SanPham] FOREIGN KEY([idSanPham]) REFERENCES [dbo].[SanPham] ([idSanPham])
GO
ALTER TABLE [dbo].[DanhGia] CHECK CONSTRAINT [FK_DanhGia_SanPham]
GO
ALTER TABLE [dbo].[DanhMuc]  WITH CHECK ADD  CONSTRAINT [FK_DanhMuc_DanhMucCha] FOREIGN KEY([idDanhMucCha]) REFERENCES [dbo].[DanhMuc] ([idDanhMuc])
GO
ALTER TABLE [dbo].[DanhMuc] CHECK CONSTRAINT [FK_DanhMuc_DanhMucCha]
GO
ALTER TABLE [dbo].[DeXuatSach]  WITH CHECK ADD  CONSTRAINT [FK_DeXuatSach_DeXuat] FOREIGN KEY([idSachDeXuat]) REFERENCES [dbo].[Sach] ([idSach])
GO
ALTER TABLE [dbo].[DeXuatSach] CHECK CONSTRAINT [FK_DeXuatSach_DeXuat]
GO
ALTER TABLE [dbo].[DeXuatSach]  WITH CHECK ADD  CONSTRAINT [FK_DeXuatSach_Goc] FOREIGN KEY([idSachGoc]) REFERENCES [dbo].[Sach] ([idSach])
GO
ALTER TABLE [dbo].[DeXuatSach] CHECK CONSTRAINT [FK_DeXuatSach_Goc]
GO
ALTER TABLE [dbo].[DeXuatSanPham]  WITH CHECK ADD  CONSTRAINT [FK_DeXuatSP_DeXuat] FOREIGN KEY([idSanPhamDeXuat]) REFERENCES [dbo].[SanPham] ([idSanPham])
GO
ALTER TABLE [dbo].[DeXuatSanPham] CHECK CONSTRAINT [FK_DeXuatSP_DeXuat]
GO
ALTER TABLE[dbo].[DeXuatSanPham]  WITH CHECK ADD  CONSTRAINT [FK_DeXuatSP_Goc] FOREIGN KEY([idSanPhamGoc]) REFERENCES [dbo].[SanPham] ([idSanPham])
GO
ALTER TABLE [dbo].[DeXuatSanPham] CHECK CONSTRAINT [FK_DeXuatSP_Goc]
GO
ALTER TABLE [dbo].[DinhLuong]  WITH CHECK ADD  CONSTRAINT [FK_DinhLuong_DonViChuyenDoi] FOREIGN KEY([idDonViSuDung]) REFERENCES [dbo].[DonViChuyenDoi] ([idChuyenDoi])
GO
ALTER TABLE [dbo].[DinhLuong] CHECK CONSTRAINT[FK_DinhLuong_DonViChuyenDoi]
GO
ALTER TABLE [dbo].[DinhLuong]  WITH CHECK ADD  CONSTRAINT[FK_DinhLuong_NguyenLieu] FOREIGN KEY([idNguyenLieu]) REFERENCES [dbo].[NguyenLieu] ([idNguyenLieu])
GO
ALTER TABLE [dbo].[DinhLuong] CHECK CONSTRAINT[FK_DinhLuong_NguyenLieu]
GO
ALTER TABLE [dbo].[DinhLuong]  WITH CHECK ADD  CONSTRAINT[FK_DinhLuong_SanPham] FOREIGN KEY([idSanPham]) REFERENCES [dbo].[SanPham] ([idSanPham]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DinhLuong] CHECK CONSTRAINT [FK_DinhLuong_SanPham]
GO
ALTER TABLE [dbo].[DonViChuyenDoi]  WITH CHECK ADD  CONSTRAINT[FK_DonViChuyenDoi_NguyenLieu] FOREIGN KEY([idNguyenLieu]) REFERENCES [dbo].[NguyenLieu] ([idNguyenLieu]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DonViChuyenDoi] CHECK CONSTRAINT[FK_DonViChuyenDoi_NguyenLieu]
GO
ALTER TABLE [dbo].[DonXinNghi]  WITH CHECK ADD  CONSTRAINT [FK_DonXinNghi_NguoiDuyet] FOREIGN KEY([idNguoiDuyet]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[DonXinNghi] CHECK CONSTRAINT [FK_DonXinNghi_NguoiDuyet]
GO
ALTER TABLE [dbo].[DonXinNghi]  WITH CHECK ADD  CONSTRAINT[FK_DonXinNghi_NhanVien] FOREIGN KEY([idNhanVien]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[DonXinNghi] CHECK CONSTRAINT[FK_DonXinNghi_NhanVien]
GO
ALTER TABLE [dbo].[GiaoDichThanhToan]  WITH CHECK ADD  CONSTRAINT [FK_GiaoDich_HoaDon] FOREIGN KEY([idHoaDon]) REFERENCES[dbo].[HoaDon] ([idHoaDon])
GO
ALTER TABLE [dbo].[GiaoDichThanhToan] CHECK CONSTRAINT [FK_GiaoDich_HoaDon]
GO
ALTER TABLE [dbo].[HoaDon]  WITH CHECK ADD  CONSTRAINT [FK_HoaDon_Ban] FOREIGN KEY([idBan]) REFERENCES [dbo].[Ban] ([idBan])
GO
ALTER TABLE [dbo].[HoaDon] CHECK CONSTRAINT [FK_HoaDon_Ban]
GO
ALTER TABLE [dbo].[HoaDon]  WITH CHECK ADD  CONSTRAINT[FK_HoaDon_KhachHang] FOREIGN KEY([idKhachHang]) REFERENCES [dbo].[KhachHang] ([idKhachHang])
GO
ALTER TABLE [dbo].[HoaDon] CHECK CONSTRAINT [FK_HoaDon_KhachHang]
GO
ALTER TABLE [dbo].[HoaDon]  WITH CHECK ADD  CONSTRAINT [FK_HoaDon_NguoiGiaoHang] FOREIGN KEY([idNguoiGiaoHang]) REFERENCES [dbo].[NguoiGiaoHang] ([idNguoiGiaoHang])
GO
ALTER TABLE [dbo].[HoaDon] CHECK CONSTRAINT [FK_HoaDon_NguoiGiaoHang]
GO
ALTER TABLE [dbo].[HoaDon]  WITH CHECK ADD  CONSTRAINT [FK_HoaDon_NhanVien] FOREIGN KEY([idNhanVien]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[HoaDon] CHECK CONSTRAINT [FK_HoaDon_NhanVien]
GO
ALTER TABLE [dbo].[HoaDon_KhuyenMai]  WITH NOCHECK ADD  CONSTRAINT[FK_HDKM_HoaDon] FOREIGN KEY([idHoaDon]) REFERENCES [dbo].[HoaDon] ([idHoaDon])
GO
ALTER TABLE [dbo].[HoaDon_KhuyenMai] CHECK CONSTRAINT[FK_HDKM_HoaDon]
GO
ALTER TABLE [dbo].[HoaDon_KhuyenMai]  WITH NOCHECK ADD  CONSTRAINT [FK_HDKM_KhuyenMai] FOREIGN KEY([idKhuyenMai]) REFERENCES [dbo].[KhuyenMai] ([idKhuyenMai])
GO
ALTER TABLE [dbo].[HoaDon_KhuyenMai] CHECK CONSTRAINT [FK_HDKM_KhuyenMai]
GO
ALTER TABLE [dbo].[KhuyenMai]  WITH CHECK ADD  CONSTRAINT [FK_KhuyenMai_SanPham] FOREIGN KEY([IdSanPhamApDung]) REFERENCES [dbo].[SanPham] ([idSanPham])
GO
ALTER TABLE [dbo].[KhuyenMai] CHECK CONSTRAINT[FK_KhuyenMai_SanPham]
GO
ALTER TABLE [dbo].[LichLamViec]  WITH CHECK ADD  CONSTRAINT [FK_LichLamViec_CaLamViec] FOREIGN KEY([idCa]) REFERENCES [dbo].[CaLamViec] ([idCa])
GO
ALTER TABLE [dbo].[LichLamViec] CHECK CONSTRAINT[FK_LichLamViec_CaLamViec]
GO
ALTER TABLE [dbo].[LichLamViec]  WITH CHECK ADD  CONSTRAINT [FK_LichLamViec_NhanVien] FOREIGN KEY([idNhanVien]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[LichLamViec] CHECK CONSTRAINT[FK_LichLamViec_NhanVien]
GO
ALTER TABLE [dbo].[NhanVien]  WITH CHECK ADD  CONSTRAINT[FK_NhanVien_VaiTro] FOREIGN KEY([idVaiTro]) REFERENCES [dbo].[VaiTro] ([idVaiTro])
GO
ALTER TABLE [dbo].[NhanVien] CHECK CONSTRAINT[FK_NhanVien_VaiTro]
GO
ALTER TABLE [dbo].[NhanVien_Quyen]  WITH CHECK ADD  CONSTRAINT [FK_NVQ_NhanVien] FOREIGN KEY([IdNhanVien]) REFERENCES [dbo].[NhanVien] ([idNhanVien]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NhanVien_Quyen] CHECK CONSTRAINT [FK_NVQ_NhanVien]
GO
ALTER TABLE[dbo].[NhanVien_Quyen]  WITH CHECK ADD  CONSTRAINT [FK_NVQ_Quyen] FOREIGN KEY([IdQuyen]) REFERENCES [dbo].[Quyen] ([idQuyen]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[NhanVien_Quyen] CHECK CONSTRAINT [FK_NVQ_Quyen]
GO
ALTER TABLE[dbo].[NhatKyHeThong]  WITH CHECK ADD  CONSTRAINT[FK_NhatKy_NhanVien] FOREIGN KEY([IdNhanVien]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[NhatKyHeThong] CHECK CONSTRAINT[FK_NhatKy_NhanVien]
GO
ALTER TABLE [dbo].[NhatKyHuyMon]  WITH CHECK ADD  CONSTRAINT [FK_NhatKyHuyMon_HoaDon] FOREIGN KEY([idHoaDon]) REFERENCES [dbo].[HoaDon] ([idHoaDon])
GO
ALTER TABLE [dbo].[NhatKyHuyMon] CHECK CONSTRAINT [FK_NhatKyHuyMon_HoaDon]
GO
ALTER TABLE [dbo].[NhatKyHuyMon]  WITH CHECK ADD  CONSTRAINT[FK_NhatKyHuyMon_NhanVien] FOREIGN KEY([idNhanVienHuy]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[NhatKyHuyMon] CHECK CONSTRAINT[FK_NhatKyHuyMon_NhanVien]
GO
ALTER TABLE [dbo].[NhatKyHuyMon]  WITH CHECK ADD  CONSTRAINT [FK_NhatKyHuyMon_SanPham] FOREIGN KEY([idSanPham]) REFERENCES [dbo].[SanPham] ([idSanPham])
GO
ALTER TABLE [dbo].[NhatKyHuyMon] CHECK CONSTRAINT [FK_NhatKyHuyMon_SanPham]
GO
ALTER TABLE [dbo].[NhuCauCaLam]  WITH CHECK ADD FOREIGN KEY([idVaiTro]) REFERENCES [dbo].[VaiTro] ([idVaiTro])
GO
ALTER TABLE [dbo].[NhuCauCaLam]  WITH CHECK ADD FOREIGN KEY([idCa]) REFERENCES [dbo].[CaLamViec] ([idCa])
GO
ALTER TABLE [dbo].[PhanHoiDanhGia]  WITH CHECK ADD  CONSTRAINT[FK_PhanHoiDanhGia_DanhGia] FOREIGN KEY([idDanhGia]) REFERENCES [dbo].[DanhGia] ([idDanhGia]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PhanHoiDanhGia] CHECK CONSTRAINT [FK_PhanHoiDanhGia_DanhGia]
GO
ALTER TABLE [dbo].[PhanHoiDanhGia]  WITH CHECK ADD  CONSTRAINT[FK_PhanHoiDanhGia_NhanVien] FOREIGN KEY([idNhanVien]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[PhanHoiDanhGia] CHECK CONSTRAINT [FK_PhanHoiDanhGia_NhanVien]
GO
ALTER TABLE [dbo].[PhieuDatBan]  WITH NOCHECK ADD  CONSTRAINT[FK_PhieuDatBan_Ban] FOREIGN KEY([idBan]) REFERENCES [dbo].[Ban] ([idBan])
GO
ALTER TABLE [dbo].[PhieuDatBan] CHECK CONSTRAINT [FK_PhieuDatBan_Ban]
GO
ALTER TABLE [dbo].[PhieuDatBan]  WITH NOCHECK ADD  CONSTRAINT[FK_PhieuDatBan_KhachHang] FOREIGN KEY([idKhachHang]) REFERENCES [dbo].[KhachHang] ([idKhachHang])
GO
ALTER TABLE [dbo].[PhieuDatBan] CHECK CONSTRAINT[FK_PhieuDatBan_KhachHang]
GO
ALTER TABLE [dbo].[PhieuKiemKho]  WITH CHECK ADD  CONSTRAINT[FK_PKK_NhanVien] FOREIGN KEY([idNhanVienKiem]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[PhieuKiemKho] CHECK CONSTRAINT [FK_PKK_NhanVien]
GO
ALTER TABLE [dbo].[PhieuLuong]  WITH CHECK ADD  CONSTRAINT [FK_PhieuLuong_NguoiPhat] FOREIGN KEY([IdNguoiPhat]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[PhieuLuong] CHECK CONSTRAINT [FK_PhieuLuong_NguoiPhat]
GO
ALTER TABLE [dbo].[PhieuLuong]  WITH CHECK ADD  CONSTRAINT[FK_PhieuLuong_NhanVien] FOREIGN KEY([idNhanVien]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[PhieuLuong] CHECK CONSTRAINT[FK_PhieuLuong_NhanVien]
GO
ALTER TABLE [dbo].[PhieuNhapKho]  WITH CHECK ADD  CONSTRAINT[FK_PhieuNhapKho_NhaCungCap] FOREIGN KEY([idNhaCungCap]) REFERENCES [dbo].[NhaCungCap] ([idNhaCungCap])
GO
ALTER TABLE[dbo].[PhieuNhapKho] CHECK CONSTRAINT [FK_PhieuNhapKho_NhaCungCap]
GO
ALTER TABLE [dbo].[PhieuNhapKho]  WITH CHECK ADD  CONSTRAINT [FK_PhieuNhapKho_NhanVien] FOREIGN KEY([idNhanVien]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[PhieuNhapKho] CHECK CONSTRAINT[FK_PhieuNhapKho_NhanVien]
GO
ALTER TABLE [dbo].[PhieuThueSach]  WITH CHECK ADD  CONSTRAINT [FK_PhieuThueSach_KhachHang] FOREIGN KEY([idKhachHang]) REFERENCES [dbo].[KhachHang] ([idKhachHang])
GO
ALTER TABLE [dbo].[PhieuThueSach] CHECK CONSTRAINT[FK_PhieuThueSach_KhachHang]
GO
ALTER TABLE [dbo].[PhieuThueSach]  WITH CHECK ADD  CONSTRAINT [FK_PhieuThueSach_NhanVien] FOREIGN KEY([idNhanVien]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[PhieuThueSach] CHECK CONSTRAINT [FK_PhieuThueSach_NhanVien]
GO
ALTER TABLE [dbo].[PhieuThuongPhat]  WITH CHECK ADD  CONSTRAINT[FK_PhieuThuongPhat_NguoiTao] FOREIGN KEY([idNguoiTao]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[PhieuThuongPhat] CHECK CONSTRAINT [FK_PhieuThuongPhat_NguoiTao]
GO
ALTER TABLE [dbo].[PhieuThuongPhat]  WITH CHECK ADD  CONSTRAINT[FK_PhieuThuongPhat_NhanVien] FOREIGN KEY([idNhanVien]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[PhieuThuongPhat] CHECK CONSTRAINT [FK_PhieuThuongPhat_NhanVien]
GO
ALTER TABLE [dbo].[PhieuThuongPhat]  WITH CHECK ADD  CONSTRAINT[FK_PhieuThuongPhat_PhieuLuong] FOREIGN KEY([idPhieuLuong]) REFERENCES [dbo].[PhieuLuong] ([idPhieuLuong])
GO
ALTER TABLE [dbo].[PhieuThuongPhat] CHECK CONSTRAINT[FK_PhieuThuongPhat_PhieuLuong]
GO
ALTER TABLE [dbo].[PhieuTraSach]  WITH CHECK ADD FOREIGN KEY([IdNhanVien]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[PhieuTraSach]  WITH CHECK ADD FOREIGN KEY([IdPhieuThueSach]) REFERENCES [dbo].[PhieuThueSach] ([idPhieuThueSach])
GO
ALTER TABLE [dbo].[PhieuXuatHuy]  WITH CHECK ADD  CONSTRAINT[FK_PXH_NhanVien] FOREIGN KEY([idNhanVienXuat]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[PhieuXuatHuy] CHECK CONSTRAINT [FK_PXH_NhanVien]
GO
ALTER TABLE [dbo].[Sach_NhaXuatBan]  WITH CHECK ADD FOREIGN KEY([idNhaXuatBan]) REFERENCES [dbo].[NhaXuatBan] ([idNhaXuatBan])
GO
ALTER TABLE [dbo].[Sach_NhaXuatBan]  WITH CHECK ADD FOREIGN KEY([idSach]) REFERENCES [dbo].[Sach] ([idSach])
GO
ALTER TABLE [dbo].[Sach_TacGia]  WITH CHECK ADD FOREIGN KEY([idSach]) REFERENCES [dbo].[Sach] ([idSach])
GO
ALTER TABLE [dbo].[Sach_TacGia]  WITH CHECK ADD FOREIGN KEY([idTacGia]) REFERENCES [dbo].[TacGia] ([idTacGia])
GO
ALTER TABLE [dbo].[Sach_TheLoai]  WITH CHECK ADD FOREIGN KEY([idSach]) REFERENCES [dbo].[Sach] ([idSach])
GO
ALTER TABLE [dbo].[Sach_TheLoai]  WITH CHECK ADD FOREIGN KEY([idTheLoai]) REFERENCES [dbo].[TheLoai] ([idTheLoai])
GO
ALTER TABLE [dbo].[SanPham]  WITH CHECK ADD  CONSTRAINT [FK_SanPham_DanhMuc] FOREIGN KEY([idDanhMuc]) REFERENCES [dbo].[DanhMuc] ([idDanhMuc])
GO
ALTER TABLE [dbo].[SanPham] CHECK CONSTRAINT [FK_SanPham_DanhMuc]
GO
ALTER TABLE [dbo].[ThongBao]  WITH CHECK ADD  CONSTRAINT [FK_ThongBao_NhanVien] FOREIGN KEY([idNhanVienTao]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[ThongBao] CHECK CONSTRAINT [FK_ThongBao_NhanVien]
GO
ALTER TABLE [dbo].[ThongBaoHoTro]  WITH CHECK ADD  CONSTRAINT[FK_ThongBaoHoTro_KhachHang] FOREIGN KEY([IdKhachHang]) REFERENCES [dbo].[KhachHang] ([idKhachHang])
GO
ALTER TABLE [dbo].[ThongBaoHoTro] CHECK CONSTRAINT [FK_ThongBaoHoTro_KhachHang]
GO
ALTER TABLE [dbo].[ThongBaoHoTro]  WITH CHECK ADD  CONSTRAINT[FK_ThongBaoHoTro_NhanVien] FOREIGN KEY([IdNhanVien]) REFERENCES [dbo].[NhanVien] ([idNhanVien])
GO
ALTER TABLE [dbo].[ThongBaoHoTro] CHECK CONSTRAINT[FK_ThongBaoHoTro_NhanVien]
GO
ALTER TABLE [dbo].[TrangThaiCheBien]  WITH CHECK ADD  CONSTRAINT [FK_TrangThaiCheBien_ChiTietHoaDon] FOREIGN KEY([IdChiTietHoaDon]) REFERENCES [dbo].[ChiTietHoaDon] ([idChiTietHoaDon]) ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TrangThaiCheBien] CHECK CONSTRAINT[FK_TrangThaiCheBien_ChiTietHoaDon]
GO
ALTER TABLE [dbo].[TrangThaiCheBien]  WITH CHECK ADD  CONSTRAINT [FK_TrangThaiCheBien_HoaDon] FOREIGN KEY([IdHoaDon]) REFERENCES [dbo].[HoaDon] ([idHoaDon])
GO
ALTER TABLE[dbo].[TrangThaiCheBien] CHECK CONSTRAINT [FK_TrangThaiCheBien_HoaDon]
GO
ALTER TABLE [dbo].[TrangThaiCheBien]  WITH CHECK ADD  CONSTRAINT[FK_TrangThaiCheBien_SanPham] FOREIGN KEY([IdSanPham]) REFERENCES [dbo].[SanPham] ([idSanPham])
GO
ALTER TABLE [dbo].[TrangThaiCheBien] CHECK CONSTRAINT [FK_TrangThaiCheBien_SanPham]
GO

ALTER TABLE [dbo].[DanhGia]  WITH CHECK ADD  CONSTRAINT[CK_DanhGia_TrangThai] CHECK  (([TrangThai]=N'Đã ẩn' OR [TrangThai]=N'Hiển thị'))
GO
ALTER TABLE [dbo].[DanhGia] CHECK CONSTRAINT[CK_DanhGia_TrangThai]
GO

ALTER TABLE [dbo].[HoaDon]  WITH CHECK ADD CHECK  (([LoaiHoaDon]=N'Giao hàng' OR [LoaiHoaDon]=N'Mang về' OR[LoaiHoaDon]=N'Tại quán'))
GO

/****** HOÀN TẤT ******/