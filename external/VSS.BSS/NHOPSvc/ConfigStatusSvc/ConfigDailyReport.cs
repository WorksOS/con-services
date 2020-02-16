using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using VSS.Nighthawk.Instrumentation;

using VSS.Hosted.VLCommon;
using log4net;
using System.Threading;

namespace VSS.Nighthawk.NHOPSvc.ConfigStatus
{
  public class ConfigDailyReport
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private const string dailyReportTask = "DailyReport";
    private static Timer dailyReportTimer;

    public static void Start()
    {
      if (dailyReportTimer == null)
        dailyReportTimer = new Timer(new TimerCallback(UpdateDailyReport));

      dailyReportTimer.Change(0, 0);
    }

    public static void Stop()
    {
      dailyReportTimer = null;
    }

    private static void UpdateDailyReport(object sender)
    {
      try
      {
        if (!CanUpdateDailyReports())
          return;

        DateTime startTime = DateTime.UtcNow;

        int updateDailyReports = 0;
        using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
        {
          List<AssetDailyReportTimes> assetsToCheckForDailyReportChange = GetAssetsToCheckForDailyReportChange(ctx);

          List<AssetDailyReportTimes> assetsCurrentTimeZoneBias =
            GetAssetsCurrentTimeZone(assetsToCheckForDailyReportChange.Select(e => e.AssetID).ToList());

          List<AssetDailyReportTimes> dailyReportsToChange =
            GetAssetsToChangeDailyReport(assetsToCheckForDailyReportChange, assetsCurrentTimeZoneBias);

          UpdateAssetsWithoutChangeToDailyReport(dailyReportsToChange, assetsToCheckForDailyReportChange,
                                                 assetsCurrentTimeZoneBias);

          updateDailyReports = SetNewDailyReports(dailyReportsToChange);

          ctx.SaveChanges();
        }

        UpdateBookmark();

        DateTime endTime = DateTime.UtcNow;

        //Save to metrics db
        List<Instrumentation.ClientMetric> lst = new List<ClientMetric>();
        ClientMetric cm = new ClientMetric()
        {
          className = "VSS.Nighthawk.NHOPSvc.ConfigStatus.ConfigDailyReport",
          context = string.Format("Updated {0} device daily reports", updateDailyReports),
          endUTC = endTime,
          startUTC = startTime,
          methodName = "UpdateDailyReport",
          source = "Config Status Svc"
        };
        lst.Add(cm);

        Instrumentation.MetricsRecorder.AddClientMetricRecords(lst);
      }
      catch (Exception e)
      {
        log.IfError("Error updating daily Report times on devices", e);
      }
      finally
      {
        if (dailyReportTimer != null)
        {
          dailyReportTimer.Change(ConfigStatusSettings.Default.DailyReportRunTimeout, TimeSpan.FromMilliseconds(-1));
        }
      }
    }

    private static void UpdateAssetsWithoutChangeToDailyReport(List<AssetDailyReportTimes> dailyReportsToChange, List<AssetDailyReportTimes> assetsToCheckForDailyReportChange, 
      IEnumerable<AssetDailyReportTimes> assetsCurrentTimeZoneBias)
    {
      //Get the assets that was not changed so the next checkutc can be updated
      IEnumerable<AssetDailyReportTimes> reportsWithoutChange = (from s in assetsToCheckForDailyReportChange
                                                join r in dailyReportsToChange on s.AssetID equals r.AssetID into temp
                                                from t in temp.DefaultIfEmpty()
                                                where t == null
                                                select s).ToList();

      //remove assets that do not have a location (not in the AssetCurrentStatus Set) so we can keep checking them until they do come in 
      var assetsToUpdate = (from r in reportsWithoutChange
                            join a in assetsCurrentTimeZoneBias on r.AssetID equals a.AssetID into temp
                            from t in temp.DefaultIfEmpty()
                            where t != null
                            select r).ToList();

      foreach (AssetDailyReportTimes assetNextDue in assetsToUpdate)
      {
        assetNextDue.CurrentDailyReport.NextCheckUTC =
          assetNextDue.CurrentDailyReport.NextCheckUTC.HasValue
          ? assetNextDue.CurrentDailyReport.NextCheckUTC.Value.Add(
            ConfigStatusSettings.Default.NextDailyReportCheck)
          : DateTime.UtcNow.Add(ConfigStatusSettings.Default.NextDailyReportCheck);
      }

      
    }

