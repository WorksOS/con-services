using System;
using Microsoft.Extensions.Configuration;

namespace VSS.ConfigurationStore
{
    public interface IConfigurationStore
    {
        string GetValueString(string v);
        bool? GetValueBool(string v);
        int GetValueInt(string v);
        TimeSpan? GetValueTimeSpan(string v);
        string GetConnectionString(string connectionType, string databaseNameKey);
        string GetConnectionString(string connectionType);
        IConfigurationSection GetSection(string key);
        IConfigurationSection GetLoggingConfig();
    }
}