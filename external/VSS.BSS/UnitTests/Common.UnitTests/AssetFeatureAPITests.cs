using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

using VSS.Hosted.VLCommon.Services.Types;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.NHOPSvc.ConfigStatus;

namespace UnitTests
{
  [TestClass]
  public class AssetFeatureAPITests : UnitTestBase
  {
    [DatabaseTest]
    [TestMethod]
    public void AssetFeatureAPI_AssetFeaturesEnabledMultipleMts522s_ReturnsValidResult()
    {
      Customer owner = TestData.TestAccount;

      Asset essentials = Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner.BSSID).Save())
                                     .WithCoreService().Save();

      Asset health = Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner.BSSID).Save())
                                 .WithCoreService().WithService(ServiceTypeEnum.StandardHealth).Save();

      Asset util = Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner.BSSID).Save())
                               .WithCoreService().WithService(ServiceTypeEnum.StandardUtilization).Save();

      Asset maint = Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner.BSSID).Save())
                                .WithCoreService().WithService(ServiceTypeEnum.VLMAINT).Save();

      Asset projmon = Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner.BSSID).Save())
        .WithCoreService().WithService(ServiceTypeEnum.e2DProjectMonitoring).WithService(ServiceTypeEnum.e3DProjectMonitoring).Save();

      Asset allServices = Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner.BSSID).Save())
                                      .WithCoreService()
                                      .WithService(ServiceTypeEnum.StandardHealth)
                                      .WithService(ServiceTypeEnum.StandardUtilization)
                                      .WithService(ServiceTypeEnum.VLMAINT)
                                      .WithService(ServiceTypeEnum.e2DProjectMonitoring)
                                      .WithService(ServiceTypeEnum.e3DProjectMonitoring)
                                      .Save();

      SessionContext session = Helpers.Sessions.GetContextFor(TestData.CustomerUserActiveUser);
      
      List<long> assetIDs = new List<long>() 
      { 
        essentials.AssetID, 
        health.AssetID,
        util.AssetID,
        maint.AssetID,
        projmon.AssetID,
        allServices.AssetID
      };

      // Check asset On support
      List<AppFeatureEnum> features = new List<AppFeatureEnum>() { AppFeatureEnum.AlertAssetOn };
      Dictionary<long, List<AppFeatureEnum>> result = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetIDs, features, session.CustomerID.Value);
      Assert.AreEqual(assetIDs.Count, result.Keys.Count, "All assets should support AlertAssetOn");

      // Check AlertServiceDue support
      features = new List<AppFeatureEnum>() { AppFeatureEnum.AlertServiceDue };
      result = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetIDs, features, session.CustomerID.Value);
      Assert.AreEqual(2, result.Keys.Count, "Maintenance plan required for AlertServiceDue");

      // Check FuelLoss support
      features = new List<AppFeatureEnum>() { AppFeatureEnum.AlertFuelLoss };
      result = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetIDs, features, session.CustomerID.Value);
      Assert.AreEqual(2, result.Keys.Count, "Utilization plan required for AlertFuelLoss");

      // Check LoadDistance support
      features = new List<AppFeatureEnum>() { AppFeatureEnum.AlertLoadDistance };
      result = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetIDs, features, session.CustomerID.Value);
      Assert.AreEqual(2, result.Keys.Count, "2D Project Monitoring plan required for AlertLoadDistance");

      // Check Project Monitoring support
      features = new List<AppFeatureEnum>() { AppFeatureEnum.AlertNoValidCellPassesInTagfile };
      result = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetIDs, features, session.CustomerID.Value);
      Assert.AreEqual(0, result.Keys.Count, "SNM940 with 3D Project Monitoring plan required for AlertNoValidCellPassesInTagfile");   
    }

    [DatabaseTest]
    [TestMethod]
    public void AssetFeatureAPI_AssetFeaturesEnabledMultipleMts522sForCorporateUser_ReturnsValidResult()
    {
      Customer owner = TestData.TestCorporate;
      
      Asset essentials = Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner.BSSID).Save())
                                     .WithCoreService().SyncWithRpt().Save();

      Asset health = Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner.BSSID).Save())
                                 .WithCoreService().WithService(ServiceTypeEnum.StandardHealth).SyncWithRpt().Save();

      Asset util = Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner.BSSID).Save())
                               .WithCoreService().WithService(ServiceTypeEnum.StandardUtilization).SyncWithRpt().Save();

      Asset maint = Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner.BSSID).Save())
                                .WithCoreService().WithService(ServiceTypeEnum.VLMAINT).SyncWithRpt().Save();

      Asset allServices = Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner.BSSID).Save())
                                      .WithCoreService()
                                      .WithService(ServiceTypeEnum.StandardHealth)
                                      .WithService(ServiceTypeEnum.StandardUtilization)
                                      .WithService(ServiceTypeEnum.VLMAINT)
                                      .SyncWithRpt().Save();

      Customer owner2 = TestData.TestCustomer;

      Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner2.BSSID).Save())
                                     .WithCoreService().SyncWithRpt().Save();

      Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner2.BSSID).Save())
                                 .WithCoreService().WithService(ServiceTypeEnum.StandardHealth).SyncWithRpt().Save();

      Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner2.BSSID).Save())
                               .WithCoreService().WithService(ServiceTypeEnum.StandardUtilization).SyncWithRpt().Save();

      Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner2.BSSID).Save())
                                .WithCoreService().WithService(ServiceTypeEnum.VLMAINT).SyncWithRpt().Save();

      Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner2.BSSID).Save())
                                      .WithCoreService()
                                      .WithService(ServiceTypeEnum.StandardHealth)
                                      .WithService(ServiceTypeEnum.StandardUtilization)
                                      .WithService(ServiceTypeEnum.VLMAINT)
                                      .SyncWithRpt().Save();

      List<long> assetIDs = new List<long>() 
      { 
        essentials.AssetID, 
        health.AssetID,
        util.AssetID,
        maint.AssetID,
        allServices.AssetID
      };

      SessionContext session = Helpers.Sessions.GetContextFor(TestData.CorporateActiveUser);
      Helpers.WorkingSet.Populate(TestData.CorporateActiveUser, true);
      
      // Check asset On support
      List<AppFeatureEnum> features = new List<AppFeatureEnum>() { AppFeatureEnum.AlertAssetOn };
      Dictionary<long, List<AppFeatureEnum>> result = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetIDs, features, session.CustomerID.Value);
      Assert.AreEqual(assetIDs.Count, result.Keys.Count, "All assets should support AlertAssetOn");

      // Check AlertServiceDue support
      features = new List<AppFeatureEnum>() { AppFeatureEnum.AlertServiceDue };
      result = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetIDs, features, session.CustomerID.Value);
      Assert.AreEqual(2, result.Keys.Count, "Maintenance plan required for AlertServiceDue");

      // Check FuelLoss support
      features = new List<AppFeatureEnum>() { AppFeatureEnum.AlertFuelLoss };
      result = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetIDs, features, session.CustomerID.Value);
      Assert.AreEqual(2, result.Keys.Count, "Utilization plan required for AlertFuelLoss");
    }

     
    [DatabaseTest]
    [TestMethod]
    public void AssetFeatureAPI_AssetFeaturesEnabledVariousDevices_ReturnsValidResult()
    {
      Customer owner = TestData.TestAccount;

      Asset mts522Asset = Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(owner.BSSID).Save())
                                      .WithCoreService()
                                      .WithService(ServiceTypeEnum.StandardHealth)
                                      .WithService(ServiceTypeEnum.StandardUtilization)
                                      .WithService(ServiceTypeEnum.VLMAINT).Save();

      Asset mts521Essentials = Entity.Asset.WithDevice(Entity.Device.MTS521.OwnerBssId(owner.BSSID).Save()).WithCoreService().Save();

      Asset ple631Essentials = Entity.Asset.WithDevice(Entity.Device.PLE631.OwnerBssId(owner.BSSID).Save()).WithCoreService().Save();

      Asset pl121Essentials = Entity.Asset.WithDevice(Entity.Device.PL121.OwnerBssId(owner.BSSID).Save()).WithCoreService().Save();

      Asset dcmEssentials = Entity.Asset.WithDevice(Entity.Device.SNM940.OwnerBssId(owner.BSSID).Save()).WithCoreService().Save();

      SessionContext session = Helpers.Sessions.GetContextFor(TestData.CustomerUserActiveUser);

      List<long> assetIDs = new List<long>() 
      { 
        mts522Asset.AssetID, 
        mts521Essentials.AssetID,
        ple631Essentials.AssetID,
        pl121Essentials.AssetID,
        dcmEssentials.AssetID
      };

      // Check asset On support
      List<AppFeatureEnum> features = new List<AppFeatureEnum>() { AppFeatureEnum.AlertAssetOn };
      Dictionary<long, List<AppFeatureEnum>> result = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetIDs, features, session.CustomerID.Value);
      Assert.AreEqual(4, result.Keys.Count, "Four assets should support AlertAssetOn");
      
      // Check asset off support
      features = new List<AppFeatureEnum>() { AppFeatureEnum.AlertAssetOff };
      result = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetIDs, features, session.CustomerID.Value);
      Assert.AreEqual(3, result.Keys.Count, "Three assets should support AlertAssetOn");

      // Check FuelLoss support
      features = new List<AppFeatureEnum>() { AppFeatureEnum.AlertFuelLoss };
      result = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetIDs, features, session.CustomerID.Value);
      Assert.AreEqual(1, result.Keys.Count, "Utilization plan required for AlertFuelLoss");

      // Check AlertServiceDue support
      features = new List<AppFeatureEnum>() { AppFeatureEnum.AlertServiceDue };
      result = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetIDs, features, session.CustomerID.Value);
      Assert.AreEqual(1, result.Keys.Count, "Maintenance plan required for AlertServiceDue");
    }
    [Ignore]//  US 36186 Remove 'Asset On' Alert capability for PL321 devices
    [DatabaseTest]
    [TestMethod]
    public void AssetFeatureAPI_TestEventsAndDiagnosticsForPL321Devices()
    {
      Asset pl321Asset = Entity.Asset.WithDevice(Entity.Device.PL321.OwnerBssId(TestData.TestAccount.BSSID).Save())
                                     .WithCoreService().WithService(ServiceTypeEnum.StandardHealth).Save();

      SessionContext session = Helpers.Sessions.GetContextFor(TestData.CustomerUserActiveUser);

      List<long> assetIDs = new List<long>() 
      { 
        pl321Asset.AssetID
      };

      // Check Fault Events
      List<AppFeatureEnum> features = new List<AppFeatureEnum>() { AppFeatureEnum.AlertFaultEvents };
      Dictionary<long, List<AppFeatureEnum>> result = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetIDs, features, session.CustomerID.Value);
      Assert.AreEqual(1, result.Keys.Count, "The asset should support AlertFaultEvents");

      // Check Fault Diagnostics
      features = new List<AppFeatureEnum>() { AppFeatureEnum.AlertFaultDiagnostics };
      result = API.AssetFeature.GetAssetsThatSupportAppFeatures(assetIDs, features, session.CustomerID.Value);
      Assert.AreEqual(1, result.Keys.Count, "The asset should support AlertFaultDiagnostics");
    }

    [TestMethod]
    [DatabaseTest]
    public void GetAppFeatureEnumFromAlertType_SuccessForAlertZone()
    {
      AppFeatureEnum result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.Zone);
      Assert.AreEqual(AppFeatureEnum.AlertZone, result, "Result for input of 'Zone' should be 'AlertZone'.");

      result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.InclusionZone);
      Assert.AreEqual(AppFeatureEnum.AlertZone, result, "Result for input of 'InclusionZone' should be 'AlertZone'.");

      result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.ExclusionZone);
      Assert.AreEqual(AppFeatureEnum.AlertZone, result, "Result for input of 'ExclusionZone' should be 'AlertZone'.");
    }

    [TestMethod]
    [DatabaseTest]
    public void GetAppFeatureEnumFromAlertType_SuccessForSiteEntryExit()
    {
      AppFeatureEnum result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.SiteEntry);
      Assert.AreEqual(AppFeatureEnum.AlertSiteEntryExit, result, "Result for input of 'SiteEntry' should be 'AlertSiteEntryExit'.");

      result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.SiteExit);
      Assert.AreEqual(AppFeatureEnum.AlertSiteEntryExit, result, "Result for input of 'SiteExit' should be 'AlertSiteEntryExit'.");

      result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.SiteEntryOrExit);
      Assert.AreEqual(AppFeatureEnum.AlertSiteEntryExit, result, "Result for input of 'SiteEntryOrExit' should be 'AlertSiteEntryExit'.");
    }

    [TestMethod]
    [DatabaseTest]
    public void GetAppFeatureEnumFromAlertType_SuccessForAssetOff()
    {
      AppFeatureEnum result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.AssetOff);
      Assert.AreEqual(AppFeatureEnum.AlertAssetOff, result, "Result for input of 'AssetOff' should be 'AlertAssetOff'.");
    }

    [TestMethod]
    [DatabaseTest]
    public void GetAppFeatureEnumFromAlertType_SuccessForNonReporting()
    {
      AppFeatureEnum result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.NotReporting);
      Assert.AreEqual(AppFeatureEnum.AlertNotReporting, result, "Result for input of 'NotReporting' should be 'AlertNonReporting'.");
    }

    [TestMethod]
    [DatabaseTest]
    public void GetAppFeatureEnumFromAlertType_SuccessForFaultCodes()
    {
      AppFeatureEnum result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.FaultCodes);
      Assert.AreEqual(AppFeatureEnum.AlertFaultEvents, result, "Result for input of 'FaultCodes' should be 'AlertFaultEvents'.");
    }

    [TestMethod]
    [DatabaseTest]
    public void GetAppFeatureEnumFromAlertType_SuccessForFluidAnalysis()
    {
      AppFeatureEnum result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.FluidAnalysis);
      Assert.AreEqual(AppFeatureEnum.AlertFluidAnalysis, result, "Result for input of 'FluidAnalysis' should be 'AlertFluidAnalysis'.");
    }

    [TestMethod]
    [DatabaseTest]
    public void GetAppFeatureEnumFromAlertType_SuccessForFuelLoss()
    {
      AppFeatureEnum result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.FuelLoss);
      Assert.AreEqual(AppFeatureEnum.AlertFuelLoss, result, "Result for input of 'FuelLoss' should be 'AlertFuelLoss'.");
    }

    [TestMethod]
    [DatabaseTest]
    public void GetAppFeatureEnumFromAlertType_SuccessForServiceDue()
    {
      AppFeatureEnum result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.ServiceDue);
      Assert.AreEqual(AppFeatureEnum.AlertServiceDue, result, "Result for input of 'ServiceDue' should be 'AlertServiceDue'.");
    }

    [TestMethod]
    [DatabaseTest]
    public void GetAppFeatureEnumFromAlertType_SuccessForLoadDistance()
    {
      AppFeatureEnum result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.LoadDistance);
      Assert.AreEqual(AppFeatureEnum.AlertLoadDistance, result, "Result for input of 'LoadDistance' should be 'AlertLoadDistance'.");
    }

    [TestMethod]
    [DatabaseTest]
    public void GetAppFeatureEnumFromAlertType_SuccessForProjectMonitoring()
    {
      AppFeatureEnum result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.UnableToDetermineProjectID);
      Assert.AreEqual(AppFeatureEnum.AlertUnableToDetermineProjectID, result, "Result for input of 'UnableToDetermineProjectID' should be 'AlertUnableToDetermineProjectID'.");
      result = API.AssetFeature.GetAppFeatureEnumFromAlertType(AlertTypeEnum.NoValidCellPassesInTagfile);
      Assert.AreEqual(AppFeatureEnum.AlertNoValidCellPassesInTagfile, result, "Result for input of 'NoValidCellPassesInTagfile' should be 'AlertNoValidCellPassesInTagfile'.");
    }

    #region VLSupport API methods Test

    [TestMethod]
    [DatabaseTest]
    public void TestGetAssetSearchResults()
    {
      Asset asset1 = Entity.Asset.WithDevice(Entity.Device.PL420.OwnerBssId(TestData.TestAccount.BSSID).Save()).SerialNumberVin("RXY001")
                                    .WithCoreService().WithService(ServiceTypeEnum.StandardHealth).Save();
      Asset asset2 = Entity.Asset.WithDevice(Entity.Device.MTS522.OwnerBssId(TestData.TestAccount.BSSID).Save()).SerialNumberVin("RXY002")
                                    .WithCoreService().WithService(ServiceTypeEnum.StandardHealth).Save();
      Asset asset3 = Entity.Asset.WithDevice(Entity.Device.MTS523.OwnerBssId(TestData.TestAccount.BSSID).Save()).SerialNumberVin("MAC003")
                                    .WithCoreService().WithService(ServiceTypeEnum.StandardHealth).Save();

      List<AssetInfo> results = API.AssetFeature.GetAssetSearchResults("RXY");
      AssetInfo result1 = results.Where(t => t.assetSerialNumber == asset1.SerialNumberVIN).Select(t => t).FirstOrDefault();
      AssetInfo result2 = results.Where(t => t.assetSerialNumber == asset2.SerialNumberVIN).Select(t => t).FirstOrDefault();

      Assert.AreEqual(2, results.Count, "Incorrect Number of search results returned");
      Assert.IsNotNull(result1, "Asset 1 Not returned as part of search results");
      Assert.IsNotNull(result2, "Asset 2 Not returned as part of search results");
    }


    [TestMethod]
    [DatabaseTest]
    public void TestGetAssetDetails()
    {
      DateTime now = DateTime.UtcNow;
      Asset asset = Entity.Asset.WithDevice(Entity.Device.MTS523.OwnerBssId(TestData.TestAccount.BSSID).Save()).SerialNumberVin("MAC003")
                                    .WithCoreService().WithService(ServiceTypeEnum.StandardHealth).Save();
      Helpers.NHRpt.DimAsset_Populate();
      SetupAssetCurrentStatus(asset.AssetID, 25, now, 25, now, 25, now, 25, now);

      AssetInfo result = API.AssetFeature.GetAssetDetails(asset.AssetID);

      Assert.AreEqual(asset.AssetID,result.assetID,"Incorrect Asset Retrieved");
      Assert.AreEqual(asset.fk_DeviceID, result.fk_DeviceID, "Incorrect DeviceID Retrieved");
      Assert.AreEqual(asset.SerialNumberVIN, result.assetSerialNumber, "Incorrect SerialNumberVIN Retrieved");
      Assert.AreEqual(asset.Name, result.assetName, "Incorrect assetName Retrieved");
      Assert.AreEqual(asset.fk_MakeCode, result.make, "Incorrect make Retrieved");
      Assert.AreEqual(asset.Model, result.model, "Incorrect model Retrieved");
      Assert.AreEqual(asset.ProductFamilyName, result.productFamily, "Incorrect productFamily Retrieved");
      Assert.AreEqual((int)(DeviceTypeEnum.Series523), result.deviceTypeID, "Incorrect deviceType  Retrieved");
      Assert.AreEqual((int)DimAssetWorkingStateEnum.AssetOn, result.lastStateID, "Incorrect lastStateID Retrieved");
      Assert.AreEqual(25, result.hourMeterValue, "Incorrect hourMeterValue Retrieved");
      Assert.AreEqual(35, result.odometerValue, "Incorrect odometerValue Retrieved");
    }


    private void SetupAssetCurrentStatus(
      long assetID,
      int? fuelPercentRemaining, DateTime? lastFuelUTC,
      double? runtimeHours, DateTime? lastRuntimeUTC,
      double? enginePTOHoursMeter, DateTime? lastEnginePTOHoursMeterUTC,
      double? transmissionPTOHoursMeter, DateTime? lastTransmissionPTOHoursMeterUTC)
    {
      AssetCurrentStatus acs = (from a in Ctx.RptContext.AssetCurrentStatus
                              where a.fk_DimAssetID == assetID
                              select a).FirstOrDefault();

       acs.fk_DimAssetID = assetID;
       acs.fk_DimAssetWorkingStateID = (int)DimAssetWorkingStateEnum.AssetOn;
       acs.FuelPercentRemaining = fuelPercentRemaining;
       acs.LastFuelUTC = lastFuelUTC;
       acs.RuntimeHours = runtimeHours;
       acs.Mileage = 35;
       acs.LastRuntimeHoursUTC = lastRuntimeUTC;
       acs.EnginePTOHoursMeter = enginePTOHoursMeter;
       acs.LastEnginePTOHoursMeterUTC = lastEnginePTOHoursMeterUTC;
       acs.TransmissionPTOHoursMeter = transmissionPTOHoursMeter;
       acs.LastTransmissionPTOHoursMeterUTC = lastTransmissionPTOHoursMeterUTC;
       acs.LastReportedUTC = lastRuntimeUTC;

      Ctx.RptContext.SaveChanges();
    }

    [TestMethod]
    [DatabaseTest]
    public void TestGetActiveServicePlans()
    {
      var customer = Entity.Customer.EndCustomer.BssId("BSS123").SyncWithRpt().Save();
      var user = Entity.ActiveUser.ForUser(Entity.User.ForCustomer(customer).WithLanguage(TestData.English).Save()).Save();
      var asset = Entity.Asset.WithDevice(Entity.Device.PL421.OwnerBssId(customer.BSSID).Save())
        .WithCoreService().WithService(ServiceTypeEnum.Essentials).WithService(ServiceTypeEnum.CATHealth).ProductFamily("GENSET").SyncWithRpt().Save();

       DateTime currentDate = DateTime.UtcNow;
      int keyDate = int.Parse(currentDate.ToString("yyyymmdd"));

      List<int> result = API.AssetFeature.GetActiveServicePlans(customer.ID, asset.AssetID, keyDate, keyDate);

      Assert.AreEqual(2, result.Count, "Incorrect Number of Service Plans Retrieved");
      Assert.IsTrue(result.Contains((int)ServiceTypeEnum.CATHealth),"Incorrect Service Plan returned");
      Assert.IsTrue(result.Contains((int)ServiceTypeEnum.Essentials), "Incorrect Service Plan returned");
    }

    [TestMethod]
    [DatabaseTest]
    public void TestGetAssetIDChanges()
    {
      var customer = Entity.Customer.EndCustomer.BssId("BSS123").SyncWithRpt().Save();
      var user = Entity.ActiveUser.ForUser(Entity.User.ForCustomer(customer).WithLanguage(TestData.English).Save()).Save();
      var asset = Entity.Asset.WithDevice(Entity.Device.PL421.OwnerBssId(customer.BSSID).Save())
        .WithCoreService().WithService(ServiceTypeEnum.Essentials).WithService(ServiceTypeEnum.CATHealth).ProductFamily("GENSET").SyncWithRpt().Save();

      AssetAlias testRecord1 = new AssetAlias()
      {
        ID = 1234,
        Name = "TestAsset",
        OwnerBSSID = customer.BSSID,
        fk_AssetID = asset.AssetID,
        fk_UserID = user.fk_UserID,
        fk_CustomerID = customer.ID,
        IBKey = "TestIBKey",
        InsertUTC = DateTime.UtcNow.AddDays(-1)
      };

      AssetAlias testRecord2 = new AssetAlias()
      {
        ID = 1004,
        Name = "TestAsset2",
        OwnerBSSID = customer.BSSID,
        fk_AssetID = asset.AssetID,
        fk_UserID = user.fk_UserID,
        fk_CustomerID = customer.ID,
        IBKey = "TestIBKey",
        InsertUTC = DateTime.UtcNow.AddDays(-2)
      };

      Ctx.OpContext.AssetAlias.AddObject(testRecord1);
      Ctx.OpContext.AssetAlias.AddObject(testRecord2);
      Ctx.OpContext.SaveChanges();

      AssetAlias result = API.AssetFeature.GetAssetIDChanges(asset.AssetID);

      Assert.AreEqual(testRecord1.ID, result.ID, "Incorrect AssetAlias record Retrieved");
    }

    [TestMethod]
    [DatabaseTest]
    public void TestGetDevicePersonality()
    {
      var customer = Entity.Customer.EndCustomer.BssId("BSS123").SyncWithRpt().Save();
      var device_522 = Entity.Device.MTS522.OwnerBssId(customer.BSSID).Save();
      var Asset = Entity.Asset.WithDevice(device_522)
        .WithCoreService().WithService(ServiceTypeEnum.Essentials).WithService(ServiceTypeEnum.CATHealth).ProductFamily("GENSET").SyncWithRpt().Save();

      var personalities = API.Device.CreateDevicePersonality(
        Ctx.OpContext,
        Asset.fk_DeviceID,
        IdGen.StringId(),
        IdGen.StringId(),
        IdGen.StringId(),
        IdGen.StringId(),
        IdGen.StringId(),
        DeviceTypeEnum.Series521);

      Assert.AreEqual(4, personalities.Count, "Expecting 4 device personality objects.");

      List<DevicePersonality> results = API.AssetFeature.GetDevicePersonality(device_522.GpsDeviceID);

      Assert.AreEqual(personalities.Count, results.Count, "Incorrect number of device personalities received");

    }
        

    [TestMethod]
    [DatabaseTest]
    public void TestGetECMInfo()
    {
      var asset = TestData.TestAssetMTS522;

      List<MTSEcmInfo> ecmInfo = new List<MTSEcmInfo>();
      MTSEcmInfo mts = new MTSEcmInfo();
      mts.actingMasterECM = true;
      mts.applicationLevel1 = 15;
      mts.datalink = (int)DatalinkEnum.CDL;
      mts.diagnosticProtocolVersion = 1;
      mts.engineSerialNumbers = new string[] { "12345", "3456" };
      mts.transmissionSerialNumbers = new string[] { "54321" };
      mts.eventProtocolVersion = 1;
      mts.mid1 = "1";
      mts.serialNumber = asset.Device.GpsDeviceID;
      // If there is an ECM message with the device gps device ID for the serial number, use the software part number as the ECM Gateway Software Part Number.
      mts.softwarePartNumber = "use_this_for_ecm_gateway_software_part_number";
      mts.syncSMUClockSupported = false;
      mts.toolSupportChangeLevel1 = 27;
      mts.toolSupportChangeLevel2 = 45;
      ecmInfo.Add(mts);

      string dataLinkType = string.Empty;

      ConfigStatusSvc cssTarget = new ConfigStatusSvc();
      cssTarget.UpdateECMInfo(asset.Device.GpsDeviceID, DeviceTypeEnum.Series522, ecmInfo);
      UpdateDatalinkIDs(asset.Device.GpsDeviceID, DeviceTypeEnum.Series522);

      List<ECMDetail> results = API.AssetFeature.GetECMInfo(asset.AssetID, asset.Device.fk_DeviceTypeID, out dataLinkType);

      Assert.AreEqual(1, results.Count, "Incorrect number of ECM Detail object returned");
    }

    private void UpdateDatalinkIDs(string gpsDeviceID, DeviceTypeEnum type)
    {
      List<ECMInfo> ecm = (from e in Ctx.OpContext.ECMInfo
                           from d in Ctx.OpContext.Device
                           where d.GpsDeviceID == gpsDeviceID && d.fk_DeviceTypeID == (int)type
                           && e.fk_DeviceID == d.ID
                           select e).ToList();
      int count = 1;
      foreach (ECMInfo e in ecm)
      {
        if (e.ID <= 0)
        {
          e.ID = count;
          count++;
        }
        foreach (ECMDatalinkInfo d in e.ECMDatalinkInfo)
        {
          if (d.ID <= 0)
          {
            d.ID = count;
            count++;
          }
        }
      }
    }


    #endregion
  }
}
