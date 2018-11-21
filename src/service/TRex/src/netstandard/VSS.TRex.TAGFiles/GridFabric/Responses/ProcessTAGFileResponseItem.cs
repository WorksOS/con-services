using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.TAGFiles.GridFabric.Responses
{
  public class ProcessTAGFileResponseItem
  {
    private const byte kVersionNumber = 1;

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
      writer.WriteByte(kVersionNumber);
      writer.WriteString(FileName);
      writer.WriteBoolean(Success);
      writer.WriteString(Exception);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      byte readVersionNumber = reader.ReadByte();

      if (readVersionNumber != kVersionNumber)
        throw new TRexSerializationVersionException(kVersionNumber, readVersionNumber);

      FileName = reader.ReadString();
      Success = reader.ReadBoolean();
      Exception = reader.ReadString();
    }
  }
}

