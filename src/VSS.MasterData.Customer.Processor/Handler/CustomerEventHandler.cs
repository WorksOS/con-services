using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using VSP.MasterData.Common.KafkaWrapper.Interfaces;
using VSP.MasterData.Common.KafkaWrapper.Models;
using VSP.MasterData.Customer.Data;
using VSP.MasterData.Customer.Data.Models;

namespace VSS.MasterData.Customer.Processor.Handler
{
    public class CustomerEventHandler : ISubscriber<KafkaMessage>
    {
        private ICustomerDataService _customerDataService;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public CustomerEventHandler(ICustomerDataService customerDataService)
        {
            _customerDataService = customerDataService;
        }

        public void Handle(KafkaMessage message)
        {
            string val = message.Value;
            var json = JObject.Parse(val);
            try
            {
                JToken token;
                if ((token = json.SelectToken("CreateCustomerEvent")) != null)
                {
                    var createCustomerEvent = JsonConvert.DeserializeObject<CreateCustomerEvent>(token.ToString());
                    Log.InfoFormat("Received a CreateCustomerEvent for CustomerUid:{0}", createCustomerEvent.CustomerUID);
                    Log.DebugFormat("Payload :{0}", token.ToString());
                    var existingCustomer = _customerDataService.GetCustomer(createCustomerEvent.CustomerUID);
                    if (existingCustomer == null) //To check whether received event is duplicate
                    {
                        Log.InfoFormat("No customer with CustomerUid : {0} exists, starting to Create a new Customer!", createCustomerEvent.CustomerUID);
                        _customerDataService.CreateCustomer(createCustomerEvent);
                        Log.Info("Customer created successfully");
                    }
                    else
                    {
                        //Customer Type cannot be changed, If duplicate message is received with different customer type need to skip it
                        if (existingCustomer.fk_CustomerTypeID == createCustomerEvent.CustomerType)
                        {
                            Log.InfoFormat("Customer already exists with CustomerUid:{0}, starting to update existing customer", createCustomerEvent.CustomerUID);
                            _customerDataService.UpdateCustomer(new UpdateCustomerEvent()
                            {
                                ActionUTC = createCustomerEvent.ActionUTC,
                                BSSID = createCustomerEvent.BSSID,
                                CustomerName = createCustomerEvent.CustomerName,
                                CustomerUID = createCustomerEvent.CustomerUID,
                                DealerAccountCode = createCustomerEvent.DealerAccountCode,
                                DealerNetwork = createCustomerEvent.DealerNetwork,
                                NetworkCustomerCode = createCustomerEvent.NetworkCustomerCode,
                                NetworkDealerCode = createCustomerEvent.NetworkDealerCode,
                                ReceivedUTC = createCustomerEvent.ReceivedUTC
                            });
                            Log.Info("Customer updated successfully");
                        }
                        else
                            Log.InfoFormat("Skipped Create Customer Event for CustomeUid:{0}", createCustomerEvent.CustomerUID);
                    }
                }
                else if ((token = json.SelectToken("UpdateCustomerEvent")) != null)
                {
                    var updateCustomerEvent = JsonConvert.DeserializeObject<UpdateCustomerEvent>(token.ToString());
                    Log.InfoFormat("Received a UpdateCustomerEvent for CustomerUid:{0}", updateCustomerEvent.CustomerUID);
                    Log.DebugFormat("Payload :{0}", token.ToString());
                    //We are tracking only CustomerName in db, skipping event if CustomerName is empty
                    if (updateCustomerEvent.CustomerUID != Guid.Empty && updateCustomerEvent.CustomerName != null)
                    {                        
                        Log.InfoFormat("Customer updated successfully {0} records updated",_customerDataService.UpdateCustomer(updateCustomerEvent));
                    }
                    else
                        Log.InfoFormat("Skipped UpdateCustomerEvent since {0} cannot be empty", (updateCustomerEvent.CustomerUID == Guid.Empty) ? "CustomerUID" : "CustomerName");
                }
                else if ((token = json.SelectToken("DeleteCustomerEvent")) != null)
                {
                    var deleteCustomerEvent = JsonConvert.DeserializeObject<DeleteCustomerEvent>(token.ToString());
                    Log.InfoFormat("Received a DeleteCustomerEvent for CustomerUid:{0}", deleteCustomerEvent.CustomerUID);
                    Log.DebugFormat("Payload :{0}", token.ToString());
                    _customerDataService.DeleteCustomer(deleteCustomerEvent);
                    Log.Info("Customer deleted successfully");
                }
                else if ((token = json.SelectToken("AssociateCustomerUserEvent")) != null)
                {
                    var associateCustomerUserEvent = JsonConvert.DeserializeObject<AssociateCustomerUserEvent>(token.ToString());
                    Log.InfoFormat("Received a AssociateCustomerUserEvent for CustomerUid:{0} and UserUid:{1}", associateCustomerUserEvent.CustomerUID, associateCustomerUserEvent.UserUID);
                    Log.DebugFormat("Payload :{0}", token.ToString());
                    if (_customerDataService.AssociateCustomerUser(associateCustomerUserEvent))
                    {
                        Log.Info("Customer user association created successfully");
                    }
                    else
                    {
                        Log.Info("Customer user association could not be created ");
                    }
                }
                else if ((token = json.SelectToken("DissociateCustomerUserEvent")) != null)
                {
                    var dissociateCustomerUserEvent = JsonConvert.DeserializeObject<DissociateCustomerUserEvent>(token.ToString());
                    Log.InfoFormat("Received a DissociateCustomerUserEvent for CustomerUid:{0} and UserUid:{1}", dissociateCustomerUserEvent.CustomerUID, dissociateCustomerUserEvent.UserUID);
                    Log.DebugFormat("Payload :{0}", token.ToString());
                    _customerDataService.DissociateCustomerUser(dissociateCustomerUserEvent);
                    Log.Info("Customer user association removed successfully");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception has occured", ex);
            }

        }
    }
}
