using System;
using VSS.Common.Exceptions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.MasterData.ProjectTests
{
  public class ImportFileV5TBCValidationTests
  {
    private readonly long _projectId;
    private readonly string _fileSpaceId;
    private readonly string _path;
    private readonly string _name;
    private readonly ImportedFileType _importedFileTypeId;
    private readonly DateTime _createdUtc;

    public ImportFileV5TBCValidationTests()
    {
      _projectId = 56666;
      _fileSpaceId = "u3bdc38d-1afe-470e-8c1c-fc241d4c5e01";
      _path = "/BC Data/Sites/Chch Test Site";
      _name = "CTCTSITECAL.dc";
      _importedFileTypeId = ImportedFileType.DesignSurface;
      _createdUtc = DateTime.UtcNow.AddDays(-0.5);
    }

    [Fact]
    public void ValidateImportFile_InvalidProjectId()
    {
      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = _name,
        ImportedFileTypeId = _importedFileTypeId,
        CreatedUtc = _createdUtc
      };

      var ex = Assert.Throws<ServiceException>(
        () => FileImportV5TBCDataValidator.ValidateUpsertImportedFileRequest(0, importedFileTbc));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2005", StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateImportFile_UnsupportedImportedFileType()
    {
      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = _name,
        ImportedFileTypeId = ImportedFileType.MassHaulPlan,
        CreatedUtc = _createdUtc
      };

      var ex = Assert.Throws<ServiceException>(
        () => FileImportV5TBCDataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2031", StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateImportFile_IncompleteLinework()
    {
      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheLineWork.dxf",
        ImportedFileTypeId = ImportedFileType.Linework,
        CreatedUtc = _createdUtc
      };

      var ex = Assert.Throws<ServiceException>(
        () => FileImportV5TBCDataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2075", StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateImportFile_CompleteLinework()
    {
      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheLineWork.dxf",
        ImportedFileTypeId = ImportedFileType.Linework,
        CreatedUtc = _createdUtc,
        LineworkFile = new LineworkFile { DxfUnitsTypeId = DxfUnitsType.ImperialFeet }
      };

      FileImportV5TBCDataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc);
    }

    [Fact]
    public void ValidateImportFile_IncompleteSurveyedSurface()
    {
      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheSurfaceFile.ttm",
        ImportedFileTypeId = ImportedFileType.SurveyedSurface,
        CreatedUtc = _createdUtc
      };

      var ex = Assert.Throws<ServiceException>(
        () => FileImportV5TBCDataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2033", StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateImportFile_CompleteSurveyedSurface()
    {
      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheSurfaceFile.ttm",
        ImportedFileTypeId = ImportedFileType.SurveyedSurface,
        CreatedUtc = _createdUtc,
        SurfaceFile = new SurfaceFile { SurveyedUtc = DateTime.UtcNow.AddDays(-1) }
      };

      FileImportV5TBCDataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc);
    }

    [Fact]
    public void ValidateImportFile_IncompleteAlignment()
    {
      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheAlignment.svl",
        ImportedFileTypeId = ImportedFileType.Alignment,
        CreatedUtc = _createdUtc
      };

      var ex = Assert.Throws<ServiceException>(
        () => FileImportV5TBCDataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2095", StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateImportFile_CompleteAlignment()
    {
      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheAlignment.svl",
        ImportedFileTypeId = ImportedFileType.Alignment,
        CreatedUtc = _createdUtc,
        AlignmentFile = new AlignmentFile { Offset = 3 }
      };

      FileImportV5TBCDataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc);
    }


    [Fact]
    public void ValidateImportFile_NoFilename()
    {
      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "",
        ImportedFileTypeId = ImportedFileType.DesignSurface,
        CreatedUtc = _createdUtc
      };

      var ex = Assert.Throws<ServiceException>(
        () => FileImportV5TBCDataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2002", StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateImportFile_IncompleteDesignSurface()
    {
      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheAlignment.svl",
        ImportedFileTypeId = ImportedFileType.DesignSurface,
        CreatedUtc = _createdUtc
      };

      var ex = Assert.Throws<ServiceException>(
        () => FileImportV5TBCDataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2032", StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateImportFile_CompleteDesignSurface()
    {
      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheAlignment.ttm",
        ImportedFileTypeId = ImportedFileType.DesignSurface,
        CreatedUtc = _createdUtc
      };

      FileImportV5TBCDataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc);
    }

    [Fact]
    public void ImportedFileV5TBC_RemoveSurveyedUtcFromName()
    {
      // JB topo southern motorway_2010-11-29T153300Z.TTM   SS=2010-11-29 15:33:00.0000000

      var nhOpName = "JB topo southern motorway_2010-11-29T153300Z.TTM";
      var expectedProjectName = "JB topo southern motorway.TTM";
      var projectName = ImportedFileUtils.RemoveSurveyedUtcFromName(nhOpName);

      Assert.Equal(expectedProjectName, projectName);
    }

    [Fact]
    public void ImportedFileV5TBC_RemoveSurveyedUtcFromNameWithDash()
    {
      var nhOpName = "Marylands Road - Marylands.ttm";
      var expectedProjectName = "Marylands Road - Marylands.ttm";
      var projectName = ImportedFileUtils.RemoveSurveyedUtcFromName(nhOpName);

      Assert.Equal(expectedProjectName, projectName);
    }

    [Fact]
    public void ImportedFileV5TBC_RemoveSurveyedUtcFromName_DoubleUtc()
    {
      // Aerial Survey 120819_2012-08-19T035400Z_2016-08-16T003724Z.TTM ssUtc=2016-08-16 00:37:24.0000000

      var nhOpName = "Aerial Survey 120819_2012-08-19T035400Z_2016-08-16T003724Z.TTM";
      var expectedProjectName = "Aerial Survey 120819_2012-08-19T035400Z.TTM";
      var projectName = ImportedFileUtils.RemoveSurveyedUtcFromName(nhOpName);

      Assert.Equal(expectedProjectName, projectName);
    }

    [Fact]
    public void ImportedFileV5TBC_RemoveSurveyedUtcFromNameWithUnderscores()
    {
      var nhOpName = "Surveyed_Surface_2010-11-29T153300Z.TTM";
      var expectedProjectName = "Surveyed_Surface.TTM";
      var projectName = ImportedFileUtils.RemoveSurveyedUtcFromName(nhOpName);

      Assert.Equal(expectedProjectName, projectName);
    }

    [Fact]
    public void ImportedFileV5TBC_RemoveSurveyedUtcFromNameWithNoSurveyedUtc()
    {
      var nhOpName = "Design_Surface.TTM";
      var expectedProjectName = nhOpName;
      var projectName = ImportedFileUtils.RemoveSurveyedUtcFromName(nhOpName);

      Assert.Equal(expectedProjectName, projectName);
    }
  }
}
