//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Net;
//using System.Threading.Tasks;
//using Newtonsoft.Json;
//using TestUtility.Model.WebApi;
//using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
//using VSS.VisionLink.Interfaces.Events.MasterData.Models;

//namespace TestUtility
//{
//  public class WebApiHelper
//  {
//    private readonly TestConfig appConfig = new TestConfig();

//    public CreateProjectEvent CreateProjectEvt { get; set; }
//    public UpdateProjectEvent UpdateProjectEvt { get; set; }
//    public DeleteProjectEvent DeleteProjectEvt { get; set; }
//    public AssociateProjectCustomer AssociateCustomerProjectEvt { get; set; }
//    public DissociateProjectCustomer DissociateCustomerProjectEvt { get; set; }
//    public AssociateProjectGeofence AssociateProjectGeofenceEvt { get; set; }

//    /// <summary>
//    /// Create the project via the web api. 
//    /// </summary>
//    /// <param name="projectUid">project UID</param>
//    /// <param name="projectId">legacy project id</param>
//    /// <param name="name">project name</param>
//    /// <param name="startDate">project start date</param>
//    /// <param name="endDate">project end date</param>
//    /// <param name="projectType">project type</param>
//    /// <param name="timezone">project time zone</param>
//    /// <param name="actionUtc">timestamp of the event</param>
//    /// <param name="statusCode">expected status code from web api call</param>
//    public void CreateProjectViaWebApi(Guid projectUid, int projectId, string name, DateTime startDate, DateTime endDate,
//      string timezone, ProjectType projectType, DateTime actionUtc, HttpStatusCode statusCode)
//    {
//      CreateProjectEvt = new CreateProjectEvent
//      {
//        ProjectID = projectId,
//        ProjectUID = projectUid,
//        ProjectName = name,
//        ProjectType = projectType,
//        ProjectBoundary = null,//not used
//        ProjectStartDate = startDate,
//        ProjectEndDate = endDate,
//        ProjectTimezone = timezone,
//        ActionUTC = actionUtc
//      };
//      CallProjectWebApi(CreateProjectEvt, string.Empty, statusCode, "Create");
//    }

//    /// <summary>
//    /// Update the project via the web api. 
//    /// </summary>
//    /// <param name="projectUid">project UID</param>
//    /// <param name="name">project name</param>
//    /// <param name="endDate">project end date</param>
//    /// <param name="timezone">project time zone</param>
//    /// <param name="actionUtc">timestamp of the event</param>
//    /// <param name="statusCode">expected status code from web api call</param>
//    /// <param name="projectType">Type of project - Standard, LandFill ProjectMonitoring</param>
//    public void UpdateProjectViaWebApi(Guid projectUid, string name, DateTime endDate, string timezone, DateTime actionUtc, HttpStatusCode statusCode, ProjectType projectType = ProjectType.Standard)
//    {
//      UpdateProjectEvt = new UpdateProjectEvent
//      {
//        ProjectUID = projectUid,
//        ProjectName = name,
//        ProjectType = projectType,
//        ProjectEndDate = endDate,
//        ProjectTimezone = timezone,
//        ActionUTC = actionUtc
//      };
//      CallProjectWebApi(UpdateProjectEvt, string.Empty, statusCode, "Update", "PUT");
//    }

//    /// <summary>
//    /// Delete the project via the web api. 
//    /// </summary>
//    /// <param name="projectUid">project UID</param>
//    /// <param name="actionUtc">timestamp of the event</param>
//    /// <param name="statusCode">expected status code from web api call</param>
//    public void DeleteProjectViaWebApi(Guid projectUid, DateTime actionUtc, HttpStatusCode statusCode)
//    {
//      DeleteProjectEvt = new DeleteProjectEvent
//      {
//        ProjectUID = projectUid,
//        ActionUTC = actionUtc
//      };
//      CallProjectWebApi(DeleteProjectEvt, string.Empty, statusCode, "Delete", "DELETE");
//    }

//    /// <summary>
//    /// Associate a customer and project via the web api. 
//    /// </summary>
//    /// <param name="projectUid">project UID</param>
//    /// <param name="customerUid">customer UID</param>
//    /// <param name="customerId">legacy customer ID</param>
//    /// <param name="actionUtc">timestamp of the event</param>
//    /// <param name="statusCode">expected status code from web api call</param>
//    public void AssociateCustomerProjectViaWebApi(Guid projectUid, Guid customerUid, int customerId, DateTime actionUtc, HttpStatusCode statusCode)
//    {
//      AssociateCustomerProjectEvt = new AssociateProjectCustomer
//      {
//        ProjectUID = projectUid,
//        CustomerUID = customerUid,
//        LegacyCustomerID = customerId,
//        RelationType = RelationType.Customer,
//        ActionUTC = actionUtc
//      };
//      CallProjectWebApi(AssociateCustomerProjectEvt, "AssociateCustomer", statusCode, "Associate customer");
//    }

