using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace AppCafebookApi.Services
{
    public class AppConfig
    {
        // Mặc định là chuỗi rỗng, không có link sẵn
        public string ApiServerUrl { get; set; } = string.Empty;
    }

    public static class AppConfigManager
    {
        // Tự động lấy thư mục gốc nơi ứng dụng đang chạy (nơi chứa file .exe)
        private static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;

        // Đường dẫn động: [Thư mục App]\SettingCafebook\AppConfig.json
        private static readonly string ConfigFilePath = Path.Combine(BaseDir, "SettingCafebook", "AppConfig.json");

        public static string? GetApiServerUrl()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);

                    // Nếu có file nhưng giá trị rỗng thì vẫn trả về rỗng
                    if (string.IsNullOrWhiteSpace(config?.ApiServerUrl))
                        return null;

                    return config.ApiServerUrl;
                }

                // Nếu chưa có file (lần đầu chạy) thì tạo file trống và trả về null
                SaveApiServerUrl(string.Empty);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đọc file cấu hình: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static void SaveApiServerUrl(string newUrl)
        {
            try
            {
                // Đảm bảo thư mục SettingCafebook luôn được tạo ra nếu chưa có
                string? directory = Path.GetDirectoryName(ConfigFilePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var config = new AppConfig { ApiServerUrl = newUrl };
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu file cấu hình: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}