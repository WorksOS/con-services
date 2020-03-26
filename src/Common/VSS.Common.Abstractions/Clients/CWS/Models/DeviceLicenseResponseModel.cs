using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class DeviceLicenseResponseModel : IMasterDataModel
  {
    public const int FREE_DEVICE_LICENSE = -1;
    /// <summary>
    /// Total number of device licenses. Value -1 means a free license.
    /// </summary>
    [JsonProperty("deviceLicenseCount")]
    public int Total { get; set; }

    public List<string> GetIdentifiers() => new List<string>();

  }
}
