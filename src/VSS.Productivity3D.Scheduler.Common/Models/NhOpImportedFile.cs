using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Scheduler.Common.Models
{
  public class NhOpImportedFile
  {
    public long LegacyImportedFileId { get; set; } // autoincrement value

    public long LegacyProjectId { get; set; }

    public string ProjectUid { get; set; } // from joined table

    public long LegacyCustomerId { get; set; }

    public string CustomerUid { get; set; } // from joined table

    public ImportedFileType ImportedFileType { get; set; } // CG and NG use same enums

    public DxfUnitsType DxfUnitsType { get; set; }

    public string Name { get; set; } // CG includes surveyedUtc

    public DateTime? SurveyedUtc { get; set; }

    public DateTime FileCreatedUtc { get; set; }

    public DateTime FileUpdatedUtc { get; set; }

    // todo NHOp has fk+UserID
    public string ImportedBy { get; set; }

    public DateTime LastActionedUtc { get; set; }


    public override bool Equals(object obj)
    {
      NhOpImportedFile importedFile = obj as NhOpImportedFile;
      if (
        importedFile?.LegacyImportedFileId != this.LegacyImportedFileId
        || importedFile.LegacyProjectId != LegacyProjectId
        || importedFile.ProjectUid != this.ProjectUid
        || importedFile.LegacyCustomerId != this.LegacyCustomerId
        || importedFile.CustomerUid != this.CustomerUid
        || importedFile.ImportedFileType != this.ImportedFileType
        || importedFile.DxfUnitsType != this.DxfUnitsType
        || importedFile.Name != this.Name
        || importedFile.SurveyedUtc != this.SurveyedUtc
        || importedFile.FileCreatedUtc != this.FileCreatedUtc
        || importedFile.FileUpdatedUtc != this.FileUpdatedUtc
        || importedFile.ImportedBy != this.ImportedBy
        || importedFile.LastActionedUtc != this.LastActionedUtc
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
