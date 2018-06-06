using System.IO;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  public class CompactionExportResult : ContractExecutionResult
  {
    public string FullFileName { get; private set; }

    private CompactionExportResult()
    {
    }

    public static CompactionExportResult Create(string fullFileName)
    {
      return new CompactionExportResult
      {
        FullFileName = fullFileName
      };
    }
  }
}
