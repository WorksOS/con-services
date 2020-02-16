using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{
  [TestClass()]
  public class NHDataSaverTest : UnitTestBase
  {
    [TestMethod()]
    [DatabaseTest]
    [Ignore]
    public void SaveAll()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      items.Add(new NHDataWrapper { Data = GetDataSwitchState(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataSwitchState(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataEngineParameters(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataEngineParameters(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataEngineStartStop(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataEngineStartStop(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataFaultDiagnostic(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataFaultDiagnostic(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataFaultEvent(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataFaultEvent(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataFenceAlarm(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataFenceAlarm(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataHoursLocation(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataHoursLocation(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataIgnOnOff(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataIgnOnOff(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataMoving(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataMoving(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataMSSKeyID(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataMSSKeyID(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataServiceMeterAdjustment(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataServiceMeterAdjustment(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataSiteState(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataSiteState(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataSpeeding(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataSpeeding(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataPassThroughPortData(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataPassThroughPortData(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataRawCANMessage(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataRawCANMessage(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataFeedDigitalSwitchStatus(t0,1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataFeedDigitalSwitchStatus(t1,2), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataFluidAnalysis(10), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataFluidAnalysis(20), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataParametersReport(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataParametersReport(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataPowerLoss(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataPowerLoss(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataTamperSecurityStatus(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataTamperSecurityStatus(t1), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataIdleTimeOut(t0), StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = GetDataIdleTimeOut(t1), StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }


      int count = (from r in Ctx.DataContext.DataSwitchStateReadOnly
                   where r.DebugRefID == -9999
                   && r.AssetID > 0
                   select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataSwitchState");

      count = (from r in Ctx.DataContext.DataEngineParametersReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataEngineParameters");

      count = (from r in Ctx.DataContext.DataEngineStartStopReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataEngineStartStop");

      count = (from r in Ctx.DataContext.DataFaultDiagnosticReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataFaultDiagnostic");

      count = (from r in Ctx.DataContext.DataFaultEventReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataFaultEvent");

      count = (from r in Ctx.DataContext.DataFenceAlarmReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataFenceAlarm");

      count = (from r in Ctx.DataContext.DataHoursLocationReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataHoursLocation");

      count = (from r in Ctx.DataContext.DataIgnOnOffReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataIgnOnOff");

      count = (from r in Ctx.DataContext.DataMovingReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataMoving");

      count = (from r in Ctx.DataContext.DataMSSKeyIDReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataMSSKeyID");

      count = (from r in Ctx.DataContext.DataServiceMeterAdjustmentReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataServiceMeterAdjustment");

      count = (from r in Ctx.DataContext.DataSiteStateReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataSiteState");

      count = (from r in Ctx.DataContext.DataSpeedingReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataSpeeding");

      count = (from r in Ctx.DataContext.DataPassThroughPortDatasReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataPassThroughPortData");

      count = (from r in Ctx.DataContext.DataRawCANMessageReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataRawCANMessage");

      count = (from r in Ctx.DataContext.DataFeedDigitalSwitchStatusReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataFeedDigitalSwitchStatus");

      count = (from r in Ctx.DataContext.DataFluidAnalysisReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataFluidAnalysis");

      count = (from r in Ctx.DataContext.DataParametersReportReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataParametersReport");

      count = (from r in Ctx.DataContext.DataPowerStateReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataPowerState");

      count = (from r in Ctx.DataContext.DataTamperSecurityStatusReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataTamperSecurityStatus");

      count = (from r in Ctx.DataContext.DataIdleTimeOutReadOnly
               where r.DebugRefID == -9999
                   && r.AssetID > 0
               select 1).Count();
      Assert.AreEqual(2, count, "Data not saved to DataIdleTimeOut");
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_FluidAnalysis()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // Duplicate key is (assetID,sampleNumber,?actionNumber)
      DataFluidAnalysis item1 = GetDataFluidAnalysis(1);
      DataFluidAnalysis item2 = GetDataFluidAnalysis(2);
      DataFluidAnalysis item3 = GetDataFluidAnalysis(2,1);

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataFluidAnalysis> saved = (from r in Ctx.DataContext.DataFluidAnalysisReadOnly
                                       where r.DebugRefID == -9999
                                         orderby r.AssetID, r.SampleNumber, r.ActionNumber
                                       select r).ToList<DataFluidAnalysis>();
      Assert.AreEqual(3, saved.Count, "Records 1 and 2 have different sample numbers. Record 3 has same sample number as record 2, but has a different ActionNumber.");

      DataFluidAnalysis first = saved[0];
      Assert.AreEqual(1, first.SampleNumber, "Expected item1 to get stored OK.");
      DataFluidAnalysis second = saved[1];
      Assert.AreEqual(2, second.SampleNumber, "Expected item2 to get stored OK because it is for a different SampleNumber");
      Assert.IsNull(second.ActionNumber, "Item 2 should have a null ActionNumber");
      DataFluidAnalysis third = saved[2];
      Assert.AreEqual(2, third.SampleNumber, "Expected item3 to get stored OK because it is for a different ActionNumber");
      Assert.IsNotNull(third.ActionNumber, "Item 3 should have a non-null ActionNumber");
      Assert.AreEqual(1, third.ActionNumber.Value, "Expected item3 to get stored OK because it is for a different ActionNumber");
    }

    [Ignore]
    [TestMethod]
    [DatabaseTest]
    public void SaveDataCustomUtilizationEventTest()
    {
      DataCustomUtilizationEvent dataCustomUtilizationEvent = new DataCustomUtilizationEvent
                                                                {
                                                                  InsertUTC = DateTime.UtcNow,
                                                                  fk_DimSourceID = (int) DimSourceEnum.PR3Gateway,
                                                                  DebugRefID = -9999,
                                                                  SourceMsgID = 888,
                                                                  AssetID = 37421,
                                                                  EventUTC = DateTime.UtcNow,
                                                                  OEMDataSourceValue = 12,
                                                                  Value = 2.12,
                                                                  ifk_OEMDataSourceTypeID = (int)OEMDataSourceTypeEnum.MID,
                                                                  ifk_DimCustomUtilizationEventTypeID = (int)DimCustomUtilizationEventTypeEnum.Payload,
                                                                  ifk_DimUnitTypeID = (int)DimUnitTypeEnum.Tonne,
                                                                  DeviceType = DeviceTypeEnum.Series522,
                                                                  GPSDeviceID = "1234"
                                                                };

      bool retryExpected = false;
      NHDataSaver.Save(new List<INHDataObject> {dataCustomUtilizationEvent}, out retryExpected);
      Assert.IsFalse(retryExpected, "No problems expected");

      List<DataCustomUtilizationEvent> saved = (from r in Ctx.DataContext.DataCustomUtilizationEventReadOnly
                                                where r.DebugRefID == -9999 && r.AssetID == 37421
                                                select r).ToList<DataCustomUtilizationEvent>();
      Assert.AreEqual(1, saved.Count);
      Assert.AreEqual(dataCustomUtilizationEvent.AssetID, saved[0].AssetID);
      Assert.AreEqual(dataCustomUtilizationEvent.fk_DimSourceID, saved[0].fk_DimSourceID);
      Assert.AreEqual(dataCustomUtilizationEvent.DebugRefID, saved[0].DebugRefID);
      Assert.AreEqual(dataCustomUtilizationEvent.SourceMsgID, saved[0].SourceMsgID);
      Assert.AreEqual(dataCustomUtilizationEvent.EventUTC, saved[0].EventUTC);
      Assert.AreEqual(dataCustomUtilizationEvent.OEMDataSourceValue, saved[0].OEMDataSourceValue);
      Assert.AreEqual(dataCustomUtilizationEvent.Value, saved[0].Value);
      Assert.AreEqual(dataCustomUtilizationEvent.ifk_OEMDataSourceTypeID, saved[0].ifk_OEMDataSourceTypeID);
      Assert.AreEqual(dataCustomUtilizationEvent.ifk_DimCustomUtilizationEventTypeID, saved[0].ifk_DimCustomUtilizationEventTypeID);
      Assert.AreEqual(dataCustomUtilizationEvent.ifk_DimUnitTypeID, saved[0].ifk_DimUnitTypeID);
    }

    [Ignore]
    [TestMethod]
    [DatabaseTest]
    public void SaveDataCustomUtilizationEventRockBreakerRuntimeHoursTest()
    {
        DataCustomUtilizationEvent dataCustomUtilizationEvent = new DataCustomUtilizationEvent
        {
            InsertUTC = DateTime.UtcNow,
            fk_DimSourceID = (int)DimSourceEnum.PR3Gateway,
            DebugRefID = -9999,
            SourceMsgID = 888,
            AssetID = 37421,
            EventUTC = DateTime.UtcNow,
            OEMDataSourceValue = 12,
            Value = 2.12,
            ifk_OEMDataSourceTypeID = (int)OEMDataSourceTypeEnum.MID,
            ifk_DimCustomUtilizationEventTypeID = (int)DimCustomUtilizationEventTypeEnum.RockBreakerRuntimeHours,
            ifk_DimUnitTypeID = (int)DimUnitTypeEnum.Tonne,
            DeviceType = DeviceTypeEnum.Series522,
            GPSDeviceID = "1234"
        };

        bool retryExpected = false;
        NHDataSaver.Save(new List<INHDataObject> { dataCustomUtilizationEvent }, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");

        List<DataCustomUtilizationEvent> saved = (from r in Ctx.DataContext.DataCustomUtilizationEventReadOnly
                                                  where r.DebugRefID == -9999 && r.AssetID == 37421
                                                  select r).ToList<DataCustomUtilizationEvent>();
        Assert.AreEqual(1, saved.Count);
        Assert.AreEqual(dataCustomUtilizationEvent.AssetID, saved[0].AssetID);
        Assert.AreEqual(dataCustomUtilizationEvent.fk_DimSourceID, saved[0].fk_DimSourceID);
        Assert.AreEqual(dataCustomUtilizationEvent.DebugRefID, saved[0].DebugRefID);
        Assert.AreEqual(dataCustomUtilizationEvent.SourceMsgID, saved[0].SourceMsgID);
        Assert.AreEqual(dataCustomUtilizationEvent.EventUTC, saved[0].EventUTC);
        Assert.AreEqual(dataCustomUtilizationEvent.OEMDataSourceValue, saved[0].OEMDataSourceValue);
        Assert.AreEqual(dataCustomUtilizationEvent.Value, saved[0].Value);
        Assert.AreEqual(dataCustomUtilizationEvent.ifk_OEMDataSourceTypeID, saved[0].ifk_OEMDataSourceTypeID);
        Assert.AreEqual(dataCustomUtilizationEvent.ifk_DimCustomUtilizationEventTypeID, saved[0].ifk_DimCustomUtilizationEventTypeID);
        Assert.AreEqual(dataCustomUtilizationEvent.ifk_DimUnitTypeID, saved[0].ifk_DimUnitTypeID);
    }

    [Ignore]
    [TestMethod]
    [DatabaseTest]
    public void SaveDataCustomUtilizationEventStandardModeRuntimeHoursTest()
    {
      DataCustomUtilizationEvent dataCustomUtilizationEvent = new DataCustomUtilizationEvent
      {
        InsertUTC = DateTime.UtcNow,
        fk_DimSourceID = (int)DimSourceEnum.DataIn,
        DebugRefID = -9999,
        SourceMsgID = 888,
        AssetID = 37421,
        EventUTC = DateTime.UtcNow,
        OEMDataSourceValue = 12,
        Value = 2.12,
        ifk_OEMDataSourceTypeID = (int)OEMDataSourceTypeEnum.MID,
        ifk_DimCustomUtilizationEventTypeID = (int)DimCustomUtilizationEventTypeEnum.StandardModeRuntimeHours,
        ifk_DimUnitTypeID = (int)DimUnitTypeEnum.Tonne,
        DeviceType = DeviceTypeEnum.Series522,
        GPSDeviceID = "1234"
      };

      bool retryExpected = false;
      NHDataSaver.Save(new List<INHDataObject> { dataCustomUtilizationEvent }, out retryExpected);
      Assert.IsFalse(retryExpected, "No problems expected");

      List<DataCustomUtilizationEvent> saved = (from r in Ctx.DataContext.DataCustomUtilizationEventReadOnly
                                                where r.DebugRefID == -9999 && r.AssetID == 37421
                                                select r).ToList<DataCustomUtilizationEvent>();
      Assert.AreEqual(1, saved.Count);
      Assert.AreEqual(dataCustomUtilizationEvent.AssetID, saved[0].AssetID);
      Assert.AreEqual(dataCustomUtilizationEvent.fk_DimSourceID, saved[0].fk_DimSourceID);
      Assert.AreEqual(dataCustomUtilizationEvent.DebugRefID, saved[0].DebugRefID);
      Assert.AreEqual(dataCustomUtilizationEvent.SourceMsgID, saved[0].SourceMsgID);
      Assert.AreEqual(dataCustomUtilizationEvent.EventUTC, saved[0].EventUTC);
      Assert.AreEqual(dataCustomUtilizationEvent.OEMDataSourceValue, saved[0].OEMDataSourceValue);
      Assert.AreEqual(dataCustomUtilizationEvent.Value, saved[0].Value);
      Assert.AreEqual(dataCustomUtilizationEvent.ifk_OEMDataSourceTypeID, saved[0].ifk_OEMDataSourceTypeID);
      Assert.AreEqual(dataCustomUtilizationEvent.ifk_DimCustomUtilizationEventTypeID, saved[0].ifk_DimCustomUtilizationEventTypeID);
      Assert.AreEqual(dataCustomUtilizationEvent.ifk_DimUnitTypeID, saved[0].ifk_DimUnitTypeID);
    }

    [Ignore]
    [TestMethod]
    [DatabaseTest]
    public void SaveDataCustomUtilizationEvent_FilterDupsWhenSavedAtTheSameTimeTest()
    {
      DataCustomUtilizationEvent dataCustomUtilizationEvent = new DataCustomUtilizationEvent
      {
        InsertUTC = DateTime.UtcNow,
        fk_DimSourceID = (int)DimSourceEnum.PR3Gateway,
        DebugRefID = -9999,
        SourceMsgID = 888,
        AssetID = 37421,
        EventUTC = DateTime.UtcNow,
        OEMDataSourceValue = 12,
        Value = 2.12,
        ifk_OEMDataSourceTypeID = (int)OEMDataSourceTypeEnum.MID,
        ifk_DimCustomUtilizationEventTypeID = (int)DimCustomUtilizationEventTypeEnum.Payload,
        ifk_DimUnitTypeID = (int)DimUnitTypeEnum.Tonne,
        DeviceType = DeviceTypeEnum.Series522,
        GPSDeviceID = "1234"
      };



      bool retryExpected = false;
      NHDataSaver.Save(new List<INHDataObject> { dataCustomUtilizationEvent, dataCustomUtilizationEvent }, out retryExpected);
      Assert.IsFalse(retryExpected, "No problems expected");

      List<DataCustomUtilizationEvent> saved = (from r in Ctx.DataContext.DataCustomUtilizationEventReadOnly
                                                where r.DebugRefID == -9999 && r.AssetID == 37421
                                                select r).ToList<DataCustomUtilizationEvent>();
      Assert.AreEqual(1, saved.Count);
      Assert.AreEqual(dataCustomUtilizationEvent.AssetID, saved[0].AssetID);
      Assert.AreEqual(dataCustomUtilizationEvent.fk_DimSourceID, saved[0].fk_DimSourceID);
      Assert.AreEqual(dataCustomUtilizationEvent.DebugRefID, saved[0].DebugRefID);
      Assert.AreEqual(dataCustomUtilizationEvent.SourceMsgID, saved[0].SourceMsgID);
      Assert.AreEqual(dataCustomUtilizationEvent.EventUTC, saved[0].EventUTC);
      Assert.AreEqual(dataCustomUtilizationEvent.OEMDataSourceValue, saved[0].OEMDataSourceValue);
      Assert.AreEqual(dataCustomUtilizationEvent.Value, saved[0].Value);
      Assert.AreEqual(dataCustomUtilizationEvent.ifk_OEMDataSourceTypeID, saved[0].ifk_OEMDataSourceTypeID);
      Assert.AreEqual(dataCustomUtilizationEvent.ifk_DimCustomUtilizationEventTypeID, saved[0].ifk_DimCustomUtilizationEventTypeID);
      Assert.AreEqual(dataCustomUtilizationEvent.ifk_DimUnitTypeID, saved[0].ifk_DimUnitTypeID);
    }

    [Ignore]
    [TestMethod]
    [DatabaseTest]
    public void SaveDataCustomUtilizationEvent_FilterDupsWhenSavedAtDifferentTimesTest()
    {
      DataCustomUtilizationEvent dataCustomUtilizationEvent = new DataCustomUtilizationEvent
      {
        InsertUTC = DateTime.UtcNow,
        fk_DimSourceID = (int)DimSourceEnum.PR3Gateway,
        DebugRefID = -9999,
        SourceMsgID = 888,
        AssetID = 37421,
        EventUTC = DateTime.UtcNow,
        OEMDataSourceValue = 12,
        Value = 2.12,
        ifk_OEMDataSourceTypeID = (int)OEMDataSourceTypeEnum.MID,
        ifk_DimCustomUtilizationEventTypeID = (int)DimCustomUtilizationEventTypeEnum.Payload,
        ifk_DimUnitTypeID = (int)DimUnitTypeEnum.Tonne,
        DeviceType = DeviceTypeEnum.Series522,
        GPSDeviceID = "1234"
      };



      bool retryExpected = false;
      NHDataSaver.Save(new List<INHDataObject> { dataCustomUtilizationEvent }, out retryExpected);
      Assert.IsFalse(retryExpected, "No problems expected");

      NHDataSaver.Save(new List<INHDataObject> { dataCustomUtilizationEvent }, out retryExpected);
      Assert.IsFalse(retryExpected, "No problems expected");

      List<DataCustomUtilizationEvent> saved = (from r in Ctx.DataContext.DataCustomUtilizationEventReadOnly
                                                where r.DebugRefID == -9999 && r.AssetID == 37421
                                                select r).ToList<DataCustomUtilizationEvent>();
      Assert.AreEqual(1, saved.Count);
      Assert.AreEqual(dataCustomUtilizationEvent.AssetID, saved[0].AssetID);
      Assert.AreEqual(dataCustomUtilizationEvent.fk_DimSourceID, saved[0].fk_DimSourceID);
      Assert.AreEqual(dataCustomUtilizationEvent.DebugRefID, saved[0].DebugRefID);
      Assert.AreEqual(dataCustomUtilizationEvent.SourceMsgID, saved[0].SourceMsgID);
      Assert.AreEqual(dataCustomUtilizationEvent.EventUTC, saved[0].EventUTC);
      Assert.AreEqual(dataCustomUtilizationEvent.OEMDataSourceValue, saved[0].OEMDataSourceValue);
      Assert.AreEqual(dataCustomUtilizationEvent.Value, saved[0].Value);
      Assert.AreEqual(dataCustomUtilizationEvent.ifk_OEMDataSourceTypeID, saved[0].ifk_OEMDataSourceTypeID);
      Assert.AreEqual(dataCustomUtilizationEvent.ifk_DimCustomUtilizationEventTypeID, saved[0].ifk_DimCustomUtilizationEventTypeID);
      Assert.AreEqual(dataCustomUtilizationEvent.ifk_DimUnitTypeID, saved[0].ifk_DimUnitTypeID);
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_SwitchState()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC and InputNumber match - everything else can be different
      DataSwitchState item1 = new DataSwitchState { ID = -1, AssetID = 10, EventUTC = t1, DebugRefID = -8888, SourceMsgID = 8888, fk_DimSourceID = 1, InputNumber = 1, IOState = true };
      DataSwitchState item2 = new DataSwitchState { ID = -1, AssetID = 10, EventUTC = t1, DebugRefID = -7777, SourceMsgID = 7777, fk_DimSourceID = 2, InputNumber = 2, IOState = false };
      DataSwitchState item3 = new DataSwitchState { ID = -1, AssetID = 10, EventUTC = t1, DebugRefID = -6666, SourceMsgID = 6666, fk_DimSourceID = 2, InputNumber = 2, IOState = false };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      List<DataSwitchState> init = (from r in Ctx.DataContext.DataSwitchStateReadOnly
                                        where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                           (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                        orderby r.EventUTC, r.DebugRefID
                                        select r).ToList<DataSwitchState>();

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataSwitchState> saved = (from r in Ctx.DataContext.DataSwitchStateReadOnly
                                         where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                            (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                         orderby r.EventUTC, r.DebugRefID
                                         select r).ToList<DataSwitchState>();
      Assert.AreEqual(2, saved.Count, "Expect input 1 and input 2 records to be saved");

      DataSwitchState first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      DataSwitchState second = saved[1];
      Assert.AreEqual(item2.AssetID, second.AssetID, "Wrong item2, assetID");
      Assert.AreEqual(item2.EventUTC, second.EventUTC, "Wrong item2, eventutc");
      Assert.AreEqual(item2.InputNumber, second.InputNumber, "Wrong item2, inputNumber");
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_EngineParams()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      // They should be considered duplicates if the assetID and eventUTC and MID and all other non-null 'data' fields match
      DataEngineParameters item1 = new DataEngineParameters
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        MID = "30",
        ConsumptionGallons = 100,
        LevelPercent = 30
      };
      DataEngineParameters item2 = new DataEngineParameters
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 2,
        MID = "30",
        EngineIdleHours = 88,
        IdleFuelGallons = 2,
        MachineIdleFuelGallons = 5,
        MachineIdleHours = 6,
        MaxFuelGallons = 7,
        Revolutions = 111,
        Starts = 99
      };
      DataEngineParameters item3 = new DataEngineParameters
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 2,
        MID = "30",
        EngineIdleHours = 88,
        IdleFuelGallons = 2,
        MachineIdleFuelGallons = 5,
        MachineIdleHours = 6,
        MaxFuelGallons = 7,
        Revolutions = 111,
        Starts = 99
      };
      DataEngineParameters item4 = new DataEngineParameters
    {
      ID = -1,
      AssetID = 10,
      EventUTC = t1,
      DebugRefID = -5555,
      SourceMsgID = 5555,
      fk_DimSourceID = 2,
      MID = "66",
      EngineIdleHours = 88,
      IdleFuelGallons = 2,
      MachineIdleFuelGallons = 5,
      MachineIdleHours = 6,
      MaxFuelGallons = 7,
      Revolutions = 111,
      Starts = 99
    };

      List<NHDataWrapper> items = new List<NHDataWrapper>();
      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item4, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataEngineParameters> saved = (from r in Ctx.DataContext.DataEngineParametersReadOnly
                                          where r.DebugRefID <= -5555 && r.AssetID == 10 &&
                                             (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666 || r.SourceMsgID == 5555)
                                          orderby r.EventUTC, r.DebugRefID
                                          select r).ToList<DataEngineParameters>();
      Assert.AreEqual(2, saved.Count, "Expect item1 to be saved, and item 2 to update item 1, because their data is complimentary. The third record is an exact duplicate of the 2nd so should be rejected. The fourth has a different MID so should be accepted.");

      DataEngineParameters first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      Assert.AreEqual(item1.ConsumptionGallons, first.ConsumptionGallons, "Consumption gallons should be inserted with first item");
      Assert.AreEqual(item2.Revolutions, first.Revolutions, "Revolutions should get updated, from second item");
      DataEngineParameters second = saved[1];
      Assert.AreEqual(-5555, second.DebugRefID, "Expected item2 to get stored OK.");
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_EngineParams_MultipleSave()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      // They should be considered duplicates if the assetID and eventUTC and MID and all other non-null 'data' fields match
      DataEngineParameters item1 = new DataEngineParameters
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        MID = "30",
        ConsumptionGallons = 100,
        LevelPercent = 30
      };
      DataEngineParameters item2 = new DataEngineParameters
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -5555,
        SourceMsgID = 5555,
        fk_DimSourceID = 2,
      };
      DataEngineParameters item3 = new DataEngineParameters
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 2,
        EngineIdleHours = 88,
        IdleFuelGallons = 2,
        MachineIdleFuelGallons = 5,
        MachineIdleHours = 6,
        MaxFuelGallons = 7,
        Revolutions = 111,
        Starts = 99,
        LevelPercent = 100
      };
    
      List<NHDataWrapper> itemsList1 = new List<NHDataWrapper>();
      itemsList1.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      itemsList1.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      
      List<NHDataWrapper> itemsList2 = new List<NHDataWrapper>();
      itemsList2.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });
      
      bool retryExpected = false;

      var sameTypeGroups = itemsList1.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }


      sameTypeGroups = itemsList2.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataEngineParameters> saved = (from r in Ctx.DataContext.DataEngineParametersReadOnly
                                          where r.DebugRefID <= -5555 && r.AssetID == 10 &&
                                             (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 5555)
                                          orderby r.EventUTC, r.DebugRefID
                                          select r).ToList<DataEngineParameters>();
      Assert.AreEqual(2, saved.Count, "Expect item1 to be saved, and item 2 to update item 1, because their data is complimentary. The third record is an exact duplicate of the 2nd so should be rejected. The fourth has a different MID so should be accepted.");

      DataEngineParameters first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      Assert.AreEqual(item1.ConsumptionGallons, first.ConsumptionGallons, "Consumption gallons should be inserted with first item");
      Assert.AreEqual(item2.Revolutions, first.Revolutions, "Revolutions should get updated, from second item");
      DataEngineParameters second = saved[1];
      Assert.AreEqual(-5555, second.DebugRefID, "Expected item2 to get stored OK.");
    }
    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_EngineStartStop()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC and IsStart flag match
      DataEngineStartStop item1 = new DataEngineStartStop
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        ifk_EngineStateID = (int)DimEngineStateEnum.EngineOn
      };
      DataEngineStartStop item2 = new DataEngineStartStop
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 1,
        ifk_EngineStateID = (int)DimEngineStateEnum.EngineOn
      };
      DataEngineStartStop item3 = new DataEngineStartStop
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 1,
        ifk_EngineStateID = (int)DimEngineStateEnum.EngineOff
      };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataEngineStartStop> saved = (from r in Ctx.DataContext.DataEngineStartStopReadOnly
                                         where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                             (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                         orderby r.EventUTC, r.DebugRefID
                                         select r).ToList<DataEngineStartStop>();
      // We used to not take the IsStart flag into account when checking for dups.  Now we are, so
      // this test has changed a bit.  There should be 3 items, not 2.
      //Assert.AreEqual(2, saved.Count, "Expect item1 and item2 to be saved, because they have different eventUTC's. The third record has a different IsStart value but the same EventUTC as another so is considered a duplicate.");
      Assert.AreEqual(3, saved.Count, "Expected all items to be saved.");

      DataEngineStartStop first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      DataEngineStartStop second = saved[1];
      Assert.AreEqual(item2.EventUTC, second.EventUTC, "Expected item2 to get stored OK.");
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_FaultDiagnostics()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC and FMI signature match
      DataFaultDiagnostic item1 = new DataFaultDiagnostic
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        CID = 1,
        FMI = 2,
        fk_DimDatalinkID = 1
      };
      DataFaultDiagnostic item2 = new DataFaultDiagnostic
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 1,
        CID = 1,
        FMI = 2,
        fk_DimDatalinkID = 1
      };
      DataFaultDiagnostic item3 = new DataFaultDiagnostic
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 1,
        CID = 1,
        FMI = 2,
        fk_DimDatalinkID = 1
      };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataFaultDiagnostic> saved = (from r in Ctx.DataContext.DataFaultDiagnosticReadOnly
                                         where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                             (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                         orderby r.EventUTC, r.DebugRefID
                                         select r).ToList<DataFaultDiagnostic>();
      Assert.AreEqual(2, saved.Count, "Expect item1 and item2 to be saved, because they have different eventUTC's. The third record is identical to the 2nd.");

      DataFaultDiagnostic first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      DataFaultDiagnostic second = saved[1];
      Assert.AreEqual(item2.EventUTC, second.EventUTC, "Expected item2 to get stored OK.");
      Assert.AreEqual(item2.CID, second.CID, "Expected item2 to get stored OK.");
      Assert.AreEqual(item2.FMI, second.FMI, "Expected item2 to get stored OK.");
      Assert.AreEqual(item2.fk_DimDatalinkID, second.fk_DimDatalinkID, "Expected item2 to get stored OK.");
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_FaultEvents()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC and EID signature match
      DataFaultEvent item1 = new DataFaultEvent
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        EID = 10,
        fk_DimDatalinkID = 1
      };
      DataFaultEvent item2 = new DataFaultEvent
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 1,
        EID = 10,
        fk_DimDatalinkID = 1
      };
      DataFaultEvent item3 = new DataFaultEvent
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 1,
        EID = 10,
        fk_DimDatalinkID = 1
      };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataFaultEvent> saved = (from r in Ctx.DataContext.DataFaultEventReadOnly
                                    where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                        (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                    orderby r.EventUTC, r.DebugRefID
                                    select r).ToList<DataFaultEvent>();
      Assert.AreEqual(2, saved.Count, "Expect item1 and item2 to be saved, because they have different eventUTC's. The third record is identical to the 2nd.");

      DataFaultEvent first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      DataFaultEvent second = saved[1];
      Assert.AreEqual(item2.EventUTC, second.EventUTC, "Expected item2 to get stored OK.");
      Assert.AreEqual(item2.EID, second.EID, "Expected item2 to get stored OK.");
      Assert.AreEqual(item2.fk_DimDatalinkID, second.fk_DimDatalinkID, "Expected item2 to get stored OK.");
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_FenceAlarm()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC and EID signature match
      DataFenceAlarm item1 = new DataFenceAlarm
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        ExclusiveWatchActive = true,
        ExclusiveWatchAlarm = false,
        InclusiveWatchActive = true,
        InclusiveWatchAlarm = true,
        SatelliteBlockage = false,
        TimeWatchActive = true,
        TimeWatchAlarm = false,
        DisconnectSwitchUsed = false
      };
      DataFenceAlarm item2 = new DataFenceAlarm
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 1,
        ExclusiveWatchActive = false,
        ExclusiveWatchAlarm = true,
        InclusiveWatchActive = false,
        InclusiveWatchAlarm = false,
        SatelliteBlockage = true,
        TimeWatchActive = false,
        TimeWatchAlarm = true,
        DisconnectSwitchUsed = true
      };
      DataFenceAlarm item3 = new DataFenceAlarm
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 1,
        ExclusiveWatchActive = true,
        ExclusiveWatchAlarm = false,
        InclusiveWatchActive = true,
        InclusiveWatchAlarm = true,
        SatelliteBlockage = false,
        TimeWatchActive = true,
        TimeWatchAlarm = false,
        DisconnectSwitchUsed = false
      };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataFenceAlarm> saved = (from r in Ctx.DataContext.DataFenceAlarmReadOnly
                                    where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                        (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                    orderby r.EventUTC, r.DebugRefID
                                    select r).ToList<DataFenceAlarm>();
      Assert.AreEqual(3, saved.Count, "Expect item1, item2 and item3 to be saved. Item2 has same eventUTC as 1 but different flag values. Item3 is identical to item1 except for EventUTC, so is not considered a duplicate");

      DataFenceAlarm first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      DataFenceAlarm second = saved[1];
      Assert.AreEqual(-7777, second.DebugRefID, "Expected item2 to get stored OK.");
      DataFenceAlarm third = saved[2];
      Assert.AreEqual(-6666, third.DebugRefID, "Expected item3 to get stored OK.");
    }


    /// <summary>
    /// Test for Bug 26836. Duplication in NH_DATA is sometimes occuring when multiple app servers are inserting data objects with same assetId,eventUTC at the same time.
    /// So let's simulate multiple app servers inserting at the same time. This test is [Ignored] since there is no guarantee that simultaneously inserting will not create duplicates and this test
    /// takes a bit of time to execute. 
    /// </summary>
    [TestMethod()]
    [DatabaseTest]
    [Ignore]
    public void HoursLocation_MultipleTheadsInsert_NoDuplicatesExist()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);
      int assetId = -98765, debugRefId = -12345, sourceId = -9999;
      int thread_Count = 10;
      List<NHDataWrapper> items = new List<NHDataWrapper>();
      int numUniqueRows = 1000;
      for (int i = 0; i < numUniqueRows; ++i)
      {
        var dhl = GetDataHoursLocation(t0);
        dhl.AssetID = assetId;
        dhl.DebugRefID = debugRefId;
        dhl.SourceMsgID = sourceId;
        dhl.EventUTC = t0.AddSeconds(i);
        items.Add(new NHDataWrapper() { Data = dhl, StoreAttempts = 0 });  
      }
      var itemsToSave = (from item in items select item.Data).ToList<INHDataObject>();

      for (int i = 0; i < 5; ++i) //just retry this test multiple times to increase probability of duplication due to timing conflict occurring. 
      {
        //Arrange
        List<DataHoursLocation> existing = (from r in Ctx.DataContext.DataHoursLocation
          where r.AssetID == assetId && r.DebugRefID == debugRefId && r.SourceMsgID == sourceId
          select r).ToList<DataHoursLocation>();

        if (existing.Any())
        {
          foreach (var existingRow in existing)
            Ctx.DataContext.DataHoursLocation.DeleteObject(existingRow);
          Ctx.DataContext.SaveChanges();
        }
        //Act
        Task[] tasks = new Task[thread_Count];
        bool retry;
        for (int thread_iter = 0; thread_iter < thread_Count; ++thread_iter)
        {
          tasks[thread_iter] = Task.Factory.StartNew(() => NHDataSaver.Save(itemsToSave, out retry));
        }

        Task.WaitAll(tasks, new TimeSpan(0, 0, 10)); //10 seconds
        Thread.Sleep(4000); //wait for some time to ensure all store procs in db are done. Otherwise context does not always pick up inserted items despite the "thread wait" in previous line
        
        //Assert
        var saved = (from r in Ctx.DataContext.DataHoursLocationReadOnly
          where r.AssetID == assetId && r.DebugRefID == debugRefId && r.SourceMsgID == sourceId
          group r by r.EventUTC).ToList();
        Assert.AreEqual(numUniqueRows, saved.Count, "unexpected number of rows");
        foreach (var g in saved)
        {
          var group_items = g.ToList<DataHoursLocation>();
          Assert.AreEqual(1, group_items.Count, "duplicates exist");
        }
      }

    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_HoursLocation()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC match
      DataHoursLocation item1 = new DataHoursLocation
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        Latitude = 1.11,
        Longitude = 2.22,
        RuntimeHours = 9
      };
      DataHoursLocation item1a = new DataHoursLocation
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        Latitude = 3.33,
        Longitude = 2.22,
        RuntimeHours = 9
      };
      DataHoursLocation item2 = new DataHoursLocation
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 1,
        Latitude = 1.11,
        Longitude = 2.22,
        RuntimeHours = 9
      };
      DataHoursLocation item3 = new DataHoursLocation
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 1,
        Latitude = 1.11,
        Longitude = 2.22,
        RuntimeHours = 9
      };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item1a, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataHoursLocation> saved = (from r in Ctx.DataContext.DataHoursLocationReadOnly
                                       where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                           (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                       orderby r.EventUTC, r.DebugRefID
                                       select r).ToList<DataHoursLocation>();
      Assert.AreEqual(2, saved.Count, "Expect item1 and item2 to be saved. Item3 has same eventUTC as 2.");

      DataHoursLocation first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      DataHoursLocation second = saved[1];
      Assert.AreEqual(item2.AssetID, second.AssetID, "Second item should match, assetID");
      Assert.AreEqual(item2.EventUTC, second.EventUTC, "Second item should match, eventutc");
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_IgnOnOff()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC match
      DataIgnOnOff item1 = new DataIgnOnOff
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        RuntimeHours = 12,
        IsOn = true
      };
      DataIgnOnOff item2 = new DataIgnOnOff
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 1,
        RuntimeHours = 12,
        IsOn = true
      };
      DataIgnOnOff item3 = new DataIgnOnOff
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 1,
        RuntimeHours = 12,
        IsOn = true
      };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataIgnOnOff> saved = (from r in Ctx.DataContext.DataIgnOnOffReadOnly
                                  where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                      (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                  orderby r.EventUTC, r.DebugRefID
                                  select r).ToList<DataIgnOnOff>();
      Assert.AreEqual(2, saved.Count, "Expect item1 and item2 to be saved. Item3 has same eventUTC as 2.");

      DataIgnOnOff first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      DataIgnOnOff second = saved[1];
      Assert.AreEqual(item2.EventUTC, second.EventUTC, "Expected item2 to get stored OK.");
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_DigitalSwitchStatus()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC match
      DataFeedDigitalSwitchStatus item1 = GetDataFeedDigitalSwitchStatus(t0, 1);
      DataFeedDigitalSwitchStatus item2 = GetDataFeedDigitalSwitchStatus(t0, 2);
      DataFeedDigitalSwitchStatus item3 = GetDataFeedDigitalSwitchStatus(t1, 1);
      DataFeedDigitalSwitchStatus item4 = GetDataFeedDigitalSwitchStatus(t1, 1);

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item4, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataFeedDigitalSwitchStatus> saved = (from r in Ctx.DataContext.DataFeedDigitalSwitchStatusReadOnly
                                  where r.DebugRefID == -9999
                                  orderby r.AssetID, r.EventUTC, r.Switch
                                             select r).ToList<DataFeedDigitalSwitchStatus>();
      Assert.AreEqual(3, saved.Count, "Expect item1, item2 and 3 or 4 to be saved.");

      DataFeedDigitalSwitchStatus first = saved[0];
      Assert.AreEqual(item1.EventUTC, first.EventUTC, "Expected item1 to get stored OK.");
      Assert.AreEqual(item1.Switch, first.Switch, "Expected item1 to get stored OK.");
      DataFeedDigitalSwitchStatus second = saved[1];
      Assert.AreEqual(item2.EventUTC, second.EventUTC, "Expected item2 to get stored OK.");
      Assert.AreEqual(item2.Switch, second.Switch, "Expected item2 to get stored OK.");
      DataFeedDigitalSwitchStatus third = saved[2];
      Assert.AreEqual(item3.EventUTC, third.EventUTC, "Expected item3 or 4 to get stored OK.");
      Assert.AreEqual(item3.Switch, third.Switch, "Expected item3 or 4 to get stored OK.");
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_ParametersReport()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // UniqueKey(assetID,eventUTC,ecmSourceAddress,pgn,spn)
      DataParametersReport item1 = GetDataParametersReport(t0, ecmSourceAddress: 2000, pgn: 10, spn: 12);
      DataParametersReport item2 = GetDataParametersReport(t0, ecmSourceAddress: 2000, pgn: 10, spn: 88);
      DataParametersReport item3 = GetDataParametersReport(t0, ecmSourceAddress: 2000, pgn: 50, spn: 12);
      DataParametersReport item4 = GetDataParametersReport(t1, ecmSourceAddress: 2000, pgn: 50, spn: 12);
      DataParametersReport item5 = GetDataParametersReport(t1, ecmSourceAddress: 2000, pgn: 50, spn: 12);

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item4, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item5, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataParametersReport> saved = (from r in Ctx.DataContext.DataParametersReportReadOnly
                                             where r.DebugRefID == -9999
                                             orderby r.AssetID, r.EventUTC, r.ECMSourceAddress, r.PGN, r.SPN
                                             select r).ToList<DataParametersReport>();
      Assert.AreEqual(4, saved.Count, "Expect item1, item2 and 3 and 4 0r 5 to be saved.");

      DataParametersReport first = saved[0];
      Assert.AreEqual(item1.EventUTC, first.EventUTC, "Expected item1 to get stored OK.");
      Assert.AreEqual(item1.ECMSourceAddress, first.ECMSourceAddress, "Expected item1 to get stored OK.");
      Assert.AreEqual(item1.PGN, first.PGN, "Expected item1 to get stored OK.");
      Assert.AreEqual(item1.SPN, first.SPN, "Expected item1 to get stored OK.");
      DataParametersReport second = saved[1];
      Assert.AreEqual(item2.ECMSourceAddress, second.ECMSourceAddress, "Expected item1 to get stored OK.");
      Assert.AreEqual(item2.PGN, second.PGN, "Expected item2 to get stored OK.");
      Assert.AreEqual(item2.SPN, second.SPN, "Expected item2 to get stored OK.");
      DataParametersReport third = saved[2];
      Assert.AreEqual(item3.ECMSourceAddress, third.ECMSourceAddress, "Expected item1 to get stored OK.");
      Assert.AreEqual(item3.PGN, third.PGN, "Expected item3 or 4 to get stored OK.");
      Assert.AreEqual(item3.SPN, third.SPN, "Expected item3 or 4 to get stored OK.");
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_StatisticsReport()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // UniqueKey(assetID,eventUTC,ecmSourceAddress,pgn,spn)
      DataStatisticsReport item1 = GetDataStatisticsReport(t0, ecmSourceAddress: 2000, pgn: 10, spn: 12);
      DataStatisticsReport item2 = GetDataStatisticsReport(t0, ecmSourceAddress: 2000, pgn: 10, spn: 88);
      DataStatisticsReport item3 = GetDataStatisticsReport(t0, ecmSourceAddress: 2000, pgn: 50, spn: 12);
      DataStatisticsReport item4 = GetDataStatisticsReport(t1, ecmSourceAddress: 2000, pgn: 50, spn: 12);
      DataStatisticsReport item5 = GetDataStatisticsReport(t1, ecmSourceAddress: 2000, pgn: 50, spn: 12);

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item4, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item5, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataStatisticsReport> saved = (from r in Ctx.DataContext.DataStatisticsReportReadOnly
                                          where r.DebugRefID == -9999
                                          orderby r.AssetID, r.EventUTC, r.ECMSourceAddress, r.PGN, r.SPN
                                          select r).ToList<DataStatisticsReport>();
      Assert.AreEqual(4, saved.Count, "Expect item1, item2 and 3 and 4 0r 5 to be saved.");

      DataStatisticsReport first = saved[0];
      Assert.AreEqual(item1.EventUTC, first.EventUTC, "Expected item1 to get stored OK.");
      Assert.AreEqual(item1.ECMSourceAddress, first.ECMSourceAddress, "Expected item1 to get stored OK.");
      Assert.AreEqual(item1.PGN, first.PGN, "Expected item1 to get stored OK.");
      Assert.AreEqual(item1.SPN, first.SPN, "Expected item1 to get stored OK.");
      DataStatisticsReport second = saved[1];
      Assert.AreEqual(item2.ECMSourceAddress, second.ECMSourceAddress, "Expected item1 to get stored OK.");
      Assert.AreEqual(item2.PGN, second.PGN, "Expected item2 to get stored OK.");
      Assert.AreEqual(item2.SPN, second.SPN, "Expected item2 to get stored OK.");
      DataStatisticsReport third = saved[2];
      Assert.AreEqual(item3.ECMSourceAddress, third.ECMSourceAddress, "Expected item1 to get stored OK.");
      Assert.AreEqual(item3.PGN, third.PGN, "Expected item3 or 4 to get stored OK.");
      Assert.AreEqual(item3.SPN, third.SPN, "Expected item3 or 4 to get stored OK.");
    }
    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_PowerLoss()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC match
      DataPowerState item1 = new DataPowerState
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
      };
      DataPowerState item2 = new DataPowerState
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 1,
        IsOn = false,
      };
      DataPowerState item3 = new DataPowerState
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 1,
        IsOn = false,
      };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataPowerState> saved = (from r in Ctx.DataContext.DataPowerStateReadOnly
                                  where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                      (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                  orderby r.EventUTC, r.DebugRefID
                                   select r).ToList<DataPowerState>();
      Assert.AreEqual(2, saved.Count, "Expect item1 and item2 to be saved. Item3 has same eventUTC as 2.");

      DataPowerState first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      DataPowerState second = saved[1];
      Assert.AreEqual(item2.EventUTC, second.EventUTC, "Expected item2 to get stored OK.");
    }


    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_Moving()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC match
      DataMoving item1 = new DataMoving
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        IsStart = true
      };
      DataMoving item2 = new DataMoving
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 1,
        IsStart = false
      };
      DataMoving item3 = new DataMoving
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 1,
        IsStart = true
      };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataMoving> saved = (from r in Ctx.DataContext.DataMovingReadOnly
                                where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                    (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                orderby r.EventUTC, r.DebugRefID
                                select r).ToList<DataMoving>();
      Assert.AreEqual(2, saved.Count, "Expect item1 and item2 to be saved. Item3 has same eventUTC as 2.");

      DataMoving first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      DataMoving second = saved[1];
      Assert.AreEqual(item2.EventUTC, second.EventUTC, "Expected item2 to get stored OK.");
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_MSSKeyID()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC match
      DataMSSKeyID item1 = new DataMSSKeyID
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        MSSKeyID = 1234,
        OperatorName="OpName1"
      };
      DataMSSKeyID item2 = new DataMSSKeyID
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 1,
        MSSKeyID = 1234,
        OperatorName = "OpName2"
      };
      DataMSSKeyID item3 = new DataMSSKeyID
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 1,
        MSSKeyID = 4321,
        OperatorName = "OpName3"
      };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataMSSKeyID> saved = (from r in Ctx.DataContext.DataMSSKeyIDReadOnly
                                  where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                      (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                  orderby r.EventUTC, r.DebugRefID
                                  select r).ToList<DataMSSKeyID>();
      Assert.AreEqual(2, saved.Count, "Expect item1 and item2 to be saved. Item3 has same eventUTC as 2.");

      DataMSSKeyID first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      DataMSSKeyID second = saved[1];
      Assert.AreEqual(item2.AssetID, second.AssetID, "Expect second record to contain data from item2, assetID.");
      Assert.AreEqual(item2.EventUTC, second.EventUTC, "Expect second record to contain data from item2, eventUTC.");
    }

    [TestMethod()]
    [DatabaseTest]
    public void SaveOperatorName_MSSKeyID()
    {
        DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
        DateTime t1 = t0.AddMilliseconds(123);
        DateTime t2 = t1.AddMilliseconds(123);

        List<NHDataWrapper> items = new List<NHDataWrapper>();

        // They should be considered duplicates if the assetID and eventUTC match
        DataMSSKeyID item1 = new DataMSSKeyID
        {
            ID = -1,
            AssetID = 10,
            EventUTC = t0,
            DebugRefID = -8888,
            SourceMsgID = 8888,
            fk_DimSourceID = 1,
            MSSKeyID = 1234,
            OperatorName = "OpName1"
        };
        DataMSSKeyID item2 = new DataMSSKeyID
        {
            ID = -1,
            AssetID = 10,
            EventUTC = t1,
            DebugRefID = -7777,
            SourceMsgID = 7777,
            fk_DimSourceID = 1,
            MSSKeyID = 1243,
            OperatorName = "OpName2"
        };
        DataMSSKeyID item3 = new DataMSSKeyID
        {
            ID = -1,
            AssetID = 10,
            EventUTC = t2,
            DebugRefID = -6666,
            SourceMsgID = 6666,
            fk_DimSourceID = 1,
            MSSKeyID = 4321,
            OperatorName = "OpName3"
        };

        items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
        items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
        items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

        bool retryExpected = false;

        var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

        foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
        {
            List<INHDataObject> data = (from wrappedItem in typeGroup
                                        where wrappedItem.Data.AssetID > 0
                                        select wrappedItem.Data).ToList<INHDataObject>();

            NHDataSaver.Save(data, out retryExpected);
            Assert.IsFalse(retryExpected, "No problems expected");
        }

        List<DataMSSKeyID> saved = (from r in Ctx.DataContext.DataMSSKeyIDReadOnly
                                    where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                        (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                    orderby r.EventUTC, r.DebugRefID
                                    select r).ToList<DataMSSKeyID>();
        Assert.AreEqual(3, saved.Count, "Saved data Count should be 3.");

        Assert.AreEqual(item1.OperatorName, saved[0].OperatorName, "Operator Name should be OpName1");
        Assert.AreEqual(item2.OperatorName, saved[1].OperatorName, "Operator Name should be OpName2");
        Assert.AreEqual(item3.OperatorName, saved[2].OperatorName, "Operator Name should be OpName3");
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_TamperSecurityStatus()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC match
      DataTamperSecurityStatus item1 = GetDataTamperSecurityStatus(t0);
      DataTamperSecurityStatus item2 = GetDataTamperSecurityStatus(t0);
      DataTamperSecurityStatus item3 = GetDataTamperSecurityStatus(t1);

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataTamperSecurityStatus> saved = (from r in Ctx.DataContext.DataTamperSecurityStatusReadOnly
                                              where r.DebugRefID == -9999
                                  orderby r.AssetID, r.EventUTC
                                              select r).ToList<DataTamperSecurityStatus>();
      Assert.AreEqual(2, saved.Count, "Expect item1 and item3 to be saved.");

      DataTamperSecurityStatus first = saved[0];
      Assert.AreEqual(item1.EventUTC, first.EventUTC, "Expected item1 to get stored OK.");
      DataTamperSecurityStatus second = saved[1];
      Assert.AreEqual(item3.AssetID, second.AssetID, "Expect second record to contain data from item3, assetID.");
      Assert.AreEqual(item3.EventUTC, second.EventUTC, "Expect second record to contain data from item3, eventUTC.");
    }


    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_SMUAdj()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC match
      DataServiceMeterAdjustment item1 = new DataServiceMeterAdjustment
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        RuntimeBeforeHours = 900,
        RuntimeAfterHours = 700
      };
      DataServiceMeterAdjustment item2 = new DataServiceMeterAdjustment
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 1,
        RuntimeBeforeHours = 900,
        RuntimeAfterHours = 700
      };
      DataServiceMeterAdjustment item3 = new DataServiceMeterAdjustment
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 1,
        RuntimeBeforeHours = 900,
        RuntimeAfterHours = 555
      };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataServiceMeterAdjustment> saved = (from r in Ctx.DataContext.DataServiceMeterAdjustmentReadOnly
                                                where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                                    (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                                orderby r.EventUTC, r.DebugRefID
                                                select r).ToList<DataServiceMeterAdjustment>();
      Assert.AreEqual(2, saved.Count, "Expect item1 and item2 to be saved. Item3 has same eventUTC as 2.");

      DataServiceMeterAdjustment first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      DataServiceMeterAdjustment second = saved[1];
      Assert.AreEqual(item2.EventUTC, second.EventUTC, "Expected item2 to get stored OK.");
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_SiteState()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC and SiteID match
      DataSiteState item1 = new DataSiteState
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        SiteID = 100,
        IsEntry = true
      };
      DataSiteState item2 = new DataSiteState
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 1,
        SiteID = 100,
        IsEntry = true
      };
      DataSiteState item3 = new DataSiteState
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 1,
        SiteID = 100,
        IsEntry = false
      };
      DataSiteState item4 = new DataSiteState
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -5555,
        SourceMsgID = 5555,
        fk_DimSourceID = 1,
        SiteID = 200,
        IsEntry = false
      };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item4, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataSiteState> saved = (from r in Ctx.DataContext.DataSiteStateReadOnly
                                   where r.DebugRefID <= -5555 && r.AssetID == 10 &&
                                       (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666 || r.SourceMsgID == 5555)
                                   orderby r.EventUTC, r.DebugRefID
                                   select r).ToList<DataSiteState>();
      Assert.AreEqual(3, saved.Count, "Expect item1 and item2 to be saved, as unique for SiteID 100. Item4 should be saved also, cause it is for a different site.");

      DataSiteState first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      DataSiteState second = saved[1];
      Assert.AreEqual(item2.EventUTC, second.EventUTC, "Expected item2 to get stored OK.");
      Assert.AreEqual(item2.SiteID, second.SiteID, "Expected item2 to get stored OK.");
      DataSiteState fourth = saved[2];
      Assert.AreEqual(-5555, fourth.DebugRefID, "Expected item2 to get stored OK.");
    }

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_Speeding()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC match
      DataSpeeding item1 = new DataSpeeding
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        IsStart = true
      };
      DataSpeeding item2 = new DataSpeeding
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 1,
        IsStart = true
      };
      DataSpeeding item3 = new DataSpeeding
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 1,
        IsStart = false
      };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataSpeeding> saved = (from r in Ctx.DataContext.DataSpeedingReadOnly
                                  where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                      (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                  orderby r.EventUTC, r.DebugRefID
                                  select r).ToList<DataSpeeding>();
      Assert.AreEqual(2, saved.Count, "Expect item1 and item2 to be saved. Item3 has same eventUTC as 2.");

      DataSpeeding first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      DataSpeeding second = saved[1];
      Assert.AreEqual(item2.EventUTC, second.EventUTC, "Expected item2 to get stored OK.");
    }
     

    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_PassThroughPortData()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC and portNumber match
      DataPassThroughPortData item1 = new DataPassThroughPortData
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1,
        PortNumber = 1,
        Payload = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }
      };
      DataPassThroughPortData item2 = new DataPassThroughPortData
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 1,
        PortNumber = 1,
        Payload = new byte[] { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }
      };
      DataPassThroughPortData item3 = new DataPassThroughPortData
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 1,
        PortNumber = 2,
        Payload = new byte[] { 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 }
      };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataPassThroughPortData> saved = (from r in Ctx.DataContext.DataPassThroughPortDatasReadOnly
                                             where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                                 (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                             orderby r.EventUTC, r.DebugRefID
                                             select r).ToList<DataPassThroughPortData>();
      Assert.AreEqual(3, saved.Count, "Expect item1 and item2 to be saved. Item3 has same eventUTC as 2.");
    }
    [Ignore]
    [TestMethod()]
    [DatabaseTest]
    public void FilterDuplicates_IdleTimeOut()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      // They should be considered duplicates if the assetID and eventUTC match
      DataIdleTimeOut item1 = new DataIdleTimeOut
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t0,
        DebugRefID = -8888,
        SourceMsgID = 8888,
        fk_DimSourceID = 1
      };
      DataIdleTimeOut item2 = new DataIdleTimeOut
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -7777,
        SourceMsgID = 7777,
        fk_DimSourceID = 1
      };
      DataIdleTimeOut item3 = new DataIdleTimeOut
      {
        ID = -1,
        AssetID = 10,
        EventUTC = t1,
        DebugRefID = -6666,
        SourceMsgID = 6666,
        fk_DimSourceID = 1
      };

      items.Add(new NHDataWrapper { Data = item1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = item3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      List<DataIdleTimeOut> saved = (from r in Ctx.DataContext.DataIdleTimeOutReadOnly
                                  where r.DebugRefID <= -6666 && r.AssetID == 10 &&
                                      (r.SourceMsgID == 8888 || r.SourceMsgID == 7777 || r.SourceMsgID == 6666)
                                  orderby r.EventUTC, r.DebugRefID
                                  select r).ToList<DataIdleTimeOut>();
      Assert.AreEqual(2, saved.Count, "Expect item1 and item2 to be saved. Item3 has same eventUTC as 2.");

      DataIdleTimeOut first = saved[0];
      Assert.AreEqual(-8888, first.DebugRefID, "Expected item1 to get stored OK.");
      DataIdleTimeOut second = saved[1];
      Assert.AreEqual(item2.EventUTC, second.EventUTC, "Expected item2 to get stored OK.");
    }

    [TestMethod()]
    [DatabaseTest]
    public void SaveWithRetriableError()
    {
      int priorAntiDataCount = (from r in Ctx.DataContext.AntiDataReadOnly select 1).Count();
      int priorDiscreteIOCount = (from r in Ctx.DataContext.DataSwitchStateReadOnly select 1).Count();

      System.Configuration.Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      cfg.AppSettings.Settings.Remove("DatabaseUser");
      cfg.AppSettings.Settings.Add("DatabaseUser", "blabla");
      cfg.Save(ConfigurationSaveMode.Modified);
      ConfigurationManager.RefreshSection("appSettings");


      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      List<NHDataWrapper> items = new List<NHDataWrapper>();
      items.Add(new NHDataWrapper { Data = GetDataSwitchState(t0), StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsTrue(retryExpected, "Save should have failed and been detected as retriable");
      }

      cfg.AppSettings.Settings.Remove("DatabaseUser");
      cfg.AppSettings.Settings.Add("DatabaseUser", "UnitTests");
      cfg.Save(ConfigurationSaveMode.Modified);
      ConfigurationManager.RefreshSection("appSettings");

      int count = (from r in Ctx.DataContext.DataSwitchStateReadOnly
                   select 1).Count();
      Assert.AreEqual(0, Math.Abs(count - priorDiscreteIOCount), "Expect no data not saved to DataSwitchState");

      count = (from r in Ctx.DataContext.AntiDataReadOnly select 1).Count();
      Assert.AreEqual(0, count - priorAntiDataCount, "Data should be flagged for retry, not saved to AntiData");
    }

    [TestMethod()]
    [DatabaseTest]
    public void SaveManyDiscreteIOSameUTC()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      items.Add(new NHDataWrapper { Data = GetDataSwitchState(t0), StoreAttempts = 0 });
      DataSwitchState secondOne = GetDataSwitchState(t0);
      secondOne.InputNumber = 3;
      items.Add(new NHDataWrapper { Data = secondOne, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      int count = (from r in Ctx.DataContext.DataSwitchStateReadOnly
                   where r.DebugRefID == -9999
                   select 1).Count();
      Assert.AreEqual(2, count, "Both InputNumber 2 and 3 should be saved, even though same eventUTC");
    }

    [TestMethod()]
    [DatabaseTest]
    public void SaveManyDiagsSameUTC()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      DataFaultDiagnostic diag1 = GetDataFaultDiagnostic(t0);
      DataFaultDiagnostic diag2 = GetDataFaultDiagnostic(t0);
      DataFaultDiagnostic diag3 = GetDataFaultDiagnostic(t0);
      DataFaultDiagnostic diag4 = GetDataFaultDiagnostic(t0);

      diag2.FMI = diag1.FMI + 1;
      diag3.CID = diag1.CID + 1;
      diag4.fk_DimDatalinkID = diag1.fk_DimDatalinkID + 1;

      items.Add(new NHDataWrapper { Data = diag1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = diag2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = diag3, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = diag4, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      int count = (from r in Ctx.DataContext.DataFaultDiagnosticReadOnly
                   where r.DebugRefID == -9999
                   select 1).Count();
      Assert.AreEqual(4, count, "All diags should be saved because their diag IDs are different, even though same eventUTC");
    }

    [TestMethod()]
    [DatabaseTest]
    public void SaveManyEventsSameUTC()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      DataFaultEvent evt1 = GetDataFaultEvent(t0);
      DataFaultEvent evt2 = GetDataFaultEvent(t0);
      DataFaultEvent evt3 = GetDataFaultEvent(t0);

      evt2.EID = evt1.EID + 1;
      evt3.fk_DimDatalinkID = evt1.fk_DimDatalinkID - 1;

      items.Add(new NHDataWrapper { Data = evt1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = evt2, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = evt3, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }
      int count = (from r in Ctx.DataContext.DataFaultEventReadOnly
                   where r.DebugRefID == -9999
                   select 1).Count();
      Assert.AreEqual(3, count, "All events should be saved because their EIDs are different, even though same eventUTC");
    }

    [TestMethod()]
    [DatabaseTest]
    public void SaveManySiteEntrySameUTC()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      DataSiteState ss1 = GetDataSiteState(t0);
      DataSiteState ss2 = GetDataSiteState(t0);

      ss2.SiteID = ss1.SiteID + 1;

      items.Add(new NHDataWrapper { Data = ss1, StoreAttempts = 0 });
      items.Add(new NHDataWrapper { Data = ss2, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      int count = (from r in Ctx.DataContext.DataSiteStateReadOnly
                   where r.DebugRefID == -9999
                   select 1).Count();
      Assert.AreEqual(2, count, "All site entrys should be saved because their siteIDs are different, even though same eventUTC");
    }

    [TestMethod()]
    [DatabaseTest]
    public void SaveEngineParamsFragmentedData()
    {
      DateTime t0 = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1));
      DateTime t1 = t0.AddMilliseconds(123);

      List<NHDataWrapper> items = new List<NHDataWrapper>();

      //Incoming engine params data without consumptionGallons and LevelPercent
      DataEngineParameters obj = GetDataEngineParameters(t0);
      obj.ConsumptionGallons = null;
      obj.LevelPercent = null;

      items.Add(new NHDataWrapper() { Data = obj, StoreAttempts = 0 });

      bool retryExpected = false;

      var sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      //Incoming engine params data with consumptionGallons and LevelPercent - without all other data
      obj = GetDataEngineParameters(t0);
      obj.EngineIdleHours = null;
      obj.IdleFuelGallons = null;

      items.Clear();
      items.Add(new NHDataWrapper { Data = obj, StoreAttempts = 0 });

      sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      //Incoming engine params data with consumptionGallons and LevelPercent - without all other data
      obj = GetDataEngineParameters(t0);
      obj.EngineIdleHours = null;
      obj.IdleFuelGallons = null;

      items.Clear();
      items.Add(new NHDataWrapper { Data = obj, StoreAttempts = 0 });

      sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      //Incoming engine params data without consumptionGallons and LevelPercent
      obj = GetDataEngineParameters(t0);
      obj.ConsumptionGallons = null;
      obj.IdleFuelGallons = null;
      obj.LevelPercent = null;

      items.Clear();
      items.Add(new NHDataWrapper() { Data = obj, StoreAttempts = 0 });

      retryExpected = false;

      sameTypeGroups = items.GroupBy<NHDataWrapper, Type>(eo => eo.Data.GetType());

      foreach (IGrouping<Type, NHDataWrapper> typeGroup in sameTypeGroups)
      {
        List<INHDataObject> data = (from wrappedItem in typeGroup
                                    where wrappedItem.Data.AssetID > 0
                                    select wrappedItem.Data).ToList<INHDataObject>();

        NHDataSaver.Save(data, out retryExpected);
        Assert.IsFalse(retryExpected, "No problems expected");
      }

      var record = (from r in Ctx.DataContext.DataEngineParametersReadOnly
                    where r.EventUTC == t0
                    && r.AssetID == 37421
                    && r.DebugRefID == -9999
                    && r.SourceMsgID == 222
                    select r);
      Assert.AreEqual(1, record.Count(), "Should only be one record");
      DataEngineParameters result = record.FirstOrDefault<DataEngineParameters>();
      Assert.AreEqual(550, result.ConsumptionGallons, "ConsumptionGallons does not match");
      Assert.AreEqual(63, result.LevelPercent, "LevelPercent does not match");
      Assert.AreEqual(1100, result.EngineIdleHours, "EngineIdleHours does not match");
      Assert.AreEqual(20, result.IdleFuelGallons, "IdleFuelGallons does not match");
      Assert.AreEqual(300, result.MachineIdleFuelGallons, "MachineIdleFuelGallons does not match");
    }

    [TestMethod]
    [DatabaseTest]
    public void FilterDuplicateInList_RAWCanMessageTest()
    {
      long assetID = Asset.ComputeAssetID("CAT", "TESTAssetID");

      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;
      DataRawCANMessage record1 = new DataRawCANMessage();
      record1.AssetID = assetID;
      record1.EventUTC = currentTime;
      record1.Message = new byte[] { 1, 2, 3 };
      record1.SourceMsgID = 222;
      data.Add(record1);

      DataRawCANMessage record2 = new DataRawCANMessage();
      record2.AssetID = assetID;
      record2.EventUTC = currentTime;
      record2.Message = new byte[] { 1, 2, 3 };
      record2.SourceMsgID = 223;
      data.Add(record2);
      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);
      var record = (from r in Ctx.DataContext.DataRawCANMessageReadOnly
                    where r.EventUTC == currentTime
                    && r.AssetID == assetID
                    select r);
      Assert.AreEqual(1, record.Count(), "Should only be one record");
    }

    [TestMethod]
    [DatabaseTest]
    public void NoDuplicateInList_RAWCanMessageTest()
    {
      long assetID = Asset.ComputeAssetID("CAT", "TESTAssetID");

      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;
      DataRawCANMessage record1 = new DataRawCANMessage();
      record1.AssetID = assetID;
      record1.EventUTC = currentTime;
      record1.Message = new byte[] { 1, 2, 3 };
      record1.SourceMsgID = 222;
      data.Add(record1);
      DataRawCANMessage record2 = new DataRawCANMessage();
      record2.AssetID = assetID;
      record2.EventUTC = currentTime;
      record2.Message = new byte[] { 1, 2, 3, 4 };
      record2.SourceMsgID = 223;
      data.Add(record2);
      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);
      var record = (from r in Ctx.DataContext.DataRawCANMessageReadOnly
                    where r.EventUTC == currentTime
                    && r.AssetID == assetID
                    select r);
      Assert.AreEqual(2, record.Count(), "Should be 2 records");
    }

    [TestMethod]
    [DatabaseTest]
    public void FilterDuplicateInDB_RAWCanMessageTest()
    {
      long assetID = Asset.ComputeAssetID("CAT", "TESTAssetID");

      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;
      DataRawCANMessage record1 = new DataRawCANMessage();
      record1.AssetID = assetID;
      record1.EventUTC = currentTime;
      record1.Message = new byte[] { 1, 2, 3 };
      record1.SourceMsgID = 222;
      data.Add(record1);

      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);
      data.Clear();

      DataRawCANMessage record2 = new DataRawCANMessage();
      record2.AssetID = assetID;
      record2.EventUTC = currentTime;
      record2.Message = new byte[] { 1, 2, 3 };
      record2.SourceMsgID = 223;
      data.Add(record2);
      
      NHDataSaver.Save(data, out retryExpected);
      var record = (from r in Ctx.DataContext.DataRawCANMessageReadOnly
                    where r.EventUTC == currentTime
                    && r.AssetID == assetID
                    select r);
      Assert.AreEqual(1, record.Count(), "Should only be one record");
    }

    [TestMethod]
    [DatabaseTest]
    public void NoDuplicateInDB_RAWCanMessageTest()
    {
      long assetID = Asset.ComputeAssetID("CAT", "TESTAssetID");

      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;
      DataRawCANMessage record1 = new DataRawCANMessage();
      record1.AssetID = assetID;
      record1.EventUTC = currentTime;
      record1.Message = new byte[] { 1, 2, 3 };
      record1.SourceMsgID = 222;
      data.Add(record1);

      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      data.Clear();
      DataRawCANMessage record2 = new DataRawCANMessage();
      record2.AssetID = assetID;
      record2.EventUTC = currentTime;
      record2.Message = new byte[] { 1, 2, 3, 4 };
      record2.SourceMsgID = 223;
      data.Add(record2);
      NHDataSaver.Save(data, out retryExpected);
      
      var record = (from r in Ctx.DataContext.DataRawCANMessageReadOnly
                    where r.EventUTC == currentTime
                    && r.AssetID == assetID
                    select r);
      Assert.AreEqual(2, record.Count(), "Should be 2 records");
    }

    [Ignore]
    [TestMethod]
    [DatabaseTest]
    public void DataParametersReport_SaveAll()
    {
      long assetID = Asset.ComputeAssetID("CAT", "ReportSaveAll");
      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;
      DataParametersReport parameter1 = new DataParametersReport();
      parameter1.AssetID = assetID;
      parameter1.DebugRefID = 2;
      parameter1.DeviceType = DeviceTypeEnum.PL420;
      parameter1.EventUTC = currentTime;
      parameter1.fk_DimSourceID = 3;
      parameter1.GPSDeviceID = "Test";
      parameter1.ifk_DimSeverityLevelID = 1;
      parameter1.InsertUTC = DateTime.UtcNow;
      parameter1.ECMSourceAddress = 2;
      parameter1.ValueFloat = 9999.9999;
      parameter1.ValueString = "text";
      parameter1.PGN = 3;
      parameter1.SourceMsgID = 15;
      parameter1.SPN = 26;
      parameter1.ifk_DimUnitTypeID = (int)DimUnitTypeEnum.KilometersPerHour;
      data.Add(parameter1);

      DataParametersReport parameter2 = new DataParametersReport();
      parameter2.AssetID = assetID;
      parameter2.DebugRefID = 2;
      parameter2.DeviceType = DeviceTypeEnum.PL420;
      parameter2.EventUTC = currentTime;
      parameter2.fk_DimSourceID = 3;
      parameter2.GPSDeviceID = "Test";
      parameter2.ifk_DimSeverityLevelID = 1;
      parameter2.InsertUTC = DateTime.UtcNow;
      parameter2.ECMSourceAddress = 27;
      parameter2.ValueFloat = 9999.9999;
      parameter2.ValueString = "text";
      parameter2.PGN = 37;
      parameter2.SourceMsgID = 15;
      parameter2.SPN = 27;
      parameter2.ifk_DimUnitTypeID = (int)DimUnitTypeEnum.KilometersPerHour;
      data.Add(parameter2);

      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      var record = (from r in Ctx.DataContext.DataParametersReportReadOnly
                    where r.AssetID == assetID
                    select r);

      Assert.AreEqual(2, record.Count(), "there should be 2 parameter reports for this asset");
    }

    [Ignore]
    [TestMethod]
    [DatabaseTest]
    public void DataParametersReport_FilteredByCode()
    {
      long assetID = Asset.ComputeAssetID("CAT", "ReportSaveAll");
      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;
      DataParametersReport parameter1 = new DataParametersReport();
      parameter1.AssetID = assetID;
      parameter1.DebugRefID = 2;
      parameter1.DeviceType = DeviceTypeEnum.PL420;
      parameter1.EventUTC = currentTime;
      parameter1.fk_DimSourceID = 3;
      parameter1.GPSDeviceID = "Test";
      parameter1.ifk_DimSeverityLevelID = 1;
      parameter1.InsertUTC = DateTime.UtcNow;
      parameter1.ECMSourceAddress = 2;
      parameter1.ValueFloat = 9999.9999;
      parameter1.ValueString = "text";
      parameter1.PGN = 3;
      parameter1.SourceMsgID = 15;
      parameter1.SPN = 26;
      parameter1.ifk_DimUnitTypeID = (int)DimUnitTypeEnum.KilometersPerHour;
      data.Add(parameter1);

      DataParametersReport parameter2 = new DataParametersReport();
      parameter2.AssetID = assetID;
      parameter2.DebugRefID = 2;
      parameter2.DeviceType = DeviceTypeEnum.PL420;
      parameter2.EventUTC = currentTime;
      parameter2.fk_DimSourceID = 3;
      parameter2.GPSDeviceID = "Test";
      parameter2.ifk_DimSeverityLevelID = 1;
      parameter2.InsertUTC = DateTime.UtcNow;
      parameter2.ECMSourceAddress = 2;
      parameter2.ValueFloat = 9999.9999;
      parameter2.ValueString = "text";
      parameter2.PGN = 3;
      parameter2.SourceMsgID = 15;
      parameter2.SPN = 26;
      parameter2.ifk_DimUnitTypeID = (int)DimUnitTypeEnum.KilometersPerHour;
      data.Add(parameter2);

      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      var record = (from r in Ctx.DataContext.DataParametersReportReadOnly
                    where r.AssetID == assetID
                    select r);

      Assert.AreEqual(1, record.Count(), "there should be 2 parameter reports for this asset");
    }

    [Ignore]
    [TestMethod]
    [DatabaseTest]
    public void DataParametersReport_FilteredByDB()
    {
      long assetID = Asset.ComputeAssetID("CAT", "ReportSaveAll");
      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;
      DataParametersReport parameter1 = new DataParametersReport();
      parameter1.AssetID = assetID;
      parameter1.DebugRefID = 2;
      parameter1.DeviceType = DeviceTypeEnum.PL420;
      parameter1.EventUTC = currentTime;
      parameter1.fk_DimSourceID = 3;
      parameter1.GPSDeviceID = "Test";
      parameter1.ifk_DimSeverityLevelID = 1;
      parameter1.InsertUTC = DateTime.UtcNow;
      parameter1.ECMSourceAddress = 2;
      parameter1.ValueFloat = 9999.9999;
      parameter1.ValueString = "text";
      parameter1.PGN = 3;
      parameter1.SourceMsgID = 15;
      parameter1.SPN = 26;
      parameter1.ifk_DimUnitTypeID = (int)DimUnitTypeEnum.KilometersPerHour;
      data.Add(parameter1);

      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      DataParametersReport parameter2 = new DataParametersReport();
      parameter2.AssetID = assetID;
      parameter2.DebugRefID = 2;
      parameter2.DeviceType = DeviceTypeEnum.PL420;
      parameter2.EventUTC = currentTime;
      parameter2.fk_DimSourceID = 3;
      parameter2.GPSDeviceID = "Test";
      parameter2.ifk_DimSeverityLevelID = 1;
      parameter2.InsertUTC = DateTime.UtcNow;
      parameter2.ECMSourceAddress = 2;
      parameter2.ValueFloat = 9999.9999;
      parameter2.ValueString = "text";
      parameter2.PGN = 3;
      parameter2.SourceMsgID = 15;
      parameter2.SPN = 26;
      parameter2.ifk_DimUnitTypeID = (int)DimUnitTypeEnum.KilometersPerHour;
      data.Add(parameter2);

      retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      var record = (from r in Ctx.DataContext.DataParametersReportReadOnly
                    where r.AssetID == assetID
                    select r);

      Assert.AreEqual(1, record.Count(), "there should be 2 parameter reports for this asset");
    }

    [TestMethod]
    [DatabaseTest]
    public void DataStatisticsReport_SaveAll()
    {
      long assetID = Asset.ComputeAssetID("CAT", "ReportSaveAll");
      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;
      DataStatisticsReport statistic1 = new DataStatisticsReport();
      statistic1.AssetID = assetID;
      statistic1.DebugRefID = 2;
      statistic1.DeviceType = DeviceTypeEnum.PL420;
      statistic1.EventUTC = currentTime;
      statistic1.fk_DimSourceID = 3;
      statistic1.GPSDeviceID = "Test";
      statistic1.Minimum = 1;
      statistic1.Maximum = 10;
      statistic1.Average = 5;
      statistic1.StandardDeviation = .2;
      statistic1.RuntimeHours = 5000;
      statistic1.InsertUTC = DateTime.UtcNow;
      statistic1.ECMSourceAddress = 2;
      statistic1.PGN = 3;
      statistic1.SourceMsgID = 15;
      statistic1.SPN = 26;
      statistic1.ifk_DimUnitTypeID = (int)DimUnitTypeEnum.RevolutionsPerMinute;
      data.Add(statistic1);

      DataStatisticsReport statistic2 = new DataStatisticsReport();
      statistic2.AssetID = assetID;
      statistic2.DebugRefID = 2;
      statistic2.DeviceType = DeviceTypeEnum.PL420;
      statistic2.EventUTC = currentTime;
      statistic2.fk_DimSourceID = 3;
      statistic2.GPSDeviceID = "Test";
      statistic2.Minimum = 1;
      statistic2.Maximum = 10;
      statistic2.Average = 5;
      statistic2.StandardDeviation = .2;
      statistic2.RuntimeHours = 5000;
      statistic2.InsertUTC = DateTime.UtcNow;
      statistic2.ECMSourceAddress = 27;
      statistic2.PGN = 37;
      statistic2.SourceMsgID = 15;
      statistic2.SPN = 27;
      statistic2.ifk_DimUnitTypeID = (int)DimUnitTypeEnum.RevolutionsPerMinute;
      data.Add(statistic2);

      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      var record = (from r in Ctx.DataContext.DataStatisticsReportReadOnly
                    where r.AssetID == assetID
                    select r);

      Assert.AreEqual(2, record.Count(), "there should be 2 statistic reports for this asset");
    }

    [TestMethod]
    [DatabaseTest]
    public void DataStatisticsReport_FilteredByCode()
    {
      long assetID = Asset.ComputeAssetID("CAT", "ReportSaveAll");
      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;
      DataStatisticsReport statistic1 = new DataStatisticsReport();
      statistic1.AssetID = assetID;
      statistic1.DebugRefID = 2;
      statistic1.DeviceType = DeviceTypeEnum.PL420;
      statistic1.EventUTC = currentTime;
      statistic1.fk_DimSourceID = 3;
      statistic1.GPSDeviceID = "Test";
      statistic1.Minimum = 1;
      statistic1.Maximum = 10;
      statistic1.Average = 5;
      statistic1.StandardDeviation = .2;
      statistic1.RuntimeHours = 5000;
      statistic1.InsertUTC = DateTime.UtcNow;
      statistic1.ECMSourceAddress = 2;
      statistic1.PGN = 3;
      statistic1.SourceMsgID = 15;
      statistic1.SPN = 26;
      statistic1.ifk_DimUnitTypeID = (int)DimUnitTypeEnum.RevolutionsPerMinute;
      data.Add(statistic1);

      DataStatisticsReport statistic2 = new DataStatisticsReport();
      statistic2.AssetID = assetID;
      statistic2.DebugRefID = 2;
      statistic2.DeviceType = DeviceTypeEnum.PL420;
      statistic2.EventUTC = currentTime;
      statistic2.fk_DimSourceID = 3;
      statistic2.GPSDeviceID = "Test";
      statistic2.Minimum = 1;
      statistic2.Maximum = 10;
      statistic2.Average = 5;
      statistic2.StandardDeviation = .2;
      statistic2.RuntimeHours = 5000;
      statistic2.InsertUTC = DateTime.UtcNow;
      statistic2.ECMSourceAddress = 2;
      statistic2.PGN = 3;
      statistic2.SourceMsgID = 15;
      statistic2.SPN = 26;
      statistic2.ifk_DimUnitTypeID = (int)DimUnitTypeEnum.RevolutionsPerMinute;
      data.Add(statistic2);

      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      var record = (from r in Ctx.DataContext.DataStatisticsReportReadOnly
                    where r.AssetID == assetID
                    select r);

      Assert.AreEqual(1, record.Count(), "there should be 1 statistic report for this asset");
    }

    [TestMethod]
    [DatabaseTest]
    public void DataStatisticsReport_FilteredByDB()
    {
      long assetID = Asset.ComputeAssetID("CAT", "ReportSaveAll");
      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;
      DataStatisticsReport statistic1 = new DataStatisticsReport();
      statistic1.AssetID = assetID;
      statistic1.DebugRefID = 2;
      statistic1.DeviceType = DeviceTypeEnum.PL420;
      statistic1.EventUTC = currentTime;
      statistic1.fk_DimSourceID = 3;
      statistic1.GPSDeviceID = "Test";
      statistic1.Minimum = 1;
      statistic1.Maximum = 10;
      statistic1.Average = 5;
      statistic1.StandardDeviation = .2;
      statistic1.RuntimeHours = 5000;
      statistic1.InsertUTC = DateTime.UtcNow;
      statistic1.ECMSourceAddress = 2;
      statistic1.PGN = 3;
      statistic1.SourceMsgID = 15;
      statistic1.SPN = 26;
      statistic1.ifk_DimUnitTypeID = (int)DimUnitTypeEnum.RevolutionsPerMinute;
      data.Add(statistic1);

      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      DataStatisticsReport statistic2 = new DataStatisticsReport();
      statistic2.AssetID = assetID;
      statistic2.DebugRefID = 2;
      statistic2.DeviceType = DeviceTypeEnum.PL420;
      statistic2.EventUTC = currentTime;
      statistic2.fk_DimSourceID = 3;
      statistic2.GPSDeviceID = "Test";
      statistic2.Minimum = 1;
      statistic2.Maximum = 10;
      statistic2.Average = 5;
      statistic2.StandardDeviation = .2;
      statistic2.RuntimeHours = 5000;
      statistic2.InsertUTC = DateTime.UtcNow;
      statistic2.ECMSourceAddress = 2;
      statistic2.PGN = 3;
      statistic2.SourceMsgID = 15;
      statistic2.SPN = 26;
      statistic2.ifk_DimUnitTypeID = (int)DimUnitTypeEnum.RevolutionsPerMinute;
      data.Add(statistic2);

      retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      var record = (from r in Ctx.DataContext.DataStatisticsReportReadOnly
                    where r.AssetID == assetID
                    select r);

      Assert.AreEqual(1, record.Count(), "there should be 1 parameter report for this asset");
    }

    [TestMethod]
    [DatabaseTest]
    public void DataPowerLossSave_SaveAll()
    {
      long assetID = Asset.ComputeAssetID("CAT", "ReportSaveAll");
      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;
      DataPowerState powerLoss1 = new DataPowerState();
      powerLoss1.AssetID = assetID;
      powerLoss1.DebugRefID = 2;
      powerLoss1.DeviceType = DeviceTypeEnum.PL420;
      powerLoss1.EventUTC = currentTime;
      powerLoss1.fk_DimSourceID = 3;
      powerLoss1.GPSDeviceID = "Test";
      powerLoss1.InsertUTC = DateTime.UtcNow;
      powerLoss1.SourceMsgID = 15;
      powerLoss1.IsOn = false;
      data.Add(powerLoss1);

      DataPowerState powerLoss2 = new DataPowerState();
      powerLoss2.AssetID = assetID;
      powerLoss2.DebugRefID = 2;
      powerLoss2.DeviceType = DeviceTypeEnum.PL420;
      powerLoss2.EventUTC = currentTime.AddHours(1);
      powerLoss2.fk_DimSourceID = 3;
      powerLoss2.GPSDeviceID = "Test";
      powerLoss2.InsertUTC = DateTime.UtcNow;
      powerLoss2.SourceMsgID = 15;
      powerLoss2.IsOn = false;
      data.Add(powerLoss2);

      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      var record = (from r in Ctx.DataContext.DataPowerStateReadOnly
                    where r.AssetID == assetID
                    select r);

      Assert.AreEqual(2, record.Count(), "there should be 2 powerLoss reports for this asset");
    }

    [TestMethod]
    [DatabaseTest]
    public void DataPowerLossSave_FilteredByCode()
    {
      long assetID = Asset.ComputeAssetID("CAT", "ReportSaveAll");
      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;
      DataPowerState powerLoss1 = new DataPowerState();
      powerLoss1.AssetID = assetID;
      powerLoss1.DebugRefID = 2;
      powerLoss1.DeviceType = DeviceTypeEnum.PL420;
      powerLoss1.EventUTC = currentTime;
      powerLoss1.fk_DimSourceID = 3;
      powerLoss1.GPSDeviceID = "Test";
      powerLoss1.InsertUTC = DateTime.UtcNow;
      powerLoss1.SourceMsgID = 15;
      powerLoss1.IsOn = false;
      data.Add(powerLoss1);

      DataPowerState powerLoss2 = new DataPowerState();
      powerLoss2.AssetID = assetID;
      powerLoss2.DebugRefID = 2;
      powerLoss2.DeviceType = DeviceTypeEnum.PL420;
      powerLoss2.EventUTC = currentTime;
      powerLoss2.fk_DimSourceID = 3;
      powerLoss2.GPSDeviceID = "Test";
      powerLoss2.InsertUTC = DateTime.UtcNow;
      powerLoss2.SourceMsgID = 15;
      powerLoss2.IsOn = false;
      data.Add(powerLoss2);

      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      var record = (from r in Ctx.DataContext.DataPowerStateReadOnly
                    where r.AssetID == assetID
                    select r);

      Assert.AreEqual(1, record.Count(), "there should be 1 powerLoss report for this asset");
    }

    [TestMethod]
    [DatabaseTest]
    public void DataPowerLossSave_FilteredByDB()
    {
      long assetID = Asset.ComputeAssetID("CAT", "ReportSaveAll");
      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;
      DataPowerState powerLoss1 = new DataPowerState();
      powerLoss1.AssetID = assetID;
      powerLoss1.DebugRefID = 2;
      powerLoss1.DeviceType = DeviceTypeEnum.PL420;
      powerLoss1.EventUTC = currentTime;
      powerLoss1.fk_DimSourceID = 3;
      powerLoss1.GPSDeviceID = "Test";
      powerLoss1.InsertUTC = DateTime.UtcNow;
      powerLoss1.SourceMsgID = 15;
      powerLoss1.IsOn = false;
      data.Add(powerLoss1);

      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      DataPowerState powerLoss2 = new DataPowerState();
      powerLoss2.AssetID = assetID;
      powerLoss2.DebugRefID = 2;
      powerLoss2.DeviceType = DeviceTypeEnum.PL420;
      powerLoss2.EventUTC = currentTime;
      powerLoss2.fk_DimSourceID = 3;
      powerLoss2.GPSDeviceID = "Test";
      powerLoss2.InsertUTC = DateTime.UtcNow;
      powerLoss2.SourceMsgID = 15;
      powerLoss2.IsOn = false;
      data.Add(powerLoss2);

      retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      var record = (from r in Ctx.DataContext.DataPowerStateReadOnly
                    where r.AssetID == assetID
                    select r);

      Assert.AreEqual(1, record.Count(), "there should be 2 parameter reports for this asset");
    }

    [TestMethod]
    [DatabaseTest]
    public void DataIdleTimeOutSave_SaveAll()
    {
      long assetID = Asset.ComputeAssetID("CAT", "ReportSaveAll");
      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;

      DataIdleTimeOut idleTimeOut1 = new DataIdleTimeOut();
      idleTimeOut1.AssetID = assetID;
      idleTimeOut1.DebugRefID = 2;
      idleTimeOut1.EventUTC = currentTime;
      idleTimeOut1.SourceMsgID = 15;
      idleTimeOut1.fk_DimSourceID = 3;
      data.Add(idleTimeOut1);

      DataIdleTimeOut idleTimeOut2 = new DataIdleTimeOut();
      idleTimeOut2.AssetID = assetID;
      idleTimeOut2.DebugRefID = 2;
      idleTimeOut2.EventUTC = currentTime.AddHours(1);
      idleTimeOut2.SourceMsgID = 15;
      idleTimeOut2.fk_DimSourceID = 3;
      data.Add(idleTimeOut2);

      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      var record = (from r in Ctx.DataContext.DataIdleTimeOutReadOnly
                    where r.AssetID == assetID
                    select r);

      Assert.AreEqual(2, record.Count(), "there should be 2 idle time outs for this asset");
    }

    [TestMethod]
    [DatabaseTest]
    public void DataIdleTimeOut_FilteredByCode()
    {
      long assetID = Asset.ComputeAssetID("CAT", "ReportSaveAll");
      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;

      DataIdleTimeOut idleTimeOut1 = new DataIdleTimeOut();
      idleTimeOut1.AssetID = assetID;
      idleTimeOut1.DebugRefID = 2;
      idleTimeOut1.EventUTC = currentTime;
      idleTimeOut1.SourceMsgID = 15;
      idleTimeOut1.fk_DimSourceID = 3;
      data.Add(idleTimeOut1);

      DataIdleTimeOut idleTimeOut2 = new DataIdleTimeOut();
      idleTimeOut2.AssetID = assetID;
      idleTimeOut2.DebugRefID = 2;
      idleTimeOut2.EventUTC = currentTime;
      idleTimeOut2.SourceMsgID = 15;
      idleTimeOut2.fk_DimSourceID = 3;
      data.Add(idleTimeOut2);


      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      var record = (from r in Ctx.DataContext.DataIdleTimeOutReadOnly
                    where r.AssetID == assetID
                    select r);

      Assert.AreEqual(1, record.Count(), "there should be 1 idle time out for this asset");
    }

    [TestMethod]
    [DatabaseTest]
    public void DataIdleTimeOut_FilteredByDB()
    {
      long assetID = Asset.ComputeAssetID("CAT", "ReportSaveAll");
      List<INHDataObject> data = new List<INHDataObject>();
      DateTime currentTime = DateTime.UtcNow;

      DataIdleTimeOut idleTimeOut1 = new DataIdleTimeOut();
      idleTimeOut1.AssetID = assetID;
      idleTimeOut1.DebugRefID = 2;
      idleTimeOut1.EventUTC = currentTime;
      idleTimeOut1.SourceMsgID = 15;
      idleTimeOut1.fk_DimSourceID = 3;
      data.Add(idleTimeOut1);

      bool retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      DataIdleTimeOut idleTimeOut2 = new DataIdleTimeOut();
      idleTimeOut2.AssetID = assetID;
      idleTimeOut2.DebugRefID = 2;
      idleTimeOut2.EventUTC = currentTime;
      idleTimeOut2.SourceMsgID = 15;
      idleTimeOut2.fk_DimSourceID = 3;
      data.Add(idleTimeOut2);

      retryExpected = false;
      NHDataSaver.Save(data, out retryExpected);

      var record = (from r in Ctx.DataContext.DataIdleTimeOutReadOnly
                    where r.AssetID == assetID
                    select r);

      Assert.AreEqual(1, record.Count(), "there should be 1 idle time out for this asset");
    }


    #region Utility functions

    protected DataSwitchState GetDataSwitchState(DateTime eventUTC)
    {
      DataSwitchState obj = new DataSwitchState();
      obj.EventUTC = eventUTC;
      obj.InsertUTC = DateTime.UtcNow;
      obj.fk_DimSourceID = (int)DimSourceEnum.PR3Gateway;
      obj.DebugRefID = -9999;
      obj.SourceMsgID = 111;
      obj.AssetID = 37421;

      obj.InputNumber = 2;
      obj.IOState = true;
      return obj;
    }

    protected DataEngineParameters GetDataEngineParameters(DateTime eventUTC)
    {
      DataEngineParameters obj = new DataEngineParameters();
      obj.EventUTC = eventUTC;
      obj.InsertUTC = DateTime.UtcNow;
      obj.fk_DimSourceID = (int)DimSourceEnum.PR3Gateway;
      obj.DebugRefID = -9999;
      obj.SourceMsgID = 222;
      obj.AssetID = 37421;

      obj.ConsumptionGallons = 550;
      obj.EngineIdleHours = 1100;
      obj.IdleFuelGallons = 20;
      obj.LevelPercent = 63;
      obj.MachineIdleFuelGallons = 300;
      obj.MachineIdleHours = 1500;
      obj.MaxFuelGallons = 300.8;
      obj.MID = "555";
      obj.Revolutions = 888;
      obj.Starts = 99;
      return obj;
    }

    protected DataEngineStartStop GetDataEngineStartStop(DateTime eventUTC)
    {
      DataEngineStartStop obj = new DataEngineStartStop();
      obj.EventUTC = eventUTC;
      obj.InsertUTC = DateTime.UtcNow;
      obj.fk_DimSourceID = (int)DimSourceEnum.PR3Gateway;
      obj.AssetID = 37421;

      obj.ifk_EngineStateID = (int)DimEngineStateEnum.EngineOn;
      obj.DebugRefID = -9999;
      obj.SourceMsgID = 222;
      return obj;
    }

    protected DataFaultDiagnostic GetDataFaultDiagnostic(DateTime eventUTC)
    {
      DataFaultDiagnostic obj = new DataFaultDiagnostic();
      obj.EventUTC = eventUTC;
      obj.InsertUTC = DateTime.UtcNow;
      obj.fk_DimSourceID = (int)DimSourceEnum.PR3Gateway;
      obj.DebugRefID = -9999;
      obj.SourceMsgID = 333;
      obj.AssetID = 37421;

      obj.CID = 111;
      obj.fk_DimDatalinkID = (int)DatalinkEnum.CDL;
      obj.fk_DimSeverityLevelID = (int)DimSeverityLevelEnum.High;
      obj.MID = "222";
      obj.FMI = 90;
      obj.Occurrences = 5;
      return obj;
    }

    protected DataFaultEvent GetDataFaultEvent(DateTime eventUTC)
    {
      DataFaultEvent obj = new DataFaultEvent();
      obj.EventUTC = eventUTC;
      obj.InsertUTC = DateTime.UtcNow;
      obj.fk_DimSourceID = (int)DimSourceEnum.PR3Gateway;
      obj.DebugRefID = -9999;
      obj.SourceMsgID = 444;
      obj.AssetID = 37421;

      obj.EID = 1;
      obj.fk_DimDatalinkID = (int)DatalinkEnum.CDL;
      obj.fk_DimSeverityLevelID = (int)DimSeverityLevelEnum.High;
      obj.MID = "222";
      obj.Occurrences = 3;
      return obj;
    }

    protected DataFenceAlarm GetDataFenceAlarm(DateTime eventUTC)
    {
      DataFenceAlarm obj = new DataFenceAlarm();
      obj.EventUTC = eventUTC;
      obj.InsertUTC = DateTime.UtcNow;
      obj.fk_DimSourceID = (int)DimSourceEnum.PR3Gateway;
      obj.DebugRefID = -9999;
      obj.SourceMsgID = 555;
      obj.AssetID = 37421;

      obj.DisconnectSwitchUsed = true;
      obj.ExclusiveWatchActive = false;
      obj.ExclusiveWatchAlarm = true;
      obj.InclusiveWatchActive = false;
      obj.InclusiveWatchAlarm = true;
      obj.SatelliteBlockage = false;
      obj.TimeWatchActive = true;
      obj.TimeWatchAlarm = false;
      return obj;
    }

    protected DataHoursLocation GetDataHoursLocation(DateTime eventUTC)
    {
      DataHoursLocation obj = new DataHoursLocation();
      obj.EventUTC = eventUTC;
      obj.InsertUTC = DateTime.UtcNow;
      obj.fk_DimSourceID = (int)DimSourceEnum.PR3Gateway;
      obj.DebugRefID = -9999;
      obj.SourceMsgID = 666;
      obj.AssetID = 37421;

      obj.Altitude = 12.4;
      obj.Latitude = 123.123;
      obj.Longitude = -32.65;
      obj.LocationAge = 56;
      obj.LocationUncertaintyMeters = 45;
      obj.OdometerMiles = 108796.5;
      obj.RuntimeHours = 7685;
      obj.SpeedMPH = 65.8;
      obj.Track = 13.5;

      return obj;
    }

    protected DataIgnOnOff GetDataIgnOnOff(DateTime eventUTC)
    {
      DataIgnOnOff obj = new DataIgnOnOff();
      obj.EventUTC = eventUTC;
      obj.InsertUTC = DateTime.UtcNow;
      obj.fk_DimSourceID = (int)DimSourceEnum.PR3Gateway;
      obj.DebugRefID = -9999;
      obj.SourceMsgID = 777;
      obj.AssetID = 37421;

      obj.IsOn = false;
      obj.RuntimeHours = 500;
      return obj;
    }

    protected DataMoving GetDataMoving(DateTime eventUTC)
    {
      DataMoving obj = new DataMoving();
      obj.EventUTC = eventUTC;
      obj.InsertUTC = DateTime.UtcNow;
      obj.fk_DimSourceID = (int)DimSourceEnum.PR3Gateway;
      obj.DebugRefID = -9999;
      obj.SourceMsgID = 888;
      obj.AssetID = 37421;

      obj.IsStart = false;
      obj.SuspiciousMove = false;
      return obj;
    }

    protected DataMSSKeyID GetDataMSSKeyID(DateTime eventUTC)
    {
      DataMSSKeyID obj = new DataMSSKeyID();

      obj.EventUTC = eventUTC;
      obj.InsertUTC = DateTime.UtcNow;
      obj.fk_DimSourceID = (int)DimSourceEnum.PR3Gateway;
      obj.DebugRefID = -9999;
      obj.SourceMsgID = 999;
      obj.AssetID = 37421;

      obj.MSSKeyID = 878787;
      return obj;
    }

    protected DataServiceMeterAdjustment GetDataServiceMeterAdjustment(DateTime eventUTC)
    {
      DataServiceMeterAdjustment obj = new DataServiceMeterAdjustment();
      obj.EventUTC = eventUTC;
      obj.InsertUTC = DateTime.UtcNow;
      obj.fk_DimSourceID = (int)DimSourceEnum.PR3Gateway;
      obj.DebugRefID = -9999;
      obj.SourceMsgID = 101010;
      obj.AssetID = 37421;

      obj.RuntimeAfterHours = 2;
      obj.RuntimeBeforeHours = 1000.9;
      return obj;
    }

    protected DataSiteState GetDataSiteState(DateTime eventUTC)
    {
      DataSiteState obj = new DataSiteState();
      obj.EventUTC = eventUTC;
      obj.InsertUTC = DateTime.UtcNow;
      obj.fk_DimSourceID = (int)DimSourceEnum.PR3Gateway;
      obj.DebugRefID = -9999;
      obj.SourceMsgID = 101010;
      obj.AssetID = 37421;

      obj.SiteID = 1212;
      obj.IsEntry = true;

      return obj;
    }

    protected DataSpeeding GetDataSpeeding(DateTime eventUTC)
    {
      DataSpeeding obj = new DataSpeeding();
      obj.EventUTC = eventUTC;
      obj.InsertUTC = DateTime.UtcNow;
      obj.fk_DimSourceID = (int)DimSourceEnum.PR3Gateway;
      obj.DebugRefID = -9999;
      obj.SourceMsgID = 101010;
      obj.AssetID = 37421;

      obj.DistanceTravelled = 1200.8;
      obj.Duration = 120;
      obj.MaxSpeedMPH = 11;
      obj.IsStart = true;
      return obj;
    }
    
    protected DataPassThroughPortData GetDataPassThroughPortData(DateTime eventUTC)
    {
      DataPassThroughPortData obj = new DataPassThroughPortData();
      obj.EventUTC = eventUTC;
      obj.InsertUTC = DateTime.UtcNow;
      obj.fk_DimSourceID = (int)DimSourceEnum.PR3Gateway;
      obj.DebugRefID = -9999;
      obj.SourceMsgID = 1244;
      obj.AssetID = 37421;

      obj.PortNumber = 1275;
      obj.Payload = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 };
      return obj;
    }

    protected DataRawCANMessage GetDataRawCANMessage(DateTime eventUTC)
    {
      DataRawCANMessage obj = new DataRawCANMessage
      {
        EventUTC=eventUTC,
        InsertUTC=DateTime.UtcNow,
        fk_DimSourceID=(int)DimSourceEnum.PR3Gateway,
        DebugRefID=-9999,
        SourceMsgID=888,
        AssetID=37421,
        Message=new byte[]{1,3,5,7,9},
        MessageHash=16,
      };

      return obj;
    }

    protected DataFeedDigitalSwitchStatus GetDataFeedDigitalSwitchStatus(DateTime eventUTC, byte switsch)
    {
      DataFeedDigitalSwitchStatus obj = new DataFeedDigitalSwitchStatus
      {
        EventUTC = eventUTC,
        InsertUTC = DateTime.UtcNow,
        fk_DimSourceID = (int)DimSourceEnum.PR3Gateway,
        DebugRefID = -9999,
        SourceMsgID = 888,
        AssetID = 37421,
        Switch = switsch,
        IsActive = true,
        PowerMode = 1,
        Pending = false
      };

      return obj;
    }

    protected DataFluidAnalysis GetDataFluidAnalysis(long sampleNumber, long? actionNumber=null)
    {
      DataFluidAnalysis obj = new DataFluidAnalysis
      {
        InsertUTC = DateTime.UtcNow,
        fk_DimSourceID = (int)DimSourceEnum.PR3Gateway,
        DebugRefID = -9999,
        SourceMsgID = 888,
        AssetID = 37421,
        SampleNumber = sampleNumber,
        ActionNumber = actionNumber,
        TextID = "sample001",
        SampleTakenDate = DateTime.Now,
        CompartmentName = "glovebox",
        CompartmentID = "123",
        Status = "S",
      };

      return obj;
    }

    protected DataParametersReport GetDataParametersReport(DateTime eventUTC, int ecmSourceAddress=1001, int pgn=10, int spn=12)
    {
      DataParametersReport obj = new DataParametersReport
      {
        EventUTC = eventUTC,
        InsertUTC = DateTime.UtcNow,
        fk_DimSourceID = (int)DimSourceEnum.PR3Gateway,
        DebugRefID = -9999,
        SourceMsgID = 888,
        AssetID = 37421,
        ECMSourceAddress = ecmSourceAddress,
        PGN = pgn,
        SPN=spn,
        ifk_DimSeverityLevelID = 2,
        ifk_DimUnitTypeID=1,
      };

      return obj;
    }

    protected DataStatisticsReport GetDataStatisticsReport(DateTime eventUTC, int ecmSourceAddress = 1001, int pgn = 10, int spn = 12)
    {
      DataStatisticsReport obj = new DataStatisticsReport
      {
        EventUTC = eventUTC,
        InsertUTC = DateTime.UtcNow,
        fk_DimSourceID = (int)DimSourceEnum.PR3Gateway,
        DebugRefID = -9999,
        SourceMsgID = 888,
        AssetID = 37421,
        ECMSourceAddress = ecmSourceAddress,
        PGN = pgn,
        SPN = spn,
        Minimum = 10,
        Maximum = 20,
        StandardDeviation = 2.3,
        ifk_DimUnitTypeID = 1,
      };

      return obj;
    }

    protected DataPowerState GetDataPowerLoss(DateTime eventUTC)
    {
      DataPowerState obj = new DataPowerState
      {
        EventUTC = eventUTC,
        InsertUTC = DateTime.UtcNow,
        fk_DimSourceID = (int)DimSourceEnum.PR3Gateway,
        DebugRefID = -9999,
        SourceMsgID = 888,
        AssetID = 37421,
        IsOn = false,
      };

      return obj;
    }

    protected DataTamperSecurityStatus GetDataTamperSecurityStatus(DateTime eventUTC)
    {
      DataTamperSecurityStatus obj = new DataTamperSecurityStatus
      {
        EventUTC = eventUTC,
        InsertUTC = DateTime.UtcNow,
        fk_DimSourceID = (int)DimSourceEnum.PR3Gateway,
        DebugRefID = -9999,
        SourceMsgID = 888,
        AssetID = 37421,
        MachineStartStatus = 2,
        MachineStartStatusTrigger=1,
      };

      return obj;
    }

    protected DataIdleTimeOut GetDataIdleTimeOut(DateTime eventUTC)
    {
      DataIdleTimeOut obj = new DataIdleTimeOut
      {
        EventUTC = eventUTC,
        InsertUTC = DateTime.UtcNow,
        fk_DimSourceID = (int)DimSourceEnum.PR3Gateway,
        DebugRefID = -9999,
        SourceMsgID = 888,
        AssetID = 37421
      };

      return obj;
    }


    #endregion

  }
}
