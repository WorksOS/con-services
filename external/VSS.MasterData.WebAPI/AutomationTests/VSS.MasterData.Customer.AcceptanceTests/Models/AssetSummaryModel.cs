using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.AssetService.AcceptanceTests.Models
{

  public class AssetSummaryModel
  {
    public Guid AssetUID { get; set; }
    public string AssetName { get; set; }
    public string SerialNumber { get; set; }
    public string MakeCode { get; set; }
    public string Model { get; set; }
    public string Family { get; set; }
    public int IconKey { get; set; }
    public string DeviceId { get; set; }
    public string DeviceType { get; set; }
    public Guid DeviceUID { get; set; }
  }
}
