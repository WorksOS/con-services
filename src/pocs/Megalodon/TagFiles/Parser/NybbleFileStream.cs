using System.IO;

namespace TagFiles.Parser
{
  public class NybbleFileStream : NybbleStream
  {
    private string FilePath;

    public NybbleFileStream(string path, FileMode fileMode)
    {
      FilePath = path;
      if (stream != null)
        stream.Close();
      if (fileMode == FileMode.Open)
      {
        if (File.Exists(path))
        {
          stream = new FileStream(path, FileMode.Open);
          StreamSizeInNybbles = stream.Length * 2;
        }
      }
      else
      {
        stream = new FileStream(path, fileMode); // typically FileMode.Create
        StreamSizeInNybbles = 0;
      }
    }

  }
}
