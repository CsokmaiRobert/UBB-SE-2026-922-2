namespace BoardRentAndProperty.Utilities
{
    using System;
    using System.Security.Cryptography;

    public class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string hashedPasswordWithSalt)
        {
            const int ExpectedComponentCount = 2;
            const int SaltIndex = 0;
            const int HashIndex = 1;

            var hashComponents = hashedPasswordWithSalt.Split(':');
            if (hashComponents.Length != ExpectedComponentCount)
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(hashComponents[SaltIndex]);
            byte[] storedHash = Convert.FromBase64String(hashComponents[HashIndex]);

            byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
    }
}
