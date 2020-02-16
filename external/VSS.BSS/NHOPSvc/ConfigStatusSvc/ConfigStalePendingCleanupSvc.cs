using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Reflection;
using VSS.Nighthawk.Instrumentation;

using VSS.Hosted.VLCommon;
using log4net;
using System.Threading;

namespace VSS.Nighthawk.NHOPSvc.ConfigStatus
{
  public class ConfigStalePendingCleanupSvc
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private const string CleanupStalePendingStatesTask = "CleanupStalePendingStates";
    private static Timer CleanupStalePendingStatesTimer;

    public static void Start()
    {
      if (CleanupStalePendingStatesTimer == null)
        CleanupStalePendingStatesTimer = new Timer(new TimerCallback(CleanupStalePendingStates));

      CleanupStalePendingStatesTimer.Change(0, 0);
    }

    public static void Stop()
    {
      CleanupStalePendingStatesTimer = null;
      UpdateBookmark(false);
    }

    private static void CleanupStalePendingStates(object sender)
    {
      bool noOtherCleanupProcessesAreRunning = UpdateBookmark(true);

      if (noOtherCleanupProcessesAreRunning)
      {
        try
        {
        
          log.IfInfo("CleanupStalePendingStates start");

          int stalePendingDeviceCount = 0;

          DateTime startTime = DateTime.UtcNow;

          int eightDaysOldKeyDate = startTime.AddDays(-8).KeyDate();

          using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
          {
            List<Device> staleDevices = (from d in opCtx.Device
              where (d.fk_DeviceTypeID == (int)DeviceTypeEnum.PL121 || d.fk_DeviceTypeID == (int)DeviceTypeEnum.PL321)
              && d.OldestPendingKeyDate < eightDaysOldKeyDate
              select d).ToList();

            foreach (var staleDevice in staleDevices)
            {
              CleanupStalePendingRegistryEntries(staleDevice, eightDaysOldKeyDate);

              log.IfInfoFormat(
                "CleanupStalePendingStates cleaning up stale device pending for: {0}  oldest key date: {1}",
                staleDevice.GpsDeviceID, staleDevice.OldestPendingKeyDate);
            }

            opCtx.SaveChanges();

            stalePendingDeviceCount = staleDevices.Count;
          }

          UpdateBookmark(false);

          DateTime endTime = DateTime.UtcNow;

          //Save to metrics db
          List<Instrumentation.ClientMetric> lst = new List<ClientMetric>();
          ClientMetric cm = new ClientMetric()
                            {
                              className = "VSS.Nighthawk.NHOPSvc.ConfigStatus.ConfigStalePendingCleanupSvc",
                              context =
                                string.Format("Cleaned up {0} device pending state registries", stalePendingDeviceCount),
                              endUTC = endTime,
                              startUTC = startTime,
                              methodName = "CleanupStalePendingStates",
                              source = "Config Status Svc"
                            };
          lst.Add(cm);

          Instrumentation.MetricsRecorder.AddClientMetricRecords(lst);
        }
        catch
        (Exception e)
        {
          log.IfError("Error CleanupStalePendingStates on devices", e);
        }
        finally
        {
          if (CleanupStalePendingStatesTimer != null)
          {
            CleanupStalePendingStatesTimer.Change(ConfigStatusSettings.Default.ConfigStalePendingCleanupTimeout,
              TimeSpan.FromMilliseconds(-1));
          }
        }

        log.IfInfo("CleanupStalePendingStates end");
      }
    }

    private static void CleanupStalePendingRegistryEntries(Device staleDevice, int eightDaysOldKeyDate)
    {
      PLConfigData data = new PLConfigData(staleDevice.DeviceDetailsXML);

      data.CleanupStalePendingRegistryEntries(eightDaysOldKeyDate);
      staleDevice.DeviceDetailsXML = data.ToXElement().ToString();
      staleDevice.OldestPendingKeyDate = data.OldestPendingKeyDate;
    }

    private static bool UpdateBookmark(bool inProgress)
    {
      // update the bookmark manager for the correctly updated values so we will properly process next time.
      // Again, new context is created here so that there is no delay that can cause concurrency problems.
      using (INH_OP opCtxWritable = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        BookmarkManager bm = GetBookmark(opCtxWritable, CleanupStalePendingStatesTask);
        
        if (inProgress == bm.InProgress) return false;

        if (inProgress)
        {
          bm.DueUTC = DateTime.UtcNow.Add(ConfigStatusSettings.Default.ConfigStalePendingCleanupTimeout);
          bm.BookmarkUTC = DateTime.UtcNow;
        }

        bm.InProgress = inProgress;
        bm.UpdateUTC = DateTime.UtcNow;

        try
        {
          if (inProgress)
            log.IfInfo("Updating ConfigStalePendingCleanupSvc bookmark to InProgress");
          else
            log.IfInfoFormat("Updating ConfigStalePendingCleanupSvc bookmark to {0}", bm.DueUTC.Value);
          
          opCtxWritable.SaveChanges();
        }
        catch (OptimisticConcurrencyException e)
        {
          log.InfoFormat("Concurreny exception occured while updating task {0} : {1}", CleanupStalePendingStatesTask, e.Message);
        }

        return true;
      }
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
  }
}
