using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace CafebookModel.Utils 
{
    public static class DataProtectionExtensions
    {
        public static string ProtectToUrlSafe(this IDataProtector protector, string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var protectedBytes = protector.Protect(bytes);
            return WebEncoders.Base64UrlEncode(protectedBytes);
        }

        public static string UnprotectFromUrlSafe(this IDataProtector protector, string urlSafeToken)
        {
            var protectedBytes = WebEncoders.Base64UrlDecode(urlSafeToken);
            var bytes = protector.Unprotect(protectedBytes);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}