using System;
using System.Reflection;
using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Kafka.DotNetClient.Model;
using VSS.Project.Data.Interfaces;
using VSS.Project.Data.Models;
using VSS.Project.Processor.Helpers;

namespace VSS.Project.Processor
{
  public class ProjectEventObserver : IObserver<ConsumerInstanceResponse>
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private IProjectService _ProjectService;

    public ProjectEventObserver(IProjectService ProjectService)
    {
      _ProjectService = ProjectService;
    }

    public void OnCompleted()
    {
      Log.Info("Completed consuming subcscription event messages");
    }

    public void OnError(Exception error)
    {
      Log.DebugFormat("Failed consuming subcscription event messages: {0} ", error.ToString());
    }

    public void OnNext(ConsumerInstanceResponse value)
    {
      Log.Debug("ProjectEventObserver.OnNext()");
      try
      {
        var payload = value.Messages.Payload;
        foreach (var binaryMessage in payload)
        {
          string val = binaryMessage.Value;
          bool success = false;
          Log.DebugFormat("Recieved Project Payload : {0} ", val);
          var json = JObject.Parse(val);
          string tokenName;

          JToken token;
          if ((token = json.SelectToken(tokenName = "CreateProjectEvent")) != null)
          {
            Log.Debug(String.Format("Recieved Create Project Payload : {0} ", token.ToString()));
            var createProjectEvent =
              JsonConvert.DeserializeObject<CreateProjectEvent>(token.ToString());
            int updatedCount = _ProjectService.StoreProject(createProjectEvent);
            success = (updatedCount == 1);
          }
          else if ((token = json.SelectToken(tokenName = "UpdateProjectEvent")) != null)
          {
            Log.Debug(String.Format("Recieved Update Project Payload : {0} ", token.ToString()));
            var updateProjectEvent =
              JsonConvert.DeserializeObject<UpdateProjectEvent>(token.ToString());
            int updatedCount = _ProjectService.StoreProject(updateProjectEvent);
            success = (updatedCount == 1);
          }
          else if ((token = json.SelectToken(tokenName = "DeleteProjectEvent")) != null)
          {
            Log.Debug(String.Format("Recieved Delete Project Payload : {0} ", token.ToString()));
            var deleteProjectEvent =
              JsonConvert.DeserializeObject<DeleteProjectEvent>(token.ToString());
            int updatedCount = _ProjectService.StoreProject(deleteProjectEvent);
            success = (updatedCount == 1);
          }
          else if ((token = json.SelectToken(tokenName = "AssociateProjectCustomerEvent")) != null)
          {
            Log.Debug(String.Format("Recieved Associate Project-Customer Payload : {0} ", token.ToString()));
            var associateProjectCustomerEvent =
              JsonConvert.DeserializeObject<AssociateProjectCustomer>(token.ToString());
            int updatedCount = _ProjectService.StoreProject(associateProjectCustomerEvent);
            success = (updatedCount == 1);
          }
          else if ((token = json.SelectToken(tokenName = "DissociateProjectCustomerEvent")) != null)
          {
            Log.Debug(String.Format("Recieved Update Project Payload : {0} ", token.ToString()));
            var dissociateProjectCustomerEvent =
              JsonConvert.DeserializeObject<DissociateProjectCustomer>(token.ToString());
            int updatedCount = _ProjectService.StoreProject(dissociateProjectCustomerEvent);
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
      }
      catch (MySqlException ex)
      {
        Log.Error("MySql Error  occured while Processing the Project Payload", ex);
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
        Log.Error("Error  occured while Processing the Project Payload", ex);
      }
      value.Commit();
    }
  }
}
