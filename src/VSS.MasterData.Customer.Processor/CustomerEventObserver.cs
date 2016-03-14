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
        var payload = value.Messages.Payload;
        foreach (var binaryMessage in payload)
        {
          string val = binaryMessage.Value;
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
              Log.InfoFormat("No customer with CustomerUid : {0} exists, starting to Create a new Customer!",
                createCustomerEvent.CustomerUID);
              _customerService.CreateCustomer(new CreateCustomerEvent()
              {
                ActionUTC = createCustomerEvent.ActionUTC,
                BSSID = createCustomerEvent.BSSID,
                CustomerName = createCustomerEvent.CustomerName,
                CustomerType =
                  (CustomerType) Enum.Parse(typeof (CustomerType), createCustomerEvent.CustomerType.ToString(), true),
                CustomerUID = createCustomerEvent.CustomerUID,
                DealerAccountCode = createCustomerEvent.DealerAccountCode,
                DealerNetwork = createCustomerEvent.DealerNetwork,
                NetworkCustomerCode = createCustomerEvent.NetworkCustomerCode,
                NetworkDealerCode = createCustomerEvent.NetworkDealerCode,
                ReceivedUTC = createCustomerEvent.ReceivedUTC
              });
              Log.Info("Customer created successfully");
              success = true;
            }
            else
            {
              //Customer Type cannot be changed, If duplicate message is received with different customer type need to skip it
              if (existingCustomer.fk_CustomerTypeID ==
                  (CustomerType) Enum.Parse(typeof (CustomerType), createCustomerEvent.CustomerType.ToString(), true))
              {
                Log.InfoFormat("Customer already exists with CustomerUid:{0}, starting to update existing customer",
                  createCustomerEvent.CustomerUID);
                _customerService.UpdateCustomer(new UpdateCustomerEvent()
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
                success = true;
              }
              else
              {
                Log.InfoFormat("Skipped Create Customer Event for CustomeUid:{0}", createCustomerEvent.CustomerUID);
                success = false;
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
              Log.InfoFormat("Customer updated successfully {0} records updated",
                _customerService.UpdateCustomer(new UpdateCustomerEvent()
                {
                  ActionUTC = updateCustomerEvent.ActionUTC,
                  BSSID = updateCustomerEvent.BSSID,
                  CustomerName = updateCustomerEvent.CustomerName,
                  CustomerUID = updateCustomerEvent.CustomerUID,
                  DealerAccountCode = updateCustomerEvent.DealerAccountCode,
                  DealerNetwork = updateCustomerEvent.DealerNetwork,
                  NetworkCustomerCode = updateCustomerEvent.NetworkCustomerCode,
                  NetworkDealerCode = updateCustomerEvent.NetworkDealerCode,
                  ReceivedUTC = updateCustomerEvent.ReceivedUTC
                }));
              success = true;
            }
            else
            {
              Log.InfoFormat("Skipped UpdateCustomerEvent since {0} cannot be empty",
                (updateCustomerEvent.CustomerUID == Guid.Empty) ? "CustomerUID" : "CustomerName");
              success = false;
            }
          }
          else if ((token = json.SelectToken(tokenName = "DeleteCustomerEvent")) != null)
          {
            var deleteCustomerEvent = JsonConvert.DeserializeObject<DeleteCustomerEvent>(token.ToString());
            Log.InfoFormat("Received a DeleteCustomerEvent for CustomerUid:{0}", deleteCustomerEvent.CustomerUID);
            Log.DebugFormat("Payload :{0}", token.ToString());
            _customerService.DeleteCustomer(new DeleteCustomerEvent()
            {
              ActionUTC = deleteCustomerEvent.ActionUTC,
              CustomerUID = deleteCustomerEvent.CustomerUID,
              ReceivedUTC = deleteCustomerEvent.ReceivedUTC
            });
            Log.Info("Customer deleted successfully");
            success = true;
          }
          else if ((token = json.SelectToken(tokenName = "AssociateCustomerUserEvent")) != null)
          {
            var associateCustomerUserEvent = JsonConvert.DeserializeObject<AssociateCustomerUserEvent>(token.ToString());
            Log.InfoFormat("Received a AssociateCustomerUserEvent for CustomerUid:{0} and UserUid:{1}",
              associateCustomerUserEvent.CustomerUID, associateCustomerUserEvent.UserUID);
            Log.DebugFormat("Payload :{0}", token.ToString());
            if (_customerService.AssociateCustomerUser(new AssociateCustomerUserEvent()
            {
              ActionUTC = associateCustomerUserEvent.ActionUTC,
              CustomerUID = associateCustomerUserEvent.CustomerUID,
              ReceivedUTC = associateCustomerUserEvent.ReceivedUTC,
              UserUID = associateCustomerUserEvent.UserUID
            }))
            {
              Log.Info("Customer user association created successfully");
              success = true;
            }
            else
            {
              Log.Info("Customer user association could not be created ");
              success = false;
            }
          }
          else if ((token = json.SelectToken(tokenName = "DissociateCustomerUserEvent")) != null)
          {
            var dissociateCustomerUserEvent =
              JsonConvert.DeserializeObject<DissociateCustomerUserEvent>(token.ToString());
            Log.InfoFormat("Received a DissociateCustomerUserEvent for CustomerUid:{0} and UserUid:{1}",
              dissociateCustomerUserEvent.CustomerUID, dissociateCustomerUserEvent.UserUID);
            Log.DebugFormat("Payload :{0}", token.ToString());
            _customerService.DissociateCustomerUser(new DissociateCustomerUserEvent()
            {
              ActionUTC = dissociateCustomerUserEvent.ActionUTC,
              CustomerUID = dissociateCustomerUserEvent.CustomerUID,
              ReceivedUTC = dissociateCustomerUserEvent.ReceivedUTC,
              UserUID = dissociateCustomerUserEvent.UserUID
            });
            Log.Info("Customer user association removed successfully");
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
              Log.WarnFormat("Consumed a message and was unable to find a proper token. Message Sample: {0}... ",
                val.Truncate(15));
          }
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
