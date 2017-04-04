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
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var customerEventArray = new[] {
       "| TableName | EventDate   | Name            | fk_CustomerTypeID | CustomerUID   |",
      $"| Customer  | 0d+09:00:00 | Boundary Test 1 | 1                 | {customerUid} |"};
      ts.PublishEventCollection(customerEventArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName     | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | LegacyCustomerId  |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | Boundary Test 1 | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWKT}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray);
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
      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
        //  $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"
      };
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName     | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | LegacyCustomerId  |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | Boundary Test 1 | LandFill    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWKT}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray);
    }

    [TestMethod]
    public void CreateProjectMonitoringProjectAndSubscriptionThenGetProjectListV4()
    {
      msg.Title("Project v4 test 2", "Create project monitoring project of and customer then read the project list");
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

      var geometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
        $"| CustomerProject     | 0d+09:20:00 |               |           |                   |                   | {customerUid}  |                  |             |                | {projectUid}  |          |                    |",
        $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName     | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | LegacyCustomerId  |IsArchived | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | Boundary Test 1 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWKT}   | {customerUid} | {legacyProjectId} |false      |" };
      ts.PublishEventCollection(projectEventArray);
      ts.GetProjectsViaWebApiV4AndCompareActualWithExpected(HttpStatusCode.OK, customerUid, projectEventArray);
    }

  }
}
