using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.AssetMgmt3D.Models
{
  /// <summary>
  /// Represents pairs of identifiers of assets 
  /// </summary>
  public class AssetDisplayModel : ContractExecutionResult, IMasterDataModel
  {
    /// <summary>
    /// Unique Identifier for the customer
    /// </summary>
    public IEnumerable<KeyValuePair<Guid, long>> assetIdentifiers { get; set; }

    public List<string> GetIdentifiers()
    {
      return assetIdentifiers?
               .Select(a => a.Key.ToString())
               .ToList()
             ?? new List<string>();
    }
  }
}