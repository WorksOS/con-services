namespace VSS.TRex.Geometry
{
  public static class GeometryExtensions
  {
    public static CoreX.Models.XYZ ToCoreX_XYZ(this VSS.TRex.Geometry.XYZ coordinates) =>
      new CoreX.Models.XYZ(coordinates.X, coordinates.Y, coordinates.Z);

    public static VSS.TRex.Geometry.XYZ ToTRex_XYZ(this CoreX.Models.XYZ coordinates) =>
      new VSS.TRex.Geometry.XYZ(coordinates.X, coordinates.Y, coordinates.Z);

    public static VSS.TRex.Common.Models.WGS84Point ToTRex_WGS84Point(this CoreX.Models.WGS84Point coordinates) =>
      new VSS.TRex.Common.Models.WGS84Point(coordinates.Lon, coordinates.Lat, coordinates.Height);

    public static CoreX.Models.XYZ[] ToCoreX_XYZ(this VSS.TRex.Geometry.XYZ[] coordinates)
    {
      var result = new CoreX.Models.XYZ[coordinates.Length];

      for (var i = 0; i < coordinates.Length; i++)
      {
        result[i] = new CoreX.Models.XYZ(coordinates[i].X, coordinates[i].Y, coordinates[i].Z);
      }

      return result;
    }

    public static VSS.TRex.Geometry.XYZ[] ToTRex_XYZ(this CoreX.Models.XYZ[] coordinates)
    {
      var result = new VSS.TRex.Geometry.XYZ[coordinates.Length];

      for (var i = 0; i < coordinates.Length; i++)
      {
        result[i] = new VSS.TRex.Geometry.XYZ(coordinates[i].X, coordinates[i].Y, coordinates[i].Z);
      }

      return result;
    }

    public static CoreX.Models.WGS84Point[] ToCoreX_WGS84Point(this VSS.TRex.Common.Models.WGS84Point[] coordinates)
    {
      var result = new CoreX.Models.WGS84Point[coordinates.Length];

      for (var i = 0; i < coordinates.Length; i++)
      {
        result[i] = new CoreX.Models.WGS84Point(coordinates[i].Lon, coordinates[i].Lat, coordinates[i].Height);
      }

      return result;
    }

    public static VSS.TRex.Common.Models.WGS84Point[] ToTRex_WGS84Point(this CoreX.Models.WGS84Point[] coordinates)
    {
      var result = new VSS.TRex.Common.Models.WGS84Point[coordinates.Length];

      for (var i = 0; i < coordinates.Length; i++)
      {
        result[i] = new Common.Models.WGS84Point(coordinates[i].Lon, coordinates[i].Lat, coordinates[i].Height);
      }

      return result;
    }
  }
}
