namespace VSS.TagFileAuth.Service.WebApiModels.RaptorServicesCommon
{
  public class TWGS84Point
  {
    // Note: Lat and Lon expressed as radians
    public double Lat;
    public double Lon;

    public void Point(double ALon, double ALat) { Lat = ALat; Lon = ALon; }

    // PointXY is provided as a way to assign a grid coordinate into
    // this structure if required.
    public void PointXY(double AX, double AY) { Lat = AY; Lon = AX; }

    //private bool Encode(const Stream : IStream){;};
    //private bool Decode(const Stream : TStream){;};
  };
}