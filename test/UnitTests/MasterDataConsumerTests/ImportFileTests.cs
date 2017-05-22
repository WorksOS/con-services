using System;
using System.IO;
using FlowUploadFilter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectWebApiCommon.Models;
using ProjectWebApiCommon.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

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
      ImportedFileType importedFileType = ImportedFileType.MassHaulPlan;
      DateTime? surfaceUtc = DateTime.UtcNow;
      DateTime fileCreatedUtc = DateTime.UtcNow;
      DateTime fileUpdatedUtc = DateTime.UtcNow;
      string importedBy = "JoeSmoe";

      var error = "CreateImportedFileV4.The file was not imported successfully";
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error));
    }

    [TestMethod]
    public void ValidateImportFile_FlowFileNameTooLong()
    {
      FlowFile file = new FlowFile() {path = "", flowFilename = ""};
      Guid projectUid = Guid.NewGuid();
      ImportedFileType importedFileType = ImportedFileType.MassHaulPlan;
      DateTime? surfaceUtc = DateTime.UtcNow;
      DateTime fileCreatedUtc = DateTime.UtcNow;
      DateTime fileUpdatedUtc = DateTime.UtcNow;
      string importedBy = "JoeSmoe";

      var error =
        "CreateImportedFileV4.Supplied filename is not valid. Either exceeds the length limit of 256 is empty or contains illegal characters.";
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateImportFile_NoPath()
    {
      FlowFile file = new FlowFile() {path = "", flowFilename = "deblah"};
      Guid projectUid = Guid.NewGuid();
      ImportedFileType importedFileType = ImportedFileType.MassHaulPlan;
      DateTime? surfaceUtc = DateTime.UtcNow;
      DateTime fileCreatedUtc = DateTime.UtcNow;
      DateTime fileUpdatedUtc = DateTime.UtcNow;
      string importedBy = "JoeSmoe";

      var error = string.Format(
        $"CreateImportedFileV4.Supplied path {0} is not valid.Either is empty or contains illegal characters.",
        file.path);
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidProjectUid()
    {
      FlowFile file = new FlowFile() {path = "blahblah", flowFilename = "deblah"};
      Guid projectUid = Guid.Empty;
      ImportedFileType importedFileType = ImportedFileType.MassHaulPlan;
      DateTime? surfaceUtc = DateTime.UtcNow;
      DateTime fileCreatedUtc = DateTime.UtcNow;
      DateTime fileUpdatedUtc = DateTime.UtcNow;
      string importedBy = "JoeSmoe";

      var error = string.Format($"The projectUid is invalid {0}.", projectUid);
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error));
    }

    [TestMethod]
    public void ValidateImportFile_UnsupportedImportedFileType()
    {
      FlowFile file = new FlowFile() {path = "blahblah", flowFilename = "deblah"};
      Guid projectUid = Guid.NewGuid();
      ImportedFileType importedFileType = ImportedFileType.Linework;
      DateTime? surfaceUtc = DateTime.UtcNow;
      DateTime fileCreatedUtc = DateTime.UtcNow;
      DateTime fileUpdatedUtc = DateTime.UtcNow;
      string importedBy = "JoeSmoe";

      var error = string.Format(
        "CreateImportedFileV4. ImportedFileType: {0}, is invalid. Only Alignment file types are supported at present",
        importedFileType.ToString());
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidFileExtension()
    {
      FlowFile file = new FlowFile() {path = "blahblah", flowFilename = "deblah.ttm"};
      Guid projectUid = Guid.NewGuid();
      ImportedFileType importedFileType = ImportedFileType.Alignment;
      DateTime? surfaceUtc = DateTime.UtcNow;
      DateTime fileCreatedUtc = DateTime.UtcNow;
      DateTime fileUpdatedUtc = DateTime.UtcNow;
      string importedBy = "JoeSmoe";

      var error = string.Format(
        "CreateImportedFileV4. ImportedFileType {0} does not match the file extension received {1}.",
        importedFileType.ToString(), Path.GetExtension(file.flowFilename));
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidFileCreatedUtc()
    {
      FlowFile file = new FlowFile() { path = "blahblah", flowFilename = "deblah.svl" };
      Guid projectUid = Guid.NewGuid();
      ImportedFileType importedFileType = ImportedFileType.Alignment;
      DateTime? surfaceUtc = DateTime.UtcNow;
      DateTime fileCreatedUtc = DateTime.MinValue;
      DateTime fileUpdatedUtc = DateTime.UtcNow;
      string importedBy = "JoeSmoe";

      var error = string.Format(
        "The fileCreatedUtc {0} is over 30 years old or >2 days in the future (utc).", fileCreatedUtc);
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidFileUpdatedUtc()
    {
      FlowFile file = new FlowFile() { path = "blahblah", flowFilename = "deblah.svl" };
      Guid projectUid = Guid.NewGuid();
      ImportedFileType importedFileType = ImportedFileType.Alignment;
      DateTime? surfaceUtc = DateTime.UtcNow;
      DateTime fileCreatedUtc = DateTime.UtcNow;
      DateTime fileUpdatedUtc = DateTime.MinValue;
      string importedBy = "JoeSmoe";

      var error = string.Format(
        "The fileUpdatedUtc {0} is over 30 years old or >2 days in the future (utc).", fileUpdatedUtc);
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateImportFile_InvalidImportedBy()
    {
      FlowFile file = new FlowFile() { path = "blahblah", flowFilename = "deblah.svl" };
      Guid projectUid = Guid.NewGuid();
      ImportedFileType importedFileType = ImportedFileType.Alignment;
      DateTime? surfaceUtc = DateTime.UtcNow;
      DateTime fileCreatedUtc = DateTime.UtcNow;
      DateTime fileUpdatedUtc = DateTime.UtcNow;
      string importedBy = null;

      var error = string.Format($"The ImportedBy is not available {importedBy}.");
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(error, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ValidateImportFile_HappyPath()
    {
      FlowFile file = new FlowFile() {path = "\\*", flowFilename = "deblah.svl"};
      Guid projectUid = Guid.NewGuid();
      ImportedFileType importedFileType = ImportedFileType.Alignment;
      DateTime? surfaceUtc = null;
      DateTime fileCreatedUtc = DateTime.UtcNow;
      DateTime fileUpdatedUtc = DateTime.UtcNow;
      string importedBy = "JoeSmoe";

      FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
        fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc);
    }
  }
}
