using ExecutorTests.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
  [TestClass]
  public class UpsertFilterExecutorTests : FilterRepositoryBase
  {
    [TestInitialize]
    public void ClassInit()
    {
      Setup();
    }

    [TestMethod]
    public async Task UpsertFilterExecutor_Transient_NoExisting()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = TestUtility.UIDs.JWT_USER_ID;
      string projectUid = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID.ToString();
      string name = string.Empty;
      FilterType filterType = FilterType.Transient;
      const string filterJson = "{\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"contributingMachines\":[{\"assetID\":\"123456789\",\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}]}";

      var request = FilterRequestFull.Create(new Dictionary<string, string>(), custUid, false, userUid, new ProjectData() { ProjectUid = projectUid }, new FilterRequest { Name = name, FilterJson = filterJson, FilterType = filterType });

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.FilterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result.FilterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJson, result.FilterDescriptor.FilterJson, "executor returned incorrect FilterJson");
    }

    [TestMethod]
    public async Task UpsertFilterExecutor_Transient_NonExisting_FilterUidNotSupported()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = string.Empty;
      FilterType filterType = FilterType.Transient;
      string filterJson = "{\"designUID\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"contributingMachines\":[{\"assetID\":\"123456789\",\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}]}";

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData() { ProjectUid = projectUid }, new FilterRequest { FilterUid = filterUid, Name = name, FilterJson = filterJson, FilterType = filterType });

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2016", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Transient filter not updateable, should not have filterUid provided.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    public async Task UpsertFilterExecutor_Transient_Existing_FilterUidNotSupported()
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = string.Empty;
      FilterType filterType = FilterType.Transient;
      string filterJson = "{\"designUID\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"contributingMachines\":[{\"assetID\":\"123456789\",\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}]}";
      string filterJsonUpdated = "{\"designUID\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"onMachineDesignID\":null,\"elevationType\":3,\"vibeStateOn\":true}";

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterType = filterType,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = FilterRequestFull.Create(null, custUid, false, userId, new ProjectData() { ProjectUid = projectUid }, new FilterRequest { FilterUid = filterUid, Name = name, FilterJson = filterJsonUpdated, FilterType = filterType });

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2016", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Transient filter not updateable, should not have filterUid provided.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    public async Task UpsertFilterExecutor_Transient_DuplicateNamesShouldBeAllowed()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = TestUtility.UIDs.JWT_USER_ID;
      string projectUid = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID.ToString();
      string name = string.Empty;
      FilterType filterType = FilterType.Transient;
      string filterJson1 = "{\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"contributingMachines\":[{\"assetID\":\"123456789\",\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}]}";
      string filterJson2 = "{\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"onMachineDesignID\":null,\"elevationType\":3,\"vibeStateOn\":true}";

      var request = FilterRequestFull.Create(new Dictionary<string, string>(), custUid, false, userUid,  new ProjectData() { ProjectUid = projectUid }, new FilterRequest { Name = name, FilterJson = filterJson1, FilterType = filterType });

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var result1 = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;
      Assert.IsNotNull(result1, "executor should always return a result");
      Assert.IsNotNull(result1.FilterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result1.FilterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJson1, result1.FilterDescriptor.FilterJson, "executor returned incorrect FilterJson");
      Assert.AreEqual(filterType, result1.FilterDescriptor.FilterType, "executor returned incorrect FilterType");

      request = FilterRequestFull.Create(new Dictionary<string, string>(), custUid, false, userUid, new ProjectData() { ProjectUid = projectUid }, new FilterRequest { Name = name, FilterJson = filterJson2, FilterType = filterType });
      var result2 = await executor.ProcessAsync(request).ConfigureAwait(false) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result2, "executor should always return a result");
      Assert.AreNotEqual(result1.FilterDescriptor.FilterUid, result2.FilterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result2.FilterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual("{\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"elevationType\":3,\"vibeStateOn\":true}", result2.FilterDescriptor.FilterJson, "executor returned incorrect FilterJson");
      Assert.AreEqual(filterType, result2.FilterDescriptor.FilterType, "executor returned incorrect FilterType");

      var fr = FilterRepo.GetFiltersForProjectUser(custUid, projectUid, userUid, true);
      fr.Wait();
      Assert.IsNotNull(fr, "should return the new filter");
      Assert.AreEqual(2, fr.Result.Count(), "should return 2 new filterUid");
    }

    [TestMethod]
    public async Task UpsertFilterExecutor_Transient_Existing_NoFilterUidProvided()
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = TestUtility.UIDs.JWT_USER_ID;
      string projectUid = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID.ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = string.Empty;
      FilterType filterType = FilterType.Transient;
      string filterJson = "{\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"contributingMachines\":[{\"assetID\":\"123456789\",\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}]}";
      string filterJsonUpdated = "{\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"onMachineDesignID\":null,\"elevationType\":3,\"vibeStateOn\":true}";

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterType = filterType,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = FilterRequestFull.Create(new Dictionary<string, string>(), custUid, false, userId, new ProjectData() { ProjectUid = projectUid }, new FilterRequest { Name = name, FilterJson = filterJsonUpdated, FilterType = filterType });

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.AreNotEqual(filterUid, result.FilterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result.FilterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual("{\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"elevationType\":3,\"vibeStateOn\":true}", result.FilterDescriptor.FilterJson, "executor returned incorrect FilterJson");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public async Task UpsertFilterExecutor_Persistent_NoExisting(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = TestUtility.UIDs.JWT_USER_ID;
      string projectUid = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID.ToString();
      string name = "the Name";
      string filterJson = "{\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"contributingMachines\":[{\"assetID\":\"123456789\",\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}]}";

      var request = FilterRequestFull.Create(new Dictionary<string, string>(), custUid, false, userUid, new ProjectData() { ProjectUid = projectUid }, new FilterRequest { Name = name, FilterJson = filterJson, FilterType = filterType });

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.FilterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result.FilterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJson, result.FilterDescriptor.FilterJson, "executor returned incorrect FilterJson");
      Assert.AreEqual(filterType, result.FilterDescriptor.FilterType, "executor returned incorrect FilterType");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public async Task UpsertFilterExecutor_Persistent_NoExisting_InvalidFilterUidProvided(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "the Name";
      string filterJson = "{\"designUID\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"contributingMachines\":[{\"assetID\":123456789,\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}]}";

      var request = FilterRequestFull.Create(null, custUid, false, userUid, new ProjectData() { ProjectUid = projectUid }, new FilterRequest { FilterUid = filterUid, Name = name, FilterJson = filterJson, FilterType = filterType });

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2021", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Unable to find persistent filterUid provided.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public async Task UpsertFilterExecutor_Persistent_Existing_ChangeJsonIgnored(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = TestUtility.UIDs.JWT_USER_ID;
      string projectUid = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID.ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "theName";
      string filterJson = "{\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"contributingMachines\":[{\"assetID\":\"123456789\",\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}]}";
      string filterJsonUpdated = "{\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"onMachineDesignID\":null,\"elevationType\":3,\"vibeStateOn\":true}";

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterType = filterType,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = FilterRequestFull.Create(new Dictionary<string, string>(), custUid, false, userId, new ProjectData() { ProjectUid = projectUid }, new FilterRequest { FilterUid = filterUid, Name = name, FilterJson = filterJsonUpdated, FilterType = filterType });

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.FilterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(filterUid, result.FilterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(name, result.FilterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJson, result.FilterDescriptor.FilterJson, "executor returned incorrect FilterJson, should be the original");
      Assert.AreEqual(filterType, result.FilterDescriptor.FilterType, "executor returned incorrect FilterType, should be the original");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public async Task UpsertFilterExecutor_Persistent_Existing_ChangeJsonAndName(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = TestUtility.UIDs.JWT_USER_ID;
      string projectUid = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID.ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "theName";
      string nameUpdated = "theName updated";
      string filterJson = "{\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"contributingMachines\":[{\"assetID\":\"123456789\",\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}]}";
      string filterJsonUpdated = "{\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"onMachineDesignID\":null,\"elevationType\":3,\"vibeStateOn\":true}";

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterType = filterType,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = FilterRequestFull.Create(new Dictionary<string, string>(), custUid, false, userId,  new ProjectData() { ProjectUid = projectUid }, new FilterRequest { FilterUid = filterUid, Name = nameUpdated, FilterJson = filterJsonUpdated, FilterType = filterType });

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.FilterDescriptor.FilterUid, "executor returned null FilterUid");
      Assert.AreEqual(filterUid, result.FilterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(nameUpdated, result.FilterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJson, result.FilterDescriptor.FilterJson, "executor returned incorrect FilterJson, should return original");
      Assert.AreEqual(filterType, result.FilterDescriptor.FilterType, "executor returned incorrect FilterType, should return original");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public async Task UpsertFilterExecutor_Persistent_ExistingName_AddNew_CaseInsensitive(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = TestUtility.UIDs.JWT_USER_ID;
      string projectUid = TestUtility.UIDs.MOCK_WEB_API_DIMENSIONS_PROJECT_UID.ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "theName";
      string nameUpdated = name.ToUpper();
      string filterJson = "{\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"contributingMachines\":[{\"assetID\":\"123456789\",\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}]}";
      string filterJsonNew = "{\"designUid\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"onMachineDesignID\":null,\"elevationType\":3,\"vibeStateOn\":true}";//should be ignored

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterType = filterType,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      // try to update a filter with same name but upper case (allowed!)
      var request = FilterRequestFull.Create(new Dictionary<string, string>(), custUid, false, userId,  new ProjectData() { ProjectUid = projectUid }, new FilterRequest { FilterUid = filterUid, Name = nameUpdated, FilterJson = filterJsonNew, FilterType = filterType });

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsNotNull(result.FilterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(filterUid, result.FilterDescriptor.FilterUid, "executor returned incorrect FilterUid");
      Assert.AreEqual(nameUpdated, result.FilterDescriptor.Name, "executor returned incorrect filter Name");
      Assert.AreEqual(filterJson, result.FilterDescriptor.FilterJson, "executor returned incorrect FilterJson - should return original");
      Assert.AreEqual(filterType, result.FilterDescriptor.FilterType, "executor returned incorrect FilterType - should return original");
    }

    [TestMethod]
    public async Task UpsertFilterExecutor_Persistent_ExistingName_NoFilterUidProvided_ChangeJson()
    {
      //Note: this test only applies to persistent filters not report filters.
      //Report filters are allowed duplicate names.

      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "theName";
      string filterJson = "{\"designUID\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"contributingMachines\":[{\"assetID\":123456789,\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}]}";
      string filterJsonUpdated = "{\"designUID\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"onMachineDesignID\":null,\"elevationType\":3,\"vibeStateOn\":true}";
      var filterType = FilterType.Persistent;

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = name,
        FilterType = filterType,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = FilterRequestFull.Create(null, custUid, false, userId,  new ProjectData() { ProjectUid = projectUid }, new FilterRequest { Name = name, FilterJson = filterJsonUpdated, FilterType = filterType });

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2039", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Unable to add persistent filter as Name already exists.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    public async Task UpsertFilterExecutor_Persistent_ExistingName_FilterUidProvided_ChangeJson()
    {
      //Note: this test only applies to persistent filters not report filters.
      //Report filters are allowed duplicate names.

      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid1 = Guid.NewGuid().ToString();
      string filterUid2 = Guid.NewGuid().ToString();
      string name = "theName";
      string filterJson = "{\"designUID\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"contributingMachines\":[{\"assetID\":123456789,\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}]}";
      string filterJsonUpdated = "{\"designUID\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"onMachineDesignID\":null,\"elevationType\":3,\"vibeStateOn\":true}";
      var filterType = FilterType.Persistent;
      ;

      var filterEvent = new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid1),
        Name = name,
        FilterType = filterType,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      };

      WriteEventToDb(filterEvent);

      filterEvent.FilterUID = Guid.Parse(filterUid2);
      filterEvent.Name = "something else";

      WriteEventToDb(filterEvent);

      // now try to change the 2nd filter to the name of the first
      var request = FilterRequestFull.Create(null, custUid, false, userId,  new ProjectData() { ProjectUid = projectUid }, new FilterRequest { FilterUid = filterUid2, Name = name, FilterJson = filterJsonUpdated, FilterType = filterType });

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      var content = ex.GetContent;
      Assert.AreNotEqual(-1, content.IndexOf("2039", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, content.IndexOf("UpsertFilter failed. Unable to add persistent filter as Name already exists.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    [DataRow(FilterType.Persistent)]
    [DataRow(FilterType.Report)]
    public async Task UpsertFilterExecutor_Persistent_Existing_FilterUidProvidedBelongsToTransient(FilterType filterType)
    {
      string custUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "theName";
      string filterJson = "{\"designUID\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"contributingMachines\":[{\"assetID\":123456789,\"machineName\":\"TheMachineName\",\"isJohnDoe\":false}]}";
      string filterJsonUpdated = "{\"designUID\":\"c2e5940c-4370-4d23-a930-b5b74a9fc22b\",\"onMachineDesignID\":null,\"elevationType\":3,\"vibeStateOn\":true}";

      WriteEventToDb(new CreateFilterEvent
      {
        CustomerUID = Guid.Parse(custUid),
        UserID = userId,
        ProjectUID = Guid.Parse(projectUid),
        FilterUID = Guid.Parse(filterUid),
        Name = string.Empty,
        FilterType = FilterType.Transient,
        FilterJson = filterJson,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow
      });

      var request = FilterRequestFull.Create(null, custUid, false, userId, new ProjectData() { ProjectUid = projectUid }, new FilterRequest { FilterUid = filterUid, Name = name, FilterJson = filterJsonUpdated, FilterType = filterType });

      var executor =
        RequestExecutorContainer.Build<UpsertFilterExecutor>(ConfigStore, Logger, ServiceExceptionHandler, FilterRepo, GeofenceRepo, ProjectListProxy, RaptorProxy, Producer, KafkaTopicName);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2021", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("UpsertFilter failed. Unable to find persistent filterUid provided.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }
  }
}