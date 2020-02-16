namespace VSS.Nighthawk.NHDataSvc.DataAccess
{
  internal interface IConnectionConfig
  {
    bool HasConfigError { get; set; }

    string GetUserName(string keyUser = "RabbitMqUser");
    string GetPassword(string keyPassword = "RabbitMqPassword");
    ushort GetHeartbeatSeconds();
    string ConnectionString(string csName = "NH_RABBITMQv2", string keyVirtualHost = "RabbitMqVirtualHost", string keyQName = "RabbitMqName");
  }
}
