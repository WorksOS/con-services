using System;
using LiteDB;

namespace TCCToDataOcean.DatabaseAgent
{
  public class MigrationObj
  {
    [BsonIgnore]
    public string TableName { get; protected set; }

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
