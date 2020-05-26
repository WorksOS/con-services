namespace VSS.MasterData.Repositories
{
  public static class RepositoryHelper
  {
    public static string WKTToSpatial(string geometryWKT)
    {
      return string.IsNullOrEmpty(geometryWKT) ? "null" : $"ST_GeomFromText('{geometryWKT}')";
    }
  }
}
