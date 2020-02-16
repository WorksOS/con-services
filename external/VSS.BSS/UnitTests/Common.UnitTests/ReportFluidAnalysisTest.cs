using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{
  [TestClass]
  public class ReportFluidAnalysisTest : UnitTestBase
  { 
    [TestMethod]
    [DatabaseTest]
    public void ReportFluidAnalysis_HappyPathTest()
    {

      Asset pl321 = TestData.TestAssetPL321;

      Helpers.NHRpt.DimAsset_Populate();

      DateTime t0 = DateTime.UtcNow.Date;
      
      DataFluidAnalysis fluid1 = CreateFluidAnalysis(Ctx.DataContext, pl321.AssetID, DateTime.UtcNow, 123, "145_rt",
        t0, "Engine SideBar", "EGR_ER", 1, "N", 145, "h", "AA", "That green stuff in plastic tank", DateTime.UtcNow);

      ExecuteFluidAnalysisScript();

      List<FluidAnalysis> Samples = (from sample in Ctx.RptContext.FluidAnalysisReadOnly
                                     where sample.ifk_DimAssetID == pl321.AssetID
                                     orderby sample.SampleNumber
                                     select sample).ToList<FluidAnalysis>();
      Assert.IsNotNull(Samples, "Failed to create FluidAnalysis Object");

      Assert.IsTrue(Samples.Count() == 1, "Should be 1 FluidAnalysis record.");
    
      Assert.AreEqual(123, Samples[0].SampleNumber, string.Format("SampleNumber incorrect"));
      Assert.AreEqual(t0.KeyDate(), Samples[0].fk_AssetKeyDate, string.Format("AssetKeyDate incorrect for Sample#:{0}", Samples[0].SampleNumber));
      Assert.AreEqual("Engine SideBar", Samples[0].CompartmentName, string.Format("CompartmentName incorrect for Sample#:{0}", Samples[0].SampleNumber));
      Assert.AreEqual("EGR_ER", Samples[0].CompartmentID, string.Format("CompartmentID incorrect for Sample#:{0}", Samples[0].SampleNumber));
      Assert.IsTrue(Samples[0].MeterValue == fluid1.MeterValue, string.Format("MeterValue incorrect for Sample#:{0}", Samples[0].SampleNumber));
      Assert.IsTrue(Samples[0].OverallEvaluation == fluid1.OverallEvaluation, string.Format("OverallEvaluation incorrect for Sample#:{0}", Samples[0].SampleNumber));
      Assert.IsTrue(Samples[0].Status == fluid1.Status, string.Format("Status incorrect for Sample#:{0}", Samples[0].SampleNumber));
    }
       
    [TestMethod]
    [DatabaseTest]
    public void ReportFluidAnalysis_ChangeEval()
    {
        // due to the unit tests not committing the original nh_data..DataFluidAnalysis,
        //   this unit test doesn't actually UPDATE the 1 row, but creates another one. 
        //   Yuk it happens to work anyway, but not the way it should in reality.
        Asset pl321 = TestData.TestAssetPL321;

        Helpers.NHRpt.DimAsset_Populate();

        DateTime t0 = DateTime.UtcNow.Date;

        DataFluidAnalysis fluid1 = CreateFluidAnalysis(Ctx.DataContext, pl321.AssetID, DateTime.UtcNow, 123, "145_rt",
          t0, "Engine SideBar", "EGR_ER", 1, "N", 145, "h", "AA", "That green stuff in plastic tank", DateTime.UtcNow);

        ExecuteFluidAnalysisScript();

       fluid1 = CreateFluidAnalysis(Ctx.DataContext, pl321.AssetID, DateTime.UtcNow, 123, "145_rt",
        t0, "Engine SideBar", "EGR_ER", 1, "N", 145, "h", "BB", "That green stuff in plastic tank", DateTime.UtcNow);

        ExecuteFluidAnalysisScript();

        List<FluidAnalysis> Samples = (from sample in Ctx.RptContext.FluidAnalysisReadOnly
                                       where sample.ifk_DimAssetID == pl321.AssetID
                                       orderby sample.SampleNumber
                                       select sample).ToList<FluidAnalysis>();
        Assert.IsNotNull(Samples, "Failed to create FluidAnalysis Object");

        Assert.IsTrue(Samples.Count() == 1, "Should be 1 FluidAnalysis record.");

        Assert.AreEqual(123, Samples[0].SampleNumber, string.Format("SampleNumber incorrect"));
        Assert.AreEqual(t0.KeyDate(), Samples[0].fk_AssetKeyDate, string.Format("AssetKeyDate incorrect for Sample#:{0}", Samples[0].SampleNumber));
        Assert.AreEqual("Engine SideBar", Samples[0].CompartmentName, string.Format("CompartmentName incorrect for Sample#:{0}", Samples[0].SampleNumber));
        Assert.AreEqual("EGR_ER", Samples[0].CompartmentID, string.Format("CompartmentID incorrect for Sample#:{0}", Samples[0].SampleNumber));
        Assert.IsTrue(Samples[0].MeterValue == fluid1.MeterValue, string.Format("MeterValue incorrect for Sample#:{0}", Samples[0].SampleNumber));
        Assert.IsTrue(Samples[0].OverallEvaluation == fluid1.OverallEvaluation, string.Format("OverallEvaluation incorrect for Sample#:{0}", Samples[0].SampleNumber));
        Assert.IsTrue(Samples[0].Status == fluid1.Status, string.Format("Status incorrect for Sample#:{0}", Samples[0].SampleNumber));
    }
    
    [TestMethod]
    [DatabaseTest]
    public void SamplePlusActions()
    {
      // Initial sample at t0, then 2 subsequent actions
      // Should result in 1 row with the latest status
      // adding and processing these out of order as additional test

      DateTime initialUTC = DateTime.UtcNow.AddHours(-2);

      Asset pl321Asset = TestData.TestAssetPL321;
      
      Helpers.NHRpt.DimAsset_Populate();


      //Set up FluidAnalysis events
     
      DateTime t0 = DateTime.UtcNow.AddMonths(-6).Date;  
      DateTime actionUTC = t0.AddMonths(2).AddDays(4).AddHours(10);
      DateTime actionUTC2 = t0.AddMonths(2).AddDays(3).AddHours(8);
      

      DataFluidAnalysis fluid1 = CreateFluidAnalysis(Ctx.DataContext, pl321Asset.AssetID, DateTime.UtcNow, 123, "145_rt",
        t0, "Engine SideBar", "EGR_ER", 1, "N", 145, "h", "AA", "That green stuff in plastic tank", DateTime.UtcNow);


      DataFluidAnalysis fluid3 = CreateFluidAnalysis(Ctx.DataContext, pl321Asset.AssetID, DateTime.UtcNow, 123, "145_rt",
        t0, "Engine SideBar", "EGR_ER", 1, "R", 145, "h", "AA", "That green stuff in plastic tank",
        actionUTC);

      SetFluidAction(Ctx.DataContext, fluid3, actionUTC, 45, "Employee 998", "Jo", "Smoe", "Does it flush?");

      DataFluidAnalysis fluid2 = CreateFluidAnalysis(Ctx.DataContext, pl321Asset.AssetID, DateTime.UtcNow, 123, "145_rt",
                                               t0, "Engine SideBar", "EGR_ER", 1,
                                               "A", 156.5, "h", "AA", "That green stuff in plastic tank",actionUTC2);

      SetFluidAction(Ctx.DataContext, fluid2, actionUTC2, 1344, "Employee 55", "Mary", "Lamb", "Shearing Test filtered twice");


    
      ExecuteFluidAnalysisScript();

      List<FluidAnalysis> Samples = (from sample in Ctx.RptContext.FluidAnalysisReadOnly
                                     where sample.ifk_DimAssetID == pl321Asset.AssetID
                                     orderby sample.SampleNumber
                                     select sample).ToList<FluidAnalysis>();
      Assert.IsNotNull(Samples, "Failed to create FluidAnalysis Object");

      Assert.IsTrue(Samples.Count() == 1, "Should be 1 FluidAnalysis record.");
      Assert.AreEqual(123, Samples[0].SampleNumber, string.Format("SampleNumber incorrect"));
      Assert.AreEqual(t0.KeyDate(), Samples[0].fk_AssetKeyDate, string.Format("AssetKeyDate incorrect for Sample#:{0}", Samples[0].SampleNumber));
      Assert.AreEqual("Engine SideBar", Samples[0].CompartmentName, string.Format("CompartmentName incorrect for Sample#:{0}", Samples[0].SampleNumber));
      Assert.AreEqual("EGR_ER", Samples[0].CompartmentID, string.Format("CompartmentID incorrect for Sample#:{0}", Samples[0].SampleNumber));
      Assert.IsTrue(Samples[0].MeterValue == fluid1.MeterValue, string.Format("MeterValue incorrect for Sample#:{0}", Samples[0].SampleNumber));
      Assert.IsTrue(Samples[0].OverallEvaluation == fluid1.OverallEvaluation, string.Format("OverallEvaluation incorrect for Sample#:{0}", Samples[0].SampleNumber));
      Assert.IsTrue(Samples[0].Status == fluid3.Status, string.Format("Status incorrect for Sample#:{0}", Samples[0].SampleNumber));
      Assert.IsTrue(Samples[0].ActionUTC == fluid3.ActionUTC, string.Format("ActionUTC incorrect for Sample#:{0}", Samples[0].SampleNumber));
      Assert.IsTrue(Samples[0].ActionedByID == fluid3.ActionedByID, string.Format("ActionedByID incorrect for Sample#:{0}", Samples[0].SampleNumber));
      Assert.IsTrue(Samples[0].ActionedByFirstName == fluid3.ActionedByFirstName, string.Format("ActionedByFirstName incorrect for Sample#:{0}", Samples[0].SampleNumber));
      Assert.IsTrue(Samples[0].ActionedByLastName == fluid3.ActionedByLastName, string.Format("ActionedByLastName incorrect for Sample#:{0}", Samples[0].SampleNumber));
    }

    private static DataFluidAnalysis CreateFluidAnalysis(INH_DATA ctx, long assetID, DateTime insertUTC, long SampleNumber, string TextID, DateTime SampleTakenDate, string CompartmentName, string CompartmentID, int dimSourceID, string status, double? MeterValue, string MeterValueUnit, string OverallEvaluation, string Description, DateTime actionUTC)
    {

      
      DataFluidAnalysis fluid = new DataFluidAnalysis();
      fluid.AssetID = assetID;      
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
      fluid.InsertUTC = insertUTC;
      fluid.UpdateUTC = insertUTC;

      ctx.DataFluidAnalysis.AddObject(fluid);
      ctx.SaveChanges();

      return fluid;
    }

    private DataFluidAnalysis SetFluidAction(INH_DATA ctx, DataFluidAnalysis fluid, DateTime actionUTC, long? actionNumber, string actionByID,
      string actionByFirstName, string actionByLastName, string actionDescription)
    {
      fluid.ActionUTC = actionUTC;
      fluid.ActionNumber = actionNumber;
      fluid.ActionedByID = actionByID;
      fluid.ActionedByFirstName = actionByFirstName;
      fluid.ActionedByLastName = actionByLastName;
      fluid.ActionDescription = actionDescription;

      ctx.SaveChanges();

      return fluid;
    }


    //[TestMethod]
    //public void ActionBeforeSample()
    //{
    //  // Action occurs before the initial sample info (this condition was contrived during testing, and not found in real data)
    //  // Should result in 1 row with the latest status
    //  // adding and processing these out of order as additional test
    //    ActiveUser admin = AdminLogin();
    //    SessionContext session = API.Session.Validate(admin.SessionID);
    //    NH_DATA dataCtx = ObjectContextFactory.NewNHContext<NH_DATA>();

    //    string gpsDeviceID = "5062002";
    //    DateTime initialUTC = DateTime.UtcNow.AddHours(-2);

    //    Asset testAsset = CreateAssetWithDevice(session, session.CustomerID.Value, gpsDeviceID, DeviceTypeEnum.PL321, initialUTC);
    //    Assert.IsNotNull(testAsset);
    //    CreateAssetSubscription(session, testAsset.ID);
    //    Helpers.NHRpt.DimTables_Populate(); //Sync NH_OP.Asset to NH_RPT.DimAsset

    //    DimAsset asset = (from da in session.NHRptContext.DimAsset
    //                      where da.ID == testAsset.ID
    //                      select da).FirstOrDefault<DimAsset>();
    //    Assert.IsNotNull(asset, "Failed to create DimAsset record");

    //    //Set up FluidAnalysis events
    //    DateTime InsertUTC = new DateTime();
    //    InsertUTC = (DateTime.UtcNow).AddHours(-1);
    //    long SampleNumber = 123;
    //    string TextID = "145_rt";
    //    DateTime SampleTakenDate = new DateTime(2010, 2, 1, 00, 00, 00);
    //    string CompartmentName = "Engine SideBar";
    //    string CompartmentID = "EGR_ER";
    //    DataFluidAnalysis fluid1 = DataFluidAnalysis.CreateDataFluidAnalysis(-1, InsertUTC.AddMinutes(3), asset.ID, SampleNumber, TextID, CompartmentName, CompartmentID, "N", 1, SampleTakenDate);
    //    fluid1.MeterValue = 145;
    //    fluid1.MeterValueUnit = "h";
    //    fluid1.OverallEvaluation = "AA";
    //    fluid1.Description = "That green stuff in plastic tank";

    //    DateTime ActionUTC = new DateTime(2010, 4, 4, 08, 00, 00);

    //    DataFluidAnalysis fluid3 = DataFluidAnalysis.CreateDataFluidAnalysis(-1, InsertUTC, asset.ID, SampleNumber, TextID, CompartmentName, CompartmentID, "R", 1, SampleTakenDate);
    //    fluid3.ActionUTC = ActionUTC.AddDays(1).AddHours(2);
    //    fluid3.ActionNumber = 45;
    //    fluid3.ActionedByID = "Employee 998";
    //    fluid3.ActionedByFirstName = "Jo";
    //    fluid3.ActionedByLastName = "Smoe";
    //    fluid3.ActionDescription = "Does it flush?";

    //    dataCtx.AddToDataFluidAnalysis(fluid3);
    //    dataCtx.SaveChanges();
    //    ExecuteFluidAnalysisScript();  //Create records in FluidAnalysis table in NH_RPT
    //    List<FluidAnalysis> Samples = (from sample in session.NHRptContext.FluidAnalysisReadOnly
    //                                   where sample.DimAsset.ID == asset.ID
    //                                   orderby sample.SampleNumber
    //                                   select sample).ToList<FluidAnalysis>();
    //    Assert.IsNotNull(Samples, "Failed to create FluidAnalysis Object");

    //    Assert.IsTrue(Samples.Count() == 1, "Should be 1 FluidAnalysis record, with no Sampleinfo e.g. .");
    //    Assert.IsTrue(Samples[0].SampleTakenDate == SampleTakenDate, string.Format("SampleTakenDate incorrect for Sample#:{0}", Samples[0].SampleNumber));
    //    Assert.IsNull(Samples[0].OverallEvaluation, string.Format("OverallEvaluation incorrect for Sample#:{0}", Samples[0].SampleNumber));
       
    //    dataCtx.AddToDataFluidAnalysis(fluid1);
    //    dataCtx.SaveChanges();
    //    ExecuteFluidAnalysisScript();  //Create records in FluidAnalysis table in NH_RPT

    //    Samples = (from sample in session.NHRptContext.FluidAnalysisReadOnly
    //               where sample.DimAsset.ID == asset.ID
    //               orderby sample.SampleNumber
    //               select sample).ToList<FluidAnalysis>();
    //    Assert.IsNotNull(Samples, "Failed to create FluidAnalysis Object");

    //    Assert.IsTrue(Samples.Count() == 1, "Should be 1 FluidAnalysis record, with no Sampleinfo e.g. .");
    //    Assert.IsTrue(Samples[0].MeterValue == fluid1.MeterValue, string.Format("MeterValue incorrect for Sample#:{0}", Samples[0].SampleNumber));

    //    DataFluidAnalysis fluid2 = DataFluidAnalysis.CreateDataFluidAnalysis(-1, InsertUTC.AddMinutes(5), asset.ID, SampleNumber, TextID,  CompartmentName, CompartmentID, "A", 1, SampleTakenDate);
    //    fluid2.MeterValue = 156.5; // only the value in initial sample counts
    //    fluid2.ActionUTC = ActionUTC;
    //    fluid2.ActionNumber = 1344;
    //    fluid2.ActionedByID = "Employee 55";
    //    fluid2.ActionedByFirstName = "Mary";
    //    fluid2.ActionedByLastName = "Lamb";
    //    fluid2.ActionDescription = "Shearing Test filtered twice";
    //    dataCtx.AddToDataFluidAnalysis(fluid2); // adding these out of order as additional test
    //    dataCtx.SaveChanges();
    //    ExecuteFluidAnalysisScript();

    //    Samples = (from sample in session.NHRptContext.FluidAnalysisReadOnly
    //               where sample.DimAsset.ID == asset.ID
    //               orderby sample.SampleNumber
    //               select sample).ToList<FluidAnalysis>();
    //    Assert.IsNotNull(Samples, "Failed to create FluidAnalysis Object");

    //    Assert.IsTrue(Samples.Count() == 1, "Should be 1 FluidAnalysis record.");
    //    Assert.IsTrue(Samples[0].SampleNumber == SampleNumber, string.Format("SampleNumber incorrect"));
    //    Assert.IsTrue(Samples[0].SampleTakenDate == SampleTakenDate, string.Format("SampleTakenUTC incorrect for Sample#:{0}", Samples[0].SampleNumber));
    //    Assert.IsTrue(Samples[0].CompartmentName == CompartmentName, string.Format("CompartmentName incorrect for Sample#:{0}", Samples[0].SampleNumber));
    //    Assert.IsTrue(Samples[0].CompartmentID == CompartmentID, string.Format("CompartmentID incorrect for Sample#:{0}", Samples[0].SampleNumber));
    //    Assert.IsTrue(Samples[0].MeterValue == fluid1.MeterValue, string.Format("MeterValue incorrect for Sample#:{0}", Samples[0].SampleNumber));
    //    Assert.IsTrue(Samples[0].OverallEvaluation == fluid1.OverallEvaluation, string.Format("OverallEvaluation incorrect for Sample#:{0}", Samples[0].SampleNumber));
    //    Assert.IsTrue(Samples[0].Status == fluid3.Status, string.Format("Status incorrect for Sample#:{0}", Samples[0].SampleNumber));
    //    Assert.IsTrue(Samples[0].ActionUTC == fluid3.ActionUTC, string.Format("ActionUTC incorrect for Sample#:{0}", Samples[0].SampleNumber));
    //    Assert.IsTrue(Samples[0].ActionedByID == fluid3.ActionedByID, string.Format("ActionedByID incorrect for Sample#:{0}", Samples[0].SampleNumber));
    //    Assert.IsTrue(Samples[0].ActionedByFirstName == fluid3.ActionedByFirstName, string.Format("ActionedByFirstName incorrect for Sample#:{0}", Samples[0].SampleNumber));
    //    Assert.IsTrue(Samples[0].ActionedByLastName == fluid3.ActionedByLastName, string.Format("ActionedByLastName incorrect for Sample#:{0}", Samples[0].SampleNumber));
    //}

    private void ExecuteFluidAnalysisScript()
    {
      Helpers.NHRpt.FluidAnalysis_Populate();
    }
   
  }
}
