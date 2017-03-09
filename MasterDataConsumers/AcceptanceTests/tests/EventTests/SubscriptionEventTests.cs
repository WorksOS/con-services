using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace EventTests
{
  [TestClass]
  public class SubscriptionEventTests
  {
    [TestMethod]
    public void CreateProjectSubscriptionEvent()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var subscriptionUid = Guid.NewGuid();
      msg.Title("Subscription test 1", "Create Project Subscription");
      // 'Project Monitoring'   'Landfill'  'Manual 3D Project Monitoring'
      var eventArray = new[] {
             "| EventType                      | EventDate   | StartDate  | EndDate    | SubscriptionType   | SubscriptionUID   |",
            $"| CreateProjectSubscriptionEvent | 0d+12:00:00 | 2012-01-01 | 9999-12-31 | Project Monitoring | {subscriptionUid} |"};

      testSupport.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Subscription", "SubscriptionUID", "fk_ServiceTypeID", "20", subscriptionUid);
    }

    [TestMethod]
    public void UpdateProjectSubscriptionEvent_Dates()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var subscriptionUid = Guid.NewGuid();
      msg.Title("Subscription test 2", "Update Project Subscription dates");
      DateTime startDate = testSupport.FirstEventDate;
      DateTime endDate = new DateTime(9999, 12, 31);
      // 'Project Monitoring'   'Landfill'  'Manual 3D Project Monitoring'
      var eventArray = new[] {
             "| EventType                      | EventDate   | StartDate               | EndDate                | SubscriptionType   | SubscriptionUID   |",
            $"| CreateProjectSubscriptionEvent | 0d+12:00:00 | {startDate}             | {endDate}              | Project Monitoring | {subscriptionUid} |",
            $"| UpdateProjectSubscriptionEvent | 1d+12:00:00 | {startDate.AddYears(2)} | {endDate.AddYears(-2)} | Project Monitoring | {subscriptionUid} |"};

      testSupport.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Subscription", "SubscriptionUID", "StartDate, EndDate", $"{startDate.AddYears(2)},{endDate.AddYears(-2)}", subscriptionUid);
    }

    /// <summary>
    /// Updating Subscription types is not allowed, test to ensure that they don't update.
    /// </summary>
    [TestMethod]
    public void UpdateProjectSubscriptionEvent_SubscriptionType()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var subscriptionUid = Guid.NewGuid();
      msg.Title("Subscription test 3", "Ensure Project Subscription cannot be updated");
      // 'Project Monitoring'   'Landfill'  'Manual 3D Project Monitoring'
      var eventArray = new[] {
             "| EventType                      | EventDate   | StartDate  | EndDate    | SubscriptionType   | SubscriptionUID   |",
            $"| CreateProjectSubscriptionEvent | 0d+12:00:00 | 2012-01-01 | 9999-12-31 | Project Monitoring | {subscriptionUid} |",
            $"| CreateProjectSubscriptionEvent | 0d+12:00:00 | 2012-01-01 | 9999-12-31 | Landfill           | {subscriptionUid} |"};

      testSupport.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Subscription", "SubscriptionUID", "fk_ServiceTypeID", "20", subscriptionUid);
    }
  }
}
