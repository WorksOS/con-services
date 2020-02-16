using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace VSS.Hosted.VLCommon
{
  public static class EncryptionUtils
  {
    public static long TextToLong(string text)
    {
      long hashCodeId = 0;
      foreach (char character in text)
      {
        hashCodeId = (hashCodeId * HashMultiplier) + character;
      }

      return hashCodeId;
    }

    private const long HashMultiplier = 31;

    public static string EncryptText( string text, string keyText )
    {
      string encrypted = null;

      if ( text != null && keyText != null )
      {
        byte[] data = Encoding.UTF8.GetBytes( text );
        byte[] key = Encoding.UTF8.GetBytes( keyText.Substring( 0, 8 ) );
        byte[] IV = Encoding.UTF8.GetBytes( keyText.Substring( key.Length - 8 ) );
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();

        using ( MemoryStream ms = new MemoryStream() )
        {
          using ( CryptoStream cs = new CryptoStream( ms, des.CreateEncryptor( key, IV ), CryptoStreamMode.Write ) )
          {
            cs.Write( data, 0, data.Length );
            cs.FlushFinalBlock();
            encrypted = Convert.ToBase64String( ms.ToArray() );
          }
        }
      }
      return encrypted;
    }

    public static string DecryptText( string text, string keyText )
    {
      string decrypted = null;

      if ( text != null && keyText != null )
      {
        byte[] data = Convert.FromBase64String( text );
        byte[] key = Encoding.UTF8.GetBytes( keyText.Substring( 0, 8 ) );
        byte[] IV = Encoding.UTF8.GetBytes( keyText.Substring( key.Length - 8 ) );
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();

        using ( MemoryStream ms = new MemoryStream() )
        {
          using ( CryptoStream cs = new CryptoStream( ms, des.CreateDecryptor( key, IV ), CryptoStreamMode.Write ) )
          {
            cs.Write( data, 0, data.Length );
            cs.FlushFinalBlock();
            decrypted = Encoding.UTF8.GetString( ms.ToArray() );
          }
        }
      }
      return decrypted;
    }

    public static string EncryptText3( string text, string keyText )
    {
      string encrypted = null;

      if ( text != null && keyText != null )
      {
        PasswordDeriveBytes pderiver = new PasswordDeriveBytes( keyText, null );
        byte[] IV = new byte[8];
        byte[] pbeKey = pderiver.CryptDeriveKey( "RC2", "MD5", 128, IV );

        RC2CryptoServiceProvider RC2 = new RC2CryptoServiceProvider();
        using ( ICryptoTransform encryptor = RC2.CreateEncryptor( pbeKey, IV ) )
        {
          using ( MemoryStream ms = new MemoryStream() )
          {
            using ( CryptoStream cs = new CryptoStream( ms, encryptor, CryptoStreamMode.Write ) )
            {
              byte[] data = Encoding.UTF8.GetBytes( text );

              cs.Write( data, 0, data.Length );
              cs.FlushFinalBlock();

              encrypted = Convert.ToBase64String( ms.ToArray() );
            }
          }
        }
      }
      return encrypted;
    }

    public static string DecryptText3( string text, string keyText )
    {
      string decrypted = null;

      if ( text != null && keyText != null )
      {
        PasswordDeriveBytes pderiver = new PasswordDeriveBytes( keyText, null );
        byte[] IV = new byte[8];
        byte[] pbeKey = pderiver.CryptDeriveKey( "RC2", "MD5", 128, IV );

        RC2CryptoServiceProvider RC2 = new RC2CryptoServiceProvider();
        using ( ICryptoTransform encryptor = RC2.CreateDecryptor( pbeKey, IV ) )
        {
          using ( MemoryStream ms = new MemoryStream() )
          {
            using ( CryptoStream cs = new CryptoStream( ms, encryptor, CryptoStreamMode.Write ) )
            {
              byte[] data = Convert.FromBase64String( text );

              cs.Write( data, 0, data.Length );
              cs.FlushFinalBlock();

              decrypted = Encoding.UTF8.GetString( ms.ToArray() );
            }
          }
        }
      }
      return decrypted;
    }

    public static bool IsSingleByte( string value )
    {
      Encoding latinEncoding = Encoding.GetEncoding( "iso-8859-1" );
      return value == latinEncoding.GetString( latinEncoding.GetBytes( value ) );
    }

    public static string TemporaryKey()
    {
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      var random = new Random();
      var secretKey = new string(
          Enumerable.Repeat(chars, 16)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
      return secretKey;
    }
  }
}
