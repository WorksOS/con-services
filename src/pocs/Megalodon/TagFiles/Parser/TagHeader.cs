using TagFiles.Utils;

namespace TagFiles.Parser
{

  /// <summary>
  /// Header record for tagfile
  /// </summary>
  public class TagHeader
  {
    public string TagfileName;
    public long TypeTableOffset = 0;
    public byte FileMajor;
    public byte FileMinor;
    public uint DictionaryID;
    public byte DictionaryMajor;
    public byte DictionaryMinor;

    public TagHeader()
    {
      TagfileName = TagUtils.MakeTagfileName("", ""); // setup default
    }

    public void UpdateTagfileName(string serial, string machineID)
    {
      TagfileName = TagUtils.MakeTagfileName(serial, machineID);
    }

    public void Read(NybbleStream stream)
    {
      FileMajor = stream.ReadNybble();
      FileMinor = stream.ReadNybble();
      DictionaryID = stream.ReadUnSignedIntegerValue(4);
      DictionaryMajor = stream.ReadNybble();
      DictionaryMinor = stream.ReadNybble();
      TypeTableOffset = stream.ReadUnSignedIntegerValue(8);
    }

    public void Write(NybbleStream stream)
    {
      FileMajor = 1;
      FileMinor = 0;
      DictionaryID = 1;
      DictionaryMajor = 1;
      DictionaryMinor = 4;

      stream.WriteNybble(FileMajor);
      stream.WriteNybble(FileMinor);
      stream.WriteFixedSizeUnsignedInt(DictionaryID,4); 
      stream.WriteNybble(DictionaryMajor);
      stream.WriteNybble(DictionaryMinor);
      stream.WriteFixedSizeUnsignedInt((uint)TypeTableOffset,8); 

    }

    public string ToDisplay()
    {
      return $"FileMajor:{FileMajor}, FileMinor:{FileMinor}, DictionaryID:{FileMinor}, DictionaryMajor:{DictionaryMajor}, DictionaryMinor:{DictionaryMinor}, TypeTableOffset:{TypeTableOffset}";
    }

  }

}
