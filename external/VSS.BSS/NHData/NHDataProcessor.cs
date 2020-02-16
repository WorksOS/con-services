using log4net;
using MassTransit;
using Spring.Aop.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Events;
using VSS.Hosted.VLCommon.Utilities;
using VSS.Nighthawk.MTSGateway.Common.Commands.NHDataSvc;
using VSS.Nighthawk.NHDataSvc.Helpers;

namespace VSS.Nighthawk.NHDataSvc
{
  /// <summary>
  /// This class implements the NH Data Service's main processing logic.
  /// 
  /// It's primary function is to reliably store data to the NH_DATA database, and to forward data to the 
  /// Alert Trigger service and Site Determination service.
  /// 
  /// It's implementation includes look up of asset IDs, from the received data's device identification (deviceType+gpsDeviceID)
  /// 
  /// This service *must* perform very well. It is a potential bottleneck for the flow of data through Nighthawk.
  /// Please keep this in mind for all extensions to this class. Minimise DB read activity. Keep data caches under control.
  /// </summary>
  [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
  public class NHDataProcessor : INHDataProcessor
  {
    /// <summary>
    /// Self-reference for Spring-proxied class
    /// </summary>
    private SpringProxyHandle<NHDataProcessor> _proxyHandle;

    private static Task assetIDTask;
    private static Task timezoneTask;
    private static readonly TimeSpan futureTimeThreshold = GetFutureTimeThreshold();

    public IServiceBus ServiceBus { get; set; }
    public IAlertSvc AlertSvc { get; set; }
    public VehicleMappingLookupCache VehicleMappingLookupCache { get; set; }

    private static bool _enablePublishingToServiceBus;

    private static TimeSpan GetFutureTimeThreshold()
    {
      TimeSpan futureTime;
      if (TimeSpan.TryParse(ConfigurationManager.AppSettings["FutureTimeThreshold"], out futureTime))
        return futureTime;
      else
        return TimeSpan.FromMinutes(5);  // default 5 minutes
    }

    public virtual SpringProxyHandle<NHDataProcessor> MyProxy
    {
      get { return _proxyHandle ?? (_proxyHandle = new SpringProxyHandle<NHDataProcessor>(this, () => (NHDataProcessor)AopContext.CurrentProxy)); }
    }

    /// <summary>
    /// Use this to invoke instance methods that have been instrumented for AOP
    /// </summary>
    private NHDataSaver _nhDataSaverInstance;
    private NHDataSaver NHDataSaverInstance
    {
      get { return _nhDataSaverInstance = _nhDataSaverInstance ?? SpringObjectFactory.CreateObject<NHDataSaver>(); }
    }

    #region Service Hosting
    /// <summary>
    /// Starts the hosting process, and prepares an Asset ID cache for use.
    /// </summary>
    internal virtual void Start()
    {
      log.IfInfo("NHDataProcessor starting up....");

      MSMQUtils.EnsureMSMQ(svcEndpointQueue, MSMQUtils.QueuePath(svcEndpointQueue), true);

      _enablePublishingToServiceBus = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["NHDataSvc.EnablePublishingToServiceBus"]) && Convert.ToBoolean(ConfigurationManager.AppSettings["NHDataSvc.EnablePublishingToServiceBus"]);

      log.IfInfoFormat("EnablePublishingToServiceBus is {0}", _enablePublishingToServiceBus);

      assetIDTask = Task.Factory.StartNew(() => AssetIDCache.Init());
      timezoneTask = Task.Factory.StartNew(() => TimeZone = API.SpatialTimeZone);
      m_host = new NHHost<NHDataProcessor>();
      m_host.StartService();

      log.IfInfo("NHDataProcessor started");
      //get the service started

      if (null == heartbeat)
      {
        heartbeat = new Timer(HeartbeatTimer);
      }
      heartbeat.Change(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

      if (_enablePublishingToServiceBus)
        VehicleMappingLookupCache.Initialize();
    }

    /// <summary>
    /// Stops the hosting process, effectively taking the service offline.
    /// </summary>
    internal virtual void Stop()
    {
      if (m_host == null) return;

      m_host.StopService();
      log.IfInfo("NHDataProcessor stopped");

      if (null != heartbeat)
      {
        heartbeat.Change(Timeout.Infinite, Timeout.Infinite);
      }

      if (_enablePublishingToServiceBus)
        VehicleMappingLookupCache.Release();
    }
    #endregion

    #region INHDataProcessor
    private static void HeartbeatTimer(object state)
    {
      log.IfInfoFormat("{0} items processed (since startup)", recordsProcessed);
      if(_enablePublishingToServiceBus)
        log.IfInfoFormat("Location items - processed:{0}, published:{1}, percentage:{2}% (since startup)", locationItemsProcessedSinceStartup, locationItemsPublishedSinceStartup, (locationItemsProcessedSinceStartup>0)?Math.Round(((double)locationItemsPublishedSinceStartup/locationItemsProcessedSinceStartup)*100,2):0);
    }

    /// <summary>
    /// This is the endpoint method for the service. This method is invoked by the WCF framework upon the arrival of
    /// data on the service's transport (a namedpipe, with an MSMQ backup).
    /// 
    /// </summary>
    /// <param name="nhDataObjects">A list of wrapped NH_DATA objects.</param>
    public virtual void Process(List<NHDataWrapper> nhDataObjects)
    {
      DateTime startTime = DateTime.UtcNow;
      try
      {
        DateTime start = DateTime.UtcNow;
        List<DataHoursLocation> locationItems;
        List<INHDataObject> alertItems;
        ((NHDataProcessor)MyProxy).SaveData(nhDataObjects, out locationItems, out alertItems);        
        DateTime end = DateTime.UtcNow;
        log.IfDebugFormat("SaveData time to process: {0}", end.Subtract(start));

        start = DateTime.UtcNow;
        if (null != locationItems && locationItems.Count > 0)
        {
          locationItemsProcessedSinceStartup += locationItems.Count;
          ((NHDataProcessor)MyProxy).ForwardSiteDispatchData(locationItems);
          ((NHDataProcessor)MyProxy).ForwardPLLocationData(locationItems);
        }

        if (alertItems != null && alertItems.Count > 0)
        {
          try
          {
            //Get all of the location data from the events that where sent in and stick it into the location list to be sent to the AlertSvc
            //also make them distinct so that there is not more than what is needed
            List<DataHoursLocation> locs =
              (from INHDataObject l in alertItems
               let locAlreadyIncluded =
                 locationItems != null &&
                 locationItems.Any(e => e.AssetID == l.AssetID && l.LocationData != null && (e.EventUTC == l.LocationData.LocationEventUTC))
               where
                 l.LocationData != null && !locAlreadyIncluded && !(l is DataHoursLocation) && l.LocationData.Latitude.HasValue &&
                 l.LocationData.Longitude.HasValue
               select
                 new DataHoursLocation
                   {
                     AssetID = l.AssetID,
                     DeviceType = l.DeviceType,
                     GPSDeviceID = l.GPSDeviceID,
                     Longitude = l.LocationData.Longitude,
                     Latitude = l.LocationData.Latitude,
                     RuntimeHours = l.LocationData.ServiceMeterHours,
                     EventUTC = l.EventUTC,
                     OdometerMiles = l.LocationData.OdometerMiles,
                     LocIsValid = true,
                     SourceMsgID = l.SourceMsgID,
                     fk_DimSourceID = l.fk_DimSourceID,
                     DebugRefID = l.DebugRefID
                   }).ToList();

            if (locationItems == null && locs.Count > 0)
              locationItems = new List<DataHoursLocation>();
            if (locs.Count > 0)
            {
              if (locationItems == null)
                locationItems = new List<DataHoursLocation>();
              locationItems.AddRange(locs);
            }
          }
          catch(Exception e)
          {
            log.IfWarn("Could not add location data from event alert will have to use DB", e);
          }

          ((NHDataProcessor)MyProxy).ForwardAlertableItems(alertItems, locationItems);
        }
        end = DateTime.UtcNow;
        log.IfDebugFormat("forwardItems time to process: {0}", end.Subtract(start));

        recordsProcessed += nhDataObjects.Count;

        // for Oculus
        if (_enablePublishingToServiceBus)
        {
          // Filter & Publish Location items only
          if (null != locationItems && locationItems.Count > 0)
            locationItemsPublishedSinceStartup += ((NHDataProcessor)MyProxy).PublishLocationItemsToServiceBus(locationItems);

          // Process other item types here
        }
      }
      catch (Exception ex)
      {
        log.IfError("Unexpected uncaught exception storing to NH_DATA (data loss!)", ex);
      }
      DateTime endTime = DateTime.UtcNow;
      log.IfDebugFormat("Total time to process: {0}", endTime.Subtract(startTime));
    }

    public virtual void DeleteServiceMeterAdjustment(string serialNumber, DeviceTypeEnum deviceType)
    {
      log.IfDebug("Getting the asset id from cache");
      var assetID = AssetIDCache.GetAssetID(serialNumber, deviceType);
      if (!assetID.HasValue) return;
      NHDataSaver.DeleteServiceMeterAdjustment(assetID.Value);
      ((NHDataProcessor) MyProxy).ServiceBus.Publish(new RuntimeAdjustmentDeletedEvent
      {
        GpsDeviceID = serialNumber,
        DeviceType = deviceType
      }, typeof (RuntimeAdjustmentDeletedEvent));
      log.IfDebugFormat("Published the runtime adjustment deleted event - GPSDeviceID:{0} DeviceType:{1}",
        serialNumber,
        deviceType);
    }

    #endregion

    #region Implementation
    /// <summary>
    /// Most data is received without an asset ID. Data cannot be stored in NH_DATA without first determining it's
    /// owner asset ID. This is determined via the device identification information (deviceType+gpsDeviceID) in the data.
    /// </summary>
    protected virtual void SetAssetIDAndDeviceTypeID(List<NHDataWrapper> nhDataObjects)
    {
      if (assetIDTask != null)
        assetIDTask.Wait();

      foreach (NHDataWrapper item in nhDataObjects)
      {
        INHDataObject dataObject = item.Data;
        AssetIDCache.AssetKeys assetID;

        if ((dataObject.AssetID == 0) && (dataObject.GPSDeviceID == null))
        {
          log.IfInfoFormat("NHDataProcessor.SetAssetIDAndDeviceTypeID: AssetID=0 and GPSDeviceID is null for this object: {0}", dataObject.ToXElement());
          assetID = null;
        }
        else
        {
          assetID = AssetIDCache.GetCacheItem(dataObject.GPSDeviceID, dataObject.DeviceType, dataObject.AssetID);
        }

        if ((assetID != null && assetID.AssetID != 0 && dataObject.AssetID == 0) || (assetID != null && assetID.AssetID != 0 && dataObject.AssetID == assetID.AssetID))
        {
          dataObject.AssetID = assetID.AssetID;
          dataObject.DeviceType = (DeviceTypeEnum)assetID.DeviceTypeID;
        }
        else
        {
          dataObject.AssetID = 0;          
        }
      }
    }

    /// <summary>
    /// Saves data to NH_DATA.
    /// Data is organized into like groups, then each group is saved in bulk.
    /// 
    /// This code makes every attempt to save the data, and not drop it.If the save fails for some unexpected reason,
    /// the save is retried, after a pause, by replaying it back through the service. This repeats a number of times,
    /// until eventually the data is dropped. Ideally, the data should never get dropped.
    /// 
    /// This method also collects up location items, and data destined for the alert system, as a convenience.
    /// </summary>
    protected virtual void SaveData(List<NHDataWrapper> nhDataObjects, out List<DataHoursLocation> locationItems, out List<INHDataObject> alertItems)
    {
      locationItems = null;
      alertItems = null;
      
      ((NHDataProcessor)MyProxy).SetAssetIDAndDeviceTypeID(nhDataObjects);

      List<int> supportedDeviceTypes = API.Maintenance.GetSupportedDeviceTypesForManualMaintenance();
      var sameTypeGroups = nhDataObjects.Where(wrappedItem => wrappedItem.Data.AssetID != 0 &&
                                                              (wrappedItem.Data.fk_DimSourceID !=
                                                               (int)DimSourceEnum.UserEntered ||
                                                               (wrappedItem.Data.fk_DimSourceID ==
                                                                (int)DimSourceEnum.UserEntered &&
                                                                supportedDeviceTypes.Contains((int)wrappedItem.Data.DeviceType)
                                                                ))
                                                                && wrappedItem.Data.EventUTC <= DateTime.UtcNow.Add(futureTimeThreshold)
                                                              && wrappedItem.StoreAttempts <= MaxRetries).GroupBy(eo => eo.Data.GetType());

      bool failedAndRetryable = false;
      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        try
        {
          List<INHDataObject> items = (from wrappedItem in typeGroup
                                       select wrappedItem.Data).ToList<INHDataObject>();
       

          if (typeGroup.Key.Name == typeof(DataHoursLocation).Name)
          {
            locationItems = ((NHDataProcessor)MyProxy).FilterLocationItems(items);

            if (NHDataSettings.Default.DoLocalTimeLookup)
            {
              ((NHDataProcessor)MyProxy).AddLocalTime(locationItems);
            }
          }

          if (typeGroup.Key == typeof(DataEngineStartStop) || typeGroup.Key == typeof(DataFaultDiagnostic) || typeGroup.Key == typeof(DataFaultEvent)
            || typeGroup.Key == typeof(DataFenceAlarm) || typeGroup.Key == typeof(DataMoving) || typeGroup.Key == typeof(DataSiteState)
            || typeGroup.Key == typeof(DataSpeeding) || typeGroup.Key == typeof(DataIgnOnOff) || typeGroup.Key == typeof(DataTamperSecurityStatus)
            || (typeGroup.Key == typeof(DataPowerState)) || typeGroup.Key == typeof(DataGensetOperationalState) || typeGroup.Key == typeof(DataRawTPMSMessage)
            || (typeGroup.Key == typeof(DataCNHCANAlarmCode)) || (typeGroup.Key == typeof(DataTirePressureMonitorSystem)) || typeGroup.Key == typeof(DataSwitchState) 
            || (typeGroup.Key == typeof(DataPowerMode)))
          {
            if (alertItems == null)
              alertItems = new List<INHDataObject>();

            alertItems.AddRange(FilterStartupFromPowerCutItems(items));
          }

          if (items.Count > 0)
          {
            NHDataSaverInstance.SaveViaInstance(items, out failedAndRetryable);
          }

          int excessiveRetryCount = (from wrappedItem in typeGroup
                                     where wrappedItem.StoreAttempts > MaxRetries
                                     select 1).Count();
          if (excessiveRetryCount > 0)
          {
            log.IfWarnFormat("{0} NH_DATA items have been discarded after {1} save attempts", excessiveRetryCount, NHDataSettings.Default.MaxRetries);
          }
        }
        catch (Exception ex)
        {
          log.IfError("Unexpected exception storing to NH_DATA. Data loss!", ex);
        }

        if (failedAndRetryable) break;
      }

      if (failedAndRetryable)
      {
        ((NHDataProcessor)MyProxy).ProcessSaveFailure(nhDataObjects);
      }
    }


    protected virtual void AddLocalTime(List<DataHoursLocation> locationItems)
    {
      if (timezoneTask != null)
        timezoneTask.Wait();
      foreach (DataHoursLocation dhl in locationItems.Where(wf => wf.Latitude.HasValue && wf.Longitude.HasValue))
      {
        try
        {
            DimTimeZone timezone = TimeZone.GetTimeZone(dhl.Latitude.Value, dhl.Longitude.Value, dhl.EventUTC);
            if (timezone != null)
            {
                dhl.EventDeviceTime = timezone.GetLocalTime(dhl.EventUTC);
                dhl.ifk_DimTimeZoneID = timezone.ID;
            }   
           

          // This is temp fix to solve the bug 20678 to sync EventDeviceTime and ifk_dimTimeZoneID(both should be null or not null).  This code block will be removed when we implement the Utility 20432
          if (dhl.EventDeviceTime == null && dhl.ifk_DimTimeZoneID != null)
          {
            dhl.EventDeviceTime = TimeZone.ToLocal(dhl.EventUTC, dhl.Latitude.Value, dhl.Longitude.Value);
            if (dhl.EventDeviceTime == null)
              dhl.ifk_DimTimeZoneID = null;
          }
        }
        catch (Exception e)
        {
          log.IfWarnFormat(e, "Problem obtaining local time using lat:{0};lon{1};eventUTC:{2}", dhl.Latitude.Value, dhl.Longitude.Value, dhl.EventUTC);
        }
      }
    }


    /// <summary>
    ///  Re-queues this data for another save attempt.
    /// </summary>
    protected virtual void ProcessSaveFailure(List<NHDataWrapper> nhDataObjects)
    {
      foreach (NHDataWrapper item in nhDataObjects)
      {
        item.StoreAttempts++;
      }

      NHDataProcessorClient.Store(nhDataObjects);
    }

    /// <summary>
    ///  Forwards data to the Site Determination service for processing.
    /// </summary>
    /// <param name="locationItems"></param>
    protected virtual void ForwardSiteDispatchData(List<DataHoursLocation> locationItems)
    {
      try
      {
        if (NHDataSettings.Default.ForwardToSiteDispatch)
        {
          SiteDispatchClient.ProcessLocation(locationItems);
        }
      }
      catch (Exception ex)
      {
        log.IfError("Unexpected error forwarding locations to Site Determination service", ex);
      }
    }

    /// <summary>
    ///This will check the locations for if the PL Device is in North or south america or anywhere else
    ///This is done because if the PL is in north or south america and the messages are sent through smtp then
    ///the address will need to be gpsDeviceID@OrbComm.net otherwise it is gpsDeviceID@OrbComm2.net
    /// </summary>
    /// <param name="locationItems"></param>
    protected virtual void ForwardPLLocationData(List<DataHoursLocation> locationItems)
    {
      try
      {
        // Only give the most recent location for any device that may have multiply reported during this run
        List<DataHoursLocation> distinctLocations = (from l in locationItems
                                                     where API.Device.IsProductLinkDevice(l.DeviceType)
                                                           && l.Latitude.HasValue
                                                           && l.Longitude.HasValue
                                                     group l by l.GPSDeviceID into g
                                                     select
                                                       g.OrderByDescending(t => t.EventUTC).FirstOrDefault()
                                                     ).Distinct().ToList();

        if (distinctLocations.Count > 0)
        {
          ConfigStatusSvcClient.ProcessPLLocationForOrbCommAddressChange(distinctLocations);
        }

      }
      catch (Exception ex)
      {
        log.IfError("Unexpected error forwarding PL location Data to Config svc", ex);
      }
    }

    /// <summary>
    /// Forwards data to the Alert Trigger service.
    /// </summary>
    /// <param name="alertItems"></param>
    /// <param name="locationItems"></param>
    protected virtual void ForwardAlertableItems(List<INHDataObject> alertItems, List<DataHoursLocation> locationItems)
    {
      log.IfDebugFormat("Sending alert items to alertsvc.Assetid :{0} EventUTC: {1}", alertItems[0].AssetID, alertItems[0].EventUTC);
      ((NHDataProcessor)MyProxy).AlertSvc.Send(alertItems, locationItems);
    }

    protected virtual List<DataHoursLocation> FilterLocationItems(List<INHDataObject> locationItems)
    {
      // Lend the SiteDet service a hand by providing the customer ID with the locations seeing as
      // we have a cache containing this info
      // this also removes all of the locations that have null lat and longs
      log.IfDebug("Filtering Locations Items to remove the bad locations");
      List<DataHoursLocation> locations =
        (from DataHoursLocation l in locationItems
         where l.LocIsValid && l.Latitude.HasValue && l.Longitude.HasValue
         select l).ToList();

      log.IfDebug("Finished Filtering Locations Items to remove the bad locations");
      return locations;
    }

    protected virtual IEnumerable<INHDataObject> FilterStartupFromPowerCutItems(IEnumerable<INHDataObject> items)
    {
      log.IfDebug("Starting 'Filtering Startup from Power-cut' Items");
      var retVal = items.Where(item => item is DataPowerState == false || (item as DataPowerState).IsOn == false).ToList();

      log.IfDebug("Finished 'Filtering Startup from Power-cut' Items");

      return retVal;
    }

    /// <summary>
    /// Publish all valid location items to Service bus.
    /// </summary>
    /// <param name="locationItems"></param>
    private int PublishLocationItemsToServiceBus(List<DataHoursLocation> locationItems)
    {
      if (locationItems == null || locationItems.Count == 0)
        return 0;

      int publishCount = 0;

      try
      {
        foreach (var locationItem in locationItems)
        {
          // This internally uses binary search, which would be faster than a linq join
          if (!VehicleMappingLookupCache.IsAssetIdActive(locationItem.AssetID))
          {
            log.IfDebugFormat("AssetID {0} is not found in VehicleMappingLookupCache, so its LocationEvent is not published.", locationItem.AssetID);
            continue;
          }

          ServiceBus.Publish(new LocationReceivedEvent
                                  { 
                                    Source = (int) EventSourceEnum.NhData,
                                    CreatedUtc = DateTime.UtcNow,
                                    AssetId = locationItem.AssetID,
                                    Latitude = locationItem.Latitude.Value,
                                    Longitude = locationItem.Longitude.Value,
                                    EventUtc = locationItem.EventUTC,
                                    InsertUtc = locationItem.InsertUTC
                                  }
                            ,typeof(LocationReceivedEvent));
          log.IfDebugFormat("Published the location event - AssetID:{0} Latitude:{1} Longitude:{2} EventUTC:{3} InsertUTC:{4}", locationItem.AssetID, locationItem.Latitude, locationItem.Longitude, locationItem.EventUTC, locationItem.InsertUTC);
          publishCount++;
        }

        log.IfDebugFormat("Published {0} location event messages to service bus", publishCount);
      }
      catch (Exception ex)
      {
        log.IfErrorFormat(ex, "Error publishing to service bus.");
      }

      return publishCount;
    }

    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private static NHHost<NHDataProcessor> m_host;
    private const string svcEndpointQueue = "NHData";
    private static readonly int MaxRetries = NHDataSettings.Default.MaxRetries;
    private static Timer heartbeat;
    private static int recordsProcessed;
    private static int locationItemsPublishedSinceStartup;
    private static int locationItemsProcessedSinceStartup;
    private static ISpatialTimeZoneAPI TimeZone { get; set; }

    #endregion
  }
}
