using System;
using System.Security.Cryptography;

byte[] salt = RandomNumberGenerator.GetBytes(16);
byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
    "password123",
    salt,
    100000,
    HashAlgorithmName.SHA256,
    32);
Console.WriteLine($"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}");
