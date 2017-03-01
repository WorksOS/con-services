using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.GenericConfiguration;
using Repositories.DBModels;

namespace Repositories
{

  public class SubscriptionRepository : RepositoryBase, IRepository<ISubscriptionEvent>
  {
    private readonly ILogger log;
    public SubscriptionRepository(IConfigurationStore connectionString, ILoggerFactory logger)  : base(connectionString)
    {
      log = logger.CreateLogger<SubscriptionRepository>();
    }

    public Dictionary<string, ServiceType> _serviceTypes = null;


    public async Task<int> StoreEvent(ISubscriptionEvent evt)
    {
      var upsertedCount = 0;

      if (_serviceTypes == null)
        _serviceTypes = (await GetServiceTypes()).ToDictionary(k => k.Name, v => v);

      if (evt is CreateProjectSubscriptionEvent)
      {
        var subscriptionEvent = (CreateProjectSubscriptionEvent)evt;
        var subscription = new Subscription();
        subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
        subscription.CustomerUID = subscriptionEvent.CustomerUID.ToString();
        subscription.ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID;
        subscription.StartDate = subscriptionEvent.StartDate.Date;
        //This is to handle CG subscriptions where we set the EndDate annually.
        //In NG the end date is the maximum unless it is cancelled/terminated.
        subscription.EndDate = subscriptionEvent.EndDate > DateTime.UtcNow ? new DateTime(9999, 12, 31) : subscriptionEvent.EndDate.Date;
        subscription.LastActionedUTC = subscriptionEvent.ActionUTC;
        upsertedCount = await UpsertSubscriptionDetail(subscription, "CreateProjectSubscriptionEvent");
      }
      else if (evt is UpdateProjectSubscriptionEvent)
      {
        var subscriptionEvent = (UpdateProjectSubscriptionEvent)evt;
        var subscription = new Subscription();
        subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();

        // this is dangerous. I suppose if current logic is chnanged to MOVE a servicePlan for rental customers
        // i.e. from one to the next customer, then this may be possible.
        //   in that scenario, what should be the relavant StartDate, EndDate and EffectiveDate? (not of concern here)
        subscription.CustomerUID = subscriptionEvent.CustomerUID.HasValue ? subscriptionEvent.CustomerUID.Value.ToString() : null;

        // should not be able to change a serviceType!!!
        // subscription.ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID;
        subscription.StartDate = subscriptionEvent.StartDate == null ? DateTime.MinValue.Date : subscriptionEvent.StartDate.Value.Date;
        // todo update allows a future endDate but create does not, is this an error?
        // also, for both create and update for start and end dates these are calendar days
        //    in the assets timezone, but the create checks for UTC time....
        subscription.EndDate = subscriptionEvent.EndDate == null ? DateTime.MaxValue.Date : subscriptionEvent.EndDate.Value.Date;
        subscription.LastActionedUTC = subscriptionEvent.ActionUTC;
        upsertedCount = await UpsertSubscriptionDetail(subscription, "UpdateProjectSubscriptionEvent");
      }
      else if (evt is AssociateProjectSubscriptionEvent)
      {
        var subscriptionEvent = (AssociateProjectSubscriptionEvent)evt;
        var projectSubscription = new ProjectSubscription();
        projectSubscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
        projectSubscription.ProjectUID = subscriptionEvent.ProjectUID.ToString();
        projectSubscription.EffectiveDate = subscriptionEvent.EffectiveDate;
        projectSubscription.LastActionedUTC = subscriptionEvent.ActionUTC;
        upsertedCount = await UpsertProjectSubscriptionDetail(projectSubscription, "AssociateProjectSubscriptionEvent");
      }
      else if (evt is CreateCustomerSubscriptionEvent)
      {
        var subscriptionEvent = (CreateCustomerSubscriptionEvent)evt;
        var subscription = new Subscription();
        subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
        subscription.CustomerUID = subscriptionEvent.CustomerUID.ToString();
        subscription.ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID;
        subscription.StartDate = subscriptionEvent.StartDate;
        //This is to handle CG subscriptions where we set the EndDate annually.
        //In NG the end date is the maximum unless it is cancelled/terminated.
        subscription.EndDate = subscriptionEvent.EndDate > DateTime.UtcNow ? new DateTime(9999, 12, 31) : subscriptionEvent.EndDate;
        subscription.LastActionedUTC = subscriptionEvent.ActionUTC;
        upsertedCount = await UpsertSubscriptionDetail(subscription, "CreateCustomerSubscriptionEvent");
      }
      else if (evt is UpdateCustomerSubscriptionEvent)
      {
        var subscriptionEvent = (UpdateCustomerSubscriptionEvent)evt;
        var subscription = new Subscription();
        subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
        subscription.StartDate = subscriptionEvent.StartDate ?? DateTime.MinValue;
        subscription.EndDate = subscriptionEvent.EndDate ?? DateTime.MinValue;
        subscription.LastActionedUTC = subscriptionEvent.ActionUTC;
        upsertedCount = await UpsertSubscriptionDetail(subscription, "UpdateCustomerSubscriptionEvent");
      }
      else if (evt is CreateAssetSubscriptionEvent)
      {
        var subscriptionEvent = (CreateAssetSubscriptionEvent)evt;
        var subscription = new Subscription();
        subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
        subscription.CustomerUID = subscriptionEvent.CustomerUID.ToString();
        subscription.ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID;
        subscription.StartDate = subscriptionEvent.StartDate;
        //This is to handle CG subscriptions where we set the EndDate annually.
        //In NG the end date is the maximum unless it is cancelled/terminated.
        subscription.EndDate = subscriptionEvent.EndDate > DateTime.UtcNow ? new DateTime(9999, 12, 31) : subscriptionEvent.EndDate;
        subscription.LastActionedUTC = subscriptionEvent.ActionUTC;
        upsertedCount = await UpsertSubscriptionDetail(subscription, "CreateAssetSubscriptionEvent");
      }
      else if (evt is UpdateAssetSubscriptionEvent)
      {
        var subscriptionEvent = (UpdateAssetSubscriptionEvent)evt;
        var subscription = new Subscription();
        subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
        subscription.CustomerUID = subscriptionEvent.CustomerUID.HasValue ? subscriptionEvent.CustomerUID.Value.ToString() : null;
        subscription.StartDate = subscriptionEvent.StartDate ?? DateTime.MinValue;
        subscription.EndDate = subscriptionEvent.EndDate ?? DateTime.MinValue;
        subscription.LastActionedUTC = subscriptionEvent.ActionUTC;        
        upsertedCount = await UpsertSubscriptionDetail(subscription, "UpdateAssetSubscriptionEvent");
      }

      return upsertedCount;
    }



