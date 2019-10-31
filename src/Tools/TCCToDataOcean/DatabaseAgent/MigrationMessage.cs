namespace TCCToDataOcean.DatabaseAgent
{
  public class MigrationMessage : MigrationObj
  {
    public string ProjectUid { get; set; }
    public string Message { get; set; }

    public MigrationMessage()
    { }

    public MigrationMessage(string projectUid, string errorMessage)
    {
      ProjectUid = projectUid;
      Message = errorMessage;
    }
  }
}
