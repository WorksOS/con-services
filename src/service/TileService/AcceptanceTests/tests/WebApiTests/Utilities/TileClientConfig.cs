using System;

namespace WebApiTests.Utilities
{
  public class TileClientConfig
  {
    public static string TileSvcBaseUri = $"{Environment.GetEnvironmentVariable("TILE_WEBSERVICES_URL")}";
  }
}
