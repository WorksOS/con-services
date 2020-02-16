using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Nighthawk.ReferenceIdentifierService.Encryption;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.Encryption
{
  [TestClass]
  public class StringEncryptorTests
  {
    private const string DispatcherAESKeyByte = "Bothriospondylus";
    private const string DispatcherAESIVByte = "GreatLeonopteryx";
    private const string PasswordToBeEncrypted = "nhpw";

    [TestMethod]
    public void EncryptStringToBytes_Null_Text_Test()
    {
      StringEncryptor strEncryptor = new StringEncryptor();
      try
      {
        strEncryptor.EncryptStringToBytes(null, null, null);
        Assert.Fail();
      }
      catch(ArgumentNullException e)
      {
        Assert.AreEqual("plainText", e.ParamName);
      }
    }

    [TestMethod]
    public void EncryptStringToBytes_Null_Key_Test()
    {
      StringEncryptor strEncryptor = new StringEncryptor();
      try
      {
        strEncryptor.EncryptStringToBytes("TEST", null, null);
        Assert.Fail();
      }
      catch (ArgumentNullException e)
      {
        Assert.AreEqual("key", e.ParamName);
      }
    }

    [TestMethod]
    public void EncryptStringToBytes_Null_IV_Test()
    {
      StringEncryptor strEncryptor = new StringEncryptor();
      try
      {
        strEncryptor.EncryptStringToBytes("TEST", new byte[1], null);
        Assert.Fail();
      }
      catch (ArgumentNullException e)
      {
        Assert.AreEqual("iv", e.ParamName);
      }
    }

    [TestMethod]
    public void TEncryptStringToBytes_Success_Test()
    {
      //arrange
      string dispatcherAESKey = DispatcherAESKeyByte;
      string dispatcherAESIV = DispatcherAESIVByte;
      string rawPassword = PasswordToBeEncrypted;

      System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

      byte[] keyBytes = encoding.GetBytes(dispatcherAESKey);
      byte[] ivBytes = encoding.GetBytes(dispatcherAESIV);
      IStringEncryptor stringEncryptor = new StringEncryptor();

      //act
      byte[] encryptedStringToByte = stringEncryptor.EncryptStringToBytes(rawPassword, keyBytes, ivBytes);

      //assert
      Assert.IsTrue(encryptedStringToByte.Length > 0);
    }

    [TestMethod]
    public void EncryptStringToBytes_WithInsufficientByteLength_Test()
    {
      //arrange
      string rawPassword = PasswordToBeEncrypted;
      string badDispatcherAESKey = "incorrectLength"; //only 15 chard in length
      string dispatcherAESIVByte = DispatcherAESIVByte;

      System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

      byte[] keyBytes = encoding.GetBytes(badDispatcherAESKey);
      byte[] ivBytes = encoding.GetBytes(dispatcherAESIVByte);
      IStringEncryptor stringEncryptor = new StringEncryptor();

      //act
      byte[] encryptedStringToByte = stringEncryptor.EncryptStringToBytes(rawPassword, keyBytes, ivBytes);

      //assert
      Assert.IsTrue(encryptedStringToByte.Length == 0);
    }

    [TestMethod]
    public void DecryptStringFromBytes_CipherTextNull_ThrowsException_Test()
    {
      //arrange
      byte[] keyBytes = new System.Text.ASCIIEncoding().GetBytes("key");
      byte[] ivBytes = new System.Text.ASCIIEncoding().GetBytes("iv");
      IStringEncryptor stringEncryptor = new StringEncryptor();

      //act
      try
      {
        stringEncryptor.DecryptStringFromBytes(null, keyBytes, ivBytes);
      }
      catch (ArgumentNullException e)
      {
        //assert
        Assert.IsTrue(e.Message.Contains("cipherText"));
      }
    }

    [TestMethod]
    public void DecryptStringFromBytes_CipherTextZeroLength_ThrowsException_Test()
    {
      //arrange
      byte[] cipherText = new byte[0];
      byte[] keyBytes = new System.Text.ASCIIEncoding().GetBytes("key");
      byte[] ivBytes = new System.Text.ASCIIEncoding().GetBytes("iv");
      IStringEncryptor stringEncryptor = new StringEncryptor();

      //act
      try
      {
        stringEncryptor.DecryptStringFromBytes(cipherText, keyBytes, ivBytes);
      }
      catch (ArgumentNullException e)
      {
        //assert
        Assert.IsTrue(e.Message.Contains("cipherText"));
      }
    }

    [TestMethod]
    public void DecryptStringFromBytes_KeyBytesNull_ThrowsException_Test()
    {
      //arrange
      byte[] cipherText = new System.Text.ASCIIEncoding().GetBytes("cipherText");
      byte[] ivBytes = new System.Text.ASCIIEncoding().GetBytes("iv");
      IStringEncryptor stringEncryptor = new StringEncryptor();

      //act
      try
      {
        stringEncryptor.DecryptStringFromBytes(cipherText, null, ivBytes);
      }
      catch (ArgumentNullException e)
      {
        //assert
        Assert.IsTrue(e.Message.Contains("key"));
      }
    }

    [TestMethod]
    public void DecryptStringFromBytes_KeyBytesZeroLength_ThrowsException_Test()
    {
      //arrange
      byte[] cipherText = new System.Text.ASCIIEncoding().GetBytes("cipherText");
      byte[] keyBytes = new byte[0];
      byte[] ivBytes = new System.Text.ASCIIEncoding().GetBytes("iv");
      IStringEncryptor stringEncryptor = new StringEncryptor();

      //act
      try
      {
        stringEncryptor.DecryptStringFromBytes(cipherText, keyBytes, ivBytes);
      }
      catch (ArgumentNullException e)
      {
        //assert
        Assert.IsTrue(e.Message.Contains("key"));
      }
    }

    [TestMethod]
    public void DecryptStringFromBytes_IVBytesNull_ThrowsException_Test()
    {
      //arrange
      byte[] cipherText = new System.Text.ASCIIEncoding().GetBytes("cipherText");
      byte[] keyBytes = new System.Text.ASCIIEncoding().GetBytes("key");
      IStringEncryptor stringEncryptor = new StringEncryptor();

      //act
      try
      {
        stringEncryptor.DecryptStringFromBytes(cipherText, keyBytes, null);
      }
      catch (ArgumentNullException e)
      {
        //assert
        Assert.IsTrue(e.Message.Contains("iv"));
      }
    }

    [TestMethod]
    public void DecryptStringFromBytes_IVBytesZeroLength_ThrowsException_Test()
    {
      //arrange
      byte[] cipherText = new System.Text.ASCIIEncoding().GetBytes("cipherText");
      byte[] keyBytes = new System.Text.ASCIIEncoding().GetBytes("iv");
      byte[] ivBytes = new byte[0];
      IStringEncryptor stringEncryptor = new StringEncryptor();

      //act
      try
      {
        stringEncryptor.DecryptStringFromBytes(cipherText, keyBytes, ivBytes);
      }
      catch (ArgumentNullException e)
      {
        //assert
        Assert.IsTrue(e.Message.Contains("iv"));
      }
    }

    [TestMethod]
    public void DecryptStringFromBytes_Test()
    {
      //arrange
      string rawPassword = PasswordToBeEncrypted;
      string dispatcherAESKey = DispatcherAESKeyByte;
      string dispatcherAESIV = DispatcherAESIVByte;

      System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

      byte[] keyBytes = encoding.GetBytes(dispatcherAESKey);
      byte[] ivBytes = encoding.GetBytes(dispatcherAESIV);
      IStringEncryptor stringEncryptor = new StringEncryptor();

      //act
      byte[] encryptedStringToByte = stringEncryptor.EncryptStringToBytes(rawPassword, keyBytes, ivBytes);
      string decryptedPassword = stringEncryptor.DecryptStringFromBytes(encryptedStringToByte, keyBytes, ivBytes);

      //assert
      Assert.IsTrue(decryptedPassword.Length > 0);
      Assert.IsTrue(rawPassword == decryptedPassword);
    }

    /// <remarks>
    /// See: http://stackoverflow.com/questions/2116607/rijndaelmanaged-padding-is-invalid-and-cannot-be-removed-that-only-occurs-when
    /// </remarks>
    [TestMethod]
    public void DecryptStringFromBytes_WithDifferentEncryptionKeyBytes_Test()
    {
      //arrange
      string rawPassword = PasswordToBeEncrypted;

      //different key bytes than what is used to encrypt 
      string badDispatcherAESKey = "incorrectStringK";

      string dispatcherAESKey = DispatcherAESKeyByte;
      string dispatcherAESIV = DispatcherAESIVByte;

      System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

      byte[] badKeyBytes = encoding.GetBytes(badDispatcherAESKey);

      byte[] goodKeyBytes = encoding.GetBytes(dispatcherAESKey);

      byte[] ivBytes = encoding.GetBytes(dispatcherAESIV);

      IStringEncryptor stringEncryptor = new StringEncryptor();

      //act
      byte[] badEncryptedStringToByte = stringEncryptor.EncryptStringToBytes(rawPassword, badKeyBytes, ivBytes);
      string decryptedPassword = stringEncryptor.DecryptStringFromBytes(badEncryptedStringToByte, goodKeyBytes, ivBytes);

      //assert
      Assert.AreNotSame(rawPassword, decryptedPassword);
    }

    [TestMethod]
    public void EncryptStringFromBytes_WithIncorrectLengthOfEncryptionKeyBytes_Test()
    {
      //arrange
      string rawPassword = PasswordToBeEncrypted;

      string badDispatcherAESKey = "incorrectlength"; //only 15 chars, should be 16

      string dispatcherAESIV = DispatcherAESIVByte;

      System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

      byte[] badKeyBytes = encoding.GetBytes(badDispatcherAESKey);

      byte[] ivBytes = encoding.GetBytes(dispatcherAESIV);

      IStringEncryptor stringEncryptor = new StringEncryptor();

      //act
      byte[] badEncryptedStringToByte = stringEncryptor.EncryptStringToBytes(rawPassword, badKeyBytes, ivBytes);

      //assert
      Assert.AreEqual(badEncryptedStringToByte.Length, 0);
    }

    [TestMethod]
    public void DecryptStringFromBytes_WithIncorrectLengthOfDecryptionKeyBytes_Test()
    {
      //arrange
      string rawPassword = PasswordToBeEncrypted;
      string goodDispatcherAESKey = DispatcherAESKeyByte;
      string badDispatcherAESKey = "incorrectlength"; //only 15 chars, should be 16
      string dispatcherAESIV = DispatcherAESIVByte;

      System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

      byte[] goodKeyBytes = encoding.GetBytes(goodDispatcherAESKey);
      byte[] badKeyBytes = encoding.GetBytes(badDispatcherAESKey);
      byte[] ivBytes = encoding.GetBytes(dispatcherAESIV);
      IStringEncryptor stringEncryptor = new StringEncryptor();

      //act
      byte[] encryptedStringToByte = stringEncryptor.EncryptStringToBytes(rawPassword, goodKeyBytes, ivBytes);
      string badEncryptedStringToByte = stringEncryptor.DecryptStringFromBytes(encryptedStringToByte, badKeyBytes, ivBytes);

      //assert
      Assert.AreEqual(badEncryptedStringToByte.Length, 0);
    }
  }

  public class MyTestClass
  {
    public string a;
    public string b;
    public string c;
  }
}
