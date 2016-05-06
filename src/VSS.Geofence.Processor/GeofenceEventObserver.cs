using System;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using org.apache.kafka.clients.consumer;
using VSS.Geofence.Data.Interfaces;
using VSS.Geofence.Data.Models;
using VSS.Geofence.Processor.Helpers;
using MySql.Data.MySqlClient;

namespace VSS.Geofence.Processor
{
  public class GeofenceEventObserver : IObserver<ConsumerRecord>
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private IGeofenceService _GeofenceService;

    public GeofenceEventObserver(IGeofenceService GeofenceService)
    {
      _GeofenceService = GeofenceService;
    }

    public void OnCompleted()
    {
      Log.Info("Completed consuming geofence event messages");
    }

    public void OnError(Exception error)
    {
      Log.DebugFormat("Failed consuming geofence event messages: {0} ", error.ToString());
    }

    public void OnNext(ConsumerRecord value)
    {
      Log.Debug("GeofenceEventObserver.OnNext()");
      try
      {
        string val = (string)value.value();

          bool success = false;
          Log.DebugFormat("Received Geofence Payload : {0} ", val);
          var json = JObject.Parse(val);
          string tokenName;

          JToken token;
          if ((token = json.SelectToken(tokenName = "CreateGeofenceEvent")) != null)
          {
            Log.Debug(String.Format("Received Create Geofence Payload : {0} ", token.ToString()));
            var createGeofenceEvent =
              JsonConvert.DeserializeObject<CreateGeofenceEvent>(token.ToString());
            int updatedCount = _GeofenceService.StoreGeofence(createGeofenceEvent);
            success = (updatedCount == 1);
          }
          else if ((token = json.SelectToken(tokenName = "UpdateGeofenceEvent")) != null)
          {
            Log.Debug(String.Format("Received Update Geofence Payload : {0} ", token.ToString()));
            var updateGeofenceEvent =
              JsonConvert.DeserializeObject<UpdateGeofenceEvent>(token.ToString());
            int updatedCount = _GeofenceService.StoreGeofence(updateGeofenceEvent);
            success = (updatedCount == 1);
          }
          else if ((token = json.SelectToken(tokenName = "DeleteGeofenceEvent")) != null)
          {
            Log.Debug(String.Format("Received Delete Geofence Payload : {0} ", token.ToString()));
            var deleteGeofenceEvent =
              JsonConvert.DeserializeObject<DeleteGeofenceEvent>(token.ToString());
            int updatedCount = _GeofenceService.StoreGeofence(deleteGeofenceEvent);
            success = (updatedCount == 1);
          }

          if (success)
          {
            if (Log.IsDebugEnabled)
              Log.Debug("Consumed " + tokenName);
          }
          else
          {
            if (Log.IsWarnEnabled)
              Log.WarnFormat("Consumed a message but discarded as not relavant {0}... ", val.Truncate(30));
          }

      }
      catch (MySqlException ex)
      {
        Log.Error("MySql Error  occured while Processing the Geofence Payload", ex);
        switch (ex.Number)
        {
          case 0: //Cannot connect to server
          case 1045: //Invalid user name and/or password
            throw;
          default:
            //todo: log exception and payload here
            break;
        }
      }
      catch (Exception ex)
      {
        //deliberately supppress
        Log.Error("Error  occured while Processing the Geofence Payload", ex);
      }
    }
  }
}
