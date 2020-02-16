using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Nighthawk.MasterDataSync.Models;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM;
using System.Collections.Generic;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  public class WorkDefinitionSyncProcessor : SyncProcessorBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private Uri WorkDefinitionApiEndPointUri { get; set; }
    private readonly string _taskName;
    private readonly IHttpRequestWrapper _httpRequestWrapper;
    private readonly IConfigurationManager _configurationManager;

    public WorkDefinitionSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
      : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
    {
      _taskName = taskName;
      _httpRequestWrapper = httpRequestWrapper;
      _configurationManager = configurationManager;

      if (string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("AssetService.WebAPIURI")))
        throw new ArgumentNullException("Uri", "AssetService api URL value cannot be empty");

      WorkDefinitionApiEndPointUri = new Uri(_configurationManager.GetAppSetting("AssetService.WebAPIURI") + "/workdefinition");
    }

    public override bool Process(ref bool isServiceStopped)
    {
      bool isDataProcessed = false;
      if (LockTaskState(_taskName, _taskTimeOutInterval))
      {
        isDataProcessed = ProcessSync(ref isServiceStopped);
        UnLockTaskState(_taskName);
      }
      return isDataProcessed;
    }

    public override bool ProcessSync(ref bool isServiceStopped)
    {
      //MasterData Insertion
      var lastProcessedId = GetLastProcessedId(_taskName);
      var saveLastUpdateUtcFlag = GetLastUpdateUTC(_taskName) == null;
      var isCreateEventProcessed = ProcessInsertionRecords(lastProcessedId, saveLastUpdateUtcFlag, ref isServiceStopped);

      //MasterData Updation
      //lastProcessedId = GetLastProcessedId(_taskName);
      var lastUpdateUtc = GetLastUpdateUTC(_taskName);
      var isUpdateEventProcessed = ProcessUpdationRecords(lastProcessedId, lastUpdateUtc, ref isServiceStopped);
      return (isCreateEventProcessed || isUpdateEventProcessed);
    }

    private bool ProcessInsertionRecords(long? lastProcessedId, bool saveLastUpdateUtcFlag, ref bool isServiceStopped)
    {
      var currentUtc = DateTime.UtcNow;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          lastProcessedId = lastProcessedId ?? Int32.MinValue;
          Log.IfInfo(string.Format("Started Processing CreateWorkDefinitionEvent. LastProcessedId : {0}", lastProcessedId));

          TaskState assetTaskState = (from m in opCtx.MasterDataSyncReadOnly
                                      where m.TaskName == StringConstants.AssetTask
                                      select new TaskState() { lastProcessedId = m.LastProcessedID ?? Int32.MinValue, InsertUtc = m.LastInsertedUTC}).FirstOrDefault();

          if (assetTaskState != null)
          {
            assetTaskState.InsertUtc = assetTaskState.InsertUtc ?? default(DateTime).AddYears(1900);

            var createWorkdefinitionDataList = (from aw in opCtx.AssetWorkingDefinitionReadOnly
              join w in opCtx.WorkDefinitionReadOnly on aw.fk_WorkDefinitionID equals w.ID
              join a in opCtx.AssetReadOnly.Where(e => e.InsertUTC < assetTaskState.InsertUtc || (e.InsertUTC == assetTaskState.InsertUtc && e.AssetID <= assetTaskState.lastProcessedId))
                .OrderBy(e => e.InsertUTC).ThenBy(e => e.AssetID) on aw.fk_AssetID equals a.AssetID into assetSubset
              from at in assetSubset.DefaultIfEmpty()
              join d in opCtx.DeviceReadOnly on at.fk_DeviceID equals d.ID into deviceSubset
              from ds in deviceSubset.DefaultIfEmpty()
              where aw.ID > lastProcessedId && aw.UpdateUTC <= currentUtc
              orderby aw.ID ascending
              select new
              {
                aw.ID,
                AssetID = (long?)at.AssetID,
                at.AssetUID,
                w.Description,
                ds.OwnerBSSID,
                aw.SensorNumber,
                aw.SensorStartIsOn,
                UpdateUTC = currentUtc
              }).Take(BatchSize).ToList();

            if (createWorkdefinitionDataList.Count < 1)
            {
              Log.IfInfo(string.Format("No {0} data left for creation", _taskName));
              return false;
            }

            var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

            if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
            {
              return true;
            }

            foreach (var createWorkDefintionData in createWorkdefinitionDataList)
            {
              if (isServiceStopped)
              {
                Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
                break;
              }

              if (createWorkDefintionData.AssetUID == null || createWorkDefintionData.AssetID == null)
              {
                Log.IfInfo("The required AssetUID's CreateEvent has not been processed yet..");
                return true;
              }

              var createWorkDefinition = new CreateWorkDefinitionEvent
              {
                AssetUID = (Guid)createWorkDefintionData.AssetUID,
                WorkDefinitionType = createWorkDefintionData.Description,
                SensorNumber = createWorkDefintionData.SensorNumber,
                StartIsOn = createWorkDefintionData.SensorStartIsOn,
                ActionUTC = createWorkDefintionData.UpdateUTC
              };

              var svcResponse = ProcessServiceRequestAndResponse(createWorkDefinition, _httpRequestWrapper,
                WorkDefinitionApiEndPointUri, requestHeader, HttpMethod.Post);

              switch (svcResponse.StatusCode)
              {
                case HttpStatusCode.OK:
                  lastProcessedId = createWorkDefintionData.ID;
                  break;
                case HttpStatusCode.Unauthorized:
                  requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                  svcResponse = ProcessServiceRequestAndResponse(createWorkDefinition, _httpRequestWrapper, WorkDefinitionApiEndPointUri, requestHeader, HttpMethod.Post);
                  if (svcResponse.StatusCode == HttpStatusCode.OK)
                  {
                    lastProcessedId = createWorkDefintionData.ID;
                  }
                  break;
                case HttpStatusCode.InternalServerError:
                  Log.IfError("Internal server error");
                  return true;
                case HttpStatusCode.BadRequest:
                  Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(createWorkDefinition));
                  lastProcessedId = createWorkDefintionData.ID;
                  break;
                case HttpStatusCode.Forbidden:
                  Log.IfError("Forbidden status code received while hitting Tpaas WorkDefinition service");
                  break;
                default:
                  Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(createWorkDefinition)));
                  return true;
              }
            }
          }
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} Insertion {1} \n {2}", _taskName, e.Message, e.StackTrace));
        }
        finally
        {
          //Saving last update utc if it is not set
          if (saveLastUpdateUtcFlag)
          {
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastUpdatedUTC = currentUtc;
            opCtx.SaveChanges();
          }
          if (lastProcessedId != Int32.MinValue)
          {
            //Update the last read utc to masterdatasync
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastProcessedID = lastProcessedId;
            opCtx.SaveChanges();
            Log.IfInfo(string.Format("Completed Processing CreateWorkDefinitionEvent. LastProcessedId : {0} ", lastProcessedId)); 
          }
          else
          {
            Log.IfInfo(string.Format("No Records Processed "));
          }
        }
      }
      return true;
    }

    private bool ProcessUpdationRecords(long? lastProcessedId, DateTime? lastUpdateUtc, ref bool isServiceStopped)
    {
      Log.IfInfo(string.Format("Started Processing UpdateWorkDefinitionEvent. LastProcessedId : {0} , LastUpdatedUTC : {1}", lastProcessedId, lastUpdateUtc));
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          var currentUtc = DateTime.UtcNow;
          var workDefinitionDataList = (from aw in opCtx.AssetWorkingDefinitionReadOnly
                                        join w in opCtx.WorkDefinitionReadOnly on aw.fk_WorkDefinitionID equals w.ID
                                        join a in opCtx.AssetReadOnly on aw.fk_AssetID equals a.AssetID
                                        join d in opCtx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                                        where aw.ID <= lastProcessedId && aw.UpdateUTC <= currentUtc && aw.UpdateUTC > lastUpdateUtc
                                        orderby aw.UpdateUTC, aw.ID
                                        select new
                                        {
                                          aw.ID,
                                          a.AssetUID,
                                          a.AssetID,
                                          w.Description,
                                          aw.SensorNumber,
                                          aw.SensorStartIsOn,
                                          aw.UpdateUTC
                                        }).Take(BatchSize).ToList();


          if (workDefinitionDataList.Count < 1)
          {
            lastUpdateUtc = currentUtc;
            Log.IfInfo(string.Format("Current UTC : {0} Updated, No {1} data to left for Updation", currentUtc, _taskName));
            return false;
          }

          var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
          {
            return true;
          }

          foreach (var updateWorkDefinitionData in workDefinitionDataList)
          {
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }
            var updateWorkDefinition = new UpdateWorkDefinitionEvent
                    {
                      AssetUID = (Guid)updateWorkDefinitionData.AssetUID,
                      WorkDefinitionType = updateWorkDefinitionData.Description,
                      SensorNumber = updateWorkDefinitionData.SensorNumber,
                      StartIsOn = updateWorkDefinitionData.SensorStartIsOn,
                      ActionUTC = updateWorkDefinitionData.UpdateUTC
                    };

            var svcResponse = ProcessServiceRequestAndResponse(updateWorkDefinition, _httpRequestWrapper, WorkDefinitionApiEndPointUri, requestHeader, HttpMethod.Put);

            switch (svcResponse.StatusCode)
            {
              case HttpStatusCode.OK:
                lastUpdateUtc = updateWorkDefinitionData.UpdateUTC;
                break;

              case HttpStatusCode.Unauthorized:
                requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                svcResponse = ProcessServiceRequestAndResponse(updateWorkDefinition, _httpRequestWrapper, WorkDefinitionApiEndPointUri, requestHeader, HttpMethod.Put);
                if (svcResponse.StatusCode == HttpStatusCode.OK)
                {
                  lastUpdateUtc = updateWorkDefinitionData.UpdateUTC;
                }
                break;
              case HttpStatusCode.InternalServerError:
                Log.IfError("Internal server error");
                return true;
              case HttpStatusCode.BadRequest:
                Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(updateWorkDefinition));
                lastUpdateUtc = updateWorkDefinitionData.UpdateUTC;
                break;
              case HttpStatusCode.Forbidden:
                Log.IfError("Forbidden status code received while hitting Tpaas preference service");
                break;
              default:
                Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(updateWorkDefinition)));
                return true;
            }
          }
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} updation {1} \n {2}", _taskName, e.Message, e.StackTrace));
        }
        finally
        {
          //Update the last read utc to masterdatasync
          opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastUpdatedUTC = lastUpdateUtc;
          opCtx.SaveChanges();
          Log.IfInfo(string.Format("Completed Processing UpdateWorkDefinitionEvent. LastProcessedId : {0} , LastUpdateUTC : {1}", lastProcessedId, lastUpdateUtc));
        }
      }
      return true;
    }
  }
}
