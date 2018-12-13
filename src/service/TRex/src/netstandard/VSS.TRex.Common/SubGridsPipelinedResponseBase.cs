using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Common
{
  /// <summary>
  /// A base class representing the generic result of requesting subgrids
  /// </summary>
  public class SubGridsPipelinedResponseBase : BaseRequestResponse, ISubGridsPipelinedReponseBase
  {
    /// <summary>
    /// The error status result from the pipeline execution
    /// </summary>
    public RequestErrorStatus ResultStatus { get; set; } = RequestErrorStatus.Unknown;

    public override void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteInt((int)ResultStatus);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      ResultStatus = (RequestErrorStatus)reader.ReadInt();
    }
  }
}
