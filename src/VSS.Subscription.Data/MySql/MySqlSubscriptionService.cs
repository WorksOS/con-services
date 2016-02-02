using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using Dapper;
using MDM.Data.Helpers;
using MySql.Data.MySqlClient;
using VSS.Subscription.Data.Models;
using VSS.Subscription.Data.Helpers;
using VSS.Subscription.Model.Interfaces;

namespace VSS.Subscription.Data.MySql
{
    public class MySqlSubscriptionService : ISubscriptionService
    {
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

        public MySqlSubscriptionService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MySql.Subscription"].ConnectionString;
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

        public void CreateAssetSubscription(CreateAssetSubscriptionEvent subscription)
        {
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
                    connection.Execute(
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

                    UpdateSubscriptionDatesForCustomer(connection, subscription.CustomerUID.ToString(), _assetSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()], customerSubscriptionDataList);
                }

                //Getting Customer SubscriptionId
                long customerSubscriptionId = GetCustomerSubscriptionID(connection, subscription.CustomerUID.ToString(), _assetSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()]);

                //Insert Asset Subscription
                connection.Open();
                connection.Execute(
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
        }

        public void UpdateAssetSubscription(UpdateAssetSubscriptionEvent updateSubscription)
        {
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
                newCustomerUID = updateSubscription.CustomerUID.HasValue ? updateSubscription.CustomerUID.Value.ToString() : ids.customerUID;
                newSubscriptionID = string.IsNullOrWhiteSpace(updateSubscription.SubscriptionType) ? ids.subscriptionID : _assetSubscriptionTypeCache[updateSubscription.SubscriptionType.ToLowerInvariant()];

                //Update Asset Subscription with Rest Of fields
                if (updateSubscription.AssetUID.HasValue || updateSubscription.DeviceUID.HasValue || updateSubscription.StartDate.HasValue || updateSubscription.EndDate.HasValue)
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
                    var rowsAffected = connection.Execute(updateAssetSubscriptionQuery, null, commandType: CommandType.Text);
                    connection.Close();
                }

