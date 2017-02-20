using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace WebApiTests
{
    [TestClass]
    public class EstimatedVolumeTestClass
    {
        [TestMethod]
        public void Multiple_day_odometer_with_estimated_volumes_load_only_config()
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();

            msg.Title("EstimatedVolume 1","Multiple day odometer with estimated volumes. Load only asset config");

            var eventArray = new[] {                                                       
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType  |",
            "| CreateAssetEvent | 0         | 03:00:00  | ESTVOL1   | CYC12345     | CAT  | 572C  | 17     | PIPE LAYER |"
            };


            testSupport.InjectEventsIntoMySqlDatabase(eventArray);               
            testSupport.CreateAssetConfigViaWebApiLoadOnly(4, true, testSupport.FirstEventDate.AddDays(-10), 200, 999);                                     
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 09:10:00  |              |             | 10000.15           | ", 
            "| SwitchStateEvent | 0         | 12:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 12:30:00  |              |             | 10010.15           | ",
            "| SwitchStateEvent | 0         | 15:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 15:00:00  |              |             | 10020.15           | ",
            "| SwitchStateEvent | 0         | 21:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 0         | 21:10:00  |              |             | 10030.15           | ", 
            "| SwitchStateEvent | 1         | 10:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 1         | 10:30:00  |              |             | 10040.15           | ",
            "| SwitchStateEvent | 1         | 16:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 1         | 16:00:00  |              |             | 10050.15           | ",
            "| SwitchStateEvent | 2         | 23:30:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 2         | 23:40:00  |              |             | 10060.15           | ", 
            "| SwitchStateEvent | 3         | 02:00:00  | 4            | SwitchOn    |                    | ",
            "| OdometerEvent    | 3         | 02:00:00  |              |             | 10070.15           | ", 
            "| SwitchStateEvent | 4         | 02:00:00  | 4            | SwitchOn    |                    | "
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            testSupport.CreateAssetConfigViaWebApiLoadOnly(4, true, testSupport.FirstEventDate.AddDays(2), 500, 999);      

            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 9,testSupport.AssetUid);         // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 8,testSupport.AssetUid);       // Verify the result in the database 

            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
            "| ESTVOL1   | CYC12345     | 17        | CAT      | 572C  | 4d+02:00:00      | 9          | 20                  | 2700             | "
            };

            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

        [TestMethod]
        public void Multiple_day_odometer_with_estimated_volumes_dump_only_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();

            msg.Title("EstimatedVolume 2","Multiple day odometer with estimated volumes. Dump only asset config");

            var eventArray = new[] {                                                        // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType         |",
            "| CreateAssetEvent | 0         | 03:00:00  | ESTVOL2   | CYC12345     | CAT  | 345   | 19     | SKID STEER LOADER |"
            };
     
            testSupport.InjectEventsIntoMySqlDatabase(eventArray);               // Inject event array into kafka 
            testSupport.CreateAssetConfigViaWebApiDumpOnly(4, false, testSupport.FirstEventDate.AddDays(-10), 50,999);  
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                         // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
            "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 09:10:00  |              |             | 10000.15           | ", 
            "| SwitchStateEvent | 0         | 12:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 12:30:00  |              |             | 10010.15           | ",
            "| SwitchStateEvent | 0         | 15:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 15:00:00  |              |             | 10020.15           | ",
            "| SwitchStateEvent | 0         | 21:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 0         | 21:10:00  |              |             | 10030.15           | ", 
            "| SwitchStateEvent | 1         | 10:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 1         | 10:30:00  |              |             | 10040.15           | ",
            "| SwitchStateEvent | 1         | 16:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 1         | 16:00:00  |              |             | 10050.15           | ",
            "| SwitchStateEvent | 2         | 23:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 2         | 23:40:00  |              |             | 10060.15           | ", 
            "| SwitchStateEvent | 3         | 02:00:00  | 4            | SwitchOff   |                    | "
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);

            testSupport.CreateAssetConfigViaWebApiDumpOnly(4, false, testSupport.FirstEventDate.AddDays(1), 300, 999);      

            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 8,testSupport.AssetUid);         // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 7,testSupport.AssetUid);       // Verify the result in the database 

            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
            "| ESTVOL2   | CYC12345     | 19        | CAT      | 345   | 3d+02:00:00      | 8          | 20                  | 1400             | "
            };

            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

        [TestMethod]
        public void Multiple_day_odometer_with_estimated_volumes_loaddump_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();

            msg.Title("EstimatedVolume 3","Multiple day odometer with estimated volumes. LoadDump asset config. Asset Filtering tested as well");

            var eventArray = new[] {                                                        // Load events into array  
            " | EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId |",
            $"| CreateAssetEvent | 0         | 03:00:00  | ESTVOL3   | CYC12345     | CAT  | 637g  | 20     |",
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);               // Inject event array into kafka 
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false, testSupport.FirstEventDate.AddDays(-30), 150, 999);                         
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);     // Verify the result in the database      

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
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false,  testSupport.FirstEventDate.AddDays(1), 300, 999);   

            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 9,testSupport.AssetUid);         // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 9,testSupport.AssetUid);       // Verify the result in the database 

            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
            "| ESTVOL3   | CYC12345     | 20        | CAT      | 637g  | 2d+21:40:00      | 4          | 40                  | 900              | "
            };

            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

        [TestMethod]
        public void Estimated_volumes_With_future_asset_config_loaddump_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();

            msg.Title("EstimatedVolume 4","Estimated volumes with a asset config in the future. LoadDump asset config");

            var eventArray = new[] {                                                                        // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType    |",
            "| CreateAssetEvent | 0         | 03:00:00  | ESTVOL4   | CYC12345     | CAT  | 994H  | 22     | TRACK LOADER |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);                               // Inject event array into kafka 
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false, testSupport.FirstEventDate.AddDays(-5), 333, 999);                         
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                                     // Verify the result in the database      

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
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false,  testSupport.FirstEventDate.AddDays(3),1000, 500);   

            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 9,testSupport.AssetUid);                         // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 9,testSupport.AssetUid);                       // Verify the result in the database 

            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
            "| ESTVOL4   | CYC12345     | 22        | CAT      | 994H  | 2d+21:40:00      | 4          | 40                  | 1332             | "
            };

            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }


        [TestMethod]
        public void Estimated_volumes_multiple_asset_configs()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();

            msg.Title("EstimatedVolume 5","Estimated volumes with multiple asset configs. LoadDump asset config");

            var eventArray = new[] {                                                        // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType          |",
            "| CreateAssetEvent | 0         | 03:00:00  | ESTVOL5   | CYC12345     | CAT  | D7E   | 23     | TRACK TYPE TRACTOR |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);               // Inject event array into kafka                     
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false,  testSupport.FirstEventDate.AddDays(-2), 111, 999);    
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false,  testSupport.FirstEventDate.AddDays(-1), 222, 999);    
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false,  testSupport.FirstEventDate, 333, 999);    
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false,  testSupport.FirstEventDate.AddDays(1), 444, 999);    
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false,  testSupport.FirstEventDate.AddDays(2), 555, 999);    
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false,  testSupport.FirstEventDate.AddDays(3), 666, 999);    


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
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false,  testSupport.FirstEventDate.AddDays(3),1000, 500);   // Update an existing asset config

            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 9,testSupport.AssetUid);         // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 9,testSupport.AssetUid);       // Verify the result in the database

            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
            "| ESTVOL5   | CYC12345     | 23        | CAT      | D7E   | 2d+21:40:00      | 4          | 40                  | 1665             | "
            };

            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }

        [TestMethod]
        public void Estimated_volumes_cycles_span_days_loaddump_asset_configs()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();

            msg.Title("EstimatedVolume 6","Estimated volumes with cycles spanning days with multiple asset configs. LoadDump asset config");

            var eventArray = new[] {                                                        // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType    |",
            "| CreateAssetEvent | 0         | 03:00:00  | ESTVOL6   | CYC12345     | CAT  | 966G  | 27     | WHEEL LOADER |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);                       // Inject event array into kafka                     
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false,  testSupport.FirstEventDate.AddDays(-2), 111, 999);    
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false,  testSupport.FirstEventDate.AddDays(-1), 222, 999);    
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false,  testSupport.FirstEventDate, 333, 999);    
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false,  testSupport.FirstEventDate.AddDays(1), 444, 999);    
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false,  testSupport.FirstEventDate.AddDays(2), 555, 999);    
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false,  testSupport.FirstEventDate.AddDays(3), 666, 999);    
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false,  testSupport.FirstEventDate.AddDays(4), 2000, 999);   

            var switchEventArray = new[] {  // 333 333 333 444 666
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
            "| SwitchStateEvent | 2         | 00:01:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 2         | 00:01:00  |              |             | 10050.15           | ",
            "| SwitchStateEvent | 3         | 01:30:00  | 5            | SwitchOn    |                    | ",
            "| OdometerEvent    | 3         | 01:30:00  |              |             | 10060.15           | ",
            "| SwitchStateEvent | 3         | 12:00:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 3         | 12:00:00  |              |             | 10070.88           | ",
            "| SwitchStateEvent | 3         | 21:30:00  | 5            | SwitchOn    |                    | ",
            "| OdometerEvent    | 3         | 21:30:00  |              |             | 10099.88           | ",
            "| SwitchStateEvent | 3         | 22:30:00  | 4            | SwitchOff   |                    | ",
            "| OdometerEvent    | 3         | 22:30:00  |              |             | 10112.88           | ",
            "| SwitchStateEvent | 4         | 04:30:00  | 5            | SwitchOn    |                    | ",
            "| OdometerEvent    | 4         | 04:30:00  |              |             | 10119.15           | ",
            };
            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);
            mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);
            mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 11,testSupport.AssetUid);        // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent",11,testSupport.AssetUid);       // Verify the result in the database

            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
            "| ESTVOL6   | CYC12345     | 27        | CAT      | 966G  | 4d+04:30:00      | 5          | 119.15              | 2442             | "
            };

            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }


        [TestMethod]
        public void Estimated_volumes_cycles_span_and_skip_days_loaddump_asset_configs()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();

            msg.Title("EstimatedVolume 7","Estimated volumes with cycles spanning and skipping days with multiple asset configs. LoadDump asset config");

            var eventArray = new[] {                                                        // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType         |",
            "| CreateAssetEvent | 0         | 03:00:00  | ESTVOL7   | CYC12345     | CAT  | 740B  | 31     | ARTICULATED TRUCK |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);                       // Inject event array into kafka                     
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false,  testSupport.FirstEventDate.AddDays(-2), 111, 999);    
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false,  testSupport.FirstEventDate.AddDays(-1), 222, 999);    
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false,  testSupport.FirstEventDate, 333, 999);    
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false,  testSupport.FirstEventDate.AddDays(1), 444, 999);    
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false,  testSupport.FirstEventDate.AddDays(2), 555, 999);    
            testSupport.CreateAssetConfigViaWebApi(5, true, 4, false,  testSupport.FirstEventDate.AddDays(4), 666, 999);    

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
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 11,testSupport.AssetUid);        // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent",11,testSupport.AssetUid);       // Verify the result in the database

            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
            "| ESTVOL7   | CYC12345     | 31        | CAT      | 740B  | 6d+04:30:00      | 5          | 119.15              | 2442             | "
            };

            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
        }
    }
}
