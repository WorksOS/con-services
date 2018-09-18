using System;
using System.Configuration;
using System.Reflection;

namespace WebApiTests.Utilities
{
  public class TileClientConfig
  {
    public static Configuration DllConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

    public static string TileSvcBaseUri = $"{Environment.GetEnvironmentVariable("TILE_WEBSERVICES_URL")}";

  }
}
