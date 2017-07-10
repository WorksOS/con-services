using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositories.DBModels;
using System;
using System.Net;
using System.Threading.Tasks;
using TestUtility;
using VSS.Productivity3D.ProjectWebApiCommon.Executors;
using VSS.Productivity3D.ProjectWebApiCommon.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace ExecutorTests
{
  [TestClass]
  public class ProjectSettingsTests : ExecutorTestsBase
  {

    [TestMethod]
    public async Task GetProjectSettingsExecutor_NonExistingProjectUid()
    {
      string projectUid = Guid.NewGuid().ToString();
      string settings = "blah";
     
      var executor = RequestExecutorContainer.Build<GetProjectSettingsExecutor>(projectRepo, configStore, logger, serviceExceptionHandler);
      var result = await executor.ProcessAsync(projectUid) as ProjectSettingsResult;
      //Assert.IsNotNull(result, "executor returned nothing");
      //Assert.AreNotEqual(projectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      //Assert.IsNotNull(result.Settings, "executor returned incorrect Settings");
    }

    [TestMethod]
    public async Task GetProjectSettingsExecutor_ExistingProjectUid()
    {
      string projectUid = Guid.NewGuid().ToString();
      string settings = "blah";

      var isCreatedOk = CreateProjectSettings(projectUid, settings);
      Assert.IsTrue(isCreatedOk, "created projectSettings association");

      var executor = RequestExecutorContainer.Build<GetProjectSettingsExecutor>(projectRepo, configStore, logger, serviceExceptionHandler);
      var result = await executor.ProcessAsync(projectUid) as ProjectSettingsResult;
      //Assert.IsNotNull(result, "executor returned nothing");
      //Assert.AreEqual(projectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      //Assert.IsNotNull(result.Settings, "executor returned incorrect Settings");
    }
  }
}

