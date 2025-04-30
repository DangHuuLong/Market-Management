using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PBL3_HK4.Entity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using PBL3_HK4.Service.Interface;
namespace PBL3_HK4.Service
{
    public class PasswordHasher : IPasswordHasher
    {
        
        private const int saltSize = 16;
        private const int keySize = 32;
        private const int iterations = 10000;
        private static readonly HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA256;
        private const char Delimiter = ';';
        public string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(saltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize);
            return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public bool VerifyPassword(string hashedpassword, string password)
        {
            var element = hashedpassword.Split(Delimiter);
            var salt = Convert.FromBase64String(element[0]);
            var hash = Convert.FromBase64String(element[1]);

            var hashInput = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize);
            return CryptographicOperations.FixedTimeEquals(hash, hashInput);
        }
    }
}
