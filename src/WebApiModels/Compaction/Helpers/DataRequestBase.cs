using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public abstract class DataRequestBase
  {
    protected ILogger Log;
    protected IConfigurationStore ConfigurationStore;
    protected IFileListProxy FileListProxy;
    protected ICompactionSettingsManager SettingsManager;

    protected long ProjectId;
    protected IDictionary<string, string> Headers;
    protected CompactionProjectSettings ProjectSettings;
    protected CompactionProjectSettingsColors ProjectSettingsColors;
    protected FilterResult Filter;
    protected DesignDescriptor DesignDescriptor;

    public void Initialize(ILogger log, IConfigurationStore configurationStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager, long projectId, CompactionProjectSettings projectSettings, CompactionProjectSettingsColors projectSettingsColors, IDictionary<string, string> headers, FilterResult filter, DesignDescriptor designDescriptor)
    {
      filter?.Validate(); // Should be moved to FilterResult.CreateFilter().

      Log = log;
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;

      ProjectId = projectId;
      Headers = headers;
      ProjectSettings = projectSettings;
      ProjectSettingsColors = projectSettingsColors;
      Filter = filter;
      DesignDescriptor = designDescriptor;
    }
  }
}
