using Apache.Ignite.Core.Cache.Configuration;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    /// <summary>
    /// Details a segment within the data store that has been replaced by a new version of a segment or cloven
    /// segments as a result of TAG file processing. This queue tracks such entries until they are no longer needed to 
    /// support consistency window semantics during queries
    /// </summary>
    public class SegmentRetirementQueueItem
    {
        /// <summary>
        /// A key field (a time) set up as an ordered (ascending) index
        /// </summary>
        [QuerySqlField(IsIndexed = true)]
        public long Date { get; set; }

        public string Value { get; set; }

        public SegmentRetirementQueueItem(long date, string value)
        {
            Date = date;
            Value = value;
        }
    }
}
