using System;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class ImportedFileHistoryItem
  {
    public DateTime FileCreatedUtc { get; set; }
    public DateTime FileUpdatedUtc { get; set; }

    public override bool Equals(object obj)
    {
      if (!(obj is ImportedFileHistoryItem otherImportedFileImport))
      {
        return false;
      }

      return otherImportedFileImport.FileCreatedUtc == FileCreatedUtc
             && otherImportedFileImport.FileUpdatedUtc == FileUpdatedUtc;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}
