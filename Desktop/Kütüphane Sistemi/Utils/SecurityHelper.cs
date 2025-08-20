using System;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagementSystem.Utils
{
    public static class SecurityHelper
    {
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var salt = GenerateSalt();
                var saltedPassword = password + salt;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes) + ":" + salt;
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                var parts = hashedPassword.Split(':');
                if (parts.Length != 2)
                    return false;

                var hash = parts[0];
                var salt = parts[1];

                using (var sha256 = SHA256.Create())
                {
                    var saltedPassword = password + salt;
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                    var newHash = Convert.ToBase64String(hashedBytes);
                    return hash == newHash;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string GenerateSalt()
        {
            var saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }
    }
}