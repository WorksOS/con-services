using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models
{
  public class AssetOnDesignPeriod : IEquatable<AssetOnDesignPeriod>
  {
    /// <summary>
    ///This design name comes from the tag file.
    ///  So long as the same tag files have been imported into Trex and Raptor,
    ///     the designNames will be the same in both systems and can be used for matching
    /// </summary>
    [JsonProperty(PropertyName = "designName")]
    public string Name { get; private set; } // todoJeannie add to Trex return and Raptor?

    /// <summary>
    ///The Trex OR Raptor design identifier.
    ///   This is a value unique and internal to each system.
    ///   Use designName for matching between systems
    /// </summary>
    [JsonProperty(PropertyName = "designId")]
    public long Id { get; private set; }

    /// <summary>
    /// Machine identifier that the design is on. Used for filtering in machine details end point only.
    /// </summary>
    [JsonIgnore]
    public long MachineId { get; set; }
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

    /// <summary>
    /// Machine identifier that the design is on. Used for filtering in machine details end point only.
    /// </summary>
    [JsonIgnore]
    public Guid? AssetUid { get; set; }


    public AssetOnDesignPeriod(string name, long id, long machineId, DateTime startDate, DateTime endDate,
      Guid? assetUid = null)
    {
      Id = id;
      Name = name;
      MachineId = machineId;
      StartDate = startDate;
      EndDate = endDate;
      AssetUid = assetUid;
    }

    #region Equality test
    public bool Equals(AssetOnDesignPeriod other)
    {
      if (other == null)
        return false;
      //Note: This is used for the Distinct query to return a unique design list
      //so only want to compare id and name. The other fields are used for details filtering.
      return this.Id == other.Id &&
             this.Name == other.Name;
    }

    public static bool operator ==(AssetOnDesignPeriod a, AssetOnDesignPeriod b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(AssetOnDesignPeriod a, AssetOnDesignPeriod b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is AssetOnDesignPeriod && this == (AssetOnDesignPeriod)obj;
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
