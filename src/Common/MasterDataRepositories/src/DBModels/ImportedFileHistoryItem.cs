using System;

namespace VSS.MasterData.Repositories.DBModels
{
  public class ImportedFileHistoryItem
  {
    public string ImportedFileUid { get; set; }
    public DateTime FileCreatedUtc { get; set; }
    public DateTime FileUpdatedUtc { get; set; }

    // if UserID is an an ApplicationContext name we don't be able to obtain email from UserUID
    public string ImportedBy { get; set; }

    public override bool Equals(object obj)
    {
      var otherImportedFileImport = obj as ImportedFileHistoryItem;
      if (otherImportedFileImport == null) return false;
      return otherImportedFileImport.ImportedFileUid == ImportedFileUid
             && otherImportedFileImport.FileCreatedUtc == FileCreatedUtc
             && otherImportedFileImport.FileUpdatedUtc == FileUpdatedUtc
             && otherImportedFileImport.ImportedBy == ImportedBy; 
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}