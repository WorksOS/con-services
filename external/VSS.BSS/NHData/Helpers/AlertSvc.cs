using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon;
using log4net;

namespace VSS.Nighthawk.NHDataSvc.Helpers
{
  public class AlertSvc : IAlertSvc
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public void Send(List<INHDataObject> alertItems, List<DataHoursLocation> locationItems)
    {
      try
      {
        AlertTriggerClient.Process(alertItems, locationItems);
      }
      catch (Exception e)
      {
        log.IfError("Unexpected error forwarding alertable events to alert trigger svc", e);
      }
    }
  }
}
