using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes
{
  public class TAGFileProcessingHeartBeatLogger
  {
    public override string ToString()
    {
      return $"#TAGFilesProcessed {TAGProcessingStatistics.TotalTAGFilesProcessedIntoModels} #CellPassesProcessed: {TAGProcessingStatistics.TotalCellPassesAggregatedIntoModels}";
    }
  }
}
