using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using VSS.ConfigurationStore;

namespace VSS.Common.ServiceDiscovery.UnitTests.Mocks
{
  public class MockConfiguration : IConfigurationStore
  {
    public MockConfiguration()
    {
      Values = new Dictionary<string, object>();
    }

    public Dictionary<string, object> Values { get; }

    public string GetValueString(string v)
    {
      return Values[v] as string;
    }

    public string GetValueString(string v, string defaultValue)
    {
      if (Values.ContainsKey(v))
        return Values[v] as string;
      return defaultValue;
    }

    public bool? GetValueBool(string v)
    {
      return Values[v] as bool?;
    }

    public bool GetValueBool(string v, bool defaultValue)
    {
      if (Values.ContainsKey(v))
        return (bool)Values[v];
      return defaultValue;
    }

    public int GetValueInt(string v)
    {
      return (int) Values[v];
    }

    public int GetValueInt(string v, int defaultValue)
    {
      if (Values.ContainsKey(v))
        return (int) Values[v];
      return defaultValue;
    }

    public uint GetValueUint(string v)
    {
      return (uint)Values[v];
    }

    public uint GetValueUint(string v, uint defaultValue)
    {
      if (Values.ContainsKey(v))
        return (uint) Values[v];
      return defaultValue;
    }

    public long GetValueLong(string v)
    {
      return (long) Values[v];
    }

    public long GetValueLong(string v, long defaultValue)
    {
      if (Values.ContainsKey(v))
        return (long) Values[v];
      return defaultValue;
    }

    public double GetValueDouble(string v)
    {
      return (double) Values[v];
    }

    public double GetValueDouble(string v, double defaultValue)
    {
      if (Values.ContainsKey(v))
        return (double) Values[v];
      return defaultValue;
    }

    public TimeSpan? GetValueTimeSpan(string v)
    {
      return Values[v] as TimeSpan?;
    }

    public TimeSpan GetValueTimeSpan(string v, TimeSpan defaultValue)
    {
      if (Values.ContainsKey(v))
        return (TimeSpan) Values[v];
      return defaultValue;
    }

    public string GetConnectionString(string connectionType, string databaseNameKey)
    {
      return Values[connectionType + databaseNameKey] as string;
    }

    public string GetConnectionString(string connectionType)
    {
      return Values[connectionType] as string;
    }

    public IConfigurationSection GetSection(string key)
    {
      throw new NotImplementedException();
    }

    public IConfigurationSection GetLoggingConfig()
    {
      throw new NotImplementedException();
    }

    public bool UseKubernetes { get; }
    public string KubernetesConfigMapName { get; }
    public string KubernetesNamespace { get; }
  }
}