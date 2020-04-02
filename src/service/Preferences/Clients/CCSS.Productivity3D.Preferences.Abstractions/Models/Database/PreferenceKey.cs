using System;
using System.Collections.Generic;
using System.Text;

namespace CCSS.Productivity3D.Preferences.Abstractions.Models.Database
{
  public class PreferenceKey
  {
    public long PreferenceKeyID { get; set; }
    public Guid? PreferenceKeyUID { get; set; }
    public string KeyName { get; set; }
  }
}
