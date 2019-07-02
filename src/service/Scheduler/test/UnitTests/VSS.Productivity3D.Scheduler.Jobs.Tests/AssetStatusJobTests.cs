using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity.Push.Models;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
using VSS.Productivity3D.AssetMgmt3D.Abstractions.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Push.Abstractions.AssetLocations;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Jobs.AssetWorksManagerJob;

namespace VSS.Productivity3D.Scheduler.Jobs.Tests
{
  [TestClass]
  public class AssetStatusJobTests
  {
    const string EXPECTED_RAPTOR_MACHINE_ROUTE_FORMAT = "/projects/{0}/machines";

    // This is defined else where, but storing locally for testing, as if we call the same definition as the code and it changes, then we'll never know
    const double RADIANS_TO_DEGREES = 180.0 / Math.PI; 

    private ILoggerFactory loggerFactory;
    private IServiceProvider serviceProvider;

    // These are all the mocks we need to create the job
    private Mock<IAssetStatusServerHubClient> mockAssetStatusServerHubClient = new Mock<IAssetStatusServerHubClient>();
    private Mock<IFleetAssetDetailsProxy> mockFleetAssetDetails = new Mock<IFleetAssetDetailsProxy>();
    private Mock<IFleetAssetSummaryProxy> mockAssetSummaryProxy = new Mock<IFleetAssetSummaryProxy>();
    private Mock<IRaptorProxy> mockRaptorProxy = new Mock<IRaptorProxy>();
    private Mock<IAssetResolverProxy> mockAssetResolverProxy = new Mock<IAssetResolverProxy>();

    [TestInitialize]
    public void TestInitialize()
    {
      var services = new ServiceCollection();
      serviceProvider = services
        .AddLogging()
        .AddSingleton<IAssetStatusServerHubClient>(mockAssetStatusServerHubClient.Object)
        .AddSingleton<IFleetAssetDetailsProxy>(mockFleetAssetDetails.Object)
        .AddSingleton<IFleetAssetSummaryProxy>(mockAssetSummaryProxy.Object)
        .AddSingleton<IRaptorProxy>(mockRaptorProxy.Object)
        .AddSingleton<IAssetResolverProxy>(mockAssetResolverProxy.Object)
        .AddTransient<IJob, AssetStatusJob>() // This is the class we are testing
        .BuildServiceProvider();

      mockAssetStatusServerHubClient.Reset();
      mockFleetAssetDetails.Reset();
      mockAssetSummaryProxy.Reset();
      mockRaptorProxy.Reset();
      mockAssetResolverProxy.Reset();

      loggerFactory = serviceProvider.GetService<ILoggerFactory>();
    }


    [TestMethod]
    public void TestCreation()
    {
      var job = serviceProvider.GetService<IJob>();
      Assert.IsNotNull(job);
      Assert.IsInstanceOfType(job, typeof(AssetStatusJob));
      Assert.AreEqual(AssetStatusJob.VSSJOB_UID, job.VSSJobUid);
    }


    [TestMethod]
    public void TestConnection()
    {
      var job = serviceProvider.GetService<IJob>();
      Assert.IsNotNull(job);

      // When we are connecting, we shouldn't attempt to connect again
      mockAssetStatusServerHubClient.Setup(m => m.IsConnecting).Returns(true);
      mockAssetStatusServerHubClient.Setup(m => m.Connected).Returns(false);

      // Validate no connects
      job.Setup(null).Wait();
      mockAssetStatusServerHubClient.Verify(m => m.Connect(), Times.Never);
      
      // Now we are connected, ensure we don't connect
      mockAssetStatusServerHubClient.Setup(m => m.IsConnecting).Returns(false);
      mockAssetStatusServerHubClient.Setup(m => m.Connected).Returns(true);

      // Validate no connects
      job.Setup(null).Wait();
      mockAssetStatusServerHubClient.Verify(m => m.Connect(), Times.Never);


      // Now we aren't connected or connecting, so we should try to connect
      mockAssetStatusServerHubClient.Setup(m => m.IsConnecting).Returns(false);
      mockAssetStatusServerHubClient.Setup(m => m.Connected).Returns(false);
      mockAssetStatusServerHubClient.Setup(m => m.Connect()).Returns(Task.CompletedTask);

      job.Setup(null).Wait();
      mockAssetStatusServerHubClient.Verify(m => m.Connect(), Times.Once);
    }


