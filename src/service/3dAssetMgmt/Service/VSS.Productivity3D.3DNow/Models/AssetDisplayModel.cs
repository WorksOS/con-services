using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.AssetMgmt3D.Models
{
  /// <summary>
  /// Represents a Customer
  /// </summary>
  public class AssetDisplayModel : ContractExecutionResult
  {
    /// <summary>
    /// Unique Identifier for the customer
    /// </summary>
    public IEnumerable<KeyValuePair<Guid,long>> assetIdentifiers { get; set; }

  }
}