                if (updateSubscription.CustomerUID.HasValue || !string.IsNullOrWhiteSpace(updateSubscription.SubscriptionType))
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
                        UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, customerSubscriptionDataList);
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
                        connection.Execute(updateCustomerSubscriptionIDQuery, null, commandType: CommandType.Text);
                        connection.Close();

                        /// Updating CustomerData for new Customer
                        var ReadNewCustomerAssetQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerAssetSubscriptionQuery,
                        ColumnName.CustomerUID, newCustomerUID, ColumnName.fk_ServiceTypeID, newSubscriptionID);
                        connection.Open();

                        var newCustomerSubscriptionDataList =
                            connection.Query<CustomerAssetSubscriptionData>(ReadNewCustomerAssetQuery).ToList();

                        connection.Close();

                        //Update Customer Subscription
                        UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, newCustomerSubscriptionDataList);

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
                        connection.Execute(string.Format(InsertCustomerSubscriptionQuery +
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

                        customerSubscriptionId = GetCustomerSubscriptionID(connection, newCustomerUID, newSubscriptionID);

                        var sb = new StringBuilder();
                        bool commaNeeded = false;
                        commaNeeded |= SqlBuilder.AppendValueParameter(customerSubscriptionId, ColumnName.fk_CustomerSubscriptionID, sb, commaNeeded);
                        commaNeeded |= SqlBuilder.AppendValueParameter(DateTime.UtcNow, ColumnName.UpdateUTC, sb, commaNeeded);

                        var updateCustomerSubscriptionIDQuery = string.Format("Update AssetSubscription set {0} where {1} = '{2}'",
                             sb, ColumnName.AssetSubscriptionUID, updateSubscription.SubscriptionUID);
                        connection.Open();
                        connection.Execute(updateCustomerSubscriptionIDQuery, null, commandType: CommandType.Text);
                        connection.Close();
                    }

                    if (customerSubscriptionDataList.Count == 0)
                    {
                        //Delete Old Customer
                        string query = string.Format("Delete from CustomerSubscription where {0} = '{1}' and {2} = {3}", ColumnName.CustomerUID, ids.customerUID, ColumnName.fk_ServiceTypeID, ids.subscriptionID);
                        connection.Open();
                        connection.Execute(query, commandType: CommandType.Text);
                        connection.Close();

                    }
                }
                else
                {
                    /// Updating CustomerData for Customer
                    var ReadExistingCustomerAssetQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerAssetSubscriptionQuery,
                    ColumnName.CustomerUID, newCustomerUID, ColumnName.fk_ServiceTypeID, newSubscriptionID);
                    connection.Open();
                    var customerSubscriptionDataList =
                                            connection.Query<CustomerAssetSubscriptionData>(ReadExistingCustomerAssetQuery)
                                                .ToList();
                    connection.Close();

                    //Update Customer Subscription
                    UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, customerSubscriptionDataList);
                }
            }
        }

        # endregion

        # region Project Subscription

        private class ProjectSubscriptionIdentifiers
        {
            public string customerUID { get; set; }
            public string projectUID { get; set; }
            public long subscriptionID { get; set; }
        }

        public void CreateProjectSubscription(CreateProjectSubscriptionEvent subscription)
        {
            if (subscription.SubscriptionType != null && !_projectSubscriptionTypeCache.ContainsKey(subscription.SubscriptionType.ToLowerInvariant()))
                throw new Exception("Invalid Project Subscription Type");

            using (var connection = new MySqlConnection(_connectionString))
            {
                var readQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerProjectSubscriptionQuery,
                        ColumnName.CustomerUID, subscription.CustomerUID, ColumnName.fk_ServiceTypeID, _projectSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()]);

                connection.Open();

                var customerSubscriptionDataList = connection.Query<CustomerProjectSubscriptionData>(readQuery).ToList();
                connection.Close();


                if (customerSubscriptionDataList.Count == 0)
                {
                    //Insert Customer Subscription
                    connection.Open();
                    connection.Execute(
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
                }
                else
                {
                    ////Update Customer Subscription

                    //Adding the incoming Project Subscription UID into List to calculate correct min/max Dates
                    var incomingSubscriptionData = new CustomerProjectSubscriptionData();
                    incomingSubscriptionData.StartDate = subscription.StartDate;
                    incomingSubscriptionData.EndDate = subscription.EndDate;
                    customerSubscriptionDataList.Add(incomingSubscriptionData);

                    UpdateSubscriptionDatesForCustomer(connection, subscription.CustomerUID.ToString(), _projectSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()], customerSubscriptionDataList);
                }

                //Getting Customer SubscriptionId
                long customerSubscriptionId = GetCustomerSubscriptionID(connection, subscription.CustomerUID.ToString(), _projectSubscriptionTypeCache[subscription.SubscriptionType.ToLowerInvariant()]);

                //Insert Project Subscription
                connection.Open();
                connection.Execute(
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
        }

        public void UpdateProjectSubscription(UpdateProjectSubscriptionEvent updateSubscription)
        {
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
                    var rowsAffected = connection.Execute(updateProjectSubscriptionQuery, null, commandType: CommandType.Text);
                    connection.Close();
                }

                if (updateSubscription.CustomerUID.HasValue || !string.IsNullOrWhiteSpace(updateSubscription.SubscriptionType))
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
                        UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, customerSubscriptionDataList);
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
                        connection.Execute(updateCustomerSubscriptionIDQuery, null, commandType: CommandType.Text);
                        connection.Close();

                        /// Updating CustomerData for new Customer
                        var ReadNewCustomerProjectQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerProjectSubscriptionQuery,
                        ColumnName.CustomerUID, newCustomerUID, ColumnName.fk_ServiceTypeID, newSubscriptionID);
                        connection.Open();

                        var newCustomerSubscriptionDataList =
                            connection.Query<CustomerProjectSubscriptionData>(ReadNewCustomerProjectQuery).ToList();

                        connection.Close();

                        //Update Customer Subscription
                        UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, newCustomerSubscriptionDataList);

                    }
                    else
                    {
                        //Query Start & End Date for Project
                        DateTime projectStartDate = new DateTime();
                        DateTime projectEndDate = new DateTime();
                        if (!(updateSubscription.StartDate.HasValue && updateSubscription.EndDate.HasValue))
                        {
                            var ReadStartEndDateProjectQuery = String.Format("select {0},{1} from ProjectSubscription where {2} = {3}",
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
                        connection.Execute(string.Format(InsertCustomerSubscriptionQuery +
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

                        customerSubscriptionId = GetCustomerSubscriptionID(connection, newCustomerUID, newSubscriptionID);

                        var sb = new StringBuilder();
                        bool commaNeeded = false;
                        commaNeeded |= SqlBuilder.AppendValueParameter(customerSubscriptionId, ColumnName.fk_CustomerSubscriptionID, sb, commaNeeded);
                        commaNeeded |= SqlBuilder.AppendValueParameter(DateTime.UtcNow, ColumnName.UpdateUTC, sb, commaNeeded);

                        var updateCustomerSubscriptionIDQuery = string.Format("Update ProjectSubscription set {0} where {1} = '{2}'",
                             sb, ColumnName.ProjectSubscriptionUID, updateSubscription.SubscriptionUID);
                        connection.Open();
                        connection.Execute(updateCustomerSubscriptionIDQuery, null, commandType: CommandType.Text);
                        connection.Close();
                    }

                    if (customerSubscriptionDataList.Count == 0)
                    {
                        //Delete Old Customer
                        string query = string.Format("Delete from CustomerSubscription where {0} = '{1}' and {2} = {3}", ColumnName.CustomerUID, ids.customerUID, ColumnName.fk_ServiceTypeID, ids.subscriptionID);
                        connection.Open();
                        connection.Execute(query, commandType: CommandType.Text);
                        connection.Close();

                    }
                }
                else
                {
                    /// Updating CustomerData for Customer
                    var ReadExistingCustomerProjectQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerProjectSubscriptionQuery,
                    ColumnName.CustomerUID, newCustomerUID, ColumnName.fk_ServiceTypeID, newSubscriptionID);
                    connection.Open();
                    var customerSubscriptionDataList =
                                            connection.Query<CustomerProjectSubscriptionData>(ReadExistingCustomerProjectQuery);
                    connection.Close();

                    //Update Customer Subscription
                    UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, customerSubscriptionDataList);
                }
            }
        }

        public void AssociateProjectSubscription(AssociateProjectSubscriptionEvent associateProjectSubscription)
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
                connection.Execute(updateCustomerSubscriptionIDQuery, null, commandType: CommandType.Text);
                connection.Close();

                var readCustomerProjectQuery = String.Format("select customer.{0} as customerUID,customer.{1} as subscriptionID,project.{2} as projectUID from ProjectSubscription project inner join CustomerSubscription customer on project.{3} = customer.{4} where project.{5} = '{6}'",
                        ColumnName.CustomerUID, ColumnName.fk_ServiceTypeID, ColumnName.ProjectUID, ColumnName.fk_CustomerSubscriptionID, ColumnName.CustomerSubscriptionID, ColumnName.ProjectSubscriptionUID, associateProjectSubscription.SubscriptionUID);
                connection.Open();
                ProjectSubscriptionIdentifiers ids = connection.Query<ProjectSubscriptionIdentifiers>(readCustomerProjectQuery).FirstOrDefault();
                connection.Close();

                /// Updating CustomerData for Customer
                var ReadExistingCustomerProjectQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerProjectSubscriptionQuery,
                ColumnName.CustomerUID, ids.customerUID, ColumnName.fk_ServiceTypeID, ids.subscriptionID);
                connection.Open();
                var customerSubscriptionDataList =
                                        connection.Query<CustomerProjectSubscriptionData>(ReadExistingCustomerProjectQuery);
                connection.Close();

                //Update Customer Subscription
                UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, customerSubscriptionDataList);
            }
        }

        public void DissociateProjectSubscription(DissociateProjectSubscriptionEvent dissociateProjectSubscription)
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
                connection.Execute(updateCustomerSubscriptionIDQuery, null, commandType: CommandType.Text);
                connection.Close();

                var readCustomerProjectQuery = String.Format("select customer.{0} as customerUID,customer.{1} as subscriptionID,project.{2} as projectUID from ProjectSubscription project inner join CustomerSubscription customer on project.{3} = customer.{4} where project.{5} = '{6}'",
                        ColumnName.CustomerUID, ColumnName.fk_ServiceTypeID, ColumnName.ProjectUID, ColumnName.fk_CustomerSubscriptionID, ColumnName.CustomerSubscriptionID, ColumnName.ProjectSubscriptionUID, dissociateProjectSubscription.SubscriptionUID);
                connection.Open();
                ProjectSubscriptionIdentifiers ids = connection.Query<ProjectSubscriptionIdentifiers>(readCustomerProjectQuery).FirstOrDefault();
                connection.Close();

                /// Updating CustomerData for Customer
                var ReadExistingCustomerProjectQuery = String.Format("{0} where customer.{1} = '{2}' and customer.{3} = {4}", ReadAllCustomerProjectSubscriptionQuery,
                ColumnName.CustomerUID, ids.customerUID, ColumnName.fk_ServiceTypeID, ids.subscriptionID);
                connection.Open();
                var customerSubscriptionDataList =
                                        connection.Query<CustomerProjectSubscriptionData>(ReadExistingCustomerProjectQuery);
                connection.Close();

                //Update Customer Subscription
                UpdateSubscriptionDatesForCustomer(connection, ids.customerUID, ids.subscriptionID, customerSubscriptionDataList);
            }
        }

        # endregion

        # region Customer Subscription

        public void CreateCustomerSubscription(CreateCustomerSubscriptionEvent subscription)
        {
            if (subscription.SubscriptionType != null && !_customerSubscriptionTypeCache.ContainsKey(subscription.SubscriptionType.ToLowerInvariant()))
                throw new Exception("Invalid Project Subscription Type");

            using (var connection = new MySqlConnection(_connectionString))
            {
                //Insert Customer Subscription
                connection.Open();
                connection.Execute(
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
            }
        }

        public void UpdateCustomerSubscription(UpdateCustomerSubscriptionEvent subscription)
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
                     sb, ColumnName.CustomerSubscriptionID, subscription.SubscriptionUID);
                connection.Open();
                connection.Execute(updateCustomerSubscriptionIDQuery, null, commandType: CommandType.Text);
                connection.Close();
            }
        }

        # endregion

        private static void UpdateSubscriptionDatesForCustomer(MySqlConnection connection, string customerUID, long subscriptionID, IEnumerable<ICustomerSubscriptionData> customerSubscriptionDataList)
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
            connection.Execute(updateCustomerSubscriptionQuery, null, commandType: CommandType.Text);
            connection.Close();
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

        public List<CustomerSubscriptionModel> GetActiveProjectSubscriptionForCustomer(Guid customerGuid)
        {
          var readCustomerQuery = String.Format("select project.{0} as ProjectSubscriptionUID, st.{1} as SubscriptionType,customer.{2},customer.{3} from CustomerSubscription customer inner join ServiceType st on customer.fk_ServiceTypeID = st.ServiceTypeID inner join ProjectSubscription project on project.fk_CustomerSubscriptionID = customer.CustomerSubscriptionID", ColumnName.ProjectSubscriptionUID, ColumnName.Name, ColumnName.StartDate, ColumnName.EndDate);
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var customerSubscriptionModelList = connection.Query<CustomerSubscriptionModel>(
                        string.Format("{0} where customer.{1} = '{2}' and project.{3} is null and customer.startDate <= UTC_TIMESTAMP() and customer.enddate >= UTC_TIMESTAMP()", readCustomerQuery, ColumnName.CustomerUID, customerGuid,ColumnName.ProjectUID))
                            .ToList();

                return customerSubscriptionModelList;
            }
        }

      public int GetProjectBySubscripion(string projectSubscriptionUid)
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
