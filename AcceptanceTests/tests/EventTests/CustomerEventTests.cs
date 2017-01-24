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

        [TestMethod]
        public void AssociateCustomerToUserTest()
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            var customerUid = Guid.NewGuid();
            var userUid = Guid.NewGuid();
            msg.Title("Customer test 4", "Associate Customer To User");
            var eventArray = new[] {
             "| EventType                  | DayOffset | Timestamp | CustomerName   | CustomerType | CustomerUID   | UserUID   |",
            $"| CreateCustomerEvent        | 0         | 09:00:00  | AssociateCust  | Customer     | {customerUid} |           |",
            $"| AssociateCustomerUserEvent | 1         | 09:00:00  |                |              | {customerUid} | {userUid} |"};

            testSupport.InjectEventsIntoKafka(eventArray);                                                   
            mysql.VerifyTestResultDatabaseRecordCount("Customer", "CustomerUID", 1, customerUid);     
            mysql.VerifyTestResultDatabaseRecordCount("CustomerUser", "UserUID", 1, userUid);                                                 
            mysql.VerifyTestResultDatabaseFieldsAreExpected("Customer","CustomerUID", "Name,fk_CustomerTypeID,IsDeleted", "AssociateCust,1,0", customerUid);
            mysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerUser","UserUID", "fk_CustomerUID,UserUID",$"{customerUid}, {userUid}", userUid);
        }

        [TestMethod]
        public void DisassociateCustomerToUserTest()
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            var customerUid = Guid.NewGuid();
            var userUid = Guid.NewGuid();
            msg.Title("Customer test 5", "Disassociate Customer To User");
            var eventArray = new[] {
             "| EventType                   | DayOffset | Timestamp | CustomerName   | CustomerType | CustomerUID   | UserUID   |",
            $"| CreateCustomerEvent         | 0         | 09:00:00  | AssociateCust  | Customer     | {customerUid} |           |",
            $"| AssociateCustomerUserEvent  | 1         | 09:00:00  |                |              | {customerUid} | {userUid} |",
            $"| DissociateCustomerUserEvent | 2         | 09:00:00  |                |              | {customerUid} | {userUid} |",
            };

            testSupport.InjectEventsIntoKafka(eventArray);                                                   
            mysql.VerifyTestResultDatabaseRecordCount("Customer", "CustomerUID", 1, customerUid);     
            mysql.VerifyTestResultDatabaseRecordCount("CustomerUser", "UserUID", 0, userUid);                                                 
            mysql.VerifyTestResultDatabaseFieldsAreExpected("Customer","CustomerUID", "Name,fk_CustomerTypeID,IsDeleted", "AssociateCust,1,0", customerUid);
        }
    }
}
