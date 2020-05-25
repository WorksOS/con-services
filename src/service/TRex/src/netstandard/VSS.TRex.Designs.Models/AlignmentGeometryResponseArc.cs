namespace VSS.TRex.Designs.Models
{
  public class AlignmentGeometryResponseArc
  {
    /// <summary>
    /// Easting of the first point on the arc, expressed in decimal degrees
    /// </summary>
    public double X1 { get; set; }

    /// <summary>
    /// Northing of the first point on the arc, expressed in decimal degrees
    /// </summary>
    public double Y1 { get; set; }

    /// <summary>
    /// Elevation of the arc start point, expressed in meters
    /// </summary>
    public double Z1 { get; set; }

    /// <summary>
    /// Easting of the second point on the arc, expressed in decimal degrees
    /// </summary>
    public double X2 { get; set; }

    /// <summary>
    /// Northing of the second point on the arc, expressed in decimal degrees
    /// </summary>
    public double Y2 { get; set; }

    /// <summary>
    /// Elevation of the arc end arc, expressed in meters
    /// </summary>
    public double Z2 { get; set; }

    /// <summary>
    /// Easting of the center point of the arc, expressed in decimal degrees
    /// </summary>
    public double XC { get; set; }

    /// <summary>
    /// Northing of the center point of the arc, expressed in decimal degrees
    /// </summary>
    public double YC { get; set; }

    /// <summary>
    /// Elevation of the arc center point, expressed in meters
    /// </summary>
    public double ZC { get; set; }

    /// <summary>
    /// Details if the arc moves clockwise from the first point to the second point.
    /// </summary>
    public bool CW { get; set; }

    // ReSharper disable once UnusedMember.Local
    private AlignmentGeometryResponseArc() { }

    public AlignmentGeometryResponseArc(double x1, double y1, double z1, double x2, double y2, double z2, double xc, double yc, double zc, bool cw)
    {
      X1 = x1;
      Y1 = y1;
      Z1 = z1;
      X2 = x2;
      Y2 = y2;
      Z1 = z2;
      XC = xc;
      YC = yc;
      ZC = zc;
      CW = cw;
    }
  }
}
