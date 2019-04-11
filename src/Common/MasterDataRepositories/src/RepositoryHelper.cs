namespace VSS.MasterData.Repositories
{
  public class RepositoryHelper
  {
    public static string GeometryWKTToSpatial(string geometryWKT)
    {
      return string.IsNullOrEmpty(geometryWKT) ? "null" : $"ST_GeomFromText('{geometryWKT}')";
    }
  }
}
