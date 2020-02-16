using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using log4net;
using Microsoft.SqlServer.Types;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Nighthawk.MasterDataSync.Models;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using CommonMDMModels = VSS.Hosted.VLCommon.Services.MDM.Models;
using System.Configuration;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  public class GeofenceSyncProcessor : SyncProcessorBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly Uri GeoFenceApiEndPointUri;
    private readonly string _taskName;
    private readonly IHttpRequestWrapper _httpRequestWrapper;
    private readonly IConfigurationManager _configurationManager;

    public GeofenceSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
      : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
    {
      _taskName = taskName;
      _httpRequestWrapper = httpRequestWrapper;
      _configurationManager = configurationManager;

      if (string.IsNullOrWhiteSpace(_configurationManager.GetAppSetting("GeofenceService.WebAPIURI")))
        throw new ArgumentNullException("Uri", "Geofence api URL value cannot be empty");

      GeoFenceApiEndPointUri = new Uri(_configurationManager.GetAppSetting("GeofenceService.WebAPIURI"));
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
      var saveLastInsertedUtcFlag = GetLastInsertUTC(_taskName) == default(DateTime).AddYears(1900);
      var isCreateEventProcessed = ProcessInsertionRecords(lastProcessedId, saveLastUpdateUtcFlag,saveLastInsertedUtcFlag, ref isServiceStopped);

      //MasterData Updation
      //lastProcessedId = GetLastProcessedId(_taskName);
      var lastUpdateUtc = GetLastUpdateUTC(_taskName);
      var isUpdateEventProcessed = ProcessUpdationRecords(lastProcessedId, lastUpdateUtc, ref isServiceStopped);

      //MasterData Migrated Users
      //lastProcessedId = GetLastProcessedId(_taskName);
      var lastMigratedUtc = GetLastInsertUTC(_taskName);
      var isMigratedEventProcessed = ProcessMigratedRecords(lastProcessedId, lastMigratedUtc, ref isServiceStopped);

      return (isCreateEventProcessed || isUpdateEventProcessed || isMigratedEventProcessed);
    }

    private bool ProcessInsertionRecords(long? lastProcessedId, bool saveLastUpdateUtcFlag, bool saveLastInsertedUtcFlag, ref bool isServiceStopped)
    {
      var currentUtc = DateTime.UtcNow;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          lastProcessedId = lastProcessedId ?? Int32.MinValue;
          Log.IfInfo(string.Format("Started Processing CreateGeofenceEvent. LastProcessedId : {0}", lastProcessedId));

          TaskState customerTaskState = (from m in opCtx.MasterDataSyncReadOnly
                                         where m.TaskName == StringConstants.CustomerTask
                                         select new TaskState() { lastProcessedId = m.LastProcessedID ?? Int32.MinValue, InsertUtc = m.LastInsertedUTC }).FirstOrDefault();

          if (customerTaskState != null)
          {
            var geofenceDataList = ReadSiteData("NH_OP", StringConstants.CreateGeofenceSPName, lastProcessedId, currentUtc, customerTaskState.lastProcessedId);

            if (!geofenceDataList.Any())
            {
              Log.IfInfo(string.Format("No {0} data left for creation", _taskName));
              return false;
            }

            var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

            if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
            {
              return true;
            }

            foreach (var geofenceData in geofenceDataList)
            {
              if (isServiceStopped)
              {
                Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
                break;
              }

              if (geofenceData.CustomerUID == null || geofenceData.CustomerId == null)
              {
                Log.IfInfo("The required CustomerId's CreateEvent has not been processed yet..");
                return true;
              }

              if (geofenceData.UserUID == null)
              {
                Log.IfInfo(string.Format("Skipping the record {0} as the UserUID value for this record is null ..", geofenceData.ID));
                lastProcessedId = geofenceData.ID;
                continue;
              }

              var createGeofence = new CreateGeofenceEvent
              {
                GeofenceName = geofenceData.GeofenceName,
                Description = geofenceData.Description,
                GeofenceType = geofenceData.GeofenceType,
                GeometryWKT = geofenceData.GeometryWKT,
                FillColor = geofenceData.Color,
                IsTransparent = geofenceData.Transparent,
                CustomerUID = geofenceData.CustomerUID,
                UserUID = geofenceData.UserUID,
                GeofenceUID = geofenceData.SiteUID,
                ActionUTC = currentUtc
              };

              var svcResponse = ProcessServiceRequestAndResponse(createGeofence, _httpRequestWrapper,
                GeoFenceApiEndPointUri, requestHeader, HttpMethod.Post);
							Log.IfInfo("Create Geofence id: " + geofenceData.ID + " returned " + svcResponse.StatusCode);
              switch (svcResponse.StatusCode)
              {
                case HttpStatusCode.OK:
                  lastProcessedId = geofenceData.ID;
                  break;
                case HttpStatusCode.Unauthorized:
                  requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                  svcResponse = ProcessServiceRequestAndResponse(createGeofence, _httpRequestWrapper, GeoFenceApiEndPointUri, requestHeader, HttpMethod.Post);
                  if (svcResponse.StatusCode == HttpStatusCode.OK)
                  {
                    lastProcessedId = geofenceData.ID;
                  }
                  break;
                case HttpStatusCode.InternalServerError:
                  Log.IfError("Internal server error");
                  return true;
                case HttpStatusCode.BadRequest:
                  Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(createGeofence));
                  lastProcessedId = geofenceData.ID;
                  break;
                case HttpStatusCode.Forbidden:
                  Log.IfError("Forbidden status code received while hitting Tpaas Geofence service");
                  break;
                default:
                  Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(createGeofence)));
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
          //Saving last inserted utc if it is not set
          if (saveLastInsertedUtcFlag)
          {
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastInsertedUTC = currentUtc;
            opCtx.SaveChanges();
          }
          if (lastProcessedId != Int32.MinValue)
          {
            //Update the last read utc to masterdatasync
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastProcessedID = lastProcessedId;
            opCtx.SaveChanges();
            Log.IfInfo(string.Format("Completed Processing CreateGeofenceEvent. LastProcessedId : {0} ", lastProcessedId)); 
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
      Log.IfInfo(string.Format("Started Processing UpdateGeofenceEvent. LastProcessedId : {0} , LastUpdatedUTC : {1}", lastProcessedId, lastUpdateUtc));
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          var currentUTC = DateTime.Now;
          var geofenceDataList = ReadSiteData("NH_OP", StringConstants.UpdateGeofenceSPName, lastProcessedId, (DateTime)lastUpdateUtc);

          if (!geofenceDataList.Any())
          {
            lastUpdateUtc = currentUTC;
            Log.IfInfo(string.Format("Current UTC : {0} Updated, No {1} data to left for updation",currentUTC, _taskName));
            return false;
          }
          var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
          {
            return true;
          }

          foreach (var geofenceData in geofenceDataList)
          {
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }

            if (geofenceData.Visible)
            {
              if (geofenceData.UserUID == null)
              {
                Log.IfInfo(string.Format("Skipping the record {0} as the UserUID value for this record is null ..", geofenceData.ID));
                lastProcessedId = geofenceData.ID;
                continue;
              }
              var updateGeofence = new UpdateGeofenceEvent
              {
                GeofenceName = geofenceData.GeofenceName,
                Description = geofenceData.Description,
                GeofenceType = geofenceData.GeofenceType,
                GeometryWKT = geofenceData.GeometryWKT,
                FillColor = geofenceData.Color,
                IsTransparent = geofenceData.Transparent,
                UserUID = geofenceData.UserUID,
                GeofenceUID = geofenceData.SiteUID,
                ActionUTC = (DateTime)geofenceData.UpdateUTC
              };

              var svcResponse = ProcessServiceRequestAndResponse(updateGeofence, _httpRequestWrapper, GeoFenceApiEndPointUri,
                requestHeader, HttpMethod.Put);
							Log.IfInfo("Update Geofence UID: " + geofenceData.SiteUID + " returned " + svcResponse.StatusCode);

              switch (svcResponse.StatusCode)
              {
                case HttpStatusCode.OK:
                  lastUpdateUtc = geofenceData.UpdateUTC;
                  break;

                case HttpStatusCode.Unauthorized:
                  requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                  svcResponse = ProcessServiceRequestAndResponse(updateGeofence, _httpRequestWrapper, GeoFenceApiEndPointUri, requestHeader, HttpMethod.Put);

                  if (svcResponse.StatusCode == HttpStatusCode.OK)
                  {
                    lastUpdateUtc = geofenceData.UpdateUTC;
                  }
                  break;
                case HttpStatusCode.InternalServerError:
                  Log.IfError("Internal server error");
                  return true;
                case HttpStatusCode.BadRequest:
                  Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(updateGeofence));
                  lastUpdateUtc = geofenceData.UpdateUTC;
                  break;
                case HttpStatusCode.Forbidden:
                  Log.IfError("Forbidden status code received while hitting Tpaas preference service");
                  break;
                default:
                  Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload = {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(updateGeofence)));
                  return true;
              }
            }
            else
            {
              var requestUri = new Uri(GeoFenceApiEndPointUri + "?geofenceUID=" + geofenceData.SiteUID + "&userUID=" +
                             geofenceData.UserUID + "&actionUTC=" + ((DateTime)geofenceData.UpdateUTC).ToString("yyyy-MM-ddThh:mm:ss"));


              var serviceRequestMessage = new CommonMDMModels.ServiceRequestMessage
              {
                RequestHeaders = requestHeader,
                RequestMethod = HttpMethod.Delete,
                RequestUrl = requestUri
              };

              var svcResponse = _httpRequestWrapper.RequestDispatcher(serviceRequestMessage);

              switch (svcResponse.StatusCode)
              {
                case HttpStatusCode.OK:
                  lastUpdateUtc = geofenceData.UpdateUTC;
                  break;

                case HttpStatusCode.Unauthorized:
                  requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);

                  serviceRequestMessage = new CommonMDMModels.ServiceRequestMessage
                     {
                       RequestHeaders = requestHeader,
                       RequestMethod = HttpMethod.Delete,
                       RequestUrl = requestUri
                     };
                  svcResponse = _httpRequestWrapper.RequestDispatcher(serviceRequestMessage);
									Log.IfInfo("Delete Geofence UID: " + geofenceData.SiteUID + " userUID:" + geofenceData.UserUID + " returned " + svcResponse.StatusCode);
                  if (svcResponse.StatusCode == HttpStatusCode.OK)
                  {
                    lastUpdateUtc = geofenceData.UpdateUTC;
                  }
                  break;
                case HttpStatusCode.InternalServerError:
                  Log.IfError("Internal server error");
                  return true;
                case HttpStatusCode.Forbidden:
                  Log.IfError("Forbidden status code received while hitting Tpaas preference service");
                  break;
                case HttpStatusCode.BadRequest:
                  Log.IfError("Error in request" + requestUri);
                  lastUpdateUtc = geofenceData.UpdateUTC;
                  break;
                default:
                  Log.IfError(string.Format("StatusCode : {0} Failed to process data. RequestURi : {1}", svcResponse.StatusCode, requestUri));
                  return true;
              }
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
          Log.IfInfo(string.Format("Completed Processing UpdateGeofenceEvent. LastProcessedId : {0} , LastUpdateUTC : {1}", lastProcessedId, lastUpdateUtc));
        }
      }
      return true;
    }

    private bool ProcessMigratedRecords(long? lastProcessedId, DateTime? lastMigratedUtc, ref bool isServiceStopped)
    {
      var currentUtc = DateTime.UtcNow;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          lastProcessedId = lastProcessedId ?? Int32.MinValue;
          Log.IfInfo(string.Format("Started Processing MigratedGeofenceEvent. LastProcessedId : {0} , LastMigratedUTC : {1}", lastProcessedId, lastMigratedUtc));
          var geofenceDataList = ReadSiteData("NH_OP", StringConstants.MigratedGeofenceSPName, lastProcessedId, (DateTime)lastMigratedUtc);

          if (!geofenceDataList.Any())
          {
            lastMigratedUtc = currentUtc;
            Log.IfInfo(string.Format("Current UTC : {0} Updated, No {1} data to left for Migration", currentUtc, _taskName));
            return false;
          }

          var requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.Contains(new KeyValuePair<string, string>(StringConstants.InvalidKey, StringConstants.InvalidValue)))
          {
            return true;
          }

          foreach (var geofenceData in geofenceDataList)
          {
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }

            var createGeofence = new CreateGeofenceEvent
            {
              GeofenceName = geofenceData.GeofenceName,
              Description = geofenceData.Description,
              GeofenceType = geofenceData.GeofenceType,
              GeometryWKT = geofenceData.GeometryWKT,
              FillColor = geofenceData.Color,
              IsTransparent = geofenceData.Transparent,
              CustomerUID = geofenceData.CustomerUID,
              UserUID = geofenceData.UserUID,
              GeofenceUID = geofenceData.SiteUID,
              ActionUTC = currentUtc
            };

            var svcResponse = ProcessServiceRequestAndResponse(createGeofence, _httpRequestWrapper,
              GeoFenceApiEndPointUri, requestHeader, HttpMethod.Post);
						Log.IfInfo("Migrate Create Geofence uid: " + geofenceData.SiteUID + " returned " + svcResponse.StatusCode);
            switch (svcResponse.StatusCode)
            {
              case HttpStatusCode.OK:
                lastMigratedUtc = geofenceData.MigratedUTC;
                break;
              case HttpStatusCode.Unauthorized:
                requestHeader = GetRequestHeaderListWithAuthenticationType(isOuthRetryCall: true);
                svcResponse = ProcessServiceRequestAndResponse(createGeofence, _httpRequestWrapper, GeoFenceApiEndPointUri, requestHeader, HttpMethod.Post);
                if (svcResponse.StatusCode == HttpStatusCode.OK)
                {
                  lastMigratedUtc = geofenceData.MigratedUTC;
                }
                break;
              case HttpStatusCode.InternalServerError:
                Log.IfError("Internal server error");
                return true;
              case HttpStatusCode.BadRequest:
                Log.IfError("Error in payload " + JsonHelper.SerializeObjectToJson(createGeofence));
                lastMigratedUtc = geofenceData.MigratedUTC;
                break;
              case HttpStatusCode.Forbidden:
                Log.IfError("Forbidden status code received while hitting Tpaas Geofence service");
                break;
              default:
                Log.IfError(string.Format("StatusCode : {0} Failed to process data. Payload : {1}", svcResponse.StatusCode, JsonHelper.SerializeObjectToJson(createGeofence)));
                return true;
            }
          }
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} Insertion {1} \n {2}", _taskName, e.Message, e.StackTrace));
        }
        finally
        {
            //Update the last read utc to masterdatasync
          opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastInsertedUTC = lastMigratedUtc;
          opCtx.SaveChanges();
          Log.IfInfo(string.Format("Completed Processing MigratedGeofenceEvent. LastMigratedUtc : {0} ", lastMigratedUtc));
        }
      }
      return true;
    }

    private IEnumerable<GeofenceData> ReadSiteData(string dbname, string spName, long? lastProcessedId, DateTime lastUpdateUTC, long? customerLastProcessedId = null)
    {
      var sp = new StoredProcDefinition(dbname, spName);
      sp.AddInput("LastProcessedId", lastProcessedId);
      sp.AddInput("LastUpdateUTC", lastUpdateUTC,true); // bool has been added to improve accuracy by passing as DateTime2
      sp.AddInput("BatchSize", BatchSize);

      if (string.Equals(spName, StringConstants.CreateGeofenceSPName))
      {
        sp.AddInput("CustomerLastProcessedId", customerLastProcessedId);
      }

      var sites = SqlReaderAccess.Read(sp, reader => new GeofenceData
      {
        ID = SqlReaderAccess.GetLong(reader, "ID"),
        GeofenceName = SqlReaderAccess.GetString(reader, "Name"),
        Description = SqlReaderAccess.GetString(reader, "Description"),
        Visible = SqlReaderAccess.GetBool(reader, "Visible"),
        GeometryWKT = (reader["PolygonST"] == DBNull.Value) ? null : GetGeometryWKT(reader),
        GeofenceType = SqlReaderAccess.GetString(reader, "SiteType"),
        Color = SqlReaderAccess.GetNullableInt(reader, "Colour"),
        SiteUID = SqlReaderAccess.GetGuid(reader, "SiteUID"),
        Transparent = SqlReaderAccess.GetBool(reader, "Transparent"),
        CustomerId = SqlReaderAccess.GetNullableLong(reader, "CustomerId"),
        CustomerUID = SqlReaderAccess.GetNullableGuid(reader, "CustomerUID"),
        UserUID = (SqlReaderAccess.GetString(reader, "UserUID") != null) ? Guid.Parse(SqlReaderAccess.GetString(reader, "UserUID")) : (Guid?)null,
        UpdateUTC = SqlReaderAccess.GetNullableDateTime(reader, "UpdateUTC"),
        MigratedUTC = (spName == StringConstants.MigratedGeofenceSPName) ? SqlReaderAccess.GetNullableDateTime(reader, "IdentityMigrationUTC") : (DateTime?)null
      }).Where(e => e.GeometryWKT != null).ToList();

      return sites;
    }

    public List<Point> GetPointsFromGeometry(SqlDataReader reader)
    {
      var geoPoints = new List<Point>();
      var geometry = ((SqlGeometry)reader["PolygonST"]);
      var geoPointsCount = geometry.STNumPoints();
      for (int i = 1; i <= geoPointsCount; i++)
      {
        var geometryPoint = geometry.STPointN(i);
        geoPoints.Add(new Point((Double)geometryPoint.STY, (Double)geometryPoint.STX));
      }
      return geoPoints;
    }

    public string GetGeometryWKT(SqlDataReader reader)
    {
      var points = GetPointsFromGeometry(reader);
      if (points.Count == 0)
        return null;

      var polygonWkt = new StringBuilder("POLYGON((");
      foreach (var point in points)
      {
        polygonWkt.Append(String.Format("{0} {1},", point.x, point.y));
      }
      polygonWkt.Append(String.Format("{0} {1}))", points[0].x, points[0].y));
      return polygonWkt.ToString();

    }
  }


  internal class GeofenceData
  {
    public long? ID { get; set; }
    public string GeofenceName { get; set; }
    public string Description { get; set; }
    public string GeometryWKT { get; set; }
    public bool Visible { get; set; }
    public string GeofenceType { get; set; }
    public int? Color { get; set; }
    public bool Transparent { get; set; }
    public Guid SiteUID { get; set; }
    public long? CustomerId { get; set; }
    public Guid? CustomerUID { get; set; }
    public Guid? UserUID { get; set; }
    public DateTime? UpdateUTC { get; set; }
    public DateTime? MigratedUTC { get; set; }
  }
}
