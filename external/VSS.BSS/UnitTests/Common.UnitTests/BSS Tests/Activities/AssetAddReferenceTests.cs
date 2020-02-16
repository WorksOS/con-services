using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Services.Interfaces;
using VSS.Nighthawk.NHBssSvc;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AssetAddReferenceTests : BssUnitTestBase
  {
    private AssetAddReference _activity;
    private Inputs _inputs;

    [TestInitialize]
    public void TestInitialize()
    {
      _activity = new AssetAddReference();
      _inputs = new Inputs();
    }

    [TestMethod]
    public void Execute_ThrowException_ReturnExceptionResult()
    {
      var serviceFake = new BssAssetServiceExceptionFake();
      Services.Assets = () => serviceFake;

      var context = new AssetDeviceContext
      {
        Asset =
        {
          MakeCode = "CAT",
          SerialNumber = "5YW00051",
          AssetUID = Guid.NewGuid(),
        },
        Device =
        {
          IbKey = "123"
        }
      };

      _inputs.Add<AssetDeviceContext>(context);
      _inputs.Add<IBssReference>(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object));

      var activityResult = _activity.Execute(_inputs);

      var notifyResult = activityResult as NotifyResult;
      Assert.IsNotNull(notifyResult, "activity should have been a NotifyResult");
      Assert.IsTrue(serviceFake.WasExecuted, "AddAssetReference method should have been invoked.");
      StringAssert.Contains(activityResult.Summary, "Failed");
    }

    [TestMethod]
    public void Execute_AssetAddAssetReference_Success()
    {
      var serviceFake = new BssAssetServiceFake(true);
      Services.Assets = () => serviceFake;

      var context = new AssetDeviceContext
      {
        Asset =
        {
          MakeCode = "CAT",
          SerialNumber = "5YW00051",
          AssetUID = Guid.NewGuid(),
        },
        Device =
        {
          IbKey = "123"
        }
      };

      _inputs.Add<AssetDeviceContext>(context);
      _inputs.Add<IBssReference>(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object));

      var activityResult = _activity.Execute(_inputs);
      Assert.IsTrue(serviceFake.WasExecuted, "AddAssetReference should have been called.");
      StringAssert.Contains(activityResult.Summary, "Success: Created");
    }

    [TestMethod]
    public void Execute_AssetAddAssetReference_NegativeIBKey_Success()
    {
      var serviceFake = new BssAssetServiceFake(true);
      Services.Assets = () => serviceFake;

      var context = new AssetDeviceContext
      {
        Asset =
        {
          MakeCode = "CAT",
          SerialNumber = "5YW00051",
          AssetUID = Guid.NewGuid(),
        },
        Device =
        {
          IbKey = "-123"
        }
      };

      _inputs.Add<AssetDeviceContext>(context);
      _inputs.Add<IBssReference>(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object));

      var activityResult = _activity.Execute(_inputs);
      Assert.IsFalse(serviceFake.WasExecuted, "AddAssetReference should not have been called.");
      StringAssert.Contains(activityResult.Summary, "Success: Not creating");
    }
  }
}
