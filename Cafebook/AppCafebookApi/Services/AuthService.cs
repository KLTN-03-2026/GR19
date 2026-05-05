using CafebookModel.Model.Shared;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.Json;

namespace AppCafebookApi.Services
{
    public static class AuthService
    {
        public static LoginResponse? CurrentUser { get; private set; }
        public static string? AuthToken => CurrentUser?.Token;

        // Thay đổi kiểu trả về ở đây
        public static async Task<(bool IsSuccess, LoginResponse? Data, string ErrorMessage)> LoginAsync(LoginRequest request)
        {
            var response = await ApiClient.Instance.PostAsJsonAsync("/api/shared/auth/login-nhan-vien", request);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
                CurrentUser = data;

                if (data != null && !string.IsNullOrEmpty(data.Token))
                {
                    ApiClient.SetAuthorizationHeader(data.Token);
                }

                return (true, data, string.Empty); // Đăng nhập thành công
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                string errorMessage = "Tài khoản hoặc mật khẩu không chính xác!";

                try
                {
                    var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    if (errorJson.TryGetProperty("message", out var msg))
                    {
                        errorMessage = msg.GetString() ?? errorMessage;
                    }
                }
                catch { }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden ||
                    response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return (false, null, errorMessage);
                }

                throw new Exception($"Lỗi từ Server ({response.StatusCode}): {errorContent}");
            }
        }

        public static void Logout()
        {
            CurrentUser = null;
            ApiClient.ClearAuthorizationHeader();
        }

        public static bool CoQuyen(string idQuyen)
        {
            if (CurrentUser == null || CurrentUser.Quyen == null) return false;

            if (CurrentUser.TenVaiTro == "Quản trị viên" || CurrentUser.Quyen.Contains("FULL_ADMIN"))
                return true;

            return CurrentUser.Quyen.Contains(idQuyen);
        }

        public static bool CoQuyen(params string[] quyenIds)
        {
            return quyenIds.Any(id => CoQuyen(id));
        }
    }
}