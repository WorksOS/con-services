namespace VSS.MasterData.Project.WebAPI.Common.Models.DeviceStatus
{
  public class GNSSAntenna
  {
    public string antennaLocation { get; set; }
    public string antennaSerialNumber { get; set; }
    public SatelliteVehicle svsUsed { get; set; }
  }

  public class SatelliteVehicle
  {
    public short gps { get; set; }
    public long gln { get; set; }
    public short bds { get; set; }
    public short gal { get; set; }
    public short irnss { get; set; }
  }
}
