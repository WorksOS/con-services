using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Dapper;
using MySql.Data.MySqlClient;
using VSS.Project.Data.Interfaces;
using VSS.Project.Data.Models;
using VSS.Subscription.Data.Models;
using log4net;
using VSS.Subscription.Data.Interfaces;

namespace VSS.Subscription.Data
{
    public class MySqlSubscriptionRepository : ISubscriptionService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _connectionString;
        private Dictionary<string, Models.ServiceType> _serviceTypes = null;

        public MySqlSubscriptionRepository()
        {
          _connectionString = ConfigurationManager.ConnectionStrings["MySql.Connection"].ConnectionString;

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
            subscription.EffectiveUTC = subscriptionEvent.EffectiveDate;
            subscription.LastActionedUTC = subscriptionEvent.ActionUTC;

            eventType = "AssociateProjectSubscriptionEvent";

            upsertedCount = UpsertSubscriptionDetail(subscription, eventType);

            if (upsertedCount > 0)
            {
              var connection = new MySqlConnection(_connectionString);

              connection.Open();

              var project = connection.Query<Project.Data.Models.Project>
                (@"SELECT ProjectUID, LastActionedUTC
                  FROM Project
                  WHERE ProjectUID = @ProjectUid", new { subscriptionEvent.ProjectUID }).FirstOrDefault();
              connection.Close();
              
              if (project == null)
                {
                  upsertedCount = projectService.StoreProject(new CreateProjectEvent(){ ProjectUID = subscriptionEvent.ProjectUID, ActionUTC = subscriptionEvent.ActionUTC });
                }

              if (upsertedCount > 0)
              {
                const string update =
                  @"UPDATE Project                
                    SET SubscriptionUID = @SubscriptionUID,
                    LastActionedUTC = @minActionDate,
                    WHERE ProjectUID = @ProjectUID";

                upsertedCount = connection.Execute(update, new { subscriptionEvent.ProjectUID, subscriptionEvent.SubscriptionUID, minActionDate = DateTime.MinValue });
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
          using (var connection = new MySqlConnection(_connectionString))
          {
            Log.DebugFormat("SubscriptionRepository: Upserting eventType{0} SubscriptionUID={1}", eventType, subscription.SubscriptionUID);

            connection.Open();
            var existing = connection.Query<Models.Subscription>
              (@"SELECT 
                  SubscriptionUID, CustomerUID, EffectiveUTC, LastActionedUTC, StartDate, EndDate, fk_ServiceTypeID
                FROM Subscription
                WHERE SubscriptionUID = @subscriptionUID", new { subscriptionUID = subscription.SubscriptionUID }).FirstOrDefault();

            if (eventType == "CreateProjectSubscriptionEvent")
            {
              upsertedCount = CreateProjectSubscriptionEx(connection, subscription, existing);
            }

            if (eventType == "UpdateProjectSubscriptionEvent")
            {
              upsertedCount = UpdateProjectSubscriptionEx(connection, subscription, existing);
            }

            if (eventType == "AssociateProjectSubscriptionEvent")
            {
              upsertedCount = AssociateProjectSubscriptionEx(connection, subscription, existing);
            }

            Log.DebugFormat("SubscriptionRepository: upserted {0} rows", upsertedCount);
            connection.Close();
          }
          return upsertedCount;
        }

        private int CreateProjectSubscriptionEx(MySqlConnection connection, Models.Subscription subscription, Models.Subscription existing)
        {
          if (existing == null)
          {
            const string insert =
              @"INSERT Subscription
                (SubscriptionUID, CustomerUID, StartDate, EffectiveUTC, LastActionedUTC, EndDate, fk_ServiceTypeID)
                VALUES
                (@SubscriptionUID, @CustomerUID, @StartDate, @EffectiveUTC, @LastActionedUTC, @EndDate, @ServiceTypeID)";

            return connection.Execute(insert, subscription);
          }

          Log.DebugFormat("SubscriptionRepository: can't create as already exists newActionedUTC {0}. So, the existing entry should be updated.", subscription.LastActionedUTC);

          return UpdateProjectSubscriptionEx(connection, subscription, existing);
        }

        private int UpdateProjectSubscriptionEx(MySqlConnection connection, Models.Subscription subscription, Models.Subscription existing)
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
                    fk_ServiceTypeId=@ServiceTypeID,
                    LastActionedUTC=@LastActionedUTC
              WHERE SubscriptionUID = @SubscriptionUID";
              return connection.Execute(update, subscription);
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

        private int AssociateProjectSubscriptionEx(MySqlConnection connection, Models.Subscription subscription, Models.Subscription existing)
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
              return connection.Execute(update, subscription);
            }
              Log.DebugFormat("SubscriptionRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
                existing.LastActionedUTC, subscription.LastActionedUTC);
          }
          else
          {
            Log.DebugFormat("SubscriptionRepository: can't update as none existing newActionedUTC {0}. So, a new entry should be created.", subscription.LastActionedUTC);

            return CreateProjectSubscriptionEx(connection, subscription, null);
          }

          return 0;
      }

      private IEnumerable<Models.ServiceType> GetServiceTypes()
      {
        IEnumerable<Models.ServiceType> serviceTypes;
        using (var connection = new MySqlConnection(_connectionString))
        {
          Log.Debug("SubscriptionRepository: Getting service types");

          connection.Open();
          serviceTypes = connection.Query<Models.ServiceType>
              (@"SELECT 
                  s.ID, s.Description AS Name, sf.ID AS ServiceTypeFamilyID, sf.Description AS ServiceTypeFamilyName
                FROM ServiceTypeEnum s JOIN ServiceTypeFamilyEnum sf on s.fk_ServiceTypeFamilyID = sf.ID"
              );
          connection.Close();
        }
        return serviceTypes;
      }
    }


}
