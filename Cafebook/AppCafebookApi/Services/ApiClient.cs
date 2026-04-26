using System.Net.Http;
using System.Net.Http.Headers;
using System;

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

                    string? serverUrl = AppConfigManager.GetApiServerUrl();
                    if (!string.IsNullOrEmpty(serverUrl))
                    {
                        _instance.BaseAddress = new Uri(serverUrl);
                    }

                    _instance.DefaultRequestHeaders.Accept.Clear();
                    _instance.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                }
                return _instance;
            }
        }

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