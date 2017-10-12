using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;

namespace ExecutorTests
{
  [TestClass]
  public class ProjectSettingsTests : ExecutorTestsBase
  {
    [TestMethod]
    public async Task GetProjectSettingsExecutor_InvalidProjectUid()
    {
      string projectUid = "";
      string settings = "";
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings);
      var ex = Assert.ThrowsException<ServiceException>(() => projectSettingsRequest.Validate());
      Assert.AreNotEqual(-1, ex.Content.IndexOf("2005", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.Content.IndexOf("Missing ProjectUID.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    public async Task GetProjectSettingsExecutor_InvalidCustomerProjectRelationship()
    {
      string customerUidOfProject = Guid.NewGuid().ToString();
      string customerUidSomeOther = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      string projectUid = Guid.NewGuid().ToString();
      string settings = "";
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings);

      var isCreatedOk = CreateCustomerProject(customerUidOfProject, projectUid);
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
          customerUidSomeOther, userId, userEmailAddress, null,
          null, null,
          null, null, null,
          projectRepo);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(projectSettingsRequest)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.Content.IndexOf("2001", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.Content.IndexOf("No access to the project for a customer or the project does not exist.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

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
    public async Task UpsertProjectSettingsExecutor_InvalidProjectSettings()
    {
      string projectUid = Guid.NewGuid().ToString();
      string settings = null;
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings);
      var ex = Assert.ThrowsException<ServiceException>(() => projectSettingsRequest.Validate());
      Assert.AreNotEqual(-1, ex.Content.IndexOf("2073", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.Content.IndexOf("ProjectSettings cannot be null.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    public async Task UpsertProjectSettingsExecutor_InvalidCustomerProjectRelationship()
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

      isCreatedOk = CreateProjectSettings(projectUid.ToString(), settings);
      Assert.IsTrue(isCreatedOk, "created projectSettings");

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settingsUpdated);

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUidSomeOther, userId, userEmailAddress, null,
        producer, kafkaTopicName,
        null, raptorProxy, null,
        projectRepo);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(projectSettingsRequest)).ConfigureAwait(false);
      Assert.AreNotEqual(-1, ex.Content.IndexOf("2001", StringComparison.Ordinal), "executor threw exception but incorrect code");
      Assert.AreNotEqual(-1, ex.Content.IndexOf("No access to the project for a customer or the project does not exist.", StringComparison.Ordinal), "executor threw exception but incorrect messaage");
    }

    [TestMethod]
    public async Task UpsertProjectSettingsExecutor_NoProjectSettingsExists()
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      var projectUid = Guid.NewGuid();
      string settings = "blah";
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settings);

      var isCreatedOk = CreateCustomerProject(customerUid, projectUid.ToString());
      Assert.IsTrue(isCreatedOk, "unable to create project for Customer");
      
      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
        customerUid, userId, userEmailAddress, null,
        producer, kafkaTopicName,
        null, raptorProxy, null,
        projectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid.ToString(), result.projectUid, "executor returned incorrect ProjectUid");
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

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, userId, userEmailAddress, null,
        producer, kafkaTopicName,
        null, raptorProxy, null,
        projectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid.ToString(), result.projectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settingsUpdated, result.settings, "executor returned incorrect Settings");
    }
   
  }
}

