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
    public string OnMachineDesignName { get; private set; } 

    /// <summary>
    ///The Trex OR Raptor design identifier.
    ///   This is a value unique and internal to each system.
    ///        Eventually this should be phased out, but until Raptor is reworked
    ///   Use designName for matching between systems
    /// This will be obsolete sooon....
    /// </summary>
    [JsonProperty(PropertyName = "designId")]
    public long OnMachineDesignId { get; private set; }

    /// <summary>
    /// Machine identifier that the design is on (Raptor). 
    /// </summary>
    [JsonProperty(PropertyName = "machineId")]
    public long MachineId { get; set; }
    /// <summary>
    /// Start date and time for the design on the machine. 
    /// </summary>
    [JsonProperty(PropertyName = "startDate")]
    public DateTime StartDate { get; private set; }
    /// <summary>
    /// End date and time for the design on the machine. 
    /// </summary>
    [JsonProperty(PropertyName = "endDate")]
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Machine identifier that the design is on (TRex). 
    /// </summary>
    [JsonProperty(PropertyName = "assetUid")]
    public Guid? AssetUid { get; set; }


    public AssetOnDesignPeriod(string onMachineDesignName, long onMachineDesignId, long machineId, DateTime startDate, DateTime endDate,
      Guid? assetUid = null)
    {
      OnMachineDesignId = onMachineDesignId;
      OnMachineDesignName = onMachineDesignName;
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
      //so only want to compare onMachineDesignId and name. The other fields are used for details filtering.
      return OnMachineDesignId == other.OnMachineDesignId &&
             OnMachineDesignName == other.OnMachineDesignName;
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
        var hashCode = OnMachineDesignName == null ? 0 : OnMachineDesignName.GetHashCode();
        hashCode = (hashCode * 397) ^ OnMachineDesignId.GetHashCode();
        return hashCode;
      }
    }
    #endregion
  }
}
