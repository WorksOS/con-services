using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class CalculateDesignProfileResponse : BaseRequestBinarizableResponse, IEquatable<CalculateDesignProfileResponse>
  {
    public XYZS[] Profile { get; set; }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      throw new NotImplementedException();
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      throw new NotImplementedException();
    }

    public bool Equals(CalculateDesignProfileResponse other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Equals(Profile, other.Profile);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((CalculateDesignProfileResponse) obj);
    }

    public override int GetHashCode()
    {
      return (Profile != null ? Profile.GetHashCode() : 0);
    }
  }
}
