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
    public DateTime? SurveyedUtc { get; set; }
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
