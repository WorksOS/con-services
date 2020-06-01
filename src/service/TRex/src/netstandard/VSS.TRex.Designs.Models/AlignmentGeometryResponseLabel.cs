namespace VSS.TRex.Designs.Models
{
  public class AlignmentGeometryResponseLabel
  {
    /// <summary>
    /// Measured (as in walked) distance along the alignment from the start of the alignment, expressed in meters
    /// </summary>
    public double Station { get; set; }

    /// <summary>
    /// Contains the Easting of the text insertion position
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Contains the Northing of the text insertion position
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Text rotation expressed as a survey angle (north is 0, increasing clockwise), in decimal degrees.
    /// </summary>
    public double Rotation { get; set; }

    public AlignmentGeometryResponseLabel() { }

    public AlignmentGeometryResponseLabel(double station, double x, double y, double rotation)
    {
      Station = station;
      X = x;
      Y = y;
      Rotation = rotation;
    }
  }
}
