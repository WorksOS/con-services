namespace VSS.MasterData.Project.WebAPI.Common.Models.DeviceStatus
{
  public class ConnectedDevice
  {
    public string nickname { get; set; }
    public string serialNumber { get; set; }
    public string model { get; set; }
    public string firmware { get; set; }
    public float batteryPercent { get; set; }
    public string licenseCodes { get; set; }
    public string swWarrantyExpUtc { get; set; }
  }
}
