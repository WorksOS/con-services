using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class DesignName : IEquatable<DesignName>
  {
    public string designName { get; set; }
    public long designId { get; set; }

    #region Equality test
    public bool Equals(DesignName other)
    {
      if (other == null)
        return false;

      return this.designId == other.designId &&
             this.designName == other.designName;
    }

    public static bool operator ==(DesignName a, DesignName b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(DesignName a, DesignName b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is DesignName && this == (DesignName)obj;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
    #endregion
  }
}
