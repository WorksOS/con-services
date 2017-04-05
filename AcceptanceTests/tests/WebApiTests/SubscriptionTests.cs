using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProjectWebApi.Models;
using TestUtility;

namespace WebApiTests
{
  [TestClass]
  public class SubscriptionTests
  {
    private readonly Msg msg = new Msg();

    [TestMethod]
    public void Create2SubscriptionsForProjectMonitoringAndCreateProjects()
    {
      msg.Title("Project Subtest 1", "Create 2 subscriptions ");
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
  }
}
