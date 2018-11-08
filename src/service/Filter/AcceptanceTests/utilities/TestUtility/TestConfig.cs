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
    public string operatingSystem = Environment.GetEnvironmentVariable("OS");
    public string coordinateSystem = "00TMSC V10-70       0   11/01/2012 11:25133111" + 
                                     "10TMUntitled Job    122212" + "78TM11" + "D5TM                                                                                                " + 
                                     "D8TM                                " + 
                                     "64TM336.2065553710000-115.026267818000.000000000000003673.708000000007198.081000000000.000000000000001.00008672300000                                " + 
                                     "65TM20925604.4741700298.257222932890" +
                                     "49TM320925604.4741670298.2572235630000.000000000000000.000000000000000.000000000000000.000000000000000.000000000000000.000000000000000.00000000000000" + 
                                     "50TM3933.054270000008174.140120000000.008730000000000.000450000000000.001907997000001.00001301300000" + 
                                     "81TM13673.746000000007198.06011000000190.3735100000000.000085670000000.00000015000000                                " + 
                                     "C8TM4SCS900 Localization             SCS900 Record                   WGS84 Equivalent Datum          ";
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


    public void SetMySqlServer(string dbServerName)
    {
      mySqlServer = dbServerName;
      UpdateConnectionString();
    }


    private void UpdateConnectionString()
    {
      DbConnectionString = $@"server={mySqlServer};database={dbSchema};userid={mySqlUser};password={mySqlPassword};port={mySqlPort};Convert Zero Datetime=True;AllowUserVariables=True;CharSet=utf8mb4;SslMode=None";
    }
  }
}
