using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility.Model.WebApi;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TestUtility
{
  public class TestSupport
  {
    #region Public Properties

    public string AssetUid { get; set; }
    public DateTime FirstEventDate { get; set; }
    public DateTime LastEventDate { get; set; }
    public Customer MockCustomer { get; set; }
    public Subscription MockSubscription { get; set; }
    public ProjectSubscription MockProjectSubscription { get; set; }
    public Guid ProjectUid { get; set; }
    public Guid CustomerUid { get; set; }
    public Guid GeofenceUid { get; set; }
    public Guid SubscriptionUid { get; set; }
    public string CustomerId { get; set; }
    public CreateProjectEvent CreateProjectEvt { get; set; }
    public UpdateProjectEvent UpdateProjectEvt { get; set; }
    public DeleteProjectEvent DeleteProjectEvt { get; set; }
    public AssociateProjectCustomer AssociateCustomerProjectEvt { get; set; }
    public DissociateProjectCustomer DissociateCustomerProjectEvt { get; set; }
    public AssociateProjectGeofence AssociateProjectGeofenceEvt { get; set; }

    public bool IsPublishToKafka { get; set; }
    public bool IsPublishToWebApi { get; set; }

    public readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
      DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
      NullValueHandling = NullValueHandling.Ignore
    };

    public readonly TestConfig TsCfg = new TestConfig();

    #endregion

    #region Private Properties

    private readonly Random rndNumber = new Random();
    private readonly object syncLock = new object();
    private const char SEPARATOR = '|';
    private readonly Msg msg = new Msg();

    #endregion

    #region Public Methods

    public TestSupport()
    {
      SetFirstEventDate();
      SetLastEventDate();
      SetAssetUid();
      SetProjectUid();
      SetCustomerUid();
      SetGeofenceUid();
      SetSubscriptionUid();
    }

    /// <summary>
    /// Set the legacy asset id
    /// </summary>
    /// <returns>get the maximum legacy asset id plus 1</returns>
    public long SetLegacyAssetId()
    {
      var mysql = new MySqlHelper();
      var query = "SELECT max(LegacyAssetID) FROM Asset;";
      var result = mysql.ExecuteMySqlQueryAndReturnRecordCountResult(TsCfg.DbConnectionString, query);
      if (string.IsNullOrEmpty(result))
      {
        return 1000;
      }
      var legacyAssetId = Convert.ToInt64(result);
      return legacyAssetId + 1;
    }

    /// <summary>
    /// Set the legacy project id from the database
    /// </summary>
    /// <returns></returns>
    public int SetLegacyProjectId()
    {
      Console.WriteLine("SetLegacyProjectId start");
      var mysql = new MySqlHelper();
      var query = "SELECT max(LegacyProjectID) FROM Project WHERE LegacyProjectID < 100000;";
      var result = mysql.ExecuteMySqlQueryAndReturnRecordCountResult(TsCfg.DbConnectionString, query);
      if (string.IsNullOrEmpty(result))
      {
        return 1000;
      }
      var legacyProjectId = Convert.ToInt32(result);
      Console.WriteLine("legacyProjectId=" + legacyProjectId);
      return legacyProjectId + 1;
    }

    /// <summary>
    /// Set up the first event date for the events to go in. Also used as project start date for project tests.
    /// </summary>
    public void SetFirstEventDate()
    {
      FirstEventDate = DateTime.SpecifyKind(DateTime.Today.AddDays(-RandomNumber(10, 360)), DateTimeKind.Unspecified);
    }

    /// <summary>
    /// Set up the last event date for the events to go in. Also used as project end date for project tests.
    /// </summary>
    public void SetLastEventDate()
    {
      LastEventDate = FirstEventDate.AddYears(2);
    }

    /// <summary>
    /// Set the asset UID to a random GUID
    /// </summary>
    public void SetAssetUid()
    {
      AssetUid = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Set the project UID to a random GUID
    /// </summary>
    public void SetProjectUid()
    {
      ProjectUid = Guid.NewGuid();
    }

    /// <summary>
    /// Set the customer UID to a random GUID
    /// </summary>
    public void SetCustomerUid()
    {
      CustomerUid = Guid.NewGuid();
    }

    /// <summary>
    /// Set the geofence UID to a random GUID
    /// </summary>
    public void SetGeofenceUid()
    {
      GeofenceUid = Guid.NewGuid();
    }

    /// <summary>
    /// Set the subscription UID to a random GUID
    /// </summary>
    public void SetSubscriptionUid()
    {
      SubscriptionUid = Guid.NewGuid();
    }

    /// <summary>
    /// Set to true if writing to kafka instead of the database
    /// </summary>
    /// <param name="iAmPublishingToKafka">true or false</param>
    public void SetPublishToKafka(bool iAmPublishingToKafka)
    {
      IsPublishToKafka = iAmPublishingToKafka;
    }

    /// <summary>
    /// Publish events to kafka from string array
    /// </summary>
    /// <param name="eventArray">string array with all the events we are going to publish</param>
    public void PublishEventCollection(string[] eventArray)
    {
      try
      {
        if (IsPublishToWebApi)
        { msg.DisplayEventsToConsoleWeb(eventArray); }
        else if (IsPublishToKafka)
        { msg.DisplayEventsToConsoleKafka(eventArray); }
        else
        { msg.DisplayEventsForDbInjectToConsole(eventArray); }

        var allColumnNames = eventArray.ElementAt(0).Split(SEPARATOR);
        var kafkaDriver = new RdKafkaDriver();
        for (var rowCnt = 1; rowCnt <= eventArray.Length - 1; rowCnt++)
        {
          var eventRow = eventArray.ElementAt(rowCnt).Split(SEPARATOR);
          dynamic dynEvt = ConvertToExpando(allColumnNames, eventRow);
          var eventDate = dynEvt.EventDate;
          LastEventDate = eventDate;
          if (IsPublishToKafka || IsPublishToWebApi)
          {
            var jsonString = BuildEventIntoObject(dynEvt);
            var topicName = SetTheKafkaTopicFromTheEvent(dynEvt.EventType);
            if (IsPublishToWebApi)
            {
              CallWebApiWithProject(jsonString, dynEvt.EventType, dynEvt.CustomerUID);
            }
            else
            {
              kafkaDriver.SendKafkaMessage(topicName, jsonString);
              WaitForTimeBasedOnNumberOfRecords(eventArray.Length);
            }
          }
          else
          {
            IsNotSameAsset(true);
            BuildMySqlInsertStringAndWriteToDatabase(dynEvt);
          }
        }
      }
      catch (Exception ex)
      {
        msg.DisplayException(ex.Message);
        throw;
      }
    }

    /// <summary>
    /// Publish event to web api
    /// </summary>
    /// <param name="eventArray"></param>
    /// <returns></returns>
    public string PublishEventToWebApi(string[] eventArray)
    {
      try
      {
        msg.DisplayEventsToConsoleWeb(eventArray);
        var allColumnNames = eventArray.ElementAt(0).Split(SEPARATOR);
        var eventRow = eventArray.ElementAt(1).Split(SEPARATOR);
        dynamic eventObject = ConvertToExpando(allColumnNames, eventRow);
        var eventDate = eventObject.EventDate;
        LastEventDate = eventDate;
        var jsonString = BuildEventIntoObject(eventObject);
        string response;
        try
        {
          response = CallWebApiWithProject(jsonString, eventObject.EventType, eventObject.CustomerUID);
        }
        catch (RuntimeBinderException)
        {
          response = CallWebApiWithProject(jsonString, eventObject.EventType, CustomerUid.ToString());
        }
        return response;
      }
      catch (Exception ex)
      {
        msg.DisplayException(ex.Message);
        return ex.Message;
      }
    }


    public ImportedFileDescriptor ConvertImportFileArrayToObject(string[] importFileArray, int row)
    {
      msg.DisplayEventsToConsoleWeb(importFileArray);
      var allColumnNames = importFileArray.ElementAt(0).Split(SEPARATOR);
      var eventRow = importFileArray.ElementAt(row).Split(SEPARATOR);
      dynamic eventObject = ConvertToExpando(allColumnNames, eventRow);
      var jsonString = BuildEventIntoObject(eventObject);
      var expectedResults = JsonConvert.DeserializeObject<ImportedFileDescriptor>(jsonString);
      return expectedResults;
    }

    /// <summary>
    /// Call the version 4 of the project master data
    /// </summary>
    /// <param name="jsonString"></param>
    /// <param name="eventType"></param>
    /// <param name="customerUid"></param>
    private string CallWebApiWithProject(string jsonString, string eventType, string customerUid)
    {
      var response = string.Empty;
      Thread.Sleep(500);
      switch (eventType)
      {
        case "CreateProjectEvent":
        case "CreateProjectRequest":
          response = CallProjectWebApiV4("api/v4/project/", HttpMethod.Post.ToString(), jsonString, customerUid);
          break;
        case "UpdateProjectEvent":
        case "UpdateProjectRequest":
          response = CallProjectWebApiV4("api/v4/project/", HttpMethod.Put.ToString(), jsonString, customerUid);
          break;
        case "DeleteProjectEvent":
          response = CallProjectWebApiV4("api/v4/project/" + ProjectUid, HttpMethod.Delete.ToString(), string.Empty, customerUid);
          break;
      }
      //Thread.Sleep(500);
      Console.WriteLine(response);
      var jsonResponse = JsonConvert.DeserializeObject<ProjectV4DescriptorsSingleResult>(response);
      if (jsonResponse.Code == 0)
      {
        ProjectUid = new Guid(jsonResponse.ProjectDescriptor.ProjectUid);
        CustomerUid = new Guid(jsonResponse.ProjectDescriptor.CustomerUid);
      }
      return jsonResponse.Message;
    }

    /// <summary>
    /// Create the project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="projectId">legacy project id</param>
    /// <param name="name">project name</param>
    /// <param name="startDate">project start date</param>
    /// <param name="endDate">project end date</param>
    /// <param name="projectType">project type</param>
    /// <param name="timezone">project time zone</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="boundary"></param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void CreateProjectViaWebApi(Guid projectUid, int projectId, string name, DateTime startDate, DateTime endDate, string timezone, ProjectType projectType, DateTime actionUtc, string boundary, HttpStatusCode statusCode)
    {
      CreateProjectEvt = new CreateProjectEvent
      {
        ProjectID = projectId,
        ProjectUID = projectUid,
        ProjectName = name,
        ProjectType = projectType,
        ProjectBoundary = boundary,
        ProjectStartDate = startDate,
        ProjectEndDate = endDate,
        ProjectTimezone = timezone,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(CreateProjectEvt, string.Empty, statusCode, "Create", HttpMethod.Post.ToString(), CustomerUid.ToString());
    }

    /// <summary>
    /// Update the project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="name">project name</param>
    /// <param name="endDate">project end date</param>
    /// <param name="timezone">project time zone</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    /// <param name="projectType"></param>
    public void UpdateProjectViaWebApi(Guid projectUid, string name, DateTime endDate, string timezone, DateTime actionUtc, HttpStatusCode statusCode, ProjectType projectType = ProjectType.Standard)
    {
      UpdateProjectEvt = new UpdateProjectEvent
      {
        ProjectUID = projectUid,
        ProjectName = name,
        ProjectType = projectType,
        ProjectEndDate = endDate,
        ProjectTimezone = timezone,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(UpdateProjectEvt, string.Empty, statusCode, "Update", HttpMethod.Put.ToString(), CustomerUid.ToString());
    }

    /// <summary>
    /// Delete the project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void DeleteProjectViaWebApi(Guid projectUid, DateTime actionUtc, HttpStatusCode statusCode)
    {
      //DeleteProjectEvt = new DeleteProjectEvent
      //{
      //  ProjectUID = projectUid,
      //  ActionUTC = actionUtc
      //};
      CallProjectWebApi(null, projectUid.ToString(), statusCode, "Delete", HttpMethod.Delete.ToString(), CustomerUid.ToString());
    }

    /// <summary>
    /// Associate a customer and project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="customerUid">customer UID</param>
    /// <param name="customerId">legacy customer ID</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void AssociateCustomerProjectViaWebApi(Guid projectUid, Guid customerUid, int customerId, DateTime actionUtc, HttpStatusCode statusCode)
    {
      AssociateCustomerProjectEvt = new AssociateProjectCustomer
      {
        ProjectUID = projectUid,
        CustomerUID = customerUid,
        LegacyCustomerID = customerId,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(AssociateCustomerProjectEvt, "AssociateCustomer", statusCode, "Associate customer", HttpMethod.Post.ToString(), customerUid.ToString());
    }

    /// <summary>
    /// Dissociate a customer and project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="customerUid">customer UID</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void DissociateProjectViaWebApi(Guid projectUid, Guid customerUid, DateTime actionUtc, HttpStatusCode statusCode)
    {
      DissociateCustomerProjectEvt = new DissociateProjectCustomer
      {
        ProjectUID = projectUid,
        CustomerUID = customerUid,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(DissociateCustomerProjectEvt, "DissociateCustomer", statusCode, "Dissociate customer", HttpMethod.Post.ToString(), CustomerUid.ToString());
    }

    /// <summary>
    /// Associate a geofence and project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="geofenceUid">geofence UID</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void AssociateGeofenceProjectViaWebApi(Guid projectUid, Guid geofenceUid, DateTime actionUtc, HttpStatusCode statusCode)
    {
      AssociateProjectGeofenceEvt = new AssociateProjectGeofence
      {
        ProjectUID = projectUid,
        GeofenceUID = geofenceUid,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(AssociateProjectGeofenceEvt, "AssociateGeofence", statusCode, "Associate geofence", HttpMethod.Post.ToString(), CustomerUid.ToString());
    }

    /// <summary>
    /// Create the project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="projectId">legacy project id</param>
    /// <param name="name">project name</param>
    /// <param name="startDate">project start date</param>
    /// <param name="endDate">project end date</param>
    /// <param name="projectType">project type</param>
    /// <param name="timezone">project time zone</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="boundary"></param>
    /// <param name="statusCode">expected status code from web api call</param>
    public string CreateProjectViaWebApiV2(string name, DateTime startDate, DateTime endDate, string timezone, ProjectType projectType, DateTime actionUtc, List<TBCPoint> boundary, HttpStatusCode statusCode)
    {
      var createProjectV2Request = CreateProjectV2Request.CreateACreateProjectV2Request(
      projectType, startDate, endDate, name, timezone, boundary,
        new BusinessCenterFile() { FileSpaceId = "u3bdc38d-1afe-470e-8c1c-fc241d4c5e01", Name = "CTCTSITECAL.dc", Path = "/BC Data/Sites/Chch Test Site" }
      );
      string requestJson;
      if (createProjectV2Request == null)
      {
        requestJson = null;
      }
      else
      {
        requestJson = JsonConvert.SerializeObject(createProjectV2Request, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      }
      return CallProjectWebApiV2(requestJson, string.Empty, "api/v2/projects/", statusCode, "Create", HttpMethod.Post.ToString(), CustomerUid.ToString());
    }

    /// <summary>
    /// Validate the TBC orgShortName for this customer via the web api. 
    /// </summary>
    /// <param name="orgShortName"></param>
    /// <param name="statusCode">expected status code from web api call</param>
    public string ValidateTbcOrgIdApiV2(string orgShortName, HttpStatusCode statusCode)
    {
      var validateTccAuthorizationRequest = ValidateTccAuthorizationRequest.CreateValidateTccAuthorizationRequest(orgShortName);
      string requestJson;
      if (validateTccAuthorizationRequest == null)
      {
        requestJson = null;
      }
      else
      {
        requestJson = JsonConvert.SerializeObject(validateTccAuthorizationRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      }
      return CallProjectWebApiV2(requestJson, string.Empty, "api/v2/preferences/tcc", statusCode, "Create", HttpMethod.Post.ToString(), CustomerUid.ToString());
    }


    /// <summary>
    /// Call web api version 3
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="customerUid"></param>
    /// <param name="expectedResultsArray"></param>
    public void GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode statusCode, Guid customerUid, string[] expectedResultsArray)
    {
      var response = CallProjectWebApi(null, string.Empty, statusCode, "Get", "GET", customerUid == Guid.Empty ? null : customerUid.ToString());
      if (statusCode == HttpStatusCode.OK)
      {
        if (expectedResultsArray.Length == 0)
        {
          var actualProjects = JsonConvert.DeserializeObject<ImmutableDictionary<int, ProjectDescriptor>>(response);
          Assert.IsTrue(expectedResultsArray.Length == actualProjects.Count, " There should not be any projects");
        }
        else
        {
          var actualProjects = JsonConvert.DeserializeObject<ImmutableDictionary<int, ProjectDescriptor>>(response);
          var expectedProjects = ConvertArrayToList(expectedResultsArray).OrderBy(p => p.ProjectUid)
            .ToImmutableDictionary(key => key.LegacyProjectId, project =>
              new ProjectDescriptor()
              {
                ProjectType = project.ProjectType,
                Name = project.Name,
                ProjectTimeZone = project.ProjectTimeZone,
                IsArchived = project.IsArchived,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                ProjectUid = project.ProjectUid,
                LegacyProjectId = project.LegacyProjectId,
                ProjectGeofenceWKT = project.ProjectGeofenceWKT,
                CustomerUID = project.CustomerUID,
                LegacyCustomerId = CustomerId,
                CoordinateSystemFileName = project.CoordinateSystemFileName
              });
          msg.DisplayResults("Expected projects :" + JsonConvert.SerializeObject(expectedProjects), "Actual from WebApi: " + response);
          Assert.IsFalse(expectedResultsArray.Length == actualProjects.Count, " Number of projects return do not match expected");
          CompareTheActualProjectDictionaryWithExpected(actualProjects, expectedProjects, true);
        }
      }
    }

    /// <summary>
    /// Call web api version 4 
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="customerUid"></param>
    /// <param name="expectedResultsArray"></param>
    /// <param name="ignoreZeros"></param>
    public void GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode statusCode, Guid customerUid, string[] expectedResultsArray, bool ignoreZeros)
    {
      var response = CallProjectWebApiV4("api/v4/project/", HttpMethod.Get.ToString(), null, customerUid.ToString());
      if (statusCode == HttpStatusCode.OK)
      {
        if (expectedResultsArray.Length == 0)
        {
          var projectDescriptorsListResult = JsonConvert.DeserializeObject<ProjectDescriptorsListResult>(response);
          var actualProjects = projectDescriptorsListResult.ProjectDescriptors.OrderBy(p => p.ProjectUid).ToList();
          Assert.IsTrue(expectedResultsArray.Length == actualProjects.Count, " There should not be any projects");
        }
        else
        {
          var projectDescriptorsListResult = JsonConvert.DeserializeObject<ProjectDescriptorsListResult>(response);
          var actualProjects = projectDescriptorsListResult.ProjectDescriptors.OrderBy(p => p.ProjectUid).ToList();
          var expectedProjects = ConvertArrayToList(expectedResultsArray).OrderBy(p => p.ProjectUid).ToList();
          msg.DisplayResults("Expected projects :" + JsonConvert.SerializeObject(expectedProjects), "Actual from WebApi: " + response);
          Assert.IsTrue(expectedResultsArray.Length - 1 == actualProjects.Count, " Number of projects return do not match expected");
          CompareTheActualProjectListWithExpected(actualProjects, expectedProjects, ignoreZeros);
        }
      }
    }

    /// <summary>
    /// Get project details for one project
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="customerUid"></param>
    /// <param name="projectUid"></param>
    /// <param name="expectedResultsArray"></param>
    /// <param name="ignoreZeros"></param>
    public void GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode statusCode, Guid customerUid, string projectUid, string[] expectedResultsArray, bool ignoreZeros)
    {
      var response = CallProjectWebApiV4("api/v4/project/" + projectUid, HttpMethod.Get.ToString(), null, customerUid.ToString());
      if (statusCode == HttpStatusCode.OK)
      {
        var projectDescriptorResult = JsonConvert.DeserializeObject<ProjectV4DescriptorsSingleResult>(response);
        var actualProject = new List<ProjectV4Descriptor> { projectDescriptorResult.ProjectDescriptor };
        var expectedProjects = ConvertArrayToProjectV4DescriptorList(expectedResultsArray).OrderBy(p => p.ProjectUid).ToList();
        msg.DisplayResults("Expected project :" + JsonConvert.SerializeObject(expectedProjects), "Actual from WebApi: " + response);
        Assert.IsTrue(actualProject.Count == 1, " There should be one project");
        CompareTheActualProjectListV4WithExpected(actualProject, expectedProjects, ignoreZeros);
      }
    }

    /// <summary>
    /// Get project details for one project
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="projectUid"></param>
    public ProjectV4Descriptor GetProjectDetailsViaWebApiV4(Guid customerUid, string projectUid)
    {
      var response = CallProjectWebApiV4("api/v4/project/" + projectUid, HttpMethod.Get.ToString(), null, customerUid.ToString());
      ProjectV4DescriptorsSingleResult projectDescriptorResult = null;
      Log.Info($"GetProjectDetailsViaWebApiV4. response: {JsonConvert.SerializeObject(response)}", Log.ContentType.ApiSend);

      if (!string.IsNullOrEmpty(response))
      {
        projectDescriptorResult = JsonConvert.DeserializeObject<ProjectV4DescriptorsSingleResult>(response);
      }
      else
      {
        Assert.IsTrue(true, " There should be one project");
      }

      return projectDescriptorResult?.ProjectDescriptor;
    }

    public GeofenceV4DescriptorsListResult GetProjectGeofencesViaWebApiV4(string customerUid, string geofenceTypeString, string projectUidString )
    {
      var routeSuffix = "api/v4/geofences" + geofenceTypeString + projectUidString;
      var response = CallProjectWebApiV4(routeSuffix, HttpMethod.Get.ToString(), null, customerUid);
      Log.Info($"GetProjectGeofencesViaWebApiV4. response: {JsonConvert.SerializeObject(response)}", Log.ContentType.ApiSend);

      if (!string.IsNullOrEmpty(response))
      {
        return JsonConvert.DeserializeObject<GeofenceV4DescriptorsListResult>(response);
      }
      return null;
    }

    public ContractExecutionResult AssociateProjectGeofencesViaWebApiV4(string customerUid, string projectUid, List<GeofenceType> geofenceTypes, List<Guid> geofenceGuids)
    {
      var updateProjectGeofenceRequest =
        UpdateProjectGeofenceRequest.CreateUpdateProjectGeofenceRequest
            (ProjectUid = Guid.Parse(projectUid), geofenceTypes, geofenceGuids);
      var messagePayload = JsonConvert.SerializeObject(updateProjectGeofenceRequest);
      var response = CallProjectWebApiV4("api/v4/geofences", HttpMethod.Put.ToString(), messagePayload, customerUid);
      Log.Info($"AssociateProjectGeofencesViaWebApiV4. response: {JsonConvert.SerializeObject(response)}", Log.ContentType.ApiSend);

      if (!string.IsNullOrEmpty(response))
      {
        return JsonConvert.DeserializeObject<ContractExecutionResult>(response);
      }
      return null;
    }

    /// <summary>
    /// Compare the two lists of projects
    /// </summary>
    /// <param name="actualProjects"></param>
    /// <param name="expectedProjects"></param>
    /// <param name="ignoreZeros">Ignore nulls or zeros if expected results</param>
    public void CompareTheActualProjectListWithExpected(List<ProjectDescriptor> actualProjects, List<ProjectDescriptor> expectedProjects, bool ignoreZeros)
    {
      for (var cntlist = 0; cntlist < actualProjects.Count; cntlist++)
      {
        var oType = actualProjects[cntlist].GetType();
        foreach (var oProperty in oType.GetProperties())
        {
          var expectedValue = oProperty.GetValue(expectedProjects[cntlist], null);
          var actualValue = oProperty.GetValue(actualProjects[cntlist], null);
          if (ignoreZeros)
          {
            if (expectedValue == null)
            {
              continue;
            }
            if (expectedValue.ToString() == "0")
            {
              continue;
            }
          }
          if (!Equals(expectedValue, actualValue))
          {
            Assert.Fail(oProperty.Name + " Expected: " + expectedValue + " is not equal to actual: " + actualValue + " on the " + cntlist + 1 + " element in the list");
          }
        }
      }
    }

    /// <summary>
    /// Compare the two lists of projects
    /// </summary>
    /// <param name="actualProjects"></param>
    /// <param name="expectedProjects"></param>
    /// <param name="ignoreZeros">Ignore nulls or zeros if expected results</param>
    public void CompareTheActualProjectDictionaryWithExpected(ImmutableDictionary<int, ProjectDescriptor> actualProjects, ImmutableDictionary<int, ProjectDescriptor> expectedProjects, bool ignoreZeros)
    {
      List<ProjectDescriptor> actualList = actualProjects.Select(p => p.Value).ToList();
      List<ProjectDescriptor> expectedList = expectedProjects.Select(p => p.Value).ToList();
      for (var cntlist = 0; cntlist < actualList.Count; cntlist++)
      {
        var oType = actualList[cntlist].GetType();
        foreach (var oProperty in oType.GetProperties())
        {
          var expectedValue = oProperty.GetValue(expectedList[cntlist], null);
          var actualValue = oProperty.GetValue(actualList[cntlist], null);
          if (ignoreZeros)
          {
            if (expectedValue == null)
            {
              continue;
            }
            if (expectedValue.ToString() == "0")
            {
              continue;
            }
          }
          if (!Equals(expectedValue, actualValue))
          {
            Assert.Fail(oProperty.Name + " Expected: " + expectedValue + " is not equal to actual: " + actualValue + " on the " + cntlist + 1 + " element in the list");
          }
        }
      }
    }


    /// <summary>
    /// Compare the two lists of projects
    /// </summary>
    /// <param name="actualProjects"></param>
    /// <param name="expectedProjects"></param>
    /// <param name="ignoreZeros">Ignore nulls or zeros if expected results</param>
    public void CompareTheActualProjectListV4WithExpected(List<ProjectV4Descriptor> actualProjects, List<ProjectV4Descriptor> expectedProjects, bool ignoreZeros)
    {
      for (var cntlist = 0; cntlist < actualProjects.Count; cntlist++)
      {
        var oType = actualProjects[cntlist].GetType();
        foreach (var oProperty in oType.GetProperties())
        {
          var expectedValue = oProperty.GetValue(expectedProjects[cntlist], null);
          var actualValue = oProperty.GetValue(actualProjects[cntlist], null);
          if (ignoreZeros)
          {
            if (expectedValue == null)
            {
              continue;
            }
            if (expectedValue.ToString() == "0")
            {
              continue;
            }
          }
          if (!Equals(expectedValue, actualValue))
          {
            Assert.Fail(oProperty.Name + " Expected: " + expectedValue + " is not equal to actual: " + actualValue + " on the " + cntlist + 1 + " element in the list");
          }
        }
      }
    }
   
    public void CompareTheActualImportFileWithExpectedV4(ImportedFileDescriptor actualFile, ImportedFileDescriptor expectedFile, bool ignoreZeros)
    {
      CompareTheActualImportFileWithExpected<ImportedFileDescriptor>(actualFile, expectedFile, ignoreZeros);
    }

    public void CompareTheActualImportFileWithExpectedV2(DesignDetailV2Result actualFile, DesignDetailV2Result expectedFile, bool ignoreZeros)
    {
      CompareTheActualImportFileWithExpected< DesignDetailV2Result>(actualFile, expectedFile, ignoreZeros);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expectedFile"></param>
    /// <param name="ignoreZeros">Ignore nulls or zeros if expected results</param>
    /// <param name="actualFile"></param>
    public void CompareTheActualImportFileWithExpected<T>(T actualFile, T expectedFile, bool ignoreZeros)
    {
      var oType = actualFile.GetType();
      foreach (var oProperty in oType.GetProperties())
      {
        var expectedValue = oProperty.GetValue(expectedFile, null);
        var actualValue = oProperty.GetValue(actualFile, null);
        if (ignoreZeros)
        {
          if (expectedValue == null)
          {
            continue;
          }
          if (expectedValue.ToString() == "0" || expectedValue.ToString().Contains("1/01/0001") || expectedValue.ToString().Contains("1/1/01"))
          {
            continue;
          }
        }
        if (!Equals(expectedValue, actualValue))
        {
          Assert.Fail(oProperty.Name + " Expected: " + expectedValue + " is not equal to actual: " + actualValue);
        }
      }
    }


    /// <summary>
    /// Inject the MockCustomer
    /// </summary>
    /// <param name="customerUid">Customer UID</param>
    /// <param name="name">Customer name</param>
    /// <param name="type">Customer type</param>
    public void CreateMockCustomer(Guid customerUid, string name, CustomerType type)
    {
      MockCustomer = new Customer
      {
        CustomerUID = customerUid.ToString(),
        Name = name,
        CustomerType = type,
        IsDeleted = false,
        LastActionedUTC = DateTime.UtcNow
      };
      var customerTypeId = (int)MockCustomer.CustomerType;
      var deleted = MockCustomer.IsDeleted ? 1 : 0;
      var query = $@"INSERT INTO `{TsCfg.dbSchema}`.{"Customer"} 
                            (CustomerUID,Name,fk_CustomerTypeID,IsDeleted,LastActionedUTC) VALUES
                            ('{MockCustomer.CustomerUID}','{MockCustomer.Name}',{customerTypeId},{deleted},'{MockCustomer.LastActionedUTC:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
      var mysqlHelper = new MySqlHelper();
      mysqlHelper.ExecuteMySqlInsert(TsCfg.DbConnectionString, query);
    }

    /// <summary>
    /// Inject the MockSubscription and MockProjectSubscription
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="subscriptionUid">Subscription UID</param>
    /// <param name="customerUid">Customer UID</param>
    /// <param name="startDate">Start date of the subscription</param>
    /// <param name="endDate">End date of the subscription</param>
    /// <param name="effectiveDate">Date at which the subscripton takes effect for the project</param>
    public void CreateMockProjectSubscription(string projectUid, string subscriptionUid, string customerUid, DateTime startDate, DateTime endDate, DateTime effectiveDate)
    {
      MockSubscription = new Subscription
      {
        SubscriptionUID = subscriptionUid,
        CustomerUID = customerUid,
        ServiceTypeID = 20,//19=Landfill, 20=Project Monitoring
        StartDate = startDate,
        EndDate = endDate,
        LastActionedUTC = DateTime.UtcNow
      };
      var query = $@"INSERT INTO `{TsCfg.dbSchema}`.{"Subscription"} 
                            (SubscriptionUID,fk_CustomerUID,fk_ServiceTypeID,StartDate,EndDate,LastActionedUTC) VALUES
                            ('{MockSubscription.SubscriptionUID}','{MockSubscription.CustomerUID}',{MockSubscription.ServiceTypeID},'{MockSubscription.StartDate:yyyy-MM-dd HH}','{MockSubscription.EndDate:yyyy-MM-dd}','{MockSubscription.LastActionedUTC:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
      var mysqlHelper = new MySqlHelper();
      mysqlHelper.ExecuteMySqlInsert(TsCfg.DbConnectionString, query);

      MockProjectSubscription = new ProjectSubscription
      {
        SubscriptionUID = subscriptionUid,
        ProjectUID = projectUid,
        EffectiveDate = effectiveDate,
        LastActionedUTC = DateTime.UtcNow
      };
      query = $@"INSERT INTO `{TsCfg.dbSchema}`.{"ProjectSubscription"} 
                            (fk_SubscriptionUID,fk_ProjectUID,EffectiveDate,LastActionedUTC) VALUES
                            ('{MockProjectSubscription.SubscriptionUID}','{MockProjectSubscription.ProjectUID}','{MockProjectSubscription.EffectiveDate:yyyy-MM-dd}','{MockProjectSubscription.LastActionedUTC:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
      mysqlHelper.ExecuteMySqlInsert(TsCfg.DbConnectionString, query);
    }

    /// <summary>
    /// Inject the MockSubscription
    /// </summary>
    /// <param name="orgId"></param>
    /// <param name="customerUid">Customer UID</param>
    public void CreateMockCustomerTbcOrgId(string orgId, string customerUid)
    {
      var lastActionedUtc = DateTime.UtcNow;

      var query = $@"INSERT INTO `{TsCfg.dbSchema}`.{"CustomerTccOrg"} 
                            (CustomerUID,TCCOrgID,LastActionedUTC) VALUES
                            ('{customerUid}','{orgId}','{lastActionedUtc:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
      var mysqlHelper = new MySqlHelper();
      mysqlHelper.ExecuteMySqlInsert(TsCfg.DbConnectionString, query);
    }

    /// <summary>
    /// Check if the test is being debugged in VS. Set to different endpoind
    /// </summary>
    public string GetBaseUri()
    {
      var baseUri = TsCfg.webApiUri;
      if (Debugger.IsAttached || TsCfg.operatingSystem == "Windows_NT")
      {
        baseUri = TsCfg.debugWebApiUri;
      }
      return baseUri;
    }

    /// <summary>
    /// Converts a special date string eg 2d+12:00:00 which signifies a two date and 12 hour offset
    /// to a normal date time based on the first event date.
    /// </summary>
    /// <param name="timeStampAndDayOffSet">Date day off set and timestamp from first event date</param>
    /// <param name="startEventDateTime"></param>
    /// <returns>Datetime</returns>
    public DateTime ConvertTimeStampAndDayOffSetToDateTime(string timeStampAndDayOffSet, DateTime startEventDateTime)
    {
      var components = Regex.Split(timeStampAndDayOffSet, @"d+\+");
      var offset = double.Parse(components[0].Trim());
      return DateTime.Parse(startEventDateTime.AddDays(offset).ToString("yyyy-MM-dd") + " " + components[1].Trim());
    }

    #endregion

    #region Private Methods
    /// <summary>
    /// Set the full kafka topic name
    /// </summary>
    /// <param name="masterDataEvent"></param>
    /// <returns></returns>
    private string SetKafkaTopicName(string masterDataEvent)
    {
      var topicName = TsCfg.masterDataTopic + masterDataEvent + TsCfg.kafkaTopicSuffix;
      return topicName;
    }


    /// <summary>
    /// Set the full topic name from the event type
    /// </summary>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private string SetTheKafkaTopicFromTheEvent(string eventType)
    {
      var topicName = string.Empty;
      switch (eventType)
      {
        case "CreateAssetEvent":
        case "UpdateAssetEvent":
        case "DeleteAssetEvent":
          topicName = SetKafkaTopicName("IAssetEvent");
          break;
        case "CreateDeviceEvent":
        case "UpdateDeviceEvent":
        case "AssociateDeviceAssetEvent":
        case "DissociateDeviceAssetEvent":
          topicName = SetKafkaTopicName("IDeviceEvent");
          break;
        case "CreateCustomerEvent":
        case "UpdateCustomerEvent":
        case "DeleteCustomerEvent":
        case "AssociateCustomerUserEvent":
        case "DissociateCustomerUserEvent":
          topicName = SetKafkaTopicName("ICustomerEvent");
          break;
        case "CreateAssetSubscriptionEvent":
        case "UpdateAssetSubscriptionEvent":
        case "CreateCustomerSubscriptionEvent":
        case "CreateProjectSubscriptionEvent":
        case "UpdateProjectSubscriptionEvent":
        case "AssociateProjectSubscriptionEvent":
          topicName = SetKafkaTopicName("ISubscriptionEvent");
          break;
        case "CreateProjectEvent":
        case "UpdateProjectEvent":
        case "DeleteProjectEvent":
          topicName = SetKafkaTopicName("IProjectEvent");
          break;
        case "AssociateProjectCustomer":
        case "AssociateProjectGeofence":
        case "CreateGeofenceEvent":
          topicName = SetKafkaTopicName("IGeofenceEvent");
          break;
      }
      return topicName;
    }


    /// <summary>
    /// Wait for time to let the data feed process
    /// </summary>
    /// <param name="count"></param>
    private static void WaitForTimeBasedOnNumberOfRecords(int count)
    {
      if (count < 7)
      { Thread.Sleep(1000); }
      if (count > 6)
      { Thread.Sleep(1000); }
      if (count > 13)
      { Thread.Sleep(1000); }
    }

    /// <summary>
    /// Create an instance of the master data events. Convert to JSON. 
    /// </summary>
    /// <param name="eventObject">event to be published</param>
    /// <returns>json string with event serialized</returns>
    private string BuildEventIntoObject(dynamic eventObject)
    {
      var jsonString = string.Empty;
      string eventType = eventObject.EventType;
      switch (eventType)
      {
        case "CreateAssetEvent":
          var createAssetEvent = new CreateAssetEvent()
          {
            ActionUTC = eventObject.EventDate,
            AssetUID = new Guid(AssetUid),
            AssetName = eventObject.AssetName,
            AssetType = eventObject.AssetType,
            SerialNumber = eventObject.SerialNumber,
            MakeCode = eventObject.Make,
            Model = eventObject.Model,
            IconKey = Convert.ToInt32(eventObject.IconKey)
          };
          if (HasProperty(eventObject, "OwningCustomerUID"))
          {
            createAssetEvent.OwningCustomerUID = new Guid(eventObject.OwningCustomerUID);
          }
          if (HasProperty(eventObject, "LegacyAssetId"))
          {
            createAssetEvent.LegacyAssetId = Convert.ToInt64(eventObject.LegacyAssetId);
          }
          if (HasProperty(eventObject, "EquipmentVIN"))
          {
            createAssetEvent.EquipmentVIN = eventObject.EquipmentVIN;
          }

          jsonString = JsonConvert.SerializeObject(new { CreateAssetEvent = createAssetEvent }, JsonSettings);
          break;
        case "UpdateAssetEvent":
          var updateAssetEvent = new UpdateAssetEvent()
          {
            ActionUTC = eventObject.EventDate,
            AssetUID = new Guid(AssetUid)
          };
          if (HasProperty(eventObject, "AssetName"))
          {
            updateAssetEvent.AssetName = eventObject.AssetName;
          }
          if (HasProperty(eventObject, "AssetType"))
          {
            updateAssetEvent.AssetType = eventObject.AssetType;
          }
          if (HasProperty(eventObject, "Model"))
          {
            updateAssetEvent.Model = eventObject.Model;
          }
          if (HasProperty(eventObject, "IconKey"))
          {
            updateAssetEvent.IconKey = Convert.ToInt32(eventObject.IconKey);
          }
          if (HasProperty(eventObject, "LegacyAssetId"))
          {
            updateAssetEvent.LegacyAssetId = Convert.ToInt32(eventObject.LegacyAssetId);
          }
          if (HasProperty(eventObject, "OwningCustomerUID"))
          {
            updateAssetEvent.OwningCustomerUID = new Guid(eventObject.OwningCustomerUID);
          }
          if (HasProperty(eventObject, "EquipmentVIN"))
          {
            updateAssetEvent.EquipmentVIN = eventObject.EquipmentVIN;
          }

          jsonString = JsonConvert.SerializeObject(new { UpdateAssetEvent = updateAssetEvent }, JsonSettings);
          break;
        case "DeleteAssetEvent":
          var deleteAssetEvent = new DeleteAssetEvent()
          {
            ActionUTC = eventObject.EventDate,
            AssetUID = new Guid(AssetUid)
          };
          jsonString = JsonConvert.SerializeObject(new { DeleteAssetEvent = deleteAssetEvent }, JsonSettings);
          break;
        case "CreateDeviceEvent":
          var createDeviceEvent = new CreateDeviceEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            DeviceSerialNumber = eventObject.DeviceSerialNumber,
            DeviceState = eventObject.DeviceState,
            DeviceType = eventObject.DeviceType,
            DeviceUID = new Guid(eventObject.DeviceUID)
          };
          if (HasProperty(eventObject, "DeregisteredUTC"))
          {
            createDeviceEvent.DeregisteredUTC = DateTime.Parse(eventObject.DeregisteredUTC);
          }
          if (HasProperty(eventObject, "DataLinkType"))
          {
            createDeviceEvent.DataLinkType = eventObject.DataLinkType;
          }
          if (HasProperty(eventObject, "GatewayFirmwarePartNumber"))
          {
            createDeviceEvent.GatewayFirmwarePartNumber = eventObject.GatewayFirmwarePartNumber;
          }
          if (HasProperty(eventObject, "MainboardSoftwareVersion"))
          {
            createDeviceEvent.MainboardSoftwareVersion = eventObject.MainboardSoftwareVersion;
          }
          if (HasProperty(eventObject, "ModuleType"))
          {
            createDeviceEvent.ModuleType = eventObject.ModuleType;
          }
          if (HasProperty(eventObject, "RadioFirmwarePartNumber"))
          {
            createDeviceEvent.RadioFirmwarePartNumber = eventObject.RadioFirmwarePartNumber;
          }
          jsonString = JsonConvert.SerializeObject(new { CreateDeviceEvent = createDeviceEvent }, JsonSettings);
          break;
        case "UpdateDeviceEvent":
          var updateDeviceEvent = new UpdateDeviceEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            DeviceSerialNumber = eventObject.DeviceSerialNumber,
            DeviceState = eventObject.DeviceState,
            DeviceType = eventObject.DeviceType,
            DeviceUID = new Guid(eventObject.DeviceUID)
          };
          if (HasProperty(eventObject, "DataLinkType"))
          {
            updateDeviceEvent.DataLinkType = eventObject.DataLinkType;
          }
          if (HasProperty(eventObject, "GatewayFirmwarePartNumber"))
          {
            updateDeviceEvent.GatewayFirmwarePartNumber = eventObject.GatewayFirmwarePartNumber;
          }
          if (HasProperty(eventObject, "MainboardSoftwareVersion"))
          {
            updateDeviceEvent.MainboardSoftwareVersion = eventObject.MainboardSoftwareVersion;
          }
          if (HasProperty(eventObject, "ModuleType"))
          {
            updateDeviceEvent.ModuleType = eventObject.ModuleType; //missing from Update repo
          }
          if (HasProperty(eventObject, "DataLinkType"))
          {
            updateDeviceEvent.RadioFirmwarePartNumber = eventObject.RadioFirmwarePartNumber;
          }
          if (HasProperty(eventObject, "DeregisteredUTC"))
          {
            updateDeviceEvent.DeregisteredUTC = DateTime.Parse(eventObject.DeregisteredUTC);
          }
          jsonString = JsonConvert.SerializeObject(new { UpdateDeviceEvent = updateDeviceEvent }, JsonSettings);
          break;
        case "AssociateDeviceAssetEvent":
          var associateDeviceEvent = new AssociateDeviceAssetEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            AssetUID = new Guid(eventObject.AssetUID),
            DeviceUID = new Guid(eventObject.DeviceUID),
          };
          jsonString = JsonConvert.SerializeObject(new { AssociateDeviceAssetEvent = associateDeviceEvent }, JsonSettings);
          break;
        case "DissociateDeviceAssetEvent":
          var dissociateDeviceEvent = new DissociateDeviceAssetEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            AssetUID = new Guid(eventObject.AssetUID),
            DeviceUID = new Guid(eventObject.DeviceUID),
          };
          jsonString = JsonConvert.SerializeObject(new { DissociateDeviceAssetEvent = dissociateDeviceEvent }, JsonSettings);
          break;

        case "CreateCustomerEvent":
          var createCustomerEvent = new CreateCustomerEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            CustomerName = eventObject.CustomerName,
            CustomerType = eventObject.CustomerType,
            CustomerUID = new Guid(eventObject.CustomerUID)
          };
          jsonString = JsonConvert.SerializeObject(new { CreateCustomerEvent = createCustomerEvent }, JsonSettings);
          break;
        case "UpdateCustomerEvent":
          var updateCustomerEvent = new UpdateCustomerEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            CustomerUID = new Guid(eventObject.CustomerUID)
          };
          if (HasProperty(eventObject, "CustomerName"))
          {
            updateCustomerEvent.CustomerName = eventObject.CustomerName;
          }
          jsonString = JsonConvert.SerializeObject(new { UpdateCustomerEvent = updateCustomerEvent }, JsonSettings);
          break;
        case "DeleteCustomerEvent":
          var deleteCustomerEvent = new DeleteCustomerEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            CustomerUID = new Guid(eventObject.CustomerUID)
          };
          jsonString = JsonConvert.SerializeObject(new { DeleteCustomerEvent = deleteCustomerEvent }, JsonSettings);
          break;
        case "AssociateCustomerUserEvent":
          var associateCustomerUserEvent = new AssociateCustomerUserEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            CustomerUID = new Guid(eventObject.CustomerUID),
            UserUID = new Guid(eventObject.UserUID)
          };
          jsonString = JsonConvert.SerializeObject(new { AssociateCustomerUserEvent = associateCustomerUserEvent }, JsonSettings);
          break;
        case "DissociateCustomerUserEvent":
          var dissociateCustomerUserEvent = new DissociateCustomerUserEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            CustomerUID = new Guid(eventObject.CustomerUID),
            UserUID = new Guid(eventObject.UserUID)
          };
          jsonString = JsonConvert.SerializeObject(new { DissociateCustomerUserEvent = dissociateCustomerUserEvent }, JsonSettings);
          break;
        case "CreateAssetSubscriptionEvent":
          var createAssetSubscriptionEvent = new CreateAssetSubscriptionEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            StartDate = DateTime.Parse(eventObject.StartDate),
            AssetUID = new Guid(eventObject.AssetUID),
            DeviceUID = new Guid(eventObject.DeviceUID),
            CustomerUID = new Guid(eventObject.CustomerUID),
            EndDate = DateTime.Parse(eventObject.EndDate),
            SubscriptionType = eventObject.SubscriptionType,
            SubscriptionUID = new Guid(eventObject.SubscriptionUID)
          };
          jsonString = JsonConvert.SerializeObject(new { CreateAssetSubscriptionEvent = createAssetSubscriptionEvent }, JsonSettings);
          break;
        case "UpdateAssetSubscriptionEvent":
          var updateAssetSubscriptionEvent = new UpdateAssetSubscriptionEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            StartDate = DateTime.Parse(eventObject.StartDate),
            AssetUID = new Guid(eventObject.AssetUID),
            DeviceUID = new Guid(eventObject.DeviceUID),
            CustomerUID = new Guid(eventObject.CustomerUID),
            EndDate = DateTime.Parse(eventObject.EndDate),
            SubscriptionType = eventObject.SubscriptionType,
            SubscriptionUID = new Guid(eventObject.SubscriptionUID)
          };
          jsonString = JsonConvert.SerializeObject(new { UpdateAssetSubscriptionEvent = updateAssetSubscriptionEvent }, JsonSettings);
          break;
        case "CreateCustomerSubscriptionEvent":
          var createCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent()
          {
            CustomerUID = new Guid(eventObject.CustomerUID),
            SubscriptionUID = new Guid(eventObject.SubscriptionUID),
            EndDate = DateTime.Parse(eventObject.EndDate),
            SubscriptionType = eventObject.SubscriptionType,
            StartDate = DateTime.Parse(eventObject.StartDate),
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate
          };
          jsonString = JsonConvert.SerializeObject(new { CreateCustomerSubscriptionEvent = createCustomerSubscriptionEvent }, JsonSettings);
          break;
        case "CreateProjectSubscriptionEvent":
          var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            StartDate = DateTime.Parse(eventObject.StartDate),
            CustomerUID = new Guid(eventObject.CustomerUID),
            EndDate = DateTime.Parse(eventObject.EndDate),
            SubscriptionType = eventObject.SubscriptionType,
            SubscriptionUID = new Guid(eventObject.SubscriptionUID)
          };
          jsonString = JsonConvert.SerializeObject(new { CreateProjectSubscriptionEvent = createProjectSubscriptionEvent }, JsonSettings);
          break;
        case "UpdateProjectSubscriptionEvent":
          var updateProjectSubscriptionEvent = new UpdateProjectSubscriptionEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            StartDate = DateTime.Parse(eventObject.StartDate),
            EndDate = DateTime.Parse(eventObject.EndDate),
            SubscriptionType = eventObject.SubscriptionType,
            SubscriptionUID = new Guid(eventObject.SubscriptionUID)
          };
          jsonString = JsonConvert.SerializeObject(new { UpdateProjectSubscriptionEvent = updateProjectSubscriptionEvent }, JsonSettings);
          break;
        case "AssociateProjectSubscriptionEvent":
          var associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            EffectiveDate = eventObject.EventDate,
            ProjectUID = new Guid(eventObject.ProjectUID),
            SubscriptionUID = new Guid(eventObject.SubscriptionUID)
          };
          jsonString = JsonConvert.SerializeObject(new { AssociateProjectSubscriptionEvent = associateProjectSubscriptionEvent }, JsonSettings);
          break;
        case "CreateProjectEvent":
          var createProjectEvent = new CreateProjectEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            ProjectStartDate = DateTime.Parse(eventObject.ProjectStartDate),
            ProjectEndDate = DateTime.Parse(eventObject.ProjectEndDate),
            ProjectName = eventObject.ProjectName,
            ProjectTimezone = eventObject.ProjectTimezone,
            ProjectType = (ProjectType)Enum.Parse(typeof(ProjectType), eventObject.ProjectType),
            ProjectBoundary = eventObject.ProjectBoundary
          };
          if (HasProperty(eventObject, "CoordinateSystem"))
          {
            createProjectEvent.CoordinateSystemFileName = eventObject.CoordinateSystem;
            createProjectEvent.CoordinateSystemFileContent = Encoding.ASCII.GetBytes(TsCfg.coordinateSystem);
          }
          if (HasProperty(eventObject, "ProjectID"))
          {
            createProjectEvent.ProjectID = int.Parse(eventObject.ProjectID);
          }
          if (HasProperty(eventObject, "ProjectUID"))
          {
            createProjectEvent.ProjectUID = new Guid(eventObject.ProjectUID);
          }
          if (HasProperty(eventObject, "CustomerUID"))
          {
            createProjectEvent.CustomerUID = new Guid(eventObject.CustomerUID);
          }
          if (HasProperty(eventObject, "Description"))
          {
            createProjectEvent.Description = eventObject.Description;
          }
          if (HasProperty(eventObject, "CustomerID"))
          {
            createProjectEvent.CustomerID = int.Parse(eventObject.CustomerID);
          }
          jsonString = IsPublishToWebApi ? JsonConvert.SerializeObject(createProjectEvent, JsonSettings) : JsonConvert.SerializeObject(new { CreateProjectEvent = createProjectEvent }, JsonSettings);
          break;
        case "CreateProjectRequest":
          Guid? cpProjectUid = null;
          Guid? cpCustomerUid = null;
          var createProjectRequest = new CreateProjectEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            ProjectStartDate = DateTime.Parse(eventObject.ProjectStartDate),
            ProjectEndDate = DateTime.Parse(eventObject.ProjectEndDate),
            ProjectName = eventObject.ProjectName,
            ProjectTimezone = eventObject.ProjectTimezone,
            ProjectType = (ProjectType)Enum.Parse(typeof(ProjectType), eventObject.ProjectType),
            ProjectBoundary = eventObject.ProjectBoundary
          };
          if (HasProperty(eventObject, "CoordinateSystem"))
          {
            createProjectRequest.CoordinateSystemFileName = eventObject.CoordinateSystem;
            createProjectRequest.CoordinateSystemFileContent = Encoding.ASCII.GetBytes(TsCfg.coordinateSystem);
          }
          if (HasProperty(eventObject, "ProjectID"))
          {
            createProjectRequest.ProjectID = int.Parse(eventObject.ProjectID);
          }
          if (HasProperty(eventObject, "ProjectUID"))
          {
            cpProjectUid = new Guid(eventObject.ProjectUID);
          }
          if (HasProperty(eventObject, "ProjectUID"))
          {
            cpCustomerUid = new Guid(eventObject.CustomerUID);
          }
          if (HasProperty(eventObject, "Description"))
          {
            createProjectRequest.Description = eventObject.Description;
          }
          if (HasProperty(eventObject, "CustomerID"))
          {
            createProjectRequest.CustomerID = int.Parse(eventObject.CustomerID);
          }
          var cprequest = CreateProjectRequest.CreateACreateProjectRequest(cpProjectUid,
            cpCustomerUid, createProjectRequest.ProjectType,
            createProjectRequest.ProjectName, createProjectRequest.Description, createProjectRequest.ProjectStartDate,
            createProjectRequest.ProjectEndDate, createProjectRequest.ProjectTimezone,
            createProjectRequest.ProjectBoundary, createProjectRequest.CustomerID,
            createProjectRequest.CoordinateSystemFileName, createProjectRequest.CoordinateSystemFileContent);
          jsonString = JsonConvert.SerializeObject(cprequest, JsonSettings);
          break;
        case "UpdateProjectEvent":
          var updateProjectEvent = new UpdateProjectEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            ProjectUID = new Guid(eventObject.ProjectUID),
          };
          if (HasProperty(eventObject, "CoordinateSystem"))
          {
            updateProjectEvent.CoordinateSystemFileName = eventObject.CoordinateSystem;
            updateProjectEvent.CoordinateSystemFileContent = Encoding.ASCII.GetBytes(TsCfg.coordinateSystem);
          }
          if (HasProperty(eventObject, "ProjectEndDate") && eventObject.ProjectEndDate != null)
          {
            updateProjectEvent.ProjectEndDate = DateTime.Parse(eventObject.ProjectEndDate);
          }
          if (HasProperty(eventObject, "ProjectTimezone"))
          {
            updateProjectEvent.ProjectTimezone = eventObject.ProjectTimezone;
          }
          if (HasProperty(eventObject, "ProjectName"))
          {
            updateProjectEvent.ProjectName = eventObject.ProjectName;
          }
          if (HasProperty(eventObject, "ProjectType"))
          {
            updateProjectEvent.ProjectType = (ProjectType)Enum.Parse(typeof(ProjectType), eventObject.ProjectType);
          }
          if (HasProperty(eventObject, "Description"))
          {
            updateProjectEvent.Description = eventObject.Description;
          }
          jsonString = IsPublishToWebApi ? JsonConvert.SerializeObject(updateProjectEvent, JsonSettings) : JsonConvert.SerializeObject(new { UpdateProjectEvent = updateProjectEvent }, JsonSettings);
          break;
        case "UpdateProjectRequest":
          var updateProjectRequest = new UpdateProjectEvent()
          {
            ProjectUID = new Guid(eventObject.ProjectUID),
          };
          if (HasProperty(eventObject, "CoordinateSystem"))
          {
            updateProjectRequest.CoordinateSystemFileName = eventObject.CoordinateSystem;
            updateProjectRequest.CoordinateSystemFileContent = Encoding.ASCII.GetBytes(TsCfg.coordinateSystem);
          }
          if (HasProperty(eventObject, "ProjectEndDate") && eventObject.ProjectEndDate != null)
          {
            updateProjectRequest.ProjectEndDate = DateTime.Parse(eventObject.ProjectEndDate);
          }
          if (HasProperty(eventObject, "ProjectName"))
          {
            updateProjectRequest.ProjectName = eventObject.ProjectName;
          }
          if (HasProperty(eventObject, "ProjectType"))
          {
            updateProjectRequest.ProjectType = (ProjectType)Enum.Parse(typeof(ProjectType), eventObject.ProjectType);
          }
          if (HasProperty(eventObject, "Description"))
          {
            updateProjectRequest.Description = eventObject.Description;
          }
          if (HasProperty(eventObject, "ProjectBoundary"))
          {
            updateProjectRequest.ProjectBoundary = eventObject.ProjectBoundary;
          }
          var request = UpdateProjectRequest.CreateUpdateProjectRequest(updateProjectRequest.ProjectUID, updateProjectRequest.ProjectType, updateProjectRequest.ProjectName, updateProjectRequest.Description,
                                              updateProjectRequest.ProjectEndDate, updateProjectRequest.CoordinateSystemFileName, updateProjectRequest.CoordinateSystemFileContent, updateProjectRequest.ProjectBoundary);
          jsonString = JsonConvert.SerializeObject(request, JsonSettings);
          break;
        case "DeleteProjectEvent":
          var deleteProjectEvent = new DeleteProjectEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            ProjectUID = new Guid(eventObject.ProjectUID)
          };
          jsonString = IsPublishToWebApi ? JsonConvert.SerializeObject(deleteProjectEvent, JsonSettings) : JsonConvert.SerializeObject(new { DeleteProjectEvent = deleteProjectEvent }, JsonSettings);
          break;
        case "AssociateProjectCustomer":
          SetKafkaTopicName("IProjectEvent");
          var associateCustomerProject = new AssociateProjectCustomer()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            ProjectUID = new Guid(eventObject.ProjectUID),
            CustomerUID = new Guid(eventObject.CustomerUID)
          };
          jsonString = JsonConvert.SerializeObject(new { AssociateProjectCustomer = associateCustomerProject }, JsonSettings);
          break;
        case "AssociateProjectGeofence":
          SetKafkaTopicName("IProjectEvent");
          var associateProjectGeofence = new AssociateProjectGeofence()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            ProjectUID = new Guid(eventObject.ProjectUID),
            GeofenceUID = new Guid(eventObject.GeofenceUID)
          };
          jsonString = JsonConvert.SerializeObject(new { AssociateProjectGeofence = associateProjectGeofence }, JsonSettings);
          break;
        case "CreateGeofenceEvent":
          SetKafkaTopicName("IGeofenceEvent");
          var createGeofenceEvent = new CreateGeofenceEvent()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            GeofenceUID = new Guid(eventObject.GeofenceUID),
            CustomerUID = new Guid(eventObject.CustomerUID),
            Description = eventObject.Description,
            FillColor = Int32.Parse(eventObject.FillColor),
            GeofenceName = eventObject.GeofenceName,
            GeofenceType = eventObject.GeofenceType,
            GeometryWKT = eventObject.GeometryWKT,
            IsTransparent = Boolean.Parse(eventObject.IsTransparent),
            UserUID = new Guid(eventObject.UserUID)
          };
          jsonString = JsonConvert.SerializeObject(new { CreateGeofenceEvent = createGeofenceEvent }, JsonSettings);
          break;

        case "ImportedFileDescriptor":
          var importedFileDescriptor = new ImportedFileDescriptor()
          {
            CustomerUid = eventObject.CustomerUid,
            FileCreatedUtc = DateTime.Parse(eventObject.FileCreatedUtc),
            FileUpdatedUtc = DateTime.Parse(eventObject.FileUpdatedUtc),
            ImportedBy = eventObject.ImportedBy,
            ProjectUid = eventObject.ProjectUid,
            Name = eventObject.Name
          };
          if (HasProperty(eventObject, "SurveyedUtc"))
          {
            importedFileDescriptor.SurveyedUtc = DateTime.Parse(eventObject.SurveyedUtc);
          }
          if (HasProperty(eventObject, "IsActivated"))
          {
            importedFileDescriptor.IsActivated = eventObject.IsActivated.ToLower() == "true";
          }
          if (HasProperty(eventObject, "DxfUnitsType)"))
          {
            switch (eventObject.DxfUnitsType)
            {
              case "0":
                importedFileDescriptor.DxfUnitsType = DxfUnitsType.Meters;
                break;
              case "1":
                importedFileDescriptor.DxfUnitsType = DxfUnitsType.ImperialFeet;
                break;
              case "2":
                importedFileDescriptor.DxfUnitsType = DxfUnitsType.UsSurveyFeet;
                break;
            }
          }

          switch (eventObject.ImportedFileType)
          {
            case "0":
              importedFileDescriptor.ImportedFileType = VSS.VisionLink.Interfaces.Events.MasterData.Models.ImportedFileType.Linework;
              break;
            case "1":
              importedFileDescriptor.ImportedFileType = VSS.VisionLink.Interfaces.Events.MasterData.Models.ImportedFileType.DesignSurface;
              break;
            case "2":
              importedFileDescriptor.ImportedFileType = VSS.VisionLink.Interfaces.Events.MasterData.Models.ImportedFileType.SurveyedSurface;
              break;
            case "3":
              importedFileDescriptor.ImportedFileType = VSS.VisionLink.Interfaces.Events.MasterData.Models.ImportedFileType.Alignment;
              break;
            case "4":
              importedFileDescriptor.ImportedFileType = VSS.VisionLink.Interfaces.Events.MasterData.Models.ImportedFileType.MobileLinework;
              break;
            case "5":
              importedFileDescriptor.ImportedFileType = VSS.VisionLink.Interfaces.Events.MasterData.Models.ImportedFileType.SiteBoundary;
              break;
            case "6":
              importedFileDescriptor.ImportedFileType = VSS.VisionLink.Interfaces.Events.MasterData.Models.ImportedFileType.ReferenceSurface;
              break;
            case "7":
              importedFileDescriptor.ImportedFileType = VSS.VisionLink.Interfaces.Events.MasterData.Models.ImportedFileType.MassHaulPlan;
              break;
          }
          jsonString = JsonConvert.SerializeObject(importedFileDescriptor, JsonSettings);
          break;
      }
      return jsonString;
    }

    /// <summary>
    /// Inserts the events into the database
    /// </summary>
    /// <param name="eventObject"></param>
    private void BuildMySqlInsertStringAndWriteToDatabase(dynamic eventObject)
    {
      string dbTable = eventObject.TableName;
      var mysqlHelper = new MySqlHelper();
      var sqlCmd = $@"INSERT INTO `{TsCfg.dbSchema}`.{dbTable} ";
      switch (dbTable)
      {
        case "Asset":
          sqlCmd += $@"(AssetUID,LegacyAssetID,Name,MakeCode,SerialNumber,Model,IconKey,AssetType,OwningCustomerUID,LastActionedUTC) VALUES 
                ('{AssetUid}',{eventObject.LegacyAssetID},'{eventObject.Name}','{eventObject.MakeCode}','{eventObject.SerialNumber}','{eventObject.Model}',
                {eventObject.IconKey},'{eventObject.AssetType}','{eventObject.OwningCustomerUID}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "AssetDevice":
          sqlCmd += $@"(fk_DeviceUID,fk_AssetUID,LastActionedUTC) VALUES 
                ('{eventObject.fk_DeviceUID}','{eventObject.fk_AssetUID}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "AssetSubscription":
          sqlCmd += $@"(fk_AssetUID,fk_SubscriptionUID,EffectiveDate,LastActionedUTC) VALUES
                     ('{eventObject.fk_AssetUID}','{eventObject.fk_SubscriptionUID}','{eventObject.EffectiveDate}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "Customer":
          sqlCmd += $@"(CustomerUID,Name,fk_CustomerTypeID,IsDeleted,LastActionedUTC) VALUES
                     ('{eventObject.CustomerUID}','{eventObject.Name}',{eventObject.fk_CustomerTypeID},0,'{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "CustomerProject":
          sqlCmd += $@"(fk_CustomerUID,fk_ProjectUID,LastActionedUTC) VALUES
                     ('{eventObject.fk_CustomerUID}','{eventObject.fk_ProjectUID}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "CustomerTccOrg":
          sqlCmd += $@"(CustomerUID,TCCOrgID,LastActionedUTC) VALUES
                     ('{eventObject.CustomerUID}','{eventObject.TCCOrgID}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "Device":
          sqlCmd += $@"(DeviceUID,DeviceSerialNumber,DeviceType,DeviceState,DataLinkType,LastActionedUTC) VALUES 
                ('{eventObject.DeviceUID}','{eventObject.DeviceSerialNumber}','{eventObject.DeviceType}','{eventObject.DeviceState}','{eventObject.DataLinkType}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "Geofence":
          sqlCmd += $@"(GeofenceUID,Name,fk_GeofenceTypeID,GeometryWKT,FillColor,IsTransparent,IsDeleted,Description,fk_CustomerUID,UserUID,LastActionedUTC) VALUES
                     ('{eventObject.GeofenceUID}','{eventObject.Name}',{eventObject.fk_GeofenceTypeID},'{eventObject.GeometryWKT}',
                       {eventObject.FillColor},{eventObject.IsTransparent},{eventObject.IsDeleted},'{eventObject.Description}',
                      '{eventObject.fk_CustomerUID}','{eventObject.UserUID}','{eventObject.LastActionedUTC:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "ImportedFile":
          sqlCmd += $@"(fk_ProjectUID, ImportedFileUID, ImportedFileID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, IsDeleted, IsActivated, LastActionedUTC) VALUES 
                     ('{eventObject.ProjectUID}', '{eventObject.ImportedFileUID}', {eventObject.ImportedFileID}, '{eventObject.CustomerUID}', {eventObject.ImportedFileType}, '{eventObject.Name}', 
                      '{eventObject.FileDescriptor}', {eventObject.FileCreatedUTC}, {eventObject.FileUpdatedUTC}, '{eventObject.ImportedBy}', {eventObject.SurveyedUTC}, {eventObject.IsDeleted}, {eventObject.IsActivated}, {eventObject.LastActionedUTC});";
          break;
        case "Project":
          var formattedPolygon = string.Format("ST_GeomFromText('{0}')", eventObject.GeometryWKT);
          sqlCmd += $@"(ProjectUID,LegacyProjectID,Name,fk_ProjectTypeID,ProjectTimeZone,LandfillTimeZone,StartDate,EndDate,GeometryWKT,PolygonST,LastActionedUTC) VALUES
                     ('{eventObject.ProjectUID}',{eventObject.LegacyProjectID},'{eventObject.Name}',{eventObject.fk_ProjectTypeID},
                      '{eventObject.ProjectTimeZone}','{eventObject.LandfillTimeZone}','{eventObject.StartDate:yyyy-MM-dd}','{eventObject.EndDate:yyyy-MM-dd}','{eventObject.GeometryWKT}',{formattedPolygon},'{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "ProjectGeofence":
          sqlCmd += $@"(fk_ProjectUID,fk_GeofenceUID,LastActionedUTC) VALUES
                     ('{eventObject.fk_ProjectUID}','{eventObject.fk_GeofenceUID}','{eventObject.LastActionedUTC:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "ProjectSubscription":
          sqlCmd += $@"(fk_SubscriptionUID,fk_ProjectUID,EffectiveDate,LastActionedUTC) VALUES
                     ('{eventObject.fk_SubscriptionUID}','{eventObject.fk_ProjectUID}','{eventObject.StartDate}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "Subscription":
          sqlCmd += $@"(SubscriptionUID,fk_CustomerUID,fk_ServiceTypeID,StartDate,EndDate,LastActionedUTC) VALUES
                     ('{eventObject.SubscriptionUID}','{eventObject.fk_CustomerUID}','{eventObject.fk_ServiceTypeID}','{eventObject.StartDate}','{eventObject.EndDate}','{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
      }
      mysqlHelper.ExecuteMySqlInsert(TsCfg.DbConnectionString, sqlCmd);
    }

    /// <summary>
    /// This is used to set multiple asset events in the database
    /// </summary>
    /// <param name="isSameAsset">True if it's the same asset id we are dealing with</param>
    private void IsNotSameAsset(bool isSameAsset)
    {
      if (isSameAsset) return;
      SetAssetUid();
      SetFirstEventDate();
    }

    /// <summary>
    /// Check that a property exists in the dynamic object
    /// </summary>
    /// <param name="obj">dynamic object</param>
    /// <param name="propertyName">Property name as string</param>
    /// <returns>true or false</returns>
    private static bool HasProperty(dynamic obj, string propertyName)
    {
      var expandoDict = (IDictionary<string, object>)obj;
      if (expandoDict.ContainsKey(propertyName) && expandoDict[propertyName] != null)
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Create an ExpandoObject of all the fields from the event array
    /// </summary>
    /// <param name="allColumnNames">All the column names from the array</param>
    /// <param name="singleEventRow">A single row of event data</param>
    /// <returns>Object with all properties from array</returns>
    private ExpandoObject ConvertToExpando(string[] allColumnNames, string[] singleEventRow)
    {
      var expObj = new ExpandoObject() as IDictionary<string, Object>;
      var colIdx = -1;
      foreach (var colName in allColumnNames)
      {
        colIdx++;
        if (colName.Trim() == string.Empty)
        { continue; }

        dynamic obj = TransformObject(singleEventRow[colIdx].Trim());
        expObj.Add(colName.Trim(), obj);
      }
      return (ExpandoObject)expObj;
    }

    /// <summary>
    /// For some of the events in the event array they need some transforming 
    /// </summary>
    /// <param name="propertyValue">returns a converted/transformed single property. Mainly used for string null or special dates</param>
    /// <returns></returns>
    private dynamic TransformObject(string propertyValue)
    {
      dynamic obj;
      if (propertyValue == "null" || propertyValue == string.Empty)
      {
        return null;
      }
      if (Regex.IsMatch(propertyValue, @"^\s*\d+d\+\d+"))
      {
        obj = ConvertTimeStampAndDayOffSetToDateTime(propertyValue, FirstEventDate);
        return obj;
      }
      obj = propertyValue;
      return obj;
    }

    /// <summary>
    /// Convert the expected results into dynamic objects and forma list
    /// </summary>
    /// <param name="eventArray"></param>
    /// <returns></returns>
    private List<ProjectDescriptor> ConvertArrayToList(string[] eventArray)
    {
      var eventList = new List<ProjectDescriptor>();
      try
      {
        var allColumnNames = eventArray.ElementAt(0).Split(SEPARATOR);
        for (var rowCnt = 1; rowCnt <= eventArray.Length - 1; rowCnt++)
        {
          var eventRow = eventArray.ElementAt(rowCnt).Split(SEPARATOR);
          dynamic eventObject = ConvertToExpando(allColumnNames, eventRow);

          var pd = new ProjectDescriptor
          {
            Name = eventObject.ProjectName,
            ProjectTimeZone = eventObject.ProjectTimezone,
            ProjectType = (ProjectType)Enum.Parse(typeof(ProjectType), eventObject.ProjectType),
            StartDate = eventObject.ProjectStartDate,
            EndDate = eventObject.ProjectEndDate,
            ProjectGeofenceWKT = eventObject.ProjectBoundary,
          };
          if (HasProperty(eventObject, "IsArchived"))
          {
            pd.IsArchived = Boolean.Parse(eventObject.IsArchived);
          }
          if (HasProperty(eventObject, "CoordinateSystem"))
          {
            pd.CoordinateSystemFileName = eventObject.CoordinateSystem;
          }
          if (HasProperty(eventObject, "ProjectID"))
          {
            pd.LegacyProjectId = int.Parse(eventObject.ProjectID);
          }
          if (HasProperty(eventObject, "ProjectUID"))
          {
            pd.ProjectUid = eventObject.ProjectUID;
          }
          if (HasProperty(eventObject, "CustomerUID"))
          {
            pd.CustomerUID = eventObject.CustomerUID;
          }
          if (HasProperty(eventObject, "CustomerID"))
          {
            pd.LegacyCustomerId = eventObject.CustomerID;
          }
          eventList.Add(pd);
        }
        return eventList;
      }
      catch (Exception ex)
      {
        msg.DisplayException(ex.Message);
        throw;
      }
    }


    /// <summary>
    /// Convert the expected results into dynamic objects and forma list
    /// </summary>
    /// <param name="eventArray"></param>
    /// <returns></returns>
    private List<ProjectV4Descriptor> ConvertArrayToProjectV4DescriptorList(string[] eventArray)
    {
      var eventList = new List<ProjectV4Descriptor>();
      try
      {
        var allColumnNames = eventArray.ElementAt(0).Split(SEPARATOR);
        for (var rowCnt = 1; rowCnt <= eventArray.Length - 1; rowCnt++)
        {
          var eventRow = eventArray.ElementAt(rowCnt).Split(SEPARATOR);
          dynamic eventObject = ConvertToExpando(allColumnNames, eventRow);

          var pd = new ProjectV4Descriptor
          {
            Name = eventObject.ProjectName,
            ProjectTimeZone = eventObject.ProjectTimezone,
            ProjectType = (ProjectType)Enum.Parse(typeof(ProjectType), eventObject.ProjectType),
            StartDate = eventObject.ProjectStartDate,
            EndDate = eventObject.ProjectEndDate,
            ProjectGeofenceWKT = eventObject.ProjectBoundary,
          };
          if (HasProperty(eventObject, "IsArchived"))
          {
            pd.IsArchived = Boolean.Parse(eventObject.IsArchived);
          }
          if (HasProperty(eventObject, "CoordinateSystem"))
          {
            pd.CoordinateSystemFileName = eventObject.CoordinateSystem;
          }
          if (HasProperty(eventObject, "ProjectID"))
          {
            pd.LegacyProjectId = int.Parse(eventObject.ProjectID);
          }
          if (HasProperty(eventObject, "ProjectUID"))
          {
            pd.ProjectUid = eventObject.ProjectUID;
          }
          if (HasProperty(eventObject, "CustomerUID"))
          {
            pd.CustomerUid = eventObject.CustomerUID;
          }
          if (HasProperty(eventObject, "CustomerID"))
          {
            pd.LegacyCustomerId = eventObject.CustomerID;
          }
          if (HasProperty(eventObject, "Description"))
          {
            pd.Description = eventObject.Description;
          }
          //if (HasProperty(eventObject, "ServiceType"))
          //{
          //  pd.ServiceType = eventObject.ServiceType.ToSafeString();
          //}
          eventList.Add(pd);
        }
        return eventList;
      }
      catch (Exception ex)
      {
        msg.DisplayException(ex.Message);
        throw;
      }
    }
    /// <summary>
    /// Generate a random number. This is use for the number of days in the past to get a start date from.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    private int RandomNumber(int min, int max)
    {
      lock (syncLock)
      {
        return rndNumber.Next(min, max);
      }
    }

    /// <summary>
    /// Call the project web api
    /// </summary>
    /// <param name="evt">THe project event containing the data</param>
    /// <param name="routeSuffix">suffix to add to base uri if required</param>
    /// <param name="statusCode">expected return code of the web api call</param>
    /// <param name="what">name of the api being called for logging</param>
    /// <param name="method">http method</param>
    /// <param name="customerUid">Customer UID to add to http headers</param>
    /// <returns>The web api response</returns>
    private string CallProjectWebApi(IProjectEvent evt, string routeSuffix, HttpStatusCode statusCode, string what, string method = "POST", string customerUid = null)
    {
      string configJson;
      if (evt == null)
      {
        configJson = null;
      }
      else
      {
        configJson = JsonConvert.SerializeObject(evt, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      }
      var uri = GetBaseUri() + "api/v3/project/" + routeSuffix;
      var restClient = new RestClientUtil();
      var response = restClient.DoHttpRequest(uri, method, configJson, statusCode, "application/json", customerUid);
      if (response.Length > 0)
      { Console.WriteLine(what + " project response:" + response); }
      return response;
    }

    /// <summary>
    /// Call the version 4 of the web api
    /// </summary>
    /// <param name="routeSuffix"></param>
    /// <param name="method"></param>
    /// <param name="configJson"></param>
    /// <param name="customerUid"></param>
    /// <param name="jwt"></param>
    /// <returns></returns>
    public string CallProjectWebApiV4(string routeSuffix, string method, string configJson, string customerUid = null, string jwt = null)
    {
      var uri = GetBaseUri() + routeSuffix;  // "http://localhost:20979/"
      Console.WriteLine("URI=" + uri);
      var restClient = new RestClientUtil();
      var response = restClient.DoHttpRequest(uri, method, configJson, HttpStatusCode.OK, "application/json", customerUid, jwt);
      return response;
    }

    /// <summary>
    /// Call the version 2 of the web api - used for BCC integration
    /// </summary>
    /// <param name="requestJson"></param>
    /// <param name="routeSuffix"></param>
    /// <param name="what"></param>
    /// <param name="method"></param>
    /// <param name="customerUid"></param>
    /// <param name="jwt"></param>
    /// <param name="endPoint"></param>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public string CallProjectWebApiV2(string requestJson, string routeSuffix, string endPoint, HttpStatusCode statusCode, string what, string method = "POST", string customerUid = null, string jwt = null)
    {
      var uri = GetBaseUri() + endPoint + routeSuffix;
      var restClient = new RestClientUtil();
      var response = restClient.DoHttpRequest(uri, method, requestJson, HttpStatusCode.OK, "application/json", customerUid, jwt);
      return response;
    }

    #endregion
  }
}
