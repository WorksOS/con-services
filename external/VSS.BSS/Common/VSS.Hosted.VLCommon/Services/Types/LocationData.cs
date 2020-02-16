using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon.Services.Types
{
  public class LocationData
  {
    public double? ServiceMeterHours { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? OdometerMiles { get; set; }
    public DateTime? LocationEventUTC { get; set; }
  }
}
