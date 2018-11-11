using System;

namespace TestUtility
{
  /// <summary>
  /// The config class has all variable config settings. This are hard coded or derived from Environment variables. 
  /// </summary>
  public class TestConfig
  {
    public string telematicsTopic = "VSS.VisionLink.Interfaces.Events.Telematics.Machine.";
    public string masterDataTopic = "VSS.Interfaces.Events.MasterData.";
    public string mySqlServer;
    public string mySqlUser = Environment.GetEnvironmentVariable("MYSQL_USERNAME");
    public string mySqlPassword = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD");
    public string mySqlPort = Environment.GetEnvironmentVariable("MYSQL_PORT");
    public string kafkaServer = Environment.GetEnvironmentVariable("KAFKA_URI") + ":" + Environment.GetEnvironmentVariable("KAFKA_PORT");
    public string dbSchema;
    public string kafkaTopicSuffix = Environment.GetEnvironmentVariable("KAFKA_TOPIC_NAME_SUFFIX");
    public string webApiUri = Environment.GetEnvironmentVariable("WEBAPI_URI");
    public string debugWebApiUri = Environment.GetEnvironmentVariable("WEBAPI_DEBUG_URI");

    public string DbConnectionString { get; private set; }

    public TestConfig()
    {
      dbSchema = Environment.GetEnvironmentVariable("MYSQL_DATABASE_NAME");
      mySqlServer = Environment.GetEnvironmentVariable("MYSQL_SERVER_NAME_VSPDB");
      DbConnectionString = $@"server={mySqlServer};database={dbSchema};userid={mySqlUser};password={mySqlPassword};port={mySqlPort};Convert Zero Datetime=True;AllowUserVariables=True;CharSet=utf8mb4;SslMode=None";
    }

    public void SetMySqlDbSchema(string dbSchemaName)
    {
      dbSchema = dbSchemaName;
      UpdateConnectionString();
    }

    private void UpdateConnectionString()
    {
      DbConnectionString = $@"server={mySqlServer};database={dbSchema};userid={mySqlUser};password={mySqlPassword};port={mySqlPort};Convert Zero Datetime=True;AllowUserVariables=True;CharSet=utf8mb4;SslMode=None";
    }
  }
}
