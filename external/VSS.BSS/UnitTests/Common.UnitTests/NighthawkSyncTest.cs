using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.NighthawkSyncTests;
using VSS.Nighthawk.EntityModels;
using VSS.Nighthawk.NighthawkSync.TopicDataProviders;
using VSS.Nighthawk.NighthawkSync.TopicDataTransformation;
using VSS.Nighthawk.NighthawkSync.TopicProcessors;
using VSS.Nighthawk.ServicesAPI;
using VSS.Nighthawk.ServicesAPI.Bss.Schema.v1;
using BSS = VSS.Nighthawk.ServicesAPI.Bss.Schema.v1;
using VSS.Nighthawk.Utilities;
using VSS.Nighthawk.NHBssSvc;
using System.Reflection;
using System.Xml.Linq;

namespace UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  [TestClass()]
  public class NighthawkSyncTest : ServerAPITestBase
  {
    private static string devFilePath = string.Empty;

    #region Additional test attributes
    // 
    //You can use the following additional attributes as you write your tests:
    //
    //Use ClassInitialize to run code before running the first test in the class
    [ClassInitialize()]
    public static void MyClassInitialize(TestContext testContext)
    {
      ConfigureEnvironment();
      devFilePath = System.Environment.CurrentDirectory;
      System.Diagnostics.Debug.WriteLine(string.Format("The devfilepath: {0}", devFilePath));

      // wire up test file-to-topic processor map 
      LoadTopicProcessorFileRootMap();
    }

    #endregion

    private static NighthawkSync GetTopicRecord(NH_OP ctx, string topicName, string topicType)
    {
      NighthawkSync topic = null;
      topic = (
        from s in ctx.NighthawkSync
        where
          String.Compare(s.TopicName, topicName) == 0 &&
          String.Compare(s.TopicType, topicType) == 0
        select s
      ).FirstOrDefault();

      return topic;
    }

    [TestMethod()]
    public void TestProvisionedAssetIdCache()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          ActiveUser me = AdminLogin();
          SessionContext session = API.Session.Validate(me.SessionID);

          List<INHDataObject> records = new List<INHDataObject>();
          EquipmentAPI equipmentAPI = new EquipmentAPI();
          DeviceAPI deviceAPI = new DeviceAPI();

          string gpsDeviceID = "888888";
          string serialNumberVIN = "ABC12345";
          string makeCode = "CAT";
          long assetID = -1;
          // Create asset and device 3 times and deactivate the first 2 devices
          for (int i = 0; i < 3; i++)
          {
            DateTime? effectiveUTC = DateTime.UtcNow;
            Asset newAsset = CreateAssetWithDevice(session, unitTestCustomerID.Value, gpsDeviceID, DeviceTypeEnum.CrossCheck, effectiveUTC.Value);
            // keep track of the latest asset ID
            assetID = newAsset.AssetID;
            Asset newAsset1 = (from asset in ctx.Asset where asset.AssetID == assetID select asset).FirstOrDefault<Asset>();
            newAsset1.SerialNumberVIN = serialNumberVIN;
            newAsset1.Make = makeCode;
            ctx.SaveChanges();

            DataFluidAnalysis dfa = new DataFluidAnalysis();
            dfa.AssetID = assetID;
            records.Add(dfa);

            if (i != 2)
            {
              //Leave the asset Activated
              //De-activate the Device
              bool deviceSuccess = deviceAPI.DeActivateDevice(ctx, session.NHRawContext, gpsDeviceID, DeviceTypeEnum.CrossCheck, DeviceStateEnum.BlacklistedCancelledService);
              Assert.IsTrue(deviceSuccess, "Failed to de-activate device");
            }
          }

          List<Asset> assetList = (from asset in ctx.Asset where asset.SerialNumberVIN == serialNumberVIN select asset).ToList<Asset>();
          List<Device> deviceList = (from device in ctx.Device where device.GpsDeviceID == gpsDeviceID select device).ToList<Device>();

          SosFileSystemDataService_Accessor target = new SosFileSystemDataService_Accessor(new DefaultDataForwardStrategy());
          NighthawkSync topic = new NighthawkSync();
          topic.TopicBookmark = "0";
          // load the cache
          bool newAssetsHaveBeenProvisioned = target.CheckAssetIdCacheForNewAssets(topic);
          Assert.IsTrue(newAssetsHaveBeenProvisioned, "new assets should have been provisioned");
          List<INHDataObject> results = target.FilterNHDataList(records);

          // Make sure there is only one result, not 3.
          Assert.AreEqual(1, results.Count, "should only be one result, not 3");
          Assert.AreEqual(assetID, results[0].AssetID, "got the wrong asset ID from the cache");
        }
      }
    }

    [TestMethod()]
    public void TestCommaSeparatedListOfLongs() {
      List<long> list = new List<long>();
      for (int i = 1; i <= 3; i++)
      {
        list.Add(i);
      }
      SosFileSystemDataService_Accessor target = new SosFileSystemDataService_Accessor(new DefaultDataForwardStrategy());
      Assert.AreEqual("1,2,3", target.GetCommaSeparatedList(list), "comma separated list did not come back as expected");
    }

    [TestMethod()]
    public void TestCommaSeparatedListOfINHDataObject()
    {
      List<INHDataObject> list = new List<INHDataObject>();
      for (int i = 1; i <= 3; i++)
      {
        DataFluidAnalysis dfa = new DataFluidAnalysis();
        dfa.AssetID = i;
        list.Add(dfa);
      }
      SosFileSystemDataService_Accessor target = new SosFileSystemDataService_Accessor(new DefaultDataForwardStrategy());
      Assert.AreEqual("1,2,3", target.GetCommaSeparatedList(list), "comma separated list did not come back as expected");
    }

    [TestMethod()]
    [ExpectedException(typeof(OptimisticConcurrencyException))]
    public void Concurrency()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        using (NH_OP ctx1 = Model.NewNHContext<NH_OP>())
        {
          SmuLocationProcessor processor = new SmuLocationProcessor(new MockTopicDataService(string.Empty), new MockMakeDataService());
          NighthawkSync topic = GetTopicRecord(ctx1, processor.TopicName, processor.TopicType);

          topic.SyncInProgress = true;
          topic.UpdateUTC = DateTime.UtcNow;

          using (NH_OP ctx2 = Model.NewNHContext<NH_OP>())
          {
            SmuLocationProcessor processor2 = new SmuLocationProcessor(new MockTopicDataService(string.Empty), new MockMakeDataService());

            NighthawkSync topic2 = GetTopicRecord(ctx2, processor.TopicName, processor.TopicType);

            topic2.SyncInProgress = true;
            topic2.UpdateUTC = DateTime.UtcNow.AddSeconds(1.0);
            int res2 = ctx2.SaveChanges();
            Assert.IsTrue(res2 > 0, "Save failed. Dammit!");
          }

          int res1 = ctx1.SaveChanges();
          Assert.AreEqual<int>(0, res1, "Save should be rejected due to concurrency mode of Fixed set on UpdateUTC field");
          Assert.IsTrue(false, "Actually, we expect an OptimisticConcurrencyException to be thrown in the SaveChanges above, so you shouldn't even get to here");
        }
      }
    }

    [TestMethod()]
    public void TestEmptyResponse()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot("empty");
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        // Get topic sync record
        SmuLocationProcessor processor = new SmuLocationProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;

        NighthawkSync topic = null;
        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          topic = GetTopicRecord(ctx, processor.TopicName, processor.TopicType);
          Assert.IsNotNull(topic);

          topic.NextSyncDueUTC = DateTime.UtcNow;
          topic.UpdateUTC = DateTime.UtcNow.AddHours(3);
          topic.SyncInProgress = true;

          ctx.SaveChanges();
        }

        RunSync(processor);

        using (NH_OP ctx = new NH_OP(Model.ConnectionString("NH_OP")))
        {
          NighthawkSync postTopic = GetTopicRecord(ctx, processor.TopicName, processor.TopicType);
          Assert.IsTrue(topic.NextSyncDueUTC.Value.CompareTo(postTopic.NextSyncDueUTC.Value) < 0);
          // TODO: Check that the next sync has been advanced correctly
        }
      }
    }

    [TestMethod()]
    public void TestMakeServiceStorage()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(MakeProcessor).Name);
        string testFilePath = Path.Combine(devFilePath, string.Format("{0}_LastCall.txt", fileRoot));
        Assert.IsTrue(File.Exists(testFilePath), testFilePath);

        var topicDataService = new MockTopicDataService(testFilePath);

        // Get topic sync record
        MakeProcessor processor = new MakeProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        RunSync(processor);

        using (NH_OP ctx = new NH_OP(Model.ConnectionString("NH_OP")))
        {
          List<Make> makeList = (from make in ctx.Make select make).ToList<Make>();
          Assert.IsTrue(makeList.Count > 0);
        }
      }
    }

    [TestMethod()]
    public void TestSMULocationSync()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(SmuLocationProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        SmuLocationProcessor processor = new SmuLocationProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestManualSMUSync()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(ManualSMUProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        ManualSMUProcessor processor = new ManualSMUProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestReplaySMULocationSync()
    {
      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(SmuLocationProcessor).Name);

        string replayFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("ReplayDevices.txt"));
        Dictionary<long, string> replayDeviceList = SetupAssetSubscriptonsForNHSyncTests(replayFilePath);

        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath, new MockDataServiceForwardReplayStrategy(testFilePath));

        var processor = new SmuLocationProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }
    [TestMethod()]
    public void TestReplayEngineSync()
    {
      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(EngineParametersProcessor).Name);

        string replayFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("ReplayDevices.txt"));
        Dictionary<long, string> replayDeviceList = SetupAssetSubscriptonsForNHSyncTests(replayFilePath);

        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath, new MockDataServiceForwardReplayStrategy(testFilePath));

        var processor = new EngineParametersProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestReplayFuelSync()
    {
      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(FuelProcessor).Name);

        string replayFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("ReplayDevices.txt"));
        Dictionary<long, string> replayDeviceList = SetupAssetSubscriptonsForNHSyncTests(replayFilePath);

        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath, new MockDataServiceForwardReplayStrategy(testFilePath));

        var processor = new FuelProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;

        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestReplayStartStopSync()
    {
      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(StartStopProcessor).Name);

        string replayFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("ReplayDevices.txt"));
        Dictionary<long, string> replayDeviceList = SetupAssetSubscriptonsForNHSyncTests(replayFilePath);

        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath, new MockDataServiceForwardReplayStrategy(testFilePath));

        var processor = new StartStopProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;

        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestReplayEventSync()
    {
      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(PLEventProcessor).Name);

        string replayFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("ReplayDevices.txt"));
        Dictionary<long, string> replayDeviceList = SetupAssetSubscriptonsForNHSyncTests(replayFilePath);

        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath, new MockDataServiceForwardReplayStrategy(testFilePath));

        var processor = new PLEventProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;

        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestReplayDiagnosticSync()
    {
      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(DiagnosticProcessor).Name);

        string replayFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("ReplayDevices.txt"));
        Dictionary<long, string> replayDeviceList = SetupAssetSubscriptonsForNHSyncTests(replayFilePath);

        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath, new MockDataServiceForwardReplayStrategy(testFilePath));

        var processor = new DiagnosticProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;

        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestHistoricalSMULocationSync()
    {
      using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(SmuLocationProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("HistoricalData/{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        var processor = new SmuLocationProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestFuelSync()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {

        string fileRoot = GetFileRoot(typeof(FuelProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        FuelProcessor processor = new FuelProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    [Ignore]
    public void TestHistoricalFuelSync()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(FuelProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("HistoricalData/{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        FuelProcessor processor = new FuelProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestStartStopSync()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(StartStopProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        StartStopProcessor processor = new StartStopProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    //[TestMethod()]
    //Commenting this test out because it should go away once Telematics Sync goes online and there is no point in fixing it
    public void TestHistoricalStartStopSync()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(StartStopProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("HistoricalData/{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        StartStopProcessor processor = new StartStopProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestFenceAlertSync3904And6000MessageContent()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(FenceAlertProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", string.Concat(fileRoot, "39046000")));
        var topicDataService = new MockTopicDataService(testFilePath);

        FenceAlertProcessor processor = new FenceAlertProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    //[TestMethod()]
    //Commenting this test out because it should go away once Telematics Sync goes online and there is no point in fixing it
    public void TestFenceAlertSync()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(FenceAlertProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        FenceAlertProcessor processor = new FenceAlertProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    //[TestMethod()]
    //Commenting this test out because it should go away once Telematics Sync goes online and there is no point in fixing it
    public void TestHistoricalFenceAlertSync()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(FenceAlertProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("HistoricalData/{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        FenceAlertProcessor processor = new FenceAlertProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestDiagnosticSync()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(DiagnosticProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        DiagnosticProcessor processor = new DiagnosticProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    //[TestMethod()]
    //Commenting this test out because it should go away once Telematics Sync goes online and there is no point in fixing it
    public void TestHistoricalDiagnosticSync()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(DiagnosticProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("HistoricalData/{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        DiagnosticProcessor processor = new DiagnosticProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestPLEventSync()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(PLEventProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        PLEventProcessor processor = new PLEventProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    [Ignore]// long running - killing the transaction timeout - TODO: will revist - jjm
    public void TestHistoricalPLEventSync()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(PLEventProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("HistoricalData/{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        PLEventProcessor processor = new PLEventProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestEngineParametersSync()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(EngineParametersProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        EngineParametersProcessor processor = new EngineParametersProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestHistoricalEngineParametersSync()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof(EngineParametersProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("HistoricalData/{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        EngineParametersProcessor processor = new EngineParametersProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod]
    public void TestDigitalSwitchStatusSync()
    {
      using(TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string fileRoot = GetFileRoot(typeof (DigitalSwitchStatusProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        DigitalSwitchStatusProcessor processor = new DigitalSwitchStatusProcessor(topicDataService, new MakeDataService());
        topicDataService.TopicProcessor = processor;
        TestTopicProcessor(processor, topicDataService);
      }
    }

    [TestMethod()]
    public void TestMakeStorage()
    {
      using (NH_OP ctx = new NH_OP(Model.ConnectionString("NH_OP")))
      {
        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          foreach (Make m in (from make in ctx.Make select make))
          {
            ctx.DeleteObject(m);
          }
          ctx.SaveChanges();

          List<Make> emptyMakeRecordList = (from make in ctx.Make select make).ToList<Make>();
          Assert.AreEqual(0, emptyMakeRecordList.Count);
        }
      }
    }

    [TestMethod()]
    public void TestDescriptionProcessor()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        string testFilePath = GetFullTestFilePathAndVerifyExistence("MakeData.txt");
        var topicDataService = new MockTopicDataService(testFilePath);

        MakeProcessor processor = new MakeProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;

        List<Make> expectedMakeList = new List<Make>();
        // Do this foreach make from input file...
        Make newMake = Make.CreateMake("CAT", DateTime.UtcNow);
        newMake.Name = "CAT";
        expectedMakeList.Add(newMake);

        Make newMake2 = Make.CreateMake("A58", DateTime.UtcNow);
        newMake2.Name = "ATLAS";
        expectedMakeList.Add(newMake2);

        using (NH_OP ctx = new NH_OP(Model.ConnectionString("NH_OP")))
        {

          RunSync(processor);

          List<Make> actualMakeList = (from make in ctx.Make select make).ToList<Make>();
          Assert.IsTrue(actualMakeList.Count >= expectedMakeList.Count, "Actual Make list is shorter than Expected Make list");

          Make expectedMake = (from m in actualMakeList
                               where m.Name.ToUpper() == newMake.Name.ToUpper()
                               select m).FirstOrDefault<Make>();
          Assert.IsNotNull(expectedMake);

          Make expectedMake2 = (from m in actualMakeList
                                where m.Name.ToUpper() == newMake2.Name.ToUpper()
                                select m).FirstOrDefault<Make>();
          Assert.IsNotNull(expectedMake2);

        }
      }
    }

    //public string GetAssetDeviceRecord(SessionContext session, string makeName, string serialNumber, bool isActive)
    //{
    //  using (NH_OP ctx = Model.NewNHContext<NH_OP>())
    //  {
    //    string gpsDeviceID = (from d in ctx.Device
    //                          where d.Asset.Make == makeName
    //                          && d.Asset.SerialNumberVIN == serialNumber
    //                          && d.Asset.Active == isActive
    //                          && d.Active
    //                          orderby d.UpdateUTC descending
    //                          select d.GpsDeviceID).FirstOrDefault<string>();
    //    return gpsDeviceID;
    //  }
    //}


    [TestMethod]
    public void TestServicePlanAssetOnboardingStatusMakeLookup()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);

        var isIHCMakeCodePresent = (from m in session.NHOpContext.MakeReadOnly
                                    where m.Code == "IHC"
                                    select 1).Any();

        if (!isIHCMakeCodePresent)
        {
          Make makeRecord = Make.CreateMake("IHC", DateTime.UtcNow);
          makeRecord.Name = "I.H.C.";
          session.NHOpContext.AddToMake(makeRecord);
          session.NHOpContext.SaveChanges();
        }

        MockTopicDataService dataService = SetupSubscriptionServiceTest(session, "{0}_BUG8816.txt", "IHC", false);

        bool subscriptionHistoryNeeded = (from ao in session.NHOpContext.AssetOnboardingStatusReadOnly where ao.Asset.Make == "I.H.C." && ao.Asset.SerialNumberVIN == "TEST0001" select ao.SubscriptionHistory).FirstOrDefault<bool>();
        Assert.IsFalse(subscriptionHistoryNeeded, "Asset Onboarding Status Subscription History flag should be false.");

        //We need to run the topic sync 2 times in order to force clearing of TopicProcessor.SubscriptionCache.
        //This is by no means an ideal way of doing this..but it works with minimal changes to code.
        RunSync(dataService.TopicProcessor);

        RunSync(dataService.TopicProcessor);

        subscriptionHistoryNeeded = (from ao in session.NHOpContext.AssetOnboardingStatusReadOnly where ao.Asset.Make == "I.H.C." && ao.Asset.SerialNumberVIN == "TEST0001" select ao.SubscriptionHistory).FirstOrDefault<bool>();
        Assert.IsTrue(subscriptionHistoryNeeded, "Asset Onboarding Status Subscription History flag should be true.");
      }
    }

    [TestMethod]
    public void TestServicePlanAssetOnboardingStatusLoadFromFile()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);

        MockTopicDataService dataService = SetupSubscriptionServiceTest(session, "{0}.txt", "CAT", false);


        bool subscriptionHistoryNeeded = (from ao in session.NHOpContext.AssetOnboardingStatusReadOnly where ao.Asset.Make == "CAT" && ao.Asset.SerialNumberVIN == "TEST0001" select ao.SubscriptionHistory).FirstOrDefault<bool>();
        Assert.IsFalse(subscriptionHistoryNeeded, "Asset Onboarding Status Subscription History flag should be false.");
        
        //We need to run the topic sync 2 times in order to force clearing of TopicProcessor.SubscriptionCache.
        //This is by no means an ideal way of doing this..but it works with minimal changes to code.
        RunSync(dataService.TopicProcessor);

        RunSync(dataService.TopicProcessor);

        subscriptionHistoryNeeded = (from ao in session.NHOpContext.AssetOnboardingStatusReadOnly
                                     where ao.Asset.Make == "CAT"
                                     && ao.Asset.SerialNumberVIN == "TEST0001"
                                     select ao.SubscriptionHistory).FirstOrDefault<bool>();
        Assert.IsTrue(subscriptionHistoryNeeded, "Asset Onboarding Status Subscription History flag should be true.");

      }
    }

    [TestMethod]
    public void TestServicePlanAssetOnboardingStatusMakeLookupForSubsLoad()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);

        var isIHCMakeCodePresent = (from m in session.NHOpContext.MakeReadOnly
                                    where m.Code == "IHC"
                                    select 1).Any();

        if (!isIHCMakeCodePresent)
        {
          Make makeRecord = Make.CreateMake("IHC", DateTime.UtcNow);
          makeRecord.Name = "I.H.C.";
          session.NHOpContext.AddToMake(makeRecord);
          session.NHOpContext.SaveChanges();
        }

        MockTopicDataService dataService = SetupSubscriptionServiceTest(session, "{0}_BUG8816.txt", "IHC", false);

        bool subscriptionHistoryNeeded = (from ao in session.NHOpContext.AssetOnboardingStatusReadOnly where ao.Asset.Make == "I.H.C." && ao.Asset.SerialNumberVIN == "TEST0001" select ao.SubscriptionHistory).FirstOrDefault<bool>();
        Assert.IsFalse(subscriptionHistoryNeeded, "Asset Onboarding Status Subscription History flag should be false.");

        dataService.TopicProcessor.TopicName = "ALLSUBS_LOAD";
        dataService.TopicProcessor.StartSync();
        dataService.TopicProcessor.Sync();
        dataService.TopicProcessor.CompleteSync();

        subscriptionHistoryNeeded = (from ao in session.NHOpContext.AssetOnboardingStatusReadOnly where ao.Asset.Make == "I.H.C." && ao.Asset.SerialNumberVIN == "TEST0001" select ao.SubscriptionHistory).FirstOrDefault<bool>();
        Assert.IsTrue(subscriptionHistoryNeeded, "Asset Onboarding Status Subscription History flag should be true.");
      }
    }

    [TestMethod]
    public void TestUpdatingOnboardStatusForSubsFollowedBySubsLoadFromFile()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);

        MockTopicDataService dataService = SetupSubscriptionServiceTest(session, "{0}.txt", "CAT", true);

        //First Pass
        dataService.TopicProcessor.StartSync();
        dataService.TopicProcessor.Sync();
        dataService.TopicProcessor.CompleteSync();


        var assetsWithOnboardingStatus = (from a in session.NHOpContext.AssetOnboardingStatus.Include("Asset")
                                          select a).ToList();
        Assert.IsTrue(assetsWithOnboardingStatus.Count > 0, "Onboarding Status not found for most recent asset from file");
        var statusForAssetFromFile = (from status in assetsWithOnboardingStatus
                                      where status.Asset.SerialNumberVIN == "TEST0001"
                                      select status).FirstOrDefault();
        Assert.IsNotNull(statusForAssetFromFile);

        //Set all historical data load flags to be TRUE to simulate a historical data load
        statusForAssetFromFile.DiagnosticsHistory = true;
        statusForAssetFromFile.EngineParameterHistory = true;
        statusForAssetFromFile.EventHistory = true;
        statusForAssetFromFile.EventReactions = true;
        statusForAssetFromFile.FenceAlertHistory = true;
        statusForAssetFromFile.FuelHistory = true;
        statusForAssetFromFile.SMUAdjustmentHistory = true;
        statusForAssetFromFile.SMULocationHistory = true;
        statusForAssetFromFile.StartStopHistory = true;

        session.NHOpContext.SaveChanges();

        //Second Pass
        dataService = SetupSubscriptionServiceTest(session, "{0}_RCE8840.txt", "CAT", true);
        dataService.TopicProcessor.TopicName = "ALLSUBS_LOAD";
        dataService.TopicProcessor.StartSync();
        dataService.TopicProcessor.Sync();
        dataService.TopicProcessor.CompleteSync();

        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          var onboardingStatus = (from a in ctx.AssetOnboardingStatus.Include("Asset")
                                  select a).ToList();
          Assert.IsTrue(onboardingStatus.Count > 0, "Onboarding Status not found for most recent asset from file");
          var statusRow = (from ss in onboardingStatus
                           where ss.Asset.SerialNumberVIN == "TEST0001"
                           select ss).FirstOrDefault();
          Assert.IsNotNull(statusForAssetFromFile);
          Assert.IsTrue(statusRow.SubscriptionHistory, "Subscription History Flag should be true.");
          Assert.IsFalse(statusRow.DiagnosticsHistory, "Diagnostic History Flag should be false.");
          Assert.IsFalse(statusRow.EngineParameterHistory, "Engine Parameter History Flag should be false.");
          Assert.IsFalse(statusRow.EventHistory, "Event History Flag should be false.");
          Assert.IsFalse(statusRow.EventReactions, "Event Reaction History Flag should be false.");
          Assert.IsFalse(statusRow.FenceAlertHistory, "Fence Alert History Flag should be false.");
          Assert.IsFalse(statusRow.FuelHistory, "Fuel History Flag should be false.");
          Assert.IsFalse(statusRow.SMUAdjustmentHistory, "SMU Adjustment History Flag should be false.");
          Assert.IsFalse(statusRow.SMULocationHistory, "SMU Location History Flag should be false.");
          Assert.IsFalse(statusRow.StartStopHistory, "Start/Stop History Flag should be false.");
        }
      }
    }

    [TestMethod]
    public void TestServicePlanLoadFromFileSubscriptionOwnedByCustomerAssetOwnedByCustomer()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);

        MockTopicDataService dataService = SetupSubscriptionServiceTest(session, "{0}_RCE8840.txt", "CAT", true);

        dataService.TopicProcessor.StartSync();
        dataService.TopicProcessor.Sync();
        dataService.TopicProcessor.CompleteSync();

        bool hasHistoricalSubscriptions =
          (from a in session.NHOpContext.AssetSubscriptionReadOnly
           where a.Asset.Make == "CAT" && a.Asset.SerialNumberVIN == "TEST0001"
           && a.StartUTC < startCoreUTC
           select 1).Any();

        Assert.IsTrue(hasHistoricalSubscriptions, "Asset should not have any historical subscriptions.");
      }
    }

    [TestMethod]
    public void TestServicePlanLoadFromFileSubscriptionOwnedByDealerAssetOwnedByDealer()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);

        MockTopicDataService dataService = SetupSubscriptionServiceTest(session, "{0}.txt", "CAT", false);

        dataService.TopicProcessor.StartSync();
        dataService.TopicProcessor.Sync();
        dataService.TopicProcessor.CompleteSync();

        bool hasHistoricalSubscriptions =
          (from a in session.NHOpContext.AssetSubscriptionReadOnly
           where a.Asset.Make == "CAT" && a.Asset.SerialNumberVIN == "TEST0001"
           && a.StartUTC < startCoreUTC
           select 1).Any();

        Assert.IsTrue(hasHistoricalSubscriptions, "Asset should not have any historical subscriptions.");
      }
    }

    [TestMethod]
    public void TestServicePlanLoadFromFileSubscriptionOwnedByDealerAssetOwnedByCustomer()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);

        MockTopicDataService dataService = SetupSubscriptionServiceTest(session, "{0}.txt", "CAT", true);

        dataService.TopicProcessor.StartSync();
        dataService.TopicProcessor.Sync();
        dataService.TopicProcessor.CompleteSync();

        bool hasHistoricalSubscriptions =
          (from a in session.NHOpContext.AssetSubscriptionReadOnly
           where a.Asset.Make == "CAT" && a.Asset.SerialNumberVIN == "TEST0001"
           && a.StartUTC < startCoreUTC
           select 1).Any();

        Assert.IsFalse(hasHistoricalSubscriptions, "Asset should not have any historical subscriptions.");
      }
    }

    [TestMethod]
    public void TestServicePlanLoadFromFileSubscriptionOwnedByCustomerAssetOwnedByDealer()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);

        MockTopicDataService dataService = SetupSubscriptionServiceTest(session, "{0}_RCE8840.txt", "CAT", false);

        dataService.TopicProcessor.StartSync();
        dataService.TopicProcessor.Sync();
        dataService.TopicProcessor.CompleteSync();

        bool hasHistoricalSubscriptions =
          (from a in session.NHOpContext.AssetSubscriptionReadOnly
           where a.Asset.Make == "CAT" && a.Asset.SerialNumberVIN == "TEST0001"
           && a.StartUTC < startCoreUTC
           select 1).Any();

        Assert.IsFalse(hasHistoricalSubscriptions, "Asset should not have any historical subscriptions.");
      }
    }

    [TestMethod]
    public void TestServicePlanLoadFromFileSubscriptionOwnedByCatAssetOwnedByCustomer()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);

        MockTopicDataService dataService = SetupSubscriptionServiceTest(session, "{0}_RCE8840_CAT.txt", "CAT", true);

        dataService.TopicProcessor.StartSync();
        dataService.TopicProcessor.Sync();
        dataService.TopicProcessor.CompleteSync();

        bool hasHistoricalSubscriptions =
          (from a in session.NHOpContext.AssetSubscriptionReadOnly
           where a.Asset.Make == "CAT" && a.Asset.SerialNumberVIN == "TEST0001"
           && a.StartUTC < startCoreUTC
           select 1).Any();

        Assert.IsFalse(hasHistoricalSubscriptions, "Asset should not have any historical subscriptions.");
      }
    }

    [TestMethod]
    public void TestServicePlanLoadFromFileSubscriptionOwnedByCatAssetOwnedByDealer()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);

        MockTopicDataService dataService = SetupSubscriptionServiceTest(session, "{0}_RCE8840_CAT.txt", "CAT", false);

        dataService.TopicProcessor.StartSync();
        dataService.TopicProcessor.Sync();
        dataService.TopicProcessor.CompleteSync();

        bool hasHistoricalSubscriptions =
          (from a in session.NHOpContext.AssetSubscriptionReadOnly
           where a.Asset.Make == "CAT" && a.Asset.SerialNumberVIN == "TEST0001"
           && a.StartUTC < startCoreUTC
           select 1).Any();

        Assert.IsFalse(hasHistoricalSubscriptions, "Asset should not have any historical subscriptions.");
      }
    }

    [TestMethod]
    public void TestServicePlanPurchaseAssetNoExistingSubscription()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);

        DateTime currentTimeUtc = DateTime.UtcNow.AddDays(-1);
        NHBssProcessor target = new NHBssProcessor();

        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          string fileRoot = GetFileRoot(typeof(SubscriptionProcessor).Name);
          string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
          var topicDataService = new MockTopicDataService(testFilePath);

          SetupDefaultAssetDevice(target, "PL321", "CAT", currentTimeUtc);

          //BSS.ServicePlan sp = NewServicePlan("Activated", currentTimeUtc.AddDays(-2), "-12345", "640715", "89500-00", "1234", DateTime.MinValue, "-99");
          //ServicePlanResult actual = target.ServicePlans(sp);
          //Assert.AreEqual("Success", actual.ServiceResultList[0].Result, "Result was not successful");

          SubscriptionProcessor processor = new SubscriptionProcessor(topicDataService, new MockMakeDataService());
          topicDataService.TopicProcessor = processor;
          RunSync(processor);

          bool hasSubscriptions =
            (from a in ctx.AssetSubscriptionReadOnly
             where a.Asset.Make == "CAT" && a.Asset.SerialNumberVIN == "TEST0001"
             select 1).Any();

          Assert.IsFalse(hasSubscriptions, "Asset should not have any subscriptions.");

        }
      }
    }

    [TestMethod]
    public void TestServicePlanPurchaseAssetWithExistingCoreSubscription()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);

        DateTime currentTimeUtc = DateTime.UtcNow.AddDays(-1);
        NHBssProcessor target = new NHBssProcessor();

        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          string fileRoot = GetFileRoot(typeof(SubscriptionProcessor).Name);
          string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
          var topicDataService = new MockTopicDataService(testFilePath);

          SetupDefaultAssetDevice(target, "PL321", "CAT", currentTimeUtc);

          BSS.ServicePlan sp = NewServicePlan("Activated", currentTimeUtc.AddDays(-2), "-12345", "640715", "89500-00", "1234", DateTime.MinValue, "-12333299");
          ServicePlanResult actual = target.ServicePlans(sp);
          Assert.AreEqual("Success", actual.ServiceResultList[0].Result, "Result was not successful");

          SubscriptionProcessor processor = new SubscriptionProcessor(topicDataService, new MockMakeDataService());
          topicDataService.TopicProcessor = processor;
          RunSync(processor);

          List<AssetSubscription> assetSubscriptions =
            (from a in ctx.AssetSubscriptionReadOnly.Include("ServicePlan")
             where a.Asset.Make == "CAT" && a.Asset.SerialNumberVIN == "TEST0001"
             select a).ToList<AssetSubscription>();

          bool hasCancelledCore = (from s in assetSubscriptions where s.ServicePlan.ID == 1 && s.EndUTC != null select 1).Count() == 2;
          Assert.IsTrue(hasCancelledCore, "Asset should have two cancelled core service plans.");

          bool hasActiveCore = (from s in assetSubscriptions where s.ServicePlan.ID == 1 && s.EndUTC == null select 1).Count() == 1;
          Assert.IsTrue(hasActiveCore, "Asset should have one active core service plan.");

          bool hasAddOnPlans = (from addOn in assetSubscriptions where !addOn.ServicePlan.Core.Value select 1).Count() == 0;
          Assert.IsTrue(hasAddOnPlans, "Asset should not have any add on service plans.");
        }
      }
    }

    [TestMethod]
    public void TestServicePlanPurchaseAssetWithExistingCoreSubscriptionAndFirstReportSent()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);

        DateTime currentTimeUtc = DateTime.UtcNow.AddDays(-1);
        NHBssProcessor target = new NHBssProcessor();

        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          string fileRoot = GetFileRoot(typeof(SubscriptionProcessor).Name);
          string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
          var topicDataService = new MockTopicDataService(testFilePath);

          SetupDefaultAssetDevice(target, "PL321", "CAT", currentTimeUtc);

          BSS.ServicePlan sp = NewServicePlan("Activated", currentTimeUtc.AddDays(-2), "-12345", "640715", "89500-00", "1234", DateTime.MinValue, "-12333299");
          ServicePlanResult actual = target.ServicePlans(sp);
          Assert.AreEqual("Success", actual.ServiceResultList[0].Result, "Result was not successful");

          SubscriptionProcessor processor = new SubscriptionProcessor(topicDataService, new MockMakeDataService());
          topicDataService.TopicProcessor = processor;

          AssetSubscription corePlan = (from s in ctx.AssetSubscription.Include("ServicePlan") where s.Asset.Make == "CAT" && s.Asset.SerialNumberVIN == "TEST0001" && s.ServicePlan.Core == true select s).FirstOrDefault<AssetSubscription>();
          corePlan.FirstReportNeeded = false;
          ctx.SaveChanges();

          RunSync(processor);

          List<AssetSubscription> assetSubscriptions =
        (from a in ctx.AssetSubscriptionReadOnly.Include("ServicePlan")
         where a.Asset.Make == "CAT" && a.Asset.SerialNumberVIN == "TEST0001"
         select a).ToList<AssetSubscription>();

          bool hasCancelledCore = (from s in assetSubscriptions where s.ServicePlan.ID == 1 && s.EndUTC != null select 1).Count() == 2;
          Assert.IsTrue(hasCancelledCore, "Asset should have two cancelled core service plans.");

          bool hasActiveCore = (from s in assetSubscriptions where s.ServicePlan.ID == 1 && s.EndUTC == null select 1).Count() == 1;
          Assert.IsTrue(hasActiveCore, "Asset should have one active core service plan.");

          bool hasAddOnPlans = (from addOn in assetSubscriptions where !addOn.ServicePlan.Core.Value && addOn.EndUTC.HasValue select 1).Count() == 1;
          Assert.IsTrue(hasAddOnPlans, "Asset should have one historically cancelled add on service plans.");
        }
      }
    }

    [TestMethod]
    public void AddingServicePlanUpdatesAssetOnboardingStatus()
    {

      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);

        DateTime currentTimeUtc = DateTime.UtcNow.AddDays(-1);
        NHBssProcessor target = new NHBssProcessor();

        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          string fileRoot = GetFileRoot(typeof(SubscriptionProcessor).Name);
          string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
          var topicDataService = new MockTopicDataService(testFilePath);

          SetupDefaultAssetDevice(target, "PL321", "CAT", currentTimeUtc);

          BSS.ServicePlan sp = NewServicePlan("Activated", currentTimeUtc.AddDays(-2), "-12345", "640715", "89500-00", "1234", DateTime.MinValue, "-12333299");
          ServicePlanResult actual = target.ServicePlans(sp);
          Assert.AreEqual("Success", actual.ServiceResultList[0].Result, "Result was not successful");

          SubscriptionProcessor processor = new SubscriptionProcessor(topicDataService, new MockMakeDataService());
          topicDataService.TopicProcessor = processor;
          RunSync(processor);

          //GET AssetStatus
          var assetOnboardingStatus = (from status in ctx.AssetOnboardingStatusReadOnly
                                       where status.Asset.Make == "CAT" && status.Asset.SerialNumberVIN == "TEST0001"
                                       select status).FirstOrDefault();

          Assert.IsNotNull(assetOnboardingStatus, "No asset onboarding record created.");
          Assert.IsTrue(assetOnboardingStatus.SubscriptionHistory);
          Assert.IsFalse(assetOnboardingStatus.DiagnosticsHistory);
          Assert.IsFalse(assetOnboardingStatus.StartStopHistory);
          Assert.IsFalse(assetOnboardingStatus.SMULocationHistory);
          Assert.IsFalse(assetOnboardingStatus.SMUAdjustmentHistory);
          Assert.IsFalse(assetOnboardingStatus.FuelHistory);
          Assert.IsFalse(assetOnboardingStatus.FenceAlertHistory);
          Assert.IsFalse(assetOnboardingStatus.EventReactions);
          Assert.IsFalse(assetOnboardingStatus.EventHistory);
          Assert.IsFalse(assetOnboardingStatus.EngineParameterHistory);
        }
      }
    }

    public static void SetupDefaultAssetDevice(NHBssProcessor target, string deviceType, string makeCode, DateTime actionUTC)
    {
      AccountHierarchy ah = NewAccountHierarchy("Created", actionUTC, "-123454321", "109876", "DEMODEALER", null, "TD00", null, "TCS Dealer", null);
      AccountHierarchyResult result = target.AccountHierarchies(ah);

      InstallBase ib = NewInstallBase("Created", actionUTC, "-12333299", "1", "-12345", deviceType, "FW123", "TEST0001", makeCode, string.Empty, "2010", "SIM1234", "3.1.1", "Activated", "1234", "A1", ah.AccountList[0].BSSID, "123");
      target.InstallBases(ib);
    }

    public static void SetupDefaultCustomerAssetDevice(string bssId, NHBssProcessor target, string deviceType, string makeCode, DateTime actionUTC)
    {
      InstallBase ib = NewInstallBase("Created", actionUTC, "-12333299", "1", "-12345", "PL321", "FW123", "TEST0001", makeCode, string.Empty, "2010", "SIM1234", "3.1.1", "Activated", "1234", "A1", bssId, "123");
      target.InstallBases(ib);
    }

    /// <summary>
    /// Tests that correct event descriptions are correctly inserted into the DB 
    /// This is the case where these descriptions did not previously exist in our DB
    /// </summary>
    //[TestMethod]
    public void TestEventDescriptionInsert()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);

        string fileRoot = GetFileRoot(typeof(PLEventDescriptionProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        PLEventDescriptionProcessor processor = new PLEventDescriptionProcessor(
          topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        RunSync(processor);

        using (NH_OP ctx = new NH_OP(Model.ConnectionString("NH_OP")))
        {
          Fault fault = (from d in ctx.FaultReadOnly
                         where d.EID == 1 &&
                         d.FaultType.ID == (int)FaultTypeEnum.Event
                         select d).FirstOrDefault<Fault>();
          Assert.IsNotNull(fault, "Failed to find PL Fault DescriptionSet");

          List<FaultDescription> faultDesc = (from fd in ctx.FaultDescriptionReadOnly.Include("Language")
                                              where fd.Fault.ID == fault.ID
                                              select fd).ToList<FaultDescription>();
          Assert.AreEqual(11, faultDesc.Count, "Number of fault descriptions do not match for DescriptionSet ID = 1");

          string englishDesc = (from f in faultDesc
                                where f.Language.ISOName == "en-US"
                                select f.Description).FirstOrDefault<string>();
          Assert.IsTrue(!string.IsNullOrEmpty(englishDesc), "Failed to find english description");
          Assert.AreEqual("Engine Oil Filter Restriction Derate", englishDesc, "English description does not match");

          string frenchDesc = (from f in faultDesc
                               where f.Language.ISOName == "fr-FR"
                               select f.Description).FirstOrDefault<string>();
          Assert.IsTrue(!string.IsNullOrEmpty(frenchDesc), "Failed to find french description");
          Assert.AreEqual("Corriger le colmatage du filtre à huile du mo", frenchDesc, "French description does not match");

          string spanishDesc = (from f in faultDesc
                                where f.Language.ISOName == "es-ES"
                                select f.Description).FirstOrDefault<string>();
          Assert.IsTrue(!string.IsNullOrEmpty(spanishDesc), "Failed to find spanish description");
          Assert.AreEqual("Reducción de potencia restringida filtro del aceite del motor", spanishDesc, "Spanish description does not match");
        }
      }
    }

    /// <summary>
    /// Tests that correct event descriptions are correctly updated into the DB 
    /// This is the case where these descriptions are present in our DB and need to be updated
    /// </summary>
    [TestMethod]
    public void TestEventDescriptionUpdate()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);

        string testFilePath =
          GetFullTestFilePathAndVerifyExistence(
            string.Format("{0}.txt", string.Concat("EIDLIST", "_UpdatedDesc")));
        var topicDataService = new MockTopicDataService(testFilePath);

        PLEventDescriptionProcessor processor = new PLEventDescriptionProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        RunSync(processor);

        using (NH_OP ctx = new NH_OP(Model.ConnectionString("NH_OP")))
        {
          Fault fault = (from d in ctx.FaultReadOnly
                         where d.EID == 99999 &&
                           d.FaultType.ID == (int)FaultTypeEnum.Event &&
                           d.Datalink.ID == (int)DatalinkEnum.CDL
                         select d).FirstOrDefault<Fault>();
          Assert.IsNotNull(fault, "Failed to find PL Fault DescriptionSet");

          List<FaultDescription> faultDesc = (from fd in ctx.FaultDescriptionReadOnly.Include("Language")
                                              where fd.Fault.ID == fault.ID
                                              select fd).ToList<FaultDescription>();

          Assert.IsTrue(faultDesc.Count > 0, "Number of fault descriptions do not match for DescriptionSet ID = 1");

          string englishDesc = (from f in faultDesc
                                where f.Language.ISOName == "en-US"
                                select f.Description).FirstOrDefault<string>();
          Assert.IsTrue(!string.IsNullOrEmpty(englishDesc), "Failed to find english description");
          Assert.AreEqual("High Altitude Shutdown", englishDesc, "English description does not match");

          string frenchDesc = (from f in faultDesc
                               where f.Language.ISOName == "fr-FR"
                               select f.Description).FirstOrDefault<string>();
          Assert.IsTrue(!string.IsNullOrEmpty(frenchDesc), "Failed to find french description");
          //Assert.AreEqual("Arrêt haute altitude", frenchDesc, "French description does not match");          

          string spanishDesc = (from f in faultDesc
                                where f.Language.ISOName == "es-ES"
                                select f.Description).FirstOrDefault<string>();
          Assert.IsTrue(!string.IsNullOrEmpty(spanishDesc), "Failed to find spanish description");
          Assert.AreEqual("Parada por elevada altitud", spanishDesc, "Spanish description does not match");
        }
      }
    }

    [TestMethod]
    public void ConvertMachineComponentDescriptionDataUsingXslTransform()
    {
      string fileRoot = GetFileRoot(typeof(MachineComponentDescriptionProcessor).Name);
      string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));

      string xmlData = string.Empty;

      using (StreamReader reader = new StreamReader(testFilePath))
      {
        xmlData = reader.ReadToEnd();
      }

      Assert.IsTrue(xmlData.Length > 0, "XML input is empty.");

      var transformer = new DataTransformerFactory(new MockDataRepository()).Create(DataTransformerType.MachineComponentDescription);

      var languageMap = new Dictionary<string, string>();
      languageMap.Add("en", "1");
      languageMap.Add("fr", "2");
      languageMap.Add("es", "3");

      var midMap = new Dictionary<string, string>();
      midMap.Add("9", "1");
      midMap.Add("10", "2");
      midMap.Add("11", "3");
      midMap.Add("119", "4");

      transformer.LoadLookupData("##langId##", languageMap);
      transformer.LoadLookupData("##midId##", midMap);

      var result = transformer.Transform(xmlData);
      Assert.IsTrue(result.Succeeded, "Transform failed.");

      string expectedResult =
        @"<?xml version=""1.0"" encoding=""utf-16"" ?>
          <root xmlns:module=""http://www.cat.com/em/feed/v6/Module"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
            <midDescriptions>
              <MIDDesc fk_MIDID=""1"" fk_LanguageID=""1"" Description=""Handheld Service Tool"" />
              <MIDDesc fk_MIDID=""1"" fk_LanguageID=""3"" Description=""Programa de servicio de mano"" />
              <MIDDesc fk_MIDID=""1"" fk_LanguageID=""2"" Description=""Dispositif manuel"" />
              <MIDDesc fk_MIDID=""2"" fk_LanguageID=""1"" Description=""DDT"" />
              <MIDDesc fk_MIDID=""2"" fk_LanguageID=""3"" Description=""DDT"" />
              <MIDDesc fk_MIDID=""2"" fk_LanguageID=""2"" Description=""DDT"" />
              <MIDDesc fk_MIDID=""3"" fk_LanguageID=""1"" Description=""Com Adapter"" />
              <MIDDesc fk_MIDID=""3"" fk_LanguageID=""3"" Description=""Adaptador com"" />
              <MIDDesc fk_MIDID=""3"" fk_LanguageID=""2"" Description=""Adaptateur Com"" />
              <MIDDesc fk_MIDID=""4"" fk_LanguageID=""1"" Description=""Paver Burner #2"" />
              <MIDDesc fk_MIDID=""4"" fk_LanguageID=""3"" Description=""Quemador pavimentadora número 2"" />
              <MIDDesc fk_MIDID=""4"" fk_LanguageID=""2"" Description=""Épandeur brûleur N°2"" />
            </midDescriptions>
          </root>";

      Assert.IsTrue(
        expectedResult.RemoveAllWhitespace().Equals(
          result.TransformedXmlSets[0].TransformedXml.RemoveAllWhitespace(), StringComparison.OrdinalIgnoreCase),
        string.Format(
          "Data not transformed as expected. Expected: {0} \r\n\r\n - Was: {1} \r\n\r\n - Xsl: {2} ",
          expectedResult,
          result.TransformedXmlSets[0],
          result.Xsl));
    }

    /// <summary>
    /// Tests the machine component description insert.
    /// </summary>
    [TestMethod]
    public void TestMachineComponentDescriptionInsert()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);

        string fileRoot = GetFileRoot(typeof(MachineComponentDescriptionProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        var processor = new MachineComponentDescriptionProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        RunSync(processor);

        using (NH_OP ctx = new NH_OP(Model.ConnectionString("NH_OP")))
        {
          // get all records and check that the count is the same as is in the test file
          int expectedMidCount = 4;
          int expectedDescriptionCount = 12; // three descs for each mid

          List<MID> dimMids =
            (from d
             in ctx.MIDReadOnly
             where d.MID1 == "9" || d.MID1 == "10" || d.MID1 == "11" || d.MID1 == "119"
             select d).ToList();

          Assert.AreEqual(expectedMidCount, dimMids.Count, "MID count is not correct.");

          List<MIDDesc> dimMidDescs =
              (from d
               in ctx.MIDDescReadOnly.Include("Language")
               where (d.MID.MID1 == "9" || d.MID.MID1 == "10" || d.MID.MID1 == "11" || d.MID.MID1 == "119")
               && (d.Language.ISOName == "en-US" || d.Language.ISOName == "es-ES" || d.Language.ISOName == "fr-FR")
               select d).ToList();
          Assert.AreEqual(expectedDescriptionCount, dimMidDescs.Count, "Description count is not correct.");

          // get the mid description collection for a given mid in the test data doc 
          // then check each message against the db to make sure it was stored successfully

          List<MIDDesc> paverBurnerDescList =
            (from d in ctx.MIDDescReadOnly.Include("Language")
             where d.MID.MID1.Equals("119", StringComparison.Ordinal)
             && (d.Language.ISOName == "en-US" || d.Language.ISOName == "es-ES" || d.Language.ISOName == "fr-FR")
             select d).ToList();
          Assert.IsTrue(
            3.Equals(paverBurnerDescList.Count),
            string.Format(
              "Incorrect number of paver burner descriptions returned: received {0} descriptions.",
              paverBurnerDescList.Count));

          string englishDescription = (from d in paverBurnerDescList
                                       where d.Language.ISOName == "en-US"
                                       select d.Description).FirstOrDefault();
          Assert.IsTrue(!string.IsNullOrEmpty(englishDescription), "English description is empty");

          string spanishDescription = (from d in paverBurnerDescList
                                       where d.Language.ISOName == "es-ES"
                                       select d.Description).FirstOrDefault();
          Assert.IsTrue(!string.IsNullOrEmpty(spanishDescription), "Spanish description is empty");

          string frenchDescription = (from d in paverBurnerDescList
                                      where d.Language.ISOName == "fr-FR"
                                      select d.Description).FirstOrDefault();
          Assert.IsTrue(!string.IsNullOrEmpty(frenchDescription), "French description is empty");
        }
      }
    }

    [TestMethod]
    public void TestMachineComponentDescriptionUpdate()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);

        string fileRoot = GetFileRoot(typeof(MachineComponentDescriptionProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        var processor = new MachineComponentDescriptionProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        RunSync(processor);

        using (NH_OP ctx = new NH_OP(Model.ConnectionString("NH_OP")))
        {
          // get all records and check that the count is the same as is in the test file
          int expectedMidCount = 4;
          int expectedDescriptionCount = 12; // three descs for each mid

          List<MID> dimMids =
           (from d
            in ctx.MIDReadOnly
            where d.MID1 == "9" || d.MID1 == "10" || d.MID1 == "11" || d.MID1 == "119"
            select d).ToList();
          Assert.AreEqual(expectedMidCount, dimMids.Count, "MID count is not correct.");

          List<MIDDesc> dimMidDescs =
              (from d
               in ctx.MIDDescReadOnly.Include("MID").Include("Language")
               where (d.MID.MID1 == "9" || d.MID.MID1 == "10" || d.MID.MID1 == "11" || d.MID.MID1 == "119")
               && (d.Language.ISOName == "en-US" || d.Language.ISOName == "es-ES" || d.Language.ISOName == "fr-FR")
               select d).ToList();
          Assert.AreEqual(expectedDescriptionCount, dimMidDescs.Count, "Description count is not correct.");

          // get the mid description collection for a given mid in the test data doc 
          // then check each message against the db to make sure it was stored successfully

          List<MIDDesc> paverBurnerDescList =
            (from d in ctx.MIDDescReadOnly.Include("MID").Include("Language")
             where d.MID.MID1.Equals("119", StringComparison.Ordinal)
            && (d.Language.ISOName == "en-US" || d.Language.ISOName == "es-ES" || d.Language.ISOName == "fr-FR")
             select d).ToList();
          Assert.AreEqual(3, paverBurnerDescList.Count, "Incorrect number of paver burner descriptions returned.");

          string englishDescription = (from d in paverBurnerDescList
                                       where d.Language.ISOName == "en-US"
                                       select d.Description).FirstOrDefault();
          Assert.IsTrue(!string.IsNullOrEmpty(englishDescription), "English description is empty");

          string spanishDescription = (from d in paverBurnerDescList
                                       where d.Language.ISOName == "es-ES"
                                       select d.Description).FirstOrDefault();
          Assert.IsTrue(!string.IsNullOrEmpty(spanishDescription), "Spanish description is empty");

          string frenchDescription = (from d in paverBurnerDescList
                                      where d.Language.ISOName == "fr-FR"
                                      select d.Description).FirstOrDefault();
          Assert.IsTrue(!string.IsNullOrEmpty(frenchDescription), "French description is empty");
        }

        testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}_Changed.txt", fileRoot));
        topicDataService.ChangeTestFile(testFilePath);
        RunSync(processor);

        using (NH_OP ctx = new NH_OP(Model.ConnectionString("NH_OP")))
        {
          // get the mid description collection for a given mid in the test data doc 
          // then check each message against the db to make sure it was stored successfully

          List<MIDDesc> paverBurnerDescList =
            (from d in ctx.MIDDescReadOnly.Include("Language")
             where d.MID.MID1.Equals("119", StringComparison.Ordinal)
             && (d.Language.ISOName == "en-US" || d.Language.ISOName == "es-ES" || d.Language.ISOName == "fr-FR")
             select d).ToList();
          Assert.AreEqual(3, paverBurnerDescList.Count, "Incorrect number of paver burner descriptions returned.");

          string englishDescription = (from d in paverBurnerDescList
                                       where d.Language.ISOName == "en-US"
                                       select d.Description).FirstOrDefault();
          string expectedEnglishDescription = "Updated - Paver Burner #2";
          Assert.IsTrue(
            expectedEnglishDescription.Equals(englishDescription, StringComparison.Ordinal),
            string.Format(
              "English description is incorrect. Expected '{0}' but was '{1}'.",
              expectedEnglishDescription,
              englishDescription));

          string expectedSpanishDescription = "Updated - Quemador pavimentadora número 2";
          string spanishDescription = (from d in paverBurnerDescList
                                       where d.Language.ISOName == "es-ES"
                                       select d.Description).FirstOrDefault();
          Assert.IsTrue(
            expectedSpanishDescription.Equals(spanishDescription, StringComparison.Ordinal),
            string.Format(
              "Spanish description is incorrect. Expected '{0}' but was '{1}'.",
              expectedSpanishDescription,
              spanishDescription));

          string expectedFrenchDescription = "Updated - Épandeur brûleur N°2";
          string frenchDescription = (from d in paverBurnerDescList
                                      where d.Language.ISOName == "fr-FR"
                                      select d.Description).FirstOrDefault();
          Assert.IsTrue(
            expectedFrenchDescription.Equals(frenchDescription, StringComparison.Ordinal),
            string.Format(
              "French description is incorrect. Expected '{0}' but was '{1}'.",
              expectedFrenchDescription,
              frenchDescription));
        }
      }
    }

    [TestMethod]
    public void TestNextBufferBlockCreation()
    {
      string blockWithNoExplicitName =
        @"<module>
            <nextBuffer>
		          <url>someUrl</url>
		          <moreData>true</moreData>
	          </nextBuffer>                      
          </module>";
      var nextBufferBlock = NextBufferBlock.Create(blockWithNoExplicitName, "/module", null);
      Assert.IsTrue(nextBufferBlock.HasMoreData, "blockWithNoExplicitName - more data incorrect.");
      Assert.IsTrue(
        "someUrl".Equals(nextBufferBlock.NextAddress, StringComparison.Ordinal),
        "blockWithNoExplicitName - NextAddress not correct");

      string blockWithExplicitName =
          @"<module:midQuery xmlns:module=""http://www.cat.com/em/feed/v6/Module"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.cat.com/em/feed/v6/Module https://emfeed.cat.com/EMFeedFiles/schema/v6/Module.xsd"">
              <nextBuffer>
		            <url>someUrl</url>
		            <moreData>true</moreData>
	            </nextBuffer>                      
            </module:midQuery>";

      var namespaces = new Dictionary<string, string>();
      namespaces.Add("module", "http://www.cat.com/em/feed/v6/Module");
      nextBufferBlock = NextBufferBlock.Create(blockWithExplicitName, "/module:midQuery", namespaces);
      Assert.IsTrue(nextBufferBlock.HasMoreData, "blockWithExplicitName - more data incorrect.");
      Assert.IsTrue(
        "someUrl".Equals(nextBufferBlock.NextAddress, StringComparison.Ordinal),
        "blockWithExplicitName - NextAddress not correct");

      string blockWithNoExplicitNameButNoData =
        @"<module>
            <nextBuffer>
		          <url></url>
		          <moreData>false</moreData>
	          </nextBuffer>                      
          </module>";
      nextBufferBlock = NextBufferBlock.Create(blockWithNoExplicitNameButNoData, "/module", null);
      Assert.IsFalse(nextBufferBlock.HasMoreData, "blockWithNoExplicitNameButNoData - more data incorrect.");
      Assert.IsTrue(
        "".Equals(nextBufferBlock.NextAddress, StringComparison.Ordinal),
        "blockWithNoExplicitNameButNoData - NextAddress not correct");

      string blockWithExplictNameButNoData =
        @"<module:midQuery xmlns:module=""http://www.cat.com/em/feed/v6/Module"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.cat.com/em/feed/v6/Module https://emfeed.cat.com/EMFeedFiles/schema/v6/Module.xsd"">
              <nextBuffer>
		            <url></url>
		            <moreData>false</moreData>
	            </nextBuffer>                      
            </module:midQuery>";
      nextBufferBlock = NextBufferBlock.Create(blockWithExplictNameButNoData, "/module:midQuery", namespaces);
      Assert.IsFalse(nextBufferBlock.HasMoreData, "blockWithExplictNameButNoData - more data incorrect.");
      Assert.IsTrue(
        "".Equals(nextBufferBlock.NextAddress, StringComparison.Ordinal),
        "blockWithExplictNameButNoData - NextAddress not correct");

      string blockWithMissingNextBuffer =
        @"<module>          
          </module>";
      nextBufferBlock = NextBufferBlock.Create(blockWithMissingNextBuffer, "/module", null);
      Assert.IsFalse(nextBufferBlock.HasMoreData, "blockWithMissingNextBuffer - more data incorrect.");
      Assert.IsTrue(
        "".Equals(nextBufferBlock.NextAddress, StringComparison.Ordinal),
        "blockWithMissingNextBuffer - NextAddress not correct");
    }

    /// <summary>
    /// Tests that correct diagnostic descriptions are inserted into the DB 
    /// This is the case where these descriptions did not previously exist in our DB
    /// </summary>
    [TestMethod]
    public void TestDiagnosticDescriptionInsert()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);

        string fileRoot = GetFileRoot(typeof(DiagnosticDescriptionProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        DiagnosticDescriptionProcessor processor = new DiagnosticDescriptionProcessor(
          topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        RunSync(processor);

        using (NH_OP ctx = new NH_OP(Model.ConnectionString("NH_OP")))
        {
          Fault fault = (from d in ctx.FaultReadOnly
                         where d.CID == 1
                         && d.FMI == 0
                         && d.FaultType.ID == (int)FaultTypeEnum.Diagnostic
                         && d.Datalink.ID == 1 //Two of the same fault but different data links exist
                         select d).FirstOrDefault<Fault>();
          Assert.IsNotNull(fault, "Failed to find PL Fault DescriptionSet");

          List<FaultDescription> faultDesc = (from fd in ctx.FaultDescriptionReadOnly.Include("Language")
                                              where fd.Fault.ID == fault.ID
                                              select fd).ToList<FaultDescription>();
          Assert.IsTrue(faultDesc.Count > 0, "No fault descriptions found for Fault with CID = 1");

          string englishDesc = (from f in faultDesc
                                where f.Language.ISOName == "en-US"
                                select f.Description).FirstOrDefault<string>();
          Assert.IsTrue(!string.IsNullOrEmpty(englishDesc), "Failed to find english description");

          string frenchDesc = (from f in faultDesc
                               where f.Language.ISOName == "fr-FR"
                               select f.Description).FirstOrDefault<string>();
          Assert.IsTrue(!string.IsNullOrEmpty(frenchDesc), "Failed to find french description");

          string spanishDesc = (from f in faultDesc
                                where f.Language.ISOName == "es-ES"
                                select f.Description).FirstOrDefault<string>();
          Assert.IsTrue(!string.IsNullOrEmpty(spanishDesc), "Failed to find spanish description");
        }
      }
    }

    /// <summary>
    /// Tests that correct diagnostic descriptions are correctly updated into the DB 
    /// This is the case where these descriptions are present in our DB and need to be updated
    /// </summary>
    [TestMethod]
    public void TestDiagnosticDescriptionUpdate()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);

        string fileRoot = GetFileRoot(typeof(DiagnosticDescriptionProcessor).Name);
        string testFilePath =
          GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", string.Concat(fileRoot, "_UpdatedDesc")));
        var topicDataService = new MockTopicDataService(testFilePath);

        var processor = new DiagnosticDescriptionProcessor(topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        RunSync(processor);

        using (NH_OP ctx = new NH_OP(Model.ConnectionString("NH_OP")))
        {
          Fault fault = (from d in ctx.FaultReadOnly
                         where d.CID == 1 &&
                         d.FMI == 0 &&
                         d.FaultType.ID == (int)FaultTypeEnum.Diagnostic &&
                         d.Datalink.ID == (int)DatalinkEnum.CDL //Two of the same fault but different data links exist
                         select d).FirstOrDefault<Fault>();
          Assert.IsNotNull(fault, "Failed to find PL Fault DescriptionSet");

          List<FaultDescription> faultDesc = (from fd in ctx.FaultDescriptionReadOnly.Include("Language")
                                              where fd.Fault.ID == fault.ID
                                              select fd).ToList<FaultDescription>();
          Assert.IsTrue(faultDesc.Count > 0, "No fault descriptions found for DescriptionSet with CID = 1");

          string englishDesc = (from f in faultDesc
                                where f.Language.ISOName == "en-US"
                                select f.Description).FirstOrDefault<string>();
          Assert.IsTrue(!string.IsNullOrEmpty(englishDesc), "Failed to find english description");

          string frenchDesc = (from f in faultDesc
                               where f.Language.ISOName == "fr-FR"
                               select f.Description).FirstOrDefault<string>();
          Assert.IsTrue(!string.IsNullOrEmpty(frenchDesc), "Failed to find french description");

          string spanishDesc = (from f in faultDesc
                                where f.Language.ISOName == "es-ES"
                                select f.Description).FirstOrDefault<string>();
          Assert.IsTrue(!string.IsNullOrEmpty(spanishDesc), "Failed to find spanish description");
        }
      }
    }

    /// <summary>
    /// Tests that the correct Signature ID is inserted into the DB 
    /// </summary>
    [TestMethod]
    public void TestSignatureID()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);

        string fileRoot = GetFileRoot(typeof(DiagnosticDescriptionProcessor).Name);
        string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
        var topicDataService = new MockTopicDataService(testFilePath);

        DiagnosticDescriptionProcessor processor = new DiagnosticDescriptionProcessor(
          topicDataService, new MockMakeDataService());
        topicDataService.TopicProcessor = processor;
        RunSync(processor);

        using (NH_OP ctx = new NH_OP(Model.ConnectionString("NH_OP")))
        {
          DataFaultDiagnostic dataFaultDiagnostic = new DataFaultDiagnostic();
          dataFaultDiagnostic.CID = 1636;
          dataFaultDiagnostic.FMI = 31;
          dataFaultDiagnostic.fk_DimDatalinkID = (int)DatalinkEnum.CDL;
          long expectedSignatureID = EncryptionUtils.TextToLong(dataFaultDiagnostic.FmiSignature);

          Fault fault = (from d in ctx.FaultReadOnly
                         where d.CID == 1636
                         && d.FMI == 31
                         && d.FaultType.ID == (int)FaultTypeEnum.Diagnostic
                         && d.Datalink.ID == (int)DatalinkEnum.CDL
                         select d).FirstOrDefault<Fault>();
          Assert.IsNotNull(fault, "Failed to find PL Fault DescriptionSet");
          long actualSignatureID = fault.SignatureID;

          Assert.AreEqual(expectedSignatureID, actualSignatureID, "Signature IDs do not match");
        }
      }
    }

    [TestMethod()]
    public void TestMakeUniqueName()
    {
      ActiveUser me = AdminLogin();
      SessionContext session = API.Session.Validate(me.SessionID);

      using (var ctx = new NH_OP(Model.ConnectionString("NH_OP")))
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          Customer newCustomer1 = (from c in ctx.Customer where c.Name.Equals("Test Customer 1") select c).FirstOrDefault();
          long? customerId = 0;
          if (null == newCustomer1)
          {
            newCustomer1 = new Customer() { Name = "Test Customer 1" };
            customerId = CreateCustomer(session, newCustomer1, CustomerTypeEnum.Customer, null);
          }
          else
          {
            customerId = newCustomer1.ID;
          }

          string make = "TEST";
          string serial = "ABC123";

          string uniqueAssetName = API.Equipment.BuildAssetName(Model.NewNHContext<NH_OP>(), customerId.Value, string.Empty, make, serial);
          Asset newAsset = API.Equipment.Create(session.NHOpContext, customerId.Value, uniqueAssetName, make, serial, "Product Family",
                                    "Sales Model", null, 0, (int)OwnershipTypeEnum.Owned);
          newAsset.UpdateUTC = DateTime.UtcNow.AddHours(3);
          session.NHOpContext.SaveChanges();

          Assert.AreEqual("TEST ABC123", uniqueAssetName);

          uniqueAssetName = API.Equipment.BuildAssetName(Model.NewNHContext<NH_OP>(), customerId.Value, string.Empty, make, serial);
          newAsset = API.Equipment.Create(session.NHOpContext, customerId.Value, uniqueAssetName, make, uniqueAssetName, "Product Family",
                              "Sales Model", null, 0, (int)OwnershipTypeEnum.Owned);
          newAsset.UpdateUTC = DateTime.UtcNow.AddHours(3);
          session.NHOpContext.SaveChanges();
          Assert.AreEqual("TEST ABC123 (2)", uniqueAssetName);

          uniqueAssetName = API.Equipment.BuildAssetName(Model.NewNHContext<NH_OP>(), customerId.Value, string.Empty, make, serial);
          newAsset = API.Equipment.Create(session.NHOpContext, customerId.Value, uniqueAssetName, make, uniqueAssetName, "Product Family",
                              "Sales Model", null, 0, (int)OwnershipTypeEnum.Owned);
          Assert.AreEqual("TEST ABC123 (3)", uniqueAssetName);
        }
      }
    }

    [TestMethod]
    public void TestAssetNameIsNickname()
    {
      ActiveUser me = AdminLogin();
      SessionContext session = API.Session.Validate(me.SessionID);

      using (var ctx = new NH_OP(Model.ConnectionString("NH_OP")))
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          // if the nickname is supplied, it should be set as the asset name
          string expectedAssetName = "SomeNickname";

          string assetName = API.Equipment.BuildAssetName(ctx, unitTestCustomerID.Value, "SomeNickname", "JJM", "1234");

          Assert.IsTrue(expectedAssetName.Equals(assetName, StringComparison.Ordinal));
        }
      }
    }

    [TestMethod]
    public void TestAssetNameIsMakeAndSerialNumber()
    {
      // if nickname is not supplied, the asset name should be a concat of 
      // the make and serial number, delimited with a space
      ActiveUser me = AdminLogin();
      SessionContext session = API.Session.Validate(me.SessionID);

      using (var ctx = new NH_OP(Model.ConnectionString("NH_OP")))
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          // if the nickname is supplied, it should be set as the asset name
          string expectedAssetName = "JJM 1234";

          string assetName = API.Equipment.BuildAssetName(ctx, unitTestCustomerID.Value, string.Empty, "JJM", "1234");

          Assert.IsTrue(expectedAssetName.Equals(assetName, StringComparison.Ordinal));
        }
      }
    }

    private void TestTopicProcessor(TopicProcessor processor, ITopicDataService topicDataService)
    {
      TestTopicProcessor(processor, ((MockTopicDataService)topicDataService).UnitTestFile, topicDataService);
    }

    #region Test Impl

    private void TestTopicProcessor(TopicProcessor processor, String path, ITopicDataService topicDataService)
    {
      Dictionary<long, string> subList = SetupAssetSubscriptonsForNHSyncTests(path);
      Assert.IsNotNull(subList, "Failed to create subscription records");
      Assert.IsTrue(subList.Count > 0);

      processor.StartSync();
      processor.Sync();
      processor.CompleteSync();

      List<NHDataWrapper> expectedList = ReadTestData(((MockTopicDataService)topicDataService).ExpectedFilePath);
      List<NHDataWrapper> actualList = ReadTestData(((MockTopicDataService)topicDataService).ActualFilePath);

      //Assert.AreEqual(expectedList.Count, actualList.Count, "Counts do not match for expected-actual lists");

      for (int i = 0; i < expectedList.Count; i++)
      {
        //Comparing everything else except AssetID as that will change everytime the test runs
        expectedList[i].Data.AssetID = 0;
        actualList[i].Data.AssetID = 0;

        string expected = expectedList[i].Data.ToXElement().ToString();
        string actual = actualList[i].Data.ToXElement().ToString();
        Assert.AreEqual(expected, actual, "Expected and Actual items do not match");
      }

      if (((MockTopicDataService)topicDataService).IsReplayEnabled)
      {
        bool hasMappedItem =
          (from nhDataObject in actualList where nhDataObject.Data.GPSDeviceID.Equals("REPLAYDEVICE1") select nhDataObject)
            .Any();
        Assert.IsTrue(hasMappedItem, "Mapped replay items not found");
      }
    }

    #endregion

    [TestMethod]
    public void TestTopicIsRequeuedAfterFailure()
    {
      // update the next run time so we can know explicitly that it has been updated
      DateTime rightNow = DateTime.UtcNow;

      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          string fileRoot = GetFileRoot(typeof(DeviceDetailsProcessor).Name);
          string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
          var topicDataService = new MockTopicDataService(testFilePath, true, false, new MockDataServiceForwardStrategy(testFilePath));

          var processor = new DeviceDetailsProcessor(topicDataService, new MockMakeDataService());
          topicDataService.TopicProcessor = processor;

          // 1. get next time for topic to run -
          // 2. force topic data retrieval failure, which should cause topic to be requeued
          // 3. test to make sure next due date is set correctly

          NighthawkSync topicSync = TopicProcessor.GetTopicRecord(ctx, "DeviceDetails", "Data");

          topicSync.NextSyncDueUTC = rightNow;
          ctx.SaveChanges();

          bool started = processor.StartSync();
          Assert.IsTrue(started, "The processor sync failed to start.");

          bool synced = processor.Sync();
          Assert.IsFalse(synced, "The sync somehow succeeded even though it was told to fail.");

          processor.AbortSync();

          using (NH_OP ctx2 = Model.NewNHContext<NH_OP>())
          {
            NighthawkSync updatedTopicSync = TopicProcessor.GetTopicRecord(ctx2, "DeviceDetails", "Data");

            Assert.IsTrue(
              updatedTopicSync.NextSyncDueUTC > topicSync.NextSyncDueUTC.Value.AddMilliseconds(topicSync.SyncInterval));
          }
        }
      }
    }

    #region Onboarding Status Tests

    [TestMethod]
    public void TestSetOnboardingHistoryFlags()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          ActiveUser me = AdminLogin();
          SessionContext session = API.Session.Validate(me.SessionID);

          FileSystemDataService service = GetFileSystemDataService(typeof(FuelProcessor).Name);

          Asset asset = CreateAssetWithDevice(session, session.CustomerID.Value, "TESTGPSDEVICEID", DeviceTypeEnum.PL321, DateTime.UtcNow);
          // Subscription History Flag is set to true so this will get picked up by the FileSystemDataService.
          AssetOnboardingStatus onboardingStatus = AssetOnboardingStatus.CreateAssetOnboardingStatus(-1, true, false, false, false, false, false, false, false, false, false, DateTime.UtcNow);
          onboardingStatus.Asset = asset;
          session.NHOpContext.AddToAssetOnboardingStatus(onboardingStatus);
          session.NHOpContext.SaveChanges();

          NighthawkSync sync = new NighthawkSync();
          ServiceProvider provider = new ServiceProvider();
          ProcessorResults results = new ProcessorResults();

          sync.TopicName = "FUEL";
          sync.TopicBookmark = "0";
          provider.ServerIPAddress = "127.0.0.1";

          //Create the "Pending' directory
          Directory.CreateDirectory(string.Format(@"{0}\{1}\Pending", provider.ServerIPAddress, sync.TopicName));

          service.GetData(sync, provider, results);

          var onboardHist = (from o in ctx.AssetOnboardingStatusReadOnly where o.Asset.AssetID == asset.AssetID select o).FirstOrDefault();

          Assert.IsNotNull(onboardHist, "Onboarding History should exist for this asset.");
          Assert.IsTrue(onboardHist.FuelHistory, "The topic onboarding flag should be true.");
        }
      }
    }

    private FileSystemDataService GetFileSystemDataService(string topicName)
    {
      string fileRoot = GetFileRoot(topicName);
      string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
      FileSystemDataService service = new FileSystemDataService(new MockDataServiceForwardStrategy(testFilePath));
      return service;
    }

    [TestMethod]
    public void TestOnboardingStatusFlags()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          ActiveUser me = AdminLogin();
          SessionContext session = API.Session.Validate(me.SessionID);

          //File name doesn't matter for this test since we're just calling the update method.
          FileSystemDataService service = GetFileSystemDataService(typeof(FuelProcessor).Name);

          AssetOnboardingStatus statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("FUEL", statusRow);
          Assert.IsTrue(statusRow.FuelHistory == true, "Fuel History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("FUEL_LOAD", statusRow);
          Assert.IsTrue(statusRow.FuelHistory == true, "Fuel History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("ENGINE", statusRow);
          Assert.IsTrue(statusRow.EngineParameterHistory == true, "Engine Parameter History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("ENGINE_LOAD", statusRow);
          Assert.IsTrue(statusRow.EngineParameterHistory == true, "Engine Parameter History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("STARTSTOP", statusRow);
          Assert.IsTrue(statusRow.StartStopHistory == true, "Start/Stop History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("STARTSTOP_LOAD", statusRow);
          Assert.IsTrue(statusRow.StartStopHistory == true, "Start/Stop History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("EVENT", statusRow);
          Assert.IsTrue(statusRow.EventHistory == true, "Event History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("EVENT_LOAD", statusRow);
          Assert.IsTrue(statusRow.EventHistory == true, "Event History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("DIAGNOSTIC", statusRow);
          Assert.IsTrue(statusRow.DiagnosticsHistory == true, "Diagnostic History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("DIAGNOSTIC_LOAD", statusRow);
          Assert.IsTrue(statusRow.DiagnosticsHistory == true, "Diagnostic History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("FENCEALERT", statusRow);
          Assert.IsTrue(statusRow.FenceAlertHistory == true, "Fence Alert History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("FENCEALERT_LOAD", statusRow);
          Assert.IsTrue(statusRow.FenceAlertHistory == true, "Fence Alert History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("MANUALSMU", statusRow);
          Assert.IsTrue(statusRow.SMUAdjustmentHistory == true, "SMU Adjustment History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("MANUALSMU_LOAD", statusRow);
          Assert.IsTrue(statusRow.SMUAdjustmentHistory == true, "SMU Adjustment History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("SMULOC2", statusRow);
          Assert.IsTrue(statusRow.SMULocationHistory == true, "SMU/Location History Flag should be true.");

          statusRow = new AssetOnboardingStatus();
          service.SetTopicOnboardingStatus("SMULOC2_LOAD", statusRow);
          Assert.IsTrue(statusRow.SMULocationHistory == true, "SMU/Location History Flag should be true.");
        }
      }
    }

    private void CreateAssetsForComparison(SessionContext session, out Asset asset1, out Asset asset2, out AssetOnboardingStatus onboardingStatus2)
    {
      // Subscription History Flag is set to true so this will get picked up by the FileSystemDataService.
      asset1 = CreateAssetWithDevice(session, session.CustomerID.Value, "TESTGPSDEVICEID", DeviceTypeEnum.PL321, DateTime.UtcNow);
      AssetOnboardingStatus onboardingStatus = AssetOnboardingStatus.CreateAssetOnboardingStatus(-1, true, false, false, false, false, false, false, false, false, false, DateTime.UtcNow);
      onboardingStatus.AssetReference.EntityKey = Model.GetEntityKey<Asset>(session.NHOpContext, asset1.AssetID);
      session.NHOpContext.AddToAssetOnboardingStatus(onboardingStatus);

      asset2 = CreateAssetWithDevice(session, session.CustomerID.Value, "TESTGPSDEVICEID2", DeviceTypeEnum.PL321, DateTime.UtcNow);
      onboardingStatus2 = AssetOnboardingStatus.CreateAssetOnboardingStatus(-1, true, false, false, false, false, false, false, false, false, false, DateTime.UtcNow);
      onboardingStatus2.AssetReference.EntityKey = Model.GetEntityKey<Asset>(session.NHOpContext, asset2.AssetID);
      session.NHOpContext.AddToAssetOnboardingStatus(onboardingStatus2);

      session.NHOpContext.SaveChanges();
    }

    [TestMethod]
    public void TestLoadingHistoricalFuelTopicAssetIDs()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          ActiveUser me = AdminLogin();
          SessionContext session = API.Session.Validate(me.SessionID);

          List<long> assetIDs = new List<long>();

          //File name doesn't matter for this test since we're just calling the update method.
          FileSystemDataService service = GetFileSystemDataService(typeof(FuelProcessor).Name);

          Asset asset1;
          Asset asset2;
          AssetOnboardingStatus onboardingStatus2;
          CreateAssetsForComparison(session, out asset1, out asset2, out onboardingStatus2);

          assetIDs = service.LoadAssetIDs(ctx, "FUEL");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "FUEL_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          onboardingStatus2.FuelHistory = true;
          session.NHOpContext.SaveChanges();

          assetIDs = service.LoadAssetIDs(ctx, "FUEL");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "FUEL_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");
        }
      }
    }

    [TestMethod]
    public void TestLoadingHistoricalEngineTopicAssetIDs()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          ActiveUser me = AdminLogin();
          SessionContext session = API.Session.Validate(me.SessionID);

          List<long> assetIDs = new List<long>();

          //File name doesn't matter for this test since we're just calling the update method.
          FileSystemDataService service = GetFileSystemDataService(typeof(EngineParametersProcessor).Name);

          Asset asset1;
          Asset asset2;
          AssetOnboardingStatus onboardingStatus2;
          CreateAssetsForComparison(session, out asset1, out asset2, out onboardingStatus2);

          assetIDs = service.LoadAssetIDs(ctx, "ENGINE");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "ENGINE_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          onboardingStatus2.EngineParameterHistory = true;
          session.NHOpContext.SaveChanges();

          assetIDs = service.LoadAssetIDs(ctx, "ENGINE");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "ENGINE_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");
        }
      }
    }

    [TestMethod]
    public void TestLoadingHistoricalLocationTopicAssetIDs()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          ActiveUser me = AdminLogin();
          SessionContext session = API.Session.Validate(me.SessionID);

          List<long> assetIDs = new List<long>();

          //File name doesn't matter for this test since we're just calling the update method.
          FileSystemDataService service = GetFileSystemDataService(typeof(SmuLocationProcessor).Name);

          Asset asset1;
          Asset asset2;
          AssetOnboardingStatus onboardingStatus2;
          CreateAssetsForComparison(session, out asset1, out asset2, out onboardingStatus2);

          assetIDs = service.LoadAssetIDs(ctx, "SMULOC2");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "SMULOC2_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          onboardingStatus2.SMULocationHistory = true;
          session.NHOpContext.SaveChanges();

          assetIDs = service.LoadAssetIDs(ctx, "SMULOC2");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "SMULOC2_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");
        }
      }
    }

    [TestMethod]
    public void TestLoadingHistoricalEventTopicAssetIDs()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          ActiveUser me = AdminLogin();
          SessionContext session = API.Session.Validate(me.SessionID);

          List<long> assetIDs = new List<long>();

          //File name doesn't matter for this test since we're just calling the update method.
          FileSystemDataService service = GetFileSystemDataService(typeof(PLEventProcessor).Name);

          Asset asset1;
          Asset asset2;
          AssetOnboardingStatus onboardingStatus2;
          CreateAssetsForComparison(session, out asset1, out asset2, out onboardingStatus2);

          assetIDs = service.LoadAssetIDs(ctx, "EVENT");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "EVENT_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          onboardingStatus2.EventHistory = true;
          session.NHOpContext.SaveChanges();

          assetIDs = service.LoadAssetIDs(ctx, "EVENT");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "EVENT_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");
        }
      }
    }

    [TestMethod]
    public void TestLoadingHistoricalDiagnosticTopicAssetIDs()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          ActiveUser me = AdminLogin();
          SessionContext session = API.Session.Validate(me.SessionID);

          List<long> assetIDs = new List<long>();

          //File name doesn't matter for this test since we're just calling the update method.
          FileSystemDataService service = GetFileSystemDataService(typeof(DiagnosticProcessor).Name);

          Asset asset1;
          Asset asset2;
          AssetOnboardingStatus onboardingStatus2;
          CreateAssetsForComparison(session, out asset1, out asset2, out onboardingStatus2);

          assetIDs = service.LoadAssetIDs(ctx, "DIAGNOSTIC");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "DIAGNOSTIC_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          onboardingStatus2.DiagnosticsHistory = true;
          session.NHOpContext.SaveChanges();

          assetIDs = service.LoadAssetIDs(ctx, "DIAGNOSTIC");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "DIAGNOSTIC_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");
        }
      }
    }

    [TestMethod]
    public void TestLoadingHistoricalFenceAlertTopicAssetIDs()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          ActiveUser me = AdminLogin();
          SessionContext session = API.Session.Validate(me.SessionID);

          List<long> assetIDs = new List<long>();

          //File name doesn't matter for this test since we're just calling the update method.
          FileSystemDataService service = GetFileSystemDataService(typeof(FenceAlertProcessor).Name);

          Asset asset1;
          Asset asset2;
          AssetOnboardingStatus onboardingStatus2;
          CreateAssetsForComparison(session, out asset1, out asset2, out onboardingStatus2);

          assetIDs = service.LoadAssetIDs(ctx, "FENCEALERT");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "FENCEALERT_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          onboardingStatus2.FenceAlertHistory = true;
          session.NHOpContext.SaveChanges();

          assetIDs = service.LoadAssetIDs(ctx, "FENCEALERT");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "FENCEALERT_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");
        }
      }
    }

    [TestMethod]
    public void TestLoadingHistoricalSMUAdjustmentTopicAssetIDs()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          ActiveUser me = AdminLogin();
          SessionContext session = API.Session.Validate(me.SessionID);

          List<long> assetIDs = new List<long>();

          //File name doesn't matter for this test since we're just calling the update method.
          FileSystemDataService service = GetFileSystemDataService(typeof(ManualSMUProcessor).Name);

          Asset asset1;
          Asset asset2;
          AssetOnboardingStatus onboardingStatus2;
          CreateAssetsForComparison(session, out asset1, out asset2, out onboardingStatus2);

          assetIDs = service.LoadAssetIDs(ctx, "MANUALSMU");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "MANUALSMU_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          onboardingStatus2.SMUAdjustmentHistory = true;
          session.NHOpContext.SaveChanges();

          assetIDs = service.LoadAssetIDs(ctx, "MANUALSMU");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "MANUALSMU_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");
        }
      }
    }

    [TestMethod]
    public void TestLoadingHistoricalStartStopTopicAssetIDs()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        using (NH_OP ctx = Model.NewNHContext<NH_OP>())
        {
          ActiveUser me = AdminLogin();
          SessionContext session = API.Session.Validate(me.SessionID);

          List<long> assetIDs = new List<long>();

          //File name doesn't matter for this test since we're just calling the update method.
          FileSystemDataService service = GetFileSystemDataService(typeof(ManualSMUProcessor).Name);

          Asset asset1;
          Asset asset2;
          AssetOnboardingStatus onboardingStatus2;
          CreateAssetsForComparison(session, out asset1, out asset2, out onboardingStatus2);

          assetIDs = service.LoadAssetIDs(ctx, "STARTSTOP");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "STARTSTOP_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && assetIDs.Contains(asset2.AssetID), "Both assets should be loaded.");

          onboardingStatus2.StartStopHistory = true;
          session.NHOpContext.SaveChanges();

          assetIDs = service.LoadAssetIDs(ctx, "STARTSTOP");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");

          assetIDs = service.LoadAssetIDs(ctx, "STARTSTOP_LOAD");
          Assert.IsTrue(assetIDs.Count > 0, "Asset IDs should be found");
          Assert.IsTrue(assetIDs.Contains(asset1.AssetID) && !assetIDs.Contains(asset2.AssetID), "Only asset1 should be loaded.");
        }
      }
    }

    #endregion

    [TestMethod]
    public void TestTopicIsRequeuedAfterFailureFromUnknownException()
    {
      // update the next run time so we can know explicitly that it has been updated
      DateTime rightNow = DateTime.UtcNow;

      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          string fileRoot = GetFileRoot(typeof(EngineParametersProcessor).Name);
          string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format("{0}.txt", fileRoot));
          var topicDataService = new MockTopicDataService(testFilePath, true, true);

          var processor = new EngineParametersProcessor(topicDataService, new MockMakeDataService());
          topicDataService.TopicProcessor = processor;

          // 1. get next time for topic to run -
          // 2. force topic data retrieval failure, which should cause topic to be requeued
          // 3. test to make sure next due date is set correctly

          NighthawkSync topicSync = TopicProcessor.GetTopicRecord(ctx, "Engine", "Data");

          topicSync.NextSyncDueUTC = rightNow;
          ctx.SaveChanges();

          bool started = processor.StartSync();
          Assert.IsTrue(started, "The processor sync failed to start.");

          bool synced = processor.Sync();
          Assert.IsFalse(synced, "The sync somehow succeeded even though it was told to fail.");

          processor.AbortSync();

          using (NH_OP ctx2 = Model.NewNHContext<NH_OP>())
          {
            NighthawkSync updatedTopicSync = TopicProcessor.GetTopicRecord(ctx2, "Engine", "Data");

            Assert.IsTrue(
              updatedTopicSync.NextSyncDueUTC > topicSync.NextSyncDueUTC.Value.AddMilliseconds(topicSync.SyncInterval));
          }
        }
      }
    }

    [TestMethod()]
    public void TestPMSerialNumberPrefixSync()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          SalesModel salesModel = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY", "301.6C", "XXX", 1000);

          var topicDataService = SetupPMSerialNumberPrefixService("{0}.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          SalesModel actual = (from sm in ctx.SalesModelReadOnly where sm.ExternalID == salesModel.ExternalID select sm).FirstOrDefault();

          Assert.AreEqual("301.6C", actual.ModelCode);
          Assert.AreEqual(salesModel.SerialNumberPrefix, actual.SerialNumberPrefix);
          Assert.AreEqual(1, actual.StartRange);
          Assert.AreEqual(99999, actual.EndRange);
        }
      }
    }

    [TestMethod()]
    public void TestPMSerialNumberPrefixSyncFilterB9H()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          SalesModel salesModel = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY", "16M", "16M", null);

          var topicDataService = SetupPMSerialNumberPrefixService("{0}_FilterB9H.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          SalesModel actual = (from sm in ctx.SalesModelReadOnly where sm.ModelCode == salesModel.ModelCode select sm).FirstOrDefault();

          Assert.IsNull(actual.ExternalID);
        }
      }
    }
    [TestMethod()]
    public void TestPMIntervalSync()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          SalesModel salesModel = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY", "modelcode", "XXX", 1000);

          var topicDataService = SetupPMIntervalServiceTest("{0}.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          List<PMInterval> actualIntervals = (from i in ctx.PMIntervalReadOnly.Include("SalesModel")
                                              where i.SalesModel.ExternalID == salesModel.ExternalID
                                              select i).ToList();

          Assert.AreEqual(8, actualIntervals.Count, "Counts do not match for expected-actual lists");
          Assert.IsFalse(actualIntervals[0].IsMetric, "First interval shouldn't be metric");
          Assert.IsFalse(actualIntervals[3].IsMetric, "4th interval shouldn't be metric");
          Assert.IsTrue(actualIntervals[4].IsMetric, "5th interval should be metric");
          Assert.IsTrue(actualIntervals[7].IsMetric, "8th interval should be metric");

          Assert.AreEqual("PM 1", actualIntervals[0].Title, "First interval Title is incorrect");
          Assert.AreEqual("PM3000", actualIntervals[3].Title, "4th interval Title is incorrect");
          Assert.AreEqual("PM 1", actualIntervals[4].Title, "5th interval Title is incorrect");
          Assert.AreEqual("PM3000", actualIntervals[7].Title, "8th interval Title is incorrect");

          Assert.AreEqual(250, actualIntervals[0].FirstInterval, "First interval FirstInterval is incorrect");
          Assert.AreEqual(3000, actualIntervals[3].FirstInterval, "4th interval FirstInterval is incorrect");
          Assert.AreEqual(250, actualIntervals[4].FirstInterval, "5th interval FirstInterval is incorrect");
          Assert.AreEqual(3000, actualIntervals[7].FirstInterval, "8th FirstInterval is incorrect");

          Assert.AreEqual(500, actualIntervals[0].NextInterval, "First interval NextInterval is incorrect");
          Assert.AreEqual(6000, actualIntervals[3].NextInterval, "4th interval NextInterval is incorrect");
          Assert.AreEqual(500, actualIntervals[4].NextInterval, "5th interval NextInterval is incorrect");
          Assert.AreEqual(6000, actualIntervals[7].NextInterval, "8th NextInterval is incorrect");
        }
      }
    }
   
    [TestMethod()]
    public void TestPMIntervalSyncZeroFrequency()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          SalesModel salesModel = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY", "modelcode", "XXX", 4351);

          var topicDataService = SetupPMIntervalServiceTest("{0}_US8998_ZEROFREQUENCY.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          List<PMInterval> actualIntervals = (from i in ctx.PMIntervalReadOnly.Include("SalesModel")
                                              where i.SalesModel.ExternalID == salesModel.ExternalID
                                              select i).ToList();

          int usIntervalCount = (from ai in actualIntervals where !ai.IsMetric select ai).Count();
          int metricIntervalCount = (from ai in actualIntervals where ai.IsMetric select ai).Count();

          Assert.IsTrue(usIntervalCount == 0, "US Intervals should not have been created for this sales model.");
          Assert.IsTrue(metricIntervalCount == 0, "Metric intervals should not have been created for this sales model.");
        }
      }
    }

    [TestMethod()]
    public void TestPMIntervalSyncNoMetric()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          SalesModel salesModel = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY", "modelcode", "XXX", 7320);

          var topicDataService = SetupPMIntervalServiceTest("{0}_BUG8815.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          List<PMInterval> actualIntervals = (from i in ctx.PMIntervalReadOnly.Include("SalesModel")
                                              where i.SalesModel.ExternalID == salesModel.ExternalID
                                              select i).ToList();

          int usIntervalCount = (from ai in actualIntervals where !ai.IsMetric select ai).Count();
          int metricIntervalCount = (from ai in actualIntervals where ai.IsMetric select ai).Count();

          Assert.IsTrue(usIntervalCount > 0, "US Intervals should have been created for this sales model.");
          Assert.IsTrue(metricIntervalCount == 0, "Metric intervals should not have been created for this sales model.");
        }
      }
    }

    [TestMethod()]
    public void TestPMIntervalSyncRemovesWhenRequired()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          SalesModel salesModel = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY", "modelcode", "XXX", 1000);

          var topicDataService = SetupPMIntervalServiceTest("{0}_US8998.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          List<PMInterval> actualIntervals = (from i in ctx.PMIntervalReadOnly.Include("SalesModel")
                                              where i.SalesModel.ExternalID == salesModel.ExternalID
                                              select i).ToList();

          int usIntervalCount = (from ai in actualIntervals where !ai.IsMetric select ai).Count();

          foreach (PMInterval interval in actualIntervals)
          {
            Assert.IsFalse(interval.Description.Contains("WHEN REQUIRED"), "'WHEN REQUIRED' Intervals should not have been created for this sales model.");
          }
        }
      }
    }

    [TestMethod()]
    public void TestPMIntervalRank()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          SalesModel salesModel = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY", "modelcode", "XXX", 1000);

          var topicDataService = SetupPMIntervalServiceTest("{0}_US8998_Rank.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          List<PMInterval> actualIntervals = (from interval in ctx.PMIntervalReadOnly.Include("SalesModel")
                                              where interval.SalesModel.ExternalID == salesModel.ExternalID
                                              && interval.IsCumulative && !interval.IsMetric
                                              orderby interval.Rank
                                              select interval).ToList();

          Assert.AreEqual(4, actualIntervals.Count, "There should be four cumulative US intervals.");
          for (int i = 0; i < actualIntervals.Count(); i++)
          {
            Assert.AreEqual(i + 1, actualIntervals[i].Rank, String.Format("Interval {0} should be rank {0}.", i + 1));
          }
        }
      }
    }

    [TestMethod()]
    public void TestPMIntervalRankBug()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          SalesModel salesModel = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY", "modelcode", "XXX", 1000);
          SalesModel salesModel2 = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY 2", "modelcode 2", "YYY", 2000);

          var topicDataService = SetupPMIntervalServiceTest("{0}_RankBug.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          List<PMInterval> actualIntervals = (from interval in ctx.PMIntervalReadOnly.Include("SalesModel")
                                              where interval.SalesModel.ExternalID == salesModel.ExternalID
                                              && interval.IsCumulative && !interval.IsMetric
                                              orderby interval.Rank
                                              select interval).ToList();

          Assert.AreEqual(3, actualIntervals.Count, "There should be four cumulative US intervals for Sales Model 1.");
          for (int i = 0; i < actualIntervals.Count(); i++)
          {
            Assert.AreEqual(i + 1, actualIntervals[i].Rank, String.Format("Interval {0} should be rank {0}.", i + 1));
          }

          actualIntervals = (from interval in ctx.PMIntervalReadOnly.Include("SalesModel")
                              where interval.SalesModel.ExternalID == salesModel2.ExternalID
                              && interval.IsCumulative && !interval.IsMetric
                              orderby interval.Rank
                              select interval).ToList();

          Assert.AreEqual(4, actualIntervals.Count, "There should be four cumulative US intervals for Sales Model 2.");
          for (int i = 0; i < actualIntervals.Count(); i++)
          {
            Assert.AreEqual(i + 1, actualIntervals[i].Rank, String.Format("Interval {0} should be rank {0}.", i + 1));
          }
        }
      }
    }

    [TestMethod()]
    public void TestPMCatCumulativeIntervalDetails()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          SalesModel salesModel = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY", "modelcode", "XXX", 1000);

          var topicDataService = SetupPMIntervalServiceTest("{0}_US8998.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          var actualIntervals = (from interval in ctx.PMIntervalReadOnly.Include("SalesModel")
                                 from catInterval in
                                   (from catInterval in ctx.PMCATIntervalReadOnly where catInterval.PMInterval.ID == interval.ID select catInterval).DefaultIfEmpty()
                                 where interval.SalesModel.ExternalID == salesModel.ExternalID
                                 && interval.IsCumulative && !interval.IsMetric
                                 orderby interval.Rank
                                 select new { interval = interval, catInterval = catInterval }).ToList();

          Assert.AreEqual("PM 1", actualIntervals[0].interval.Title, "The first cumulative interval should be PM 1.");
          Assert.AreEqual("PM 1 (250 HOUR INTERVAL) ENGINE OIL CHANGE", actualIntervals[0].interval.Description, "The Description for PM 1 should be 'PM 1 (250 HOUR INTERVAL) ENGINE OIL CHANGE'.");
          Assert.AreEqual(1, actualIntervals[0].interval.Rank, "The Rank for PM 1 should be 1.");
          Assert.AreEqual("7501", actualIntervals[0].catInterval.CompCode, "The Comp Code should be 7501.");
          Assert.AreEqual("PM 1", actualIntervals[0].catInterval.SMCSDescription, "The SMCS Description should be 'PM 1'.");
          TestCommonCumulativeFields(actualIntervals[0].interval, actualIntervals[0].catInterval);

          Assert.AreEqual("PM 2", actualIntervals[1].interval.Title, "The second cumulative interval should be PM 2.");
          Assert.AreEqual("PM 2 (500 HOUR INTERVAL)", actualIntervals[1].interval.Description, "The Description for PM 2 should be 'PM 2 (500 HOUR INTERVAL)'.");
          Assert.AreEqual(2, actualIntervals[1].interval.Rank, "The rank for PM 2 should be 2.");
          Assert.AreEqual("7502", actualIntervals[1].catInterval.CompCode, "The Comp Code should be 7502.");
          Assert.AreEqual("PM 2", actualIntervals[1].catInterval.SMCSDescription, "The SMCS Description should be 'PM 2'.");
          TestCommonCumulativeFields(actualIntervals[1].interval, actualIntervals[1].catInterval);

          Assert.AreEqual("PM 3", actualIntervals[2].interval.Title, "The third cumulative interval should be PM 3.");
          Assert.AreEqual("PM 3 (1000 HOUR INTERVAL)", actualIntervals[2].interval.Description, "The Comp Description for PM 3 should be 'PM 3 (1000 HOUR INTERVAL)'.");
          Assert.AreEqual(3, actualIntervals[2].interval.Rank, "The rank for PM 3 should be 3.");
          Assert.AreEqual("7503", actualIntervals[2].catInterval.CompCode, "The Comp Code should be 7503.");
          Assert.AreEqual("PM 3", actualIntervals[2].catInterval.SMCSDescription, "The SMCS Description should be 'PM 3'.");
          TestCommonCumulativeFields(actualIntervals[2].interval, actualIntervals[2].catInterval);

          Assert.AreEqual("PM 4", actualIntervals[3].interval.Title, "The fourth cumulative interval should be PM 4.");
          Assert.AreEqual("PM 4 (2000 HOUR INTERVAL)", actualIntervals[3].interval.Description, "The Description for PM 4 should be 'PM 4 (2000 HOUR INTERVAL)'.");
          Assert.AreEqual(4, actualIntervals[3].interval.Rank, "The rank for PM 4 should be 4.");
          Assert.AreEqual("7504", actualIntervals[3].catInterval.CompCode, "The Comp Code should be 7504.");
          Assert.AreEqual("PM 4", actualIntervals[3].catInterval.SMCSDescription, "The SMCS Description should be 'PM 4'.");
          TestCommonCumulativeFields(actualIntervals[3].interval, actualIntervals[3].catInterval);
        }
      }
    }

    public void TestCommonCumulativeFields(PMInterval interval, PMCATInterval catInterval)
    {
      Assert.IsTrue(interval.IsCumulative, "The PM 1 should be a cumulative interval.");
      Assert.AreEqual("PERFORM", catInterval.JobDescription, "The Job Description should be 'PERFORM'.");
      Assert.AreEqual("540", catInterval.JobCode, "The Job Code should be 540.");
      Assert.IsNull(catInterval.QuantityCode, "The Quantity Code should be null.");
      Assert.IsNull(catInterval.QuantityDescription, "The Quantity Description should be null.");
      Assert.IsNull(catInterval.JobCondCode, "The Job Cond Code should be null.");
      Assert.IsNull(catInterval.JobCondDescription, "The Job Cond Description should be null.");
      Assert.IsNull(catInterval.JobLocationCode, "The Job Location Code should be null.");
      Assert.IsNull(catInterval.JobLocationDescription, "The Job Location Description should be null.");
      Assert.IsNull(catInterval.ModCode, "The Mod Code should be null.");
      Assert.IsNull(catInterval.ModDescription, "The Mod Description should be null.");
      Assert.IsNull(catInterval.CabTypeCode, "The Cab Type Code should be null.");
      Assert.IsNull(catInterval.CabTypeDescription, "The Cab Type Description should be null.");
      Assert.IsNull(catInterval.WAppCode, "The WApp Code should be null.");
      Assert.IsNull(catInterval.WAppDescription, "The WApp Description should be null.");
    }

    [TestMethod()]
    public void TestPMCatIncrementalIntervalDetails()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          SalesModel salesModel = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY", "modelcode", "XXX", 1000);

          var topicDataService = SetupPMIntervalServiceTest("{0}_US8998.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          var incrementalIntervals = (from interval in ctx.PMIntervalReadOnly.Include("SalesModel")
                                      from catInterval in
                                        (from catInterval in ctx.PMCATIntervalReadOnly where catInterval.PMInterval.ID == interval.ID select catInterval).DefaultIfEmpty()
                                      where interval.SalesModel.ExternalID == salesModel.ExternalID
                                      && !interval.IsCumulative && !interval.IsMetric
                                      orderby interval.Title
                                      select new { interval = interval, catInterval = catInterval }).ToList();

          Assert.AreEqual("PM 1F", incrementalIntervals[0].interval.Title, "The first incremental interval should be PM 1F.");
          Assert.AreEqual("PM 1F (250 HOUR INITIAL INTERVAL)", incrementalIntervals[0].interval.Description, "The Description for PM 1F should be 'PM 1F (250 HOUR INITIAL INTERVAL)'.");
          Assert.AreEqual("7501", incrementalIntervals[0].catInterval.CompCode, "The Comp Code should be 7501.");
          Assert.AreEqual("PM 1", incrementalIntervals[0].catInterval.SMCSDescription, "The SMCS Description should be 'PM 1'.");
          TestCommonIncrementalFields(incrementalIntervals[0].interval, incrementalIntervals[0].catInterval);

          Assert.AreEqual("PM 2F", incrementalIntervals[1].interval.Title, "The second incremental interval should be PM 1.");
          Assert.AreEqual("PM 2F (500 HOUR INITIAL INTERVAL)", incrementalIntervals[1].interval.Description, "The Description for PM 2F should be 'PM 2F (500 HOUR INITIAL INTERVAL)'.");
          Assert.AreEqual("7502", incrementalIntervals[1].catInterval.CompCode, "The Comp Code should be 7502.");
          Assert.AreEqual("PM 2", incrementalIntervals[1].catInterval.SMCSDescription, "The SMCS Description should be 'PM 2'.");
          TestCommonIncrementalFields(incrementalIntervals[1].interval, incrementalIntervals[1].catInterval);

          Assert.AreEqual("PM12000", incrementalIntervals[2].interval.Title, "The third incremental interval should be PM12000.");
          Assert.AreEqual("PM12000 (12000 HOUR INTERVAL)", incrementalIntervals[2].interval.Description, "The Description for PM12000 should be 'PM12000 (12000 HOUR INTERVAL)'.");
          Assert.AreEqual("7548", incrementalIntervals[2].catInterval.CompCode, "The Comp Code should be 7548.");
          Assert.AreEqual("12000 SERVICE HOUR MAINTENANCE", incrementalIntervals[2].catInterval.SMCSDescription, "The SMCS Description should be '12000 SERVICE HOUR MAINTENANCE'.");
          TestCommonIncrementalFields(incrementalIntervals[2].interval, incrementalIntervals[2].catInterval);

          Assert.AreEqual("PM3000", incrementalIntervals[3].interval.Title, "The fourth incremental interval should be PM3000.");
          Assert.AreEqual("PM3000 (3000 HOUR INTERVAL)", incrementalIntervals[3].interval.Description, "The Description for PM3000 should be 'PM3000 (3000 HOUR INTERVAL)'.");
          Assert.AreEqual("7545", incrementalIntervals[3].catInterval.CompCode, "The Comp Code should be 7545.");
          Assert.AreEqual("3000 SERVICE HOUR MAINTENANCE", incrementalIntervals[3].catInterval.SMCSDescription, "The SMCS Description should be '3000 SERVICE HOUR MAINTENANCE'.");
          TestCommonIncrementalFields(incrementalIntervals[3].interval, incrementalIntervals[3].catInterval);

          Assert.AreEqual("PM5000", incrementalIntervals[4].interval.Title, "The fifth incremental interval should be PM5000.");
          Assert.AreEqual("PM5000 (5000 HOUR INTERVAL)", incrementalIntervals[4].interval.Description, "The Description for PM5000 should be 'PM5000 (5000 HOUR INTERVAL)'.");
          Assert.AreEqual("7546", incrementalIntervals[4].catInterval.CompCode, "The Comp Code should be 7546.");
          Assert.AreEqual("5000 SERVICE HOUR MAINTENANCE", incrementalIntervals[4].catInterval.SMCSDescription, "The SMCS Description should be '5000 SERVICE HOUR MAINTENANCE'.");
          TestCommonIncrementalFields(incrementalIntervals[4].interval, incrementalIntervals[4].catInterval);

          Assert.AreEqual("PM6000", incrementalIntervals[5].interval.Title, "The sixth incremental interval should be PM6000.");
          Assert.AreEqual("PM6000 (6000 HOUR INTERVAL) HYDRAULIC OIL CHANGE", incrementalIntervals[5].interval.Description, "The Description for PM6000 should be 'PM6000 (6000 HOUR INTERVAL) HYDRAULIC OIL CHANGE'.");
          Assert.AreEqual("7543", incrementalIntervals[5].catInterval.CompCode, "The Comp Code should be 7543.");
          Assert.AreEqual("6000 SERVICE HOUR MAINTENANCE", incrementalIntervals[5].catInterval.SMCSDescription, "The SMCS Description should be '6000 SERVICE HOUR MAINTENANCE'.");
          TestCommonIncrementalFields(incrementalIntervals[5].interval, incrementalIntervals[5].catInterval);
        }
      }
    }

    public void TestCommonIncrementalFields(PMInterval interval, PMCATInterval catInterval)
    {
      Assert.AreEqual(0, interval.Rank, "The Rank should be 0.");
      Assert.IsFalse(interval.IsCumulative, "The PM should be an incremental interval.");
      Assert.AreEqual("PERFORM", catInterval.JobDescription, "The Job Description should be 'PERFORM'.");
      Assert.AreEqual("540", catInterval.JobCode, "The Job Code should be 540.");
      if (catInterval.QuantityCode != null && catInterval.QuantityCode.Equals("F"))
      {
        Assert.AreEqual("FIRST ONE", catInterval.QuantityDescription, "The Quantity Description should be 'FIRST ONE'.");
        Assert.AreEqual("F", catInterval.QuantityCode, "The Quantity Code should be 'F'.");
      }
      else
      {
        Assert.IsNull(catInterval.QuantityCode, "The Quantity Code should be null.");
        Assert.IsNull(catInterval.QuantityDescription, "The Quantity Description should be null.");
      }
      Assert.IsNull(catInterval.JobCondCode, "The Job Cond Code should be null.");
      Assert.IsNull(catInterval.JobCondDescription, "The Job Cond Description should be null.");
      Assert.IsNull(catInterval.JobLocationCode, "The Job Location Code should be null.");
      Assert.IsNull(catInterval.JobLocationDescription, "The Job Location Description should be null.");
      Assert.IsNull(catInterval.ModCode, "The Mod Code should be null.");
      Assert.IsNull(catInterval.ModDescription, "The Mod Description should be null.");
      Assert.IsNull(catInterval.CabTypeCode, "The Cab Type Code should be null.");
      Assert.IsNull(catInterval.CabTypeDescription, "The Cab Type Description should be null.");
      Assert.IsNull(catInterval.WAppCode, "The WApp Code should be null.");
      Assert.IsNull(catInterval.WAppDescription, "The WApp Description should be null.");
    }

    [TestMethod()]
    public void TestPMTitleParsing()
    {
      var topicDataService = SetupPMIntervalServiceTest("{0}.txt", "ReplayDevices.txt");

      TestPMTitle(topicDataService.TopicProcessor, String.Empty, String.Empty);
      TestPMTitle(topicDataService.TopicProcessor, "PM 2 FLUIDS (500 HOUR INTERVAL) FLUIDS ONLY", "PM 2");
      TestPMTitle(topicDataService.TopicProcessor, "PM 3 SOS (1000 HOUR INTERVAL) SOS ONLY", "PM 3 SOS");
      TestPMTitle(topicDataService.TopicProcessor, "PM4000 (4000 HOUR INTERVAL)", "PM4000");
      TestPMTitle(topicDataService.TopicProcessor, "PM 1 (250 HOUR INTERVAL)", "PM 1");
      TestPMTitle(topicDataService.TopicProcessor, "PM 2F (500 HOUR INTERVAL)", "PM 2F");
      TestPMTitle(topicDataService.TopicProcessor, "PM 2 FLUIDS 500 HOUR INTERVAL FLUIDS ONLY", "PM 2");
      TestPMTitle(topicDataService.TopicProcessor, "PM4000 METRIC (4000 HOUR INTERVAL) HYDRAULIC OIL CHANGE", "PM4000");
      TestPMTitle(topicDataService.TopicProcessor, "PM4000 FLUIDS (4000 HOUR INTERVAL) FLUIDS ONLY", "PM4000");
      TestPMTitle(topicDataService.TopicProcessor, "PM4000 MTX (4000 HOUR INTERVAL) HYDRAULIC OIL CHANGE", "PM4000");
      TestPMTitle(topicDataService.TopicProcessor, "PM120000000 (12000 HOUR INTERVAL)", "PM120000");
      TestPMTitle(topicDataService.TopicProcessor, "ID 670 PM6000 (6000 HOUR INTERVAL)", "ID 670");
      TestPMTitle(topicDataService.TopicProcessor, "PM6000 (6000 HOUR INTERVAL)", "PM6000");
    }

    public void TestPMTitle(TopicProcessor processor, string inputTitle, string expectedTitle)
    {
      PMIntervalProcessor pmProcessor = (PMIntervalProcessor)processor;
      string outputTitle = pmProcessor.GetPMTitleFromDescription(inputTitle);
      Assert.AreEqual(expectedTitle, outputTitle, "The expected title did not match the output title.");
    }

    [TestMethod()]
    public void TestPMChecklistAndPartsSync()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          PMInterval expectedInterval = CreateDefaultPMInterval(ctx, "PM", 250, 500, false, false, false, "Description", 2000, false, 0);

          var topicDataService = SetupPMChecklistService("{0}.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          List<PMCheckListStep> actualChecklistSteps = (from i in ctx.PMCheckListStepReadOnly.Include("PMPart").Include("PMInterval")
                                                        where i.PMInterval.Any(f => f.ExternalID == expectedInterval.ExternalID)
                                                        select i).ToList();

          Assert.AreEqual(3, actualChecklistSteps.Count, "Counts do not match for expected-actual lists");
          Assert.AreEqual(expectedInterval.ExternalID, actualChecklistSteps[0].PMInterval.FirstOrDefault().ExternalID, "Interval ExternalID is incorrect");
          Assert.IsFalse(actualChecklistSteps[0].IsCustom, "ChecklistStep shouldn't be custom");
          Assert.IsFalse(actualChecklistSteps[2].IsDeleted, "ChecklistStep shouldn't be deleted");

          Assert.AreEqual("TAKE & ANALYZE S-O-S SAMPLE FR ENGINE OIL", actualChecklistSteps[0].Title, "First interval Title is incorrect");
          Assert.AreEqual("PERFORM PM 1", actualChecklistSteps[2].Title, "Third interval Title is incorrect");

          Assert.AreEqual(3000, actualChecklistSteps[0].ExternalID, "First ChecklistStep ExternalID is incorrect");
          Assert.AreEqual(3002, actualChecklistSteps[2].ExternalID, "Third ChecklistStep ExternalID is incorrect");

          Assert.AreEqual(3, actualChecklistSteps[0].PMPart.Count(), "First ChecklistStep part count is incorrect");
          Assert.AreEqual(1, actualChecklistSteps[2].PMPart.Count(), "Third ChecklistStep part count is incorrect");

          List<PMPart> parts = actualChecklistSteps[0].PMPart.ToList();

          Assert.IsFalse(actualChecklistSteps[0].PMPart.ToList()[0].IsCustom, "Part shouldn't be custom");
          Assert.IsFalse(actualChecklistSteps[0].PMPart.ToList()[0].IsDeleted, "Part shouldn't be deleted");
          Assert.AreEqual(2.2, actualChecklistSteps[0].PMPart.ToList()[2].Quantity, "Part Quantity is incorrect");
          Assert.AreEqual("Monkey Wrench", actualChecklistSteps[0].PMPart.ToList()[2].PartNumber, "PartNumber is incorrect");
          Assert.AreEqual("I'm a monkey wrench", actualChecklistSteps[0].PMPart.ToList()[2].Title, "Part Title is incorrect");
          Assert.AreEqual("Throw me in the works & turn", actualChecklistSteps[0].PMPart.ToList()[2].PartNotes, "PartNotes is incorrect");
        }
      }
    }

    [TestMethod()]
    public void TestPMCATChecklistDetails()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          PMInterval expectedInterval = CreateDefaultPMInterval(ctx, "PM", 250, 500, false, false, false, "Description", 2000, false, 0);

          var topicDataService = SetupPMChecklistService("{0}.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          var actualChecklistSteps = (from step in ctx.PMCheckListStepReadOnly.Include("SalesModel").Include("PMPart").Include("PMInterval")
                                      from catStep in
                                        (from catStep in ctx.PMCATCheckListStepReadOnly where catStep.PMCheckListStep.ID == step.ID select catStep).DefaultIfEmpty()
                                      where step.PMInterval.Any(f => f.ExternalID == expectedInterval.ExternalID)
                                      orderby step.Title
                                      select new { step = step, catStep = catStep }).ToList();

          Assert.AreEqual("CHECK FINAL DRIVE FLUID LEVEL", actualChecklistSteps[0].step.Title, "The first checklist step should be 'CHECK FINAL DRIVE FLUID LEVEL'.");
          Assert.AreEqual(3001, actualChecklistSteps[0].step.ExternalID, "The external ID should be 3001.");
          Assert.AreEqual("4050", actualChecklistSteps[0].catStep.CompCode, "The Comp Code should be 4050.");
          Assert.AreEqual("535", actualChecklistSteps[0].catStep.JobCode, "The Job Code should be 535.");
          Assert.AreEqual("CHECK", actualChecklistSteps[0].catStep.JobDescription, "The Job Description should be 'CHECK'.");
          Assert.AreEqual("FLV", actualChecklistSteps[0].catStep.ModCode, "The Mod Code should be 'FLV'.");
          Assert.AreEqual("FLUID LEVEL", actualChecklistSteps[0].catStep.ModDescription, "The Mod Description should be 'FLUID LEVEL'.");
          Assert.AreEqual("PM 1", actualChecklistSteps[0].catStep.ServiceIntervalGroupNumber, "The Service Interval Group Number should be 'PM 1'.");
          Assert.AreEqual("FINAL DRIVE", actualChecklistSteps[0].catStep.SMCSDescription, "The SMCS Description should be 'FINAL DRIVE'.");
          TestCommonChecklistStepFields(actualChecklistSteps[0].step, actualChecklistSteps[0].catStep);

          Assert.AreEqual("PERFORM PM 1", actualChecklistSteps[1].step.Title, "The second checklist step should be 'PERFORM PM 1'.");
          Assert.AreEqual(3002, actualChecklistSteps[1].step.ExternalID, "The external ID should be 3002.");
          Assert.AreEqual("7501", actualChecklistSteps[1].catStep.CompCode, "The Comp Code should be 7501.");
          Assert.AreEqual("540", actualChecklistSteps[1].catStep.JobCode, "The Job Code should be 540.");
          Assert.AreEqual("PERFORM", actualChecklistSteps[1].catStep.JobDescription, "The Job Description should be 'PERFORM'.");
          Assert.IsNull(actualChecklistSteps[1].catStep.ModCode, "The Mod Code should be null.");
          Assert.IsNull(actualChecklistSteps[1].catStep.ModDescription, "The Mod Description should be null.");
          Assert.AreEqual("PM 1", actualChecklistSteps[1].catStep.ServiceIntervalGroupNumber, "The Service Interval Group Number should be 'PM 1'.");
          Assert.AreEqual("PM 1", actualChecklistSteps[1].catStep.SMCSDescription, "The SMCS Description should be 'PM 1'.");
          Assert.AreEqual("999.9", actualChecklistSteps[1].catStep.QuantityCode, "The Quantity Code should be '999.9'.");
          TestCommonChecklistStepFields(actualChecklistSteps[1].step, actualChecklistSteps[1].catStep);

          Assert.AreEqual("TAKE & ANALYZE S-O-S SAMPLE FR ENGINE OIL", actualChecklistSteps[2].step.Title, "The third checklist step should be 'TAKE & ANALYZE S-O-S SAMPLE FR ENGINE OIL'.");
          Assert.AreEqual(3000, actualChecklistSteps[2].step.ExternalID, "The external ID should be 3000.");
          Assert.AreEqual("1348", actualChecklistSteps[2].catStep.CompCode, "The Comp Code should be 1348.");
          Assert.AreEqual("008", actualChecklistSteps[2].catStep.JobCode, "The Job Code should be 008.");
          Assert.AreEqual("TAKE & ANALYZE S-O-S SAMPLE FR", actualChecklistSteps[2].catStep.JobDescription, "The Job Description should be 'TAKE & ANALYZE S-O-S SAMPLE FR'.");
          Assert.IsNull(actualChecklistSteps[2].catStep.ModCode, "The Mod Code should be null.");
          Assert.IsNull(actualChecklistSteps[2].catStep.ModDescription, "The Mod Description should be null.");
          Assert.AreEqual("PM 1", actualChecklistSteps[2].catStep.ServiceIntervalGroupNumber, "The Service Interval Group Number should be 'PM 1'.");
          Assert.AreEqual("ENGINE OIL", actualChecklistSteps[2].catStep.SMCSDescription, "The SMCS Description should be 'ENGINE OIL'.");
          Assert.AreEqual("1.1", actualChecklistSteps[2].catStep.QuantityCode, "The Quantity Code should be '1.1'.");
          TestCommonChecklistStepFields(actualChecklistSteps[2].step, actualChecklistSteps[2].catStep);
        }
      }
    }

    private void TestCommonChecklistStepFields(PMCheckListStep step, PMCATCheckListStep catStep)
    {
      Assert.IsNull(catStep.JobCondCode, "The Job Cond Code should be null.");
      Assert.IsNull(catStep.JobCondDescription, "The Job Cond Description should be null.");
      Assert.IsNull(catStep.JobLocationCode, "The Job Location Code should be null.");
      Assert.IsNull(catStep.JobLocationDescription, "The Job Location Description should be null.");
      Assert.IsNull(catStep.CabTypeCode, "The Cab Type Code should be null.");
      Assert.IsNull(catStep.CabTypeDescription, "The Cab Type Description should be null.");
      Assert.IsNull(catStep.WAppCode, "The WApp Code should be null.");
      Assert.IsNull(catStep.WAppDescription, "The WApp Description should be null.");
    }

    [TestMethod()]
    public void TestPMSerialNumberPrefixUpdateSync()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          SalesModel salesModel = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY", "301.6C", "XXX", 1000);
          var topicDataService = SetupPMSerialNumberPrefixService("{0}.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          topicDataService = SetupPMSerialNumberPrefixService("{0}_Updated.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          SalesModel actual = (from sm in ctx.SalesModelReadOnly
                               where sm.ExternalID == 1001
                               select sm).FirstOrDefault();

          Assert.AreEqual("301.6C", actual.ModelCode);
        }
      }
    }

    [TestMethod()]
    public void TestPMIntervalUpdateSync()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          SalesModel salesModel = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY", "modelcode", "XXX", 1000);

          var topicDataService = SetupPMIntervalServiceTest("{0}.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          topicDataService = SetupPMIntervalServiceTest("{0}_Updated.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          List<PMInterval> actualIntervals = (from i in ctx.PMIntervalReadOnly.Include("SalesModel")
                                              where i.SalesModel.ExternalID == 1000
                                              select i).ToList();

          Assert.AreEqual(10, actualIntervals.Count, "Counts do not match for expected-actual lists");
          Assert.IsFalse(actualIntervals[0].IsMetric, "First interval shouldn't be metric");
          Assert.IsFalse(actualIntervals[3].IsMetric, "4th interval shouldn't be metric");
          Assert.IsTrue(actualIntervals[4].IsMetric, "5th interval should be metric");
          Assert.IsTrue(actualIntervals[9].IsMetric, "10th interval should be metric");

          Assert.AreEqual("PM 1", actualIntervals[0].Title, "First interval Title is incorrect");
          Assert.AreEqual("PM4000", actualIntervals[3].Title, "4th interval Title is incorrect");
          Assert.AreEqual("PM 1", actualIntervals[4].Title, "5th interval Title is incorrect");
          Assert.AreEqual("PM4000", actualIntervals[7].Title, "8th interval Title is incorrect");

          Assert.AreEqual(350, actualIntervals[0].FirstInterval, "First interval FirstInterval is incorrect");
          Assert.AreEqual(4000, actualIntervals[3].FirstInterval, "4th interval FirstInterval is incorrect");
          Assert.AreEqual(350, actualIntervals[4].FirstInterval, "5th interval FirstInterval is incorrect");
          Assert.AreEqual(4000, actualIntervals[7].FirstInterval, "8th FirstInterval is incorrect");

          Assert.AreEqual(700, actualIntervals[0].NextInterval, "First interval NextInterval is incorrect");
          Assert.AreEqual(8000, actualIntervals[3].NextInterval, "Ninth interval NextInterval is incorrect");
          Assert.AreEqual(700, actualIntervals[4].NextInterval, "Tenth interval NextInterval is incorrect");
          Assert.AreEqual(8000, actualIntervals[7].NextInterval, "8th NextInterval is incorrect");

          Assert.AreEqual(8000, actualIntervals[9].NextInterval, "10th NextInterval is incorrect");
        }
      }
    }
 
    [TestMethod()]
    public void TestPMChecklistAndPartsUpdateSync()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          PMInterval expectedInterval = CreateDefaultPMInterval(ctx, "PM", 250, 500, false, false, false, "Description", 2000, false, 0);

          var topicDataService = SetupPMChecklistService("{0}.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          topicDataService = SetupPMChecklistService("{0}_Updated.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);

          List<PMCheckListStep> actualChecklistSteps =
            (from i in ctx.PMCheckListStepReadOnly.Include("PMPart").Include("PMInterval")
             where i.PMInterval.Any(f => f.ExternalID == 2000)
             select i).ToList();

          Assert.AreEqual(4, actualChecklistSteps.Count, "Counts do not match for expected-actual lists");
          Assert.AreEqual(expectedInterval.ExternalID, actualChecklistSteps[0].PMInterval.FirstOrDefault().ExternalID,
                          "Interval ExternalID is incorrect");
          Assert.IsFalse(actualChecklistSteps[0].IsCustom, "ChecklistStep shouldn't be custom");
          Assert.IsFalse(actualChecklistSteps[2].IsDeleted, "ChecklistStep shouldn't be deleted");

          Assert.AreEqual("TAKE & ANALYZE S-O-S SAMPLE FR ENGINE OIL UPDATED", actualChecklistSteps[0].Title, 
                          "First interval Title is incorrect");
          Assert.AreEqual(actualChecklistSteps[2].Title, "PERFORM PM 1 UPDATED", "Third interval Title is incorrect");

          Assert.AreEqual(3000, actualChecklistSteps[0].ExternalID, "First ChecklistStep ExternalID is incorrect");
          Assert.AreEqual(3003, actualChecklistSteps[3].ExternalID, "Third ChecklistStep ExternalID is incorrect");

          Assert.AreEqual(2, actualChecklistSteps[0].PMPart.Count(), "First ChecklistStep part count is incorrect");
          Assert.AreEqual(1, actualChecklistSteps[2].PMPart.Count(), "Third ChecklistStep part count is incorrect");
          Assert.AreEqual(1, actualChecklistSteps[3].PMPart.Count(), "Fourth ChecklistStep part count is incorrect");

          Assert.AreEqual(999.9, actualChecklistSteps[0].PMPart.ToList()[0].Quantity, "Part shouldn't be custom");
          Assert.AreEqual("SCHEDULED OIL SAMPLE & UPDATED", actualChecklistSteps[0].PMPart.ToList()[0].Title, "Part title incorrect");
          Assert.AreEqual(999.9, actualChecklistSteps[2].PMPart.ToList()[0].Quantity, "Part Quantity is incorrect");
          Assert.AreEqual("SOS-1", actualChecklistSteps[2].PMPart.ToList()[0].PartNumber,
                          "PartNumber is incorrect");
          Assert.IsFalse(actualChecklistSteps[0].PMPart.ToList()[0].IsCustom, "Part shouldn't be custom");
          Assert.IsFalse(actualChecklistSteps[0].PMPart.ToList()[0].IsDeleted, "Part shouldn't be deleted");
        }
      }
    }
    
    [TestMethod()]
    public void TestPMChecklistAndPartsDataServiceBookmark()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          SalesModel salesModel = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY", "modelcode", "XXX", 1000);
          var topicDataService = SetupPMIntervalServiceTest("{0}_US8998_Bookmark.txt", "ReplayDevices.txt");
          RunSync(topicDataService.TopicProcessor);
          
          Stack<long> externalIDs = PMChecklistAndPartsDataService.GetPMIntervalExternalIDsStartingFromBookmark(ctx, 108030);

          Assert.AreEqual( 9,externalIDs.Count(), "External ID count is incorrect.");
          Assert.AreEqual(108031,externalIDs.Pop(),  "External IDs don't match.");
          Assert.AreEqual(108034,externalIDs.Pop(),  "External IDs don't match.");
          Assert.AreEqual(108035,externalIDs.Pop(),  "External IDs don't match.");
        }
      }
    }

    [TestMethod()]
    public void TestPMIntervalDataServiceBookmark()
    {
      using (NH_OP ctx = Model.NewNHContext<NH_OP>())
      {
        using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          for (int index = 0; index < 5; index++ )
          {
            SalesModel salesModel = SalesModel.CreateSalesModel(-1, index.ToString(), index.ToString(), DateTime.UtcNow);
            salesModel.ExternalID = index;
            ctx.AddToSalesModel(salesModel);
            salesModel.ProductFamily = (from pf in ctx.ProductFamily select pf).First();
          }
          ctx.SaveChanges();

          Stack<long?> externalIDs = PMIntervalDataService.GetSalesModelExternalIDsStartingFromBookmark(ctx, 2);
          
          Assert.AreEqual(2,externalIDs.Count(),  "External ID count is incorrect.");
          Assert.AreEqual(3, externalIDs.Pop(), "External IDs don't match.");
          Assert.AreEqual(4, externalIDs.Pop(), "External IDs don't match.");
        }
      }
    }

    /// <summary>
    /// GlobalGram Enabled and Satellite Number Exists
    /// </summary>
    [TestMethod()]
    public void TestDeviceDetailsProcessorForGlobalGramEnabledAndSatelliteNumber()
    {
      using (NH_RAW ctx = Model.NewNHContext<NH_RAW>())
      {
        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          var moduleCode = "DQ0000015888Q1";

          CreateDeviceInPLDeviceTable(ctx, moduleCode, globalGramEnabled: true, satelliteNumber: 10);

          MockTopicDataService topicDataService = SetupDeviceDetailsProcessorService("{0}_GlobalGram.txt", "DeviceDetails_GlobalGram.txt");

          RunSync(topicDataService.TopicProcessor);

          var globalGramEnabledField = "Y";
          var satelliteNumber = 10;

          var plDevice = (from pldevice in ctx.PLDeviceReadOnly
                        where pldevice.ModuleCode == moduleCode
                        select pldevice).FirstOrDefault();

          bool? passedInGlobalGram = string.IsNullOrWhiteSpace(globalGramEnabledField) ? (bool?)null : 
              string.Compare(globalGramEnabledField, "Y", true) == 0 ? true : false;
          Assert.AreEqual(moduleCode, plDevice.ModuleCode, "Device must be found in the PLDevice table.");
          Assert.AreEqual(passedInGlobalGram, plDevice.GlobalgramEnabled, "Globla Gram flags should match after the sync");
          Assert.AreEqual(satelliteNumber, plDevice.SatelliteNumber, "Satellite numbers should match after the sync");
        }
      }
    }

    /// <summary>
    /// Missing Global Gram and Satellite Number
    /// </summary>
    [TestMethod()]
    public void TestDeviceDetailsProcessorForMissingGlobalGramAndSatelliteNumber()
    {
      using(NH_RAW ctx = Model.NewNHContext<NH_RAW>())
      {
        using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        {
          var moduleCode = "DQ0000015888Q1";

          CreateDeviceInPLDeviceTable(ctx, moduleCode);

          MockTopicDataService topicDataService = SetupDeviceDetailsProcessorService("{0}_MissingGlobalGram.txt", "DeviceDetails_MissingGlobalGram.txt");

          RunSync(topicDataService.TopicProcessor);
          
          
          var globalGramEnabledField = string.Empty;
          var satelliteNumber = (int?)null;

          var plDevice = (from pldevice in ctx.PLDeviceReadOnly
                            where pldevice.ModuleCode == moduleCode
                            select pldevice).FirstOrDefault();

          bool? passedInGlobalGram = string.IsNullOrWhiteSpace(globalGramEnabledField) ? (bool?)null :
            string.Compare(globalGramEnabledField, "Y", true) == 0 ? true : false;
          Assert.AreEqual(moduleCode, plDevice.ModuleCode, "Device must be found in the PLDevice table.");
          Assert.AreEqual(passedInGlobalGram, plDevice.GlobalgramEnabled, "Globla Gram flags should match after the sync");
          Assert.AreEqual(satelliteNumber, plDevice.SatelliteNumber, "Satellite numbers should match after the sync");
        }
      }
    }

    #region Helper methods

    public static SalesModel SetupTestSalesModel(NH_OP ctx, string familyName, string modelName, string serialNumberPrefix, long? externalID)
    {
      return SetupTestSalesModel(ctx, familyName, modelName, serialNumberPrefix, 1, 99999, externalID);
    }

    public static SalesModel SetupTestSalesModel(NH_OP ctx, string familyName, string modelName, string serialNumberPrefix, int startRange, int endRange, long? externalID)
    {
      ProductFamily productFamily = ProductFamily.CreateProductFamily(-1, familyName, DateTime.UtcNow);
      ctx.AddToProductFamily(productFamily);
      ctx.SaveChanges();
      productFamily = (from pf in ctx.ProductFamily
                       where pf.Name == familyName
                       select pf).FirstOrDefault();

      SalesModel salesModel = SalesModel.CreateSalesModel(-1, modelName, serialNumberPrefix, DateTime.UtcNow);
      salesModel.ProductFamily = productFamily;
      salesModel.StartRange = startRange;
      salesModel.EndRange = endRange;
      ctx.AddToSalesModel(salesModel);
      ctx.SaveChanges();
      salesModel = (from sm in ctx.SalesModel.Include("ProductFamily")
                    where sm.SerialNumberPrefix == serialNumberPrefix
                    && sm.ProductFamily.ID == productFamily.ID
                    && sm.StartRange == startRange
                    && sm.EndRange == endRange
                    select sm).FirstOrDefault();
      salesModel.ExternalID = externalID;
      ctx.SaveChanges();

      return salesModel;
    }

    public static SalesModel SetupTestSalesModel(NH_OP ctx, string modelName, string serialNumberPrefix, int startRange, int endRange, long? externalID)
    {
      ProductFamily productFamily = (from pf in ctx.ProductFamily
                       where pf.Name == "TestFamily"
                       select pf).FirstOrDefault();

      SalesModel salesModel = SalesModel.CreateSalesModel(-1, modelName, serialNumberPrefix, DateTime.UtcNow);
      salesModel.ProductFamily = productFamily;
      salesModel.StartRange = startRange;
      salesModel.EndRange = endRange;
      ctx.AddToSalesModel(salesModel);
      ctx.SaveChanges();
      salesModel = (from sm in ctx.SalesModel.Include("ProductFamily")
                    where sm.SerialNumberPrefix == serialNumberPrefix
                    && sm.ProductFamily.ID == productFamily.ID
                    && sm.StartRange == startRange
                    && sm.EndRange == endRange
                    select sm).FirstOrDefault();
      salesModel.ExternalID = externalID;
      ctx.SaveChanges();

      return salesModel;
    }

    public static MockTopicDataService SetupPMSerialNumberPrefixService(string fileName, string testFileName)
    {
      string fileRoot = GetFileRoot(typeof(PMSerialNumberPrefixProcessor).Name);

      string replayFilePath = GetFullTestFilePathAndVerifyExistence(string.Format(testFileName));
      Dictionary<long, string> replayDeviceList = SetupAssetSubscriptonsForNHSyncTests(replayFilePath);

      string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format(fileName, fileRoot));
      var topicDataService = new MockTopicDataService(testFilePath, new MockDataServiceForwardReplayStrategy(testFilePath));

      var processor = new PMSerialNumberPrefixProcessor(topicDataService, new MockMakeDataService());
      topicDataService.TopicProcessor = processor;

      return topicDataService;
    }

    public static MockTopicDataService SetupPMIntervalServiceTest(string fileName, string testFileName)
    {
      string fileRoot = GetFileRoot(typeof(PMIntervalProcessor).Name);

      string replayFilePath = GetFullTestFilePathAndVerifyExistence(string.Format(testFileName));
      Dictionary<long, string> replayDeviceList = SetupAssetSubscriptonsForNHSyncTests(replayFilePath);

      string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format(fileName, fileRoot));
      var topicDataService = new MockTopicDataService(testFilePath, new MockDataServiceForwardReplayStrategy(testFilePath));

      topicDataService.TopicProcessor = new PMIntervalProcessor(topicDataService, new MockMakeDataService());

      return topicDataService;
    }

    public static MockTopicDataService SetupPMChecklistService(string fileName, string testFileName)
    {
      string fileRoot = GetFileRoot(typeof(PMChecklistAndPartsProcessor).Name);

      string replayFilePath = GetFullTestFilePathAndVerifyExistence(string.Format(testFileName));
      Dictionary<long, string> replayDeviceList = SetupAssetSubscriptonsForNHSyncTests(replayFilePath);

      string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format(fileName, fileRoot));
      var topicDataService = new MockTopicDataService(testFilePath, new MockDataServiceForwardReplayStrategy(testFilePath));

      var processor = new PMChecklistAndPartsProcessor(topicDataService, new MockMakeDataService());
      topicDataService.TopicProcessor = processor;

      return topicDataService;
    }

    public static MockTopicDataService SetupDeviceDetailsProcessorService(string fileName, string testFileName)
    {
      string fileRoot = GetFileRoot(typeof(DeviceDetailsProcessor).Name);

      string replayFilePath = GetFullTestFilePathAndVerifyExistence(string.Format(testFileName));
      Dictionary<long, string> replayDeviceList = SetupAssetSubscriptonsForNHSyncTests(replayFilePath);

      string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format(fileName, fileRoot));
      var topicDataService = new MockTopicDataService(testFilePath, new MockDataServiceForwardReplayStrategy(testFilePath));

      var processor = new DeviceDetailsProcessor(topicDataService, new MockMakeDataService());
      topicDataService.TopicProcessor = processor;

      return topicDataService;
    }

    public static void RunSync(TopicProcessor processor)
    {
      processor.StartSync();
      processor.Sync();
      processor.CompleteSync();
    }

    public static PMInterval CreateDefaultPMInterval(NH_OP ctx, string title, double firstInterval, double nextInterval, bool isCustom, bool isDeleted, bool isMetric, string description, long externalID, bool isCumulative, Int16 rank)
    {
      SalesModel salesModel = SetupTestSalesModel(ctx, "TEST PRODUCT FAMILY", "modelcode", "XXX", 1000);
      return CreatePMInterval(ctx, title, firstInterval, nextInterval, isCustom, isDeleted, DateTime.UtcNow, isMetric, description, isCumulative, rank ,salesModel, PMTrackingTypeEnum.RuntimeHours, externalID);
    }

    public static PMInterval CreatePMInterval(NH_OP ctx, string title, double firstInterval, double nextInterval, bool isCustom, bool isDeleted, DateTime updateUTC, bool isMetric, string description, bool isCumulative, Int16 rank, SalesModel salesModel, PMTrackingTypeEnum trackingType, long externalID)
    {
      PMInterval expectedInterval = PMInterval.CreatePMInterval(-1, title, firstInterval, nextInterval, isCustom, isDeleted, updateUTC, isMetric, isCumulative, rank);
      expectedInterval.Description = description;
      expectedInterval.SalesModel = salesModel;
      expectedInterval.PMTrackingType = (from tt in ctx.PMTrackingType where tt.ID == (int)trackingType select tt).FirstOrDefault();
      ctx.AddToPMInterval(expectedInterval);
      ctx.SaveChanges();
      expectedInterval = (from i in ctx.PMInterval where i.Title == title select i).FirstOrDefault();
      expectedInterval.ExternalID = externalID;
      ctx.SaveChanges();

      return expectedInterval;
    }

    public static string GetFullTestFilePathAndVerifyExistence(string filename)
    {
      string testFilePath = Path.Combine(devFilePath, filename);
      Assert.IsTrue(File.Exists(testFilePath), testFilePath);
      System.Diagnostics.Debug.WriteLine(string.Format("UnitTestFile = '{0}'", testFilePath));

      return testFilePath;
    }

    public static void LoadTopicProcessorFileRootMap()
    {
      // This method was added as I, jjm, was introducing the topic data service refactoring - the test file names
      // had three ways of being loaded, i combined them all into the pattern of getting the root by the type of 
      // processor rather than using the topic name, which is not available at creation time as private constant but 
      // not static in some of them, but assigned at creation in others.

      if (m_processorToFileRootMap == null)
      {
        m_processorToFileRootMap = new Dictionary<string, string>();
      }

      if (m_processorToFileRootMap.Count <= 0)
      {
        m_processorToFileRootMap.Add("empty", "EmptyTestData");
        m_processorToFileRootMap.Add(typeof(DeviceDetailsProcessor).Name, "DEVICEDETAILS");
        m_processorToFileRootMap.Add(typeof(DiagnosticDescriptionProcessor).Name, "FMILIST");
        m_processorToFileRootMap.Add(typeof(DiagnosticProcessor).Name, "DIAGNOSTIC");
        m_processorToFileRootMap.Add(typeof(EngineParametersProcessor).Name, "ENGINE");
        m_processorToFileRootMap.Add(typeof(FenceAlertProcessor).Name, "FENCEALERT");
        m_processorToFileRootMap.Add(typeof(FuelProcessor).Name, "FUEL");
        m_processorToFileRootMap.Add(typeof(MachineComponentDescriptionProcessor).Name, "MIDLIST");
        m_processorToFileRootMap.Add(typeof(MakeProcessor).Name, "ALLMAKE");
        m_processorToFileRootMap.Add(typeof(PLEventDescriptionProcessor).Name, "EIDLIST");
        m_processorToFileRootMap.Add(typeof(PLEventProcessor).Name, "EVENT");
        m_processorToFileRootMap.Add(typeof(ProductFamilyDescriptionProcessor).Name, "PRODUCT");
        m_processorToFileRootMap.Add(typeof(SmuLocationProcessor).Name, "SMULOC2");
        m_processorToFileRootMap.Add(typeof(ManualSMUProcessor).Name, "MANUALSMU");
        m_processorToFileRootMap.Add(typeof(StartStopProcessor).Name, "STARTSTOP");
        m_processorToFileRootMap.Add(typeof(SubscriptionProcessor).Name, "ALLSUBS");
        m_processorToFileRootMap.Add(typeof(EventReactionProcessor).Name, "EVENTREACTION");
        m_processorToFileRootMap.Add(typeof(PMSerialNumberPrefixProcessor).Name, "PMPREFIX");
        m_processorToFileRootMap.Add(typeof(PMIntervalProcessor).Name, "PMINTERVAL");
        m_processorToFileRootMap.Add(typeof(PMChecklistAndPartsProcessor).Name, "PMCHECKLIST");
        m_processorToFileRootMap.Add(typeof(DigitalSwitchStatusProcessor).Name, "DIGSTATUS2");
      }
    }

    public static string GetFileRoot(string typeName)
    {
      string fileRoot = string.Empty;

      m_processorToFileRootMap.TryGetValue(typeName, out fileRoot);

      return fileRoot;
    }

    public static Dictionary<string, string> m_processorToFileRootMap = new Dictionary<string, string>();

    public static List<NHDataWrapper> ReadTestData(string path)
    {
      List<NHDataWrapper> eventList = new List<NHDataWrapper>();

      using (StreamReader sr = File.OpenText(path))
      {
        eventList.AddRange(DataContractDumper.ReadObject<List<NHDataWrapper>>(sr));
      }
      return eventList;
    }

    public static DateTime currentTimeUtc = DateTime.UtcNow.AddDays(-1);
    public static DateTime startCoreUTC = DateTime.UtcNow.AddDays(-3).StartOfDay();

    public static MockTopicDataService SetupSubscriptionServiceTest(SessionContext session, string fileName, string makeCode, bool assetOwnedByCustomer)
    {
      NH_OP ctx = session.NHOpContext;

      string fileRoot = GetFileRoot(typeof(SubscriptionProcessor).Name);
      string testFilePath = GetFullTestFilePathAndVerifyExistence(string.Format(fileName, fileRoot));
      var topicDataService = new MockTopicDataService(testFilePath);

      NHBssProcessor target = new NHBssProcessor();

      if (assetOwnedByCustomer)
      {
        // Setup install base to be related to Unit Test Customer
        Customer customer = (from c in ctx.Customer where c.ID == session.CustomerID.Value select c).FirstOrDefault<Customer>();
        customer.BSSID = "-1234567";
        customer.ExternalCustomerID = "-1234567";
        ctx.SaveChanges();

        Assert.IsNotNull(customer.BSSID, "BSS ID for Unit Test Customer should be found.");

        SetupDefaultCustomerAssetDevice(customer.BSSID, target, "PL321", makeCode, currentTimeUtc);
      }
      else
      {
        SetupDefaultAssetDevice(target, "PL321", makeCode, currentTimeUtc);
      }

      BSS.ServicePlan sp = NewServicePlan("Activated", startCoreUTC, "-12345", "640715", "89500-00", "1234", DateTime.MinValue, "-12333299");
      ServicePlanResult actual = target.ServicePlans(sp);
      Assert.AreEqual("Success", actual.ServiceResultList[0].Result, "Result was not successful");

      topicDataService.TopicProcessor = new SubscriptionProcessor(topicDataService, new MockMakeDataService());

      return topicDataService;
    }

    private void CreateDeviceInPLDeviceTable(NH_RAW ctx, string moduleCode, bool? globalGramEnabled = null, int? satelliteNumber = null)
    {
      PLDevice pl = PLDevice.CreatePLDevice(moduleCode, DateTime.UtcNow, true, true);
      pl.DeviceStateReference.EntityKey = Model.GetEntityKey<DeviceState>(ctx, 2);
      pl.GlobalgramEnabled = globalGramEnabled;
      pl.SatelliteNumber = satelliteNumber;
      ctx.AddToPLDevice(pl);
      ctx.SaveChanges();
    }

    #endregion
  }
}
