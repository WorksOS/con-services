using System.Collections.Generic;

namespace VSS.MasterData.Repositories.DBModels
{
  public class ImportedFileHistory
  {
    // ImportedFile includes the most recent one. There must be at least 1 - for the initial create.

    public List<ImportedFileHistoryItem> ImportedFileHistoryItems { get; set; }

    public ImportedFileHistory(List<ImportedFileHistoryItem> importedFileHistoryItems)
    {
      ImportedFileHistoryItems = importedFileHistoryItems;
    }

    public override bool Equals(object obj)
    {
      var otherImportedFileHistory = obj as ImportedFileHistory;

      if ((ImportedFileHistoryItems == null) != (otherImportedFileHistory?.ImportedFileHistoryItems == null)) return false;
      if (ImportedFileHistoryItems == null || (otherImportedFileHistory?.ImportedFileHistoryItems == null))
        return true;
      if (ImportedFileHistoryItems.Count != otherImportedFileHistory.ImportedFileHistoryItems.Count) return false;

      for (int i = 0; i < otherImportedFileHistory.ImportedFileHistoryItems.Count; i++)
      {
        bool isOk = ImportedFileHistoryItems[i].Equals(otherImportedFileHistory.ImportedFileHistoryItems[i]);
        if (!isOk)
          return false;
      }
      return true;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}