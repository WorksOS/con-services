using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.MasterData.Models.Models
{
  public class GeofenceListData : BaseDataResult, IMasterDataModel
  {
    public List<GeofenceData> GeofenceData { get; set; }

    public List<string> GetIdentifiers() => GeofenceData?
                                              .SelectMany(g => g.GetIdentifiers())
                                              .Distinct()
                                              .ToList()
                                            ?? new List<string>();
  }
}
