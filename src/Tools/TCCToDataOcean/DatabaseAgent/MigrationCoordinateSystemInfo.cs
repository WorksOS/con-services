using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean.DatabaseAgent
{
  public class MigrationCoordinateSystemInfo : MigrationObj
  {
    public string ProjectUid { get; set; }
    public DxfUnitsType DxfUnitsType { get; set; }
    public string ProjectionTypeCode { get; set; }
    public string ProjectionName { get; set; }

    public MigrationCoordinateSystemInfo()
    { 
      TableName = Table.CoordinateSystemInfo;
    }
  }
}
