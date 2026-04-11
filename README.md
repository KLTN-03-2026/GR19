<div align="center">
  <h1 align="center">☕ HỆ THỐNG QUẢN LÝ VÀ ĐẶT BÀN CAFEBOOK TÍCH HỢP CHATBOT AI HỖ TRỢ TƯ VẤN 📚</h1>
  <p align="center">
    <b>Giải pháp quản lý thông minh tích hợp Nhà sách & Quán Cà phê</b>
    <br />
    <i>Sử dụng kiến trúc phân lớp, ASP.NET Core 8.0 và Trợ lý ảo AI</i>
  </p>

</div>

---

## 📌 Mục lục
* [Giới thiệu dự án](#-giới-thiệu-dự-án)
* [Tính năng chính](#-tính-năng-chính)
* [Công nghệ sử dụng](#-công-nghệ-sử-dụng)
* [Yêu cầu môi trường & Công cụ](#️-yêu-cầu-môi-trường--công-cụ)
* [Cấu hình Cơ sở dữ liệu](#️-cấu-hình-cơ-sở-dữ-liệu)
* [Khởi chạy chương trình](#-khởi-chạy-chương-trình)
* [Tài khoản Test](#-tài-khoản-test)
* [Thông tin nhóm phát triển](#-thông-tin-nhóm-phát-triển)

---

## 📖 Giới thiệu dự án

**CafeBook System** là một hệ thống quản lý toàn diện được thiết kế để giải quyết bài toán vận hành cho mô hình kinh doanh kết hợp giữa quán cà phê và hiệu sách. Hệ thống không chỉ dừng lại ở việc bán hàng mà còn tối ưu hóa trải nghiệm khách hàng thông qua **Trợ lý AI** và giúp chủ doanh nghiệp quản lý tài chính, nhân sự một cách khoa học.

> [!NOTE]
> Dự án được xây dựng với cấu trúc đa nền tảng (Web & Desktop) kết nối thông qua hệ thống API tập trung.

---

## ✨ Tính năng chính

### 🛒 Quản lý bán hàng & POS
- [x] Quản lý danh mục thực đơn (đồ ăn, thức uống) và thư viện sách.
- [x] Nghiệp vụ POS tại quầy: Gọi món, thanh toán, quản lý sơ đồ bàn (trạng thái).
- [x] Quản lý Thuê & Trả sách: Lập phiếu thuê, xử lý sách trễ hạn.
- [x] Quản lý đơn hàng giao đi (Order delivery): Tiếp nhận, xác nhận hoặc hủy đơn và in phiếu giao hàng.

### 🤖 Trợ lý AI thông minh
- [x] Tư vấn, giải đáp thắc mắc khách hàng 24/7
- [x] Hỗ trợ đặt bàn trực tiếp ngay trong khung chat và giải đáp dịch vụ.
- [x] Cơ chế Handoff: Chuyển luồng hội thoại từ AI sang nhân viên tư vấn trực tiếp khi có yêu cầu chuyên sâu.

### 📊 Quản trị & Tài chính
- [x] Quản lý nhân sự toàn diện: Chấm công, xếp lịch làm việc, phê duyệt đơn xin nghỉ và tính lương, thưởng/phạt.
- [x] Quản lý kho: Kiểm soát hiện trạng, nhập/xuất kho, định lượng nguyên liệu và quản lý nhà cung cấp.
- [x] Dashboard báo cáo & thống kê: Theo dõi doanh thu, tồn kho sách/nguyên liệu và hiệu suất nhân viên.
---

## 🛠 Công nghệ sử dụng

| Thành phần | Công nghệ |
| :--- | :--- |
| **Backend API** | ![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core_8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white) ![Entity Framework](https://img.shields.io/badge/ASP.NET_Core_Web_App_API-512BD4?style=flat-square&logo=dotnet&logoColor=white) |
| **Frontend Web** | ![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core_8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white) ![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core_Web_API-512BD4?style=flat-square&logo=dotnet&logoColor=white)  |
| **Desktop App** |![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core_8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white) ![WPF](https://img.shields.io/badge/WPF_Application-0078D4?style=flat-square&logo=windows&logoColor=white) |
| **Class Library** |![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core_8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white) ![ASP.NET Core](https://img.shields.io/badge/.NET_Class_Library-512BD4?style=flat-square&logo=dotnet&logoColor=white) |
| **Database** | ![SQL Server](https://img.shields.io/badge/SQL_Server_Management_Studio_22-0078D4?style=flat-square&logo=microsoft-sql-server&logoColor=white) |

---

## 🛠️ Yêu cầu môi trường & Công cụ

Để dự án hoạt động ổn định và không phát sinh lỗi kết nối, máy tính cần đáp ứng các môi trường sau:

* **IDE (Trình biên dịch):** Yêu cầu tối thiểu **Visual Studio 2022** (hoặc mới hơn).
* **Cơ sở dữ liệu:** Cài bản **Microsoft SQL Server 2025 (RTM) - 17.0.1000.7 (X64)** Oct 21 2025 12:05:57 Copyright (C) 2025 Microsoft Corporation Express Edition (64-bit).
* **Công cụ quản lý DB:** Yêu cầu cài đặt **SQL Server Management Studio 22** (SSMS).

> 💡 **Lưu ý cài đặt:** Bộ cài đặt chuẩn đã được nhóm đính kèm sẵn tại thư mục: `GR19/Cafebook/tailieucode/appcancai/`. 
> Khuyến nghị cài thêm file `SQL2025-SSEI-Expr.exe` để không phải cấu hình lại đường dẫn kết nối. Nếu bạn dùng bản khác, vui lòng vào file `appsettings.json` của `CafebookApi` để đổi đường dẫn tương ứng.

---

## 🗄️ Cấu hình Cơ sở dữ liệu

Dữ liệu nguồn của dự án được đặt tại thư mục: `GR19/Cafebook/DatabaseCafebook/`. Trước khi chạy dự án, bạn cần nạp cơ sở dữ liệu vào SQL Server:

* **Kiểm tra phiên bản SQL Server đang dùng:** Mở SSMS, tạo một Query mới và chạy lệnh `SELECT @@VERSION;` để biết máy đang chạy bản nào.
* **Đối với SQL Server 2025:** Bạn có thể tiến hành Restore Database để chạy từ file `.bak` có sẵn trong thư mục.
* **Đối với các phiên bản cũ hơn (VD: SQL 2019):** Vui lòng dùng file `Cafebookbakup.sql` và chạy (Execute) script để tạo CSDL, tránh lỗi xung đột khi dùng file `.bak`.

---

## 💻 Khởi chạy chương trình

Dự án được thiết kế theo kiến trúc Microservices/API, do đó cần phải chạy đồng thời nhiều project (API, Web, App). Hãy thực hiện theo các bước sau:

* Mở file Solution (`.sln`) của dự án bằng Visual Studio 2022(hoặc mới hơn).
* Chuột phải vào Solution (dòng đầu tiên trên Solution Explorer) -> Chọn **Properties** (hoặc **Set Startup Projects...**).
* Trong cửa sổ cấu hình, chọn mục **Multiple startup projects**.
* Ấn vào biểu tượng dấu cộng **`(+)` (New Profile)** và cài đặt cột **Action** cho các project như hình dưới đây:

![Image](https://github.com/user-attachments/assets/572944bf-8507-4b75-88ad-0238ea441958)

  - `AppCafebookApi` -> **Start**
  - `CafebookApi` -> **Start**
  - `CafebookModel` -> None
  - `WebCafebookApi` -> **Start**

* Xong ấn **Áp dụng (Apply)** rồi ấn **OK**.
* Nhấn phím **F5** để chạy dự án.

---

## 🔑 Tài khoản Test

Để thuận tiện cho việc kiểm thử và chấm điểm, giảng viên có thể sử dụng tài khoản có quyền hạn cao nhất (Quản lý/Admin) để đăng nhập:

* **Tài khoản test (Quyền cao nhất):** `quanly@cafebook.vn`
* **Mật khẩu:** `123456`

---

## 👥 Thông tin nhóm phát triển

**Nhóm 19 - Đồ án Tốt nghiệp KLTN 03-2026**<br>
**Giảng viên hướng dẫn:** ThS. Phạm Phú Khương

| STT | Họ và Tên | Mã Sinh Viên | Vai trò trong nhóm |
| :---: | :--- | :---: | :--- |
| 1 | **Huỳnh Ngọc Phú** | 28211106495 | Quản lý dự án (Scrum Master/Frontend) |
| 2 | **Nguyễn Minh Tú** | 28211105717 | Thành viên (Frontend/WPF UI/QA/Tester) |
| 3 | **Nguyễn Tú Uyên** | 28201149694 | Thành viên (Frontend/Web UI/QA/Tester) |
| 4 | **Lâm Chu Bảo Toàn** | 28211105266 | Thành viên (Full Stack Developer) |
| 5 | **Vương Quốc Hưng** | 28211145208 | Thành viên (WPF UI/QA/Tester) |
