using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiTests
{
  [TestClass]
  public class ProjectTests
  {

    [TestMethod]
    public void Create_Project_All_Ok()
    {
      var msg = new Msg();
      msg.Title("projects 1", "Create a project");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      CreateProject(testSupport, mysql, 123456789, "project 1", ProjectType.Standard);
      var dateRange = FormatProjectDateRangeDatabase(testSupport);
      //mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", "LegacyProjectID,Name,ProjectTimeZone,StartDate,EndDate", 
      //  "123456789,project 1,New Zealand Standard Time," + dateRange, testSupport.ProjectUid);  
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", "LegacyProjectID,Name,ProjectTimeZone,StartDate,EndDate",
        $"123456789,project 1,New Zealand Standard Time, {testSupport.FirstEventDate}, {testSupport.LastEventDate}", testSupport.ProjectUid);
    }

    [TestMethod]
    public void Create_Project_Twice()
    {
      var msg = new Msg();
      msg.Title("projects 2", "Create a project twice");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      CreateProject(testSupport, mysql, 123456789, "project 2", ProjectType.Standard);
      testSupport.CreateProjectViaWebApi(testSupport.ProjectUid, 123456789, "project 2", testSupport.FirstEventDate, 
        testSupport.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public void Create_Project_Bad_Data()
    {
      var msg = new Msg();
      msg.Title("projects 3", "Create a project with bad data");

      var testSupport = new TestSupport();
      //No action UTC
      testSupport.CreateProjectViaWebApi(testSupport.ProjectUid, 123456789, "project 3", testSupport.FirstEventDate,
        testSupport.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.MinValue, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //No project UID
      testSupport.CreateProjectViaWebApi(Guid.Empty, 123456789, "project 3", testSupport.FirstEventDate,
        testSupport.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //No time zone
      testSupport.CreateProjectViaWebApi(testSupport.ProjectUid, 123456789, "project 3", testSupport.FirstEventDate, 
        testSupport.LastEventDate, null, ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //No project name
      testSupport.CreateProjectViaWebApi(testSupport.ProjectUid, 123456789, null, testSupport.FirstEventDate, 
        testSupport.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //No start date
      testSupport.CreateProjectViaWebApi(testSupport.ProjectUid, 123456789, "project 3", DateTime.MinValue, 
        testSupport.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //No end date
      testSupport.CreateProjectViaWebApi(testSupport.ProjectUid, 123456789, "project 3", testSupport.FirstEventDate,
        DateTime.MinValue, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //Bad end date
      testSupport.CreateProjectViaWebApi(testSupport.ProjectUid, 123456789, "project 3", testSupport.FirstEventDate,
        testSupport.FirstEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //Bad date range
      testSupport.CreateProjectViaWebApi(testSupport.ProjectUid, 123456789, "project 3", testSupport.LastEventDate,
        testSupport.FirstEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //No legacy project ID
      testSupport.CreateProjectViaWebApi(testSupport.ProjectUid, 0, "project 3", testSupport.FirstEventDate,
        testSupport.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.BadRequest);
      //No boundary
      testSupport.CreateProjectViaWebApi(testSupport.ProjectUid, 123456789, "project 3", testSupport.FirstEventDate,
        testSupport.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, null, HttpStatusCode.BadRequest);
      //Invalid boundary
      testSupport.CreateProjectViaWebApi(testSupport.ProjectUid, 123456789, "project 3", testSupport.FirstEventDate,
        testSupport.LastEventDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, "POLYGON((-121.347189366818 38.8361907402694))", HttpStatusCode.BadRequest);

    }

    [TestMethod]
    public void Update_Project_After_Create()
    {
      var msg = new Msg();
      msg.Title("projects 4", "Update a project after create");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      CreateProject(testSupport, mysql, 123456789, "project 4", ProjectType.Standard);
      var updatedEndDate = testSupport.FirstEventDate.AddYears(3);
      testSupport.UpdateProjectViaWebApi(testSupport.ProjectUid, "project 4 updated",
        updatedEndDate, "New Zealand Standard Time", DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", "Name,EndDate",
        "project 4 updated," + updatedEndDate, testSupport.ProjectUid);
    }

    [TestMethod]
    public void Update_Project_Before_Create()
    {
      var msg = new Msg();
      msg.Title("projects 5", "Update a project before create");

      var testSupport = new TestSupport();
      testSupport.UpdateProjectViaWebApi(testSupport.ProjectUid, "project 5",
        testSupport.LastEventDate, "New Zealand Standard Time", DateTime.UtcNow, HttpStatusCode.BadRequest);
    }


    [TestMethod]
    public void Update_Project_Bad_Data()
    {
      var msg = new Msg();
      msg.Title("projects 6", "Update a project with bad data");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      CreateProject(testSupport, mysql, 123456789, "project 6", ProjectType.Standard);

      //No action UTC
      testSupport.UpdateProjectViaWebApi(testSupport.ProjectUid, "project 6",
        testSupport.LastEventDate, "New Zealand Standard Time", DateTime.MinValue, HttpStatusCode.BadRequest);
      //No project UID
      testSupport.UpdateProjectViaWebApi(Guid.Empty, "project 6",
        testSupport.LastEventDate, "New Zealand Standard Time", DateTime.UtcNow, HttpStatusCode.BadRequest);
      //No project name
      testSupport.UpdateProjectViaWebApi(testSupport.ProjectUid, null,
        testSupport.LastEventDate, "New Zealand Standard Time", DateTime.UtcNow, HttpStatusCode.BadRequest);
      //No end date
      testSupport.UpdateProjectViaWebApi(testSupport.ProjectUid, "project 6",
        DateTime.MinValue, "New Zealand Standard Time", DateTime.UtcNow, HttpStatusCode.BadRequest);
      //Bad end date (before start)
      testSupport.UpdateProjectViaWebApi(testSupport.ProjectUid, "project 6",
        testSupport.FirstEventDate.AddMonths(-1), "New Zealand Standard Time", DateTime.UtcNow, HttpStatusCode.BadRequest);
      //Trying to change timezone (before start)
      testSupport.UpdateProjectViaWebApi(testSupport.ProjectUid, "project 6",
        testSupport.LastEventDate, "Mountain Standard Time", DateTime.UtcNow, HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public void Delete_Project_After_Create()
    {
      var msg = new Msg();
      msg.Title("projects 7", "Delete a project after create");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      CreateProject(testSupport, mysql, 123456789, "project 7", ProjectType.Standard);

      testSupport.DeleteProjectViaWebApi(testSupport.ProjectUid, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", "IsDeleted", "1", testSupport.ProjectUid);
    }

    [TestMethod]
    public void Delete_Project_Before_Create()
    {
      var msg = new Msg();
      msg.Title("projects 8", "Delete a project before create");

      var testSupport = new TestSupport();
      testSupport.DeleteProjectViaWebApi(testSupport.ProjectUid, DateTime.UtcNow, HttpStatusCode.BadRequest);
    }


    [TestMethod]
    public void Delete_Project_Bad_Data()
    {
      var msg = new Msg();
      msg.Title("projects 9", "Delete a project with bad data");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      CreateProject(testSupport, mysql, 123456789, "project 9", ProjectType.Standard);

      //No action UTC
      testSupport.DeleteProjectViaWebApi(testSupport.ProjectUid, DateTime.MinValue, HttpStatusCode.BadRequest);
      //No project UID
      testSupport.DeleteProjectViaWebApi(Guid.Empty, DateTime.UtcNow, HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public void Associate_Customer_Project_After_Create()
    {
      var msg = new Msg();
      msg.Title("projects 10", "Associate a customer with a project after create project");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      CreateProjectAndAssociateWithCustomer(testSupport, mysql, 123456789, "project 10", ProjectType.Standard, 111111111);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID", "fk_CustomerUID,LegacyCustomerID", testSupport.CustomerUid + ",111111111", testSupport.ProjectUid);
    }

    [TestMethod]
    public void Associate_Customer_Project_Before_Create()
    {
      var msg = new Msg();
      msg.Title("projects 11", "Associate a customer with a project before create project");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      testSupport.AssociateCustomerProjectViaWebApi(testSupport.ProjectUid, testSupport.CustomerUid, 111111111, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_ProjectUID", 1, testSupport.ProjectUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID", "fk_CustomerUID,LegacyCustomerID", testSupport.CustomerUid + ",111111111", testSupport.ProjectUid);
    }

    [TestMethod]
    public void Associate_Customer_Project_Twice()
    {
      var msg = new Msg();
      msg.Title("projects 12", "Associate a customer with a project after it has already been associated");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      testSupport.AssociateCustomerProjectViaWebApi(testSupport.ProjectUid, testSupport.CustomerUid, 111111111, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_ProjectUID", 1, testSupport.ProjectUid);
      testSupport.AssociateCustomerProjectViaWebApi(testSupport.ProjectUid, Guid.NewGuid(), 222222222, DateTime.UtcNow, HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public void Associate_Customer_Project_Bad_Data()
    {
      var msg = new Msg();
      msg.Title("projects 13", "Associate a customer and a project with bad data");

      var testSupport = new TestSupport();
      //No action UTC
      testSupport.AssociateCustomerProjectViaWebApi(testSupport.ProjectUid, testSupport.CustomerUid, 111111111, DateTime.MinValue, HttpStatusCode.BadRequest);
      //No project UID
      testSupport.AssociateCustomerProjectViaWebApi(Guid.Empty, testSupport.CustomerUid, 111111111, DateTime.UtcNow, HttpStatusCode.BadRequest);
      //No customer UID
      testSupport.AssociateCustomerProjectViaWebApi(testSupport.ProjectUid, Guid.Empty, 111111111, DateTime.UtcNow, HttpStatusCode.BadRequest);
      //No legacy customer ID
      testSupport.AssociateCustomerProjectViaWebApi(testSupport.ProjectUid, testSupport.CustomerUid, 0, DateTime.UtcNow, HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public void Dissociate_Customer_Project_After_Associate()
    {
      var msg = new Msg();
      msg.Title("projects 14", "Dissociate a customer from a project after associate");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      testSupport.AssociateCustomerProjectViaWebApi(testSupport.ProjectUid, testSupport.CustomerUid, 111111111, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_ProjectUID", 1, testSupport.ProjectUid);
      testSupport.DissociateProjectViaWebApi(testSupport.ProjectUid, testSupport.CustomerUid, DateTime.UtcNow, HttpStatusCode.NotImplemented);
      //At the moment, dissociate is not stored in the web api database so don't check database
    }

    [TestMethod]
    public void Dissociate_Customer_Project_Before_Associate()
    {
      var msg = new Msg();
      msg.Title("projects 15", "Dissociate a customer from a project before associate");

      var testSupport = new TestSupport();
      testSupport.DissociateProjectViaWebApi(testSupport.ProjectUid, testSupport.CustomerUid, DateTime.UtcNow, HttpStatusCode.NotImplemented);
    }

    [TestMethod]
    public void Dissociate_Customer_Project_Bad_Data()
    {
      var msg = new Msg();
      msg.Title("projects 16", "Dissociate a customer and a project with bad data");

      var testSupport = new TestSupport();
      //No action UTC
      testSupport.DissociateProjectViaWebApi(testSupport.ProjectUid, testSupport.CustomerUid, DateTime.MinValue, HttpStatusCode.BadRequest);
      //No project UID
      testSupport.DissociateProjectViaWebApi(Guid.Empty, testSupport.CustomerUid, DateTime.UtcNow, HttpStatusCode.BadRequest);
      //No customer UID
      testSupport.DissociateProjectViaWebApi(testSupport.ProjectUid, Guid.Empty, DateTime.UtcNow, HttpStatusCode.NotImplemented);
    }

    [TestMethod]
    public void Associate_Geofence_Project_After_Create()
    {
      var msg = new Msg();
      msg.Title("projects 17", "Associate a geofence with a project after create project");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      CreateProject(testSupport, mysql, 123456789, "project 17", ProjectType.Standard);

      testSupport.AssociateGeofenceProjectViaWebApi(testSupport.ProjectUid, testSupport.GeofenceUid, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_ProjectUID", 1, testSupport.ProjectUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_ProjectUID", "fk_GeofenceUID", testSupport.GeofenceUid.ToString(), testSupport.ProjectUid);
    }

    [TestMethod]
    public void Associate_Geofence_Project_Before_Create()
    {
      var msg = new Msg();
      msg.Title("projects 18", "Associate a geofence with a project before create project");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      testSupport.AssociateGeofenceProjectViaWebApi(testSupport.ProjectUid, testSupport.GeofenceUid, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_ProjectUID", 1, testSupport.ProjectUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_ProjectUID", "fk_GeofenceUID", testSupport.GeofenceUid.ToString(), testSupport.ProjectUid);
    }


    [TestMethod]
    public void Associate_Geofence_Project_Bad_Data()
    {
      var msg = new Msg();
      msg.Title("projects 19", "Associate a geofence and a project with bad data");

      var testSupport = new TestSupport();
      //No action UTC
      testSupport.AssociateGeofenceProjectViaWebApi(testSupport.ProjectUid, testSupport.GeofenceUid, DateTime.MinValue, HttpStatusCode.BadRequest);
      //No project UID
      testSupport.AssociateGeofenceProjectViaWebApi(Guid.Empty, testSupport.GeofenceUid, DateTime.UtcNow, HttpStatusCode.BadRequest);
      //No customer UID
      testSupport.AssociateGeofenceProjectViaWebApi(testSupport.ProjectUid, Guid.Empty, DateTime.UtcNow, HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public void Get_Projects_With_CustomerUid()
    {
      var msg = new Msg();
      msg.Title("projects 20", "Get projects with customer UID header");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      var projectUid1 = testSupport.ProjectUid;
      CreateProjectAndAssociateWithCustomer(testSupport, mysql, 123456789, "project 20-1", ProjectType.Standard, 111111111);

      testSupport.SetProjectUid();
      var projectUid2 = testSupport.ProjectUid;
      CreateProjectAndAssociateWithCustomer(testSupport, mysql, 987654321, "project 20-2", ProjectType.Standard, 111111111);

      CreateMockCustomer(testSupport);

      var dateRange = FormatProjectDateRangeWebApi(testSupport);
      var expectedProjects = new[] {
            "| IsArchived | Name         | ProjectTimeZone           | ProjectType | StartDate | EndDate | ProjectUid          | LegacyProjectId | ProjectGeofenceWKT | ",
            "| false      | project 20-1 | New Zealand Standard Time | Standard    | " + dateRange + "   | " + projectUid1 + " | 123456789       | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
            "| false      | project 20-2 | New Zealand Standard Time | Standard    | " + dateRange + "   | " + projectUid2 + " | 987654321       | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"
            };

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, testSupport.CustomerUid, expectedProjects);
    }

    [TestMethod]
    public void Get_Projects_No_CustomerUid()
    {
      var msg = new Msg();
      msg.Title("projects 21", "Get projects with no customer UID header");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      CreateProjectAndAssociateWithCustomer(testSupport, mysql, 987654321, "project 21-1", ProjectType.Standard, 111111111);

      CreateMockCustomer(testSupport);

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.Forbidden, Guid.Empty, null);
    }

    [TestMethod]
    public void Get_Projects_For_One_Of_Multiple_Customers()
    {
      var msg = new Msg();
      msg.Title("projects 22", "Get projects for one of many customers");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      //Customer 1
      CreateProjectAndAssociateWithCustomer(testSupport, mysql, 123456789, "project 22-1", ProjectType.Standard, 111111111);
      CreateMockCustomer(testSupport);
      //Customer 2
      testSupport.SetProjectUid();
      testSupport.SetCustomerUid();
      CreateProjectAndAssociateWithCustomer(testSupport, mysql, 987654321, "project 22-2", ProjectType.Standard, 222222222);

      testSupport.CreateMockCustomer(testSupport.CustomerUid, "customer 2", CustomerType.Customer);

      var dateRange = FormatProjectDateRangeWebApi(testSupport);
      var expectedProjects = new[] {
            "| IsArchived | Name         | ProjectTimeZone           | ProjectType | StartDate | EndDate | ProjectUid                     | LegacyProjectId | ProjectGeofenceWKT |",
            "| false      | project 22-2 | New Zealand Standard Time | Standard    | " + dateRange + "   | " + testSupport.ProjectUid + " | 987654321       | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"
            };

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, testSupport.CustomerUid, expectedProjects);
    }

    [TestMethod]
    public void Get_Projects_With_Deleted_Projects()
    {
      //Deleted projects are archived

      var msg = new Msg();
      msg.Title("projects 23", "Get projects for customer with some deleted");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      var projectUid1 = testSupport.ProjectUid;
      CreateProjectAndAssociateWithCustomer(testSupport, mysql, 123456789, "project 23-1", ProjectType.Standard, 222222222);
      testSupport.DeleteProjectViaWebApi(projectUid1, DateTime.UtcNow, HttpStatusCode.OK);

      testSupport.SetProjectUid();
      var projectUid2 = testSupport.ProjectUid;
      CreateProjectAndAssociateWithCustomer(testSupport, mysql, 987654321, "project 23-2", ProjectType.Standard, 222222222);

      CreateMockCustomer(testSupport);

      var dateRange = FormatProjectDateRangeWebApi(testSupport);
      var expectedProjects = new[] {
            "| IsArchived | Name         | ProjectTimeZone           | ProjectType | StartDate | EndDate | ProjectUid          | LegacyProjectId | ProjectGeofenceWKT |",
            "| true       | project 23-1 | New Zealand Standard Time | Standard    | " + dateRange + "   | " + projectUid1 + " | 123456789       | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
            "| false      | project 23-2 | New Zealand Standard Time | Standard    | " + dateRange + "   | " + projectUid2 + " | 987654321       | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"
            };

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, testSupport.CustomerUid, expectedProjects);
    }
    [TestMethod]
    public void Get_Projects_With_Multiple_Subscriptions()
    {
      var msg = new Msg();
      msg.Title("projects 24", "Get projects with multiple subscriptions");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      CreateProjectAndAssociateWithCustomer(testSupport, mysql, 123459876, "project 24", ProjectType.ProjectMonitoring, 222222222);
      CreateMockCustomer(testSupport);
      CreateMockSubscription(testSupport, testSupport.FirstEventDate, testSupport.FirstEventDate.AddYears(1));
      testSupport.SetSubscriptionUid();
      CreateMockSubscription(testSupport, testSupport.FirstEventDate.AddYears(1).AddDays(1), new DateTime(9999, 12, 31));

      var dateRange = FormatProjectDateRangeWebApi(testSupport);
      var expectedProjects = new[] {
            "| IsArchived | Name         | ProjectTimeZone           | ProjectType       | StartDate | EndDate | ProjectUid                     | LegacyProjectId | ProjectGeofenceWKT |",
            "| false      | project 24   | New Zealand Standard Time | ProjectMonitoring | " + dateRange + "   | " + testSupport.ProjectUid + " | 123459876       | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"
            };

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, testSupport.CustomerUid, expectedProjects);
    }
    [TestMethod]
    public void Get_Projects_With_Various_Project_Types()
    {
      var msg = new Msg();
      msg.Title("projects 25", "Get projects of different project types");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      var projectUid1 = testSupport.ProjectUid;
      CreateProjectAndAssociateWithCustomer(testSupport, mysql, 123456789, "project 25-1", ProjectType.Standard, 222222222);

      testSupport.SetProjectUid();
      var projectUid2 = testSupport.ProjectUid;
      CreateProjectAndAssociateWithCustomer(testSupport, mysql, 987654321, "project 25-2", ProjectType.ProjectMonitoring, 222222222);
      CreateMockSubscription(testSupport, testSupport.FirstEventDate, testSupport.FirstEventDate.AddYears(1));

      testSupport.SetProjectUid();
      var projectUid3 = testSupport.ProjectUid;
      CreateProjectAndAssociateWithCustomer(testSupport, mysql, 123459876, "project 25-3", ProjectType.LandFill, 222222222);
      testSupport.SetSubscriptionUid();
      CreateMockSubscription(testSupport, testSupport.FirstEventDate, testSupport.FirstEventDate.AddYears(1));

      CreateMockCustomer(testSupport);

      var dateRange = FormatProjectDateRangeWebApi(testSupport);
      var expectedProjects = new[] {
            "| IsArchived | Name         | ProjectTimeZone           | ProjectType       | StartDate | EndDate | ProjectUid          | LegacyProjectId | ProjectGeofenceWKT |",
            "| false      | project 25-1 | New Zealand Standard Time | Standard          | " + dateRange + "   | " + projectUid1 + " | 123456789       | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
            "| false      | project 25-2 | New Zealand Standard Time | ProjectMonitoring | " + dateRange + "   | " + projectUid2 + " | 987654321       | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |",
            "| false      | project 25-3 | New Zealand Standard Time | LandFill          | " + dateRange + "   | " + projectUid3 + " | 123459876       | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"
            };

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, testSupport.CustomerUid, expectedProjects);
    }
    [TestMethod]
    public void Get_Projects_With_Enddated_Subscriptions()
    {
      var msg = new Msg();
      msg.Title("projects 26", "Get projects with enddated subscriptions");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      CreateProjectAndAssociateWithCustomer(testSupport, mysql, 555555555, "project 26", ProjectType.ProjectMonitoring, 222222222);
      CreateMockCustomer(testSupport);
      CreateMockSubscription(testSupport, testSupport.FirstEventDate.AddYears(-1), testSupport.FirstEventDate);

      var dateRange = FormatProjectDateRangeWebApi(testSupport);
      var expectedProjects = new[] {
            "| IsArchived | Name         | ProjectTimeZone           | ProjectType       | StartDate | EndDate | ProjectUid                     | LegacyProjectId | ProjectGeofenceWKT |",
            "| true       | project 26   | New Zealand Standard Time | ProjectMonitoring | " + dateRange + "   | " + testSupport.ProjectUid + " | 555555555       | POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694)) |"
            };

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, testSupport.CustomerUid, expectedProjects);
    }

    #region privates

    private void CreateMockCustomer(TestSupport testSupport)
    {
      testSupport.CreateMockCustomer(testSupport.CustomerUid, "customer 1", CustomerType.Customer);
    }

    private void CreateProject(TestSupport testSupport, MySqlHelper mysql, int projectId, string projectName, ProjectType projectType)
    {
      testSupport.CreateProjectViaWebApi(testSupport.ProjectUid, projectId, projectName, testSupport.FirstEventDate,
        testSupport.LastEventDate, "New Zealand Standard Time", projectType, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, testSupport.ProjectUid);
    }

    private void CreateProjectAndAssociateWithCustomer(TestSupport testSupport, MySqlHelper mysql, int projectId, string projectName, ProjectType projectType, int customerId)
    {
      CreateProject(testSupport, mysql, projectId, projectName, projectType);
      testSupport.AssociateCustomerProjectViaWebApi(testSupport.ProjectUid, testSupport.CustomerUid, customerId, DateTime.UtcNow, HttpStatusCode.OK);
      mysql.VerifyTestResultDatabaseRecordCount("CustomerProject", "fk_ProjectUID", 1, testSupport.ProjectUid);
    }

    private void CreateMockSubscription(TestSupport testSupport, DateTime subStartDate, DateTime subEndDate)
    {
      testSupport.CreateMockProjectSubscription(testSupport.ProjectUid.ToString(), testSupport.SubscriptionUid.ToString(), 
        testSupport.CustomerUid.ToString(), subStartDate, subEndDate, subStartDate);
    }

    private string FormatProjectDateRangeWebApi(TestSupport testSupport)
    {
      return testSupport.FirstEventDate.ToString("O") + " | " + testSupport.LastEventDate.ToString("O");
    }

    private string FormatProjectDateRangeDatabase(TestSupport testSupport)
    {
      return string.Format("{0},{1}", testSupport.FirstEventDate.ToString(DB_DATE_FORMAT), testSupport.LastEventDate.ToString(DB_DATE_FORMAT));
    }

    private string FormatProjectDateDatabase(DateTime date)
    {
      return date.ToString(DB_DATE_FORMAT);
    }

    private const string DB_DATE_FORMAT = "d/MM/yyyy hh:mm:ss tt";

    private const string PROJECT_BOUNDARY =
      "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";

      //Old v1 format
         // "-121.347189366818,38.8361907402694;-121.349260032177,38.8361656688414;-121.349217116833,38.8387897637231;-121.347275197506,38.8387145521594;-121.347189366818,38.8361907402694;-121.347189366818,38.8361907402694";

    #endregion

  }
}
