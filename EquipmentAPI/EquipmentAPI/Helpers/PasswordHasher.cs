using System.Text;
using System.Security.Cryptography;

namespace EquipmentAPI.Helpers
{
    public class PasswordHasher
    {
        public static string Hash(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
