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

            testSupport.InjectEventsIntoKafka(eventArray);                                                   
            mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);                                       
            mysql.VerifyTestResultDatabaseFieldsAreExpected("Subscription","SubscriptionUID", "fk_ServiceTypeID", "20", subscriptionUid);
        }

        [TestMethod] 
        public void UpdateProjectSubscriptionEvent()
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            var subscriptionUid = Guid.NewGuid();
            msg.Title("Subscription test 2", "Update Project Subscription");
            // 'Project Monitoring'   'Landfill'  'Manual 3D Project Monitoring'
            var eventArray = new[] {
             "| EventType                      | EventDate   | StartDate  | EndDate    | SubscriptionType   | SubscriptionUID   |",
            $"| CreateProjectSubscriptionEvent | 0d+12:00:00 | 2012-01-01 | 9999-12-31 | Project Monitoring | {subscriptionUid} |",
            $"| UpdateProjectSubscriptionEvent | 1d+12:00:00 | 2014-12-12 | 9999-12-31 | Landfill           | {subscriptionUid} |"};

            testSupport.InjectEventsIntoKafka(eventArray);                                                   
            mysql.VerifyTestResultDatabaseRecordCount("Subscription", "SubscriptionUID", 1, subscriptionUid);                                       
            mysql.VerifyTestResultDatabaseFieldsAreExpected("Subscription","SubscriptionUID", "fk_ServiceTypeID", "19", subscriptionUid);
        }
    }
}
