using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using Xunit;

namespace TestUtility
{
  public class TestSupport
  {
    private const string PROJECT_DB_SCHEMA_NAME = "CCSS-Project";

    public string AssetUid { get; set; }
    public DateTime FirstEventDate { get; set; }
    public DateTime LastEventDate { get; set; }    
    public Guid ProjectUid { get; set; }
    public Guid CustomerUid { get; set; }
    public Guid GeofenceUid { get; set; }

    public CreateProjectEvent CreateProjectEvt { get; set; }
    public UpdateProjectEvent UpdateProjectEvt { get; set; }

    public bool IsPublishToWebApi { get; set; }

    public readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
      DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
      NullValueHandling = NullValueHandling.Ignore
    };

    private readonly Random rndNumber = new Random();
    private readonly object syncLock = new object();
    private const char SEPARATOR = '|';

    private static readonly TestConfig _testConfig;
    private static readonly object _shortRaptorProjectIDLock = new object();

    private static int _currentShortRaptorProjectID;

    static TestSupport()
    {
      _testConfig = new TestConfig(PROJECT_DB_SCHEMA_NAME);

      const string query = "SELECT max(ShortRaptorProjectID) FROM Project;";

      var result = MySqlHelper.ExecuteRead(query);
      var index = string.IsNullOrEmpty(result)
        ? 1000
        : Convert.ToInt32(result);

      _currentShortRaptorProjectID = Math.Max(index, _currentShortRaptorProjectID);
    }

    public TestSupport()
    {
      SetFirstEventDate();
      SetLastEventDate();
      SetAssetUid();
      SetProjectUid();
      SetCustomerUid();
      SetGeofenceUid();
    }

    public static int GenerateShortRaptorProjectID()
    {
      lock (_shortRaptorProjectIDLock)
      {
        _currentShortRaptorProjectID += 1;

        return _currentShortRaptorProjectID;
      }
    }

