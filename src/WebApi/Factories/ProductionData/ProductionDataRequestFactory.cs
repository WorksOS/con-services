using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Helpers;

namespace VSS.Productivity3D.WebApi.Factories.ProductionData
{
  /// <summary>
  /// 
  /// </summary>
  public class ProductionDataRequestFactory : IProductionDataRequestFactory
  {
    private readonly ILogger log;
    private readonly IConfigurationStore configStore;
    private readonly IFileListProxy fileListProxy;
    private readonly ICompactionSettingsManager settingsManager;
    private long _projectId;
    private IDictionary<string, string> _headers;
    private CompactionProjectSettings _projectSettings;
    private Filter _filter;
    private DesignDescriptor _designDescriptor;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="logger">ILoggerFactory service implementation</param>
    /// <param name="configStore">IConfigurationStore service implementation</param>
    /// <param name="fileListProxy">MasterDataProxies IFileListProxy service</param>
    /// <param name="settingsManager">ICompactionSettingsManager service implementation</param>
    public ProductionDataRequestFactory(ILoggerFactory logger, IConfigurationStore configStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      this.configStore = configStore;
      this.fileListProxy = fileListProxy;
      this.settingsManager = settingsManager;
    }

    /// <summary>
    /// Create instance of T.
    /// </summary>
    /// <typeparam name="T">Derived implementation of DataRequestBase</typeparam>
    /// <returns>Returns instance of T with required attributes set.</returns>
    public T Create<T>(Action<ProductionDataRequestFactory> action) where T : DataRequestBase, new()
    {
      action(this);

      var obj = new T();
      obj.Initialize(log, configStore, fileListProxy, settingsManager, _projectId, _projectSettings, _headers, _filter, _designDescriptor);

      return obj;
    }

    /// <summary>
    /// Sets the ProjectID
    /// </summary>
    /// <param name="projectId"></param>
    public ProductionDataRequestFactory ProjectId(long projectId)
    {
      _projectId = projectId;
      return this;
    }

    /// <summary>
    /// Sets the collection of custom headers used on the service request.
    /// </summary>
    /// <param name="headers"></param>
    public ProductionDataRequestFactory Headers(IDictionary<string, string> headers)
    {
      _headers = headers;
      return this;
    }

    /// <summary>
    /// Sets the compaction settings used for the project.
    /// </summary>
    /// <param name="projectSettings"></param>
    public ProductionDataRequestFactory ProjectSettings(CompactionProjectSettings projectSettings)
    {
      _projectSettings = projectSettings;
      return this;
    }

    /// <summary>
    /// Sets the filter.
    /// </summary>
    /// <param name="filter">Filter model for the raptor query.</param>
    public ProductionDataRequestFactory Filter(Filter filter)
    {
      _filter = filter;
      return this;
    }

    /// <summary>
    /// Sets the design descriptor.
    /// </summary>
    /// <param name="designDescriptor">Design for the raptor query.</param>
    public ProductionDataRequestFactory DesignDescriptor(DesignDescriptor designDescriptor)
    {
      _designDescriptor = designDescriptor;
      return this;
    }
  }
}