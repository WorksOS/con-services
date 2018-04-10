using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for the CoordinateSystemFile resource.
  /// </summary>
  // [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })] (Aaron) Disabled temporarily until we can resolve the cache invalidation problem
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class CompactionCoordinateSystemController : BaseController
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionCoordinateSystemController(IASNodeClient raptorClient, ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler, IConfigurationStore configStore, IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, IFilterServiceProxy filterServiceProxy, ICompactionSettingsManager settingsManager, IProductionDataRequestFactory requestFactory, IPreferenceProxy prefProxy) :
      base(loggerFactory, loggerFactory.CreateLogger<CompactionCoordinateSystemController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      raptorClient = this.raptorClient;
    }
  }
}
