using System;
using System.Collections.Generic;
using System.Linq;

namespace VSS.MasterData.Repositories.DBModels
{
  public class ImportedFileHistory
  {
    // ImportedFile includes the most recent one. There must be at least 1 - for the initial create.

    public List<ImportedFileUpsert> ImportedFileUpsertList { get; set; }

    public ImportedFileHistory(List<ImportedFileUpsert> importedFileUpsertList)
    {
      ImportedFileUpsertList = importedFileUpsertList;
    }

    public override bool Equals(object obj)
    {
      var otherImportedFileHistory = obj as ImportedFileHistory;

      if (otherImportedFileHistory == null || !otherImportedFileHistory.ImportedFileUpsertList.Any()) return false;
      if (otherImportedFileHistory.ImportedFileUpsertList.Any() != ImportedFileUpsertList.Any()) return false;
      if (otherImportedFileHistory.ImportedFileUpsertList.Count != ImportedFileUpsertList.Count()) return false;

      for (int i = 0; i < otherImportedFileHistory.ImportedFileUpsertList.Count; i++)
      {
        // todo do this with the ImportedFileUpsert
        bool isOk = true;
        isOk = otherImportedFileHistory.ImportedFileUpsertList[i].ImportedFileUid == ImportedFileUpsertList[i].ImportedFileUid
               && otherImportedFileHistory.ImportedFileUpsertList[i].FileCreatedUtc == ImportedFileUpsertList[i].FileCreatedUtc
               && otherImportedFileHistory.ImportedFileUpsertList[i].FileUpdatedUtc == ImportedFileUpsertList[i].FileUpdatedUtc
               && otherImportedFileHistory.ImportedFileUpsertList[i].ImportedBy == ImportedFileUpsertList[i].ImportedBy;
        if (isOk == false)
          return false;
      }
      return true;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }

  public class ImportedFileUpsert
  {
    public string ImportedFileUid { get; set; }
    public DateTime FileCreatedUtc { get; set; }
    public DateTime FileUpdatedUtc { get; set; }

    // if UserID is an an ApplicationContext name we don't be able to obtain email from UserUID
    public string ImportedBy { get; set; }

    // may contain an ApplicationContext name or UserGuid
    public string UserID { get; set; }

    public override bool Equals(object obj)
    {
      var otherImportedFileImport = obj as ImportedFileUpsert;
      if (otherImportedFileImport == null) return false;
      return otherImportedFileImport.ImportedFileUid == ImportedFileUid
             && otherImportedFileImport.FileCreatedUtc == FileCreatedUtc
             && otherImportedFileImport.FileUpdatedUtc == FileUpdatedUtc
             && otherImportedFileImport.ImportedBy == ImportedBy
             && otherImportedFileImport.UserID == UserID; 
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}