using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using VSS.Productivity3D.Project.Abstractions.Interfaces;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  public abstract class DataRequestBase
  {
    protected ILogger Log;
    protected IConfigurationStore ConfigurationStore;
    protected IFileImportProxy FileImportProxy;
    protected ICompactionSettingsManager SettingsManager;

    protected Guid? ProjectUid;
    protected long ProjectId;
    protected IHeaderDictionary Headers;
    protected CompactionProjectSettings ProjectSettings;
    protected CompactionProjectSettingsColors ProjectSettingsColors;
    protected FilterResult Filter;
    protected DesignDescriptor DesignDescriptor;

    public void Initialize(ILogger log, IConfigurationStore configurationStore, IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager, Guid? projectUid, long projectId, CompactionProjectSettings projectSettings, CompactionProjectSettingsColors projectSettingsColors, IHeaderDictionary headers, FilterResult filter, DesignDescriptor designDescriptor)
    {
      filter?.Validate(); // Should be moved to FilterResult.CreateFilterObsolete().

      Log = log;
      ConfigurationStore = configurationStore;
      FileImportProxy = fileImportProxy;
      SettingsManager = settingsManager;

      ProjectUid = projectUid;
      ProjectId = projectId;
      Headers = headers;
      ProjectSettings = projectSettings;
      ProjectSettingsColors = projectSettingsColors;
      Filter = filter;
      DesignDescriptor = designDescriptor;
    }
  }
}
