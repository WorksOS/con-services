using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace TCCToDataOcean.Types
{
  public class MigrationErrorCodesProvider : ContractExecutionStatesEnum
  {
    public MigrationErrorCodesProvider()
    {
      DynamicAddwithOffset("Unable to obtain Project Web API URL.", 1);
      DynamicAddwithOffset("Unable to obtain Temporary folder name.", 2);
      DynamicAddwithOffset("Unable to obtain Imported File Web API URL.", 3);
      DynamicAddwithOffset("Unable to obtain TCC fileSpaceId.", 48);
      DynamicAddwithOffset("GetCoordinateSystemFromFileRepo: Exception reading file {0}.", 79);
      DynamicAddwithOffset("GetCoordinateSystemFromFileRepo: Returned file invalid {0}.", 80);
    }
  }
}
