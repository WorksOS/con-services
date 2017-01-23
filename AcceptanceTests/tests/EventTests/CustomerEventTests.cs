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
            msg.Title("Create Customer test 1", "Create one customer");
            var eventArray = new[] {
            " | EventType           | DayOffset | Timestamp | CustomerName | CustomerType | CustomerUID   |",
            $"| CreateCustomerEvent | 0         | 09:00:00  | CustName     | CustType     | {customerUid} |"
            };

            testSupport.InjectEventsIntoKafka(eventArray);                                           // Inject events into kafka             
           // mysql.VerifyTestResultDatabaseRecordCount("Asset", 1, testSupport.AssetUid);                                         // Verify the the result in the database  
           // mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "Name,SerialNumber,MakeCode,Model,IconKey,AssetType", "YT669,8JG89719,CAT,345DL,10,Unassigned", testSupport.AssetUid);
        }

        //[TestMethod]
        // public void Inject_Two_Create_Asset_Events_With_SameUid()
        // {
        //     var msg = new Msg();
        //     var testSupport = new TestSupport();
        //     var mysql = new MySqlHelper();
        //     msg.Title("Asset test 2","Try to create two asset events. Only one event is put in the database. ");
        //     var eventArray = new[] {
        //     "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId |",
        //     "| CreateAssetEvent | 0         | 09:00:00  | ASSET1    | 8JG89719     | CAT  | 345DL | 10     |",
        //     "| CreateAssetEvent | 0         | 09:00:00  | CU672     | 8JG89719     | DEE  | 990H  | 27     |"   // 2nd create works as update 
        //     };

        //     testSupport.InjectEventsIntoKafka(eventArray);                                           // Inject events into kafka             
        //     mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                                         // Verify the the result in the database  
        //     mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "Name,SerialNumber,MakeCode,Model,IconKey", "CU672,8JG89719,DEE,990H,27",testSupport.AssetUid);  
        // }

        // [TestMethod]
        // public void Inject_A_Create_Asset_Event_Then_Update_Asset_Event()
        // {
        //     var msg = new Msg();
        //     var testSupport = new TestSupport();
        //     var mysql = new MySqlHelper();
        //     msg.Title("Asset test 3","Create an asset events then update the asset. ");
        //     var eventArray = new[] {
        //     "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId |",
        //     "| CreateAssetEvent | 0         | 09:00:00  | ASSET3    | ZZZZZZZZ     | ZZZ  | ZZZ   | 999    |",
        //     "| UpdateAssetEvent | 0         | 10:00:00  | CU672     | 8JG89719     | ZZZ  | 990H  | 27     |"  // Can't change make or serial number
        //     };

        //     testSupport.InjectEventsIntoKafka(eventArray);                                         // Inject events into kafka             
        //     mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                                       // Verify the the result in the database  
        //     mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "Name,SerialNumber,MakeCode,Model,IconKey", "CU672,ZZZZZZZZ,ZZZ,990H,27",testSupport.AssetUid);  
        // }
        // [TestMethod]
        // public void Inject_A_Update_Asset_Event_Then_Create_Asset_Event()
        // {
        //     var msg = new Msg();
        //     var testSupport = new TestSupport();
        //     var mysql = new MySqlHelper();

        //     msg.Title("Asset test 4","Update an asset events then send a create event for the same asset. The create gets treated as a update");
        //     var eventArray = new[] {
        //     "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
        //     "| UpdateAssetEvent | 0         | 09:00:00  | ASSET4    | ZZZZZZZZ     | ZZZ  | ZZZ   | 999    | TRACTOR   |", 
        //     "| CreateAssetEvent | 0         | 10:00:00  | CU672     | 8JG89719     | CAT  | 990H  | 27     | SCRAPER   |"   
        //     };

        //     testSupport.InjectEventsIntoKafka(eventArray);                                         // Inject events into kafka             
        //     mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                                       // Verify the the result in the database  
        //     mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "Name,SerialNumber,MakeCode,Model,IconKey,AssetType", "CU672,8JG89719,CAT,990H,27,SCRAPER",testSupport.AssetUid);  
        // }

        // [TestMethod]
        // public void Inject_A_Create_Asset_Event_Then_Delete_Asset_Event()
        // {
        //     var msg = new Msg();
        //     var testSupport = new TestSupport();
        //     var mysql = new MySqlHelper();

        //     msg.Title("Asset test 5","Create an asset events then delete it.");
        //     var eventArray = new[] {
        //     "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
        //     "| CreateAssetEvent | 0         | 09:00:00  | ASSET5    | SN663332     | CAT  | 627   | 20     | SCRAPER   |",
        //     "| DeleteAssetEvent | 1         | 10:00:00  | ASSET5    | SN663332     | CAT  | 627   | 20     | SCRAPER   |"  
        //     };

        //     testSupport.InjectEventsIntoKafka(eventArray);                                          // Inject events into kafka             
        //     mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                                        // Verify the the result in the database          
        //     mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "Name,IsDeleted", "ASSET5,True",testSupport.AssetUid);   // Verify records in the database are correct
        // }

        // [TestMethod]
        // public void Inject_A_Delete_Asset_Event_When_No_Create_Asset_Event()
        // {
        //     var msg = new Msg();
        //     var testSupport = new TestSupport();
        //     var mysql = new MySqlHelper();

        //     msg.Title("Asset test 6","Delete an asset events that doesnt exist");
        //     var eventArray = new[] {
        //     "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
        //     "| DeleteAssetEvent | 1         | 10:00:00  | ASSET6    | SN663332     | CAT  | 627   | 20     | SCRAPER   |"  
        //     };

        //     testSupport.InjectEventsIntoKafka(eventArray);                                         // Inject events into kafka 
        //     mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                                       // Verify the the result in the database
        //     mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "Name,IsDeleted", ",True",testSupport.AssetUid);        // Verify records in the database are correct
        // }
    }
}
