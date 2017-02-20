using System;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using TestUtility.Model.WebApi;

namespace WebApiTests
{
    [TestClass]
    public class ProductFamilyTestsClass
    {
        [TestMethod]
        public void Multiple_day_odometer_with_estimated_volumes_loaddump_config()          
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("ProductFamily 1","Multiple day odometer with estimated volumes. LoadDump asset config. Asset Filtering tested as well");

            var assetType = Guid.NewGuid().ToString();  
            var eventArray = new[] {                                                                // Load events into array  
            " | EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType       |",
            $"| CreateAssetEvent | 0         | 03:00:00  | PRD001    | 12345678     | CAT  | 637G  | 20     | {assetType}     |",
            };

            testSupport.InjectEventsIntoMySqlDatabase(eventArray);                                  // Inject event array into kafka 
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false, testSupport.FirstEventDate.AddDays(-30), 150, 999);                         
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
            testSupport.CreateAssetConfigViaWebApi(4, true, 4, false,  testSupport.FirstEventDate.AddDays(1), 300, 999);   

            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 9,testSupport.AssetUid);         // Verify the result in the database 
            mysql.VerifyTestResultDatabaseRecordCount("OdometerMeterEvent", 9,testSupport.AssetUid);       // Verify the result in the database 

            var expectedAssetSummary = new[] {                                                                  // Load events into array  
            "| assetName | serialNumber | assetIcon | makeCode | model | lastReportedTime | cycleCount | distanceTravelledKm | volumeCubicMeter | ",
            "| PRD001    | 12345678     | 20        | CAT      | 637G  | 2d+21:40:00      | 4          | 40                  | 900              | "
            };

            testSupport.CompareActualAssetCyclesSummaryWithExpected(expectedAssetSummary);

            var baseUri = testSupport.GetBaseUri();
            var uri = string.Format(baseUri + "cycles?startDate={0}&endDate={1}&productFamily={2}", 
                testSupport.FirstEventDate.Date.AddDays(-2).ToString("yyyy-MM-dd"), 
                testSupport.LastEventDate.Date.AddDays(2).ToString("yyyy-MM-dd"),
                assetType.Trim());
            var restClient = new RestClientUtil();
            var response = restClient.DoHttpRequest(uri, "GET", "application/json", null, HttpStatusCode.OK);
            var cycleCountList = JsonConvert.DeserializeObject<AssetCycleSummaryResult>(response);
            if (cycleCountList.assetCycles.Any(x => x.assetUid == testSupport.AssetUid))
            {
                if (!string.IsNullOrEmpty(assetType))
                {
                    Assert.AreEqual(1, cycleCountList.assetCycles.Count, " There should only be one asset since the the product family filter=" + assetType + " for asset:" + testSupport.AssetUid);
                }
                var expectedAssetSummaryObject = testSupport.ConvertArrayToObject<AssetCycleData>(expectedAssetSummary);
                var actualAssetSummary = cycleCountList.assetCycles.First(x => x.assetUid ==testSupport.AssetUid);
                msg.DisplayResults("Expected : " + JsonConvert.SerializeObject(expectedAssetSummary),
                                   "Actual   : " + JsonConvert.SerializeObject( actualAssetSummary));
                Assert.AreEqual(expectedAssetSummaryObject, actualAssetSummary);
            }
            else
            {
                Assert.Fail("Failed to find the asset: " + response);
            }
            
            //----------------------------------------------------------------------------------------------------------------------------------
            // Test the assetcounts?grouping=productfamily . This will return all in the database. At the moment this cannot be checked.

            uri = string.Format(baseUri+ "assetcounts?grouping=productfamily");
            var response2 = restClient.DoHttpRequest(uri, "GET", "application/json", null, HttpStatusCode.OK);
            var countList = JsonConvert.DeserializeObject<AssetCountResult>(response2);
            if (countList == null)
            {
                Assert.Fail("The endpoint " + uri + " did not return any data. It is null");
                return;
            }
            msg.DisplayResults("Expected a number greater than zero", countList.countData.Count + " is the count of the collection");
            if (countList.countData.Count < 1)
            {
                Assert.Fail("The endpoint " + uri + " returned less than 1 in the count");
            }
            //----------------------------------------------------------------------------------------------------------------------------------
        }


        [TestMethod]
        public void Inject_Load_And_Dump_Events_For_2_assets_one_Day_On_different_switch_numbers_configured_switch_On_and_Off_events() 
        {
            var msg = new Msg();
            var testSupport = new TestSupport();
            var mysql = new MySqlHelper();
            msg.Title("ProductFamily 2","One days worth of switch events for two assets. Asset 1 config switch 1 load On and dump Off. Asset 2 config switch 3 load On and dump off. Both on same day at same times.");

            var assetType1 = Guid.NewGuid().ToString(); 
            var assetEventArray = new[] {                                                                   // Load events into array  
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType  |",
            "| CreateAssetEvent | 0         | 09:00:00  | PRD002    | 12345678     | CAT  | 312H  | 24     |" + assetType1 + "|",
            };

            testSupport.InjectEventsIntoMySqlDatabase(assetEventArray);                                             // Inject event array into kafka 
            testSupport.CreateMockAssetConfig(1, true, 1, false);                                           // switch 1 switch on = load , switch 1 off = dump             
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                     // Verify the result in the database      

            var switchEventArray = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState |",
            "| SwitchStateEvent | 0         | 09:00:00  | 1            | SwitchOn    |",
            "| SwitchStateEvent | 0         | 09:05:00  | 1            | SwitchOff   |",
            "| SwitchStateEvent | 0         | 10:00:00  | 1            | SwitchOn    |",
            "| SwitchStateEvent | 0         | 10:05:00  | 1            | SwitchOff   |",
            "| SwitchStateEvent | 0         | 10:30:00  | 1            | SwitchOn    |",
            "| SwitchStateEvent | 0         | 10:35:00  | 1            | SwitchOff   |",
            "| SwitchStateEvent | 0         | 11:30:00  | 1            | SwitchOn    |",
            "| SwitchStateEvent | 0         | 11:35:00  | 1            | SwitchOff   |"
            };
            testSupport.InjectEventsIntoMySqlDatabase(switchEventArray);                                          // Inject event array into kafka 
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 8,testSupport.AssetUid);        // Verify the result in the database 
            var assetType2 = Guid.NewGuid().ToString(); 
            var asset2EventArray = new[] {
            "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType |",
            "| CreateAssetEvent | 0         | 09:00:00  | PRD002-1  | 12345678     | TTT  | 631G  | 22     |" + assetType2 + "|" };
            testSupport.SetAssetUid();
            testSupport.InjectEventsIntoMySqlDatabase(asset2EventArray);                                           // Inject event array into kafka 
            testSupport.CreateMockAssetConfig(3, true, 3, false);                                          // switch 3 switch on = load , switch 3 off = dump             
            mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                    // Verify the result in the database      

            var switchEventArrayAsset2 = new[] {
            "| EventType        | DayOffset | Timestamp | SwitchNumber | SwitchState |",
            "| SwitchStateEvent | 0         | 09:00:00  | 3            | SwitchOn    |",
            "| SwitchStateEvent | 0         | 09:05:00  | 3            | SwitchOff   |",
            "| SwitchStateEvent | 0         | 10:00:00  | 3            | SwitchOn    |",
            "| SwitchStateEvent | 0         | 10:05:00  | 3            | SwitchOff   |",
            "| SwitchStateEvent | 0         | 10:30:00  | 3            | SwitchOn    |",
            "| SwitchStateEvent | 0         | 10:35:00  | 3            | SwitchOff   |",
            "| SwitchStateEvent | 0         | 11:30:00  | 3            | SwitchOn    |",
            "| SwitchStateEvent | 0         | 11:35:00  | 3            | SwitchOff   |"
            };

            testSupport.InjectEventsIntoMySqlDatabase(switchEventArrayAsset2);                                      // Inject event array into kafka 
            mysql.VerifyTestResultDatabaseRecordCount("TimeStampedEvent", 8,testSupport.AssetUid);          // Verify the result in the database   
            // Test the results 
            var baseUri = testSupport.GetBaseUri();
            var uri = string.Format(baseUri + "cycles?startDate={0}&endDate={1}", 
                testSupport.FirstEventDate.Date.AddDays(-2).ToString("yyyy-MM-dd"),
                testSupport.LastEventDate.Date.AddDays(2).ToString("yyyy-MM-dd"));

            var bothFiltersUri = uri + "&productFamily=" + assetType1 + "&productFamily=" + assetType2;
            var restClient = new RestClientUtil();
            var response = restClient.DoHttpRequest(bothFiltersUri, "GET", "application/json", null, HttpStatusCode.OK);
            var cycleCountList = JsonConvert.DeserializeObject<AssetCycleSummaryResult>(response);
                // Total all cycles for filter
            var allCycles = cycleCountList.assetCycles.Aggregate<AssetCycleData, int?>(0, (current, cycles) => current + cycles.cycleCount);
            Assert.AreEqual(8, allCycles, " The cycle count doesn't match for the uri=" + bothFiltersUri + " for asset:" + testSupport.AssetUid );
            // 4 cycles for each asset id
            if (cycleCountList.assetCycles.Any(x => x.assetUid == testSupport.AssetUid))
            {
                var actualCycleCount = cycleCountList.assetCycles.First(x => x.assetUid == testSupport.AssetUid);
                msg.DisplayResults(4 + " cycles", actualCycleCount.cycleCount +  " cycles");
                Assert.AreEqual(4, actualCycleCount.cycleCount);
            }
            else
            {
                msg.DisplayResults(8 + " cycles", " 0 cycles found as asset id not in response from web api");
                Assert.AreEqual(8, 0);
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------
            // Test single asset filter

            var singleFilterUri = uri + "&productFamily=" + assetType2;
            response = restClient.DoHttpRequest(singleFilterUri, "GET", "application/json", null, HttpStatusCode.OK);
            var cycleCountList2 = JsonConvert.DeserializeObject<AssetCycleSummaryResult>(response);
                // Total all cycles for filter
            var allCycles2 = cycleCountList2.assetCycles.Aggregate<AssetCycleData, int?>(0, (current, cycles) => current + cycles.cycleCount);
            Assert.AreEqual(4, allCycles2, " The cycle count doesn't match for the uri=" + bothFiltersUri + " for asset:" + testSupport.AssetUid );

            if (cycleCountList2.assetCycles.Any(x => x.assetUid == testSupport.AssetUid))
            {
                var actualCycleCount = cycleCountList.assetCycles.First(x => x.assetUid == testSupport.AssetUid);
                msg.DisplayResults(4 + " cycles", actualCycleCount.cycleCount +  " cycles");
                Assert.AreEqual(4, actualCycleCount.cycleCount);
            }
            else
            {
                msg.DisplayResults(4 + " cycles", " 0 cycles found as asset id not in response from web api");
                Assert.AreEqual(4, 0);
            }
            //-----------------------------------------------------------------------------------------------------------------------------------------

        }
    }
}
