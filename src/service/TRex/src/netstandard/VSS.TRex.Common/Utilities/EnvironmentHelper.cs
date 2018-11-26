using System;
using System.Configuration;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.Common.Utilities
{
  public static class EnvironmentHelper
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Gets environment variable from the supplied target location. Throws <see cref="ConfigurationErrorsException"/> if not found.
    /// </summary>
    public static string GetEnvironmentVariable(string key, string defaultValue = null, EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine)
    {
      string value = Environment.GetEnvironmentVariable(key, target);

      if (!string.IsNullOrEmpty(value))
      {
        return value;
      }

      if (string.IsNullOrEmpty(defaultValue))
      {
        throw new ConfigurationErrorsException($"Missing environment variable ({target}): {key}");
      }

      Log.LogInformation($"Missing environment variable ({target}): {key}, using default value: {defaultValue}");

      return defaultValue;
    }
  }
}
