using CafebookModel.Model.Shared;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace AppCafebookApi.Services
{
    public static class AuthService
    {
        public static LoginResponse? CurrentUser { get; private set; }
        public static string? AuthToken => CurrentUser?.Token;


        public static async Task<LoginResponse?> LoginAsync(LoginRequest request)
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

                return data;
            }
            else
            {
                // ĐỌC LỖI THẬT TỪ SERVER VÀ HIỂN THỊ LÊN
                string errorContent = await response.Content.ReadAsStringAsync();
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