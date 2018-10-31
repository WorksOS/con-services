namespace VSS.TRex.CoordinateSystems.Models
{
  public class DatumInfo
  {
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
