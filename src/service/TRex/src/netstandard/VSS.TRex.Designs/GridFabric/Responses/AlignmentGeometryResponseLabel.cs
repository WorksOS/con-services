namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class AlignmentGeometryResponseLabel
  {
    /// <summary>
    /// Measured (as in walked) distance along the alignment from the start of the alignment, expressed in meters
    /// </summary>
    public double Station { get; set; }

    /// <summary>
    /// Contains the WGS84 latitude (expressed decimal degrees) of the test insertion position
    /// </summary>
    public double Lat { get; set; }

    /// <summary>
    /// Contains the WGS84 longitude (expressed decimal degrees) of the test insertion position
    /// </summary>
    public double Lon { get; set; }

    /// <summary>
    /// Text rotation expressed as a survey angle (north is 0, increasing clockwise), in decimal degrees.
    /// </summary>
    public double Rotation { get; set; }

    private AlignmentGeometryResponseLabel() { }

    public AlignmentGeometryResponseLabel(double station, double lat, double lon, double rotation)
    {
      Station = station;
      Lat = lat;
      Lon = lon;
      Rotation = rotation;
    }
  }
}