    /// <summary>
    /// Set up the first event date for the events to go in. Also used as project start date for project tests.
    /// </summary>
    public void SetFirstEventDate() => FirstEventDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-RandomNumber(10, 360)), DateTimeKind.Unspecified);

    /// <summary>
    /// Set up the last event date for the events to go in. Also used as project end date for project tests.
    /// </summary>
    public void SetLastEventDate() => LastEventDate = FirstEventDate.AddYears(2);

    /// <summary>
    /// Set the asset UID to a random GUID
    /// </summary>
    public void SetAssetUid() => AssetUid = Guid.NewGuid().ToString();

    /// <summary>
    /// Set the project UID to a random GUID
    /// </summary>
    public void SetProjectUid() => ProjectUid = Guid.NewGuid();

    /// <summary>
    /// Set the customer UID to a random GUID
    /// </summary>
    public void SetCustomerUid() => CustomerUid = Guid.NewGuid();

    /// <summary>
    /// Set the geofence UID to a random GUID
    /// </summary>
    public void SetGeofenceUid() => GeofenceUid = Guid.NewGuid();
        
    /// <summary>
    /// Publish events to kafka from string array
    /// </summary>
    public async Task PublishEventCollection(string[] eventArray, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
      try
      {
        if (IsPublishToWebApi)
        {
          Msg.DisplayEventsToConsoleWeb(eventArray);
        }       
        else
        {
          Msg.DisplayEventsForDbInjectToConsole(eventArray);
        }

        var allColumnNames = eventArray.ElementAt(0).Split(SEPARATOR);

        for (var rowCnt = 1; rowCnt <= eventArray.Length - 1; rowCnt++)
        {
          var eventRow = eventArray.ElementAt(rowCnt).Split(SEPARATOR);
          dynamic dynEvt = ConvertToExpando(allColumnNames, eventRow);
          var eventDate = dynEvt.EventDate;
          LastEventDate = eventDate;
          if (IsPublishToWebApi)
          {
            var jsonString = BuildEventIntoObject(dynEvt);
            await CallWebApiWithProject(jsonString, dynEvt.EventType, dynEvt.CustomerUID, statusCode);     
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
        Msg.DisplayException(ex.Message);
        throw;
      }
    }

    /// <summary>
    /// Publish event to web api
    /// </summary>
    public async Task<string> PublishEventToWebApi(string[] eventArray, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
      try
      {
        Msg.DisplayEventsToConsoleWeb(eventArray);
        var allColumnNames = eventArray.ElementAt(0).Split(SEPARATOR);
        var eventRow = eventArray.ElementAt(1).Split(SEPARATOR);
        dynamic eventObject = ConvertToExpando(allColumnNames, eventRow);
        var eventDate = eventObject.EventDate;
        LastEventDate = eventDate;
        var jsonString = BuildEventIntoObject(eventObject);
        string response;
        try
        {
          response = await CallWebApiWithProject(jsonString, eventObject.EventType, eventObject.CustomerUID, statusCode);
        }
        catch (RuntimeBinderException)
        {
          response = await CallWebApiWithProject(jsonString, eventObject.EventType, CustomerUid.ToString(), statusCode);
        }
        return response;
      }
      catch (Exception ex)
      {
        Msg.DisplayException(ex.Message);
        return ex.Message;
      }
    }

    public ImportedFileDescriptor ConvertImportFileArrayToObject(string[] importFileArray, int row)
    {
      Msg.DisplayEventsToConsoleWeb(importFileArray);
      var allColumnNames = importFileArray.ElementAt(0).Split(SEPARATOR);
      var eventRow = importFileArray.ElementAt(row).Split(SEPARATOR);
      dynamic eventObject = ConvertToExpando(allColumnNames, eventRow);
      var jsonString = BuildEventIntoObject(eventObject);
      var expectedResults = JsonConvert.DeserializeObject<ImportedFileDescriptor>(jsonString);
      return expectedResults;
    }

    /// <summary>
    /// Create the project via the web api. 
    /// </summary>
    public Task<string> CreateProjectViaWebApiV5TBC(string name, DateTime startDate, DateTime endDate, string timezone, ProjectType projectType, List<TBCPoint> boundary)
    {
      var createProjectV2Request = CreateProjectV5Request.CreateACreateProjectV5Request(
      projectType, startDate, endDate, name, timezone, boundary,
        new BusinessCenterFile { FileSpaceId = "u3bdc38d-1afe-470e-8c1c-fc241d4c5e01", Name = "CTCTSITECAL.dc", Path = "/BC Data/Sites/Chch Test Site" }
      );

      var requestJson = createProjectV2Request == null
        ? null
        : JsonConvert.SerializeObject(createProjectV2Request, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });

      return CallProjectWebApi("api/v5/projects/", HttpMethod.Post, requestJson, CustomerUid.ToString());
    }

    /// <summary>
    /// Validate the TBC orgShortName for this customer via the web api. 
    /// </summary>
    public Task<string> ValidateTbcOrgIdApiV2(string orgShortName)
    {
      var validateTccAuthorizationRequest = ValidateTccAuthorizationRequest.CreateValidateTccAuthorizationRequest(orgShortName);

      var requestJson = validateTccAuthorizationRequest == null
        ? null
        : JsonConvert.SerializeObject(validateTccAuthorizationRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });

      return CallProjectWebApi("api/v5/preferences/tcc", HttpMethod.Post, requestJson, CustomerUid.ToString());
    }

    /// <summary>
    /// Call the version 4 of the project master data
    /// </summary>
    private async Task<string> CallWebApiWithProject(string jsonString, string eventType, string customerUid, HttpStatusCode statusCode)
    {
      var response = string.Empty;

      switch (eventType)
      {
        case "CreateProjectEvent":
        case "CreateProjectRequest":
          response = await CallProjectWebApi("api/v6/project/", HttpMethod.Post, jsonString, customerUid, statusCode: statusCode);
          break;
        case "UpdateProjectEvent":
        case "UpdateProjectRequest":
          response = await CallProjectWebApi("api/v6/project/", HttpMethod.Put, jsonString, customerUid, statusCode: statusCode);
          break;
        case "DeleteProjectEvent":
          response = await CallProjectWebApi("api/v6/project/" + ProjectUid, HttpMethod.Delete, string.Empty, customerUid, statusCode: statusCode);
          break;
      }

      var jsonResponse = JsonConvert.DeserializeObject<ProjectV6DescriptorsSingleResult>(response);

      if (jsonResponse.Code == 0)
      {
        ProjectUid = new Guid(jsonResponse.ProjectDescriptor.ProjectUid);
        CustomerUid = new Guid(jsonResponse.ProjectDescriptor.CustomerUid);
      }

      return jsonResponse.Message;
    }
   
   
    /// <summary>
    /// Call web api version 6 
    /// </summary>
    public async Task GetProjectsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode statusCode, Guid customerUid, string[] expectedResultsArray, bool ignoreZeros)
    {
      var response = await CallProjectWebApi("api/v6/project/", HttpMethod.Get, null, customerUid.ToString());
      if (statusCode == HttpStatusCode.OK)
      {
        if (expectedResultsArray.Length == 0)
        {
          var projectDescriptorsListResult = JsonConvert.DeserializeObject<ProjectDescriptorsListResult>(response);
          var actualProjects = projectDescriptorsListResult.ProjectDescriptors.OrderBy(p => p.ProjectUid).ToList();
          Assert.True(expectedResultsArray.Length == actualProjects.Count, " There should not be any projects");
        }
        else
        {
          var projectDescriptorsListResult = JsonConvert.DeserializeObject<ProjectDescriptorsListResult>(response);
          var actualProjects = projectDescriptorsListResult.ProjectDescriptors.OrderBy(p => p.ProjectUid).ToList();
          var expectedProjects = ConvertArrayToList(expectedResultsArray).OrderBy(p => p.ProjectUid).ToList();
          Msg.DisplayResults("Expected projects :" + JsonConvert.SerializeObject(expectedProjects), "Actual from WebApi: " + response);
          Assert.True(expectedResultsArray.Length - 1 == actualProjects.Count, " Number of projects return do not match expected");
          CompareTheActualProjectListWithExpected(actualProjects, expectedProjects, ignoreZeros);
        }
      }
    }

    /// <summary>
    /// Get project details for one project
    /// </summary>
    public async Task GetProjectDetailsViaWebApiV6AndCompareActualWithExpected(HttpStatusCode statusCode, Guid customerUid, string projectUid, string[] expectedResultsArray, bool ignoreZeros)
    {
      var response = await CallProjectWebApi("api/v6/project/" + projectUid, HttpMethod.Get, null, customerUid.ToString());
      if (statusCode == HttpStatusCode.OK)
      {
        var projectDescriptorResult = JsonConvert.DeserializeObject<ProjectV6DescriptorsSingleResult>(response);
        var actualProject = new List<ProjectV6Descriptor> { projectDescriptorResult.ProjectDescriptor };
        var expectedProjects = ConvertArrayToProjectV6DescriptorList(expectedResultsArray).OrderBy(p => p.ProjectUid).ToList();
        Msg.DisplayResults("Expected project :" + JsonConvert.SerializeObject(expectedProjects), "Actual from WebApi: " + response);
        Assert.True(actualProject.Count == 1, " There should be one project");
        CompareTheActualProjectListV6WithExpected(actualProject, expectedProjects, ignoreZeros);
      }
    }

    /// <summary>
    /// Get project details for one project
    /// </summary>
    public async Task<ProjectV6Descriptor> GetProjectDetailsViaWebApiV6(Guid customerUid, string projectUid, HttpStatusCode statusCode)
    {
      var response = await CallProjectWebApi("api/v6/project/" + projectUid, HttpMethod.Get, null, customerUid.ToString(), statusCode: statusCode);
      
      if (string.IsNullOrEmpty(response))
      {
        throw new Exception("There should be one project");
      }

      return JsonConvert.DeserializeObject<ProjectV6DescriptorsSingleResult>(response)
                        .ProjectDescriptor;
    }


    /// <summary>
    /// Compare the two lists of projects
    /// </summary>
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

          Assert.Equal(expectedValue, actualValue);
        }
      }
    }

    /// <summary>
    /// Compare the two lists of projects
    /// </summary>
    public void CompareTheActualProjectDictionaryWithExpected(ImmutableDictionary<int, ProjectDescriptor> actualProjects, ImmutableDictionary<int, ProjectDescriptor> expectedProjects, bool ignoreZeros)
    {
      var actualList = actualProjects.Select(p => p.Value).ToList();
      var expectedList = expectedProjects.Select(p => p.Value).ToList();
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

          Assert.Equal(expectedValue, actualValue);
        }
      }
    }


    /// <summary>
    /// Compare the two lists of projects
    /// </summary>
    public void CompareTheActualProjectListV6WithExpected(List<ProjectV6Descriptor> actualProjects, List<ProjectV6Descriptor> expectedProjects, bool ignoreZeros)
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

          Assert.Equal(expectedValue, actualValue);
        }
      }
    }

    public void CompareTheActualImportFileWithExpectedV6(ImportedFileDescriptor actualFile, ImportedFileDescriptor expectedFile, bool ignoreZeros)
    {
      CompareTheActualImportFileWithExpected(actualFile, expectedFile, ignoreZeros);
    }

    /// <summary>
    /// 
    /// </summary>
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

        Assert.Equal(expectedValue, actualValue);
      }
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
        case "CreateProjectRequest":
          string cpCustomerUid = null;
          string cpDescription = null;
          string cpCoordinateSystemFileName = null;
          byte[] cpCoordinateSystemFileContent = null;
          if (HasProperty(eventObject, "CoordinateSystem"))
          {
            cpCoordinateSystemFileName = eventObject.CoordinateSystem;
            cpCoordinateSystemFileContent = Encoding.ASCII.GetBytes(_testConfig.coordinateSystem);
          }
           
          if (HasProperty(eventObject, "CustomerUID"))
          {
            cpCustomerUid = eventObject.CustomerUID;
          }
          if (HasProperty(eventObject, "Description"))
          {
            cpDescription = eventObject.Description;
          }
          var cprequest = CreateProjectRequest.CreateACreateProjectRequest(cpCustomerUid,
            (ProjectType)Enum.Parse(typeof(ProjectType), eventObject.ProjectType),
            eventObject.ProjectName, cpDescription, DateTime.Parse(eventObject.ProjectStartDate),
            DateTime.Parse(eventObject.ProjectEndDate), eventObject.ProjectTimezone,
            eventObject.ProjectBoundary, cpCoordinateSystemFileName, cpCoordinateSystemFileContent);
          jsonString = JsonConvert.SerializeObject(cprequest, JsonSettings);
          break;        
        case "UpdateProjectRequest":        
          var updateProjectRequest = new UpdateProjectEvent()
          {
            ProjectUID = new Guid(eventObject.ProjectUID),
          };
          if (HasProperty(eventObject, "CoordinateSystem"))
          {
            updateProjectRequest.CoordinateSystemFileName = eventObject.CoordinateSystem;
            updateProjectRequest.CoordinateSystemFileContent = Encoding.ASCII.GetBytes(_testConfig.coordinateSystem);
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
        case "AssociateProjectGeofence":          
          var associateProjectGeofence = new AssociateProjectGeofence()
          {
            ActionUTC = eventObject.EventDate,
            ReceivedUTC = eventObject.EventDate,
            ProjectUID = eventObject.ProjectUID,
            GeofenceUID = eventObject.GeofenceUID
          };
          jsonString = JsonConvert.SerializeObject(new { AssociateProjectGeofence = associateProjectGeofence }, JsonSettings);
          break;
        case "ImportedFileDescriptor":
          var importedFileDescriptor = new ImportedFileDescriptor
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
          if (HasProperty(eventObject, "ParentUid"))
          {
            importedFileDescriptor.ParentUid = Guid.Parse(eventObject.ParentUid);
          }
          if (HasProperty(eventObject, "Offset"))
          {
            importedFileDescriptor.Offset = double.Parse(eventObject.Offset);
          }
          if (HasProperty(eventObject, "IsActivated"))
          {
            importedFileDescriptor.IsActivated = eventObject.IsActivated.ToLower() == "true";
          }
          if (HasProperty(eventObject, "DxfUnitsType)"))
          {
            importedFileDescriptor.DxfUnitsType = Enum.Parse<DxfUnitsType>((string)eventObject.DxfUnitsType);

          }

          importedFileDescriptor.ImportedFileType = Enum.Parse<ImportedFileType>((string)eventObject.ImportedFileType);
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
      var sqlCmd = $@"INSERT INTO `{_testConfig.dbSchema}`.{dbTable} ";

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
          sqlCmd += $@"(GeofenceUID,Name,fk_GeofenceTypeID,PolygonST,FillColor,IsTransparent,IsDeleted,Description,fk_CustomerUID,UserUID,LastActionedUTC) VALUES
                     ('{eventObject.GeofenceUID}','{eventObject.Name}',{eventObject.fk_GeofenceTypeID},ST_GeomFromText('{eventObject.GeometryWKT}'),
                       {eventObject.FillColor},{eventObject.IsTransparent},{eventObject.IsDeleted},'{eventObject.Description}',
                      '{eventObject.fk_CustomerUID}','{eventObject.UserUID}','{eventObject.LastActionedUTC:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
          break;
        case "ImportedFile":
          sqlCmd += $@"(fk_ProjectUID, ImportedFileUID, ImportedFileID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_ReferenceImportedFileUid, Offset, IsDeleted, IsActivated, LastActionedUTC) VALUES 
                     ('{eventObject.ProjectUID}', '{eventObject.ImportedFileUID}', {eventObject.ImportedFileID}, '{eventObject.CustomerUID}', {eventObject.ImportedFileType}, '{eventObject.Name}', 
                      '{eventObject.FileDescriptor}', {eventObject.FileCreatedUTC}, {eventObject.FileUpdatedUTC}, '{eventObject.ImportedBy}', {eventObject.SurveyedUTC}, {eventObject.ParentUid}, {eventObject.Offset}, {eventObject.IsDeleted}, {eventObject.IsActivated}, {eventObject.LastActionedUTC});";
          break;
        case "Project":
          var formattedPolygon = string.Format("ST_GeomFromText('{0}')", eventObject.GeometryWKT);
          sqlCmd += $@"(ProjectUID,LegacyProjectID,Name,fk_ProjectTypeID,ProjectTimeZone,LandfillTimeZone,StartDate,EndDate,PolygonST,LastActionedUTC) VALUES
                     ('{eventObject.ProjectUID}',{eventObject.LegacyProjectID},'{eventObject.Name}',{eventObject.fk_ProjectTypeID},
                      '{eventObject.ProjectTimeZone}','{eventObject.LandfillTimeZone}','{eventObject.StartDate:yyyy-MM-dd}','{eventObject.EndDate:yyyy-MM-dd}',{formattedPolygon},'{eventObject.EventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
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

      MySqlHelper.ExecuteNonQuery(sqlCmd);
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
      return expandoDict.ContainsKey(propertyName) && expandoDict[propertyName] != null;
    }

    /// <summary>
    /// Create an ExpandoObject of all the fields from the event array
    /// </summary>
    /// <param name="allColumnNames">All the column names from the array</param>
    /// <param name="singleEventRow">A single row of event data</param>
    /// <returns>Object with all properties from array</returns>
    private ExpandoObject ConvertToExpando(string[] allColumnNames, string[] singleEventRow)
    {
      var expObj = new ExpandoObject() as IDictionary<string, object>;
      var colIdx = -1;

      foreach (var colName in allColumnNames)
      {
        colIdx++;
        if (colName.Trim() == string.Empty)
        { continue; }

        var obj = TransformObject(singleEventRow[colIdx].Trim());
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
        obj = DateTimeHelper.ConvertTimeStampAndDayOffSetToDateTime(propertyValue, FirstEventDate);
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
            pd.IsArchived = bool.Parse(eventObject.IsArchived);
          }
          if (HasProperty(eventObject, "CoordinateSystem"))
          {
            pd.CoordinateSystemFileName = eventObject.CoordinateSystem;
          }
          if (HasProperty(eventObject, "ShortRaptorProjectId"))
          {
            pd.ShortRaptorProjectId = int.Parse(eventObject.ShortRaptorProjectId);
          }
          if (HasProperty(eventObject, "ProjectUID"))
          {
            pd.ProjectUid = eventObject.ProjectUID;
          }
          if (HasProperty(eventObject, "CustomerUID"))
          {
            pd.CustomerUID = eventObject.CustomerUID;
          }          
          eventList.Add(pd);
        }
        return eventList;
      }
      catch (Exception ex)
      {
        Msg.DisplayException(ex.Message);
        throw;
      }
    }


    /// <summary>
    /// Convert the expected results into dynamic objects and forma list
    /// </summary>
    /// <param name="eventArray"></param>
    /// <returns></returns>
    private List<ProjectV6Descriptor> ConvertArrayToProjectV6DescriptorList(string[] eventArray)
    {
      var eventList = new List<ProjectV6Descriptor>();
      try
      {
        var allColumnNames = eventArray.ElementAt(0).Split(SEPARATOR);
        for (var rowCnt = 1; rowCnt <= eventArray.Length - 1; rowCnt++)
        {
          var eventRow = eventArray.ElementAt(rowCnt).Split(SEPARATOR);
          dynamic eventObject = ConvertToExpando(allColumnNames, eventRow);

          var pd = new ProjectV6Descriptor
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
            pd.IsArchived = bool.Parse(eventObject.IsArchived);
          }
          if (HasProperty(eventObject, "CoordinateSystem"))
          {
            pd.CoordinateSystemFileName = eventObject.CoordinateSystem;
          }
          if (HasProperty(eventObject, "ProjectID"))
          {
            pd.ShortRaptorProjectId = int.Parse(eventObject.ShortRaptorProjectId);
          }
          if (HasProperty(eventObject, "ProjectUID"))
          {
            pd.ProjectUid = eventObject.ProjectUID;
          }
          if (HasProperty(eventObject, "CustomerUID"))
          {
            pd.CustomerUid = eventObject.CustomerUID;
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
        Msg.DisplayException(ex.Message);
        throw;
      }
    }
    /// <summary>
    /// Generate a random number. This is use for the number of days in the past to get a start date from.
    /// </summary>
    private int RandomNumber(int min, int max)
    {
      lock (syncLock)
      {
        return rndNumber.Next(min, max);
      }
    }

    public Task<string> CallProjectWebApi(string routeSuffix, HttpMethod method, string configJson, string customerUid = null, string jwt = null, HttpStatusCode statusCode = HttpStatusCode.OK)
      => RestClient.SendHttpClientRequest($"{routeSuffix}", method, MediaTypes.JSON, MediaTypes.JSON, customerUid, configJson, jwt, expectedHttpCode: statusCode);
  }
}
