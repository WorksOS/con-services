using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;
using VSS.MasterData.Repositories.DBModels;

namespace WebApiTests
{
  [TestClass]
  public class ProjectV4Tests
  {
    private readonly Msg msg = new Msg();

    [TestMethod]
    public void CreateStandardProjectAndGetProjectListV4()
    {
      msg.Title("Project v4 test 1", "Create standard project and customer then read the project list. No subscription");
      var ts = new TestSupport(); 
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 1 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName     | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
    }

    [TestMethod] 
    public void CreateLandfillProjectAndSubscriptionThenGetProjectListV4()
    {
      msg.Title("Project v4 test 2", "Create landfill project and customer then read the project list");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"};
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName     | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description                 |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 2 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | Boundary Test 2 Description |"};
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
    }

    [TestMethod]
    public void CreateProjectMonitoringProjectAndSubscriptionThenGetProjectListV4()
    {
      msg.Title("Project v4 test 3", "Create project monitoring project of and customer then read the project list");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
      };
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName     | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 3 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
    }


    [TestMethod]
    public void CreateProjectWithoutLegacyProjectId()
    {
      msg.Title("Project v4 test 4", "Create standard project with out legacy project id");
      var ts = new TestSupport(); 
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 1 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName     | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 4 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
    }

