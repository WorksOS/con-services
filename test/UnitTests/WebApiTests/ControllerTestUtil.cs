using System.IO;

namespace VSS.Raptor.Service.WebApiTests
{
  public class ControllerTestUtil
  { 
    /// <summary>
    /// Converts file contents to an array of bytes...
    /// </summary>
    /// <param name="input">Full file name.</param>
    /// <returns>Contents of the file as an array of bytes.</returns>
    /// 
    public static byte[] FileToByteArray(string input)
    {
      byte[] output = null;

      FileStream sourceFile = new FileStream(input, FileMode.Open, System.IO.FileAccess.Read); // Open streamer...

      BinaryReader binReader = new BinaryReader(sourceFile);
      try
      {
        output = binReader.ReadBytes((int)sourceFile.Length);
      }
      finally
      {
        sourceFile.Close(); // Dispose streamer...          
        binReader.Close(); // Dispose reader
      }

      return output;
    }

  }
}
