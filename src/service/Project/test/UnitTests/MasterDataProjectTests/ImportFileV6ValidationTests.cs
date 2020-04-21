using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.FlowJSHandler;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Project.Abstractions.Utilities;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.MasterData.ProjectTests
{
  public class ImportFileV6ValidationTests
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

    static ImportFileV6ValidationTests()
    {
      projectErrorCodesProvider = new ProjectErrorCodesProvider();
    }

    public ImportFileV6ValidationTests()
    {
      var serviceCollection = new ServiceCollection();
      serviceCollection
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ProjectErrorCodesProvider>(); 
      var serviceProvider = serviceCollection.BuildServiceProvider();
      ServiceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
    }

    [Fact]
    public void ValidateImportFile_NoFlowFile()
    {
      var ex = Assert.Throws<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(null, projectUid, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(27)));
    }

    [Fact]
    public void ValidateImportFile_FlowFileNameTooLong()
    {
      var file = new FlowFile { path = "", flowFilename = "" };

      var ex = Assert.Throws<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(28)));
    }

    [Fact]
    public void ValidateImportFile_NoPath()
    {
      var file = new FlowFile { path = "", flowFilename = "deblah" };

      var ex = Assert.Throws<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(29)));
    }

    [Fact]
    public void ValidateImportFile_InvalidProjectUid()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah" };

      var ex = Assert.Throws<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, Guid.Empty, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(5)));
    }

    [Fact]
    public void ValidateImportFile_UnsupportedImportedFileType()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah" };

      var ex = Assert.Throws<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.MassHaulPlan, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(31)));
    }

    [Fact]
    public void ValidateImportFile_InvalidFileExtension()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah.ttm" };

      var ex = Assert.Throws<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }

    [Fact]
    public void ValidateImportFile_InvalidFileCreatedUtc()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah.svl" };

      var ex = Assert.Throws<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          DateTime.MinValue, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(33)));
    }

    [Fact]
    public void ValidateImportFile_InvalidFileUpdatedUtc()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah.svl" };

      var ex = Assert.Throws<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, DateTime.MinValue, IMPORTED_BY, surfaceUtc, parentUid, offset));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(34)));
    }

    [Fact]
    public void ValidateImportFile_InvalidImportedBy()
    {
      var file = new FlowFile { path = "blahblah", flowFilename = "deblah.svl" };

      var ex = Assert.Throws<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, null, surfaceUtc, parentUid, offset));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(35)));
    }

    [Fact]
    public void ValidateImportFile_LineworkHappyPath()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.dxf" };

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Linework, DxfUnitsType.Meters,
        fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, parentUid, offset);
    }

    [Fact]
    public void ValidateImportFile_LineworkWrongFileExtension()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.bbbb" };

      var ex = Assert.Throws<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Linework, DxfUnitsType.Meters,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, null, 0));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }

    [Fact]
    public void ValidateImportFile_DesignSurfaceHappyPath()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.ttm" };

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.DesignSurface, DxfUnitsType.ImperialFeet,
        fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, null, 0);
    }

    [Fact]
    public void ValidateImportFile_DesignSurfaceWrongFileExtension()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.bbbb" };

      var ex = Assert.Throws<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.DesignSurface, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, null, 0));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }

    [Fact]
    public void ValidateImportFile_ReferenceSurfaceHappyPath()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah" };

      FileImportDataValidator.ValidateUpsertImportedFileRequest(projectUid, ImportedFileType.ReferenceSurface, DxfUnitsType.ImperialFeet,
        fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, "deblah", parentUid, offset);
    }

    [Fact]
    public void ValidateImportFile_ReferenceSurfaceMissingParent()
    {
      var ex = Assert.Throws<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(projectUid, ImportedFileType.ReferenceSurface, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, "deblah", null, offset));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(118)));
    }

    [Fact]
    public void ValidateImportFile_ReferenceSurfaceMissingOffset()
    {
      var ex = Assert.Throws<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(projectUid, ImportedFileType.ReferenceSurface, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, "deblah", parentUid, 0));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(118)));
    }

    [Fact]
    public void ValidateImportFile_SurveyedSurfaceHappyPath()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.ttm" };

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.DesignSurface, DxfUnitsType.ImperialFeet,
        fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, null, 0);
    }

    [Fact]
    public void ValidateImportFile_SurveyedSurfaceWrongFileExtension()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.bbbb" };

      var ex = Assert.Throws<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.DesignSurface, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, surfaceUtc, null, 0));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }

    [Fact]
    public void ValidateImportFile_SurveyedSurfaceMissingUtc()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.ttm" };

      var ex = Assert.Throws<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.SurveyedSurface, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, null, 0));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(36)));
    }


    [Fact]
    public void ValidateImportFile_AlignmentHappyPath()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.svl" };

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
        fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, null, 0);
    }

    [Fact]
    public void ValidateImportFile_AlignmentWrongFileExtension()
    {
      var file = new FlowFile { path = "\\*", flowFilename = "deblah.ttm" };

      var ex = Assert.Throws<ServiceException>(
        () => FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, ImportedFileType.Alignment, DxfUnitsType.ImperialFeet,
          fileCreatedUtc, fileUpdatedUtc, IMPORTED_BY, null, null, 0));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(32)));
    }

    [Theory]
    [InlineData(ImportedFileType.Alignment, "true", "false")]
    [InlineData(ImportedFileType.Alignment, "false", "true")]
    [InlineData(ImportedFileType.DesignSurface, "true", "false")]
    [InlineData(ImportedFileType.DesignSurface, "true", "true")]
    [InlineData(ImportedFileType.DesignSurface, "false", "true")]
    [InlineData(ImportedFileType.SurveyedSurface, "false", "true")]
    [InlineData(ImportedFileType.ReferenceSurface, "true", "false")]
    [InlineData(ImportedFileType.ReferenceSurface, "true", "true")]
    [InlineData(ImportedFileType.ReferenceSurface, "false", "true")]
    public void ValidateImportFile_EnvironmentVariables_HappyPath(ImportedFileType importedFileType, string raptorEnabled, string tRexEnabled)
    {
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns(tRexEnabled);
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns(raptorEnabled);
      ImportedFileUtils.ValidateEnvironmentVariables(importedFileType, mockConfigStore.Object, ServiceExceptionHandler);
    }

    [Theory]
    [InlineData(ImportedFileType.Alignment, "false", "false")]
    [InlineData(ImportedFileType.DesignSurface, "false", "false")]
    [InlineData(ImportedFileType.ReferenceSurface, "false", "false")]
    public void ValidateImportFile_EnvironmentVariables_UnHappyPath(ImportedFileType importedFileType, string raptorEnabled, string tRexEnabled)
    {
      var mockConfigStore = new Mock<IConfigurationStore>();
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT")).Returns(tRexEnabled);
      mockConfigStore.Setup(x => x.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT")).Returns(raptorEnabled);
      var ex = Assert.Throws<ServiceException>(
        () => ImportedFileUtils.ValidateEnvironmentVariables(importedFileType, mockConfigStore.Object, ServiceExceptionHandler));
      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(113)));
    }

  }
}
