using System;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling
{
  public class UserPreferenceV1Result : PreferenceKeyV1Result
  {
    public string PreferenceJson { get; set; }
    public string SchemaVersion { get; set; }
  }
}
