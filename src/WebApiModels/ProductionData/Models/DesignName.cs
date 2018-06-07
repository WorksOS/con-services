using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  public class DesignName : IEquatable<DesignName>
  {
    /// <summary>
    ///The name of the design.
    /// </summary>
    [JsonProperty(PropertyName = "designName")]
    public string Name { get; private set; }
    /// <summary>
    ///The Raptor design identifier.
    /// </summary>
    [JsonProperty(PropertyName = "designId")]
    public long Id { get; private set; }
    /// <summary>
    /// Machine identifier that the design is on. Used for filtering in machine details end point only.
    /// </summary>
    [JsonIgnore]
    public long MachineId { get; private set; }
    /// <summary>
    /// Start date and time for the design on the machine. Used for filtering in machine details end point only.
    /// </summary>
    [JsonIgnore]
    public DateTime StartDate { get; private set; }
    /// <summary>
    /// End date and time for the design on the machine. Used for filtering in machine details end point only.
    /// </summary>
    [JsonIgnore]
    public DateTime EndDate { get; private set; }

    public static DesignName CreateDesignNames(string name, long id, long machineId, DateTime startDate, DateTime endDate)
    {
        return new DesignName
        {
          Id = id,
          Name = name,
          MachineId = machineId,
          StartDate = startDate,
          EndDate = endDate
        };
    }

    #region Equality test
    public bool Equals(DesignName other)
    {
      if (other == null)
        return false;
      //Note: This is used for the Distinct query to return a unique design list
      //so only want to compare id and name. The other fields are used for details filtering.
      return this.Id == other.Id &&
             this.Name == other.Name;
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
      unchecked
      {
        var hashCode = Name == null ? 0 : Name.GetHashCode();
        hashCode = (hashCode * 397) ^ Id.GetHashCode();
        return hashCode;
      }
    }
    #endregion
  }
}
