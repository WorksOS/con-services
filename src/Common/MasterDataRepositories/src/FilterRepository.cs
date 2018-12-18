using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Repositories
{
  public class FilterRepository : RepositoryBase, IRepository<IFilterEvent>, IFilterRepository
  {
    public FilterRepository(IConfigurationStore connectionString, ILoggerFactory logger) : base(connectionString,
      logger)
    {
      Log = logger.CreateLogger<FilterRepository>();
    }

    #region store

    public Task<int> StoreEvent(IFilterEvent evt)
    {
      // following are immutable: FilterUID, fk_CustomerUid, fk_ProjectUID, UserID
      // filterJson is only updateable if transient 

      Log.LogDebug($"Event type is {evt.GetType()}");
      switch (evt)
      {
        case CreateFilterEvent @event:
          {
            var filter = new Filter
            {
              CustomerUid = @event.CustomerUID.ToString(),
              UserId = @event.UserID,
              ProjectUid = @event.ProjectUID.ToString(),
              FilterUid = @event.FilterUID.ToString(),
              Name = @event.Name,
              FilterJson = @event.FilterJson,
              FilterType = @event.FilterType,
              LastActionedUtc = @event.ActionUTC
            };

            return @event.FilterType == FilterType.Transient
              ? CreateFilter(filter, null)
              : UpsertFilterDetail(filter, "CreateFilterEvent");
          }
        case UpdateFilterEvent @event:
          {
            var filter = new Filter
            {
              CustomerUid = @event.CustomerUID.ToString(),
              UserId = @event.UserID,
              ProjectUid = @event.ProjectUID.ToString(),
              FilterUid = @event.FilterUID.ToString(),
              Name = @event.Name,
              FilterJson = @event.FilterJson,
              FilterType = @event.FilterType,
              LastActionedUtc = @event.ActionUTC
            };

            return UpsertFilterDetail(filter, "UpdateFilterEvent");
          }
        case DeleteFilterEvent @event:
          {
            var filter = new Filter
            {
              CustomerUid = @event.CustomerUID.ToString(),
              UserId = @event.UserID,
              ProjectUid = @event.ProjectUID.ToString(),
              FilterUid = @event.FilterUID.ToString(),
              LastActionedUtc = @event.ActionUTC
            };
           
            return UpsertFilterDetail(filter, "DeleteFilterEvent");
          }
        default:
          {
            Log.LogWarning("Unsupported Filter event type");
            return Task.FromResult(0);
          }
      }
    }

    /// <summary>
    ///     All detail-related columns can be inserted,
    ///     but only certain columns can be updated.
    ///     on deletion, a flag will be set.
    /// </summary>
    private async Task<int> UpsertFilterDetail(Filter filter, string eventType)
    {
      var upsertedCount = 0;
      var existing = (await QueryWithAsyncPolicy<Filter>(@"SELECT 
                f.FilterUID, f.fk_CustomerUid AS CustomerUID, 
                f.fk_ProjectUID AS ProjectUID, f.UserID,                                  
                f.Name, f.FilterJson, f.fk_FilterTypeID as FilterType,
                f.IsDeleted, f.LastActionedUTC
              FROM Filter f
              WHERE f.FilterUID = @FilterUid",
        new {FilterUid = filter.FilterUid})).FirstOrDefault();

      if (eventType == "CreateFilterEvent")
        upsertedCount = await CreateFilter(filter, existing);

      if (eventType == "UpdateFilterEvent")
        upsertedCount = await UpdateFilter(filter, existing);

      if (eventType == "DeleteFilterEvent")
        upsertedCount = await DeleteFilter(filter, existing);
      return upsertedCount;
    }


    private Task<int> CreateFilter(Filter filter, Filter existing)
    {
      Log.LogDebug($"{nameof(FilterRepository)}/{nameof(CreateFilter)}: filter={JsonConvert.SerializeObject(filter)}))')");

      if (existing == null)
      {
        const string insert =
          @"INSERT Filter
                 (fk_CustomerUid, UserID, fk_ProjectUID, FilterUID,
                  Name, FilterJson, fk_FilterTypeID,
                  IsDeleted, LastActionedUTC)
              VALUES
                (@CustomerUid, @UserId, @ProjectUid, @FilterUid,  
                    @Name, @FilterJson, @FilterType,
                    @IsDeleted, @LastActionedUtc)";

        return ExecuteWithAsyncPolicy(insert, filter);
      }

      // a delete was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
      //       update everything but ActionUTC from the create
      if (existing.LastActionedUtc >= filter.LastActionedUtc && existing.IsDeleted)
      {
        filter.IsDeleted = true;
        Log.LogDebug("FilterRepository/CreateFilter: going to update filter if received after a delete");
        const string update =
          @"UPDATE Filter
              SET Name = @Name,
                  FilterJson = @FilterJson,
                  fk_FilterTypeID = @FilterType,
              WHERE FilterUID = @FilterUid";

        return ExecuteWithAsyncPolicy(update, filter);
      }

      // If Create received after it's been deleted then ignore it as Name; BoundaryJson and actionUtc will be more recent.
      return Task.FromResult(0);
    }

    private async Task<int> UpdateFilter(Filter filter, Filter existing)
    {
      Log.LogDebug($"FilterRepository/UpdateFilter: filter={JsonConvert.SerializeObject(filter)}))')");
      int upsertedCount = 0;

      // following are immutable: FilterUID, fk_CustomerUid, fk_ProjectUID, UserID, FilterType
      if (existing != null && existing.FilterType == FilterType.Transient)
        return upsertedCount;

      if (existing != null)
      {
        const string update =
          @"UPDATE Filter
              SET Name = @Name,
                  FilterJson = @FilterJson,
                  LastActionedUTC = @LastActionedUtc
              WHERE FilterUID = @FilterUid";

        upsertedCount = await ExecuteWithAsyncPolicy(update, filter);
        Log.LogDebug($"FilterRepository/UpdateFilter: updated {upsertedCount}");
        return upsertedCount;
      }

      // update received before create
      const string insert =
        @"INSERT Filter
                 (fk_CustomerUid, UserID, fk_ProjectUID, FilterUID,
                  Name, FilterJson, fk_FilterTypeID,
                  IsDeleted, LastActionedUTC)
            VALUES
              (@CustomerUid, @UserId, @ProjectUid, @FilterUid,  
               @Name, @FilterJson, @FilterType,
               @IsDeleted, @LastActionedUtc)";

      upsertedCount = await ExecuteWithAsyncPolicy(insert, filter);
      Log.LogDebug($"FilterRepository/UpdateFilter: created {upsertedCount}");
      return upsertedCount;
    }

    private async Task<int> DeleteFilter(Filter filter, Filter existing)
    {
      Log.LogDebug($"FilterRepository/DeleteFilter: project={JsonConvert.SerializeObject(filter)})')");

      var upsertedCount = 0;
      if (existing != null)
      {
        if (filter.LastActionedUtc >= existing.LastActionedUtc)
        {
          Log.LogDebug($"FilterRepository/DeleteFilter: updating filter");

          const string update =
            @"UPDATE Filter                
                  SET IsDeleted = 1,
                    LastActionedUTC = @LastActionedUtc
                  WHERE FilterUID = @FilterUid";
          upsertedCount = await ExecuteWithAsyncPolicy(update, filter);
          Log.LogDebug(
            $"FilterRepository/DeleteFilter: upserted {upsertedCount} rows");
          return upsertedCount;
        }
      }
      else
      {
        // a delete was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
        Log.LogDebug(
          $"FilterRepository/DeleteFilter: delete event where no filter exists, creating one. filter={filter.FilterUid}");

        filter.Name = string.Empty;
        filter.FilterJson = string.Empty;

        const string delete =
          @"INSERT Filter
                 (fk_CustomerUid, UserID, fk_ProjectUID, FilterUID,
                  Name, FilterJson, fk_FilterTypeID,
                  IsDeleted, LastActionedUTC)
              VALUES
                (@CustomerUid, @UserId, @ProjectUid, @FilterUid,  
                 @Name, @FilterJson, @FilterType,
                 1, @LastActionedUtc)";

        upsertedCount = await ExecuteWithAsyncPolicy(delete, filter);
        Log.LogDebug(
          $"FilterRepository/DeleteFilter: inserted {upsertedCount} rows.");
        return upsertedCount;
      }

      return upsertedCount;
    }

    #endregion store


    #region getters

    /// <summary>
    ///   get all active filters for a customer/Project/User
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="projectUid"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<IEnumerable<Filter>> GetFiltersForProjectUser(string customerUid, string projectUid, string userId,
      bool includeAll = false)
    {
      string queryString = null;

      if (includeAll)
        queryString = @"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.UserID, 
                f.fk_ProjectUID AS ProjectUID, f.FilterUID,                   
                f.Name, f.FilterJson, f.fk_FilterTypeID as FilterType,
                f.IsDeleted, f.LastActionedUTC
              FROM Filter f
              WHERE f.fk_CustomerUID = @CustomerUid 
                AND f.fk_ProjectUID = @ProjectUid 
                AND f.UserID = @UserId 
                AND f.IsDeleted = 0";
      else
        queryString = $@"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.UserID, 
                f.fk_ProjectUID AS ProjectUID, f.FilterUID,                   
                f.Name, f.FilterJson, f.fk_FilterTypeID as FilterType,
                f.IsDeleted, f.LastActionedUTC
              FROM Filter f
              WHERE f.fk_CustomerUID = @CustomerUid 
                AND f.fk_ProjectUID = @ProjectUid 
                AND f.UserID = @UserId 
                AND f.IsDeleted = 0
                AND f.fk_FilterTypeID = {(int) FilterType.Persistent}";

      return (QueryWithAsyncPolicy<Filter>(queryString,
        new {CustomerUid = customerUid, ProjectUid = projectUid, UserId = userId}));
    }

    /// <summary>
    ///   get all active filters for a project
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public Task<IEnumerable<Filter>> GetFiltersForProject(string projectUid)
    {
      return (QueryWithAsyncPolicy<Filter>($@"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.UserID, 
                f.fk_ProjectUID AS ProjectUID, f.FilterUID,                   
                f.Name, f.FilterJson, f.fk_FilterTypeID as FilterType,
                f.IsDeleted, f.LastActionedUTC
              FROM Filter f
              WHERE f.fk_ProjectUID = @ProjectUid 
                AND f.IsDeleted = 0
                AND f.fk_FilterTypeID = {(int) FilterType.Persistent}",
        new {ProjectUid = projectUid}));
    }

    /// <summary>
    /// get filter if active
    /// </summary>
    /// <param name="filterUid"></param>
    /// <returns></returns>
    public async Task<Filter> GetFilter(string filterUid)
    {
      return (await QueryWithAsyncPolicy<Filter>(@"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.UserID, 
                f.fk_ProjectUID AS ProjectUID, f.FilterUID,                  
                f.Name, f.FilterJson, f.fk_FilterTypeID as FilterType,
                f.IsDeleted, f.LastActionedUTC
              FROM Filter f
              WHERE f.FilterUID = @FilterUid 
                AND f.IsDeleted = 0",
        new {FilterUid = filterUid})).FirstOrDefault();
    }

    public async Task<int> DeleteTransientFilters(string deleteOlderThanUtc)
    {
      var deletedCount = 0;
      var delete =
        "DELETE FROM Filter " +
        "  WHERE ID > 0 " +
        $"    AND fk_FilterTypeID = {(int) FilterType.Transient}" +
        $"    AND LastActionedUTC < '{deleteOlderThanUtc}';";

      Log.LogDebug($"FilterRepository/DeleteTransientFilters SQL: {delete}");
      deletedCount = await ExecuteWithAsyncPolicy(delete);
      Log.LogDebug($"FilterRepository/DeleteTransientFilters: deleted {deletedCount} rows");
      return deletedCount;
    }

    /// <summary>
    /// get filter if active
    /// </summary>
    /// <param name="filterUid"></param>
    /// <returns></returns>
    public async Task<Filter> GetFilterForUnitTest(string filterUid)
    {
      return (await QueryWithAsyncPolicy<Filter>(@"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.UserID, 
                f.fk_ProjectUID AS ProjectUID, f.FilterUID,                  
                f.Name, f.FilterJson, f.fk_FilterTypeID as FilterType, 
                f.IsDeleted, f.LastActionedUTC
              FROM Filter f
              WHERE f.FilterUID = @FilterUid",
        new {FilterUid = filterUid})).FirstOrDefault();
    }

    #endregion getters
  }
}
