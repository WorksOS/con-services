namespace VSS.MasterData.Project.WebAPI.Common.Models.DeviceStatus
{
  public class GNSSAntenna
  {
    public string antennaLocation { get; set; }
    public string antennaSerialNumber { get; set; }
    public SatelliteVehicle svsUsed { get; set; }

    /// <summary>
    /// Public default constructor.
    /// </summary>
    public GNSSAntenna() {}

    /// <summary>
    /// Public constructor with parameters.
    /// </summary>
    /// <param name="antenna"></param>
    public GNSSAntenna(GNSSAntenna antenna)
    {
      antennaLocation = antenna.antennaLocation;
      antennaSerialNumber = antenna.antennaSerialNumber;
      svsUsed = new SatelliteVehicle(antenna.svsUsed);
    }
  }

  public class SatelliteVehicle
  {
    public short gps { get; set; }
    public long gln { get; set; }
    public short bds { get; set; }
    public short gal { get; set; }
    public short irnss { get; set; }

    /// <summary>
    /// Public default constructor.
    /// </summary>
    public SatelliteVehicle() {}

    /// <summary>
    /// Public constructor with parameters.
    /// </summary>
    /// <param name="vehicle"></param>
    public SatelliteVehicle(SatelliteVehicle vehicle)
    {
      gps = vehicle.gps;
      gln = vehicle.gln;
      bds = vehicle.bds;
      gal = vehicle.gal;
      irnss = vehicle.irnss;
    }
  }
}
