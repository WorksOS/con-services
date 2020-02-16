using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ManageCustomerServiceViewsTests : BssUnitTestBase
  {
    ManageCustomerServiceViews activity;
    Inputs inputs;
    DeviceServiceContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new ManageCustomerServiceViews();
      context = new DeviceServiceContext
      {
        ServiceType = ServiceTypeEnum.Essentials,
        OwnerVisibilityDate = DateTime.UtcNow,
        ExistingDeviceAsset = { DeviceId = IdGen.GetId(), OwnerBSSID = IdGen.GetId().ToString(), AssetId = IdGen.GetId() },
        ActionUTC = DateTime.UtcNow,
      };
      inputs = new Inputs();
      inputs.Add<DeviceServiceContext>(context);
    }

    [TestMethod]
    public void Execute_ServiceViewTerminateForCustomer_Failure()
    {
      var fake = new BssServiceViewServiceFake(serviceViewDtos: null);
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Error, result.Type, "Type should be error.");
      StringAssert.Contains(result.Summary, ManageCustomerServiceViews.NULL_RESULT_MESSAGE, "Summary is expeted to contain the mesage.");
    }

    [TestMethod]
    public void Execute_ServiceViewTerminateForCustomer_ZeroCount_Success()
    {
      var fake = new BssServiceViewServiceFake(serviceViewDtos: new List<ServiceViewInfoDto> { });
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Information, result.Type, "Type should be information.");
      StringAssert.Contains(result.Summary, ManageCustomerServiceViews.COUNT_IS_ZERO_MESSAGE, "Summary is expeted to contain the mesage.");
    }

    [TestMethod]
    public void Execute_ServiceViewTerminateForCustomer_ServiceViewsExist_Success()
    {
      var fake = new BssServiceViewServiceFake(serviceViewDtos: new List<ServiceViewInfoDto> 
      { new ServiceViewInfoDto 
        { 
          AssetId = IdGen.GetId(), 
          AssetSerialNumber = IdGen.GetId().ToString(), 
          CustomerId = IdGen.GetId(), 
          CustomerName = IdGen.GetId().ToString(), 
          EndDateKey = DateTime.UtcNow.KeyDate(), 
          ServiceTypeName = ServiceTypeEnum.Essentials.ToString(), 
          ServiceViewId = IdGen.GetId(), 
          StartDateKey = DateTime.UtcNow.KeyDate() 
        } 
      });
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Information, result.Type, "Type should be information.");
      StringAssert.Contains(result.Summary, string.Format(ManageCustomerServiceViews.SUCCESS_MESSAGE, 1), "Summary is expeted to contain the mesage.");
    }

    [TestMethod]
    public void Execute_ServiceViewTerminateForCustomer_ServiceViewsExist_Exception()
    {
      var fake = new BssServiceViewServiceExceptionFake(new NotImplementedException());
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Exception, result.Type, "Type should be information.");
      StringAssert.Contains(result.Summary, ManageCustomerServiceViews.FAILURE_MESSAGE, "Summary is expeted to contain the mesage.");
    }

  }
}
