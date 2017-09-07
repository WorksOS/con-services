using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Extensions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
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
    protected List<long> ExcludedIds;
    protected Filter Filter;
    protected DesignDescriptor DesignDescriptor;

    public void Initialize(ILogger log, IConfigurationStore configurationStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager, 
      long projectId, CompactionProjectSettings projectSettings, IDictionary<string, string> headers, List<long> excludedIds, Filter filter, DesignDescriptor designDescriptor)
    {
      Log = log;
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;

      ProjectId = projectId;
      Headers = headers;
      ProjectSettings = projectSettings;
      ExcludedIds = excludedIds;
      Filter = filter;
      DesignDescriptor = designDescriptor;
    }
  }
}