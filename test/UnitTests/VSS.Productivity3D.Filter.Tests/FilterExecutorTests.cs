using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.Productivity3D.Filter.Common.Executors;
using VSS.Productivity3D.Filter.Common.ResultHandling;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class FilterExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    public async Task GetFilterExecutor()
    {
      string custUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string filterUid = Guid.NewGuid().ToString();
      string name = "blah";
      string filterJson = "theJsonString";

      var projectListProxy = new Mock<IProjectListProxy>();
      var projects = new List<ProjectData>
      {
        new ProjectData() {CustomerUid = custUid, ProjectUid = projectUid}
      };
      projectListProxy.Setup(ps => ps.GetProjectsV4(It.IsAny<string>(), null)).ReturnsAsync(projects);


      var filterRepo = new Mock<IFilterRepository>();
      var filter = new MasterData.Repositories.DBModels.Filter() { ProjectUid = projectUid, FilterUid = filterUid, Name = name, FilterJson = filterJson, LastActionedUtc = DateTime.UtcNow}; 
      filterRepo.Setup(ps => ps.GetFilter(It.IsAny<string>())).ReturnsAsync(filter);
      // todo map Filter to FilterDescriptor
      FilterDescriptor descriptor =
        new FilterDescriptor() {FilterUid = filterUid, Name = name, FilterJson = filterJson};

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();

      var executor = RequestExecutorContainer.Build<GetFilterExecutor>(configStore, logger, serviceExceptionHandler, projectListProxy.Object, filterRepo.Object, producer.Object, kafkaTopicName);
      var result = await executor.ProcessAsync(projectUid) as FilterDescriptorSingleResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(descriptor, result.filterDescriptor, "executor returned incorrect filterDescriptor");
    }
  }
}

