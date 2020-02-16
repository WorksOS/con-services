using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Web.Security;

namespace VSS.Hosted.VLCommon
{
  public static class HashUtils
  {
    public static string CreateSalt(int saltSize)
    {
      // Allocate a byte array, which will hold the salt.
      byte[] saltBytes = new byte[saltSize];

      // Initialize a random number generator.
      RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

      // Fill the salt with cryptographically strong byte values.
      rng.GetNonZeroBytes(saltBytes);

      return Convert.ToBase64String(saltBytes);
    }

    public static string ComputeHash(string plainText,
                             string hashAlgorithm,
                             string salt)
    {
      string saltAndPwd = string.Concat(plainText, salt);
      return FormsAuthentication.HashPasswordForStoringInConfigFile(saltAndPwd, "SHA1");
    }

    /*
     * Cannot have a different hash algorithm to TCM so this has been
     * replaced by the one TCM uses - see above
     * 
    public static string ComputeHash(string plainText,
                                     string hashAlgorithm,
                                     string salt)
    {

      // Convert plain text into a byte array.
      byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
      byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

      // Allocate array, which will hold plain text and salt.
      byte[] plainTextWithSaltBytes =
              new byte[plainTextBytes.Length + saltBytes.Length];

      // Copy plain text bytes into resulting array.
      for (int i = 0; i < plainTextBytes.Length; i++)
        plainTextWithSaltBytes[i] = plainTextBytes[i];

      // Append salt bytes to the resulting array.
      for (int i = 0; i < saltBytes.Length; i++)
        plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];

      // Because we support multiple hashing algorithms, we must define
      // hash object as a common (abstract) base class. We will specify the
      // actual hashing algorithm class later during object creation.
      HashAlgorithm hash;

      // Make sure hashing algorithm name is specified.
      if (string.IsNullOrEmpty(hashAlgorithm))
        hashAlgorithm = "SHA1";

      // Initialize appropriate hashing algorithm class.
      switch (hashAlgorithm.ToUpper())
      {
        case "SHA1":
          hash = new SHA1Managed();
          break;

        case "SHA256":
          hash = new SHA256Managed();
          break;

        case "SHA384":
          hash = new SHA384Managed();
          break;

        case "SHA512":
          hash = new SHA512Managed();
          break;

        default:
          hash = new MD5CryptoServiceProvider();
          break;
      }

      // Compute hash value of our plain text with appended salt.
      byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

      // Create array which will hold hash and original salt bytes.
      byte[] hashWithSaltBytes = new byte[hashBytes.Length +
                                          saltBytes.Length];

      // Copy hash bytes into resulting array.
      for (int i = 0; i < hashBytes.Length; i++)
        hashWithSaltBytes[i] = hashBytes[i];

      // Append salt bytes to the result.
      for (int i = 0; i < saltBytes.Length; i++)
        hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];

      // Convert result into a base64-encoded string.
      string hashValue = Convert.ToBase64String(hashWithSaltBytes);

      // Return the result.
      return hashValue;
    }

    public static bool VerifyHash(string plainText,
                              string hashAlgorithm,
                              string hashValue)
    {
      // Convert base64-encoded hash value into a byte array.
      byte[] hashWithSaltBytes = Convert.FromBase64String(hashValue);

      // We must know size of hash (without salt).
      int hashSizeInBits, hashSizeInBytes;

      // Make sure that hashing algorithm name is specified.
      if (string.IsNullOrEmpty(hashAlgorithm))
        hashAlgorithm = "SHA1";

      // Size of hash is based on the specified algorithm.
      switch (hashAlgorithm.ToUpper())
      {
        case "SHA1":
          hashSizeInBits = 160;
          break;

        case "SHA256":
          hashSizeInBits = 256;
          break;

        case "SHA384":
          hashSizeInBits = 384;
          break;

        case "SHA512":
          hashSizeInBits = 512;
          break;

        default: // Must be MD5
          hashSizeInBits = 128;
          break;
      }

      // Convert size of hash from bits to bytes.
      hashSizeInBytes = hashSizeInBits / 8;

      // Make sure that the specified hash value is long enough.
      if (hashWithSaltBytes.Length < hashSizeInBytes)
        return false;

      // Allocate array to hold original salt bytes retrieved from hash.
      byte[] saltBytes = new byte[hashWithSaltBytes.Length -
                                  hashSizeInBytes];

      // Copy salt from the end of the hash to the new array.
      for (int i = 0; i < saltBytes.Length; i++)
        saltBytes[i] = hashWithSaltBytes[hashSizeInBytes + i];

      // Compute a new hash string.
      string expectedHashString =
                  ComputeHash(plainText, hashAlgorithm, Encoding.UTF8.GetString(saltBytes));

      // If the computed hash matches the specified hash,
      // the plain text value must be correct.
      return (hashValue == expectedHashString);
    }
    */

    public static long GenerateMD5HashFromByteArray(string key, byte[] input)
    {
      System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

      byte[] hashKey = new Byte[key.Length];

      hashKey = encoding.GetBytes(key);

      HMACMD5 myhmacMD5 = new HMACMD5(hashKey);

      byte[] hash = myhmacMD5.ComputeHash(input);

      return BitConverter.ToInt64(hash, 0);
    }

    public static long GenerateMD5HashFromString(string key, string input)
    {
      System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

      byte[] hashKey = new Byte[key.Length];

      hashKey = encoding.GetBytes(key);

      HMACMD5 myhmacMD5 = new HMACMD5(hashKey);

      byte[] hash = myhmacMD5.ComputeHash(encoding.GetBytes(input));

      return BitConverter.ToInt64(hash, 0);
    }
  }
}
