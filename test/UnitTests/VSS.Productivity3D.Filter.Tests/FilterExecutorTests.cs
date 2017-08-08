using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using FilterDescriptor = VSS.Productivity3D.Filter.Common.ResultHandling.FilterDescriptor;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class FilterExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    public async Task GetFilterExecutor()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "theJsonString";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var filterRepo = new Mock<IFilterRepository>();
      var filter = new MasterData.Repositories.DBModels.Filter()
      {
        CustomerUid = custUid,
        UserUid = userUid,
        ProjectUid = projectUid,
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson,
        LastActionedUtc = DateTime.UtcNow
      };
      filterRepo.Setup(ps => ps.GetFilter(It.IsAny<string>())).ReturnsAsync(filter);
      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid);
      var executor =
        RequestExecutorContainer.Build<GetFilterExecutor>(configStore, logger, serviceExceptionHandler,
          filterRepo.Object);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(filterToTest.filterDescriptor.FilterUid, result.filterDescriptor.FilterUid,
        "executor returned incorrect filterDescriptor FilterUid");
      Assert.AreEqual(filterToTest.filterDescriptor.Name, result.filterDescriptor.Name,
        "executor returned incorrect filterDescriptor Name");
      Assert.AreEqual(filterToTest.filterDescriptor.FilterJson, result.filterDescriptor.FilterJson,
        "executor returned incorrect filterDescriptor FilterJson");
    }

    [TestMethod]
    public async Task GetFiltersExecutor()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "theJsonString";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var filterRepo = new Mock<IFilterRepository>();
      var filters = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserUid = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid,
          Name = name,
          FilterJson = filterJson,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(filters);

      var filterListToTest = new FilterDescriptorListResult
      {
        filterDescriptors = filters.Select(filter =>
            AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter))
          .ToImmutableList()
      };

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid);
      var executor =
        RequestExecutorContainer.Build<GetFiltersExecutor>(configStore, logger, serviceExceptionHandler,
          filterRepo.Object);
      var filterListResult = await executor.ProcessAsync(request) as FilterDescriptorListResult;

      Assert.IsNotNull(filterListResult, "executor failed");
      Assert.AreEqual(filterListToTest.filterDescriptors[0], filterListResult.filterDescriptors[0],
        "executor returned incorrect filterDescriptor");
      Assert.AreEqual(filterListToTest.filterDescriptors[0].FilterUid, filterListResult.filterDescriptors[0].FilterUid,
        "executor returned incorrect filterDescriptor FilterUid");
      Assert.AreEqual(filterListToTest.filterDescriptors[0].Name, filterListResult.filterDescriptors[0].Name,
        "executor returned incorrect filterDescriptor Name");
      Assert.AreEqual(filterListToTest.filterDescriptors[0].FilterJson,
        filterListResult.filterDescriptors[0].FilterJson, "executor returned incorrect filterDescriptor FilterJson");
    }

    [TestMethod]
    public async Task UpsertFilterExecutor_Transient()
    {
      // this scenario, the filterUid is supplied, and is provided in request
      // so this will result in an updated filter
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = string.Empty;
      string filterJson = "theJsonString";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var raptorProxy = new Mock<IRaptorProxy>();
      var producer = new Mock<IKafka>();
      string kafkaTopicName = "whatever";

      var filterRepo = new Mock<IFilterRepository>();
      var filter = new MasterData.Repositories.DBModels.Filter()
      {
        CustomerUid = custUid,
        UserUid = userUid,
        ProjectUid = projectUid,
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson,
        LastActionedUtc = DateTime.UtcNow
      };
      var filters = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserUid = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid,
          Name = name,
          FilterJson = filterJson,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(filters);
      filterRepo.Setup(ps => ps.StoreEvent(It.IsAny<UpdateFilterEvent>())).ReturnsAsync(1);

      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, projectListProxy.Object, raptorProxy.Object, producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(filterToTest.filterDescriptor.FilterUid, result.filterDescriptor.FilterUid,
        "executor returned incorrect filterDescriptor FilterUid");
      Assert.AreEqual(filterToTest.filterDescriptor.Name, result.filterDescriptor.Name,
        "executor returned incorrect filterDescriptor Name");
      Assert.AreEqual(filterToTest.filterDescriptor.FilterJson, result.filterDescriptor.FilterJson,
        "executor returned incorrect filterDescriptor FilterJson");
    }

    [TestMethod]
    public async Task UpsertFilterExecutor_Persistant()
    {
      // this scenario, the filterUid is supplied, and is provided in request
      // so this will result in a deleted then created filter
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "not entry";
      string filterJson = "theJsonString";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var raptorProxy = new Mock<IRaptorProxy>();
      var producer = new Mock<IKafka>();
      string kafkaTopicName = "whatever";

      var filterRepo = new Mock<IFilterRepository>();
      var filter = new MasterData.Repositories.DBModels.Filter()
      {
        CustomerUid = custUid,
        UserUid = userUid,
        ProjectUid = projectUid,
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson,
        LastActionedUtc = DateTime.UtcNow
      };
      var filters = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserUid = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid,
          Name = name,
          FilterJson = filterJson,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(filters);
      filterRepo.Setup(ps => ps.StoreEvent(It.IsAny<DeleteFilterEvent>())).ReturnsAsync(1);
      filterRepo.Setup(ps => ps.StoreEvent(It.IsAny<CreateFilterEvent>())).ReturnsAsync(1);

      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      var executor = RequestExecutorContainer.Build<UpsertFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, projectListProxy.Object, raptorProxy.Object, producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(filterToTest.filterDescriptor.FilterUid, result.filterDescriptor.FilterUid,
        "executor returned incorrect filterDescriptor FilterUid");
      Assert.AreEqual(filterToTest.filterDescriptor.Name, result.filterDescriptor.Name,
        "executor returned incorrect filterDescriptor Name");
      Assert.AreEqual(filterToTest.filterDescriptor.FilterJson, result.filterDescriptor.FilterJson,
        "executor returned incorrect filterDescriptor FilterJson");
    }

    [TestMethod]
    public async Task DeleteFilterExecutor()
    {
      string custUid = Guid.NewGuid().ToString();
      string userUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "not entry";
      string filterJson = "theJsonString";

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectListProxy = new Mock<IProjectListProxy>();
      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(ps => ps.NotifyFilterChange(It.IsAny<Guid>(), null)).ReturnsAsync(new BaseDataResult());

      var producer = new Mock<IKafka>();
      string kafkaTopicName = "whatever";

      var filterRepo = new Mock<IFilterRepository>();
      var filter = new MasterData.Repositories.DBModels.Filter()
      {
        CustomerUid = custUid,
        UserUid = userUid,
        ProjectUid = projectUid,
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson,
        LastActionedUtc = DateTime.UtcNow
      };
      var filters = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter()
        {
          CustomerUid = custUid,
          UserUid = userUid,
          ProjectUid = projectUid,
          FilterUid = filterUid,
          Name = name,
          FilterJson = filterJson,
          LastActionedUtc = DateTime.UtcNow
        }
      };
      filterRepo.Setup(ps => ps.GetFiltersForProjectUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(filters);
      filterRepo.Setup(ps => ps.StoreEvent(It.IsAny<DeleteFilterEvent>())).ReturnsAsync(1);

      var filterToTest = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));

      var request =
        FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid, name, filterJson);
      var executor = RequestExecutorContainer.Build<DeleteFilterExecutor>(configStore, logger, serviceExceptionHandler,
        filterRepo.Object, projectListProxy.Object, raptorProxy.Object, producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(request);

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(0, result.Code, "executor returned incorrect result code");
    }
  }
}

