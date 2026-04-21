// Vị trí lưu: E:\Tai Lieu Hoc Tap\N19 KLTN 032026\Cafebook\WebCafebookApi\Services\SessionExtensions.cs
using System.Text.Json;

namespace WebCafebookApi.Services
{
    public static class SessionExtensions
    {
        // Khai báo sẵn tên Key để sau này gọi không bị sai chính tả
        public const string CartKey = "GioHang";

        // Hàm lưu vào Session
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Hàm đọc từ Session ra
        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}