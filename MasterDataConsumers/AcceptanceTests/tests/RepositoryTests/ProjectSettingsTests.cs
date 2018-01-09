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
    public void UpsertProjectSettings_NotExisting()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectUid = Guid.NewGuid();
      string settings = @"<ProjectSettings>  
        < CompactionSettings >
        < OverrideTargetCMV > false </ OverrideTargetCMV >
        < OverrideTargetCMVValue > 50 </ OverrideTargetCMVValue >
        < MinTargetCMVPercent > 80 </ MinTargetCMVPercent >
        < MaxTargetCMVPercent > 130 </ MaxTargetCMVPercent >
        < OverrideTargetPassCount > false </ OverrideTargetPassCount >
        < OverrideTargetPassCountValue > 5 </ OverrideTargetPassCountValue >
        < OverrideTargetLiftThickness > false </ OverrideTargetLiftThickness >
        < OverrideTargetLiftThicknessMeters > 0.5 </ OverrideTargetLiftThicknessMeters >
        < CompactedLiftThickness > true </ CompactedLiftThickness >
        < ShowCCVSummaryTopLayerOnly > true </ ShowCCVSummaryTopLayerOnly >
        < FirstPassThickness > 0 </ FirstPassThickness >
        < OverrideTemperatureRange > false </ OverrideTemperatureRange >
        < MinTemperatureRange > 65 </ MinTemperatureRange >
        < MaxTemperatureRange > 175 </ MaxTemperatureRange >
        < OverrideTargetMDP > false </ OverrideTargetMDP >
        < OverrideTargetMDPValue > 50 </ OverrideTargetMDPValue >
        < MinTargetMDPPercent > 80 </ MinTargetMDPPercent >
        < MaxTargetMDPPercent > 130 </ MaxTargetMDPPercent >
        < ShowMDPSummaryTopLayerOnly > true </ ShowMDPSummaryTopLayerOnly >
        </ CompactionSettings >
        < VolumeSettings >
        < ApplyShrinkageAndBulking > false </ ApplyShrinkageAndBulking >
        < PercentShrinkage > 0 </ PercentShrinkage >
        < PercentBulking > 0 </ PercentBulking >
        < NoChangeTolerance > 0.02 </ NoChangeTolerance >
        </ VolumeSettings >
        < ExpiryPromptDismissed > false </ ExpiryPromptDismissed >
        </ ProjectSettings > ";

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

      var g = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve settings from projectRepo");

      var projectSettingsList = g.Result.ToList();
      Assert.AreEqual(1, projectSettingsList.Count(), "Should be 1 and only 1 projectSetting");
      Assert.AreEqual(projectUid.ToString(), projectSettingsList[0].ProjectUid, "projectUid is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.ProjectSettingsType, projectSettingsList[0].ProjectSettingsType, "type is incorrect from projectRepo");
      Assert.AreEqual(settings, projectSettingsList[0].Settings, "settings is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.UserID, projectSettingsList[0].UserID, "UserID is incorrect from projectRepo");

      var single = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), (int)ProjectSettingsType.Targets);
      single.Wait();
      Assert.IsNotNull(single.Result, "Unable to retrieve individual settings from projectRepo");
      
      Assert.AreEqual(projectUid.ToString(), single.Result.ProjectUid, "projectUid is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.ProjectSettingsType, single.Result.ProjectSettingsType, "type is incorrect from projectRepo");
      Assert.AreEqual(settings, single.Result.Settings, "settings is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.UserID, single.Result.UserID, "UserID is incorrect from projectRepo");

      single = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), (int)ProjectSettingsType.Unknown);
      single.Wait();
      Assert.IsNull(single.Result, "Should be no unknown settings from projectRepo");

      single = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString(), (int)ProjectSettingsType.ImportedFiles);
      single.Wait();
      Assert.IsNull(single.Result, "Should be no ImportedFiles settings from projectRepo");
    }

    /// <summary>
    /// Update ProjectSettings 
    ///   settings exist already for the project
    /// </summary>
    [TestMethod]
    public void UpsertProjectSettings_UpdateExisting()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectUid = Guid.NewGuid();
      string settings = @"<ProjectSettings>  
        </ ProjectSettings > ";
      string settingsupdated = @"<ProjectSettings>  
        < CompactionSettings >
        < OverrideTargetCMV > false </ OverrideTargetCMV >
        < OverrideTargetCMVValue > 50 </ OverrideTargetCMVValue >
        < MinTargetCMVPercent > 80 </ MinTargetCMVPercent >
        < MaxTargetCMVPercent > 130 </ MaxTargetCMVPercent >
        < OverrideTargetPassCount > false </ OverrideTargetPassCount >
        < OverrideTargetPassCountValue > 5 </ OverrideTargetPassCountValue >
        < OverrideTargetLiftThickness > false </ OverrideTargetLiftThickness >
        < OverrideTargetLiftThicknessMeters > 0.5 </ OverrideTargetLiftThicknessMeters >
        < CompactedLiftThickness > true </ CompactedLiftThickness >
        < ShowCCVSummaryTopLayerOnly > true </ ShowCCVSummaryTopLayerOnly >
        < FirstPassThickness > 0 </ FirstPassThickness >
        < OverrideTemperatureRange > false </ OverrideTemperatureRange >
        < MinTemperatureRange > 65 </ MinTemperatureRange >
        < MaxTemperatureRange > 175 </ MaxTemperatureRange >
        < OverrideTargetMDP > false </ OverrideTargetMDP >
        < OverrideTargetMDPValue > 50 </ OverrideTargetMDPValue >
        < MinTargetMDPPercent > 80 </ MinTargetMDPPercent >
        < MaxTargetMDPPercent > 130 </ MaxTargetMDPPercent >
        < ShowMDPSummaryTopLayerOnly > true </ ShowMDPSummaryTopLayerOnly >
        </ CompactionSettings >
        < VolumeSettings >
        < ApplyShrinkageAndBulking > false </ ApplyShrinkageAndBulking >
        < PercentShrinkage > 0 </ PercentShrinkage >
        < PercentBulking > 0 </ PercentBulking >
        < NoChangeTolerance > 0.02 </ NoChangeTolerance >
        </ VolumeSettings >
        < ExpiryPromptDismissed > false </ ExpiryPromptDismissed >
        </ ProjectSettings > ";

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

      var g = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString());
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
    public void UpsertProjectSettings_UpdateExisting_CantChangeType()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectUid = Guid.NewGuid();
      string settings = @"<ProjectSettings>  
        </ ProjectSettings > ";
      string settingsupdated = @"<ProjectSettings>  
        < CompactionSettings >
        < OverrideTargetCMV > false </ OverrideTargetCMV >
        < OverrideTargetCMVValue > 50 </ OverrideTargetCMVValue >
        < MinTargetCMVPercent > 80 </ MinTargetCMVPercent >
        < MaxTargetCMVPercent > 130 </ MaxTargetCMVPercent >
        < OverrideTargetPassCount > false </ OverrideTargetPassCount >
        < OverrideTargetPassCountValue > 5 </ OverrideTargetPassCountValue >
        < OverrideTargetLiftThickness > false </ OverrideTargetLiftThickness >
        < OverrideTargetLiftThicknessMeters > 0.5 </ OverrideTargetLiftThicknessMeters >
        < CompactedLiftThickness > true </ CompactedLiftThickness >
        < ShowCCVSummaryTopLayerOnly > true </ ShowCCVSummaryTopLayerOnly >
        < FirstPassThickness > 0 </ FirstPassThickness >
        < OverrideTemperatureRange > false </ OverrideTemperatureRange >
        < MinTemperatureRange > 65 </ MinTemperatureRange >
        < MaxTemperatureRange > 175 </ MaxTemperatureRange >
        < OverrideTargetMDP > false </ OverrideTargetMDP >
        < OverrideTargetMDPValue > 50 </ OverrideTargetMDPValue >
        < MinTargetMDPPercent > 80 </ MinTargetMDPPercent >
        < MaxTargetMDPPercent > 130 </ MaxTargetMDPPercent >
        < ShowMDPSummaryTopLayerOnly > true </ ShowMDPSummaryTopLayerOnly >
        </ CompactionSettings >
        < VolumeSettings >
        < ApplyShrinkageAndBulking > false </ ApplyShrinkageAndBulking >
        < PercentShrinkage > 0 </ PercentShrinkage >
        < PercentBulking > 0 </ PercentBulking >
        < NoChangeTolerance > 0.02 </ NoChangeTolerance >
        </ VolumeSettings >
        < ExpiryPromptDismissed > false </ ExpiryPromptDismissed >
        </ ProjectSettings > ";

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
        UserID = Guid.NewGuid().ToString(),
        ActionUTC = actionUTC.AddMilliseconds(2)
      };

      var s = projectContext.StoreEvent(createProjectSettingsEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "ProjectSettings event not written");

      s = projectContext.StoreEvent(updatedProjectSettingsEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "ProjectSettings event not updated");

      var g = projectContext.GetProjectSettings(createProjectSettingsEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve settings from projectRepo");

      var projectSettingsList = g.Result.ToList();
      Assert.AreEqual(1, projectSettingsList.Count(), "Should be 1 and only 1 projectSetting");
      Assert.AreEqual(projectUid.ToString(), projectSettingsList[0].ProjectUid, "projectUid is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.ProjectSettingsType, projectSettingsList[0].ProjectSettingsType, "type is incorrect from projectRepo");
      Assert.AreEqual(settingsupdated, projectSettingsList[0].Settings, "settings is incorrect from projectRepo");
      Assert.AreEqual(createProjectSettingsEvent.UserID, projectSettingsList[0].UserID, "UserID is incorrect from projectRepo");
    }

  }
}