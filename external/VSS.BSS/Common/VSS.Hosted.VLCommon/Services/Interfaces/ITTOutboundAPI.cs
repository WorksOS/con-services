using System;

using VSS.Hosted.VLCommon.TrimTracMessages;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon
{
    public interface ITTOutboundAPI
    {
      bool CalibrateRuntimeHour(string[] gpsDeviceIDs, long runtime);
      bool ResetRuntimeHour(INH_OP opCtx1, string[] gpsDeviceIDs, long runtime);
      bool SendDailyReportConfig(INH_OP opCtx1, string[] gpsDeviceIDs, int delayTimeout_T4);
      bool SetNetworkInterfaceConfiguration(string[] gpsDeviceIDs, string gprsAPN, string gprsUserName, string gprsPassword);
      bool SetPrimaryIPAddressConfiguration(string[] gpsDeviceIDs, string gprsDestinationAddress);
      bool SetRateConfiguration(INH_OP opCtx1, string[] gpsDeviceIDs, int delayTimeout_T4, int idleTimeout_T1);
      bool SetReportConfiguration(INH_OP opCtx1, string[] gpsDeviceIDs, int runtimeLPABasedCountdown_T30, EMode_Z runtimeLPABased, int ignitionSenseOverride);
      void Set_Alert_State_Clear(string gpsDeviceID, bool HPA, bool MPA, bool LPA);
      void Set_Alert_State_Acknowledge(string gpsDeviceID, bool HPA, bool MPA, bool LPA);
    }
}
