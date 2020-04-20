using System;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling
{
  public class PreferenceKeyV1Result : ContractExecutionResult
  {
    public string PreferenceKeyName { get; set; }
    public Guid PreferenceKeyUID { get; set; }
  }
}
