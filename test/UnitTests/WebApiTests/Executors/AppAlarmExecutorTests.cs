using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.TagFileAuth.Service.WebApi.Interfaces;
using VSS.TagFileAuth.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using VSS.TagFileAuth.Service.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.ResultHandling;
using VSS.TagFileAuth.Service.Executors;
using System;

namespace VSS.TagFileAuth.Service.WebApiTests.Executors
{
  [TestClass]
  public class AppAlarmExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public void CanCallAppAlarmExecutorNoValidInput()
    {
      AppAlarmRequest AppAlarmRequest = new AppAlarmRequest();
      AppAlarmResult AppAlarmResult = new AppAlarmResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<AppAlarmExecutor>(factory).Process(AppAlarmRequest) as AppAlarmResult;
      Assert.IsNotNull(result, "executor returned nothing");
      // todo Assert.AreEqual(-1, result., "executor returned incorrect legacy AppAlarm");
    }

    [TestMethod]
    public void CanCallGetPAppAlarmExecutorWithLegacyAssetId()
    {
      TSigLogMessageClass alarmType = null;
      string message = "the message";
      string exceptionMessage = "the exception message";
      var eventkeyDate = DateTime.UtcNow;
      AppAlarmRequest AppAlarmRequest = AppAlarmRequest.CreateAppAlarmRequest(alarmType, message, exceptionMessage);

      AppAlarmResult AppAlarmResult = new AppAlarmResult();
      var factory = serviceProvider.GetRequiredService<IRepositoryFactory>();

      var result = RequestExecutorContainer.Build<AppAlarmExecutor>(factory).Process(AppAlarmRequest) as AppAlarmResult;
      Assert.IsNotNull(result, "executor returned nothing");
      // todo  Assert.AreEqual(-1, result.AppAlarm, "executor returned incorrect legacy AppAlarm");
    }

    
  }
}
