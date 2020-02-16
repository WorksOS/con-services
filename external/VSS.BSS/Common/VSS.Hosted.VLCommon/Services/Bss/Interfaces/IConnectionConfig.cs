using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Hosted.VLCommon.Services.Bss.Interfaces
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
