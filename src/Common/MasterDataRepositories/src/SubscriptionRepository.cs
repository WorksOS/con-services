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
  public class SubscriptionRepository : RepositoryBase, IRepository<ISubscriptionEvent>, ISubscriptionRepository
  {
    private Dictionary<string, ServiceType> _serviceTypes;

    public SubscriptionRepository(IConfigurationStore connectionString, ILoggerFactory logger) : base(
      connectionString, logger)
    {
      Log = logger.CreateLogger<SubscriptionRepository>();
    }

    #region store

    public async Task<int> StoreEvent(ISubscriptionEvent evt)
    {
      var upsertedCount = 0;
      if (evt == null)
      {
        Log.LogWarning("Unsupported subscription event type");
        return 0;
      }

      Log.LogDebug($"Event type is {evt.GetType()}");

      if (evt is CreateProjectSubscriptionEvent)
      {
        var subscriptionEvent = (CreateProjectSubscriptionEvent) evt;
        if (!(await IsServiceTypeValidAsync(subscriptionEvent.SubscriptionType, "Project")))
        {
          return 0;
        }

        var subscription = new Subscription
        {
          SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString(),
          CustomerUID = subscriptionEvent.CustomerUID.ToString(),
          ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID,
          StartDate = subscriptionEvent.StartDate.Date,
          EndDate = subscriptionEvent.EndDate > DateTime.UtcNow
            ? new DateTime(9999, 12, 31)
            : subscriptionEvent.EndDate.Date,
          LastActionedUTC = subscriptionEvent.ActionUTC
        };
        //This is to handle CG subscriptions where we set the EndDate annually.
        //In NG the end date is the maximum unless it is cancelled/terminated.
        upsertedCount = await UpsertSubscriptionDetail(subscription, "CreateProjectSubscriptionEvent");
      }
      else if (evt is UpdateProjectSubscriptionEvent)
      {
        var subscriptionEvent = (UpdateProjectSubscriptionEvent) evt;
        var subscription = new Subscription
        {
          SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString(),
          CustomerUID = subscriptionEvent.CustomerUID?.ToString(),
          StartDate = subscriptionEvent.StartDate?.Date ?? DateTime.MinValue.Date,
          EndDate = subscriptionEvent.EndDate?.Date ?? DateTime.MaxValue.Date,
          LastActionedUTC = subscriptionEvent.ActionUTC
        };

        // this is dangerous. I suppose if current logic is changed to MOVE a servicePlan for rental customers
        // i.e. from one to the next customer, then this may be possible.
        //   in that scenario, what should be the relavant StartDate, EndDate and EffectiveDate? (not of concern here)

        // should not be able to change a serviceType!!!
        // subscription.ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID;
        // todo update allows a future endDate but create does not, is this an error?
        // also, for both create and update for start and end dates these are calendar days
        //    in the assets timezone, but the create checks for UTC time....
        upsertedCount = await UpsertSubscriptionDetail(subscription, "UpdateProjectSubscriptionEvent");
      }
      else if (evt is AssociateProjectSubscriptionEvent)
      {
        var subscriptionEvent = (AssociateProjectSubscriptionEvent) evt;
        var projectSubscription =
          new ProjectSubscription
          {
            SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString(),
            ProjectUID = subscriptionEvent.ProjectUID.ToString(),
            EffectiveDate = subscriptionEvent.EffectiveDate,
            LastActionedUTC = subscriptionEvent.ActionUTC
          };
        upsertedCount =
          await UpsertProjectSubscriptionDetail(projectSubscription, "AssociateProjectSubscriptionEvent");
      }
      else if (evt is DissociateProjectSubscriptionEvent)
      {
        var subscriptionEvent = (DissociateProjectSubscriptionEvent) evt;
        var projectSubscription =
          new ProjectSubscription
          {
            SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString(),
            ProjectUID = subscriptionEvent.ProjectUID.ToString(),
            EffectiveDate = subscriptionEvent.EffectiveDate,
            LastActionedUTC = subscriptionEvent.ActionUTC
          };
        upsertedCount =
          await UpsertProjectSubscriptionDetail(projectSubscription, "DissociateProjectSubscriptionEvent");
      }
      else if (evt is CreateCustomerSubscriptionEvent)
      {
        var subscriptionEvent = (CreateCustomerSubscriptionEvent) evt;
        if (!(await IsServiceTypeValidAsync(subscriptionEvent.SubscriptionType, "Customer")))
        {
          return 0;
        }

        var subscription = new Subscription
        {
          SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString(),
          CustomerUID = subscriptionEvent.CustomerUID.ToString(),
          ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID,
          StartDate = subscriptionEvent.StartDate,
          EndDate = subscriptionEvent.EndDate > DateTime.UtcNow
            ? new DateTime(9999, 12, 31)
            : subscriptionEvent.EndDate,
          LastActionedUTC = subscriptionEvent.ActionUTC
        };
        //This is to handle CG subscriptions where we set the EndDate annually.
        //In NG the end date is the maximum unless it is cancelled/terminated.
        upsertedCount = await UpsertSubscriptionDetail(subscription, "CreateCustomerSubscriptionEvent");
      }
      else if (evt is UpdateCustomerSubscriptionEvent)
      {
        var subscriptionEvent = (UpdateCustomerSubscriptionEvent) evt;
        var subscription = new Subscription
        {
          SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString(),
          StartDate = subscriptionEvent.StartDate ?? DateTime.MinValue,
          EndDate = subscriptionEvent.EndDate ?? DateTime.MinValue,
          LastActionedUTC = subscriptionEvent.ActionUTC
        };
        upsertedCount = await UpsertSubscriptionDetail(subscription, "UpdateCustomerSubscriptionEvent");
      }
      else if (evt is CreateAssetSubscriptionEvent)
      {
        var subscriptionEvent = (CreateAssetSubscriptionEvent) evt;
        if (!(await IsServiceTypeValidAsync(subscriptionEvent.SubscriptionType, "Asset")))
        {
          return 0;
        }

        var subscription = new Subscription
        {
          SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString(),
          CustomerUID = subscriptionEvent.CustomerUID.ToString(),
          ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID,
          StartDate = subscriptionEvent.StartDate,
          EndDate = subscriptionEvent.EndDate > DateTime.UtcNow
            ? new DateTime(9999, 12, 31)
            : subscriptionEvent.EndDate,
          LastActionedUTC = subscriptionEvent.ActionUTC
        };
        //This is to handle CG subscriptions where we set the EndDate annually.
        //In NG the end date is the maximum unless it is cancelled/terminated.
        upsertedCount = await UpsertSubscriptionDetail(subscription, "CreateAssetSubscriptionEvent");

        if (upsertedCount == 1)
        {
          var assetSubscription =
            new AssetSubscription
            {
              SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString(),
              AssetUID = subscriptionEvent.AssetUID.ToString(),
              LastActionedUTC = subscriptionEvent.ActionUTC
            };
          upsertedCount = await UpsertAssetSubscriptionDetail(assetSubscription);
        }
      }
      else if (evt is UpdateAssetSubscriptionEvent)
      {
        var subscriptionEvent = (UpdateAssetSubscriptionEvent) evt;
        if (!(await IsServiceTypeValidAsync(subscriptionEvent.SubscriptionType, "Asset")))
        {
          return 0;
        }

        var subscription = new Subscription
        {
          SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString(),
          CustomerUID = subscriptionEvent.CustomerUID?.ToString(),
          StartDate = subscriptionEvent.StartDate ?? DateTime.MinValue,
          EndDate = subscriptionEvent.EndDate ?? DateTime.MinValue,
          LastActionedUTC = subscriptionEvent.ActionUTC
        };
        upsertedCount = await UpsertSubscriptionDetail(subscription, "UpdateAssetSubscriptionEvent");

        if (upsertedCount == 1)
        {
          var assetSubscription =
            new AssetSubscription
            {
              SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString(),
              AssetUID = subscriptionEvent.AssetUID.ToString(),
              LastActionedUTC = subscriptionEvent.ActionUTC
            };
          upsertedCount = await UpsertAssetSubscriptionDetail(assetSubscription);
        }
      }

      return upsertedCount;
    }


    /// <summary>
    ///     All detail-related columns can be inserted,
    ///     but only certain columns can be updated.
    /// </summary>
    /// <param name="subscription"></param>
    /// <param name="eventType"></param>
    /// <returns>Number of upserted records</returns>
    private async Task<int> UpsertSubscriptionDetail(Subscription subscription, string eventType)
    {
      var upsertedCount = 0;

      var existing = (await QueryWithAsyncPolicy<Subscription>
      (@"SELECT 
            SubscriptionUID, fk_CustomerUID AS CustomerUID, StartDate, EndDate, fk_ServiceTypeID AS ServiceTypeID, LastActionedUTC 
          FROM Subscription
          WHERE SubscriptionUID = @SubscriptionUID",
        new {SubscriptionUID = subscription.SubscriptionUID}
      )).FirstOrDefault();

      if (eventType == "CreateProjectSubscriptionEvent" || eventType == "CreateCustomerSubscriptionEvent" ||
          eventType == "CreateAssetSubscriptionEvent")
        upsertedCount = await CreateSubscription(subscription, existing);

      if (eventType == "UpdateProjectSubscriptionEvent" || eventType == "UpdateCustomerSubscriptionEvent" ||
          eventType == "UpdateAssetSubscriptionEvent")
        upsertedCount = await UpdateSubscription(subscription, existing);


      return upsertedCount;
    }

    private async Task<int> CreateSubscription(Subscription subscription, Subscription existing)
    {
      var upsertedCount = 0;
      if (existing == null)
      {
        Log.LogDebug(
          $"SubscriptionRepository/CreateSubscription: going to create subscription={JsonConvert.SerializeObject(subscription)}");

        const string insert =
          @"INSERT Subscription
                (SubscriptionUID, fk_CustomerUID, StartDate, EndDate, fk_ServiceTypeID, LastActionedUTC)
              VALUES
                (@SubscriptionUID, @CustomerUID, @StartDate, @EndDate, @ServiceTypeID, @LastActionedUTC)";

        upsertedCount = await ExecuteWithAsyncPolicy(insert, subscription);
        Log.LogDebug(
          $"SubscriptionRepository/CreateSubscription: upserted {upsertedCount} rows for: subscriptionUid:{subscription.SubscriptionUID}");

        return upsertedCount;
      }

      Log.LogDebug("SubscriptionRepository/CreateSubscription: can't create as already exists.");
      return upsertedCount;
    }

    private async Task<int> UpdateSubscription(Subscription subscription, Subscription existing)
    {
      // this code (copied from official masterData service)
      //   allows customerUID and serviceType to be updated - is this intentional?

      var upsertedCount = 0;
      if (existing != null)
      {
        if (subscription.LastActionedUTC >= existing.LastActionedUTC)
        {
          Log.LogDebug(
            $"SubscriptionRepository/UpdateSubscription: going to update subscription={JsonConvert.SerializeObject(subscription)}");

          //subscription only has values for columns to be updated
          if (string.IsNullOrEmpty(subscription.CustomerUID))
            subscription.CustomerUID = existing.CustomerUID;
          if (subscription.StartDate == DateTime.MinValue)
            subscription.StartDate = existing.StartDate;
          if (subscription.EndDate == DateTime.MinValue)
            subscription.EndDate = existing.EndDate;
          if (subscription.ServiceTypeID == 0)
            subscription.ServiceTypeID = existing.ServiceTypeID;

          const string update =
            @"UPDATE Subscription                
                  SET fk_CustomerUID = @CustomerUID,
                      StartDate=@StartDate, 
                      EndDate=@EndDate, 
                      fk_ServiceTypeID=@ServiceTypeID,
                      LastActionedUTC=@LastActionedUTC
                WHERE SubscriptionUID = @SubscriptionUID";

          upsertedCount = await ExecuteWithAsyncPolicy(update, subscription);
          Log.LogDebug(
            $"SubscriptionRepository/UpdateSubscription: upserted {upsertedCount} rows for: subscriptionUid:{subscription.SubscriptionUID}");
          return upsertedCount;
        }
      }

      Log.LogDebug(
        "SubscriptionRepository/UpdateSubscription: can't update as none exists. This may be an unsupported subscription type.");
      return upsertedCount;
    }


    private async Task<int> UpsertProjectSubscriptionDetail(ProjectSubscription projectSubscription, string eventType)
    {
      var upsertedCount = 0;

      var existing = (await QueryWithAsyncPolicy<ProjectSubscription>
      (@"SELECT 
            fk_SubscriptionUID AS SubscriptionUID, fk_ProjectUID AS ProjectUID, EffectiveDate, LastActionedUTC
          FROM ProjectSubscription
          WHERE fk_ProjectUID = @ProjectUID AND fk_SubscriptionUID = @SubscriptionUID",
        new {ProjectUID = projectSubscription.ProjectUID, SubscriptionUID = projectSubscription.SubscriptionUID}
      )).FirstOrDefault();

      if (eventType == "AssociateProjectSubscriptionEvent")
        upsertedCount = await AssociateProjectSubscription(projectSubscription, existing);

      if (eventType == "DissociateProjectSubscriptionEvent")
        upsertedCount = await DissociateProjectSubscription(projectSubscription, existing);

      return upsertedCount;
    }

    private async Task<int> AssociateProjectSubscription(ProjectSubscription projectSubscription,
      ProjectSubscription existing)
    {
      var upsertedCount = 0;

      if (existing == null)
      {
        Log.LogDebug(
          $"SubscriptionRepository/AssociateProjectSubscription: going to create projectSubscription={JsonConvert.SerializeObject(projectSubscription)}");

        const string insert =
          @"INSERT ProjectSubscription
                (fk_SubscriptionUID, fk_ProjectUID, EffectiveDate, LastActionedUTC)
              VALUES
                (@SubscriptionUID, @ProjectUID, @EffectiveDate, @LastActionedUTC)";

        upsertedCount = await ExecuteWithAsyncPolicy(insert, projectSubscription);
        Log.LogDebug(
          $"SubscriptionRepository/AssociateProjectSubscription: upserted {upsertedCount} rows for: SubscriptionUid:{projectSubscription.SubscriptionUID}");

        return upsertedCount;
      }

      Log.LogDebug("SubscriptionRepository/AssociateProjectSubscription: can't create as already exists.");
      return upsertedCount;
    }

    private async Task<int> DissociateProjectSubscription(ProjectSubscription projectSubscription,
      ProjectSubscription existing)
    {
      var upsertedCount = 0;

      if (existing != null)
      {
        if (projectSubscription.LastActionedUTC >= existing.LastActionedUTC)
        {
          const string delete =
            @"DELETE FROM ProjectSubscription
                WHERE fk_SubscriptionUID = @SubscriptionUID 
                  AND fk_ProjectUID = @ProjectUID";
          upsertedCount = await ExecuteWithAsyncPolicy(delete, projectSubscription);
          Log.LogDebug(
            $"SubscriptionRepository/DissociateProjectSubscription: upserted {upsertedCount} rows for: subscriptionUid:{projectSubscription.SubscriptionUID}");
          return upsertedCount;
        }

        // may have been associated again since, so don't delete
        Log.LogDebug("SubscriptionRepository/DissociateProjectSubscription: old delete event ignored");
      }
      else
      {
        Log.LogDebug("SubscriptionRepository/DissociateProjectSubscription: can't delete as none existing");
      }

      return upsertedCount;
    }


    private async Task<int> UpsertAssetSubscriptionDetail(AssetSubscription assetSubscription)
    {
      int upsertedCount;

      var existing = (await QueryWithAsyncPolicy<AssetSubscription>
      (@"SELECT 
            fk_SubscriptionUID AS SubscriptionUID, fk_AssetUID AS AssetUID, EffectiveDate, LastActionedUTC
          FROM AssetSubscription
          WHERE fk_AssetUID = @AssetUID 
            AND fk_SubscriptionUID = @SubscriptionUID",
        new {AssetUID = assetSubscription.AssetUID, SubscriptionUID = assetSubscription.SubscriptionUID}
      )).FirstOrDefault();

      upsertedCount = await AssociateAssetSubscription(assetSubscription, existing);

      return upsertedCount;
    }

    private async Task<int> AssociateAssetSubscription(AssetSubscription assetSubscription, AssetSubscription existing)
    {
      var upsertedCount = 0;

      if (existing == null)
      {
        Log.LogDebug(
          $"SubscriptionRepository/AssociateAssetSubscription: going to create assetSubscription={JsonConvert.SerializeObject(assetSubscription)}");

        const string insert =
          @"INSERT AssetSubscription
                (fk_SubscriptionUID, fk_AssetUID, EffectiveDate, LastActionedUTC)
              VALUES
                (@SubscriptionUID, @AssetUID, @EffectiveDate, @LastActionedUTC)";

        upsertedCount = await ExecuteWithAsyncPolicy(insert, assetSubscription);
        Log.LogDebug(
          $"SubscriptionRepository/AssociateAssetSubscription: upserted {upsertedCount} rows for: SubscriptionUid:{assetSubscription.SubscriptionUID}");
        return upsertedCount;
      }

      Log.LogDebug("SubscriptionRepository/AssociateAssetSubscription: can't create as already exists.");
      return upsertedCount;
    }

    private Task<IEnumerable<ServiceType>> GetServiceTypes()
    {
      // ProjectService and 3dpm services are only interested in 4 service types
      var select = "SELECT " +
            "    s.ID, s.Description AS Name, sf.ID AS ServiceTypeFamilyID, sf.Description AS ServiceTypeFamilyName " +
            "  FROM ServiceTypeEnum s  " +
            "    JOIN ServiceTypeFamilyEnum sf on s.fk_ServiceTypeFamilyID = sf.ID " +
            $"  WHERE s.ID IN ({(int)ServiceTypeEnum.ThreeDProjectMonitoring}, {(int)ServiceTypeEnum.Manual3DProjectMonitoring}, {(int)ServiceTypeEnum.Landfill}, {(int)ServiceTypeEnum.ProjectMonitoring})";

      var serviceTypes = QueryWithAsyncPolicy<ServiceType>(select);

      return serviceTypes;
    }

    private async Task<bool> IsServiceTypeValidAsync(string subscriptionType, string subscriptionFamily)
    {
      if (_serviceTypes == null)
        _serviceTypes = (await GetServiceTypes()).ToDictionary(k => k.Name, v => v);

      ServiceType serviceType;
      var doesServiceTypeExist = _serviceTypes.TryGetValue(subscriptionType, out serviceType);
      if (!doesServiceTypeExist)
      {
        Log.LogWarning($"Unsupported SubscriptionType: {subscriptionType}");
        return false;
      }

      if (serviceType.ServiceTypeFamilyName != subscriptionFamily)
      {
        Log.LogWarning($"Invalid SubscriptionFamily {serviceType.ServiceTypeFamilyName} for Event type.");
        return false;
      }

      return true;
    }

    #endregion store


    #region getters

    public async Task<Subscription> GetSubscription(string subscriptionUid)
    {
      var subscription = (await QueryWithAsyncPolicy<Subscription>
      (@"SELECT 
            SubscriptionUID, fk_CustomerUID AS CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, LastActionedUTC
          FROM Subscription
          WHERE SubscriptionUID = @SubscriptionUID"
        , new {SubscriptionUID = subscriptionUid}
      )).FirstOrDefault();

      return subscription;
    }

    public Task<IEnumerable<Subscription>> GetSubscriptionsByCustomer(string customerUid, DateTime validAtDate)
    {
      var subscription = QueryWithAsyncPolicy<Subscription>
      (@"SELECT 
                SubscriptionUID, fk_CustomerUID AS CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, LastActionedUTC
              FROM Subscription
              WHERE fk_CustomerUID = @CustomerUID
                AND @validAtDate BETWEEN StartDate AND EndDate"
        , new {CustomerUID = customerUid, validAtDate}
      );

      return subscription;
    }

    public Task<IEnumerable<Subscription>> GetProjectBasedSubscriptionsByCustomer(string customerUid,
      DateTime validAtDate)
    {
      var select =
        "SELECT  " +
        "    SubscriptionUID, fk_CustomerUID AS CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, LastActionedUTC " +
        "  FROM Subscription " +
        "  WHERE fk_CustomerUID = @CustomerUID " +
        $"    AND fk_ServiceTypeID IN ({(int)ServiceTypeEnum.ThreeDProjectMonitoring}, {(int)ServiceTypeEnum.Manual3DProjectMonitoring}, {(int)ServiceTypeEnum.Landfill}, {(int)ServiceTypeEnum.ProjectMonitoring}) " +
        "    AND @validAtDate BETWEEN StartDate AND EndDate";

      var subscription = QueryWithAsyncPolicy<Subscription>(select, new {CustomerUID = customerUid, validAtDate});

      return subscription;
    }

    public Task<IEnumerable<Subscription>> GetFreeProjectSubscriptionsByCustomer(string customerUid,
      DateTime validAtDate)
    {
      // if a sub is attached to a project, and the project is deleted, the sub cannot be used by another project
      var select =
        "SELECT " +
        "   s.SubscriptionUID, s.fk_CustomerUID AS CustomerUID, s.fk_ServiceTypeID AS ServiceTypeID, s.StartDate, s.EndDate, s.LastActionedUTC " +
        "  FROM Subscription s " +
        "    LEFT OUTER JOIN ProjectSubscription ps ON ps.fk_SubscriptionUID = s.SubscriptionUID " +
        "  WHERE fk_CustomerUID = @CustomerUID " +
        "   AND @validAtDate BETWEEN StartDate AND EndDate " +
        $"   AND fk_ServiceTypeID IN ({(int)ServiceTypeEnum.Landfill}, {(int)ServiceTypeEnum.ProjectMonitoring}) " +
        "   AND ps.fk_SubscriptionUID IS NULL";

      var subscription = QueryWithAsyncPolicy<Subscription>(select, new {CustomerUID = customerUid, validAtDate}
      );

      return subscription;
    }

    public Task<IEnumerable<Subscription>> GetSubscriptionsByAsset(string assetUid, DateTime validAtDate)
    {
      var select =
        "SELECT " +
        "    s.SubscriptionUID, s.fk_CustomerUID as CustomerUID, s.fk_ServiceTypeID as ServiceTypeID, s.StartDate, s.EndDate, s.LastActionedUTC " +
        " FROM AssetSubscription aSub " +
        "   INNER JOIN Subscription s ON s.SubscriptionUID = aSub.fk_SubscriptionUID " +
        " WHERE aSub.fk_AssetUID = @assetUid " +
        $"   AND fk_ServiceTypeID = {(int)ServiceTypeEnum.ThreeDProjectMonitoring} " +
        "   AND @validAtDate BETWEEN s.StartDate AND s.EndDate";

      var subscription = QueryWithAsyncPolicy<Subscription>(select, new {assetUid, validAtDate});

      return subscription;
    }

    public Task<IEnumerable<Subscription>> GetSubscriptions_UnitTest(string subscriptionUid)
    {
      var subscriptions = QueryWithAsyncPolicy<Subscription>
      (@"SELECT 
            SubscriptionUID, fk_CustomerUID AS CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, LastActionedUTC
          FROM Subscription
          WHERE SubscriptionUID = @SubscriptionUID"
        , new {SubscriptionUID = subscriptionUid}
      );

      return subscriptions;
    }

    public Task<IEnumerable<ProjectSubscription>> GetProjectSubscriptions_UnitTest(string subscriptionUid)
    {
      var projectSubscriptions = QueryWithAsyncPolicy<ProjectSubscription>
      (@"SELECT 
              fk_SubscriptionUID AS SubscriptionUID, fk_ProjectUID AS ProjectUID, EffectiveDate, LastActionedUTC
            FROM ProjectSubscription
            WHERE fk_SubscriptionUID = @SubscriptionUID"
        , new {SubscriptionUID = subscriptionUid}
      );

      return projectSubscriptions;
    }

    #endregion
  }
}
