using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  public class DesignNames : IEquatable<DesignNames>
  {
    /// <summary>
    ///The name of the design.
    /// </summary>
    public string designName { get; private set; }
    /// <summary>
    ///The Raptor design identifier.
    /// </summary>
    public long designId { get; private set; }
    /// <summary>
    /// Machine identifier that the design is on. Used for filtering in machine details end point only.
    /// </summary>
    [JsonIgnore]
    public long machineId { get; private set; }
    /// <summary>
    /// Start date and time for the design on the machine. Used for filtering in machine details end point only.
    /// </summary>
    [JsonIgnore]
    public DateTime startDate { get; private set; }
    /// <summary>
    /// End date and time for the design on the machine. Used for filtering in machine details end point only.
    /// </summary>
    [JsonIgnore]
    public DateTime endDate { get; private set; }

    public static DesignNames CreateDesignNames(string name, long id, long machineId, DateTime startDate, DateTime endDate)
    {
        return new DesignNames
        {
          designId = id,
          designName = name,
          machineId = machineId,
          startDate = startDate,
          endDate = endDate
        };
    }

    #region Equality test
    public bool Equals(DesignNames other)
    {
      if (other == null)
        return false;
      //Note: This is used for the Distinct query to return a unique design list
      //so only want to compare id and name. The other fields are used for details filtering.
      return this.designId == other.designId &&
             this.designName == other.designName;
    }

    public static bool operator ==(DesignNames a, DesignNames b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(DesignNames a, DesignNames b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is DesignNames && this == (DesignNames)obj;
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = designName == null ? 0 : designName.GetHashCode();
        hashCode = (hashCode * 397) ^ designId.GetHashCode();
        return hashCode;
      }
    }
    #endregion
  }
}
