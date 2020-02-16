using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{
  [TestClass]
  public class A5N210SecondWindowTestsFaults : UnitTestBase
  {
    private Asset testAsset;
    private DimFault cdlDimFault;
    private DimFault j1939DimFault;
    private DateTime cdlEventUTC;
    private DateTime j1939EventUTC;
    private long cdlEventCIDValue;
    private long cdlEventFMIValue;
    private long j1939EventCIDValue;
    private long j1939EventFMIValue;
    private long toleranceSeconds;
    private const string CID = "CID";
    private const string FMI = "FMI";
    private const long SourceMsgID = 2547856;

    private void Initialize(Asset testAssetToUse = null)
    {
      testAsset = testAssetToUse ?? TestData.TestAssetPLE641;

      toleranceSeconds = GetToleranceSeconds();

      cdlDimFault = AddDimFaultDiagNoDescription(DatalinkEnum.CDL, 222, 12345);
      j1939DimFault = AddDimFaultDiagNoDescription(DatalinkEnum.J1939, 666, 23456);

      cdlEventUTC = DateTime.UtcNow.AddDays(-2);
      j1939EventUTC = cdlEventUTC.AddHours(-8.5);

      cdlEventCIDValue = GetDimFaultParameterValue((DatalinkEnum)cdlDimFault.fk_DimDatalinkID, cdlDimFault, CID);
      cdlEventFMIValue = GetDimFaultParameterValue((DatalinkEnum)cdlDimFault.fk_DimDatalinkID, cdlDimFault, FMI);
      j1939EventCIDValue = GetDimFaultParameterValue((DatalinkEnum)j1939DimFault.fk_DimDatalinkID, j1939DimFault, CID);
      j1939EventFMIValue = GetDimFaultParameterValue((DatalinkEnum)j1939DimFault.fk_DimDatalinkID, j1939DimFault, FMI);

      // Add a CDL and J1939 Fault to which hours location should or should not be matched in each test
      Helpers.NHData.DataFaultDiagnostics_Add(testAsset.AssetID, (int)cdlEventCIDValue, (int)cdlEventFMIValue, eventUTC: cdlEventUTC,
        occurrences: 2, level: 2, masterMsgId: SourceMsgID);
      Helpers.NHData.DataFaultDiagnostics_Add(testAsset.AssetID, (int)j1939EventCIDValue, (int)j1939EventFMIValue, eventUTC: j1939EventUTC,
        occurrences: 7, datalink: (int)DatalinkEnum.J1939, level: 1, masterMsgId: SourceMsgID);
    }

    private int GetToleranceSeconds()
    {
      return int.Parse(
        (from config in Ctx.RptContext.DimConfigurationReadOnly
          where config.Name == "EventOffsetSeconds"
          select config.Value).Single());
    }

    [TestMethod]
    [DatabaseTest]
    public void SimultaneousHoursLocationShouldBeMatched()
    {
      Initialize();

      // No offset
      AddHoursLocation(0);

      List<FactFault> exceptionList = ExecuteETL();

      AssertHoursLocationWasMatched(exceptionList);
    }

    [TestMethod]
    [DatabaseTest]
    public void OffsetHoursLocationShouldNotBeMatchedForNonA5N2()
    {
      Initialize(TestData.TestAssetPL321);

      // Offset within the window
      AddHoursLocation(toleranceSeconds / 2.0);

      List<FactFault> exceptionList = ExecuteETL();

      AssertHoursLocationWasNotMatched(exceptionList);
    }

    [TestMethod]
    [DatabaseTest]
    public void HoursLocationBehindButWithinWindowShouldBeMatched()
    {
      Initialize();

      // Offset within the window
      AddHoursLocation(toleranceSeconds / 2.0);

      List<FactFault> exceptionList = ExecuteETL();

      AssertHoursLocationWasMatched(exceptionList);
    }

    [TestMethod]
    [DatabaseTest]
    public void HoursLocationAheadButWithinWindowShouldBeMatched()
    {
      Initialize();

      // Offset within the window
      AddHoursLocation(-toleranceSeconds / 2.0);

      List<FactFault> exceptionList = ExecuteETL();

      AssertHoursLocationWasMatched(exceptionList);
    }

    [TestMethod]
    [DatabaseTest]
    public void HoursLocationBehindButAtWindowBoundaryShouldBeMatched()
    {
      Initialize();

      // Offset at the window boundary
      AddHoursLocation(toleranceSeconds);

      List<FactFault> exceptionList = ExecuteETL();

      AssertHoursLocationWasMatched(exceptionList);
    }

    [TestMethod]
    [DatabaseTest]
    public void HoursLocationAheadButAtWindowBoundaryShouldBeMatched()
    {
      Initialize();

      // Offset at the window boundary
      AddHoursLocation(-toleranceSeconds);

      List<FactFault> exceptionList = ExecuteETL();

      AssertHoursLocationWasMatched(exceptionList);
    }

    [TestMethod]
    [DatabaseTest]
    public void HoursLocationBehindAndOutsideWindowBoundaryShouldNotBeMatched()
    {
      Initialize();

      // Offset outside the window
      AddHoursLocation(toleranceSeconds * 2);

      List<FactFault> exceptionList = ExecuteETL();

      AssertHoursLocationWasNotMatched(exceptionList);
    }

    [TestMethod]
    [DatabaseTest]
    public void HoursLocationAheadAndOutsideWindowBoundaryShouldNotBeMatched()
    {
      Initialize();

      // Offset outside the window
      AddHoursLocation(-toleranceSeconds * 2);

      List<FactFault> exceptionList = ExecuteETL();

      AssertHoursLocationWasNotMatched(exceptionList);
    }

    [TestMethod]
    [DatabaseTest]
    public void WhenTwoHoursLocationArePresentTheClosestOneShouldBeUsed()

    
    {
      Initialize();

      AddHoursLocation(toleranceSeconds / 3.0); // this one is closer
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, eventUtc: cdlEventUTC.AddSeconds(toleranceSeconds / 2.0),
        runtimeHours: 4500, latitude: 50.55, longitude: -98.12, speed: 55, masterMsgId: SourceMsgID);
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, eventUtc: j1939EventUTC.AddSeconds(toleranceSeconds / 2.0),
        runtimeHours: 5500, latitude: 60.55, longitude: -28.12, speed: 65, masterMsgId: SourceMsgID);

      List<FactFault> exceptionList = ExecuteETL();

      AssertHoursLocationWasMatched(exceptionList);
    }

    [TestMethod]
    [DatabaseTest]
    public void WhenFaultProcessingIsBehindItShouldStillMatchUpWithHoursLocation()
    {
      // add hours/location first and process before adding the faults in Initialize()
      testAsset = TestData.TestAssetPLE641;
      cdlEventUTC = DateTime.UtcNow.AddDays(-2);
      j1939EventUTC = cdlEventUTC.AddHours(-8.5);
      AddHoursLocation(GetToleranceSeconds() / 2.0);
      ExecuteETL();

      // now add the faults
      Initialize();

      // now execute again with the faults in place, simulating fault processing running behind
      List<FactFault> exceptionList = ExecuteETL();

      AssertHoursLocationWasMatched(exceptionList);
    }

    #region Privates

    private List<FactFault> ExecuteETL()
    {
      SqlAccessMethods.ExecuteNonQuery(new StoredProcDefinition("NH_RPT", "uspPub_LatestEvents_PopulateA"));
      StoredProcDefinition sp = new StoredProcDefinition("NH_RPT", "uspPub_FactFault_Populate");
      sp.AddInput("@waitForLateEventsUTC", DateTime.UtcNow);
      SqlAccessMethods.ExecuteNonQuery(sp);   

      var exceptionList = (from em in Ctx.RptContext.FactFaultReadOnly
        where em.ifk_DimAssetID == testAsset.AssetID
        orderby em.EventUTC
        select em).ToList();
      return exceptionList;
    }

    private void AssertHoursLocationWasMatched(List<FactFault> exceptionList)
    {
      Assert.AreEqual(2, exceptionList.Count(), "There should be 2 faults in the table");

      // j1939 Fault
      Assert.AreEqual(string.Format("SPN: {0}, FMI: {1}, DL: J1939", j1939EventCIDValue, j1939EventFMIValue),
        exceptionList[0].DefaultDescription, "Description incorrect for J1939 Fault");
      Assert.AreEqual(40.55, exceptionList[0].Latitude);
      Assert.AreEqual(-98.12, exceptionList[0].Longitude);
      Assert.AreEqual(3500, exceptionList[0].RuntimeHoursMeter);

      // CDL Fault
      Assert.AreEqual(string.Format("CID: {0}, FMI: {1}, DL: CDL", cdlEventCIDValue, cdlEventFMIValue),
        exceptionList[1].DefaultDescription, "Description incorrect for CDL Fault");
      Assert.AreEqual(30.55, exceptionList[1].Latitude);
      Assert.AreEqual(-88.12, exceptionList[1].Longitude);
      Assert.AreEqual(2500, exceptionList[1].RuntimeHoursMeter);
    }

    private void AssertHoursLocationWasNotMatched(List<FactFault> exceptionList)
    {
      Assert.AreEqual(2, exceptionList.Count(), "There should be 2 faults in the table");

      // j1939 Fault
      Assert.AreEqual(string.Format("SPN: {0}, FMI: {1}, DL: J1939", j1939EventCIDValue, j1939EventFMIValue),
        exceptionList[0].DefaultDescription, "Description incorrect for J1939 Fault");
      Assert.AreEqual(null, exceptionList[0].Latitude);
      Assert.AreEqual(null, exceptionList[0].Longitude);
      Assert.AreEqual(null, exceptionList[0].RuntimeHoursMeter);

      // CDL Fault
      Assert.AreEqual(string.Format("CID: {0}, FMI: {1}, DL: CDL", cdlEventCIDValue, cdlEventFMIValue),
        exceptionList[1].DefaultDescription, "Description incorrect for CDL Fault");
      Assert.AreEqual(null, exceptionList[1].Latitude);
      Assert.AreEqual(null, exceptionList[1].Longitude);
      Assert.AreEqual(null, exceptionList[1].RuntimeHoursMeter);
    }

    private void AddHoursLocation(double offsetSeconds)
    {
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, eventUtc: cdlEventUTC.AddSeconds(offsetSeconds),
        runtimeHours: 2500,
        latitude: 30.55, longitude: -88.12, speed: 35, masterMsgId: SourceMsgID);
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, eventUtc: j1939EventUTC.AddSeconds(offsetSeconds),
        runtimeHours: 3500,
        latitude: 40.55, longitude: -98.12, speed: 45, masterMsgId: SourceMsgID);
    }

    private DimFault AddDimFaultDiagNoDescription(DatalinkEnum dl, int fmi, int cid)
    {
      DimFault dimFault = new DimFault { fk_DimDatalinkID = (int)dl, fk_DimFaultTypeID = (int)FaultTypeEnum.Diagnostic, UpdateUTC = DateTime.UtcNow.AddDays(-5) };
      string cidOrSpn = "CID: ";

      if (dl == DatalinkEnum.J1939)
        cidOrSpn = "SPN: ";

      dimFault.CodedDescription = string.Format("{0}{1}, FMI:{2} ", cidOrSpn, cid, fmi);
      Ctx.RptContext.DimFault.AddObject(dimFault);
      Ctx.RptContext.SaveChanges();

      DimFaultParameter cidDimFaultParameter = new DimFaultParameter();
      cidDimFaultParameter.fk_DimFaultParameterTypeID = GetDimFaultParameterType(dl, CID).ID;
      cidDimFaultParameter.Value = cid;
      cidDimFaultParameter.fk_DimFaultID = dimFault.ID;

      DimFaultParameter fmiDimFaultParameter = new DimFaultParameter();
      fmiDimFaultParameter.fk_DimFaultParameterTypeID = GetDimFaultParameterType(dl, FMI).ID;
      fmiDimFaultParameter.Value = fmi;
      fmiDimFaultParameter.fk_DimFaultID = dimFault.ID;

      Ctx.RptContext.DimFaultParameter.AddObject(cidDimFaultParameter);
      Ctx.RptContext.DimFaultParameter.AddObject(fmiDimFaultParameter);
      Ctx.RptContext.SaveChanges();

      return dimFault;
    }

    private DimFaultParameterType GetDimFaultParameterType(DatalinkEnum dl, string description)
    {
      if (dl == DatalinkEnum.J1939 && description == CID)
        description = "SPN";
      return (from d in Ctx.RptContext.DimFaultParameterTypeReadOnly
              where d.Description == description &&
                    d.fk_DimDatalinkID == (int)dl
              select d).FirstOrDefault();
    }

    private long GetDimFaultParameterValue(DatalinkEnum dl, DimFault dimFault, string description)
    {
      long dimFaultParameterTypeID = GetDimFaultParameterType(dl, description).ID;
      long faultParameterValue =
        (from dfp in Ctx.RptContext.DimFaultParameter
          where dfp.fk_DimFaultParameterTypeID == dimFaultParameterTypeID && dfp.fk_DimFaultID == dimFault.ID
          select dfp.Value).Single() ?? -1;
      return faultParameterValue;
    }

    #endregion

  }
}
