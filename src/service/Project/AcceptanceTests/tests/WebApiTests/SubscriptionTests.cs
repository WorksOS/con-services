using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.Models;

namespace WebApiTests
{
  [TestClass]
  public class SubscriptionTests
  {
    private readonly Msg _msg = new Msg();

    [TestMethod]
    public void Get2SubscriptionsForProjectMonitoring()
    {
      _msg.Title("Project Subtest 1", "Get 2 project monitoring subscriptions ");
      var ts = new TestSupport();
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
      ts.PublishEventCollection(eventsArray);
      var response = ts.CallProjectWebApiV4("api/v4/subscriptions", "GET", null, customerUid.ToString());
      var objresp = JsonConvert.DeserializeObject<SubscriptionsListResult>(response);
      Assert.AreEqual(objresp.SubscriptionDescriptors.Count, 2, " Expecting 2 subscriptions in the results");
      Assert.AreEqual(objresp.Message, "success", "The message in the response should be success");
    }

    [TestMethod]
    public void Get4SubscriptionsForProjectMonitoringAndLandFill()
    {
      _msg.Title("Project Subtest 2", "Get 2 project monitoring and 2 landfill subscriptions ");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid1 = Guid.NewGuid();
      var subscriptionUid2 = Guid.NewGuid();
      var subscriptionUid3 = Guid.NewGuid();
      var subscriptionUid4 = Guid.NewGuid();
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
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid3} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
        $"| Subscription        | 0d+09:50:00 |               |           |                   | {subscriptionUid4} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",

      };
      ts.PublishEventCollection(eventsArray);
      var response = ts.CallProjectWebApiV4("api/v4/subscriptions", "GET", null, customerUid.ToString());
      var objresp = JsonConvert.DeserializeObject<SubscriptionsListResult>(response);
      Assert.AreEqual(objresp.SubscriptionDescriptors.Count, 4, " Expecting 4 subscriptions in the results");
      Assert.AreEqual(objresp.Message, "success", "The message in the response should be success");
      foreach (var sub in objresp.SubscriptionDescriptors)
      {
        if (sub.ServiceTypeName == "Landfill" || sub.ServiceTypeName == "ProjectMonitoring") { }
        else
        { Assert.Fail("The subscription should be Landfill or Project Monitoring"); }
      }
    }

    [TestMethod]
    public void Create4SubscriptionsAndUse2ForProjects()
    {
      _msg.Title("Project Subtest 3", "Get 4 subscriptions and use two ");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid1 = Guid.NewGuid();
      var subscriptionUid2 = Guid.NewGuid();
      var subscriptionUid3 = Guid.NewGuid();
      var subscriptionUid4 = Guid.NewGuid();
      var legacyProjectId1 = ts.SetLegacyProjectId();
      var legacyProjectId2 = legacyProjectId1 + 1;
      var projectUid1 = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt1 = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      const string geometryWkt2 = "POLYGON((-115.025723657623 36.2101347890754,-115.026281557098 36.2056332151707,-115.018041811005 36.205460072542,-115.025723657623 36.2101347890754))";
      var eventsArray = new[]
      {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID    | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                    |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                    |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid1} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
        $"| Subscription        | 0d+09:50:00 |               |           |                   | {subscriptionUid2} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid3} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
        $"| Subscription        | 0d+09:50:00 |               |           |                   | {subscriptionUid4} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
        $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                    |                |                  | {startDate} |                | {projectUid1} |          | {subscriptionUid1}  |",
        $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                    |                |                  | {startDate} |                | {projectUid2} |          | {subscriptionUid2}  |"
      };
      ts.PublishEventCollection(eventsArray);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName     | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | CoordinateSystem      | ",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid1} | {legacyProjectId1} | Project Sub 4-1 | ProjectMonitoring | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} | false      | BootCampDimensions.dc |" ,
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid2} | {legacyProjectId2} | Project Sub 4-2 | ProjectMonitoring | Mountain Standard Time    | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId1} | false      | BootCampDimensions.dc |" };
      ts.PublishEventCollection(projectEventArray);
      //Thread.Sleep(3000);
      var response = ts.CallProjectWebApiV4("api/v4/subscriptions", "GET", null, customerUid.ToString());
      var objresp = JsonConvert.DeserializeObject<SubscriptionsListResult>(response);
      Assert.AreEqual(objresp.SubscriptionDescriptors.Count, 2, " Expecting 2 subscriptions in the results");
      Assert.AreEqual(objresp.Message, "success", "The message in the response should be success");
      foreach (var sub in objresp.SubscriptionDescriptors)
      {
        if (sub.ServiceTypeName == "Landfill") { }
        else
        { Assert.Fail("The subscription should be Landfill or Project Monitoring"); }
      }
    }

    [TestMethod]
    public void Create4SubscriptionsAndUse2ForLandfillProjects()
    {
      _msg.Title("Project Subtest 4", "Get 4 subscriptions and use two ");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid1 = Guid.NewGuid();
      var subscriptionUid2 = Guid.NewGuid();
      var subscriptionUid3 = Guid.NewGuid();
      var subscriptionUid4 = Guid.NewGuid();
      var legacyProjectId1 = ts.SetLegacyProjectId();
      var legacyProjectId2 = legacyProjectId1 + 1;
      var projectUid1 = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt1 = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      const string geometryWkt2 = "POLYGON((-115.025723657623 36.2101347890754,-115.026281557098 36.2056332151707,-115.018041811005 36.205460072542,-115.025723657623 36.2101347890754))";
      var eventsArray = new[]
      {
         "| TableName           | EventDate   | CustomerUID   | Name      | fk_CustomerTypeID | SubscriptionUID    | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | CustName  | 1                 |                    |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |           |                   |                    |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid1} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
        $"| Subscription        | 0d+09:50:00 |               |           |                   | {subscriptionUid2} | {customerUid}  | 20               | {startDate} | {endDate}      |               |          |                    |",
        $"| Subscription        | 0d+09:10:00 |               |           |                   | {subscriptionUid3} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
        $"| Subscription        | 0d+09:50:00 |               |           |                   | {subscriptionUid4} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
        $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                    |                |                  | {startDate} |                | {projectUid1} |          | {subscriptionUid3}  |",
        $"| ProjectSubscription | 0d+09:20:00 |               |           |                   |                    |                |                  | {startDate} |                | {projectUid2} |          | {subscriptionUid4}  |"
      };
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID    | ProjectID          | ProjectName     | ProjectType       | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary  | CustomerUID   | CustomerID         | IsArchived | CoordinateSystem      |",
      $"| CreateProjectEvent | 0d+10:00:00 | {projectUid1} | {legacyProjectId1} | Project Sub 4-1 | LandFill          | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt1}   | {customerUid} | {legacyProjectId1} | false      | BootCampDimensions.dc |" ,
      $"| CreateProjectEvent | 0d+10:00:00 | {projectUid2} | {legacyProjectId2} | Project Sub 4-2 | LandFill          | Mountain Standard Time    | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt2}   | {customerUid} | {legacyProjectId1} | false      | BootCampDimensions.dc |" };
      ts.PublishEventCollection(projectEventArray);
      //Thread.Sleep(5000);
      var response = ts.CallProjectWebApiV4("api/v4/subscriptions", "GET", null, customerUid.ToString());
      var objresp = JsonConvert.DeserializeObject<SubscriptionsListResult>(response);
      Assert.AreEqual(2, objresp.SubscriptionDescriptors.Count, " Expected subscriptions mismatch");
      Assert.AreEqual("success", objresp.Message, "The message in the response should be success");

      foreach (var sub in objresp.SubscriptionDescriptors)
      {
        if (sub.ServiceTypeName != "ProjectMonitoring")
        {
          Assert.Fail("The subscription should be Project Monitoring");
        }
      }
    }
  }
}