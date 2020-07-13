namespace VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus
{
  public class GNSSAntenna
  {
    public string antennaLocation { get; set; }
    public string antennaSerialNumber { get; set; }
    public SatelliteVehicle svsUsed { get; set; }

    public GNSSAntenna()
    {
    }
  }
}

