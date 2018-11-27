using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Common
{
  /// <summary>
  /// A base class representing the generic result of requesting subgrids
  /// </summary>
  public class SubGridsPipelinedReponseBase : BaseRequestResponse, ISubGridsPipelinedReponseBase, IEquatable<SubGridsPipelinedReponseBase>
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

    public bool Equals(SubGridsPipelinedReponseBase other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return ResultStatus == other.ResultStatus;
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
