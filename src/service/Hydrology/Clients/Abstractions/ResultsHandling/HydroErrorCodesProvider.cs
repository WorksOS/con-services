using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Abstractions.ResultsHandling
{
  public class HydroErrorCodesProvider : ContractExecutionStatesEnum
  {
    public HydroErrorCodesProvider()
    {
      this.DynamicAddwithOffset("Invalid ProjectUid.", 1);
      this.DynamicAddwithOffset("Invalid FilterUid.", 2);
      this.DynamicAddwithOffset("Must have a valid resultant zip file name.", 3);
      this.DynamicAddwithOffset("Resolution must be between 0.005 and < 1,000,000.", 4);
      this.DynamicAddwithOffset("Current ground design has too few TIN entities, must have at least 3.", 5);
      this.DynamicAddwithOffset("TTM conversion failed. triangleCount differs with dxf", 6);
      this.DynamicAddwithOffset("Failed to zip images.", 7);
    }
  }
}

