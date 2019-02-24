namespace TCCToDataOcean.DatabaseAgent
{
  public class MigrationErrorMessage : MigrationObj
  {
    public string ProjectUid { get; set; }
    public string Error { get; set; }

    public MigrationErrorMessage()
    { }

    public MigrationErrorMessage(string projectUid, string errorMessage)
    {
      ProjectUid = projectUid;
      Error = errorMessage;
    }
  }
}