//    /// <summary>
//    /// Dissociate a customer and project via the web api. 
//    /// </summary>
//    /// <param name="projectUid">project UID</param>
//    /// <param name="customerUid">customer UID</param>
//    /// <param name="actionUtc">timestamp of the event</param>
//    /// <param name="statusCode">expected status code from web api call</param>
//    public void DissociateProjectViaWebApi(Guid projectUid, Guid customerUid, DateTime actionUtc, HttpStatusCode statusCode)
//    {
//      DissociateCustomerProjectEvt = new DissociateProjectCustomer
//      {
//        ProjectUID = projectUid,
//        CustomerUID = customerUid,
//        ActionUTC = actionUtc
//      };
//      CallProjectWebApi(DissociateCustomerProjectEvt, "DissociateCustomer", statusCode, "Dissociate customer");
//    }

//    /// <summary>
//    /// Associate a geofence and project via the web api. 
//    /// </summary>
//    /// <param name="projectUid">project UID</param>
//    /// <param name="geofenceUid">geofence UID</param>
//    /// <param name="actionUtc">timestamp of the event</param>
//    /// <param name="statusCode">expected status code from web api call</param>
//    public void AssociateGeofenceProjectViaWebApi(Guid projectUid, Guid geofenceUid, DateTime actionUtc, HttpStatusCode statusCode)
//    {
//      AssociateProjectGeofenceEvt = new AssociateProjectGeofence
//      {
//        ProjectUID = projectUid,
//        GeofenceUID = geofenceUid,
//        ActionUTC = actionUtc
//      };
//      CallProjectWebApi(AssociateProjectGeofenceEvt, "AssociateGeofence", statusCode, "Associate geofence");
//    }

//    public void GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode statusCode, Guid customerUid, string[] expectedResultsArray)
//    {
//      var response = CallProjectWebApi(null, null, statusCode, "Get", "GET", customerUid == Guid.Empty ? null : customerUid.ToString());
//      if (statusCode == HttpStatusCode.OK)
//      {
//      //  var actualProjects = JsonConvert.DeserializeObject<List<ProjectDescriptor>>(response).OrderBy(p => p.ProjectUid).ToList();
//        //var expectedProjects =
//        //  ConvertArrayToList<ProjectDescriptor>(expectedResultsArray).OrderBy(p => p.ProjectUid).ToList();
//        //msg.DisplayResults("Expected projects :" + JsonConvert.SerializeObject(expectedProjects),
//        //  "Actual from WebApi: " + response);
//        //CollectionAssert.AreEqual(expectedProjects, actualProjects);
//      }
//    }

//    /// <summary>
//    /// Call the project web api
//    /// </summary>
//    /// <param name="evt">THe project event containing the data</param>
//    /// <param name="routeSuffix">suffix to add to base uri if required</param>
//    /// <param name="statusCode">expected return code of the web api call</param>
//    /// <param name="what">name of the api being called for logging</param>
//    /// <param name="method">http method</param>
//    /// <param name="customerUid">Customer UID to add to http headers</param>
//    /// <returns>The web api response</returns>
//    //private string CallProjectWebApi(IProjectEvent evt, string routeSuffix, HttpStatusCode statusCode, string what, string method = "POST", string customerUid = null)
//    //{
//    //  var configJson = evt == null ? null : JsonConvert.SerializeObject(evt, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
//    //  var restClient = new RestClient();
//    //  var response = restClient.DoHttpRequest(GetBaseUri() + routeSuffix, method, "application/json", configJson, statusCode, customerUid);
//    //  Console.WriteLine(what + " project response:" + response);
//    //  return response;
//    //}

//    /// <summary>
//    /// Check if the test is being debugged in VS. Set to different endpoind
//    /// </summary>
//    /// <returns></returns>
//    public string GetBaseUri()
//    {
//      var baseUri = appConfig.webApiUri;
//      if (Debugger.IsAttached)
//      {
//        baseUri = appConfig.debugWebApiUri;
//      }
//      return baseUri;
//    }
//  }
//}
