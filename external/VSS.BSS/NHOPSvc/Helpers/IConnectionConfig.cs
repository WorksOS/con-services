namespace VSS.Nighthawk.NHOPSvc.Helpers
{
  public interface IConnectionConfig
  {
    bool HasConfigError { get; set; }

    string GetUserName(string keyUser = "RabbitMqUser");
    string GetPassword(string keyPassword = "RabbitMqPassword");
    ushort GetHeartbeatSeconds();
    string ConnectionString(string csName = "NH_RABBITMQ", string keyVirtualHost = "RabbitMqVirtualHost", string keyQName = "RabbitMqName");
  }
}
