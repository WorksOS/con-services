using System;
using System.IO;
using FlowUploadFilter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectWebApiCommon.Models;
using ProjectWebApiCommon.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace MasterDataConsumerTests
{
  [TestClass]
  public class ImportFileTests
  {
    protected ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();

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

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(contractExecutionStatesEnum.FirstNameWithOffset(27)));
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

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(contractExecutionStatesEnum.FirstNameWithOffset(28)));
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

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(contractExecutionStatesEnum.FirstNameWithOffset(29)));
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
      
      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(contractExecutionStatesEnum.FirstNameWithOffset(5)));
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

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(contractExecutionStatesEnum.FirstNameWithOffset(31)));
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

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(contractExecutionStatesEnum.FirstNameWithOffset(32)));
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

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(contractExecutionStatesEnum.FirstNameWithOffset(33)));
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

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(contractExecutionStatesEnum.FirstNameWithOffset(34)));
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

      var ex = Assert.ThrowsException<ServiceException>(
        () => FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType,
          fileCreatedUtc, fileUpdatedUtc, importedBy, surfaceUtc));
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(contractExecutionStatesEnum.FirstNameWithOffset(35)));
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
