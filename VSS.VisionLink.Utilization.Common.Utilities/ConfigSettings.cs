using System;
using System.Configuration;

namespace VSS.VisionLink.Utilization.Common.Utilities
{
  public static class ConfigSettings
  {
    public static string GetConnectionString(string connectionName)
    {
      var config = ConfigurationManager.ConnectionStrings[connectionName];
      if (config == null)
      {
        var error = string.Format("Could not find connection string for {0}", connectionName);
        throw new Exception(error);
      }
      var connString = config.ConnectionString;

      if (connString.Contains("XXXUSERXXX"))
      {
        var dbUser = ConfigurationManager.AppSettings["DatabaseUser"];
        var dbPwd = ConfigurationManager.AppSettings["DatabasePassword"];

        if (string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPwd))
        {
          throw new InvalidOperationException(
            string.Format(
              "Your application is attempting to use the {0} connection but is missing 'DatabaseUser' or 'DatabasePassword' credentials in the config file",
              connectionName));
        }

        connString = connString.Replace("XXXUSERXXX", dbUser);
        connString = connString.Replace("XXXPASSWORDXXX", dbPwd);
      }
      return connString;
    }
  }
}