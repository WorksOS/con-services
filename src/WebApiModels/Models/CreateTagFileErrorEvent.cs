// todo move to VSS.VisionLink.Interfaces
using System;

namespace VSS.VisionLink.Interfaces.Events.TagFile
{
  public class CreateTagFileErrorEvent : ITagFileEvent
  {
    public Guid TagFileErrorUID { get; set; }

    public Guid CustomerUID { get; set; }

    public string MachineName { get; set; }
    public string DisplaySerialNumber { get; set; }

    public DateTime TagFileCreatedUTC { get; set; }

    public TagFileError ErrorCode { get; set; }

    public Guid? AssetUID { get; set; }

    // optional
    public string DeviceSerialNumber { get; set; }

    public Guid? ProjectUID { get; set; }

    // when received in TFA service
    public DateTime ActionUTC { get; set; }

    // todo do we need to keep this?
    public DateTime ReceivedUTC { get; set; }
  }
}
