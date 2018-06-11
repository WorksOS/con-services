using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ImportFileV2Tests
  {
    protected ProjectErrorCodesProvider projectErrorCodesProvider = new ProjectErrorCodesProvider();
    private readonly long _projectId;
    private readonly string _fileSpaceId;
    private readonly string _path;
    private readonly string _name;
    private readonly ImportedFileType _importedFileTypeId;
    private readonly DateTime _createdUtc;

    public ImportFileV2Tests()
    {
      _projectId = 56666;
      _fileSpaceId = "u3bdc38d-1afe-470e-8c1c-fc241d4c5e01";
      _path = "/BC Data/Sites/Chch Test Site";
      _name = "CTCTSITECAL.dc";
      _importedFileTypeId = ImportedFileType.DesignSurface;
      _createdUtc = DateTime.UtcNow.AddDays(-0.5);
  }

    [TestMethod]
    public void ValidateImportFile_InvalidProjectId()
    {
      var importedFileTbc = new ImportedFileTbc()
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = _name,
        ImportedFileTypeId = _importedFileTypeId,
        CreatedUtc = _createdUtc
      };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportV2DataValidator.ValidateUpsertImportedFileRequest(0, importedFileTbc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2005", StringComparison.Ordinal), "Expected error number 2005");
    }

    [TestMethod]
    public void ValidateImportFile_UnsupportedImportedFileType()
    {
      var importedFileTbc = new ImportedFileTbc()
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = _name,
        ImportedFileTypeId = ImportedFileType.MassHaulPlan,
        CreatedUtc = _createdUtc
      };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportV2DataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2031", StringComparison.Ordinal), "Expected error number 2031");
    }

    [TestMethod]
    public void ValidateImportFile_IncompleteLinework()
    {
      var importedFileTbc = new ImportedFileTbc()
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheLineWork.dxf",
        ImportedFileTypeId = ImportedFileType.Linework,
        CreatedUtc = _createdUtc
      };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportV2DataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2075", StringComparison.Ordinal), "Expected error number 2075");
    }

    [TestMethod]
    public void ValidateImportFile_CompleteLinework()
    {
      var importedFileTbc = new ImportedFileTbc()
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheLineWork.dxf",
        ImportedFileTypeId = ImportedFileType.Linework,
        CreatedUtc = _createdUtc,
        LineworkFile = new LineworkFile() { DxfUnitsTypeId = DxfUnitsType.ImperialFeet}
      };

      FileImportV2DataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc);
    }

    [TestMethod]
    public void ValidateImportFile_IncompleteSurveyedSurface()
    {
      var importedFileTbc = new ImportedFileTbc()
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheSurfaceFile.ttm",
        ImportedFileTypeId = ImportedFileType.SurveyedSurface,
        CreatedUtc = _createdUtc
      };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportV2DataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2033", StringComparison.Ordinal), "Expected error number 2033");
    }

    [TestMethod]
    public void ValidateImportFile_CompleteSurveyedSurface()
    {
      var importedFileTbc = new ImportedFileTbc()
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheSurfaceFile.ttm", 
        ImportedFileTypeId = ImportedFileType.SurveyedSurface,
        CreatedUtc = _createdUtc,
        SurfaceFile = new SurfaceFile() {SurveyedUtc = DateTime.UtcNow.AddDays(-1)}
      };

      FileImportV2DataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc);
    }

    [TestMethod]
    public void ValidateImportFile_IncompleteAlignment()
    {
      var importedFileTbc = new ImportedFileTbc()
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheAlignment.svl",
        ImportedFileTypeId = ImportedFileType.Alignment,
        CreatedUtc = _createdUtc
      };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportV2DataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2095", StringComparison.Ordinal), "Expected error number 2095");
    }

    [TestMethod]
    public void ValidateImportFile_CompleteAlignment()
    {
      var importedFileTbc = new ImportedFileTbc()
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheAlignment.svl",
        ImportedFileTypeId = ImportedFileType.Alignment,
        CreatedUtc = _createdUtc,
        AlignmentFile = new AlignmentFile() { Offset = 3 }
      };

      FileImportV2DataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc);
    }


    [TestMethod]
    public void ValidateImportFile_NoFilename()
    {
      var importedFileTbc = new ImportedFileTbc()
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "",
        ImportedFileTypeId = ImportedFileType.DesignSurface,
        CreatedUtc = _createdUtc
      };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportV2DataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2002", StringComparison.Ordinal), "Expected error number 2002");
    }

    [TestMethod]
    public void ValidateImportFile_IncompleteDesignSurface()
    {
      var importedFileTbc = new ImportedFileTbc()
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheAlignment.svl",
        ImportedFileTypeId = ImportedFileType.DesignSurface,
        CreatedUtc = _createdUtc
      };

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportV2DataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("2032", StringComparison.Ordinal), "Expected error number 2032");
    }

    [TestMethod]
    public void ValidateImportFile_CompleteDesignSurface()
    {
      var importedFileTbc = new ImportedFileTbc()
      {
        FileSpaceId = _fileSpaceId,
        Path = _path,
        Name = "TheAlignment.ttm",
        ImportedFileTypeId = ImportedFileType.DesignSurface,
        CreatedUtc = _createdUtc
      };

      FileImportV2DataValidator.ValidateUpsertImportedFileRequest(_projectId, importedFileTbc);
    }

    [TestMethod]
    public void ImportedFileV2_RemoveSurveyedUtcFromName()
    {
      // JB topo southern motorway_2010-11-29T153300Z.TTM   SS=2010-11-29 15:33:00.0000000

      var nhOpName = "JB topo southern motorway_2010-11-29T153300Z.TTM";
      var expectedProjectName = "JB topo southern motorway.TTM";
      var projectName = ImportedFileUtils.RemoveSurveyedUtcFromName(nhOpName);

      Assert.AreEqual(expectedProjectName, projectName, "File name has not been converted correctly");
    }

    [TestMethod]
    public void ImportedFileV2_RemoveSurveyedUtcFromNameWithDash()
    {
      var nhOpName = "Marylands Road - Marylands.ttm";
      var expectedProjectName = "Marylands Road - Marylands.ttm";
      var projectName = ImportedFileUtils.RemoveSurveyedUtcFromName(nhOpName);

      Assert.AreEqual(expectedProjectName, projectName, "File name has not been converted correctly");
    }

    [TestMethod]
    public void ImportedFileV2_RemoveSurveyedUtcFromName_DoubleUtc()
    {
      // Aerial Survey 120819_2012-08-19T035400Z_2016-08-16T003724Z.TTM ssUtc=2016-08-16 00:37:24.0000000

      var nhOpName = "Aerial Survey 120819_2012-08-19T035400Z_2016-08-16T003724Z.TTM";
      var expectedProjectName = "Aerial Survey 120819_2012-08-19T035400Z.TTM";
      var projectName = ImportedFileUtils.RemoveSurveyedUtcFromName(nhOpName);

      Assert.AreEqual(expectedProjectName, projectName, "File name has not been converted correctly");
    }

    [TestMethod]
    public void ImportedFileV2_RemoveSurveyedUtcFromNameWithUnderscores()
    {
      var nhOpName = "Surveyed_Surface_2010-11-29T153300Z.TTM";
      var expectedProjectName = "Surveyed_Surface.TTM";
      var projectName = ImportedFileUtils.RemoveSurveyedUtcFromName(nhOpName);

      Assert.AreEqual(expectedProjectName, projectName, "File name has not been converted correctly");
    }

    [TestMethod]
    public void ImportedFileV2_RemoveSurveyedUtcFromNameWithNoSurveyedUtc()
    {
      var nhOpName = "Design_Surface.TTM";
      var expectedProjectName = nhOpName;
      var projectName = ImportedFileUtils.RemoveSurveyedUtcFromName(nhOpName);

      Assert.AreEqual(expectedProjectName, projectName, "File name has not been converted correctly");
    }
  }
}
