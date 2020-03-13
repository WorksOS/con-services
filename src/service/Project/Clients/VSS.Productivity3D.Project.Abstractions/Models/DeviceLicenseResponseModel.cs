using Newtonsoft.Json;

namespace VSS.Productivity3D.Project.Abstractions.Models
{

  // todoMaveric temp should use from CWSClient
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
