using System.Collections.Generic;
using System.Linq;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  public class AssetDataResult : BaseDataResult, IMasterDataModel
  {
    public List<AssetData> Assets { get; set; }

    public List<string> GetIdentifiers() =>
      Assets?
        .SelectMany(c => c.GetIdentifiers())
        .Distinct()
        .ToList() ?? new List<string>();
  }
}
