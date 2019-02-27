namespace TCCToDataOcean.DatabaseAgent
{
  public enum MigrationState
  {
    Unknown,
    Pending,
    InProgress,
    Completed,
    Failed,
    FileNotFound
  }
}
