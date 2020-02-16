using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace VSS.Nighthawk.NHDataSvc.DataAccess
{
  internal static class ConfigConstants
  {
    public const string UserPh = "XXXUSERXXX";
    public const string PasswordPh = "XXXPASSWORDXXX";
    public const string MqHostPh = "XXXMQHOSTXXX";
    public const string MqNamePh = "XXXMQNAMEXXX";
  }

  /// <summary>
  /// RabbitMQ client connection strings base on the following base definition:
  /// rabbitmq://account:password@sjc3-nhd-appv01.vss-eng.com:8002/VirtualHost/QueueName
  /// Connection string should be configured in machine config file specificly for each environment in this format:
  /// connectionStrings
  ///   add name="NH_RABBITMQ" connectionString="rabbitmq://XXXUSERXXX:XXXPASSWORDXXX@sjc3-nhd-appv01.vss-eng.com:8002/XXXMQHOSTXXX/XXXMQNAMEXXX" /
  /// /connectionStrings
  /// Each endpoint managed by appSettings:
  /// appSettings
  ///    add key="RabbitMqUser" value="_NHDataOut" /
  ///    add key="RabbitMqPassword" value="~d4t40ut" /
  ///    add key="RabbitMqVirtualHost" value="Sandbox" /
  ///    add key="RabbitMqName" value="HA.DataOut" /
  /// /appSettings
  /// </summary>
  internal class RabbitMqConnectionConfig : IConnectionConfig
  {
    public bool HasConfigError { get; set; }

    public RabbitMqConnectionConfig()
    {
      HasConfigError = false;
    }

    #region Connection String util

    public string ConnectionString(string csName = "NH_RABBITMQv2", string keyVirtualHost = "RabbitMqVirtualHost", string keyQName = "RabbitMqName")
    {
      // Get the ConnectionStrings collection.
      string connString = ConfigurationManager.ConnectionStrings[csName].ConnectionString;

      if (string.IsNullOrWhiteSpace(connString))
      {
        HasConfigError = true;
        return connString;
      }

      if (!connString.Contains(ConfigConstants.MqNamePh))
      {
        HasConfigError = true;
        return connString;
      }

      string machineRabbitMqVirtualHost = "[" + Environment.MachineName + "]" + keyVirtualHost;
      string machineRabbitMqName = "[" + Environment.MachineName + "]" + keyQName;

      // This is done like this mainly so a developer can force the db connection string to contain their own creds, for debugging purposes.
      string rabbitMqVirtualHost = ConfigurationManager.AppSettings[machineRabbitMqVirtualHost] ?? ConfigurationManager.AppSettings[keyVirtualHost];
      string rabbitMqName = ConfigurationManager.AppSettings[machineRabbitMqName] ?? ConfigurationManager.AppSettings[keyQName];

      connString = connString.Replace(ConfigConstants.MqHostPh, rabbitMqVirtualHost);
      connString = connString.Replace(ConfigConstants.MqNamePh, rabbitMqName);

      return connString;
    }

    #endregion

    public string GetUserName(string keyUser = "RabbitMqUser")
    {
      string machineRabbitMqUser = "[" + Environment.MachineName + "]" + keyUser;

      // This is done like this mainly so a developer can force the db connection string to contain their own creds, for debugging purposes.
      string dataOutRabbitMqUser = ConfigurationManager.AppSettings[machineRabbitMqUser] ?? ConfigurationManager.AppSettings[keyUser];

      if (string.IsNullOrWhiteSpace(dataOutRabbitMqUser))
      {
        throw new InvalidOperationException(string.Format("Your application is attempting to use the RabbitMQ but missing 'RabbitMqUser' credentials in it's app.config"));
      }

      return dataOutRabbitMqUser;
    }

    public string GetPassword(string keyPassword = "RabbitMqPassword")
    {
      string machineRabbitMqPassword = "[" + Environment.MachineName + "]" + keyPassword;

      // This is done like this mainly so a developer can force the db connection string to contain their own creds, for debugging purposes.
      string dataOutRabbitMqPassword = ConfigurationManager.AppSettings[machineRabbitMqPassword] ?? ConfigurationManager.AppSettings[keyPassword];

      if (string.IsNullOrWhiteSpace(dataOutRabbitMqPassword))
      {
        throw new InvalidOperationException(string.Format("Your application is attempting to use the RabbitMQ but is missing 'RabbitMqPassword' credentials in it's app.config"));
      }

      return dataOutRabbitMqPassword;
    }

    public ushort GetHeartbeatSeconds()
    {
      string machineRabbitMqHeartbeatSeconds = "[" + Environment.MachineName + "]" + "RabbitMqHeartbeatSeconds";

      // This is done like this mainly so a developer can force the db connection string to contain their own creds, for debugging purposes.
      string heartbearString = ConfigurationManager.AppSettings[machineRabbitMqHeartbeatSeconds] ?? ConfigurationManager.AppSettings["RabbitMqHeartbeatSeconds"];

      ushort heartbeat;
      if (string.IsNullOrWhiteSpace(heartbearString) || !ushort.TryParse(heartbearString, out heartbeat))
      {
        throw new InvalidOperationException(string.Format("Your application is attempting to use the RabbitMQ but is missing 'heartbeat seconds' in it's app.config"));
      }

      return heartbeat;
    }
  }
}
