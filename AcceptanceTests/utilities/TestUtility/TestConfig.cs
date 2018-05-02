using System;

namespace TestUtility
{
  /// <summary>
  /// The config class has all variable config settings. This are hard coded or derived from Environment variables. 
  /// </summary>
  public class TestConfig
  {
    public string webApiUri = Environment.GetEnvironmentVariable("WEBAPI_URI");
    public string debugWebApiUri = Environment.GetEnvironmentVariable("WEBAPI_DEBUG_URI");
    public string operatingSystem = Environment.GetEnvironmentVariable("OS");
    public string vetaExportUrl = Environment.GetEnvironmentVariable("VETA_EXPORT_URL");

    public TestConfig()
    {
    }

 
  }
}
