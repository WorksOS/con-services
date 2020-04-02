using System;
using System.Linq;
using RepositoryTests.Internal;
using VSS.Productivity3D.Project.Repository;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using Xunit;

namespace RepositoryTests
{
  public class ProjectSettingsTests : TestControllerBase
  {
    ProjectRepository projectRepo;
    public ProjectSettingsTests()
    {
      SetupLogging();
      projectRepo = new ProjectRepository(configStore, loggerFactory);
    }

    /// <summary>
    /// Create ProjectSettings
    ///   setting doesn't exist already.
    /// </summary>
    [Fact]
    public void CreateProjectSettings_DoesntExist()
    {
      var actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectUid = Guid.NewGuid().ToString();
      string settings = TargetJsonString;

      var createProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = projectUid,
        ProjectSettingsType = ProjectSettingsType.Targets,
        Settings = settings,
        UserID = Guid.NewGuid().ToString(),
        ActionUTC = actionUTC
      };
      
      var s = projectRepo.StoreEvent(createProjectSettingsEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = projectRepo.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID);
      g.Wait();
      Assert.NotNull(g.Result);

      var projectSettingsList = g.Result.ToList();
      Assert.Single(projectSettingsList);
      Assert.Equal(projectUid.ToString(), projectSettingsList[0].ProjectUid);
      Assert.Equal(createProjectSettingsEvent.ProjectSettingsType, projectSettingsList[0].ProjectSettingsType);
      Assert.Equal(settings, projectSettingsList[0].Settings);
      Assert.Equal(createProjectSettingsEvent.UserID, projectSettingsList[0].UserID);

      var single = projectRepo.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID, ProjectSettingsType.Targets);
      single.Wait();
      Assert.NotNull(single.Result);

      Assert.Equal(projectUid.ToString(), single.Result.ProjectUid);
      Assert.Equal(createProjectSettingsEvent.ProjectSettingsType, single.Result.ProjectSettingsType);
      Assert.Equal(settings, single.Result.Settings);
      Assert.Equal(createProjectSettingsEvent.UserID, single.Result.UserID);

      single = projectRepo.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID, ProjectSettingsType.Unknown);
      single.Wait();
      Assert.Null(single.Result);

      single = projectRepo.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID, ProjectSettingsType.ImportedFiles);
      single.Wait();
      Assert.Null(single.Result);
    }

    /// <summary>
    /// Update ProjectSettings 
    ///   settings exist already for the project
    /// </summary>
    [Fact]
    public void UpsertProjectSettings_Exists()
    {
      var actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectUid = Guid.NewGuid().ToString();
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

      var s = projectRepo.StoreEvent(createProjectSettingsEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      s = projectRepo.StoreEvent(updatedProjectSettingsEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = projectRepo.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID);
      g.Wait();
      Assert.NotNull(g.Result);

      var projectSettingsList = g.Result.ToList();
      Assert.Single(projectSettingsList);
      Assert.Equal(projectUid.ToString(), projectSettingsList[0].ProjectUid);
      Assert.Equal(createProjectSettingsEvent.ProjectSettingsType, projectSettingsList[0].ProjectSettingsType);
      Assert.Equal(settingsupdated, projectSettingsList[0].Settings);
      Assert.Equal(createProjectSettingsEvent.UserID, projectSettingsList[0].UserID);
    }

    /// <summary>
    /// Update ProjectSettings 
    ///   should not allow change of type or UserID
    /// </summary>
    [Fact]
    public void UpsertProjectSettings_CantChangeType()
    {
      var actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectUid = Guid.NewGuid().ToString();
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

      var s = projectRepo.StoreEvent(createProjectSettingsEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      s = projectRepo.StoreEvent(updatedProjectSettingsEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = projectRepo.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID, ProjectSettingsType.Targets);
      g.Wait();
      Assert.NotNull(g.Result);

      var projectSettings = g.Result;
      Assert.Equal(projectUid.ToString(), projectSettings.ProjectUid);
      Assert.Equal(createProjectSettingsEvent.ProjectSettingsType, projectSettings.ProjectSettingsType);
      Assert.Equal(createProjectSettingsEvent.Settings, projectSettings.Settings);
      Assert.Equal(createProjectSettingsEvent.UserID, projectSettings.UserID);

      var settingsList = projectRepo.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID);
      settingsList.Wait();
      Assert.NotNull(settingsList.Result);

      var projectSettingsList = settingsList.Result.ToList();
      Assert.Equal(2, projectSettingsList.Count());
    }

    /// <summary>
    /// Get ProjectSettings 
    ///   need to request for the correct user
    /// </summary>
    [Fact]
    public void CreateProjectSettings_WrongUser()
    {
      var actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectUid = Guid.NewGuid().ToString();
      string settings = TargetJsonString;

      var createProjectSettingsEvent = new UpdateProjectSettingsEvent()
      {
        ProjectUID = projectUid,
        ProjectSettingsType = ProjectSettingsType.Targets,
        Settings = settings,
        UserID = Guid.NewGuid().ToString(),
        ActionUTC = actionUTC
      };

      var s = projectRepo.StoreEvent(createProjectSettingsEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = projectRepo.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID);
      g.Wait();
      Assert.NotNull(g.Result);

      var projectSettingsList = g.Result.ToList();
      Assert.Single(projectSettingsList);

      g = projectRepo.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), Guid.NewGuid().ToString());
      g.Wait();
      Assert.NotNull(g.Result);

      projectSettingsList = g.Result.ToList();
      Assert.Empty(projectSettingsList);


      var single = projectRepo.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), createProjectSettingsEvent.UserID, ProjectSettingsType.Targets);
      single.Wait();
      Assert.NotNull(single.Result);

      single = projectRepo.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), Guid.NewGuid().ToString(), ProjectSettingsType.Targets);
      single.Wait();
      Assert.Null(single.Result);

    }

  }
}
