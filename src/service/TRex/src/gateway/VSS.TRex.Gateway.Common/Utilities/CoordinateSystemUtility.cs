using System;
using System.IO;
using System.IO.Compression;
using SimpleBase;

namespace VSS.TRex.Gateway.Common.Utilities
{
  public static class CoordinateSystemUtility
  {
    public static string FromCSIBKeyToString(string csibKey)
    {
      var byteArray = FromCSIBKeyToBytes(csibKey);

      if (byteArray == null)
        return string.Empty;

      return Convert.ToBase64String(byteArray);
    }

    public static byte[] FromCSIBKeyToBytes(string csibKey)
    {
      const string NO_KEY = "-1";
      const int BYTES_OFFSET = 4;

      if (csibKey == NO_KEY)
        return null;

      byte[] gzBuffer = Base32.Crockford.Decode(csibKey);

      using (var ms = new MemoryStream())
      {
        int msgLength = BitConverter.ToInt32(gzBuffer, 0);
        ms.Write(gzBuffer, BYTES_OFFSET, gzBuffer.Length - BYTES_OFFSET);

        byte[] buffer = new byte[msgLength];

        ms.Position = 0;
        using (var zip = new GZipStream(ms, CompressionMode.Decompress))
          zip.Read(buffer, 0, buffer.Length);

        return buffer;
      }
    }
  }
}
