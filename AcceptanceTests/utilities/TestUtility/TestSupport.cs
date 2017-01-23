using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility.Model.TestEvents;
//using VSS.Project.Service.WebApiModels.Models;
using TestUtility.Model.WebApi;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System.Text.RegularExpressions;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace TestUtility
{
    public class TestSupport
    {
        #region Public Properties

        public string AssetUid { get; set; }
        public DateTime FirstEventDate { get; set; }
        public DateTime LastEventDate { get; set; }
        // public AssetConfigData AssetConfig { get; set; }
        public Guid ProjectUid { get; set; }
        public Guid CustomerUid { get; set; }
        public Guid GeofenceUid { get; set; }

        public CreateProjectEvent CreateProjectEvt { get; set; }
        public UpdateProjectEvent UpdateProjectEvt { get; set; }
        public DeleteProjectEvent DeleteProjectEvt { get; set; }
        public AssociateProjectCustomer AssociateCustomerProjectEvt { get; set; }
        public DissociateProjectCustomer DissociateCustomerProjectEvt { get; set; }
        public AssociateProjectGeofence AssociateProjectGeofenceEvt { get; set; }
    #endregion

    #region Private Properties

    private readonly Random rndNumber = new Random();
        private readonly object syncLock = new object();
        private const char SEPARATOR = '|';
        private readonly TestConfig appConfig = new TestConfig();
        private readonly Msg msg = new Msg();

        #endregion

        #region Public Methods

        public TestSupport()
        {
            SetFirstEventDate();
            SetAssetUid();
            SetProjectUid();
            SetCustomerUid();
            SetGeofenceUid();
        }

    /// <summary>
    /// Set up the first event date for the events to go in
    /// </summary>
    public void SetFirstEventDate()
        {
            FirstEventDate = DateTime.Today.AddDays(-RandomNumber(10, 360));
        }

        /// <summary>
        /// Set the asset UID to a random GUID
        /// </summary>
        public void SetAssetUid()
        {
            AssetUid = Guid.NewGuid().ToString();
        }

    /// <summary>
    /// Set the project UID to a random GUID
    /// </summary>
    public void SetProjectUid()
    {
      ProjectUid = Guid.NewGuid();
    }

    /// <summary>
    /// Set the customer UID to a random GUID
    /// </summary>
    public void SetCustomerUid()
    {
      CustomerUid = Guid.NewGuid();
    }

    /// <summary>
    /// Set the geofence UID to a random GUID
    /// </summary>
    public void SetGeofenceUid()
    {
      GeofenceUid = Guid.NewGuid();
    }

    /// <summary>
    /// Inject all events from the test into kafka
    /// </summary>
    /// <param name="eventArray">all of the events</param>
    public void InjectEventsIntoKafka(string[] eventArray)
        {
            msg.DisplayEventsToConsole(eventArray);
            var allEvents = ConvertArrayToList<MachineEvents>(eventArray);
            WriteAListOfMachineEventsToKafka(allEvents);
            WaitForTimeBasedOnNumberOfRecords(allEvents.Count);
        }

        /// <summary>
        /// Inject events into database for existing asset
        /// </summary>
        /// <param name="eventArray">all of the events</param>
        /// <param name="isSameAsset">Set to false to create a new asset</param>
        public void InjectEventsIntoMySqlDatabase(string[] eventArray, bool isSameAsset = true)
        {
            msg.DisplayEventsForDbInjectToConsole(eventArray);
            var allEvents = ConvertArrayToList<MachineEvents>(eventArray);
            WriteAListOfMachineEventsToMySqlDatabase(allEvents, isSameAsset);
        }

    /// <summary>
    /// Create the project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="projectId">legacy project id</param>
    /// <param name="name">project name</param>
    /// <param name="startDate">project start date</param>
    /// <param name="endDate">project end date</param>
    /// <param name="timezone">project time zone</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void CreateProjectViaWebApi(Guid projectUid, int projectId, string name, DateTime startDate, DateTime endDate, 
      string timezone, DateTime actionUtc, HttpStatusCode statusCode)
    {
      CreateProjectEvt = new CreateProjectEvent
      {
        ProjectID = projectId,
        ProjectUID = projectUid,
        ProjectName = name,
        ProjectType = ProjectType.Standard,
        ProjectBoundary = null,//not used
        ProjectStartDate = startDate,
        ProjectEndDate = endDate,
        ProjectTimezone = timezone,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(CreateProjectEvt, string.Empty, statusCode, "Create");
    }

    /// <summary>
    /// Update the project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="name">project name</param>
    /// <param name="endDate">project end date</param>
    /// <param name="timezone">project time zone</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void UpdateProjectViaWebApi(Guid projectUid, string name, DateTime endDate, string timezone, DateTime actionUtc, HttpStatusCode statusCode)
    {
      UpdateProjectEvt = new UpdateProjectEvent
      {
        ProjectUID = projectUid,
        ProjectName = name,
        ProjectType = ProjectType.Standard,
        ProjectEndDate = endDate,
        ProjectTimezone = timezone,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(UpdateProjectEvt, string.Empty, statusCode, "Update", "PUT");
    }

    /// <summary>
    /// Delete the project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void DeleteProjectViaWebApi(Guid projectUid, DateTime actionUtc, HttpStatusCode statusCode)
    {
      DeleteProjectEvt = new DeleteProjectEvent
      {
        ProjectUID = projectUid,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(DeleteProjectEvt, string.Empty, statusCode, "Delete", "DELETE");
    }

    /// <summary>
    /// Associate a customer and project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="customerUid">customer UID</param>
    /// <param name="customerId">legacy customer ID</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void AssociateCustomerProjectViaWebApi(Guid projectUid, Guid customerUid, int customerId, DateTime actionUtc, HttpStatusCode statusCode)
    {
      AssociateCustomerProjectEvt = new AssociateProjectCustomer
      {
        ProjectUID = projectUid,
        CustomerUID = customerUid,
        LegacyCustomerID = customerId,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(AssociateCustomerProjectEvt, "/AssociateCustomer", statusCode, "Associate customer");
    }

    /// <summary>
    /// Dissociate a customer and project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="customerUid">customer UID</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void DissociateProjectViaWebApi(Guid projectUid, Guid customerUid, DateTime actionUtc, HttpStatusCode statusCode)
    {
      DissociateCustomerProjectEvt = new DissociateProjectCustomer
      {
        ProjectUID = projectUid,
        CustomerUID = customerUid,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(DissociateCustomerProjectEvt, "/DissociateCustomer", statusCode, "Dissociate customer");
    }

    /// <summary>
    /// Associate a geofence and project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="geofenceUid">geofence UID</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void AssociateGeofenceProjectViaWebApi(Guid projectUid, Guid geofenceUid, DateTime actionUtc, HttpStatusCode statusCode)
    {
      AssociateProjectGeofenceEvt = new AssociateProjectGeofence
      {
        ProjectUID = projectUid,
        GeofenceUID = geofenceUid,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(AssociateProjectGeofenceEvt, "/AssociateGeofence", statusCode, "Associate geofence");
    }
    /// <summary>
    /// Call the project web api
    /// </summary>
    /// <param name="evt">THe project event containing the data</param>
    /// <param name="routeSuffix">suffix to add to base uri if required</param>
    /// <param name="statusCode">expected return code of the web api call</param>
    /// <param name="what">name of the api being called for logging</param>
    /// <param name="method">http method</param>
    private void CallProjectWebApi(IProjectEvent evt, string routeSuffix, HttpStatusCode statusCode, string what, string method="POST")
    {
      var configJson = JsonConvert.SerializeObject(evt, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var restClient = new RestClientUtil();
      var response = restClient.DoHttpRequest(GetBaseUri() + routeSuffix, method, "application/json", configJson, statusCode);
      Console.WriteLine(what + " project response:" + response);
    }

    //        /// <summary>
    //        /// Compare the actual results of cycle details to the expected results. The cycle details are a list of cycle data for a date range.
    //        /// </summary>
    //        /// <param name="expectedResultsArray">string array of expected results for cycle details</param>
    //        /// <param name="dayFrom">number of days offset from the start date</param>
    //        /// <param name="dayEnd">number of days offset from the end date</param>
    //        public void CompareActualAssetCycleDetailsWithExpectedResults(string[] expectedResultsArray, int dayFrom = -2, int dayEnd = 2 )
    //        {            
    //            var expectedCycles = ConvertArrayToList<Cycle>(expectedResultsArray);                                     //convert array to list of cycle objects
    //            var baseUri = GetBaseUri();
    //            var uri = string.Format(baseUri + "{0}/cycles?startDate={1}&endDate={2}", AssetUid, FirstEventDate.Date.AddDays(dayFrom).ToString("yyyy-MM-dd"), LastEventDate.Date.AddDays(dayEnd).ToString("yyyy-MM-dd"));
    //            var restClient = new RestClientUtil();
    //            var response = restClient.DoHttpRequest(uri, "GET", "application/json", null, HttpStatusCode.OK);
    //            var assetCycleDetails = JsonConvert.DeserializeObject<AssetCycleDetails>(response);
    //            msg.DisplayResults("Expected cycles :" + JsonConvert.SerializeObject(expectedCycles),
    //                               "Actual from WebApi: " + JsonConvert.SerializeObject( assetCycleDetails));

    //            CollectionAssert.AreEqual(expectedCycles,  assetCycleDetails.cycles);            
    //        }

    //        /// <summary>
    //        /// Compare the actual results of cycle details to the expected results. The cycle details are a list of cycle data for a date range.
    //        /// This test ensures that the summary returned by the endpoint with cycle details is correct.
    //        /// </summary>
    //        /// <param name="expectedResultsArray">string array of expected results for asset summary</param>
    //        /// <param name="dayFrom">number of days offset from the start date</param>
    //        /// <param name="dayEnd">number of days offset from the end date</param>
    //        public void CompareActualAssetDetailsFromCyclesEndpointWithExpectedResults(string[] expectedResultsArray, int dayFrom = -2, int dayEnd = 2 )
    //        {
    //            var expectedSummary = ConvertArrayToObject<AssetCycleData>(expectedResultsArray);                                     //convert array to list of cycle objects
    //            var baseUri = GetBaseUri();
    //            var uri = string.Format(baseUri + "{0}/cycles?startDate={1}&endDate={2}", AssetUid, FirstEventDate.Date.AddDays(dayFrom).ToString("yyyy-MM-dd"), LastEventDate.Date.AddDays(dayEnd).ToString("yyyy-MM-dd"));
    //            var restClient = new RestClientUtil();
    //            var response = restClient.DoHttpRequest(uri, "GET", "application/json", null, HttpStatusCode.OK);
    //            var assetCycleDetails = JsonConvert.DeserializeObject<AssetCycleDetails>(response);
    //            msg.DisplayResults("Expected cycles :" + JsonConvert.SerializeObject(expectedSummary),
    //                               "Actual from WebApi: " + JsonConvert.SerializeObject(assetCycleDetails));

    //            Assert.AreEqual(expectedSummary, assetCycleDetails.asset);
    //        }

    //        /// <summary>
    //        /// Compare the Asset in the string array to the one in a list of assets. The asset that is being compared is searched for in the list.  
    //        /// </summary>
    //        /// <param name="eventAssetCycles">Asset cycle summary </param>
    //        public void CompareActualAssetCyclesSummaryWithExpected(string[] eventAssetCycles)
    //        {
    //            var expectedAssetSummary = ConvertArrayToObject<AssetCycleData>(eventAssetCycles);
    //            var baseUri = GetBaseUri();
    //            var uri = string.Format(baseUri + "cycles?startDate={0}&endDate={1}", FirstEventDate.Date.AddDays(-2).ToString("yyyy-MM-dd"),LastEventDate.Date.AddDays(2).ToString("yyyy-MM-dd"));
    //            var restClient = new RestClientUtil();
    //            var response = restClient.DoHttpRequest(uri, "GET", "application/json", null, HttpStatusCode.OK);
    //            var assetCycleSummaryDetails = JsonConvert.DeserializeObject<AssetCycleSummaryResult>(response);
    //            if (assetCycleSummaryDetails.assetCycles.Any(x => x.assetUid == AssetUid))
    //            {
    //                var actualAssetSummary = assetCycleSummaryDetails.assetCycles.First(x => x.assetUid == AssetUid);
    //                msg.DisplayResults("Expected : " + JsonConvert.SerializeObject(expectedAssetSummary),
    //                                   "Actual   : " + JsonConvert.SerializeObject( actualAssetSummary));
    //                Assert.AreEqual(expectedAssetSummary, actualAssetSummary);
    //            }
    //        }

    //        /// <summary>
    //        /// Verify the asset count for in the web api 
    //        /// </summary>
    //        public void VerifyAssetCountInWebApi(string productFamily, int expectedAssetCount)
    //        {
    //            var baseUri = GetBaseUri();
    //            var uri = string.Format(baseUri + "assetcounts?{0}",productFamily);
    //            var restClient = new RestClientUtil();
    //            var response = restClient.DoHttpRequest(uri, "GET", "application/json", null, HttpStatusCode.OK);
    //            var actualAssetCountList = JsonConvert.DeserializeObject<AssetCountResult>(response);
    //            if (actualAssetCountList.countData.Count == 1)
    //            {
    //                msg.DisplayResults(expectedAssetCount + " assets", actualAssetCountList.countData[0].Count + " assets");
    //                Assert.AreEqual(expectedAssetCount, actualAssetCountList.countData[0].Count, "Expected Product family does not match actual");
    //                return;
    //            }
    //            if (actualAssetCountList.countData.Count > 1)
    //            {
    //                var totalCount = actualAssetCountList.countData.Sum(countData => countData.Count);
    //                msg.DisplayResults(expectedAssetCount + " assets", totalCount + " assets");
    //                Assert.AreEqual(expectedAssetCount, totalCount, "All expected Product family counts do not match actual count total");
    //            }
    //            Assert.Fail("The expected count is " + expectedAssetCount + " and the acutal is " + actualAssetCountList.countData.Count);
    //        }

    //        #region Create Asset Config methods
    //        /// <summary>
    //        /// Create the asset config via the web api. This always creates the config in the past.
    //        /// </summary>
    //        /// <param name="loadSwitch">load switch number</param>
    //        /// <param name="isLoad">true or false depending on what you want a load to be</param>
    //        /// <param name="dumpSwitch">dump switch number</param>
    //        /// <param name="isDump">true or false depending on what you want a dump to be</param>
    //        /// <param name="assetConfigDate"></param>
    //        /// <param name="volumePerCycleCubicMeter"></param>
    //        /// <param name="targetCyclesPerDay"></param>
    //        public void CreateAssetConfigViaWebApi(int loadSwitch, bool isLoad, int dumpSwitch, bool isDump, DateTime? assetConfigDate = null, int volumePerCycleCubicMeter = 100, int targetCyclesPerDay = 10  )
    //        {
    //            if (assetConfigDate == null)
    //            {
    //                assetConfigDate = FirstEventDate.AddDays(-5);
    //            }
    //            AssetConfig = new AssetConfigData
    //            {
    //                assetIdentifier = AssetUid,
    //                startDate = assetConfigDate,
    //                loadSwitchNumber = loadSwitch,
    //                loadSwitchOpen = isLoad,
    //                dumpSwitchNumber = dumpSwitch,
    //                dumpSwitchOpen = isDump,
    //                targetCyclesPerDay = targetCyclesPerDay,
    //                volumePerCycleCubicMeter = volumePerCycleCubicMeter,
    //#pragma warning disable 612
    //                allowPastConfig = true
    //#pragma warning restore 612
    //            };
    //            var configJson = JsonConvert.SerializeObject(AssetConfig, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
    //            var baseUri = GetBaseUri();
    //            var uri = baseUri + string.Format("asset/{0}", AssetUid); 
    //            var restClient = new RestClientUtil();
    //            var response = restClient.DoHttpRequest(uri, "POST", "application/json", configJson, HttpStatusCode.OK);
    //            Console.WriteLine("Create config response:" + response);
    //       }

    //        /// <summary>
    //        /// Create the asset config via the web api. This always creates the config in the past.
    //        /// </summary>
    //        /// <param name="loadSwitch">load switch number</param>
    //        /// <param name="isLoad">true or false depending on what you want a load to be</param>
    //        /// <param name="assetConfigDate"></param>
    //        /// <param name="volumePerCycleCubicMeter"></param>
    //        /// <param name="targetCyclesPerDay"></param>         
    //        public void CreateAssetConfigViaWebApiLoadOnly(int loadSwitch, bool isLoad, DateTime? assetConfigDate = null, int volumePerCycleCubicMeter = 100, int targetCyclesPerDay = 10)
    //        {
    //            if (assetConfigDate == null)
    //            {
    //                assetConfigDate = FirstEventDate.AddDays(-5);
    //            }
    //            AssetConfig = new AssetConfigData
    //            {
    //                assetIdentifier = AssetUid,
    //                startDate = assetConfigDate,
    //                loadSwitchNumber = loadSwitch,
    //                loadSwitchOpen = isLoad,
    //                targetCyclesPerDay = targetCyclesPerDay,
    //                volumePerCycleCubicMeter = volumePerCycleCubicMeter,
    //#pragma warning disable 612
    //                allowPastConfig = true
    //#pragma warning restore 612
    //            };
    //            var configJson = JsonConvert.SerializeObject(AssetConfig, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
    //            var baseUri = GetBaseUri();
    //            var uri = baseUri + string.Format("asset/{0}", AssetUid); 
    //            var restClient = new RestClientUtil();
    //            var response = restClient.DoHttpRequest(uri, "POST", "application/json", configJson, HttpStatusCode.OK);
    //            Console.WriteLine("Create config response:" + response);
    //        }

    //        /// <summary>
    //        /// Create the asset config via the web api. This always creates the config in the past.
    //        /// </summary>
    //        /// <param name="dumpSwitch">dump switch number</param>
    //        /// <param name="isDump">true or false depending on what you want a dump to be</param>
    //        /// <param name="assetConfigDate"></param>
    //        /// <param name="volumePerCycleCubicMeter"></param>
    //        /// <param name="targetCyclesPerDay"></param>          
    //        public void CreateAssetConfigViaWebApiDumpOnly(int dumpSwitch, bool isDump, DateTime? assetConfigDate = null, int volumePerCycleCubicMeter = 100, int targetCyclesPerDay = 10  )
    //        {
    //            if (assetConfigDate == null)
    //            {
    //                assetConfigDate = FirstEventDate.AddDays(-5);
    //            }
    //            AssetConfig = new AssetConfigData
    //            {
    //                assetIdentifier = AssetUid,
    //                startDate = assetConfigDate,
    //                dumpSwitchNumber = dumpSwitch,
    //                dumpSwitchOpen = isDump,
    //                targetCyclesPerDay = targetCyclesPerDay,
    //                volumePerCycleCubicMeter = volumePerCycleCubicMeter,
    //#pragma warning disable 612
    //                allowPastConfig = true
    //#pragma warning restore 612
    //            };
    //            var configJson = JsonConvert.SerializeObject(AssetConfig, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
    //            var baseUri = GetBaseUri();
    //            var uri = baseUri + string.Format("asset/{0}", AssetUid); 
    //            var restClient = new RestClientUtil();
    //            var response = restClient.DoHttpRequest(uri, "POST", "application/json", configJson, HttpStatusCode.OK);
    //            Console.WriteLine("Create config response:" + response);
    //        }

    //        /// <summary>
    //        /// Inject the MockAssetConfig
    //        /// </summary>
    //        /// <param name="loadSwitch">Load switch number</param>
    //        /// <param name="isLoad">True if the switch state on then it is a Load. If set to false and switch state is off then it is a load</param>
    //        /// <param name="dumpSwitch">Dump switch number</param>
    //        /// <param name="isDump">True if the switch state on then it is a Dump. If set to false and switch state is off then it is a dump</param>
    //        public void CreateMockAssetConfig(int loadSwitch, bool isLoad, int dumpSwitch, bool isDump)
    //        {
    //            AssetConfig = new AssetConfigData
    //            {
    //              assetIdentifier = AssetUid,
    //              startDate = FirstEventDate,
    //              loadSwitchNumber = loadSwitch,
    //              loadSwitchOpen = isLoad,
    //              dumpSwitchNumber = dumpSwitch,
    //              dumpSwitchOpen = isDump,
    //              targetCyclesPerDay = 1000,
    //              volumePerCycleCubicMeter = 100,
    //#pragma warning disable 612
    //              allowPastConfig = true
    //#pragma warning restore 612
    //            };
    //            var startKeyDate = KeyDate(FirstEventDate);
    //            var query = $@"INSERT INTO `{appConfig.dbSchema}`.{"AssetConfiguration"} 
    //                        (AssetUID,StartKeyDate,LoadSwitchNumber,LoadSwitchWorkStartState,DumpSwitchNumber,DumpSwitchWorkStartState,TargetCyclesPerDay,VolumePerCycleCubicMeter) VALUES
    //                        ('{AssetUid}',{startKeyDate},{AssetConfig.loadSwitchNumber},{AssetConfig.loadSwitchOpen},{AssetConfig.dumpSwitchNumber},{AssetConfig.dumpSwitchOpen},{AssetConfig.targetCyclesPerDay},{AssetConfig.volumePerCycleCubicMeter});";
    //            var mysqlHelper = new MySqlHelper();
    //            mysqlHelper.ExecuteMySqlInsert(appConfig.dbConnectionString, query);            
    //        }

    //        /// <summary>
    //        /// Inject the MockAssetConfig
    //        /// </summary>
    //        /// <param name="loadSwitch">Load switch number</param>
    //        /// <param name="isLoad">True if the switch state on then it is a Load. If set to false and switch state is off then it is a load</param>
    //        public void CreateMockAssetConfigLoadOnly(int loadSwitch, bool isLoad)
    //        {
    //            AssetConfig = new AssetConfigData
    //            {
    //                assetIdentifier = AssetUid,
    //                startDate = FirstEventDate,
    //                loadSwitchNumber = loadSwitch,
    //                loadSwitchOpen = isLoad,
    //                targetCyclesPerDay = 1000,
    //                volumePerCycleCubicMeter = 100,
    //#pragma warning disable 612
    //                allowPastConfig = true
    //#pragma warning restore 612
    //            };
    //            var startKeyDate = KeyDate(FirstEventDate);
    //            var query = $@"INSERT INTO `{appConfig.dbSchema}`.{"AssetConfiguration"} 
    //                        (AssetUID,StartKeyDate,LoadSwitchNumber,LoadSwitchWorkStartState,TargetCyclesPerDay,VolumePerCycleCubicMeter) VALUES
    //                        ('{AssetUid}',{startKeyDate},{AssetConfig.loadSwitchNumber},{AssetConfig.loadSwitchOpen},{AssetConfig.targetCyclesPerDay},{AssetConfig.volumePerCycleCubicMeter});";

    //            var mysqlHelper = new MySqlHelper();
    //            mysqlHelper.ExecuteMySqlInsert(appConfig.dbConnectionString, query);            
    //        }

    //        /// <summary>
    //        /// Inject the MockAssetConfig
    //        /// </summary>
    //        /// <param name="dumpSwitch">Dump switch number</param>
    //        /// <param name="isDump">True if the switch state on then it is a Dump. If set to false and switch state is off then it is a dump</param>
    //        public void CreateMockAssetConfigDumpOnly(int dumpSwitch, bool isDump)
    //        {
    //            AssetConfig = new AssetConfigData
    //            {
    //                assetIdentifier = AssetUid,
    //                startDate = FirstEventDate,
    //                dumpSwitchNumber = dumpSwitch,
    //                dumpSwitchOpen = isDump,
    //                targetCyclesPerDay = 1000,
    //                volumePerCycleCubicMeter = 100,
    //#pragma warning disable 612
    //                allowPastConfig = true
    //#pragma warning restore 612
    //            };
    //            var startKeyDate = KeyDate(FirstEventDate);
    //            var query = $@"INSERT INTO `{appConfig.dbSchema}`.{"AssetConfiguration"} 
    //                        (AssetUID,StartKeyDate,DumpSwitchNumber,DumpSwitchWorkStartState,TargetCyclesPerDay,VolumePerCycleCubicMeter) VALUES
    //                        ('{AssetUid}',{startKeyDate},{AssetConfig.dumpSwitchNumber},{AssetConfig.dumpSwitchOpen},{AssetConfig.targetCyclesPerDay},{AssetConfig.volumePerCycleCubicMeter});";

    //            var mysqlHelper = new MySqlHelper();
    //            mysqlHelper.ExecuteMySqlInsert(appConfig.dbConnectionString, query);            
    //        }


    /// <summary>
    /// Check if the test is being debugged in VS. Set to different endpoind
    /// </summary>
    /// <returns></returns>
    public string GetBaseUri()
        {
            var baseUri = appConfig.webApiUri;
            if (Debugger.IsAttached)
            {
                baseUri = appConfig.debugWebApiUri;
            }
            return baseUri;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Publish event to kafka
        /// </summary>
        /// <param name="allEvents">List of events in a inputEvent class format</param>
        private void WriteAListOfMachineEventsToKafka(List<MachineEvents> allEvents)
        {
            var kafkaDriver = new RdKafkaDriver();
            foreach (var singleEvent in allEvents)
            {
                var dayOffSet = Convert.ToInt32(singleEvent.DayOffset);
                var eventDate = FirstEventDate.AddDays(dayOffSet) + DateTime.ParseExact(singleEvent.Timestamp, "HH:mm:ss", CultureInfo.InvariantCulture).TimeOfDay;
                var eventUtc = eventDate; // eventUtc is eventdate without offset applied
                var deviceTime = singleEvent.UtcOffsetHours == "nullOffset" ? null : DateTimeExtensions.ToIso8601DateTime(eventUtc, Convert.ToDouble(singleEvent.UtcOffsetHours));
                LastEventDate = eventDate; // Always set the event date to be the last one. Assume the go in sequential order. 
                var jsonSettings = new JsonSerializerSettings {DateTimeZoneHandling = DateTimeZoneHandling.Unspecified};
                var topicName = appConfig.masterDataTopic + singleEvent.EventType + appConfig.kafkaTopicSuffix;

                switch (singleEvent.EventType)
                {
                    #region "Customer Events"
                    case "CreateCustomerEvent":
                        var createCustomerEvent = new CreateCustomerEvent()
                        {
                            ActionUTC = eventUtc,
                            ReceivedUTC = eventUtc,
                            CustomerName = singleEvent.CustomerName,
                            CustomerType = singleEvent.CustomerType,
                            CustomerUID = new Guid(singleEvent.CustomerUID)                            
                        };
                        kafkaDriver.SendKafkaMessage(topicName,JsonConvert.SerializeObject(createCustomerEvent,jsonSettings));
                        break;
                    case "UpdateCustomerEvent":
                        var updateCustomerEvent = new UpdateCustomerEvent()
                        {
                            ActionUTC = eventUtc,
                            ReceivedUTC = eventUtc,
                            CustomerName = singleEvent.CustomerName,
                            CustomerUID = new Guid(singleEvent.CustomerUID)                            
                        };
                        kafkaDriver.SendKafkaMessage(topicName,JsonConvert.SerializeObject(updateCustomerEvent,jsonSettings));
                        break;
                    case "DeleteCustomerEvent":
                        var deleteCustomerEvent = new DeleteCustomerEvent()
                        {
                            ActionUTC = eventUtc,
                            ReceivedUTC = eventUtc,
                            CustomerUID = new Guid(singleEvent.CustomerUID)                            
                        };
                        kafkaDriver.SendKafkaMessage(topicName,JsonConvert.SerializeObject(deleteCustomerEvent,jsonSettings));
                        break;
                    case "AssociateCustomerUserEvent":
                        var associateCustomerUserEvent = new AssociateCustomerUserEvent()
                        {
                            ActionUTC = eventUtc,
                            ReceivedUTC = eventUtc,
                            CustomerUID = new Guid(singleEvent.CustomerUID),
                            UserUID = new Guid(singleEvent.UserUID)                                                      
                        };
                        kafkaDriver.SendKafkaMessage(topicName,JsonConvert.SerializeObject(associateCustomerUserEvent,jsonSettings));
                        break;
                    #endregion

                    case "CreateProjectEvent":
                        topicName = appConfig.masterDataTopic + "IProjectEvent" + appConfig.kafkaTopicSuffix;
                        var createProjectEvent = new CreateProjectEvent()
                        {
                            ActionUTC = eventUtc,
                            ReceivedUTC = eventUtc,
                            ProjectBoundary = singleEvent.ProjectBoundary,
                            //ProjectEndDate = singleEvent.ProjectEndDate,
                            //ProjectID = singleEvent.ProjectID,
                            //ProjectName =singleEvent.ProjectName,
                            //ProjectStartDate = singleEvent.ProjectStartDate,
                            //ProjectTimezone = singleEvent.ProjectTimezone,
                            //ProjectType = singleEvent.ProjectType,
                            //ProjectUID = singleEvent.ProjectUID                          
                        };
                        kafkaDriver.SendKafkaMessage(topicName,JsonConvert.SerializeObject(createProjectEvent,jsonSettings));
                        break;


                    #region CreateAssetEvent
                    case "CreateAssetEvent":
                        var createAssetEvent = new CreateAssetEvent()
                        {
                            ActionUTC = eventUtc,
                            AssetUID = new Guid(AssetUid),
                            AssetName = singleEvent.AssetName,
                            AssetType = singleEvent.AssetType,
                            SerialNumber = singleEvent.SerialNumber,
                            MakeCode = singleEvent.Make,
                            Model = singleEvent.Model,
                            IconKey = Convert.ToInt32(singleEvent.IconId)
                        };
                        kafkaDriver.SendKafkaMessage(appConfig.masterDataTopic + "IAssetEvent" + appConfig.kafkaTopicSuffix,
                            JsonConvert.SerializeObject(new { CreateAssetEvent = createAssetEvent }, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified }));
                        break;
                    #endregion

                    #region UpdateAssetEvent
                    case "UpdateAssetEvent":
                        var updateAssetEvent = new UpdateAssetEvent()
                        {
                            ActionUTC = eventUtc,
                            AssetUID = new Guid(AssetUid)
                        };
                        if (!string.IsNullOrEmpty(singleEvent.AssetName))
                        {
                            updateAssetEvent.AssetName = singleEvent.AssetName;
                        }
                        if (!string.IsNullOrEmpty(singleEvent.AssetType))
                        {
                            updateAssetEvent.AssetType = singleEvent.AssetType;
                        }
                        if (!string.IsNullOrEmpty(singleEvent.Model))
                        {
                            updateAssetEvent.Model = singleEvent.Model;
                        }
                        if (!string.IsNullOrEmpty(singleEvent.IconId))
                        {
                            updateAssetEvent.IconKey = Convert.ToInt32(singleEvent.IconId);
                        }
                        kafkaDriver.SendKafkaMessage(appConfig.masterDataTopic + "IAssetEvent" + appConfig.kafkaTopicSuffix,
                            JsonConvert.SerializeObject(new { UpdateAssetEvent = updateAssetEvent }, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified }));
                        break;
                    #endregion

                    #region DeleteAssetEvent
                    case "DeleteAssetEvent":
                        var deleteAssetEvent = new DeleteAssetEvent()
                        {
                            ActionUTC = eventUtc,
                            AssetUID = new Guid(AssetUid),
                        };
                        kafkaDriver.SendKafkaMessage(appConfig.masterDataTopic + "IAssetEvent" + appConfig.kafkaTopicSuffix,
                            JsonConvert.SerializeObject(new { DeleteAssetEvent = deleteAssetEvent }, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified }));
                        break;
                        #endregion
                }
            }
        }

        /// <summary>
        /// Inset the events into the database 
        /// </summary>
        /// <param name="allEvents">List of all the events</param>
        /// <param name="isSameAsset">Is all the same asset</param>
        private void WriteAListOfMachineEventsToMySqlDatabase(List<MachineEvents> allEvents, bool isSameAsset = true)
        {
            foreach (var singleEvent in allEvents)
            {
                // This is used to set multiple asset events in the database
                if (!isSameAsset)
                {
                    SetAssetUid();
                    SetFirstEventDate();
                   // CreateMockAssetConfigLoadOnly(1, true);
                }

                // Set the dates. Note: Timestamp from the test scenarios is always eventUtc. The device time is then calculated from eventUtc and local offset or location event offset applied.
                var dayOffSet = Convert.ToInt32(singleEvent.DayOffset);
                var eventDate = FirstEventDate.AddDays(dayOffSet) + DateTime.ParseExact(singleEvent.Timestamp, "HH:mm:ss", CultureInfo.InvariantCulture).TimeOfDay;
                var eventUtc = eventDate;
                //var deviceTime = singleEvent.UtcOffsetHours == "null" ? null : DateTimeExtensions.ToIso8601DateTime(eventUtc, Convert.ToDouble(singleEvent.UtcOffsetHours));
                var eventKeyDate = KeyDate(eventDate);
                LastEventDate = eventDate; // Always set the event date to be the last one. Assume the go in sequential order. 
                var mysqlHelper = new MySqlHelper();
                switch (singleEvent.EventType)
                {
                    //#region SwitchStateEvent
                    //case "SwitchStateEvent":
                    //    var eventType = GetTimeStampedEventTypeFromAssetConfig(singleEvent);
                    //    var switchStatequery = $@"INSERT INTO `{appConfig.dbSchema}`.{"TimeStampedEvent"} 
                    //                        (AssetUID,fk_EventTypeID,EventUTC,EventDeviceTime,EventKeyDate) VALUES
                    //                        ('{AssetUid}',{eventType},'{eventUtc:yyyy-MM-dd HH\:mm\:ss.fffffff}','{eventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}',{eventKeyDate});";
                    //    mysqlHelper.ExecuteMySqlInsert(appConfig.dbConnectionString, switchStatequery);
                    //    break;
                    //#endregion

                    //#region OdometerEvent
                    //case "OdometerEvent":
                    //    var odometerMeter = singleEvent.OdometerKilometers;
                    //    var odometerEventQuery = $@"INSERT INTO `{appConfig.dbSchema}`.{"OdometerMeterEvent"} 
                    //                        (AssetUID,OdometerMeter,EventUTC,EventDeviceTime,EventKeyDate) VALUES
                    //                        ('{AssetUid}',{odometerMeter},'{eventUtc:yyyy-MM-dd HH\:mm\:ss.fffffff}','{eventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}',{eventKeyDate});";
                    //    mysqlHelper.ExecuteMySqlInsert(appConfig.dbConnectionString, odometerEventQuery);
                    //    break;
                    //#endregion

                    //#region LocationEvent
                    //case "LocationEvent":

                    //    break;
                    //#endregion

                    #region CreateAssetEvent
                    case "CreateAssetEvent":
                        var createAssetEventQuery = $@"INSERT INTO `{appConfig.dbSchema}`.{"Asset"} 
                                            (AssetUID,Name,MakeCode,SerialNumber,Model,IconKey,AssetType,LastActionedUTC) VALUES
                                            ('{AssetUid}','{singleEvent.AssetName}','{singleEvent.Make}','{singleEvent.SerialNumber}','{singleEvent.Model}',{singleEvent.IconId},'{singleEvent.AssetType}','{eventUtc:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
                        mysqlHelper.ExecuteMySqlInsert(appConfig.dbConnectionString, createAssetEventQuery);
                        break;
                        #endregion
                }
            }
        }

        //        /// <summary>
        //        /// Work out if the event is a load or a switch event
        //        /// </summary>
        //        /// <param name="singleEvent">Internal event type of MachineEvents</param>
        //        /// <returns>100 or 101 depending on the event type</returns>
        //        private int GetTimeStampedEventTypeFromAssetConfig(MachineEvents singleEvent)
        //        {
        //            var switchNo = Convert.ToInt32(singleEvent.SwitchNumber);
        //            if (AssetConfig.loadSwitchNumber == switchNo || AssetConfig.dumpSwitchNumber == switchNo)
        //            {
        //                if (AssetConfig.loadSwitchOpen == true && singleEvent.SwitchState == "SwitchOn")
        //                {
        //                    return 100;
        //                }
        //                if (AssetConfig.dumpSwitchOpen == true && singleEvent.SwitchState == "SwitchOn")
        //                {
        //                    return 101;
        //                }     
        //                if (AssetConfig.loadSwitchOpen == false && singleEvent.SwitchState == "SwitchOff")
        //                {
        //                    return 100;
        //                }
        //                if (AssetConfig.dumpSwitchOpen == false && singleEvent.SwitchState == "SwitchOff")
        //                {
        //                    return 101;
        //                }                                
        //            }
        //            return 100;
        //        }


        /// <summary>
        /// Convert a string array of the type used for tests to a list of objects.
        /// </summary>
        /// <param name="objectDetailsArray">Array containing the details for each object, first row is header, subsequent rows are objects one object per row</param>
        /// <returns>List of objects</returns>
        private List<T> ConvertArrayToList<T>(IEnumerable<string> objectDetailsArray)
        {
            var allObjects = new List<T>();
            var allColumnNames = string.Empty;
            var rowCnt = 0;
            foreach (var row in objectDetailsArray)
            {
                rowCnt++;
                var rowValues = row.Split(SEPARATOR);
                if (rowCnt == 1)
                {
                    allColumnNames = row;
                    continue;
                }
                var obj = Activator.CreateInstance<T>();
                SetObjectPropertyValues(obj, rowValues, allColumnNames);
                allObjects.Add(obj);
            }
            return allObjects;
        }


        /// <summary>
        /// Convert a string array of the type used for tests to an object.
        /// </summary>
        /// <param name="objectDetailsArray">Two elment array, first row is header second is data.</param>
        /// <returns>object</returns>
        public T ConvertArrayToObject<T>(IEnumerable<string> objectDetailsArray)
        {
            var obj = Activator.CreateInstance<T>();

            if (objectDetailsArray.Count() == 2)
            {
                SetObjectPropertyValues(obj, objectDetailsArray.ElementAt(1).Split(SEPARATOR), objectDetailsArray.ElementAt(0));
            }

            return obj;
        }


        /// <summary>
        /// Set properties of an object with the values from the corresponding column of the string array
        /// </summary>
        /// <param name="obj">object we are wanting to set the properties of </param>
        /// <param name="dataRow">row of data</param>
        /// <param name="header">column headers</param>
        private void SetObjectPropertyValues<T>(T obj, string[] dataRow, string header)
        {
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                int idx;
                idx = GetColumnIndex(header, property.Name.Trim());
                obj = SetPropertyValue(obj, property, dataRow, idx);
            }
        }


        /// <summary>
        /// Set the property value for a specific field with the value from the string array
        /// </summary>
        /// <param name="obj"> class. The class we are setting the properties for </param>
        /// <param name="property">One field in PropertyInfo format</param>
        /// <param name="dataRow">One row of data from the string array</param>
        /// <param name="idx">index of where the field is in the string array</param>
        /// <returns>cycle with values in the properties</returns>
        private T SetPropertyValue<T>(T obj, PropertyInfo property, string[] dataRow, int idx)
        {
            if (idx == -1 || dataRow[idx].Trim() == "na")
            {
                return obj;
            }
            else if (dataRow[idx].Trim() == "null" || dataRow[idx].Trim() == "")
            {
                property.SetValue(obj, null);
            }
            else if (Nullable.GetUnderlyingType(property.PropertyType) == typeof(DateTime))
            {
                //Check to see if this is one of our special datetimes with offset included
                if (Regex.IsMatch(dataRow[idx], @"^\s*\d+d\+\d+"))
                {
                    //This is a date column and its offset is defined as the number prior to the 'd+' in the string
                    String[] components = Regex.Split(dataRow[idx], @"d+\+");
                    var offset = Double.Parse(components[0].Trim());
                    DateTime dateTime = DateTime.Parse(FirstEventDate.AddDays(offset).ToString("yyyy-MM-dd") + " " + components[1].Trim());
                    property.SetValue(obj, dateTime);
                }
                else //treat as normal date
                {
                    DateTime dateTime = DateTime.Parse(dataRow[idx].Trim());
                    property.SetValue(obj, dateTime);
                }
            }
            else if (property.PropertyType == typeof(string) && Regex.IsMatch(dataRow[idx], @"^\s*\d+d\+\d+"))
            {
                //This is a date column and its offset is defined as the number prior to the 'd+' in the string
                String[] components = Regex.Split(dataRow[idx], @"d+\+");
                var offset = Double.Parse(components[0].Trim());
                DateTime dateTime = DateTime.Parse(FirstEventDate.AddDays(offset).ToString("yyyy-MM-dd") + " " + components[1].Trim());
                property.SetValue(obj, dateTime.ToString(CultureInfo.InvariantCulture));
            }
            else //Work out the type and set the value.
            {
                Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                object value = (dataRow[idx].Trim() == null) ? null : Convert.ChangeType(dataRow[idx].Trim(), t);
                property.SetValue(obj, value, null);
            }
            return obj;
        }


        /// <summary>
        /// Get the index of where the column header is in the array
        /// </summary>
        /// <param name="header">string with all the column headers in it</param>
        /// <param name="columnName">specific column name we are trying to locate</param>
        /// <returns>index in the string array this is</returns>
        private int GetColumnIndex(string header, string columnName)
        {
            string[] headerValues = header.Replace(" ", "").Split(SEPARATOR);
            var idx = Array.FindIndex(headerValues, s => s.Equals(columnName));
            return idx;
        }

        /// <summary>
        /// Generate a random number. This is use for the number of days in the past to get a start date from.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private int RandomNumber(int min, int max)
        {
            lock (syncLock)
            {
                return rndNumber.Next(min, max);
            }
        }

        /// <summary>
        /// Get the keydate from the datetime
        /// </summary>
        /// <param name="eventDate"></param>
        /// <returns></returns>
        public int KeyDate(DateTime eventDate)
        {
            return (eventDate.Year * 10000) + (eventDate.Month * 100) + (eventDate.Day);
        }

        /// <summary>
        /// Wait for time to let the data feed process
        /// </summary>
        /// <param name="count"></param>
        private void WaitForTimeBasedOnNumberOfRecords(int count)
        {
            if (count < 7)
            { Thread.Sleep(1000); }
            if (count > 6)
            { Thread.Sleep(1000); }
            if (count > 13)
            { Thread.Sleep(1000); }
        }

        #endregion
    }
}
