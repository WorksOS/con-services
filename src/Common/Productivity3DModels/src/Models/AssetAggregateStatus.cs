using System;
using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Models.Models
{
  public class AssetAggregateStatus : IMasterDataModel
  {
    public Guid CustomerUid { get; set; }

    public Guid ProjectUid { get; set; }

    public Guid? AssetUid { get; set; } 

    

    public DateTime? LocationLastUpdatedUtc { get; set; }

    public double? Longitude { get; set; }

    public double? Latitude { get; set; }

    public string Design { get; set; }

    public double? FuelLevel { get; set; }

    public DateTime? FuelLevelLastUpdatedUtc { get; set; }
    public int AssetIcon { get; set; }
    public int? LiftNumber { get; set; }
    public string DeviceName { get; set; }

    /// <summary>
    /// Not to be confused with AssetId Primary Key
    /// </summary>
    public string AssetIdentifier { get; set; }

    public AssetSummary UtilizationSummary { get; set; }
    public string MachineName { get; set; }
    public string AssetSerialNumber { get; set; }

    public List<string> GetIdentifiers()
    {
      return new List<string>()
      {
        CustomerUid.ToString(),
        ProjectUid.ToString(), 
        AssetUid.ToString()
      };
    }
  }
}
