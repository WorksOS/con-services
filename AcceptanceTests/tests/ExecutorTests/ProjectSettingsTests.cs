using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
    [TestClass]
  public class ProjectSettingsTests : ExecutorTestsBase
  {
    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    public async Task GetProjectSettingsExecutor_InvalidProjectUid(ProjectSettingsType settingsType)
    {
      string projectUid = string.Empty;
      string settings = string.Empty;
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);
      var ex = Assert.ThrowsException<ServiceException>(() => projectSettingsRequest.Validate());
      Assert.AreNotEqual(-1, ex.Content.IndexOf("2005", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.Content.IndexOf("Missing ProjectUID.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    //[DataRow(ProjectSettingsType.ImportedFiles)]
    [DataRow(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_InvalidCustomerProjectRelationship(ProjectSettingsType settingsType)
    {
      string customerUidOfProject = Guid.NewGuid().ToString();
      string customerUidSomeOther = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      string projectUid = Guid.NewGuid().ToString();
      string settings = string.Empty;
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);

      var isCreatedOk = CreateCustomerProject(customerUidOfProject, projectUid);
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
          customerUidSomeOther, userId, userEmailAddress, CustomHeaders(customerUidOfProject),
          null, null,
          null, null, null,
          projectRepo);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(projectSettingsRequest)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.Content.IndexOf("2001", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.Content.IndexOf("No access to the project for a customer or the project does not exist.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    //[DataRow(ProjectSettingsType.ImportedFiles)]
    [DataRow(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_NoSettingsExists(ProjectSettingsType settingsType)
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      string projectUid = Guid.NewGuid().ToString();
      string settings = string.Empty;
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);

      var isCreatedOk = CreateCustomerProject(customerUid, projectUid);
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
          ( logger, configStore, serviceExceptionHandler,
            customerUid, userId, userEmailAddress, CustomHeaders(customerUid),
            null, null,
            null, null, null,
            projectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect ProjectUid");
      Assert.IsNull(result.settings, "executor should have returned null Settings");
      Assert.AreEqual(settingsType, result.projectSettingsType, $"executor should have returned {settingsType} SettingsType");
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    public async Task GetProjectSettingsExecutor_Exists(ProjectSettingsType settingsType)
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      string projectUid = Guid.NewGuid().ToString();
      string settings = "blah";

      var isCreatedOk = CreateCustomerProject(customerUid, projectUid);
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");

      isCreatedOk = CreateProjectSettings(projectUid, userId, settings, settingsType);
      Assert.IsTrue(isCreatedOk, "created projectSettings");
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
          customerUid, userId, userEmailAddress, CustomHeaders(customerUid),
          null, null,
          null, null, null,
          projectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settings, result.settings, "executor returned incorrect Settings");
      Assert.AreEqual(settingsType, result.projectSettingsType, "executor returned incorrect projectSettingsType");
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    public async Task UpsertProjectSettingsExecutor_InvalidProjectSettings(ProjectSettingsType settingsType)
    {
      string projectUid = Guid.NewGuid().ToString();
      string settings = null;
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);
      var ex = Assert.ThrowsException<ServiceException>(() => projectSettingsRequest.Validate());
      Assert.AreNotEqual(-1, ex.Content.IndexOf("2073", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.Content.IndexOf("ProjectSettings cannot be null.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    public async Task UpsertProjectSettingsExecutor_InvalidCustomerProjectRelationship(ProjectSettingsType settingsType)
    {
      string customerUidOfProject = Guid.NewGuid().ToString();
      string customerUidSomeOther = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      var projectUid = Guid.NewGuid();
      string settings = "blah";
      string settingsUpdated = "blah Is Updated";

      var isCreatedOk = CreateCustomerProject(customerUidOfProject, projectUid.ToString());
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");

      isCreatedOk = CreateProjectSettings(projectUid.ToString(), userId, settings, settingsType);
      Assert.IsTrue(isCreatedOk, "created projectSettings");

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settingsUpdated, settingsType);

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUidSomeOther, userId, userEmailAddress, CustomHeaders(customerUidOfProject),
        producer, kafkaTopicName,
        null, raptorProxy, null,
        projectRepo);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(projectSettingsRequest)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.Content.IndexOf("2001", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.Content.IndexOf("No access to the project for a customer or the project does not exist.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    public async Task UpsertProjectSettingsExecutor_NoProjectSettingsExists(ProjectSettingsType settingsType)
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      var projectUid = Guid.NewGuid();
      string settings = "blah";
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settings, settingsType);

      var isCreatedOk = CreateCustomerProject(customerUid, projectUid.ToString());
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");
      
      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
        customerUid, userId, userEmailAddress, CustomHeaders(customerUid),
        producer, kafkaTopicName,
        null, raptorProxy, null,
        projectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid.ToString(), result.projectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settings, result.settings, "executor returned incorrect Settings");
      Assert.AreEqual(settingsType, result.projectSettingsType, "executor returned incorrect projectSettingsType");
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    public async Task UpsertProjectSettingsExecutor_Exists(ProjectSettingsType settingsType)
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      var projectUid = Guid.NewGuid();
      string settings = "blah";
      string settingsUpdated = "blah Is Updated";

      var isCreatedOk = CreateCustomerProject(customerUid, projectUid.ToString());
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");

      isCreatedOk = CreateProjectSettings(projectUid.ToString(), userId, settings, settingsType);
      Assert.IsTrue(isCreatedOk, "created projectSettings");

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settingsUpdated, settingsType);

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, userId, userEmailAddress, CustomHeaders(customerUid),
        producer, kafkaTopicName,
        null, raptorProxy, null,
        projectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid.ToString(), result.projectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settingsUpdated, result.settings, "executor returned incorrect Settings");
      Assert.AreEqual(settingsType, result.projectSettingsType, "executor returned incorrect projectSettingsType");
    }
  }
}