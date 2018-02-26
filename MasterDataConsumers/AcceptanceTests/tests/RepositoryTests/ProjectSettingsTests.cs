using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryTests.Internal;
using System;
using System.Linq;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace RepositoryTests
{
  [TestClass]
  public class ProjectSettingsTests : TestControllerBase
  {
    ProjectRepository projectContext;

    [TestInitialize]
    public void Init()
    {
      SetupLogging();
      projectContext = new ProjectRepository(ServiceProvider.GetService<IConfigurationStore>(),
        ServiceProvider.GetService<ILoggerFactory>());
    }

    /// <summary>
    /// Create ProjectSettings
    ///   setting doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateProjectSettings_DoesntExist()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectUid = Guid.NewGuid();
      string settings = TargetJsonString;

      var createProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = projectUid,
        ProjectSettingsType = ProjectSettingsType.Targets,
        Settings = settings,
        UserID = Guid.NewGuid().ToString(),
        ActionUTC = actionUTC
      };
      
      var s = projectContext.StoreEvent(createProjectSettingsEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "ProjectSettings event not written");

      var g = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve settings from projectRepo");

      var projectSettingsList = g.Result.ToList();
      Assert.AreEqual(1, projectSettingsList.Count(), "Should be 1 and only 1 projectSetting");
      Assert.AreEqual(projectUid.ToString(), projectSettingsList[0].ProjectUid, "projectUid is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.ProjectSettingsType, projectSettingsList[0].ProjectSettingsType, "type is incorrect from projectRepo");
      Assert.AreEqual(settings, projectSettingsList[0].Settings, "settings is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.UserID, projectSettingsList[0].UserID, "UserID is incorrect from projectRepo");

      var single = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID, ProjectSettingsType.Targets);
      single.Wait();
      Assert.IsNotNull(single.Result, "Unable to retrieve individual settings from projectRepo");
      
      Assert.AreEqual(projectUid.ToString(), single.Result.ProjectUid, "projectUid is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.ProjectSettingsType, single.Result.ProjectSettingsType, "type is incorrect from projectRepo");
      Assert.AreEqual(settings, single.Result.Settings, "settings is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.UserID, single.Result.UserID, "UserID is incorrect from projectRepo");

      single = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID, ProjectSettingsType.Unknown);
      single.Wait();
      Assert.IsNull(single.Result, "Should be no unknown settings from projectRepo");

      single = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID, ProjectSettingsType.ImportedFiles);
      single.Wait();
      Assert.IsNull(single.Result, "Should be no ImportedFiles settings from projectRepo");
    }

    /// <summary>
    /// Update ProjectSettings 
    ///   settings exist already for the project
    /// </summary>
    [TestMethod]
    public void UpsertProjectSettings_Exists()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectUid = Guid.NewGuid();
      string settings = @"<ProjectSettings>  
        </ ProjectSettings > ";
      string settingsupdated = TargetJsonString;

      var createProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = projectUid,
        ProjectSettingsType = ProjectSettingsType.Targets,
        Settings = settings,
        UserID = Guid.NewGuid().ToString(),
        ActionUTC = actionUTC
      };

      var updatedProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = projectUid,
        ProjectSettingsType = createProjectSettingsEvent.ProjectSettingsType, 
        Settings = settingsupdated,
        UserID = createProjectSettingsEvent.UserID,
        ActionUTC = actionUTC.AddMilliseconds(2)
      };

      var s = projectContext.StoreEvent(createProjectSettingsEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "ProjectSettings event not written");

      s = projectContext.StoreEvent(updatedProjectSettingsEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "ProjectSettings event not updated");

      var g = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve settings from projectRepo");

      var projectSettingsList = g.Result.ToList();
      Assert.AreEqual(1, projectSettingsList.Count(), "Should be 1 and only 1 projectSetting");
      Assert.AreEqual(projectUid.ToString(), projectSettingsList[0].ProjectUid, "projectUid is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.ProjectSettingsType, projectSettingsList[0].ProjectSettingsType, "type is incorrect from projectRepo");
      Assert.AreEqual(settingsupdated, projectSettingsList[0].Settings, "settings is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.UserID, projectSettingsList[0].UserID, "UserID is incorrect from projectRepo");
    }

    /// <summary>
    /// Update ProjectSettings 
    ///   should not allow change of type or UserID
    /// </summary>
    [TestMethod]
    public void UpsertProjectSettings_CantChangeType()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectUid = Guid.NewGuid();
      string settings = @"<ProjectSettings>  
        </ ProjectSettings > ";
      string settingsupdated = TargetJsonString;

      var createProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = projectUid,
        ProjectSettingsType = ProjectSettingsType.Targets,
        Settings = settings,
        UserID = Guid.NewGuid().ToString(),
        ActionUTC = actionUTC
      };

      var updatedProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = projectUid,
        ProjectSettingsType = ProjectSettingsType.ImportedFiles,
        Settings = settingsupdated,
        UserID = createProjectSettingsEvent.UserID,
        ActionUTC = actionUTC.AddMilliseconds(2)
      };

      var s = projectContext.StoreEvent(createProjectSettingsEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "ProjectSettings event not written");

      s = projectContext.StoreEvent(updatedProjectSettingsEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "ProjectSettings event not updated");

      var g = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID, ProjectSettingsType.Targets);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve settings from projectRepo");

      var projectSettings = g.Result;
      Assert.AreEqual(projectUid.ToString(), projectSettings.ProjectUid, "projectUid is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.ProjectSettingsType, projectSettings.ProjectSettingsType, "type is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.Settings, projectSettings.Settings, "settings is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.UserID, projectSettings.UserID, "UserID is incorrect from projectRepo");

      var settingsList = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID);
      settingsList.Wait();
      Assert.IsNotNull(settingsList.Result, "Unable to retrieve settings from projectRepo");

      var projectSettingsList = settingsList.Result.ToList();
      Assert.AreEqual(2, projectSettingsList.Count(), "Should be 2 projectSettings");
    }

    /// <summary>
    /// Get ProjectSettings 
    ///   need to request for the correct user
    /// </summary>
    [TestMethod]
    public void CreateProjectSettings_WrongUser()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectUid = Guid.NewGuid();
      string settings = TargetJsonString;

      var createProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = projectUid,
        ProjectSettingsType = ProjectSettingsType.Targets,
        Settings = settings,
        UserID = Guid.NewGuid().ToString(),
        ActionUTC = actionUTC
      };

      var s = projectContext.StoreEvent(createProjectSettingsEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "ProjectSettings event not written");

      var g = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve settings from projectRepo");

      var projectSettingsList = g.Result.ToList();
      Assert.AreEqual(1, projectSettingsList.Count(), "Should be 1 and only 1 projectSetting");

      g = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), Guid.NewGuid().ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve settings from projectRepo");

      projectSettingsList = g.Result.ToList();
      Assert.AreEqual(0, projectSettingsList.Count(), "Should not be able to return another users ProjectSettings");


      var single = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID, ProjectSettingsType.Targets);
      single.Wait();
      Assert.IsNotNull(single.Result, "Unable to retrieve individual settings from projectRepo");

      single = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), Guid.NewGuid().ToString(), ProjectSettingsType.Targets);
      single.Wait();
      Assert.IsNull(single.Result, "Should not be able to return another users individual ProjectSettings");
      
    }

  }
}