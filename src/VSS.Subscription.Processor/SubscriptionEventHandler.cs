using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSP.MasterData.Common.KafkaWrapper.Interfaces;
using VSP.MasterData.Common.KafkaWrapper.Models;
using VSS.Subscription.Data.Models;
using VSS.Subscription.Model.Interfaces;

namespace VSS.Subscription.Processor
{
    public class SubscriptionEventHandler : ISubscriber<KafkaMessage>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ISubscriptionService _subscriptionService;

        public SubscriptionEventHandler(ISubscriptionService subscriptionService)
		{
            _subscriptionService = subscriptionService;
		}

		public void Handle(KafkaMessage message)
		{
			string val = message.Value;
			bool success = false;
			var json = JObject.Parse(val);
            
			try
			{
				JToken token;
                if ((token = json.SelectToken("CreateAssetSubscriptionEvent")) != null)
                {
                    Log.Debug(String.Format("Recieved Create Asset Subscription Payload : {0} ", token.ToString()));
                    var createAssetSubscriptionEvent = JsonConvert.DeserializeObject<CreateAssetSubscriptionEvent>(token.ToString());
                    _subscriptionService.CreateAssetSubscription(createAssetSubscriptionEvent);
                    success = true;
                    //todo: figure out what to do with success - should i throw an exception on failure? or just log?
                }
                else if ((token = json.SelectToken("UpdateAssetSubscriptionEvent")) != null)
                {
                    Log.Debug(String.Format("Recieved Update Asset Subscription Payload : {0} ", token.ToString()));
                    var updateAssetSubscriptionEvent = JsonConvert.DeserializeObject<UpdateAssetSubscriptionEvent>(token.ToString());
                    _subscriptionService.UpdateAssetSubscription(updateAssetSubscriptionEvent);
                    success = true;
                }
                else if ((token = json.SelectToken("CreateProjectSubscriptionEvent")) != null)
                {
                    Log.Debug(String.Format("Recieved Create Project Subscription Payload : {0} ", token.ToString()));
                    var createProjectSubscriptionEvent = JsonConvert.DeserializeObject<CreateProjectSubscriptionEvent>(token.ToString());
                    _subscriptionService.CreateProjectSubscription(createProjectSubscriptionEvent);
                    success = true;
                }
                else if ((token = json.SelectToken("UpdateProjectSubscriptionEvent")) != null)
                {
                    Log.Debug(String.Format("Recieved Update Project Subscription Payload : {0} ", token.ToString()));
                    var updateProjectSubscriptionEvent = JsonConvert.DeserializeObject<UpdateProjectSubscriptionEvent>(token.ToString());
                    _subscriptionService.UpdateProjectSubscription(updateProjectSubscriptionEvent);
                    success = true;
                }
                else if ((token = json.SelectToken("AssociateProjectSubscriptionEvent")) != null)
                {
                    Log.Debug(String.Format("Recieved Associate Project Subscription Payload : {0} ", token.ToString()));
                    var associateProjectSubscriptionEvent = JsonConvert.DeserializeObject<AssociateProjectSubscriptionEvent>(token.ToString());
                    _subscriptionService.AssociateProjectSubscription(associateProjectSubscriptionEvent);
                    success = true;
                }
                else if ((token = json.SelectToken("DissociateProjectSubscriptionEvent")) != null)
                {
                    Log.Debug(String.Format("Recieved Dissociate Project Subscription Payload : {0} ", token.ToString()));
                    var dissociateProjectSubscriptionEvent = JsonConvert.DeserializeObject<DissociateProjectSubscriptionEvent>(token.ToString());
                    _subscriptionService.DissociateProjectSubscription(dissociateProjectSubscriptionEvent);
                    success = true;
                }
                else if ((token = json.SelectToken("CreateCustomerSubscriptionEvent")) != null)
                {
                    Log.Debug(String.Format("Recieved Create Customer Subscription Payload : {0} ", token.ToString()));
                    var createCustomerSubscriptionEvent = JsonConvert.DeserializeObject<CreateCustomerSubscriptionEvent>(token.ToString());
                    _subscriptionService.CreateCustomerSubscription(createCustomerSubscriptionEvent);
                    success = true;
                }
                else if ((token = json.SelectToken("UpdateCustomerSubscriptionEvent")) != null)
                {
                    Log.Debug(String.Format("Recieved Update Customer Subscription Payload : {0} ", token.ToString()));
                    var updateCustomerSubscriptionEvent = JsonConvert.DeserializeObject<UpdateCustomerSubscriptionEvent>(token.ToString());
                    _subscriptionService.UpdateCustomerSubscription(updateCustomerSubscriptionEvent);
                    success = true;
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
