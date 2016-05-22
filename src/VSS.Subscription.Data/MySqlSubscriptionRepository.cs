using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapper;
using VSS.Landfill.Common.Repositories;
using VSS.Project.Data.Interfaces;
using VSS.Subscription.Data.Models;
using log4net;
using VSS.Subscription.Data.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Subscription.Data
{
  public class MySqlSubscriptionRepository : RepositoryBase, ISubscriptionService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //private readonly string _connectionString;
        public Dictionary<string, Models.ServiceType> _serviceTypes = null;

        public MySqlSubscriptionRepository()
        {
          _serviceTypes = GetServiceTypes().ToDictionary(k => k.Name, v => v);
        }

 
        public int StoreSubscription(ISubscriptionEvent evt, IProjectService projectService)
        {
          var upsertedCount = 0;
          string eventType = "Unknown";

          if (evt is CreateProjectSubscriptionEvent)
          {
            //Only store Landfill subscriptions
            var subscriptionEvent = (CreateProjectSubscriptionEvent)evt;
            if (subscriptionEvent.SubscriptionType.ToLower() == "landfill")
            {
              var subscription = new Models.Subscription();

              subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
              subscription.CustomerUID = subscriptionEvent.CustomerUID.ToString();
              subscription.ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID;
              subscription.StartDate = subscriptionEvent.StartDate;
              //This is to handle CG subscriptions where we set the EndDate annually.
              //In NG the end date is the maximum unless it is cancelled/terminated.
              subscription.EndDate = subscriptionEvent.EndDate > DateTime.UtcNow ? new DateTime(9999, 12, 31) : subscriptionEvent.EndDate;
              subscription.LastActionedUTC = subscriptionEvent.ActionUTC;

              eventType = "CreateProjectSubscriptionEvent";

              upsertedCount = UpsertSubscriptionDetail(subscription, eventType);
            }
          }
          else if (evt is UpdateProjectSubscriptionEvent)
          {
            var subscriptionEvent = (UpdateProjectSubscriptionEvent)evt;
            if (subscriptionEvent.SubscriptionType.ToLower() == "landfill")
            {
              var subscription = new Models.Subscription();

              subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
              subscription.CustomerUID = subscriptionEvent.CustomerUID.HasValue ? subscriptionEvent.CustomerUID.Value.ToString() : null;
              subscription.ServiceTypeID = _serviceTypes[subscriptionEvent.SubscriptionType].ID;
              subscription.StartDate = subscriptionEvent.StartDate ?? DateTime.MinValue;
              subscription.EndDate = subscriptionEvent.EndDate ?? DateTime.MinValue;
              subscription.LastActionedUTC = subscriptionEvent.ActionUTC;

              eventType = "UpdateProjectSubscriptionEvent";

              upsertedCount = UpsertSubscriptionDetail(subscription, eventType);
            }
          }
          else if (evt is AssociateProjectSubscriptionEvent)
          {
            var subscription = new Models.Subscription();

            var subscriptionEvent = (AssociateProjectSubscriptionEvent)evt;

            subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
            subscription.CustomerUID = String.Empty;            
            subscription.EffectiveUTC = subscriptionEvent.EffectiveDate;
            subscription.LastActionedUTC = subscriptionEvent.ActionUTC;

            eventType = "AssociateProjectSubscriptionEvent";

            upsertedCount = UpsertSubscriptionDetail(subscription, eventType);

            if (upsertedCount > 0)
            {
              PerhapsOpenConnection();

              var project = Connection.Query<Project.Data.Models.Project>
                (@"SELECT ProjectUID, LastActionedUTC
                  FROM Project
                  WHERE ProjectUID = @ProjectUID", new { subscriptionEvent.ProjectUID }).FirstOrDefault();

              PerhapsCloseConnection();
              
              if (project == null)
                {
                  upsertedCount = projectService.StoreProject(
                    new CreateProjectEvent(){ ProjectUID = subscriptionEvent.ProjectUID, 
                                              ProjectName = String.Empty,
                                              ProjectTimezone = String.Empty, 
                                              ActionUTC = subscriptionEvent.ActionUTC });
                }

              if (upsertedCount > 0)
              {
                PerhapsOpenConnection();

                const string update =
                  @"UPDATE Project                
                    SET SubscriptionUID = @subscriptionUID, LastActionedUTC = @minActionDate
                    WHERE ProjectUID = @projectUID";

                upsertedCount = Connection.Execute(update, new { projectUID = subscriptionEvent.ProjectUID, subscriptionUID = subscriptionEvent.SubscriptionUID, minActionDate = DateTime.MinValue });

                PerhapsCloseConnection();
              }
            }
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
        private int UpsertSubscriptionDetail(Models.Subscription subscription, string eventType)
        {
          int upsertedCount = 0;

          PerhapsOpenConnection();

          Log.DebugFormat("SubscriptionRepository: Upserting eventType{0} SubscriptionUID={1}", eventType, subscription.SubscriptionUID);

          var existing = Connection.Query<Models.Subscription>
            (@"SELECT 
                SubscriptionUID, CustomerUID, EffectiveUTC, LastActionedUTC, StartDate, EndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Subscription
              WHERE SubscriptionUID = @subscriptionUID", new { subscriptionUID = subscription.SubscriptionUID }).FirstOrDefault();

          if (eventType == "CreateProjectSubscriptionEvent")
          {
            upsertedCount = CreateProjectSubscriptionEx(subscription, existing);
          }

          if (eventType == "UpdateProjectSubscriptionEvent")
          {
            upsertedCount = UpdateProjectSubscriptionEx(subscription, existing);
          }

          if (eventType == "AssociateProjectSubscriptionEvent")
          {
            upsertedCount = AssociateProjectSubscriptionEx(subscription, existing);
          }

          Log.DebugFormat("SubscriptionRepository: upserted {0} rows", upsertedCount);

          PerhapsCloseConnection();

          return upsertedCount;
        }

        private int CreateProjectSubscriptionEx(Models.Subscription subscription, Models.Subscription existing)
        {
          if (existing == null)
          {
            const string insert =
              @"INSERT Subscription
                (SubscriptionUID, CustomerUID, StartDate, EffectiveUTC, LastActionedUTC, EndDate, fk_ServiceTypeID)
                VALUES
                (@SubscriptionUID, @CustomerUID, @StartDate, @EffectiveUTC, @LastActionedUTC, @EndDate, @ServiceTypeID)";

            return Connection.Execute(insert, subscription);
          }

          Log.DebugFormat("SubscriptionRepository: can't create as already exists newActionedUTC {0}. So, the existing entry should be updated.", subscription.LastActionedUTC);

          return UpdateProjectSubscriptionEx(subscription, existing);
        }

        private int UpdateProjectSubscriptionEx(Models.Subscription subscription, Models.Subscription existing)
        {
          if (existing != null)
          {
            if (subscription.LastActionedUTC >= existing.LastActionedUTC)
            {
              //subscription only has values for columns to be updated
              if (string.IsNullOrEmpty(subscription.CustomerUID))
                subscription.CustomerUID = existing.CustomerUID;
              if (subscription.StartDate == DateTime.MinValue)
                subscription.StartDate = existing.StartDate;
              if (subscription.EndDate == DateTime.MinValue)
                subscription.EndDate = existing.EndDate;

              const string update =
                @"UPDATE Subscription                
                SET SubscriptionUID = @SubscriptionUID,
                    CustomerUID = @CustomerUID,
                    StartDate=@StartDate, 
                    EndDate=@EndDate, 
                    fk_ServiceTypeID=@ServiceTypeID,
                    LastActionedUTC=@LastActionedUTC
              WHERE SubscriptionUID = @SubscriptionUID";
              return Connection.Execute(update, subscription);
            }

            Log.DebugFormat("SubscriptionRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
              existing.LastActionedUTC, subscription.LastActionedUTC);
          }
          else
          {
            Log.DebugFormat("SubscriptionRepository: can't update as none existing newActionedUTC {0}",
              subscription.LastActionedUTC);
          }
          return 0;
        }

        private int AssociateProjectSubscriptionEx(Models.Subscription subscription, Models.Subscription existing)
        {
          if (existing != null)
          {
            if (subscription.LastActionedUTC >= existing.LastActionedUTC)
            {
              const string update =
                @"UPDATE Subscription                
                  SET SubscriptionUID = @SubscriptionUID,
                      EffectiveUTC = @EffectiveUTC,
                      LastActionedUTC = @LastActionedUTC
                  WHERE SubscriptionUID = @SubscriptionUID";
              return Connection.Execute(update, subscription);
            }
              Log.DebugFormat("SubscriptionRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
                existing.LastActionedUTC, subscription.LastActionedUTC);
          }
          else
          {
            Log.DebugFormat("SubscriptionRepository: can't update as none existing newActionedUTC {0}. So, a new entry should be created.", subscription.LastActionedUTC);

            return CreateProjectSubscriptionEx(subscription, null);
          }

          return 0;
      }

      private IEnumerable<Models.ServiceType> GetServiceTypes()
      {
        PerhapsOpenConnection();
        
        Log.Debug("SubscriptionRepository: Getting service types");

        var serviceTypes = Connection.Query<ServiceType>
            (@"SELECT 
                s.ID, s.Description AS Name, sf.ID AS ServiceTypeFamilyID, sf.Description AS ServiceTypeFamilyName
              FROM ServiceTypeEnum s JOIN ServiceTypeFamilyEnum sf on s.fk_ServiceTypeFamilyID = sf.ID"
            );

        PerhapsCloseConnection();

        return serviceTypes;
      }

      public Models.Subscription GetSubscription(string subscriptionUid)
      {
        PerhapsOpenConnection();

        var subscription = Connection.Query<Models.Subscription>
          (@"SELECT 
                  SubscriptionUID, CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, EffectiveUTC, LastActionedUTC
              FROM Subscription
              WHERE SubscriptionUID = @subscriptionUid"
            , new { subscriptionUid }
          ).FirstOrDefault();

        PerhapsCloseConnection();

        return subscription;
      }

      public IEnumerable<Models.Subscription> GetSubscriptions()
      {
        PerhapsOpenConnection();

        var subscriptions = Connection.Query<Models.Subscription>
          (@"SELECT 
                  SubscriptionUID, CustomerUID, fk_ServiceTypeID AS ServiceTypeID, StartDate, EndDate, EffectiveUTC, LastActionedUTC
              FROM Subscription"
           );

        PerhapsCloseConnection();

        return subscriptions;
      }

    }
}
