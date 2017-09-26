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

      var request = FilterRequestFull.Create(custUid, false, userUid, projectUid, filterUid, name, filterJson);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result.filterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJson, result.filterDescriptor.FilterJson, "executor returned incorrect FilterJson");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Transient_NonExisting_FilterUidNotSupported()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "";
      string filterJson = "theJsonString";

      var request = FilterRequestFull.Create(custUid, false, userUid, projectUid, filterUid, name, filterJson);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2016", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Transient filter not updateable, should not have filterUid provided.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Transient_Existing_FilterUidNotSupported()
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "";
      string filterJson = "theJsonString";
      string filterJsonUpdated = "theJsonString updated";

      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
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

      var request = FilterRequestFull.Create(custUid, false, userId, projectUid, filterUid, name, filterJsonUpdated);
      
      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2016", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Transient filter not updateable, should not have filterUid provided.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Transient_DuplicateNamesShouldBeAllowed()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string name = "";
      string filterJson1 = "theJsonString";
      string filterJson2 = "theJsonString different";

      var request = FilterRequestFull.Create(custUid, false, userUid, projectUid, null, name, filterJson1);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var result1 = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;
      Assert.IsNotNull(result1, "executor should always return a result");
      Assert.IsNotNull(result1.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result1.filterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJson1, result1.filterDescriptor.FilterJson, "executor returned incorrect FilterJson");

      request = FilterRequestFull.Create(custUid, false, userUid, projectUid, null, name, filterJson2);
      var result2 = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result2, "executor should always return a result");
      Assert.AreNotEqual(result1.filterDescriptor.FilterUid, result2.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result2.filterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJson2, result2.filterDescriptor.FilterJson, "executor returned incorrect FilterJson");

      var fr = filterRepo.GetFiltersForProjectUser(custUid, projectUid, userUid, true);
      fr.Wait();
      Assert.IsNotNull(fr, "should return the new filter");
      Assert.AreEqual(2, fr.Result.Count(), "should return 2 new filterUid");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Transient_Existing_NoFilterUidProvided()
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "";
      string filterJson = "theJsonString";
      string filterJsonUpdated = "theJsonString updated";

      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
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

      var request = FilterRequestFull.Create(custUid, false, userId, projectUid, null, name, filterJsonUpdated);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreNotEqual(filterUid, result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result.filterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJsonUpdated, result.filterDescriptor.FilterJson, "executor returned incorrect FilterJson");
    }




    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Persistent_NoExisting()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = null;
      string name = "the Name";
      string filterJson = "theJsonString";

      var request = FilterRequestFull.Create(custUid, false, userUid, projectUid, filterUid, name, filterJson);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result.filterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJson, result.filterDescriptor.FilterJson, "executor returned incorrect FilterJson");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Persistent_NoExisting_InvalidFilterUidProvided()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "the Name";
      string filterJson = "theJsonString";

      var request = FilterRequestFull.Create(custUid, false, userUid, projectUid, filterUid, name, filterJson);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2021", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Unable to find persistent filterUid provided.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Persistent_Existing_ChangeJsonIgnored()
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "theName";
      string filterJson = "theJsonString";
      string filterJsonUpdated = "theJsonString updated";

      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      filterRepo.StoreEvent(createFilterEvent).Wait();

      var request = FilterRequestFull.Create(custUid, false, userId, projectUid, filterUid, name, filterJsonUpdated);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(filterUid, result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result.filterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJson, result.filterDescriptor.FilterJson, "executor returned incorrect FilterJson, should be the original");

      var fr = filterRepo.GetFiltersForProjectUser(custUid, projectUid, userId);
      fr.Wait();
      Assert.IsNotNull(fr, "should return the updated filter");
      Assert.AreEqual(1, fr.Result.Count(), "should return 1 updatedw filterUid");
      Assert.AreEqual(filterUid, fr.Result.ToList()[0].FilterUid, "should return same filterUid");
      Assert.AreEqual(name, fr.Result.ToList()[0].Name, "should return same name");
      Assert.AreEqual(filterJson, fr.Result.ToList()[0].FilterJson, "should return a original FilterJson");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Persistent_Existing_ChangeJsonAndName()
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "theName";
      string nameUpdated = "theName updated";
      string filterJson = "theJsonString";
      string filterJsonUpdated = "theJsonString updated";

      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      filterRepo.StoreEvent(createFilterEvent).Wait();

      var request = FilterRequestFull.Create(custUid, false, userId, projectUid, filterUid, nameUpdated, filterJsonUpdated);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(filterUid, result.filterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(nameUpdated, result.filterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJson, result.filterDescriptor.FilterJson, "executor returned incorrect FilterJson, should return original");
      
      var fr = filterRepo.GetFiltersForProjectUser(custUid, projectUid, userId);
      fr.Wait();
      Assert.IsNotNull(fr, "should return the updated filter");
      Assert.AreEqual(1, fr.Result.Count(), "should return 1 filter");
      Assert.AreEqual(filterUid, fr.Result.ToList()[0].FilterUid, "should return same filterUid");
      Assert.AreEqual(nameUpdated, fr.Result.ToList()[0].Name, "should return new name");
      Assert.AreEqual(filterJson, fr.Result.ToList()[0].FilterJson, "should return original FilterJson");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Persistent_ExistingName_AddNew_CaseInsensitive()
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "theName";
      string filterJson = "theJsonString";
      string filterJsonNew = "theJsonString updated"; // should be ignored

      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      filterRepo.StoreEvent(createFilterEvent).Wait();

      // try to add a new filterUid with same name but upper case
      var request = FilterRequestFull.Create(custUid, false, userId, projectUid, null, name.ToUpper(), filterJsonNew);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2039", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Unable to add persistent filter as Name already exists.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
      
      var fr = filterRepo.GetFiltersForProjectUser(custUid, projectUid, userId);
      fr.Wait();
      Assert.IsNotNull(fr, "should return the updated filter");
      Assert.AreEqual(1, fr.Result.Count(), "should return 1 filter");
      Assert.AreEqual(filterUid, fr.Result.ToList()[0].FilterUid, "should return same filterUid");
      Assert.AreEqual(name, fr.Result.ToList()[0].Name, "should return new name");
      Assert.AreEqual(filterJson, fr.Result.ToList()[0].FilterJson, "should return a new FilterJson");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Persistent_ExistingName_NoFilterUidProvided_ChangeJson()
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "theName";
      string filterJson = "theJsonString";
      string filterJsonUpdated = "theJsonString updated";

      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      filterRepo.StoreEvent(createFilterEvent).Wait();

      var request = FilterRequestFull.Create(custUid, false, userId, projectUid, null, name, filterJsonUpdated);
 
      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2039", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Unable to add persistent filter as Name already exists.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Persistent_ExistingName_FilterUidProvided_ChangeJson()
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid1 = Guid.NewGuid().ToString();
      string filterUid2 = Guid.NewGuid().ToString();
      string name = "theName";
      string filterJson = "theJsonString";
      string filterJsonUpdated = "theJsonString updated";

      var createFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid1),
        Name = name,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      filterRepo.StoreEvent(createFilterEvent).Wait();

      createFilterEvent.FilterUID = Guid.Parse(filterUid2);
      createFilterEvent.Name = "something else";
      filterRepo.StoreEvent(createFilterEvent).Wait();

      // now try to change the 2nd filter to the name of the first
      var request = FilterRequestFull.Create(custUid, false, userId, projectUid, filterUid2, name, filterJsonUpdated);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2039", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Unable to add persistent filter as Name already exists.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    public async System.Threading.Tasks.Task UpsertFilterExecutor_Persistent_Existing_FilterUidProvidedBelongsToTransient()
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "theName";
      string filterJson = "theJsonString";
      string filterJsonUpdated = "theJsonString updated";

      var createTransientFilterEvent = new CreateFilterEvent()
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = "",
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };
      filterRepo.StoreEvent(createTransientFilterEvent).Wait();

      var request = FilterRequestFull.Create(custUid, false, userId, projectUid, filterUid, name, filterJsonUpdated);

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler, filterRepo, projectListProxy, raptorProxy, producer, kafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2021", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Unable to find persistent filterUid provided.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }
  }
}
