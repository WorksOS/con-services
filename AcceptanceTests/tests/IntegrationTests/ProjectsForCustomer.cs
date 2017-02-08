using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System.Net;
using VSS.Geofence.Data.Models;

namespace IntegrationTests
{
  [TestClass]
  public class ProjectsForCustomer
  {
    private const string PROJECT_DB_SCHEMA_NAME = "VSS-MasterData-Project-Only";

    [TestMethod]
    public void Create_Project_Then_Retrieve()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 1";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, 1);

      var expectedProjects = new string[] {
            "| IsArchived | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                 | ProjectUid    | LegacyProjectId | ",
           $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid} | 100             |" };

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Multiple_Projects_And_Inject_Required_Data_Then_Retrieve()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid1 = Guid.NewGuid();
      var projectGuid2 = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 2";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid1, projectName, startDate, endDate, 1);
      Create_And_Subscribe_AdditionalProjects_for_existing_Customer(testSupport, customerGuid, projectGuid2, projectName, startDate, endDate, 2);

      var expectedProjects = new string[] {
      "   | IsArchived | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                 | ProjectUid     | LegacyProjectId |",
        $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid1} | 100             |",
        $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid2} | 100             |" };

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }

    [TestMethod]
    public void Create_Then_Update_Project_Name()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 3";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, 1);

      testSupport.UpdateProjectViaWebApi(projectGuid, "New Name", endDate, "New Zealand Standard Time", DateTime.Now, HttpStatusCode.OK);

      var expectedProjects = new string[] {
            "| IsArchived | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                 | ProjectUid    | LegacyProjectId | ",
           $"| false      | New Name      | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid} | 100             |" };
      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);

    }


    [TestMethod]
    public void Create_Then_Update_Project_EndDate()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 4";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, 1, "Central Standard Time");

      testSupport.UpdateProjectViaWebApi(projectGuid, projectName, endDate.AddDays(10), "Central Standard Time", DateTime.Now, HttpStatusCode.OK);
      var expectedProjects = new string[] {
            "| IsArchived | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                             | ProjectUid    | LegacyProjectId | ",
           $"| false      | {projectName} | Central Standard Time     | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.AddDays(10).ToString("O")} | {projectGuid} | 100             |" };
      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);

    }


    [TestMethod]
    public void Create_Then_Try_Update_Project_TimeZone()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 5";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, 1);

      testSupport.UpdateProjectViaWebApi(projectGuid, projectName, endDate, "Central Standard Time", DateTime.Now, HttpStatusCode.Forbidden);
      var expectedProjects = new string[] {
            "| IsArchived | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                 | ProjectUid    | LegacyProjectId | ",
           $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid} | 100             |" };
      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }

    [TestMethod]
    public void Create_Project_Then_Update_Project_Type()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 5";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, 1);

      testSupport.UpdateProjectViaWebApi(projectGuid, projectName, endDate, "New Zealand Standard Time", DateTime.Now, HttpStatusCode.OK, ProjectType.ProjectMonitoring);
      var expectedProjects = new string[] {
            "| IsArchived | Name          | ProjectTimeZone           | ProjectType                     | StartDate                 | EndDate                 | ProjectUid    | LegacyProjectId | ",
           $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.ProjectMonitoring} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid} | 100             |" };
      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Then_Delete_Project()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 6";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, 1);
      var expectedProjects = new string[] {
            "| IsArchived | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                 | ProjectUid    | LegacyProjectId | ",
           $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid} | 100             |" };
      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);

      testSupport.DeleteProjectViaWebApi(projectGuid, DateTime.Now, HttpStatusCode.OK);

      expectedProjects = new string[] {
            "| IsArchived | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                 | ProjectUid    | LegacyProjectId | ",
           $"| true       | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid} | 100             |" };
      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Then_Associate_Geofence_with_Project()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      var userGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 7";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("1000d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, 1);
      
      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName | GeofenceType | GeofenceUID    | GeometryWKT | IsTransparent | UserUID    | ",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Trump        | 1            | {geofenceGuid} | 1,2,3,4,5,6 | {false}       | {userGuid} |"};

      testSupport.InjectEventsIntoKafka(geofenceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID",
        "fk_CustomerUID, Name", //Fields
        $"{customerGuid}, Trump", //Expected
        geofenceGuid);

      testSupport.AssociateGeofenceProjectViaWebApi(projectGuid, geofenceGuid, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID",
        "fk_ProjectUID", //Fields
        $"{projectGuid}", //Expected
        geofenceGuid);


      var expectedProjects = new string[] {
            "| IsArchived  | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                 | ProjectUid    | LegacyProjectId | ProjectGeofenceWKT | ",
           $"| false       | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid} | 100             | 1,2,3,4,5,6        |" };
      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    /// <summary>
    /// TODO!!!!
    /// Currently it is possible to associate multiple GeofenceType.Project to a project, this should not be 
    /// allowed but they way project boundaries are defined is most likely to change making this a valid operation.
    /// This test currently checks the database to ensure that the associations exist but when/if getProjectGeofences is created
    /// the this should test that method.
    /// </summary>
    [TestMethod]
    public void Create_Then_Associate_Multiple_Project_Type_Geofences_with_Project()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      var geofenceGuid2 = Guid.NewGuid();
      var userGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 8";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("1000d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, 1);

      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName   | GeofenceType           | GeofenceUID     | GeometryWKT | IsTransparent | UserUID    | ",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Walls of Ston  | {GeofenceType.Project} | {geofenceGuid}  | 1,2,3       | {false}       | {userGuid} |" ,
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Hadrian's Wall | {GeofenceType.Project} | {geofenceGuid2} | 4,5,6       | {false}       | {userGuid} |"};

      testSupport.InjectEventsIntoKafka(geofenceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "UserUID", 2, userGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID",
        "fk_CustomerUID, Name", //Fields
        $"{customerGuid}, Walls of Ston", //Expected
        geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID",
        "fk_CustomerUID, Name", //Fields
        $"{customerGuid}, Hadrian's Wall", //Expected
        geofenceGuid2);

      testSupport.AssociateGeofenceProjectViaWebApi(projectGuid, geofenceGuid, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID",
        "fk_ProjectUID", //Fields
        $"{projectGuid}", //Expected
        geofenceGuid);

      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID",
        "fk_ProjectUID", //Fields
        $"{projectGuid}", //Expected
        geofenceGuid);

      testSupport.AssociateGeofenceProjectViaWebApi(projectGuid, geofenceGuid2, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid2);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID",
        "fk_ProjectUID", //Fields
        $"{projectGuid}", //Expected
        geofenceGuid);

      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid2);
      projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID",
        "fk_ProjectUID", //Fields
        $"{projectGuid}", //Expected
        geofenceGuid);

    }

    [TestMethod]
    public void Create_Then_Associate_Multiple_NonProject_Type_Geofences_with_Project()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      var geofenceGuid2 = Guid.NewGuid();
      var geofenceGuid3 = Guid.NewGuid();
      var userGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 8";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("1000d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, 1);

      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName   | GeofenceType            | GeofenceUID     | GeometryWKT | IsTransparent | UserUID    | ",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Sacsayhuamán   | {GeofenceType.Project}  | {geofenceGuid}  | 1,2,3       | {false}       | {userGuid} |" ,
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Walls of Troy  | {GeofenceType.Generic}  | {geofenceGuid2} | 4,5,6       | {false}       | {userGuid} |" ,
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Wailing Wall   | {GeofenceType.Landfill} | {geofenceGuid3} | 42,69,88    | {false}       | {userGuid} |"};

      testSupport.InjectEventsIntoKafka(geofenceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "UserUID", 3, userGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID",
        "fk_CustomerUID, Name", //Fields
        $"{customerGuid}, Sacsayhuamán", //Expected
        geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID",
        "fk_CustomerUID, Name", //Fields
        $"{customerGuid}, Walls of Troy", //Expected
        geofenceGuid2);

      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID",
        "fk_CustomerUID, Name", //Fields
        $"{customerGuid}, Wailing Wall", //Expected
        geofenceGuid3);

      testSupport.AssociateGeofenceProjectViaWebApi(projectGuid, geofenceGuid, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID",
        "fk_ProjectUID", //Fields
        $"{projectGuid}", //Expected
        geofenceGuid);

      testSupport.AssociateGeofenceProjectViaWebApi(projectGuid, geofenceGuid2, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid2);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID",
        "fk_ProjectUID", //Fields
        $"{projectGuid}", //Expected
        geofenceGuid2);

      testSupport.AssociateGeofenceProjectViaWebApi(projectGuid, geofenceGuid3, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid3);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID",
        "fk_ProjectUID", //Fields
        $"{projectGuid}", //Expected
        geofenceGuid3);

      var expectedProjects = new string[] {
            "| IsArchived  | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                 | ProjectUid    | LegacyProjectId | ProjectGeofenceWKT | ",
           $"| false       | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid} | 100             | 1,2,3              |" };
      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Project_Then_DissociateCustomer()
    {
      //TODO: This currently does nothing need to confirm that this is the intention.
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 9";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("400d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, 1);

      var expectedProjects = new string[] {
            "| IsArchived | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                 | ProjectUid    | LegacyProjectId | ",
           $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid} | 100             |" };

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);

      testSupport.DissociateProjectViaWebApi(projectGuid, customerGuid, DateTime.Now, HttpStatusCode.NotImplemented);

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);

    }


    [TestMethod]
    public void Try_To_Associate_Project_With_NonExistant_Customer()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();

      string projectName = $"Integration Test Project 10";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");
            
      testSupport.CreateMockProjectSubscription(projectGuid.ToString(), Guid.NewGuid().ToString(), customerGuid.ToString(), startDate, endDate, startDate);

      testSupport.CreateProjectViaWebApi(projectGuid, 100, projectName, startDate,
      endDate, "New Zealand Standard Time", ProjectType.ProjectMonitoring, DateTime.UtcNow, HttpStatusCode.OK);

      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid); //check that project is in webapi db
      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid); //check that project is in consumer db

      testSupport.AssociateCustomerProjectViaWebApi(projectGuid, customerGuid, 1, DateTime.UtcNow, HttpStatusCode.OK);

      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_ProjectUID", 1, projectGuid);

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, new string[] { });
    }


    [TestMethod]
    public void Try_To_Associate_Project_With_Multiple_Customers()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 11";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, 1);

      var expectedProjects = new string[] {
            "| IsArchived | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                 | ProjectUid    | LegacyProjectId | ",
           $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid} | 100             |" };

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
      testSupport.AssociateCustomerProjectViaWebApi(projectGuid, Guid.NewGuid(), 102, DateTime.Now, HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// TODO
    /// Currently this is allowed, although this may be revisited in the future
    /// </summary>
    [TestMethod]
    public void Try_To_Associate_Geofence_With_Multiple_Projects()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var projectGuid2 = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var customerGuid2 = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      var userGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 12";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("1000d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, 1);
      Create_And_Subscribe_AdditionalProjects_for_existing_Customer(testSupport, customerGuid, projectGuid2, projectName, startDate, endDate, 2);


      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName | GeofenceType | GeofenceUID    | GeometryWKT   | IsTransparent | UserUID    | ",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Berlin Wall  | 1            | {geofenceGuid} | 1,2,3         | {false}       | {userGuid} |"};

      testSupport.InjectEventsIntoKafka(geofenceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID",
        "fk_CustomerUID, Name", //Fields
        $"{customerGuid}, Berlin Wall", //Expected
        geofenceGuid);

      testSupport.AssociateGeofenceProjectViaWebApi(projectGuid, geofenceGuid, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID",
        "fk_ProjectUID", //Fields
        $"{projectGuid}", //Expected
        geofenceGuid);

      testSupport.AssociateGeofenceProjectViaWebApi(projectGuid2, geofenceGuid, DateTime.Now, HttpStatusCode.BadRequest);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_ProjectUID", 1, projectGuid2);


      var expectedProjects = new string[] {
            "| IsArchived  | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                 | ProjectUid     | LegacyProjectId | ProjectGeofenceWKT | ",
           $"| false       | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid}  | 100             | 1,2,3              |",
           $"| false       | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid2} | 100             | 1,2,3              |" };
      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Try_To_Create_Project_In_The_Past()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var projectGuid2 = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 13";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("400d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, 1);
      testSupport.CreateProjectViaWebApi(projectGuid2, 100, projectName, startDate.AddYears(-5),
        endDate.AddYears(-5), "New Zealand Standard Time", ProjectType.ProjectMonitoring, DateTime.UtcNow, HttpStatusCode.BadRequest);

      var expectedProjects = new string[] {
            "| IsArchived | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                 | ProjectUid    | LegacyProjectId | ",
           $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid} | 100             |" };

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Try_To_Create_Project_Which_Ends_Before_Starts()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var projectGuid2 = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 14";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("1000d+00:00:00");

      Create_Customer_Then_Project_And_Subscriptions(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, 1);

      testSupport.CreateProjectViaWebApi(projectGuid2, 100, projectName, endDate,
        startDate, "New Zealand Standard Time", ProjectType.ProjectMonitoring, DateTime.UtcNow, HttpStatusCode.BadRequest);


      var expectedProjects = new string[] {
            "| IsArchived | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                 | ProjectUid    | LegacyProjectId | ",
           $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid} | 100             |" };

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }

    [TestMethod]
    public void Create_Everything_Out_Of_Order()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var projectGuid2 = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 15";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("1000d+00:00:00");

      //create subscription
      testSupport.CreateMockProjectSubscription(projectGuid.ToString(), Guid.NewGuid().ToString(), customerGuid.ToString(), startDate, endDate, startDate);
      
      //Create and associate geofence
      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName         | GeofenceType | GeofenceUID    | GeometryWKT   | IsTransparent | UserUID          | ",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Great Wall of China  | 1            | {geofenceGuid} | 1,2,3         | {false}       | {Guid.NewGuid()} |"};

      testSupport.InjectEventsIntoKafka(geofenceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID",
        "fk_CustomerUID, Name", //Fields
        $"{customerGuid}, Great Wall of China", //Expected
        geofenceGuid);

      testSupport.AssociateGeofenceProjectViaWebApi(projectGuid, geofenceGuid, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID",
        "fk_ProjectUID", //Fields
        $"{projectGuid}", //Expected
        geofenceGuid);


      //create project
      testSupport.CreateProjectViaWebApi(projectGuid, 100, projectName, startDate,
      endDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, HttpStatusCode.OK);

      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid); //check that project is in webapi db
      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid); //check that project is in consumer db
      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, new string[] { });

      //Associate non existant customer with project
      testSupport.AssociateCustomerProjectViaWebApi(projectGuid, customerGuid, 1, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_ProjectUID", 1, projectGuid);
      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, new string[] { });

      //Create customer
      var customerEventArray = new[] {
             "| EventType           | EventDate   | CustomerName | CustomerType | CustomerUID    |",
            $"| CreateCustomerEvent | 0d+09:00:00 | E2ECust1     | Customer     | {customerGuid} |"};

      testSupport.InjectEventsIntoKafka(customerEventArray); //Create customer to associate project with

      mysql.VerifyTestResultDatabaseRecordCount("Customer", "CustomerUID", 1, customerGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Customer", "CustomerUID",
        "name", //Fields
        $"E2ECust1", //Expected
        customerGuid);

      
      var expectedProjects = new string[] {
            "| IsArchived | Name          | ProjectTimeZone           | ProjectType            | StartDate                 | EndDate                 | ProjectUid    | LegacyProjectId | ProjectGeofenceWKT | ",
           $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate.ToString("O")} | {endDate.ToString("O")} | {projectGuid} | 100             | 1,2,3              |" };

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }



    private void Create_Customer_Then_Project_And_Subscriptions(TestSupport testSupport, Guid customerGuid, Guid projectGuid, string projectName,  DateTime startDate, DateTime endDate, int numProjectsForCustomer, string timeZone = "New Zealand Standard Time")
    {
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var customerEventArray = new[] {
             "| EventType           | EventDate   | CustomerName | CustomerType | CustomerUID    |",
            $"| CreateCustomerEvent | 0d+09:00:00 | E2ECust1     | Customer     | {customerGuid} |"};

      testSupport.InjectEventsIntoKafka(customerEventArray); //Create customer to associate project with

      mysql.VerifyTestResultDatabaseRecordCount("Customer", "CustomerUID", 1, customerGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Customer", "CustomerUID",
        "name", //Fields
        $"E2ECust1", //Expected
        customerGuid);

      Create_And_Subscribe_AdditionalProjects_for_existing_Customer(testSupport, customerGuid, projectGuid, projectName, startDate, endDate, numProjectsForCustomer, ProjectType.Standard, timeZone);

      
    }

    private void Create_And_Subscribe_AdditionalProjects_for_existing_Customer(TestSupport testSupport, Guid customerGuid, Guid projectGuid, string projectName, DateTime startDate, DateTime endDate, int numProjectsForCustomer, ProjectType projectType = ProjectType.Standard, string timeZone = "New Zealand Standard Time")
    {
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(PROJECT_DB_SCHEMA_NAME);
      var subscriptionUid = Guid.NewGuid();
      var eventArray = new[] {
       "| EventType                         | EventDate   | StartDate  | EndDate    | SubscriptionType   | SubscriptionUID   | EffectiveDate | ProjectUID    |",
      $"| CreateProjectSubscriptionEvent    | 0d+12:00:00 | 2012-01-01 | 9999-12-31 | Project Monitoring | {subscriptionUid} |               |               |",
      $"| AssociateProjectSubscriptionEvent | 0d+09:00:00 |            |            |                    | {subscriptionUid} | 2012-01-01    | {projectGuid} |"}; 

      testSupport.InjectEventsIntoKafka(eventArray);
    //  testSupport.CreateMockProjectSubscription(projectGuid.ToString(), Guid.NewGuid().ToString(), customerGuid.ToString(), startDate, endDate, startDate);

      testSupport.CreateProjectViaWebApi(projectGuid, 100, projectName, startDate,
      endDate, timeZone, projectType , DateTime.UtcNow, HttpStatusCode.OK);

      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid); //check that project is in webapi db
      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid); //check that project is in consumer db

      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"{projectName}, 100, {(int)projectType}, {startDate}, {endDate}", //Expected
        projectGuid);

      projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"{projectName}, 100, {(int)projectType}, {startDate}, {endDate}", //Expected
        projectGuid);

      testSupport.AssociateCustomerProjectViaWebApi(projectGuid, customerGuid, 1, DateTime.UtcNow, HttpStatusCode.OK);

      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_CustomerUID", numProjectsForCustomer, customerGuid); //check that number of associated projects is as expected
      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_CustomerUID", numProjectsForCustomer, customerGuid);

      projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID",
        "fk_CustomerUID, fk_ProjectUID", //Fields
        $"{customerGuid}, {projectGuid}", //Expected
        projectGuid);

      mysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID",
        "fk_CustomerUID, fk_ProjectUID", //Fields
        $"{customerGuid}, {projectGuid}", //Expected
        projectGuid);
    }


  }

}