    [TestMethod] 
    public void Create2SubscriptionsForLandfillAndCreateProjects()
    {
      msg.Title("Project v4 test 6", "Create landfill project and customer then read the project list");
      var ts = new TestSupport();
      var legacyProjectId1 = ts.SetLegacyProjectId();
      var legacyProjectId2 = legacyProjectId1+1;
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
      const string geometryWkt1 = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      const string geometryWkt2 = "POLYGON((-115.025723657623 36.2101347890754,-115.026281557098 36.2056332151707,-115.018041811005 36.205460072542,-115.025723657623 36.2101347890754))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID    | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID  |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                    |                |                  |             |                |               |          |                     |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                    |                |                  |             |                |               | {tccOrg} |                     |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid1} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                     |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid2} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                     |"};

      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 6-1 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} | false      | BootCampDimensions.dc |" ,
      $"| CreateProjectEvent | 1d+09:00:00 | {projectUid2} | Boundary Test 6-2 | LandFill    | W. Europe Standard Time   | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId1} | false      | BootCampDimensions.dc |" };
      ts.PublishEventCollection(projectEventArray);
      Thread.Sleep(6000);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
    }

    [TestMethod]
    public void Create2SubscriptionsForProjectMonitoringAndCreateProjects()
    {
      msg.Title("Project v4 test 7", "Create 2 subscriptions for project monitoring and create projects");
      var ts = new TestSupport();
      var legacyProjectId1 = ts.SetLegacyProjectId();
      var legacyProjectId2 = legacyProjectId1+1;
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
      const string geometryWkt1 = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      const string geometryWkt2 = "POLYGON((-115.025723657623 36.2101347890754,-115.026281557098 36.2056332151707,-115.018041811005 36.205460072542,-115.025723657623 36.2101347890754))";
      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID    | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                    |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                    |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid1} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
        $"| Subscription        | 0d+09:50:00 |               |           |                   | {subscriptionUid2} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
      };
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 7-1 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} | false      |" ,
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | Boundary Test 7-2 | ProjectMonitoring | Mountain Standard Time    | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId1} | false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
    }

    [TestMethod]
    public void Create2SubscriptionsForProjectMonitoringAndTryToCreateALandfillProjects()
    {
      msg.Title("Project v4 test 8", "Create 2 subscriptions for project monitoring and try to create a landfill project");
      var ts = new TestSupport();
      var legacyProjectId1 = ts.SetLegacyProjectId();
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
      const string geometryWkt1 = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID    | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                    |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                    |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid1} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
      $"| Subscription        | 0d+09:50:00 |               |           |                   | {subscriptionUid2} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                    |                |                  | {startDate} |                | {projectUid1} |          | {subscriptionUid1}  |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                    |                |                  | {startDate} |                | {projectUid2} |          | {subscriptionUid2}  |"
      };
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName     | ProjectType  | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 8 | LandFill     | Mountain Standard Time    | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} | false      | BootCampDimensions.dc |" };
      var response = ts.PublishEventToWebApi(projectEventArray);

      Assert.IsTrue(response == "There are no available subscriptions for the selected customer.", "Should not be any subscriptions so project not created. Response: " + response);
    }


    [TestMethod]
    public void Create2StandardProjectsWithAdjacentBoundarys()
    {
      msg.Title("Project v4 test 9", "Create standard project and customer with adjacent boundarys");
      var ts = new TestSupport(); 
      var legacyProjectId1 = ts.SetLegacyProjectId();
      var legacyProjectId2 = legacyProjectId1 + 1;
      var projectUid1 = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      const string geometryWkt1 = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965,172.595831670724 -43.5427038560109))";
      const string geometryWkt2 = "POLYGON((172.595838 -43.542708,172.594636 -43.54389,172.596568 -43.5438,172.595838 -43.542708))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 1 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray1 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 9-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} |false      |" };
      var response1 = ts.PublishEventToWebApi(projectEventArray1);
      Assert.IsTrue(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | Boundary Test 9-2 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId2} |false      |" };
      var response2 = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response2 == "success", "Response is unexpected. Should be a success. Response: " + response2);

      var projectEventArray3 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 9-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} |false      |" ,
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | Boundary Test 9-2 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId2} |false      |" };

      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray3, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid2, projectEventArray2, true);
    }

    [TestMethod]
    public void Create2StandardProjectsWithOverLappingBoundarys()
    {
      msg.Title("Project v4 test 10", "Create standard project and customer with overlapping boundarys");
      var ts = new TestSupport(); 
      var legacyProjectId1 = ts.SetLegacyProjectId();
      var legacyProjectId2 = legacyProjectId1 + 1;
      var projectUid1 = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      const string geometryWkt1 = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965,172.595831670724 -43.5427038560109))";
      const string geometryWkt2 = "POLYGON((172.595071 -43.542112,172.595562 -43.543218,172.59766 -43.542353,172.595071 -43.542112))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 1 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray1 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 9-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} |false      |" };
      var response1 = ts.PublishEventToWebApi(projectEventArray1);
      Assert.IsTrue(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | Boundary Test 9-2 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId2} |false      |" };
      var response2 = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response2 == "Project boundary overlaps another project, for this customer and time span.", "Response is unexpected. Should be a success. Response: " + response2);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray1, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid1, projectEventArray1, true);
    }

    [TestMethod]
    public void Create2StandardProjectsWithOverlappingBoundarysButProjectDatesArent()
    {
      msg.Title("Project v4 test 11", "Create standard project and customer with overlapping boundarys but project dates aren't");
      var ts = new TestSupport(); 
      var legacyProjectId1 = ts.SetLegacyProjectId();
      var legacyProjectId2 = legacyProjectId1 + 1;
      var projectUid1 = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = DateTime.Now.Date.AddMonths(2);
      var startDateTime2 = DateTime.Now.Date.AddMonths(3);
      var endDateTime2 = DateTime.Now.Date.AddMonths(9);
      const string geometryWkt1 = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965,172.595831670724 -43.5427038560109))";
      const string geometryWkt2 = "POLYGON((172.595071 -43.542112,172.595562 -43.543218,172.59766 -43.542353,172.595071 -43.542112))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name             | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 11 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray1 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName        | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 11-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} |false      |" };
      var response1 = ts.PublishEventToWebApi(projectEventArray1);
      Assert.IsTrue(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName        | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | Boundary Test 11-2 | Standard    | New Zealand Standard Time |{startDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff} | {geometryWkt2}   | {customerUid} | {legacyProjectId2} |false      |" };
      var response2 = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response2 == "success", "Response is unexpected. Should be a success. Response: " + response2);

      var projectEventArray3 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName        | ProjectType | ProjectTimezone           | ProjectStartDate                             | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 11-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}   | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} |false      |" ,
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | Boundary Test 11-2 | Standard    | New Zealand Standard Time |{startDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff} | {geometryWkt2}   | {customerUid} | {legacyProjectId2} |false      |" };
      //Thread.Sleep(3000);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray3, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid2, projectEventArray2, true);
    }

    [TestMethod] 
    public void TryCreateLandfillProjectWithNoSubscription()
    {
      msg.Title("Project v4 test 12", "Try to create landfill project with no subscription");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      };

      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 12 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc |" };
      var response1 = ts.PublishEventToWebApi(projectEventArray);
      Assert.IsTrue(response1 == "There are no available subscriptions for the selected customer.", "Response is unexpected. Should be There are no available subscriptions for the selected customer. Response: " + response1);
    }

    [TestMethod]
    public void TryCreateProjectMonitoringProjectWithNoSubscription()
    {
      msg.Title("Project v4 test 13", "Try to create project monitoring project with no subscription");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      };
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 13 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      var response1 = ts.PublishEventToWebApi(projectEventArray);
      Assert.IsTrue(response1 == "There are no available subscriptions for the selected customer.", "Response is unexpected. Should be There are no available subscriptions for the selected customer. Response: " + response1);
    }

   [TestMethod] 
    public void Create2LandfillProjectsWithOverLappingBoundarys()
    {
      msg.Title("Project v4 test 14", "Create standard project and customer with overlapping boundarys");
      var ts = new TestSupport();
      var legacyProjectId1 = ts.SetLegacyProjectId();
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
      const string geometryWkt1 = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965,172.595831670724 -43.5427038560109))";
      const string geometryWkt2 = "POLYGON((172.595309 -43.542883,172.594735 -43.543357,172.594037 -43.542756,172.595309 -43.542883))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID    | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID  |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                    |                |                  |             |                |               |          |                     |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                    |                |                  |             |                |               | {tccOrg} |                     |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid1} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                     |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid2} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                     |"};

      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray1 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | Boundary Test 14  | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} | false      | BootCampDimensions.dc |" };
      var response1 = ts.PublishEventToWebApi(projectEventArray1);
      Assert.IsTrue(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 1d+09:00:00 | {projectUid2} | Boundary Test 14a | LandFill    | W. Europe Standard Time   | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId1} | false      | BootCampDimensions.dc |" };
      var response2 = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response2 == "Project boundary overlaps another project, for this customer and time span.", "Response is unexpected. Should be a Project boundary overlaps another project, for this customer and time span. Response: " + response2);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray1, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid1, projectEventArray1, true);
    }

    [TestMethod]
    public void CreateLandfillProjectThenUpdateEndDate()
    {
      msg.Title("Project v4 test 15", "Create landfill project and customer then update end date");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
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
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 15 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc |" };
      ts.PublishEventCollection(projectEventArray);
      Thread.Sleep(5000);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description      | ",
      $"| UpdateProjectRequest | 1d+09:00:00 | {projectUid} | Boundary Test 15 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | test description |" };
      ts.PublishEventCollection(projectEventArray2);
      Thread.Sleep(5000);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, true);
    }

    [TestMethod] 
    public void CreateLandfillProjectThenUpdateProjectNameEndDateAndDescription()
    {
      msg.Title("Project v4 test 16", "Create landfill project and customer then try and update project name, end date and description");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
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
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 16 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc |" };
      ts.PublishEventCollection(projectEventArray);
      Thread.Sleep(5000);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName          | ProjectType | ProjectTimezone            | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description |",
      $"| UpdateProjectRequest | 0d+09:00:00 | {projectUid} | Boundary Test chg 16 | LandFill    | New Zealand Standard Time  | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | Change Desc |" };
      ts.PublishEventToWebApi(projectEventArray2);
      //Thread.Sleep(3000);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, true);
    }

    [TestMethod] 
    public void CreateLandfillProjectThenUpdateProjectTypeToStandard()
    {
      var msgNumber = "17a";
      msg.Title($"Project v4 test {msgNumber}", "Create landfill project and customer then update project type and cordinate system file");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
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
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 17 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description       |",
      $"| UpdateProjectRequest | 0d+10:00:00 | {projectUid} | Boundary Test 17 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | ChangeCordinatesystem | dummy description |" };
      var response = ts.PublishEventToWebApi(projectEventArray2);
      var expectedErrorMessage =
        "UpdateProjectV4: Invalid ProjectType. Can ony be changed from Standard to Landfill/Civil.";
      Assert.IsTrue(response == expectedErrorMessage, $"Response is unexpected. Should be: {expectedErrorMessage} Response: " + response);
    }

    [TestMethod]
    public void CreateStandardProjectThenUpdateProjectTypeAndCoordinateFile()
    {
      var msgNumber = "17a";
      msg.Title($"Project v4 test {msgNumber}", "Create standard project and customer then update project type and cordinate system file");
      var ts = new TestSupport();
      var mysql = new MySqlHelper();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
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
      ts.IsPublishToWebApi = true;
      var projectName = $"Boundary Test {msgNumber}";
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description       |",
      $"| UpdateProjectRequest | 0d+10:00:00 | {projectUid} | {projectName} | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | ChangeCordinatesystem | dummy description |" };
      var response = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, true);
    }


    [TestMethod]
    public void CreateStandardProject_ThenUpdateProjectTypeAndProjectBoundary()
    {
      var msgNumber = "17b";
      msg.Title($"Project v4 test {msgNumber}", "Create standard project then update project type and boundary");
      var ts = new TestSupport();
      var mysql = new MySqlHelper();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.Now.Date.AddYears(2);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      const string updatedGeometryWkt = "POLYGON((-12 3,-12.3 3,-12.3 4,-12.3 4,-12.8 4,-12 3))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"};
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectName = $"Boundary Test {msgNumber}";
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary      | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description       |",
      $"| UpdateProjectRequest | 0d+10:00:00 | {projectUid} | {projectName} | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {updatedGeometryWkt} | {customerUid} | {legacyProjectId} |false      | ChangeCordinatesystem | dummy description |" };

      // we haven't written the Geofence DB row
      // projectSvc should pass as we now create a new Geofence and ProjectGeofence for the Project.
      var response = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response == "success", "Response is unexpected. Should be a 'success'. Response: " + response);

      // check that that Project.ProjectBoundary is the changed type and geometry
      //    can't verify that the Subscription is available again as it is done vis subSvc. Must be done manually
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, true);
   }

    [TestMethod]
    public void CreateStandardProject_ThenUpdateProjectBoundary_MissMatchedProjectGeofence()
    {
      var msgNumber = "17b";
      msg.Title($"Project v4 test {msgNumber}", "Create standard project then update project boundary, where there is a mismatched ProjectGeofence");
      var ts = new TestSupport();
      var mysql = new MySqlHelper();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var endDateTime2 = DateTime.Now.Date.AddYears(2);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      const string updatedGeometryWkt = "POLYGON((-12 3,-12.3 3,-12.3 4,-12.3 4,-12.8 4,-12 3))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"};
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectName = $"Boundary Test {msgNumber}";
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      // now create the mismatched Geofence in the db, which we want the Project to associate to
      var geofenceSvcCreatedGeofenceUid = Guid.NewGuid().ToString();
      var projectSvcExpectedGeofenceUid = mysql.VerifyProjectGeofence(projectUid, 1);
      var geofenceEventArray = new[] {
        "| TableName  | EventType           | EventDate   | fk_CustomerUID | Description | FillColor  | GeofenceName  | fk_GeofenceTypeID            | GeofenceUID                      | GeometryWKT   | IsTransparent | Name          | UserUID        | IsDeleted | LastActionedUTC |",
        $"| Geofence   | CreateGeofenceEvent | 0d+09:00:00 | {customerUid}  | Fence       | 1         | {projectName} | {(int) GeofenceType.Project} | {geofenceSvcCreatedGeofenceUid}  | {geometryWkt} | {false}       | {projectName} | {customerUid}  | {false}   | 0d+09:00:00     |"};
      ts.IsPublishToWebApi = false;
      ts.PublishEventCollection(geofenceEventArray);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary      | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description       |",
      $"| UpdateProjectRequest | 0d+10:00:00 | {projectUid} | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {updatedGeometryWkt} | {customerUid} | {legacyProjectId} |false      | ChangeCordinatesystem | dummy description |" };

      // we expect the publish to fail, as call to Geofence mock will fail. That's ok.
      // projectSvc should adjust ProjectGeofence to point to the good geofence.
      var response = ts.PublishEventToWebApi(projectEventArray2);

      var expectedMessage = "UpdateGeofenceInGeofenceService: Unable to find the projects Geofence.";
      Assert.AreEqual(expectedMessage, response, "Response is unexpected. ");
      var updatedGeofenceUid = mysql.VerifyProjectGeofence(projectUid, 1);
      Assert.AreEqual(geofenceSvcCreatedGeofenceUid, updatedGeofenceUid, "ProjectGeofence Should have been re-positioned to the new GeofenceUid.");
    }


    [TestMethod]
    public void CreateStandardProjectWithCoordinateSystemAndGetProjectListV4()
    {
      msg.Title("Project v4 test 18", "Create standard project and With CoordinateSystem customer then read the project list. No subscription");
      var ts = new TestSupport(); 
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 1 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 18 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc |" };
      ts.PublishEventCollection(projectEventArray);
      //Thread.Sleep(3000);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
    }

    [TestMethod] 
    public void CreateLandfillProjectWithCoordinateSystemAndSubscriptionThenGetProjectListV4()
    {
      msg.Title("Project v4 test 19", "Create landfill project With CoordinateSystem and customer then read the project list");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"};

      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 19 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc |" };
      ts.PublishEventCollection(projectEventArray);
      Thread.Sleep(5000);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
    }

    [TestMethod]
    public void CreateProjectMonitoringWithCoordinateSystemProjectAndSubscriptionThenGetProjectListV4()
    {
      msg.Title("Project v4 test 20", "Create project monitoring project With CoordinateSystem of and customer then read the project list");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
      };
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 20 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc |" };
      ts.PublishEventCollection(projectEventArray);
      //Thread.Sleep(3000);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
    }

    [TestMethod]
    public void TryCreateLandfillProjectWithoutCoordinateSystem()
    {
      msg.Title("Project v4 test 21", "Try to create landfill project without a corordinate system");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 21 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | " };
      ts.PublishEventCollection(projectEventArray);
      var response = ts.PublishEventToWebApi(projectEventArray);
      Assert.IsTrue(response == "Landfill is missing its CoordinateSystem.", "Response is unexpected. Should be a Landfill is missing its CoordinateSystem. Response: " + response);
    }

    [TestMethod]
    public void CreateStandardProjectThenUpdateCoordinateSystem()
    {
      msg.Title("Project v4 test 22", "Create standard project and customer then update coordinate system");

      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
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
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | Description                  |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 22 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | Boundary Test 22 description |"};
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
      var geofenceUid = mysql.VerifyProjectGeofence(projectUid, 1);
      var geofenceEventArray = new[] {
         "| TableName  | EventType           | EventDate   | fk_CustomerUID | Description | FillColor | GeofenceName | fk_GeofenceTypeID            | GeofenceUID    | GeometryWKT   | IsTransparent | Name | UserUID        | IsDeleted | LastActionedUTC |",
        $"| Geofence   | CreateGeofenceEvent | 0d+09:00:00 | {customerUid}  | Fence       | 1         | Trump        | {(int) GeofenceType.Project} | {geofenceUid}  | {geometryWkt} | {false}       | Blah | {customerUid}  | {false}   | 0d+09:00:00     |"};
      ts.IsPublishToWebApi = false;
      ts.PublishEventCollection(geofenceEventArray);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description        |",
      $"| UpdateProjectRequest | 0d+09:00:00 | {projectUid} | Boundary Test 22 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | Change description |"};
      var response = ts.PublishEventToWebApi(projectEventArray2);

      Assert.IsTrue(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, true);
      mysql.VerifyProjectGeofence(projectUid, 1);
    }
    
    [TestMethod]
    public void CreateStandardProjectThenUpdateCoordinateSystemThenDelete()
    {
      msg.Title("Project v4 test 23", "Create standard project and customer then update coordinate system then archive");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
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
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 23 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} | false      |"};
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
      $"| UpdateProjectRequest | 1d+09:00:00 | {projectUid} | Boundary Test 23 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} | false      | BootCampDimensions.dc | description |" };
      ts.PublishEventToWebApi(projectEventArray2);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, true);

      ts.FirstEventDate = DateTime.Now;
      ts.ProjectUid = new Guid(projectUid);
      var projectEventArray3 = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | ",
      $"| DeleteProjectEvent | 1d+09:00:00 | {projectUid} | Boundary Test 23 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} | true       | BootCampDimensions.dc |" };
      var response = ts.PublishEventToWebApi(projectEventArray3);
      Assert.IsTrue(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray3, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray3, true);
    }

    [TestMethod]
    public void CreateStandardProjectThenUpdateProjectBoundary()
    {
      msg.Title("Project v4 test 24", "Create standard project then update projectBoundary");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
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
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | Description                  |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 24 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | Boundary Test 22 description |"};
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, true);
      var geofenceUid = mysql.VerifyProjectGeofence(projectUid, 1);
      var geofenceEventArray = new[] {
         "| TableName  | EventType           | EventDate   | fk_CustomerUID | Description | FillColor | GeofenceName | fk_GeofenceTypeID            | GeofenceUID    | GeometryWKT   | IsTransparent | Name | UserUID        | IsDeleted | LastActionedUTC |",
        $"| Geofence   | CreateGeofenceEvent | 0d+09:00:00 | {customerUid}  | Fence       | 1         | Trump        | {(int) GeofenceType.Project} | {geofenceUid}  | {geometryWkt} | {false}       | Blah | {customerUid}  | {false}   | 0d+09:00:00     |"};
      ts.IsPublishToWebApi = false;
      ts.PublishEventCollection(geofenceEventArray);

      const string updatedGeometryWkt = "POLYGON((-122 39,-122.3 39.8,-122.3 39.8,-122.34 39.83,-122.8 39.4,-122 39))";
      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description        |",
      $"| UpdateProjectRequest | 0d+09:00:00 | {projectUid} | Boundary Test 24 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {updatedGeometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | Change description |"};
      var response = ts.PublishEventToWebApi(projectEventArray2);

      Assert.IsTrue(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, true);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, true);
      mysql.VerifyProjectGeofence(projectUid, 1);
    }

    [TestMethod]
    public void CreateStandardProjectsThenUpdateProjectBoundary_Overlapping()
    {
      msg.Title("Project v4 test 25", "Create 2standard projects, 2nd with boundary to be updated then update 1st projectBoundary");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
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
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      const string updatedGeometryWkt = "POLYGON((-12 3,-12.3 3,-12.3 4,-12.3 4,-12.8 4,-12 3))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                             | ProjectEndDate                              | ProjectBoundary        | CustomerUID   | CustomerID        |IsArchived | Description                  |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid}  | Boundary Test 22 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}   | {geometryWkt}          | {customerUid} | {legacyProjectId} |false      | Boundary Test 22 description |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | Boundary Test 23 | Standard    | New Zealand Standard Time | {startDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {updatedGeometryWkt}   | {customerUid} | {legacyProjectId} |false      | Boundary Test 22 description |"};

      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      var geofenceUid = mysql.VerifyProjectGeofence(projectUid, 1);
      var geofenceEventArray = new[] {
         "| TableName  | EventType           | EventDate   | fk_CustomerUID | Description | FillColor | GeofenceName | fk_GeofenceTypeID            | GeofenceUID    | GeometryWKT   | IsTransparent | Name | UserUID        | IsDeleted | LastActionedUTC |",
        $"| Geofence   | CreateGeofenceEvent | 0d+09:00:00 | {customerUid}  | Fence       | 1         | Trump        | {(int) GeofenceType.Project} | {geofenceUid}  | {geometryWkt} | {false}       | Blah | {customerUid}  | {false}   | 0d+09:00:00     |"};
      ts.IsPublishToWebApi = false;
      ts.PublishEventCollection(geofenceEventArray);
      var geofenceUid2 = mysql.VerifyProjectGeofence(projectUid2, 1);
      var geofenceEventArray2 = new[] {
         "| TableName  | EventType           | EventDate   | fk_CustomerUID | Description | FillColor | GeofenceName | fk_GeofenceTypeID            | GeofenceUID     | GeometryWKT          | IsTransparent | Name | UserUID        | IsDeleted | LastActionedUTC |",
        $"| Geofence   | CreateGeofenceEvent | 0d+09:00:00 | {customerUid}  | Fence       | 1         | Trump        | {(int) GeofenceType.Project} | {geofenceUid2}  | {updatedGeometryWkt} | {false}       | Blah | {customerUid}  | {false}   | 0d+09:00:00     |"};
      ts.IsPublishToWebApi = false;
      ts.PublishEventCollection(geofenceEventArray2);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description        |",
      $"| UpdateProjectRequest | 0d+09:00:00 | {projectUid} | Boundary Test 22 | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {updatedGeometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | Change description |"};
      var response = ts.PublishEventToWebApi(projectEventArray2);

      Assert.IsTrue(response == "Project boundary overlaps another project, for this customer and time span.", "Response is unexpected. Should fail with overlap. Response: " + response);
    }

    [TestMethod]
    public void CreateStandardProjectThenUpdateProjectBoundary_OverlappingSelf_OK()
    {
      var msgNumber = "25a";
      msg.Title($"Project v4 test {msgNumber}", "Create standard project, then update with overlapping projectBoundary");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
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
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectName = $"Boundary Test {msgNumber}";
      var projectEventArray = new[]
      {
         "| EventType          | EventDate   | ProjectUID    | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                             | ProjectEndDate                              | ProjectBoundary        | CustomerUID   | CustomerID        |IsArchived | Description                  |",
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid}  | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}   | {geometryWkt}          | {customerUid} | {legacyProjectId} |false      | Boundary Test 22 description |"};
 
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
      var geofenceUid = mysql.VerifyProjectGeofence(projectUid, 1);
      var geofenceEventArray = new[] {
         "| TableName  | EventType           | EventDate   | fk_CustomerUID | Description | FillColor | GeofenceName | fk_GeofenceTypeID            | GeofenceUID    | GeometryWKT   | IsTransparent | Name | UserUID        | IsDeleted | LastActionedUTC |",
        $"| Geofence   | CreateGeofenceEvent | 0d+09:00:00 | {customerUid}  | Fence       | 1         | Trump        | {(int) GeofenceType.Project} | {geofenceUid}  | {geometryWkt} | {false}       | Blah | {customerUid}  | {false}   | 0d+09:00:00     |"};
      ts.IsPublishToWebApi = false;
      ts.PublishEventCollection(geofenceEventArray);

      var projectEventArray2 = new[] {
       "| EventType            | EventDate   | ProjectUID   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description        |",
      $"| UpdateProjectRequest | 0d+09:00:00 | {projectUid} | {projectName} | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {overlapGeometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | Change description |"};
      var response = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
    }

    [TestMethod]
    public void CreateStandardProjectWithNoProjectUidAndGetProjectListV4()
    {
      msg.Title("Project v4 test 26", "Create standard project and customer then read the project list. No project id");
      var ts = new TestSupport(); 
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name             | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 24 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | ",
      $"| CreateProjectRequest | 0d+09:00:00 | No Project ID | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | " };
      var response = ts.PublishEventToWebApi(projectEventArray);
      Assert.IsTrue(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
    }

    [TestMethod]
    public void CreateStandardProjectWithNoCustomerUidAndGetProjectListV4()
    {
      msg.Title("Project v4 test 27", "Create standard project and customer then read the project list. No customer id and no project id");
      var ts = new TestSupport();
      ts.SetCustomerUid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name             | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 25 | 1                 | {ts.CustomerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName    | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | ",
      $"| CreateProjectRequest | 0d+09:00:00 | No Customer ID | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | " };
      var response = ts.PublishEventToWebApi(projectEventArray);
      Assert.IsTrue(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, ts.CustomerUid, projectEventArray, true);
    }

    [TestMethod]
    public void CreateLandfillGeofencesThenQueryForAvailable()
    {
      msg.Title("Project v4 test 28", "Create landfill sites, then query available for customer and associated for project");

      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt =
        "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[]
      {
        "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
       $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
       $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"
      };
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[]
      {
        "| EventType            | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem         |",
       $"| CreateProjectRequest | 0d+09:00:00 | {projectUid} | Boundary Test 24 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc    |"
      };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);

      var geofenceUidProject = mysql.VerifyProjectGeofence(projectUid, 1);
      var geofenceUidLandfillSite1 = Guid.NewGuid();
      var geofenceUidLandfillSite2 = Guid.NewGuid();
      var geofenceEventArray = new[]
      {
        "| TableName  | EventType           | EventDate   | fk_CustomerUID | Description   | FillColor | GeofenceName  | fk_GeofenceTypeID            | GeofenceUID                 | GeometryWKT   | IsTransparent | Name | UserUID        | IsDeleted | LastActionedUTC |",
        $"| Geofence   | CreateGeofenceEvent | 0d+09:00:00 | {customerUid}  | Project       | 1         | Project       | {(int) GeofenceType.Project}  | {geofenceUidProject}        | {geometryWkt} | {false}       | Blah | {customerUid}  | {false}   | 0d+09:00:00     |",
        $"| Geofence   | CreateGeofenceEvent | 0d+09:00:00 | {customerUid}  | LandfillSite1 | 1         | LandfillSite1 | {(int) GeofenceType.Landfill} | {geofenceUidLandfillSite1}  | {geometryWkt} | {false}       | Blah | {customerUid}  | {false}   | 0d+09:00:00     |",
        $"| Geofence   | CreateGeofenceEvent | 0d+09:00:00 | {customerUid}  | LandfillSite2 | 1         | LandfillSite2 | {(int) GeofenceType.Landfill} | {geofenceUidLandfillSite2}  | {geometryWkt} | {false}       | Blah | {customerUid}  | {false}   | 0d+09:00:00     |"
      };
      ts.IsPublishToWebApi = false;
      ts.PublishEventCollection(geofenceEventArray);

      var availableGeofences = ts.GetProjectGeofencesViaWebApiV4(customerUid.ToString(), "?geofenceType=Landfill", string.Empty);
      Assert.IsNotNull(availableGeofences);
      Assert.AreEqual(2, availableGeofences.GeofenceDescriptors.Count, "Incorrect number of available Geofences.");

      var associatedGeofences = ts.GetProjectGeofencesViaWebApiV4(customerUid.ToString(), "?geofenceType=Landfill", $"&projectUid={projectUid}");
      Assert.IsNotNull(associatedGeofences);
      Assert.AreEqual(0, associatedGeofences.GeofenceDescriptors.Count, "Incorrect number of associated Geofences.");
    }

    [TestMethod]
    public void CreateLandfillGeofenceThenAssociate()
    {
      msg.Title("Project v4 test 29", "Create landfill sites, then associate to project");

      var mysql = new MySqlHelper();
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt =
        "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[]
      {
        "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
       $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
       $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |"
      };
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[]
      {
        "| EventType            | EventDate   | ProjectUID   | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem         |",
       $"| CreateProjectRequest | 0d+09:00:00 | {projectUid} | Boundary Test 29 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc    |"
      };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);

      var geofenceUidProject = mysql.VerifyProjectGeofence(projectUid, 1);
      var geofenceUidLandfillSite1 = Guid.NewGuid();
      var geofenceUidLandfillSite2 = Guid.NewGuid();
      var geofenceEventArray = new[]
      {
        "| TableName  | EventType           | EventDate   | fk_CustomerUID | Description   | FillColor | GeofenceName  | fk_GeofenceTypeID            | GeofenceUID                 | GeometryWKT   | IsTransparent | Name | UserUID        | IsDeleted | LastActionedUTC |",
        $"| Geofence   | CreateGeofenceEvent | 0d+09:00:00 | {customerUid}  | Project       | 1         | Project       | {(int) GeofenceType.Project}  | {geofenceUidProject}        | {geometryWkt} | {false}       | Blah | {customerUid}  | {false}   | 0d+09:00:00     |",
        $"| Geofence   | CreateGeofenceEvent | 0d+09:00:00 | {customerUid}  | LandfillSite1 | 1         | LandfillSite1 | {(int) GeofenceType.Landfill} | {geofenceUidLandfillSite1}  | {geometryWkt} | {false}       | Blah | {customerUid}  | {false}   | 0d+09:00:00     |",
        $"| Geofence   | CreateGeofenceEvent | 0d+09:00:00 | {customerUid}  | LandfillSite2 | 1         | LandfillSite2 | {(int) GeofenceType.Landfill} | {geofenceUidLandfillSite2}  | {geometryWkt} | {false}       | Blah | {customerUid}  | {false}   | 0d+09:00:00     |"
      };
      ts.IsPublishToWebApi = false;
      ts.PublishEventCollection(geofenceEventArray);

      var associatedResult = ts.AssociateProjectGeofencesViaWebApiV4(customerUid.ToString(), projectUid, new List<GeofenceType>(){GeofenceType.Landfill}, new List<Guid>(){ geofenceUidLandfillSite1 });
      Assert.IsNotNull(associatedResult);
      Assert.AreEqual("success", associatedResult.Message);

      var availableGeofences = ts.GetProjectGeofencesViaWebApiV4(customerUid.ToString(), "?geofenceType=Landfill", string.Empty);
      Assert.IsNotNull(availableGeofences);
      Assert.AreEqual(1, availableGeofences.GeofenceDescriptors.Count, "Incorrect number of available Geofences.");
      Assert.AreEqual(geofenceUidLandfillSite2.ToString(), availableGeofences.GeofenceDescriptors[0].GeofenceUid, "Incorrect available GeofenceUid.");

      var associatedGeofences = ts.GetProjectGeofencesViaWebApiV4(customerUid.ToString(), "?geofenceType=Landfill", $"&projectUid={projectUid}");
      Assert.IsNotNull(associatedGeofences);
      Assert.AreEqual(1, associatedGeofences.GeofenceDescriptors.Count, "Incorrect number of associated Geofences.");
      Assert.AreEqual(geofenceUidLandfillSite1.ToString(), associatedGeofences.GeofenceDescriptors[0].GeofenceUid, "Incorrect associated GeofenceUid.");
    }
  }
}
