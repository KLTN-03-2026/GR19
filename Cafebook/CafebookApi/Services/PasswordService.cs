using System.Security.Cryptography;

namespace CafebookApi.Services
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string storedPassword, string providedPassword);
    }

    public class PasswordService : IPasswordService
    {
        private readonly bool _enableEncryption;
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 100000;
        private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
        private const string Prefix = "$PBKDF2$"; // Tiền tố để nhận diện mật khẩu đã mã hóa

        public PasswordService(IConfiguration configuration)
        {
            // Đọc cấu hình từ file EnablePasswordEncryption.json
            _enableEncryption = configuration.GetValue<bool>("PasswordSettings:EnableEncryption", false);
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return password;
            if (!_enableEncryption) return password; // Nếu tắt mã hóa -> trả về text gốc

            // Băm mật khẩu bằng thuật toán PBKDF2 của .NET
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _hashAlgorithmName, KeySize);

            // Lưu trữ theo format: $PBKDF2$:Salt:Hash
            return string.Join(":", Prefix, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public bool VerifyPassword(string storedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword) || string.IsNullOrEmpty(providedPassword)) return false;

            // Nếu mật khẩu trong CSDL không chứa tiền tố băm (tức là đang lưu dạng text thường cũ)
            // hoặc cấu hình đang tắt mã hóa -> So sánh trực tiếp
            if (!storedPassword.StartsWith(Prefix))
            {
                return storedPassword == providedPassword;
            }

            // Nếu mật khẩu trong CSDL đã được mã hóa
            try
            {
                var parts = storedPassword.Split(':');
                if (parts.Length != 3) return false;

                var salt = Convert.FromBase64String(parts[1]);
                var hash = Convert.FromBase64String(parts[2]);

                var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(providedPassword, salt, Iterations, _hashAlgorithmName, KeySize);

                return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
            }
            catch
            {
                return false;
            }
        }
    }
}