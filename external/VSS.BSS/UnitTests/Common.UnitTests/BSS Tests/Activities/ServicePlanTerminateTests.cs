using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ServicePlanTerminateTests : BssUnitTestBase
  {
    ServicePlanTerminate activity;
    Inputs inputs;
    DeviceServiceContext context;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new ServicePlanTerminate();
      inputs = new Inputs();
      context = new DeviceServiceContext
      {
        PlanLineID = IdGen.GetId().ToString(),
        ServiceTerminationDate = DateTime.UtcNow
      };
      inputs.Add<DeviceServiceContext>(context);
    }

    [TestMethod]
    public void Execute_ServicePlanTerminate_Success()
    {
      var fake = new BssServiceViewServiceFake(new Tuple<Service, IList<ServiceViewInfoDto>>(new Service { },
          new List<ServiceViewInfoDto> { 
          new ServiceViewInfoDto { 
            ServiceViewId = IdGen.GetId(), 
            ServiceTypeName = context.ServiceType.ToString(), 
            StartDateKey = DateTime.UtcNow.KeyDate(), 
            EndDateKey = DateTime.UtcNow.KeyDate(), 
            AssetId = IdGen.GetId(), 
            AssetSerialNumber = IdGen.GetId().ToString(), 
            CustomerId = IdGen.GetId(), 
            CustomerName = IdGen.GetId().ToString() 
          } }));
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Information, result.Type, "Result type should be Information.");
      StringAssert.Contains(result.Summary, "ServiceViews terminated", "Summary should contain success.");
    }

    [DatabaseTest]
    [TestMethod]
    public void TestReleaseAssetAfterServiceTermination()
    {
      Inputs input = new Inputs();
      DeviceServiceContext dsContext = new DeviceServiceContext();

      ServiceViewAPI target = new ServiceViewAPI();
      Customer customer = TestData.TestCustomer;
      Asset asset = TestData.TestAssetMTS521;
      DateTime currentDateTime = DateTime.UtcNow;

      Service service = target.CreateService(Ctx.OpContext, asset.fk_DeviceID, "TestBssID", currentDateTime, ServiceTypeEnum.Essentials);

      dsContext.IBKey = asset.Device.IBKey;
      dsContext.ServiceTerminationDate = currentDateTime;
      dsContext.PlanLineID = service.BSSLineID;
      dsContext.ExistingDeviceAsset.DeviceId = asset.fk_DeviceID;

      input.Add<DeviceServiceContext>(dsContext);

      var result = activity.Execute(input);

      Asset modifiedAsset = Ctx.OpContext.AssetReadOnly.Where(t => t.AssetID == asset.AssetID).Select(t => t).SingleOrDefault();

      Assert.AreEqual(0, modifiedAsset.fk_StoreID, "The asset should be mapped to NoStore");

    }

    [TestMethod]
    public void Execute_ServicePlanTerminate_NullResult_Failure()
    {
      var fake = new BssServiceViewServiceFake(serviceServiceViews: null);
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Error, result.Type, "Result type should be Error.");
      StringAssert.Contains(result.Summary, ServicePlanTerminate.COUNT_IS_ZERO_MESSAGE, "Summary should contain the message.");
    }

    [TestMethod]
    public void Execute_ServicePlanTerminate_NullService_Failure()
    {
      var fake = new BssServiceViewServiceFake(new Tuple<Service, IList<ServiceViewInfoDto>>(null, new List<ServiceViewInfoDto> { }));
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Error, result.Type, "Result type should be Error.");
      StringAssert.Contains(result.Summary, ServicePlanTerminate.COUNT_IS_ZERO_MESSAGE, "Summary should contain the message.");
    }

    [TestMethod]
    public void Execute_ServicePlanTerminate_ZeroServiceView_Failure()
    {
      var fake = new BssServiceViewServiceFake(new Tuple<Service, IList<ServiceViewInfoDto>>(new Service { }, new List<ServiceViewInfoDto> { }));
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Information, result.Type, "Result type should be Information.");
      StringAssert.Contains(result.Summary, "No ServiceViews were terminated.", "Summary should contain the message.");
    }

    [TestMethod]
    public void Execute_ServicePlanTerminate_NullServiceView_Failure()
    {
      var fake = new BssServiceViewServiceFake(new Tuple<Service, IList<ServiceViewInfoDto>>(new Service { }, null));
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Information, result.Type, "Result type should be Information.");
      StringAssert.Contains(result.Summary, "No ServiceViews were terminated.", "Summary should contain the message.");
    }

    [TestMethod]
    public void Execute_ServicePlanTerminate_Exception()
    {
      var fake = new BssServiceViewServiceExceptionFake(new NotImplementedException());
      Services.ServiceViews = () => fake;

      var result = activity.Execute(inputs);
      Assert.AreEqual(ResultType.Exception, result.Type, "Result type should be Exception.");
      StringAssert.Contains(result.Summary, "Failed to terminate Service and Service Views", "Summary should contain the message.");
    }
  }
}