    /// <summary>
    /// All detail-related columns can be inserted, 
    ///    but only certain columns can be updated.
    /// </summary>
    /// <param name="subscription"></param>
    /// <param name="eventType"></param>
    /// <returns>Number of upserted records</returns>
    private async Task<int> UpsertSubscriptionDetail(Subscription subscription, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      var existing = (await Connection.QueryAsync<Subscription>
        (@"SELECT 
                SubscriptionUID, fk_CustomerUID AS CustomerUID, StartDate, EndDate, fk_ServiceTypeID AS ServiceTypeID, LastActionedUTC 
              FROM Subscription
              WHERE SubscriptionUID = @subscriptionUID",
          new { subscriptionUID = subscription.SubscriptionUID }
          )).FirstOrDefault();

      if (eventType == "CreateProjectSubscriptionEvent" || eventType == "CreateCustomerSubscriptionEvent" || eventType == "CreateAssetSubscriptionEvent")
      {
        upsertedCount = await CreateSubscription(subscription, existing);
      }

      if (eventType == "UpdateProjectSubscriptionEvent" || eventType == "UpdateCustomerSubscriptionEvent" || eventType == "UpdateAssetSubscriptionEvent")
      {
        upsertedCount = await UpdateSubscription(subscription, existing);
      }
      
      PerhapsCloseConnection();

      return upsertedCount;
    }

