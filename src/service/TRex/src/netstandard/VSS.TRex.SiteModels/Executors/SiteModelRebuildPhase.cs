namespace VSS.TRex.SiteModels.Executors
{
  public enum SiteModelRebuildPhase
  {
    Unknown,

    /// <summary>
    /// The initial deletion phase is in progress
    /// </summary>
    Deleting,

    /// <summary>
    /// TAG files are currently being submitted into the project
    /// </summary>
    Submitting,

    /// <summary>
    /// All TAG files have been submitted, and monitoring of progress is underway
    /// </summary>
    Monitoring,

    /// <summary>
    /// All site model rebuild operations are complete
    /// </summary>
    Complete
  }
}
