using System;
using Trimble.CsdManagementWrapper;

namespace CoreX.Wrapper.Types
{
  public static class Utils
  {
    public static CppEmbeddedDataCallback EmbeddedDataCallback = new CppEmbeddedDataCallback();
    public static CppFileListCallback FileListCallBack = new CppFileListCallback();
    public const double MISSING_VALUE = -9.99e27;
    public const double MISSING_LIMIT = -9.99e26;
    public const double PI = 3.14159265358979323846;

    public static sbyte[] IntPtrToSByte(IntPtr value, int length)
    {
      byte[] bytes = new byte[length];
      System.Runtime.InteropServices.Marshal.Copy(value, bytes, 0, length);

      // convert byte array to sbyte array
      sbyte[] sbytes = new sbyte[bytes.Length];
      for (int i = 0; i < bytes.Length; i++)
        sbytes[i] = (sbyte)bytes[i];

      return sbytes;
    }
  }
}
