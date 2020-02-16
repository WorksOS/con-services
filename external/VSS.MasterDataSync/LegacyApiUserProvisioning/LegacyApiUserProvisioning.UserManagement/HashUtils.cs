using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace LegacyApiUserProvisioning.UserManagement
{
    public class HashUtils
    {
        public static string CreateSalt(int saltSize)
        {
            // Allocate a byte array, which will hold the salt.
            var saltBytes = new byte[saltSize];

            // Initialize a random number generator.
            var rng = new RNGCryptoServiceProvider();

            // Fill the salt with cryptographically strong byte values.
            rng.GetNonZeroBytes(saltBytes);

            return Convert.ToBase64String(saltBytes);
        }

        public static string ComputeHash(string plainText,
            string hashAlgorithm,
            string salt)
        {
            var saltAndPwd = string.Concat(plainText, salt);
            return FormsAuthentication.HashPasswordForStoringInConfigFile(saltAndPwd, hashAlgorithm);
        }
    }
}
