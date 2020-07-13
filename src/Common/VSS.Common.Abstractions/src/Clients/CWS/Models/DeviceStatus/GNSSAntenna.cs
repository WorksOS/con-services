namespace VSS.Common.Abstractions.Clients.CWS.Models.DeviceStatus
{
  public class GNSSAntenna
  {
    public string antennaLocation { get; set; }
    public string antennaSerialNumber { get; set; }
    public SatelliteVehicle svsUsed { get; set; }

    public GNSSAntenna() { }
  }

  public class SatelliteVehicle
  {
    public long? gps { get; set; }
    public long? gln { get; set; }
    public long? bds { get; set; }
    public long? gal { get; set; }
    public long? irnss { get; set; }

    public SatelliteVehicle() { }
  }
}
