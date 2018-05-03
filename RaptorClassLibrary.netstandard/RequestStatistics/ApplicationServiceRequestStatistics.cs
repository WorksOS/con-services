namespace VSS.TRex.RequestStatistics
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

        public struct StatisticsElement
        {
            private long value;
            // ReSharper disable once ConvertToAutoPropertyWhenPossible
            public long Value { get => value; }

            public void Increment() => System.Threading.Interlocked.Increment(ref value);
        }

        public StatisticsElement NumVolumeRequests = new StatisticsElement();
        public StatisticsElement NumVolumeRequestsFailed = new StatisticsElement();
        public StatisticsElement NumVolumeRequestsCompleted = new StatisticsElement();
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

        // No-args constructor
        public ApplicationServiceRequestStatistics()
        {
        }
    }
}
