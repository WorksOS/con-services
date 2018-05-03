using System;
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
    public FilterRepository(IConfigurationStore _connectionString, ILoggerFactory logger) : base(_connectionString,
      logger)
    {
      log = logger.CreateLogger<FilterRepository>();
    }

    #region store

    public async Task<int> StoreEvent(IFilterEvent evt)
    {
      // following are immutable: FilterUID, fk_CustomerUid, fk_ProjectUID, UserID
      // filterJson is only updateable if transient 
      var upsertedCount = 0;
      if (evt == null)
      {
        log.LogWarning("Unsupported Filter event type");
        return 0;
      }

      log.LogDebug($"Event type is {evt.GetType()}");
      if (evt is CreateFilterEvent)
      {
        var filterEvent = (CreateFilterEvent)evt;
        var filter = new Filter
        {
          CustomerUid = filterEvent.CustomerUID.ToString(),
          UserId = filterEvent.UserID,
          ProjectUid = filterEvent.ProjectUID.ToString(),
          FilterUid = filterEvent.FilterUID.ToString(),
          Name = filterEvent.Name,
          FilterJson = filterEvent.FilterJson,
          FilterType = filterEvent.FilterType,
          LastActionedUtc = filterEvent.ActionUTC
        };

        upsertedCount = await UpsertFilterDetail(filter, "CreateFilterEvent");
      }
      else if (evt is UpdateFilterEvent)
      {
        var filterEvent = (UpdateFilterEvent)evt;
        var filter = new Filter
        {
          CustomerUid = filterEvent.CustomerUID.ToString(),
          UserId = filterEvent.UserID,
          ProjectUid = filterEvent.ProjectUID.ToString(),
          FilterUid = filterEvent.FilterUID.ToString(),
          Name = filterEvent.Name,
          FilterJson = filterEvent.FilterJson,
          FilterType = filterEvent.FilterType,
          LastActionedUtc = filterEvent.ActionUTC
        };
        upsertedCount = await UpsertFilterDetail(filter, "UpdateFilterEvent");
      }
      else if (evt is DeleteFilterEvent)
      {
        var filterEvent = (DeleteFilterEvent)evt;
        var filter = new Filter
        {
          CustomerUid = filterEvent.CustomerUID.ToString(),
          UserId = filterEvent.UserID,
          ProjectUid = filterEvent.ProjectUID.ToString(),
          FilterUid = filterEvent.FilterUID.ToString(),
          LastActionedUtc = filterEvent.ActionUTC
        };
        upsertedCount = await UpsertFilterDetail(filter, "DeleteFilterEvent");
      }

      return upsertedCount;
    }


    /// <summary>
    ///     All detail-related columns can be inserted,
    ///     but only certain columns can be updated.
    ///     on deletion, a flag will be set.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private async Task<int> UpsertFilterDetail(Filter filter, string eventType)
    {
      var upsertedCount = 0;
      var existing = (await QueryWithAsyncPolicy<Filter>(@"SELECT 
                f.FilterUID, f.fk_CustomerUid AS CustomerUID, 
                f.fk_ProjectUID AS ProjectUID, f.UserID,                                  
                f.Name, f.FilterJson, f.fk_FilterTypeID as FilterType,
                f.IsDeleted, f.LastActionedUTC
              FROM Filter f
              WHERE f.FilterUID = @filterUid",
        new { filter.FilterUid })).FirstOrDefault();

      if (eventType == "CreateFilterEvent")
        upsertedCount = await CreateFilter(filter, existing);

      if (eventType == "UpdateFilterEvent")
        upsertedCount = await UpdateFilter(filter, existing);

      if (eventType == "DeleteFilterEvent")
        upsertedCount = await DeleteFilter(filter, existing);
      return upsertedCount;
    }


    private async Task<int> CreateFilter(Filter filter, Filter existing)
    {
      log.LogDebug($"FilterRepository/CreateFilter: filter={JsonConvert.SerializeObject(filter)}))')");
      int upsertedCount = 0;

      if (existing == null)
      {
        const string insert =
          @"INSERT Filter
                 (fk_CustomerUid, UserID, fk_ProjectUID, FilterUID,
                  Name, FilterJson, fk_FilterTypeID,
                  IsDeleted, LastActionedUTC)
            VALUES
              (@CustomerUid, @UserID, @ProjectUID, @FilterUID,  
                  @Name, @FilterJson, @FilterType,
                  @IsDeleted, @LastActionedUTC)";

        upsertedCount = await ExecuteWithAsyncPolicy(insert, filter);
        log.LogDebug($"FilterRepository/CreateFilter: created {upsertedCount}");
        return upsertedCount;
      }

      // a delete was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
      //       update everything but ActionUTC from the create
      if (existing.LastActionedUtc >= filter.LastActionedUtc && existing.IsDeleted)
      {
        filter.IsDeleted = true;
        log.LogDebug("FilterRepository/CreateFilter: going to update filter if received after a delete");
        const string update =
          @"UPDATE Filter
              SET Name = @Name,
                  FilterJson = @FilterJson,
                  fk_FilterTypeID = @FilterType,
              WHERE FilterUID = @FilterUID";

        upsertedCount = await ExecuteWithAsyncPolicy(update, filter);
        log.LogDebug($"FilterRepository/CreateFilter: (update): updated {upsertedCount}");
        return upsertedCount;
      }

      // If Create received after it's been deleted then ignore it as Name; BoundaryJson and actionUtc will be more recent.
      return upsertedCount;
    }

    private async Task<int> UpdateFilter(Filter filter, Filter existing)
    {
      log.LogDebug($"FilterRepository/UpdateFilter: filter={JsonConvert.SerializeObject(filter)}))')");
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
                  LastActionedUTC = @LastActionedUTC
              WHERE FilterUID = @FilterUID";

        upsertedCount = await ExecuteWithAsyncPolicy(update, filter);
        log.LogDebug($"FilterRepository/UpdateFilter: updated {upsertedCount}");
        return upsertedCount;
      }

      // update received before create
      const string insert =
        @"INSERT Filter
                 (fk_CustomerUid, UserID, fk_ProjectUID, FilterUID,
                  Name, FilterJson, fk_FilterTypeID,
                  IsDeleted, LastActionedUTC)
            VALUES
              (@CustomerUid, @UserID, @ProjectUID, @FilterUID,  
               @Name, @FilterJson, @FilterType,
               @IsDeleted, @LastActionedUTC)";

      upsertedCount = await ExecuteWithAsyncPolicy(insert, filter);
      log.LogDebug($"FilterRepository/UpdateFilter: created {upsertedCount}");
      return upsertedCount;
    }

    private async Task<int> DeleteFilter(Filter filter, Filter existing)
    {
      log.LogDebug($"FilterRepository/DeleteFilter: project={JsonConvert.SerializeObject(filter)})')");

      var upsertedCount = 0;
      if (existing != null)
      {
        if (filter.LastActionedUtc >= existing.LastActionedUtc)
        {
          log.LogDebug($"FilterRepository/DeleteFilter: updating filter");

          const string update =
            @"UPDATE Filter                
                  SET IsDeleted = 1,
                    LastActionedUTC = @LastActionedUTC
                  WHERE FilterUID = @FilterUid";
          upsertedCount = await ExecuteWithAsyncPolicy(update, filter);
          log.LogDebug(
            $"FilterRepository/DeleteFilter: upserted {upsertedCount} rows");
          return upsertedCount;
        }
      }
      else
      {
        // a delete was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
        log.LogDebug(
          $"FilterRepository/DeleteFilter: delete event where no filter exists, creating one. filter={filter.FilterUid}");

        filter.Name = string.Empty;
        filter.FilterJson = string.Empty;

        const string delete =
          @"INSERT Filter
                 (fk_CustomerUid, UserID, fk_ProjectUID, FilterUID,
                  Name, FilterJson, fk_FilterTypeID,
                  IsDeleted, LastActionedUTC)
            VALUES
              (@CustomerUid, @UserID, @ProjectUID, @FilterUID,  
               @Name, @FilterUid, @FilterType,
               1, @LastActionedUTC)";

        upsertedCount = await ExecuteWithAsyncPolicy(delete, filter);
        log.LogDebug(
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
    public async Task<IEnumerable<Filter>> GetFiltersForProjectUser(string customerUid, string projectUid, string userId, bool includeAll = false)
    {
      string queryString = null;
      
      if (includeAll)
        queryString = @"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.UserID, 
                f.fk_ProjectUID AS ProjectUID, f.FilterUID,                   
                f.Name, f.FilterJson, f.fk_FilterTypeID as FilterType,
                f.IsDeleted, f.LastActionedUTC
              FROM Filter f
              WHERE f.fk_CustomerUID = @customerUid 
                AND f.fk_ProjectUID = @projectUid 
                AND f.UserID = @userId 
                AND f.IsDeleted = 0";
      else
        queryString = $@"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.UserID, 
                f.fk_ProjectUID AS ProjectUID, f.FilterUID,                   
                f.Name, f.FilterJson, f.fk_FilterTypeID as FilterType,
                f.IsDeleted, f.LastActionedUTC
              FROM Filter f
              WHERE f.fk_CustomerUID = @customerUid 
                AND f.fk_ProjectUID = @projectUid 
                AND f.UserID = @userId 
                AND f.IsDeleted = 0
                AND f.fk_FilterTypeID = {(int)FilterType.Persistent}";

      var filters = (await QueryWithAsyncPolicy<Filter>(queryString,
        new { customerUid, projectUid, userId }));
      return filters;
    }

    /// <summary>
    ///   get all active filters for a project
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Filter>> GetFiltersForProject(string projectUid)
    {
      var filters = (await QueryWithAsyncPolicy<Filter>($@"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.UserID, 
                f.fk_ProjectUID AS ProjectUID, f.FilterUID,                   
                f.Name, f.FilterJson, f.fk_FilterTypeID as FilterType,
                f.IsDeleted, f.LastActionedUTC
              FROM Filter f
              WHERE f.fk_ProjectUID = @projectUid 
                AND f.IsDeleted = 0
                AND f.fk_FilterTypeID = {(int)FilterType.Persistent}",
        new { projectUid }));
      return filters;
    }


    /// <summary>
    /// Returns filters which will be removed if cleanup is run.
    /// </summary>
    /// <param name="ageInMinutesToBeDeleted"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Filter>> GetTransientFiltersToBeCleaned(int ageInMinutesToBeDeleted)
    {
      var cutoffActionUtcToDelete = DateTime.Now.AddMinutes(-ageInMinutesToBeDeleted).ToString("yyyy-MM-dd HH:mm:ss");
      var filters = (await QueryWithAsyncPolicy<Filter>($@"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.UserID, 
                f.fk_ProjectUID AS ProjectUID, f.FilterUID,                   
                f.Name, f.FilterJson, f.fk_FilterTypeID as FilterType,
                f.IsDeleted, f.LastActionedUTC
              FROM Filter f
              WHERE f.fk_FilterTypeID = 1 
                AND f.LastActionedUTC < @cutoffActionUtcToDelete",
        new { cutoffActionUtcToDelete }));
      return filters;
    }

    /// <summary>
    /// get filter if active
    /// </summary>
    /// <param name="filterUid"></param>
    /// <returns></returns>
    public async Task<Filter> GetFilter(string filterUid)
    {
      var filter = (await QueryWithAsyncPolicy<Filter>(@"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.UserID, 
                f.fk_ProjectUID AS ProjectUID, f.FilterUID,                  
                f.Name, f.FilterJson, f.fk_FilterTypeID as FilterType,
                f.IsDeleted, f.LastActionedUTC
              FROM Filter f
              WHERE f.FilterUID = @filterUid 
                AND f.IsDeleted = 0",
        new { filterUid })).FirstOrDefault();
      return filter;
    }

    /// <summary>
    /// get filter if active
    /// </summary>
    /// <param name="filterUid"></param>
    /// <returns></returns>
    public async Task<Filter> GetFilterForUnitTest(string filterUid)
    {
      var filter = (await QueryWithAsyncPolicy<Filter>(@"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.UserID, 
                f.fk_ProjectUID AS ProjectUID, f.FilterUID,                  
                f.Name, f.FilterJson, f.fk_FilterTypeID as FilterType, 
                f.IsDeleted, f.LastActionedUTC
              FROM Filter f
              WHERE f.FilterUID = @filterUid",
        new { filterUid })).FirstOrDefault();
      return filter;
    }
    #endregion getters
  }
}