    /// <summary>
    /// Test Case: Each subscription (driven from the UI normally) will be processed via the Asset Job
    /// </summary>
    [TestMethod]
    public void TestProcessingSubscriptions()
    {
      var job = serviceProvider.GetService<IJob>();
      Assert.IsNotNull(job);

      var subscription1 = new AssetUpdateSubscriptionModel()
      {
        ProjectUid = Guid.Parse("4A856F8C-08D7-4C6E-B6BF-6852658E6A72"),
        CustomerUid = Guid.Parse("A66D7657-59FC-4C56-8E92-495AADEFD819"),
        AuthorizationHeader = "TEST AUTH",
        JWTAssertion = "TEST JWT"
      };

      var subscription2 = new AssetUpdateSubscriptionModel()
      {
        ProjectUid = Guid.Parse("5DADE1FA-7FAF-4822-93D9-DAE9CFC9BA77"),
        CustomerUid = Guid.Parse("E04F38AD-44C7-4C8B-95E3-F597E4A1C205"),
        AuthorizationHeader = "TEST AUTH",
        JWTAssertion = "TEST JWT"
      };

      Assert.AreNotEqual(subscription1.ProjectUid, subscription2.ProjectUid, "Mock subscriptions must be different");
      Assert.AreNotEqual(subscription1.CustomerUid, subscription2.CustomerUid, "Mock subscriptions must be different");

      var subscriptions = new List<AssetUpdateSubscriptionModel> {subscription1, subscription2};

      // We will ensure that we get the correct subscriptions, and process each subscription
      mockAssetStatusServerHubClient.Setup(m => m.GetSubscriptions()).Returns(Task.FromResult(subscriptions));

      // The first step is to call out to 3dp for each subscription, ensure we do this (with the correct headers)
      mockRaptorProxy.Setup(m =>
        m.ExecuteGenericV2Request<Machine3DStatuses>(
          It.IsAny<string>(),
          It.IsAny<HttpMethod>(),
          It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>()))
        .Returns(Task.FromResult(new Machine3DStatuses()));

      // Execute the call to the job
      job.Run(null).Wait();

      // Now we should have called out for the subscriptions, and for each one called out to raptor with the correct project UID and headers
      // First Confirm the subscriptions
      mockAssetStatusServerHubClient.Verify(m => m.GetSubscriptions(), Times.Once);

      var subscription1Headers = GetExpectedHeaders(subscription1.CustomerUid, subscription1.AuthorizationHeader, subscription1.JWTAssertion);

      var subscription1Route = string.Format(EXPECTED_RAPTOR_MACHINE_ROUTE_FORMAT, subscription1.ProjectUid);

      mockRaptorProxy.Verify(m => m.ExecuteGenericV2Request<Machine3DStatuses>(
          It.Is<string>(s => s == subscription1Route),
          It.IsAny<HttpMethod>(),
          It.IsAny<Stream>(),
          It.Is<IDictionary<string, string>>(d => DictionaryContentEquals(d, subscription1Headers))),
        Times.Once);

      // Ensure the second subscription was called too
      var subscription2Headers = GetExpectedHeaders(subscription2.CustomerUid, subscription2.AuthorizationHeader, subscription2.JWTAssertion);
      var subscription2Route = string.Format(EXPECTED_RAPTOR_MACHINE_ROUTE_FORMAT, subscription2.ProjectUid);

      mockRaptorProxy.Verify(m => m.ExecuteGenericV2Request<Machine3DStatuses>(
          It.Is<string>(s => s == subscription2Route),
          It.IsAny<HttpMethod>(),
          It.IsAny<Stream>(),
          It.Is<IDictionary<string, string>>(d => DictionaryContentEquals(d, subscription2Headers))),
        Times.Once);

      // And confirm there are no other calls to the request
      mockRaptorProxy.Verify(m => m.ExecuteGenericV2Request<Machine3DStatuses>(
          It.IsAny<string>(),
          It.IsAny<HttpMethod>(),
          It.IsAny<Stream>(),
          It.IsAny<IDictionary<string, string>>()),
        Times.Exactly(2));
    }

