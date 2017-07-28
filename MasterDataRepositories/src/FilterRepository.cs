using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Repositories
{
  public class FilterRepository : RepositoryBase, IRepository<IFilterEvent>, IFilterRepository
  {
    private readonly ILogger log;

    public FilterRepository(IConfigurationStore _connectionString, ILoggerFactory logger) : base(_connectionString,
      logger)
    {
      log = logger.CreateLogger<FilterRepository>();
    }


    #region store

    public async Task<int> StoreEvent(IFilterEvent evt)
    {
      var upsertedCount = 0;
      if (evt == null)
      {
        log.LogWarning("Unsupported Filter event type");
        return 0;
      }

      log.LogDebug($"Event type is {evt.GetType()}");
      //if (evt is CreateFilterEvent)
      //{
      //  var filterEvent = (CreateFilterEvent) evt;
      //  var filter = new Filter()
      //  {
      //    CustomerUid = filterEvent.CustomerUid,
      //    UserUid = filterEvent.UserUid,
      //    ProjectUid = filterEvent.ProjectUid,
      //    FilterUid = filterEvent.FilterUid,
      //    Name = filterEvent.Name,
      //    FilterJson = filterEvent.FilterJson,
      //    LastActionedUtc = filterEvent.ActionUTC
      //  };

      //  upsertedCount = await UpsertFilterDetail(filter, "CreateFilterEvent");
      //}
      //else if (evt is UpdateFilterEvent)
      //{
      //  var filterEvent = (UpdateFilterEvent) evt;
      //  var filter = new Filter()
      //  {
      //    CustomerUid = filterEvent.CustomerUid,
      //    UserUid = filterEvent.UserUid,
      //    ProjectUid = filterEvent.ProjectUid,
      //    FilterUid = filterEvent.FilterUid,
      //    Name = filterEvent.Name,
      //    FilterJson = filterEvent.FilterJson,
      //    LastActionedUtc = filterEvent.ActionUTC
      //  };
      //  upsertedCount = await UpsertFilterDetail(filter, "UpdateFilterEvent");
      //}
      //else if (evt is DeleteFilterEvent)
      //{
      //  var filterEvent = (DeleteFilterEvent) evt;
      //  var filter = new Filter()
      //  {
      //    FilterUid = filterEvent.FilterUid,
      //    LastActionedUtc = filterEvent.ActionUTC
      //  };
      //  upsertedCount = await UpsertFilterDetail(filter, "DeleteFilterEvent", filterEvent.DeletePermanently);
      //}
      ////else if (evt is UndeleteFilterEvent)
      ////{
      ////  var filterEvent = (UndeleteFilterEvent)evt;
      ////  var filter = new Filter()
      ////  {
      ////    FilterUid = filterEvent.FilterUid,
      ////    LastActionedUtc = filterEvent.ActionUTC
      ////  };
      ////  upsertedCount = await UpsertFilterDetail(filter, "UndeleteFilterEvent");
      ////}

      return upsertedCount;
    }


    /// <summary>
    ///     All detail-related columns can be inserted,
    ///     but only certain columns can be updated.
    ///     on deletion, a flag will be set.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="eventType"></param>
    /// <param name="isDeletePermanently"></param>
    /// <returns></returns>
    private async Task<int> UpsertFilterDetail(Filter filter, string eventType, bool isDeletePermanently = false)
    {
      var upsertedCount = 0;
      var existing = (await QueryWithAsyncPolicy<Filter>(@"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.fk_UserUID AS UserUID, 
                f.fk_ProjectUID AS ProjectUID,  f.FilterUID,                
                f.Name, f.FilterJson, 
                f.IsDeleted, p.LastActionedUTC
              FROM Filter f
              WHERE f.FilterUID = @filterUid AND f.IsDeleted = 0",
        new {filter.FilterUid})).FirstOrDefault();

      if (eventType == "UpdateFilterEvent")
        upsertedCount = await UpsertFilter(filter, existing);

      if (eventType == "DeleteFilterEvent")
        upsertedCount = await DeleteFilter(filter, existing, isDeletePermanently);
      return upsertedCount;
    }


    private async Task<int> UpsertFilter(Filter filter, Filter existing) 
    {
      log.LogDebug($"FilterRepository/UpsertFilter: filter={JsonConvert.SerializeObject(filter)}))')");
      int upsertedCount = 0;

      // following are immutable: FilterUID, fk_CustomerUid, fk_ProjectUID, fk_UserUID
      // only updateable if transient i.e empty name
      if (existing != null && !string.IsNullOrEmpty(existing.Name))
        return upsertedCount;

      const string upsert =
        @"INSERT Filter
                 (fk_CustomerUid, fk_UserUID, fk_ProjectUID, FilterUID,
                  Name, FilterJson, 
                  IsDeleted, LastActionedUTC)
            VALUES
              (@CustomerUid, @UserUID, @ProjectUID, @FilterUID,  
                  @Name, @FilterJson, 
                  @IsDeleted, @LastActionedUTC)
            ON DUPLICATE KEY UPDATE
              LastActionedUTC =
                IF ( ( VALUES(LastActionedUTC) >= LastActionedUTC), 
                    VALUES(LastActionedUTC), LastActionedUTC),
              Name =
                IF ( ( VALUES(LastActionedUTC) >= LastActionedUTC), 
                    VALUES(Name), Name),
              FilterJson =
                IF ( ( VALUES(LastActionedUTC) >= LastActionedUTC), 
                    VALUES(FilterJson), FilterJson)";

      upsertedCount = await ExecuteWithAsyncPolicy(upsert, filter);
      log.LogDebug(
        $"FilterRepository/UpsertFilter: upserted {upsertedCount} rows (1=insert, 2=update)");
      return upsertedCount.CalculateUpsertCount();
    }

    private async Task<int> DeleteFilter(Filter filter, Filter existing, bool isDeletePermanently)
    {
      log.LogDebug(
        $"FilterRepository/DeleteFilter: project={JsonConvert.SerializeObject(filter)} permanently: {isDeletePermanently}))')");

      var upsertedCount = 0;
      if (existing != null)
      {
        if (filter.LastActionedUtc >= existing.LastActionedUtc)
        {
          if (isDeletePermanently)
          {
            log.LogDebug(
              $"FilterRepository/DeleteFilter: deleting a filter permanently");
            const string delete =
              @"DELETE FROM Filter
                    WHERE FilterUID = @FilterUid";
            upsertedCount = await ExecuteWithAsyncPolicy(delete, filter);
            log.LogDebug(
              $"FilterRepository/DeleteFilter: deleted {upsertedCount}");
            return upsertedCount;
          }
          else
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
      }
      else
      {
        // a delete was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
        log.LogDebug(
          $"FilterRepository/DeleteFilter: delete event where no filter exists, creating one. filter={filter.FilterUid}");
       
        const string delete =
          "INSERT Filter " +
          "    (fk_CustomerUid, fk_UserUID, fk_ProjectUID, FilterUID, IsDeleted, LastActionedUTC)" +
          "  VALUES " +
          "    (@CustomerUid,  @UserUID,  @ProjectUID, @FilterUID, 1, @LastActionedUTC)";

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
    ///   get all active filters for a project
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Filter>> GetFiltersForProject(string projectUid)
    {
      var filters = (await QueryWithAsyncPolicy<Filter>(@"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.fk_UserUID AS UserUID, 
                f.fk_ProjectUID AS ProjectUID, f.FilterUID,                   
                f.Name, f.FilterJson, 
                f.IsDeleted, p.LastActionedUTC
              FROM Filter f
              WHERE f.fk_ProjectUID = @projectUid AND f.IsDeleted = 0",
        new {projectUid}));
      return filters;
    }

    /// <summary>
    /// get filter if active
    /// </summary>
    /// <param name="filterUid"></param>
    /// <returns></returns>
    public async Task<Filter> GetFilter(string filterUid)
    {
      var filters = (await QueryWithAsyncPolicy<Filter>(@"SELECT 
                f.fk_CustomerUid AS CustomerUID, f.fk_UserUID AS UserUID, 
                f.fk_ProjectUID AS ProjectUID, f.FilterUID,                  
                f.Name, f.FilterJson, 
                f.IsDeleted, p.LastActionedUTC
              FROM Filter f
              WHERE f.FilterUID = @filterUid AND f.IsDeleted = 0",
        new {filterUid})).FirstOrDefault();
      return filters;
    }
    #endregion getters
  }
}