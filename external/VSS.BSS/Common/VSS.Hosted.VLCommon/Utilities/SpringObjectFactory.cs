using System;
using System.Configuration;
using System.Reflection;
using System.Xml;
using log4net;
using Spring.Context;
using Spring.Context.Support;

namespace VSS.Hosted.VLCommon
{
  public static class SpringObjectFactory
  {
    public static T CreateObject<T>(string alternateConfigFilepath = null, bool rethrowExceptions = false) where T : new()
    {
      T returnObject;
      string objectName = typeof(T).FullName;
      try
      {
        IApplicationContext context = null;
        if(String.IsNullOrEmpty(alternateConfigFilepath))
        {
          context = ContextRegistry.GetContext();
        }
        else
        {
          context = new XmlApplicationContext(false, alternateConfigFilepath);
        }
        if (context != null)
        {
          returnObject = (context.ContainsObjectDefinition(objectName)) ? (T)context[objectName] : new T();
        }
        else
        {
          returnObject = new T();
          LogDebug("Using non-IOC version of type (context missing) {0}", typeof(T).FullName);
        }
      }
      catch (Exception e)
      {
        returnObject = new T();
        if (rethrowExceptions)
        {
          throw;
        }
        LogDebug("Using non-IOC version of type {0}\r\n{1}", typeof(T).FullName, e.ToString());
      }
      return returnObject;
    }

    public static T CreateObjectWithConstructorArgs<T>(string alternateConfigFilepath = null, bool rethrowExceptions = false)
    {
      T returnObject;
      string objectName = typeof(T).FullName;
      try
      {
        IApplicationContext context = null;
        if (String.IsNullOrEmpty(alternateConfigFilepath))
        {
          context = ContextRegistry.GetContext();
        }
        else
        {
          context = new XmlApplicationContext(false, alternateConfigFilepath);
        }

        if (context != null)
        {
          returnObject = (context.ContainsObjectDefinition(objectName)) ? (T)context.GetObject(objectName) : default(T);
        }
        else
        {
          returnObject = default(T);
        }
      }
      catch (Exception e)
      {
        returnObject = default(T);
        if (rethrowExceptions)
        {
          throw;
        }
        LogDebug("Using non-IOC version of type {0}\r\n{1}", typeof(T).FullName, e.ToString());
      }
      return returnObject;
    }

    private static void LogDebug(string format, params string[] msgs)
    {
      if (!String.IsNullOrEmpty(format))
      {
        ILog log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);
        log.IfDebug(String.Format(format, msgs));
      }
    }
  }
}