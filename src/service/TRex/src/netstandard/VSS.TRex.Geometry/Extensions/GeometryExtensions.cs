namespace VSS.TRex.Geometry
{
  public static class GeometryExtensions
  {
    public static CoreXModels.XYZ ToCoreX_XYZ(this VSS.TRex.Geometry.XYZ coordinates) =>
      new CoreXModels.XYZ(coordinates.X, coordinates.Y, coordinates.Z);

    public static VSS.TRex.Geometry.XYZ ToTRex_XYZ(this CoreXModels.XYZ coordinates) =>
      new VSS.TRex.Geometry.XYZ(coordinates.X, coordinates.Y, coordinates.Z);

    public static VSS.TRex.Common.Models.WGS84Point ToTRex_WGS84Point(this CoreXModels.WGS84Point coordinates) =>
      new VSS.TRex.Common.Models.WGS84Point(coordinates.Lon, coordinates.Lat, coordinates.Height);

    public static CoreXModels.XYZ[] ToCoreX_XYZ(this VSS.TRex.Geometry.XYZ[] coordinates)
    {
      var result = new CoreXModels.XYZ[coordinates.Length];

      for (var i = 0; i < coordinates.Length; i++)
      {
        result[i] = new CoreXModels.XYZ(coordinates[i].X, coordinates[i].Y, coordinates[i].Z);
      }

      return result;
    }

    public static VSS.TRex.Geometry.XYZ[] ToTRex_XYZ(this CoreXModels.XYZ[] coordinates)
    {
      var result = new VSS.TRex.Geometry.XYZ[coordinates.Length];

      for (var i = 0; i < coordinates.Length; i++)
      {
        result[i] = new VSS.TRex.Geometry.XYZ(coordinates[i].X, coordinates[i].Y, coordinates[i].Z);
      }

      return result;
    }

    public static CoreXModels.WGS84Point[] ToCoreX_WGS84Point(this VSS.TRex.Common.Models.WGS84Point[] coordinates)
    {
      var result = new CoreXModels.WGS84Point[coordinates.Length];

      for (var i = 0; i < coordinates.Length; i++)
      {
        result[i] = new CoreXModels.WGS84Point(coordinates[i].Lon, coordinates[i].Lat, coordinates[i].Height);
      }

      return result;
    }

    public static VSS.TRex.Common.Models.WGS84Point[] ToTRex_WGS84Point(this CoreXModels.WGS84Point[] coordinates)
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
