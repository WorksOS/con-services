using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using TestUtility.Model.WebApi;

namespace WebApiTests
{
    [TestClass]
    public class WidgetTestsClass
    {
        [TestMethod]
        public void Cycle_count_widget_test()          
        {
            const int TARGETCYCLECOUNT = 999;
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Widget 1","Cycle count widget end point test ");

            // Start from middle of month
            var middleOfMonth = new DateTime(testSupport.FirstEventDate.Year, testSupport.FirstEventDate.Month, 12);
             
            int deltaDays = DayOfWeek.Monday - middleOfMonth.DayOfWeek; 
            var startWeek = middleOfMonth.AddDays(deltaDays);
            testSupport.FirstEventDate = startWeek;      
            var startMonth = new DateTime(testSupport.FirstEventDate.Year, testSupport.FirstEventDate.Month, 1);
            var noDaysInMonth = (testSupport.FirstEventDate - startMonth).TotalDays;

            var assetType = Guid.NewGuid().ToString();  
            var eventArray = new[] {                                                                // Load events into array  
            " | EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType       |",
            $"| CreateAssetEvent | 0         | 03:00:00  | WIDGET1   | 12345678     | CAT  | 637G  | 20     | {assetType}     |",
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);                                  // Inject event array into kafka 
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false, testSupport.FirstEventDate.AddDays(-30), 150, TARGETCYCLECOUNT);                         
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);             // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:10:00  |              |             | 10000.15           | ",
            "| SwitchStateEvent | 0         | 12:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 12:30:00  |              |             | 10010.15           | ",
            "| SwitchStateEvent | 0         | 16:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 16:00:00  |              |             | 10020.15           | ",
            "| SwitchStateEvent | 0         | 23:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 23:20:00  |              |             | 10030.15           | ",
            "| SwitchStateEvent | 1         | 04:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 1         | 04:30:00  |              |             | 10040.15           | ",
            "| SwitchStateEvent | 1         | 12:01:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 1         | 12:01:00  |              |             | 10050.15           | ",
            "| SwitchStateEvent | 2         | 01:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 2         | 01:30:00  |              |             | 10060.15           | ",
            "| SwitchStateEvent | 2         | 12:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 2         | 12:10:00  |              |             | 10070.88           | ",
            "| SwitchStateEvent | 2         | 21:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 2         | 21:40:00  |              |             | 10099.15           | "
            };
            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false,  testSupport.FirstEventDate.AddDays(1), 300, TARGETCYCLECOUNT);   

            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 9,testSupport.AssetUid);   // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 9,testSupport.AssetUid); // Verify the result in the database 

            var baseUri = testSupport.GetBaseUri();
            var uri = string.Format(baseUri + "cyclesummary?date={0}&productFamily={1}", 
                testSupport.FirstEventDate.Date.AddDays(2).ToString("yyyy-MM-dd"), 
                assetType.Trim());
            var restClient = new RestClientUtil();
            var response = restClient.DoHttpRequest(uri, "GET", "application/json", null, HttpStatusCode.OK);
            var actualResult = JsonConvert.DeserializeObject<CycleSummaryResult>(response);

            var expectedResult = new CycleSummaryResult
            {
                day = new CycleSummaryData {totalCycleCount = 1, targetCycleCount = TARGETCYCLECOUNT, averageCycleCount = 1.0},
                week = new CycleSummaryData {totalCycleCount = 4, targetCycleCount = 3 * TARGETCYCLECOUNT, averageCycleCount = 4.0},
                month = new CycleSummaryData {totalCycleCount = 4, targetCycleCount = ((int)noDaysInMonth+3) * TARGETCYCLECOUNT, averageCycleCount = 4.0}
            };

            Assert.AreEqual(expectedResult.day, actualResult.day, " day cycle summary result doesn't match expected");
            Assert.AreEqual(expectedResult.week, actualResult.week, " week cycle summary result doesn't match expected");
            Assert.AreEqual(expectedResult.month, actualResult.month, " month cycle summary result doesn't match expected");
        }

        [TestMethod]
        public void Cycle_count_widget_test_cycles_over_month_end()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Widget 1","Cycle count widget end point test ");

            // Work out the date and number of days for the expected results.  
            var startDate = new DateTime(testSupport.FirstEventDate.Year, testSupport.FirstEventDate.Month, 1).AddDays(-2);
            testSupport.FirstEventDate = startDate;
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)startDate.AddDays(2).DayOfWeek + 7) % 7;
            DateTime nextMonday = startDate.AddDays(2  + daysUntilMonday);
            DateTime lastMonday = nextMonday.AddDays(-7);
            var daysTillLastMonday = (startDate.AddDays(2) - lastMonday).TotalDays+1;
            var noDaysLeft = daysTillLastMonday;
            if (daysTillLastMonday > 2)
                { noDaysLeft = 3; }
            if (startDate.AddDays(2).DayOfWeek == DayOfWeek.Monday)
            {
                noDaysLeft = 1;
                daysTillLastMonday = 1;
            }


            var assetType = Guid.NewGuid().ToString();  
            var eventArray = new[] {                                                                // Load events into array  
             " | EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType       |",
             $"| CreateAssetEvent | 0         | 03:00:00  | WIDGET2   | 12345678     | CAT  | 637G  | 20     | {assetType}     |",
            };

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:10:00  |              |             | 10000.15           | ",
            "| SwitchStateEvent | 0         | 12:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 12:30:00  |              |             | 10010.15           | ",
            "| SwitchStateEvent | 1         | 16:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 1         | 16:00:00  |              |             | 10020.15           | ",
            "| SwitchStateEvent | 1         | 23:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 1         | 23:00:00  |              |             | 10030.15           | ",
            "| SwitchStateEvent | 2         | 04:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 2         | 04:30:00  |              |             | 10040.15           | ",
            "| SwitchStateEvent | 2         | 12:01:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 2         | 12:01:00  |              |             | 10050.15           | ",
            "| SwitchStateEvent | 3         | 01:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 3         | 01:30:00  |              |             | 10060.15           | ",
            "| SwitchStateEvent | 3         | 12:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 3         | 12:00:00  |              |             | 10070.15           | ",
            "| SwitchStateEvent | 4         | 21:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 4         | 21:30:00  |              |             | 10080.15           | ",
            "| SwitchStateEvent | 5         | 09:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 5         | 09:00:00  |              |             | 10090.15           | ",
            "| SwitchStateEvent | 5         | 12:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 5         | 12:30:00  |              |             | 10100.15           | ",
            "| SwitchStateEvent | 6         | 16:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 6         | 16:00:00  |              |             | 10110.15           | ",
            "| SwitchStateEvent | 6         | 23:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 6         | 23:00:00  |              |             | 10120.15           | ",
            "| SwitchStateEvent | 7         | 04:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 7         | 04:30:00  |              |             | 10130.15           | ",
            "| SwitchStateEvent | 7         | 12:01:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 7         | 12:01:00  |              |             | 10140.15           | "
            };
            
            // Inject the asset event into the database
            testSupport.InjectEventsIntoMySqlDatabase(eventArray);                                   
            testSupport.CreateAssetConfigViaWebApiDumpOnly(4, true, testSupport.FirstEventDate.AddDays(-30), 100, 15);                         
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                   

            // Inject the switch and odometer events into the database
            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 15,testSupport.AssetUid);   
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 15,testSupport.AssetUid);  

            // Build the URI and call the cyclesummary endpoint
            var baseUri = testSupport.GetBaseUri();
            var uri = string.Format(baseUri + "cyclesummary?date={0}&productFamily={1}", 
                                    testSupport.FirstEventDate.Date.AddDays(2).ToString("yyyy-MM-dd"), 
                                    assetType.Trim());

            var restClient = new RestClientUtil();
            var response = restClient.DoHttpRequest(uri, "GET", "application/json", null, HttpStatusCode.OK);
            var actualResult = JsonConvert.DeserializeObject<CycleSummaryResult>(response);
            // Check the actual result with expected
            var expectedResult = new CycleSummaryResult
            {
                day = new CycleSummaryData {totalCycleCount = 2, targetCycleCount = 15, averageCycleCount = 2.0},   
                week = new CycleSummaryData {totalCycleCount = (int)noDaysLeft * 2, targetCycleCount = 15 * (int)daysTillLastMonday, averageCycleCount = 2 * (int)noDaysLeft},             
                month = new CycleSummaryData {totalCycleCount = 2, targetCycleCount = 15, averageCycleCount = 2.0}
            };

            Assert.AreEqual(expectedResult.day, actualResult.day, " day cycle summary result doesn't match expected");
            Assert.AreEqual(expectedResult.week, actualResult.week, " week cycle summary result doesn't match expected");
            Assert.AreEqual(expectedResult.month, actualResult.month, " month cycle summary result doesn't match expected");
        }
    }
}
