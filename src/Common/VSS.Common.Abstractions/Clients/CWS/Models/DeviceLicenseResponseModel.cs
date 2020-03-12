using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  public class DeviceLicenseResponseModel
  {
    public const int FREE_DEVICE_LICENSE = -1;
    /// <summary>
    /// Total number of device licenses. Value -1 means a free license.
    /// </summary>
    [JsonProperty("deviceLicenseCount")]
    public int Total { get; set; }
  }
}
