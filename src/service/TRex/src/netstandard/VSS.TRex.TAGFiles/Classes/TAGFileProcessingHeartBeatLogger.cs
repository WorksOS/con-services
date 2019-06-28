using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.TAGFiles.Classes
{
  public class TAGFileProcessingHeartBeatLogger : IHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<TAGFileProcessingHeartBeatLogger>();

    public void HeartBeat()
    {
      Log.LogInformation("Heartbeat: " + ToString());
    }

    public override string ToString()
    {
      return $"#TAGFilesProcessed {TAGProcessingStatistics.TotalTAGFilesProcessedIntoModels} #CellPassesProcessed: {TAGProcessingStatistics.TotalCellPassesAggregatedIntoModels}";
    }
  }
}
