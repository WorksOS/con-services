using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Repositories.DBModels
{
  public class ImportedFile
  {
    public string ProjectUid { get; set; }
    public string ImportedFileUid { get; set; }
    public long ImportedFileId { get; set; }
    public string CustomerUid { get; set; }
    public ImportedFileType ImportedFileType { get; set; }
    public string Name { get; set; }
    public string FileDescriptor { get; set; }
    public DateTime FileCreatedUtc { get; set; }
    public DateTime FileUpdatedUtc { get; set; }
    public string ImportedBy { get; set; }
    public DateTime? SurveyedUtc { get; set; }
    public DxfUnitsType DxfUnitsType { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActivated { get; set; }
    public DateTime LastActionedUtc { get; set; }


    public override bool Equals(object obj)
    {
      var otherImportedFile = obj as ImportedFile;
      if (otherImportedFile == null) return false;
      return otherImportedFile.ProjectUid == ProjectUid
             && otherImportedFile.ImportedFileUid == ImportedFileUid
             && otherImportedFile.ImportedFileId == ImportedFileId
             && otherImportedFile.CustomerUid == CustomerUid
             && otherImportedFile.ImportedFileType == ImportedFileType
             && otherImportedFile.Name == Name
             && otherImportedFile.FileDescriptor == FileDescriptor
             && otherImportedFile.FileCreatedUtc == FileCreatedUtc
             && otherImportedFile.FileUpdatedUtc == FileUpdatedUtc
             && otherImportedFile.ImportedBy == ImportedBy
             && otherImportedFile.IsDeleted == IsDeleted
             && otherImportedFile.IsActivated == IsActivated
             && otherImportedFile.SurveyedUtc == SurveyedUtc
             && otherImportedFile.DxfUnitsType == DxfUnitsType
             && otherImportedFile.LastActionedUtc == LastActionedUtc;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}