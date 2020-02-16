using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using log4net;

// This file is also used in the Client code, hence the refs to client's TCMAccountManager.
#if TT_PARSER_APP
#if USE_2_1
using VSS.Nighthawk.TCMAccountManager.TCMAdminAPI_2_1;
using TT_RTKX = Trimble.Construction.TCMAccountManager.TCMAdminAPI_2_1.TT_RTKX1;
using TT_RTKx = Trimble.Construction.TCMAccountManager.TCMAdminAPI_2_1.TT_RTKx;
using EAutomaticMessageLogDump = Trimble.Construction.TCMAccountManager.TCMAdminAPI_2_1.EAutomaticMessageLogDump;
using EGPSFixRate = Trimble.Construction.TCMAccountManager.TCMAdminAPI_2_1.EGPSFixRate;
#elif USE_2_0
using VSS.Nighthawk.TCMAccountManager.TCMAdminAPI_2_0;
using TT_RTKX = Trimble.Construction.TCMAccountManager.TCMAdminAPI_2_0.TT_RTKX1;
using TT_RTKx = Trimble.Construction.TCMAccountManager.TCMAdminAPI_2_0.TT_RTKx;
using EAutomaticMessageLogDump = Trimble.Construction.TCMAccountManager.TCMAdminAPI_2_0.EMode_Z;
using EGPSFixRate = System.Int32;
#endif
#else
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.ServiceContracts;

using System.Reflection;
#endif

namespace VSS.Hosted.VLCommon.TrimTracMessages
{
  public static class TTParser
  {
    public static TT Parse(string dat)
    {
      if (dat == null)
        return null;
      dat = dat.Trim();
      string prefix = dat.TrimStart('>', '+', 'A', 'T');
      if (prefix.Length < 4)
        return null;
      if (prefix.Substring(4, 1) == "?")
        return null;

      TT rtnCmd = null;

      switch (prefix.Substring(0, 4))
      {
        case "CTKC":
          rtnCmd = Parse_CTKC(dat);
          break;
        case "CTKF":
          rtnCmd = Parse_CTKF(dat);
          break;
        case "CTKJ":
          rtnCmd = Parse_CTKJ(dat);
          break;
        case "CTKG":
          rtnCmd = Parse_CTKG(dat);
          break;
        case "CTKK":
          rtnCmd = Parse_CTKK(dat);
          break;
        case "CTKP":
          rtnCmd = Parse_CTKP(dat);
          break;
        case "CTKX":
          rtnCmd = Parse_CTKX(dat);
          break;
        case "CTKY":
          rtnCmd = Parse_CTKY(dat);
          break;
        case "CTKZ":
          rtnCmd = Parse_CTKZ(dat);
          break;

        case "QTKA":
          rtnCmd = Parse_QTK_(dat, TT_QTK_.QUERY_CONFIG.ApplicationParameters);
          break;
        case "QTKF":
          rtnCmd = Parse_QTK_(dat, TT_QTK_.QUERY_CONFIG.GPRSConnectionParameters);
          break;
        case "QTKG":
          rtnCmd = Parse_QTK_(dat, TT_QTK_.QUERY_CONFIG.GPSParameters);
          break;
        case "QTKJ":
          rtnCmd = Parse_QTK_(dat, TT_QTK_.QUERY_CONFIG.GPRSSetupParameters);
          break;
        case "QTKV":
          rtnCmd = Parse_QTK_(dat, TT_QTK_.QUERY_CONFIG.ProvisioningParameters);
          break;
        case "QTKX":
          rtnCmd = Parse_QTK_(dat, TT_QTK_.QUERY_CONFIG.ExtendedApplicationPara);
          break;
        case "QTKY":
          rtnCmd = Parse_QTK_(dat, TT_QTK_.QUERY_CONFIG.ModuleApplicationPara);
          break;
        case "QTKZ":
          rtnCmd = Parse_QTK_(dat, TT_QTK_.QUERY_CONFIG.Extended2ApplicationPara);
          break;
#if !TT_PARSER_APP || USE_2_1
        case "QTKI":
          rtnCmd = Parse_QTK_(dat, TT_QTK_.QUERY_CONFIG.DeviceIdentification);
          break;
        case "QTKT":
          rtnCmd = Parse_QTK_(dat, TT_QTK_.QUERY_CONFIG.Firwmare);
          break;
#endif

        case "QTKD":
          rtnCmd = Parse_QTKD(dat);
          break;
        case "QTKK":
          rtnCmd = Parse_QTKK(dat);
          break;
        case "QTKM":
          rtnCmd = Parse_QTKM(dat);
          break;
        case "QTKR":
          rtnCmd = Parse_QTKR(dat);
          break;
        case "QTKU":
          rtnCmd = Parse_QTKU(dat);
          break;

        case "RTKM":
          rtnCmd = Parse_RTKM(dat);
          break;
        case "RTKS":
          rtnCmd = Parse_RTKS(dat);
          break;
        case "RTKP":
          rtnCmd = Parse_RTKP(dat);
          break;
        case "RTKA":
          rtnCmd = Parse_RTKA(dat);
          break;
        case "RTKL":
          rtnCmd = Parse_RTKL(dat);
          break;
        case "RTKF":
          rtnCmd = Parse_RTKF(dat);
          break;
        case "RTKJ":
          rtnCmd = Parse_RTKJ(dat);
          break;
        case "RTKG":
          rtnCmd = Parse_RTKG(dat);
          break;
        case "RTKK":
          rtnCmd = Parse_RTKK(dat);
          break;
        case "RTKV":
          rtnCmd = Parse_RTKV(dat);
          break;
        case "RTKX":
          rtnCmd = Parse_RTKX(dat);
          break;
        case "RTKY":
          rtnCmd = Parse_RTKY(dat);
          break;
        case "RTKZ":
          rtnCmd = Parse_RTKZ(dat);
          break;

        case "RTKU":
          rtnCmd = Parse_RTKU(dat);
          break;
        case "RTK1":
          rtnCmd = Parse_RTKx(dat, 1);
          break;
        case "RTK2":
          rtnCmd = Parse_RTKx(dat, 2);
          break;
        case "RTK3":
          rtnCmd = Parse_RTKx(dat, 3);
          break;
        case "RTK4":
          rtnCmd = Parse_RTKx(dat, 4);
          break;
        case "RTKR":
          rtnCmd = Parse_RTKR(dat);
          break;

#if !TT_PARSER_APP || USE_2_1
        case "QTKN":
          rtnCmd = Parse_QTKN(dat);
          break;
        case "RTKN":
          rtnCmd = Parse_RTKN(dat);
          break;
        case "RTKI":
          rtnCmd = Parse_RTKI(dat);
          break;
        case "RTKT":
          rtnCmd = Parse_RTKT(dat);
          break;
#endif
        case "STKA":
          rtnCmd = Parse_STKA(dat);
          break;
        case "STKL":
          rtnCmd = Parse_STKL(dat);
          break;
        case "STKF":
          rtnCmd = Parse_STKF(dat);
          break;
        case "STKJ":
          rtnCmd = Parse_STKJ(dat);
          break;
        case "STKG":
          rtnCmd = Parse_STKG(dat);
          break;
        case "STKK":
          rtnCmd = Parse_STKK(dat);
          break;
        //case "STKV":
        //  rtnCmd = Parse_STKV(dat);
        //  break;
        case "STKX":
          rtnCmd = Parse_STKX(dat);
          break;
        case "STKY":
          rtnCmd = Parse_STKY(dat);
          break;
        case "STKZ":
          rtnCmd = Parse_STKZ(dat);
          break;
      }
      if (rtnCmd != null)
        rtnCmd.OriginalParseData = dat;

      return rtnCmd;
    }

    static public string SwitchToString(TT that)
    {
      //don't check OriginalParseData - it may not be up to date.
      switch (that.GetType().Name)
      {
        case "TT_CTKC": return ToString((TT_CTKC)that);
        case "TT_CTKF": return ToString((TT_CTKF)that);
        case "TT_CTKJ": return ToString((TT_CTKJ)that);
        case "TT_CTKG": return ToString((TT_CTKG)that);
        case "TT_CTKK": return ToString((TT_CTKK)that);
        case "TT_CTKP": return ToString((TT_CTKP)that);
        case "TT_CTKX": return ToString((TT_CTKX)that);
        case "TT_CTKY": return ToString((TT_CTKY)that);
        case "TT_CTKZ": return ToString((TT_CTKZ)that);

        case "TT_STKA": return ToString((TT_STKA)that);
        case "TT_STKF": return ToString((TT_STKF)that);
        case "TT_STKJ": return ToString((TT_STKJ)that);
        case "TT_STKL": return ToString((TT_STKL)that);
        case "TT_STKG": return ToString((TT_STKG)that);
        case "TT_STKK": return ToString((TT_STKK)that);
        case "TT_STKP": return ToString((TT_STKP)that);
        case "TT_STKX": return ToString((TT_STKX)that);
        case "TT_STKY": return ToString((TT_STKY)that);
        case "TT_STKZ": return ToString((TT_STKZ)that);
#if !TT_PARSER_APP || USE_2_1
        case "TT_STKI": return ToString((TT_STKI)that);
        case "TT_STKT": return ToString((TT_STKT)that);
#endif

        case "TT_QTKK": return ToString((TT_QTKK)that);
        case "TT_QTKD": return ToString((TT_QTKD)that);
        case "TT_QTKM": return ToString((TT_QTKM)that);
        case "TT_QTK_": return ToString((TT_QTK_)that);
        case "TT_QTKU": return ToString((TT_QTKU)that);
        case "TT_QTKR": return ToString((TT_QTKR)that);
#if !TT_PARSER_APP || USE_2_1
        case "TT_QTKN": return ToString((TT_QTKN)that);
#endif

        case "TT_RTKM": return ToString((TT_RTKM)that);
        case "TT_RTKS": return ToString((TT_RTKS)that);
        case "TT_RTKP": return ToString((TT_RTKP)that);
        case "TT_RTKA": return ToString((TT_RTKA)that);
        case "TT_RTKL": return ToString((TT_RTKL)that);
        case "TT_RTKF": return ToString((TT_RTKF)that);
        case "TT_RTKJ": return ToString((TT_RTKJ)that);
        case "TT_RTKG": return ToString((TT_RTKG)that);
        case "TT_RTKK": return ToString((TT_RTKK)that);
        case "TT_RTKV": return ToString((TT_RTKV)that);
        case "TT_RTKx": return ToString((TT_RTKx)that);
        case "TT_RTKX1":
        case "TT_RTKX": return ToString((TT_RTKX)that);
        case "TT_RTKY": return ToString((TT_RTKY)that);
        case "TT_RTKZ": return ToString((TT_RTKZ)that);
#if !TT_PARSER_APP || USE_2_1
        case "TT_RTKN": return ToString((TT_RTKN)that);
        case "TT_RTKI": return ToString((TT_RTKI)that);
        case "TT_RTKT": return ToString((TT_RTKT)that);
#endif
      }
      return null;
    }

    static public Regex taipRx = new Regex(@">[QRS]TK[A-Z0-9][^<>]*?(;PW=[A-Z0-9]{8})?(;ID=[A-Z0-9]{8})?;\*([0-9A-F\*]{0,2})<", RegexOptions.Compiled);
    static public IEnumerable<TT> ParseMany(string unitId, string messages)
    {
      MatchCollection taipMatches = taipRx.Matches(messages.ToUpper());

      foreach (Match taipMatch in taipMatches)
        yield return Parse(EnsurePWIDChkSum(unitId, taipMatch.Value, false));
    }

    static public IEnumerable<string> Split(string unitId, string messages)
    {
      MatchCollection taipMatches = taipRx.Matches(messages.ToUpper());

      foreach (Match taipMatch in taipMatches)
        yield return EnsurePWIDChkSum(unitId, taipMatch.Value, true);
    }

    static public string AddFrame(string message, bool hasPW)
    {
      return AddFrame(null, message, hasPW);
    }

    static public string AddFrame(string unitId, string message, bool hasPW)
    {
      return EnsurePWIDChkSum(unitId, ">" + message + ";*<", hasPW);
    }

    static public string RemoveFrame(string msg)
    {
      Match match = match = taipRx.Match(msg);
      if (!match.Success)
        return null;
      if (!string.IsNullOrEmpty(match.Groups[1].Value))
      {
        msg = msg.Substring(0, match.Groups[1].Index) + msg.Substring(match.Groups[1].Index + match.Groups[1].Length);
        match = taipRx.Match(msg);
      }
      if (!string.IsNullOrEmpty(match.Groups[2].Value))
      {
        msg = msg.Substring(0, match.Groups[2].Index) + msg.Substring(match.Groups[2].Index + match.Groups[2].Length);
        match = taipRx.Match(msg);
      }
      if (!string.IsNullOrEmpty(match.Groups[3].Value))
      {
        msg = msg.Substring(0, match.Groups[3].Index) + msg.Substring(match.Groups[3].Index + match.Groups[3].Length);
      }
      msg = msg.Trim('>', '<', '*', ';');
      return msg;
    }

    static public string EnsurePWIDChkSum(string unitId, string msg, bool hasPW)
    {
      Match match = match = taipRx.Match(msg);
      if (!match.Success)
        return null;
      if (hasPW && string.IsNullOrEmpty(match.Groups[1].Value))
      {
        if (string.IsNullOrEmpty(match.Groups[2].Value))
          msg = msg.Substring(0, msg.LastIndexOf(';') + 1) + "PW=00000000" + msg.Substring(msg.LastIndexOf(';'));
        else
        {
          int ndx = msg.LastIndexOf(';', match.Groups[2].Index);
          msg = msg.Substring(0, ndx + 1) + "PW=00000000" + msg.Substring(ndx);
        }
        match = taipRx.Match(msg);
      }
      if (string.IsNullOrEmpty(match.Groups[2].Value))
      {
        if (unitId == null)
          throw new NotSupportedException("ID=xx missing in message and not supplied as argument");
        msg = msg.Substring(0, msg.LastIndexOf(';') + 1) + "ID=" + unitId + msg.Substring(msg.LastIndexOf(';'));
        match = taipRx.Match(msg);
      }
      return AddTaipChecksum(msg);
    }

    static Regex csRx = new Regex(@";\*[0-9A-F\*]{0,2}<", RegexOptions.Compiled);
    static string AddTaipChecksum(string message)
    {
      Match csMatch = csRx.Match(message);
      if (csMatch.Success)
      {
        string chksum = ";*" + CalculateChkSum(message, csMatch.Index + 1) + "<";
        if (csMatch.Value.Length == 5 && csMatch.Value.Substring(2, 2) != "**")
          System.Diagnostics.Trace.WriteLineIf(csMatch.Value != chksum, "Bad checksum, dude");
        return message.Replace(csMatch.Value, chksum);
      }
      else
        return message;
    }

    public static string CalculateChkSum(TT cmd)
    {
      string msg = cmd.ToString();
      Match csMatch = csRx.Match(msg);
      if (csMatch.Success)
        return CalculateChkSum(msg, csMatch.Index + 1);
      return null;
    }

    static string CalculateChkSum(string message, int offset)
    {
      UInt16 checksum = 0;
      for (int i = 0; i <= offset; i++)
        checksum ^= (UInt16)message[i];
      return string.Format("{0:X2}", checksum);
    }

    static string SetIDPW(string dat, TT_OTA_CMD cmd)
    {
      string d = dat.Substring(dat.IndexOf('>'));
      cmd.ID = d.Substring(d.IndexOf(";ID=") + 4, 8);
      if (d.IndexOf(";PW=") != -1)
        cmd.PW = d.Substring(d.IndexOf(";PW=") + 4, 8);
      return d;
    }

    static string SetID(string dat, TT_ID_CMD cmd)
    {
      string d = dat.Substring(dat.IndexOf('>'));
      cmd.ID = d.Substring(d.IndexOf(";ID=") + 4, 8);
      return d.Substring(0, d.IndexOf(";"));
    }

    static byte GPSLeapSeconds = 15; // current as at Dec 2008
    static public DateTime ConvertGPSTimeToGMTDateTime(int GPSWeek, int GPSSecInWeek)
    {
      DateTime currDT = new DateTime(1980, 1, 6, 0, 0, 0);
      currDT = currDT.AddDays(GPSWeek * 7);
      currDT = currDT.AddSeconds(GPSSecInWeek);
      currDT = currDT.AddSeconds(-GPSLeapSeconds);
      return currDT;
    }

