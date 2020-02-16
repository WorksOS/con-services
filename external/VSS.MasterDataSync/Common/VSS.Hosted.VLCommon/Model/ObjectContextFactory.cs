using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Text.RegularExpressions;

namespace VSS.Hosted.VLCommon
{
  public static class ObjectContextFactory
  {
    #region ObjectContext factory

    public static void ContextCreationFuncs(Func<bool, INH_OP> opFunc,
                                            Func<bool, INH_RPT> rptFunc,
                                            Func<bool, INH_RAW> rawFunc,
                                            Func<bool, INH_DATA> datFunc,
                                            Func<bool, INH_OEMDATA> oemFunc)
    {
      CreateNHOPContext = opFunc;
      CreateNHRPTContext = rptFunc;
      CreateNHRAWContext = rawFunc;
      CreateNHDataContext = datFunc;
      CreateNHOEMDataContext = oemFunc;
    }

    /*
     *  The creation method used by ObjectContextFactory.NewNHContext<T>() 
     *  can be swapped out with other any creation method 
     *  that returns a type that implements the interface.
     *  
     *  to swap in another creation method anytime write this statement
     *  Model.CreateNhOpContext = () => new SomeOtherTypeThatImplementsINH_OP(); 
     *  
     *  ObjectContextFactory.NewNHContext<T> will then use the new creation method specified.
     */
    private static Func<bool, INH_OP> CreateNHOPContext = StoreOpContext;
    private static Func<bool, INH_RPT> CreateNHRPTContext = StoreRptContext;
    private static Func<bool, INH_RAW> CreateNHRAWContext = StoreRawContext;
    private static Func<bool, INH_DATA> CreateNHDataContext = StoreDataContext;
    private static Func<bool, INH_OEMDATA> CreateNHOEMDataContext = StoreOemContext;

    public static INH_OP StoreOpContext(bool readOnly)
    {
      string connStr = ConnectionString("NH_OP");

      return new NH_OP(connStr, readOnly);
    }

    public static INH_RPT StoreRptContext(bool readOnly)
    {
      string connStr = ConnectionString("NH_RPT");

      return new NH_RPT(connStr, readOnly);
    }

    public static INH_RAW StoreRawContext(bool readOnly)
    {
      string connStr = ConnectionString("NH_RAW");

      return new NH_RAW(connStr, readOnly);
    }

    public static INH_DATA StoreDataContext(bool readOnly)
    {
      string connStr = ConnectionString("NH_DATA");

      return new NH_DATA(connStr, readOnly);
    }

    public static INH_OEMDATA StoreOemContext(bool readOnly)
    {
      string connStr = ConnectionString("NH_OEMDATA");

      return new NH_OEMDATA(connStr, readOnly);
    }

    public static T NewNHContext<T>(bool readOnly = false) where T : IDisposable
    {
      if (typeof(T) == typeof(INH_RAW))
      {
        return (T)CreateNHRAWContext(readOnly);
      }

      if (typeof(T) == typeof(INH_OP))
      {
        return (T)CreateNHOPContext(readOnly);
      }

      if (typeof(T) == typeof(INH_RPT))
      {
        return (T)CreateNHRPTContext(readOnly);
      }

      if (typeof(T) == typeof(INH_DATA))
      {
        return (T)CreateNHDataContext(readOnly);
      }

      if (typeof(T) == typeof(INH_OEMDATA))
      {
        return (T)CreateNHOEMDataContext(readOnly);
      }

      return default(T);
    }

    #endregion

    #region Connection String utils

    public static string ConnectionString(string dbName)
    {
      string connString = ConfigurationManager.ConnectionStrings[dbName].ConnectionString;

      if (connString.Contains("XXXUSERXXX"))
      {
        // Use a machine specific db usr/pwd setting if exists, else fall back to appln usr/pwd settings.
        // This is done like this mainly so a developer can force the db connection string to contain their own
        // creds, for debugging purposes.
        string machineDbUserKey = "[" + Environment.MachineName + "]DatabaseUser";
        string machineDbPwdKey = "[" + Environment.MachineName + "]DatabasePassword";
        string dbUser = ConfigurationManager.AppSettings[machineDbUserKey] ?? ConfigurationManager.AppSettings["DatabaseUser"];
        string dbPwd = ConfigurationManager.AppSettings[machineDbPwdKey] ?? ConfigurationManager.AppSettings["DatabasePassword"];

        if (string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPwd))
        {
          throw new InvalidOperationException(string.Format("Your application is attempting to use the {0} entity model but is missing 'DatabaseUser' or 'DatabasePassword' credentials in it's app.config", dbName));
        }

        connString = connString.Replace("XXXUSERXXX", dbUser);
        connString = connString.Replace("XXXPASSWORDXXX", dbPwd);
      }
      return connString;
    }

    public static string DbUserConnectionString(string dbName, string dbUsername, string dbPassword)
    {
        /* This method is used to get a connection string for a given dbusername and dbpassword keys
         */
        string connString = ConfigurationManager.ConnectionStrings[dbName].ConnectionString;

        if (connString.Contains("XXXUSERXXX"))
        {
            // Use a machine specific db usr/pwd setting if exists, else fall back to appln usr/pwd settings.
            // This is done like this mainly so a developer can force the db connection string to contain their own
            // creds, for debugging purposes.
            string machineDbUserKey = "[" + Environment.MachineName + "]" + dbUsername;
            string machineDbPwdKey = "[" + Environment.MachineName + "]" + dbPassword;
            string dbUser = ConfigurationManager.AppSettings[machineDbUserKey] ?? ConfigurationManager.AppSettings[dbUsername];
            string dbPwd = ConfigurationManager.AppSettings[machineDbPwdKey] ?? ConfigurationManager.AppSettings[dbPassword];

            if (string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbPwd))
            {
                throw new InvalidOperationException(string.Format("Your application is attempting to use the {0} entity model but is missing 'DatabaseUser' or 'DatabasePassword' credentials in it's app.config", dbName));
            }

            connString = connString.Replace("XXXUSERXXX", dbUser);
            connString = connString.Replace("XXXPASSWORDXXX", dbPwd);
        }
        return connString;
    }

    public static string DbProviderConnectionString(string dbName)
    {
      string efConnString = ConnectionString(dbName);

      //string providerConnStr = "provider connection string=\"";
      String providerConnStr = null;
      Regex connStrRE = new Regex("provider connection string=['\"]([^'\"]*)['\"]");
      Match matches = connStrRE.Match(efConnString);
      if (matches != null)
      {
        providerConnStr = matches.Groups[1].Value;
      }

      return providerConnStr;
    }

    private static string DbUser(string dbName)
    {
      string machineDbUserKey = "[" + Environment.MachineName + "]DatabaseUser";
      return ConfigurationManager.AppSettings[machineDbUserKey] ?? ConfigurationManager.AppSettings["DatabaseUser"];
    }
    #endregion

  }
}
