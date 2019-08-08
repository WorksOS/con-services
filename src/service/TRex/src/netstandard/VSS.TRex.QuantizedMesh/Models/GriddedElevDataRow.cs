namespace VSS.TRex.QuantizedMesh.Models
{
  public struct GriddedElevDataRow
  {
    public double Northing { get; set; }
    public double Easting { get; set; }
    public float Elevation { get; set; }

    public GriddedElevDataRow(double northing, double easting, float elevation)
    {
      Northing = northing;
      Easting = easting;
      Elevation = elevation;
    }

  }
}
