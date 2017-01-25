using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace EventTests
{
    [TestClass]
    public class SubscriptionEventTests
    {
        [TestMethod] [Ignore]
        public void CreateProjectSubscriptionEvent()
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            var subscriptionUid = Guid.NewGuid();
            msg.Title("Customer test 1", "Create one customer");
            var eventArray = new[] {
             "| EventType                      | EventDate   | StartDate  | EndDate  | SubscriptionType | SubscriptionUID   |",
            $"| CreateProjectSubscriptionEvent | 0d+12:00:00 | 2012-01-01 | 99991231 |                  | {subscriptionUid} |"};

            testSupport.InjectEventsIntoKafka(eventArray);                                                   
            mysql.VerifyTestResultDatabaseRecordCount("Customer", "CustomerUID", 1, subscriptionUid);                                       
            mysql.VerifyTestResultDatabaseFieldsAreExpected("Customer","CustomerUID", "Name,fk_CustomerTypeID,IsDeleted", "CustName,1,0", subscriptionUid);
        }

        [TestMethod] [Ignore]
        public void UpdateCustomerEvent()
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            var customerUid = Guid.NewGuid();
            msg.Title("Customer test 2", "Update one customer");
            var eventArray = new[] {
             "| EventType           | EventDate   | CustomerName | CustomerType | CustomerUID   |",
            $"| CreateCustomerEvent | 0d+09:00:00 | CustName     | Customer     | {customerUid} |",
            $"| UpdateCustomerEvent | 0d+10:00:00 | UpdatedName  | Customer     | {customerUid} |"};

            testSupport.InjectEventsIntoKafka(eventArray);                                                   
            mysql.VerifyTestResultDatabaseRecordCount("Customer", "CustomerUID", 1, customerUid);                                       
            mysql.VerifyTestResultDatabaseFieldsAreExpected("Customer","CustomerUID", "Name,fk_CustomerTypeID,IsDeleted", "UpdatedName,1,0", customerUid);
        }
    }
}
