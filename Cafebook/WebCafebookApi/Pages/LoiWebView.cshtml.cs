using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebCafebookApi.Pages
{
    public class LoiWebViewModel : PageModel
    {
        // Đổi tên từ StatusCode thành ErrorCode để không trùng với PageModel
        public int ErrorCode { get; set; }
        public string ErrorTitle { get; set; } = "Đã xảy ra lỗi";
        public string ErrorMessage { get; set; } = "Hệ thống đang gặp sự cố. Vui lòng thử lại sau.";
        public string ErrorIcon { get; set; } = "error";

        public void OnGet(int code = 500)
        {
            ErrorCode = code;

            // Dịch mã lỗi HTTP sang thông báo thân thiện cho Khách hàng
            switch (code)
            {
                case 400:
                    ErrorTitle = "Yêu cầu không hợp lệ";
                    ErrorMessage = "Dữ liệu bạn gửi lên không đúng định dạng. Vui lòng kiểm tra lại.";
                    ErrorIcon = "warning";
                    break;
                case 401:
                    ErrorTitle = "Yêu cầu đăng nhập";
                    ErrorMessage = "Phiên làm việc đã hết hạn hoặc bạn cần đăng nhập để xem trang này.";
                    ErrorIcon = "lock";
                    break;
                case 403:
                    ErrorTitle = "Truy cập bị từ chối";
                    ErrorMessage = "Bạn không có quyền truy cập vào khu vực này. Vui lòng liên hệ quản trị viên.";
                    ErrorIcon = "gpp_bad";
                    break;
                case 404:
                    ErrorTitle = "Không tìm thấy trang";
                    ErrorMessage = "Nội dung bạn tìm kiếm không tồn tại, đã bị xóa hoặc sai đường dẫn.";
                    ErrorIcon = "search_off";
                    break;
                case 500:
                default:
                    ErrorTitle = "Lỗi hệ thống";
                    ErrorMessage = "Máy chủ đang tạm thời quá tải hoặc gặp sự cố. Đội ngũ kỹ thuật đã ghi nhận và đang xử lý.";
                    ErrorIcon = "dns";
                    break;
            }
        }
    }
}