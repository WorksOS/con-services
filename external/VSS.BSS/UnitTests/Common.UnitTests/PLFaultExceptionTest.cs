using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
  [TestClass]
  public class PLFaultExceptionTest : UnitTestBase
  {
    private const string CID = "CID";
    private const string FMI = "FMI";
    private const string EID = "EID";

    /// <summary>
    /// 2 occurences of faultA within 8 hours, then one after the 8 hours
    /// 1 different faultB within 8 hours of the first of faultA
    /// should result All faults showing in FactFault table
    /// additionally tests the presence of a PLStatusMessage (for RT) is optional
    /// test that a severity level of 0 (unknown) is stored in FactFault table as 2 (medium)
    /// </summary>
    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void FaultExceptionMultipleFaults()
    {
      Asset testAsset = TestData.TestAssetPL321;

      DimFault e1 = AddDimFaultWithDescription(DatalinkEnum.CDL, 3611, "Description 1 the very first");
      DimFault e2 = AddDimFaultWithDescription(DatalinkEnum.CDL, 3622, "Description 2 the second one");

      long sourceMsgID = 2547856;
      DateTime eventUTC1 = DateTime.UtcNow.AddDays(-2);        // new DateTime(2009, 11, 4, 22, 30, 00);
      DateTime eventUTC2 = eventUTC1.AddDays(1).AddHours(-20); // new DateTime(2009, 11, 5, 02, 00, 00);
      DateTime eventUTC3 = eventUTC1.AddDays(1).AddHours(-12); // new DateTime(2009, 11, 5, 10, 30, 00);
      DateTime eventUTC4 = eventUTC1.AddHours(3);              // new DateTime(2009, 11, 5, 01, 00, 00);

      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, eventUtc: DateTime.UtcNow.AddDays(-1), runtimeHours: 2500, latitude: 30.55, longitude: -88.12, speed: 35, masterMsgId: sourceMsgID);

      int dimFaultParamterValue1 = GetDimFaultParameterValue((DatalinkEnum)e1.fk_DimDatalinkID, e1, EID);
      int dimFaultParamterValue2 = GetDimFaultParameterValue((DatalinkEnum)e2.fk_DimDatalinkID, e2, EID);

      Helpers.NHData.DataFaultEvent_Add(testAsset.AssetID, dimFaultParamterValue1, eventUTC: eventUTC1, occurrences: 2, level: 0, masterMsgId: sourceMsgID);
      Helpers.NHData.DataFaultEvent_Add(testAsset.AssetID, dimFaultParamterValue1, eventUTC: eventUTC2, occurrences: 2, level: 2, masterMsgId: sourceMsgID);
      Helpers.NHData.DataFaultEvent_Add(testAsset.AssetID, dimFaultParamterValue1, eventUTC: eventUTC3, occurrences: 2, level: 2, masterMsgId: sourceMsgID);
      Helpers.NHData.DataFaultEvent_Add(testAsset.AssetID, dimFaultParamterValue2, eventUTC: eventUTC4, occurrences: 7, datalinkID: (int)DatalinkEnum.CDL, level: 1, masterMsgId: 3652487);

      ExecFactFaultPopulate();

      var exceptionList = (from em in Ctx.RptContext.FactFaultReadOnly
                           where em.ifk_DimAssetID == testAsset.AssetID
                           orderby em.EventUTC
                           select new { FaultType = em.ifk_DimFaultTypeID, em.EventUTC, em.DefaultDescription, em.ifk_DimSeverityLevelID }).ToList();

      Assert.AreEqual(4, exceptionList.Count(), "There should be 4 faults in the table");
      Assert.AreEqual((int)FaultTypeEnum.Event, exceptionList[0].FaultType, "Unexpected exception type");
      Assert.AreEqual(eventUTC1, exceptionList[0].EventUTC, "Event time incorrect for Fault 0");
 //     Assert.AreEqual(e1.DimFaultDescription.Single().Description, exceptionList[0].DefaultDescription, "Description incorrect for Fault 0");
      Assert.AreEqual(2, exceptionList[0].ifk_DimSeverityLevelID, "SeverityLevel should have changed for Fault 0");
      Assert.AreEqual(eventUTC4, exceptionList[1].EventUTC, "Event time incorrect for Fault 1");
 //     Assert.AreEqual(e2.DimFaultDescription.Single().Description, exceptionList[1].DefaultDescription, "Description incorrect for Fault 1");
      Assert.AreEqual(1, exceptionList[1].ifk_DimSeverityLevelID, "SeverityLevel should remain at 1 Fault 1");
      Assert.AreEqual(eventUTC2, exceptionList[2].EventUTC, "Event time incorrect for Fault 2");
      Assert.AreEqual(eventUTC3, exceptionList[3].EventUTC, "Event time incorrect for Fault 3");

      ExecFactFaultPopulate();

      var count = (from em in Ctx.RptContext.FactFaultReadOnly
                   where em.ifk_DimAssetID == testAsset.AssetID
                   orderby em.EventUTC
                   select new { FaultType = em.ifk_DimFaultTypeID, em.EventUTC, em.DefaultDescription }).Count();

      Assert.AreEqual(4, count, "There should STILL be 4 faults in the table");
    }

    // 2 occurances of diagnosticA within 8 hours, then one after the 8 hours
    // 1 different diagnosticB within 8 hours of the first of diagnosticA
    // should result in all diagnostics showing in FactFault table
    // additionally tests that presence of a PLStatusMessage (for RT) is optional
    // additionally tests: diagnosticsIDs are unique only within componentID and DatalinkIDs
    //   i.e. the same DiagnosticID can occur for component A or B on Datalink 1 or 2, and should have a unique description
    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void FaultExceptionMultipleDiagnostics()
    {
      Asset testAsset = TestData.TestAssetPL321;

      int FMI = 3999;
      int CID1 = 98765;
      string MID1 = "1500";
      int CID2 = 98764;
      string MID2 = "1600";

      AddDimFaultDiagWithDescription(DatalinkEnum.CDL, FMI, CID1, "Description 1 the very first");
      AddDimFaultDiagWithDescription(DatalinkEnum.J1939, FMI, CID2, "Description 2 the second one");

      DateTime eventUTC1 = DateTime.UtcNow.AddDays(-2);        //new DateTime(2009, 11, 4, 22, 30, 00);
      DateTime eventUTC2 = eventUTC1.AddDays(1).AddHours(-20); //new DateTime(2009, 11, 5, 02, 00, 00);
      DateTime eventUTC3 = eventUTC1.AddDays(1).AddHours(-12); //new DateTime(2009, 11, 5, 10, 30, 00);
      DateTime eventUTC4 = eventUTC1.AddHours(-21);            //new DateTime(2009, 11, 4, 01, 00, 00);

      Helpers.NHData.DataFaultDiagnostics_Add(testAsset.AssetID, CID1, FMI, mID:MID1, eventUTC: eventUTC1, occurrences: 2, level: 2);
      Helpers.NHData.DataFaultDiagnostics_Add(testAsset.AssetID, CID1, FMI, mID: MID1, eventUTC: eventUTC2, occurrences: 2, level: 2);
      Helpers.NHData.DataFaultDiagnostics_Add(testAsset.AssetID, CID1, FMI, mID: MID1, eventUTC: eventUTC3, occurrences: 2, level: 2);
      Helpers.NHData.DataFaultDiagnostics_Add(testAsset.AssetID, CID2, FMI, mID: MID2, eventUTC: eventUTC4, occurrences: 7, level: 3, datalink: (int)DatalinkEnum.J1939);

      ExecFactFaultPopulate();

      var exceptionList = (from em in Ctx.RptContext.FactFaultReadOnly
                           where em.ifk_DimAssetID == testAsset.AssetID
                           orderby em.EventUTC
                           select new { FaultType = em.ifk_DimFaultTypeID, em.EventUTC, em.DefaultDescription }).ToList();

      Assert.AreEqual(4, exceptionList.Count(), "There should be 4 diagnostics in the table");
      Assert.AreEqual((int)FaultTypeEnum.Diagnostic, exceptionList[0].FaultType, "Unexpected exception type");
      Assert.AreEqual(eventUTC4, exceptionList[0].EventUTC, "Event time incorrect for Fault 0");
 //     Assert.AreEqual(e2.DimFaultDescription.First().Description, exceptionList[0].DefaultDescription, "Description incorrect for Fault 0");
      Assert.AreEqual(eventUTC1, exceptionList[1].EventUTC, "Event time incorrect for Fault 1");
 //     Assert.AreEqual(e1.DimFaultDescription.First().Description, exceptionList[1].DefaultDescription, "Description incorrect for Fault 1");
      Assert.AreEqual(eventUTC2, exceptionList[2].EventUTC, "Event time incorrect for Fault 2");
      Assert.AreEqual(eventUTC3, exceptionList[3].EventUTC, "Event time incorrect for Fault 3");
    }

    // No Description for DimFault-Event. Validate EM record created with DefaultDescription.
    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void FaultExceptionTestDefaultDesc()
    {
      Asset testAsset = TestData.TestAssetPL321;

      DimFault e1 = AddDimFaultNoDescription(DatalinkEnum.CDL, 3611);
      DimFault e2 = AddDimFaultNoDescription(DatalinkEnum.CDL, 3622);

      long sourceMsgID = 2547856;
      DateTime eventUTC3 = DateTime.UtcNow.AddDays(-2);        //new DateTime(2009, 11, 5, 10, 30, 00);
      DateTime eventUTC4 = eventUTC3.AddHours(-8.5);           // new DateTime(2009, 11, 5, 01, 00, 00);

      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, eventUtc: DateTime.UtcNow.AddDays(-1), runtimeHours: 2500, latitude: 30.55, longitude: -88.12, speed: 35, masterMsgId: sourceMsgID);

      int dimFaultParamterValue1 = GetDimFaultParameterValue((DatalinkEnum)e1.fk_DimDatalinkID, e1, EID);
      int dimFaultParamterValue2 = GetDimFaultParameterValue((DatalinkEnum)e2.fk_DimDatalinkID, e2, EID);

      Helpers.NHData.DataFaultEvent_Add(testAsset.AssetID, dimFaultParamterValue1, eventUTC: eventUTC3, occurrences: 2, level: 2, masterMsgId: sourceMsgID);
      Helpers.NHData.DataFaultEvent_Add(testAsset.AssetID, dimFaultParamterValue2, eventUTC: eventUTC4, occurrences: 7, datalinkID: (int)DatalinkEnum.J1939, level: 1, masterMsgId: 3652487);

      ExecFactFaultPopulate();

      var exceptionList = (from em in Ctx.RptContext.FactFaultReadOnly
                           where em.ifk_DimAssetID == testAsset.AssetID
                           orderby em.EventUTC
                           select new { FaultType = em.ifk_DimFaultTypeID, em.EventUTC, em.DefaultDescription, em.ifk_DimSeverityLevelID }).ToList();

      Assert.AreEqual(2, exceptionList.Count(), "There should be 2 faults in the table");
      Assert.AreEqual(string.Format("EID: {0}, DL: J1939", dimFaultParamterValue2), exceptionList[0].DefaultDescription, "Description incorrect for J1939 Fault");
      Assert.AreEqual(string.Format("EID: {0}, DL: CDL", dimFaultParamterValue1), exceptionList[1].DefaultDescription, "Description incorrect for CDL Fault");
    }

    // No Description for DimFault-Diagnostic. Validate EM record created with DefaultDescription.
    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void FaultExceptionTestDefaultDiagDesc()
    {
      Asset testAsset = TestData.TestAssetPL321;

      DimFault e1 = AddDimFaultDiagNoDescription(DatalinkEnum.CDL, 222, 12345);
      DimFault e2 = AddDimFaultDiagNoDescription(DatalinkEnum.J1939, 666, 23456);

      long sourceMsgID = 2547856;
      DateTime eventUTC3 = DateTime.UtcNow.AddDays(-2);        //new DateTime(2009, 11, 5, 10, 30, 00);
      DateTime eventUTC4 = eventUTC3.AddHours(-8.5);           //new DateTime(2009, 11, 5, 01, 00, 00);

      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, eventUtc: DateTime.UtcNow.AddDays(-1), runtimeHours: 2500, latitude: 30.55, longitude: -88.12, speed: 35, masterMsgId: sourceMsgID);

      int e1CIDValue = GetDimFaultParameterValue((DatalinkEnum)e1.fk_DimDatalinkID, e1, CID);
      int e1FMIValue = GetDimFaultParameterValue((DatalinkEnum)e1.fk_DimDatalinkID, e1, FMI);
      int e2CIDValue = GetDimFaultParameterValue((DatalinkEnum)e2.fk_DimDatalinkID, e2, CID);
      int e2FMIValue = GetDimFaultParameterValue((DatalinkEnum)e2.fk_DimDatalinkID, e2, FMI);

      Helpers.NHData.DataFaultDiagnostics_Add(testAsset.AssetID, e1CIDValue, e1FMIValue, eventUTC: eventUTC3, occurrences: 2, level: 2, masterMsgId: sourceMsgID);
      Helpers.NHData.DataFaultDiagnostics_Add(testAsset.AssetID, e2CIDValue, e2FMIValue, eventUTC: eventUTC4, occurrences: 7, datalink: (int)DatalinkEnum.J1939, level: 1, masterMsgId: 3652487);

      ExecFactFaultPopulate();

      var exceptionList = (from em in Ctx.RptContext.FactFaultReadOnly
                           where em.ifk_DimAssetID == testAsset.AssetID
                           orderby em.EventUTC
                           select new { FaultType = em.ifk_DimFaultTypeID, em.EventUTC, em.DefaultDescription, em.ifk_DimSeverityLevelID }).ToList();

      Assert.AreEqual(2, exceptionList.Count(), "There should be 2 faults in the table");
      Assert.AreEqual(string.Format("SPN: {0}, FMI: {1}, DL: J1939", e2CIDValue, e2FMIValue), exceptionList[0].DefaultDescription, "Description incorrect for J1939 Fault");
      Assert.AreEqual(string.Format("CID: {0}, FMI: {1}, DL: CDL", e1CIDValue, e1FMIValue), exceptionList[1].DefaultDescription, "Description incorrect for CDL Fault");
    }

    // No Description for user's language - defaultDescription still provided.
    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void NoDescriptionInUsersLanguage()
    {
      Asset testAsset = TestData.TestAssetPL321;

      DimFault e1 = AddDimFaultDiagWithDescription(DatalinkEnum.CDL, 222, 12345, "Hunky derky werky strurky", LanguageEnum.daDK);

      long sourceMsgID = 2547856;
      DateTime eventUTC = DateTime.UtcNow.AddDays(-2);        //new DateTime(2009, 11, 5, 10, 30, 00);

      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, eventUtc: DateTime.UtcNow.AddDays(-1), runtimeHours: 2500, latitude: 30.55, longitude: -88.12, speed: 35, masterMsgId: sourceMsgID);

      int e1CIDValue = GetDimFaultParameterValue((DatalinkEnum)e1.fk_DimDatalinkID, e1, CID);
      int e1FMIValue = GetDimFaultParameterValue((DatalinkEnum)e1.fk_DimDatalinkID, e1, FMI);

      Helpers.NHData.DataFaultDiagnostics_Add(testAsset.AssetID, e1CIDValue, e1FMIValue, eventUTC: eventUTC, occurrences: 2, level: 2, masterMsgId: sourceMsgID);

      ExecFactFaultPopulate();

      //var exceptionList = (from em in Ctx.RptContext.FactFaultReadOnly
      //                     join fault in Ctx.RptContext.DimFaultReadOnly on em.ifk_DimFaultID equals fault.ID
      //                     let langDescription = (from fd in fault.DimFaultDescription where fd.DimLanguage.ISOName == "en-US" select fd.Description).FirstOrDefault()
      //                     where em.ifk_DimAssetID == testAsset.AssetID
      //                     orderby em.EventUTC
      //                     select new { FaultType = em.ifk_DimFaultTypeID, em.EventUTC, em.DefaultDescription, em.ifk_DimSeverityLevelID, enUSDescription=langDescription }).ToList();

  //    Assert.AreEqual(1, exceptionList.Count(), "There should be 1 fault in the table");
  //    Assert.AreEqual(string.Format("CID: {0}, FMI: {1}, DL: CDL", e1CIDValue, e1FMIValue), exceptionList[0].DefaultDescription, "Description incorrect for CDL Fault");
 //     Assert.IsNull(exceptionList[0].enUSDescription, "Description incorrect for en-US");
    }

    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void RemappedSeverityAndEID()
    {
      // There is an ECM board bug which produces incorrect EID/SeverityLevel codes.
      // Map the EID and SeverityLevel using the DimSeverityLevelIDMap table.
      //           EID 2, SeverityLevel 0 is mapped to 2/3
      //           EID 168 SeverityLevel 1 is mapped to 43/1

      Asset testAsset = TestData.TestAssetPL321;

      DimFault e1 = AddDimFaultWithDescription(DatalinkEnum.CDL, 2, "Description 1 the very first");
      DimFault e2 = AddDimFaultWithDescription(DatalinkEnum.CDL, 168, "Description 2 the second one");

      long sourceMsgID = 2547856;
      DateTime eventUTC1 = DateTime.UtcNow.AddDays(-2);        //new DateTime(2009, 11, 4, 22, 30, 00);
      DateTime eventUTC2 = eventUTC1.AddDays(1);               //new DateTime(2009, 11, 5, 02, 00, 00);

      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, eventUtc: DateTime.UtcNow.AddDays(-1), runtimeHours: 2500, latitude: 30.55, longitude: -88.12, speed: 35, masterMsgId: sourceMsgID);

      int e1EIDValue = GetDimFaultParameterValue((DatalinkEnum)e1.fk_DimDatalinkID, e1, EID);
      int e2EIDValue = GetDimFaultParameterValue((DatalinkEnum)e2.fk_DimDatalinkID, e2, EID);

      Helpers.NHData.DataFaultEvent_Add(testAsset.AssetID, e1EIDValue, eventUTC: eventUTC1, occurrences: 1, level: 0, masterMsgId: sourceMsgID);
      Helpers.NHData.DataFaultEvent_Add(testAsset.AssetID, e2EIDValue, eventUTC: eventUTC2, occurrences: 1, level: 1, masterMsgId: sourceMsgID);

      ExecFactFaultPopulate();

      var exceptionList = (from em in Ctx.RptContext.FactFaultReadOnly
                           where em.ifk_DimAssetID == testAsset.AssetID
                           orderby em.EventUTC
                           select new { em.EID, em.EIDOrig, FaultType = em.ifk_DimFaultTypeID, em.EventUTC, em.DefaultDescription, em.ifk_DimSeverityLevelID }).ToList();

      Assert.AreEqual(2, exceptionList.Count(), "There should be 4 faults in the table");
      Assert.AreEqual(3, exceptionList[0].ifk_DimSeverityLevelID, "SeverityLevel should have changed for Fault 0");
      Assert.AreEqual(2, exceptionList[0].EID.Value, "EID should not have changed for Fault 0");
      Assert.AreEqual(exceptionList[0].EIDOrig.Value, exceptionList[0].EID.Value, "EID should be same as original for F0");
      Assert.AreEqual(1, exceptionList[1].ifk_DimSeverityLevelID, "SeverityLevel should have changed for Fault 1");
      Assert.AreEqual(43, exceptionList[1].EID.Value, "EID should have mapped for Fault 1");
      Assert.AreNotEqual(exceptionList[1].EIDOrig.Value, exceptionList[1].EID.Value, "EID should not be same as original for F1");
    }

    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void InvalidLocationInvalidRT()
    {
      Asset testAsset = TestData.TestAssetPL321;

      DimFault e1 = AddDimFaultWithDescription(DatalinkEnum.CDL, 2, "Description 1 the very first");

      long sourceMsgID = 2547856;
      DateTime eventUTC1 = DateTime.UtcNow.AddDays(-2);        //new DateTime(2009, 11, 4, 22, 30, 00);

      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, eventUtc: DateTime.UtcNow.AddDays(-1), runtimeHours: null, latitude: null, longitude: null, speed: 35, masterMsgId: sourceMsgID);

      int e1EIDValue = GetDimFaultParameterValue((DatalinkEnum)e1.fk_DimDatalinkID, e1, EID);

      Helpers.NHData.DataFaultEvent_Add(testAsset.AssetID, e1EIDValue, eventUTC: eventUTC1, occurrences: 1, level: 0, masterMsgId: sourceMsgID);

      ExecFactFaultPopulate();

      var exceptionList = (from em in Ctx.RptContext.FactFaultReadOnly
                           where em.ifk_DimAssetID == testAsset.AssetID
                           orderby em.EventUTC
                           select em).ToList();

      Assert.AreEqual(1, exceptionList.Count(), "Wrong count");
      Assert.IsNull(exceptionList[0].Latitude, "Should get the EM record, but with a NULL Lat.");
      Assert.IsNull(exceptionList[0].Longitude, "Should get the EM record, but with a NULL Lon.");
      Assert.IsNull(exceptionList[0].RuntimeHoursMeter, "Should get the EM record, but with a NULL RT.");
    }

    [TestMethod]
    [DatabaseTest]
    [Ignore]
    public void ValidLocationValidRT()
    {
      Asset testAsset = TestData.TestAssetPL321;

      DimFault e1 = AddDimFaultWithDescription(DatalinkEnum.CDL, 2, "Description 1 the very first");

      long sourceMsgID = 2547856;
      DateTime eventUTC1 = DateTime.UtcNow.AddDays(-2);        //new DateTime(2009, 11, 4, 22, 30, 00);

      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, eventUtc: eventUTC1, runtimeHours: 123, latitude: 12, longitude: 124, speed: 35, masterMsgId: sourceMsgID);

      int e1EIDValue = GetDimFaultParameterValue((DatalinkEnum)e1.fk_DimDatalinkID, e1, EID);

      Helpers.NHData.DataFaultEvent_Add(testAsset.AssetID, e1EIDValue, eventUTC: eventUTC1, occurrences: 1, level: 0, masterMsgId: sourceMsgID);

      ExecFactFaultPopulate();

      var exceptionList = (from em in Ctx.RptContext.FactFaultReadOnly
                           where em.ifk_DimAssetID == testAsset.AssetID
                           orderby em.EventUTC
                           select em).ToList();

      Assert.AreEqual(1, exceptionList.Count(), "Wrong count");
      Assert.AreEqual(12, exceptionList[0].Latitude.Value, "Should get the EM record, with lat from hours/location at same time");
      Assert.AreEqual(124, exceptionList[0].Longitude.Value, "Should get the EM record, with lon from hours/location at same time");
      Assert.AreEqual(123, exceptionList[0].RuntimeHoursMeter.Value, "Should get the EM record, with RT from hours/location at same time.");
    }

    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void OldLocation()
    {
      Asset testAsset = TestData.TestAssetPL321;

      DimFault e1 = AddDimFaultWithDescription(DatalinkEnum.CDL, 2, "Description 1 the very first");

      long sourceMsgID = 2547856;
      DateTime eventUTC1 = DateTime.UtcNow.AddDays(-2);        //new DateTime(2009, 11, 4, 22, 30, 00);

      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, eventUtc: eventUTC1.AddMinutes(-5), runtimeHours: 123, latitude: 12, longitude: 124, speed: 35, masterMsgId: sourceMsgID);

      int e1EIDValue = GetDimFaultParameterValue((DatalinkEnum)e1.fk_DimDatalinkID, e1, EID);

      Helpers.NHData.DataFaultEvent_Add(testAsset.AssetID, e1EIDValue, eventUTC: eventUTC1, occurrences: 1, level: 0, masterMsgId: sourceMsgID);

      ExecFactFaultPopulate();

      var exceptionList = (from em in Ctx.RptContext.FactFaultReadOnly
                           where em.ifk_DimAssetID == testAsset.AssetID
                           orderby em.EventUTC
                           select em).ToList();

      Assert.AreEqual(1, exceptionList.Count(), "Wrong count");
      Assert.IsNull(exceptionList[0].Latitude, "Should get the EM record, but with a NULL Lat.");
      Assert.IsNull(exceptionList[0].Longitude, "Should get the EM record, but with a NULL Lon.");
      Assert.IsNull(exceptionList[0].RuntimeHoursMeter, "Should get the EM record, but with a NULL RT.");
    }

    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void NeverHadLocation()
    {
      Asset testAsset = TestData.TestAssetPL321;

      DimFault e1 = AddDimFaultWithDescription(DatalinkEnum.CDL, 2, "Description 1 the very first");

      long sourceMsgID = 2547856;
      DateTime eventUTC1 = DateTime.UtcNow.AddDays(-1);

      int e1EIDValue = GetDimFaultParameterValue((DatalinkEnum)e1.fk_DimDatalinkID, e1, EID);

      Helpers.NHData.DataFaultEvent_Add(testAsset.AssetID, e1EIDValue, eventUTC: eventUTC1, occurrences: 1, level: 0, masterMsgId: sourceMsgID);

      ExecFactFaultPopulate();

      var exceptionList = (from em in Ctx.RptContext.FactFaultReadOnly
                           where em.ifk_DimAssetID == testAsset.AssetID
                           orderby em.EventUTC
                           select em).ToList();

      Assert.AreEqual(1, exceptionList.Count(), "Wrong count");
      Assert.IsNull(exceptionList[0].Latitude, "Should get the EM record, but with a NULL Lat.");
      Assert.IsNull(exceptionList[0].Longitude, "Should get the EM record, but with a NULL Lon.");
    }


    #region Privates
    protected void ExecFactFaultPopulate()
    {
      string sprocName = "uspPub_FactFault_Populate";
      StoredProcDefinition storedProc = new StoredProcDefinition("NH_RPT", sprocName);

      SqlAccessMethods.ExecuteNonQuery(storedProc);
    }

    private DimFault AddDimFaultWithDescription(DatalinkEnum dl, int eid, string description, LanguageEnum language = LanguageEnum.enUS)
    {
      DimFault dimFault = new DimFault {fk_DimDatalinkID = (int)dl, fk_DimFaultTypeID = (int)FaultTypeEnum.Event, UpdateUTC = DateTime.UtcNow.AddDays(-5) };
      dimFault.CodedDescription = description;
      Ctx.RptContext.DimFault.AddObject(dimFault);
      Ctx.RptContext.SaveChanges();

      DimFaultParameter dimFaultParameter = new DimFaultParameter();
      dimFaultParameter.fk_DimFaultParameterTypeID = GetDimFaultParameterType(dl, EID).ID;
      dimFaultParameter.Value = eid;
      dimFaultParameter.fk_DimFaultID = dimFault.ID;
      Ctx.RptContext.DimFaultParameter.AddObject(dimFaultParameter);
      Ctx.RptContext.SaveChanges();

      DimFaultDescription dimFaultDescription = new DimFaultDescription { Description = description, fk_DimLanguageID = (int)language, fk_DimFaultID = dimFault.ID, UpdateUTC = DateTime.UtcNow.AddDays(-5) };
      Ctx.RptContext.DimFaultDescription.AddObject(dimFaultDescription);
      Ctx.RptContext.SaveChanges();
      
      return dimFault;
    }

    private DimFault AddDimFaultDiagWithDescription(DatalinkEnum dl, int fmi, int cid, string description, LanguageEnum language = LanguageEnum.enUS)
    {

      DimFault dimFault = new DimFault { fk_DimDatalinkID = (int)dl, fk_DimFaultTypeID = (int)FaultTypeEnum.Diagnostic, UpdateUTC = DateTime.UtcNow.AddDays(-5) };
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

      DimFaultDescription dfd = new DimFaultDescription ();
      dfd.Description = description;
      dfd.fk_DimLanguageID = (int)language;
      dfd.UpdateUTC = DateTime.UtcNow.AddDays(-5); 
      dfd.fk_DimFaultID = dimFault.ID;
      Ctx.RptContext.DimFaultDescription.AddObject(dfd);
      Ctx.RptContext.SaveChanges();

      return dimFault;
    }

    private DimFault AddDimFaultNoDescription(DatalinkEnum dl, int eid)
    {
      DimFault dimFault = new DimFault { fk_DimDatalinkID = (int)dl, fk_DimFaultTypeID = (int)FaultTypeEnum.Event, UpdateUTC = DateTime.UtcNow.AddDays(-5) };
      dimFault.CodedDescription = string.Format("{0}_{1}", eid, dl.ToString());
      Ctx.RptContext.DimFault.AddObject(dimFault);
      Ctx.RptContext.SaveChanges();

      DimFaultParameter dimFaultParameter = new DimFaultParameter();
      dimFaultParameter.fk_DimFaultParameterTypeID = GetDimFaultParameterType(dl, EID).ID;
      dimFaultParameter.Value = eid;
      dimFaultParameter.fk_DimFaultID = dimFault.ID;
      Ctx.RptContext.DimFaultParameter.AddObject(dimFaultParameter);
      Ctx.RptContext.SaveChanges();

      return dimFault;
    }

    private DimFault AddDimFaultDiagNoDescription(DatalinkEnum dl, int fmi, int cid)
    {
      DimFault dimFault = new DimFault { fk_DimDatalinkID = (int)dl, fk_DimFaultTypeID = (int)FaultTypeEnum.Diagnostic, UpdateUTC = DateTime.UtcNow.AddDays(-5) };
      string cidOrSpn = "CID: ";

      if (dl == DatalinkEnum.J1939)
        cidOrSpn = "SPN: ";

      dimFault.CodedDescription = string.Format("{0}{1}, FMI:{2} ", cidOrSpn, cid, fmi, dl.ToString());
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

    private int GetDimFaultParameterValue(DatalinkEnum dl, DimFault dimFault, string description)
    {
      long dimFaultParameterTypeID = GetDimFaultParameterType(dl, description).ID;
      int faultParameterValue = (int)(from dfp in Ctx.RptContext.DimFaultParameter 
                                 where dfp.fk_DimFaultParameterTypeID == dimFaultParameterTypeID
                                   && dfp.fk_DimFaultID == dimFault.ID
                                 select dfp.Value).FirstOrDefault().Value;
      return faultParameterValue;
    }

    #endregion
  }
}
