using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Common
{
  /// <summary>
  /// A base class representing the generic result of requesting sub grids
  /// </summary>
  public class SubGridsPipelinedResponseBase : BaseRequestResponse, ISubGridsPipelinedReponseBase
  {
    private static byte VERSION_NUMBER = 1;

    /// <summary>
    /// The error status result from the pipeline execution
    /// </summary>
    public RequestErrorStatus ResultStatus { get; set; } = RequestErrorStatus.Unknown;

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)ResultStatus);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        ResultStatus = (RequestErrorStatus) reader.ReadInt();
      }
    }
  }
}
