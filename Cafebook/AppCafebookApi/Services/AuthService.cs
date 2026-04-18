using CafebookModel.Model.Shared;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using AppCafebookApi.Utils;
using System;

namespace AppCafebookApi.Services
{
    public static class AuthService
    {
        public static LoginResponse? CurrentUser { get; private set; }
        public static string? AuthToken => CurrentUser?.Token;
        private static HttpClient _httpClient = new HttpClient();

        public static void InitializeHttpClient()
        {
            // Nâng cấp: Lấy URL động từ AppConfigManager
            string? serverUrl = AppConfigManager.GetApiServerUrl();
            if (!string.IsNullOrEmpty(serverUrl))
            {
                _httpClient.BaseAddress = new Uri(serverUrl);
            }
        }

        public static void ReloadHttpClient()
        {
            string? serverUrl = AppConfigManager.GetApiServerUrl();
            if (!string.IsNullOrEmpty(serverUrl))
            {
                // Bắt buộc phải tạo mới HttpClient vì cái cũ không cho phép đổi BaseAddress nữa
                _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri(serverUrl);
            }
        }

        public static async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            if (_httpClient.BaseAddress == null) InitializeHttpClient();

            var response = await _httpClient.PostAsJsonAsync("/api/shared/auth/login-nhan-vien", request);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
                CurrentUser = data;
                return data;
            }
            return null;
        }

        public static void Logout() { CurrentUser = null; }

        // HÀM KIỂM TRA QUYỀN NÂNG CAO
        public static bool CoQuyen(string idQuyen)
        {
            if (CurrentUser == null || CurrentUser.Quyen == null) return false;

            // Xử lý quyền tối cao (FULL_QL, FULL_NV) hoặc vai trò Quản trị viên
            if (CurrentUser.TenVaiTro == "Quản trị viên" ||
                CurrentUser.Quyen.Contains("FULL_QL") ||
                CurrentUser.Quyen.Contains("FULL_NV"))
                return true;

            return CurrentUser.Quyen.Contains(idQuyen);
        }

        public static bool CoQuyen(params string[] quyenIds)
        {
            return quyenIds.Any(id => CoQuyen(id));
        }
    }
}