    private static List<AssetDailyReportTimes> GetAssetsToChangeDailyReport(IEnumerable<AssetDailyReportTimes> assetsToCheckForDailyReportChange, IEnumerable<AssetDailyReportTimes> assetsCurrentTimeZoneBias)
    {
      var assetsToUpdateDailyReport = (from currentReport in assetsToCheckForDailyReportChange
                                       from currentStatus in assetsCurrentTimeZoneBias
                                       where currentReport.AssetID == currentStatus.AssetID
                                             && (!currentReport.LastDailyReportTZBiasMinutes.HasValue ||
                                             currentStatus.CurrentTZBiasMinutes >=
                                             currentReport.LastDailyReportTZBiasMinutes +
                                             ConfigStatusSettings.Default.AllowableTimeBeforeDailyReportUpdate.
                                               TotalMinutes
                                             ||
                                             currentStatus.CurrentTZBiasMinutes <=
                                             currentReport.LastDailyReportTZBiasMinutes -
                                             ConfigStatusSettings.Default.AllowableTimeBeforeDailyReportUpdate.
                                               TotalMinutes)
                                       select
                                         new AssetDailyReportTimes
                                           {
                                             AssetID = currentReport.AssetID,
                                             CurrentDailyReport = currentReport.CurrentDailyReport,
                                             CurrentTZBiasMinutes = currentStatus.CurrentTZBiasMinutes,
                                             DeviceType = currentReport.DeviceType,
                                             GpsDeviceID = currentReport.GpsDeviceID,
                                             LastDailyReportTZBiasMinutes = currentReport.LastDailyReportTZBiasMinutes
                                           }).ToList();

      return assetsToUpdateDailyReport;
    }

    private static List<AssetDailyReportTimes> GetAssetsCurrentTimeZone(List<long> assetsToCheckDailyReport)
    {
      List<AssetDailyReportTimes> assetsCurrentDailyReport;
      //from retrieved assets get the timezone bias from asset current status that we have received a location from
      using (INH_RPT rptCtx = ObjectContextFactory.NewNHContext<INH_RPT>(true))
      {
        log.IfInfo("Retreiving AssetCurrentStatus for current time zone of asset");

        if (assetsToCheckDailyReport.Any() && assetsToCheckDailyReport.Count() <= 1000)
        {
          assetsCurrentDailyReport = (from acs in rptCtx.AssetCurrentStatusReadOnly
                                      where assetsToCheckDailyReport.Contains(acs.fk_DimAssetID)
                                            && acs.LastLocationUTC != null
                                      select
                                        new AssetDailyReportTimes
                                          {AssetID = acs.fk_DimAssetID, CurrentTZBiasMinutes = acs.TZBiasMinutes}).
            ToList();
        }
        else
        {
          //I do not expect this to happen much if ever since the daily report timer will run every so often and only update the ones that are needed at that time
          //this is mostly here just in case a single run gets too large for the contains method to handle which could happen if too many devices are waiting for a location to come in or this 
          //does not run for too long
          var assetsNewDailyReport = (from acs in rptCtx.AssetCurrentStatusReadOnly
                                      where acs.LastLocationUTC != null
                                      select
                                        new AssetDailyReportTimes { AssetID = acs.fk_DimAssetID, CurrentTZBiasMinutes = acs.TZBiasMinutes }).
            ToList();

          assetsCurrentDailyReport =
            (from a in assetsNewDailyReport where assetsToCheckDailyReport.Contains(a.AssetID) select a).ToList();
        }
        log.IfInfoFormat("Retreived {0} AssetCurrentStatus for current time zone of asset", assetsCurrentDailyReport.Count);
      }

      return assetsCurrentDailyReport;
    }

    private static List<AssetDailyReportTimes> GetAssetsToCheckForDailyReportChange(INH_OP ctx)
    {
      //retrieve Devices that need to be checked for new daily report time
      const int deviceState = (int) DeviceStateEnum.Subscribed;
      log.IfInfo("Retreiving Assets to check for updated Daily report");
      List<AssetDailyReportTimes> assetsToCheckForDailyReportChange = (from d in ctx.Device
                                                join dr in ctx.DailyReport on d.ID equals dr.fk_DeviceID
                                                join a in ctx.Asset on d.ID equals a.fk_DeviceID
                                                where
                                                  d.fk_DeviceStateID == deviceState &&
                                                  (dr.IsUserCustomized == null || dr.IsUserCustomized == false)
                                                  && (dr.NextCheckUTC == null || dr.NextCheckUTC <= DateTime.UtcNow)
                                                  && d.fk_DeviceTypeID != (int)DeviceTypeEnum.MANUALDEVICE
                                                select
                                                  new AssetDailyReportTimes
                                                    {
                                                      AssetID = a.AssetID,
                                                      GpsDeviceID = d.GpsDeviceID,
                                                      DeviceType = d.fk_DeviceTypeID,
                                                      LastDailyReportTZBiasMinutes = dr.LastDailyReportTZBias,
                                                      CurrentDailyReport = dr
                                                    }).ToList();

      log.IfInfoFormat("Retreived {0} Assets to check for updated Daily report", assetsToCheckForDailyReportChange.Count);
      return assetsToCheckForDailyReportChange;
    }

