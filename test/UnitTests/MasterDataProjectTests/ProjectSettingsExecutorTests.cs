using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
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
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    public async Task GetProjectSettingsExecutor_NoDataExists(ProjectSettingsType settingsType)
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      string projectUid = Guid.NewGuid().ToString();

      var projectRepo = new Mock<IProjectRepository>();
      var project = new Repositories.DBModels.Project() { CustomerUID = customerUid, ProjectUID = projectUid };
      var projectList = new List<Repositories.DBModels.Project>(); projectList.Add(project);
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), It.IsAny<string>(), settingsType)).ReturnsAsync(new ProjectSettings());

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, string.Empty, settingsType);

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
      Assert.AreEqual(ProjectSettingsType.Unknown, result.projectSettingsType, "executor should have returned unknown settings type");
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    public async Task GetProjectSettingsExecutor_DataExists(ProjectSettingsType settingsType)
    {
      string customerUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string settings = string.Empty;
      string userId = "my app";

      var projectRepo = new Mock<IProjectRepository>();
      var project = new Repositories.DBModels.Project() { CustomerUID = customerUid, ProjectUID = projectUid };
      var projectList = new List<Repositories.DBModels.Project>(); projectList.Add(project);
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      var projectSettings = new ProjectSettings { ProjectUid = projectUid, Settings = settings, ProjectSettingsType = settingsType, UserID = userId };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), userId, settingsType)).ReturnsAsync(projectSettings);

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, userId, null, null,
        null, null,
        null, null, null,
         projectRepo.Object );
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect projectUid");
      Assert.AreEqual(settings, result.settings, "executor should have returned settings");
      Assert.AreEqual(settingsType, result.projectSettingsType, "executor returned incorrect projectSettingsType");
    }

    [TestMethod]
    public async Task GetProjectSettingsExecutor_ProjectCustomerValidationFails()
    {
      string customerUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string userId = "my app";

      var projectRepo = new Mock<IProjectRepository>();
      var projectList = new List<Repositories.DBModels.Project>(); 
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ProjectSettingsType>())).ReturnsAsync(new ProjectSettings());

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, string.Empty, ProjectSettingsType.Targets);

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
          customerUid, userId, null, null,
          null, null,
          null, null, null,
          projectRepo.Object);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>( async () =>
        await executor.ProcessAsync(projectSettingsRequest));

      Assert.AreNotEqual(-1, ex.Content.IndexOf(contractExecutionStatesEnum.FirstNameWithOffset(1)));
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    public async Task UpsertProjectSettingsExecutor_Targets(ProjectSettingsType settingsType)
    {
      string customerUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string settings = "blah";
      string userId = "my app";

      var projectRepo = new Mock<IProjectRepository>();
      var projectSettings = new ProjectSettings { ProjectUid = projectUid, Settings = settings, ProjectSettingsType = settingsType, UserID = userId};
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), userId, settingsType)).ReturnsAsync(projectSettings);
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
        customerUid, userId, null, null,
        producer.Object, kafkaTopicName,
        null, raptorProxy.Object, null,
        projectRepo.Object);
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect projectUid");
      Assert.AreEqual(settings, result.settings, "executor returned incorrect settings");
      Assert.AreEqual(settingsType, result.projectSettingsType, "executor returned incorrect projectSettingsType");
    }

    [TestMethod]
    public void ProjectSettingsRequestShouldNotSerializeType()
    {
      string projectUid = Guid.NewGuid().ToString();
      string settings = "blah";

      var request = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, ProjectSettingsType.Targets);
      var json = JsonConvert.SerializeObject(request);
      Assert.IsFalse(json.Contains("ProjectSettingsType"));
    }

    [TestMethod]
    public void ProjectSettingsResultShouldNotSerializeType()
    {
      string projectUid = Guid.NewGuid().ToString();
      string settings = "blah";

      var result = ProjectSettingsResult.CreateProjectSettingsResult(projectUid, settings, ProjectSettingsType.Targets);
      var json = JsonConvert.SerializeObject(result);
      Assert.IsFalse(json.Contains("ProjectSettingsType"));
    }



  }
}
