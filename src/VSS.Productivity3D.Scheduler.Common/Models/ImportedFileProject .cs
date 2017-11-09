using VSS.MasterData.Repositories.DBModels;

namespace VSS.Productivity3D.Scheduler.Common.Models
{
  public class ImportedFileProject : ImportedFile
  {
    public long LegacyProjectId;

    public long LegacyCustomerId { get; set; }

    public override bool Equals(object obj)
    {
      ImportedFileProject other = obj as ImportedFileProject;
      if (
        other?.LegacyProjectId != LegacyProjectId
        || other.LegacyCustomerId != LegacyCustomerId
        || other.ProjectUid == ProjectUid
        || other.ImportedFileUid == ImportedFileUid
        || other.ImportedFileId == ImportedFileId
        || other.LegacyImportedFileId == LegacyImportedFileId
        || other.CustomerUid == CustomerUid
        || other.ImportedFileType == ImportedFileType
        || other.Name == Name
        || other.FileDescriptor == FileDescriptor
        || other.FileCreatedUtc == FileCreatedUtc
        || other.FileUpdatedUtc == FileUpdatedUtc
        || other.ImportedBy == ImportedBy
        || other.IsDeleted == IsDeleted
        || other.IsActivated == IsActivated
        || other.SurveyedUtc == SurveyedUtc
        || other.DxfUnitsType == DxfUnitsType
        || other.LastActionedUtc == LastActionedUtc
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