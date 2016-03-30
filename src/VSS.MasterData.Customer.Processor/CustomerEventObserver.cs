using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using MySql.Data.MySqlClient;
using VSP.MasterData.Common.Logging;
using VSS.Customer.Data.Interfaces;
using VSS.Customer.Data.Models;
using VSS.Customer.Processor.Helpers;
using VSS.Kafka.DotNetClient.Model;

namespace VSS.Customer.Processor
{
  public class CustomerEventObserver : IObserver<ConsumerInstanceResponse>
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private ICustomerService _customerService;

    public CustomerEventObserver(ICustomerService customerService)
    {
      _customerService = customerService;
    }

    public void OnCompleted()
    {
      Log.IfInfo("Completed consuming customer event messages");
    }

    public void OnError(Exception error)
    {
      Log.IfError("Failed consuming customer event messages");
    }

    public void OnNext(ConsumerInstanceResponse value)
    {
      try
      {
          string val = value.ToString();
          bool success = false;
          Log.DebugFormat("Recieved Customer Payload : {0} ", val);
          var json = JObject.Parse(val);
          string tokenName;

          JToken token;
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
              if (existingCustomer.fk_CustomerTypeID ==
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
               
              Log.InfoFormat(success ? String.Format("Customer updated successfully {0} records updated", updatedCount) : "Failed to update customer");
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
                val.Truncate(15));
          }
      }
      catch (MySqlException ex)
      {
        Log.Error("MySql Error  occured while Processing the Subscription Payload", ex);
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
        Log.Error("Error  occured while Processing the Subscription Payload", ex);
      }
      value.Commit();
    }
  }
}
