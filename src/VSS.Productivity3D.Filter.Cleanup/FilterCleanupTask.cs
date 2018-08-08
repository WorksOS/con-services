using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Cleanup.Utilities;

namespace VSS.Productivity3D.Filter.Cleanup
{
  /// <summary>
  /// FilterCleanupTask to remove old transient filters
  /// Note that transient filters are not written to kafka,
  ///   therefore can simply be deleted from local db.
  /// </summary>
  public class FilterCleanupTask
  {
    private readonly ILogger _log;
    private readonly IConfigurationStore _configStore;
    private readonly IServiceExceptionHandler _serviceExceptionHandler;
    private readonly IFilterRepository _filterRepository;
    private static int DefaultFilterAgeDefaultMinutes { get; } = 4;
    private readonly int _ageInMinutesToDelete;

    /// <summary>
    /// Initializes the FilterCleanupTask 
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="filterRepository"></param>
    public FilterCleanupTask(IConfigurationStore configStore, ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IFilterRepository filterRepository)
    {
      _log = loggerFactory.CreateLogger<FilterCleanupTask>();
      _configStore = configStore;
      _serviceExceptionHandler = serviceExceptionHandler;
      _filterRepository = filterRepository;

      if (!int.TryParse(_configStore.GetValueString("SCHEDULER_FILTER_CLEANUP_TASK_AGE_MINUTES"), out _ageInMinutesToDelete))
      {
        _ageInMinutesToDelete = DefaultFilterAgeDefaultMinutes;
      }
    }


    /// <summary>
    /// cleanup transient filters over n minutes old
    /// </summary>
    public async Task<int> FilterCleanup()
    {
      var startUtc = DateTime.UtcNow;
      var deleteOlderThanUtc = startUtc.AddMinutes(-_ageInMinutesToDelete).ToString("yyyy-MM-dd HH:mm:ss"); // mySql requires this format 
      _log.LogInformation($"FilterCleanup: ageInMinutesToDelete: {_ageInMinutesToDelete} deleteOlderThanUtc: {deleteOlderThanUtc}.");

      Dictionary<string, object> newRelicAttributes;
      var deletedCount = 0;
      try
      {
        deletedCount = await _filterRepository.DeleteTransientFilters(deleteOlderThanUtc);
      }
      catch (Exception ex)
      {
        newRelicAttributes = new Dictionary<string, object> {
          { "message", string.Format($"execute delete on filter DB exeception: {ex.Message}") }
        };
        NewRelicUtils.NotifyNewRelic("FilterCleanup", "Fatal", startUtc, _log, newRelicAttributes);
        _serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 78, ex.Message);
      }

      newRelicAttributes = new Dictionary<string, object> {
        { "message", "Task completed." }, { "ageInMinutesToDelete", _ageInMinutesToDelete }, {"deleteOlderThanUtc", deleteOlderThanUtc }, {"deletedCount", deletedCount}
      };
      NewRelicUtils.NotifyNewRelic("FilterCleanup", "Information", startUtc, _log, newRelicAttributes);

      return deletedCount;
    }
  }
}

