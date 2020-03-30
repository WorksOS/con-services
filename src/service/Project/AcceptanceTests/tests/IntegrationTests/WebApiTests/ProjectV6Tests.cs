using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using IntegrationTests.UtilityClasses;
using TestUtility;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Repositories.DBModels;
using Xunit;

namespace IntegrationTests.WebApiTests
{
  public class ProjectV6Tests : WebApiTestsBase
  {
    [Fact]
    public async Task CreateStandardProjectAndGetProjectListV4()
    {

      Msg.Title("Project v4 test 1", "Create standard project and customer then read the project list. No subscription");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);

      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 1 | 1                 | {customerUid} |"};
      await ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName     | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      |" };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
    }

    [Fact]
    public async Task CreateLandfillProjectAndSubscriptionThenGetProjectListV4()
    {
      Msg.Title("Project v4 test 2", "Create landfill project and customer then read the project list");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"};
      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName     | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description                 |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 2 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | BootCampDimensions.dc | Boundary Test 2 Description |"};
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
    }

    [Fact]
    public async Task CreateProjectMonitoringProjectAndSubscriptionThenGetProjectListV4()
    {
      Msg.Title("Project v4 test 3", "Create project monitoring project of and customer then read the project list");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
      };
      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName     | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 3 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      |" };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
    }

    [Fact]
    public async Task CreateProjectWithoutLegacyProjectId()
    {
      Msg.Title("Project v4 test 4", "Create standard project with out legacy project id");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);

      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 1 | 1                 | {customerUid} |"};
      await ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName     | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 4 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      |" };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
    }

    [Fact]
    public async Task Create2SubscriptionsForLandfillAndCreateProjects()
    {
      Msg.Title("Project v4 test 6", "Create landfill project and customer then read the project list");
      var ts = new TestSupport();
      var legacyProjectId1 = TestSupport.GenerateLegacyProjectId();
      var projectUid1 = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid1 = Guid.NewGuid();
      var subscriptionUid2 = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID    | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID  |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                    |                |                  |             |                |               |          |                     |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                    |                |                  |             |                |               | {tccOrg} |                     |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid1} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                     |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid2} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                     |"};

      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 6-1 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {legacyProjectId1} | false      | BootCampDimensions.dc |" ,
      $"| CreateProjectEvent | 1d+09:00:00 | {projectUid2} | Boundary Test 6-2 | LandFill    | W. Europe Standard Time   | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary2}   | {customerUid} | {legacyProjectId1} | false      | BootCampDimensions.dc |" };
      await ts.PublishEventCollection(projectEventArray);

      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
    }

    [Fact]
    public async Task Create2SubscriptionsForProjectMonitoringAndCreateProjects()
    {
      Msg.Title("Project v4 test 7", "Create 2 subscriptions for project monitoring and create projects");
      var ts = new TestSupport();
      var legacyProjectId1 = TestSupport.GenerateLegacyProjectId();
      var projectUid1 = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid1 = Guid.NewGuid();
      var subscriptionUid2 = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID    | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                    |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                    |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid1} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
        $"| Subscription        | 0d+09:50:00 |               |           |                   | {subscriptionUid2} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
      };
      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 7-1 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {legacyProjectId1} | false      |" ,
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | Boundary Test 7-2 | ProjectMonitoring | Mountain Standard Time    | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary2}   | {customerUid} | {legacyProjectId1} | false      |" };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
    }

    [Fact]
    public async Task Create2SubscriptionsForProjectMonitoringAndTryToCreateALandfillProjects()
    {
      Msg.Title("Project v4 test 8", "Create 2 subscriptions for project monitoring and try to create a landfill project");
      var ts = new TestSupport();
      var legacyProjectId1 = TestSupport.GenerateLegacyProjectId();
      var projectUid1 = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid1 = Guid.NewGuid();
      var subscriptionUid2 = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID    | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                    |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                    |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid1} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
      $"| Subscription        | 0d+09:50:00 |               |           |                   | {subscriptionUid2} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                    |                |                  | {startDate} |                | {projectUid1} |          | {subscriptionUid1}  |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                    |                |                  | {startDate} |                | {projectUid2} |          | {subscriptionUid2}  |"
      };
      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName     | ProjectType  | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 8 | LandFill     | Mountain Standard Time    | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {legacyProjectId1} | false      | BootCampDimensions.dc |" };
      var response = await ts.PublishEventToWebApi(projectEventArray, HttpStatusCode.BadRequest);

      Assert.True(response == "There are no available subscriptions for the selected customer.", "Should not be any subscriptions so project not created. Response: " + response);
    }


    [Fact]
    public async Task Create2StandardProjectsWithAdjacentBoundarys()
    {
      Msg.Title("Project v4 test 9", "Create standard project and customer with adjacent boundarys");
      var ts = new TestSupport();
      var legacyProjectId1 = TestSupport.GenerateLegacyProjectId();
      var legacyProjectId2 = TestSupport.GenerateLegacyProjectId();
      var projectUid1 = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);

      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 1 | 1                 | {customerUid} |"};
      await ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray1 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 9-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary3}   | {customerUid} | {legacyProjectId1} |false      |" };
      var response1 = await ts.PublishEventToWebApi(projectEventArray1);
      Assert.True(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | Boundary Test 9-2 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary4}   | {customerUid} | {legacyProjectId2} |false      |" };
      var response2 = await ts.PublishEventToWebApi(projectEventArray2);
      Assert.True(response2 == "success", "Response is unexpected. Should be a success. Response: " + response2);

      var projectEventArray3 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 9-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary3}   | {customerUid} | {legacyProjectId1} |false      |" ,
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | Boundary Test 9-2 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary4}   | {customerUid} | {legacyProjectId2} |false      |" };

      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray3, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid2, projectEventArray2, true);
    }

    [Fact]
    public async Task Create2StandardProjectsWithOverLappingBoundarys()
    {
      Msg.Title("Project v4 test 10", "Create standard project and customer with overlapping boundarys");
      var ts = new TestSupport();
      var legacyProjectId1 = TestSupport.GenerateLegacyProjectId();
      var legacyProjectId2 = TestSupport.GenerateLegacyProjectId();
      var projectUid1 = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);

      const string geometryWkt2 = "POLYGON((172.595071 -43.542112,172.595562 -43.543218,172.59766 -43.542353,172.595071 -43.542112))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 1 | 1                 | {customerUid} |"};
      await ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray1 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 9-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary3}   | {customerUid} | {legacyProjectId1} |false      |" };
      var response1 = await ts.PublishEventToWebApi(projectEventArray1);
      Assert.True(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | Boundary Test 9-2 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId2} |false      |" };
      var response2 = await ts.PublishEventToWebApi(projectEventArray2, HttpStatusCode.BadRequest);
      Assert.True(response2 == "Project boundary overlaps another project, for this customer and time span.", "Response is unexpected. Should be a success. Response: " + response2);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray1, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid1, projectEventArray1, true);
    }

    [Fact]
    public async Task Create2StandardProjectsWithOverlappingBoundarysButProjectDatesArent()
    {
      Msg.Title("Project v4 test 11", "Create standard project and customer with overlapping boundarys but project dates aren't");
      var ts = new TestSupport();
      var legacyProjectId1 = TestSupport.GenerateLegacyProjectId();
      var legacyProjectId2 = TestSupport.GenerateLegacyProjectId();
      var projectUid1 = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = DateTime.UtcNow.Date.AddMonths(2);
      var startDateTime2 = DateTime.UtcNow.Date.AddMonths(3);
      var endDateTime2 = DateTime.UtcNow.Date.AddMonths(9);

      const string geometryWkt2 = "POLYGON((172.595071 -43.542112,172.595562 -43.543218,172.59766 -43.542353,172.595071 -43.542112))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name             | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 11 | 1                 | {customerUid} |"};
      await ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray1 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName        | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 11-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary3}   | {customerUid} | {legacyProjectId1} |false      |" };
      var response1 = await ts.PublishEventToWebApi(projectEventArray1);
      Assert.True(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName        | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | Boundary Test 11-2 | Standard    | New Zealand Standard Time |{startDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff} | {geometryWkt2}   | {customerUid} | {legacyProjectId2} |false      |" };
      var response2 = await ts.PublishEventToWebApi(projectEventArray2);
      Assert.True(response2 == "success", "Response is unexpected. Should be a success. Response: " + response2);

      var projectEventArray3 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName        | ProjectType | ProjectTimezone           | ProjectStartDate                             | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 11-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}   | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary3}   | {customerUid} | {legacyProjectId1} |false      |" ,
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | Boundary Test 11-2 | Standard    | New Zealand Standard Time |{startDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff} | {geometryWkt2}   | {customerUid} | {legacyProjectId2} |false      |" };

      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray3, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid2, projectEventArray2, true);
    }

    [Fact]
    public async Task TryCreateLandfillProjectWithNoSubscription()
    {
      Msg.Title("Project v4 test 12", "Try to create landfill project with no subscription");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);

      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      };

      await ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 12 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | BootCampDimensions.dc |" };
      var response1 = await ts.PublishEventToWebApi(projectEventArray, HttpStatusCode.BadRequest);
      Assert.True(response1 == "There are no available subscriptions for the selected customer.", "Response is unexpected. Should be There are no available subscriptions for the selected customer. Response: " + response1);
    }

    [Fact]
    public async Task TryCreateProjectMonitoringProjectWithNoSubscription()
    {
      Msg.Title("Project v4 test 13", "Try to create project monitoring project with no subscription");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);

      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      };
      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 13 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      |" };
      var response1 = await ts.PublishEventToWebApi(projectEventArray, HttpStatusCode.BadRequest);
      Assert.True(response1 == "There are no available subscriptions for the selected customer.", "Response is unexpected. Should be There are no available subscriptions for the selected customer. Response: " + response1);
    }

    [Fact]
    public async Task Create2LandfillProjectsWithOverLappingBoundarys()
    {
      Msg.Title("Project v4 test 14", "Create standard project and customer with overlapping boundarys");
      var ts = new TestSupport();
      var legacyProjectId1 = TestSupport.GenerateLegacyProjectId();
      var projectUid1 = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid1 = Guid.NewGuid();
      var subscriptionUid2 = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      const string geometryWkt2 = "POLYGON((172.595309 -43.542883,172.594735 -43.543357,172.594037 -43.542756,172.595309 -43.542883))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID    | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID  |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                    |                |                  |             |                |               |          |                     |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                    |                |                  |             |                |               | {tccOrg} |                     |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid1} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                     |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid2} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                     |"};

      await ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray1 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 14  | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary3}   | {customerUid} | {legacyProjectId1} | false      | BootCampDimensions.dc |" };
      var response1 = await ts.PublishEventToWebApi(projectEventArray1);
      Assert.True(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 1d+09:00:00 | {projectUid2} | Boundary Test 14a | LandFill    | W. Europe Standard Time   | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId1} | false      | BootCampDimensions.dc |" };
      var response2 = await ts.PublishEventToWebApi(projectEventArray2, HttpStatusCode.BadRequest);
      Assert.True(response2 == "Project boundary overlaps another project, for this customer and time span.", "Response is unexpected. Should be a Project boundary overlaps another project, for this customer and time span. Response: " + response2);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray1, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid1, projectEventArray1, true);
    }

    [Fact]
    public async Task CreateLandfillProjectThenUpdateEndDate()
    {
      Msg.Title("Project v4 test 15", "Create landfill project and customer then update end date");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.UtcNow.Date.AddYears(2);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"};
      await ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 15 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | BootCampDimensions.dc |" };
      await ts.PublishEventCollection(projectEventArray);

      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description      | ",
      $"| UpdateProjectRequest | 1d+09:00:00 | {projectUid} | Boundary Test 15 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | BootCampDimensions.dc | test description |" };
      await ts.PublishEventCollection(projectEventArray2);

      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, true);
    }

    [Fact]
    public async Task CreateLandfillProjectThenUpdateProjectNameEndDateAndDescription()
    {
      Msg.Title("Project v4 test 16", "Create landfill project and customer then try and update project name, end date and description");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.UtcNow.Date.AddYears(2);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"};
      await ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 16 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | BootCampDimensions.dc |" };
      await ts.PublishEventCollection(projectEventArray);

      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName          | ProjectType | ProjectTimezone            | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description |",
      $"| UpdateProjectRequest | 0d+09:00:00 | {projectUid} | Boundary Test chg 16 | LandFill    | New Zealand Standard Time  | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | BootCampDimensions.dc | Change Desc |" };
      await ts.PublishEventToWebApi(projectEventArray2);

      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, true);
    }

    [Fact]
    public async Task CreateLandfillProjectThenUpdateProjectTypeToStandard()
    {
      var msgNumber = "17a";
      Msg.Title($"Project v4 test {msgNumber}", "Create landfill project and customer then update project type and cordinate system file");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.UtcNow.Date.AddYears(2);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"};
      await ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 17 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | BootCampDimensions.dc |" };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description       |",
      $"| UpdateProjectRequest | 0d+10:00:00 | {projectUid} | Boundary Test 17 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | ChangeCordinatesystem | dummy description |" };
      var response = await ts.PublishEventToWebApi(projectEventArray2, HttpStatusCode.InternalServerError);
      var expectedErrorMessage =
        "UpdateProjectV4: Invalid ProjectType. Can ony be changed from Standard to Landfill/Civil.";
      Assert.True(response == expectedErrorMessage, $"Response is unexpected. Should be: {expectedErrorMessage} Response: " + response);
    }

    [Fact]
    public async Task CreateStandardProjectThenUpdateProjectTypeAndCoordinateFile()
    {
      var msgNumber = "17a";
      Msg.Title($"Project v4 test {msgNumber}", "Create standard project and customer then update project type and cordinate system file");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.UtcNow.Date.AddYears(2);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"};
      await ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectName = $"Boundary Test {msgNumber}";
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | BootCampDimensions.dc |" };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description       |",
      $"| UpdateProjectRequest | 0d+10:00:00 | {projectUid} | {projectName} | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | ChangeCordinatesystem | dummy description |" };
      var response = await ts.PublishEventToWebApi(projectEventArray2);
      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, true);
    }

    [Fact]
    public async Task CreateStandardProject_ThenUpdateProjectTypeAndProjectBoundary()
    {
      var msgNumber = "17b";
      Msg.Title($"Project v4 test {msgNumber}", "Create standard project then update project type and boundary");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.UtcNow.Date.AddYears(2);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      const string updatedGeometryWkt = "POLYGON((-12 3,-12.3 3,-12.3 4,-12.3 4,-12.8 4,-12 3))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"};
      await ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectName = $"Boundary Test {msgNumber}";
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | BootCampDimensions.dc |" };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary      | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description       |",
      $"| UpdateProjectRequest | 0d+10:00:00 | {projectUid} | {projectName} | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {updatedGeometryWkt} | {customerUid} | 1 |false      | ChangeCordinatesystem | dummy description |" };

      // we haven't written the Geofence DB row
      // projectSvc should pass as we now create a new Geofence and ProjectGeofence for the Project.
      var response = await ts.PublishEventToWebApi(projectEventArray2);
      Assert.True(response == "success", "Response is unexpected. Should be a 'success'. Response: " + response);

      // check that that Project.ProjectBoundary is the changed type and geometry
      //    can't verify that the Subscription is available again as it is done vis subSvc. Must be done manually
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, true);
    }

    [Fact]
    public async Task CreateStandardProject_ThenUpdateProjectBoundary_MissMatchedProjectGeofence()
    {
      var msgNumber = "17b";
      Msg.Title($"Project v4 test {msgNumber}", "Create standard project then update project boundary, where there is a mismatched ProjectGeofence");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.UtcNow.Date.AddYears(2);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      const string updatedGeometryWkt = "POLYGON((-12 3,-12.3 3,-12.3 4,-12.3 4,-12.8 4,-12 3))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"};
      await ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectName = $"Boundary Test {msgNumber}";
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | BootCampDimensions.dc |" };
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary      | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description       |",
      $"| UpdateProjectRequest | 0d+10:00:00 | {projectUid} | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {updatedGeometryWkt} | {customerUid} | 1 |false      | ChangeCordinatesystem | dummy description |" };
      var response = await ts.PublishEventToWebApi(projectEventArray2);

      var expectedMessage = "success";
      Assert.Equal(expectedMessage, response);
    }

    [Fact]
    public async Task CreateStandardProjectWithCoordinateSystemAndGetProjectListV4()
    {
      Msg.Title("Project v4 test 18", "Create standard project and With CoordinateSystem customer then read the project list. No subscription");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);

      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 1 | 1                 | {customerUid} |"};
      await ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 18 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | BootCampDimensions.dc |" };
      await ts.PublishEventCollection(projectEventArray);

      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
    }

    [Fact]
    public async Task CreateLandfillProjectWithCoordinateSystemAndSubscriptionThenGetProjectListV4()
    {
      Msg.Title("Project v4 test 19", "Create landfill project With CoordinateSystem and customer then read the project list");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"};

      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 19 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | BootCampDimensions.dc |" };
      await ts.PublishEventCollection(projectEventArray);

      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
    }

    [Fact]
    public async Task CreateProjectMonitoringWithCoordinateSystemProjectAndSubscriptionThenGetProjectListV4()
    {
      Msg.Title("Project v4 test 20", "Create project monitoring project With CoordinateSystem of and customer then read the project list");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
      };
      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 20 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | BootCampDimensions.dc |" };
      await ts.PublishEventCollection(projectEventArray);

      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
    }

    [Fact]
    public async Task TryCreateLandfillProjectWithoutCoordinateSystem()
    {
      Msg.Title("Project v4 test 21", "Try to create landfill project without a corordinate system");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      await ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 21 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | " };
      await ts.PublishEventCollection(projectEventArray, HttpStatusCode.BadRequest);
      var response = await ts.PublishEventToWebApi(projectEventArray, HttpStatusCode.BadRequest);
      Assert.True(response == "Landfill is missing its CoordinateSystem.", "Response is unexpected. Should be a Landfill is missing its CoordinateSystem. Response: " + response);
    }

    [Fact]
    public async Task CreateStandardProjectThenUpdateCoordinateSystem()
    {
      Msg.Title("Project v4 test 22", "Create standard project and customer then update coordinate system");

      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.UtcNow.Date.AddYears(2);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      await ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | Description                  |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 22 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | Boundary Test 22 description |"};
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description        |",
      $"| UpdateProjectRequest | 0d+09:00:00 | {projectUid} | Boundary Test 22 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | BootCampDimensions.dc | Change description |"};
      var response = await ts.PublishEventToWebApi(projectEventArray2);

      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, true);
    }

    [Fact]
    public async Task CreateStandardProjectThenUpdateCoordinateSystemThenDelete()
    {
      Msg.Title("Project v4 test 23", "Create standard project and customer then update coordinate system then archive");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.UtcNow.Date.AddYears(2);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      await ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 23 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 | false      |"};
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
      $"| UpdateProjectRequest | 1d+09:00:00 | {projectUid} | Boundary Test 23 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 | false      | BootCampDimensions.dc | description |" };
      await ts.PublishEventToWebApi(projectEventArray2);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, true);

      ts.FirstEventDate = DateTime.UtcNow;
      ts.ProjectUid = new Guid(projectUid);

      // project deletion sets the ProjectEndDate to now, in the projects timezone.
      //    this may cause the endDate to be a day earlier/later than 'NowUtc',
      //    depending on when this test is run.
      // note that only projectUID is passed from this array to the ProjectSvc endpoint,
      //    the others are simply used for comparison
      var endDateTime2Reset = DateTime.UtcNow.ToLocalDateTime("Pacific/Auckland")?.Date;

      var projectEventArray3 = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | ",
      $"| DeleteProjectEvent | 1d+09:00:00 | {projectUid} | Boundary Test 23 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2Reset:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 | true       | BootCampDimensions.dc |" };
      var response = await ts.PublishEventToWebApi(projectEventArray3);
      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray3, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray3, true);
    }

    [Fact]
    public async Task CreateStandardProjectThenUpdateProjectBoundary()
    {
      Msg.Title("Project v4 test 24", "Create standard project then update projectBoundary");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.UtcNow.Date.AddYears(2);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      await ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | Description                  |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 24 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | 1 |false      | Boundary Test 22 description |"};
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      const string updatedGeometryWkt = "POLYGON((-122 39,-122.3 39.8,-122.3 39.8,-122.34 39.83,-122.8 39.4,-122 39))";
      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description        |",
      $"| UpdateProjectRequest | 0d+09:00:00 | {projectUid} | Boundary Test 24 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {updatedGeometryWkt}   | {customerUid} | 1 |false      | BootCampDimensions.dc | Change description |"};
      var response = await ts.PublishEventToWebApi(projectEventArray2);

      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      await ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, true);
    }

    [Fact]
    public async Task CreateStandardProjectsThenUpdateProjectBoundary_Overlapping()
    {
      Msg.Title("Project v4 test 25", "Create 2standard projects, 2nd with boundary to be updated then update 1st projectBoundary");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var startDateTime2 = startDateTime.AddDays(1);
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = endDateTime;
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      const string updatedGeometryWkt = "POLYGON((-12 3,-12.3 3,-12.3 4,-12.3 4,-12.8 4,-12 3))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      await ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                             | ProjectEndDate                              | ProjectBoundary        | CustomerUID   | CustomerID        |IsArchived | Description                  |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid}  | Boundary Test 22 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}   | {Boundaries.Boundary1}          | {customerUid} | 1 |false      | Boundary Test 22 description |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | Boundary Test 23 | Standard    | New Zealand Standard Time | {startDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {updatedGeometryWkt}   | {customerUid} | 1 |false      | Boundary Test 22 description |"};
      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description        |",
      $"| UpdateProjectRequest | 0d+09:00:00 | {projectUid} | Boundary Test 22 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {updatedGeometryWkt}   | {customerUid} | 1 |false      | BootCampDimensions.dc | Change description |"};
      var response = await ts.PublishEventToWebApi(projectEventArray2, HttpStatusCode.BadRequest);

      Assert.True(response == "Project boundary overlaps another project, for this customer and time span.", "Response is unexpected. Should fail with overlap. Response: " + response);
    }

    [Fact]
    public async Task CreateStandardProjectThenUpdateProjectBoundary_OverlappingSelf_OK()
    {
      var msgNumber = "25a";
      Msg.Title($"Project v4 test {msgNumber}", "Create standard project, then update with overlapping projectBoundary");
      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = endDateTime;
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-12 3,-12.3 3,-12.3 4,-12.3 4,-12.8 4,-12 3))";
      const string overlapGeometryWkt = "POLYGON((-12 3.0,-12.3 3,-12.3 4,-12.3 4,-12.8 4,-12 3))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      await ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectName = $"Boundary Test {msgNumber}";
      var projectEventArray = new[]
      {
         "| EventType          | EventDate   | ProjectUID    | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                             | ProjectEndDate                              | ProjectBoundary        | CustomerUID   | CustomerID        |IsArchived | Description                  |",
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid}  | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}   | {geometryWkt}          | {customerUid} | 1 |false      | Boundary Test 22 description |"};

      await ts.PublishEventCollection(projectEventArray);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description        |",
      $"| UpdateProjectRequest | 0d+09:00:00 | {projectUid} | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {overlapGeometryWkt}   | {customerUid} | 1 |false      | BootCampDimensions.dc | Change description |"};
      var response = await ts.PublishEventToWebApi(projectEventArray2);
      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
    }

    [Fact]
    public async Task CreateStandardProjectWithNoProjectUidAndGetProjectListV4()
    {
      Msg.Title("Project v4 test 26", "Create standard project and customer then read the project list. No project id");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);

      var customerEventArray = new[] {
       "| TableName | EventDate   | Name             | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 24 | 1                 | {customerUid} |"};
      await ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | No Project ID | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | " };
      var response = await ts.PublishEventToWebApi(projectEventArray);
      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
    }

    [Fact]
    public async Task CreateStandardProjectWithNoCustomerUidAndGetProjectListV4()
    {
      Msg.Title("Project v4 test 27", "Create standard project and customer then read the project list. No customer id and no project id");
      var ts = new TestSupport();
      ts.SetCustomerUid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);

      var customerEventArray = new[] {
       "| TableName | EventDate   | Name             | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 25 | 1                 | {ts.CustomerUid} |"};
      await ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName    | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | ",
      $"| CreateProjectRequest | 0d+09:00:00 | No Customer ID | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | " };
      var response = await ts.PublishEventToWebApi(projectEventArray);
      Assert.True(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      await ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, ts.CustomerUid, projectEventArray, true);
    }   
   
  }
}
