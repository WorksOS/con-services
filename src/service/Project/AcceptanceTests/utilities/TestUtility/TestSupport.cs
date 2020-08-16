using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CCSS.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace TestUtility
{
  public class TestSupport
  {
    private const string PROJECT_DB_SCHEMA_NAME = "CCSS-Project";

    public DateTime FirstEventDate { get; set; }
    public DateTime LastEventDate { get; set; }
    public Guid ProjectUid { get; set; }
    public Guid CustomerUid { get; set; }

    public readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
      DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
      NullValueHandling = NullValueHandling.Ignore,
      ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    private readonly Random rndNumber = new Random();
    private readonly object syncLock = new object();
    private const char SEPARATOR = '|';

    private static readonly TestConfig _testConfig;

    private static List<TBCPoint> _boundaryLL;
    private static BusinessCenterFile _businessCenterFile;

    static TestSupport()
    {
      _testConfig = new TestConfig(PROJECT_DB_SCHEMA_NAME);

      const string query = "SELECT max(ShortRaptorProjectID) FROM Project;";

      var result = MySqlHelper.ExecuteRead(query);
      var index = string.IsNullOrEmpty(result)
        ? 1000
        : Convert.ToInt32(result);
    }

    public TestSupport()
    {
      SetFirstEventDate();
      SetLastEventDate();
      SetProjectUid();
      SetCustomerUid();

      _boundaryLL = new List<TBCPoint>
      {
        new TBCPoint(-43.5, 172.6),
        new TBCPoint(-43.5003, 172.6),
        new TBCPoint(-43.5003, 172.603),
        new TBCPoint(-43.5, 172.603)
      };

      _businessCenterFile = new BusinessCenterFile { FileSpaceId = "u3bdc38d-1afe-470e-8c1c-fc241d4c5e01", Name = "CTCTSITECAL.dc", Path = "/BC Data/Sites/Chch Test Site" };
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
    /// Set the project UID to a random GUID
    /// </summary>
    public void SetProjectUid() => ProjectUid = Guid.NewGuid();

    /// <summary>
    /// Set the customer UID to a random GUID
    /// </summary>
    public void SetCustomerUid() => CustomerUid = Guid.NewGuid();


    public async Task<CreateProjectResponseModel> CreateCustomerProject(ICwsProjectClient cwsProjectClient, string customerUid,
      string boundary = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))")
    {
      var createProjectRequestModel = new CreateProjectRequestModel
      {
        AccountId = customerUid,
        ProjectName = "wotever",
        Boundary = GeometryConversion.MapProjectBoundary(boundary)
      };

      var response = await cwsProjectClient.CreateProject(createProjectRequestModel);
      return response;
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
    public Task<string> CreateProjectViaWebApiV5TBC(string name)
    {
      var createProjectV5Request = CreateProjectV5Request.CreateACreateProjectV5Request(name, _boundaryLL, _businessCenterFile);

      var requestJson = createProjectV5Request == null
        ? null
        : JsonConvert.SerializeObject(createProjectV5Request, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });

      return CallProjectWebApi("api/v5/projects/", HttpMethod.Post, requestJson, CustomerUid.ToString());
    }

    /// <summary>
    /// Get a project by its shortProjectId via the WebAPI 
    /// </summary>
    public Task<string> GetProjectViaWebApiV5TBC(long projectId)
    {
      return  CallProjectWebApi($"api/v5/project/{projectId}", HttpMethod.Get, null, CustomerUid.ToString());
    }

    /// <summary>
    /// Get project list via the web api. Includes the shortProjectId in response 
    /// </summary>
    public Task<string> GetProjectViaWebApiV5TBC()
    {
      return CallProjectWebApi($"api/v5/project", HttpMethod.Get, null, CustomerUid.ToString());
    }

    /// <summary>
    /// Validate the TBC orgShortName for this customer via the web api. 
    /// </summary>
    public Task<string> ValidateTbcOrgIdApiV5(string orgShortName)
    {
      var validateTccAuthorizationRequest = ValidateTccAuthorizationRequest.CreateValidateTccAuthorizationRequest(orgShortName);

      var requestJson = validateTccAuthorizationRequest == null
        ? null
        : JsonConvert.SerializeObject(validateTccAuthorizationRequest, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });

      return CallProjectWebApi("api/v5/preferences/tcc", HttpMethod.Post, requestJson, CustomerUid.ToString());
    }

   
    /// <summary>
    /// Call projectSvc deviceGateway endpoint for deviceLKS List for project
    /// </summary>
    public async Task<string> GetDeviceLKSList(string customerUid, string projectUid, DateTime? earliestOfInterestUtc = null)
    {
      var route = "api/v1/devices";
      route += $"?projectUid={projectUid}";
      if (earliestOfInterestUtc != null)
        route += $"&earliestOfInterestUtc={earliestOfInterestUtc:yyyy-MM-ddTHH:mm:ssZ}";
      return await CallProjectWebApi(route, HttpMethod.Get,null, customerUid.ToString());
    }

    /// <summary>
    ///Call projectSvc deviceGateway endpoint deviceLKS for deviceName
    /// </summary>
    public async Task<string> GetDeviceLKS(string customerUid, string deviceName, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
      var route = "api/v1/device";
      route += $"?deviceName={deviceName}";
      return await CallProjectWebApi(route, HttpMethod.Get, null, customerUid.ToString(), statusCode: statusCode);
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

          // this is not setup in our create, but by another process within cws
          if (oProperty.Name == "UserProjectRole")
            continue;

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
        case "ProjectConfigurationFileResponseModel":
          var importedFileDescriptor2 = new ImportedFileDescriptor
          {
            CustomerUid = eventObject.CustomerUid,
            ProjectUid = eventObject.ProjectUid,
            Name = eventObject.Name,
            ImportedFileType = Enum.Parse<ImportedFileType>((string)eventObject.ImportedFileType)
          };
          jsonString = JsonConvert.SerializeObject(importedFileDescriptor2, JsonSettings);
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
        case "ImportedFile":
          sqlCmd += $@"(fk_ProjectUID, ImportedFileUID, ImportedFileID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_ReferenceImportedFileUid, Offset, IsDeleted, IsActivated, LastActionedUTC) VALUES 
                     ('{eventObject.ProjectUID}', '{eventObject.ImportedFileUID}', {eventObject.ImportedFileID}, '{eventObject.CustomerUID}', {eventObject.ImportedFileType}, '{eventObject.Name}', 
                      '{eventObject.FileDescriptor}', {eventObject.FileCreatedUTC}, {eventObject.FileUpdatedUTC}, '{eventObject.ImportedBy}', {eventObject.SurveyedUTC}, {eventObject.ParentUid}, {eventObject.Offset}, {eventObject.IsDeleted}, {eventObject.IsActivated}, {eventObject.LastActionedUTC});";
          break;
      }

      MySqlHelper.ExecuteNonQuery(sqlCmd);
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
            ProjectType = (CwsProjectType)Enum.Parse(typeof(CwsProjectType), eventObject.ProjectType),
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
          if (HasProperty(eventObject, "ProjectUID"))
          {
            pd.ProjectUid = eventObject.ProjectUID;
          }
          if (HasProperty(eventObject, "CustomerUID"))
          {
            pd.CustomerUid = eventObject.CustomerUID;
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
