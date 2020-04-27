using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace VSS.Productivity3D.Project.Abstractions.Models
{
  /// <summary>
  /// Represents pairs of identifiers of assets 
  /// </summary>
  public class DeviceMatchingModel : ContractExecutionResult, IMasterDataModel
  {
    /// <summary>
    /// Unique Identifier for the customer
    /// </summary>
    public IEnumerable<KeyValuePair<Guid, long>> deviceIdentifiers { get; set; }

    /// <summary>
    /// Convert a List of Device Database Models to Result Model, validating the Guid string can be parsed
    /// </summary>
    public static DeviceMatchingModel FromDeviceList(IEnumerable<Device> devices)
    {
      var results = devices.Select(d => Guid.TryParse(d.DeviceUID, out var g)
                                     ? new KeyValuePair<Guid, long>(g, d.ShortRaptorAssetID)
                                     : new KeyValuePair<Guid, long>(Guid.Empty, d.ShortRaptorAssetID)).ToList();

      return new DeviceMatchingModel
      {
        deviceIdentifiers = results
      };
    }

    public List<string> GetIdentifiers()
    {
      return deviceIdentifiers?
               .Select(a => a.Key.ToString())
               .ToList()
             ?? new List<string>();
    }
  }
}
