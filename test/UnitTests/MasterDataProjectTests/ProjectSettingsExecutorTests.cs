using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ProjectSettingsExecutorTests : ExecutorBaseTests
  {
    protected ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();

    [TestMethod]
    public async Task GetProjectSettingsExecutor_NoDataExists()
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      string projectUid = Guid.NewGuid().ToString();

      var projectRepo = new Mock<IProjectRepository>();
      var project = new Repositories.DBModels.Project() { CustomerUID = customerUid, ProjectUID = projectUid };
      var projectList = new List<Repositories.DBModels.Project>(); projectList.Add(project);
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>())).ReturnsAsync(new ProjectSettings());

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, "");

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
        customerUid, userId, userEmailAddress, null,
        null, null,
        null, null, null,
        projectRepo.Object );
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      
      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect projectUid");
      Assert.IsNull(result.settings, "executor should have returned empty settings");
    }

    [TestMethod]
    public async Task GetProjectSettingsExecutor_DataExists()
    {
      string customerUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string settings = "";

      var projectRepo = new Mock<IProjectRepository>();
      var project = new Repositories.DBModels.Project() { CustomerUID = customerUid, ProjectUID = projectUid };
      var projectList = new List<Repositories.DBModels.Project>(); projectList.Add(project);
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      var projectSettings = new ProjectSettings() { ProjectUid = projectUid, Settings = settings };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>())).ReturnsAsync(projectSettings);

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings);

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, null, null, null,
        null, null,
        null, null, null,
         projectRepo.Object );
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect projectUid");
      Assert.AreEqual(settings, result.settings, "executor should have returned settings");
    }

    [TestMethod]
    public async Task GetProjectSettingsExecutor_ProjectCustomerValidationFails()
    {
      string customerUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();

      var projectRepo = new Mock<IProjectRepository>();
      var projectList = new List<Repositories.DBModels.Project>(); 
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>())).ReturnsAsync(new ProjectSettings());

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, "");

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
          customerUid, null, null, null,
          null, null,
          null, null, null,
          projectRepo.Object);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>( async () =>
        await executor.ProcessAsync(projectSettingsRequest));

      Assert.AreNotEqual(-1, ex.Content.IndexOf(contractExecutionStatesEnum.FirstNameWithOffset(1)));
    }

    [TestMethod]
    public async Task UpdateProjectSettingsExecutor()
    {
      string customerUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string settings = "blah";

      var projectRepo = new Mock<IProjectRepository>();
      var projectSettings = new ProjectSettings() { ProjectUid = projectUid, Settings = settings };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>())).ReturnsAsync(projectSettings);
      var projectList = new List<Repositories.DBModels.Project>();
      projectList.Add(new Repositories.DBModels.Project(){ ProjectUID = projectUid});
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(ps => ps.StoreEvent(It.IsAny<UpdateProjectSettingsEvent>())).ReturnsAsync(1); 

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
 
      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(r => r.ValidateProjectSettings(It.IsAny<Guid>(), It.IsAny<string>(),
        It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new BaseDataResult());


      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
        customerUid, null, null, null,
        producer.Object, kafkaTopicName,
        null, raptorProxy.Object, null,
        projectRepo.Object);
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect projectUid");
      Assert.AreEqual(settings, result.settings, "executor returned incorrect settings");
    }

  }
}
