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
        public DbSet<NhatKyHeThong> NhatKyHeThongs { get; set; }
        public DbSet<GopY> GopYs { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================================
            // 1. CẤU HÌNH KHÓA CHÍNH PHỨC HỢP (COMPOSITE KEYS)
            // =========================================================
            modelBuilder.Entity<HoaDon_KhuyenMai>().HasKey(hk => new { hk.IdHoaDon, hk.IdKhuyenMai });
            modelBuilder.Entity<DinhLuong>().HasKey(d => new { d.IdSanPham, d.IdNguyenLieu });
            modelBuilder.Entity<ChiTietPhuThuHoaDon>().HasKey(c => new { c.IdHoaDon, c.IdPhuThu });
            modelBuilder.Entity<ChiTietNhapKho>().HasKey(c => new { c.IdPhieuNhapKho, c.IdNguyenLieu });
            modelBuilder.Entity<ChiTietKiemKho>().HasKey(c => new { c.IdPhieuKiemKho, c.IdNguyenLieu });
            modelBuilder.Entity<ChiTietXuatHuy>().HasKey(c => new { c.IdPhieuXuatHuy, c.IdNguyenLieu });
            modelBuilder.Entity<ChiTietPhieuThue>().HasKey(c => new { c.IdPhieuThueSach, c.IdSach });
            modelBuilder.Entity<ChiTietPhieuTra>().HasKey(c => new { c.IdPhieuTra, c.IdSach });
            modelBuilder.Entity<SachTacGia>().HasKey(st => new { st.IdSach, st.IdTacGia });
            modelBuilder.Entity<SachTheLoai>().HasKey(st => new { st.IdSach, st.IdTheLoai });
            modelBuilder.Entity<SachNhaXuatBan>().HasKey(sn => new { sn.IdSach, sn.IdNhaXuatBan });
            modelBuilder.Entity<NhanVien_Quyen>().HasKey(nq => new { nq.IdNhanVien, nq.IdQuyen });

            // =========================================================
            // 2. CẤU HÌNH QUAN HỆ & RÀNG BUỘC (RELATIONSHIPS)
            // =========================================================

            // --- NhanVien_Quyen ---
            modelBuilder.Entity<NhanVien_Quyen>(entity =>
            {
                entity.HasOne(nq => nq.NhanVien)
                    .WithMany(n => n.NhanVienQuyens)
                    .HasForeignKey(nq => nq.IdNhanVien)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(nq => nq.Quyen)
                    .WithMany(q => q.NhanVienQuyens)
                    .HasForeignKey(nq => nq.IdQuyen)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- DeXuatSanPham ---
            modelBuilder.Entity<DeXuatSanPham>(entity =>
            {
                entity.HasKey(e => new { e.IdSanPhamGoc, e.IdSanPhamDeXuat, e.LoaiDeXuat });

                entity.HasOne(d => d.SanPhamGoc)
                    .WithMany(s => s.DeXuatSanPhamGocs)
                    .HasForeignKey(d => d.IdSanPhamGoc)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.SanPhamDeXuat)
                    .WithMany(s => s.DeXuatSanPhamDeXuats)
                    .HasForeignKey(d => d.IdSanPhamDeXuat)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- DeXuatSach ---
            modelBuilder.Entity<DeXuatSach>(entity =>
            {
                entity.HasKey(e => new { e.IdSachGoc, e.IdSachDeXuat, e.LoaiDeXuat });

                entity.HasOne(d => d.SachGoc)
                    .WithMany(s => s.DeXuatSachGocs)
                    .HasForeignKey(d => d.IdSachGoc)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.SachDeXuat)
                    .WithMany(s => s.DeXuatSachDeXuats)
                    .HasForeignKey(d => d.IdSachDeXuat)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- HoaDon ---
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

            // --- Các bảng khác ---
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
        }

        // ====================================================================
        // HỆ THỐNG TỰ ĐỘNG GHI NHẬT KÝ (AUDIT LOG)
        // ====================================================================
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            bool isAuthenticated = _httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true;
            string? userRole = isAuthenticated ? _httpContextAccessor!.HttpContext!.User.FindFirst(ClaimTypes.Role)?.Value : null;

            string? staffIdStr = isAuthenticated ? _httpContextAccessor!.HttpContext!.User.FindFirst("IdNhanVien")?.Value : null;
            string? genericIdStr = isAuthenticated ? _httpContextAccessor!.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;

            string? ipAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            int? currentStaffId = null;
            int? currentCustomerId = null;
            string currentRole = "Khách vãng lai";

            if (isAuthenticated)
            {
                if (userRole == "NhanVien" || userRole == "QuanLy")
                {
                    string? idToParse = staffIdStr ?? genericIdStr;
                    if (int.TryParse(idToParse, out int id) && id > 0)
                    {
                        currentStaffId = id;
                    }
                    currentRole = userRole == "QuanLy" ? "Quản lý" : "Nhân viên";
                }
                else if (userRole == "KhachHang")
                {
                    if (int.TryParse(genericIdStr, out int id) && id > 0)
                    {
                        currentCustomerId = id;
                    }
                    currentRole = "Khách hàng";
                }
            }

            // 3. THU THẬP CÁC THAY ĐỔI
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is not CafebookModel.Model.ModelEntities.NhatKyHeThong &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            var auditEntries = new List<CafebookModel.Model.ModelEntities.NhatKyHeThong>();

            foreach (var entry in entries)
            {
                var audit = new CafebookModel.Model.ModelEntities.NhatKyHeThong
                {
                    IdNhanVien = currentStaffId,
                    IdKhachHang = currentCustomerId, // Gán IdKhachHang
                    VaiTro = currentRole,            // Gán VaiTro
                    BangBiAnhHuong = entry.Entity.GetType().Name,
                    ThoiGian = DateTime.Now,
                    DiaChiIP = ipAddress
                };

                var key = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                audit.KhoaChinh = key?.CurrentValue?.ToString();

                // HanhDong giờ đây chỉ còn thuần túy là THAO TÁC
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

                            if (!Equals(oldVal, newVal))
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

            if (auditEntries.Any())
            {
                await this.NhatKyHeThongs.AddRangeAsync(auditEntries, cancellationToken);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
    
