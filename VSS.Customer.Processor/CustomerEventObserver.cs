using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using MySql.Data.MySqlClient;
using org.apache.kafka.clients.consumer;
using VSS.Customer.Data.Interfaces;
using VSS.Customer.Data.Models;
using VSS.Customer.Processor.Helpers;
using VSS.UserCustomer.Data.Interfaces;
using VSS.UserCustomer.Data.Models;

namespace VSS.Customer.Processor
{
  public class CustomerEventObserver : IObserver<ConsumerRecord>
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private ICustomerService _customerService;
    private IUserCustomerService _userCustomerService;

    public CustomerEventObserver(ICustomerService customerService, IUserCustomerService userCustomerService)
    {
      _customerService = customerService;
      _userCustomerService = userCustomerService;
    }

    public void OnCompleted()
    {
      Log.Info("Completed consuming customer event messages");
    }

    public void OnError(Exception error)
    {
      Log.Error("Failed consuming customer event messages");
    }

    public void OnNext(ConsumerRecord value)
    {
      try
      {
          string val = (string)value.value();
          bool success = false;
          Log.DebugFormat("Recieved Customer Payload : {0} ", val);
          var json = JObject.Parse(val);
          string tokenName;

          JToken token;

          if ((token = json.SelectToken(tokenName = "AssociateCustomerUserEvent")) != null)
          {
            var associateCustomerUserEvent = JsonConvert.DeserializeObject<AssociateCustomerUserEvent>(token.ToString());
            Log.InfoFormat("Received a AssociateCustomerUserEvent for CustomerUid:{0} and UserUid:{1}",
              associateCustomerUserEvent.CustomerUID, associateCustomerUserEvent.UserUID);
            Log.DebugFormat("Payload :{0}", token.ToString());

            success = (_userCustomerService.StoreUserCustomer(associateCustomerUserEvent) == 1);

            Log.Info(success ? "Customer user association created successfully" : "Customer user association could not be created ");
          }
          else if ((token = json.SelectToken(tokenName = "DissociateCustomerUserEvent")) != null)
          {
            var dissociateCustomerUserEvent =
              JsonConvert.DeserializeObject<DissociateCustomerUserEvent>(token.ToString());
            Log.InfoFormat("Received a DissociateCustomerUserEvent for CustomerUid:{0} and UserUid:{1}",
              dissociateCustomerUserEvent.CustomerUID, dissociateCustomerUserEvent.UserUID);
            Log.DebugFormat("Payload :{0}", token.ToString());

            success = _userCustomerService.StoreUserCustomer(dissociateCustomerUserEvent) == 1;

            Log.Info(success ? "Customer user association removed successfully" : "Customer user association could not be removed");
          }
          else
          if ((token = json.SelectToken(tokenName = "CreateCustomerEvent")) != null)
          {
            var createCustomerEvent = JsonConvert.DeserializeObject<CreateCustomerEvent>(token.ToString());
            Log.InfoFormat("Received a CreateCustomerEvent for CustomerUid:{0} payload: {1}", createCustomerEvent.CustomerUID, token.ToString());
            var existingCustomer = _customerService.GetCustomer(createCustomerEvent.CustomerUID);
            if (existingCustomer == null) //To check whether received event is duplicate
            {
              Log.InfoFormat("No customer with CustomerUid : {0} exists, starting to Create a new Customer!", createCustomerEvent.CustomerUID);
              
              success = (_customerService.StoreCustomer(createCustomerEvent) == 1);

              Log.Info(success ? "Customer created successfully" : "Failed to create customer");
            }
            else
            {
              //Customer Type cannot be changed, If duplicate message is received with different customer type need to skip it
              if (existingCustomer.CustomerType ==
                  (CustomerType) Enum.Parse(typeof (CustomerType), createCustomerEvent.CustomerType.ToString(), true))
              {
                Log.InfoFormat("Customer already exists with CustomerUid:{0}, starting to update existing customer", createCustomerEvent.CustomerUID);

                success = _customerService.StoreCustomer(new UpdateCustomerEvent()
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
                }) == 1;
                

                Log.Info(success ? "Customer updated successfully" : "Failed to update customer");
              }
              else
              {
                Log.InfoFormat("Skipped Create Customer Event for CustomeUid:{0}", createCustomerEvent.CustomerUID);
              }
            }
          }
          else if ((token = json.SelectToken(tokenName = "UpdateCustomerEvent")) != null)
          {
            var updateCustomerEvent = JsonConvert.DeserializeObject<UpdateCustomerEvent>(token.ToString());
            Log.InfoFormat("Received a UpdateCustomerEvent for CustomerUid:{0}", updateCustomerEvent.CustomerUID);
            Log.DebugFormat("Payload :{0}", token.ToString());
            //We are tracking only CustomerName in db, skipping event if CustomerName is empty
            if (updateCustomerEvent.CustomerUID != Guid.Empty && updateCustomerEvent.CustomerName != null)
            {
              int updatedCount = _customerService.StoreCustomer(updateCustomerEvent);
              
              success = (updatedCount == 1);
               
              Log.InfoFormat(success ? String.Format("Customer updated successfully: {0} record(s) updated", updatedCount) : "Failed to update customer");
            }
            else
            {
              Log.InfoFormat("Skipped UpdateCustomerEvent since {0} cannot be empty",
                (updateCustomerEvent.CustomerUID == Guid.Empty) ? "CustomerUID" : "CustomerName");
            }
          }
          else if ((token = json.SelectToken(tokenName = "DeleteCustomerEvent")) != null)
          {
            var deleteCustomerEvent = JsonConvert.DeserializeObject<DeleteCustomerEvent>(token.ToString());
            Log.InfoFormat("Received a DeleteCustomerEvent for CustomerUid:{0}", deleteCustomerEvent.CustomerUID);
            Log.DebugFormat("Payload :{0}", token.ToString());
            
            success = (_customerService.StoreCustomer(deleteCustomerEvent) == 1);

            Log.Info(success ? "Customer deleted successfully" : "Failed to delete customer");
          }

          if (success)
          {
            if (Log.IsDebugEnabled)
              Log.Debug("Consumed " + tokenName);
          }
          else
          {
            if (Log.IsWarnEnabled)
              Log.WarnFormat("Consumed a message and was unable to find a proper token. Message Sample: {0}... ",
                val.Truncate(100));
          }
      }
      catch (MySqlException ex)
      {
        Log.Error("MySql Error  occured while Processing the Customer Payload", ex);
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
        Log.Error("Error  occured while Processing the Customer Payload", ex);
      }
    }
  }
}
