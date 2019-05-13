using System;
using VSS.VisionLink.Interfaces.Events.Notifications.Enums;

namespace VSS.VisionLink.Interfaces.Events.Notifications.Events
{
  public class CreateTagFileErrorEvent
  {
    public Guid TagFileErrorUID { get; set; }

    public string MachineName { get; set; }

    public string DisplaySerialNumber { get; set; }

    public DateTime TagFileCreatedUTC { get; set; }

    public TagFileError ErrorCode { get; set; }

    // following are optional
    public Guid? CustomerUID { get; set; }

    public Guid? AssetUID { get; set; }

    public string DeviceSerialNumber { get; set; }

    public int? DeviceType { get; set; }

    public Guid? ProjectUID { get; set; }

    // following are raw values from TagFileHarvester
    public string TccOrgId { get; set; }

    public long? LegacyAssetId { get; set; }

    public int? LegacyProjectId { get; set; }

    // when was received in TFA service
    public DateTime ActionUTC { get; set; }
  }
}

