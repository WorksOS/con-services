using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;

namespace ExecutorTests
{
  [TestClass]
  public class ProjectSettingsTests : ExecutorTestsBase
  {
    [TestMethod]
    public async Task GetProjectSettingsExecutor_NoSettingsExists()
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      string projectUid = Guid.NewGuid().ToString();
      string settings = "";
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings);

      var isCreatedOk = CreateCustomerProject(customerUid, projectUid);
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
          ( logger, configStore, serviceExceptionHandler,
            customerUid, userId, userEmailAddress, null,
            null, null,
            null, null, null,
            projectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;


      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect ProjectUid");
      Assert.IsNull(result.settings, "executor should have returned null Settings");
    }

    [TestMethod]
    public async Task GetProjectSettingsExecutor_Exists()
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      string projectUid = Guid.NewGuid().ToString();
      string settings = "blah";

      var isCreatedOk = CreateCustomerProject(customerUid, projectUid);
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");

      isCreatedOk = CreateProjectSettings(projectUid, settings);
      Assert.IsTrue(isCreatedOk, "created projectSettings");
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings);

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
          customerUid, userId, userEmailAddress, null,
          null, null,
          null, null, null,
          projectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settings, result.settings, "executor returned incorrect Settings");
    }

    [TestMethod]
    public async Task UpsertProjectSettingsExecutor_NoneExists()
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      var projectUid = Guid.NewGuid();
      string settings = "blah";
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settings);
      string kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                              configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");

      var isCreatedOk = CreateCustomerProject(customerUid, projectUid.ToString());
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");


      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
        customerUid, userId, userEmailAddress, null,
        producer, kafkaTopicName,
        null, null, null,
        projectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settings, result.settings, "executor returned incorrect Settings");
    }

    [TestMethod]
    public async Task UpsertProjectSettingsExecutor_Exists()
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      var projectUid = Guid.NewGuid();
      string settings = "blah";
      string settingsUpdated = "blah Is Updated";

      var isCreatedOk = CreateCustomerProject(customerUid, projectUid.ToString());
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");

      isCreatedOk = CreateProjectSettings(projectUid.ToString(), settings);
      Assert.IsTrue(isCreatedOk, "created projectSettings");

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settingsUpdated);

      string kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                              configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, userId, userEmailAddress, null,
        producer, kafkaTopicName,
        null, null, null,
        projectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settingsUpdated, result.settings, "executor returned incorrect Settings");
    }

    [TestMethod]
    public async Task ValidateProjectSettings_Failure()
    {
      //var projectUid = Guid.NewGuid();
      //string settings = "blah";
      //var log = logger.CreateLogger<ProjectSettingsTests>();

      //var projectSettingsRequest =
      //  ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settings);
      //await ProjectSettingsValidation.RaptorValidateProjectSettings(raptorProxy, log,
      //  serviceExceptionHandler, projectSettingsRequest, customHeaders);
      // todo
    }
  }
}