    private static int SetNewDailyReports(List<AssetDailyReportTimes> assetsToChangeDailyReport)
    {
      int count = 0;
      foreach (AssetDailyReportTimes assetToUpdate in assetsToChangeDailyReport)
      {
        if(API.Device.IsProductLinkDevice((DeviceTypeEnum)assetToUpdate.DeviceType))
          UpdatePLDailyReport(assetToUpdate);
        else if (DeviceTypeFeatureMap.DeviceTypePropertyValue((DeviceTypeEnum)assetToUpdate.DeviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.MTSInOut.ToValString())
          UpdateMTSDailyReport(assetToUpdate);

        count++;
      }
      return count;
    }

    private static void UpdatePLDailyReport(AssetDailyReportTimes currentAssetToUpdate)
    {

      //if the device is a PL Device send the daily report message for 11 pm only since the device has it's own randomizing method
      TimeSpan dailyReportTime =
        ConfigStatusSettings.Default.DefaultDailyReportUTCTime.Add(
          -TimeSpan.FromMinutes(currentAssetToUpdate.CurrentTZBiasMinutes));

      DateTime currentTime = DateTime.UtcNow;
      DateTime dateToSend = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day,
                                         dailyReportTime.Hours, dailyReportTime.Minutes,
                                         dailyReportTime.Seconds,
                                         dailyReportTime.Milliseconds);

      log.IfInfoFormat("Updating AssetID: {0} DeviceType: {1} gpsDeviceID: {2} to DailyReport of {3}",
                       currentAssetToUpdate.AssetID, (DeviceTypeEnum)currentAssetToUpdate.DeviceType, currentAssetToUpdate.GpsDeviceID,
                       dateToSend);
        using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
        {
            API.PLOutbound.SendReportIntervalsConfig(opCtx1, currentAssetToUpdate.GpsDeviceID,
                (DeviceTypeEnum) currentAssetToUpdate.DeviceType,
                null, null, null, null, null, null, dateToSend, null, null,
                null,
                null);
            currentAssetToUpdate.CurrentDailyReport.LastDailyReportTZBias = currentAssetToUpdate.CurrentTZBiasMinutes;
            currentAssetToUpdate.CurrentDailyReport.LastDailyReportUTC = dateToSend;
            currentAssetToUpdate.CurrentDailyReport.NextCheckUTC =
                currentAssetToUpdate.CurrentDailyReport.NextCheckUTC.HasValue
                    ? currentAssetToUpdate.CurrentDailyReport.NextCheckUTC.Value.Add(
                        ConfigStatusSettings.Default.NextDailyReportCheck)
                    : DateTime.UtcNow.Add(ConfigStatusSettings.Default.NextDailyReportCheck);
        }
    }

