using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

using VSS.Hosted.VLCommon;
using System.Data;
using System.Diagnostics;

using VSS.Hosted.VLCommon.TrimTracMessages;

namespace VSS.Hosted.VLCommon
{
  internal class TTOutboundAPI : ITTOutboundAPI
  {
    public bool CalibrateRuntimeHour(string[] gpsDeviceIDs, long runtime)
    {
        // Method used anywhere hence Opening New Connection to fix compile error
        using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
        {
            return SetRuntimeCounter(opCtx1, gpsDeviceIDs, runtime, true, false);
        }
    }

    public bool ResetRuntimeHour(INH_OP opCtx1, string[] gpsDeviceIDs, long runtime)
    {
      return SetRuntimeCounter(opCtx1, gpsDeviceIDs, runtime, false, true);//Runtime hour meter should be reset to zero when provisioned for the first time/when device is transferred
    }

    private static bool SetRuntimeCounter(INH_OP opCtx1, string[] gpsDeviceIDs, long runtime, bool calibrate, bool reset)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
         throw new ArgumentException("Invalid parameters");

      bool success = true;
      List<TTOut> TTOutList = new List<TTOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
          TT_QTKM configQueryMtr = new TT_QTKM();
          configQueryMtr.ID = API.Device.IMEI2UnitID(gpsDeviceID);

          if (calibrate)
              CalibrationHelper.CalibrateDeviceRuntime(opCtx1, gpsDeviceIDs, DeviceTypeEnum.TrimTrac, (double)runtime);

          if (reset)
          {
              configQueryMtr.RuntimeLPABasedQuery = EReportFlag.ReportWithReset;
              configQueryMtr.RuntimeMotionBasedQuery = EReportFlag.ReportWithReset;
          }
          else
          {
              configQueryMtr.RuntimeLPABasedQuery = EReportFlag.ReportOnly_NoReset;
              configQueryMtr.RuntimeMotionBasedQuery = EReportFlag.ReportOnly_NoReset;
          }

          string msg = string.Format("QTKM{0}{1}{2}{3}", (int)configQueryMtr.RuntimeMotionBasedQuery, (int)configQueryMtr.RuntimeLPABasedQuery, TTParser.PW_CMD(configQueryMtr), TTParser.ID_CMD(configQueryMtr));
          string payLoad = TTParser.EnsurePWIDChkSum(configQueryMtr.ID, ">" + msg + ";*<", true);

          var TTOut = new TTOut
          {
              ID = 0,
              InsertUTC = DateTime.UtcNow,
              Status = (int)MessageStatusEnum.Pending,
              Payload = payLoad,
              UnitID = configQueryMtr.ID
          };

          TTOutList.Add(TTOut);
      }

      StoreTTOut(opCtx1, TTOutList, "CC runtime mileage messages");
      
