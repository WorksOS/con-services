using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;

namespace MegalodonUnitTests
{
  public class TestingConfig : IConfigurationStore
  {

    private NameValueCollection _config = new NameValueCollection();

    public TestingConfig(ILoggerFactory logger, IConfigurationRoot configurationOverrides = null)
    {
    }

    public bool UseKubernetes => throw new NotImplementedException();

    public string KubernetesConfigMapName => throw new NotImplementedException();

    public string KubernetesNamespace => throw new NotImplementedException();

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
      throw new NotImplementedException();
    }

    public string GetConnectionString(string connectionType, string databaseNameKey)
    {
      throw new NotImplementedException();
    }

    public string GetConnectionString(string connectionType)
    {
      throw new NotImplementedException();
    }

    public IConfigurationSection GetLoggingConfig()
    {
      throw new NotImplementedException();
    }

    public IConfigurationSection GetSection(string key)
    {
      throw new NotImplementedException();
    }

    public bool? GetValueBool(string v)
    {
      if (_config.Count < 1)
        Load();
      var tmp = _config[v];
      return tmp.ToLower() == "true" ? true : false;  

    }

    public bool GetValueBool(string v, bool defaultValue)
    {
      throw new NotImplementedException();
    }

    public double GetValueDouble(string v)
    {
      throw new NotImplementedException();
    }

    public double GetValueDouble(string v, double defaultValue)
    {
      throw new NotImplementedException();
    }

    public Guid GetValueGuid(string v)
    {
      throw new NotImplementedException();
    }

    public Guid GetValueGuid(string v, Guid defaultValue)
    {
      throw new NotImplementedException();
    }

    public int GetValueInt(string v)
    {
      throw new NotImplementedException();
    }

    public int GetValueInt(string v, int defaultValue)
    {
      throw new NotImplementedException();
    }

    public long GetValueLong(string v)
    {
      throw new NotImplementedException();
    }

    public long GetValueLong(string v, long defaultValue)
    {
      throw new NotImplementedException();
    }

    public string GetValueString(string v)
    {
      //   throw new NotImplementedException();
      if (_config.Count < 1)
        Load();
      return _config[v];
    }

    public string GetValueString(string v, string defaultValue)
    {
      throw new NotImplementedException();
    }

    public TimeSpan? GetValueTimeSpan(string v)
    {
      throw new NotImplementedException();
    }

    public TimeSpan GetValueTimeSpan(string v, TimeSpan defaultValue)
    {
      throw new NotImplementedException();
    }

    public uint GetValueUint(string v)
    {
      throw new NotImplementedException();
    }

    public uint GetValueUint(string v, uint defaultValue)
    {
      throw new NotImplementedException();
    }

    public void Load()
    {
      _config.Add("TCIP", "127.0.0.1");
      _config.Add("Port", "1500");
      _config.Add("DebugTraceToLog", "false");
    }

    public IEnumerable<string> ProduceSubKeys(IEnumerable<string> earlierKeys, string prefix, string delimiter)
    {
      throw new NotImplementedException();
    }

    public void Set(string key, string value)
    {
      throw new NotImplementedException();
    }

    public bool TryGet(string key, out string value)
    {
      value = _config[key];
      return true;
    }

  }
}
