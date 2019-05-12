using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// To support getting ProjectStatistics from either Raptor or TRex
  ///    we need to muck around converting between Ids and Guids
  /// </summary>
  public class ProjectStatisticsHelper 
  {
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfigurationStore _configStore;
    private readonly IFileListProxy _fileListProxy;
    private readonly ITRexCompactionDataProxy _tRexCompactionDataProxy;

#if RAPTOR
    private readonly IASNodeClient _raptorClient;
#endif

    public ProjectStatisticsHelper(ILoggerFactory loggerFactory, IConfigurationStore configStore, 
      IFileListProxy fileListProxy, ITRexCompactionDataProxy tRexCompactionDataProxy
#if RAPTOR
        , IASNodeClient raptorClient
#endif
      )
    {
      _loggerFactory = loggerFactory;
      _configStore = configStore;
      _fileListProxy = fileListProxy;
      _tRexCompactionDataProxy = tRexCompactionDataProxy;
#if RAPTOR
      _raptorClient = raptorClient;
#endif
    }

    /// <summary>
    /// Gets the ids of the surveyed surfaces to exclude from Raptor/TRex calculations. 
    /// This is the deactivated ones.
    /// </summary>
    /// <returns>The list of file ids for the surveyed surfaces to be excluded</returns>
    public async Task<List<long>> GetExcludedSurveyedSurfaceIds(Guid projectUid, string userId, IDictionary<string, string> customHeaders)
    {
      var fileList = await _fileListProxy.GetFiles(projectUid.ToString(), userId, customHeaders);
      if (fileList == null || fileList.Count == 0)
      {
        return null;
      }

      var results = fileList
        .Where(f => f.ImportedFileType == ImportedFileType.SurveyedSurface && !f.IsActivated)
        .Select(f => f.LegacyFileId).ToList();

      return results;
    }

    /// <summary>
    /// Gets the Uid and Id lists of the surveyed surfaces to exclude from TRex/Raptor calculations. 
    /// This is the deactivated ones.
    /// </summary>
    private async Task<(List<Guid> Uids, List<long> Ids)> GetExcludedSurveyedSurfaces(Guid projectUid, string userId, IDictionary<string, string> customHeaders)
    {
      var fileList = await _fileListProxy.GetFiles(projectUid.ToString(), userId, customHeaders);
      if (fileList == null || fileList.Count == 0)
        return (null, null);

      var uidList = fileList
        .Where(f => f.ImportedFileType == ImportedFileType.SurveyedSurface && !f.IsActivated)
        .Select(f => Guid.Parse(f.ImportedFileUid)).ToList();

      var idList = fileList
        .Where(f => f.ImportedFileType == ImportedFileType.SurveyedSurface && !f.IsActivated)
        .Select(f => f.LegacyFileId).ToList();
      return (uidList, idList);
    }

    /// <summary>
    /// Gets the Uid and Id lists of the surveyed surfaces to exclude from TRex/Raptor calculations. 
    /// This is the deactivated ones.
    /// </summary>
    private async Task<List<Guid>> GetExcludedSurveyedSurfacesMatches(Guid projectUid, List<long> Ids, string userId, IDictionary<string, string> customHeaders)
    {
      var fileList = await _fileListProxy.GetFiles(projectUid.ToString(), userId, customHeaders);
      if (fileList == null || fileList.Count == 0)
        return null;

      var uidList = fileList
        .Where(f => f.ImportedFileType == ImportedFileType.SurveyedSurface
                    && Ids.Contains(f.LegacyFileId))
        .Select(f => Guid.Parse(f.ImportedFileUid)).ToList();
      return uidList;
    }

    public async Task<ProjectStatisticsResult> GetProjectStatisticsWithProjectSsExclusions(Guid projectUid, long projectId, string userId, IDictionary<string, string> customHeaders)
    {
      var excludedSSs = await GetExcludedSurveyedSurfaces(projectUid, userId, customHeaders);

      var request = new ProjectStatisticsMultiRequest(projectUid, projectId,
        excludedSSs.Uids?.ToArray(), excludedSSs.Ids?.ToArray());

      return await
        RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(_loggerFactory,
#if RAPTOR
            _raptorClient,
#endif
            configStore: _configStore, trexCompactionDataProxy: _tRexCompactionDataProxy)
          .ProcessAsync(request) as ProjectStatisticsResult;
    }

    // special case for obsolete controllers
    // supported only on Raptor
    public Task<ContractExecutionResult> GetProjectStatisticsWithExclusions(long projectId, long[] excludedSsIds)
    {
      var request = new ProjectStatisticsMultiRequest(null, projectId,
        new Guid[0], excludedSsIds);

      return RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(_loggerFactory,
#if RAPTOR
            _raptorClient,
#endif
            configStore: _configStore, trexCompactionDataProxy: _tRexCompactionDataProxy)
          .ProcessAsync(request);
    }

    public async Task<ProjectStatisticsResult> GetProjectStatisticsWithFilterSsExclusions(Guid projectUid, long projectId, List<long> filterSSExclusionList, string userId, IDictionary<string, string> customHeaders)
    {
      List<Guid> excludedSSUids = null;
      if (filterSSExclusionList != null && filterSSExclusionList.Count > 0)
      {
        excludedSSUids = await GetExcludedSurveyedSurfacesMatches(projectUid, filterSSExclusionList.ToList(), userId, customHeaders);
      }

      var request = new ProjectStatisticsMultiRequest(projectUid, projectId,
        excludedSSUids?.ToArray(), filterSSExclusionList?.ToArray());

      return await
        RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(_loggerFactory,
#if RAPTOR
            _raptorClient,
#endif
            configStore: _configStore, trexCompactionDataProxy: _tRexCompactionDataProxy)
          .ProcessAsync(request) as ProjectStatisticsResult;
    }
  }
}
