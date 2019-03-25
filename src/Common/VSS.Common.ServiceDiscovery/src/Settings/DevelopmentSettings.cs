using System;
using System.Collections.Generic;
using System.IO;

namespace VSS.Common.ServiceDiscovery.Settings
{
  public class DisplaySetting
  {
    public string Name { get;set; }

    public string Value { get; set; }
  }

  public class SettingsEntry
  {
    public SettingsEntry()
    {
      Options = new List<DisplaySetting>();
    }

    public bool IsInternal { get; set; }

    public List<DisplaySetting> Options { get; set; }
  }

  public class DevelopmentSettings
  {
    public DevelopmentSettings()
    {
      AvailableSettings = new Dictionary<string, SettingsEntry>();
      SelectedSettings = new Dictionary<string, string>();
    }
    public Dictionary<string, SettingsEntry> AvailableSettings { get; }

    public Dictionary<string, string> SelectedSettings { get; }

    public static string Filename =>   Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
      "VSS",
      "services.json");
  }
}