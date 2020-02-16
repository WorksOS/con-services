using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{
  [TestClass()]
  public class ReportLogicTestTT : ReportLogicTestBase
  {
  
    [DatabaseTest]
    [TestMethod]
    public void RuntimesOver2Days()
    {
      // Trimtracs ONLY have HoursLocation events, and some of these contain runtimes. TT has no possiblity of determining IdleTime or Fuel.
      // TTs follow the MeterDelta method
      // DeviceDate          RuntimeMeter      RuntimeHours
      //  1 Feb 2010 08:00    1000 
      //  2 Feb 2010 11:30    1003              3      

      var target = new EquipmentAPI();
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDevice(Entity.Device.TrimTrac.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService()
                  .WithDefaultAssetUtilizationSettings()
                  .SyncWithRpt()
                  .Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Monday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day, 08, 00, 00);
      int testStartKeyDate = testStartDeviceDay.KeyDate();
      DateTime startOfDeviceDay = testStartDeviceDay;
      long? runtimeHours = 1000;
      double? latitude = 20; // GMT
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(8);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      runtimeHours = 1003;
      eventUTC = startOfDeviceDay.AddDays(1).AddHours(11).AddMinutes(30);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == asset.AssetID
                                           && aud.fk_AssetPriorKeyDate != null
                                          orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(testStartDeviceDay.AddDays(1).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartKeyDate, util[0].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(3, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.IsNull(util[0].WorkingHours, string.Format("Working hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.IsNull(util[0].IdleHours, string.Format("Idle hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
  }


    [DatabaseTest]
    [TestMethod]
    public void RuntimesOn1DayOnly()
    {
      // The case where RunTime events for only 1 day. According to the business rules
      // actual working time for a TT is not calculated unless we events for > 1 DeviceDay 24hr period
      //   DeviceDate          RuntimeMeter      RuntimeHours
      //   1 Feb 2010 08:00    1000 
      //   1 Feb 2010 11:30    1003              N/A      

      var target = new EquipmentAPI();
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDevice(Entity.Device.TrimTrac.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService()
                  .WithDefaultAssetUtilizationSettings()
                  .SyncWithRpt()
                  .Save();
      var assetUtilization = Entity.AssetExpectedRuntimeHoursProjected.ForAsset(asset).Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Monday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day, 08, 00, 00);
      int testStartKeyDate = testStartDeviceDay.KeyDate();
      DateTime startOfDeviceDay = testStartDeviceDay;
      long? runtimeHours = 1000;
      double? latitude = 20; // GMT
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(8);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      runtimeHours = 1003;
      eventUTC = startOfDeviceDay.AddHours(11).AddMinutes(30);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == asset.AssetID
                                           && aud.fk_AssetPriorKeyDate != null
                                          orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(0, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

    }
  }
}
