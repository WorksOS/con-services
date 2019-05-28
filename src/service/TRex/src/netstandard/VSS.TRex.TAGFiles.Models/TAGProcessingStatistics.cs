using System.Threading;

namespace VSS.TRex.TAGFiles.Models
{
  public class TAGProcessingStatistics
  {
    private static long totalCellPassesAggregatedIntoModels;
    public static long TotalCellPassesAggregatedIntoModels => Interlocked.Add(ref totalCellPassesAggregatedIntoModels, 0);

    public static void IncrementTotalCellPassesAggregatedIntoModels(long incBy) =>
      Interlocked.Add(ref totalCellPassesAggregatedIntoModels, incBy);

    private static long totalTAGFilesProcessedIntoModels;
    public static long TotalTAGFilesProcessedIntoModels => Interlocked.Add(ref totalTAGFilesProcessedIntoModels, 0);

    public static void IncrementTotalTAGFilesProcessedIntoModels(long incBy) =>
      Interlocked.Add(ref totalTAGFilesProcessedIntoModels, incBy);
  }
}
