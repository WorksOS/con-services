using System.Collections.Generic;
using System.Diagnostics;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.TAGFiles.GridFabric.Responses
{
  public class ProcessTAGFileResponse : BaseRequestResponse
  {
    private const byte versionNumber = 1;

    public List<ProcessTAGFileResponseItem> Results { get; set; } = new List<ProcessTAGFileResponseItem>();

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public ProcessTAGFileResponse()
    {
    }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(versionNumber);
      writer.WriteInt(Results.Count);
      Results.ForEach(result => result.ToBinary(writer));
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      byte readVersionNumber = reader.ReadByte();

      if (readVersionNumber != versionNumber)
        throw new TRexSerializationVersionException(versionNumber, readVersionNumber);

      for (int i = 0; i < reader.ReadInt(); i++)
      {
        Results.Add(new ProcessTAGFileResponseItem(reader));
      }
    }
  }
}
