﻿using System;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;
using VSS.MasterData.Models.Internal;
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
      var ts = new TestSupport { IsPublishToKafka = true };
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var projectName = "Integration Test Project 1";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00", ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);

      var expectedProjects = new[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID     | ProjectID    | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid}  | {projectId}  | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };

      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Multiple_Projects_And_Inject_Required_Data_Then_Retrieve()
    {
      var ts = new TestSupport { IsPublishToKafka = true };
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid1 = Guid.NewGuid();
      var projectGuid2 = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId1 = ts.SetLegacyProjectId();
      var projectId2 = projectId1 + 2;
      var projectName = "Integration Test Project 2";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00", ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid1, projectId1, projectName, startDate, endDate, 1);
      Create_And_Subscribe_AdditionalProjects_for_existing_Customer(ts, customerGuid, projectGuid2, projectId2, projectName, startDate, endDate, 2);

      var expectedProjects = new[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID     | ProjectID     | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid1} | {projectId1}  | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid2} | {projectId2}  | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };

      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }

    [TestMethod]
    public void Create_Then_Update_Project_Name()
    {
      var ts = new TestSupport { IsPublishToKafka = true };
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectName = "Integration Test Project 3";
      var projectId = ts.SetLegacyProjectId();
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00", ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);
      ts.UpdateProjectViaWebApiV3(projectGuid, "New Name", endDate, "New Zealand Standard Time", DateTime.UtcNow, HttpStatusCode.OK);

      var expectedProjects = new[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID     | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | New Name      | New Zealand Standard Time | {ProjectType.Standard} |{startDate:O}     | {endDate:O}    | {projectGuid} | {projectId}   | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Then_Update_Project_EndDate()
    {
      var ts = new TestSupport { IsPublishToKafka = true };
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var projectName = "Integration Test Project 4";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00", ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1, "Central Standard Time");

      ts.UpdateProjectViaWebApiV3(projectGuid, projectName, endDate.AddDays(10), "Central Standard Time", DateTime.UtcNow, HttpStatusCode.OK);
      var expectedProjects = new[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate          | ProjectUID    | ProjectID     | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | Central Standard Time     | {ProjectType.Standard} | {startDate:O}    | {endDate.AddDays(10):O} | {projectGuid} | {projectId}   | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Then_Try_Update_Project_TimeZone()
    {
      var ts = new TestSupport { IsPublishToKafka = true };
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var projectName = "Integration Test Project 5";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00", ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);

      ts.UpdateProjectViaWebApiV3(projectGuid, projectName, endDate, "Central Standard Time", DateTime.UtcNow, HttpStatusCode.Forbidden);
      var expectedProjects = new[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID     | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId}   | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }

    [TestMethod]
    public void Create_Project_Then_Update_Project_Type()
    {
      var ts = new TestSupport { IsPublishToKafka = true };
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var projectName = "Integration Test Project 5";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00", ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);

      ts.UpdateProjectViaWebApiV3(projectGuid, projectName, endDate, "New Zealand Standard Time", DateTime.UtcNow, HttpStatusCode.OK, ProjectType.ProjectMonitoring);
      var expectedProjects = new[] {
      "| IsArchived | ProjectName   | ProjectTimezone            | ProjectType                     | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time  | {ProjectType.ProjectMonitoring} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Then_Delete_Project()
    {
      var ts = new TestSupport { IsPublishToKafka = true };
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var projectName = "Integration Test Project 6";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00", ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);
      var expectedProjects = new[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
      ts.DeleteProjectViaWebApiV3(projectGuid, DateTime.UtcNow, HttpStatusCode.OK);

      // on project deletion, endDate is set to now, in the projects timezone.
      var endDateReset = DateTime.UtcNow.ToLocalDateTime("Pacific/Auckland")?.Date.ToString("O");
      expectedProjects = new[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| true       | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDateReset} | {projectGuid} | {projectId} | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Then_Associate_Geofence_with_Project()
    {
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      var userGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var projectName = "Integration Test Project 7";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("1000d+00:00:00", ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);

      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName | GeofenceType | GeofenceUID    | GeometryWKT | IsTransparent | UserUID    | ",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Trump        | 1            | {geofenceGuid} | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) | {false}       | {userGuid} | "};

      ts.PublishEventCollection(geofenceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID", "fk_CustomerUID, Name", $"{customerGuid}, Trump", geofenceGuid);

      ts.AssociateGeofenceProjectViaWebApiV3(projectGuid, geofenceGuid, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID", "fk_ProjectUID", $"{projectGuid}", geofenceGuid);

      var expectedProjects = new[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid}   | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))  |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Then_Associate_Multiple_NonProject_Type_Geofences_with_Project()
    {
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      var geofenceGuid2 = Guid.NewGuid();
      var geofenceGuid3 = Guid.NewGuid();
      var userGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var projectName = "Integration Test Project 8";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("1000d+00:00:00", ts.FirstEventDate);
      var geometryWKT =
        "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);

      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName   | GeofenceType            | GeofenceUID     | GeometryWKT   | IsTransparent | UserUID    | ",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Sacsayhuamán   | {GeofenceType.Project}  | {geofenceGuid}  | {geometryWKT} | {false}       | {userGuid} |" ,
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Walls of Troy  | {GeofenceType.Generic}  | {geofenceGuid2} | {geometryWKT} | {false}       | {userGuid} |" ,
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Wailing Wall   | {GeofenceType.Landfill} | {geofenceGuid3} | {geometryWKT} | {false}       | {userGuid} |"};

      ts.PublishEventCollection(geofenceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "UserUID", 3, userGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID", "fk_CustomerUID, Name", $"{customerGuid}, Sacsayhuamán", geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID", "fk_CustomerUID, Name", $"{customerGuid}, Walls of Troy", geofenceGuid2);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID", "fk_CustomerUID, Name", $"{customerGuid}, Wailing Wall", geofenceGuid3);

      ts.AssociateGeofenceProjectViaWebApiV3(projectGuid, geofenceGuid, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID", "fk_ProjectUID", $"{projectGuid}", geofenceGuid);

      ts.AssociateGeofenceProjectViaWebApiV3(projectGuid, geofenceGuid2, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid2);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID", "fk_ProjectUID", $"{projectGuid}", geofenceGuid2);

      ts.AssociateGeofenceProjectViaWebApiV3(projectGuid, geofenceGuid3, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid3);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID", "fk_ProjectUID", $"{projectGuid}", geofenceGuid3);

      var expectedProjects = new[] {
      "| IsArchived | ProjectName    | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID    | LegacyCustomerId | ProjectBoundary | ",
     $"| false       | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid} | 1                | {geometryWKT}   |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }

    [TestMethod]
    public void Try_To_Associate_Project_With_NonExistant_Customer()
    {
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var projectName = "Integration Test Project 10";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00", ts.FirstEventDate);

      ts.CreateMockProjectSubscription(projectGuid.ToString(), Guid.NewGuid().ToString(), customerGuid.ToString(), startDate, endDate, startDate);
      ts.CreateProjectViaWebApiV3(projectGuid, projectId, projectName, startDate, endDate, "New Zealand Standard Time", ProjectType.ProjectMonitoring, DateTime.UtcNow, "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))", HttpStatusCode.OK);

      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      ts.AssociateCustomerProjectViaWebApiV3(projectGuid, customerGuid, 1, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_ProjectUID", 1, projectGuid);
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, new string[] { });
    }


    [TestMethod]
    public void Try_To_Associate_Project_With_Multiple_Customers()
    {
      var ts = new TestSupport { IsPublishToKafka = true };
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var customerGuid = Guid.NewGuid();
      var projectName = "Integration Test Project 11";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00", ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);

      var expectedProjects = new[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID    | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid} | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };

      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
      ts.AssociateCustomerProjectViaWebApiV3(projectGuid, Guid.NewGuid(), 102, DateTime.UtcNow, HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Currently this is allowed, although this may be revisited in the future
    /// </summary>
    [TestMethod]
    public void Try_To_Associate_Geofence_With_Multiple_Projects()
    {
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var projectGuid2 = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      var userGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var projectId2 = projectId + 2;
      var projectName = "Integration Test Project 12";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("1000d+00:00:00", ts.FirstEventDate);
      var geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);
      Create_And_Subscribe_AdditionalProjects_for_existing_Customer(ts, customerGuid, projectGuid2, projectId2, projectName, startDate, endDate, 2);

      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName | GeofenceType | GeofenceUID    | GeometryWKT   | IsTransparent | UserUID    | ",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Berlin Wall  | 1            | {geofenceGuid} | {geometryWkt} | {false}       | {userGuid} |"};

      ts.PublishEventCollection(geofenceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID", "fk_CustomerUID, Name", $"{customerGuid}, Berlin Wall", geofenceGuid);

      ts.AssociateGeofenceProjectViaWebApiV3(projectGuid, geofenceGuid, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID", "fk_ProjectUID", $"{projectGuid}", geofenceGuid);

      ts.AssociateGeofenceProjectViaWebApiV3(projectGuid2, geofenceGuid, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_ProjectUID", 1, projectGuid2);

      var expectedProjects = new[] {
      "| IsArchived | ProjectName    | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID     | ProjectID   | CustomerUID    | LegacyCustomerId | ProjectBoundary | ",
     $"| false       | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid}  | {projectId} | {customerGuid} | 1                | {geometryWkt}   |",
     $"| false       | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid2} | {projectId2}| {customerGuid} | 1                | {geometryWkt}   |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }


    [TestMethod]
    public void Create_Project_In_The_Past()
    {
      var ts = new TestSupport { IsPublishToKafka = true };
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var projectGuid2 = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var projectName = "Integration Test Project 13";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("400d+00:00:00", ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);
      ts.CreateProjectViaWebApiV3(projectGuid2, projectId, projectName, startDate.AddYears(-5), endDate.AddYears(-5), "New Zealand Standard Time", ProjectType.ProjectMonitoring, DateTime.UtcNow, "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))", HttpStatusCode.OK);

      var expectedProjects = new[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID    | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid} | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };

      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }

    [TestMethod]
    public void Try_To_Create_Project_Which_Ends_Before_Starts()
    {
      var ts = new TestSupport { IsPublishToKafka = true };
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var projectGuid2 = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var projectName = "Integration Test Project 14";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("1000d+00:00:00", ts.FirstEventDate);

      Create_Customer_Then_Project_And_Subscriptions(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, 1);
      ts.CreateProjectViaWebApiV3(projectGuid2, projectId, projectName, endDate, startDate, "New Zealand Standard Time", ProjectType.ProjectMonitoring, DateTime.UtcNow, "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))", HttpStatusCode.BadRequest);

      var expectedProjects = new[] {
      "| IsArchived | ProjectName   | ProjectTimezone           | ProjectType            | ProjectStartDate | ProjectEndDate | ProjectUID    | ProjectID   | CustomerUID    | LegacyCustomerId | ProjectBoundary | ",
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid} | 1                | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }

    [TestMethod]
    public void Create_Everything_Out_Of_Order()
    {
      var ts = new TestSupport { IsPublishToKafka = true };
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      var projectId = ts.SetLegacyProjectId();
      var projectName = "Integration Test Project 15";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("1000d+00:00:00", ts.FirstEventDate);

      //create subscription
      ts.CreateMockProjectSubscription(projectGuid.ToString(), Guid.NewGuid().ToString(), customerGuid.ToString(), startDate, endDate, startDate);

      //Create and associate geofence
      var geometryWKT =
        "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName         | GeofenceType | GeofenceUID    | GeometryWKT   | IsTransparent | UserUID          | ",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | Great Wall of China  | 1            | {geofenceGuid} | {geometryWKT} | {false}       | {Guid.NewGuid()} |"};

      ts.PublishEventCollection(geofenceEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Geofence", "GeofenceUID", "fk_CustomerUID, Name", $"{customerGuid}, Great Wall of China", geofenceGuid);

      ts.AssociateGeofenceProjectViaWebApiV3(projectGuid, geofenceGuid, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID", "fk_ProjectUID", $"{projectGuid}", geofenceGuid);

      //create project
      ts.CreateProjectViaWebApiV3(projectGuid, projectId, projectName, startDate, endDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, geometryWKT, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid); //check that project is in webapi db
      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid); //check that project is in consumer db
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, new string[] { });

      //Associate non existant customer with project
      ts.AssociateCustomerProjectViaWebApiV3(projectGuid, customerGuid, 1, DateTime.UtcNow, HttpStatusCode.OK);
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
     $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate:O}    | {endDate:O}    | {projectGuid} | {projectId} | {customerGuid} | 1                | {geometryWKT}   |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);
    }

    [TestMethod]
    public void CreateStandardProjectThenUpdateProjectTypeToLandFillAndCheckDatabase()
    {
      var msg = new Msg();
      var mysql = new MySqlHelper();
      msg.Title("Project Integration test", "Create standard project then update project type to landfill and check project-only database");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.UtcNow.Date.AddYears(2);
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
      var projectName = "Standard To LandFill test";
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
      Thread.Sleep(2000);
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", "fk_ProjectTypeID", "1", projectUid);
    }

    [TestMethod]
    public void CreateStandardProjectThenUpdateProjectTypeToCivilAndCheckDatabase()
    {
      var msg = new Msg();
      var mysql = new MySqlHelper();
      msg.Title("Project Integration test", "Create standard project then update project type to civil project monitoring and check project-only database");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.UtcNow.Date.AddYears(2);
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
      var projectName = "Standard To Civil test";
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
      Thread.Sleep(2000);
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", "fk_ProjectTypeID", "2", projectUid);
    }


    private void Create_Customer_Then_Project_And_Subscriptions(TestSupport ts, Guid customerGuid, Guid projectGuid, int projectId, string projectName, DateTime startDate, DateTime endDate, int numProjectsForCustomer, string timeZone = "New Zealand Standard Time")
    {
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
      var customerEventArray = new[] {
       "| EventType           | EventDate   | CustomerName | CustomerType | CustomerUID    |",
      $"| CreateCustomerEvent | 0d+09:00:00 | E2ECust1     | Customer     | {customerGuid} |"};

      ts.PublishEventCollection(customerEventArray); //Create customer to associate project with
      mysql.VerifyTestResultDatabaseRecordCount("Customer", "CustomerUID", 1, customerGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Customer", "CustomerUID", "name", "E2ECust1", customerGuid);
      Create_And_Subscribe_AdditionalProjects_for_existing_Customer(ts, customerGuid, projectGuid, projectId, projectName, startDate, endDate, numProjectsForCustomer, ProjectType.Standard, timeZone);
    }

    private void Create_And_Subscribe_AdditionalProjects_for_existing_Customer(TestSupport ts, Guid customerGuid, Guid projectGuid, int projectId, string projectName, DateTime startDate, DateTime endDate, int numProjectsForCustomer, ProjectType projectType = ProjectType.Standard, string timeZone = "New Zealand Standard Time")
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

      ts.CreateProjectViaWebApiV3(projectGuid, projectId, projectName, startDate, endDate, timeZone, projectType, DateTime.UtcNow, "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))", HttpStatusCode.OK);

      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", $"{projectName}, {projectId}, {(int)projectType}, {startDate}, {endDate}", projectGuid);
      projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", $"{projectName},{projectId}, {(int)projectType}, {startDate}, {endDate}", projectGuid);

      ts.AssociateCustomerProjectViaWebApiV3(projectGuid, customerGuid, 1, DateTime.UtcNow, HttpStatusCode.OK);

      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_CustomerUID", numProjectsForCustomer, customerGuid); //check that number of associated projects is as expected
      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_CustomerUID", numProjectsForCustomer, customerGuid);
      projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID", "fk_CustomerUID, fk_ProjectUID", $"{customerGuid}, {projectGuid}", projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID", "fk_CustomerUID, fk_ProjectUID", $"{customerGuid}, {projectGuid}", projectGuid);
    }
  }
}
