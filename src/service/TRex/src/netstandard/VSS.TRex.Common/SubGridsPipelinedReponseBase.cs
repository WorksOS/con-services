using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Common
{
  /// <summary>
  /// A base class representing the generic result of requesting subgrids
  /// </summary>
  public class SubGridsPipelinedReponseBase : BaseRequestResponse, ISubGridsPipelinedReponseBase, IEquatable<BaseRequestResponse>
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

    protected bool Equals(SubGridsPipelinedReponseBase other)
    {
      return ResultStatus == other.ResultStatus;
    }

    public bool Equals(BaseRequestResponse other)
    {
      return Equals(other as SubGridsPipelinedReponseBase);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((SubGridsPipelinedReponseBase) obj);
    }

    public override int GetHashCode()
    {
      return (int) ResultStatus;
    }
  }
}
