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

    /// <summary>
    /// Public default constructor.
    /// </summary>
    public ConnectedDevice() {}

    /// <summary>
    /// Public constructor with parameters.
    /// </summary>
    /// <param name="device"></param>
    public ConnectedDevice(ConnectedDevice device)
    {
      nickname = device.nickname;
      serialNumber = device.serialNumber;
      model = device.model;
      firmware = device.firmware;
      batteryPercent = device.batteryPercent;
      licenseCodes = device.licenseCodes;
      swWarrantyExpUtc = device.swWarrantyExpUtc;
    }
  }
}