    private async Task<int> CreateSubscription(Subscription subscription, Subscription existing)
    {
      var upsertedCount = 0;
      if (existing == null)
      {
        log.LogDebug("SubscriptionRepository/CreateSubscription: going to create subscription={0}", JsonConvert.SerializeObject(subscription));

        const string insert =
          @"INSERT Subscription
                (SubscriptionUID, fk_CustomerUID, StartDate, EndDate, fk_ServiceTypeID, LastActionedUTC)
              VALUES
                (@SubscriptionUID, @CustomerUID, @StartDate, @EndDate, @ServiceTypeID, @LastActionedUTC)";
        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          upsertedCount = await Connection.ExecuteAsync(insert, subscription);
          log.LogDebug("SubscriptionRepository/CreateSubscription: upserted {0} rows (1=insert, 2=update) for: subscriptionUid:{1}", upsertedCount, subscription.SubscriptionUID);
          return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }

      log.LogDebug("SubscriptionRepository/CreateSubscription: can't create as already exists subscription={0}", JsonConvert.SerializeObject(subscription));
      return upsertedCount;
    }

    private async Task<int> UpdateSubscription(Subscription subscription, Subscription existing)
    {
      // todo this code allows customerUID and serviceType to be updated - is this intentional?

      var upsertedCount = 0;
      if (existing != null)
      {
        if (subscription.LastActionedUTC >= existing.LastActionedUTC)
        {
          log.LogDebug("SubscriptionRepository/UpdateSubscription: going to create subscription={0}", JsonConvert.SerializeObject(subscription));

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
                  SET SubscriptionUID = @SubscriptionUID,
                      fk_CustomerUID = @CustomerUID,
                      StartDate=@StartDate, 
                      EndDate=@EndDate, 
                      fk_ServiceTypeID=@ServiceTypeID,
                      LastActionedUTC=@LastActionedUTC
                WHERE SubscriptionUID = @SubscriptionUID";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            upsertedCount = await Connection.ExecuteAsync(update, subscription);
            log.LogDebug("SubscriptionRepository/UpdateSubscription: upserted {0} rows (1=insert, 2=update) for: subscriptionUid:{1}", upsertedCount, subscription.SubscriptionUID);
            return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
          });
        }

