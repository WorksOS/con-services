using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AopAlliance.Intercept;
using log4net;
using System.Reflection;

using VSS.Hosted.VLCommon;
using System.Collections;
using Microsoft.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon
{
  public class MethodCallLogger : IMethodInterceptor
  {
    public object Invoke(IMethodInvocation invocation)
    {
      MethodInfo mi = invocation.Method;
      string serviceName = invocation.Target.ToString().Split('.').Last<string>();
      try
      {
        object[] attributes = mi.GetCustomAttributes(typeof(ObfuscateAttribute), false);
        ObfuscateAttribute obfuscateAttribute = attributes != null && attributes.Length > 0 ? (ObfuscateAttribute)attributes[0] : null;

        StringBuilder builder = new StringBuilder();
        builder.Append(String.Format(
            "B {0}:{1}(", serviceName, mi.Name));
        bool firstArg = true;
        if (invocation.Arguments != null)
        {
          foreach (object arg in invocation.Arguments)
          {
            if (firstArg)
              firstArg = false;
            else
              builder.Append(",");
            string json = JavaScriptObjectSerializer.Serialize(arg);
            // Obfuscate passwords
            if (obfuscateAttribute != null && arg != null && arg.Equals(invocation.Arguments[obfuscateAttribute.ArgumentIndex]))
            {
              //Is it just a string to obfuscate?
              if (string.IsNullOrEmpty(obfuscateAttribute.PropertyName))
                json = "*****";
              else
              {
                //Instance of a class. Obfuscate the specified property. 
                //json is the whole instance as a string as "property":"value" pairs
                string key = string.Format("\"{0}\":", obfuscateAttribute.PropertyName);
                int keyIndex = json.IndexOf(key);
                if (keyIndex >= 0)
                {
                  int startIndex = keyIndex + key.Length + 1;
                  int endIndex = json.IndexOf('"', startIndex);
                  string prefix = json.Substring(0, startIndex);
                  string suffix = json.Substring(endIndex);
                  json = string.Format("{0}*****{1}", prefix, suffix);
                }
              }
            }
            //Truncate big arguments
            else if (json.Length > MAX_RESULT_SIZE)
            {
              json = string.Format("(TRUNCATED){0}", json.Substring(0, MAX_RESULT_SIZE));
            }
            builder.Append(json);
          }
        }
        builder.Append(")");
        log.IfInfo(builder.ToString());
      }
      catch (Exception e)
      {
        log.Warn("Unexpected error logging method call", e);
        log.IfInfoFormat("B {0}:{1}(???????????????????)", serviceName, mi.Name);
      }

      object returnValue = null;
      try
      {
        returnValue = invocation.Proceed();
      }
      catch
      {
        log.IfInfoFormat("E {0}:{1} failed with thrown exception", serviceName, mi.Name);

        throw;
      }

      try
      {
        string res = JavaScriptObjectSerializer.Serialize(returnValue);

        if (res.Length > MAX_RESULT_SIZE)
        {
          log.IfInfoFormat("E {0}:{1} returned (TRUNCATED) {2}", serviceName, mi.Name, res.Substring(0, MAX_RESULT_SIZE));
        }
        else
        {
          log.IfInfoFormat("E {0}:{1} returned {2}", serviceName, mi.Name, res);
        }
      }
      catch (Exception ee)
      {
        log.Warn("Unexpected error logging method call return value", ee);
        log.IfInfoFormat("E {0}:{1} returned (????)", serviceName, mi.Name);
      }
      

      return returnValue;
    }

    private static readonly ILog log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);
    private static readonly int MAX_RESULT_SIZE = 1500;
  }

}
