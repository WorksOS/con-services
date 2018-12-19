using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using TestUtility;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace IntegrationTests
{
  [TestClass]
  public class ProjectIntTests
  {
    private const string PROJECT_DB_SCHEMA_NAME = "VSS-MasterData-Project-Only";

    [TestMethod]
    public void Create_Project_Then_Retrieve()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var projectName = "Integration Test Project 1";
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);

      var expectedProjects = new[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID     | ProjectID    | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid}  | {projectId}  | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };

      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Multiple_Projects_And_Inject_Required_Data_Then_Retrieve()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid1 = Guid.NewGuid();
      var projectGuid2 = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId1 = ts.SetLegacyProjectId();
      var projectId2 = projectId1+2;
      string projectName = $"Integration Test Project 2";
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid1, projectId1, projectName, startDate, endDate, 1);
      Create_And_Subscribe_AdditionalProjects_for_existing_Customer(ts, customerGuid, projectGuid2,projectId2,projectName, startDate, endDate, 2);

      var expectedProjects = new [] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID     | ProjectID     | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid1} | {projectId1}  | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid2} | {projectId2}  | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };

      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }

    [TestMethod]
    public void Create_Then_Update_Project_Name()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 3";
      var projectId = ts.SetLegacyProjectId();
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid,projectId, projectName, startDate, endDate, 1);
      ts.UpdateProjectViaWebApi(projectGuid, "New Name", endDate, "New Zealand Standard Time", DateTime.Now, HttpStatusCode.OK);

      var expectedProjects = new string[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID     | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | New Name      | New Zealand Standard Time | {ProjectType.Standard} |{startDate:O}     | {endDate:O}    | {projectGuid} | {projectId}   | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Then_Update_Project_EndDate()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      string projectName = $"Integration Test Project 4";
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1, "Central Standard Time");

      ts.UpdateProjectViaWebApi(projectGuid, projectName, endDate.AddDays(10), "Central Standard Time", DateTime.Now, HttpStatusCode.OK);
      var expectedProjects = new string[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate          | ProjectUID    | ProjectID     | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | Central Standard Time     | {ProjectType.Standard} | {startDate:O}    | {endDate.AddDays(10):O} | {projectGuid} | {projectId}   | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Then_Try_Update_Project_TimeZone()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      string projectName = $"Integration Test Project 5";
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid,projectId, projectName, startDate, endDate, 1);

      ts.UpdateProjectViaWebApi(projectGuid, projectName, endDate, "Central Standard Time", DateTime.Now, HttpStatusCode.Forbidden);
      var expectedProjects = new string[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID     | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId}   | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }

    [TestMethod]
    public void Create_Project_Then_Update_Project_Type()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      string projectName = $"Integration Test Project 5";
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);

      ts.UpdateProjectViaWebApi(projectGuid, projectName, endDate, "New Zealand Standard Time", DateTime.Now, HttpStatusCode.OK, ProjectType.ProjectMonitoring);
      var expectedProjects = new string[] {
      "| IsArchived | ProjectName   | ProjectTimezone            | ProjectType                     | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time  | {ProjectType.ProjectMonitoring} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Then_Delete_Project()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      string projectName = $"Integration Test Project 6";
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);
      var expectedProjects = new string[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
      ts.DeleteProjectViaWebApi(projectGuid, DateTime.Now, HttpStatusCode.OK);
      expectedProjects = new string[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| true       | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Then_Associate_Geofence_with_Project()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      var userGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      string projectName = $"Integration Test Project 7";
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("1000d+00:00:00",ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);
      
      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName | GeofenceType | GeofenceUID    | GeometryWKT | IsTransparent | UserUID    | ",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Trump        | 1            | {geofenceGuid} | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) | {false}       | {userGuid} | "};

      ts.PublishEventCollection(geofenceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID","fk_CustomerUID, Name", $"{customerGuid}, Trump", geofenceGuid);

      ts.AssociateGeofenceProjectViaWebApi(projectGuid, geofenceGuid, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID","fk_ProjectUID", $"{projectGuid}", geofenceGuid);

      var expectedProjects = new string[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))  |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }
    

    [TestMethod]
    public void Create_Then_Associate_Multiple_NonProject_Type_Geofences_with_Project()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      var geofenceGuid2 = Guid.NewGuid();
      var geofenceGuid3 = Guid.NewGuid();
      var userGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      string projectName = $"Integration Test Project 8";
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("1000d+00:00:00",ts.FirstEventDate);
      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid,projectId, projectName, startDate, endDate, 1);

      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName   | GeofenceType            | GeofenceUID     | GeometryWKT | IsTransparent | UserUID    | ",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Sacsayhuamán   | {GeofenceType.Project}  | {geofenceGuid}  | 1,2,3       | {false}       | {userGuid} |" ,
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Walls of Troy  | {GeofenceType.Generic}  | {geofenceGuid2} | 4,5,6       | {false}       | {userGuid} |" ,
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Wailing Wall   | {GeofenceType.Landfill} | {geofenceGuid3} | 42,69,88    | {false}       | {userGuid} |"};

      ts.PublishEventCollection(geofenceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "UserUID", 3, userGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID","fk_CustomerUID, Name", $"{customerGuid}, Sacsayhuamán",geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID","fk_CustomerUID, Name", $"{customerGuid}, Walls of Troy", geofenceGuid2);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID","fk_CustomerUID, Name",$"{customerGuid}, Wailing Wall",geofenceGuid3);

      ts.AssociateGeofenceProjectViaWebApi(projectGuid, geofenceGuid, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID","fk_ProjectUID",$"{projectGuid}",geofenceGuid);

      ts.AssociateGeofenceProjectViaWebApi(projectGuid, geofenceGuid2, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid2);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID","fk_ProjectUID",$"{projectGuid}",geofenceGuid2);

      ts.AssociateGeofenceProjectViaWebApi(projectGuid, geofenceGuid3, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid3);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID","fk_ProjectUID",$"{projectGuid}",geofenceGuid3);

      var expectedProjects = new string[] {
      "| IsArchived | ProjectName    | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false       | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid} | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))    |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    // Dissociating projects from customers is not supported 
    //[Ignore]
    //[TestMethod]
    //public void Create_Project_Then_DissociateCustomer()
    //{
    //  var msg = new Msg();
    //  var ts = new TestSupport { IsPublishToKafka = true};
    //  var mysql = new MySqlHelper();
    //  var projectConsumerMysql = new MySqlHelper();
    //  projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
    //  var projectGuid = Guid.NewGuid();
    //  var customerGuid = Guid.NewGuid();
    //  var projectId = ts.SetLegacyProjectId();
    //  string projectName = $"Integration Test Project 9";
    //  DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
    //  DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("400d+00:00:00",ts.FirstEventDate);

    //  Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);

    //  var expectedProjects = new string[] {
    //  "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID    | LegacyCustomerId | ProjectBoundary | ",
    // $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid} | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };

    //  ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    //  ts.DissociateProjectViaWebApi(projectGuid, customerGuid, DateTime.Now, HttpStatusCode.NotImplemented);
    //  ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    //}


    [TestMethod]
    public void Try_To_Associate_Project_With_NonExistant_Customer()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      string projectName = $"Integration Test Project 10";
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",ts.FirstEventDate);
            
      ts.CreateMockProjectSubscription(projectGuid.ToString(), Guid.NewGuid().ToString(), customerGuid.ToString(), startDate, endDate, startDate);
      ts.CreateProjectViaWebApi(projectGuid, projectId, projectName, startDate, endDate, "New Zealand Standard Time", ProjectType.ProjectMonitoring, DateTime.UtcNow, "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))", HttpStatusCode.OK);

      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      ts.AssociateCustomerProjectViaWebApi(projectGuid, customerGuid, 1, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_ProjectUID", 1, projectGuid);
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, new string[] { });
    }


    [TestMethod]
    public void Try_To_Associate_Project_With_Multiple_Customers()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var customerGuid = Guid.NewGuid();
      var projectName = "Integration Test Project 11";
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);

      var expectedProjects = new string[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID    | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid} | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };

      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
      ts.AssociateCustomerProjectViaWebApi(projectGuid, Guid.NewGuid(), 102, DateTime.Now, HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Currently this is allowed, although this may be revisited in the future
    /// </summary>
    [TestMethod] 
    public void Try_To_Associate_Geofence_With_Multiple_Projects()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var projectGuid2 = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      var userGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var projectId2 = projectId+2;
      var projectName = "Integration Test Project 12";
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("1000d+00:00:00",ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId,projectName, startDate, endDate, 1);
      Create_And_Subscribe_AdditionalProjects_for_existing_Customer(ts, customerGuid, projectGuid2,projectId2, projectName, startDate, endDate, 2);

      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName | GeofenceType | GeofenceUID    | GeometryWKT   | IsTransparent | UserUID    | ",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Berlin Wall  | 1            | {geofenceGuid} | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) | {false}       | {userGuid} |"};

      ts.PublishEventCollection(geofenceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID","fk_CustomerUID, Name", $"{customerGuid}, Berlin Wall", geofenceGuid);

      ts.AssociateGeofenceProjectViaWebApi(projectGuid, geofenceGuid, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID", "fk_ProjectUID", $"{projectGuid}", geofenceGuid);

      ts.AssociateGeofenceProjectViaWebApi(projectGuid2, geofenceGuid, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_ProjectUID", 1, projectGuid2);

      var expectedProjects = new string[] {
      "| IsArchived | ProjectName    | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID     | ProjectID   | CustomerUID    | LegacyCustomerId | ProjectBoundary | ",
     $"| false       | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid}  | {projectId} | {customerGuid} | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
     $"| false       | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid2} | {projectId2}| {customerGuid} | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Project_In_The_Past()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var projectGuid2 = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      string projectName = $"Integration Test Project 13";
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("400d+00:00:00",ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);
      ts.CreateProjectViaWebApi(projectGuid2,projectId, projectName, startDate.AddYears(-5),endDate.AddYears(-5), "New Zealand Standard Time", ProjectType.ProjectMonitoring, DateTime.UtcNow, "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))", HttpStatusCode.OK);

      var expectedProjects = new [] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID    | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid} | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };

      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }

    [TestMethod]
    public void Try_To_Create_Project_Which_Ends_Before_Starts()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var projectGuid2 = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      string projectName = $"Integration Test Project 14";
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("1000d+00:00:00",ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);
      ts.CreateProjectViaWebApi(projectGuid2, projectId, projectName, endDate,startDate, "New Zealand Standard Time", ProjectType.ProjectMonitoring, DateTime.UtcNow, "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))", HttpStatusCode.BadRequest);

      var expectedProjects = new string[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID    | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid} | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }

    [TestMethod]
    public void Create_Everything_Out_Of_Order()
    {
      var msg = new Msg();
      var ts = new TestSupport { IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      string projectName = $"Integration Test Project 15";
      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("1000d+00:00:00",ts.FirstEventDate);

      //create subscription
      ts.CreateMockProjectSubscription(projectGuid.ToString(), Guid.NewGuid().ToString(), customerGuid.ToString(), startDate, endDate, startDate);
      
      //Create and associate geofence
      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName         | GeofenceType | GeofenceUID    | GeometryWKT   | IsTransparent | UserUID          | ",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Great Wall of China  | 1            | {geofenceGuid} | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))   | {false}       | {Guid.NewGuid()} |"};

      ts.PublishEventCollection(geofenceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID", "fk_CustomerUID, Name", $"{customerGuid}, Great Wall of China", geofenceGuid);

      ts.AssociateGeofenceProjectViaWebApi(projectGuid, geofenceGuid, DateTime.Now, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID", "fk_ProjectUID", $"{projectGuid}", geofenceGuid);

      //create project
      ts.CreateProjectViaWebApi(projectGuid, projectId, projectName, startDate, endDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))", HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid); //check that project is in webapi db
      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid); //check that project is in consumer db
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, new string[] { });

      //Associate non existant customer with project
      ts.AssociateCustomerProjectViaWebApi(projectGuid, customerGuid, 1, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_ProjectUID", 1, projectGuid);
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, new string[] { });

      //Create customer
      var customerEventArray = new[] {
       "| EventType           | EventDate   | CustomerName | CustomerType | CustomerUID    |",
      $"| CreateCustomerEvent | 0d+09:00:00 | E2ECust1     | Customer     | {customerGuid} |"};

      ts.PublishEventCollection(customerEventArray); //Create customer to associate project with
      mysql.VerifyTestResultDatabaseRecordCount("Customer", "CustomerUID", 1, customerGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Customer", "CustomerUID", "name", "E2ECust1", customerGuid);
      
      var expectedProjects = new[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID    | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid} | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))              |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }

    [TestMethod]
    public void CreateStandardProjectThenUpdateProjectTypeToLandFillAndCheckDatabase()
    {
      var msg = new Msg();
      var mysql = new MySqlHelper();
      msg.Title($"Project Integration test", "Create standard project then update project type to landfill and check project-only database");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.Now.Date.AddYears(2);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"};
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);
      ts.IsPublishToWebApi = true;
      var projectName = $"Standard To LandFill test";
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc |" };
      ts.PublishEventCollection(projectEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectUid);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid.ToString(), projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description       |",
      $"| UpdateProjectRequest | 0d+10:00:00 | {projectUid} | {projectName} | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | dummy description |" };
      var response = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid.ToString(), projectEventArray2, true);

      // Now check the project type has changed and the kafka message consumed properly.
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", "fk_ProjectTypeID", "1", projectUid);
    }

    [TestMethod]
    public void CreateStandardProjectThenUpdateProjectTypeToCivilAndCheckDatabase()
    {
      var msg = new Msg();
      var mysql = new MySqlHelper();
      msg.Title($"Project Integration test", "Create standard project then update project type to civil project monitoring and check project-only database");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.Now.Date.AddYears(2);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |"};
      ts.PublishEventCollection(eventsArray);
      mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);

      ts.IsPublishToWebApi = true;
      var projectName = $"Standard To LandFill test";
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc |" };
      ts.PublishEventCollection(projectEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectUid);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid.ToString(), projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName   | ProjectType                     | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description       |",
      $"| UpdateProjectRequest | 0d+10:00:00 | {projectUid} | {projectName} | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | dummy description |" };
      var response = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid.ToString(), projectEventArray2, true);

      // Now check the project type has changed and the kafka message consumed properly.
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", "fk_ProjectTypeID", "2", projectUid);
    }


    private void Create_Customer_Then_Project_And_Subscriptions(TestSupport ts, Guid customerGuid, Guid projectGuid, int projectId, string projectName,  DateTime startDate, DateTime endDate, int numProjectsForCustomer, string timeZone = "New Zealand Standard Time")
    {
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var customerEventArray = new[] {
       "| EventType           | EventDate   | CustomerName | CustomerType | CustomerUID    |",
      $"| CreateCustomerEvent | 0d+09:00:00 | E2ECust1     | Customer     | {customerGuid} |"};

      ts.PublishEventCollection(customerEventArray); //Create customer to associate project with
      mysql.VerifyTestResultDatabaseRecordCount("Customer", "CustomerUID", 1, customerGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Customer", "CustomerUID","name", $"E2ECust1", customerGuid);
      Create_And_Subscribe_AdditionalProjects_for_existing_Customer(ts, customerGuid, projectGuid,projectId, projectName, startDate, endDate, numProjectsForCustomer, ProjectType.Standard, timeZone);     
    }

    private void Create_And_Subscribe_AdditionalProjects_for_existing_Customer(TestSupport ts, Guid customerGuid, Guid projectGuid,int projectId, string projectName, DateTime startDate, DateTime endDate, int numProjectsForCustomer, ProjectType projectType = ProjectType.Standard, string timeZone = "New Zealand Standard Time")
    {
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var subscriptionUid = Guid.NewGuid();
      var eventArray = new[] {
       "| EventType                         | EventDate   | StartDate  | EndDate    | SubscriptionType   | SubscriptionUID   | EffectiveDate | ProjectUID    | CustomerUID    |",
      $"| CreateProjectSubscriptionEvent    | 0d+12:00:00 | 2012-01-01 | 9999-12-31 | Project Monitoring | {subscriptionUid} |               |               | {customerGuid} |",
      $"| AssociateProjectSubscriptionEvent | 0d+09:00:00 |            |            |                    | {subscriptionUid} | 2012-01-01    | {projectGuid} |                |"}; 

      ts.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectSubscription", "fk_SubscriptionUID", 1, subscriptionUid); // Test the database record is there
      //  ts.CreateMockProjectSubscription(projectGuid.ToString(), Guid.NewGuid().ToString(), customerGuid.ToString(), startDate, endDate, startDate);

      ts.CreateProjectViaWebApi(projectGuid,projectId, projectName, startDate,endDate, timeZone, projectType , DateTime.UtcNow, "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))", HttpStatusCode.OK);

      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid); 
      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid); 
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID","Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", $"{projectName}, {projectId}, {(int)projectType}, {startDate}, {endDate}", projectGuid);
      projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID","Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", $"{projectName},{projectId}, {(int)projectType}, {startDate}, {endDate}", projectGuid);

      ts.AssociateCustomerProjectViaWebApi(projectGuid, customerGuid, 1, DateTime.UtcNow, HttpStatusCode.OK);

      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_CustomerUID", numProjectsForCustomer, customerGuid); //check that number of associated projects is as expected
      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_CustomerUID", numProjectsForCustomer, customerGuid);
      projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID","fk_CustomerUID, fk_ProjectUID",$"{customerGuid}, {projectGuid}",projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID","fk_CustomerUID, fk_ProjectUID",$"{customerGuid}, {projectGuid}",projectGuid);
    }
  }
}
