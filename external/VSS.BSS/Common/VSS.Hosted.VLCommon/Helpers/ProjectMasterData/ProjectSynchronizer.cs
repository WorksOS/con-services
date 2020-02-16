using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using log4net;
using RestSharp;
using VSS.Hosted.VLCommon;

namespace ThreeDAPIs.ProjectMasterData
{
  public class ProjectSynchronizer
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    private static readonly string projectMasterDataBaseUrl = ConfigurationManager.AppSettings["ProjectMasterDataUrl"];
    private static readonly string projectMasterDataTimeout = ConfigurationManager.AppSettings["ProjectMasterDataTimeout"];

    public bool SyncCreateProject(long id, Guid uid, int startKeyDate, int endKeyDate, string name, string timezone, 
      ProjectTypeEnum projectType, List<Point> points, DateTime actionUTC, out string errorMessage)
    {
      string boundary = points.Aggregate(string.Empty, (current, point) => string.Format("{0}{1},{2};", current, point.Latitude, point.Longitude));
      //Remove trailing ;
      boundary = boundary.Substring(0, boundary.Length - 1);
      CreateProjectEvent evt = new CreateProjectEvent
                          {
                            ProjectID = id,
                            ProjectUID = uid,
                            ProjectStartDate = startKeyDate.FromKeyDate(),
                            ProjectEndDate = endKeyDate.FromKeyDate(),
                            ProjectName = name,
                            ProjectTimezone = timezone,
                            ProjectType = (int)projectType,
                            ProjectBoundary = boundary,
                            ActionUTC = actionUTC,
                            ReceivedUTC = actionUTC //Web service will set when it receives this event
                          };
      return ExecuteRequest(Method.POST, "/v1", evt, out errorMessage);
    }

    public bool SyncAssignProjectToCustomer(Guid projectUid, Guid customerUid, long customerID, DateTime actionUTC, out string errorMessage)
    {
      AssociateProjectCustomer evt = new AssociateProjectCustomer
                                {
                                    ProjectUID = projectUid,
                                    CustomerUID = customerUid,
                                    LegacyCustomerID = customerID,
                                    RelationType = (int)RelationType.Owner,
                                    ActionUTC = actionUTC,
                                    ReceivedUTC = actionUTC
                                };
      return ExecuteRequest(Method.POST, "/v2/AssociateCustomer", evt, out errorMessage);
    }

    public bool SyncUpdateProject(Guid uid, string name, int endKeyDate, string timezone, int projectType, 
      DateTime actionUTC, out string errorMessage)
    {
      UpdateProjectEvent evt = new UpdateProjectEvent
                          {
                            ProjectUID = uid,
                            ProjectName = name,
                            ProjectEndDate = endKeyDate.FromKeyDate(),
                            ProjectTimezone = timezone,
                            ProjectType = projectType,
                            ActionUTC = actionUTC,
                            ReceivedUTC = actionUTC
                          };
      return ExecuteRequest(Method.PUT, "/v1", evt, out errorMessage);
    }


    public bool SyncDeleteProject(Guid projectUid, DateTime actionUTC, out string errorMessage)
    {
      //aka Archive Project
      DeleteProjectEvent evt = new DeleteProjectEvent
                               {
                                   ProjectUID = projectUid,
                                   ActionUTC = actionUTC,
                                   ReceivedUTC = actionUTC
                               };
      return ExecuteRequest(Method.DELETE, "/v1", evt, out errorMessage);
    }

    public bool SyncRestoreProject(Guid projectUid, DateTime actionUTC, out string errorMessage)
    {
      //aka Undelete Project
      RestoreProjectEvent evt = new RestoreProjectEvent
      {
        ProjectUID = projectUid,
        ActionUTC = actionUTC,
        ReceivedUTC = actionUTC
      };
      return ExecuteRequest(Method.POST, "/v1/Restore", evt, out errorMessage);
    }

    public bool SyncAssignSiteToProject(Guid projectUid, Guid siteUid, DateTime actionUTC, out string errorMessage)
    {
      AssociateProjectGeofence evt = new AssociateProjectGeofence
      {
        ProjectUID = projectUid,
        GeofenceUID = siteUid,
        ActionUTC = actionUTC,
        ReceivedUTC = actionUTC
      };
      return ExecuteRequest(Method.POST, "/v1/AssociateGeofence", evt, out errorMessage);
    }

    private bool ExecuteRequest(Method method, string contractPath, object requestData, out string errorMessage)
    {
      errorMessage = null;
      if (string.IsNullOrEmpty(projectMasterDataBaseUrl))
      {
        errorMessage = "Configuration Error - no project master data url specified";
        log.IfWarn(errorMessage);
        return false;
        //throw new Exception("Configuration Error - no project master data url specified");
      }

      int timeout;
      if (!int.TryParse(projectMasterDataTimeout, out timeout))
      {
        log.IfInfo("Missing or invalid project master data timeout, using default 3 mins");
        timeout = 180000;
      }

      RestClient client = new RestClient(projectMasterDataBaseUrl);
      client.Timeout = timeout;
      client.ReadWriteTimeout = timeout;
      RestRequest request = new RestRequest(contractPath, method);
      request.RequestFormat = DataFormat.Json;
      request.AddHeader("Accept", "application/json");

      var properties = from p in requestData.GetType().GetProperties()
                       where p.GetValue(requestData) != null
                       select new { p.Name, Value = p.GetValue(requestData) };
      foreach (var p in properties)
      {
        if (method == Method.GET || method == Method.DELETE)
          request.AddQueryParameter(p.Name, p.Value.ToString());
        else
          request.AddParameter(p.Name, p.Value, ParameterType.GetOrPost);
      }

      IRestResponse response = null;

      var reqTask = client.ExecuteTaskAsync(request);
      if (!reqTask.Wait(timeout))
      {
        errorMessage = string.Format("ProjectSynchronizer ran out of time for completion for request {0}", contractPath);
        log.IfWarn(errorMessage);
        return false;
      }

      response = reqTask.Result;

      bool success = response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == HttpStatusCode.OK;
      if (success)
      {
        log.IfDebugFormat("Response Status: StatusCode={0}, StatusDescription={1}",
            response.StatusCode, response.StatusDescription);
      }
      else
      {
        errorMessage = string.Format("Project Master Data Web API request failed: ResponseStatus={0}, StatusCode={1}, StatusDescription={2}, ErrorMessage={3}",
          response.ResponseStatus, response.StatusCode, response.StatusDescription, response.ErrorMessage);
        log.IfWarn(errorMessage);
      }
      return success;
    }

  }
}
