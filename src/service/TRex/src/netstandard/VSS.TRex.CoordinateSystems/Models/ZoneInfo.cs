namespace VSS.TRex.CoordinateSystems.Models
{
  public class ZoneInfo
  {
    public string OriginType;
    public bool CanRectify;
    public string AzimuthType;
    public double Azimuth;
    public double DenmarkSystem;
    public double OriginLatitude;
    public double OriginLongitude;
    public double OriginNorth;
    public double OriginEast;
    public double OriginScale;
    public double Rotation;
    public string NorthGridFileName;
    public string EastGridFileName;
    public string GridFileName;
    public string ShiftGridFileName;
    public string SnakeGridFileName;
    public double NorthParallel;
    public double SouthParallel;
    public double FerroConstant;
    public bool IsSouthGrid;
    public bool IsWestGrid;
    public bool IsSouthAzimuth;
    public ZoneHorizontalAdjustment HorizontalAdjustment;
    public ZoneVerticalAdjustment VerticalAdjustment;
    public ZoneLocalSiteParameters LocalSiteParameters;
    public ZoneAzimuthPoints AzimuthPoints;
    public Extents Extents;
    public bool IsValid;
    public int ZoneSystemId;
    public string ZoneType;
    public string ZoneName;
    public string ZoneGroupName;
    public int DefaultDatumSystemId;
    public int DefaultGeoidSystemId;
  }
}
