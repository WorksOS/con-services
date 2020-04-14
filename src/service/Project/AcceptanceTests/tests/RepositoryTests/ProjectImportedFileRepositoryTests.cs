using System;
using System.Linq;
using RepositoryTests.Internal;
using VSS.Productivity3D.Project.Repository;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace RepositoryTests
{
  public class ProjectImportedFileRepositoryTests : TestControllerBase
  {
    ProjectRepository projectRepo;

    public ProjectImportedFileRepositoryTests()
    {
      SetupLogging();
      projectRepo = new ProjectRepository(configStore, loggerFactory);
    }


    #region ImportedFiles

    /// <summary>
    /// Create ImportedFile - Happy path i.e. 
    ///   ImportedFile doesn't exist already.
    /// </summary>
    [Fact]
    public void CreateImportedFile_HappyPath()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

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
        MinZoomLevel = 0,
        MaxZoomLevel = 0,
        ActionUTC = actionUtc
      };

      var s = projectRepo.StoreEvent(createImportedFileEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = projectRepo.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);
      Assert.Single(g.Result.ImportedFileHistory.ImportedFileHistoryItems);

      var gList = projectRepo.GetImportedFiles(createImportedFileEvent.ProjectUID.ToString());
      gList.Wait();
      Assert.NotNull(gList.Result);

      var ifList = gList.Result.ToList();
      Assert.Single(ifList);
      Assert.Equal(DxfUnitsType.Meters, ifList[0].DxfUnitsType);
      Assert.Single(ifList[0].ImportedFileHistory.ImportedFileHistoryItems);
      Assert.Equal(createImportedFileEvent.FileUpdatedUtc, ifList[0].ImportedFileHistory.ImportedFileHistoryItems[0].FileUpdatedUtc);

      Assert.Equal(g.Result.ImportedFileHistory, ifList[0].ImportedFileHistory);
      Assert.Equal(g.Result.ImportedFileHistory.ImportedFileHistoryItems[0], ifList[0].ImportedFileHistory.ImportedFileHistoryItems[0]);
    }

    /// <summary>
    /// Create ImportedFile - Happy path i.e. 
    ///   ImportedFile doesn't exist already.
    ///   Alignment type has non-default Units
    /// </summary>
    [Fact]
    public void CreateImportedFile_HappyPath_WithDXFUnits()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createImportedFileEvent = new CreateImportedFileEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999),
        CustomerUID = Guid.NewGuid(),
        ImportedFileType = ImportedFileType.Alignment,
        Name = "Test alignment type.svl",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        DxfUnitsType = DxfUnitsType.UsSurveyFeet,
        ActionUTC = actionUtc
      };

      var s = projectRepo.StoreEvent(createImportedFileEvent);
      s.Wait();
      Assert.Equal(1, s.Result);

      var g = projectRepo.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);

      var gList = projectRepo.GetImportedFiles(createImportedFileEvent.ProjectUID.ToString());
      gList.Wait();
      Assert.NotNull(gList.Result);

      var ifList = gList.Result.ToList();
      Assert.Single(ifList);
      Assert.Equal(DxfUnitsType.UsSurveyFeet, ifList[0].DxfUnitsType);
    }

    /// <summary>
    /// Create ImportedFile - Happy path i.e. 
    ///   ImportedFile doesn't exist already.
    ///   Alignment type has non-default Units
    /// </summary>
    [Fact]
    public void CreateImportedFile_HappyPath_WithParent()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();


      //Create the parent design first
      var createImportedFileEvent1 = new CreateImportedFileEvent()
      {
        ProjectUID = new Guid(projectUid),
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999),
        CustomerUID = new Guid(customerUid),
        ImportedFileType = ImportedFileType.DesignSurface,
        Name = "Test design type.ttm",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        ActionUTC = actionUtc
      };

      //Create reference surface
      var createImportedFileEvent2 = new CreateImportedFileEvent()
      {
        ProjectUID = new Guid(projectUid),
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999),
        CustomerUID = new Guid(customerUid),
        ImportedFileType = ImportedFileType.ReferenceSurface,
        Name = "Test reference type.ttm",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        ActionUTC = actionUtc,
        ParentUID = createImportedFileEvent1.ImportedFileUID,
        Offset = 1.5
      };

      projectRepo.StoreEvent(createImportedFileEvent1).Wait();
      projectRepo.StoreEvent(createImportedFileEvent2).Wait();

      var g = projectRepo.GetImportedFile(createImportedFileEvent1.ImportedFileUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);

      g = projectRepo.GetImportedFile(createImportedFileEvent2.ImportedFileUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);

      Assert.Equal(createImportedFileEvent2.ParentUID.ToString(), g.Result.ParentUid);
      Assert.Equal(createImportedFileEvent2.Offset, g.Result.Offset);

      var g2 = projectRepo.GetReferencedImportedFiles(createImportedFileEvent1.ImportedFileUID.ToString());
      g2.Wait();
      Assert.NotNull(g2.Result);
      var list = g2.Result.ToList();
      Assert.Single(list);
      Assert.Equal(createImportedFileEvent2.ImportedFileUID.ToString(), list[0].ImportedFileUid);
    }

    /// <summary>
    /// Create ImportedFile - HappyPath 
    ///   ImportedFiles for multiple projects, should only return those for requested project.
    /// </summary>
    [Fact]
    public void CreateImportedFile_MultiProjects()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid().ToString();
      var project1 = Guid.NewGuid().ToString();
      var project2 = Guid.NewGuid().ToString();
      var project3 = Guid.NewGuid().ToString();

      var createImportedFileEventP1_1 = new CreateImportedFileEvent()
      {
        ProjectUID = new Guid(project1),
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999),
        CustomerUID = new Guid(customerUid),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        SurveyedUTC = actionUtc.AddDays(-1),
        MinZoomLevel = 0,
        MaxZoomLevel = 0,
        ActionUTC = actionUtc
      };

      var createImportedFileEventP1_2 = new CreateImportedFileEvent()
      {
        ProjectUID = new Guid(project1),
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999) + 1,
        CustomerUID = new Guid(customerUid),
        ImportedFileType = ImportedFileType.Alignment,
        Name = "Test alginment type.svl",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        SurveyedUTC = actionUtc.AddDays(-1),
        MinZoomLevel = 13,
        MaxZoomLevel = 16,
        ActionUTC = actionUtc
      };

      var createImportedFileEventP2_1 = new CreateImportedFileEvent()
      {
        ProjectUID = new Guid(project2),
        ImportedFileUID = Guid.NewGuid(),
        ImportedFileID = new Random().Next(1, 1999999) + 2,
        CustomerUID = new Guid(customerUid),
        ImportedFileType = ImportedFileType.SurveyedSurface,
        Name = "Test SS type.ttm",
        FileDescriptor = "fd",
        FileCreatedUtc = actionUtc,
        FileUpdatedUtc = actionUtc,
        ImportedBy = "JoeSmoe",
        SurveyedUTC = actionUtc.AddDays(-1),
        MinZoomLevel = 0,
        MaxZoomLevel = 0,
        ActionUTC = actionUtc
      };


      projectRepo.StoreEvent(createImportedFileEventP1_1).Wait();
      projectRepo.StoreEvent(createImportedFileEventP1_2).Wait();
      projectRepo.StoreEvent(createImportedFileEventP2_1).Wait();

      var g = projectRepo.GetImportedFile(createImportedFileEventP1_2.ImportedFileUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);
      Assert.NotNull(g.Result.FileDescriptor);
      Assert.Single(g.Result.ImportedFileHistory.ImportedFileHistoryItems);

      var gList = projectRepo.GetImportedFiles(project1.ToString());
      gList.Wait();
      Assert.NotNull(gList.Result);

      var ifList = gList.Result.ToList();
      Assert.Equal(2, ifList.Count);

      gList = projectRepo.GetImportedFiles(project2.ToString());
      gList.Wait();
      Assert.NotNull(gList.Result);
      ifList = gList.Result.ToList();
      Assert.Single(ifList);

      gList = projectRepo.GetImportedFiles(project3.ToString());
      gList.Wait();
      Assert.NotNull(gList.Result);

      ifList = gList.Result.ToList();
      Assert.Empty(ifList);
    }

    /// <summary>
    /// Update ImportedFile - Happy path i.e. 
    ///   ImportedFile exists already.
    /// </summary>
    [Fact]
    public void UpdateImportedFile_HappyPath()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

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
        MinZoomLevel = 18,
        MaxZoomLevel = 20,
        Offset = 1.5,
        ActionUTC = actionUtc
      };

      projectRepo.StoreEvent(createImportedFileEvent).Wait();

      var updateImportedFileEvent = new UpdateImportedFileEvent()
      {
        ProjectUID = createImportedFileEvent.ProjectUID,
        ImportedFileUID = createImportedFileEvent.ImportedFileUID,
        FileDescriptor = "fd",
        MinZoomLevel = 16,
        MaxZoomLevel = 19,
        Offset = 2.8,
        FileCreatedUtc = actionUtc.AddDays(2).AddHours(2),
        FileUpdatedUtc = actionUtc.AddDays(2).AddHours(3),
        ImportedBy = "JoeSmoe3",
        ActionUTC = actionUtc.AddHours(1)
      };

      projectRepo.StoreEvent(updateImportedFileEvent).Wait();

      var g = projectRepo.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);
      Assert.Equal(updateImportedFileEvent.ActionUTC, g.Result.LastActionedUtc);
      Assert.Equal(updateImportedFileEvent.FileDescriptor, g.Result.FileDescriptor);
      Assert.Equal(updateImportedFileEvent.MinZoomLevel, g.Result.MinZoomLevel);
      Assert.Equal(updateImportedFileEvent.MaxZoomLevel, g.Result.MaxZoomLevel);
      Assert.Equal(updateImportedFileEvent.Offset, g.Result.Offset);
      Assert.Equal(2, g.Result.ImportedFileHistory.ImportedFileHistoryItems.Count);
      Assert.Equal(createImportedFileEvent.FileCreatedUtc, g.Result.ImportedFileHistory.ImportedFileHistoryItems[0].FileCreatedUtc);
      Assert.Equal(createImportedFileEvent.FileUpdatedUtc, g.Result.ImportedFileHistory.ImportedFileHistoryItems[0].FileUpdatedUtc);
      Assert.Equal(createImportedFileEvent.ImportedBy, g.Result.ImportedFileHistory.ImportedFileHistoryItems[0].ImportedBy);
      Assert.Equal(updateImportedFileEvent.FileCreatedUtc, g.Result.ImportedFileHistory.ImportedFileHistoryItems[1].FileCreatedUtc);
      Assert.Equal(updateImportedFileEvent.FileUpdatedUtc, g.Result.ImportedFileHistory.ImportedFileHistoryItems[1].FileUpdatedUtc);
      Assert.Equal(updateImportedFileEvent.ImportedBy, g.Result.ImportedFileHistory.ImportedFileHistoryItems[1].ImportedBy);
    }

    /// <summary>
    /// Update ImportedFile - Happy path i.e. 
    ///   ImportedFile exists already.
    /// </summary>
    [Fact]
    public void UpdateImportedFile_SameCreateUpdate()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

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
        MinZoomLevel = 18,
        MaxZoomLevel = 20,
        ActionUTC = actionUtc
      };

      projectRepo.StoreEvent(createImportedFileEvent).Wait();

      var updateImportedFileEvent = new UpdateImportedFileEvent()
      {
        ProjectUID = createImportedFileEvent.ProjectUID,
        ImportedFileUID = createImportedFileEvent.ImportedFileUID,
        FileDescriptor = "fd",
        MinZoomLevel = 16,
        MaxZoomLevel = 19,
        FileCreatedUtc = createImportedFileEvent.FileCreatedUtc,
        FileUpdatedUtc = createImportedFileEvent.FileUpdatedUtc,
        ImportedBy = "JoeSmoe3",
        ActionUTC = actionUtc.AddHours(1)
      };

      projectRepo.StoreEvent(updateImportedFileEvent).Wait();

      var g = projectRepo.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);
      Assert.Equal(updateImportedFileEvent.ActionUTC, g.Result.LastActionedUtc);
      Assert.Equal(updateImportedFileEvent.FileDescriptor, g.Result.FileDescriptor);
      Assert.Equal(updateImportedFileEvent.MinZoomLevel, g.Result.MinZoomLevel);
      Assert.Equal(updateImportedFileEvent.MaxZoomLevel, g.Result.MaxZoomLevel);
      Assert.Single(g.Result.ImportedFileHistory.ImportedFileHistoryItems);
      Assert.Equal(createImportedFileEvent.FileCreatedUtc, g.Result.ImportedFileHistory.ImportedFileHistoryItems[0].FileCreatedUtc);
      Assert.Equal(createImportedFileEvent.FileUpdatedUtc, g.Result.ImportedFileHistory.ImportedFileHistoryItems[0].FileUpdatedUtc);
      Assert.Equal(createImportedFileEvent.ImportedBy, g.Result.ImportedFileHistory.ImportedFileHistoryItems[0].ImportedBy);
    }


    /// <summary>
    /// Delete ImportedFile - Happy path i.e. 
    ///   ImportedFile exists already.
    /// </summary>
    [Fact]
    public void DeleteImportedFile_HappyPath()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

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

      projectRepo.StoreEvent(createImportedFileEvent).Wait();

      var deleteImportedFileEvent = new DeleteImportedFileEvent()
      {
        ProjectUID = createImportedFileEvent.ProjectUID,
        ImportedFileUID = createImportedFileEvent.ImportedFileUID,
        ActionUTC = actionUtc.AddHours(1)
      };

      projectRepo.StoreEvent(deleteImportedFileEvent).Wait();

      var g = projectRepo.GetImportedFile(createImportedFileEvent.ProjectUID.ToString());
      g.Wait();
      Assert.Null(g.Result);

      g = projectRepo.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());
      g.Wait();
      Assert.NotNull(g.Result);
      Assert.True(g.Result.IsDeleted);
    }

    /// <summary>
    /// Delete ImportedFile permanently
    /// </summary>
    [Fact]
    public void DeleteImportedFile_Permanently()
    {
      var actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

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

      projectRepo.StoreEvent(createImportedFileEvent).Wait();

      var deleteImportedFileEvent = new DeleteImportedFileEvent()
      {
        ProjectUID = createImportedFileEvent.ProjectUID,
        ImportedFileUID = createImportedFileEvent.ImportedFileUID,
        DeletePermanently = true,
        ActionUTC = actionUtc.AddHours(1)
      };

      projectRepo.StoreEvent(deleteImportedFileEvent).Wait();

      var g = projectRepo.GetImportedFile(createImportedFileEvent.ProjectUID.ToString());
      g.Wait();
      Assert.Null(g.Result);

      g = projectRepo.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString());
      g.Wait();
      Assert.Null(g.Result);
    }   

    #endregion ImportedFiles
  }
}
