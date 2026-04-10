using System.Net.Http;
using System.Net.Http.Headers;
using System;
using AppCafebookApi.Utils;

namespace AppCafebookApi.Services
{
    public static class ApiClient
    {
        private static HttpClient? _instance;

        public static HttpClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HttpClient();

                    // 1. Đọc URL động từ File cấu hình
                    string? serverUrl = AppConfigManager.GetApiServerUrl();
                    if (!string.IsNullOrEmpty(serverUrl))
                    {
                        _instance.BaseAddress = new Uri(serverUrl);
                    }

                    _instance.DefaultRequestHeaders.Accept.Clear();
                    _instance.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                }

                // 2. Tự động đính kèm Token vào Header nếu đã đăng nhập
                if (!string.IsNullOrEmpty(AuthService.AuthToken))
                {
                    _instance.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", AuthService.AuthToken);
                }
                else
                {
                    _instance.DefaultRequestHeaders.Authorization = null;
                }

                return _instance;
            }
        }

        // Hàm này được gọi nếu người dùng đổi Cấu hình IP và muốn reset lại HttpClient
        public static void ResetInstance()
        {
            _instance = null;
        }

        public static void SetAuthorizationHeader(string token)
        {
            Instance.DefaultRequestHeaders.Authorization = null;
            if (!string.IsNullOrEmpty(token))
            {
                Instance.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public static void ClearAuthorizationHeader()
        {
            Instance.DefaultRequestHeaders.Authorization = null;
        }
    }
}