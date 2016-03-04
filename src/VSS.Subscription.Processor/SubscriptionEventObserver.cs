using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSP.MasterData.Common.Logging;
using VSS.Messaging.Kafka.Interfaces;
using VSS.Subscription.Data.Models;
using VSS.Subscription.Model.Interfaces;
using VSS.Subscription.Processor.Helpers;
using VSS.Kafka.DotNetClient.Interfaces;
using VSS.Kafka.DotNetClient.Model;
using System.Threading.Tasks;
using VSS.Kafka.DotNetClient.Common;

namespace VSS.Subscription.Processor
{
    public class SubscriptionEventObserver :  IObserver<ConsumerInstanceResponse>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ISubscriptionService _subscriptionService;

        public SubscriptionEventObserver(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        public void OnCompleted()
        {
            Log.IfInfo("Completed consuming subcscription event messages");
        }

        public void OnError(Exception error)
        {
            Log.IfError("Failed consuming subcscription event messages");
        }

        public void OnNext(ConsumerInstanceResponse value)
        {
            try
            {
                var payload = value.Messages.Payload;
                foreach (var binaryMessage in payload)
                {
                    string val = binaryMessage.Value;
                    bool success = false;
                    var json = JObject.Parse(val);
                    string tokenName;

                    JToken token;
                    if ((token = json.SelectToken(tokenName = "CreateAssetSubscriptionEvent")) != null)
                    {
                        Log.Debug(String.Format("Recieved Create Asset Subscription Payload : {0} ", token.ToString()));
                        var createAssetSubscriptionEvent =
                          JsonConvert.DeserializeObject<CreateAssetSubscriptionEvent>(token.ToString());
                        _subscriptionService.CreateAssetSubscription(createAssetSubscriptionEvent);
                        success = true;
                        //todo: figure out what to do with success - should i throw an exception on failure? or just log?
                    }
                    else if ((token = json.SelectToken(tokenName = "UpdateAssetSubscriptionEvent")) != null)
                    {
                        Log.Debug(String.Format("Recieved Update Asset Subscription Payload : {0} ", token.ToString()));
                        var updateAssetSubscriptionEvent =
                          JsonConvert.DeserializeObject<UpdateAssetSubscriptionEvent>(token.ToString());
                        _subscriptionService.UpdateAssetSubscription(updateAssetSubscriptionEvent);
                        success = true;
                    }
                    else if ((token = json.SelectToken(tokenName = "CreateProjectSubscriptionEvent")) != null)
                    {
                        Log.Debug(String.Format("Recieved Create Project Subscription Payload : {0} ", token.ToString()));
                        var createProjectSubscriptionEvent =
                          JsonConvert.DeserializeObject<CreateProjectSubscriptionEvent>(token.ToString());
                        _subscriptionService.CreateProjectSubscription(createProjectSubscriptionEvent);
                        success = true;
                    }
                    else if ((token = json.SelectToken(tokenName = "UpdateProjectSubscriptionEvent")) != null)
                    {
                        Log.Debug(String.Format("Recieved Update Project Subscription Payload : {0} ", token.ToString()));
                        var updateProjectSubscriptionEvent =
                          JsonConvert.DeserializeObject<UpdateProjectSubscriptionEvent>(token.ToString());
                        _subscriptionService.UpdateProjectSubscription(updateProjectSubscriptionEvent);
                        success = true;
                    }
                    else if ((token = json.SelectToken(tokenName = "AssociateProjectSubscriptionEvent")) != null)
                    {
                        Log.Debug(String.Format("Recieved Associate Project Subscription Payload : {0} ", token.ToString()));
                        var associateProjectSubscriptionEvent =
                          JsonConvert.DeserializeObject<AssociateProjectSubscriptionEvent>(token.ToString());
                        _subscriptionService.AssociateProjectSubscription(associateProjectSubscriptionEvent);
                        success = true;
                    }
                    else if ((token = json.SelectToken(tokenName = "DissociateProjectSubscriptionEvent")) != null)
                    {
                        Log.Debug(String.Format("Recieved Dissociate Project Subscription Payload : {0} ", token.ToString()));
                        var dissociateProjectSubscriptionEvent =
                          JsonConvert.DeserializeObject<DissociateProjectSubscriptionEvent>(token.ToString());
                        _subscriptionService.DissociateProjectSubscription(dissociateProjectSubscriptionEvent);
                        success = true;
                    }
                    else if ((token = json.SelectToken(tokenName = "CreateCustomerSubscriptionEvent")) != null)
                    {
                        Log.Debug(String.Format("Recieved Create Customer Subscription Payload : {0} ", token.ToString()));
                        var createCustomerSubscriptionEvent =
                          JsonConvert.DeserializeObject<CreateCustomerSubscriptionEvent>(token.ToString());
                        _subscriptionService.CreateCustomerSubscription(createCustomerSubscriptionEvent);
                        success = true;
                    }
                    else if ((token = json.SelectToken(tokenName = "UpdateCustomerSubscriptionEvent")) != null)
                    {
                        Log.Debug(String.Format("Recieved Update Customer Subscription Payload : {0} ", token.ToString()));
                        var updateCustomerSubscriptionEvent =
                          JsonConvert.DeserializeObject<UpdateCustomerSubscriptionEvent>(token.ToString());
                        _subscriptionService.UpdateCustomerSubscription(updateCustomerSubscriptionEvent);
                        success = true;
                    }

                    if (success)
                    {
                        if (Log.IsDebugEnabled)
                            Log.Debug("Consumed " + tokenName);
                    }
                    else
                    {
                        if (Log.IsWarnEnabled)
                            Log.WarnFormat("Consumed a message and was unable to find a proper token. Message Sample: {0}... ", val.Truncate(15));
                    }
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
            value.Commit();
        }
    }
}
