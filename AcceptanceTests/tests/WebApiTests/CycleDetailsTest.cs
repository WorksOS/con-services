using System;
using System.Diagnostics;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace WebApiTests
{
    [TestClass]
    public class CycleDetailsTestClass
    {
        [TestMethod]
        public void Three_cycles_for_load_only_config()
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();

            msg.Title("Cycle details 1", "Three cycles with odometer events mixed. Load only asset config");

            var eventArray = new[]
            {                 
                "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
                "| CreateAssetEvent | 0         | 03:00:00  | CYCDET1   | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray); // Inject event array into kafka 
            testSupport.CreateMockAssetConfigLoadOnly(2, true);
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);     // Verify the result in the database      

            var switchEventArray = new[]
            {
                "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
                "| SwitchStateEvent | 0         | 09:00:00  | 2            | SwitchOn    |                    | ",
                "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000              | ",
                "| SwitchStateEvent | 0         | 10:30:00  | 2            | SwitchOn    |                    | ",
                "| OdometerEvent    | 0         | 10:30:00  |              |             | 10010              | ",
                "| SwitchStateEvent | 0         | 12:00:00  | 2            | SwitchOn    |                    | ",
                "| OdometerEvent    | 0         | 12:00:00  |              |             | 10020              | "
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);           // Inject event array into kafka 
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 3,testSupport.AssetUid);              // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 3,testSupport.AssetUid);            // Verify the result in the database 
            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
            "| CYCDET1   | CYC12345     | 27        | CAT      | 312H  | 0d+12:00:00      | 3          | 20                  | 300              | "
            };

            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
            testSupport.CompareActualAssetDetailsFromCyclesEndpointWithExpectedResults(expectedAssetSummary);

            var expectedCycleResults = new[] {
            "| startCycleDeviceTime  | endCycleDeviceTime  | dumpDeviceTime | volumePerCycleCubicMeter | odometerStartCycleValue | odometerEndCycleValue | odometerDumpCycleValue | distanceTravelled | cycleLengthMinutes |",
            "| null                  | 0d+09:00:00         | null           | 100                      | null                    | 10000                 | null                   | null              | null               |",
            "| 0d+09:00:00           | 0d+10:30:00         | null           | 100                      | 10000                   | 10010                 | null                   | 10                | 90                 |",
            "| 0d+10:30:00           | 0d+12:00:00         | null           | 100                      | 10010                   | 10020                 | null                   | 10                | 90                 |"
            };

            testSupport.CompareActualAssetCycleDetailsWithExpectedResults(expectedCycleResults,-2, 2);
        }


        [TestMethod]
        public void Three_Cycles_For_Dump_only_config()
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();

            msg.Title("Cycle details 2", "Three cycles with odometer events mixed. Dump only asset config");

            var eventArray = new[] {                                                        // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCDET2   | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);               // Inject event array into kafka 
            testSupport.CreateMockAssetConfigDumpOnly(2, true);
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);     // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 2            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000              | ",
            "| SwitchStateEvent | 0         | 10:30:00  | 2            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 10:30:00  |              |             | 10010              | ",
            "| SwitchStateEvent | 0         | 12:00:00  | 2            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 12:00:00  |              |             | 10020              | "
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);      // Inject event array into kafka 
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 3,testSupport.AssetUid);              // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 3,testSupport.AssetUid);            // Verify the result in the database 
            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
            "| CYCDET2   | CYC12345     | 27        | CAT      | 312H  | 0d+12:00:00      | 3          | 20                  | 300              | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
            testSupport.CompareActualAssetDetailsFromCyclesEndpointWithExpectedResults(expectedAssetSummary);

            var expectedCycleResults = new[] {
            "| startCycleDeviceTime | endCycleDeviceTime | dumpDeviceTime | volumePerCycleCubicMeter | odometerStartCycleValue | odometerEndCycleValue | odometerDumpCycleValue | distanceTravelled | cycleLengthMinutes |",
            "| null                 | 0d+09:00:00        | null           | 100                      | null                    | 10000                 | null                   | null              | null               |",
            "| 0d+09:00:00          | 0d+10:30:00        | null           | 100                      | 10000                   | 10010                 | null                   | 10                | 90                 |",
            "| 0d+10:30:00          | 0d+12:00:00        | null           | 100                      | 10010                   | 10020                 | null                   | 10                | 90                 |",
            };
            testSupport.CompareActualAssetCycleDetailsWithExpectedResults(expectedCycleResults,-2, 2);
        }


        [TestMethod]
        public void Estimated_volumes_With_future_asset_config_loaddump_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Cycle details 3","Cycle details with estimated volumes, distance and cycles. LoadDump asset config");

            var eventArray = new[] {                                                                                            // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType    |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCDET3   | CYC12345     | CAT  | 994H  | 22     | TRACK LOADER |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);                                                  // Inject event array into kafka 
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false, testSupport.FirstEventDate.AddDays(-5), 333, 999);                         
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                                        // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000.15           | ",
            "| SwitchStateEvent | 0         | 12:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 12:30:00  |              |             | 10010.15           | ",
            "| SwitchStateEvent | 0         | 16:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 16:00:00  |              |             | 10020.15           | ",
            "| SwitchStateEvent | 0         | 23:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 23:00:00  |              |             | 10030.15           | ",
            "| SwitchStateEvent | 1         | 04:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 1         | 04:30:00  |              |             | 10040.15           | ",
            "| SwitchStateEvent | 1         | 12:01:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 1         | 12:01:00  |              |             | 10050.15           | ",
            "| SwitchStateEvent | 2         | 01:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 2         | 01:30:00  |              |             | 10060.15           | ",
            "| SwitchStateEvent | 2         | 12:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 2         | 12:00:00  |              |             | 10070.88           | ",
            "| SwitchStateEvent | 2         | 21:30:15  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 2         | 21:30:15  |              |             | 10099.15           | ",
            "| SwitchStateEvent | 2         | 21:31:15  | 4            | SwitchOff   |                    | ", 
            "| OdometerEvent    | 2         | 21:31:15  |              |             | 10199.15           | ",
            "| SwitchStateEvent | 2         | 21:32:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 2         | 21:32:16  |              |             | 10299.15           | "
            };
            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 11,testSupport.AssetUid);              // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 11,testSupport.AssetUid);            // Verify the result in the database 
            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
            "| CYCDET3   | CYC12345     | 22        | CAT      | 994H  | 2d+21:32:16      | 5          | 299                 | 1665             | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
            testSupport.CompareActualAssetDetailsFromCyclesEndpointWithExpectedResults(expectedAssetSummary);


            // Now verify the cycle details
            var expectedCycleResults = new[] {
            "| startCycleDeviceTime | endCycleDeviceTime | dumpDeviceTime | volumePerCycleCubicMeter | odometerStartCycleValue | odometerEndCycleValue | odometerDumpCycleValue | distanceTravelled | cycleLengthMinutes | cycleReportedDeviceTime |",
            "| 0d+09:00:00          | 0d+16:00:00        | 0d+12:30:00    | 333                      | 10000.15                | 10020.15              | 10010.15               | 20                | 420                | 0d+16:00:00             |",
            "| 0d+16:00:00          | 1d+04:30:00        | 0d+23:00:00    | 333                      | 10020.15                | 10040.15              | 10030.15               | 20                | 750                | 1d+04:30:00             |",
            "| 1d+04:30:00          | 2d+01:30:00        | 1d+12:01:00    | 333                      | 10040.15                | 10060.15              | 10050.15               | 20                | 1260               | 2d+01:30:00             |",
            "| 2d+01:30:00          | 2d+21:30:15        | 2d+12:00:00    | 333                      | 10060.15                | 10099.15              | 10070.88               | 39                | 1200.25            | 2d+21:30:15             |",
            "| 2d+21:30:15          | 2d+21:32:00        | 2d+21:31:15    | 333                      | 10099.15                | 10299.15              | 10199.15               | 200               | 1.75               | 2d+21:32:00             |",
            };

            testSupport.CompareActualAssetCycleDetailsWithExpectedResults(expectedCycleResults,-2, 2);
        }



        [TestMethod]
        public void Cycles_details_span_and_skip_days_loaddump_asset_configs()
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Cycle details 4", "Cycle details with estimated volumes, cycles spanning and skipping days with multiple asset configs. LoadDump asset config");

            var eventArray = new[] {                                                        // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType         |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCDET4   | CYC12345     | CAT  | 740B  | 31     | ARTICULATED TRUCK |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);                       // Inject event array into kafka                     
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);             // Verify the result in the database      
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false, testSupport.FirstEventDate.AddDays(-2), 111, 999);
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false, testSupport.FirstEventDate.AddDays(-1), 222, 999);
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false, testSupport.FirstEventDate, 333, 999);
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false, testSupport.FirstEventDate.AddDays(1), 444, 999);
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false, testSupport.FirstEventDate.AddDays(2), 555, 999); 
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false, testSupport.FirstEventDate.AddDays(4), 666, 999);

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 5            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000              | ",
            "| SwitchStateEvent | 0         | 12:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 12:30:00  |              |             | 10010.15           | ",
            "| SwitchStateEvent | 0         | 13:00:00  | 5            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 13:00:00  |              |             | 10020.15           | ",
            "| SwitchStateEvent | 0         | 16:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 16:00:00  |              |             | 10030.15           | ",
            "| SwitchStateEvent | 1         | 16:30:00  | 5            | SwitchOn    |                    | ",
            "| OdometerEvent    | 1         | 16:30:00  |              |             | 10040.15           | ",
            "| SwitchStateEvent | 3         | 00:01:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 3         | 00:01:00  |              |             | 10050.15           | ",
            "| SwitchStateEvent | 5         | 01:30:00  | 5            | SwitchOn    |                    | ",
            "| OdometerEvent    | 5         | 01:30:00  |              |             | 10060.15           | ",
            "| SwitchStateEvent | 5         | 12:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 5         | 12:00:00  |              |             | 10070.88           | ",
            "| SwitchStateEvent | 5         | 21:30:00  | 5            | SwitchOn    |                    | ",
            "| OdometerEvent    | 5         | 21:30:00  |              |             | 10099.88           | ",
            "| SwitchStateEvent | 5         | 22:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 5         | 22:30:00  |              |             | 10112.88           | ",
            "| SwitchStateEvent | 6         | 04:30:00  | 5            | SwitchOn    |                    | ",
            "| OdometerEvent    | 6         | 04:30:00  |              |             | 10119.15           | ",
            };
            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 11,testSupport.AssetUid);              // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 11,testSupport.AssetUid);            // Verify the result in the database 
            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
            "| CYCDET4   | CYC12345     | 31        | CAT      | 740B  | 6d+04:30:00      | 5          | 119.15              | 2442             | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
            testSupport.CompareActualAssetDetailsFromCyclesEndpointWithExpectedResults(expectedAssetSummary);

            // Now verify the cycle details
            var expectedCycleResults = new[] {
            "| startCycleDeviceTime | endCycleDeviceTime | dumpDeviceTime | volumePerCycleCubicMeter | odometerStartCycleValue | odometerEndCycleValue | odometerDumpCycleValue | distanceTravelled | cycleLengthMinutes | ",
            "| 0d+09:00:00          | 0d+13:00:00        | 0d+12:30:00    | 333                      | 10000                   | 10020.15              | 10010.15               | 20.15             | 240                | ",
            "| 0d+13:00:00          | 1d+16:30:00        | 0d+16:00:00    | 333                      | 10020.15                | 10040.15              | 10030.15               | 20                | 1650               | ",
            "| 1d+16:30:00          | 5d+01:30:00        | 3d+00:01:00    | 444                      | 10040.15                | 10060.15              | 10050.15               | 20                | 4860               | ",
            "| 5d+01:30:00          | 5d+21:30:00        | 5d+12:00:00    | 666                      | 10060.15                | 10099.88              | 10070.88               | 39.73             | 1200               | ",
            "| 5d+21:30:00          | 6d+04:30:00        | 5d+22:30:00    | 666                      | 10099.88                | 10119.15              | 10112.88               | 19.27             | 420                | "
            };
            testSupport.CompareActualAssetCycleDetailsWithExpectedResults(expectedCycleResults, -2, 2);

            var expectedCycleResults2 = new[] {
            "| startCycleDeviceTime | endCycleDeviceTime | dumpDeviceTime | volumePerCycleCubicMeter | odometerStartCycleValue | odometerEndCycleValue | odometerDumpCycleValue | distanceTravelled | cycleLengthMinutes | ",
            "| 5d+01:30:00          | 5d+21:30:00        | 5d+12:00:00    | 666                      | 10060.15                | 10099.88              | 10070.88               | 39.73             | 1200               | ",
            "| 5d+21:30:00          | 6d+04:30:00        | 5d+22:30:00    | 666                      | 10099.88                | 10119.15              | 10112.88               | 19.27             | 420                | " 
            };
            testSupport.CompareActualAssetCycleDetailsWithExpectedResults(expectedCycleResults2, 2, 2);

            // Now verify the cycle details
            var expectedCycleResults3 = new[] {
            "| startCycleDeviceTime | endCycleDeviceTime | dumpDeviceTime | volumePerCycleCubicMeter | odometerStartCycleValue | odometerEndCycleValue | odometerDumpCycleValue | distanceTravelled | cycleLengthMinutes | ",
            "| 0d+09:00:00          | 0d+13:00:00        | 0d+12:30:00    | 333                      | 10000                   | 10020.15              | 10010.15               | 20.15             | 240                | ",
            "| 0d+13:00:00          | 1d+16:30:00        | 0d+16:00:00    | 333                      | 10020.15                | 10040.15              | 10030.15               | 20                | 1650               | ",
            "| 1d+16:30:00          | null               | 3d+00:01:00    | 444                      | 10040.15                | null                  | 10050.15               | null              | null               | ",
            };
            testSupport.CompareActualAssetCycleDetailsWithExpectedResults(expectedCycleResults3, -2, -2);
        }


        [TestMethod]
        public void InvalidAssetUID()
        {
            var testAsset = Guid.NewGuid();
            var appConfig = new TestConfig();
            var baseUri = appConfig.webApiUri;
            if (Debugger.IsAttached)
            {
                baseUri = appConfig.debugWebApiUri;
            }
            string uri = baseUri + string.Format("{0}/cycles?startDate={1}&endDate={2}", "3434fgfg",DateTime.UtcNow.Date, DateTime.UtcNow.Date);
            var restClient = new RestClientUtil();
            var response = restClient.DoHttpRequest(uri, "GET", "application/json", null, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void InvalidDate()
        {
            var testAsset = Guid.NewGuid();
            var appConfig = new TestConfig();
            var baseUri = appConfig.webApiUri;
            if (Debugger.IsAttached)
            {
                baseUri = appConfig.debugWebApiUri;
            }
            string uri = baseUri + string.Format("{0}/cycles?startDate={1}&endDate={2}", Guid.NewGuid(), "werwer234234", DateTime.UtcNow.Date);
            var restClient = new RestClientUtil();
            var response = restClient.DoHttpRequest(uri, "GET", "application/json", null, HttpStatusCode.InternalServerError);
        }

        [TestMethod]
        public void NoDate()
        {
            var testAsset = Guid.NewGuid();
            var appConfig = new TestConfig();
            var baseUri = appConfig.webApiUri;
            if (Debugger.IsAttached)
            {
                baseUri = appConfig.debugWebApiUri;
            }
            string uri = baseUri + string.Format("{0}/cycles", Guid.NewGuid());
            var restClient = new RestClientUtil();
            var response = restClient.DoHttpRequest(uri, "GET", "application/json", null, HttpStatusCode.InternalServerError);
        }

    }
}