    /// <summary>
    /// Test Case: Each 3d machine return from 3dp for a subscription shall we processed, and trigger an event to the UI
    /// </summary>
    [TestMethod]
    public void TestProcessing3dAssetsOnly()
    {
      var job = serviceProvider.GetService<IJob>();
      Assert.IsNotNull(job);
      
      var subscription = new AssetUpdateSubscriptionModel()
      {
        ProjectUid = Guid.Parse("038A3ACB-C985-4E3A-AB98-FF3C939B30BF"),
        CustomerUid = Guid.Parse("12DCFAF5-96A4-4E27-BE53-28A226A7AAB6"),
        AuthorizationHeader = "TEST AUTH",
        JWTAssertion = "TEST JWT"
      };
      
      var machine1 = new MachineStatus(123, 
        "Test Machine1", 
        true, 
        "LAST DESIGN 1", 
        6433, 
        new DateTime(2010,1,2,3,4,5, DateTimeKind.Utc),
        0.59915074193701334, // radians
        -1.470376021323053, // radians
        null,
        null);

      var machineResult = new Machine3DStatuses(ContractExecutionStatesEnum.ExecutedSuccessfully);
      machineResult.MachineStatuses.Add(machine1);

      var expectedResult = new AssetAggregateStatus()
      {
        AssetIdentifier =  "Test Machine1",
        LiftNumber = 6433,
        CustomerUid = subscription.CustomerUid,
        ProjectUid = subscription.ProjectUid,
        LocationLastUpdatedUtc = machine1.lastKnownTimeStamp,
        AssetUid = null,
        Design = machine1.lastKnownDesignName,
        FuelLevel = null,
        FuelLevelLastUpdatedUtc = null,
        Latitude = machine1.lastKnownLatitude * RADIANS_TO_DEGREES,
        Longitude = machine1.lastKnownLongitude * RADIANS_TO_DEGREES,
        MachineName = "Test Machine1"
      };

      var expectedHeaders = GetExpectedHeaders(subscription.CustomerUid, subscription.AuthorizationHeader, subscription.JWTAssertion);
      AssetAggregateStatus resultStatus = null; // Keep a reference to the event generated for comparasion

      // Setup the returns
      mockAssetStatusServerHubClient
        .Setup(m => m.GetSubscriptions())
        .Returns(Task.FromResult(new List<AssetUpdateSubscriptionModel>{subscription}));

      mockRaptorProxy.Setup(m =>
          m.ExecuteGenericV2Request<Machine3DStatuses>(
            It.IsAny<string>(),
            It.IsAny<HttpMethod>(),
            It.IsAny<Stream>(),
            It.IsAny<IDictionary<string, string>>()))
        .Returns(Task.FromResult(machineResult));

      mockAssetResolverProxy
        .Setup(m => m.GetMatchingAssets(
          It.IsAny<List<long>>(), 
          It.IsAny<IDictionary<string, string>>()))
        .Returns(Task.FromResult<IEnumerable<KeyValuePair<Guid, long>>>(null));

      mockAssetStatusServerHubClient
        .Setup(m => m.UpdateAssetLocationsForClient(It.IsAny<AssetAggregateStatus>()))
        .Returns(Task.FromResult(true))
        .Callback<AssetAggregateStatus>((e) => { resultStatus = e; });

      // Run the test
      job.Run(null).Wait();

      // Validate the calls
      mockAssetResolverProxy
        .Verify(m => m.GetMatchingAssets(
            It.Is<List<long>>(l => l.Count == 1 && l[0] == machine1.AssetId),
            It.Is<IDictionary<string, string>>(d => DictionaryContentEquals(d, expectedHeaders))),
          Times.Once);

      // We should not call this if we have not matching 3d/2d assets
      mockAssetResolverProxy
        .Verify(m => m.GetMatching3D2DAssets(It.IsAny<MatchingAssetsDisplayModel>(),
            It.IsAny<IDictionary<string, string>>()),
          Times.Never);

      // We should have received one event 
      mockAssetStatusServerHubClient
        .Verify(m =>
            m.UpdateAssetLocationsForClient(It.IsAny<AssetAggregateStatus>()),
          Times.Once());

      // Validate it is the expected result
      Assert.IsNotNull(resultStatus);
      resultStatus.Should().BeEquivalentTo(expectedResult);
    }

