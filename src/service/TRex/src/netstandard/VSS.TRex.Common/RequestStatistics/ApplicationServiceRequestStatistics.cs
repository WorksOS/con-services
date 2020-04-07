namespace VSS.TRex.Common.RequestStatistics
{
  /// <summary>
  /// Tracks statistics relating to the number and variety of requests being serviced.
  /// </summary>
  public class ApplicationServiceRequestStatistics
  {
    /// <summary>
    /// Singleton instance of the application request statistics
    /// </summary>
    public static ApplicationServiceRequestStatistics Instance = new ApplicationServiceRequestStatistics();

    public StatisticsElement NumSimpleVolumeRequests = new StatisticsElement();
    public StatisticsElement NumSimpleVolumeRequestsFailed = new StatisticsElement();
    public StatisticsElement NumSimpleVolumeRequestsCompleted = new StatisticsElement();
    public StatisticsElement NumProgressiveVolumeRequests = new StatisticsElement();
    public StatisticsElement NumProgressiveVolumeRequestsFailed = new StatisticsElement();
    public StatisticsElement NumProgressiveVolumeRequestsCompleted = new StatisticsElement();
    public StatisticsElement NumMapTileRequests = new StatisticsElement();
    public StatisticsElement NumMapTileRequestsFailed = new StatisticsElement();
    public StatisticsElement NumMapTileRequestsCompleted = new StatisticsElement();
    public StatisticsElement NumMapTileRequestsCancelled = new StatisticsElement();
    public StatisticsElement NumProfileRequests = new StatisticsElement();
    public StatisticsElement NumProfileRequestsFailed = new StatisticsElement();
    public StatisticsElement NumProfileRequestsCompleted = new StatisticsElement();
    public StatisticsElement NumCellPassRequests = new StatisticsElement();
    public StatisticsElement NumCellPassRequestsFailed = new StatisticsElement();
    public StatisticsElement NumCellPassRequestsCompleted = new StatisticsElement();
    public StatisticsElement NumCellProfileRequests = new StatisticsElement();
    public StatisticsElement NumCellProfileRequestsFailed = new StatisticsElement();
    public StatisticsElement NumCellProfileRequestsCompleted = new StatisticsElement();
    public StatisticsElement NumSurfaceExportRequests = new StatisticsElement();
    public StatisticsElement NumSurfaceExportFailed = new StatisticsElement();
    public StatisticsElement NumSurfaceExportCompleted = new StatisticsElement();
    public StatisticsElement NumSurfaceExportCancelled = new StatisticsElement();
    public StatisticsElement NumSubgridPageRequests = new StatisticsElement();
    public StatisticsElement NumSubgridPageRequestsFailed = new StatisticsElement();
    public StatisticsElement NumSubgridPageRequestsCompleted = new StatisticsElement();
    public StatisticsElement NumSubgridPageRequestsCancelled = new StatisticsElement();
    public StatisticsElement NumStatisticsRequests = new StatisticsElement();
    public StatisticsElement NumStatisticsRequestsFailed = new StatisticsElement();
    public StatisticsElement NumStatisticsRequestsCompleted = new StatisticsElement();
    public StatisticsElement NumQMTileRequests = new StatisticsElement();

    // No-args constructor
    public ApplicationServiceRequestStatistics()
    {
    }
  }
}
