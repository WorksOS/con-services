using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper;
using Microsoft.SqlServer.Server;
using MySql.Data.MySqlClient;
using VSS.Project.Data.Interfaces;
using VSS.Project.Data.Models;
using VSS.Subscription.Data.Models;
using VSS.Subscription.Data.Helpers;
using log4net;
using Newtonsoft.Json;
using VSS.Subscription.Data.Interfaces;

namespace VSS.Subscription.Data
{
    public class MySqlSubscriptionRepository : ISubscriptionService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static class ColumnName
        {
            public const string AssetSubscriptionID = "AssetSubscriptionID";
            public const string AssetSubscriptionUID = "AssetSubscriptionUID";
            public const string AssetUID = "fk_AssetUID";
            public const string DeviceUID = "fk_DeviceUID";
            public const string fk_CustomerSubscriptionID = "fk_CustomerSubscriptionID";
            public const string CustomerSubscriptionID = "CustomerSubscriptionID";
            public const string CustomerUID = "fk_CustomerUID";
            public const string fk_ServiceTypeID = "fk_ServiceTypeID";
            public const string StartDate = "StartDate";
            public const string EndDate = "EndDate";
            public const string InsertUTC = "InsertUTC";
            public const string UpdateUTC = "UpdateUTC";
            public const string ServiceTypeID = "ServiceTypeID";
            public const string Name = "Name";
            public const string FamilyName = "FamilyName";
            public const string ServiceTypeFamilyID = "ServiceTypeFamilyID";
            public const string fk_ServiceTypeFamilyID = "fk_ServiceTypeFamilyID";
            public const string ProjectSubscriptionID = "ProjectSubscriptionID";
            public const string ProjectSubscriptionUID = "ProjectSubscriptionUID";
            public const string ProjectUID = "fk_ProjectUID";

            //Alias ColumnName
        }

        private readonly string _connectionString;
        private Dictionary<string, Int64> _assetSubscriptionTypeCache = new Dictionary<string, Int64>();
        private Dictionary<string, Int64> _projectSubscriptionTypeCache = new Dictionary<string, Int64>();
        private Dictionary<string, Int64> _customerSubscriptionTypeCache = new Dictionary<string, Int64>();

        public MySqlSubscriptionRepository()
        {
          _connectionString = ConfigurationManager.ConnectionStrings["MySql.Connection"].ConnectionString;
          GetServicePlan();
        }

        private static readonly string ReadAllServiceViewQuery = string.Format("select st.{0},st.{1},stf.{4} from ServiceType st inner join ServiceTypeFamily stf on st.{2} = stf.{3}", ColumnName.Name, ColumnName.ServiceTypeID, ColumnName.fk_ServiceTypeFamilyID, ColumnName.ServiceTypeFamilyID, ColumnName.FamilyName);

        private static readonly string ReadAllCustomerProjectSubscriptionQuery =
                string.Format("select project.{0} ,project.{1} as StartDate, project.{2} as EndDate,customer.{3} as CustomerSubscriptionId,customer.{4} as SubscriptionTypeId from CustomerSubscription customer left outer join ProjectSubscription project on project.{5} = customer.{3}",
                 ColumnName.ProjectUID, ColumnName.StartDate, ColumnName.EndDate, ColumnName.CustomerSubscriptionID, ColumnName.fk_ServiceTypeID, ColumnName.fk_CustomerSubscriptionID);

        private static readonly string ReadAllCustomerAssetSubscriptionQuery =
                string.Format("select asset.{0} ,asset.{1} as StartDate, asset.{2} as EndDate,customer.{3} as CustomerSubscriptionId,customer.{4} as SubscriptionTypeId from CustomerSubscription customer left outer join AssetSubscription asset on asset.{5} = customer.{3}",
                 ColumnName.AssetUID, ColumnName.StartDate, ColumnName.EndDate, ColumnName.CustomerSubscriptionID, ColumnName.fk_ServiceTypeID, ColumnName.fk_CustomerSubscriptionID);

        private static readonly string InsertAssetSubscriptionQuery =
                string.Format(
                                "insert into AssetSubscription ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}) ",
                                ColumnName.AssetSubscriptionUID, ColumnName.AssetUID, ColumnName.DeviceUID, ColumnName.fk_CustomerSubscriptionID, ColumnName.StartDate, ColumnName.EndDate, ColumnName.InsertUTC, ColumnName.UpdateUTC
                                );

        private static readonly string InsertProjectSubscriptionQuery =
        string.Format(
                        "insert into ProjectSubscription ({0}, {1}, {2}, {3}, {4}, {5}) ",
                        ColumnName.ProjectSubscriptionUID, ColumnName.fk_CustomerSubscriptionID, ColumnName.StartDate, ColumnName.EndDate, ColumnName.InsertUTC, ColumnName.UpdateUTC
                        );

        private static readonly string InsertCustomerSubscriptionQuery =
            string.Format(
                    "insert into CustomerSubscription ({0}, {1}, {2}, {3}, {4}, {5}) ",
                    ColumnName.CustomerUID, ColumnName.fk_ServiceTypeID, ColumnName.StartDate, ColumnName.EndDate, ColumnName.InsertUTC, ColumnName.UpdateUTC);