    #region -- *TKL --
    // ALERT_STATE
    // >RTKABCD;ID=YYYYYYYY;*ZZ<
    // 01234567
    static public TT_RTKL Parse_RTKL(string dat)
    {
      TT_RTKL ret = new TT_RTKL();

      string d = SetID(dat, ret);
      ret.Data = new ALERT_STATE();
      ret.Data.HPAStatus = d.Substring(5, 1);
      ret.Data.MPAStatus = d.Substring(6, 1);
      ret.Data.LPAStatus = d.Substring(7, 1);
      ret.OriginalParseData = dat;
      return ret;
    }

    static public TT_STKL Parse_STKL(string dat)
    {
      TT_STKL ret = new TT_STKL();

      string d = SetID(dat, ret);
      ret.Data = new ALERT_STATE();
      ret.Data.HPAStatus = d.Substring(5, 1);
      ret.Data.MPAStatus = d.Substring(6, 1);
      ret.Data.LPAStatus = d.Substring(7, 1);
      ret.OriginalParseData = dat;
      return ret;
    }

    static public string ToString(TT_RTKL that)
    {
      return AddFrame(string.Format("RTKL{0}{1}{2};ID={3}",
        (int)HPAStatus(that), (int)MPAStatus(that), (int)LPAStatus(that), that.ID), false);
    }
    /// <summary>
    /// HPA Status
    ///   0=Normal; 1=Activated; 2=Sent; 3=Acknowledged; 4=Monitor Activated
    /// </summary>
    static public ETTAlertState HPAStatus(TT_RTKL that) { return (ETTAlertState)Enum.Parse(typeof(ETTAlertState), that.Data.HPAStatus); }

    /// <summary>
    /// MPA Status
    ///   0=Normal; 1=Activated; 2=Sent; 3=Acknowledged; 4=Monitor Activated
    /// </summary>
    static public ETTAlertState MPAStatus(TT_RTKL that) { return (ETTAlertState)Enum.Parse(typeof(ETTAlertState), that.Data.MPAStatus); }

    /// <summary>
    /// LPA Status
    ///   0=Normal; 1=Activated; 2=Sent; 3=Acknowledged; 4=Monitor Activated
    /// </summary>
    static public ETTAlertState LPAStatus(TT_RTKL that) { return (ETTAlertState)Enum.Parse(typeof(ETTAlertState), that.Data.LPAStatus); }
    static public string ToString(TT_STKL that)
    {
      return AddFrame(string.Format("STKL{0}{1}{2}{3}{4}",
        that.Data.HPAStatus.Substring(0, 1), that.Data.MPAStatus.Substring(0, 1), that.Data.LPAStatus.Substring(0, 1),
        PW_CMD(that), ID_CMD(that)), true);
    }
    #endregion

    #region -- APP_CONFIG --
    static public TT_CTKC Parse_CTKC(string dat)
    {
      TT_CTKC ret = new TT_CTKC();

      string[] cols = dat.Split(',', ':', '=');

      ret.Data = new APP_CONFIG();
      ret.Data.IDLETimeout_T1 = Convert.ToInt32(cols[1].Trim());
      ret.Data.FIXTimeout_T2 = Convert.ToInt32(cols[2].Trim());
      ret.Data.TRANSMITTimeout_T3 = Convert.ToInt32(cols[3].Trim());
      ret.Data.DELAYTimeout_T4 = Convert.ToInt32(cols[4].Trim());
      ret.Data.QUERYTimeout_T5 = Convert.ToInt32(cols[5].Trim());
      ret.Data.AlmanacTimeout_T6 = Convert.ToInt32(cols[6].Trim());
      ret.Data.StaticMotionFilterTimeout_T7 = Convert.ToInt32(cols[7].Trim());
      ret.Data.MotionReportFlag = (EMotionReportFlag)Convert.ToInt32(cols[8].Trim());
      ret.Data.ReportDelayFlag = (EReportDelayFlag)Convert.ToInt32(cols[9].Trim());
      ret.Data.DiagnosticsMode = (EDiagnosticsMode)Convert.ToInt32(cols[10].Trim());
      ret.Data.CommunicationMode = (ECommunicationMode)Convert.ToInt32(cols[11].Trim());

      ret.OriginalParseData = dat;
      return ret;
    }
    // >RTKABBBBBBCCCCCCDDDDDDEEEEEEFFFFFFGGGTTHIJKLLLLMMMMMMNPP;ID=YYYYYYYY;*ZZ<
    // 012345678901234567890123456789012345678901234567890123456
    static public TT_STKA Parse_STKA(string dat)
    {
      TT_STKA ret = new TT_STKA();

      string d = SetID(dat, ret);
      ret.Data = new APP_CONFIG();
      ret.Data.IDLETimeout_T1 = Convert.ToInt32(d.Substring(5, 6));                         // BBBBBB
      ret.Data.FIXTimeout_T2 = Convert.ToInt32(d.Substring(11, 6));                         // CCCCCC
      ret.Data.TRANSMITTimeout_T3 = Convert.ToInt32(d.Substring(17, 6));                    // DDDDDD
      ret.Data.DELAYTimeout_T4 = Convert.ToInt32(d.Substring(23, 6));                       // EEEEEE
      ret.Data.QUERYTimeout_T5 = Convert.ToInt32(d.Substring(29, 6));                       // FFFFFF
      ret.Data.AlmanacTimeout_T6 = Convert.ToInt32(d.Substring(35, 3));                     // GGG
      ret.Data.StaticMotionFilterTimeout_T7 = Convert.ToInt32(d.Substring(38, 2));          // TT
      ret.Data.MotionReportFlag = (EMotionReportFlag)Convert.ToInt32(d.Substring(40, 1));   // H
      ret.Data.ReportDelayFlag = (EReportDelayFlag)Convert.ToInt32(d.Substring(41, 1));     // I
      ret.Data.DiagnosticsMode = (EDiagnosticsMode)Convert.ToInt32(d.Substring(42, 1));     // J
      ret.Data.CommunicationMode = (ECommunicationMode)Convert.ToInt32(d.Substring(43, 1)); // K
      ret.OriginalParseData = dat;
      return ret;
    }

    static public TT_RTKA Parse_RTKA(string dat)
    {
      TT_RTKA ret = new TT_RTKA();

      string d = SetID(dat, ret);
      ret.Data = new APP_CONFIG();
      ret.Data.IDLETimeout_T1 = Convert.ToInt32(d.Substring(5, 6));                         // BBBBBB
      ret.Data.FIXTimeout_T2 = Convert.ToInt32(d.Substring(11, 6));                         // CCCCCC
      ret.Data.TRANSMITTimeout_T3 = Convert.ToInt32(d.Substring(17, 6));                    // DDDDDD
      ret.Data.DELAYTimeout_T4 = Convert.ToInt32(d.Substring(23, 6));                       // EEEEEE
      ret.Data.QUERYTimeout_T5 = Convert.ToInt32(d.Substring(29, 6));                       // FFFFFF
      ret.Data.AlmanacTimeout_T6 = Convert.ToInt32(d.Substring(35, 3));                     // GGG
      ret.Data.StaticMotionFilterTimeout_T7 = Convert.ToInt32(d.Substring(38, 2));          // TT
      ret.Data.MotionReportFlag = (EMotionReportFlag)Convert.ToInt32(d.Substring(40, 1));   // H
      ret.Data.ReportDelayFlag = (EReportDelayFlag)Convert.ToInt32(d.Substring(41, 1));     // I
      ret.Data.DiagnosticsMode = (EDiagnosticsMode)Convert.ToInt32(d.Substring(42, 1));     // J
      ret.Data.CommunicationMode = (ECommunicationMode)Convert.ToInt32(d.Substring(43, 1)); // K
      if (d.Length > 44)
      {
        ret.BatteryChangeWeek = Convert.ToInt32(d.Substring(44, 4));                        // LLLL
        ret.BatteryChangeTime = Convert.ToInt32(d.Substring(48, 6));                        // MMMMMM
        if (d.Substring(54).Contains("."))
          ret.FirmwareVersion = new Version(d.Substring(54, 4));                            // N.PP
        else
          ret.FirmwareVersion = new Version(Convert.ToInt32(d.Substring(54, 1)), Convert.ToInt32(d.Substring(55, 2)));
      }
      ret.OriginalParseData = dat;
      return ret;
    }

    static public APP_CONFIG Create_APP_CONFIG(int IDLETimeout_T1, int FIXTimeout_T2, int TRANSMITTimeout_T3,
      int DELAYTimeout_T4, int QUERYTimeout_T5, int AlmanacTimeout_T6, int StaticMotionFilterTimeout_T7,
      EMotionReportFlag MotionReportFlag, EReportDelayFlag ReportDelayFlag, EDiagnosticsMode DiagnosticsMode,
      ECommunicationMode CommunicationMode)
    {
      APP_CONFIG data = new APP_CONFIG();
      data.IDLETimeout_T1 = IDLETimeout_T1;
      data.FIXTimeout_T2 = FIXTimeout_T2;
      data.TRANSMITTimeout_T3 = TRANSMITTimeout_T3;
      data.DELAYTimeout_T4 = DELAYTimeout_T4;
      data.QUERYTimeout_T5 = QUERYTimeout_T5;
      data.AlmanacTimeout_T6 = AlmanacTimeout_T6;
      data.StaticMotionFilterTimeout_T7 = StaticMotionFilterTimeout_T7;
      data.MotionReportFlag = MotionReportFlag;
      data.ReportDelayFlag = ReportDelayFlag;
      data.DiagnosticsMode = DiagnosticsMode;
      data.CommunicationMode = CommunicationMode;
      return data;
    }

    static public TT_CTKC CreateCTK(APP_CONFIG data)
    {
      TT_CTKC ret = new TT_CTKC();
      ret.Data = data;
      return ret;
    }

    static public TT_STKA CreateSTK(APP_CONFIG data, string PW)
    {
      TT_STKA ret = new TT_STKA();
      ret.PW = PW;
      ret.Data = data;
      return ret;
    }

