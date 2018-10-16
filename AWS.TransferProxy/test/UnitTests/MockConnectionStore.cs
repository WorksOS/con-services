using Microsoft.Extensions.Configuration;
using System;
using VSS.ConfigurationStore;

namespace VSS.AWS.TransferProxy.UnitTests
{
  public class MockConnectionStore : IConfigurationStore
  {
    public string GetValueString(string v)
    {
      return v;
    }

    public bool? GetValueBool(string v)
    {
      throw new NotImplementedException();
    }

    public int GetValueInt(string v)
    {
      throw new NotImplementedException();
    }

    public TimeSpan? GetValueTimeSpan(string v)
    {
      return new TimeSpan();
    }

    public string GetConnectionString(string connectionType)
    {
      throw new NotImplementedException();
    }

    public string GetConnectionString(string connectionType, string databaseNameKey)
    {
      throw new NotImplementedException();
    }

    public IConfigurationSection GetSection(string key)
    {
      throw new NotImplementedException();
    }

    public IConfigurationSection GetLoggingConfig()
    {
      throw new NotImplementedException();
    }
  }
}
