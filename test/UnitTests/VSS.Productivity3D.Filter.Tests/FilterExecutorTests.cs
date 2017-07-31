using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities;

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

      // todo not needed
      var projectListProxy = new Mock<IProjectListProxy>();
      
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

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid, filterUid);
      var executor = RequestExecutorContainer.Build<GetFilterExecutor>(configStore, logger, serviceExceptionHandler, projectListProxy.Object, filterRepo.Object, producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(request) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(filterToTest.filterDescriptor.FilterUid, result.filterDescriptor.FilterUid, "executor returned incorrect filterDescriptor FilterUid");
      Assert.AreEqual(filterToTest.filterDescriptor.Name, result.filterDescriptor.Name, "executor returned incorrect filterDescriptor Name");
      Assert.AreEqual(filterToTest.filterDescriptor.FilterJson, result.filterDescriptor.FilterJson, "executor returned incorrect filterDescriptor FilterJson");
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

      // todo not needed for get
      var projectListProxy = new Mock<IProjectListProxy>();
    
      var filterRepo = new Mock<IFilterRepository>();
      var filters = new List<MasterData.Repositories.DBModels.Filter>()
      {
        new MasterData.Repositories.DBModels.Filter() {CustomerUid = custUid, UserUid = userUid, ProjectUid = projectUid, FilterUid = filterUid, Name = name, FilterJson = filterJson, LastActionedUtc = DateTime.UtcNow }
      };
      filterRepo.Setup(ps => ps.GetFiltersForProject(It.IsAny<string>())).ReturnsAsync(filters);

      var filterListToTest = new FilterDescriptorListResult
      {
        filterDescriptors = filters.Select(filter =>
            AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter))
          .ToImmutableList()
      };
      
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();

      var request = FilterRequestFull.CreateFilterFullRequest(custUid, false, userUid, projectUid);
      var executor = RequestExecutorContainer.Build<GetFiltersExecutor>(configStore, logger, serviceExceptionHandler, projectListProxy.Object, filterRepo.Object, producer.Object, kafkaTopicName);
      var filterListResult = await executor.ProcessAsync(request) as FilterDescriptorListResult;

      Assert.IsNotNull(filterListResult, "executor failed");
      Assert.AreEqual(filterListToTest.filterDescriptors[0], filterListResult.filterDescriptors[0], "executor returned incorrect filterDescriptor");
      Assert.AreEqual(filterListToTest.filterDescriptors[0].FilterUid, filterListResult.filterDescriptors[0].FilterUid, "executor returned incorrect filterDescriptor FilterUid");
      Assert.AreEqual(filterListToTest.filterDescriptors[0].Name, filterListResult.filterDescriptors[0].Name, "executor returned incorrect filterDescriptor Name");
      Assert.AreEqual(filterListToTest.filterDescriptors[0].FilterJson, filterListResult.filterDescriptors[0].FilterJson, "executor returned incorrect filterDescriptor FilterJson");
    }
  }
}

