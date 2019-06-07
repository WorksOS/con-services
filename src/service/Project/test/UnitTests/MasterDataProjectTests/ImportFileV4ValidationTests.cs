using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.FlowJSHandler;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ImportFileV4ValidationTests
  {
    protected IServiceExceptionHandler ServiceExceptionHandler;
    private const string IMPORTED_BY = "JoeSmoe";

    private static readonly ProjectErrorCodesProvider projectErrorCodesProvider;
    private static readonly Guid projectUid = Guid.NewGuid();
    private static readonly DateTime? surfaceUtc = DateTime.UtcNow;
    private static readonly DateTime fileCreatedUtc = DateTime.UtcNow;
    private static readonly DateTime fileUpdatedUtc = DateTime.UtcNow;
    private static readonly Guid? parentUid = Guid.NewGuid();
    private static readonly double? offset = 1.5;

    static ImportFileV4ValidationTests()
    {
      projectErrorCodesProvider = new ProjectErrorCodesProvider();
    }

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();
      serviceCollection
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>(); 
      var serviceProvider = serviceCollection.BuildServiceProvider();
      ServiceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
    }

    [TestMethod]
    public void ValidateImportFile_NoFlowFile()
    {
      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(null, projectUid, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(27)));
    }

    [TestMethod]
    public void ValidateImportFile_FlowFileNameTooLong()
    {
      var file = new FlowFile { path = "", flowFilename = "" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(28)));
    }

    [TestMethod]
    public void ValidateImportFile_NoPath()
    {
      var file = new FlowFile { path = "", flowFilename = "deblah" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(29)));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidProjectUid()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, Guid.Empty, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(5)));
    }

    [TestMethod]
    public void ValidateImportFile_UnsupportedImportedFileType()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(31)));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidFileExtension()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah.ttm" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidFileCreatedUtc()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah.svl" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          DateTime.MinValue, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(33)));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidFileUpdatedUtc()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah.svl" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, DateTime.MinValue, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(34)));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidImportedBy()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah.svl" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, null, surfaceUtc, parentUid, offset));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(35)));
    }

    [TestMethod]
    public void ValidateImportFile_LineworkHappyPath()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.dxf" };

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Linework, DxfUnitsType.Meters,
        fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, parentUid, offset);
    }

    [TestMethod]
    public void ValidateImportFile_LineworkWrongFileExtension()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.bbbb" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Linework, DxfUnitsType.Meters,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, null, 0));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }

    [TestMethod]
    public void ValidateImportFile_DesignSurfaceHappyPath()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.ttm" };

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.DesignSurface, DxfUnitsType.ImperialFeet,
        fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, null, 0);
    }

    [TestMethod]
    public void ValidateImportFile_DesignSurfaceWrongFileExtension()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.bbbb" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.DesignSurface, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, null, 0));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }

    [TestMethod]
    public void ValidateImportFile_ReferenceSurfaceHappyPath()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah" };

      FileImportDataValidator.ValidateUpsertImportedFileRequest(projectUid, ImportedFileType.ReferenceSurface, DxfUnitsType.ImperialFeet,
        fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, "deblah", parentUid, offset);
    }

    [TestMethod]
    public void ValidateImportFile_ReferenceSurfaceMissingParent()
    {
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(projectUid, ImportedFileType.ReferenceSurface, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, "deblah", null, offset));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(118)));
    }

    [TestMethod]
    public void ValidateImportFile_ReferenceSurfaceMissingOffset()
    {
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(projectUid, ImportedFileType.ReferenceSurface, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, "deblah", parentUid, 0));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(118)));
    }

    [TestMethod]
    public void ValidateImportFile_SurveyedSurfaceHappyPath()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.ttm" };

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.DesignSurface, DxfUnitsType.ImperialFeet,
        fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, null, 0);
    }

    [TestMethod]
    public void ValidateImportFile_SurveyedSurfaceWrongFileExtension()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.bbbb" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.DesignSurface, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, null, 0));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }

    [TestMethod]
    public void ValidateImportFile_SurveyedSurfaceMissingUtc()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.ttm" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.SurveyedSurface, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, null, 0));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(36)));
    }


    [TestMethod]
    public void ValidateImportFile_AlignmentHappyPath()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.svl" };

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
        fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, null, 0);
    }

    [TestMethod]
    public void ValidateImportFile_AlignmentWrongFileExtension()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.ttm" };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, null, 0));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }

    [TestMethod]
    [DataRow(ImportedFileType.Alignment, "true", "false")]
    [DataRow(ImportedFileType.Alignment, "false", "true")]
    [DataRow(ImportedFileType.DesignSurface, "true", "false")]
    [DataRow(ImportedFileType.DesignSurface, "true", "true")]
    [DataRow(ImportedFileType.DesignSurface, "false", "true")]
    [DataRow(ImportedFileType.SurveyedSurface, "false", "true")]
    [DataRow(ImportedFileType.ReferenceSurface, "true", "false")]
    [DataRow(ImportedFileType.ReferenceSurface, "true", "true")]
    [DataRow(ImportedFileType.ReferenceSurface, "false", "true")]
    public void ValidateImportFile_EnvironmentVariables_HappyPath(ImportedFileType importedFileType, string raptorEnabled, string tRexEnabled)
    {
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns(tRexEnabled);
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns(raptorEnabled);
      ImportedFileUtils.ValidateEnvironmentVariables(importedFileType, mockConfigStore.Object, ServiceExceptionHandler);
    }

    [TestMethod]
    [DataRow(ImportedFileType.Alignment, "false", "false")]
    [DataRow(ImportedFileType.DesignSurface, "false", "false")]
    [DataRow(ImportedFileType.ReferenceSurface, "false", "false")]
    public void ValidateImportFile_EnvironmentVariables_UnHappyPath(ImportedFileType importedFileType, string raptorEnabled, string tRexEnabled)
    {
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns(tRexEnabled);
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns(raptorEnabled);
      var ex = Assert.ThrowsException<ServiceException>(
        () => ImportedFileUtils.ValidateEnvironmentVariables(importedFileType, mockConfigStore.Object, ServiceExceptionHandler));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(113)));
    }

  }
}
