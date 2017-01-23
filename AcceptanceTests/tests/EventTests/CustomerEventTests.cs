using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace EventTests
{
    [TestClass]
    public class CustomerEventTests
    {
        [TestMethod]
        public void CreateCustomerEvent()
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            var customerUid = Guid.NewGuid();
            msg.Title("Customer test 1", "Create one customer");
            var eventArray = new[] {
             "| EventType           | DayOffset | Timestamp | CustomerName | CustomerType | CustomerUID   |",
            $"| CreateCustomerEvent | 0         | 09:00:00  | CustName     | Customer     | {customerUid} |"};

            testSupport.InjectEventsIntoKafka(eventArray);                                                   
            mysql.VerifyTestResultDatabaseRecordCount("Customer", "CustomerUID", 1, customerUid);                                       
            mysql.VerifyTestResultDatabaseFieldsAreExpected("Customer","CustomerUID", "Name,fk_CustomerTypeID,IsDeleted", "CustName,1,0", customerUid);
        }

        [TestMethod]
        public void UpdateCustomerEvent()
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            var customerUid = Guid.NewGuid();
            msg.Title("Customer test 2", "Update one customer");
            var eventArray = new[] {
             "| EventType           | DayOffset | Timestamp | CustomerName | CustomerType | CustomerUID   |",
            $"| CreateCustomerEvent | 0         | 09:00:00  | CustName     | Customer     | {customerUid} |",
            $"| UpdateCustomerEvent | 0         | 09:00:00  | UpdatedName  | Customer     | {customerUid} |"};

            testSupport.InjectEventsIntoKafka(eventArray);                                                   
            mysql.VerifyTestResultDatabaseRecordCount("Customer", "CustomerUID", 1, customerUid);                                       
            mysql.VerifyTestResultDatabaseFieldsAreExpected("Customer","CustomerUID", "Name,fk_CustomerTypeID,IsDeleted", "UpdatedName,1,0", customerUid);
        }

        [TestMethod]
        public void DeleteCustomerEvent()
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            var customerUid = Guid.NewGuid();
            msg.Title("Customer test 3", "Delete a customer");
            var eventArray = new[] {
             "| EventType           | DayOffset | Timestamp | CustomerName   | CustomerType | CustomerUID   |",
            $"| CreateCustomerEvent | 0         | 09:00:00  | DeleteCustName | Customer     | {customerUid} |",
            $"| DeleteCustomerEvent | 0         | 09:00:00  | DeleteCustName | Customer     | {customerUid} |"};

            testSupport.InjectEventsIntoKafka(eventArray);                                                   
            mysql.VerifyTestResultDatabaseRecordCount("Customer", "CustomerUID", 1, customerUid);                                       
            mysql.VerifyTestResultDatabaseFieldsAreExpected("Customer","CustomerUID", "Name,fk_CustomerTypeID,IsDeleted", "DeleteCustName,1,1", customerUid);
        }
    }
}
