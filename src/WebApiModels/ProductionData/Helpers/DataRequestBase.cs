using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Helpers
{
  public abstract class DataRequestBase
  {
    public ILogger log;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    public IConfigurationStore configStore;

    /// <summary>
    /// For getting list of imported files for a project
    /// </summary>
    public IFileListProxy fileListProxy;

    /// <summary>
    /// For getting compaction settings for a project
    /// </summary>
    public ICompactionSettingsManager settingsManager;

    public long ProjectId;
    public IDictionary<string, string> Headers;
    public CompactionProjectSettings ProjectSettings;
    public List<long> ExcludedIds;
  }
}