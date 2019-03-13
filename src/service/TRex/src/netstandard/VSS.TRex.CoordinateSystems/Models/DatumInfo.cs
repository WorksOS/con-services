namespace VSS.TRex.CoordinateSystems.Models
{
  public class DatumInfo
  {
    public bool DirectionIsLocalToWGS84;
    public double TranslationX;
    public double TranslationY;
    public double TranslationZ;
    public double RotationX;
    public double RotationY;
    public double RotationZ;
    public double Scale;
    public string LatitudeShiftGridFileName;
    public string LongitudeShiftGridFileName;
    public string HeightShiftGridFileName;
    public string EllipseName;
    public double EllipseA;
    public double EllipseInverseFlat;
    public int EllipseSystemId;
    public string GlobalEllipseName;
    public double GlobalEllipseA;
    public double GlobalEllipseInverseFlat;
    public int GlobalEllipseSystemId;
    public bool IsValid;
    public int DatumSystemId;
    public string DatumType;
    public string DatumName;
    public double Area;
    public Extents Extents;
  }
}
