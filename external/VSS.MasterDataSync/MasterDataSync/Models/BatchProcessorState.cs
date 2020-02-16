namespace VSS.Nighthawk.MasterDataSync.Models
{
  public enum BatchProcessorState
  {
    NoRecordsToProcess,
    RecordsExists_FailedToProcess,
    RecordExists_DependentEventsNotProcessed,
    MoreRecordsToProcess,
    AllRecordsProcessedSuccessfully
  };
}
