using System;
using System.Reflection;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using org.apache.kafka.clients.consumer;
using VSS.Geofence.Data.Interfaces;
using VSS.Landfill.Common.Helpers;
using VSS.Subscription.Data.Interfaces;
using VSS.Project.Data.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Subscription.Processor
{
  public class SubscriptionEventObserver : IObserver<ConsumerRecord>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ISubscriptionService _subscriptionService;
        private IProjectService _projectService;
        private IGeofenceService _geofenceService;

        public SubscriptionEventObserver(ISubscriptionService subscriptionService, IProjectService projectService, IGeofenceService geofenceService)
        {
            _subscriptionService = subscriptionService;
            _projectService = projectService;
            _geofenceService = geofenceService;
        }

        public void OnCompleted()
        {
            Log.Info("Completed consuming subcscription event messages");
        }

        public void OnError(Exception error)
        {
            Log.DebugFormat("Failed consuming subcscription event messages: {0} ", error.ToString());
        }

        public void OnNext(ConsumerRecord value)
        {
          Log.Debug("SubscriptionEventObserver.OnNext()");
            try
            {
                  string val = (string)value.value();
                  bool success = false;
                  Log.DebugFormat("Received Subscription Payload : {0} ", val);
                  var json = JObject.Parse(val);
                  string tokenName;

                  JToken token;
                  if ((token = json.SelectToken(tokenName = "CreateProjectSubscriptionEvent")) != null)
                  {
                    Log.Debug(String.Format("Received Create Project Subscription Payload : {0} ", token.ToString()));
                      
                    var createProjectSubscriptionEvent = JsonConvert.DeserializeObject<CreateProjectSubscriptionEvent>(token.ToString());
                      
                    Log.DebugFormat("Project subscription {0}", createProjectSubscriptionEvent );

                    success = _subscriptionService.StoreSubscription(createProjectSubscriptionEvent, _projectService, _geofenceService) == 1;

                    Log.Info(success ? "Project Subscription created successfully" : "Failed to create Project Subscription");
                  }
                  else if ((token = json.SelectToken(tokenName = "UpdateProjectSubscriptionEvent")) != null)
                  {
                    Log.Debug(String.Format("Received Update Project Subscription Payload : {0} ", token.ToString()));

                    var updateProjectSubscriptionEvent = JsonConvert.DeserializeObject<UpdateProjectSubscriptionEvent>(token.ToString());

                    int updatedCount = _subscriptionService.StoreSubscription(updateProjectSubscriptionEvent, _projectService, _geofenceService);

                    success = (updatedCount == 1);

                    Log.InfoFormat(success ? String.Format("Project Subscription updated successfully: {0} record(s) updated", updatedCount) : "Failed to update Project Subscription");
                  }
                  else if ((token = json.SelectToken(tokenName = "AssociateProjectSubscriptionEvent")) != null)
                  {
                    Log.Debug(String.Format("Received Associate Project Subscription Payload : {0} ", token.ToString()));

                    var associateProjectSubscriptionEvent = JsonConvert.DeserializeObject<AssociateProjectSubscriptionEvent>(token.ToString());

                    success = _subscriptionService.StoreSubscription(associateProjectSubscriptionEvent, _projectService, _geofenceService) == 1;

                    Log.Info(success ? "Project Subscription was associated successfully" : "Failed to associate Project Subscription");
                  }
                  else if ((token = json.SelectToken(tokenName = "DissociateProjectSubscriptionEvent")) != null)
                  {
                    Log.Debug(String.Format("Received Dissociate Project Subscription Payload : {0} ", token.ToString()));

                    var dissociateProjectSubscriptionEvent = JsonConvert.DeserializeObject<DissociateProjectSubscriptionEvent>(token.ToString());

                    success = _subscriptionService.StoreSubscription(dissociateProjectSubscriptionEvent, _projectService, _geofenceService) == 1;

                    Log.Info(success ? "Project Subscription was dissociated successfully" : "Failed to dissociate Project Subscription");
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
                Log.Error("MySql Error  occured while Processing the Subscription Payload", ex);
                switch (ex.Number)
                {
                    case 0: //Cannot connect to server
                    case 1045:	//Invalid user name and/or password
                        throw;
                    default:
                        //todo: log exception and payload here
                        break;
                }
            }
            catch (Exception ex)
            {
                //deliberately supppress
                Log.Error("Error  occured while Processing the Subscription Payload", ex);
            }
        }
    }
}
