using VSS.MasterData.Repositories.DBModels;

namespace VSS.Productivity3D.Scheduler.Common.Models
{
  public class ProjectImportedFile : ImportedFile
  {
    public long LegacyProjectId;

    public long LegacyCustomerId { get; set; }

    public override bool Equals(object obj)
    {
      ProjectImportedFile otherImportedFile = obj as ProjectImportedFile;
      if (
        otherImportedFile?.LegacyProjectId != LegacyProjectId
        || otherImportedFile.LegacyCustomerId != LegacyCustomerId
        || otherImportedFile.ProjectUid == ProjectUid
        || otherImportedFile.ImportedFileUid == ImportedFileUid
        || otherImportedFile.ImportedFileId == ImportedFileId
        || otherImportedFile.LegacyImportedFileId == LegacyImportedFileId
        || otherImportedFile.CustomerUid == CustomerUid
        || otherImportedFile.ImportedFileType == ImportedFileType
        || otherImportedFile.Name == Name
        || otherImportedFile.FileDescriptor == FileDescriptor
        || otherImportedFile.FileCreatedUtc == FileCreatedUtc
        || otherImportedFile.FileUpdatedUtc == FileUpdatedUtc
        || otherImportedFile.ImportedBy == ImportedBy
        || otherImportedFile.IsDeleted == IsDeleted
        || otherImportedFile.IsActivated == IsActivated
        || otherImportedFile.SurveyedUtc == SurveyedUtc
        || otherImportedFile.DxfUnitsType == DxfUnitsType
        || otherImportedFile.LastActionedUtc == LastActionedUtc
      )
        return false;
      return true;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}