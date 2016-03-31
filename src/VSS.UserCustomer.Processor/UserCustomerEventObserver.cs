using System;
using System.Reflection;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSP.MasterData.Common.Logging;
using VSS.UserCustomer.Data.Interfaces;
using VSS.UserCustomer.Data.Models;
using VSS.UserCustomer.Processor.Helpers;
using VSS.Kafka.DotNetClient.Model;

namespace VSS.UserCustomer.Processor
{
  public class UserCustomerEventObserver : IObserver<ConsumerInstanceResponse>
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private IUserCustomerService _userCustomerService;

    public UserCustomerEventObserver(IUserCustomerService userCustomerService)
    {
      _userCustomerService = userCustomerService;
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