    private static void UpdateMTSDailyReport(AssetDailyReportTimes currentAssetToUpdate)
    {
      Random randomMinute = new Random((int) DateTime.Now.Ticks & 0x0000FFFF);
      int randomTimeToSend = randomMinute.Next(1, 59);

      TimeSpan time =
        ConfigStatusSettings.Default.DefaultDailyReportUTCTime.Add(TimeSpan.FromHours(-1)).Add(
          TimeSpan.FromMinutes(-currentAssetToUpdate.CurrentTZBiasMinutes + randomTimeToSend));
      
      log.IfInfoFormat("Updating AssetID: {0} DeviceType: {1} gpsDeviceID: {2} to DailyReport of {3}",
                       currentAssetToUpdate.AssetID, (DeviceTypeEnum)currentAssetToUpdate.DeviceType, currentAssetToUpdate.GpsDeviceID,
                       new TimeSpan(0, time.Hours, randomTimeToSend, 0));
      DateTime currentDate = DateTime.UtcNow;
      currentAssetToUpdate.CurrentDailyReport.LastDailyReportTZBias = currentAssetToUpdate.CurrentTZBiasMinutes;
      currentAssetToUpdate.CurrentDailyReport.LastDailyReportUTC = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day,
                                                                        time.Hours, randomTimeToSend, 0);
      currentAssetToUpdate.CurrentDailyReport.NextCheckUTC =
        currentAssetToUpdate.CurrentDailyReport.NextCheckUTC.HasValue
          ? currentAssetToUpdate.CurrentDailyReport.NextCheckUTC.Value.Add(
            ConfigStatusSettings.Default.NextDailyReportCheck)
          : DateTime.UtcNow.Add(ConfigStatusSettings.Default.NextDailyReportCheck);
        using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
        {
            API.MTSOutbound.SendDailyReportConfig(opCtx1, new string[] {currentAssetToUpdate.GpsDeviceID},
                (DeviceTypeEnum) currentAssetToUpdate.DeviceType, true, (byte) time.Hours,
                (byte) time.Minutes,
                "UTC");
        }
    }

    private static void UpdateBookmark()
    {
      // update the bookmark manager for the correctly updated values so we will properly process next time.
      // Again, new context is created here so that there is no delay that can cause concurrency problems.
      using (INH_OP opCtxWritable = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        BookmarkManager bm = GetBookmark(opCtxWritable, dailyReportTask);
        bm.DueUTC = DateTime.UtcNow.Add(ConfigStatusSettings.Default.DailyReportRunTimeout);
        bm.BookmarkUTC = DateTime.UtcNow;
        bm.InProgress = false;
        bm.UpdateUTC = DateTime.UtcNow;
        try
        {
          log.IfDebugFormat("Updating DailyReport bookmark to {0}", bm.DueUTC.Value);
          opCtxWritable.SaveChanges();
        }
        catch (OptimisticConcurrencyException e)
        {
          log.InfoFormat("Concurreny exception occured while updating task {0} : {1}", dailyReportTask, e.Message);
        }
      }
    }

    private static bool CanUpdateDailyReports()
    {
      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        BookmarkManager bm = GetBookmark(ctx, dailyReportTask);
        try
        {
          if (!bm.InProgress && (bm.DueUTC == null || bm.DueUTC <= DateTime.UtcNow))
          {
            bm.InProgress = true;
            bm.StartUTC = DateTime.UtcNow;
            bm.UpdateServer = Environment.MachineName;
            bm.UpdateUTC = DateTime.UtcNow;
            ctx.SaveChanges();
            log.IfDebug("Daily Report Task will is currently due and will run");
            return true;
          }
          // Stuck? Unstick it.
          else if (((bm.InProgress) && bm.UpdateServer == Environment.MachineName) || ((DateTime.UtcNow >= bm.DueUTC) && (bm.InProgress) &&
                   (DateTime.UtcNow >= bm.StartUTC.Value.Add(ConfigStatusSettings.Default.DailyReportRunTimeout))))
          {
            bm.InProgress = false;
            bm.UpdateUTC = DateTime.UtcNow;

            ctx.SaveChanges();
            log.IfWarn("Daily Report Updater stuck. Unsticking...");
          }
        }
        catch (OptimisticConcurrencyException)
        // Will occur if another instance of the Daily Report Config service is running concurrently with this one,
        // and it has set the bookmark to "in progress" since this instance loaded/changed its copy of the bookmark manager.
        {
          log.IfWarn("ConfigStatusSvc.CanUpdateReports: Optimistic Concurrency Exception occurred");
          ((NH_OP)ctx).Refresh(RefreshMode.StoreWins, bm);
        }
      }
      return false;
    }

    private static BookmarkManager GetBookmark(INH_OP opCtxWritable, string task)
    {
      BookmarkManager bm = (from bms in opCtxWritable.BookmarkManager where bms.Task == task select bms).SingleOrDefault();

      if (null == bm)
      {
        bm = new BookmarkManager() { ID = -1, Task = task, InProgress = false, UpdateUTC = DateTime.UtcNow };
        opCtxWritable.BookmarkManager.AddObject(bm);
        opCtxWritable.SaveChanges();
      }

      return bm;
    }

    private class AssetDailyReportTimes
    {
      public long AssetID;
      public string GpsDeviceID;
      public int DeviceType;
      public int? LastDailyReportTZBiasMinutes;
      public int CurrentTZBiasMinutes;
      public DailyReport CurrentDailyReport;
    }
  }
}
