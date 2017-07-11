using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.Productivity3D.ProjectWebApiCommon.Executors;
using VSS.Productivity3D.ProjectWebApiCommon.Models;
using VSS.Productivity3D.ProjectWebApiCommon.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace ExecutorTests
{
  [TestClass]
  public class ProjectSettingsTests : ExecutorTestsBase
  {

    [TestMethod]
    public async Task GetProjectSettingsExecutor_NoneExists()
    {
      string projectUid = Guid.NewGuid().ToString();
 
      var executor = RequestExecutorContainer.Build<GetProjectSettingsExecutor>(projectRepo, raptorProxy, configStore, logger, serviceExceptionHandler, customHeaders, producer);
      var result = await executor.ProcessAsync(projectUid) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.IsNull(result.Settings, "executor should have returned null Settings");
    }

    [TestMethod]
    public async Task GetProjectSettingsExecutor_Exists()
    {
      string projectUid = Guid.NewGuid().ToString();
      string settings = "blah";

      var isCreatedOk = CreateProjectSettings(projectUid, settings);
      Assert.IsTrue(isCreatedOk, "created projectSettings");

      var executor = RequestExecutorContainer.Build<GetProjectSettingsExecutor>(projectRepo, raptorProxy, configStore, logger, serviceExceptionHandler, customHeaders, producer);
      var result = await executor.ProcessAsync(projectUid) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settings, result.Settings, "executor returned incorrect Settings");
    }

    [TestMethod]
    public async Task UpsertProjectSettingsExecutor_NoneExists()
    {
      var projectUid = Guid.NewGuid();
      string settings = "blah";
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settings);
      string kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" + configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");

      var executor = RequestExecutorContainer.Build<UpsertProjectSettingsExecutor>(projectRepo, raptorProxy, configStore, logger, serviceExceptionHandler, customHeaders, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settings, result.Settings, "executor returned incorrect Settings");
    }

    [TestMethod]
    public async Task UpsertProjectSettingsExecutor_Exists()
    {
      var projectUid = Guid.NewGuid();
      string settings = "blah";
      string settingsUpdated = "blah Is Updated";

      var isCreatedOk = CreateProjectSettings(projectUid.ToString(), settings);
      Assert.IsTrue(isCreatedOk, "created projectSettings");

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settingsUpdated);
      var executor = RequestExecutorContainer.Build<UpsertProjectSettingsExecutor>(projectRepo, raptorProxy, configStore, logger, serviceExceptionHandler, customHeaders, producer);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settingsUpdated, result.Settings, "executor returned incorrect Settings");
    }
  }
}

