using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositories;
using RepositoryTests.Internal;
using System;
using System.Linq;
using VSS.GenericConfiguration;
using VSS.Productivity3D.Repo;
using VSS.Productivity3D.Repo.ExtendedModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace RepositoryTests
{
  [TestClass]
  public class ProjectImportedFileRepositoryTests : TestControllerBase
  {
    ProjectRepository _projectContext;

    [TestInitialize]
    public void Init()
    {
      SetupLogging();

      new CustomerRepository(ServiceProvider.GetService<IConfigurationStore>(), ServiceProvider.GetService<ILoggerFactory>());
      _projectContext = new ProjectRepository(ServiceProvider.GetService<IConfigurationStore>(), ServiceProvider.GetService<ILoggerFactory>());
      new SubscriptionRepository(ServiceProvider.GetService<IConfigurationStore>(), ServiceProvider.GetService<ILoggerFactory>());
    }


    #region ImportedFiles

    /// <summary>
    /// Create ImportedFile - Happy path i.e. 
    ///   ImportedFile doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateImportedFile_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createImportedFileEvent = new CreateImportedFileEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999),
        CustomerUID = Guid.NewGuid(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };

      var s = _projectContext.StoreEvent(createImportedFileEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "ImportedFile event not written");

      var g = _projectContext.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve ImportedFile from ProjectRepo");

      var gList = _projectContext.GetImportedFiles(createImportedFileEvent.ProjectUID.ToString());
      gList.Wait();
      Assert.IsNotNull(gList.Result, "Unable to retrieve ImportedFiles from ProjectRepo");
      Assert.AreEqual(1, gList.Result.Count(), "ImportedFile count is incorrect from ProjectRepo");
    }

    /// <summary>
    /// Create ImportedFile - HappyPath 
    ///   ImportedFiles for multiple projects, should only return those for requested project.
    /// </summary>
    [TestMethod]
    public void CreateImportedFile_MultiProjects()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();
      var project1 = Guid.NewGuid();
      var project2 = Guid.NewGuid();
      var project3 = Guid.NewGuid();

      var createImportedFileEventP1_1 = new CreateImportedFileEvent()
      {
        ProjectUID = project1,
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999),
        CustomerUID = customerUid,
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };

      var createImportedFileEventP1_2 = new CreateImportedFileEvent()
      {
        ProjectUID = project1,
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999) + 1,
        CustomerUID = customerUid,
        ImportedFileType = ImportedFileType.Alignment,
        Name = "Test alginment type.svl",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };

      var createImportedFileEventP2_1 = new CreateImportedFileEvent()
      {
        ProjectUID = project2,
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999) + 2,
        CustomerUID = customerUid,
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };


      _projectContext.StoreEvent(createImportedFileEventP1_1).Wait();
      _projectContext.StoreEvent(createImportedFileEventP1_2).Wait();
      _projectContext.StoreEvent(createImportedFileEventP2_1).Wait();

      var g = _projectContext.GetImportedFile(createImportedFileEventP1_2.ImportedFileUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve ImportedFile from ProjectRepo");
      Assert.IsNotNull(g.Result.FileDescriptor, "Unable to retrieve fileDescriptor from ImportedFile");

      var gList = _projectContext.GetImportedFiles(project1.ToString());
      gList.Wait();
      Assert.IsNotNull(gList.Result, "Unable to retrieve ImportedFiles for Project1 from ProjectRepo");
      Assert.AreEqual(2, gList.Result.Count(), "ImportedFile count is incorrect for Project1 from ProjectRepo");

      gList = _projectContext.GetImportedFiles(project2.ToString());
      gList.Wait();
      Assert.IsNotNull(gList.Result, "Unable to retrieve ImportedFiles for Project2 from ProjectRepo");
      Assert.AreEqual(1, gList.Result.Count(), "ImportedFile count is incorrect for Project2 from ProjectRepo");

      gList = _projectContext.GetImportedFiles(project3.ToString());
      gList.Wait();
      Assert.IsNotNull(gList.Result, "Unable to retrieve ImportedFiles for Project3 from ProjectRepo");
      Assert.AreEqual(0, gList.Result.Count(), "ImportedFile count is incorrect for Project3 from ProjectRepo");
    }

    /// <summary>
    /// Update ImportedFile - Happy path i.e. 
    ///   ImportedFile exists already.
    /// </summary>
    [TestMethod]
    public void UpdateImportedFile_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createImportedFileEvent = new CreateImportedFileEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999),
        CustomerUID = Guid.NewGuid(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };

      _projectContext.StoreEvent(createImportedFileEvent).Wait();

      var updateImportedFileEvent = new UpdateImportedFileEvent()
      {
        ProjectUID = createImportedFileEvent.ProjectUID,
        ImportedFileUID = createImportedFileEvent.ImportedFileUID,
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe2",
        ActionUTC = actionUtc.AddHours(1)
      };

      _projectContext.StoreEvent(updateImportedFileEvent).Wait();

      var g = _projectContext.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve ImportedFile from ProjectRepo");
      Assert.AreEqual(updateImportedFileEvent.ActionUTC, g.Result.LastActionedUtc, "ImportedFile actionUtc was not updated");
      Assert.AreEqual(updateImportedFileEvent.FileDescriptor, g.Result.FileDescriptor, "ImportedFile FileDescriptor was not updated");
    }

    /// <summary>
    /// Delete ImportedFile - Happy path i.e. 
    ///   ImportedFile exists already.
    /// </summary>
    [TestMethod]
    public void DeleteImportedFile_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createImportedFileEvent = new CreateImportedFileEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999),
        CustomerUID = Guid.NewGuid(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };

      _projectContext.StoreEvent(createImportedFileEvent).Wait();

      var deleteImportedFileEvent = new DeleteImportedFileEvent()
      {
        ProjectUID = createImportedFileEvent.ProjectUID,
        ImportedFileUID = createImportedFileEvent.ImportedFileUID,
        ActionUTC = actionUtc.AddHours(1)
      };

      _projectContext.StoreEvent(deleteImportedFileEvent).Wait();

      var g = _projectContext.GetImportedFile(createImportedFileEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve ImportedFile from ProjectRepo");

      g = _projectContext.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Should be able to retrieve ImportedFile from ProjectRepo via GUID ");
      Assert.IsTrue(g.Result.IsDeleted, "Should be able to retrieve deleted flag via GUID ");
    }

    /// <summary>
    /// Delete ImportedFile permanently
    /// </summary>
    [TestMethod]
    public void DeleteImportedFile_Permanently()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createImportedFileEvent = new CreateImportedFileEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999),
        CustomerUID = Guid.NewGuid(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };

      _projectContext.StoreEvent(createImportedFileEvent).Wait();

      var deleteImportedFileEvent = new DeleteImportedFileEvent()
      {
        ProjectUID = createImportedFileEvent.ProjectUID,
        ImportedFileUID = createImportedFileEvent.ImportedFileUID,
        DeletePermanently = true,
        ActionUTC = actionUtc.AddHours(1)
      };

      _projectContext.StoreEvent(deleteImportedFileEvent).Wait();

      var g = _projectContext.GetImportedFile(createImportedFileEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve ImportedFile from ProjectRepo");

      g = _projectContext.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should be able to retrieve ImportedFile from ProjectRepo via GUID ");
    }

    /// <summary>
    /// UnDelete ImportedFile used internally by ProjectMDM for rolling back after failure
    /// </summary>
    [TestMethod]
    public void DeleteImportedFile_ThenUndelete()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createImportedFileEvent = new CreateImportedFileEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999),
        CustomerUID = Guid.NewGuid(),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        SurveyedUTC = actionUtc.AddDays(-1),
        ActionUTC = actionUtc
      };

      var deleteImportedFileEvent = new DeleteImportedFileEvent()
      {
        ProjectUID = createImportedFileEvent.ProjectUID,
        ImportedFileUID = createImportedFileEvent.ImportedFileUID,
        ActionUTC = actionUtc.AddHours(1)
      };

      var undeleteImportedFileEvent = new UndeleteImportedFileEvent()
      {
        ProjectUID = createImportedFileEvent.ProjectUID,
        ImportedFileUID = createImportedFileEvent.ImportedFileUID,
        ActionUTC = actionUtc.AddHours(1)
      };

       _projectContext.StoreEvent(createImportedFileEvent).Wait();

      var g = _projectContext.GetImportedFiles(createImportedFileEvent.ProjectUID.ToString());
      g.Wait();
      Assert.AreEqual(1, g.Result.Count(), "Should be able to retrieve ImportedFile from ProjectRepo");

      _projectContext.StoreEvent(deleteImportedFileEvent).Wait();
      g = _projectContext.GetImportedFiles(createImportedFileEvent.ProjectUID.ToString());
      g.Wait();
      Assert.AreEqual(0, g.Result.Count(), "Should not be able to retrieve ImportedFile from ProjectRepo");

      var s = _projectContext.StoreEvent(undeleteImportedFileEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Undelete imported file failed");

      g = _projectContext.GetImportedFiles(createImportedFileEvent.ProjectUID.ToString());
      g.Wait();
      Assert.AreEqual(1, g.Result.Count(), "Should be able to retrieve ImportedFile from ProjectRepo");
      Assert.IsFalse(g.Result.ToList()[0].IsDeleted, "imported file deleted flag should be false");
    }

    #endregion ImportedFiles
  }
}