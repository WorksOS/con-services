using System.IO;
using System.Text;

namespace MegalodonServer
{
  public static class DumpPacketHelper
  {

    public static void DumpPacket(string content)
    {
      string pathString = @"c:\megalodon\log";
      System.IO.Directory.CreateDirectory(pathString);
      pathString = System.IO.Path.Combine(pathString, "megalodonstring.dat");
      using (StreamWriter w = File.AppendText(pathString))
      {
        w.Write($"{content}\r\n");
      }
    }


    public static void DumpPacket2(string content)
    {
      string pathString = @"c:\megalodon\log";
      System.IO.Directory.CreateDirectory(pathString);
      pathString = System.IO.Path.Combine(pathString, "megalodonstringUTF8.dat");
      using (StreamWriter w = File.AppendText(pathString))
      {
        w.Write($"{content}\r\n");
      }
    }

    public static bool SaveData(ref byte[] Data)
    {
      BinaryWriter Writer = null;
      string Name = @"c:\megalodon\log\megalodonbinary.dat";
      string pathString = @"c:\megalodon\log";
      System.IO.Directory.CreateDirectory(pathString);

      try
      {
        // Create a new stream to write to the file
      //  Writer = new BinaryWriter(File.OpenWrite(Name));
        Writer = new BinaryWriter(File.Open(Name, FileMode.OpenOrCreate));

        // Writer raw data                
        Writer.Write(Data);
        Writer.Flush();
        Writer.Close();
      }
      catch
      {
        //...
        return false;
      }

      return true;
    }


    public static bool SaveData2(ref byte[] Data)
    {
      BinaryWriter Writer = null;
      string Name = @"c:\megalodon\log\megalodonbinary2.dat";
      string pathString = @"c:\megalodon\log";
      System.IO.Directory.CreateDirectory(pathString);

      try
      {
        // Create a new stream to write to the file
        //  Writer = new BinaryWriter(File.OpenWrite(Name));
        Writer = new BinaryWriter(File.Open(Name, FileMode.OpenOrCreate));

        // Writer raw data                
        Writer.Write(Data);
        Writer.Flush();
        Writer.Close();
      }
      catch
      {
        //...
        return false;
      }

      return true;
    }


  }
}
