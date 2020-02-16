using System;

namespace VSS.Hosted.VLCommon
{
  public class EvaluateTireAlerts
  {
    public EvaluateTireAlerts()
    {
    }    

    public static bool HasAlertStatus(int alertState, int tireAlertStatus)
    {
      return ((alertState & tireAlertStatus) > 0);     
    }
  }
}
