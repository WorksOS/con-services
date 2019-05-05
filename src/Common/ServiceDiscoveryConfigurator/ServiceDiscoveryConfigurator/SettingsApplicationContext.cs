using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using ServiceDiscoveryConfigurator.Properties;
using VSS.Common.Abstractions.ServiceDiscovery.Constants;
using VSS.Common.ServiceDiscovery.Settings;

namespace ServiceDiscoveryConfigurator
{
  public class SettingsApplicationContext : ApplicationContext
  {
    private readonly NotifyIcon trayIcon;

    private DevelopmentSettings developmentSettings;

    public SettingsApplicationContext ()
    {
      LoadSettings();

      // Initialize Tray Icon
      trayIcon = new NotifyIcon()
      {
        Icon = Resources.AppIcon,
        ContextMenu = new ContextMenu(GetMenuItems()),
        Visible = true
      };


      FileSystemWatcher watcher = new FileSystemWatcher
      {
        Path = Path.GetDirectoryName(DevelopmentSettings.Filename),
        
        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
        Filter = "*.json"
      };
    
      // Add event handlers.
      watcher.Changed += (_, __) =>
      {
        LoadSettings();
        ReloadSettings();
      };

      // Begin watching.
      watcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// Load the existing settings into memory from settings file if it exists
    /// If the settings file doesn't exists, create it
    /// </summary>
    private void LoadSettings()
    {
      if (File.Exists(DevelopmentSettings.Filename))
      {
        // https://stackoverflow.com/questions/7260792/filesystemwatcher-ioexception
        while(IsFileLocked(new FileInfo(DevelopmentSettings.Filename)))
          Thread.Sleep(100);

        var data = File.ReadAllText(DevelopmentSettings.Filename);

        try
        {
          developmentSettings = JsonConvert.DeserializeObject<DevelopmentSettings>(data);
        }
        catch (JsonException e)
        {
          MessageBox.Show($"Failed to read settings, due to error : {e.Message}. Delete the file or fix the file and rerun",
            "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
          Environment.Exit(1);
        }
      }

      else
      {
        // Create a new file
        CreateNewSettings();
        SaveSettings();
      }
    }

    /// <summary>
    /// Create a new in memory settings file, with what we know as the default values now.
    /// </summary>
    private void CreateNewSettings()
    {
      developmentSettings = new DevelopmentSettings();
      developmentSettings.AvailableSettings.Add(ServiceNameConstants.FILTER_SERVICE,
        new SettingsEntry()
        {
          IsInternal = true,
          Options = new List<DisplaySetting>()
          {
            new DisplaySetting()
            {
              Name = "Dev",
              Value = "http://filter.dev.eks.vspengg.com/"
            },
            new DisplaySetting()
            {
              Name = "Alpha",
              Value = "http://filter.alpha.eks.vspengg.com/"
            }
          }
        });

      developmentSettings.AvailableSettings.Add(ServiceNameConstants.PRODUCTIVITY_3D_SERVICE,
        new SettingsEntry()
        {
          IsInternal = true,
          Options = new List<DisplaySetting>()
          {
            new DisplaySetting()
            {
              Name = "Dev",
              Value = "http://10.97.96.42:5001/"
            },
            new DisplaySetting()
            {
              Name = "Alpha",
              Value = "http://internal-merinoraptor-balancer-1570378562.us-west-2.elb.amazonaws.com/"
            }
          }
        });

      developmentSettings.AvailableSettings.Add(ServiceNameConstants.PROJECT_SERVICE,
        new SettingsEntry()
        {
          IsInternal = true,
          Options = new List<DisplaySetting>()
          {
            new DisplaySetting()
            {
              Name = "Dev",
              Value = "http://project.dev.eks.vspengg.com/"
            },
            new DisplaySetting()
            {
              Name = "Alpha",
              Value = "http://project.alpha.eks.vspengg.com/"
            }
          }
        });

      developmentSettings.AvailableSettings.Add(ServiceNameConstants.SCHEDULER_SERVICE,
        new SettingsEntry()
        {
          IsInternal = true,
          Options = new List<DisplaySetting>()
          {
            new DisplaySetting()
            {
              Name = "Dev",
              Value = "http://scheduler.dev.eks.vspengg.com/"
            },
            new DisplaySetting()
            {
              Name = "Alpha",
              Value = "http://scheduler.alpha.eks.vspengg.com/"
            }
          }
        });

      developmentSettings.AvailableSettings.Add(ServiceNameConstants.PUSH_SERVICE,
        new SettingsEntry()
        {
          IsInternal = true,
          Options = new List<DisplaySetting>()
          {
            new DisplaySetting()
            {
              Name = "Dev",
              Value = "http://push.dev.eks.vspengg.com/"
            },
            new DisplaySetting()
            {
              Name = "Alpha",
              Value = "http://push.alpha.eks.vspengg.com/"
            }
          }
        });

      developmentSettings.AvailableSettings.Add(ServiceNameConstants.TILE_SERVICE,
        new SettingsEntry()
        {
          IsInternal = true,
          Options = new List<DisplaySetting>()
          {
            new DisplaySetting()
            {
              Name = "Dev",
              Value = "http://tile.dev.eks.vspengg.com"
            },
            new DisplaySetting()
            {
              Name = "Alpha",
              Value = "http://tile.alpha.eks.vspengg.com"
            }
          }
        });

      developmentSettings.AvailableSettings.Add(ServiceNameConstants.ASSETMGMT3D_SERVICE,
        new SettingsEntry()
        {
          IsInternal = true,
          Options = new List<DisplaySetting>()
          {
            new DisplaySetting()
            {
              Name = "Dev",
              Value = "http://assetmgmt3d.dev.eks.vspengg.com"
            },
            new DisplaySetting()
            {
              Name = "Alpha",
              Value = "http://assetmgmt3d.alpha.eks.vspengg.com"
            }
          }
        });
    }

    /// <summary>
    /// Save the current settings to the settings file as JSON
    /// </summary>
    private void SaveSettings()
    {
      if (developmentSettings == null)
        return;
      var json = JsonConvert.SerializeObject(developmentSettings, Formatting.Indented);

      var path = new FileInfo(DevelopmentSettings.Filename).Directory;
      if (!path.Exists)
        path.Create();

      File.WriteAllText(DevelopmentSettings.Filename, json); // overwrites the file
    }

    /// <summary>
    /// Reload the settings, and rebuild the context menu
    /// </summary>
    private void ReloadSettings()
    {
      LoadSettings();
      trayIcon.ContextMenu = new ContextMenu(GetMenuItems());
    }

    /// <summary>
    /// Get a Menu Item for a particular Service (includes sub items to represent each available option for that service)
    /// </summary>
    private MenuItem GetMenuItem(string key)
    {
      var settings = developmentSettings.AvailableSettings[key];

      var menuItem = new MenuItem(key);

      developmentSettings.SelectedSettings.TryGetValue(key, out var selectedServer);

      var foundSelectedSetting = false;

      foreach (var setting in settings.Options)
      {
        var item = new MenuItem($"{setting.Name} ({setting.Value}", OnSelect)
        {
          Tag = new KeyValuePair<string, DisplaySetting>(key, setting)
        };

        if(string.Compare( setting.Value, selectedServer, StringComparison.Ordinal) == 0)
        {
          foundSelectedSetting = true;
          item.Checked = true;
        }

        menuItem.MenuItems.Add(item);
      }

      if (!foundSelectedSetting && !string.IsNullOrEmpty(selectedServer))
      {
        var selectedMenuItem = new MenuItem($"Selected {selectedServer}")
        {
          Enabled = false,
          Checked = true
        };
        menuItem.MenuItems.Add(selectedMenuItem);
      }

      menuItem.MenuItems.Add(new MenuItem("-"));

      var customMenuItem = new MenuItem("Custom", OnCreateCustom)
      {
        Tag = key
      };

      menuItem.MenuItems.Add(customMenuItem);


      return menuItem;
    }

    /// <summary>
    /// Build the list of menu items used in the tray
    /// </summary>
    private MenuItem[] GetMenuItems()
    {
      var results = new List<MenuItem>();

      if (developmentSettings != null)
      {
        var internalKeys = developmentSettings
          .AvailableSettings
          .Keys
          .Where(k => developmentSettings.AvailableSettings[k].IsInternal)
          .OrderBy(k => k)
          .ToList();

        var externalKeys = developmentSettings
          .AvailableSettings
          .Keys
          .Where(k => !developmentSettings.AvailableSettings[k].IsInternal)
          .OrderBy(k => k)
          .ToList();

        results.Add(new MenuItem("Internal") {Enabled = false});
        results.AddRange(internalKeys.Select(GetMenuItem));
        results.Add(new MenuItem("-"));
        results.Add(new MenuItem("External") {Enabled = false});
        results.AddRange(externalKeys.Select(GetMenuItem));
      }
      
      results.Add(new MenuItem("-"));
      results.Add(new MenuItem("Set All To Dev", OnSetAll)
      {
        Tag = "Dev"
      });
      results.Add(new MenuItem("Set All To Alpha", OnSetAll)
      {
        Tag = "Alpha"
      });

      results.Add(new MenuItem("-"));
      results.Add(new MenuItem("Exit", Exit));
      return results.ToArray();
    }

    /// <summary>
    /// Helper method to see if a file is locked when using a file system water
    /// </summary>
    private static bool IsFileLocked(FileInfo file)
    {
      FileStream stream = null;

      try
      {
        stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
      }
      catch (IOException)
      {
        //the file is unavailable because it is:
        //still being written to
        //or being processed by another thread
        //or does not exist (has already been processed)
        return true;
      }
      finally
      {
        if (stream != null)
          stream.Close();
      }

      //file is not locked
      return false;
    }

    /// <summary>
    /// Select a predefined value for a service
    /// </summary>
    private void OnSelect(object sender, EventArgs e)
    {
      if (sender is MenuItem menuItem)
      {
        if (menuItem.Tag is KeyValuePair<string, DisplaySetting> setting)
        {
          developmentSettings.SelectedSettings[setting.Key] = setting.Value.Value;
          SaveSettings();
          ReloadSettings();
        }
      }
    }

    /// <summary>
    /// Create a custom value for a service
    /// </summary>
    private void OnCreateCustom(object sender, EventArgs e)
    {
      if (!(sender is MenuItem menuItem)) 
        return;
      if (!(menuItem.Tag is string key)) 
        return;

      developmentSettings.SelectedSettings.TryGetValue(key, out var currentValue);

      var server = FormCustomServer.GetSetting(key, currentValue);
      if (string.IsNullOrEmpty(server)) 
        return;
      developmentSettings.SelectedSettings[key] = server;
            
      SaveSettings();
      ReloadSettings();
    }

    /// <summary>
    /// Set all services to a particular environment value
    /// </summary>
    private void OnSetAll(object sender, EventArgs e)
    {
      if (!(sender is MenuItem menuItem)) 
        return;
      if (!(menuItem.Tag is string name)) 
        return;

      foreach (var developmentSetting in developmentSettings.AvailableSettings)
      {
        foreach (var displaySetting in developmentSetting.Value.Options)
        {
          if (string.Compare(displaySetting.Name, name, StringComparison.Ordinal) != 0) 
            continue;

          // We have a setting with this name (e.g Dev, Alpha) set it as our active
          developmentSettings.SelectedSettings[developmentSetting.Key] = displaySetting.Value;
          break;
        }
      }

      SaveSettings();
      ReloadSettings();
    }

    /// <summary>
    /// Event event
    /// </summary>
    private void Exit(object sender, EventArgs e)
    {
      // Hide tray icon, otherwise it will remain shown until user mouses over it
      trayIcon.Visible = false;

      Application.Exit();
    }
  }
}