        log.LogDebug("SubscriptionRepository/UpdateSubscription: old update event ignored subscription={0}", JsonConvert.SerializeObject(subscription));        
      }
      else
      {
        log.LogDebug("SubscriptionRepository/UpdateSubscription: can't update as none existing subscription={0}", JsonConvert.SerializeObject(subscription));
      }
      return upsertedCount;
    }

    private async Task<int> UpsertProjectSubscriptionDetail(ProjectSubscription projectSubscription, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();
      
      var existing = (await Connection.QueryAsync<ProjectSubscription>
          (@"SELECT 
                fk_SubscriptionUID AS SubscriptionUID, fk_ProjectUID AS ProjectUID, EffectiveDate, LastActionedUTC
              FROM ProjectSubscription
              WHERE fk_ProjectUID = @projectUID AND fk_SubscriptionUID = @subscriptionUID",
          new { projectUID = projectSubscription.ProjectUID, subscriptionUID = projectSubscription.SubscriptionUID }
          )).FirstOrDefault();

      if (eventType == "AssociateProjectSubscriptionEvent")
      {
        upsertedCount = await AssociateProjectSubscription(projectSubscription, existing);
      }
      
      PerhapsCloseConnection();
      return upsertedCount;
    }

    private async Task<int> AssociateProjectSubscription(ProjectSubscription projectSubscription, ProjectSubscription existing)
    {
      var upsertedCount = 0;
      await PerhapsOpenConnection();

      if (existing == null)
      {
        log.LogDebug("SubscriptionRepository/AssociateProjectSubscription: going to create projectSubscription={0}", JsonConvert.SerializeObject(projectSubscription));

        const string insert =
          @"INSERT ProjectSubscription
                (fk_SubscriptionUID, fk_ProjectUID, EffectiveDate, LastActionedUTC)
              VALUES
                (@SubscriptionUID, @ProjectUID, @EffectiveDate, @LastActionedUTC)";

        PerhapsCloseConnection();
        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          upsertedCount = await Connection.ExecuteAsync(insert, projectSubscription);
          log.LogDebug("SubscriptionRepository/AssociateProjectSubscription: upserted {0} rows (1=insert, 2=update) for: SubscriptionUid:{1}", upsertedCount, projectSubscription.SubscriptionUID);
          return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }

      log.LogDebug("SubscriptionRepository/AssociateProjectSubscription: can't create as already exists projectSubscription={0}", JsonConvert.SerializeObject(projectSubscription));
      PerhapsCloseConnection();     
      return upsertedCount;
    }

    private async Task<IEnumerable<ServiceType>> GetServiceTypes()
    {
      await PerhapsOpenConnection();
      
      var serviceTypes = (await Connection.QueryAsync<ServiceType>
          (@"SELECT 
                s.ID, s.Description AS Name, sf.ID AS ServiceTypeFamilyID, sf.Description AS ServiceTypeFamilyName
              FROM ServiceTypeEnum s JOIN ServiceTypeFamilyEnum sf on s.fk_ServiceTypeFamilyID = sf.ID"
          ));
      PerhapsCloseConnection();
      return serviceTypes;
    }


    #region getters
    public async Task<Subscription> GetSubscription(string subscriptionUid)
    {
      await PerhapsOpenConnection();

      var subscription = (await Connection.QueryAsync<Subscription>
        (@"SELECT 
                SubscriptionUID, fk_CustomerUID AS CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, LastActionedUTC
              FROM Subscription
              WHERE SubscriptionUID = @subscriptionUid"
          , new { subscriptionUid }
        )).FirstOrDefault();

      PerhapsCloseConnection();
      return subscription;
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptionsByCustomer(string customerUid, DateTime validAtDate)
    {
      await PerhapsOpenConnection();

      var subscription = (await Connection.QueryAsync<Subscription>
        (@"SELECT 
                SubscriptionUID, fk_CustomerUID AS CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, LastActionedUTC
              FROM Subscription
              WHERE fk_CustomerUID = @customerUid
                AND @validAtDate BETWEEN StartDate AND EndDate"
          , new { customerUid,  validAtDate }
        ));

      PerhapsCloseConnection();
      return subscription;
    }

    public async Task<IEnumerable<Subscription>> GetSubscriptions_UnitTest(string subscriptionUid)
    {
      await PerhapsOpenConnection();

      var subscriptions = (await Connection.QueryAsync<Subscription>
        (@"SELECT 
                SubscriptionUID, fk_CustomerUID AS CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, LastActionedUTC
              FROM Subscription
              WHERE SubscriptionUID = @subscriptionUid"
          , new { subscriptionUid }
         ));

      PerhapsCloseConnection();
      return subscriptions;
    }

    public async Task<IEnumerable<ProjectSubscription>> GetProjectSubscriptions_UnitTest(string subscriptionUid)
    {
      await PerhapsOpenConnection();
      
      var projectSubscriptions = (await Connection.QueryAsync<ProjectSubscription>
          (@"SELECT 
                fk_SubscriptionUID AS SubscriptionUID, fk_ProjectUID AS ProjectUID, EffectiveDate, LastActionedUTC
              FROM ProjectSubscription
              WHERE fk_SubscriptionUID = @subscriptionUID"
            , new { subscriptionUid }
          ));

      PerhapsCloseConnection();
      return projectSubscriptions;
    }
    #endregion

  }
}