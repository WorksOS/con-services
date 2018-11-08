using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Models;
using Dapper;
using log4net;
using Newtonsoft.Json;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace MasterDataRepo
{
    public class SubscriptionRepo : RepositoryBase
    {
      private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public Dictionary<string, ServiceType> _serviceTypes = null;

    public SubscriptionRepo()
    {
      _serviceTypes = GetServiceTypes().ToDictionary(k => k.Name, v => v);
    }
    
    public int StoreSubscription(ISubscriptionEvent evt)
    {
      var upsertedCount = 0;

      if (evt is CreateProjectSubscriptionEvent)
      {
        //Only store Landfill subscriptions
        var subscriptionEvent = (CreateProjectSubscriptionEvent)evt;
        if (subscriptionEvent.SubscriptionType.ToLower() == "landfill")
        {
          var subscription = new Subscription();
          subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
          subscription.CustomerUID = subscriptionEvent.CustomerUID.ToString();
          subscription.ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID;
          subscription.StartDate = subscriptionEvent.StartDate;
          //This is to handle CG subscriptions where we set the EndDate annually.
          //In NG the end date is the maximum unless it is cancelled/terminated.
          subscription.EndDate = subscriptionEvent.EndDate > DateTime.UtcNow ? new DateTime(9999, 12, 31) : subscriptionEvent.EndDate;
          subscription.LastActionedUTC = subscriptionEvent.ActionUTC;
          upsertedCount = UpsertSubscriptionDetail(subscription, "CreateProjectSubscriptionEvent");
        }
      }
      //else if (evt is UpdateProjectSubscriptionEvent)
      //{
      //  var subscriptionEvent = (UpdateProjectSubscriptionEvent)evt;
      //  if (subscriptionEvent.SubscriptionType.ToLower() == "landfill")
      //  {
      //    var subscription = new Models.Subscription();
      //    subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
      //    subscription.CustomerUID = subscriptionEvent.CustomerUID.HasValue ? subscriptionEvent.CustomerUID.Value.ToString() : null;
      //    subscription.ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID;
      //    subscription.StartDate = subscriptionEvent.StartDate ?? DateTime.MinValue;
      //    subscription.EndDate = subscriptionEvent.EndDate ?? DateTime.MinValue;
      //    subscription.LastActionedUTC = subscriptionEvent.ActionUTC;
      //    upsertedCount = UpsertSubscriptionDetail(subscription, "UpdateProjectSubscriptionEvent");
      //  }
      //}
      else if (evt is AssociateProjectSubscriptionEvent)
      {
        var subscriptionEvent = (AssociateProjectSubscriptionEvent)evt;
        var projectSubscription = new ProjectSubscription();
        projectSubscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
        projectSubscription.ProjectUID = subscriptionEvent.ProjectUID.ToString();
        projectSubscription.LastActionedUTC = subscriptionEvent.ActionUTC;
        upsertedCount = UpsertProjectSubscriptionDetail(projectSubscription, "AssociateProjectSubscriptionEvent");
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
    private int UpsertSubscriptionDetail(Subscription subscription, string eventType)
    {
      int upsertedCount = 0;

      PerhapsOpenConnection();

      Log.DebugFormat("SubscriptionRepository: Upserting eventType={0} SubscriptionUID={1}", eventType, subscription.SubscriptionUID);

      var existing = Connection.Query<Subscription>
      (@"SELECT 
              SubscriptionUID, fk_CustomerUID, LastActionedUTC, StartDate, EndDate, fk_ServiceTypeID AS ServiceTypeID
            FROM Subscription
            WHERE SubscriptionUID = @subscriptionUID", new { subscriptionUID = subscription.SubscriptionUID }).FirstOrDefault();

      if (eventType == "CreateProjectSubscriptionEvent")
      {
        upsertedCount = CreateProjectSubscription(subscription, existing);
      }

      //if (eventType == "UpdateProjectSubscriptionEvent")
      //{
      //  upsertedCount = UpdateProjectSubscription(subscription, existing);
      //}

      Log.DebugFormat("SubscriptionRepository: upserted {0} rows", upsertedCount);

      PerhapsCloseConnection();

      return upsertedCount;
    }

    private int CreateProjectSubscription(Subscription subscription, Subscription existing)
    {
      if (existing == null)
      {
        const string insert =
          @"INSERT Subscription
                (SubscriptionUID, fk_CustomerUID, StartDate, LastActionedUTC, EndDate, fk_ServiceTypeID)
                VALUES
                (@SubscriptionUID, @CustomerUID, @StartDate, @LastActionedUTC, @EndDate, @ServiceTypeID)";

        return Connection.Execute(insert, subscription);
      }

      Log.DebugFormat("SubscriptionRepository: can't create as already exists newActionedUTC {0}. So, the existing entry should be updated.", subscription.LastActionedUTC);
      return 0;
    }

    //private int UpdateProjectSubscription(Models.Subscription subscription, Models.Subscription existing)
    //{
    //  if (existing != null)
    //  {
    //    if (subscription.LastActionedUTC >= existing.LastActionedUTC)
    //    {
    //      //subscription only has values for columns to be updated
    //      if (string.IsNullOrEmpty(subscription.CustomerUID))
    //        subscription.CustomerUID = existing.CustomerUID;
    //      if (subscription.StartDate == DateTime.MinValue)
    //        subscription.StartDate = existing.StartDate;
    //      if (subscription.EndDate == DateTime.MinValue)
    //        subscription.EndDate = existing.EndDate;

    //      const string update =
    //        @"UPDATE Subscription                
    //            SET SubscriptionUID = @SubscriptionUID,
    //                CustomerUID = @CustomerUID,
    //                StartDate=@StartDate, 
    //                EndDate=@EndDate, 
    //                fk_ServiceTypeID=@ServiceTypeID,
    //                LastActionedUTC=@LastActionedUTC
    //          WHERE SubscriptionUID = @SubscriptionUID";
    //      return Connection.Execute(update, subscription);
    //    }

    //    Log.DebugFormat("SubscriptionRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
    //      existing.LastActionedUTC, subscription.LastActionedUTC);
    //  }
    //  else
    //  {
    //    Log.DebugFormat("SubscriptionRepository: can't update as none existing newActionedUTC {0}",
    //      subscription.LastActionedUTC);
    //  }
    //  return 0;
    //}

    private int UpsertProjectSubscriptionDetail(ProjectSubscription projectSubscription, string eventType)
    {
      int upsertedCount = 0;

      PerhapsOpenConnection();

      Log.DebugFormat("SubscriptionRepository: Upserting eventType={0} ProjectUid={1}, SubscriptionUid={2}",
        eventType, projectSubscription.ProjectUID, projectSubscription.SubscriptionUID);

      var existing = Connection.Query<ProjectSubscription>
      (@"SELECT 
              fk_SubscriptionUID AS SubscriptionUID, fk_ProjectUID AS ProjectUID, LastActionedUTC
              FROM ProjectSubscription
              WHERE fk_ProjectUID = @projectUID AND fk_SubscriptionUID = @subscriptionUID",
        new { projectUID = projectSubscription.ProjectUID, subscriptionUID = projectSubscription.SubscriptionUID }).FirstOrDefault();

      if (eventType == "AssociateProjectSubscriptionEvent")
      {
        upsertedCount = AssociateProjectSubscription(projectSubscription, existing);
      }

      Log.DebugFormat("SubscriptionRepository: upserted {0} rows", upsertedCount);

      PerhapsCloseConnection();

      return upsertedCount;
    }

    private int AssociateProjectSubscription(ProjectSubscription projectSubscription, ProjectSubscription existing)
    {
      if (existing == null)
      {
        const string insert =
          @"INSERT ProjectSubscription
                (fk_SubscriptionUID, fk_ProjectUID, LastActionedUTC)
                VALUES
                (@SubscriptionUID, @ProjectUID, @LastActionedUTC)";

        return Connection.Execute(insert, projectSubscription);
      }

      Log.DebugFormat("SubscriptionRepository: can't create as already exists newActionedUTC={0}", projectSubscription.LastActionedUTC);
      return 0;
    }

    private IEnumerable<ServiceType> GetServiceTypes()
    {
      PerhapsOpenConnection();

      Log.Debug("SubscriptionRepository: Getting service types");

      var serviceTypes = Connection.Query<ServiceType>
      (@"SELECT 
            s.ID, s.Description AS Name, sf.ID AS ServiceTypeFamilyID, sf.Description AS ServiceTypeFamilyName
          FROM ServiceTypeEnum s 
            JOIN ServiceTypeFamilyEnum sf on s.fk_ServiceTypeFamilyID = sf.ID"
      );

      PerhapsCloseConnection();

      return serviceTypes;
    }

    //public Subscription GetSubscription(string subscriptionUid)
    //{
    //  PerhapsOpenConnection();

    //  var subscription = Connection.Query<Models.Subscription>
    //  (@"SELECT 
    //              SubscriptionUID, CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, EffectiveUTC, LastActionedUTC
    //          FROM Subscription
    //          WHERE SubscriptionUID = @subscriptionUid"
    //    , new { subscriptionUid }
    //  ).FirstOrDefault();

    //  PerhapsCloseConnection();

    //  return subscription;
    //}

    //public IEnumerable<Models.Subscription> GetSubscriptions()
    //{
    //  PerhapsOpenConnection();

    //  var subscriptions = Connection.Query<Models.Subscription>
    //  (@"SELECT 
    //              SubscriptionUID, CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, EffectiveUTC, LastActionedUTC
    //          FROM Subscription"
    //  );

    //  PerhapsCloseConnection();

    //  return subscriptions;
    //}

  }
}
