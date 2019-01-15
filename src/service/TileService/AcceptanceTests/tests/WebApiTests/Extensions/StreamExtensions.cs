using System.IO;

namespace WebApiTests.Extensions
{
  public static class StreamExtensions
  {
    public static byte[] ToByteArray(this Stream stream)
    {
      using (var ms = new MemoryStream())
      {
        if(stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

        stream.CopyTo(ms);
        return ms.ToArray();
      }
    }
  }
}
