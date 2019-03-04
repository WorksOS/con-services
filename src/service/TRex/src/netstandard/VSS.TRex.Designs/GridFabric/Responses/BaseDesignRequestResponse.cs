using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class BaseDesignRequestResponse : BaseRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public DesignProfilerRequestResult RequestResult { get; set; } = DesignProfilerRequestResult.UnknownError;

    public override void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteByte((byte)RequestResult);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      RequestResult = (DesignProfilerRequestResult) reader.ReadByte();
    }
  }
}
