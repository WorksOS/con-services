using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using UnitTests.WebApi;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{
  [TestClass]
  public class OemAssetInformationStrategyTests : UnitTestBase
  {
    [TestMethod]
    public void Ctor_NullContext_Throws()
    {
      AssertEx.Throws<ArgumentNullException>(() => new OemAssetInformationStrategy(null), "ctx");
    }

    [TestMethod]
    public void GetStrategy_MakeIsCat_ReturnsCatStrategy()
    {
      var strategy = new OemAssetInformationStrategy(Ctx.OpContext).GetStrategy("CAT");
      Assert.IsInstanceOfType(strategy, typeof(CatAssetInformationStrategy));
    }

    [TestMethod]
    public void GetStrategy_MakeIsFgWilson_ReturnsCatStrategy()
    {
      var strategy = new OemAssetInformationStrategy(Ctx.OpContext).GetStrategy("F80");
      Assert.IsInstanceOfType(strategy, typeof(CatAssetInformationStrategy));
    }

    [TestMethod]
    public void GetStrategy_MakeIsOlympia_ReturnsCatStrategy()
    {
      var strategy = new OemAssetInformationStrategy(Ctx.OpContext).GetStrategy("O80");
      Assert.IsInstanceOfType(strategy, typeof(CatAssetInformationStrategy));
    }

    [TestMethod]
    public void GetStrategy_MakeIsVermeer_ReturnsVermeerStrategy()
    {
      var strategy = new OemAssetInformationStrategy(Ctx.OpContext).GetStrategy("VER");
      Assert.IsInstanceOfType(strategy, typeof(VermeerAssetInformationStrategy));
    }

    [TestMethod]
    public void GetStrategy_MakeIsUnknown_ReturnsNullStrategy()
    {
      var strategy = new OemAssetInformationStrategy(Ctx.OpContext).GetStrategy("COULD BE ANY MAKE THAT DOESN'T HAVE A STRATEGY");
      Assert.IsInstanceOfType(strategy, typeof(NullAssetInformationStrategy));
    }

    [TestMethod]
    public void Execute_NullAsset_Throws()
    {
      var oemStrategy = new OemAssetInformationStrategy(Ctx.OpContext);
      AssertEx.Throws<ArgumentNullException>(() => oemStrategy.UpdateAsset(null), "asset");
    }
  }

  [TestClass]
  public class CatAssetInformationStrategyTests : UnitTestBase
  {
    //bug 23731
    [TestMethod]
    public void ModelUpdateTest()
    {
      //Add Sales Model
      var productFamily = Entity.ProductFamily.Name("OHT").Description("OFF HIGHWAY TRUCKS").Save();
      Entity.SalesModel.ModelCode("795FAC").SerialNumberPrefix("ERM").StartRange(1).EndRange(99999).Description(
        "795FAC").ForProductFamily(productFamily).Save();
      var device = Entity.Device.PL321.GpsDeviceId("DQCAT0152967Q1").DeviceState(DeviceStateEnum.Subscribed).OwnerBssId(TestData.TestDealer.BSSID).Save();
      var asset =
        Entity.Asset.Name("2330").SerialNumberVin("ERM00279").MakeCode("CAT").ModelName("795F").ManufactureYear(2013).
          InsertUtc(DateTime.UtcNow).UpdateUtc(DateTime.UtcNow).ProductFamily("OFF HIGHWAY TRUCKS").IconID(15).
          WithDevice(device).IsMetric(false).Save();

      var oemStrategy = new OemAssetInformationStrategy(Ctx.OpContext).GetStrategy("CAT");
      oemStrategy.UpdateAsset(asset);

      Assert.AreEqual(false, oemStrategy.UseStoreInformation);
    }

    //bug 30063
    [TestMethod]
    public void ModelUpdateTest_TestSerialNumber_UsesInfoFromBssMessage()
    {
      var asset =
        Entity.Asset.Name("2330").SerialNumberVin("SN12091133RN").MakeCode("CAT").ModelName("795F").ManufactureYear(2013).
          InsertUtc(DateTime.UtcNow).UpdateUtc(DateTime.UtcNow).ProductFamily("OFF HIGHWAY TRUCKS").IconID(15).IsMetric(false).Build();

      var oemStrategy = new OemAssetInformationStrategy(Ctx.OpContext).GetStrategy("CAT");
      oemStrategy.UpdateAsset(asset);

      Assert.AreEqual(true, oemStrategy.UseStoreInformation);
    }
  }

  [TestClass]
  public class VermeerAssetInformationStrategyTests : UnitTestBase
  {
    [TestMethod]
    [DatabaseTest]
    public void ModelUpdateTest()
    {
      var oemStrategy = new OemAssetInformationStrategy(Ctx.OpContext).GetStrategy("VER");
      Assert.AreEqual(true, oemStrategy.UseStoreInformation);
    }
  }
}
