using System;

namespace CCSS.Productivity3D.Preferences.Abstractions.Models.Database
{
  public class UserPreferenceKey
  {
    public Guid? PreferenceKeyUID { get; set; }
    public string KeyName { get; set; }
    public string PreferenceJson { get; set; }//value
    public string SchemaVersion { get; set; }
  }
}