      return success;          
    }

    internal static void StoreTTOut(INH_OP opCtx1, List<TTOut> msgs, string errorDescription)
    {
      int result;
        foreach (TTOut msg in msgs)
        {
            opCtx1.TTOut.AddObject(msg);
        }
        result = opCtx1.SaveChanges();
      if (result <= 0)
        throw new InvalidOperationException("Failed to save " + errorDescription);
    }

    public bool SendDailyReportConfig(INH_OP opCtx1, string[] gpsDeviceIDs, int delayTimeout_T4)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success;

      if (SetModuleAppConfig(opCtx1, gpsDeviceIDs) && SetReportConfiguration(opCtx1, gpsDeviceIDs) && SetRateConfiguration(opCtx1, gpsDeviceIDs, delayTimeout_T4))
          success = true;
      else
          throw new InvalidOperationException("Failed to set the reporting interval configuration");
      return success;
    }

    public bool SetReportConfiguration(INH_OP opCtx1, string[] gpsDeviceIDs, int runtimeLPABasedCountdown_T30 = 0, EMode_Z runtimeLPABased = EMode_Z.Enabled, int ignitionSenseOverride = 2)
    {
      bool success = true;

      List<TTOut> TTOutList = new List<TTOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        TT_STKZ config = new TT_STKZ();
        config.Data = new EXT2_APP_CONFIG();
        config.Data.MotionCounterThreshold = 5;
        config.Data.ScheduledHoursMode = EMode_Z.Disabled;
        config.Data.ScheduledHoursDailyStartTime_T27 = 0;
        config.Data.ScheduledHoursWorkDayLength_T28 = 43200;

        config.Data.ScheduledHoursFirstWeeklyWorkDay = DayOfWeek.Monday;
        config.Data.ScheduledHoursWorkDaysPerWeek = 5;
        config.Data.RuntimeMotionBased = EMode_Z.Disabled;
        config.Data.RuntimeLPABased = runtimeLPABased;
        config.Data.RuntimeMotionBasedCountdown_T29 = 0;
        config.Data.RuntimeLPABasedCountdown_T30 = runtimeLPABasedCountdown_T30;

        config.Data.AutomaticMessageLogDump = EAutomaticMessageLogDump.Enabled;
        config.Data.GPSFixRate = EGPSFixRate._1HzGPSOperationExceptWhile;
        config.Data.LPASpeedingReportInputArmingDelay_T31 = 0;
        config.Data.GeofenceType = EGeofenceType.Inclusive;
        config.Data.SpeedEnforcement = 0;

        config.Data.SpeedingReportMode = ESpeedingReportMode.ReportAllViolations;
        config.Data.SpeedingCountdownTimer = 0;

        config.Data.ReportingOptions = 0;


        config.Data.IgnitionSenseOverride = ignitionSenseOverride;
        config.Data.EngineIdleThreshold = 0;
        config.Data.EngineIdleReportEnabled = EEngineIdleReportEnabled.NoEngineIdleReportSent;

        config.Data.SpeedThresholdTime = 0;
        config.Data.MovingSpeed = 0;
        config.Data.EarlyStopTimer = 0;
        config.Data.Reserved6 = 0;

        string msg = string.Format("STKZ{0:D4}{1:D1}{2:D5}" +
                                    "{3:D5}{4:D1}{5:D1}" +
                                    "{6:D1}{7:D1}{8:D3}" +
                                    "{9:D3}{10:D1}{11:D1}" +
                                    "{12:D3}{13:D1}{14:D3}" +
                                    "{15:D1}{16:D5}" +
                                    "{17:X4}{18:D1}{19:D4}{20:D1}" +
                                    "{21:D4}{22:D2}{23:D4}{24:D4}{25}{26}",
            /* 0 */ config.Data.MotionCounterThreshold, (int)config.Data.ScheduledHoursMode, config.Data.ScheduledHoursDailyStartTime_T27,
            /* 3 */ config.Data.ScheduledHoursWorkDayLength_T28, (int)config.Data.ScheduledHoursFirstWeeklyWorkDay, config.Data.ScheduledHoursWorkDaysPerWeek,
            /* 6 */ (int)config.Data.RuntimeMotionBased, (int)config.Data.RuntimeLPABased, config.Data.RuntimeMotionBasedCountdown_T29,
            /* 9 */ config.Data.RuntimeLPABasedCountdown_T30, (int)config.Data.AutomaticMessageLogDump, (int)config.Data.GPSFixRate,
            /* 12 */config.Data.LPASpeedingReportInputArmingDelay_T31, (int)config.Data.GeofenceType, config.Data.SpeedEnforcement,
            /* 15 */(int)config.Data.SpeedingReportMode, config.Data.SpeedingCountdownTimer, config.Data.ReportingOptions, config.Data.IgnitionSenseOverride, config.Data.EngineIdleThreshold, (int)config.Data.EngineIdleReportEnabled,
            /* 21 */config.Data.SpeedThresholdTime, config.Data.MovingSpeed, config.Data.EarlyStopTimer, config.Data.Reserved6, TTParser.PW_CMD(config), TTParser.ID_CMD(config));

        string unitID = API.Device.IMEI2UnitID(gpsDeviceID);
        string payLoad = TTParser.EnsurePWIDChkSum(unitID, ">" + msg + ";*<", true);

        var TTOut = new TTOut
        {
            ID = 0,
            InsertUTC = DateTime.UtcNow,
            Status = (int)MessageStatusEnum.Pending,
            Payload = payLoad,
            UnitID = unitID
        };

        TTOutList.Add(TTOut);
      }

      StoreTTOut(opCtx1, TTOutList, "Report Interval Config");
      return success;
    }

    public bool SetModuleAppConfig(INH_OP opCtx1, string[] gpsDeviceIDs)
    {
      bool success = true;

      List<TTOut> TTOutList = new List<TTOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        TT_STKY vehicleAdptrCtrlMod = new TT_STKY();
        vehicleAdptrCtrlMod.Data = new MODULE_APP_CONFIG();
        vehicleAdptrCtrlMod.Data.HPAIdleTimeout_T11 = 10;
        vehicleAdptrCtrlMod.Data.MPAIdleTimeout_T12 = 1710;
        vehicleAdptrCtrlMod.Data.HPADelayTimeout_T13 = 10;
        vehicleAdptrCtrlMod.Data.MPADelayTimeout_T14 = 1710;
        vehicleAdptrCtrlMod.Data.HPATransmitTimeout_T15 = 0;
        vehicleAdptrCtrlMod.Data.MPATransmitTimeout_T16 = 120;
        vehicleAdptrCtrlMod.Data.HPAQueryTimeout_T17 = 60;
        vehicleAdptrCtrlMod.Data.HPATransmitAttempts_N5 = 0;
        vehicleAdptrCtrlMod.Data.MPATransmitAttempts_N6 = 1;
        vehicleAdptrCtrlMod.Data.LPATransmitAttempts_N7 = 0;
        vehicleAdptrCtrlMod.Data.HPAMode = EMode_Y.Disabled;
        vehicleAdptrCtrlMod.Data.MPAMode = EMode_Y.NetworkAcknowledgement;
        vehicleAdptrCtrlMod.Data.LPAMode = EMode_Y.MonitorOnly;

        string msg = string.Format("STKY{0:D6}{1:D6}{2:D6}{3:D6}{4:D6}{5:D6}{6:D6}{7:D3}{8:D3}{9:D3}{10}{11}{12}{13}{14}",
        vehicleAdptrCtrlMod.Data.HPAIdleTimeout_T11, vehicleAdptrCtrlMod.Data.MPAIdleTimeout_T12, vehicleAdptrCtrlMod.Data.HPADelayTimeout_T13,
        vehicleAdptrCtrlMod.Data.MPADelayTimeout_T14, vehicleAdptrCtrlMod.Data.HPATransmitTimeout_T15, vehicleAdptrCtrlMod.Data.MPATransmitTimeout_T16,
        vehicleAdptrCtrlMod.Data.HPAQueryTimeout_T17, vehicleAdptrCtrlMod.Data.HPATransmitAttempts_N5, vehicleAdptrCtrlMod.Data.MPATransmitAttempts_N6,
        vehicleAdptrCtrlMod.Data.LPATransmitAttempts_N7, (int)vehicleAdptrCtrlMod.Data.HPAMode, (int)vehicleAdptrCtrlMod.Data.MPAMode, (int)vehicleAdptrCtrlMod.Data.LPAMode,
        TTParser.PW_CMD(vehicleAdptrCtrlMod), TTParser.ID_CMD(vehicleAdptrCtrlMod));

        string unitID = API.Device.IMEI2UnitID(gpsDeviceID);
        string payLoad = TTParser.EnsurePWIDChkSum(unitID, ">" + msg + ";*<", true);
        var TTOut = new TTOut
        {
            ID = 0,
            InsertUTC = DateTime.UtcNow,
            Status = (int)MessageStatusEnum.Pending,
            Payload = payLoad,
            UnitID = unitID
        };

        TTOutList.Add(TTOut);
      }
      StoreTTOut(opCtx1, TTOutList, "module app config");
      return success;
    }

    public bool SetRateConfiguration(INH_OP opCtx1, string[] gpsDeviceIDs, int delayTimeout_T4, int idleTmeout_T1 = 28710)
    {
      bool success = true;

      List<TTOut> TTOutList = new List<TTOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        TT_STKA reportrate = new TT_STKA();
        reportrate.Data = new APP_CONFIG();
        reportrate.Data.IDLETimeout_T1 = idleTmeout_T1;// 28710 = 8 hrs minus latency
        reportrate.Data.FIXTimeout_T2 = 300;
        reportrate.Data.TRANSMITTimeout_T3 = 300;
        reportrate.Data.DELAYTimeout_T4 = delayTimeout_T4; 
        reportrate.Data.QUERYTimeout_T5 = 20;
        reportrate.Data.AlmanacTimeout_T6 = 168;
        reportrate.Data.StaticMotionFilterTimeout_T7 = 20;
        reportrate.Data.MotionReportFlag = EMotionReportFlag.None;
        reportrate.Data.ReportDelayFlag = EReportDelayFlag.TxAllMessages;
        reportrate.Data.DiagnosticsMode = EDiagnosticsMode.LED;
        reportrate.Data.CommunicationMode = ECommunicationMode.GPRS;


        string msg = string.Format("STKA{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}", TTParser.ToString(6, reportrate.Data._IDLETimeout_T1), TTParser.ToString(6, reportrate.Data._FIXTimeout_T2), TTParser.ToString(6, reportrate.Data._TRANSMITTimeout_T3), TTParser.ToString(6, reportrate.Data._DELAYTimeout_T4),
        TTParser.ToString(6, reportrate.Data._QUERYTimeout_T5), TTParser.ToString(3, reportrate.Data._AlmanacTimeout_T6), TTParser.ToString(2, reportrate.Data._StaticMotionFilterTimeout_T7),
        TTParser.ToString(1, (int?)reportrate.Data._MotionReportFlag), TTParser.ToString(1, (int?)reportrate.Data._ReportDelayFlag), TTParser.ToString(1, (int?)reportrate.Data._DiagnosticsMode), TTParser.ToString(1, (int?)reportrate.Data._CommunicationMode), TTParser.PW_CMD(reportrate), TTParser.ID_CMD(reportrate));
        string unitID = API.Device.IMEI2UnitID(gpsDeviceID);
        string payLoad = TTParser.EnsurePWIDChkSum(unitID, ">" + msg + ";*<", true);

        var TTOut = new TTOut
        {
            ID = 0,
            InsertUTC = DateTime.UtcNow,
            Status = (int)MessageStatusEnum.Pending,
            Payload = payLoad,
            UnitID = unitID
        };

        TTOutList.Add(TTOut);
      }
      StoreTTOut(opCtx1, TTOutList, "Reporting Interval Rate");
      return success;
    }

    public void Set_Alert_State_Clear(string unitID, bool HPA, bool MPA, bool LPA)
    {
      List<TTOut> TTOutList = new List<TTOut>();
      string msg = string.Format(">STKL{0}{1}{2};*<", HPA ? TTAlertResponse.Clear : TTAlertResponse.DontCare, MPA ? TTAlertResponse.Clear : TTAlertResponse.DontCare, LPA ? TTAlertResponse.Clear : TTAlertResponse.DontCare);

      string payLoad = TTParser.EnsurePWIDChkSum(unitID,msg , true);
      
        var TTOut = new TTOut
        {
            ID = 0,
            InsertUTC = DateTime.UtcNow,
            Status = (int)MessageStatusEnum.Pending,
            Payload = payLoad,
            UnitID = unitID
        };

      TTOutList.Add(TTOut);
        using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
        {
            StoreTTOut(opCtx1, TTOutList, " Message to clear HPA/MPA/LPA status");
        }
    }

    public void Set_Alert_State_Acknowledge(string unitID, bool HPA, bool MPA, bool LPA)
    {
      List<TTOut> TTOutList = new List<TTOut>();
      string msg = string.Format(">STKL{0}{1}{2};*<", HPA ? TTAlertResponse.Acknowledged : TTAlertResponse.DontCare, MPA ? TTAlertResponse.Acknowledged : TTAlertResponse.DontCare, LPA ? TTAlertResponse.Acknowledged : TTAlertResponse.DontCare);
     
      string payLoad = TTParser.EnsurePWIDChkSum(unitID, msg , true);

      var TTOut = new TTOut
      {
        ID = 0,
        InsertUTC = DateTime.UtcNow,
        Status = (int)MessageStatusEnum.Pending,
        Payload = payLoad,
        UnitID = unitID
      };

      TTOutList.Add(TTOut);
        using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
        {
            StoreTTOut(opCtx1, TTOutList, "Message to acknowledge HPA/MPA/LPA status");
        }
    }

    public bool SetNetworkInterfaceConfiguration(string[] gpsDeviceIDs, string gprsAPN, string gprsUserName, string gprsPassword)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
       throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<TTOut> TTOutList = new List<TTOut>(); 
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        TT_STKJ gprsSetupConfig = new TT_STKJ();
        gprsSetupConfig.Data = new GPRS_SETUP_CONFIG();
        gprsAPN = gprsAPN.ToString().Replace("http://", "");
        gprsAPN = gprsAPN.ToString().Replace("https://", "");
        gprsSetupConfig.Data.GPRSAPN = gprsAPN;
        gprsSetupConfig.Data.GPRSUsername = "";
        gprsSetupConfig.Data.GPRSPassword = "";
        string msg = string.Format("STKJ{0}\"{1}\"{2}\"{3}{4}", gprsSetupConfig.Data.GPRSAPN, gprsSetupConfig.Data.GPRSUsername, gprsSetupConfig.Data.GPRSPassword, TTParser.PW_CMD(gprsSetupConfig), TTParser.ID_CMD(gprsSetupConfig));
        string unitID = API.Device.IMEI2UnitID(gpsDeviceID);
        string payLoad = TTParser.EnsurePWIDChkSum(unitID, ">" + msg + ";*<", true);

        var TTOut = new TTOut
        {
            ID = 0,
            InsertUTC = DateTime.UtcNow,
            Status = (int)MessageStatusEnum.Pending,
            Payload = payLoad,
            UnitID = unitID
        };


        TTOutList.Add(TTOut);
      }
        using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
        {
            StoreTTOut(opCtx1, TTOutList, "APN configuration");
        }
        return success;
    }
    public bool SetPrimaryIPAddressConfiguration(string[] gpsDeviceIDs, string gprsDestinationAddress)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<TTOut> TTOutList = new List<TTOut>(); 
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        TT_STKF ipAddrConfig = new TT_STKF();
        ipAddrConfig.Data = new GPRS_CONNECT_CONFIG();
        ipAddrConfig.Data.GPRSTransportProtocol = EGPRSTransportProtocol.TCP;
        ipAddrConfig.Data.GPRSSessionProtocol = EGPRSSessionProtocol.None;
        ipAddrConfig.Data.GPRSSessionKeepAliveTimeout_T25 = 600;
        ipAddrConfig.Data.GPRSSessionTimeout_T26 = 0;
        ipAddrConfig.Data.GPRSDestinationAddress = gprsDestinationAddress + ":1121";//1121 corresponds to the port. This value is taken from TTGateway config file
        string unitID = API.Device.IMEI2UnitID(gpsDeviceID);
        string msg = string.Format("STKF{0}{1}{2:D5}{3:D5}{4}\"{5}{6}",
        (int)ipAddrConfig.Data.GPRSTransportProtocol, (int)ipAddrConfig.Data.GPRSSessionProtocol,
        ipAddrConfig.Data.GPRSSessionKeepAliveTimeout_T25, ipAddrConfig.Data.GPRSSessionTimeout_T26,
        ipAddrConfig.Data.GPRSDestinationAddress, TTParser.PW_CMD(ipAddrConfig), TTParser.ID_CMD(ipAddrConfig));// ipAddrConfig.ToString();//STKF100060000000155.63.162.52";PW=00000000
        string payLoad = TTParser.EnsurePWIDChkSum(unitID, ">" + msg + ";*<", true);

        var TTOut = new TTOut
        {
            ID = 0,
            InsertUTC = DateTime.UtcNow,
            Status = (int)MessageStatusEnum.Pending,
            Payload = payLoad,
            UnitID = unitID
        };

        TTOutList.Add(TTOut);
      }
        using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
        {
            StoreTTOut(opCtx1, TTOutList, "IP configuration");
        }
        return success;
    }
  }
}
