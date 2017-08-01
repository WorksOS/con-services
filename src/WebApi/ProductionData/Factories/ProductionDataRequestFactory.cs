using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.ProductionData.Helpers;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.ProductionData.Factories
{
  /// <summary>
  /// 
  /// </summary>
  public class ProductionDataRequestFactory : IProductionDataRequestFactory
  {
    private readonly ILogger log;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private readonly IConfigurationStore configStore;

    /// <summary>
    /// For getting list of imported files for a project
    /// </summary>
    private readonly IFileListProxy fileListProxy;

    /// <summary>
    /// For getting compaction settings for a project
    /// </summary>
    private readonly ICompactionSettingsManager settingsManager;

    private long _projectId;
    private IDictionary<string, string> _headers;
    private CompactionProjectSettings _projectSettings;
    private List<long> _excludedIds;

    public ProductionDataRequestFactory(ILoggerFactory logger, IConfigurationStore configStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      log = logger.CreateLogger<SliceProfileDataRequestHelper>();
      this.configStore = configStore;
      this.fileListProxy = fileListProxy;
      this.settingsManager = settingsManager;
    }

    /// <summary>
    /// 
    /// 
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Create<T>(Action<ProductionDataRequestFactory> action) where T : DataRequestBase, new()
    {
      //   var factory = new ProductionDataRequestFactory();
      action(this);

      var obj = new T
      {
        log = log,
        configStore = configStore,
        fileListProxy = fileListProxy,
        settingsManager = settingsManager,
        ProjectId = _projectId,
        ProjectSettings = _projectSettings,
        Headers = _headers,
        ExcludedIds = _excludedIds
      };

      return obj;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="projectId"></param>
    public ProductionDataRequestFactory ProjectId(long projectId)
    {
      _projectId = projectId;
      return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="headers"></param>
    public ProductionDataRequestFactory Headers(IDictionary<string, string> headers)
    {
      _headers = headers;
      return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="projectSettings"></param>
    public ProductionDataRequestFactory ProjectSettings(CompactionProjectSettings projectSettings)
    {
      _projectSettings = projectSettings;
      return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="excludedIds"></param>
    /// <returns></returns>
    public ProductionDataRequestFactory ExcludedIds(List<long> excludedIds)
    {
      _excludedIds = excludedIds;
      return this;
    }
  }
}