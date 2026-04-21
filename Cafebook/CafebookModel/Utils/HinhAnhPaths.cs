// Tập tin: CafebookModel/Utils/HinhAnhPaths.cs
using System.IO;

namespace CafebookModel.Utils
{
    public static class HinhAnhPaths
    {
        // --- WPF Pack URIs (Defaults/Fallbacks) ---
        public const string DefaultAvatar = "/Assets/Images/default-avatar.png";
        public const string DefaultBookCover = "/Assets/Images/default-book-cover.png";
        public const string DefaultFoodIcon = "/Assets/Images/default-food-icon.png";

        // --- WEB Pack URIs (BỎ CHỮ wwwroot ĐI, BẮT ĐẦU BẰNG DẤU /) ---
        public const string WebDefaultAvatar = "/images/avatars/default-avatar.png";
        public const string WebDefaultBookCover = "/images/books/default-book-cover.png";
        public const string WebDefaultFoodIcon = "/images/default-food-icon.png"; 

        // --- Server Relative URL Paths (Dùng /) ---
        public const string UrlAvatarNV = "/images/avatars/avatarNV";
        public const string UrlAvatarKH = "/images/avatars/avatarKH";
        public const string UrlBooks = "/images/books";
        public const string UrlFoods = "/images/foods";
        public const string UrlBuildnhapkho = "/images/BuildNhapKho";
    }
}