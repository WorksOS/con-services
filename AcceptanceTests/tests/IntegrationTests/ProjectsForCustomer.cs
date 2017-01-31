using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System.Net;

namespace IntegrationTests
{
  [TestClass]
  public class ProjectsForCustomer
  {
    private readonly string projectDBSchemaName = "ProjectConsumers-VSS-MasterData-Project";

    [TestMethod]
    public void Create_Project_And_Inject_Required_Data_Then_Retrieve()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.updateDBSchemaName(projectDBSchemaName);
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Integration Test Project 1";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");


      var customerEventArray = new[] {
             "| EventType           | EventDate   | CustomerName | CustomerType | CustomerUID   |",
            $"| CreateCustomerEvent | 0d+09:00:00 | CustName     | Customer     | {customerGuid} |"};

      testSupport.InjectEventsIntoKafka(customerEventArray); //Create customer to associate project with

      testSupport.CreateProjectViaWebApi(projectGuid, 100, projectName, startDate,
      endDate, "New Zealand Standard Time", ProjectType.Standard, DateTime.UtcNow, HttpStatusCode.OK);

      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid); //check that project is in webapi db
      //projectConsumerMysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid); //check that project is in consumer db

      testSupport.AssociateCustomerProjectViaWebApi(projectGuid, customerGuid, 1, DateTime.UtcNow, HttpStatusCode.OK);

      //projectConsumerMysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID",
      //  "fk_CustomerUID, fk_ProjectUID", //Fields
      //  $"{customerGuid}, {projectGuid}", //Expected
      //  projectGuid);

      mysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID",
        "fk_CustomerUID, fk_ProjectUID", //Fields
        $"{customerGuid}, {projectGuid}", //Expected
        projectGuid);


      var expectedProjects = new string[] {
            "| IsArchived | Name          | ProjectTimeZone           | ProjectType            | StartDate   | EndDate   | ProjectUid    | LegacyProjectId | ",
           $"| false      | {projectName} | New Zealand Standard Time | {ProjectType.Standard} | {startDate} | {endDate} | {projectGuid} | 100             |" };

      testSupport.GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode.OK, customerGuid, expectedProjects);

      //msg.Title("Create Project test 14", "Create one project");
      //var projectEventArray = new[] {
      //  "| EventType          | EventDate   | ProjectID | ProjectUID    | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate |" ,
      // $"| CreateProjectEvent | 0d+09:00:00 | 1         | {projectGuid} | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      |"};

      //testSupport.InjectEventsIntoKafka(projectEventArray);

      //var associateEventArray = new[] {
      //  "| EventType                | EventDate   | ProjectUID    | CustomerUID    | ",
      // $"| AssociateProjectCustomer | 0d+09:00:00 | {projectGuid} | {customerGuid} | "};


      //testSupport.InjectEventsIntoKafka(associateEventArray);
      //Verify project has been associated

    }
  }










  //[TestMethod]
  //public void Inject_Load_And_Dump_Events_For_Two_Days_Create_Asset_With_Asset_And_Config()
  //{
  //    var msg = new Msg();
  //    var testSupport = new TestSupport();
  //    var mysql = new MySqlHelper();
  //    msg.Title("Integration cycle details 1", "Two days worth of switch events to kafka for same switch. Create an asset config. ");

  //    var eventArray = new[] {                                                                  // Load events into array  
  //    "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId |",
  //    "| CreateAssetEvent | 0         | 09:00:00  | INTCYCDT1 | DET05073     | CAT  | 980H  | 27     |"
  //    };

  //    testSupport.InjectEventsIntoKafka(eventArray);                                 // Inject event array into kafka         
  //    testSupport.CreateAssetConfigViaWebApi(5, true, 5, false);                                // switch 5 switch on = load , switch off = dump    
  //    mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);               // Verify the result in the database      

  //    var switchEventArray = new[] {
  //    "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState |",
  //    "| SwitchStateEvent | 0         | 09:00:00  | 5            | SwitchOn    |",
  //    "| SwitchStateEvent | 0         | 10:00:00  | 5            | SwitchOff   |",
  //    "| SwitchStateEvent | 0         | 10:30:00  | 5            | SwitchOn    |",
  //    "| SwitchStateEvent | 0         | 11:30:00  | 5            | SwitchOff   |",
  //    "| SwitchStateEvent | 1         | 09:00:00  | 5            | SwitchOn    |",
  //    "| SwitchStateEvent | 1         | 10:00:00  | 5            | SwitchOff   |",
  //    "| SwitchStateEvent | 1         | 10:30:00  | 5            | SwitchOn    |",
  //    "| SwitchStateEvent | 1         | 11:30:00  | 5            | SwitchOff   |"
  //    };

  //    testSupport.InjectEventsIntoKafka(switchEventArray);                          // Inject event array into kafka 
  //    mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 8,testSupport.AssetUid);    // Verify the result in the database 
  //    var expectedAssetSummary = new[] {                                                                  // Load events into array  
  //    "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | volumeCubicMeter | ",
  //    "| INTCYCDT1 | DET05073     | 27        | CAT      | 980H  | 1d+11:30:00      | 4          | 400              | "
  //    };

  //    testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
  //    testSupport.CompareActualAssetDetailsFromCyclesEndpointWithExpectedResults(expectedAssetSummary);


  //    var expectedResultsArray = new[] {
  //    "| startCycleDeviceTime | endCycleDeviceTime   | dumpDeviceTime | volumePerCycleCubicMeter | odometerStartCycleValue | odometerEndCycleValue | odometerDumpCycleValue | distanceTravelled |",
  //    "| 0d+09:00:00          | 0d+10:30:00          | 0d+10:00:00    | 100                      | null                    | null                  | null                   | null              |",
  //    "| 0d+10:30:00          | 1d+09:00:00          | 0d+11:30:00    | 100                      | null                    | null                  | null                   | null              |",
  //    "| 1d+09:00:00          | 1d+10:30:00          | 1d+10:00:00    | 100                      | null                    | null                  | null                   | null              |",
  //    "| 1d+10:30:00          | null                 | 1d+11:30:00    | 100                      | null                    | null                  | null                   | null              |"
  //    };
  //    testSupport.CompareActualAssetCycleDetailsWithExpectedResults(expectedResultsArray,-2, 2);
  //}


  //[TestMethod]
  //public void First_load_cycle_has_no_distance_for_first_cycle_load_only_config()
  //{
  //    var msg = new Msg();
  //    var testSupport = new TestSupport();
  //    var mysql = new MySqlHelper();

  //    msg.Title("Integration cycle details 2", "First load cycle has no distance travelled for first cycle. Load only config");

  //    var eventArray = new[] {                                                        // Load events into array  
  //    "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId |",
  //    "| CreateAssetEvent | 0         | 03:00:00  | INTCYCDT2 | CYC12345     | CAT  | 312H  | 27     |"
  //    };

  //    testSupport.InjectEventsIntoKafka(eventArray);                       // Inject event array into kafka 
  //    testSupport.CreateAssetConfigViaWebApiLoadOnly(1, true);                       // switch 1 on = load            
  //    mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      

  //    var switchEventArray = new[] {
  //    "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
  //    "| SwitchStateEvent | 0         | 09:00:00  | 1            | SwitchOn    |                    | ",
  //    "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000              | ",
  //    "| SwitchStateEvent | 0         | 10:30:00  | 1            | SwitchOn    |                    | ",
  //    "| OdometerEvent    | 0         | 10:30:00  |              |             | 10010              | ",
  //    "| SwitchStateEvent | 0         | 12:00:00  | 1            | SwitchOn    |                    | ",
  //    "| OdometerEvent    | 0         | 12:00:00  |              |             | 10020              | ",
  //    };

  //    testSupport.InjectEventsIntoKafka(switchEventArray);              // Inject event array into kafka 
  //    mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 3,testSupport.AssetUid);       // Verify the result in the database 
  //    mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 3,testSupport.AssetUid);     // Verify the result in the database 
  //    var expectedResultsArray = new[] {
  //    "| startCycleDeviceTime | endCycleDeviceTime | dumpDeviceTime | volumePerCycleCubicMeter | odometerStartCycleValue | odometerEndCycleValue | odometerDumpCycleValue | distanceTravelled |",
  //    "| null                 | 0d+09:00:00        | null           | 100                      | null                    | 10000                 | null                   | null              |",
  //    "| 0d+09:00:00          | 0d+10:30:00        | null           | 100                      | 10000                   | 10010                 | null                   | 10                |",
  //    "| 0d+10:30:00          | 0d+12:00:00        | null           | 100                      | 10010                   | 10020                 | null                   | 10                |",
  //    };
  //    testSupport.CompareActualAssetCycleDetailsWithExpectedResults(expectedResultsArray,-2, 2);

  //}

  //[TestMethod]
  //public void Estimated_volumes_With_future_asset_config_loaddump_config()
  //{
  //    var msg = new Msg();
  //    var testSupport = new TestSupport();
  //    var mysql = new MySqlHelper();
  //    msg.Title("Integration Cycle details 3", "Cycle details with estimated volumes, distance and cycles. LoadDump asset config. With short cycle ");

  //    var eventArray = new[] {                                                                                            // Load events into array  
  //    "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType    |",
  //    "| CreateAssetEvent | 0         | 03:00:00  | CYCDET3   | CYC12345     | CAT  | 994H  | 22     | TRACK LOADER |"
  //    };

  //    testSupport.InjectEventsIntoKafka(eventArray);                                                  // Inject event array into kafka 
  //    testSupport.CreateAssetConfigViaWebApi(4, true, 4, false, testSupport.FirstEventDate.AddDays(-5), 333, 999);
  //    mysql.VerifyTestResultDatabaseRecordCount("Asset", 1, testSupport.AssetUid);                                        // Verify the result in the database      

  //    var switchEventArray = new[] {
  //    "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState | OdometerKilometers | ",
  //    "| SwitchStateEvent | 0         | 09:00:00  | 4            | SwitchOn    |                    | ",
  //    "| OdometerEvent    | 0         | 09:00:00  |              |             | 10000.15           | ",
  //    "| SwitchStateEvent | 0         | 12:30:00  | 4            | SwitchOff   |                    | ",
  //    "| OdometerEvent    | 0         | 12:30:00  |              |             | 10010.15           | ",
  //    "| SwitchStateEvent | 0         | 16:00:00  | 4            | SwitchOn    |                    | ",
  //    "| OdometerEvent    | 0         | 16:00:00  |              |             | 10020.15           | ",
  //    "| SwitchStateEvent | 0         | 23:00:00  | 4            | SwitchOff   |                    | ",
  //    "| OdometerEvent    | 0         | 23:00:00  |              |             | 10030.15           | ",
  //    "| SwitchStateEvent | 1         | 04:30:00  | 4            | SwitchOn    |                    | ",
  //    "| OdometerEvent    | 1         | 04:30:00  |              |             | 10040.15           | ",
  //    "| SwitchStateEvent | 1         | 12:01:00  | 4            | SwitchOff   |                    | ",
  //    "| OdometerEvent    | 1         | 12:01:00  |              |             | 10050.15           | ",
  //    "| SwitchStateEvent | 2         | 01:30:00  | 4            | SwitchOn    |                    | ",
  //    "| OdometerEvent    | 2         | 01:30:00  |              |             | 10060.15           | ",
  //    "| SwitchStateEvent | 2         | 12:00:00  | 4            | SwitchOff   |                    | ",
  //    "| OdometerEvent    | 2         | 12:00:00  |              |             | 10070.88           | ",
  //    "| SwitchStateEvent | 2         | 21:30:15  | 4            | SwitchOn    |                    | ",
  //    "| OdometerEvent    | 2         | 21:30:15  |              |             | 10099.15           | ",
  //    "| SwitchStateEvent | 2         | 21:31:15  | 4            | SwitchOff   |                    | ",
  //    "| OdometerEvent    | 2         | 21:31:15  |              |             | 10199.15           | ",
  //    "| SwitchStateEvent | 2         | 21:32:00  | 4            | SwitchOn    |                    | ",
  //    "| OdometerEvent    | 2         | 21:32:16  |              |             | 10299.15           | "
  //    };
  //    testSupport.InjectEventsIntoKafka(switchEventArray);

  //    mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 11, testSupport.AssetUid);              // Verify the result in the database 
  //    mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 11, testSupport.AssetUid);            // Verify the result in the database 
  //    var expectedAssetSummary = new[] {                                                                  // Load events into array  
  //    "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
  //    "| CYCDET3   | CYC12345     | 22        | CAT      | 994H  | 2d+21:32:16      | 5          | 299                 | 1665             | "
  //    };
  //    testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);
  //    testSupport.CompareActualAssetDetailsFromCyclesEndpointWithExpectedResults(expectedAssetSummary);


  //    // Now verify the cycle details
  //    var expectedCycleResults = new[] {
  //    "| startCycleDeviceTime | endCycleDeviceTime | dumpDeviceTime | volumePerCycleCubicMeter | odometerStartCycleValue | odometerEndCycleValue | odometerDumpCycleValue | distanceTravelled | cycleLengthMinutes | cycleReportedDeviceTime |",
  //    "| 0d+09:00:00          | 0d+16:00:00        | 0d+12:30:00    | 333                      | 10000.15                | 10020.15              | 10010.15               | 20                | 420                | 0d+16:00:00             |",
  //    "| 0d+16:00:00          | 1d+04:30:00        | 0d+23:00:00    | 333                      | 10020.15                | 10040.15              | 10030.15               | 20                | 750                | 1d+04:30:00             |",
  //    "| 1d+04:30:00          | 2d+01:30:00        | 1d+12:01:00    | 333                      | 10040.15                | 10060.15              | 10050.15               | 20                | 1260               | 2d+01:30:00             |",
  //    "| 2d+01:30:00          | 2d+21:30:15        | 2d+12:00:00    | 333                      | 10060.15                | 10099.15              | 10070.88               | 39                | 1200.25            | 2d+21:30:15             |",
  //    "| 2d+21:30:15          | 2d+21:32:00        | 2d+21:31:15    | 333                      | 10099.15                | 10299.15              | 10199.15               | 200               | 1.75               | 2d+21:32:00             |",
  //    };
  //    testSupport.CompareActualAssetCycleDetailsWithExpectedResults(expectedCycleResults, -2, 2);
  //}


}
