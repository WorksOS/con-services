using Apache.Ignite.Core.Binary;

namespace VSS.TRex.TAGFiles.GridFabric.Responses
{
  public class ProcessTAGFileResponseItem
  {
    public string FileName { get; set; }

    public bool Success { get; set; }

    public string Exception { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public ProcessTAGFileResponseItem()
    {
    }

    /// <summary>
    /// Creates a new item and serialises its content from the supplied IBinaryRawReader
    /// </summary>
    public ProcessTAGFileResponseItem(IBinaryRawReader reader)
    {
      FromBinary(reader);
    }

    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteString(FileName);
      writer.WriteBoolean(Success);
      writer.WriteString(Exception);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      FileName = reader.ReadString();
      Success = reader.ReadBoolean();
      Exception = reader.ReadString();
    }
  }
}

