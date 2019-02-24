using System;

namespace TCCToDataOcean.DatabaseAgent
{
  public class MigrationObj
  {
    public int Id { get; set; }
    public DateTime DateTimeCreated { get; set; }
    public DateTime DateTimeUpdated { get; set; }

    public MigrationObj()
    {
      DateTimeCreated = DateTime.UtcNow;
      DateTimeUpdated = DateTimeCreated;
    }
  }
}
