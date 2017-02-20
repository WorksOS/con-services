using TestUtility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WebApiTests
{
    [TestClass]
    public class CycleDistanceTestClass
    {
        [TestMethod]
        public void First_load_cycle_has_no_distance_for_first_cycle_load_only_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Cycle Distance 1","First load cycle has no distance travelled for first cycle. Load only asset config");

            var eventArray = new[] {                                                      
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCLE1    | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);            
            testSupport.CreateMockAssetConfigLoadOnly(1, true );                                     
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                   // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 1            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000              | "
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);      
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 1,testSupport.AssetUid);       // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 1,testSupport.AssetUid);     // Verify the result in the database

            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | distanceTravelledKm | ",
            "| CYCLE1    | CYC12345     | 27        | CAT      | 312H  | 1          | null                | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

        [TestMethod]
        public void First_load_cycle_has_no_distance_for_first_cycle_Dump_only_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Cycle Distance 2","First load cycle has no distance travelled for first cycle. Dump only asset config");

            var eventArray = new[] {                                                        // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCLE2    | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);              
            testSupport.CreateMockAssetConfigDumpOnly(2, true);                            
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 2            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000              | "
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);      
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 1,testSupport.AssetUid);       // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 1,testSupport.AssetUid);     // Verify the result in the database 

            var expectedAssetSummary = new[] {                                                                   
            "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | distanceTravelledKm | ",
            "| CYCLE2    | CYC12345     | 27        | CAT      | 312H  | 1          | null                | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

        [TestMethod]
        public void First_load_cycle_has_no_distance_for_first_cycle_LoadDump_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Cycle Distance 3","First load cycle has no distance travelled for first cycle. LoadDump asset config");

            var eventArray = new[] {                                                        
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCLE3    | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);               
            testSupport.CreateMockAssetConfig(2, true, 2, false);                                     
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 2            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000              | ",
            "| SwitchStateEvent | 0         | 10:30:00  | 2            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 10:30:00  |              |             | 10010              | ",
            "| SwitchStateEvent | 0         | 12:00:00  | 2            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 12:00:00  |              |             | 10020              | ",
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);  
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 3,testSupport.AssetUid);       // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 3,testSupport.AssetUid);     // Verify the result in the database

            var expectedAssetSummary = new[] {                                                                  
            "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | distanceTravelledKm | ",
            "| CYCLE3    | CYC12345     | 27        | CAT      | 312H  | 1          | 20                  | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

       [TestMethod]
        public void Multiple_load_cycles_and_distance_on_one_day_load_only_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Cycle Distance 4","Multiple load cycles and distance on one day. Load only asset config");

            var eventArray = new[] {                                                         
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCLE4    | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);               
            testSupport.CreateMockAssetConfigLoadOnly(4, true );                                     
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000.15           | ",
            "| SwitchStateEvent | 0         | 10:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 10:30:00  |              |             | 10010.15           | ",
            "| SwitchStateEvent | 0         | 12:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 12:00:00  |              |             | 10020.15           | ",
            "| SwitchStateEvent | 0         | 13:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 13:00:00  |              |             | 10030.15           | ",
            "| SwitchStateEvent | 0         | 14:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 14:30:00  |              |             | 10040.15           | ",
            "| SwitchStateEvent | 0         | 16:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 16:00:00  |              |             | 10050.15           | ",
            "| SwitchStateEvent | 0         | 17:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 17:30:00  |              |             | 10060.15           | ",
            "| SwitchStateEvent | 0         | 19:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 19:00:00  |              |             | 10070.88           | ",
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);      
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 8,testSupport.AssetUid);       // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 8,testSupport.AssetUid);     // Verify the result in the database

            var expectedAssetSummary = new[] {                                                                  
            "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | distanceTravelledKm | ",
            "| CYCLE4    | CYC12345     | 27        | CAT      | 312H  | 8          | 70.73               | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

        [TestMethod]
        public void Multiple_load_cycles_and_distance_on_one_day_dump_only_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Cycle Distance 5","Multiple load cycles and distance on one day. Dump only asset config");

            var eventArray = new[] {                                                       
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCLE5    | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);               
            testSupport.CreateMockAssetConfigDumpOnly(4, false );                            
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                         

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000.15           | ",
            "| SwitchStateEvent | 0         | 10:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 10:30:00  |              |             | 10010.15           | ",
            "| SwitchStateEvent | 0         | 12:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 12:00:00  |              |             | 10020.15           | ",
            "| SwitchStateEvent | 0         | 13:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 13:00:00  |              |             | 10030.15           | ",
            "| SwitchStateEvent | 0         | 14:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 14:30:00  |              |             | 10040.15           | ",
            "| SwitchStateEvent | 0         | 16:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 16:00:00  |              |             | 10050.15           | ",
            "| SwitchStateEvent | 0         | 17:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 17:30:00  |              |             | 10060.15           | ",
            "| SwitchStateEvent | 0         | 19:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 19:00:00  |              |             | 10070.88           | ",
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);      
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 8,testSupport.AssetUid);       // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 8,testSupport.AssetUid);     // Verify the result in the database 

            var expectedAssetSummary = new[] {                                                                  
            "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | distanceTravelledKm | ",
            "| CYCLE5    | CYC12345     | 27        | CAT      | 312H  | 8          | 70.73               | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

        [TestMethod]
        public void Multiple_load_cycles_and_distance_on_one_day_loaddump_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Cycle Distance 6","Multiple load cycles and distance on one day. LoadDump asset config");

            var eventArray = new[] {                                                          
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCLE6    | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);               
            testSupport.CreateMockAssetConfig(4, true, 4, false);                                     
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000.15           | ",
            "| SwitchStateEvent | 0         | 10:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 10:30:00  |              |             | 10010.15           | ",
            "| SwitchStateEvent | 0         | 12:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 12:00:00  |              |             | 10020.15           | ",
            "| SwitchStateEvent | 0         | 13:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 13:00:00  |              |             | 10030.15           | ",
            "| SwitchStateEvent | 0         | 14:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 14:30:00  |              |             | 10040.15           | ",
            "| SwitchStateEvent | 0         | 16:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 16:00:00  |              |             | 10050.15           | ",
            "| SwitchStateEvent | 0         | 17:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 17:30:00  |              |             | 10060.15           | ",
            "| SwitchStateEvent | 0         | 19:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 19:00:00  |              |             | 10070.88           | ",
            "| SwitchStateEvent | 0         | 21:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 21:30:00  |              |             | 10099.15           | ",
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);      
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 9,testSupport.AssetUid);       // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 9,testSupport.AssetUid);     // Verify the result in the database 

            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | distanceTravelledKm | ",
            "| CYCLE6    | CYC12345     | 27        | CAT      | 312H  | 4          | 99                  | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

       [TestMethod]
        public void Multiple_day_cycles_and_distance_load_only_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Cycle Distance 7","Multiple day cycles and distance. Load only asset config");

            var eventArray = new[] {                                                        // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCLE7    | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);           
            testSupport.CreateMockAssetConfigLoadOnly(4, true );                                     
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000.15           | ",
            "| SwitchStateEvent | 0         | 12:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 12:30:00  |              |             | 10010.15           | ",
            "| SwitchStateEvent | 0         | 15:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 15:00:00  |              |             | 10020.15           | ",
            "| SwitchStateEvent | 0         | 21:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 21:00:00  |              |             | 10030.15           | ",
            "| SwitchStateEvent | 1         | 10:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 1         | 10:30:00  |              |             | 10040.15           | ",
            "| SwitchStateEvent | 1         | 16:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 1         | 16:00:00  |              |             | 10050.15           | ",
            "| SwitchStateEvent | 2         | 23:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 2         | 23:30:00  |              |             | 10060.15           | ",
            "| SwitchStateEvent | 3         | 02:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 3         | 02:00:00  |              |             | 10070.88           | "
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);      
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 8,testSupport.AssetUid);       // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 8,testSupport.AssetUid);     // Verify the result in the database

            var expectedAssetSummary = new[] {                                                                  
            "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | distanceTravelledKm | ",
            "| CYCLE7    | CYC12345     | 27        | CAT      | 312H  | 8          | 70.73               | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

        [TestMethod]
        public void Multiple_day_cycles_and_distance_on_one_day_dump_only_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Cycle Distance 8","Multiple day cycles and distance. Dump only asset config");

            var eventArray = new[] {                                                        // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCLE8    | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);               
            testSupport.CreateMockAssetConfigDumpOnly(4, false );                            
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000.15           | ",
            "| SwitchStateEvent | 0         | 12:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 12:30:00  |              |             | 10010.15           | ",
            "| SwitchStateEvent | 0         | 15:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 15:00:00  |              |             | 10020.15           | ",
            "| SwitchStateEvent | 0         | 21:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 21:00:00  |              |             | 10030.15           | ",
            "| SwitchStateEvent | 1         | 10:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 1         | 10:30:00  |              |             | 10040.15           | ",
            "| SwitchStateEvent | 1         | 16:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 1         | 16:00:00  |              |             | 10050.15           | ",
            "| SwitchStateEvent | 2         | 23:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 2         | 23:30:00  |              |             | 10060.15           | ",
            "| SwitchStateEvent | 3         | 02:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 3         | 02:00:00  |              |             | 10070.88           | "
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);      
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 8,testSupport.AssetUid);       // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 8,testSupport.AssetUid);     // Verify the result in the database 

            var expectedAssetSummary = new[] {                                                                  
            "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | distanceTravelledKm | ",
            "| CYCLE8    | CYC12345     | 27        | CAT      | 312H  | 8          | 70.73               | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

        [TestMethod]
        public void Multiple_day_load_cycles_and_distance_loaddump_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Cycle Distance 9","Multiple day cycles and distance. LoadDump asset config");

            var eventArray = new[] {                                                        // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCLE9    | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);               
            testSupport.CreateMockAssetConfig(4, true, 4, false);                                     
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      

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
            "| SwitchStateEvent | 2         | 21:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 2         | 21:30:00  |              |             | 10099.15           | ",
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);     
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);                                         // Create a utc offset
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 9,testSupport.AssetUid);       // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 9,testSupport.AssetUid);     // Verify the result in the database 

            var expectedAssetSummary = new[] {                                                                
            "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | distanceTravelledKm | ",
            "| CYCLE9    | CYC12345     | 27        | CAT      | 312H  | 4          | 99                  | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

       [TestMethod]
        public void Multiple_day_odometer_outside_threshold_load_only_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Cycle Distance 10","Multiple day odometer outside threshold. Load only asset config");

            var eventArray = new[] {                                                        
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCLE10   | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);               
            testSupport.CreateMockAssetConfigLoadOnly(4, true );                                     
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:10:00  |              |             | 10000.15           | ",  // 10 minutes later
            "| SwitchStateEvent | 0         | 12:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 12:30:00  |              |             | 10010.15           | ",
            "| SwitchStateEvent | 0         | 15:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 15:00:00  |              |             | 10020.15           | ",
            "| SwitchStateEvent | 0         | 21:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 21:10:00  |              |             | 10030.15           | ",  // 10 minutes later
            "| SwitchStateEvent | 1         | 10:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 1         | 10:30:00  |              |             | 10040.15           | ",
            "| SwitchStateEvent | 1         | 16:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 1         | 16:00:00  |              |             | 10050.15           | ",
            "| SwitchStateEvent | 2         | 23:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 2         | 23:40:00  |              |             | 10060.15           | ", // 10 minutes later
            "| SwitchStateEvent | 3         | 02:00:00  | 4            | SwitchOn    |                    | "
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);      
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 8,testSupport.AssetUid);       // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 7,testSupport.AssetUid);     // Verify the result in the database

            var expectedAssetSummary = new[] {                                                                  
            "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | distanceTravelledKm | ",
            "| CYCLE10   | CYC12345     | 27        | CAT      | 312H  | 8          | 20                  | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

        [TestMethod]
        public void Multiple_day_odometer_outside_threshold_dump_only_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("Cycle Distance 11","Multiple day odometer outside threshold. Dump only asset config");

            var eventArray = new[] {                                                        // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCLE11   | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };
            testSupport.InjectEventsIntoMySqlDatabase(eventArray);             
            testSupport.CreateMockAssetConfigDumpOnly(4, false );                            
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 09:10:00  |              |             | 10000.15           | ",  // 10 minutes later
            "| SwitchStateEvent | 0         | 12:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 12:30:00  |              |             | 10010.15           | ",
            "| SwitchStateEvent | 0         | 15:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 15:00:00  |              |             | 10020.15           | ",
            "| SwitchStateEvent | 0         | 21:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 21:10:00  |              |             | 10030.15           | ",  // 10 minutes later
            "| SwitchStateEvent | 1         | 10:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 1         | 10:30:00  |              |             | 10040.15           | ",
            "| SwitchStateEvent | 1         | 16:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 1         | 16:00:00  |              |             | 10050.15           | ",
            "| SwitchStateEvent | 2         | 23:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 2         | 23:40:00  |              |             | 10060.15           | ", // 10 minutes later
            "| SwitchStateEvent | 3         | 02:00:00  | 4            | SwitchOff   |                    | "
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);     
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 8,testSupport.AssetUid);       // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 7,testSupport.AssetUid);     // Verify the result in the database

            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | distanceTravelledKm | ",
            "| CYCLE11   | CYC12345     | 27        | CAT      | 312H  | 8          | 20                  | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

        [TestMethod]
        public void Multiple_day_odometer_outside_threshold_loaddump_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();;
            msg.Title("Cycle Distance 12","Multiple day odometer outside threshold. LoadDump asset config");

            var eventArray = new[] {                                                        
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 03:00:00  | CYCLE12   | CYC12345     | CAT  | 312H  | 27     | TRUCK     |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);              
            testSupport.CreateMockAssetConfig(4, true, 4, false);                                     
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      

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
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 9,testSupport.AssetUid);       // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 9,testSupport.AssetUid);     // Verify the result in the database

            var expectedAssetSummary = new[] {                                                                 
            "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | distanceTravelledKm | ",
            "| CYCLE12   | CYC12345     | 27        | CAT      | 312H  | 4          | 40                  | "
            };
            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

    }
}
