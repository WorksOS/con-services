namespace VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon
{
  public class TWGS84Point
  {
    // Note: Lat and Lon expressed as radians
    public double Lat;
    public double Lon;

    public TWGS84Point(double ALon, double ALat) { Lat = ALat; Lon = ALon; }
  }
}