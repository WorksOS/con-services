using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling
{
  public class PreferenceErrorCodesProvider : ContractExecutionStatesEnum
  {
    public PreferenceErrorCodesProvider()
    {
      this.DynamicAddwithOffset("", 1);
    }
  }
}
