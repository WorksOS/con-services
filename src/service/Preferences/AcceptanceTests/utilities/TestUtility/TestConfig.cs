using System;

namespace TestUtility
{
  /// <summary>
  /// The config class has all variable config settings. This are hard coded or derived from Environment variables. 
  /// </summary>
  public class TestConfig
  {
    public string mySqlServer;
    public string mySqlUser = Environment.GetEnvironmentVariable("MYSQL_USERNAME");
    public string mySqlPassword = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD");
    public string mySqlPort = Environment.GetEnvironmentVariable("MYSQL_PORT");
    public string dbSchema;
    public string webApiUri = Environment.GetEnvironmentVariable("WEBAPI_URI");
    public string debugWebApiUri = "http://localhost:5000/"; // Environment.GetEnvironmentVariable("WEBAPI_DEBUG_URI");
    public string operatingSystem = Environment.GetEnvironmentVariable("OS");
    public string DbConnectionString { get; private set; }

    public TestConfig(string databaseSchemaName = null)
    {
      dbSchema = !string.IsNullOrEmpty(databaseSchemaName)
        ? databaseSchemaName
        : Environment.GetEnvironmentVariable("MYSQL_DATABASE_NAME");

      dbSchema = "CCSS-Project";

      mySqlServer = Environment.GetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB");

      UpdateConnectionString();
    }

    private void UpdateConnectionString()
    {
      DbConnectionString = $@"server={mySqlServer};database={dbSchema};userid={mySqlUser};password={mySqlPassword};port={mySqlPort};Convert Zero Datetime=True;AllowUserVariables=True;CharSet=utf8mb4;SslMode=None";
    }
  }
}
