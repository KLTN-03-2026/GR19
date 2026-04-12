using CafebookModel.Model.ModelEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;
using System.Text.Json;

namespace CafebookApi.Data
{
    public class CafebookDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CafebookDbContext(DbContextOptions<CafebookDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        #region --- Khai báo các Bảng (DbSet) ---
        public DbSet<Ban> Bans { get; set; }
        public DbSet<BangChamCong> BangChamCongs { get; set; }
        public DbSet<CaiDat> CaiDats { get; set; }
        public DbSet<CaLamViec> CaLamViecs { get; set; }
        public DbSet<ChatLichSu> ChatLichSus { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public DbSet<ChiTietKiemKho> ChiTietKiemKhos { get; set; }
        public DbSet<ChiTietNhapKho> ChiTietNhapKhos { get; set; }
        public DbSet<ChiTietPhieuThue> ChiTietPhieuThues { get; set; }
        public DbSet<ChiTietPhieuTra> ChiTietPhieuTras { get; set; }
        public DbSet<ChiTietPhuThuHoaDon> ChiTietPhuThuHoaDons { get; set; }
        public DbSet<ChiTietXuatHuy> ChiTietXuatHuys { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<DeXuatSach> DeXuatSachs { get; set; }
        public DbSet<DeXuatSanPham> DeXuatSanPhams { get; set; }
        public DbSet<DinhLuong> DinhLuongs { get; set; }
        public DbSet<DonViChuyenDoi> DonViChuyenDois { get; set; }
        public DbSet<DonXinNghi> DonXinNghis { get; set; }
        public DbSet<GiaoDichThanhToan> GiaoDichThanhToans { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<HoaDon_KhuyenMai> HoaDonKhuyenMais { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<KhuVuc> KhuVucs { get; set; }
        public DbSet<KhuyenMai> KhuyenMais { get; set; }
        public DbSet<LichLamViec> LichLamViecs { get; set; }
        public DbSet<NguyenLieu> NguyenLieus { get; set; }
        public DbSet<NhaCungCap> NhaCungCaps { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<NhatKyHuyMon> NhatKyHuyMons { get; set; }
        public DbSet<NhaXuatBan> NhaXuatBans { get; set; }
        public DbSet<PhanHoiDanhGia> PhanHoiDanhGias { get; set; }
        public DbSet<PhieuDatBan> PhieuDatBans { get; set; }
        public DbSet<PhieuKiemKho> PhieuKiemKhos { get; set; }
        public DbSet<PhieuLuong> PhieuLuongs { get; set; }
        public DbSet<PhieuNhapKho> PhieuNhapKhos { get; set; }
        public DbSet<PhieuThueSach> PhieuThueSachs { get; set; }
        public DbSet<PhieuThuongPhat> PhieuThuongPhats { get; set; }
        public DbSet<PhieuTraSach> PhieuTraSachs { get; set; }
        public DbSet<PhieuXuatHuy> PhieuXuatHuys { get; set; }
        public DbSet<PhuThu> PhuThus { get; set; }
        public DbSet<Quyen> Quyens { get; set; }
        public DbSet<Sach> Sachs { get; set; }
        public DbSet<SachNhaXuatBan> SachNhaXuatBans { get; set; }
        public DbSet<SachTacGia> SachTacGias { get; set; }
        public DbSet<SachTheLoai> SachTheLoais { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<TacGia> TacGias { get; set; }
        public DbSet<TheLoai> TheLoais { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }
        public DbSet<ThongBaoHoTro> ThongBaoHoTros { get; set; }
        public DbSet<TrangThaiCheBien> TrangThaiCheBiens { get; set; }
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<NhuCauCaLam> NhuCauCaLams { get; set; }
        public DbSet<NguoiGiaoHang> NguoiGiaoHangs { get; set; }
        public virtual DbSet<NhanVien_Quyen> NhanVienQuyens { get; set; }
        public DbSet<NhatKyHeThong> NhatKyHeThongs { get; set; } // Bảng mới thêm
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình Khóa chính phức hợp cho các bảng trung gian
            //modelBuilder.Entity<NhanVien_Quyen>().HasKey(vq => new { vq.IdNhanVien, vq.IdQuyen });
            modelBuilder.Entity<HoaDon_KhuyenMai>().HasKey(hk => new { hk.IdHoaDon, hk.IdKhuyenMai });
            modelBuilder.Entity<DinhLuong>().HasKey(d => new { d.IdSanPham, d.IdNguyenLieu });
            modelBuilder.Entity<DeXuatSach>().HasKey(d => new { d.IdSachGoc, d.IdSachDeXuat, d.LoaiDeXuat });
            modelBuilder.Entity<DeXuatSanPham>().HasKey(d => new { d.IdSanPhamGoc, d.IdSanPhamDeXuat, d.LoaiDeXuat });
            modelBuilder.Entity<ChiTietPhuThuHoaDon>().HasKey(c => new { c.IdHoaDon, c.IdPhuThu });
            modelBuilder.Entity<ChiTietNhapKho>().HasKey(c => new { c.IdPhieuNhapKho, c.IdNguyenLieu });
            modelBuilder.Entity<ChiTietKiemKho>().HasKey(c => new { c.IdPhieuKiemKho, c.IdNguyenLieu });
            modelBuilder.Entity<ChiTietXuatHuy>().HasKey(c => new { c.IdPhieuXuatHuy, c.IdNguyenLieu });
            modelBuilder.Entity<ChiTietPhieuThue>().HasKey(c => new { c.IdPhieuThueSach, c.IdSach });
            modelBuilder.Entity<ChiTietPhieuTra>().HasKey(c => new { c.IdPhieuTra, c.IdSach });
            modelBuilder.Entity<SachTacGia>().HasKey(s => new { s.IdSach, s.IdTacGia });
            modelBuilder.Entity<SachTheLoai>().HasKey(s => new { s.IdSach, s.IdTheLoai });
            modelBuilder.Entity<SachNhaXuatBan>().HasKey(s => new { s.IdSach, s.IdNhaXuatBan });
            // THÊM ĐOẠN CẤU HÌNH MỚI NÀY:
            modelBuilder.Entity<NhanVien_Quyen>()
                .HasKey(nq => new { nq.IdNhanVien, nq.IdQuyen });

            modelBuilder.Entity<NhanVien_Quyen>()
                .HasOne(nq => nq.NhanVien)
                .WithMany(n => n.NhanVienQuyens)
                .HasForeignKey(nq => nq.IdNhanVien)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NhanVien_Quyen>()
                .HasOne(nq => nq.Quyen)
                .WithMany(q => q.NhanVienQuyens)
                .HasForeignKey(nq => nq.IdQuyen)
                .OnDelete(DeleteBehavior.Cascade);
        
            // Cấu hình rõ ràng cho DeXuatSanPham
            modelBuilder.Entity<DeXuatSanPham>(entity =>
                {
                    entity.HasKey(e => new { e.IdSanPhamGoc, e.IdSanPhamDeXuat, e.LoaiDeXuat });

                    entity.HasOne(d => d.SanPhamGoc)
                        .WithMany(s => s.DeXuatSanPhamGocs) // Nối đúng vào danh sách Gốc trong SanPham
                        .HasForeignKey(d => d.IdSanPhamGoc)
                        .OnDelete(DeleteBehavior.NoAction);

                    entity.HasOne(d => d.SanPhamDeXuat)
                        .WithMany(s => s.DeXuatSanPhamDeXuats) // Nối đúng vào danh sách Đề xuất trong SanPham
                        .HasForeignKey(d => d.IdSanPhamDeXuat)
                        .OnDelete(DeleteBehavior.NoAction);
                });

            // Cấu hình rõ ràng cho DeXuatSach
            modelBuilder.Entity<DeXuatSach>(entity =>
            {
                entity.HasKey(e => new { e.IdSachGoc, e.IdSachDeXuat, e.LoaiDeXuat });

                entity.HasOne(d => d.SachGoc)
                    .WithMany(s => s.DeXuatSachGocs) // Nối đúng vào danh sách Gốc trong Sach
                    .HasForeignKey(d => d.IdSachGoc)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.SachDeXuat)
                    .WithMany(s => s.DeXuatSachDeXuats) // Nối đúng vào danh sách Đề xuất trong Sach
                    .HasForeignKey(d => d.IdSachDeXuat)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // 2. Cấu hình quan hệ NhanVien - HoaDon (2 khóa ngoại cùng trỏ về 1 bảng)
            modelBuilder.Entity<HoaDon>(entity =>
            {
                entity.HasOne(d => d.NhanVienTao)
                    .WithMany(p => p.HoaDonsTao)
                    .HasForeignKey(d => d.IdNhanVien)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.NhanVienGiaoHang)
                    .WithMany(p => p.HoaDonsGiao)
                    .HasForeignKey(d => d.IdNguoiGiaoHang)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // 3. Xử lý các quan hệ vòng (Cascade Delete) để tránh lỗi SQL
            modelBuilder.Entity<DonXinNghi>()
                .HasOne(d => d.NguoiDuyet)
                .WithMany(n => n.DonXinNghiNguoiDuyets)
                .HasForeignKey(d => d.IdNguoiDuyet)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PhieuLuong>()
                .HasOne(p => p.NguoiPhat)
                .WithMany(n => n.PhieuLuongsDaPhat)
                .HasForeignKey(p => p.IdNguoiPhat)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<NhatKyHuyMon>()
                .HasOne(n => n.NhanVienHuy)
                .WithMany(nv => nv.NhatKyHuyMons)
                .HasForeignKey(n => n.IdNhanVienHuy)
                .OnDelete(DeleteBehavior.NoAction);
            // Dán vào cuối hàm OnModelCreating trong CafebookDbContext.cs
            modelBuilder.Entity<DeXuatSanPham>()
                .HasOne(d => d.SanPhamGoc)
                .WithMany()
                .HasForeignKey(d => d.IdSanPhamGoc)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DeXuatSanPham>()
                .HasOne(d => d.SanPhamDeXuat)
                .WithMany()
                .HasForeignKey(d => d.IdSanPhamDeXuat)
                .OnDelete(DeleteBehavior.NoAction);
        }
        // ====================================================================
        // HỆ THỐNG TỰ ĐỘNG GHI NHẬT KÝ (AUDIT LOG)
        // ====================================================================
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. Lấy ID người dùng từ Token thông qua HttpContext
            int? currentUserId = null;
            var userIdStr = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int id)) currentUserId = id;

            string? ipAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            // 2. Thu thập các thay đổi
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is not NhatKyHeThong &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            var auditEntries = new List<NhatKyHeThong>();

            foreach (var entry in entries)
            {
                var audit = new NhatKyHeThong
                {
                    IdNhanVien = currentUserId,
                    BangBiAnhHuong = entry.Entity.GetType().Name,
                    ThoiGian = DateTime.Now,
                    DiaChiIP = ipAddress
                };

                // Lấy khóa chính (ID) của dòng bị sửa
                var key = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                audit.KhoaChinh = key?.CurrentValue?.ToString();

                switch (entry.State)
                {
                    case EntityState.Added:
                        audit.HanhDong = "THÊM MỚI";
                        audit.DuLieuMoi = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
                        break;

                    case EntityState.Deleted:
                        audit.HanhDong = "XÓA";
                        audit.DuLieuCu = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                        break;

                    case EntityState.Modified:
                        audit.HanhDong = "CẬP NHẬT";
                        var oldValues = new Dictionary<string, object?>();
                        var newValues = new Dictionary<string, object?>();

                        foreach (var prop in entry.OriginalValues.Properties)
                        {
                            var oldVal = entry.OriginalValues[prop];
                            var newVal = entry.CurrentValues[prop];

                            if (!Equals(oldVal, newVal)) // Chỉ lưu những cột có thay đổi thực sự
                            {
                                oldValues[prop.Name] = oldVal;
                                newValues[prop.Name] = newVal;
                            }
                        }
                        audit.DuLieuCu = JsonSerializer.Serialize(oldValues);
                        audit.DuLieuMoi = JsonSerializer.Serialize(newValues);
                        break;
                }
                auditEntries.Add(audit);
            }

            // Lưu log vào context
            if (auditEntries.Any())
            {
                await this.NhatKyHeThongs.AddRangeAsync(auditEntries, cancellationToken);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
    