    /// <summary>
    /// Test Case: Get an asset that exists in both 3d and 2d, and ensure it uses the latest location and information from both
    /// </summary>
    [TestMethod]
    public void TestProcessing3dand2dAssets()
    {
       var job = serviceProvider.GetService<IJob>();
      Assert.IsNotNull(job);
      
      var subscription = new AssetUpdateSubscriptionModel()
      {
        ProjectUid = Guid.Parse("7842662B-F8E5-4752-A64B-261CE7EDA152"),
        CustomerUid = Guid.Parse("8BB0D3A3-A755-4AD9-A4FA-999D6C1C104C"),
        AuthorizationHeader = "TEST AUTH",
        JWTAssertion = "TEST JWT"
      };
      
      var machine = new MachineStatus(55743, 
        "Test Machine from 3d", 
        true, 
        "LAST DESIGN FROM 3D", 
        15215, 
        new DateTime(2018,1,2,3,4,5, DateTimeKind.Utc),
        0.59915074193701334, // radians
        -1.470376021323053, // radians
        null,
        null);

      var machineResult = new Machine3DStatuses(ContractExecutionStatesEnum.ExecutedSuccessfully);
      machineResult.MachineStatuses.Add(machine);

      var assetDetails = new AssetDetails()
      {
        AssetId = "55743 - test",
        AssetUid = "47C03885-4845-4637-90B5-4CCE5D8DA040",
        LastReportedLocationLatitude = 123.45d,
        LastReportedLocationLongitude = 43.1d,
        LastLocationUpdateUtc = machine.lastKnownTimeStamp.Value.AddSeconds(1),
        FuelLevelLastReported = 2.1d,
        LastPercentFuelRemainingUtc = new DateTime(2019, 1, 1, 23, 41, 23, DateTimeKind.Utc)
      };

      var expectedResult = new AssetAggregateStatus()
      {
        CustomerUid = subscription.CustomerUid,
        ProjectUid = subscription.ProjectUid,
        LocationLastUpdatedUtc = assetDetails.LastLocationUpdateUtc.ToUniversalTime(),
        AssetUid = Guid.Parse("47C03885-4845-4637-90B5-4CCE5D8DA040"),
        Design = machine.lastKnownDesignName,
        FuelLevel = assetDetails.FuelLevelLastReported,
        FuelLevelLastUpdatedUtc = assetDetails.FuelReportedTimeUtc,
        Latitude = assetDetails.LastReportedLocationLatitude,
        Longitude = assetDetails.LastReportedLocationLongitude,
        LiftNumber = 15215,
        AssetIdentifier = "55743 - test", // will take this from the 2d data, as a higher priority that the 3d machine name
        MachineName = "Test Machine from 3d"
      };

      var expectedHeaders = GetExpectedHeaders(subscription.CustomerUid, subscription.AuthorizationHeader, subscription.JWTAssertion);
      AssetAggregateStatus resultStatus = null; // Keep a reference to the event generated for comparasion

      // Setup the returns
      mockAssetStatusServerHubClient
        .Setup(m => m.GetSubscriptions())
        .Returns(Task.FromResult(new List<AssetUpdateSubscriptionModel>{subscription}));

      mockRaptorProxy.Setup(m =>
          m.ExecuteGenericV2Request<Machine3DStatuses>(
            It.IsAny<string>(),
            It.IsAny<HttpMethod>(),
            It.IsAny<Stream>(),
            It.IsAny<IDictionary<string, string>>()))
        .Returns(Task.FromResult(machineResult));

      mockAssetResolverProxy
        .Setup(m => m.GetMatchingAssets(
          It.IsAny<List<long>>(), 
          It.IsAny<IDictionary<string, string>>()))
        .Returns(Task.FromResult<IEnumerable<KeyValuePair<Guid, long>>>(new KeyValuePair<Guid, long>[]
        {
          new KeyValuePair<Guid, long>(Guid.Parse(assetDetails.AssetUid), machine.AssetId), 
        }));

      mockFleetAssetDetails
        .Setup(m => m.GetAssetDetails(
          It.IsAny<string>(), 
          It.IsAny<IDictionary<string, string>>()))
        .Returns(Task.FromResult(assetDetails));

      mockAssetStatusServerHubClient
        .Setup(m => m.UpdateAssetLocationsForClient(It.IsAny<AssetAggregateStatus>()))
        .Returns(Task.FromResult(true))
        .Callback<AssetAggregateStatus>((e) => { resultStatus = e; });

      // Run the test
      job.Run(null).Wait();

      // Validate the calls
      mockAssetResolverProxy
        .Verify(m => m.GetMatchingAssets(
            It.Is<List<long>>(l => l.Count == 1 && l[0] == machine.AssetId),
            It.Is<IDictionary<string, string>>(d => DictionaryContentEquals(d, expectedHeaders))),
          Times.Once);

      // We should have this called for our asset uid which matched to the 3d asset id
      mockFleetAssetDetails
        .Verify(m => m.GetAssetDetails(
            It.Is<string>(s => Guid.Parse(s) == Guid.Parse(assetDetails.AssetUid)),
            It.Is<IDictionary<string, string>>(d => DictionaryContentEquals(d, expectedHeaders))),
          Times.Once);

      // We should have received one event 
      mockAssetStatusServerHubClient
        .Verify(m =>
            m.UpdateAssetLocationsForClient(It.IsAny<AssetAggregateStatus>()),
          Times.Once());

      // Validate it is the expected result
      Assert.IsNotNull(resultStatus);
      resultStatus.Should().BeEquivalentTo(expectedResult);
    }
      

    private IDictionary<string, string> GetExpectedHeaders(Guid customerUid, string authHeader, string jwt)
    {
      var headers = new Dictionary<string, string>()
      {
        {HeaderConstants.X_VISION_LINK_CUSTOMER_UID, customerUid.ToString()}, 
        {HeaderConstants.AUTHORIZATION, authHeader},
        {HeaderConstants.X_JWT_ASSERTION, jwt}
      };

      return headers;
    }

    // https://stackoverflow.com/questions/21758074/c-sharp-compare-two-dictionaries-for-equality
    private bool DictionaryContentEquals<TKey, TValue>(IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> otherDictionary)
    {
      return (otherDictionary ?? new Dictionary<TKey, TValue>())
        .OrderBy(kvp => kvp.Key)
        .SequenceEqual((dictionary ?? new Dictionary<TKey, TValue>())
          .OrderBy(kvp => kvp.Key));
    }
  }
}
