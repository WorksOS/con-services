using System;

namespace Repositories.DBModels
{
  public class ImportedFile
  {
    public string ProjectUid { get; set; }
    public string ImportedFileUid { get; set; }
    public string CustomerUid { get; set; }
    public ImportedFileType ImportedFileType { get; set; }
    public string Name { get; set; }
    public string FileDescriptor { get; set; }
    public DateTime FileCreatedUtc { get; set; }
    public DateTime FileUpdatedUtc { get; set; }
    public string ImportedBy { get; set; }
    public DateTime? SurveyedUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime LastActionedUtc { get; set; }


    public override bool Equals(object obj)
    {
      var otherImportedFile = obj as ImportedFile;
      if (otherImportedFile == null) return false;
      return otherImportedFile.ProjectUid == this.ProjectUid
             && otherImportedFile.ImportedFileUid == this.ImportedFileUid
             && otherImportedFile.CustomerUid == this.CustomerUid 
             && otherImportedFile.ImportedFileType == this.ImportedFileType
             && otherImportedFile.Name == this.Name
             && otherImportedFile.FileDescriptor == this.FileDescriptor
             && otherImportedFile.FileCreatedUtc == this.FileCreatedUtc
             && otherImportedFile.FileUpdatedUtc == this.FileUpdatedUtc
             && otherImportedFile.ImportedBy == this.ImportedBy
             && otherImportedFile.IsDeleted == this.IsDeleted
             && otherImportedFile.SurveyedUtc == this.SurveyedUtc
             && otherImportedFile.LastActionedUtc == this.LastActionedUtc
        ;
    }

    public override int GetHashCode()
    {
      return 0;
    }

  }
}
