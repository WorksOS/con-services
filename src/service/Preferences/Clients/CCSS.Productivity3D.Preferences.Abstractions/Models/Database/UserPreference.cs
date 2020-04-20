using System;

namespace CCSS.Productivity3D.Preferences.Abstractions.Models.Database
{
  public class UserPreference
  {
    public long UserPreferenceID { get; set; }
    public string UserUID { get; set; }
    public long PreferenceKeyID { get; set; }
    public string PreferenceJson { get; set; }//value
    public string SchemaVersion { get; set; }
  }
}
