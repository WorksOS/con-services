using System.Collections.Generic;

namespace VSS.MasterData.Models.Models
{
  public class AssetDevice
  {
    public string DeviceUid { get; set; }
    public string DeviceType { get; set; }
    public string DeviceSerialNumber { get; set; }
    public string DeviceState { get; set; }
    public List<AssetActiveServicePlan> ActiveServicePlans { get; set; }
  }
}