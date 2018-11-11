using System;
using Microsoft.Extensions.Configuration;

namespace VSS.ConfigurationStore
{
  public interface IConfigurationStore
  {
    string GetValueString(string v);
    string GetValueString(string v, string defaultValue);
    bool? GetValueBool(string v);
    bool GetValueBool(string v, bool defaultValue);
    int GetValueInt(string v);
    int GetValueInt(string v, int defaultValue);
    long GetValueLong(string v);
    long GetValueLong(string v, long defaultValue);
    TimeSpan? GetValueTimeSpan(string v);
    TimeSpan GetValueTimeSpan(string v, TimeSpan defaultValue);
    string GetConnectionString(string connectionType, string databaseNameKey);
    string GetConnectionString(string connectionType);
    IConfigurationSection GetSection(string key);
    IConfigurationSection GetLoggingConfig();
  }
}