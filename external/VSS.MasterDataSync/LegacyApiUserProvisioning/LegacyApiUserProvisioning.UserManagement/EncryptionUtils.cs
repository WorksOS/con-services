using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LegacyApiUserProvisioning.UserManagement
{
    public static class EncryptionUtils
    {

        public static string EncryptText(string text, string keyText)
        {
            if (text == null || keyText == null) return null;

            string encrypted;
            var data = Encoding.UTF8.GetBytes(text);
            var key = Encoding.UTF8.GetBytes(keyText.Substring(0, 8));
            var iv = Encoding.UTF8.GetBytes(keyText.Substring(key.Length - 8));
            var des = new DESCryptoServiceProvider();

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, des.CreateEncryptor(key, iv), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();
                    encrypted = Convert.ToBase64String(ms.ToArray());
                }
            }
            return encrypted;
        }
        
    }
}