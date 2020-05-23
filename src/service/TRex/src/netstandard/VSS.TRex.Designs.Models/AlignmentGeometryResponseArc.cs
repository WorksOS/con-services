namespace VSS.TRex.Designs.Models
{
  public class AlignmentGeometryResponseArc
  {
    /// <summary>
    /// Latitude of the first WGS84 point on the arc, expressed in decimal degrees
    /// </summary>
    public double Lat1 { get; set; }

    /// <summary>
    /// Longitude of the first WGS84 point on the arc, expressed in decimal degrees
    /// </summary>
    public double Lon1 { get; set; }

    /// <summary>
    /// Elevation of the arc start point, expressed in meters
    /// </summary>
    public double Elev1 { get; set; }

    /// <summary>
    /// Latitude of the second WGS84 point on the arc, expressed in decimal degrees
    /// </summary>
    public double Lat2 { get; set; }

    /// <summary>
    /// Longitude of the second WGS84 point on the arc, expressed in decimal degrees
    /// </summary>
    public double Lon2 { get; set; }

    /// <summary>
    /// Elevation of the arc end arc, expressed in meters
    /// </summary>
    public double Elev2 { get; set; }

    /// <summary>
    /// Latitude of the center WGS84 point of the arc, expressed in decimal degrees
    /// </summary>
    public double LatC { get; set; }

    /// <summary>
    /// Longitude of the center WGS84 point of the arc, expressed in decimal degrees
    /// </summary>
    public double LonC { get; set; }

    /// <summary>
    /// Elevation of the arc center point, expressed in meters
    /// </summary>
    public double ElevC { get; set; }

    /// <summary>
    /// Details if the arc moves clockwise from the first point to the second point.
    /// </summary>
    public bool CW { get; set; }

    // ReSharper disable once UnusedMember.Local
    private AlignmentGeometryResponseArc() { }

    public AlignmentGeometryResponseArc(double lat1, double lon1, double elev1, double lat2, double lon2, double elev2, double latC, double lonC, double elevC, bool cw)
    {
      Lat1 = lat1;
      Lon1 = lon1;
      Elev1 = elev1;
      Lat2 = lat2;
      Lon2 = lon2;
      Elev1 = elev2;
      LatC = latC;
      LonC = lonC;
      ElevC = elevC;
      CW = cw;
    }
  }
}
