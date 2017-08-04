using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
  [TestClass]
  public class UpsertFilterExecutorTests : TestControllerBase
  {

    [TestInitialize]
    public void Init()
    {
      SetupDI();
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Transient_NoExisting()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = null;
      string name = "";
      string filterJson = "theJsonString";

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      request.Validate(serviceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result.filterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJson, result.filterDescriptor.FilterJson, "executor returned incorrect FilterJson");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Transient_NoExisting_InvalidFilterUidProvided()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "";
      string filterJson = "theJsonString";

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      request.Validate(serviceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, producer, kafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2016", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Unable to find transient filterUid provided.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Transient_Existing()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "";
      string filterJson = "theJsonString";
      string filterJsonUpdated = "theJsonString updated";

      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserUID = Guid.Parse(userUid),
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      var s = filterRepo.StoreEvent(createFilterEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event not written");

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJsonUpdated);
      request.Validate(serviceExceptionHandler);
      
      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result.filterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJsonUpdated, result.filterDescriptor.FilterJson, "executor returned incorrect FilterJson");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Transient_Existing_NoFilterUidProvided()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "";
      string filterJson = "theJsonString";
      string filterJsonUpdated = "theJsonString updated";

      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserUID = Guid.Parse(userUid),
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      var s = filterRepo.StoreEvent(createFilterEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Filter event not written");

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, null, name, filterJsonUpdated);
      request.Validate(serviceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreEqual(filterUid, result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result.filterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJsonUpdated, result.filterDescriptor.FilterJson, "executor returned incorrect FilterJson");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Persistant_NoExisting()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = null;
      string name = "the Name";
      string filterJson = "theJsonString";

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      request.Validate(serviceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result.filterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJson, result.filterDescriptor.FilterJson, "executor returned incorrect FilterJson");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Persistant_NoExisting_InvalidFilterUidProvided()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "the Name";
      string filterJson = "theJsonString";

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      request.Validate(serviceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, producer, kafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2021", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Unable to find persistant filterUid provided.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Persistant_Existing_ChangeJson()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "theName";
      string filterJson = "theJsonString";
      string filterJsonUpdated = "theJsonString updated";

      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserUID = Guid.Parse(userUid),
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      filterRepo.StoreEvent(createFilterEvent).Wait();

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJsonUpdated);
      request.Validate(serviceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreNotEqual(filterUid, result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result.filterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJsonUpdated, result.filterDescriptor.FilterJson, "executor returned incorrect FilterJson");

      var rd = filterRepo.GetFilterForUnitTest(filterUid);
      rd.Wait();
      Assert.IsNotNull(rd, "should return the deleted filter");
      Assert.AreEqual(filterUid, rd.Result.FilterUid, "should return the original filterUid");
      Assert.AreEqual(name, rd.Result.Name, "should return original name");
      Assert.AreEqual(filterJson, rd.Result.FilterJson, "should return original FilterJson");

      var fr = filterRepo.GetFiltersForProjectUser(custUid, projectUid, userUid);
      fr.Wait();
      Assert.IsNotNull(fr, "should return the new filter");
      Assert.AreEqual(1, fr.Result.Count(), "should return 1 new filterUid");
      Assert.AreNotEqual(filterUid, fr.Result.ToList()[0].FilterUid, "should return a new filterUid");
      Assert.AreEqual(name, fr.Result.ToList()[0].Name, "should return same name");
      Assert.AreEqual(filterJsonUpdated, fr.Result.ToList()[0].FilterJson, "should return a new FilterJson");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Persistant_Existing_ChangeJsonAndName()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "theName";
      string nameUpdated = "theName updated";
      string filterJson = "theJsonString";
      string filterJsonUpdated = "theJsonString updated";

      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserUID = Guid.Parse(userUid),
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      filterRepo.StoreEvent(createFilterEvent).Wait();

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, nameUpdated, filterJsonUpdated);
      request.Validate(serviceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreNotEqual(filterUid, result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(nameUpdated, result.filterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJsonUpdated, result.filterDescriptor.FilterJson, "executor returned incorrect FilterJson");

      var rd = filterRepo.GetFilterForUnitTest(filterUid);
      rd.Wait();
      Assert.IsNotNull(rd, "should return the deleted filter");
      Assert.AreEqual(filterUid, rd.Result.FilterUid, "should return the original filterUid");
      Assert.AreEqual(name, rd.Result.Name, "should return original name");
      Assert.AreEqual(filterJson, rd.Result.FilterJson, "should return original FilterJson");

      var fr = filterRepo.GetFiltersForProjectUser(custUid, projectUid, userUid);
      fr.Wait();
      Assert.IsNotNull(fr, "should return the new filter");
      Assert.AreEqual(1, fr.Result.Count(), "should return 1 new filterUid");
      Assert.AreNotEqual(filterUid, fr.Result.ToList()[0].FilterUid, "should return a new filterUid");
      Assert.AreEqual(nameUpdated, fr.Result.ToList()[0].Name, "should return new name");
      Assert.AreEqual(filterJsonUpdated, fr.Result.ToList()[0].FilterJson, "should return a new FilterJson");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Persistant_Existing_NoFilterUidProvided_ChangeJson()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "theName";
      string filterJson = "theJsonString";
      string filterJsonUpdated = "theJsonString updated";

      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserUID = Guid.Parse(userUid),
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      filterRepo.StoreEvent(createFilterEvent).Wait();

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, null, name, filterJsonUpdated);
      request.Validate(serviceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreNotEqual(filterUid, result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result.filterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJsonUpdated, result.filterDescriptor.FilterJson, "executor returned incorrect FilterJson");

      var rd = filterRepo.GetFilterForUnitTest(filterUid);
      rd.Wait();
      Assert.IsNotNull(rd, "should return the deleted filter");
      Assert.AreEqual(filterUid, rd.Result.FilterUid, "should return the original filterUid");
      Assert.AreEqual(name, rd.Result.Name, "should return original name");
      Assert.AreEqual(filterJson, rd.Result.FilterJson, "should return original FilterJson");

      var fr = filterRepo.GetFiltersForProjectUser(custUid, projectUid, userUid);
      fr.Wait();
      Assert.IsNotNull(fr, "should return the new filter");
      Assert.AreEqual(1, fr.Result.Count(), "should return 1 new filterUid");
      Assert.AreNotEqual(filterUid, fr.Result.ToList()[0].FilterUid, "should return a new filterUid");
      Assert.AreEqual(name, fr.Result.ToList()[0].Name, "should return same name");
      Assert.AreEqual(filterJsonUpdated, fr.Result.ToList()[0].FilterJson, "should return a new FilterJson");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Persistant_Existing_FilterUidProvidedBelongsToTransient()

    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "theName";
      string filterJson = "theJsonString";
      string filterJsonUpdated = "theJsonString updated";

      var createTransientFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserUID = Guid.Parse(userUid),
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = "",
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      filterRepo.StoreEvent(createTransientFilterEvent).Wait();

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJsonUpdated);
      request.Validate(serviceExceptionHandler);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, producer, kafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2021", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Unable to find persistant filterUid provided.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }
  }
}
