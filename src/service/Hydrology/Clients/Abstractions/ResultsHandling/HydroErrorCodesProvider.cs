using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Abstractions.ResultsHandling
{
  public class HydroErrorCodesProvider : ContractExecutionStatesEnum
  {
    public HydroErrorCodesProvider()
    {
      this.DynamicAddwithOffset("Invalid ProjectUid.", 1);
      this.DynamicAddwithOffset("Filter not supported at yet.", 2);
      this.DynamicAddwithOffset("Resolution must be > 0 and < 1,000,000.", 3);
      this.DynamicAddwithOffset("Must have a resultant file name.", 4);
    }
  }
}

