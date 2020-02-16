using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{
  public class ReportLogicTestBase : UnitTestBase
  {
    protected double? GMTlatitude = 20; // GMT - so we don't have to worry about timezone offsets
    protected double? GMTlongitude = -8.021608;

    protected void AssertAssetOperationTotals(INH_RPT rptCtx, long assetID, int assetKeyDate, int expectedSegmentCount,
                    double expectedIdleHours, double expectedWorkingHours, double expectedRunningHours)
    {

      List<FactAssetOperationPeriod> faup = (from aSPeriod in rptCtx.FactAssetOperationPeriodReadOnly
                                             where aSPeriod.ifk_DimAssetID == assetID
                                              && aSPeriod.fk_AssetKeyDate == assetKeyDate
                                             orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
                                             select aSPeriod).ToList<FactAssetOperationPeriod>();

      DateTime startOfDeviceDay = assetKeyDate.FromKeyDate();
      Assert.IsNotNull(faup, "Failed to create FactAssetOperationPeriod object");
      if (expectedSegmentCount > -1)
        Assert.AreEqual(expectedSegmentCount, faup.Count(), "Incorrect count of FactAssetOperationnPeriod records on: {0}.", startOfDeviceDay);

      IEnumerable<FactAssetOperationPeriod> values;
      values = from u in faup
               where u.ifk_DimAssetWorkingStateID == 2 // 2=idling
               select u;
      Assert.AreEqual(expectedIdleHours,
            ((double)values.Sum(d =>
                ((d.EndStateDeviceLocal > startOfDeviceDay.AddDays(1) ? startOfDeviceDay.AddDays(1) : d.EndStateDeviceLocal)
                  - (d.StartStateDeviceLocal < startOfDeviceDay ? startOfDeviceDay : d.StartStateDeviceLocal))
                .TotalHours)),
            string.Format("IdleHoursIncorrect in FactAssetOperationPeriod records on: {0}", startOfDeviceDay));

      values = from u in faup
               where u.ifk_DimAssetWorkingStateID == 8 // 8=working
               select u;
      Assert.AreEqual(expectedWorkingHours,
            ((double)values.Sum(d =>
                ((d.EndStateDeviceLocal > startOfDeviceDay.AddDays(1) ? startOfDeviceDay.AddDays(1) : d.EndStateDeviceLocal)
                  - (d.StartStateDeviceLocal < startOfDeviceDay ? startOfDeviceDay : d.StartStateDeviceLocal))
                .TotalHours)),
            string.Format("WorkingHoursIncorrect in FactAssetUtilizationPeriod records on: {0}.", startOfDeviceDay));

      values = from u in faup
               where u.ifk_DimAssetWorkingStateID == 1 // 1=assetOn
               select u;
      Assert.AreEqual(expectedRunningHours,
            ((double)values.Sum(d =>
                ((d.EndStateDeviceLocal > startOfDeviceDay.AddDays(1) ? startOfDeviceDay.AddDays(1) : d.EndStateDeviceLocal)
                  - (d.StartStateDeviceLocal < startOfDeviceDay ? startOfDeviceDay : d.StartStateDeviceLocal))
                .TotalHours)),
            string.Format("RunningHoursIncorrect in FactAssetUtilizationPeriod records on: {0}.", startOfDeviceDay));
    }

    protected void SetupNH_DATAForDay(long assetID, DateTime? startOfDeviceDay = null, int? day = null, 
                                double? runtimeHours = null, double? idleHours = -1, double? totalFuel = -1, double? idleFuel = -1, long runtimeOffsetMs = 0)
    {
      SetupNH_DATA(assetID, startOfDeviceDay, day, runtimeHours, idleHours, totalFuel, null, null, null, null, idleFuel, null, null, runtimeOffsetMs);
    }

    protected void SetupNH_DATAForDateTime(long assetID, DateTime? startOfDeviceDay = null, int? day = null, 
                                double? runtimeHours = null, double? idleHours = -1, double? totalFuel = -1, double? idleFuel = -1, long runtimeOffsetMs = 0, DateTime? eventUTCoverride = null)
    {
      SetupNH_DATA(assetID, startOfDeviceDay, day, runtimeHours, idleHours, totalFuel, null, null, null, null, idleFuel, null, null, runtimeOffsetMs, eventUTCoverride);
    }

    protected void SetupNH_DATAForOffsetSeconds(long assetID, DateTime? startOfDeviceDay = null, int? day = null, 
                                double? runtimeHours = null, double? engineIdleHours = null, double? consumptionGallons = null, double? maxFuelGallons = null, double? machineIdleFuelGallons = null, long? revolutions = null, int? starts = null,
                                double? idleFuelGallons = null, double? percentRemaining = null, double? machineIdleHours = null, long runtimeOffsetMs = 0, int eventOffsetSeconds = 0)
    {
        DateTime eventUTC = startOfDeviceDay.Value.AddDays(day.Value).AddSeconds(eventOffsetSeconds);

        if (maxFuelGallons.HasValue || idleFuelGallons.HasValue || machineIdleFuelGallons.HasValue || engineIdleHours.HasValue || starts.HasValue || revolutions.HasValue || consumptionGallons.HasValue || percentRemaining.HasValue || machineIdleHours.HasValue)
        {
            Helpers.NHData.EngineParameters_Add(assetID, eventUTC, maxFuelGallons, idleFuelGallons, machineIdleFuelGallons, engineIdleHours, starts,
                                                revolutions, consumptionGallons, percentRemaining, machineIdleHours);
        }

        Helpers.NHData.DataHoursLocation_Add(assetID, DimSourceEnum.TelematicsSync, eventUTC.AddMilliseconds(runtimeOffsetMs), runtimeHours: runtimeHours, latitude: GMTlatitude,
                                                  longitude: GMTlongitude);
    }

    protected void SetupNH_DATA(long assetID, DateTime? startOfDeviceDay = null, int? day = null, 
                                double? runtimeHours = null, double? engineIdleHours = null, double? consumptionGallons = null, double? maxFuelGallons = null, double? machineIdleFuelGallons = null, long? revolutions = null, int? starts = null,
                                double? idleFuelGallons = null, double? percentRemaining = null, double? machineIdleHours = null, long runtimeOffsetMs = 0, DateTime? eventUTCoverride = null)
    {
      DateTime eventUTC = eventUTCoverride.HasValue ? eventUTCoverride.Value : startOfDeviceDay.Value.AddDays(day.Value);

      if (maxFuelGallons.HasValue || idleFuelGallons.HasValue || machineIdleFuelGallons.HasValue || engineIdleHours.HasValue || starts.HasValue || revolutions.HasValue || consumptionGallons.HasValue || percentRemaining.HasValue || machineIdleHours.HasValue)
      {
        Helpers.NHData.EngineParameters_Add(assetID, eventUTC, maxFuelGallons, idleFuelGallons, machineIdleFuelGallons, engineIdleHours, starts,
                                            revolutions, consumptionGallons, percentRemaining, machineIdleHours);
      }

      Helpers.NHData.DataHoursLocation_Add(assetID, DimSourceEnum.TelematicsSync, eventUTC.AddMilliseconds(runtimeOffsetMs), runtimeHours: runtimeHours, latitude: GMTlatitude,
                                                longitude: GMTlongitude);
    }

    protected void SetupNH_DATA_WithOptionalHoursLocation(long assetID, DateTime? startOfDeviceDay = null, int? day = null, double? runtimeHours = null, double? engineIdleHours = null, double? consumptionGallons = null, double? maxFuelGallons = null, double? machineIdleFuelGallons = null, long? revolutions = null, int? starts = null,
                            double? idleFuelGallons = null, double? percentRemaining = null, double? machineIdleHours = null, long runtimeOffsetMs = 0)
    {
      DateTime eventUTC = startOfDeviceDay.Value.AddDays(day.Value);

      if (maxFuelGallons.HasValue || idleFuelGallons.HasValue || machineIdleFuelGallons.HasValue || engineIdleHours.HasValue || starts.HasValue || revolutions.HasValue || consumptionGallons.HasValue || percentRemaining.HasValue || machineIdleHours.HasValue)
      {
        Helpers.NHData.EngineParameters_Add(assetID, eventUTC, maxFuelGallons, idleFuelGallons, machineIdleFuelGallons, engineIdleHours, starts,
                                            revolutions, consumptionGallons, percentRemaining, machineIdleHours);
      }

      if (GMTlatitude.HasValue || GMTlongitude.HasValue || runtimeHours.HasValue)
      {
        Helpers.NHData.DataHoursLocation_Add(assetID, DimSourceEnum.TelematicsSync, eventUTC.AddMilliseconds(runtimeOffsetMs), runtimeHours: runtimeHours, latitude: GMTlatitude,
                                                longitude: GMTlongitude);
      }
    }

    protected void SetupNH_DATA_Engine_ForDay(long assetID, long milliseconds, DateTime? startOfDeviceDay = null, int? day = null, double? runtimeHours = null, double? idleHours = -1, double? totalFuel = -1, double? idleFuel = -1)
    {
      SetupNH_DATA_Payload_Engine(assetID, milliseconds, startOfDeviceDay, day, runtimeHours, idleHours, totalFuel, null, null, null, null, idleFuel, null, null);
    }

    protected void SetupNH_DATA_Payload_Engine(long assetID, long milliseconds, DateTime? startOfDeviceDay = null, int? day = null, double? runtimeHours = null, double? engineIdleHours = null, double? consumptionGallons = null, double? maxFuelGallons = null, double? machineIdleFuelGallons = null, long? revolutions = null, int? starts = null,
                                  double? idleFuelGallons = null, double? percentRemaining = null, double? machineIdleHours = null)
    {
      DateTime eventUTC = startOfDeviceDay.Value.AddDays(day.Value).AddMilliseconds(milliseconds);

      Helpers.NHData.DataHoursLocation_Add(assetID, DimSourceEnum.TelematicsSync, eventUTC, runtimeHours: runtimeHours, latitude: GMTlatitude,
                                            longitude: GMTlongitude);

      if (maxFuelGallons.HasValue || idleFuelGallons.HasValue || machineIdleFuelGallons.HasValue || engineIdleHours.HasValue || starts.HasValue || revolutions.HasValue || consumptionGallons.HasValue || percentRemaining.HasValue || machineIdleHours.HasValue)
      {
        Helpers.NHData.EngineParameters_Add(assetID, eventUTC, maxFuelGallons, idleFuelGallons, machineIdleFuelGallons, engineIdleHours, starts,
                                            revolutions, consumptionGallons, percentRemaining, machineIdleHours);
      }
    }

    protected void SetupNH_DATA_Payload_ForDay(long assetID, long milliseconds, DateTime? startOfDeviceDay = null, int? day = null, long? payload = null, long? cycles = null, long? utilization = null)
    {
      DateTime eventUTC = startOfDeviceDay.Value.AddDays(day.Value).AddMilliseconds(milliseconds);


      if (payload.HasValue || utilization.HasValue || cycles.HasValue)
      {
        Helpers.NHData.PayloadCycle_Add(assetID, eventUTC, payload, cycles, utilization);
      }
    }

    protected void AssertNH_RPTFactUtilizationDaily(List<FactAssetUtilizationDaily> fauds, DateTime? startOfDeviceDay = null, int? day = -1, DateTime? eventUTCoverride = null,
      double? runtimeHoursDelta = -1,
      double? idleHoursDelta = null,
      double? workingHoursDelta = null,
      double? totalFuelDelta = null,
      double? idleFuelDelta = null,
      long? workingFuelDelta = null)
    {
      DateTime eventUTC = eventUTCoverride.HasValue ? eventUTCoverride.Value : startOfDeviceDay.Value.AddDays(day.Value);

      FactAssetUtilizationDaily faud = (from f in fauds where f.EventUTC == eventUTC select f).FirstOrDefault();
      Assert.IsNotNull(faud, "FactAssetUtilizationDaily record is missing for day " + day);
      Assert.AreEqual(runtimeHoursDelta, faud.RuntimeHours, "RuntimeHoursDelta is incorrect for day " + day);
      if (idleHoursDelta.HasValue) Assert.AreEqual(idleHoursDelta, faud.IdleHours, "IdleHours is incorrect for day " + day);
      if (workingHoursDelta.HasValue) Assert.AreEqual(workingHoursDelta, faud.WorkingHours, "WorkingHours is incorrect for day " + day);
      if (totalFuelDelta.HasValue) Assert.AreEqual(totalFuelDelta, faud.TotalFuelConsumedGallons, "TotalFuelConsumedGallons is incorrect for day " + day);
      if (idleFuelDelta.HasValue) Assert.AreEqual(idleFuelDelta, faud.IdleFuelConsumedGallons, "IdleFuelConsumedGallons is incorrect for day " + day);
      if (workingFuelDelta.HasValue) Assert.AreEqual(workingFuelDelta, faud.WorkingFuelConsumedGallons, "WorkingFuelConsumedGallons is incorrect for day " + day);
    }

    protected void AssertNH_RPTFactUtilizationDailyFlags(List<FactAssetUtilizationDaily> fauds, DateTime? startOfDeviceDay = null, int? day = -1, DateTime? eventUTCoverride = null,
      DimUtilizationCalloutTypeEnum runtimeHoursDeltaFlag = DimUtilizationCalloutTypeEnum.None,
      DimUtilizationCalloutTypeEnum idleHoursDeltaFlag = DimUtilizationCalloutTypeEnum.None,
      DimUtilizationCalloutTypeEnum workingHoursDeltaFlag = DimUtilizationCalloutTypeEnum.None,
      DimUtilizationCalloutTypeEnum totalFuelDeltaFlag = DimUtilizationCalloutTypeEnum.None,
      DimUtilizationCalloutTypeEnum idleFuelDeltaFlag = DimUtilizationCalloutTypeEnum.None)
    {
      DateTime eventUTC = eventUTCoverride.HasValue ? eventUTCoverride.Value : startOfDeviceDay.Value.AddDays(day.Value);

      FactAssetUtilizationDaily faud = (from f in fauds where f.EventUTC == eventUTC select f).FirstOrDefault();
      Assert.IsNotNull(faud, "FactAssetUtilizationDaily record is missing for day " + day);
      Assert.AreEqual((int)runtimeHoursDeltaFlag, faud.ifk_RuntimeHoursCalloutTypeID, "RuntimeHoursDeltaFlag should be " + runtimeHoursDeltaFlag.ToString() + " for day " + day);
      Assert.AreEqual((int)idleHoursDeltaFlag, faud.ifk_IdleHoursCalloutTypeID, "IdleHoursFlag should be " + idleHoursDeltaFlag.ToString() + " for day " + day);
      Assert.AreEqual((int)workingHoursDeltaFlag, faud.ifk_WorkingHoursCalloutTypeID, "WorkingHoursFlag should be " + workingHoursDeltaFlag.ToString() + " for day " + day);
      Assert.AreEqual((int)totalFuelDeltaFlag, faud.ifk_TotalFuelConsumedGallonsCalloutTypeID, "TotalFuelConsumedGallonsFlag should be " + totalFuelDeltaFlag.ToString() + " for day " + day);
      Assert.AreEqual((int)idleFuelDeltaFlag, faud.ifk_IdleFuelConsumedGallonsCalloutTypeID, "IdleFuelConsumedGallonsFlag should be " + idleFuelDeltaFlag.ToString() + " for day " + day);
    }

    protected void AssertNH_RPTFactPayloadCycleUtilizationDaily(List<FactPayloadCycleUtilizationDaily> fpcus, long milliseconds, DateTime? startOfDeviceDay = null, int? day = -1,
      double? payloadValue = -1, double? cycleValue = -1, bool checkPayload = true, bool checkCycles = true)
    {
      DateTime eventUTC = startOfDeviceDay.Value.AddDays(day.Value).AddMilliseconds(milliseconds);
      FactPayloadCycleUtilizationDaily fpcud = (from f in fpcus where f.EventUTC == eventUTC select f).FirstOrDefault();

      Assert.IsNotNull(fpcud, "FactPayloadCycleUtilizationDaily record is missing for day " + day);

      //jcm
      if (checkPayload)
      {
        if (payloadValue.HasValue)
          Assert.AreEqual(payloadValue, fpcud.PayloadMeterDeltaTonne.Value, "Payload value is incorrect for day " + day);
        else
          Assert.IsNull(fpcud.PayloadMeterDeltaTonne, "Payload value is incorrect for day " + day);
      }

      if (checkCycles)
      {
        if (cycleValue.HasValue)
          Assert.AreEqual(cycleValue, fpcud.CycleMeterDeltaCount.Value, "Cycle value is incorrect for day " + day);
        else
          Assert.IsNull(fpcud.CycleMeterDeltaCount, "Cycle value is incorrect for day " + day);
      }
    }

    protected void AssertNH_RPTFactPayloadCycleUtilizationDailyFlags(List<FactPayloadCycleUtilizationDaily> fpcus, long milliseconds, DateTime? startOfDeviceDay = null, int? day = -1,
      DimUtilizationCalloutTypeEnum payloadCalloutFlag = DimUtilizationCalloutTypeEnum.None,
      DimUtilizationCalloutTypeEnum cycleCalloutFlag = DimUtilizationCalloutTypeEnum.None)
    {
      DateTime eventUTC = startOfDeviceDay.Value.AddDays(day.Value).AddMilliseconds(milliseconds);
      FactPayloadCycleUtilizationDaily fpcud = (from f in fpcus where f.EventUTC == eventUTC select f).FirstOrDefault();

      Assert.IsNotNull(fpcud, "FactPayloadCycleUtilizationDaily record is missing for day " + day);
      Assert.AreEqual((int)payloadCalloutFlag, fpcud.PayloadMeterDeltaCalloutTypeID, "PayloadMeterDeltaCalloutFlag for payload should be " + payloadCalloutFlag.ToString() + " for day " + day);
      Assert.AreEqual((int)cycleCalloutFlag, fpcud.CycleMeterDeltaCalloutTypeID, "CycleMeterDeltaCalloutFlag for payload should be " + cycleCalloutFlag.ToString() + " for day " + day);
    }

    protected void AssertNH_RPTFactPayloadCycleUtilizationDaily(List<FactPayloadCycleUtilizationDaily> fpcuds, DateTime? startOfDeviceDay = null, int? day = -1,
      double? payloadMeterTonne = null, double? cycleMeterCount = null, double? payloadMeterDeltaTonne = null, double? cycleMeterDeltaCount = null,
      double? runtimeHours = null, double? workingHours = null, double? totalFuelConsumedGallons = null, double? workingFuelConsumedGallons = null)
    {
      DateTime eventUTC = startOfDeviceDay.Value.AddDays(day.Value);

      FactPayloadCycleUtilizationDaily fpcud = (from f in fpcuds where f.EventUTC == eventUTC select f).FirstOrDefault();
      Assert.IsNotNull(fpcud, "FactPayloadCycleUtilizationDaily record is missing for day " + day);

      Assert.AreEqual(payloadMeterTonne, fpcud.PayloadMeterTonne, "PayloadMeterTonne is incorrect for day " + day);
      Assert.AreEqual(cycleMeterCount, fpcud.CycleMeterCount, "CycleMeterCount is incorrect for day " + day);
      Assert.AreEqual(payloadMeterDeltaTonne, fpcud.PayloadMeterDeltaTonne, "PayloadMeterDeltaTonne is incorrect for day " + day);
      Assert.AreEqual(cycleMeterDeltaCount, fpcud.CycleMeterDeltaCount, "CycleMeterDeltaCount is incorrect for day " + day);
      Assert.AreEqual(runtimeHours, fpcud.RuntimeHours, "RuntimeHours is incorrect for day " + day);
      Assert.AreEqual(workingHours, fpcud.WorkingHours, "WorkingHours is incorrect for day " + day);
      Assert.AreEqual(totalFuelConsumedGallons, fpcud.TotalFuelConsumedGallons, "TotalFuelConsumedGallons is incorrect for day " + day);
      Assert.AreEqual(workingFuelConsumedGallons, fpcud.WorkingFuelConsumedGallons, "WorkingFuelConsumedGallons is incorrect for day " + day);
    }

    protected void AssertNH_RPTFactPayloadCycleUtilizationFlags(List<FactPayloadCycleUtilizationDaily> fpcuds, DateTime? startOfDeviceDay = null, int day = 0, int hour = 0,
      DimUtilizationCalloutTypeEnum payloadMeterDeltaCalloutTypeID = DimUtilizationCalloutTypeEnum.None,
      DimUtilizationCalloutTypeEnum cycleMeterDeltaCalloutTypeID = DimUtilizationCalloutTypeEnum.None,
      DimUtilizationCalloutTypeEnum runtimeHoursCalloutTypeID = DimUtilizationCalloutTypeEnum.None,
      DimUtilizationCalloutTypeEnum workingHoursCalloutTypeID = DimUtilizationCalloutTypeEnum.None,
      DimUtilizationCalloutTypeEnum totalFuelConsumedGallonsCalloutTypeID = DimUtilizationCalloutTypeEnum.None,
      DimUtilizationCalloutTypeEnum workingFuelConsumedGallonsCalloutTypeID = DimUtilizationCalloutTypeEnum.None)
    {
      DateTime eventUTC = startOfDeviceDay.Value.AddDays(day).AddHours(hour);

      FactPayloadCycleUtilizationDaily fpcud = (from f in fpcuds where f.EventUTC == eventUTC select f).FirstOrDefault();
      Assert.IsNotNull(fpcud, "FactPayloadCycleUtilizationDaily record is missing for day " + day);

      Assert.AreEqual((int)payloadMeterDeltaCalloutTypeID, fpcud.PayloadMeterDeltaCalloutTypeID, "PayloadMeterDeltaCalloutTypeID for payload should be " + payloadMeterDeltaCalloutTypeID.ToString() + " for day " + day);
      Assert.AreEqual((int)cycleMeterDeltaCalloutTypeID, fpcud.CycleMeterDeltaCalloutTypeID, "CycleMeterDeltaCalloutTypeID for payload should be " + cycleMeterDeltaCalloutTypeID.ToString() + " for day " + day);
      Assert.AreEqual((int)runtimeHoursCalloutTypeID, fpcud.RuntimeHoursCalloutTypeID, "RuntimeHoursCalloutTypeID for payload should be " + runtimeHoursCalloutTypeID.ToString() + " for day " + day);
      Assert.AreEqual((int)workingHoursCalloutTypeID, fpcud.WorkingHoursCalloutTypeID, "WorkingHoursCalloutTypeID for payload should be " + workingHoursCalloutTypeID.ToString() + " for day " + day);
      Assert.AreEqual((int)totalFuelConsumedGallonsCalloutTypeID, fpcud.TotalFuelConsumedGallonsCalloutTypeID, "TotalFuelConsumedGallonsCalloutTypeID for payload should be " + totalFuelConsumedGallonsCalloutTypeID.ToString() + " for day " + day);
      Assert.AreEqual((int)workingFuelConsumedGallonsCalloutTypeID, fpcud.WorkingFuelConsumedGallonsCalloutTypeID, "WorkingFuelConsumedGallonsCalloutTypeID for payload should be " + workingFuelConsumedGallonsCalloutTypeID.ToString() + " for day " + day);
    }

    #region ETLs

    protected void SyncNhDataToNhReport()
    {
      Helpers.NHRpt.LatestEvents_Populate();
    }

    protected void ExecuteEventTimeStampTransformScript()
    {
      Helpers.ExecuteStoredProcedure(Database.NH_RPT, "uspPub_FactAssetOperationPeriod_Populate_EventTimeStamp");
      Helpers.ExecuteStoredProcedure(Database.NH_RPT, "uspPub_WorkingAssetSiteVisit_Populate");
      Helpers.ExecuteStoredProcedure(Database.NH_RPT, "uspPub_FactAssetUtilizationDaily_Populate_EventTimeStamp");
      Helpers.ExecuteStoredProcedure(Database.NH_RPT, "uspPub_FactAssetSiteUtilization_Populate");   // assembles fact table from WorkingAssetCycleEvent and FactAssetLoadCountPeriod
    }

    protected void ExecuteMeterDeltaTransformScript()
    {
      // to get the AssetOperation facts in FactUtilizationPeriod table, need to run this ETL also. 
      ExecuteStoredProc("uspPub_FactAssetOperationPeriod_Populate_MeterDelta");

      ExecuteStoredProc("uspPub_FactAssetUtilizationDaily_Populate_MeterDelta");
    }

    protected void ExecutePayloadCalloutPopulationScript()
    {
      ExecuteStoredProc("uspPub_FactPayloadCycleUtilizationDaily_Populate");      
    }

    protected void ExecuteFactFaultPopulate()
    {
      Helpers.NHRpt.FactFault_Populate();
    }

    protected static void ExecuteStoredProc(string procName)
    {
      SqlAccessMethods.ExecuteNonQuery(new StoredProcDefinition("NH_RPT", procName));
    }
    #endregion

    #region DataCreation

    protected Asset SetupDefaultAsset(Device device = null, string serialNumberVIN = "ABC123")
    {
      device = device ?? TestData.TestPL321;
      Asset asset = Entity.Asset.SerialNumberVin(serialNumberVIN).WithDevice(device).WithCoreService().WithDefaultAssetUtilizationSettings().SyncWithRpt().Save();
      long assetID = asset.AssetID;
      Entity.AssetWorkingDefinition.ForAsset(assetID)
        .WorkDefinition(WorkDefinitionEnum.MeterDelta)
        .Save();
      Entity.AssetBurnRates.ForAsset(assetID)
        .EstimatedIdleBurnRateGallonsPerHour(5.0)
        .EstimatedWorkingBurnRateGallonsPerHour(10.0).Save();
      Helpers.NHRpt.DimTables_Populate();
      return asset;
    }

    protected DataFluidAnalysis CreateFluidAnalysis(INH_DATA ctx, long assetID, DateTime insertUTC, long SampleNumber, string TextID, DateTime SampleTakenDate, string CompartmentName, string CompartmentID, int dimSourceID, string status, double? MeterValue, string MeterValueUnit, string OverallEvaluation, string Description, DateTime actionUTC)
    {


      DataFluidAnalysis fluid = new DataFluidAnalysis();
      fluid.AssetID = assetID;
      fluid.InsertUTC = insertUTC;
      fluid.SampleNumber = SampleNumber;
      fluid.fk_DimSourceID = dimSourceID;
      fluid.Status = status;
      fluid.TextID = TextID;
      fluid.SampleTakenDate = SampleTakenDate;
      fluid.CompartmentName = CompartmentName;
      fluid.CompartmentID = CompartmentID;
      fluid.MeterValue = MeterValue;
      fluid.MeterValueUnit = MeterValueUnit;
      fluid.OverallEvaluation = OverallEvaluation;
      fluid.Description = Description;
      fluid.ActionUTC = actionUTC;

      ctx.DataFluidAnalysis.AddObject(fluid);
      ctx.SaveChanges();

      return fluid;
    }

    #endregion

    
  }
}
