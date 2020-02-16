using System;
using System.Configuration;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Timers;
using log4net;
using VSS.Hosted.VLCommon;
using System.Data.Entity;
using Timer = System.Timers.Timer;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Common;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  public class TaskScheduler
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private int TaskTimeOutInterval { get; set; }
    public int ServiceWakeUpInterval { get; set; }
    private const int DefaultServiceWakeUpInterval = 60000;
    private const int DefaultTaskTimeOutInterval = 620000;
    private int _concurrencyExceptionCounter;
    private bool _isDataProcessed;
    private readonly IHttpRequestWrapper _httpRequestWrapper;
    private readonly IConfigurationManager _configurationManager;
    private readonly ITpassAuthorizationManager _tpassAuthorizationManager;
    private readonly ICacheManager _cacheManager;
    private Thread _processThread;
    private bool _isServiceStopped;

    public TaskScheduler(IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
    {
      //This means the interval time after which the service has to wake up
      ServiceWakeUpInterval = (ConfigurationManager.AppSettings["ServiceWakeUpInterval"] != null) ? Convert.ToInt32(ConfigurationManager.AppSettings["ServiceWakeUpInterval"]) : DefaultServiceWakeUpInterval;
      // This means that if the task startutc is greater than this interval, then the task is assumed to have stuck
      //and so some app server can pick up this task and can process records
      TaskTimeOutInterval = (ConfigurationManager.AppSettings["TaskTimeoutInterval"] != null) ? (Convert.ToInt32(ConfigurationManager.AppSettings["TaskTimeoutInterval"]) / 60000) : DefaultTaskTimeOutInterval;
      _httpRequestWrapper = httpRequestWrapper;
      _configurationManager = configurationManager;
      _cacheManager = cacheManager;
      _tpassAuthorizationManager = tpassAuthorizationManager;
    }

    public bool Start()
    {
      try
      {
        ThreadStart ts = PickupSyncTaskAndProcess;
        _processThread = new Thread(ts) { IsBackground = false };
        _processThread.Start();
        return true;
      }
      catch (Exception ex)
      {
        Log.IfError(string.Format("Message {0} \n Source {1} \n StackTrace {2}", ex.Message, ex.Source, ex.StackTrace));
        return false;
      }
    }

    //When a task returns no data to process go to idle state
    private void GotoIdleState(int serviceTimerInterval = 1)
    {
      var timer = new Timer(serviceTimerInterval) { AutoReset = false };
      timer.Elapsed += PickupSyncTaskAndProcessHandler;
      timer.Start();
    }

    private void PickupSyncTaskAndProcessHandler(object sender, ElapsedEventArgs e)
    {
      Log.IfDebug(string.Format("I am {0} Waking-up. Looking for tasks to process", Environment.MachineName));
      _concurrencyExceptionCounter = 0;
      PickupSyncTaskAndProcess();
    }

    private void PickupSyncTaskAndProcess()
    {
      try
      {
        if (!_isServiceStopped)
        {
          using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
          {
            //Take the oldest tasks among 'yet to progress' tasks or 'In Progress' task held up longer by an app server
            var taskToProcess =
              opCtx.MasterDataSyncReadOnly.Where(i => (!i.InProgress || (i.InProgress && DbFunctions.DiffMinutes(i.StartUTC, DateTime.UtcNow) >= TaskTimeOutInterval)) && i.IsActive)
                .OrderBy(u => u.EndUTC)
                .FirstOrDefault();

            if (taskToProcess != null)
            {
              var taskName = taskToProcess.TaskName;
              switch (taskName)
              {
                case StringConstants.AssetTask:
                  var assetProcessor = new AssetSyncProcessor(taskName, _httpRequestWrapper, _configurationManager, _cacheManager, _tpassAuthorizationManager);
                  _isDataProcessed = assetProcessor.Process(ref _isServiceStopped);
                  break;
                case StringConstants.GroupTask:
                  var groupProcessor = new GroupSyncProcessor(taskName, _httpRequestWrapper, _configurationManager, _cacheManager, _tpassAuthorizationManager);
                  _isDataProcessed = groupProcessor.Process(ref _isServiceStopped);
                  break;
                case StringConstants.CustomerTask:
                  var customerProcessor = new CustomerSyncProcessor(taskName, _httpRequestWrapper, _configurationManager, _cacheManager, _tpassAuthorizationManager);
                  _isDataProcessed = customerProcessor.Process(ref _isServiceStopped);
                  break;
                case StringConstants.CustomerUserTask:
                  var customerUserProcessor = new CustomerUserSyncProcessor(taskName, _httpRequestWrapper, _configurationManager, _cacheManager, _tpassAuthorizationManager);
                  _isDataProcessed = customerUserProcessor.Process(ref _isServiceStopped);
                  break;
                case StringConstants.CustomerAssetTask:
                  var customerAssetProcessor = new CustomerAssetSyncProcessor(taskName, _httpRequestWrapper, _configurationManager, _cacheManager, _tpassAuthorizationManager);
                  _isDataProcessed = customerAssetProcessor.Process(ref _isServiceStopped);
                  break;
                case StringConstants.DeviceTask:
                  var deviceProcessor = new DeviceSyncProcessor(taskName, _httpRequestWrapper, _configurationManager, _cacheManager, _tpassAuthorizationManager);
                  _isDataProcessed = deviceProcessor.Process(ref _isServiceStopped);
                  break;
                case StringConstants.GeofenceTask:
                  var geofenceProcessor = new GeofenceSyncProcessor(taskName, _httpRequestWrapper, _configurationManager, _cacheManager, _tpassAuthorizationManager);
                  _isDataProcessed = geofenceProcessor.Process(ref _isServiceStopped);
                  break;
                case StringConstants.SubscriptionTask:
                  var subscriptionProcessor = new SubscriptionSyncProcessor(taskName, _httpRequestWrapper, _configurationManager, _cacheManager, _tpassAuthorizationManager);
                  _isDataProcessed = subscriptionProcessor.Process(ref _isServiceStopped);
                  break;
                case StringConstants.WorkDefinitionTask:
                  var workDefintionProcessor = new WorkDefinitionSyncProcessor(taskName, _httpRequestWrapper, _configurationManager, _cacheManager, _tpassAuthorizationManager);
                  _isDataProcessed = workDefintionProcessor.Process(ref _isServiceStopped);
                  break;
                case StringConstants.MarkSiteFavoritesTask:
                  var userSiteFavoritesSyncProcessor = new UserSiteFavoritesSyncProcessor(taskName, _httpRequestWrapper, _configurationManager, _cacheManager, _tpassAuthorizationManager);
                  _isDataProcessed = userSiteFavoritesSyncProcessor.Process(ref _isServiceStopped);
                  break;
                case StringConstants.PreferenceTask:
                  var userPrefSyncProcessor = new PreferenceSyncProcessor(taskName, _httpRequestWrapper, _configurationManager, _cacheManager, _tpassAuthorizationManager);
                  _isDataProcessed = userPrefSyncProcessor.Process(ref _isServiceStopped);
                  break;
                case StringConstants.DeviceAssetTask:
                  var deviceAssetSyncProcessor = new DeviceAssetSyncProcessor(taskName, _httpRequestWrapper,_configurationManager, _cacheManager, _tpassAuthorizationManager);
                  _isDataProcessed = deviceAssetSyncProcessor.Process(ref _isServiceStopped);
                  break;
                case StringConstants.CustomerHierarchyTask:
                  var customerHierarchySyncProcessor = new CustomerHierarchySyncProcessor(taskName, _httpRequestWrapper, _configurationManager, _cacheManager, _tpassAuthorizationManager);
                  _isDataProcessed = customerHierarchySyncProcessor.Process(ref _isServiceStopped);
                  break;
                case StringConstants.DeviceTransferReplacementTask:
                  var deviceTransferReplacementSyncProcessor = new DeviceTransferReplacementSyncProcessor(taskName, _httpRequestWrapper, _configurationManager, _cacheManager, _tpassAuthorizationManager);
                  _isDataProcessed = deviceTransferReplacementSyncProcessor.Process(ref _isServiceStopped);
                  break;
              }
            }
          }
        }
      }
      catch (OptimisticConcurrencyException)
      {
        _concurrencyExceptionCounter++;
        Log.IfError(string.Format("OptimisticConcurrencyException. Retrying {0}", _concurrencyExceptionCounter));

        if (_concurrencyExceptionCounter < 3)
        {
          Thread.Sleep(10000);
          PickupSyncTaskAndProcess();
        }
        else
        {
          Log.IfError(string.Format("OptimisticConcurrencyException. Retrying exhausted after {0} times", _concurrencyExceptionCounter));
          GotoIdleState(ServiceWakeUpInterval);
        }
      }
      catch (DbUpdateConcurrencyException)
      {
        _concurrencyExceptionCounter++;
        Log.IfError(string.Format("DbUpdateConcurrencyException. Retrying {0}", _concurrencyExceptionCounter));
        if (_concurrencyExceptionCounter < 3)
        {
          Thread.Sleep(10000);
          PickupSyncTaskAndProcess();
        }
        else
        {
          Log.IfError(string.Format("DbUpdateConcurrencyException. Retrying exhausted after {0} times", _concurrencyExceptionCounter));
          GotoIdleState(ServiceWakeUpInterval);
        }
      }
      catch (ArgumentNullException ex)
      {
        Log.IfError(string.Format("ArgumentNullException \n {0} \n {1}", ex.Message, ex.StackTrace));
      }
      catch (Exception ex)
      {
        Log.IfError(string.Format("Exception \n {0} \n {1}", ex.Message, ex.StackTrace));
      }
      finally
      {
        Log.IfDebug(string.Format("{0} Sleeping. No tasks to Process", Environment.MachineName));
        if (!_isServiceStopped)
        {
          if (_isDataProcessed)
          {
            PickupSyncTaskAndProcess();
          }
          else
            GotoIdleState(ServiceWakeUpInterval);
        }
      }
    }

    public void Stop()
    {
      _isServiceStopped = true;
      _processThread.Join();
      SyncProcessorBase.UnLockAllTaskState();
    }
  }
}