        public int StoreSubscription(ISubscriptionEvent evt, IProjectService projectService)
        {
          var upsertedCount = 0;
          string eventType = "Unknown";

          if (evt is CreateAssetSubscriptionEvent)
          {
            upsertedCount = CreateAssetSubscription((CreateAssetSubscriptionEvent)evt);
          }
          else if (evt is UpdateAssetSubscriptionEvent)
          {
            upsertedCount = UpdateAssetSubscription((UpdateAssetSubscriptionEvent)evt);
          }
          else if (evt is CreateProjectSubscriptionEvent)
          {
            //upsertedCount = CreateProjectSubscription((CreateProjectSubscriptionEvent)evt);

            var subscription = new Models.Subscription();

            var subscriptionEvent = (CreateProjectSubscriptionEvent)evt;

            subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
            subscription.CustomerUID = subscriptionEvent.CustomerUID.ToString();
            subscription.StartDate = subscriptionEvent.StartDate;
            subscription.EndDate = subscriptionEvent.EndDate;
            subscription.LastActionedUTC = subscriptionEvent.ActionUTC;

            eventType = "CreateProjectSubscriptionEvent";

            upsertedCount = UpsertSubscriptionDetail(subscription, eventType);
          }
          else if (evt is UpdateProjectSubscriptionEvent)
          {
            //upsertedCount = UpdateProjectSubscription((UpdateProjectSubscriptionEvent)evt);

            var subscription = new Models.Subscription();

            var subscriptionEvent = (UpdateProjectSubscriptionEvent)evt;

            subscription.SubscriptionUID = subscriptionEvent.SubscriptionUID.ToString();
            subscription.CustomerUID = subscriptionEvent.CustomerUID.ToString();
            subscription.StartDate = subscriptionEvent.StartDate ?? DateTime.UtcNow;
            subscription.EndDate = subscriptionEvent.EndDate ?? DateTime.UtcNow;
            subscription.LastActionedUTC = subscriptionEvent.ActionUTC;

            eventType = "UpdateProjectSubscriptionEvent";

            upsertedCount = UpsertSubscriptionDetail(subscription, eventType);
          }
          else if (evt is AssociateProjectSubscriptionEvent)
          {
            //upsertedCount = AssociateProjectSubscription((DissociateProjectSubscriptionEvent)evt);

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
          else if (evt is DissociateProjectSubscriptionEvent)
          {
            upsertedCount = DissociateProjectSubscription((DissociateProjectSubscriptionEvent)evt);
          }
          else if (evt is CreateCustomerSubscriptionEvent)
          {
            upsertedCount = CreateCustomerSubscription((CreateCustomerSubscriptionEvent)evt);
          }
          else if (evt is UpdateCustomerSubscriptionEvent)
          {
            upsertedCount = UpdateCustomerSubscription((UpdateCustomerSubscriptionEvent)evt);
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
                  SubscriptionUID, CustomerUID, EffectiveUTC, LastActionedUTC, StartDate, EndDate
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
                (SubscriptionUID, CustomerUID, StartDate, EffectiveUTC, LastActionedUTC, EndDate)
                VALUES
                (@SubscriptionUID, @CustomerUID, @StartDate, @EffectiveUTC, @LastActionedUTC, @EndDate)";

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
              const string update =
                @"UPDATE Subscription                
                SET SubscriptionUID = @SubscriptionUID,
                    CustomerUID = @CustomerUID,
                    StartDate=@StartDate, 
                    EndDate=@EndDate, 
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
                      EffectiveDate = @EffectiveUTC,
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





      private void GetServicePlan()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                MySqlCommand mySqlCommand = new MySqlCommand(ReadAllServiceViewQuery, connection);
                connection.Open();
                MySqlDataReader mySqlDataReader;
                mySqlDataReader = mySqlCommand.ExecuteReader();
                while (mySqlDataReader.Read())
                {
                    var familyName = mySqlDataReader.GetString(ColumnName.FamilyName).ToLowerInvariant();

                    if (familyName == "asset")
                        _assetSubscriptionTypeCache.Add(mySqlDataReader.GetString(ColumnName.Name).ToLowerInvariant(), mySqlDataReader.GetInt64(ColumnName.ServiceTypeID));

                    if (familyName == "project")
                        _projectSubscriptionTypeCache.Add(mySqlDataReader.GetString(ColumnName.Name).ToLowerInvariant(), mySqlDataReader.GetInt64(ColumnName.ServiceTypeID));

                    if (familyName == "customer")
                        _customerSubscriptionTypeCache.Add(mySqlDataReader.GetString(ColumnName.Name).ToLowerInvariant(), mySqlDataReader.GetInt64(ColumnName.ServiceTypeID));
                }
            }
        }

        # region Asset Subscription
        private class AssetSubscriptionIdentifiers
        {
            public string customerUID { get; set; }
            public string assetUID { get; set; }
            public long subscriptionID { get; set; }
        }

        public int CreateAssetSubscription(CreateAssetSubscriptionEvent subscription)
        {
            int upsertedCount = 0;

            if (subscription.SubscriptionType != null && !_assetSubscriptionTypeCache.ContainsKey(subscription.SubscriptionType.ToLowerInvariant()))
                throw new Exception("Invalid Asset Subscription Type");

            using (var connection = new MySqlConnection(_connectionString))
            {
                var readQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerAssetSubscriptionQuery,
                        ColumnName.CustomerUID, subscription.CustomerUID, ColumnName.fk_ServiceTypeID, _assetSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()]);

                connection.Open();

                var customerSubscriptionDataList = connection.Query<CustomerAssetSubscriptionData>(readQuery).ToList();
                connection.Close();


                if (customerSubscriptionDataList.Count == 0)
                {
                    //Insert Customer Subscription
                    connection.Open();
                    upsertedCount = connection.Execute(
                        string.Format(InsertCustomerSubscriptionQuery +
                            "values (@fk_CustomerUID, @fk_ServiceTypeID, @StartDate, @EndDate, @InsertUTC, @UpdateUTC);"
                    ),
                    new
                    {
                        fk_CustomerUID = subscription.CustomerUID,
                        fk_ServiceTypeID = _assetSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()],
                        StartDate = subscription.StartDate,
                        EndDate = subscription.EndDate,
                        InsertUTC = DateTime.UtcNow,
                        UpdateUTC = DateTime.UtcNow
                    });
                    connection.Close();
                }
                else
                {
                    ////Update Customer Subscription

                    //Adding the incoming Asset Subscription UID into List to calculate correct min/max Dates
                    var incomingSubscriptionData = new CustomerAssetSubscriptionData();
                    incomingSubscriptionData.StartDate = subscription.StartDate;
                    incomingSubscriptionData.EndDate = subscription.EndDate;
                    customerSubscriptionDataList.Add(incomingSubscriptionData);

                    upsertedCount = UpdateSubscriptionDatesForCustomer(connection, subscription.CustomerUID.ToString(), _assetSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()], customerSubscriptionDataList);

