using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  public class GeofenceDataResult : IMasterDataModel
  {
    public List<GeofenceData> Geofences { get; set; }

    public List<string> GetIdentifiers() => Geofences?
                                         .SelectMany(g => g.GetIdentifiers())
                                         .Distinct()
                                         .ToList() 
                                       ?? new List<string>();
  }
}