    static public string ToString(TT_CTKC that)
    {
      // IDLE,FIX,TRANSMIT,DELAY,QUERY,Almanac,Static,Motion,Report,Diag,Comm
      return string.Format("AT+CTKC={0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
        that.Data.IDLETimeout_T1, that.Data.FIXTimeout_T2, that.Data.TRANSMITTimeout_T3, that.Data.DELAYTimeout_T4,
        that.Data.QUERYTimeout_T5, that.Data.AlmanacTimeout_T6, that.Data.StaticMotionFilterTimeout_T7,
        (int)that.Data.MotionReportFlag, (int)that.Data.ReportDelayFlag, (int)that.Data.DiagnosticsMode,
        (int)that.Data.CommunicationMode);
    }
    static public string ToString(int digits, int? value)
    {
      return value == null ? new string('?', digits) : value.Value.ToString("D" + digits);
    }
    static public string ToString(TT_STKA that)
    {
      return AddFrame(string.Format("STKA{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}",
        ToString(6, that.Data._IDLETimeout_T1), ToString(6, that.Data._FIXTimeout_T2), ToString(6, that.Data._TRANSMITTimeout_T3), ToString(6, that.Data._DELAYTimeout_T4),
        ToString(6, that.Data._QUERYTimeout_T5), ToString(3, that.Data._AlmanacTimeout_T6), ToString(2, that.Data._StaticMotionFilterTimeout_T7),
        ToString(1, (int?)that.Data._MotionReportFlag), ToString(1, (int?)that.Data._ReportDelayFlag), ToString(1, (int?)that.Data._DiagnosticsMode), ToString(1, (int?)that.Data._CommunicationMode),
        PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString(TT_RTKA that)
    {
      return AddFrame(string.Format("RTKA{0:D6}{1:D6}{2:D6}{3:D6}{4:D6}{5:D3}{6:D2}{7}{8}{9}{10}{11:D4}{12:D6}{13}{14:D2};ID={15}",
        that.Data.IDLETimeout_T1, that.Data.FIXTimeout_T2, that.Data.TRANSMITTimeout_T3, that.Data.DELAYTimeout_T4,
        that.Data.QUERYTimeout_T5, that.Data.AlmanacTimeout_T6, that.Data.StaticMotionFilterTimeout_T7,
        (int)that.Data.MotionReportFlag, (int)that.Data.ReportDelayFlag, (int)that.Data.DiagnosticsMode, (int)that.Data.CommunicationMode,
        that.BatteryChangeWeek, that.BatteryChangeTime,
        that.FirmwareVersion == null ? 0 : that.FirmwareVersion.Major, that.FirmwareVersion == null ? 0 : that.FirmwareVersion.Minor,
        that.ID), false);
    }
    #endregion

    #region -- GPRS_CONNECT_CONFIG --
    static public TT_CTKF Parse_CTKF(string dat)
    {
      TT_CTKF ret = new TT_CTKF();

      string[] cols = dat.Split(',', ':', '=');

      ret.Data = new GPRS_CONNECT_CONFIG();
      ret.Data.GPRSTransportProtocol = (EGPRSTransportProtocol)Convert.ToInt32(cols[1].Trim());
      ret.Data.GPRSSessionProtocol = (EGPRSSessionProtocol)Convert.ToInt32(cols[2].Trim());

      ret.Data.GPRSSessionKeepAliveTimeout_T25 = Convert.ToInt32(cols[3]);
      ret.Data.GPRSSessionTimeout_T26 = Convert.ToInt32(cols[4]);
      ret.Data.GPRSDestinationAddress = cols[5].TrimStart('"') + ':' + cols[6].TrimEnd('"');

      ret.OriginalParseData = dat;
      return ret;
    }
    // >RTKABCDDDDDEEEEEFFFFFFFFFFFFFFFFFFFFF”;ID=YYYYYYYY;*ZZ< 
    // 01234567890123456789012345678901234567
    static public TT_RTKF Parse_RTKF(string dat)
    {
      TT_RTKF ret = new TT_RTKF();

      string d = SetID(dat, ret);
      ret.Data = new GPRS_CONNECT_CONFIG();
      ret.Data.GPRSTransportProtocol = (EGPRSTransportProtocol)Convert.ToInt32(d.Substring(5, 1));
      ret.Data.GPRSSessionProtocol = (EGPRSSessionProtocol)Convert.ToInt32(d.Substring(6, 1));

      ret.Data.GPRSSessionKeepAliveTimeout_T25 = Convert.ToInt32(d.Substring(7, 5));
      ret.Data.GPRSSessionTimeout_T26 = Convert.ToInt32(d.Substring(12, 5));
      ret.Data.GPRSDestinationAddress = d.Substring(17, d.IndexOf('"', 17) - 17);
      ret.OriginalParseData = dat;
      return ret;
    }

    static public TT_STKF Parse_STKF(string dat)
    {
      TT_STKF ret = new TT_STKF();

      string d = SetID(dat, ret);
      ret.Data = new GPRS_CONNECT_CONFIG();
      ret.Data.GPRSTransportProtocol = (EGPRSTransportProtocol)Convert.ToInt32(d.Substring(5, 1));
      ret.Data.GPRSSessionProtocol = (EGPRSSessionProtocol)Convert.ToInt32(d.Substring(6, 1));

      ret.Data.GPRSSessionKeepAliveTimeout_T25 = Convert.ToInt32(d.Substring(7, 5));
      ret.Data.GPRSSessionTimeout_T26 = Convert.ToInt32(d.Substring(12, 5));
      ret.Data.GPRSDestinationAddress = d.Substring(17, d.IndexOf('"', 17) - 17);
      ret.OriginalParseData = dat;
      return ret;
    }

    static public GPRS_CONNECT_CONFIG Create_GPRS_CONNECT_CONFIG(EGPRSTransportProtocol GPRSTransportProtocol,
      EGPRSSessionProtocol GPRSSessionProtocol, int GPRSSessionKeepAliveTimeout_T25, int GPRSSessionTimeout_T26,
      string GPRSDestinationAddress)
    {
      GPRS_CONNECT_CONFIG data = new GPRS_CONNECT_CONFIG();
      data.GPRSTransportProtocol = GPRSTransportProtocol;
      data.GPRSSessionProtocol = GPRSSessionProtocol;
      data.GPRSSessionKeepAliveTimeout_T25 = GPRSSessionKeepAliveTimeout_T25;
      data.GPRSSessionTimeout_T26 = GPRSSessionTimeout_T26;
      data.GPRSDestinationAddress = GPRSDestinationAddress;
      return data;
    }

    static public TT_CTKF CreateCTK(GPRS_CONNECT_CONFIG data) { TT_CTKF ret = new TT_CTKF(); ret.Data = data; return ret; }
    static public TT_STKF CreateSTK(GPRS_CONNECT_CONFIG data, string PW) { TT_STKF ret = new TT_STKF(); ret.PW = PW; ret.Data = data; return ret; }

    static public string ToString(TT_CTKF that)
    {
      return string.Format("AT+CTKF={0},{1},{2},{3},\"{4}\"",
       (int)that.Data.GPRSTransportProtocol, (int)that.Data.GPRSSessionProtocol,
       that.Data.GPRSSessionKeepAliveTimeout_T25, that.Data.GPRSSessionTimeout_T26,
       that.Data.GPRSDestinationAddress);
    }
    static public string ToString(TT_STKF that)
    {
      return AddFrame(string.Format("STKF{0}{1}{2:D5}{3:D5}{4}\"{5}{6}",
        (int)that.Data.GPRSTransportProtocol, (int)that.Data.GPRSSessionProtocol,
        that.Data.GPRSSessionKeepAliveTimeout_T25, that.Data.GPRSSessionTimeout_T26,
        that.Data.GPRSDestinationAddress, PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString(TT_RTKF that)
    {
      return AddFrame(string.Format("RTKF{0}{1}{2:D5}{3:D5}{4}\";ID={5}",
        (int)that.Data.GPRSTransportProtocol, (int)that.Data.GPRSSessionProtocol,
        that.Data.GPRSSessionKeepAliveTimeout_T25, that.Data.GPRSSessionTimeout_T26,
        that.Data.GPRSDestinationAddress, that.ID), false);
    }
    #endregion

    #region -- GPRS_SETUP_CONFIG --
    static public TT_CTKJ Parse_CTKJ(string dat)
    {
      TT_CTKJ ret = new TT_CTKJ();

      string[] cols = dat.Split(',', ':', '=');

      ret.Data = new GPRS_SETUP_CONFIG();
      ret.Data.GPRSAPN = cols[1].Trim().Trim('"');
      ret.Data.GPRSUsername = cols[2].Trim('"');
      ret.Data.GPRSPassword = cols[3].Trim('"');

      ret.OriginalParseData = dat;
      return ret;
    }
    // >RTKABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB”CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC”DDDDDDDDDDDDDDDDDDDD”;ID=YYYYYYYY;*ZZ< 
    // 01234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456
    static public TT_RTKJ Parse_RTKJ(string dat)
    {
      TT_RTKJ ret = new TT_RTKJ();

      string d = SetID(dat, ret);
      ret.Data = new GPRS_SETUP_CONFIG();
      int p = d.IndexOf('"', 5);
      ret.Data.GPRSAPN = d.Substring(5, p - 5);
      ++p;
      int np = d.IndexOf('"', p);
      ret.Data.GPRSUsername = d.Substring(p, np - p);
      p = np + 1;
      np = d.IndexOf('"', p);
      ret.Data.GPRSPassword = d.Substring(p, np - p);

      ret.OriginalParseData = dat;
      return ret;
    }

    static public TT_STKJ Parse_STKJ(string dat)
    {
      TT_STKJ ret = new TT_STKJ();

      string d = SetID(dat, ret);
      ret.Data = new GPRS_SETUP_CONFIG();
      int p = d.IndexOf('"', 5);
      ret.Data.GPRSAPN = d.Substring(5, p - 5);
      ++p;
      int np = d.IndexOf('"', p);
      ret.Data.GPRSUsername = d.Substring(p, np - p);
      p = np + 1;
      np = d.IndexOf('"', p);
      ret.Data.GPRSPassword = d.Substring(p, np - p);

      ret.OriginalParseData = dat;
      return ret;
    } 
    
    static public GPRS_SETUP_CONFIG Create_GPRS_SETUP_CONFIG(string GPRSAPN, string GPRSUsername,
      string GPRSPassword)
    {
      GPRS_SETUP_CONFIG data = new GPRS_SETUP_CONFIG();
      data.GPRSAPN = GPRSAPN;
      data.GPRSUsername = GPRSUsername;
      data.GPRSPassword = GPRSPassword;
      return data;
    }

    static public TT_CTKJ CreateCTK(GPRS_SETUP_CONFIG data) { TT_CTKJ ret = new TT_CTKJ(); ret.Data = data; return ret; }
    static public TT_STKJ CreateSTK(GPRS_SETUP_CONFIG data, string PW) { TT_STKJ ret = new TT_STKJ(); ret.PW = PW; ret.Data = data; return ret; }

    static public string ToString(TT_CTKJ that)
    {
      return string.Format("AT+CTKJ=\"{0}\",\"{1}\",\"{2}\"",
        that.Data.GPRSAPN, that.Data.GPRSUsername, that.Data.GPRSPassword);
    }
    static public string ToString(TT_STKJ that)
    {
      return AddFrame(string.Format("STKJ{0}\"{1}\"{2}\"{3}{4}",
        that.Data.GPRSAPN, that.Data.GPRSUsername, that.Data.GPRSPassword, PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString(TT_RTKJ that)
    {
      return AddFrame(string.Format("RTKJ{0}\"{1}\"{2}\";ID={3}",
        that.Data.GPRSAPN, that.Data.GPRSUsername, that.Data.GPRSPassword, that.ID), false);
    }
    #endregion

    #region -- GPS_CONFIG --
    static public TT_CTKG Parse_CTKG(string dat)
    {
      TT_CTKG ret = new TT_CTKG();

      string[] cols = dat.Split(',', ':', '=');

      ret.Data = new GPS_CONFIG();
      ret.Data.GPSElevationMask = Convert.ToInt32(cols[1]);
      ret.Data.GPSPDOPMask = Convert.ToInt32(cols[2]);
      ret.Data.GPSPDOPSwitch = Convert.ToInt32(cols[3]);
      ret.Data.GPSSignalMask = Convert.ToInt32(cols[4]);
      ret.Data.GPSDynamicsMode = Convert.ToInt32(cols[5]);

      ret.OriginalParseData = dat;
      return ret;
    }
    // >RTKABBCCCDDDEEEF;ID=YYYYYYYY;*ZZ<
    // 01234567890123456
    static public TT_RTKG Parse_RTKG(string dat)
    {
      TT_RTKG ret = new TT_RTKG();

      string d = SetID(dat, ret);
      ret.Data = new GPS_CONFIG();
      ret.Data.GPSElevationMask = Convert.ToInt32(d.Substring(5, 2));
      ret.Data.GPSPDOPMask = Convert.ToInt32(d.Substring(7, 3));
      ret.Data.GPSPDOPSwitch = Convert.ToInt32(d.Substring(10, 3));
      ret.Data.GPSSignalMask = Convert.ToInt32(d.Substring(13, 3));
      ret.Data.GPSDynamicsMode = Convert.ToInt32(d.Substring(16, 1));

      ret.OriginalParseData = dat;
      return ret;
    }

    static public TT_STKG Parse_STKG(string dat)
    {
      TT_STKG ret = new TT_STKG();

      string d = SetID(dat, ret);
      ret.Data = new GPS_CONFIG();
      ret.Data.GPSElevationMask = Convert.ToInt32(d.Substring(5, 2));
      ret.Data.GPSPDOPMask = Convert.ToInt32(d.Substring(7, 3));
      ret.Data.GPSPDOPSwitch = Convert.ToInt32(d.Substring(10, 3));
      ret.Data.GPSSignalMask = Convert.ToInt32(d.Substring(13, 3));
      ret.Data.GPSDynamicsMode = Convert.ToInt32(d.Substring(16, 1));

      ret.OriginalParseData = dat;
      return ret;
    }

    static public GPS_CONFIG Create_GPS_CONFIG(int GPSElevationMask, int GPSPDOPMask, int GPSPDOPSwitch,
      int GPSSignalMask, int GPSDynamicsMode)
    {
      GPS_CONFIG data = new GPS_CONFIG();
      data.GPSElevationMask = GPSElevationMask;
      data.GPSPDOPMask = GPSPDOPMask;
      data.GPSPDOPSwitch = GPSPDOPSwitch;
      data.GPSSignalMask = GPSSignalMask;
      data.GPSDynamicsMode = GPSDynamicsMode;
      return data;
    }

    static public TT_CTKG CreateCTK(GPS_CONFIG data) { TT_CTKG ret = new TT_CTKG(); ret.Data = data; return ret; }
    static public TT_STKG CreateSTK(GPS_CONFIG data, string PW) { TT_STKG ret = new TT_STKG(); ret.PW = PW; ret.Data = data; return ret; }

    static public string ToString(TT_CTKG that)
    {
      return string.Format("AT+CTKG={0},{1},{2},{3},{4}",
        that.Data.GPSElevationMask, that.Data.GPSPDOPMask, that.Data.GPSPDOPSwitch, that.Data.GPSSignalMask, that.Data.GPSDynamicsMode);
    }
    static public string ToString(TT_STKG that)
    {
      return AddFrame(string.Format("STKG{0:D2}{1:D3}{2:D3}{3:D3}{4}{5}{6}",
        that.Data.GPSElevationMask, that.Data.GPSPDOPMask, that.Data.GPSPDOPSwitch,
        that.Data.GPSSignalMask, that.Data.GPSDynamicsMode, PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString(TT_RTKG that)
    {
      return AddFrame(string.Format("RTKG{0:D2}{1:D3}{2:D3}{3:D3}{4};ID={5}",
        that.Data.GPSElevationMask, that.Data.GPSPDOPMask, that.Data.GPSPDOPSwitch,
        that.Data.GPSSignalMask, that.Data.GPSDynamicsMode, that.ID), false);
    }
    #endregion

    #region -- GEOFENCE_CONFIG --
    static public TT_CTKK Parse_CTKK(string dat)
    {
      TT_CTKK ret = new TT_CTKK();

      string[] cols = dat.Split(',', ':', '=');

      ret.Data = new GEOFENCE_CONFIG();
      ret.Data.GeofenceID = Convert.ToInt32(cols[1]);
      ret.Data.GeofenceEnable = (EGeofenceEnable)Convert.ToInt32(cols[2].Trim());
      ret.Data.GeofenceEnforcement = (EGeofenceEnforcement)Convert.ToInt32(cols[3].Trim());

      ret.Data.GeofenceDeltaX = Convert.ToInt32(cols[4].Trim());
      ret.Data.GeofenceDeltaY = Convert.ToInt32(cols[5].Trim());

      ret.Data.GeofenceCenterLatitude = Convert.ToDouble(cols[6].Trim('"'));
      ret.Data.GeofenceCenterLongitude = Convert.ToDouble(cols[7].Trim(' ', '"'));

      ret.OriginalParseData = dat;
      return ret;
    }
    // >RTKABBCDEEEEEFFFFFGGGHHHHHHHIIIIJJJJJJJ;ID=YYYYYYYY;*ZZ< 
    // 0123456789012345678901234567890123456789
    static public TT_RTKK Parse_RTKK(string dat)
    {
      TT_RTKK ret = new TT_RTKK();

      string d = SetID(dat, ret);
      ret.Data = new GEOFENCE_CONFIG();
      ret.Data.GeofenceID = Convert.ToInt32(d.Substring(5, 2));
      ret.Data.GeofenceEnable = (EGeofenceEnable)Convert.ToInt32(d.Substring(7, 1));
      ret.Data.GeofenceEnforcement = (EGeofenceEnforcement)Convert.ToInt32(d.Substring(8, 1));
      ret.Data.GeofenceDeltaX = Convert.ToInt32(d.Substring(9, 5));
      ret.Data.GeofenceDeltaY = Convert.ToInt32(d.Substring(14, 5));
      ret.Data.GeofenceCenterLatitude = Convert.ToDouble(d.Substring(19, 3) + "." + d.Substring(22, 7));
      ret.Data.GeofenceCenterLongitude = Convert.ToDouble(d.Substring(29, 4) + "." + d.Substring(33, 7));
      ret.OriginalParseData = dat;
      return ret;
    }

    static public TT_STKK Parse_STKK(string dat)
    {
      TT_STKK ret = new TT_STKK();

      string d = SetID(dat, ret);
      ret.Data = new GEOFENCE_CONFIG();
      ret.Data.GeofenceID = Convert.ToInt32(d.Substring(5, 2));
      ret.Data.GeofenceEnable = (EGeofenceEnable)Convert.ToInt32(d.Substring(7, 1));
      ret.Data.GeofenceEnforcement = (EGeofenceEnforcement)Convert.ToInt32(d.Substring(8, 1));
      ret.Data.GeofenceDeltaX = Convert.ToInt32(d.Substring(9, 5));
      ret.Data.GeofenceDeltaY = Convert.ToInt32(d.Substring(14, 5));
      ret.Data.GeofenceCenterLatitude = Convert.ToDouble(d.Substring(19, 3) + "." + d.Substring(22, 7));
      ret.Data.GeofenceCenterLongitude = Convert.ToDouble(d.Substring(29, 4) + "." + d.Substring(33, 7));
      ret.OriginalParseData = dat;
      return ret;
    }

    static public GEOFENCE_CONFIG Create_GEOFENCE_CONFIG(int GeofenceID, EGeofenceEnable GeofenceEnable, EGeofenceEnforcement GeofenceEnforcement,
      int GeofenceDeltaX, int GeofenceDeltaY, double GeofenceCenterLatitude, double GeofenceCenterLongitude)
    {
      GEOFENCE_CONFIG data = new GEOFENCE_CONFIG();
      data.GeofenceID = GeofenceID;
      data.GeofenceEnable = GeofenceEnable;
      data.GeofenceEnforcement = GeofenceEnforcement;
      data.GeofenceDeltaX = GeofenceDeltaX;
      data.GeofenceDeltaY = GeofenceDeltaY;
      data.GeofenceCenterLatitude = GeofenceCenterLatitude;
      data.GeofenceCenterLongitude = GeofenceCenterLongitude;
      return data;
    }

    static public TT_CTKK CreateCTK(GEOFENCE_CONFIG data) { TT_CTKK ret = new TT_CTKK(); ret.Data = data; return ret; }
    static public TT_STKK CreateSTK(GEOFENCE_CONFIG data, string PW) { TT_STKK ret = new TT_STKK(); ret.PW = PW; ret.Data = data; return ret; }

    static public string ToString(TT_CTKK that)
    {
      return string.Format("AT+CTKK={0},{1},{2},{3},{4},\"{5:+0.0000000;-0.0000000}\",\"{6:+0.0000000;-0.0000000}\"",
        that.Data.GeofenceID, (int)that.Data.GeofenceEnable, (int)that.Data.GeofenceEnforcement,
        that.Data.GeofenceDeltaX, that.Data.GeofenceDeltaY,
        that.Data.GeofenceCenterLatitude, that.Data.GeofenceCenterLongitude);
    }
    static public string ToString(TT_STKK that)
    {
      return AddFrame(string.Format("STKK{0:D2}{1}{2}{3:D5}{4:D5}{5:+000000000;-000000000}{6:+0000000000;-0000000000}{7}{8}",
        that.Data.GeofenceID, (int)that.Data.GeofenceEnable, (int)that.Data.GeofenceEnforcement,
        that.Data.GeofenceDeltaX, that.Data.GeofenceDeltaY,
        that.Data.GeofenceCenterLatitude * 10000000, that.Data.GeofenceCenterLongitude * 10000000, PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString(TT_RTKK that)
    {
      return AddFrame(string.Format("RTKK{0:D2}{1}{2}{3:D5}{4:D5}{5:+000000000;-000000000}{6:+0000000000;-0000000000};ID={7}",
        that.Data.GeofenceID, (int)that.Data.GeofenceEnable, (int)that.Data.GeofenceEnforcement,
        that.Data.GeofenceDeltaX, that.Data.GeofenceDeltaY,
        that.Data.GeofenceCenterLatitude * 10000000, that.Data.GeofenceCenterLongitude * 10000000, that.ID), false);
    }
    static public string ToString(TT_QTKK that)
    {
      return AddFrame(string.Format("QTKK{0:D2}{1}{2}", that.GeofenceID, PW_CMD(that), ID_CMD(that)), true);
    }
    static public TT_QTKK Parse_QTKK(string dat)
    {
      TT_QTKK ret = new TT_QTKK();

      string d = SetIDPW(dat, ret);
      ret.GeofenceID = Convert.ToInt32(d.Substring(5, 2));
      ret.OriginalParseData = dat;
      return ret;
    }

    #endregion

    #region -- PROV_CONFIG --
    static public TT_CTKP Parse_CTKP(string dat)
    {
      TT_CTKP ret = new TT_CTKP();

      //parse
      //string dat1 = "\"ID\",\"sms d a\",\"***\",\"*******\"";
      string[] cols = dat.Split(',', '=', ':');

      ret.Data = new PROV_CONFIG();
      ret.UnitID = cols[1].Trim().Trim('"');
      ret.Data.SMSDestinationAddress = cols[2].Trim('"');
      ret.SIMPIN = cols[3].Trim('"');
      ret.SecurityPassword = cols[4].Trim('"');

      ret.OriginalParseData = dat;
      return ret;
    }
    // >RTKABBBBBBBBBBBBBBBBBBBBBBBB;ID=YYYYYYYY;*ZZ< 
    // 01234567890123456789012345678
    static public TT_RTKV Parse_RTKV(string dat)
    {
      TT_RTKV ret = new TT_RTKV();

      string d = SetID(dat, ret);
      ret.Data = new PROV_CONFIG();
      ret.Data.SMSDestinationAddress = d.Substring(5, 24).Trim();
      ret.OriginalParseData = dat;
      return ret;
    }

    static public PROV_CONFIG Create_PROV_CONFIG(string SMSDestinationAddress)
    {
      PROV_CONFIG data = new PROV_CONFIG();
      data.SMSDestinationAddress = SMSDestinationAddress;
      return data;
    }

    static public TT_CTKP CreateCTK(PROV_CONFIG data, string UnitID, string SIMPIN, string SecurityPassword)
    {
      TT_CTKP ret = new TT_CTKP();
      ret.Data = data;
      ret.UnitID = UnitID;
      ret.SIMPIN = SIMPIN;
      ret.SecurityPassword = SecurityPassword;
      return ret;
    }
    static public TT_STKP CreateSTK(PROV_CONFIG data, string PW) { TT_STKP ret = new TT_STKP(); ret.PW = PW; ret.Data = data; return ret; }

    static public string ToString(TT_CTKP that)
    {
      return string.Format("AT+CTKP=\"{0}\",\"{1}\",\"{2}\",\"{3}\"",
        that.UnitID, that.Data.SMSDestinationAddress, that.SIMPIN, that.SecurityPassword);
    }
    static public string ToString(TT_STKP that)
    {
      return AddFrame(string.Format("STKV{0}{1}{2}",
        that.Data.SMSDestinationAddress.PadLeft(24, ' '), PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString(TT_RTKP that)
    {
      if (that.PositionData == null)
      {
        TT_RTKS tmp = new TT_RTKS();
        tmp.ID = that.ID;
        tmp.Data = that.Data;
        return ToString(tmp);
      }
      return AddFrame(string.Format("RTKP{0:X4}{1}{2:D3}{3}{4:D4}{5:D6}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17}{18:+000000000;-000000000}{19:+0000000000;-0000000000}{20:+00000;-00000}{21:D3}{22:D3};ID={23}",
        that.Data.ProtocolSequenceNumber, (int)that.Data.TriggerType, that.Data.BatteryLevel, that.Data.BatteryChangedFlag ? 'T' : 'F',
        that.Data.GPSTimeWeek, that.Data.GPSTimeSeconds, (int)that.Data.GPSStatusCode, (int)that.Data.GSMStatusCode,
        (int)that.Data.PositionAge, (int)that.Data.HPAStatus, (int)that.Data.MPAStatus, (int)that.Data.LPAStatus,
        (int)that.Data.ExternalPower, (int)that.Data.GeofenceStatus, (int)that.Data.ExtendedGPSStatusCode,
        (int)that.Data.SpeedingStatus, (int)that.Data.ScheduledHoursFlag, that.Data.Reserved,
        that.PositionData.Latitude * 10000000, that.PositionData.Longitude * 10000000, that.PositionData.Altitude, that.PositionData.Speed, that.PositionData.Heading,
        that.ID), false);
    }
    #endregion

    #region -- EXT_APP_CONFIG --
    static public TT_CTKX Parse_CTKX(string dat)
    {
      TT_CTKX ret = new TT_CTKX();

      //parse
      string[] cols = dat.Split(',', ':', '=');

      ret.Data = new EXT_APP_CONFIG();
      ret.Data.ScheduledReportMode = (EScheduledReportMode)Convert.ToInt32(cols[1].Trim());
      ret.Data.ScheduledReportTime_T18 = Convert.ToInt32(cols[2].Trim());
      ret.Data.InMotionPolling = (EInMotionPolling)Convert.ToInt32(cols[3].Trim());
      ret.Data.AnytimePolling = (EAnytimePolling)Convert.ToInt32(cols[4].Trim());

      ret.Data.PollingDutyCycleFrequency_T19 = Convert.ToInt32(cols[5].Trim());
      ret.Data.PollingDutyCycleOnTime_T20 = Convert.ToInt32(cols[6].Trim());

      ret.Data.QueryHoldFlag = (EFlag)Convert.ToInt32(cols[7].Trim());

      ret.Data.PositionReportTransmitAttempts_N1 = Convert.ToInt32(cols[9].Trim());
      ret.Data.StatusMessageTransmitAttempts_N2 = Convert.ToInt32(cols[10].Trim());

      ret.Data.StaticMotionFilterCounter_N3 = Convert.ToInt32(cols[11].Trim());
      ret.Data.DynamicMotionFilterTimeout_T21 = Convert.ToInt32(cols[12].Trim());
      ret.Data.DynamicMotionFilterCounter_N4 = Convert.ToInt32(cols[13].Trim());

      ret.Data.MotionSensorOverride = (EMotionSensorOverride)Convert.ToInt32(cols[14].Trim());

      ret.OriginalParseData = dat;
      return ret;
    }
    // >RTKABCCCCCCDEFFFFFFGGGGGGHIJJJKKKLLMMNNO;ID=YYYYYYYY;*ZZ< 
    // 01234567890123456789012345678901234567890
    static public TT_RTKX Parse_RTKX(string dat)
    {
      TT_RTKX ret = new TT_RTKX();

      string d = SetID(dat, ret);
      ret.Data = new EXT_APP_CONFIG();
      ret.Data.ScheduledReportMode = (EScheduledReportMode)Convert.ToInt32(d.Substring(5, 1));
      ret.Data.ScheduledReportTime_T18 = Convert.ToInt32(d.Substring(6, 6));
      ret.Data.InMotionPolling = (EInMotionPolling)Convert.ToInt32(d.Substring(12, 1));
      ret.Data.AnytimePolling = (EAnytimePolling)Convert.ToInt32(d.Substring(13, 1));
      ret.Data.PollingDutyCycleFrequency_T19 = Convert.ToInt32(d.Substring(14, 6));
      ret.Data.PollingDutyCycleOnTime_T20 = Convert.ToInt32(d.Substring(20, 6));
      ret.Data.QueryHoldFlag = (EFlag)Convert.ToInt32(d.Substring(26, 1));
      ret.Data.Reserved = Convert.ToInt32(d.Substring(27, 1));
      ret.Data.PositionReportTransmitAttempts_N1 = Convert.ToInt32(d.Substring(28, 3));
      ret.Data.StatusMessageTransmitAttempts_N2 = Convert.ToInt32(d.Substring(31, 3));
      ret.Data.StaticMotionFilterCounter_N3 = Convert.ToInt32(d.Substring(34, 2));
      ret.Data.DynamicMotionFilterTimeout_T21 = Convert.ToInt32(d.Substring(36, 2));
      ret.Data.DynamicMotionFilterCounter_N4 = Convert.ToInt32(d.Substring(38, 2));
      ret.Data.MotionSensorOverride = (EMotionSensorOverride)Convert.ToInt32(d.Substring(40, 1));
      ret.OriginalParseData = dat;
      return ret;
    }

    static public TT_STKX Parse_STKX(string dat)
    {
      TT_STKX ret = new TT_STKX();

      string d = SetID(dat, ret);
      ret.Data = new EXT_APP_CONFIG();
      ret.Data.ScheduledReportMode = (EScheduledReportMode)Convert.ToInt32(d.Substring(5, 1));
      ret.Data.ScheduledReportTime_T18 = Convert.ToInt32(d.Substring(6, 6));
      ret.Data.InMotionPolling = (EInMotionPolling)Convert.ToInt32(d.Substring(12, 1));
      ret.Data.AnytimePolling = (EAnytimePolling)Convert.ToInt32(d.Substring(13, 1));
      ret.Data.PollingDutyCycleFrequency_T19 = Convert.ToInt32(d.Substring(14, 6));
      ret.Data.PollingDutyCycleOnTime_T20 = Convert.ToInt32(d.Substring(20, 6));
      ret.Data.QueryHoldFlag = (EFlag)Convert.ToInt32(d.Substring(26, 1));
      ret.Data.Reserved = Convert.ToInt32(d.Substring(27, 1));
      ret.Data.PositionReportTransmitAttempts_N1 = Convert.ToInt32(d.Substring(28, 3));
      ret.Data.StatusMessageTransmitAttempts_N2 = Convert.ToInt32(d.Substring(31, 3));
      ret.Data.StaticMotionFilterCounter_N3 = Convert.ToInt32(d.Substring(34, 2));
      ret.Data.DynamicMotionFilterTimeout_T21 = Convert.ToInt32(d.Substring(36, 2));
      ret.Data.DynamicMotionFilterCounter_N4 = Convert.ToInt32(d.Substring(38, 2));
      ret.Data.MotionSensorOverride = (EMotionSensorOverride)Convert.ToInt32(d.Substring(40, 1));
      ret.OriginalParseData = dat;
      return ret;
    }

    static public EXT_APP_CONFIG Create_EXT_APP_CONFIG(EScheduledReportMode ScheduledReportMode, int ScheduledReportTime_T18,
      EInMotionPolling InMotionPolling, EAnytimePolling AnytimePolling, int PollingDutyCycleFrequency_T19,
      int PollingDutyCycleOnTime_T20, EFlag QueryHoldFlag, int Reserved, int PositionReportTransmitAttempts_N1,
      int StatusMessageTransmitAttempts_N2, int StaticMotionFilterCounter_N3, int DynamicMotionFilterTimeout_T21,
      int DynamicMotionFilterCounter_N4, EMotionSensorOverride MotionSensorOverride)
    {
      EXT_APP_CONFIG data = new EXT_APP_CONFIG();
      data.ScheduledReportMode = ScheduledReportMode;
      data.ScheduledReportTime_T18 = ScheduledReportTime_T18;
      data.InMotionPolling = InMotionPolling;
      data.AnytimePolling = AnytimePolling;
      data.PollingDutyCycleFrequency_T19 = PollingDutyCycleFrequency_T19;
      data.PollingDutyCycleOnTime_T20 = PollingDutyCycleOnTime_T20;
      data.QueryHoldFlag = QueryHoldFlag;
      data.Reserved = Reserved;
      data.PositionReportTransmitAttempts_N1 = PositionReportTransmitAttempts_N1;
      data.StatusMessageTransmitAttempts_N2 = StatusMessageTransmitAttempts_N2;
      data.StaticMotionFilterCounter_N3 = StaticMotionFilterCounter_N3;
      data.DynamicMotionFilterTimeout_T21 = DynamicMotionFilterTimeout_T21;
      data.DynamicMotionFilterCounter_N4 = DynamicMotionFilterCounter_N4;
      data.MotionSensorOverride = MotionSensorOverride;
      return data;
    }

    static public TT_CTKX CreateCTK(EXT_APP_CONFIG data) { TT_CTKX ret = new TT_CTKX(); ret.Data = data; return ret; }
    static public TT_STKX CreateSTK(EXT_APP_CONFIG data, string PW) { TT_STKX ret = new TT_STKX(); ret.PW = PW; ret.Data = data; return ret; }

    static public string ToString(TT_CTKX that)
    {
      return string.Format("AT+CTKX={0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
        (int)that.Data.ScheduledReportMode, that.Data.ScheduledReportTime_T18, (int)that.Data.InMotionPolling,
        (int)that.Data.AnytimePolling, that.Data.PollingDutyCycleFrequency_T19, that.Data.PollingDutyCycleOnTime_T20,
        (int)that.Data.QueryHoldFlag, that.Data.Reserved, that.Data.PositionReportTransmitAttempts_N1,
        that.Data.StatusMessageTransmitAttempts_N2, that.Data.StaticMotionFilterCounter_N3,
        that.Data.DynamicMotionFilterTimeout_T21, that.Data.DynamicMotionFilterCounter_N4,
        (int)that.Data.MotionSensorOverride);
    }
    static public string ToString(TT_STKX that)
    {
      return AddFrame(string.Format("STKX{0}{1:D6}{2}{3}{4:D6}{5:D6}{6}{7}{8:D3}{9:D3}{10:D2}{11:D2}{12:D2}{13}{14}{15}",
        (int)that.Data.ScheduledReportMode, that.Data.ScheduledReportTime_T18, (int)that.Data.InMotionPolling,
        (int)that.Data.AnytimePolling, that.Data.PollingDutyCycleFrequency_T19, that.Data.PollingDutyCycleOnTime_T20,
        (int)that.Data.QueryHoldFlag, that.Data.Reserved, that.Data.PositionReportTransmitAttempts_N1,
        that.Data.StatusMessageTransmitAttempts_N2, that.Data.StaticMotionFilterCounter_N3,
        that.Data.DynamicMotionFilterTimeout_T21, that.Data.DynamicMotionFilterCounter_N4,
        (int)that.Data.MotionSensorOverride,
        PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString(TT_RTKX that)
    {
      return AddFrame(string.Format("RTKX{0}{1:D6}{2}{3}{4:D6}{5:D6}{6}{7}{8:D3}{9:D3}{10:D2}{11:D2}{12:D2}{13};ID={14}",
        (int)that.Data.ScheduledReportMode, that.Data.ScheduledReportTime_T18, (int)that.Data.InMotionPolling,
        (int)that.Data.AnytimePolling, that.Data.PollingDutyCycleFrequency_T19, that.Data.PollingDutyCycleOnTime_T20,
        (int)that.Data.QueryHoldFlag, that.Data.Reserved, that.Data.PositionReportTransmitAttempts_N1,
        that.Data.StatusMessageTransmitAttempts_N2, that.Data.StaticMotionFilterCounter_N3,
        that.Data.DynamicMotionFilterTimeout_T21, that.Data.DynamicMotionFilterCounter_N4,
        (int)that.Data.MotionSensorOverride,
        that.ID), false);
    }
    #endregion

    #region -- MODULE_APP_CONFIG --
    static public TT_CTKY Parse_CTKY(string dat)
    {
      TT_CTKY ret = new TT_CTKY();

      string[] cols = dat.Split(',', ':', '=');

      ret.Data = new MODULE_APP_CONFIG();
      ret.Data.HPAIdleTimeout_T11 = Convert.ToInt32(cols[1].Trim());
      ret.Data.MPAIdleTimeout_T12 = Convert.ToInt32(cols[2].Trim());
      ret.Data.HPADelayTimeout_T13 = Convert.ToInt32(cols[3].Trim());
      ret.Data.MPADelayTimeout_T14 = Convert.ToInt32(cols[4].Trim());

      ret.Data.HPATransmitTimeout_T15 = Convert.ToInt32(cols[5].Trim());
      ret.Data.MPATransmitTimeout_T16 = Convert.ToInt32(cols[6].Trim());
      ret.Data.HPAQueryTimeout_T17 = Convert.ToInt32(cols[7].Trim());

      ret.Data.HPATransmitAttempts_N5 = Convert.ToInt32(cols[8].Trim());
      ret.Data.MPATransmitAttempts_N6 = Convert.ToInt32(cols[9].Trim());
      ret.Data.LPATransmitAttempts_N7 = Convert.ToInt32(cols[10].Trim());

      ret.Data.HPAMode = (EMode_Y)Convert.ToInt32(cols[11].Trim());
      ret.Data.MPAMode = (EMode_Y)Convert.ToInt32(cols[12].Trim());
      ret.Data.LPAMode = (EMode_Y)Convert.ToInt32(cols[13].Trim());

      ret.OriginalParseData = dat;
      return ret;
    }
    // >RTKABBBBBBCCCCCCDDDDDDEEEEEEFFFFFFGGGGGGHHHHHHIIIJJJKKKLMN;ID=YYYYYYYY;*ZZ< 
    // 01234567890123456789012345678901234567890123456789012345678
    static public TT_RTKY Parse_RTKY(string dat)
    {
      TT_RTKY ret = new TT_RTKY();

      string d = SetID(dat, ret);
      ret.Data = new MODULE_APP_CONFIG();
      ret.Data.HPAIdleTimeout_T11 = Convert.ToInt32(d.Substring(5, 6));
      ret.Data.MPAIdleTimeout_T12 = Convert.ToInt32(d.Substring(11, 6));
      ret.Data.HPADelayTimeout_T13 = Convert.ToInt32(d.Substring(17, 6));
      ret.Data.MPADelayTimeout_T14 = Convert.ToInt32(d.Substring(23, 6));
      ret.Data.HPATransmitTimeout_T15 = Convert.ToInt32(d.Substring(29, 6));
      ret.Data.MPATransmitTimeout_T16 = Convert.ToInt32(d.Substring(35, 6));
      ret.Data.HPAQueryTimeout_T17 = Convert.ToInt32(d.Substring(41, 6));
      ret.Data.HPATransmitAttempts_N5 = Convert.ToInt32(d.Substring(47, 3));
      ret.Data.MPATransmitAttempts_N6 = Convert.ToInt32(d.Substring(50, 3));
      ret.Data.LPATransmitAttempts_N7 = Convert.ToInt32(d.Substring(53, 3));
      ret.Data.HPAMode = (EMode_Y)Convert.ToInt32(d.Substring(56, 1));
      ret.Data.MPAMode = (EMode_Y)Convert.ToInt32(d.Substring(57, 1));
      ret.Data.LPAMode = (EMode_Y)Convert.ToInt32(d.Substring(58, 1));
      ret.OriginalParseData = dat;
      return ret;
    }

    static public TT_STKY Parse_STKY(string dat)
    {
      TT_STKY ret = new TT_STKY();

      string d = SetID(dat, ret);
      ret.Data = new MODULE_APP_CONFIG();
      ret.Data.HPAIdleTimeout_T11 = Convert.ToInt32(d.Substring(5, 6));
      ret.Data.MPAIdleTimeout_T12 = Convert.ToInt32(d.Substring(11, 6));
      ret.Data.HPADelayTimeout_T13 = Convert.ToInt32(d.Substring(17, 6));
      ret.Data.MPADelayTimeout_T14 = Convert.ToInt32(d.Substring(23, 6));
      ret.Data.HPATransmitTimeout_T15 = Convert.ToInt32(d.Substring(29, 6));
      ret.Data.MPATransmitTimeout_T16 = Convert.ToInt32(d.Substring(35, 6));
      ret.Data.HPAQueryTimeout_T17 = Convert.ToInt32(d.Substring(41, 6));
      ret.Data.HPATransmitAttempts_N5 = Convert.ToInt32(d.Substring(47, 3));
      ret.Data.MPATransmitAttempts_N6 = Convert.ToInt32(d.Substring(50, 3));
      ret.Data.LPATransmitAttempts_N7 = Convert.ToInt32(d.Substring(53, 3));
      ret.Data.HPAMode = (EMode_Y)Convert.ToInt32(d.Substring(56, 1));
      ret.Data.MPAMode = (EMode_Y)Convert.ToInt32(d.Substring(57, 1));
      ret.Data.LPAMode = (EMode_Y)Convert.ToInt32(d.Substring(58, 1));
      ret.OriginalParseData = dat;
      return ret;
    }

    static public MODULE_APP_CONFIG Create_MODULE_APP_CONFIG(int HPAIdleTimeout_T11, int MPAIdleTimeout_T12, int HPADelayTimeout_T13,
      int MPADelayTimeout_T14, int HPATransmitTimeout_T15, int MPATransmitTimeout_T16, int HPAQueryTimeout_T17, int HPATransmitAttempts_N5,
      int MPATransmitAttempts_N6, int LPATransmitAttempts_N7, EMode_Y HPAMode, EMode_Y MPAMode, EMode_Y LPAMode)
    {
      MODULE_APP_CONFIG data = new MODULE_APP_CONFIG();
      data.HPAIdleTimeout_T11 = HPAIdleTimeout_T11;
      data.MPAIdleTimeout_T12 = MPAIdleTimeout_T12;
      data.HPADelayTimeout_T13 = HPADelayTimeout_T13;
      data.MPADelayTimeout_T14 = MPADelayTimeout_T14;
      data.HPATransmitTimeout_T15 = HPATransmitTimeout_T15;
      data.MPATransmitTimeout_T16 = MPATransmitTimeout_T16;
      data.HPAQueryTimeout_T17 = HPAQueryTimeout_T17;
      data.HPATransmitAttempts_N5 = HPATransmitAttempts_N5;
      data.MPATransmitAttempts_N6 = MPATransmitAttempts_N6;
      data.LPATransmitAttempts_N7 = LPATransmitAttempts_N7;
      data.HPAMode = HPAMode;
      data.MPAMode = MPAMode;
      data.LPAMode = LPAMode;
      return data;
    }

    static public TT_CTKY CreateCTK(MODULE_APP_CONFIG data) { TT_CTKY ret = new TT_CTKY(); ret.Data = data; return ret; }
    static public TT_STKY CreateSTK(MODULE_APP_CONFIG data, string PW) { TT_STKY ret = new TT_STKY(); ret.PW = PW; ret.Data = data; return ret; }

    static public string ToString(TT_CTKY that)
    {
      return string.Format("AT+CTKY={0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
       that.Data.HPAIdleTimeout_T11, that.Data.MPAIdleTimeout_T12, that.Data.HPADelayTimeout_T13,
       that.Data.MPADelayTimeout_T14, that.Data.HPATransmitTimeout_T15, that.Data.MPATransmitTimeout_T16,
       that.Data.HPAQueryTimeout_T17, that.Data.HPATransmitAttempts_N5, that.Data.MPATransmitAttempts_N6,
       that.Data.LPATransmitAttempts_N7, (int)that.Data.HPAMode, (int)that.Data.MPAMode, (int)that.Data.LPAMode);
    }
    static public string ToString(TT_STKY that)
    {
      return AddFrame(string.Format("STKY{0:D6}{1:D6}{2:D6}{3:D6}{4:D6}{5:D6}{6:D6}{7:D3}{8:D3}{9:D3}{10}{11}{12}{13}{14}",
        that.Data.HPAIdleTimeout_T11, that.Data.MPAIdleTimeout_T12, that.Data.HPADelayTimeout_T13,
        that.Data.MPADelayTimeout_T14, that.Data.HPATransmitTimeout_T15, that.Data.MPATransmitTimeout_T16,
        that.Data.HPAQueryTimeout_T17, that.Data.HPATransmitAttempts_N5, that.Data.MPATransmitAttempts_N6,
        that.Data.LPATransmitAttempts_N7, (int)that.Data.HPAMode, (int)that.Data.MPAMode, (int)that.Data.LPAMode,
        PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString(TT_RTKY that)
    {
      return AddFrame(string.Format("RTKY{0:D6}{1:D6}{2:D6}{3:D6}{4:D6}{5:D6}{6:D6}{7:D3}{8:D3}{9:D3}{10}{11}{12};ID={13}",
        that.Data.HPAIdleTimeout_T11, that.Data.MPAIdleTimeout_T12, that.Data.HPADelayTimeout_T13,
        that.Data.MPADelayTimeout_T14, that.Data.HPATransmitTimeout_T15, that.Data.MPATransmitTimeout_T16,
        that.Data.HPAQueryTimeout_T17, that.Data.HPATransmitAttempts_N5, that.Data.MPATransmitAttempts_N6,
        that.Data.LPATransmitAttempts_N7, (int)that.Data.HPAMode, (int)that.Data.MPAMode, (int)that.Data.LPAMode, that.ID), false);
    }
    #endregion

    #region -- EXT2_APP_CONFIG --
    static public TT_CTKZ Parse_CTKZ(string dat)
    {
      // AT+CTKZ=<Motion Sensor Threshold>, <Hours of Operation Enable>, <Hours of Operation Start Time>, 
      //         <Hours of Operation Duration>, <Hours of Operation Start of Work Week>, <Hours of Operation Work Days per Week>, 
      //         <Motion Runtime Meter Enable>, <LPA Runtime Meter Enable>, <Motion Meter Report Threshold>, 
      //         <LPA Meter Report Threshold>, <Auto Log Dump Enable>, <Fix Rate>, <LPA Input Delay>, <Geofence Type>, 
      //         <Speed Enforcement>, <Speeding Report Mode>, <Speeding Countdown Timer>, <Report Options>, 
      //         <Ignition Sense Override>, <Engine Idle Threshold>, <Engine Idle Report Enabled>, <Speed Threshold Time>, 
      //         <Moving Speed>, <Early Stop Timer>
      TT_CTKZ ret = new TT_CTKZ();

      string[] cols = dat.Split(',', ':', '=');

      ret.Data = new EXT2_APP_CONFIG();
      ret.Data.MotionCounterThreshold = Convert.ToInt32(cols[1].Trim());
      ret.Data.ScheduledHoursMode = (EMode_Z)Convert.ToInt32(cols[2].Trim());
      ret.Data.ScheduledHoursDailyStartTime_T27 = Convert.ToInt32(cols[3].Trim());
      ret.Data.ScheduledHoursWorkDayLength_T28 = Convert.ToInt32(cols[4].Trim());
      ret.Data.ScheduledHoursFirstWeeklyWorkDay = (DayOfWeek)Convert.ToInt32(cols[5].Trim());
      ret.Data.ScheduledHoursWorkDaysPerWeek = Convert.ToInt32(cols[6].Trim());
      ret.Data.RuntimeMotionBased = (EMode_Z)Convert.ToInt32(cols[7].Trim());
      ret.Data.RuntimeLPABased = (EMode_Z)Convert.ToInt32(cols[8].Trim());
      ret.Data.RuntimeMotionBasedCountdown_T29 = Convert.ToInt32(cols[9]);
      ret.Data.RuntimeLPABasedCountdown_T30 = Convert.ToInt32(cols[10]);
      ret.Data.AutomaticMessageLogDump = (EAutomaticMessageLogDump)Convert.ToInt32(cols[11].Trim());
      ret.Data.GPSFixRate = (EGPSFixRate)Convert.ToInt32(cols[12].Trim());
      ret.Data.LPASpeedingReportInputArmingDelay_T31 = Convert.ToInt32(cols[13].Trim());
      ret.Data.GeofenceType = (EGeofenceType)Convert.ToInt32(cols[14].Trim());
      ret.Data.SpeedEnforcement = Convert.ToInt32(cols[15].Trim());
      ret.Data.SpeedingReportMode = (ESpeedingReportMode)Convert.ToInt32(cols[16].Trim());
      ret.Data.SpeedingCountdownTimer = Convert.ToInt32(cols[17]);
#if !TT_PARSER_APP || USE_2_1
      ret.Data.ReportingOptions = Int32.Parse(cols[18], System.Globalization.NumberStyles.HexNumber);
      ret.Data.IgnitionSenseOverride = Convert.ToInt32(cols[19]);
      ret.Data.EngineIdleThreshold = Convert.ToInt32(cols[20]);
      ret.Data.EngineIdleReportEnabled = (EEngineIdleReportEnabled)Convert.ToInt32(cols[21]);
      ret.Data.SpeedThresholdTime = Convert.ToInt32(cols[22]);
      ret.Data.MovingSpeed = Convert.ToInt32(cols[23]);
      ret.Data.EarlyStopTimer = Convert.ToInt32(cols[24]);
      ret.Data.Reserved6 = cols.Length >= 25 ? Convert.ToInt32(cols[24]) : 0;
#else
      ret.Data.Reserved0 = Convert.ToInt32(cols[18]);
      ret.Data.Reserved1 = Convert.ToInt32(cols[19]);
      ret.Data.Reserved2 = Convert.ToInt32(cols[20]);
      ret.Data.Reserved3 = Convert.ToInt32(cols[21]);
      ret.Data.Reserved4 = Convert.ToInt32(cols[22]);
      ret.Data.Reserved5 = Convert.ToInt32(cols[23]);
      ret.Data.Reserved6 = Convert.ToInt32(cols[24]);
#endif

      ret.OriginalParseData = dat;
      return ret;
    }

    static public TT_RTKZ Parse_RTKZ(string dat)
    {
      //           1         2         3         4         5         6
      // 012345678901234567890123456789012345678901234567890123456789012345678
      // >RTKABBBBCDDDDDEEEEEFGHIJJJKKKLMNNNOQQQRSSSSSTTTTUVVVVWXXXXYYZZZZaaaa;ID=YYYYYYYY;*ZZ<
      TT_RTKZ ret = new TT_RTKZ();

      string d = SetID(dat, ret);
      ret.Data = new EXT2_APP_CONFIG();
      ret.Data.MotionCounterThreshold = Convert.ToInt32(d.Substring(5, 4));
      ret.Data.ScheduledHoursMode = (EMode_Z)Convert.ToInt32(d.Substring(9, 1));
      ret.Data.ScheduledHoursDailyStartTime_T27 = Convert.ToInt32(d.Substring(10, 5));
      ret.Data.ScheduledHoursWorkDayLength_T28 = Convert.ToInt32(d.Substring(15, 5));
      ret.Data.ScheduledHoursFirstWeeklyWorkDay = (DayOfWeek)Convert.ToInt32(d.Substring(20, 1));
      ret.Data.ScheduledHoursWorkDaysPerWeek = Convert.ToInt32(d.Substring(21, 1));
      ret.Data.RuntimeMotionBased = (EMode_Z)Convert.ToInt32(d.Substring(22, 1));
      ret.Data.RuntimeLPABased = (EMode_Z)Convert.ToInt32(d.Substring(23, 1));
      ret.Data.RuntimeMotionBasedCountdown_T29 = Convert.ToInt32(d.Substring(24, 3));
      ret.Data.RuntimeLPABasedCountdown_T30 = Convert.ToInt32(d.Substring(27, 3));

      int AutomaticMessageLogDump;
      if (int.TryParse(d.Substring(30, 1), out AutomaticMessageLogDump))
        ret.Data.AutomaticMessageLogDump = (EAutomaticMessageLogDump)AutomaticMessageLogDump;
      else
      {
        if (d.Substring(30, "Enabled".Length) == "Enabled")
        {
          ret.Data.AutomaticMessageLogDump = EAutomaticMessageLogDump.Enabled;
          d = d.Substring(0, 31) + d.Substring(30 + "Enabled".Length);
        }
        else if (d.Substring(30, "Disabled".Length) == "Disabled")
        {
          ret.Data.AutomaticMessageLogDump = EAutomaticMessageLogDump.Disabled;
          d = d.Substring(0, 31) + d.Substring(30 + "Disabled".Length);
        }
        else
          throw new FormatException(string.Format("Unknown value: {0}", d.Substring(30, 4)));
      }

      ret.Data.GPSFixRate = (EGPSFixRate)Convert.ToInt32(d.Substring(31, 1));
      ret.Data.LPASpeedingReportInputArmingDelay_T31 = Convert.ToInt32(d.Substring(32, 3));
      ret.Data.GeofenceType = (EGeofenceType)Convert.ToInt32(d.Substring(35, 1));
      ret.Data.SpeedEnforcement = Convert.ToInt32(d.Substring(36, 3));
      ret.Data.SpeedingReportMode = (ESpeedingReportMode)Convert.ToInt32(d.Substring(39, 1));
      ret.Data.SpeedingCountdownTimer = Convert.ToInt32(d.Substring(40, 5));
#if !TT_PARSER_APP || USE_2_1
      if (d.Length > 50)
      {
        ret.Data.ReportingOptions = Int32.Parse(d.Substring(45, 4), System.Globalization.NumberStyles.HexNumber); // TTTT
        ret.Data.IgnitionSenseOverride = Convert.ToInt32(d.Substring(49, 1)); // U
        ret.Data.EngineIdleThreshold = Convert.ToInt32(d.Substring(50, 4)); // VVVV
        ret.Data.EngineIdleReportEnabled = (EEngineIdleReportEnabled)Convert.ToInt32(d.Substring(54, 1)); // W
        ret.Data.SpeedThresholdTime = Convert.ToInt32(d.Substring(55, 4)); // XXXX
        ret.Data.MovingSpeed = Convert.ToInt32(d.Substring(59, 2)); // YY
        ret.Data.EarlyStopTimer = Convert.ToInt32(d.Substring(61, 4)); // ZZZZ
        ret.Data.Reserved6 = Convert.ToInt32(d.Substring(65, 3)); // aaaa;
      }
      else
      {
        ret.Data.EngineIdleReportEnabled = EEngineIdleReportEnabled.NoEngineIdleReportSent;
        ret.Data.ReportingOptions = ret.Data.IgnitionSenseOverride = ret.Data.EngineIdleThreshold = ret.Data.SpeedThresholdTime
          = ret.Data.MovingSpeed = ret.Data.EarlyStopTimer = ret.Data.Reserved6 = 0;
      }
#else
      ret.Data.Reserved0 = ret.Data.Reserved1 = ret.Data.Reserved2 = ret.Data.Reserved3 = ret.Data.Reserved4 = ret.Data.Reserved5 = ret.Data.Reserved6 = 0;
#endif
      ret.OriginalParseData = dat;
      return ret;
    }

    static public TT_STKZ Parse_STKZ(string dat)
    {
      //           1         2         3         4         5         6
      // 012345678901234567890123456789012345678901234567890123456789012345678
      // >RTKABBBBCDDDDDEEEEEFGHIJJJKKKLMNNNOQQQRSSSSSTTTTUVVVVWXXXXYYZZZZaaaa;ID=YYYYYYYY;*ZZ<
      TT_STKZ ret = new TT_STKZ();

      string d = SetID(dat, ret);
      ret.Data = new EXT2_APP_CONFIG();
      ret.Data.MotionCounterThreshold = Convert.ToInt32(d.Substring(5, 4));
      ret.Data.ScheduledHoursMode = (EMode_Z)Convert.ToInt32(d.Substring(9, 1));
      ret.Data.ScheduledHoursDailyStartTime_T27 = Convert.ToInt32(d.Substring(10, 5));
      ret.Data.ScheduledHoursWorkDayLength_T28 = Convert.ToInt32(d.Substring(15, 5));
      ret.Data.ScheduledHoursFirstWeeklyWorkDay = (DayOfWeek)Convert.ToInt32(d.Substring(20, 1));
      ret.Data.ScheduledHoursWorkDaysPerWeek = Convert.ToInt32(d.Substring(21, 1));
      ret.Data.RuntimeMotionBased = (EMode_Z)Convert.ToInt32(d.Substring(22, 1));
      ret.Data.RuntimeLPABased = (EMode_Z)Convert.ToInt32(d.Substring(23, 1));
      ret.Data.RuntimeMotionBasedCountdown_T29 = Convert.ToInt32(d.Substring(24, 3));
      ret.Data.RuntimeLPABasedCountdown_T30 = Convert.ToInt32(d.Substring(27, 3));

      int AutomaticMessageLogDump;
      if (int.TryParse(d.Substring(30, 1), out AutomaticMessageLogDump))
        ret.Data.AutomaticMessageLogDump = (EAutomaticMessageLogDump)AutomaticMessageLogDump;
      else
      {
        if (d.Substring(30, "Enabled".Length) == "Enabled")
        {
          ret.Data.AutomaticMessageLogDump = EAutomaticMessageLogDump.Enabled;
          d = d.Substring(0, 31) + d.Substring(30 + "Enabled".Length);
        }
        else if (d.Substring(30, "Disabled".Length) == "Disabled")
        {
          ret.Data.AutomaticMessageLogDump = EAutomaticMessageLogDump.Disabled;
          d = d.Substring(0, 31) + d.Substring(30 + "Disabled".Length);
        }
        else
          throw new FormatException(string.Format("Unknown value: {0}", d.Substring(30, 4)));
      }

      ret.Data.GPSFixRate = (EGPSFixRate)Convert.ToInt32(d.Substring(31, 1));
      ret.Data.LPASpeedingReportInputArmingDelay_T31 = Convert.ToInt32(d.Substring(32, 3));
      ret.Data.GeofenceType = (EGeofenceType)Convert.ToInt32(d.Substring(35, 1));
      ret.Data.SpeedEnforcement = Convert.ToInt32(d.Substring(36, 3));
      ret.Data.SpeedingReportMode = (ESpeedingReportMode)Convert.ToInt32(d.Substring(39, 1));
      ret.Data.SpeedingCountdownTimer = Convert.ToInt32(d.Substring(40, 5));
#if !TT_PARSER_APP || USE_2_1
      if (d.Length > 50)
      {
        ret.Data.ReportingOptions = Int32.Parse(d.Substring(45, 4), System.Globalization.NumberStyles.HexNumber); // TTTT
        ret.Data.IgnitionSenseOverride = Convert.ToInt32(d.Substring(49, 1)); // U
        ret.Data.EngineIdleThreshold = Convert.ToInt32(d.Substring(50, 4)); // VVVV
        ret.Data.EngineIdleReportEnabled = (EEngineIdleReportEnabled)Convert.ToInt32(d.Substring(54, 1)); // W
        ret.Data.SpeedThresholdTime = Convert.ToInt32(d.Substring(55, 4)); // XXXX
        ret.Data.MovingSpeed = Convert.ToInt32(d.Substring(59, 2)); // YY
        ret.Data.EarlyStopTimer = Convert.ToInt32(d.Substring(61, 4)); // ZZZZ
        ret.Data.Reserved6 = Convert.ToInt32(d.Substring(65, 3)); // aaaa;
      }
      else
      {
        ret.Data.EngineIdleReportEnabled = EEngineIdleReportEnabled.NoEngineIdleReportSent;
        ret.Data.ReportingOptions = ret.Data.IgnitionSenseOverride = ret.Data.EngineIdleThreshold = ret.Data.SpeedThresholdTime
          = ret.Data.MovingSpeed = ret.Data.EarlyStopTimer = ret.Data.Reserved6 = 0;
      }
#else
      ret.Data.Reserved0 = ret.Data.Reserved1 = ret.Data.Reserved2 = ret.Data.Reserved3 = ret.Data.Reserved4 = ret.Data.Reserved5 = ret.Data.Reserved6 = 0;
#endif
      ret.OriginalParseData = dat;
      return ret;
    }

    static public EXT2_APP_CONFIG Create_EXT2_APP_CONFIG(int MotionCounterThreshold, EMode_Z ScheduledHoursMode,
      int ScheduledHoursDailyStartTime_T27, int ScheduledHoursWorkDayLength_T28, DayOfWeek ScheduledHoursFirstWeeklyWorkDay,
      int ScheduledHoursWorkDaysPerWeek, EMode_Z RuntimeMotionBased, EMode_Z RuntimeLPABased, int RuntimeMotionBasedCountdown_T29,
      int RuntimeLPABasedCountdown_T30, EAutomaticMessageLogDump AutomaticMessageLogDump, EGPSFixRate GPSFixRate, int LPASpeedingReportInputArmingDelay_T31,
      EGeofenceType GeofenceType, int SpeedEnforcement, ESpeedingReportMode SpeedingReportMode, int SpeedingCountdownTimer
#if !TT_PARSER_APP || USE_2_1
, int ReportingOptions, int IgnitionSenseOverride, int EngineIdleThreshold, EEngineIdleReportEnabled EngineIdleReportEnable,
      int SpeedThresholdTime, int MovingSpeed
#endif
, int EarlyStopTimer
    )
    {
      EXT2_APP_CONFIG data = new EXT2_APP_CONFIG();
      data.MotionCounterThreshold = MotionCounterThreshold;
      data.ScheduledHoursMode = ScheduledHoursMode;
      data.ScheduledHoursDailyStartTime_T27 = ScheduledHoursDailyStartTime_T27;
      data.ScheduledHoursWorkDayLength_T28 = ScheduledHoursWorkDayLength_T28;
      data.ScheduledHoursFirstWeeklyWorkDay = ScheduledHoursFirstWeeklyWorkDay;
      data.ScheduledHoursWorkDaysPerWeek = ScheduledHoursWorkDaysPerWeek;
      data.RuntimeMotionBased = RuntimeMotionBased;
      data.RuntimeLPABased = RuntimeLPABased;
      data.RuntimeMotionBasedCountdown_T29 = RuntimeMotionBasedCountdown_T29;
      data.RuntimeLPABasedCountdown_T30 = RuntimeLPABasedCountdown_T30;
      data.AutomaticMessageLogDump = AutomaticMessageLogDump;
      data.GPSFixRate = GPSFixRate;
      data.LPASpeedingReportInputArmingDelay_T31 = LPASpeedingReportInputArmingDelay_T31;
      data.GeofenceType = GeofenceType;
      data.SpeedEnforcement = SpeedEnforcement;
      data.SpeedingReportMode = SpeedingReportMode;
      data.SpeedingCountdownTimer = SpeedingCountdownTimer;
#if !TT_PARSER_APP || USE_2_1
      data.ReportingOptions = ReportingOptions;
      data.IgnitionSenseOverride = IgnitionSenseOverride;
      data.EngineIdleThreshold = EngineIdleThreshold;
      data.EngineIdleReportEnabled = EngineIdleReportEnable;
      data.SpeedThresholdTime = SpeedThresholdTime;
      data.MovingSpeed = MovingSpeed;
      data.EarlyStopTimer = EarlyStopTimer;
      data.Reserved6 = 0;
#else
      data.Reserved0 = data.Reserved1 = data.Reserved2 = data.Reserved3 = data.Reserved4 = data.Reserved5 = data.Reserved5 = 0;
      data.Reserved6 = EarlyStopTimer;
#endif
      return data;
    }

    static public TT_CTKZ CreateCTK(EXT2_APP_CONFIG data) { TT_CTKZ ret = new TT_CTKZ(); ret.Data = data; return ret; }
    static public TT_STKZ CreateSTK(EXT2_APP_CONFIG data, string PW) { TT_STKZ ret = new TT_STKZ(); ret.PW = PW; ret.Data = data; return ret; }

    static public string ToString(TT_CTKZ that)
    {
      // AT+CTKZ=(0)<Motion Sensor Threshold>, (1)<Hours of Operation Enable>, (2)<Hours of Operation Start Time>, 
      //         (3)<Hours of Operation Duration>, (4)<Hours of Operation Start of Work Week>, (5)<Hours of Operation Work Days per Week>, 
      //         (6)<Motion Runtime Meter Enable>, (7)<LPA Runtime Meter Enable>, (8)<Motion Meter Report Threshold>, 
      //         (9)<LPA Meter Report Threshold>, (10)<Auto Log Dump Enable>, (11)<Fix Rate>, (12)<LPA Input Delay>, (13)<Geofence Type>, 
      //         (14)<Speed Enforcement>, (15)<Speeding Report Mode>, (16)<Speeding Countdown Timer>, (17)<Report Options>, 
      //         (18)<Ignition Sense Override>, (19)<Engine Idle Threshold>, (20)<Engine Idle Report Enabled>, (21)<Speed Threshold Time>, 
      //         (22)<Moving Speed>, (23)<Early Stop Timer>, (24)TBD
      return string.Format("AT+CTKZ={0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17:X4},{18},{19},{20},{21},{22},{23}",
        that.Data.MotionCounterThreshold, (int)that.Data.ScheduledHoursMode, that.Data.ScheduledHoursDailyStartTime_T27,
        that.Data.ScheduledHoursWorkDayLength_T28, (int)that.Data.ScheduledHoursFirstWeeklyWorkDay,
        that.Data.ScheduledHoursWorkDaysPerWeek, (int)that.Data.RuntimeMotionBased, (int)that.Data.RuntimeLPABased,
        that.Data.RuntimeMotionBasedCountdown_T29, that.Data.RuntimeLPABasedCountdown_T30,
        (int)that.Data.AutomaticMessageLogDump, (int)that.Data.GPSFixRate, that.Data.LPASpeedingReportInputArmingDelay_T31,
        (int)that.Data.GeofenceType, that.Data.SpeedEnforcement, (int)that.Data.SpeedingReportMode, that.Data.SpeedingCountdownTimer,
#if !TT_PARSER_APP || USE_2_1
 that.Data.ReportingOptions, that.Data.IgnitionSenseOverride, that.Data.EngineIdleThreshold, (int)that.Data.EngineIdleReportEnabled,
        that.Data.SpeedThresholdTime, that.Data.MovingSpeed, that.Data.EarlyStopTimer, that.Data.Reserved6
#else
        that.Data.Reserved0, that.Data.Reserved1, that.Data.Reserved2, that.Data.Reserved3,
        that.Data.Reserved4, that.Data.Reserved5, that.Data.Reserved6, 0
#endif
);
    }
    static public string ToString(TT_STKZ that)
    {
      //                               1                       2
      //      0   12    3    45678  9  012  34  56    7   89   01   2 3   4
      // >STKZBBBBCDDDDDEEEEEFGHIJJJKKKLMNNNOQQQRSSSSSTTTTUVVVVWXXXXYYZZZZaaaa;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ<
      return AddFrame(string.Format("STKZ{0:D4}{1:D1}{2:D5}" +
                                    "{3:D5}{4:D1}{5:D1}" +
                                    "{6:D1}{7:D1}{8:D3}" +
                                    "{9:D3}{10:D1}{11:D1}" +
                                    "{12:D3}{13:D1}{14:D3}" +
                                    "{15:D1}{16:D5}" +
                                    "{17:X4}{18:D1}{19:D4}{20:D1}" +
                                    "{21:D4}{22:D2}{23:D4}{24:D4}{25}{26}",
        /* 0 */ that.Data.MotionCounterThreshold, (int)that.Data.ScheduledHoursMode, that.Data.ScheduledHoursDailyStartTime_T27,
        /* 3 */ that.Data.ScheduledHoursWorkDayLength_T28, (int)that.Data.ScheduledHoursFirstWeeklyWorkDay, that.Data.ScheduledHoursWorkDaysPerWeek,
        /* 6 */ (int)that.Data.RuntimeMotionBased, (int)that.Data.RuntimeLPABased, that.Data.RuntimeMotionBasedCountdown_T29,
        /* 9 */ that.Data.RuntimeLPABasedCountdown_T30, (int)that.Data.AutomaticMessageLogDump, (int)that.Data.GPSFixRate,
        /* 12 */that.Data.LPASpeedingReportInputArmingDelay_T31, (int)that.Data.GeofenceType, that.Data.SpeedEnforcement,
        /* 15 */(int)that.Data.SpeedingReportMode, that.Data.SpeedingCountdownTimer,
#if !TT_PARSER_APP || USE_2_1
        /* 17 */that.Data.ReportingOptions, that.Data.IgnitionSenseOverride, that.Data.EngineIdleThreshold, (int)that.Data.EngineIdleReportEnabled,
        /* 21 */that.Data.SpeedThresholdTime, that.Data.MovingSpeed, that.Data.EarlyStopTimer, that.Data.Reserved6,
#else
        that.Data.Reserved0, that.Data.Reserved1, that.Data.Reserved2, that.Data.Reserved3,
        that.Data.Reserved4, that.Data.Reserved5, that.Data.Reserved6, 0,
#endif
 PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString(TT_RTKZ that)
    {
      //                               1                       2
      //      0   12    3    45678  9  012  34  56    7   89   01   2 3   4
      // >RTKZBBBBCDDDDDEEEEEFGHIJJJKKKLMNNNOQQQRSSSSSTTTTUVVVVWXXXXYYZZZZaaaa;ID=YYYYYYYY;*ZZ<
      return AddFrame(string.Format("RTKZ{0:D4}{1:D1}{2:D5}" +
                                    "{3:D5}{4:D1}{5:D1}" +
                                    "{6:D1}{7:D1}{8:D3}" +
                                    "{9:D3}{10:D1}{11:D1}" +
                                    "{12:D3}{13:D1}{14:D3}" +
                                    "{15:D1}{16:D5}" +
                                    "{17:X4}{18:D1}{19:D4}{20:D1}" +
                                    "{21:D4}{22:D2}{23:D4}{24:D4};ID={25}",
        /* 0 */ that.Data.MotionCounterThreshold, (int)that.Data.ScheduledHoursMode, that.Data.ScheduledHoursDailyStartTime_T27,
        /* 3 */ that.Data.ScheduledHoursWorkDayLength_T28, (int)that.Data.ScheduledHoursFirstWeeklyWorkDay, that.Data.ScheduledHoursWorkDaysPerWeek,
        /* 6 */ (int)that.Data.RuntimeMotionBased, (int)that.Data.RuntimeLPABased, that.Data.RuntimeMotionBasedCountdown_T29,
        /* 9 */ that.Data.RuntimeLPABasedCountdown_T30, (int)that.Data.AutomaticMessageLogDump, (int)that.Data.GPSFixRate,
        /* 12 */that.Data.LPASpeedingReportInputArmingDelay_T31, (int)that.Data.GeofenceType, that.Data.SpeedEnforcement,
        /* 15 */(int)that.Data.SpeedingReportMode, that.Data.SpeedingCountdownTimer,
#if !TT_PARSER_APP || USE_2_1
        /* 17 */that.Data.ReportingOptions, that.Data.IgnitionSenseOverride, that.Data.EngineIdleThreshold, (int)that.Data.EngineIdleReportEnabled,
        /* 21 */that.Data.SpeedThresholdTime, that.Data.MovingSpeed, that.Data.EarlyStopTimer, that.Data.Reserved6,
#else
        that.Data.Reserved0, that.Data.Reserved1, that.Data.Reserved2, that.Data.Reserved3,
        that.Data.Reserved4, that.Data.Reserved5, that.Data.Reserved6, 0,
#endif
 that.ID), false);
    }

#if !TT_PARSER_APP || USE_2_1
    static public TT_QTKN Parse_QTKN(string dat)
    {
      // >QTKNBC;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ<
      TT_QTKN ret = new TT_QTKN();

      string d = SetIDPW(dat, ret);
      ret.ResetOdometerFlag = (EReportFlag)Convert.ToInt32(d.Substring(5, 1));
      ret.ResetEngineIdleRuntimeMeterFlag = (EReportFlag)Convert.ToInt32(d.Substring(6, 1));
      ret.OriginalParseData = dat;
      return ret;
    }
    static public string ToString(TT_QTKN that)
    {
      return AddFrame(string.Format("QTKN{0:D1}{1:D1}{2}{3}",
        (int)that.ResetOdometerFlag, (int)that.ResetEngineIdleRuntimeMeterFlag,
        PW_CMD(that), ID_CMD(that)), true);
    }

    static public TT_RTKN Parse_RTKN(string dat)
    {
      // >RTKABCDDDDDDDDDDEEEEEEEEEE;ID=YYYYYYYY;*ZZ<
      TT_RTKN ret = new TT_RTKN();

      string d = SetID(dat, ret);
      ret.OdometerMeterFlag = (EMeterFlag)Convert.ToInt32(d.Substring(5, 1));
      ret.EngineIdleRuntimeMeterFlag = (EMeterFlag)Convert.ToInt32(d.Substring(6, 1));
      ret.CurrentOdometer = Convert.ToInt32(d.Substring(7, 10));
      ret.CurrentEngineIdleRuntimeMeter = Convert.ToInt32(d.Substring(17, 10));
      ret.OriginalParseData = dat;
      return ret;
    }

    static public string ToString(TT_RTKN that)
    {
      return AddFrame(string.Format("RTKN{0:D1}{1:D1}{2:D10}{3:D10}{4}",
        (int)that.OdometerMeterFlag, (int)that.EngineIdleRuntimeMeterFlag, that.CurrentOdometer, that.CurrentEngineIdleRuntimeMeter,
        ID_CMD(that)), false);
    }

    static public string ToString(TT_STKI that)
    {
      // >STKIBBBBBBBBCCCCCCCC;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ<
      return AddFrame(string.Format("STKI{0,-8}{1,-8}{2}{3}",
        that.Data.DeviceID, that.Data.SecurityPassword,
        PW_CMD(that), ID_CMD(that)), true);
    }

    static public TT_RTKI Parse_RTKI(string dat)
    {
      // >RTKIBCDDDDDDDDDDEEEEEEEEEE;ID=YYYYYYYY;*ZZ<
      TT_RTKI ret = new TT_RTKI();

      string d = SetID(dat, ret);
      ret.Data = new IDENT();
      ret.Data.DeviceID = d.Substring(5, 8);
      ret.Data.SecurityPassword = d.Substring(13, 8);
      ret.OriginalParseData = dat;
      return ret;
    }

    static public TT_STKI Parse_STKI(string dat)
    {
      // >STKIBCDDDDDDDDDDEEEEEEEEEE;ID=YYYYYYYY;*ZZ<
      TT_STKI ret = new TT_STKI();

      string d = SetID(dat, ret);
      ret.Data = new IDENT();
      ret.Data.DeviceID = d.Substring(5, 8);
      ret.Data.SecurityPassword = d.Substring(13, 8);
      ret.OriginalParseData = dat;
      return ret;
    }

    static public string ToString(TT_RTKI that)
    {
      return AddFrame(string.Format("RTKI{0,-8}{1,-8}{2}",
        that.Data.DeviceID, that.Data.SecurityPassword,
        ID_CMD(that)), true);
    }

    static public string ToString(TT_STKT that)
    {
      // >STKTBFFFFFFFFCCCCCCCCDDDDDDDDDDDDDDD”EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE”;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ<
      return AddFrame(string.Format("STKT{0:D1}{1:D8}{2:X8}{3}\"{4}\"{5}{6}",
        (int)that.OTAFirmwareUpgradeCommand, that.Data.OTAFileSize, that.Data.OTAFileChecksum, that.Data.OTATFTPIPAddress, that.Data.OTATFTPFilename,
        PW_CMD(that), ID_CMD(that)), true);
    }

    static public TT_RTKT Parse_RTKT(string dat)
    {
      // >RTKTBFFFFFFFFCCCCCCCCCCDDDDDDDDDDDDDDD”EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE”;ID=YYYYYYYY;*ZZ<
      TT_RTKT ret = new TT_RTKT();

      string d = SetID(dat, ret);

      ret.OTAFirmwareUpgradeState = Convert.ToInt32(d.Substring(5, 1));
      ret.Data = new OTA_FIRWMARE();
      ret.Data.OTAFileSize = Convert.ToInt32(d.Substring(6, 8));
      ret.Data.OTAFileChecksum = UInt32.Parse(d.Substring(14, 8), System.Globalization.NumberStyles.HexNumber);

      int p = d.IndexOf('"', 22);
      ret.Data.OTATFTPIPAddress = d.Substring(22, p - 22);
      ++p;
      int np = d.IndexOf('"', p);
      ret.Data.OTATFTPFilename = d.Substring(p, np - p);
      ret.OriginalParseData = dat;
      return ret;
    }
    static public string ToString(TT_RTKT that)
    {
      // >RTKTBFFFFFFFFCCCCCCCCCCDDDDDDDDDDDDDDD”EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE”;ID=YYYYYYYY;*ZZ<
      return AddFrame(string.Format("RTKT{0:D1}{1:D8}{2:X8}{3}\"{4}\"{5}",
        that.OTAFirmwareUpgradeState, that.Data.OTAFileSize, that.Data.OTAFileChecksum, that.Data.OTATFTPIPAddress, that.Data.OTATFTPFilename,
        ID_CMD(that)), false);
    }
#endif

    public static string PW_CMD(TT_OTA_CMD that) { return string.IsNullOrEmpty(that.PW) ? "" : string.Format(";PW={0}", that.PW); }
    public static string ID_CMD(TT_ID_CMD that) { return string.IsNullOrEmpty(that.ID) || that.ID == "00000000" ? "" : string.Format(";ID={0}", that.ID); }
    #endregion

    #region -- Parse --

    // >RTKABCDDDDDDDDDDEEEEEEEEEE;ID=YYYYYYYY;*ZZ< 
    // 012345678901234567890123456
    static public TT_RTKM Parse_RTKM(string dat)
    {
      TT_RTKM ret = new TT_RTKM();

      string d = SetID(dat, ret);
      ret.RuntimeMotionBasedResetConfirmation = (EReportFlag)Convert.ToInt32(d.Substring(5, 1));
      ret.RuntimeLPABasedResetConfirmation = (EReportFlag)Convert.ToInt32(d.Substring(6, 1));
      ret.RuntimeMotionBasedReading = Convert.ToInt32(d.Substring(7, 10));
      ret.RuntimeLPABasedReading = Convert.ToInt32(d.Substring(17, 10));

      ret.OriginalParseData = dat;
      return ret;
    }
    static public string ToString(TT_RTKM that)
    {
      return AddFrame(string.Format("RTKM{0}{1}{2:D10}{3:D10};ID={4}",
        (int)that.RuntimeMotionBasedResetConfirmation, (int)that.RuntimeLPABasedResetConfirmation,
        that.RuntimeMotionBasedReading, that.RuntimeLPABasedReading,
        that.ID), false);
    }
    static public string ToString(TT_QTKM that)
    {
      return AddFrame(string.Format("QTKM{0}{1}{2}{3}", (int)that.RuntimeMotionBasedQuery, (int)that.RuntimeLPABasedQuery,
        PW_CMD(that), ID_CMD(that)), true);
    }
    static public TT_QTKM Parse_QTKM(string dat)
    {
      TT_QTKM ret = new TT_QTKM();

      string d = SetIDPW(dat, ret);
      ret.RuntimeMotionBasedQuery = (EReportFlag)Convert.ToInt32(d.Substring(5, 1)); ;
      ret.RuntimeLPABasedQuery = (EReportFlag)Convert.ToInt32(d.Substring(6, 1));
      ret.OriginalParseData = dat;
      return ret;
    }

    static public TT_RTKS Parse_RTKS(string dat)
    {
      TT_RTKS ret = new TT_RTKS();
      string msgData = dat.Split(';')[0];
      SetID(dat, ret);
      ret.Data = new STATUS_MSG();
      SetSTATUS_MSG(ret.Data, msgData);

      ret.OriginalParseData = dat;
      return ret;
    }

    // >RTKABBBBCDDDEFFFFGGGGGGSTVOPQRWXabU[HHHIIIIIIIJJJJKKKKKKKLLLLLLMMMNNN];ID=YYYYYYYY;*ZZ< 
    // 012345678901234567890123456789012345 678901234567890123456789012345678
    static public TT_RTKP Parse_RTKP(string dat)
    {
      TT_RTKP ret = new TT_RTKP();

      //parse
      string msgData = dat.Split(';')[0];

      if (msgData.Substring(4, 1) == "P")
        ret.PositionData = new REPORT_POS();

      SetID(dat, ret);
      ret.Data = new STATUS_MSG();
      SetSTATUS_MSG(ret.Data, msgData);

      //if Position Report
      if (ret.PositionData != null)
      {
        ret.PositionData.Latitude = Convert.ToDouble(msgData.Substring(36, 3) + "." + msgData.Substring(39, 7));
        ret.PositionData.Longitude = Convert.ToDouble(msgData.Substring(46, 4) + "." + msgData.Substring(50, 7));
        ret.PositionData.Altitude = Convert.ToInt32(msgData.Substring(57, 6));
        ret.PositionData.Speed = Convert.ToInt32(msgData.Substring(63, 3));
        ret.PositionData.Heading = Convert.ToInt32(msgData.Substring(66, 3));
      }

      ret.OriginalParseData = dat;
      return ret;
    }

    static void SetSTATUS_MSG(STATUS_MSG msg, string msgData)
    {
      msg.ProtocolSequenceNumber = Int32.Parse(msgData.Substring(5, 4), System.Globalization.NumberStyles.HexNumber);
      msg.TriggerType = (ETriggerType)Convert.ToInt32(msgData.Substring(9, 1));
      msg.BatteryLevel = Convert.ToInt32(msgData.Substring(10, 3));
      msg.BatteryChangedFlag = msgData.Substring(13, 1) == "T";
      msg.GPSTimeWeek = Convert.ToInt32(msgData.Substring(14, 4));
      msg.GPSTimeSeconds = Convert.ToInt32(msgData.Substring(18, 6));
      msg.GPSStatusCode = (EGPSStatusCode)Convert.ToInt32(msgData.Substring(24, 1));
      msg.GSMStatusCode = (EGSMStatusCode)Convert.ToInt32(msgData.Substring(25, 1));
      msg.PositionAge = (EPositionAge)Convert.ToInt32(msgData.Substring(26, 1));
      msg.HPAStatus = (ETTAlertState)Convert.ToInt32(msgData.Substring(27, 1));
      msg.MPAStatus = (ETTAlertState)Convert.ToInt32(msgData.Substring(28, 1));
      msg.LPAStatus = (ETTAlertState)Convert.ToInt32(msgData.Substring(29, 1));
      msg.ExternalPower = (EExternalPower)Convert.ToInt32(msgData.Substring(30, 1));
      msg.GeofenceStatus = (EGeofenceStatus)Convert.ToInt32(msgData.Substring(31, 1));
      msg.ExtendedGPSStatusCode = (EExtendedGPSStatusCode)Convert.ToInt32(msgData.Substring(32, 1));
      msg.SpeedingStatus = (ESpeedingStatus)Convert.ToInt32(msgData.Substring(33, 1));
      msg.ScheduledHoursFlag = (ESpeedingStatus)Convert.ToInt32(msgData.Substring(34, 1));
    }

    #endregion

    #region -- ToString --
    static public string ToString(TT_QTKD that)
    {
      return AddFrame(string.Format("QTKD{0}{1:D4}{2}{3}",
        PositionQueryModeStr(that), that.PositionQueryFixTimeout, PW_CMD(that), ID_CMD(that)), true);
    }
    static public TT_QTKD Parse_QTKD(string dat)
    {
      TT_QTKD ret = new TT_QTKD();
      string d = SetIDPW(dat, ret);
      ret.PositionQueryMode = PositionQueryModeStr(d.Substring(5, 1)[0]);
      ret.PositionQueryFixTimeout = Convert.ToInt32(d.Substring(6, 4));
      ret.OriginalParseData = dat;
      return ret;
    }

    static public TT_QTKD.EPositionQueryMode PositionQueryModeStr(char mode)
    {
      switch (mode)
      {
        case 'S': return TT_QTKD.EPositionQueryMode.ComputeIfPositionAged;
        case 'P': return TT_QTKD.EPositionQueryMode.ComputeNewPositionFix;
        case 'L': return TT_QTKD.EPositionQueryMode.StatusReportWithLastLoggedPosition;
      }
      throw new NotSupportedException(string.Format("Mode={0}", mode));
    }

    static public char PositionQueryModeStr(TT_QTKD that)
    {
      switch (that.PositionQueryMode)
      {
        case TT_QTKD.EPositionQueryMode.ComputeIfPositionAged: return 'S';
        case TT_QTKD.EPositionQueryMode.ComputeNewPositionFix: return 'P';
        case TT_QTKD.EPositionQueryMode.StatusReportWithLastLoggedPosition: return 'L';
      }
      throw new NotSupportedException(string.Format("PositionQueryMode={0}", that.PositionQueryMode));
    }
    static public string ToString(TT_RTKS that)
    {
      return AddFrame(string.Format("RTKS{0:X4}{1}{2:D3}{3}{4:D4}{5:D6}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}{17};ID={18}",
        that.Data.ProtocolSequenceNumber, (int)that.Data.TriggerType, that.Data.BatteryLevel, that.Data.BatteryChangedFlag ? 'T' : 'F',
        that.Data.GPSTimeWeek, that.Data.GPSTimeSeconds, (int)that.Data.GPSStatusCode, (int)that.Data.GSMStatusCode,
        (int)that.Data.PositionAge, (int)that.Data.HPAStatus, (int)that.Data.MPAStatus, (int)that.Data.LPAStatus,
        (int)that.Data.ExternalPower, (int)that.Data.GeofenceStatus, (int)that.Data.ExtendedGPSStatusCode,
        (int)that.Data.SpeedingStatus, (int)that.Data.ScheduledHoursFlag, that.Data.Reserved,
        that.ID), false);
    }
    static public string ToString(TT_RTKV that)
    {
      return AddFrame(string.Format("RTKV{0};ID={1}",
        that.Data.SMSDestinationAddress.PadLeft(24, ' '), that.ID), false);
    }

    static public string ToString(TT_RTKU that)
    {
      return AddFrame(string.Format("RTKU{0}{1}{2:D16};ID={3}",
        (int)that.Data.Output1, (int)that.Data.Output2, that.Data.Reserved, that.ID), false);
    }
    static public TT_RTKU Parse_RTKU(string dat)
    {
      TT_RTKU ret = new TT_RTKU();
      string d = SetID(dat, ret);
      ret.Data = new CONTROL_OUTPUT();
      ret.Data.Output1 = (EPinState)Convert.ToInt32(d.Substring(5, 1));
      ret.Data.Output2 = (EPinState)Convert.ToInt32(d.Substring(6, 1));
      ret.OriginalParseData = dat;
      return ret;
    }

    static public TT_STKU Parse_STKU(string dat)
    {
      TT_STKU ret = new TT_STKU();
      string d = SetID(dat, ret);
      ret.Data = new CONTROL_OUTPUT();
      ret.Data.Output1 = (EPinState)Convert.ToInt32(d.Substring(5, 1));
      ret.Data.Output2 = (EPinState)Convert.ToInt32(d.Substring(6, 1));
      ret.OriginalParseData = dat;
      return ret;
    }

    static public string ToString(TT_RTKx that)
    {
      return AddFrame(string.Format("RTK{0}{1};ID={2}", that.ReportIndex, that.Message, that.ID), false);
    }
    static public TT_RTKx Parse_RTKx(string dat, int ndx)
    {
      TT_RTKx ret = new TT_RTKx();
      string d = SetID(dat, ret);
      ret.ReportIndex = ndx;
      ret.Message = d.Substring(5);
      ret.OriginalParseData = dat;
      return ret;
    }

    static public string ToString(TT_RTKR that)
    {
      return AddFrame(string.Format("RTKR{0:D4};ID={1}", that.NumberOfMessangesSent, that.ID), false);
    }
    static public TT_RTKR Parse_RTKR(string dat)
    {
      TT_RTKR ret = new TT_RTKR();
      string d = SetID(dat, ret);
      ret.NumberOfMessangesSent = Convert.ToInt32(d.Substring(5, 4));
      ret.OriginalParseData = dat;
      return ret;
    }

    static public string ToString(TT_CTKE that)
    {
      return string.Format("AT+CTKE={0}", (int)that.EraseRestoreMode);
    }
    static public string ToString(TT_STKU that)
    {
      return AddFrame(string.Format("STKU{0}{1}{2:D16}{3}{4}",
        (int)that.Data.Output1, (int)that.Data.Output2, that.Data.Reserved, PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString(TT_QTK_ that)
    {
      return AddFrame(string.Format("QTK{0}{1}{2}", TrimTracConfigurationSet(that), PW_CMD(that), ID_CMD(that)), true);
    }
    static public TT_QTK_ Parse_QTK_(string msg, TT_QTK_.QUERY_CONFIG config)
    {
      TT_QTK_ ret = new TT_QTK_();
      SetIDPW(msg, ret);
      ret.Config = config;
      ret.OriginalParseData = msg;
      return ret;
    }

    static public char TrimTracConfigurationSet(TT_QTK_ that)
    {
      switch (that.Config)
      {
        case TT_QTK_.QUERY_CONFIG.ApplicationParameters: return 'A';
        case TT_QTK_.QUERY_CONFIG.GPRSConnectionParameters: return 'F';
        case TT_QTK_.QUERY_CONFIG.GPSParameters: return 'G';
        case TT_QTK_.QUERY_CONFIG.GPRSSetupParameters: return 'J';
        case TT_QTK_.QUERY_CONFIG.ProvisioningParameters: return 'V';
        case TT_QTK_.QUERY_CONFIG.ExtendedApplicationPara: return 'X';
        case TT_QTK_.QUERY_CONFIG.ModuleApplicationPara: return 'Y';
        case TT_QTK_.QUERY_CONFIG.Extended2ApplicationPara: return 'Z';
#if !TT_PARSER_APP || USE_2_1
        case TT_QTK_.QUERY_CONFIG.VAMOutput: return 'U';
        case TT_QTK_.QUERY_CONFIG.DeviceIdentification: return 'I';
        case TT_QTK_.QUERY_CONFIG.Firwmare: return 'T';
#endif
      }
      throw new NotSupportedException(string.Format("Config={0}", that.Config));
    }
    static public TT_QTKU Parse_QTKU(string dat)
    {
      TT_QTKU ret = new TT_QTKU();
      SetIDPW(dat, ret);
      ret.OriginalParseData = dat;
      return ret;
    }
    static public string ToString(TT_QTKU that)
    {
      return AddFrame(string.Format("QTKU{0}{1}", PW_CMD(that), ID_CMD(that)), true);
    }
    static public char AggregateLogReportingFlagStr(TT_QTKR that) { return that.AggregateLogReportingFlag == EFlag.Enabled ? 'T' : 'F'; }
    static public EFlag AggregateLogReportingFlag(string that) { return that == "T" ? EFlag.Enabled : that == "F" ? EFlag.Disabled : EFlag.Unknown; }
    static public char StopRESP_QUERY_LOGStr(TT_QTKR that) { return that.StopRESP_QUERY_LOG == EStopRESP_QUERY_LOG.DoNotSend ? 'T' : 'F'; }
    static public EStopRESP_QUERY_LOG StopRESP_QUERY_LOG(string that) { return that == "T" ? EStopRESP_QUERY_LOG.DoNotSend : EStopRESP_QUERY_LOG.Send; }
    static public char Filter1Str(TT_QTKR that) { return that.Filter1 == EFilter.All ? 'Z' : 'U'; }
    /// Filter 1: 'Z' = All, 'U' = Unsent only.  
    static public EFilter Filter1(string that) { return that == "Z" ? EFilter.All : EFilter.UnsentOnly; }
    static public char Filter2Str(TT_QTKR that) { return that.Filter2 == EFilter.All ? 'Z' : that.Filter2 == EFilter.PositionOnly ? 'P' : 'S'; }
    /// Filter 2: 'Z' = All, 'P' = Position only, 'S' = Status only. 
    static public EFilter Filter2(string that) { return that == "Z" ? EFilter.All : that == "P" ? EFilter.PositionOnly : EFilter.StatusOnly; }
    static public char Filter3Str(TT_QTKR that) { return that.Filter3 == EFilter.All ? 'Z' : 'A'; }
    /// Filter 3: 'Z' = All, 'A' = Alert only 
    static public EFilter Filter3(string that) { return that == "Z" ? EFilter.All : EFilter.AlertOnly; }
    static public char TimeRangeStr(TT_QTKR that)
    {
      switch (that.TimeRange)
      {
        case ETimeRange.Unused: return 'Z';
        case ETimeRange.Newest: return 'N';
        case ETimeRange.Oldest: return 'O';
      }
      throw new NotSupportedException(string.Format("TimeRange={0}", that.TimeRange));
    }
    static public ETimeRange TimeRange(string that)
    {
      switch (that)
      {
        case "Z": return ETimeRange.Unused;
        case "N": return ETimeRange.Newest;
        case "O": return ETimeRange.Oldest;
      }
      throw new NotSupportedException(string.Format("TimeRange={0}", that));
    }
    static public string ToString2(TT_QTKR that)
    {
      return AddFrame(string.Format("QTKR{0:X4}{1:X4}{2}{3}",
         that.BeginningProtocolSequenceNumber, that.EndingProtocolSequenceNumber, PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString4(TT_QTKR that)
    {
      return AddFrame(string.Format("QTKR{0:X4}{1:X4}{2}{3}{4}{5}",
         that.BeginningProtocolSequenceNumber, that.EndingProtocolSequenceNumber,
         AggregateLogReportingFlagStr(that), StopRESP_QUERY_LOGStr(that), PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString10(TT_QTKR that)
    {
      return AddFrame(string.Format("QTKR{0:X4}{1:X4}{2}{3}{4}{5}{6}{7}{8:D3}{9}{10}{11}",
         that.BeginningProtocolSequenceNumber, that.EndingProtocolSequenceNumber,
         AggregateLogReportingFlagStr(that), StopRESP_QUERY_LOGStr(that),
         Filter1Str(that), Filter2Str(that), Filter3Str(that), TimeRangeStr(that), that.MaximumNumberOfMessages, (int)that.LastMessage,
         PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString12(TT_QTKR that)
    {
      return AddFrame(string.Format("QTKR{0:X4}{1:X4}{2}{3}{4}{5}{6}{7}{8:D3}{9}{10:D4}{11:D6}{12}{13}",
         that.BeginningProtocolSequenceNumber, that.EndingProtocolSequenceNumber,
         AggregateLogReportingFlagStr(that), StopRESP_QUERY_LOGStr(that),
         Filter1Str(that), Filter2Str(that), Filter3Str(that), TimeRangeStr(that), that.MaximumNumberOfMessages, (int)that.LastMessage,
         that.GPSStartingDateWeek, that.GPSStartingDateSeconds,
         PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString14(TT_QTKR that)
    {
      return AddFrame(string.Format("QTKR{0:X4}{1:X4}{2}{3}{4}{5}{6}{7}{8:D3}{9}{10:D4}{11:D6}{12:D4}{13:D6}{14}{15}",
         that.BeginningProtocolSequenceNumber, that.EndingProtocolSequenceNumber,
         AggregateLogReportingFlagStr(that), StopRESP_QUERY_LOGStr(that),
         Filter1Str(that), Filter2Str(that), Filter3Str(that), TimeRangeStr(that), that.MaximumNumberOfMessages, (int)that.LastMessage,
         that.GPSStartingDateWeek, that.GPSStartingDateSeconds,
         that.GPSEndingDateWeek, that.GPSEndingDateSeconds,
         PW_CMD(that), ID_CMD(that)), true);
    }
    static public string ToString(TT_QTKR that)
    {
#if !TT_PARSER_APP || USE_2_1
      if (that.AggregateLogReportingFlag == null)
        return ToString2(that);
      if (that.Filter1 == null)
        return ToString4(that);
      if (that.GPSStartingDateWeek == null)
        return ToString10(that);
      if (that.GPSEndingDateWeek == null)
        return ToString12(that);
#endif
      return ToString14(that);
    }
    static public TT_QTKR Parse_QTKR(string dat)
    {
      TT_QTKR ret = new TT_QTKR();
      string msgData = SetIDPW(dat, ret);
      msgData = msgData.Substring(0, msgData.IndexOf(';'));

      ret.BeginningProtocolSequenceNumber = Int32.Parse(msgData.Substring(5, 4), System.Globalization.NumberStyles.HexNumber);
      ret.EndingProtocolSequenceNumber = Int32.Parse(msgData.Substring(9, 4), System.Globalization.NumberStyles.HexNumber);
      if (msgData.Length > 13)
      {
        ret.AggregateLogReportingFlag = AggregateLogReportingFlag(msgData.Substring(13, 1));
        ret.StopRESP_QUERY_LOG = StopRESP_QUERY_LOG(msgData.Substring(14, 1));
        if (msgData.Length > 15)
        {
          ret.Filter1 = Filter1(msgData.Substring(15, 1));
          ret.Filter2 = Filter2(msgData.Substring(16, 1));
          ret.Filter3 = Filter3(msgData.Substring(17, 1));
          ret.TimeRange = TimeRange(msgData.Substring(18, 1));
          ret.MaximumNumberOfMessages = Convert.ToInt32(msgData.Substring(19, 3));
          ret.LastMessage = (ELastMessage)Convert.ToInt32(msgData.Substring(22, 1));
          if (msgData.Length > 23)
          {
            ret.GPSStartingDateWeek = Convert.ToInt32(msgData.Substring(23, 4));
            ret.GPSStartingDateSeconds = Convert.ToInt32(msgData.Substring(27, 6));
            if (msgData.Length > 33)
            {
              ret.GPSEndingDateWeek = Convert.ToInt32(msgData.Substring(33, 4));
              ret.GPSEndingDateSeconds = Convert.ToInt32(msgData.Substring(37, 6));
            }
          }
        }
      }
      ret.OriginalParseData = dat;
      return ret;
    }

    static public string ToString(ETTAlertState status)
    {
      switch (status)
      {
        case ETTAlertState.Normal: return "Normal";
        case ETTAlertState.Activated: return "Activated";
        case ETTAlertState.Sent: return "Sent";
        case ETTAlertState.Acknowledged: return "Acknowledged";
        case ETTAlertState.MonitorActivated: return "Monitor Activated";
      }
      return null;
    }

    static public string ToString(EReportFlag flag)
    {
      switch (flag)
      {
        case EReportFlag.ReportOnly_NoReset: return "Report without Reset";
        case EReportFlag.ReportWithReset: return "Report with Reset";
        case EReportFlag.Disabled: return "Disabled";
      }
      return null;
    }
    #endregion

    #region -- Legacy TCM --

    private static string GetResponseDescription(string message)
    {
      int startIdx = message[0] == '>' ? 1 : 0;
      int length = 4;

      if (message[startIdx + 3] == 'K')
      {
        length = 6;
      }
      return message.Substring(startIdx, length);
    }

    public static string GetTrimTracTrigger(TT tt)
    {
      TrimTracData ttData = new TrimTracData(tt);
      return ttData.Trigger;
    }

    public static bool HasPowerLoss(STATUS_MSG data)
    {
      return data.TriggerType == ETriggerType.ExceptionReportAlert;
    }

    public static DateTime GetEventUTCFromGpsTime(STATUS_MSG statusData, DateTime defaultDateTime)
    {
      DateTime eventUtc = DateTime.MinValue;

      if (statusData != null)
      {
        eventUtc = TTParser.ConvertGPSTimeToGMTDateTime(statusData.GPSTimeWeek, statusData.GPSTimeSeconds);
        DateTime initialTrimTracDate = new DateTime(2001, 1, 1, 1, 0, 0);

        // After a battery swap, the first RTKS and RTKP messages have an invalid time
        if (eventUtc <= initialTrimTracDate)
        {
          eventUtc = defaultDateTime;
        }
      }
      return eventUtc;
    }

    public static DateTime FixRTKMEventTime(TT posnOrStatus, DateTime runtimeUTC)
    {
      // When an RTKM is accompanied by an RTKS or RTKP, the accuracy of the eventUTC of the Runtime event can be improved.

      TT_RTKS status = posnOrStatus as TT_RTKS;
      TT_RTKP posnWithStatus = posnOrStatus as TT_RTKP;
      DateTime betterUTC = DateTime.MinValue;
      if (status != null)
        betterUTC = GetEventUTCFromGpsTime(status.Data, runtimeUTC);
      else if (posnWithStatus != null)
        betterUTC = GetEventUTCFromGpsTime(posnWithStatus.Data, runtimeUTC);

      // if the runtime was delivered as a Runtime Meter Report, use the eventUTC from the P or S message
      // as that indicates the actual time the meter was read
      if (status != null && status.Data.TriggerType == ETriggerType.RuntimeMeterReport)
      {
        return betterUTC;
      }
      // else, the runtime has been queried by the gateway, on receipt of a P or S record while in motion,
      // and can share the P or S message eventUTC, with the 
      // exception of a special case where the Gateway combines the runtime message with an 'old' P or S message,
      if (Math.Abs(runtimeUTC.Ticks - betterUTC.Ticks) <= TimeSpan.FromMinutes(30.0).Ticks)
      {
        return betterUTC;
      }

      return runtimeUTC;
    }

    #endregion
  }
}
