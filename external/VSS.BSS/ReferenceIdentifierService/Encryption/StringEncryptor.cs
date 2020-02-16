using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using VSS.Hosted.VLCommon;

namespace VSS.Nighthawk.ReferenceIdentifierService.Encryption
{
  public class StringEncryptor : IStringEncryptor
  {
    #region private
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    #endregion

    public byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
    {
      // Check arguments.
      if (string.IsNullOrEmpty(plainText))
        throw new ArgumentNullException("plainText");
      if (key == null || key.Length <= 0)
        throw new ArgumentNullException("key");
      if (iv == null || iv.Length <= 0)
        throw new ArgumentNullException("iv");

      AesManaged aesAlg = null;

      // Create the streams used for encryption.
      MemoryStream memoryStream = new MemoryStream();

      try
      {
        // Create the encryption algorithm object with the specified key and IV.
        aesAlg = new AesManaged {KeySize = 128, Key = key, IV = iv, Padding = PaddingMode.PKCS7};
        //in order to prevent the dreaded "padding-is-invalid-and-cannot-be-removed" error

        // Create an encryptor to perform the stream transform.
        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        using (var streamWriter = new StreamWriter(cryptoStream))
        {
          //Write all data to the stream.
          streamWriter.Write(plainText);
        }
      }
      catch (CryptographicException ce)
      {
        Log.IfInfoFormat("EncryptStringToBytes ERROR: " + ce.Message);
        memoryStream.Flush();
      }
      finally
      {
        if (aesAlg != null)
          aesAlg.Clear();
      }

      // Return the encrypted bytes from the memory stream.
      return memoryStream.ToArray();
    }

    /// <summary>
    /// Developer:
    /// You will sometimes get a message about "invalid padding" when encryption and decryption have not used the same key or initialisation vector. 
    /// Padding is a number of bytes added to the end of your plaintext to make it up to a full number of blocks for the cipher to work on. 
    /// In PKCS7 padding each byte is equal to the number of bytes added, so it can always be removed after decryption. 
    /// BOTTOM LINE:
    /// If the EncryptionKey[] and the Decryption Key[] are different, the decryption has led to a string where the last n bytes are not equal to the value n of the last byte.
    /// This why we catch the CryptographicException and erase the return string.
    /// </summary>
    /// <param name="cipherText"></param>
    /// <param name="key"></param>
    /// <param name="iv"></param>
    /// <returns></returns>
    public string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
    {
      // Check arguments.
      if (cipherText == null || cipherText.Length <= 0)
        throw new ArgumentNullException("cipherText");
      if (key == null || key.Length <= 0)
        throw new ArgumentNullException("key");
      if (iv == null || iv.Length <= 0)
        throw new ArgumentNullException("iv");

      AesManaged aesAlg = null;
      string plaintext;

      try
      {
        // Create a the encryption algorithm object with the specified key and IV.
        aesAlg = new AesManaged {Key = key, IV = iv, Padding = PaddingMode.PKCS7};
        //in order to prevent the dreaded "padding-is-invalid-and-cannot-be-removed" error

        // Create a decrytor to perform the stream transform.
        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        // Create the streams used for decryption.
        using (var memoryStream = new MemoryStream(cipherText))
        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
        using (var streamReader = new StreamReader(cryptoStream))
        {
          // Read the decrypted bytes from the decrypting stream and place them in a string.
          plaintext = streamReader.ReadToEnd();
        }
      }
      catch (CryptographicException ce)
      {
        Log.IfInfoFormat("DecryptStringFromBytes ERROR: " + ce.Message);
        plaintext = "";
      }
      catch (Exception ex)
      {
        //STRANGE VisualStudio behaviour here! even though we cannot step into this exception, 
        //it is always thrown during the streamReader.ReadToEnd() call when encryption and decryption Keys differ during UnitTesting
        Log.IfInfoFormat("DecryptStringFromBytes ERROR: " + ex.Message);
        plaintext = "";
      }
      finally
      {
        if (aesAlg != null)
          aesAlg.Clear();
      }

      return plaintext;
    }
  }
}

