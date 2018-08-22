using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.FlowJSHandler;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ImportFileTests
  {
    private const string IMPORTED_BY = "JoeSmoe";

    private static readonly ProjectErrorCodesProvider projectErrorCodesProvider;
    private static readonly Guid projectUid = Guid.NewGuid();
    private static readonly DateTime? surfaceUtc = DateTime.UtcNow;
    private static readonly DateTime fileCreatedUtc = DateTime.UtcNow;
    private static readonly DateTime fileUpdatedUtc = DateTime.UtcNow;

    static ImportFileTests()
    {
      projectErrorCodesProvider = new ProjectErrorCodesProvider();
    }

    [TestMethod]
    public void ValidateImportFile_NoFlowFile()
    {
      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(null, projectUid, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(27)));
    }

    [TestMethod]
    public void ValidateImportFile_FlowFileNameTooLong()
    {
      var file = new FlowFile { path = "", flowFilename = "" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(28)));
    }

    [TestMethod]
    public void ValidateImportFile_NoPath()
    {
      var file = new FlowFile { path = "", flowFilename = "deblah" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(29)));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidProjectUid()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, Guid.Empty, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(5)));
    }

    [TestMethod]
    public void ValidateImportFile_UnsupportedImportedFileType()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(31)));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidFileExtension()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah.ttm" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidFileCreatedUtc()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah.svl" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          DateTime.MinValue, fileUpdatedUtc, IMPORTED_BY, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(33)));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidFileUpdatedUtc()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah.svl" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, DateTime.MinValue, IMPORTED_BY, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(34)));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidImportedBy()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah.svl" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, null, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(35)));
    }

    [TestMethod]
    public void ValidateImportFile_LineworkHappyPath()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.dxf" };

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Linework, DxfUnitsType.Meters,
        fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null);
    }

    [TestMethod]
    public void ValidateImportFile_LineworkWrongFileExtension()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.bbbb" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Linework, DxfUnitsType.Meters,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }

    [TestMethod]
    public void ValidateImportFile_DesignSurfaceHappyPath()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.ttm" };

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.DesignSurface, DxfUnitsType.ImperialFeet,
        fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null);
    }

    [TestMethod]
    public void ValidateImportFile_DesignSurfaceWrongFileExtension()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.bbbb" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.DesignSurface, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }


    [TestMethod]
    public void ValidateImportFile_SurveyedSurfaceHappyPath()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.ttm" };

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.DesignSurface, DxfUnitsType.ImperialFeet,
        fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc);
    }

    [TestMethod]
    public void ValidateImportFile_SurveyedSurfaceWrongFileExtension()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.bbbb" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.DesignSurface, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }

    [TestMethod]
    public void ValidateImportFile_SurveyedSurfaceMissingUtc()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.ttm" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.SurveyedSurface, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(36)));
    }


    [TestMethod]
    public void ValidateImportFile_AlignmentHappyPath()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.svl" };

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
        fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null);
    }

    [TestMethod]
    public void ValidateImportFile_AlignmentWrongFileExtension()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.ttm" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }
  }
}
