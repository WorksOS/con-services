using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AopAlliance.Intercept;
using System.ServiceModel;
using log4net;

using System.Reflection;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon
{
  public class ExceptionLogger : IMethodInterceptor
  {
    public object Invoke(IMethodInvocation invocation)
    {
      object returnValue = null;

      try
      {
        returnValue = invocation.Proceed();
      }
      catch (UnauthorizedAccessException)
      {
        //Login failures are things that we are not worried about logging to app alarms table
        throw;
      }
      catch (Exception ex)
      {
        if (null != ex.InnerException && ex.InnerException is IntentionallyThrownException)
          log.IfInfo("ServicesAPI IntentionallyThrownException", ex);
        else
          log.IfError("ServicesAPI Exception", ex);
        throw;
      }

      return returnValue;
    }

    private static readonly ILog log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);
  }
}
