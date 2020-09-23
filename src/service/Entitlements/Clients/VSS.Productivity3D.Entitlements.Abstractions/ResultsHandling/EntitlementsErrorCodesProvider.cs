using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Entitlements.Abstractions.ResultsHandling
{
  public class EntitlementsErrorCodesProvider : ContractExecutionStatesEnum
  {
    public EntitlementsErrorCodesProvider()
    {
      this.DynamicAddwithOffset("Missing User.", 2);
      this.DynamicAddwithOffset("Missing User email.", 3);
      this.DynamicAddwithOffset("Missing User ID.", 4);
      this.DynamicAddwithOffset("Provided email does not match JWT.", 5);
      this.DynamicAddwithOffset("Provided uuid does not match JWT.", 6);
      this.DynamicAddwithOffset("No Organization Identifier provided.", 7);
    }
  }
}
