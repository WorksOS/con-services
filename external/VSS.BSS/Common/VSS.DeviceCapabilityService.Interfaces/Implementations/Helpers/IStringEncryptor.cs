
namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers
{
  public interface IStringEncryptor
  {
    byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv);
    string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv);
  }
}