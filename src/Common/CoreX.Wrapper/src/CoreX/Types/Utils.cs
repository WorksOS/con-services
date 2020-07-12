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
      var bytes = new byte[length];
      System.Runtime.InteropServices.Marshal.Copy(value, bytes, 0, length);

      return (sbyte[]) (Array) bytes;
    }
  }
}
