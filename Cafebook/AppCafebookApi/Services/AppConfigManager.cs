using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace AppCafebookApi.Services
{
    public class AppConfig
    {
        public string ApiServerUrl { get; set; } = string.Empty;
    }

    public static class AppConfigManager
    {
        private static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;

        private static readonly string ConfigFilePath = Path.Combine(BaseDir, "SettingCafebook", "AppConfig.json");

        public static string? GetApiServerUrl()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);

                    if (string.IsNullOrWhiteSpace(config?.ApiServerUrl))
                        return null;

                    return config.ApiServerUrl;
                }

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