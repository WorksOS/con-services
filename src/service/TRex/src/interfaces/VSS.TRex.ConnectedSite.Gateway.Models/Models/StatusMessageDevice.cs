using System;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.ConnectedSite.Gateway.Abstractions;
using VSS.VisionLink.Interfaces.Events.Commands.MTS;

namespace VSS.TRex.ConnectedSite.Gateway.Models
{
  public class StatusMessageDevice : IStatusMessageDevice
  {
    private string _model;
    public string Model
    {
      get => _model == "torch" ? "SNM94x" : _model;
      set => _model = value;
    }

    public string SerialNumber { get; set; }
    public string Nickname { get; set; }
    public string Firmware { get; set; }
    public int? BatteryPercent { get; set; }
    public string LicenseCodes { get; set; }
    public DateTime? WarrantyExpirationUtc { get; set; }
  }
}
