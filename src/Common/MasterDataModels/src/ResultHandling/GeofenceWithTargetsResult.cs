using System.Collections.Generic;
using System.Linq;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  public class GeofenceWithTargetsResult : IMasterDataModel
  {
    public List<GeofenceWithTargetsData> Results;

    public List<string> GetIdentifiers() => Results?
                                              .SelectMany(g => g.GetIdentifiers())
                                              .Distinct()
                                              .ToList()
                                            ?? new List<string>();
  }
}
