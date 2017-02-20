using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace WebApiTests
{
  [TestClass]
  public class CyclesPerHourTests
  {
    [TestMethod]
    public void Single_asset_with_multiple_cycles_and_runtime_hours_with_callout()          
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();

      msg.Title("cycles per hour 1","Single asset with multiple cycles and runtime hours with callout");

      testSupport.AssetUid = "378e1ee8-1f21-e311-9ee2-00505688274d";
      mysql.DeleteAllRecordsForAnAsset(testSupport.AssetUid);

      var eventArray = new[] {                                                                        
      "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType    |",
      "| CreateAssetEvent | 0         | 03:00:00  | CPH1      | BTX00366     | CAT  | 834H  | 25     | TRACK LOADER |"
      };

      testSupport.InjectEventsIntoMySqlDatabase(eventArray);                              
      testSupport.CreateAssetConfigViaWebApi(4, true, 4, false, testSupport.FirstEventDate.AddDays(-5), 333, 999);                         
      mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                                       

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

      mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 9,testSupport.AssetUid);                         
      mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 9,testSupport.AssetUid);                      

      var expectedAssetSummary = new[] {                                                                  
      "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | cyclesPerHr |",
      "| CPH1      | BTX00366     | 25        | CAT      | 834H  | 2d+21:40:00      | 4          | 40                  | 1332             | null        |"
      };

      testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary, -366,5);
    }

    [TestMethod]
    public void Single_asset_with_multiple_cycles_and_runtime_hours_with_no_callout()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();

      msg.Title("cycles per hour 2", "Single asset with multiple cycles and runtime hours with no callout");

      testSupport.AssetUid = "e85871e8-1f21-e311-9ee2-00505688274d";
      mysql.DeleteAllRecordsForAnAsset(testSupport.AssetUid);

      var eventArray = new[] {
      "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType    |",
      "| CreateAssetEvent | 0         | 03:00:00  | CPH2      | A7D00742     | CAT  | 972H  | 27     | TRACK        |"
      };

      testSupport.InjectEventsIntoMySqlDatabase(eventArray);
      testSupport.CreateAssetConfigViaWebApi(4, true, 4, false, testSupport.FirstEventDate.AddDays(-5), 333, 999);
      mysql.VerifyTestResultDatabaseRecordCount("Asset", 1, testSupport.AssetUid);

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
      mysql.CreateAssetUtcOffset(0, testSupport.AssetUid);
      mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);

      mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 9, testSupport.AssetUid);
      mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 9, testSupport.AssetUid);

      var expectedAssetSummary = new[] {
      "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | cyclesPerHr |",
      "| CPH2      | A7D00742     | 27        | CAT      | 972H  | 2d+21:40:00      | 4          | 40                  | 1332             | 0.002974    |"
      };

      testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary, -366, 5);
    }


    [TestMethod]
    public void Single_asset_with_dump_only_config_multiple_cycles_and_runtime_hours_with_no_callout()          
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();

      msg.Title("cycles per hour 3", "Single asset with dump only config multiple cycles and runtime hours with no callout");

      testSupport.AssetUid = "acca5ce8-1f21-e311-9ee2-00505688274d";
      mysql.DeleteAllRecordsForAnAsset(testSupport.AssetUid);

      var eventArray = new[] {                                                                        
      "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
      "| CreateAssetEvent | 0         | 03:00:00  | CPH3      | B9H00452     | CAT  | 16M   | 14     | TRUCK     |"
      };
      testSupport.InjectEventsIntoMySqlDatabase(eventArray);                              
      testSupport.CreateAssetConfigViaWebApiDumpOnly(4, true, testSupport.FirstEventDate.AddDays(-5));                         
      mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);     
      mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);                                  

      var switchEventArray = new[] {
      "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
      "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 0         | 09:10:00  |              |             | 10000              | ",
      "| SwitchStateEvent | 0         | 12:30:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 0         | 12:30:00  |              |             | 10010              | ",
      "| SwitchStateEvent | 0         | 16:00:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 0         | 16:00:00  |              |             | 10020              | ",
      "| SwitchStateEvent | 0         | 23:00:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 0         | 23:00:00  |              |             | 10030              | ",
      "| SwitchStateEvent | 1         | 04:30:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 1         | 04:30:00  |              |             | 10040              | ",
      "| SwitchStateEvent | 1         | 12:01:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 1         | 12:01:00  |              |             | 10050              | ",
      "| SwitchStateEvent | 2         | 01:30:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 2         | 01:30:00  |              |             | 10060              | ",
      "| SwitchStateEvent | 2         | 12:10:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 2         | 12:10:00  |              |             | 10070              | ",
      "| SwitchStateEvent | 2         | 21:30:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 2         | 21:30:00  |              |             | 10099              | "
      };

      testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);
      mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);

      mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 9,testSupport.AssetUid);                         
      mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 9,testSupport.AssetUid);                      

      var expectedAssetSummary = new[] {                                                                  
      "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | cyclesPerHr |",
      "| CPH3      | B9H00452     | 14        | CAT      | 16M   | 2d+21:30:00      | 9          | 89                  | 900              | 0.214286    |"
      };

      testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary, -366,5);
    }

    [TestMethod]
    public void Single_asset_dump_only_config_with_hundreds_of_cycles_and_runtime_hours_with_no_callout()          
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();

      msg.Title("cycles per hour 4", "Single asset dump only config with hundreds of cycles and runtime hours with no callout");

      testSupport.AssetUid = "782348e8-1f21-e311-9ee2-00505688274d";
      testSupport.FirstEventDate = DateTime.Now.Date.AddYears(-1);
      mysql.DeleteAllRecordsForAnAsset(testSupport.AssetUid);

      var eventArray = new[] {                                                                        
      "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
      "| CreateAssetEvent | 0         | 03:00:00  | CPH4      | BXY02816     | CAT  | 988H  | 27     | TRUCK     |"
      };
      testSupport.InjectEventsIntoMySqlDatabase(eventArray);                              
      testSupport.CreateAssetConfigViaWebApiDumpOnly(4, true, testSupport.FirstEventDate.AddDays(-5));                         
      mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);     
      mysql.CreateAssetUtcOffset(0,testSupport.AssetUid);                                  

      var switchEventArray = new[] {
      "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | ",
      "| SwitchStateEvent | 0         | 08:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 10:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 11:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 12:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 13:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 14:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 15:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 16:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 17:00:00  | 4            | SwitchOn    | "
      };

      for (var noTimes = 0; noTimes < 100; noTimes++)
      {
          testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);
          testSupport.FirstEventDate = testSupport.FirstEventDate.AddDays(1);
      }
            
      mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
      mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 1000,testSupport.AssetUid);                                        

      var expectedAssetSummary = new[] {                                                                  
      "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | volumeCubicMeter | cyclesPerHr |",  // runtimeHours=1744 
      "| CPH4      | BXY02816     | 27        | CAT      | 988H  | 1000       | 100000.0         | 0.573394    |"   // * 1000 = 0.573394
      };

      testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary, -366, 5);
    }

    [TestMethod]
    public void Single_asset_with_load_only_config_multiple_cycles_and_runtime_hours_with_no_callout()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();

      msg.Title("cycles per hour 5", "Single asset with load only config multiple cycles and runtime hours with no callout");

      testSupport.AssetUid = "acca5ce8-1f21-e311-9ee2-00505688274d";
      mysql.DeleteAllRecordsForAnAsset(testSupport.AssetUid);

      var eventArray = new[] {
      "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
      "| CreateAssetEvent | 0         | 03:00:00  | CPH5      | B9H00452     | CAT  | 16M   | 14     | TRUCK     |"
      };
      testSupport.InjectEventsIntoMySqlDatabase(eventArray);
      testSupport.CreateAssetConfigViaWebApiLoadOnly(4, true, testSupport.FirstEventDate.AddDays(-5));
      mysql.VerifyTestResultDatabaseRecordCount("Asset", 1, testSupport.AssetUid);
      mysql.CreateAssetUtcOffset(0, testSupport.AssetUid);

      var switchEventArray = new[] {
      "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
      "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 0         | 09:10:00  |              |             | 10000              | ",
      "| SwitchStateEvent | 0         | 12:30:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 0         | 12:30:00  |              |             | 10010              | ",
      "| SwitchStateEvent | 0         | 16:00:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 0         | 16:00:00  |              |             | 10020              | ",
      "| SwitchStateEvent | 0         | 23:00:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 0         | 23:00:00  |              |             | 10030              | ",
      "| SwitchStateEvent | 1         | 04:30:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 1         | 04:30:00  |              |             | 10040              | ",
      "| SwitchStateEvent | 1         | 12:01:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 1         | 12:01:00  |              |             | 10050              | ",
      "| SwitchStateEvent | 2         | 01:30:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 2         | 01:30:00  |              |             | 10060              | ",
      "| SwitchStateEvent | 2         | 12:10:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 2         | 12:10:00  |              |             | 10070              | ",
      "| SwitchStateEvent | 2         | 21:30:00  | 4            | SwitchOn    |                    | ",
      "| OdometerEvent    | 2         | 21:30:00  |              |             | 10099              | "
      };

      testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);
      mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);

      mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 9, testSupport.AssetUid);
      mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 9, testSupport.AssetUid);

      var expectedAssetSummary = new[] {
      "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | cyclesPerHr |",
      "| CPH5      | B9H00452     | 14        | CAT      | 16M   | 2d+21:30:00      | 9          | 89                  | 900              | 0.214286    |"
      };

      testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary, -366, 5);
    }

    [TestMethod]
    public void Single_asset_load_only_config_with_hundreds_of_cycles_and_runtime_hours_with_no_callout()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();

      msg.Title("cycles per hour 6", "Single asset load only config with hundreds of cycles and runtime hours with no callout");

      testSupport.AssetUid = "782348e8-1f21-e311-9ee2-00505688274d";
      testSupport.FirstEventDate = DateTime.Now.Date.AddYears(-1);
      mysql.DeleteAllRecordsForAnAsset(testSupport.AssetUid);

      var eventArray = new[] {
      "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
      "| CreateAssetEvent | 0         | 03:00:00  | CPH6      | BXY02816     | CAT  | 988H  | 27     | TRUCK     |"
      };
      testSupport.InjectEventsIntoMySqlDatabase(eventArray);
      testSupport.CreateAssetConfigViaWebApiLoadOnly(4, true, testSupport.FirstEventDate.AddDays(-5));
      mysql.VerifyTestResultDatabaseRecordCount("Asset", 1, testSupport.AssetUid);
      mysql.CreateAssetUtcOffset(0, testSupport.AssetUid);

      var switchEventArray = new[] {
      "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | ",
      "| SwitchStateEvent | 0         | 08:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 10:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 11:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 12:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 13:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 14:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 15:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 16:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 17:00:00  | 4            | SwitchOn    | "
      };

      for (var noTimes = 0; noTimes < 100; noTimes++)
      {
        testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);
        testSupport.FirstEventDate = testSupport.FirstEventDate.AddDays(1);
      }

      mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
      mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 1000, testSupport.AssetUid);

      var expectedAssetSummary = new[] {
      "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | volumeCubicMeter | cyclesPerHr |",  // runtimeHours=1744 
      "| CPH6      | BXY02816     | 27        | CAT      | 988H  | 1000       | 100000.0         | 0.573394    |"   // * 1000 = 0.573394
      };

      testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary, -366, 5);
    }

    [TestMethod]
    public void Single_asset_dump_only_config_with_hundreds_of_cycles_and_runtime_hours_with_callout()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();

      msg.Title("cycles per hour 7", "Single asset dump only config with hundreds of cycles and runtime hours with callout");

      testSupport.AssetUid = "c2d45ce8-1f21-e311-9ee2-00505688274d";
      testSupport.FirstEventDate = DateTime.Now.Date.AddYears(-1);
      mysql.DeleteAllRecordsForAnAsset(testSupport.AssetUid);

      var eventArray = new[] {
      "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
      "| CreateAssetEvent | 0         | 03:00:00  | CPH7      | JMS02860     | CAT  | 980H  | 0      | TRUCK     |"
      };
      testSupport.InjectEventsIntoMySqlDatabase(eventArray);
      testSupport.CreateAssetConfigViaWebApiDumpOnly(4, true, testSupport.FirstEventDate.AddDays(-5));
      mysql.VerifyTestResultDatabaseRecordCount("Asset", 1, testSupport.AssetUid);
      mysql.CreateAssetUtcOffset(0, testSupport.AssetUid);

      var switchEventArray = new[] {
      "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | ",
      "| SwitchStateEvent | 0         | 08:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 10:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 11:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 12:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 13:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 14:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 15:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 16:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 17:00:00  | 4            | SwitchOn    | "
      };

      for (var noTimes = 0; noTimes < 100; noTimes++)
      {
        testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);
        testSupport.FirstEventDate = testSupport.FirstEventDate.AddDays(1);
      }

      mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
      mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 1000, testSupport.AssetUid);

      var expectedAssetSummary = new[] {
      "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | volumeCubicMeter | cyclesPerHr |",  // runtimeHours=1744 
      "| CPH7      | JMS02860     | 0         | CAT      | 980H  | 1000       | 100000.0         | null        |"   // MultipleDayDelta callout
      };

      testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary, -366, 5);
    }

    [TestMethod]
    public void Single_asset_load_only_config_with_hundreds_of_cycles_and_runtime_hours_with_callout()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();

      msg.Title("cycles per hour 8", "Single asset load only config with hundreds of cycles and runtime hours with callout");

      testSupport.AssetUid = "c2d45ce8-1f21-e311-9ee2-00505688274d";
      testSupport.FirstEventDate = DateTime.Now.Date.AddYears(-1);
      mysql.DeleteAllRecordsForAnAsset(testSupport.AssetUid);

      var eventArray = new[] {
      "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
      "| CreateAssetEvent | 0         | 03:00:00  | CPH8      | JMS02860     | CAT  | 980H  | 0      | TRUCK     |"
      };
      testSupport.InjectEventsIntoMySqlDatabase(eventArray);
      testSupport.CreateAssetConfigViaWebApiLoadOnly(4, true, testSupport.FirstEventDate.AddDays(-5));
      mysql.VerifyTestResultDatabaseRecordCount("Asset", 1, testSupport.AssetUid);
      mysql.CreateAssetUtcOffset(0, testSupport.AssetUid);

      var switchEventArray = new[] {
      "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | ",
      "| SwitchStateEvent | 0         | 08:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 10:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 11:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 12:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 13:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 14:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 15:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 16:00:00  | 4            | SwitchOn    | ",
      "| SwitchStateEvent | 0         | 17:00:00  | 4            | SwitchOn    | "
      };

      for (var noTimes = 0; noTimes < 100; noTimes++)
      {
        testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);
        testSupport.FirstEventDate = testSupport.FirstEventDate.AddDays(1);
      }

      mysql.CreateDateLastReported(testSupport.AssetUid, testSupport.LastEventDate);
      mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 1000, testSupport.AssetUid);

      var expectedAssetSummary = new[] {
      "| assetName | serialNumber | assetIcon | makeCode | model | cycleCount | volumeCubicMeter | cyclesPerHr |",  // runtimeHours=1744 
      "| CPH8      | JMS02860     | 0         | CAT      | 980H  | 1000       | 100000.0         | null        |"   // MultipleDayDelta callout
      };

      testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary, -366, 5);
    }
  }
}
