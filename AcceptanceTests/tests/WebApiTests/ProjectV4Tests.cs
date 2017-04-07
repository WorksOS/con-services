using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

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
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName     | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | Boundary Test 1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, false);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, false);
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
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"
      };

      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName     | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | Boundary Test 2 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, false);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, false);
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
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName     | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | Boundary Test 3 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, false);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, false);
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
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName     | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} |                   | Boundary Test 4 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
    }

    [TestMethod] [Ignore]
    public void CreateProjectWithoutLegacyCustomerId()
    {
      msg.Title("Project v4 test 5", "Create standard project with out legacy customer id");
      var ts = new TestSupport(); 
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 5 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName     | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | Boundary Test 5 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | false      |"};
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, true);
    }

    [TestMethod]
    public void Create2SubscriptionsFoLandfillAndCreateProjects()
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
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid2} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                     |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                    |                |                  | {startDate} |                | {projectUid1} |          | {subscriptionUid1}  |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                    |                |                  | {startDate} |                | {projectUid2} |          | {subscriptionUid2}  |"};

      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | {legacyProjectId1} | Boundary Test 6-1 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} | false      |" ,
      $"| CreateProjectEvent | 1d+09:00:00 | {projectUid2} | {legacyProjectId2} | Boundary Test 6-2 | LandFill    | Western European Time     | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId1} | false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, false);
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
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName       | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | {legacyProjectId1} | Boundary Test 7-1 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} | false      |" ,
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | {legacyProjectId2} | Boundary Test 7-2 | ProjectMonitoring | Mountain Standard Time    | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId1} | false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, false);
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
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName     | ProjectType  | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | {legacyProjectId1} | Boundary Test 8 | LandFill     | Mountain Standard Time    | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} | false      |" 
      };
      var response = ts.PublishEventToWebApi(projectEventArray);

      Assert.IsTrue(response == "No available subscriptions for the selected customer", "Should not be any subscriptions so project not created. Response: " + response);
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
      const string geometryWkt1 = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))";
      const string geometryWkt2 = "POLYGON((172.595838000000 -43.5427080000000,172.594636000000 -43.5438900000000,172.596568 -43.543800,172.595838000000 -43.5427080000000))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 1 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray1 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | {legacyProjectId1} | Boundary Test 9-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} |false      |" };
      var response1 = ts.PublishEventToWebApi(projectEventArray1);
      Assert.IsTrue(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | {legacyProjectId2} | Boundary Test 9-2 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId2} |false      |" };
      var response2 = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response2 == "success", "Response is unexpected. Should be a success. Response: " + response2);

      var projectEventArray3 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | {legacyProjectId1} | Boundary Test 9-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} |false      |" ,
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | {legacyProjectId2} | Boundary Test 9-2 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId2} |false      |" };

      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray3, false);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid2, projectEventArray2, false);
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
      const string geometryWkt1 = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))";
      const string geometryWkt2 = "POLYGON((172.595071 -43.542112,172.595562 -43.543218,172.597660 -43.542353,172.595071 -43.542112))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 1 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray1 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | {legacyProjectId1} | Boundary Test 9-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} |false      |" };
      var response1 = ts.PublishEventToWebApi(projectEventArray1);
      Assert.IsTrue(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName       | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | {legacyProjectId2} | Boundary Test 9-2 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId2} |false      |" };
      var response2 = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response2 == "Project boundary overlaps another project, for this customer and time span", "Response is unexpected. Should be a success. Response: " + response2);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray1, false);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid1, projectEventArray1, false);
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
      const string geometryWkt1 = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))";
      const string geometryWkt2 = "POLYGON((172.595071 -43.542112,172.595562 -43.543218,172.597660 -43.542353,172.595071 -43.542112))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name             | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 11 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray1 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName        | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | {legacyProjectId1} | Boundary Test 11-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} |false      |" };
      var response1 = ts.PublishEventToWebApi(projectEventArray1);
      Assert.IsTrue(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName        | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | {legacyProjectId2} | Boundary Test 11-2 | Standard    | New Zealand Standard Time |{startDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff} | {geometryWkt2}   | {customerUid} | {legacyProjectId2} |false      |" };
      var response2 = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response2 == "success", "Response is unexpected. Should be a success. Response: " + response2);

      var projectEventArray3 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName        | ProjectType | ProjectTimezone           | ProjectStartDate                             | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | {legacyProjectId1} | Boundary Test 11-1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}   | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} |false      |" ,
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | {legacyProjectId2} | Boundary Test 11-2 | Standard    | New Zealand Standard Time |{startDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff} | {geometryWkt2}   | {customerUid} | {legacyProjectId2} |false      |" };

      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray3, false);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid2, projectEventArray2, false);
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
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | Boundary Test 12 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      var response1 = ts.PublishEventToWebApi(projectEventArray);
      Assert.IsTrue(response1 == "No available subscriptions for the selected customer", "Response is unexpected. Should be No available subscriptions for the selected customer. Response: " + response1);
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
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName      | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | Boundary Test 13 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      var response1 = ts.PublishEventToWebApi(projectEventArray);
      Assert.IsTrue(response1 == "No available subscriptions for the selected customer", "Response is unexpected. Should be No available subscriptions for the selected customer. Response: " + response1);
    }

   [TestMethod]
    public void Create2LandfillProjectsWithOverLappingBoundarys()
    {
      msg.Title("Project v4 test 14", "Create standard project and customer with overlapping boundarys");
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
      const string geometryWkt1 = "POLYGON((172.595831670724 -43.5427038560109,172.594630041089 -43.5438859356773,172.59329966542 -43.542486101965, 172.595831670724 -43.5427038560109))";
      const string geometryWkt2 = "POLYGON((172.595309 -43.542883,172.594735 -43.543357,172.594037 -43.542756,172.595309 -43.542883))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID    | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID  |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                    |                |                  |             |                |               |          |                     |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                    |                |                  |             |                |               | {tccOrg} |                     |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid1} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                     |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid2} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                     |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                    |                |                  | {startDate} |                | {projectUid1} |          | {subscriptionUid1}  |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                    |                |                  | {startDate} |                | {projectUid2} |          | {subscriptionUid2}  |"};

      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray1 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | {legacyProjectId1} | Boundary Test 14 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} | false      |"};
      var response1 = ts.PublishEventToWebApi(projectEventArray1);
      Assert.IsTrue(response1 == "success", "Response is unexpected. Should be a success. Response: " + response1);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived |",
      $"| CreateProjectEvent | 1d+09:00:00 | {projectUid2} | {legacyProjectId2} | Boundary Test 14 | LandFill    | Western European Time     | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId1} | false      |" };
      var response2 = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response2 == "Project boundary overlaps another project, for this customer and time span", "Response is unexpected. Should be a Project boundary overlaps another project, for this customer and time span. Response: " + response2);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray1, false);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid1, projectEventArray1, false);
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
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | Boundary Test 15 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, false);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, false);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| UpdateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | Boundary Test 15 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray2);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, false);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, false);
    }

    [TestMethod]
    public void CreateLandfillProjectThenTryUpdateTimeZone()
    {
      msg.Title("Project v4 test 16", "Create landfill project and customer then try and update time zone");
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
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | Boundary Test 16 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, false);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, false);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName      | ProjectType | ProjectTimezone       | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| UpdateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | Boundary Test 16 | LandFill    | Western European Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      var response = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response == "Project timezone cannot be updated", "Response is unexpected. Should be a Project timezone cannot be updated. Response: " + response);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, false);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, false);
    }

    [TestMethod]
    public void CreateLandfillProjectThenUpdateProjectName()
    {
      msg.Title("Project v4 test 17", "Create landfill project and customer then update project name");
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
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName      | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | Boundary Test 17 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray, false);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray, false);

      var projectEventArray2 = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName     | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                              | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | ",
      $"| UpdateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | new name        | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime2:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      |" };
      var response = ts.PublishEventToWebApi(projectEventArray2);
      Assert.IsTrue(response == "success", "Response is unexpected. Should be a success. Response: " + response);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray2, false);
      ts.GetProjectDetailsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectUid, projectEventArray2, false);
    }
  }
}
