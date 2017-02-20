using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace WebApiTests
{
    [TestClass]
    public class CycleCountTestClass
    {      
        [TestMethod]
        public void Three_cycles_for_load_only_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();

            msg.Title("Cycle count 1","Three cycles with odometer events mixed. Load only asset config");

            var eventArray = new[] {                                                                      // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCLE1    | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);                                        // Inject event array into kafka 
            testSupport.CreateMockAssetConfigLoadOnly(2, true );                                     
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                   // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 2            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000              | ",
            "| SwitchStateEvent | 0         | 10:30:00  | 2            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 10:30:00  |              |             | 10010              | ",
            "| SwitchStateEvent | 0         | 12:00:00  | 2            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 12:00:00  |              |             | 10020              | "
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);                                 // Inject event array into kafka 
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);                                          // Create a utc offset
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 3,testSupport.AssetUid);       // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 3,testSupport.AssetUid);     // Verify the result in the database 
            var expectedAssetSummary = new[] {                                                           // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
            "| CYCLE1    | CYC12345     | 27        | CAT      | 312H  | 0d+12:00:00      | 3          | null                | 300              | "
            };

            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

        [TestMethod]
        public void Three_Cycles_For_Dump_only_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();

            msg.Title("Cycle count 2","Three cycles with odometer events mixed. Dump only asset config");

            var eventArray = new[] {                                                                        // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCLE2    | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);                                          // Inject event array into kafka 
            testSupport.CreateMockAssetConfigDumpOnly(2, true);                            
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 2            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000              | ",
            "| SwitchStateEvent | 0         | 10:30:00  | 2            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 10:30:00  |              |             | 10010              | ",
            "| SwitchStateEvent | 0         | 12:00:00  | 2            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 12:00:00  |              |             | 10020              | "
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);                                 // Inject event array into kafka 
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);                                          // Create a utc offset
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 3,testSupport.AssetUid);       // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 3,testSupport.AssetUid);     // Verify the result in the database 
            var expectedAssetSummary = new[] {                                                           // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
            "| CYCLE2    | CYC12345     | 27        | CAT      | 312H  | 0d+12:00:00      | 3          | 20                  | 300              | "
            };

            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }
    }
}