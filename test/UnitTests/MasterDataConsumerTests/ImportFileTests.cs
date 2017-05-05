using System;
using System.IO;
using FlowUploadFilter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectWebApiCommon.Models;
using ProjectWebApiCommon.ResultsHandling;
using Repositories.DBModels;

namespace VSS.Visionlink.Project.UnitTests
{
  [TestClass]
  public class ImportFileTests
  {
    [TestMethod]
    public void ValidateImportFile_NoFlowFile()
    {
      FlowFile file = null; 
      Guid projectUid = Guid.NewGuid();
      ImportedFileType importedFileType = ImportedFileType.Unknown;
      DateTime? surveyedSurfaceUtc = new DateTime();

      var error = "CreateImportedFileV4.The file was not imported successfully";
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateCreateImportedFileRequest(file, projectUid, importedFileType, surveyedSurfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error));
    }

    [TestMethod]
    public void ValidateImportFile_FlowFileNameTooLong()
    {
      FlowFile file = new FlowFile() {path = "", flowFilename = ""};
      Guid projectUid = Guid.NewGuid();
      ImportedFileType importedFileType = ImportedFileType.Unknown;
      DateTime? surveyedSurfaceUtc = new DateTime();

      var error =
        "CreateImportedFileV4.Supplied filename is not valid. Either exceeds the length limit of 256 is empty or contains illegal characters.";
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateCreateImportedFileRequest(file, projectUid, importedFileType, surveyedSurfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error, StringComparison.Ordinal));
    }

    [TestMethod]
    [Ignore]
    public void ValidateImportFile_NoPath()
    {
      FlowFile file = new FlowFile() { path = "", flowFilename = "" };
      Guid projectUid = Guid.NewGuid();
      ImportedFileType importedFileType = ImportedFileType.Unknown;
      DateTime? surveyedSurfaceUtc = new DateTime();

      var error = string.Format($"CreateImportedFileV4.Supplied path {0} is not valid.Either is empty or contains illegal characters.", file.path);
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateCreateImportedFileRequest(file, projectUid, importedFileType, surveyedSurfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidProjectUid()
    {
      FlowFile file = new FlowFile() { path = "blahblah", flowFilename = "deblah" };
      Guid projectUid = Guid.Empty;
      ImportedFileType importedFileType = ImportedFileType.Unknown;
      DateTime? surveyedSurfaceUtc = new DateTime();

      var error = string.Format($"The projectUid is invalid {0}.", projectUid);
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateCreateImportedFileRequest(file, projectUid, importedFileType, surveyedSurfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error));
    }


    [TestMethod]
    [Ignore]
    public void ValidateImportFile_NoImportedFileType()
    {
      FlowFile file = new FlowFile() {path = "blahblah", flowFilename = "deblah"};
      Guid projectUid = Guid.NewGuid();
      ImportedFileType importedFileType = ImportedFileType.Unknown;
      DateTime? surveyedSurfaceUtc = new DateTime();

      var error = string.Format("CreateImportedFileV4. ImportedFileType {0} not known", importedFileType.ToString());
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateCreateImportedFileRequest(file, projectUid, importedFileType, surveyedSurfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error, StringComparison.Ordinal));
    }

    [TestMethod]
    [Ignore]
    public void ValidateImportFile_UnsupportedImportedFileType()
    {
      FlowFile file = new FlowFile() { path = "blahblah", flowFilename = "deblah" };
      Guid projectUid = Guid.NewGuid();
      ImportedFileType importedFileType = ImportedFileType.Linework;
      DateTime? surveyedSurfaceUtc = new DateTime();

      var error = string.Format(
        "CreateImportedFileV4. ImportedFileType: {0}, is invalid. Only Alignment file types are supported at present",
        importedFileType.ToString());
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateCreateImportedFileRequest(file, projectUid, importedFileType, surveyedSurfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error, StringComparison.Ordinal));
    }

    [TestMethod]
    [Ignore]
    public void ValidateImportFile_InvalidFileExtension()
    {
      FlowFile file = new FlowFile() { path = "blahblah", flowFilename = "deblah.ttm" };
      Guid projectUid = Guid.Empty;
      ImportedFileType importedFileType = ImportedFileType.Alignment;
      DateTime? surveyedSurfaceUtc = new DateTime();

      var error = string.Format("CreateImportedFileV4. ImportedFileType {0} does not match the file extension received {1}.", importedFileType.ToString(), Path.GetExtension(file.flowFilename));
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateCreateImportedFileRequest(file, projectUid, importedFileType, surveyedSurfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error, StringComparison.Ordinal));
    }

    [TestMethod]
    [Ignore]
    public void ValidateImportFile_InvalidFilePath()
    {
      FlowFile file = new FlowFile() { path = "blahblah", flowFilename = "deblah.svl" };
      Guid projectUid = Guid.Empty;
      ImportedFileType importedFileType = ImportedFileType.Alignment;
      DateTime? surveyedSurfaceUtc = new DateTime();

      var error = string.Format("The uploaded file {0} is not accessible.", file.path);
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateCreateImportedFileRequest(file, projectUid, importedFileType, surveyedSurfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error, StringComparison.Ordinal));
    }

    [TestMethod]
    [Ignore]
    public void ValidateImportFile_HappyPath()
    {
      FlowFile file = new FlowFile() { path = "\\*", flowFilename = "deblah.svl" };
      Guid projectUid = Guid.NewGuid();
      ImportedFileType importedFileType = ImportedFileType.Alignment;
      DateTime? surveyedSurfaceUtc = null;

      FileImportDataValidator.ValidateCreateImportedFileRequest(file, projectUid, importedFileType, surveyedSurfaceUtc);
    }

  }
}
