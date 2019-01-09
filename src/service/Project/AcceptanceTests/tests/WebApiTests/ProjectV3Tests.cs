using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using TestUtility;
using VSS.MasterData.Models.Internal;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiTests
{
  [TestClass]
  public class ProjectV3Tests
  {
    private const string PROJECT_BOUNDARY = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";

    [TestMethod]
    public void Create_Project_All_Ok()
    {
      var msg = new Msg();
      msg.Title("projects 1", "Create a project");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var projectId = ts.SetLegacyProjectId();
      CreateProject(ts, mysql, projectId, "project 1", ProjectType.Standard);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", "LegacyProjectID,Name,ProjectTimezone,StartDate,EndDate", $"{projectId},project 1,New Zealand Standard Time, {ts.FirstEventDate}, {ts.LastEventDate}", ts.ProjectUid);
    }

    [TestMethod]
    public void Create_Project_Twice()
    {
      var msg = new Msg();
      msg.Title("projects 2", "Create a project twice");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var projectId = ts.SetLegacyProjectId();
      CreateProject(ts, mysql, projectId, "project 2", ProjectType.Standard);
      ts.CreateProjectViaWebApi(ts.ProjectUid, projectId, "project 2", ts.FirstEventDate, ts.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public void Create_Project_Bad_Data()
    {
      var msg = new Msg();
      msg.Title("projects 3", "Create a project with bad data");

      var ts = new TestSupport();
      var projectId = ts.SetLegacyProjectId();
      //No action UTC
      ts.CreateProjectViaWebApi(ts.ProjectUid, projectId, "project 3", ts.FirstEventDate, ts.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.MinValue, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //No project UID
      ts.CreateProjectViaWebApi(Guid.Empty, projectId, "project 3", ts.FirstEventDate, ts.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //No time zone
      ts.CreateProjectViaWebApi(ts.ProjectUid, projectId, "project 3", ts.FirstEventDate, ts.LastEventDate, null, ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //No project name
      ts.CreateProjectViaWebApi(ts.ProjectUid, projectId, null, ts.FirstEventDate, ts.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //No start date
      ts.CreateProjectViaWebApi(ts.ProjectUid, projectId, "project 3", DateTime.MinValue, ts.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //No end date
      ts.CreateProjectViaWebApi(ts.ProjectUid, projectId, "project 3", ts.FirstEventDate, DateTime.MinValue, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //Bad end date
      //    ts.CreateProjectViaWebApi(ts.ProjectUid, projectId, "project 3", ts.FirstEventDate, ts.FirstEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //Bad date range
      ts.CreateProjectViaWebApi(ts.ProjectUid, projectId, "project 3", ts.LastEventDate, ts.FirstEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //No legacy project ID
      // ts.CreateProjectViaWebApi(ts.ProjectUid, 0, "project 3", ts.FirstEventDate, ts.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //No boundary
      ts.CreateProjectViaWebApi(ts.ProjectUid, projectId, "project 3", ts.FirstEventDate, ts.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, null, HttpStatusCode.BadRequest);
      //Invalid boundary
      ts.CreateProjectViaWebApi(ts.ProjectUid, projectId, "project 3", ts.FirstEventDate, ts.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, "POLYGON((-121.347189366818 38.8361907402694))", HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public void Update_Project_After_Create()
    {
      var msg = new Msg();
      msg.Title("projects 4", "Update a project after create");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var projectId = ts.SetLegacyProjectId();
      CreateProject(ts, mysql, projectId, "project 4", ProjectType.Standard);
      var updatedEndDate = ts.FirstEventDate.AddYears(3);
      ts.UpdateProjectViaWebApi(ts.ProjectUid, "project 4 updated", updatedEndDate, "New Zealand Standard Time", DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", "Name,EndDate", "project 4 updated," + updatedEndDate, ts.ProjectUid);
    }

    [TestMethod]
    public void Update_Project_Before_Create()
    {
      var msg = new Msg();
      msg.Title("projects 5", "Update a project before create");
      var ts = new TestSupport();
      ts.UpdateProjectViaWebApi(ts.ProjectUid, "project 5", ts.LastEventDate, "New Zealand Standard Time", DateTime.UtcNow, HttpStatusCode.BadRequest);
    }


    [TestMethod]
    public void Update_Project_Bad_Data()
    {
      var msg = new Msg();
      msg.Title("projects 6", "Update a project with bad data");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var projectId = ts.SetLegacyProjectId();
      CreateProject(ts, mysql, projectId, "project 6", ProjectType.Standard);

      //No action UTC
      ts.UpdateProjectViaWebApi(ts.ProjectUid, "project 6", ts.LastEventDate, "New Zealand Standard Time", DateTime.MinValue, HttpStatusCode.BadRequest);
      //No project UID
      ts.UpdateProjectViaWebApi(Guid.Empty, "project 6", ts.LastEventDate, "New Zealand Standard Time", DateTime.UtcNow, HttpStatusCode.BadRequest);
      //No project name
      ts.UpdateProjectViaWebApi(ts.ProjectUid, null, ts.LastEventDate, "New Zealand Standard Time", DateTime.UtcNow, HttpStatusCode.BadRequest);
      //No end date
      ts.UpdateProjectViaWebApi(ts.ProjectUid, "project 6", DateTime.MinValue, "New Zealand Standard Time", DateTime.UtcNow, HttpStatusCode.BadRequest);
      //Bad end date (before start)
      ts.UpdateProjectViaWebApi(ts.ProjectUid, "project 6", ts.FirstEventDate.AddMonths(-1), "New Zealand Standard Time", DateTime.UtcNow, HttpStatusCode.BadRequest);
      //Trying to change timezone (before start)
      ts.UpdateProjectViaWebApi(ts.ProjectUid, "project 6", ts.LastEventDate, "Mountain Standard Time", DateTime.UtcNow, HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public void Delete_Project_After_Create()
    {
      var msg = new Msg();
      msg.Title("projects 7", "Delete a project after create");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var projectId = ts.SetLegacyProjectId();
      CreateProject(ts, mysql, projectId, "project 7", ProjectType.Standard);
      ts.DeleteProjectViaWebApi(ts.ProjectUid, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", "IsDeleted", "1", ts.ProjectUid);
    }

    [TestMethod]
    public void Delete_Project_Before_Create()
    {
      var msg = new Msg();
      msg.Title("projects 8", "Delete a project before create");
      var ts = new TestSupport();
      ts.DeleteProjectViaWebApi(ts.ProjectUid, DateTime.UtcNow, HttpStatusCode.BadRequest);
    }


    [TestMethod]
    public void Delete_Project_Bad_Data()
    {
      var msg = new Msg();
      msg.Title("projects 9", "Delete a project with bad data");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var projectId = ts.SetLegacyProjectId();
      CreateProject(ts, mysql, projectId, "project 9", ProjectType.Standard);
      //No project UID
      ts.DeleteProjectViaWebApi(Guid.Empty, DateTime.UtcNow, HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public void Associate_Customer_Project_After_Create()
    {
      var msg = new Msg();
      msg.Title("projects 10", "Associate a customer with a project after create project");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var projectId = ts.SetLegacyProjectId();
      CreateProjectAndAssociateWithCustomer(ts, mysql, projectId, "project 10", ProjectType.Standard, 111111111);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID", "fk_CustomerUID,LegacyCustomerID", ts.CustomerUid + ",111111111", ts.ProjectUid);
    }

    [TestMethod]
    public void Associate_Customer_Project_Before_Create()
    {
      var msg = new Msg();
      msg.Title("projects 11", "Associate a customer with a project before create project");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      ts.AssociateCustomerProjectViaWebApi(ts.ProjectUid, ts.CustomerUid, 111111111, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_ProjectUID", 1, ts.ProjectUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID", "fk_CustomerUID,LegacyCustomerID", ts.CustomerUid + ",111111111", ts.ProjectUid);
    }

    [TestMethod]
    public void Associate_Customer_Project_Twice()
    {
      var msg = new Msg();
      msg.Title("projects 12", "Associate a customer with a project after it has already been associated");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      ts.AssociateCustomerProjectViaWebApi(ts.ProjectUid, ts.CustomerUid, 111111111, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_ProjectUID", 1, ts.ProjectUid);
      ts.AssociateCustomerProjectViaWebApi(ts.ProjectUid, Guid.NewGuid(), 222222222, DateTime.UtcNow, HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public void Associate_Customer_Project_Bad_Data()
    {
      var msg = new Msg();
      msg.Title("projects 13", "Associate a customer and a project with bad data");
      var ts = new TestSupport();
      //No action UTC
      ts.AssociateCustomerProjectViaWebApi(ts.ProjectUid, ts.CustomerUid, 111111111, DateTime.MinValue, HttpStatusCode.BadRequest);
      //No project UID
      ts.AssociateCustomerProjectViaWebApi(Guid.Empty, ts.CustomerUid, 111111111, DateTime.UtcNow, HttpStatusCode.BadRequest);
      //No customer UID
      ts.AssociateCustomerProjectViaWebApi(ts.ProjectUid, Guid.Empty, 111111111, DateTime.UtcNow, HttpStatusCode.BadRequest);
      //No legacy customer ID
      ts.AssociateCustomerProjectViaWebApi(ts.ProjectUid, ts.CustomerUid, 0, DateTime.UtcNow, HttpStatusCode.BadRequest);
    }

    // Dissociating projects from customers is not supported 
    //[TestMethod]
    //public void Dissociate_Customer_Project_After_Associate()
    //{
    //  var msg = new Msg();
    //  msg.Title("projects 14", "Dissociate a customer from a project after associate");
    //  var mysql = new MySqlHelper();
    //  var ts = new TestSupport();
    //  ts.AssociateCustomerProjectViaWebApi(ts.ProjectUid, ts.CustomerUid, 111111111, DateTime.UtcNow, HttpStatusCode.OK);
    //  mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_ProjectUID", 1, ts.ProjectUid);
    //  ts.DissociateProjectViaWebApi(ts.ProjectUid, ts.CustomerUid, DateTime.UtcNow, HttpStatusCode.NotImplemented);
    //  //At the moment, dissociate is not stored in the web api database so don't check database
    //}

    //[TestMethod]
    //public void Dissociate_Customer_Project_Before_Associate()
    //{
    //  var msg = new Msg();
    //  msg.Title("projects 15", "Dissociate a customer from a project before associate");
    //  var ts = new TestSupport();
    //  ts.DissociateProjectViaWebApi(ts.ProjectUid, ts.CustomerUid, DateTime.UtcNow, HttpStatusCode.NotImplemented);
    //}

    //[TestMethod]
    //public void Dissociate_Customer_Project_Bad_Data()
    //{
    //  var msg = new Msg();
    //  msg.Title("projects 16", "Dissociate a customer and a project with bad data");
    //  var ts = new TestSupport();
    //  //   var projectId = ts.SetLegacyProjectId();
    //  //No action UTC
    //  ts.DissociateProjectViaWebApi(ts.ProjectUid, ts.CustomerUid, DateTime.MinValue, HttpStatusCode.BadRequest);
    //  //No project UID
    //  ts.DissociateProjectViaWebApi(Guid.Empty, ts.CustomerUid, DateTime.UtcNow, HttpStatusCode.BadRequest);
    //  //No customer UID
    //  ts.DissociateProjectViaWebApi(ts.ProjectUid, Guid.Empty, DateTime.UtcNow, HttpStatusCode.NotImplemented);
    //}

    [TestMethod]
    public void Associate_Geofence_Project_After_Create()
    {
      var msg = new Msg();
      msg.Title("projects 17", "Associate a geofence with a project after create project");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var projectId = ts.SetLegacyProjectId();
      CreateProject(ts, mysql, projectId, "project 17", ProjectType.Standard);
      ts.AssociateGeofenceProjectViaWebApi(ts.ProjectUid, ts.GeofenceUid, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_ProjectUID", 1, ts.ProjectUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_ProjectUID", "fk_GeofenceUID", ts.GeofenceUid.ToString(), ts.ProjectUid);
    }

    [TestMethod]
    public void Associate_Geofence_Project_Before_Create()
    {
      var msg = new Msg();
      msg.Title("projects 18", "Associate a geofence with a project before create project");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      ts.AssociateGeofenceProjectViaWebApi(ts.ProjectUid, ts.GeofenceUid, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_ProjectUID", 1, ts.ProjectUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_ProjectUID", "fk_GeofenceUID", ts.GeofenceUid.ToString(), ts.ProjectUid);
    }


    [TestMethod]
    public void Associate_Geofence_Project_Bad_Data()
    {
      var msg = new Msg();
      msg.Title("projects 19", "Associate a geofence and a project with bad data");
      var ts = new TestSupport();
      //No action UTC
      ts.AssociateGeofenceProjectViaWebApi(ts.ProjectUid, ts.GeofenceUid, DateTime.MinValue, HttpStatusCode.BadRequest);
      //No project UID
      ts.AssociateGeofenceProjectViaWebApi(Guid.Empty, ts.GeofenceUid, DateTime.UtcNow, HttpStatusCode.BadRequest);
      //No customer UID
      ts.AssociateGeofenceProjectViaWebApi(ts.ProjectUid, Guid.Empty, DateTime.UtcNow, HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public void Get_Projects_With_CustomerUid()
    {
      var msg = new Msg();
      msg.Title("projects 20", "Get projects with customer UID header");
      var mysql = new MySqlHelper();

      var ts = new TestSupport();
      var projectUid1 = ts.ProjectUid;
      var projectId1 = ts.SetLegacyProjectId();
      CreateProjectAndAssociateWithCustomer(ts, mysql, projectId1, "project 20-1", ProjectType.Standard, 111111111);

      ts.SetProjectUid();
      var projectUid2 = ts.ProjectUid;
      var projectId2 = projectId1 + 1;
      CreateProjectAndAssociateWithCustomer(ts, mysql, projectId2, "project 20-2", ProjectType.Standard, 111111111);
      CreateMockCustomer(ts);

      var dateRange = FormatProjectDateRangeWebApi(ts);
      var expectedProjects = new[] {
      "| IsArchived | ProjectName  | ProjectTimezone           | ProjectType | ProjectStartDate | ProjectEndDate | ProjectUID     | ProjectID    | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
      "| false      | project 20-1 | New Zealand Standard Time | Standard    | " + dateRange + $"                | {projectUid1}  | {projectId1} | {ts.CustomerUid} | 111111111        | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
      "| false      | project 20-2 | New Zealand Standard Time | Standard    | " + dateRange + $"                | {projectUid2}  | {projectId2} | {ts.CustomerUid} | 111111111        | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"};

      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, ts.CustomerUid, expectedProjects);
    }

    [TestMethod]
    public void Get_Projects_No_CustomerUid()
    {
      var msg = new Msg();
      msg.Title("projects 21", "Get projects with no customer UID header");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var projectId = ts.SetLegacyProjectId();
      CreateProjectAndAssociateWithCustomer(ts, mysql, projectId, "project 21-1", ProjectType.Standard, 111111111);
      CreateMockCustomer(ts);
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.Forbidden, Guid.Empty, null);
    }

    [TestMethod]
    public void Get_Projects_For_One_Of_Multiple_Customers()
    {
      var msg = new Msg();
      msg.Title("projects 22", "Get projects for one of many customers");
      var mysql = new MySqlHelper();

      var ts = new TestSupport();
      var projectId1 = ts.SetLegacyProjectId();
      //Customer 1
      CreateProjectAndAssociateWithCustomer(ts, mysql, projectId1, "project 22-1", ProjectType.Standard, 111111111);
      CreateMockCustomer(ts);
      //Customer 2
      ts.SetProjectUid();
      ts.SetCustomerUid();
      var projectId2 = ts.SetLegacyProjectId();
      CreateProjectAndAssociateWithCustomer(ts, mysql, projectId2, "project 22-2", ProjectType.Standard, 222222222);
      ts.CreateMockCustomer(ts.CustomerUid, "customer 2", CustomerType.Customer);

      var dateRange = FormatProjectDateRangeWebApi(ts);
      var expectedProjects = new[] {
      "| IsArchived | ProjectName  | ProjectTimezone           | ProjectType | ProjectStartDate | ProjectEndDate | ProjectUID      | ProjectID    | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
      "| false      | project 22-2 | New Zealand Standard Time | Standard    | " + dateRange + $"                | {ts.ProjectUid} | {projectId2} | {ts.CustomerUid} | 222222222        | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"};

      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, ts.CustomerUid, expectedProjects);
    }

    [TestMethod]
    public void Get_Projects_With_Deleted_Projects()
    {
      //Deleted projects are archived

      var msg = new Msg();
      msg.Title("projects 23", "Get projects for customer with some deleted");
      var mysql = new MySqlHelper();

      var ts = new TestSupport();
      var projectUid1 = ts.ProjectUid;
      var projectId1 = ts.SetLegacyProjectId();
      CreateProjectAndAssociateWithCustomer(ts, mysql, projectId1, "project 23-1", ProjectType.Standard, 222222222);
      ts.DeleteProjectViaWebApi(projectUid1, DateTime.UtcNow, HttpStatusCode.OK);

      ts.SetProjectUid();
      var projectUid2 = ts.ProjectUid;
      var projectId2 = ts.SetLegacyProjectId();
      CreateProjectAndAssociateWithCustomer(ts, mysql, projectId2, "project 23-2", ProjectType.Standard, 222222222);
      CreateMockCustomer(ts);

      var dateRange = FormatProjectDateRangeWebApi(ts);
      // on project deletion, endDate is set to now, in the projects timezone.
      var dateRangeResetEndDate = ts.FirstEventDate.ToString("O") + " | " + DateTime.UtcNow.ToLocalDateTime("Pacific/Auckland").Date.ToString("O");
      var expectedProjects = new[] {
      "| IsArchived | ProjectName  | ProjectTimezone           | ProjectType | ProjectStartDate | ProjectEndDate | ProjectUID     | ProjectID    | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
      "| true       | project 23-1 | New Zealand Standard Time | Standard    | " + dateRangeResetEndDate + $"                | {projectUid1}  | {projectId1} | {ts.CustomerUid} | 222222222        | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
      "| false      | project 23-2 | New Zealand Standard Time | Standard    | " + dateRange + $"                | {projectUid2}  | {projectId2} | {ts.CustomerUid} | 222222222        | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"};

      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, ts.CustomerUid, expectedProjects);
    }

    [TestMethod]
    public void Get_Projects_With_Multiple_Subscriptions()
    {
      var msg = new Msg();
      msg.Title("projects 24", "Get projects with multiple subscriptions");
      var mysql = new MySqlHelper();

      var ts = new TestSupport();
      var projectId = ts.SetLegacyProjectId();
      CreateProjectAndAssociateWithCustomer(ts, mysql, projectId, "project 24", ProjectType.ProjectMonitoring, 222222222);
      CreateMockCustomer(ts);
      CreateMockProjectSubscription(ts, ts.FirstEventDate, ts.FirstEventDate.AddYears(1));
      ts.SetSubscriptionUid();
      CreateMockProjectSubscription(ts, ts.FirstEventDate.AddYears(1).AddDays(1), new DateTime(9999, 12, 31));
      var dateRange = FormatProjectDateRangeWebApi(ts);
      var expectedProjects = new[] {
      "| IsArchived | ProjectName  | ProjectTimezone           | ProjectType       | ProjectStartDate | ProjectEndDate | ProjectUID      | ProjectID   | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
      "| false      | project 24   | New Zealand Standard Time | ProjectMonitoring | " + dateRange + $"                | {ts.ProjectUid} | {projectId} | {ts.CustomerUid} | 222222222        | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"};

      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, ts.CustomerUid, expectedProjects);
    }
    [TestMethod]
    public void Get_Projects_With_Various_Project_Types()
    {
      var msg = new Msg();
      msg.Title("projects 25", "Get projects of different project types");
      var mysql = new MySqlHelper();

      var ts = new TestSupport { CustomerId = "222222222" };
      var projectUid1 = ts.ProjectUid;
      var projectId1 = ts.SetLegacyProjectId();
      CreateProjectAndAssociateWithCustomer(ts, mysql, projectId1, "project 25-1", ProjectType.Standard, 222222222);

      ts.SetProjectUid();
      var projectUid2 = ts.ProjectUid;
      var projectId2 = ts.SetLegacyProjectId();
      CreateProjectAndAssociateWithCustomer(ts, mysql, projectId2, "project 25-2", ProjectType.ProjectMonitoring, 222222222);
      CreateMockProjectSubscription(ts, ts.FirstEventDate, ts.FirstEventDate.AddYears(1));

      ts.SetProjectUid();
      var projectUid3 = ts.ProjectUid;
      var projectId3 = ts.SetLegacyProjectId();
      CreateProjectAndAssociateWithCustomer(ts, mysql, projectId3, "project 25-3", ProjectType.LandFill, 222222222);
      ts.SetSubscriptionUid();
      CreateMockProjectSubscription(ts, ts.FirstEventDate, ts.FirstEventDate.AddYears(1));
      CreateMockCustomer(ts);

      var dateRange = FormatProjectDateRangeWebApi(ts);
      var expectedProjects = new[] {
      "| IsArchived | ProjectName  | ProjectTimezone           | ProjectType       | ProjectStartDate | ProjectEndDate | ProjectUID     | ProjectID   | CustomerUID      | LegacyCustomerId | ProjectBoundary | ",
      "| false      | project 25-1 | New Zealand Standard Time | Standard          | " + dateRange + $"                | {projectUid1} | {projectId1} | {ts.CustomerUid} | 222222222        | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
      "| false      | project 25-2 | New Zealand Standard Time | ProjectMonitoring | " + dateRange + $"                | {projectUid2} | {projectId2} | {ts.CustomerUid} | 222222222        | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
      "| false      | project 25-3 | New Zealand Standard Time | LandFill          | " + dateRange + $"                | {projectUid3} | {projectId3} | {ts.CustomerUid} | 222222222        | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };

      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, ts.CustomerUid, expectedProjects);
    }
    [TestMethod]
    public void Get_Projects_With_Enddated_Subscriptions()
    {
      var msg = new Msg();
      msg.Title("projects 26", "Get projects with enddated subscriptions");
      var mysql = new MySqlHelper();
      var ts = new TestSupport { CustomerId = "222222222" };
      var projectId = ts.SetLegacyProjectId();
      CreateProjectAndAssociateWithCustomer(ts, mysql, projectId, "project 26", ProjectType.ProjectMonitoring, 222222222);
      CreateMockCustomer(ts);
      CreateMockProjectSubscription(ts, ts.FirstEventDate.AddYears(-1), ts.FirstEventDate);
      var dateRange = FormatProjectDateRangeWebApi(ts);
      var expectedProjects = new[] {
      "| IsArchived | ProjectName  | ProjectTimezone           | ProjectType       | ProjectStartDate | ProjectEndDate | ProjectUID      | ProjectID   |  CustomerUID     | LegacyCustomerId |ProjectBoundary | ",
      "| true       | project 26   | New Zealand Standard Time | ProjectMonitoring | " + dateRange + $"                | {ts.ProjectUid} | {projectId} | {ts.CustomerUid} | 222222222        | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |" };
      ts.GetProjectsViaWebApiV3AndCompareActualWithExpected(HttpStatusCode.OK, ts.CustomerUid, expectedProjects);
    }

    #region privates

    private void CreateMockCustomer(TestSupport ts)
    {
      ts.CreateMockCustomer(ts.CustomerUid, "customer 1", CustomerType.Customer);
    }

    private void CreateProject(TestSupport ts, MySqlHelper mysql, int projectId, string projectName, ProjectType projectType)
    {
      ts.CreateProjectViaWebApi(ts.ProjectUid, projectId, projectName, ts.FirstEventDate, ts.LastEventDate, "New Zealand Standard Time", projectType, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, ts.ProjectUid);
    }

    private void CreateProjectAndAssociateWithCustomer(TestSupport ts, MySqlHelper mysql, int projectId, string projectName, ProjectType projectType, int customerId)
    {
      CreateProject(ts, mysql, projectId, projectName, projectType);
      ts.AssociateCustomerProjectViaWebApi(ts.ProjectUid, ts.CustomerUid, customerId, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_ProjectUID", 1, ts.ProjectUid);
    }

    private void CreateMockProjectSubscription(TestSupport ts, DateTime subStartDate, DateTime subEndDate)
    {
      ts.CreateMockProjectSubscription(ts.ProjectUid.ToString(), ts.SubscriptionUid.ToString(), ts.CustomerUid.ToString(), subStartDate, subEndDate, subStartDate);
    }

    private string FormatProjectDateRangeWebApi(TestSupport ts)
    {
      return ts.FirstEventDate.ToString("O") + " | " + ts.LastEventDate.ToString("O");
    }
    #endregion
  }
}