                    if (upsertedCount == 0)
                    {
                      Log.Error(String.Format("CreateAssetSubscription: Failed to update Customer Subscription with UID {0} for existing customer with UID {1}.", subscription.SubscriptionUID, subscription.CustomerUID));
                      return upsertedCount;
                    }
                }

                if (upsertedCount == 0)
                {
                  Log.Error(String.Format("CreateAssetSubscription: Failed to create Asset Subscription for customer with UID {0}", subscription.CustomerUID));
                  return upsertedCount;
                }

                //Getting Customer SubscriptionId
                long customerSubscriptionId = GetCustomerSubscriptionID(connection, subscription.CustomerUID.ToString(), _assetSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()]);

                //Insert Asset Subscription
                connection.Open();
                upsertedCount = connection.Execute(
                        string.Format(InsertAssetSubscriptionQuery +
                                    "values (@AssetSubscriptionUID,@fk_AssetUID, @fk_DeviceUID, @fk_CustomerSubscriptionID, @StartDate, @EndDate, @InsertUTC, @UpdateUTC);"),
                        new
                        {
                            AssetSubscriptionUID = subscription.SubscriptionUID.ToString(),
                            fk_AssetUID = subscription.AssetUID.ToString(),
                            fk_DeviceUID = subscription.DeviceUID.ToString(),
                            fk_CustomerSubscriptionID = customerSubscriptionId,
                            StartDate = subscription.StartDate,
                            EndDate = subscription.EndDate,
                            InsertUTC = DateTime.UtcNow,
                            UpdateUTC = DateTime.UtcNow
                        });
                connection.Close();
            }

          return upsertedCount;
        }

        public int UpdateAssetSubscription(UpdateAssetSubscriptionEvent updateSubscription)
        {
            int upsertedCount = 0;

            if (updateSubscription.SubscriptionType != null && !_assetSubscriptionTypeCache.ContainsKey(updateSubscription.SubscriptionType.ToLowerInvariant()))
                throw new Exception("Invalid Asset Subscription Type");

            using (var connection = new MySqlConnection(_connectionString))
            {
                string newCustomerUID = String.Empty;
                long newSubscriptionID = 0;

                //Getting Old Customer Asset Relation
                var readCustomerAssetQuery = String.Format("select customer.{0} as customerUID,customer.{1} as subscriptionID,asset.{2} as assetUID from AssetSubscription asset inner join CustomerSubscription customer on asset.{3} = customer.{4} where asset.{5} = '{6}'",
                        ColumnName.CustomerUID, ColumnName.fk_ServiceTypeID, ColumnName.AssetUID, ColumnName.fk_CustomerSubscriptionID, ColumnName.CustomerSubscriptionID, ColumnName.AssetSubscriptionUID, updateSubscription.SubscriptionUID);
                connection.Open();
                AssetSubscriptionIdentifiers ids = connection.Query<AssetSubscriptionIdentifiers>(readCustomerAssetQuery).FirstOrDefault();
                connection.Close();

                if (ids == null) // todo Merino-fix
                {
                  Log.Error(String.Format("UpdateAssetSubscription: Subscription with UID {0} doesn't exist.", updateSubscription.SubscriptionUID));
                  return upsertedCount;
                }

                newCustomerUID = updateSubscription.CustomerUID.HasValue ? updateSubscription.CustomerUID.Value.ToString() : ids.customerUID;
                newSubscriptionID = string.IsNullOrWhiteSpace(updateSubscription.SubscriptionType) ? ids.subscriptionID : _assetSubscriptionTypeCache[updateSubscription.SubscriptionType.ToLowerInvariant()];
                Log.DebugFormat("UpdateAssetSubscription {0}", JsonConvert.SerializeObject(updateSubscription));
              
                //Update Asset Subscription with Rest Of fields
                if (updateSubscription.AssetUID.HasValue || updateSubscription.DeviceUID.HasValue
                  ||updateSubscription.StartDate.HasValue || updateSubscription.EndDate.HasValue)
                {
                    var sbAsset = new StringBuilder();
                    bool assetCommaNeeded = false;
                    if (updateSubscription.AssetUID.HasValue)
                    {
                        assetCommaNeeded |= SqlBuilder.AppendValueParameter(updateSubscription.AssetUID.Value, ColumnName.AssetUID, sbAsset, assetCommaNeeded);
                        ids.assetUID = updateSubscription.AssetUID.Value.ToString();
                    }
                    if (updateSubscription.DeviceUID.HasValue)
                    {
                        assetCommaNeeded |= SqlBuilder.AppendValueParameter(updateSubscription.DeviceUID.Value, ColumnName.DeviceUID, sbAsset, assetCommaNeeded);
                    }
                    if (updateSubscription.StartDate.HasValue)
                        assetCommaNeeded |= SqlBuilder.AppendValueParameter(updateSubscription.StartDate.Value, ColumnName.StartDate, sbAsset, assetCommaNeeded);
                    if (updateSubscription.EndDate.HasValue)
                        assetCommaNeeded |= SqlBuilder.AppendValueParameter(updateSubscription.EndDate.Value, ColumnName.EndDate, sbAsset, assetCommaNeeded);
                    assetCommaNeeded |= SqlBuilder.AppendValueParameter(DateTime.UtcNow, ColumnName.UpdateUTC, sbAsset, assetCommaNeeded);

                    var updateAssetSubscriptionQuery = string.Format("Update AssetSubscription set {0} where {1} = '{2}'",
                         sbAsset, ColumnName.AssetSubscriptionUID, updateSubscription.SubscriptionUID);
                    connection.Open();
                    upsertedCount = connection.Execute(updateAssetSubscriptionQuery, null, commandType: CommandType.Text);
                    connection.Close();

                    if (upsertedCount == 0)
                    {
                      Log.Error(String.Format("UpdateAssetSubscription: Failed to update Asset Subscription with UID {0}.", updateSubscription.SubscriptionUID));
                      return upsertedCount;
                    }
                   
                }

                if ((updateSubscription.CustomerUID.HasValue && ids.customerUID != newCustomerUID) || (!string.IsNullOrWhiteSpace(updateSubscription.SubscriptionType) && ids.subscriptionID != newSubscriptionID))
                {
                    var ReadOldCustomerAssetQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerAssetSubscriptionQuery,
                    ColumnName.CustomerUID, ids.customerUID, ColumnName.fk_ServiceTypeID, ids.subscriptionID);
                    connection.Open();
                    #region dapper

                    var customerSubscriptionDataList =
                        connection.Query<CustomerAssetSubscriptionData>(ReadOldCustomerAssetQuery)
                            .Where(x => x.fk_AssetUID != ids.assetUID)
                            .ToList();
                    #endregion


                    connection.Close();
                    if (customerSubscriptionDataList.Count > 0)
                    {
                        //Update Old Customer Subscription
                        upsertedCount = UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, customerSubscriptionDataList);

                      if (upsertedCount == 0)
                      {
                        Log.Error(String.Format("UpdateAssetSubscription: Failed to update Customer Subscription with UID {0} for existing customer with UID {1}.", ids.subscriptionID, ids.customerUID));
                        return upsertedCount;
                      }
                    }

                    //Updating New Customer Asset Relation 
                    long customerSubscriptionId = GetCustomerSubscriptionID(connection, newCustomerUID, newSubscriptionID);

                    if (customerSubscriptionId != 0)
                    {
                        var sb = new StringBuilder();
                        bool commaNeeded = false;
                        commaNeeded |= SqlBuilder.AppendValueParameter(customerSubscriptionId, ColumnName.fk_CustomerSubscriptionID, sb, commaNeeded);
                        commaNeeded |= SqlBuilder.AppendValueParameter(DateTime.UtcNow, ColumnName.UpdateUTC, sb, commaNeeded);
                        var updateCustomerSubscriptionIDQuery = string.Format("Update AssetSubscription set {0} where {1} = '{2}'",
                             sb, ColumnName.AssetSubscriptionUID, updateSubscription.SubscriptionUID);
                        connection.Open();
                        upsertedCount = connection.Execute(updateCustomerSubscriptionIDQuery, null, commandType: CommandType.Text);
                        connection.Close();

                        if (upsertedCount == 0)
                        {
                          Log.Error(String.Format("UpdateAssetSubscription: Failed to update Asset Subscription with UID {0} for new customer with UID {1}.", newSubscriptionID, newCustomerUID));
                          return upsertedCount;
                        }

                        // Updating CustomerData for new Customer
                        var ReadNewCustomerAssetQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerAssetSubscriptionQuery,
                        ColumnName.CustomerUID, newCustomerUID, ColumnName.fk_ServiceTypeID, newSubscriptionID);
                        connection.Open();

                        var newCustomerSubscriptionDataList =
                            connection.Query<CustomerAssetSubscriptionData>(ReadNewCustomerAssetQuery).ToList();

                        connection.Close();

                        //Update Customer Subscription
                        upsertedCount = UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, newCustomerSubscriptionDataList);

                        if (upsertedCount == 0)
                        {
                          Log.Error(String.Format("UpdateAssetSubscription: Failed to update Customer Subscription with UID {0} for existing customer with UID {1}.", ids.subscriptionID, ids.customerUID));
                          return upsertedCount;
                        }
                    }
                    else
                    {
                        //Query Start & End Date for Asset
                        DateTime assetStartDate = new DateTime();
                        DateTime assetEndDate = new DateTime();
                        if (!(updateSubscription.StartDate.HasValue && updateSubscription.EndDate.HasValue))
                        {
                            var ReadStartEndDateAssetQuery = String.Format("select {0},{1} from AssetSubscription where {2} = '{3}'",
                                ColumnName.StartDate, ColumnName.EndDate, ColumnName.AssetSubscriptionUID, updateSubscription.SubscriptionUID);
                            var mySqlReadStartEndDateAssetCommand = new MySqlCommand(ReadStartEndDateAssetQuery, connection);
                            MySqlDataReader mySqlReadStartEndDateAssetDataReader;
                            connection.Open();
                            mySqlReadStartEndDateAssetDataReader = mySqlReadStartEndDateAssetCommand.ExecuteReader();
                            while (mySqlReadStartEndDateAssetDataReader.Read())
                            {
                                assetStartDate = mySqlReadStartEndDateAssetDataReader.GetDateTime(ColumnName.StartDate);
                                assetEndDate = mySqlReadStartEndDateAssetDataReader.GetDateTime(ColumnName.EndDate);
                            }
                            connection.Close();
                        }

                        var newStartDate = updateSubscription.StartDate.HasValue ? updateSubscription.StartDate.Value : assetStartDate;
                        var newEndDate = updateSubscription.EndDate.HasValue ? updateSubscription.EndDate.Value : assetEndDate;

                        // Insert Customer 
                        connection.Open();
                        upsertedCount = connection.Execute(string.Format(InsertCustomerSubscriptionQuery +
                            "values (@CustomerUID, @fk_ServiceTypeID, @StartDate, @EndDate, @InsertUTC, @UpdateUTC);"
                            ),
                            new
                            {
                                CustomerUID = newCustomerUID,
                                fk_ServiceTypeID = newSubscriptionID,
                                StartDate = newStartDate,
                                EndDate = newEndDate,
                                InsertUTC = DateTime.UtcNow,
                                UpdateUTC = DateTime.UtcNow
                            });
                        connection.Close();

                        if (upsertedCount == 0)
                        {
                          Log.Error(String.Format("UpdateAssetSubscription: Failed to insert Customer Subscription with UID {0} for new customer with UID {1}.", newSubscriptionID, newCustomerUID));
                          return upsertedCount;
                        }

                        customerSubscriptionId = GetCustomerSubscriptionID(connection, newCustomerUID, newSubscriptionID);

                        var sb = new StringBuilder();
                        bool commaNeeded = false;
                        commaNeeded |= SqlBuilder.AppendValueParameter(customerSubscriptionId, ColumnName.fk_CustomerSubscriptionID, sb, commaNeeded);
                        commaNeeded |= SqlBuilder.AppendValueParameter(DateTime.UtcNow, ColumnName.UpdateUTC, sb, commaNeeded);

                        var updateCustomerSubscriptionIDQuery = string.Format("Update AssetSubscription set {0} where {1} = '{2}'",
                             sb, ColumnName.AssetSubscriptionUID, updateSubscription.SubscriptionUID);
                        connection.Open();
                        upsertedCount = connection.Execute(updateCustomerSubscriptionIDQuery, null, commandType: CommandType.Text);
                        connection.Close();

                        if (upsertedCount == 0)
                        {
                          Log.Error(String.Format("UpdateAssetSubscription: Failed to update Asset Subscription with UID {0} for new customer with UID {1}.", updateSubscription.SubscriptionUID, newCustomerUID));
                          return upsertedCount;
                        }
                    }

                    if (customerSubscriptionDataList.Count == 0)
                    {
                        //Delete Old Customer
                        string query = string.Format("Delete from CustomerSubscription where {0} = '{1}' and {2} = {3}", ColumnName.CustomerUID, ids.customerUID, ColumnName.fk_ServiceTypeID, ids.subscriptionID);
                        connection.Open();
                        upsertedCount = connection.Execute(query, commandType: CommandType.Text);
                        connection.Close();

                        if (upsertedCount == 0)
                        {
                          Log.Error(String.Format("UpdateAssetSubscription: Failed to delete Customer Subscription with UID {0} for new customer with UID {1}.", ids.subscriptionID, ids.customerUID));
                          return upsertedCount;
                        }
                    }
                }
                else
                {
                    // Updating CustomerData for Customer
                    var ReadExistingCustomerAssetQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerAssetSubscriptionQuery,
                    ColumnName.CustomerUID, newCustomerUID, ColumnName.fk_ServiceTypeID, newSubscriptionID);
                    connection.Open();
                    var customerSubscriptionDataList =
                                            connection.Query<CustomerAssetSubscriptionData>(ReadExistingCustomerAssetQuery)
                                                .ToList();
                    connection.Close();

                    //Update Customer Subscription
                    upsertedCount = UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, customerSubscriptionDataList);

                    if (upsertedCount == 0)
                    {
                      Log.Error(String.Format("UpdateAssetSubscription: Failed to update Customer Subscription with UID {0} for existing customer with UID {1}.", ids.subscriptionID, ids.customerUID));
                      return upsertedCount;
                    }
                }
            }

          return upsertedCount;
        }

        # endregion

        # region Project Subscription

        private class ProjectSubscriptionIdentifiers
        {
            public string customerUID { get; set; }
            public string projectUID { get; set; }
            public long subscriptionID { get; set; }
        }

        public int CreateProjectSubscription(CreateProjectSubscriptionEvent subscription)
        {
          int upsertedCount = 0;

          if (subscription.SubscriptionType != null && !_projectSubscriptionTypeCache.ContainsKey(subscription.SubscriptionType.ToLowerInvariant()))
            throw new Exception("Invalid Project Subscription Type");

            using (var connection = new MySqlConnection(_connectionString))
            {
                var readQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerProjectSubscriptionQuery,
                        ColumnName.CustomerUID, subscription.CustomerUID, ColumnName.fk_ServiceTypeID, _projectSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()]);

                connection.Open();

                var customerSubscriptionDataList = connection.Query<CustomerProjectSubscriptionData>(readQuery).ToList();
                Log.DebugFormat("CreateProjectSubscription(): readQuery {0} customerSubscriptionDataList {1}", readQuery, customerSubscriptionDataList);
                connection.Close();


                if (customerSubscriptionDataList.Count == 0)
                {
                    //Insert Customer Subscription
                    connection.Open();
                    upsertedCount = connection.Execute(
                    string.Format(InsertCustomerSubscriptionQuery +
                            "values (@fk_CustomerUID, @fk_ServiceTypeID, @StartDate, @EndDate, @InsertUTC, @UpdateUTC);"
                    ),
                    new
                    {
                        fk_CustomerUID = subscription.CustomerUID,
                        fk_ServiceTypeID = _projectSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()],
                        StartDate = subscription.StartDate,
                        EndDate = subscription.EndDate,
                        InsertUTC = DateTime.UtcNow,
                        UpdateUTC = DateTime.UtcNow
                    });
                    connection.Close();
                    Log.DebugFormat("CreateProjectSubscription(): cust-sub count 0 @fk_CustomerUID: {0} @fk_ServiceTypeID: {1} @StartDate: {2}, @EndDate {3}",
                                    subscription.CustomerUID, _projectSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()],
                                    subscription.StartDate, subscription.EndDate);

                }
                else
                {
                    ////Update Customer Subscription

                    //Adding the incoming Project Subscription UID into List to calculate correct min/max Dates
                    var incomingSubscriptionData = new CustomerProjectSubscriptionData();
                    incomingSubscriptionData.StartDate = subscription.StartDate;
                    incomingSubscriptionData.EndDate = subscription.EndDate;
                    customerSubscriptionDataList.Add(incomingSubscriptionData);

                    upsertedCount = UpdateSubscriptionDatesForCustomer(connection, subscription.CustomerUID.ToString(), _projectSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()], customerSubscriptionDataList);
                    
                    Log.DebugFormat("CreateProjectSubscription(): cust-sub count >0 @fk_CustomerUID: {0} @fk_ServiceTypeID: {1} @StartDate: {2}, @EndDate {3}",
                      subscription.CustomerUID, _projectSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()],
                      subscription.StartDate, subscription.EndDate);

                    if (upsertedCount == 0)
                    {
                      Log.Error(String.Format("CreateProjectSubscription: Failed to update Customer Subscription with UID {0} for existing customer with UID {1}.", subscription.SubscriptionUID, subscription.CustomerUID));
                      return upsertedCount;
                    }
                }

                if (upsertedCount == 0)
                {
                  Log.Error(String.Format("CreateProjectSubscription: Failed to create Project Subscription for customer with UID {0}", subscription.CustomerUID));
                  return upsertedCount;
                }

                //Getting Customer SubscriptionId
                long customerSubscriptionId = GetCustomerSubscriptionID(connection, subscription.CustomerUID.ToString(), _projectSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()]);

                Log.DebugFormat("CreateProjectSubscription(): cust-sub count >0 @fk_CustomerUID: {0} @fk_ServiceTypeID: {1} @StartDate: {2}, @EndDate {3}",
                        subscription.CustomerUID, _projectSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()],
                        subscription.StartDate, subscription.EndDate);

                //Insert Project Subscription
                connection.Open();
                upsertedCount = connection.Execute(
                        string.Format(InsertProjectSubscriptionQuery +
                                    "values (@ProjectSubscriptionUID, @fk_CustomerSubscriptionID, @StartDate, @EndDate, @InsertUTC, @UpdateUTC);"),
                        new
                        {
                            ProjectSubscriptionUID = subscription.SubscriptionUID.ToString(),
                            fk_CustomerSubscriptionID = customerSubscriptionId,
                            StartDate = subscription.StartDate,
                            EndDate = subscription.EndDate,
                            InsertUTC = DateTime.UtcNow,
                            UpdateUTC = DateTime.UtcNow
                        });
                connection.Close();
            }

          return upsertedCount;
        }

        public int UpdateProjectSubscription(UpdateProjectSubscriptionEvent updateSubscription)
        {
          int upsertedCount = 0;

          if (updateSubscription.SubscriptionType != null && !_projectSubscriptionTypeCache.ContainsKey(updateSubscription.SubscriptionType.ToLowerInvariant()))
            throw new Exception("Invalid Project Subscription Type");

            using (var connection = new MySqlConnection(_connectionString))
            {
                string newCustomerUID = String.Empty;
                long newSubscriptionID = 0;

                //Getting Old Customer Project Relation
                var readCustomerProjectQuery = String.Format("select customer.{0} as customerUID,customer.{1} as subscriptionID,project.{2} as projectUID from ProjectSubscription project inner join CustomerSubscription customer on project.{3} = customer.{4} where project.{5} = '{6}'",
                        ColumnName.CustomerUID, ColumnName.fk_ServiceTypeID, ColumnName.ProjectUID, ColumnName.fk_CustomerSubscriptionID, ColumnName.CustomerSubscriptionID, ColumnName.ProjectSubscriptionUID, updateSubscription.SubscriptionUID);
                connection.Open();
                ProjectSubscriptionIdentifiers ids = connection.Query<ProjectSubscriptionIdentifiers>(readCustomerProjectQuery).FirstOrDefault();
                connection.Close();

                if (ids == null) // todo Merino-fix
                {
                  Log.Error(String.Format("UpdateProjectSubscription: Subscription with UID {0} doesn't exist.", updateSubscription.SubscriptionUID));
                  return upsertedCount;
                }

                newCustomerUID = updateSubscription.CustomerUID.HasValue ? updateSubscription.CustomerUID.Value.ToString() : ids.customerUID;
                newSubscriptionID = string.IsNullOrWhiteSpace(updateSubscription.SubscriptionType) ? ids.subscriptionID : _projectSubscriptionTypeCache[updateSubscription.SubscriptionType.ToLowerInvariant()];

                //Update Project Subscription with Rest Of fields
                if (updateSubscription.StartDate.HasValue || updateSubscription.EndDate.HasValue)
                {
                    var sbProject = new StringBuilder();
                    bool projectCommaNeeded = false;

                    if (updateSubscription.StartDate.HasValue)
                        projectCommaNeeded |= SqlBuilder.AppendValueParameter(updateSubscription.StartDate.Value, ColumnName.StartDate, sbProject, projectCommaNeeded);
                    if (updateSubscription.EndDate.HasValue)
                        projectCommaNeeded |= SqlBuilder.AppendValueParameter(updateSubscription.EndDate.Value, ColumnName.EndDate, sbProject, projectCommaNeeded);
                    projectCommaNeeded |= SqlBuilder.AppendValueParameter(DateTime.UtcNow, ColumnName.UpdateUTC, sbProject, projectCommaNeeded);

                    var updateProjectSubscriptionQuery = string.Format("Update ProjectSubscription set {0} where {1} = '{2}'",
                         sbProject, ColumnName.ProjectSubscriptionUID, updateSubscription.SubscriptionUID);
                    connection.Open();
                    upsertedCount = connection.Execute(updateProjectSubscriptionQuery, null, commandType: CommandType.Text);
                    connection.Close();

                    if (upsertedCount == 0)
                    {
                      Log.Error(String.Format("UpdateProjectSubscription: Failed to update Project Subscription with UID {0}.", updateSubscription.SubscriptionUID));
                      return upsertedCount;
                    }
                }

                if ((updateSubscription.CustomerUID.HasValue && ids.customerUID != newCustomerUID) || (!string.IsNullOrWhiteSpace(updateSubscription.SubscriptionType) && ids.subscriptionID != newSubscriptionID))
                {
                    var ReadOldCustomerProjectQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerProjectSubscriptionQuery,
                    ColumnName.CustomerUID, ids.customerUID, ColumnName.fk_ServiceTypeID, ids.subscriptionID);
                    connection.Open();
                    var customerSubscriptionDataList =
                        connection.Query<CustomerProjectSubscriptionData>(ReadOldCustomerProjectQuery)
                            .ToList();
                    connection.Close();

                    customerSubscriptionDataList = String.IsNullOrEmpty(ids.projectUID) ? customerSubscriptionDataList.Where(x => x.SubscriptionTypeId != ids.subscriptionID).ToList()
                        : customerSubscriptionDataList.Where(x => x.fk_ProjectUID != ids.projectUID).ToList();

                    if (customerSubscriptionDataList.Count > 0)
                    {
                        //Update Old Customer Subscription
                        upsertedCount = UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, customerSubscriptionDataList);

                      if (upsertedCount == 0)
                      {
                        Log.Error(String.Format("UpdateProjectSubscription: Failed to update Customer Subscription with UID {0} for existing customer with UID {1}.", ids.subscriptionID, ids.customerUID));
                        return upsertedCount;
                      }
                    }

                    //Updating New Customer Project Relation 
                    long customerSubscriptionId = GetCustomerSubscriptionID(connection, newCustomerUID, newSubscriptionID);

                    if (customerSubscriptionId != 0)
                    {
                        var sb = new StringBuilder();
                        bool commaNeeded = false;
                        commaNeeded |= SqlBuilder.AppendValueParameter(customerSubscriptionId, ColumnName.fk_CustomerSubscriptionID, sb, commaNeeded);
                        commaNeeded |= SqlBuilder.AppendValueParameter(DateTime.UtcNow, ColumnName.UpdateUTC, sb, commaNeeded);
                        var updateCustomerSubscriptionIDQuery = string.Format("Update ProjectSubscription set {0} where {1} = '{2}'",
                             sb, ColumnName.ProjectSubscriptionUID, updateSubscription.SubscriptionUID);
                        connection.Open();
                        upsertedCount = connection.Execute(updateCustomerSubscriptionIDQuery, null, commandType: CommandType.Text);
                        connection.Close();

                        if (upsertedCount == 0)
                        {
                          Log.Error(String.Format("UpdateProjectSubscription: Failed to update Project Subscription with UID {0} for new customer with UID {1}.", newSubscriptionID, newCustomerUID));
                          return upsertedCount;
                        }

                        // Updating CustomerData for new Customer
                        var ReadNewCustomerProjectQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerProjectSubscriptionQuery,
                        ColumnName.CustomerUID, newCustomerUID, ColumnName.fk_ServiceTypeID, newSubscriptionID);
                        connection.Open();

                        var newCustomerSubscriptionDataList =
                            connection.Query<CustomerProjectSubscriptionData>(ReadNewCustomerProjectQuery).ToList();

                        connection.Close();

                        //Update Customer Subscription
                        upsertedCount = UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, newCustomerSubscriptionDataList);

                        if (upsertedCount == 0)
                        {
                          Log.Error(String.Format("UpdateProjectSubscription: Failed to update Customer Subscription with UID {0} for existing customer with UID {1}.", ids.subscriptionID, ids.customerUID));
                          return upsertedCount;
                        }
                    }
                    else
                    {
                        //Query Start & End Date for Project
                        DateTime projectStartDate = new DateTime();
                        DateTime projectEndDate = new DateTime();
                        if (!(updateSubscription.StartDate.HasValue && updateSubscription.EndDate.HasValue))
                        {
                            var ReadStartEndDateProjectQuery = String.Format("select {0},{1} from ProjectSubscription where {2} = '{3}'",
                                ColumnName.StartDate, ColumnName.EndDate, ColumnName.ProjectSubscriptionUID, updateSubscription.SubscriptionUID);
                            var mySqlReadStartEndDateProjectCommand = new MySqlCommand(ReadStartEndDateProjectQuery, connection);
                            MySqlDataReader mySqlReadStartEndDateProjectDataReader;
                            connection.Open();
                            mySqlReadStartEndDateProjectDataReader = mySqlReadStartEndDateProjectCommand.ExecuteReader();
                            while (mySqlReadStartEndDateProjectDataReader.Read())
                            {
                                projectStartDate = mySqlReadStartEndDateProjectDataReader.GetDateTime(ColumnName.StartDate);
                                projectEndDate = mySqlReadStartEndDateProjectDataReader.GetDateTime(ColumnName.EndDate);
                            }
                            connection.Close();
                        }

                        var newStartDate = updateSubscription.StartDate.HasValue ? updateSubscription.StartDate.Value : projectStartDate;
                        var newEndDate = updateSubscription.EndDate.HasValue ? updateSubscription.EndDate.Value : projectEndDate;

                        // Insert Customer 
                        connection.Open();
                        upsertedCount = connection.Execute(string.Format(InsertCustomerSubscriptionQuery +
                            "values (@CustomerUID, @fk_ServiceTypeID, @StartDate, @EndDate, @InsertUTC, @UpdateUTC);"
                            ),
                            new
                            {
                                CustomerUID = newCustomerUID,
                                fk_ServiceTypeID = newSubscriptionID,
                                StartDate = newStartDate,
                                EndDate = newEndDate,
                                InsertUTC = DateTime.UtcNow,
                                UpdateUTC = DateTime.UtcNow
                            });
                        connection.Close();

                        if (upsertedCount == 0)
                        {
                          Log.Error(String.Format("UpdateProjectSubscription: Failed to insert Customer Subscription with UID {0} for new customer with UID {1}.", newSubscriptionID, newCustomerUID));
                          return upsertedCount;
                        }

                        customerSubscriptionId = GetCustomerSubscriptionID(connection, newCustomerUID, newSubscriptionID);

                        var sb = new StringBuilder();
                        bool commaNeeded = false;
                        commaNeeded |= SqlBuilder.AppendValueParameter(customerSubscriptionId, ColumnName.fk_CustomerSubscriptionID, sb, commaNeeded);
                        commaNeeded |= SqlBuilder.AppendValueParameter(DateTime.UtcNow, ColumnName.UpdateUTC, sb, commaNeeded);

                        var updateCustomerSubscriptionIDQuery = string.Format("Update ProjectSubscription set {0} where {1} = '{2}'",
                             sb, ColumnName.ProjectSubscriptionUID, updateSubscription.SubscriptionUID);
                        connection.Open();
                        upsertedCount = connection.Execute(updateCustomerSubscriptionIDQuery, null, commandType: CommandType.Text);
                        connection.Close();

                        if (upsertedCount == 0)
                        {
                          Log.Error(String.Format("UpdateProjectSubscription: Failed to update Project Subscription with UID {0} for new customer with UID {1}.", updateSubscription.SubscriptionUID, newCustomerUID));
                          return upsertedCount;
                        }
                    }

                    if (customerSubscriptionDataList.Count == 0)
                    {
                        //Delete Old Customer
                        string query = string.Format("Delete from CustomerSubscription where {0} = '{1}' and {2} = {3}", ColumnName.CustomerUID, ids.customerUID, ColumnName.fk_ServiceTypeID, ids.subscriptionID);
                        connection.Open();
                        upsertedCount = connection.Execute(query, commandType: CommandType.Text);
                        connection.Close();

                        if (upsertedCount == 0)
                        {
                          Log.Error(String.Format("UpdateProjectSubscription: Failed to delete Customer Subscription with UID {0} for new customer with UID {1}.", ids.subscriptionID, ids.customerUID));
                          return upsertedCount;
                        }
                    }
                }
                else
                {
                    // Updating CustomerData for Customer
                    var ReadExistingCustomerProjectQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerProjectSubscriptionQuery,
                    ColumnName.CustomerUID, newCustomerUID, ColumnName.fk_ServiceTypeID, newSubscriptionID);
                    connection.Open();
                    var customerSubscriptionDataList =
                                            connection.Query<CustomerProjectSubscriptionData>(ReadExistingCustomerProjectQuery);
                    connection.Close();

                    //Update Customer Subscription
                    upsertedCount = UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, customerSubscriptionDataList);

                    if (upsertedCount == 0)
                    {
                      Log.Error(String.Format("UpdateProjectSubscription: Failed to update Customer Subscription with UID {0} for existing customer with UID {1}.", ids.subscriptionID, ids.customerUID));
                      return upsertedCount;
                    }
                }
            }

          return upsertedCount;
        }

        public int AssociateProjectSubscription(AssociateProjectSubscriptionEvent associateProjectSubscription)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var sb = new StringBuilder();
                bool commaNeeded = false;
                commaNeeded |= SqlBuilder.AppendValueParameter(associateProjectSubscription.ProjectUID, ColumnName.ProjectUID, sb, commaNeeded);
                commaNeeded |= SqlBuilder.AppendValueParameter(associateProjectSubscription.EffectiveDate, ColumnName.StartDate, sb, commaNeeded);
                commaNeeded |= SqlBuilder.AppendValueParameter(DateTime.UtcNow, ColumnName.UpdateUTC, sb, commaNeeded);

                var updateCustomerSubscriptionIDQuery = string.Format("Update ProjectSubscription set {0} where {1} = '{2}'",
                     sb, ColumnName.ProjectSubscriptionUID, associateProjectSubscription.SubscriptionUID);
                connection.Open();
                int upsertedCount = connection.Execute(updateCustomerSubscriptionIDQuery, null, commandType: CommandType.Text);
                connection.Close();

                if (upsertedCount == 0)
                {
                  Log.Error(String.Format("AssociateProjectSubscription: Failed to update Project Subscription with UID {0} for project with UID {1}.", associateProjectSubscription.SubscriptionUID, associateProjectSubscription.ProjectUID));
                  return upsertedCount;
                }

                var readCustomerProjectQuery = String.Format("select customer.{0} as customerUID,customer.{1} as subscriptionID,project.{2} as projectUID from ProjectSubscription project inner join CustomerSubscription customer on project.{3} = customer.{4} where project.{5} = '{6}'",
                        ColumnName.CustomerUID, ColumnName.fk_ServiceTypeID, ColumnName.ProjectUID, ColumnName.fk_CustomerSubscriptionID, ColumnName.CustomerSubscriptionID, ColumnName.ProjectSubscriptionUID, associateProjectSubscription.SubscriptionUID);
                connection.Open();
                ProjectSubscriptionIdentifiers ids = connection.Query<ProjectSubscriptionIdentifiers>(readCustomerProjectQuery).FirstOrDefault();
                connection.Close();

                // Updating CustomerData for Customer
                var ReadExistingCustomerProjectQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerProjectSubscriptionQuery,
                ColumnName.CustomerUID, ids.customerUID, ColumnName.fk_ServiceTypeID, ids.subscriptionID);
                connection.Open();
                var customerSubscriptionDataList =
                                        connection.Query<CustomerProjectSubscriptionData>(ReadExistingCustomerProjectQuery);
                connection.Close();

                //Update Customer Subscription
                upsertedCount = UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, customerSubscriptionDataList);

                if (upsertedCount == 0)
                {
                  Log.Error(String.Format("AssociateProjectSubscription: Failed to update Customer Subscription with UID {0} for existing customer with UID {1}.", ids.subscriptionID, ids.customerUID));
                  return upsertedCount;
                }

                return upsertedCount;
            }
        }

        public int DissociateProjectSubscription(DissociateProjectSubscriptionEvent dissociateProjectSubscription)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                var sb = new StringBuilder();
                bool commaNeeded = false;
                commaNeeded |= SqlBuilder.AppendValueParameter(dissociateProjectSubscription.ProjectUID, ColumnName.ProjectUID, sb, commaNeeded);
                commaNeeded |= SqlBuilder.AppendValueParameter(dissociateProjectSubscription.EffectiveDate, ColumnName.EndDate, sb, commaNeeded);
                commaNeeded |= SqlBuilder.AppendValueParameter(DateTime.UtcNow, ColumnName.UpdateUTC, sb, commaNeeded);

                var updateCustomerSubscriptionIDQuery = string.Format("Update ProjectSubscription set {0} where {1} = '{2}'",
                     sb, ColumnName.ProjectSubscriptionUID, dissociateProjectSubscription.SubscriptionUID);
                connection.Open();
                int upsertedCount = connection.Execute(updateCustomerSubscriptionIDQuery, null, commandType: CommandType.Text);
                connection.Close();

                if (upsertedCount == 0)
                {
                  Log.Error(String.Format("DissociateProjectSubscription: Failed to update Project Subscription with UID {0} for project with UID {1}.", dissociateProjectSubscription.SubscriptionUID, dissociateProjectSubscription.ProjectUID));
                  return upsertedCount;
                }

                var readCustomerProjectQuery = String.Format("select customer.{0} as customerUID,customer.{1} as subscriptionID,project.{2} as projectUID from ProjectSubscription project inner join CustomerSubscription customer on project.{3} = customer.{4} where project.{5} = '{6}'",
                        ColumnName.CustomerUID, ColumnName.fk_ServiceTypeID, ColumnName.ProjectUID, ColumnName.fk_CustomerSubscriptionID, ColumnName.CustomerSubscriptionID, ColumnName.ProjectSubscriptionUID, dissociateProjectSubscription.SubscriptionUID);
                connection.Open();
                ProjectSubscriptionIdentifiers ids = connection.Query<ProjectSubscriptionIdentifiers>(readCustomerProjectQuery).FirstOrDefault();
                connection.Close();

                // Updating CustomerData for Customer
                var ReadExistingCustomerProjectQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerProjectSubscriptionQuery,
                ColumnName.CustomerUID, ids.customerUID, ColumnName.fk_ServiceTypeID, ids.subscriptionID);
                connection.Open();
                var customerSubscriptionDataList =
                                        connection.Query<CustomerProjectSubscriptionData>(ReadExistingCustomerProjectQuery);
                connection.Close();

                //Update Customer Subscription
                upsertedCount = UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, customerSubscriptionDataList);

                if (upsertedCount == 0)
                {
                  Log.Error(String.Format("DissociateProjectSubscription: Failed to update Customer Subscription with UID {0} for existing customer with UID {1}.", ids.subscriptionID, ids.customerUID));
                  return upsertedCount;
                }
                
                return upsertedCount;
            }
        }

        # endregion

        # region Customer Subscription

        public int CreateCustomerSubscription(CreateCustomerSubscriptionEvent subscription)
        {
            if (subscription.SubscriptionType != null && !_customerSubscriptionTypeCache.ContainsKey(subscription.SubscriptionType.ToLowerInvariant()))
                throw new Exception("Invalid Project Subscription Type");

            using (var connection = new MySqlConnection(_connectionString))
            {
                //Insert Customer Subscription
                connection.Open();
                int upsertedCount = connection.Execute(
                string.Format(InsertCustomerSubscriptionQuery +
                        "values (@fk_CustomerUID, @fk_ServiceTypeID, @StartDate, @EndDate, @InsertUTC, @UpdateUTC);"
                ),
                new
                {
                    fk_CustomerUID = subscription.CustomerUID,
                    fk_ServiceTypeID = _customerSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()],
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                    InsertUTC = DateTime.UtcNow,
                    UpdateUTC = DateTime.UtcNow
                });
                connection.Close();

                return upsertedCount;
            }
        }

        public int UpdateCustomerSubscription(UpdateCustomerSubscriptionEvent subscription)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                //Update Customer Subscription
                var sb = new StringBuilder();
                bool commaNeeded = false;
                if (subscription.StartDate.HasValue)
                    commaNeeded |= SqlBuilder.AppendValueParameter(subscription.StartDate.Value, ColumnName.StartDate, sb, commaNeeded);
                if (subscription.EndDate.HasValue)
                    commaNeeded |= SqlBuilder.AppendValueParameter(subscription.EndDate.Value, ColumnName.EndDate, sb, commaNeeded);

                commaNeeded |= SqlBuilder.AppendValueParameter(DateTime.UtcNow, ColumnName.UpdateUTC, sb, commaNeeded);

                var updateCustomerSubscriptionIDQuery = string.Format("Update CustomerSubscription set {0} where {1} = '{2}'",
                     sb, ColumnName.CustomerUID, subscription.SubscriptionUID);
                connection.Open();
                int upsertedCount = connection.Execute(updateCustomerSubscriptionIDQuery, null, commandType: CommandType.Text);
                connection.Close();

                return upsertedCount;
            }
        }

        # endregion

        public static int UpdateSubscriptionDatesForCustomer(MySqlConnection connection, string customerUID, long subscriptionID, IEnumerable<ICustomerSubscriptionData> customerSubscriptionDataList)
        {
            var minCustomerSubscriptionStartDate = DateTime.MaxValue;
            var maxCustomerSubscriptionEndDate = DateTime.MinValue;
            foreach (var customerSubscriptionData in customerSubscriptionDataList)
            {
                if (customerSubscriptionData.StartDate <= DateTime.UtcNow && customerSubscriptionData.EndDate >= DateTime.UtcNow)
                {
                    if (customerSubscriptionData.StartDate < minCustomerSubscriptionStartDate)
                        minCustomerSubscriptionStartDate = customerSubscriptionData.StartDate;
                    if (customerSubscriptionData.EndDate > maxCustomerSubscriptionEndDate)
                        maxCustomerSubscriptionEndDate = customerSubscriptionData.EndDate;
                }
            }
            var sbCustomer = new StringBuilder();
            bool commaNeeded = false;
            commaNeeded |= SqlBuilder.AppendValueParameter(minCustomerSubscriptionStartDate, ColumnName.StartDate, sbCustomer, commaNeeded);
            commaNeeded |= SqlBuilder.AppendValueParameter(maxCustomerSubscriptionEndDate, ColumnName.EndDate, sbCustomer, commaNeeded);
            commaNeeded |= SqlBuilder.AppendValueParameter(DateTime.UtcNow, ColumnName.UpdateUTC, sbCustomer, commaNeeded);

            var updateCustomerSubscriptionQuery = string.Format("Update CustomerSubscription set {0} where {1} = '{2}' and {3} = {4}",
                 sbCustomer, ColumnName.CustomerUID, customerUID, ColumnName.fk_ServiceTypeID, subscriptionID);

            connection.Open();

            int upsertedCount = connection.Execute(updateCustomerSubscriptionQuery, null, commandType: CommandType.Text);

            connection.Close();

            return upsertedCount;
        }

        private static long GetCustomerSubscriptionID(MySqlConnection connection, string customerUID, long subscriptionID)
        {
            var ReadCustomerQuery = String.Format("select {0} from CustomerSubscription where {1} = '{2}' and {3} = {4}",
                ColumnName.CustomerSubscriptionID, ColumnName.CustomerUID, customerUID, ColumnName.fk_ServiceTypeID, subscriptionID);
            var mySqlReadCustomerSubscriptionCommand = new MySqlCommand(ReadCustomerQuery, connection);
            connection.Open();
            var dbObject = mySqlReadCustomerSubscriptionCommand.ExecuteScalar();
            var customerSubscriptionId = dbObject == null ? 0 : (long)dbObject;
            connection.Close();
            return customerSubscriptionId;
        }

        #region Read Operations

      public List<CustomerSubscriptionModel> GetSubscriptionForCustomer(Guid customerGuid)
        {
            var readCustomerQuery = String.Format("select st.{0} as SubscriptionType,customer.{1},customer.{2} from CustomerSubscription customer inner join ServiceType st on customer.fk_ServiceTypeID = st.ServiceTypeID", ColumnName.Name, ColumnName.StartDate, ColumnName.EndDate);
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var customerSubscriptionModelList = connection.Query<CustomerSubscriptionModel>(
                        string.Format("{0} where {1} = '{2}' and customer.startDate <= UTC_TIMESTAMP() and customer.enddate >= UTC_TIMESTAMP()", readCustomerQuery, ColumnName.CustomerUID, customerGuid))
                            .ToList();

                return customerSubscriptionModelList;
            }
        }

        public List<ActiveProjectCustomerSubscriptionModel> GetActiveProjectSubscriptionForCustomer(Guid customerGuid)
        {
            var readCustomerQuery = String.Format("select project.{0} as SubscriptionGuid,st.{1} as SubscriptionType,customer.{2},customer.{3} from CustomerSubscription customer inner join ServiceType st on customer.fk_ServiceTypeID = st.ServiceTypeID inner join ProjectSubscription project on project.fk_CustomerSubscriptionID = customer.CustomerSubscriptionID", ColumnName.ProjectSubscriptionUID, ColumnName.Name, ColumnName.StartDate, ColumnName.EndDate);
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var activeProjectcustomerSubscriptionModelList = connection.Query<ActiveProjectCustomerSubscriptionModel>(
                        string.Format("{0} where customer.{1} = '{2}' and project.{3} = '' and customer.startDate <= UTC_TIMESTAMP() and customer.enddate >= UTC_TIMESTAMP()", readCustomerQuery, ColumnName.CustomerUID, customerGuid,ColumnName.ProjectUID))
                            .ToList();

                return activeProjectcustomerSubscriptionModelList;
            }
        }

        public int GetProjectBySubscription(string projectSubscriptionUid)
        {
          var readCustomerQuery =
            String.Format("select projectId from projects where SubscriptionUid = '{0}'", projectSubscriptionUid);
          using (var connection = new MySqlConnection(_connectionString))
          {
            connection.Open();
            var customerSubscriptionModelList = connection.Query<int>(readCustomerQuery);
            if (customerSubscriptionModelList == null)
              return -1;

            return customerSubscriptionModelList.First();
          }
        }

        #endregion
    }


}
