using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ServicePlanActivateTests : BssUnitTestBase
  {
    private ServicePlanActivate _activity;
    private Inputs _inputs;
    private DeviceServiceContext _context;

    [TestInitialize]
    public void TestInitialize()
    {
      _activity = new ServicePlanActivate();
      _inputs = new Inputs();
      _context = new DeviceServiceContext
      {
        IBKey = IdGen.GetId().ToString(CultureInfo.InvariantCulture),
        PlanLineID = IdGen.GetId().ToString(CultureInfo.InvariantCulture),
        ActionUTC = DateTime.UtcNow,
        ServiceType = ServiceTypeEnum.Essentials,
        OwnerVisibilityDate = DateTime.UtcNow,
        ExistingDeviceAsset = { DeviceId = IdGen.GetId(), Type = DeviceTypeEnum.PL321 }
      };
      _inputs.Add<DeviceServiceContext>(_context);
    }
    
    [TestMethod]
    public void Execute_ServicePlanActivate_Success()
    {
      var customerName = IdGen.GetId().ToString(CultureInfo.InvariantCulture);
      var fake = new BssServiceViewServiceFake(new Tuple<Service, IList<ServiceViewInfoDto>>(new Service(),
        new List<ServiceViewInfoDto> { 
          new ServiceViewInfoDto { 
            ServiceViewId = IdGen.GetId(), 
            ServiceTypeName = _context.ServiceType.ToString(), 
            StartDateKey = DateTime.UtcNow.KeyDate(), 
            EndDateKey = DateTime.UtcNow.KeyDate(), 
            AssetId = IdGen.GetId(), 
            AssetSerialNumber = IdGen.GetId().ToString(CultureInfo.InvariantCulture), 
            CustomerId = IdGen.GetId(), 
            CustomerName = customerName
          } }));
      Services.ServiceViews = () => fake;

      var result = _activity.Execute(_inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Information, result.Type, "Result type should be Information.");
      StringAssert.Contains(result.Summary, "ServiceViews created", "Summary should contain success.");
      StringAssert.Contains(result.Summary, "Name: " + customerName, "Summary should contain customer name.");
    }

    [TestMethod]
    public void Execute_ServicePlanActivate_CustomerNameContainsCurlyBraces_Success()
    {
      var customerName = IdGen.GetId().ToString(CultureInfo.InvariantCulture) + " {One} {Two}";
      var fake = new BssServiceViewServiceFake(new Tuple<Service, IList<ServiceViewInfoDto>>(new Service(),
        new List<ServiceViewInfoDto> { 
          new ServiceViewInfoDto { 
            ServiceViewId = IdGen.GetId(), 
            ServiceTypeName = _context.ServiceType.ToString(), 
            StartDateKey = DateTime.UtcNow.KeyDate(), 
            EndDateKey = DateTime.UtcNow.KeyDate(), 
            AssetId = IdGen.GetId(), 
            AssetSerialNumber = IdGen.GetId().ToString(CultureInfo.InvariantCulture), 
            CustomerId = IdGen.GetId(), 
            CustomerName = customerName
          } }));
      Services.ServiceViews = () => fake;

      var result = _activity.Execute(_inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Information, result.Type, "Result type should be Information.");
      StringAssert.Contains(result.Summary, "ServiceViews created", "Summary should contain success.");
      StringAssert.Contains(result.Summary, "Name: " + customerName, "Summary should contain customer name.");
    }

    [TestMethod]
    public void Execute_ServicePlanActivate_CustomerNameContainsParentheses_Success()
    {
      var customerName = IdGen.GetId().ToString(CultureInfo.InvariantCulture) + " (One) (Two)";
      var fake = new BssServiceViewServiceFake(new Tuple<Service, IList<ServiceViewInfoDto>>(new Service(),
        new List<ServiceViewInfoDto> { 
          new ServiceViewInfoDto { 
            ServiceViewId = IdGen.GetId(), 
            ServiceTypeName = _context.ServiceType.ToString(), 
            StartDateKey = DateTime.UtcNow.KeyDate(), 
            EndDateKey = DateTime.UtcNow.KeyDate(), 
            AssetId = IdGen.GetId(), 
            AssetSerialNumber = IdGen.GetId().ToString(CultureInfo.InvariantCulture), 
            CustomerId = IdGen.GetId(), 
            CustomerName = customerName
          } }));
      Services.ServiceViews = () => fake;

      var result = _activity.Execute(_inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Information, result.Type, "Result type should be Information.");
      StringAssert.Contains(result.Summary, "ServiceViews created", "Summary should contain success.");
      StringAssert.Contains(result.Summary, "Name: " + customerName, "Summary should contain customer name.");
    }

    [TestMethod]
    public void Execute_ServicePlanActivate_NullResult_Failure()
    {
      var fake = new BssServiceViewServiceFake(serviceServiceViews: null);
      Services.ServiceViews = () => fake;

      var result = _activity.Execute(_inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Error, result.Type, "Result type should be Error.");
      StringAssert.Contains(result.Summary, ServicePlanActivate.COUNT_IS_ZERO_MESSAGE, "Summary should contain the message.");
    }

    [TestMethod]
    public void Execute_ServicePlanActivate_NullService_Failure()
    {
      var fake = new BssServiceViewServiceFake(new Tuple<Service, IList<ServiceViewInfoDto>>(null, new List<ServiceViewInfoDto>()));
      Services.ServiceViews = () => fake;

      var result = _activity.Execute(_inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Error, result.Type, "Result type should be Error.");
      StringAssert.Contains(result.Summary, ServicePlanActivate.COUNT_IS_ZERO_MESSAGE, "Summary should contain the message.");
    }

    [TestMethod]
    public void Execute_ServicePlanActivate_ZeroServiceView_Failure()
    {
      var fake = new BssServiceViewServiceFake(new Tuple<Service, IList<ServiceViewInfoDto>>(new Service(), new List<ServiceViewInfoDto>()));
      Services.ServiceViews = () => fake;

      var result = _activity.Execute(_inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Information, result.Type, "Result type should be Information.");
      StringAssert.Contains(result.Summary, "No ServiceViews were created.", "Summary should contain the message.");
    }

    [TestMethod]
    public void Execute_ServicePlanActivate_NullServiceView_Failure()
    {
      var fake = new BssServiceViewServiceFake(new Tuple<Service, IList<ServiceViewInfoDto>>(new Service(), null));
      Services.ServiceViews = () => fake;

      var result = _activity.Execute(_inputs);
      Assert.IsTrue(fake.WasExecuted);
      Assert.AreEqual(ResultType.Information, result.Type, "Result type should be Information.");
      StringAssert.Contains(result.Summary, "No ServiceViews were created.", "Summary should contain the message.");
    }

    [TestMethod]
    public void Execute_ServicePlanActivate_Exception()
    {
      var fake = new BssServiceViewServiceExceptionFake(new NotImplementedException());
      Services.ServiceViews = () => fake;

      var result = _activity.Execute(_inputs);
      Assert.AreEqual(ResultType.Exception, result.Type, "Result type should be Exception.");
      StringAssert.Contains(result.Summary, "Failed to create Service Views for Device", "Summary should contain the message.");
    }
  }
}