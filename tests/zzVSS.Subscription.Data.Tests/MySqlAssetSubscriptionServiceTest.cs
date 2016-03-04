using System;
using VSS.Subscription.Data.MySql;
using VSS.Subscription.Data.Models;
using System.Linq;
using Xunit;
using Dapper;
using MDM.Data.Tests.Attributes;
using System.Collections.Generic;
using KellermanSoftware.CompareNetObjects;
using MySql.Data.MySqlClient;
using System.Configuration;
using VSS.Geofence.Data.Tests.Helpers;

namespace VSS.Subscription.Data.Tests
{
    public class MySqlSubscriptionServiceTest
    {
        readonly MySqlSubscriptionService _subscriptionService;

        public MySqlSubscriptionServiceTest()
        {
            _subscriptionService = new MySqlSubscriptionService();
        }

        private CreateAssetSubscriptionEvent GetMeACreateAssetSubscriptionEvent(string subscriptionType)
        {
            var assetSubscription = new CreateAssetSubscriptionEvent()
            {
                ActionUTC = DateTime.UtcNow,
                CustomerUID = Guid.NewGuid(),
                AssetUID = Guid.NewGuid(),
                DeviceUID = Guid.NewGuid(),
                StartDate = DateTime.UtcNow.AddDays(-1).Date,
                EndDate = DateTime.UtcNow.AddDays(1).Date,
                SubscriptionType = subscriptionType,
                SubscriptionUID = Guid.NewGuid(),
                ReceivedUTC = DateTime.UtcNow
            };
            return assetSubscription;
        }

        private UpdateAssetSubscriptionEvent GetMeAnUpdateAssetSubscriptionEvent(Guid subscriptionGuid, Guid? customerGuid = null, Guid? AssetGuid = null, Guid? DeviceGuid = null,
            DateTime? startDate = null, DateTime? endDate = null, string subscriptionType = null)
        {
            var assetSubscription = new UpdateAssetSubscriptionEvent()
            {
                ActionUTC = DateTime.UtcNow,
                CustomerUID = customerGuid,
                AssetUID = AssetGuid,
                DeviceUID = DeviceGuid,
                StartDate = startDate,
                EndDate = endDate,
                SubscriptionType = subscriptionType,
                SubscriptionUID = subscriptionGuid,
                ReceivedUTC = DateTime.UtcNow
            };
            return assetSubscription;
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void AddAssetSubscription_ValidNewCustomerNewAssetSubscription_SucceedsInInsert()
        {
            var assetSubscription = GetMeACreateAssetSubscriptionEvent("Essentials");
            _subscriptionService.CreateAssetSubscription(assetSubscription);
            var assetFetched = getAssetSubscription(assetSubscription.SubscriptionUID);
            Assert.NotNull(assetFetched);
            assetFetched.AssetUID = new Guid(assetFetched.AssetUIDString);
            assetFetched.SubscriptionUID = new Guid(assetFetched.SubscriptionUIDString);
            var assetConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId", "AssetUIDString" } };
            var assetCompareLogic = new CompareLogic(assetConfig);
            ComparisonResult assetResult = assetCompareLogic.Compare(assetSubscription, assetFetched);
            Assert.True(assetResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(assetFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "AssetUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            ComparisonResult customerResult = customerCompareLogic.Compare(assetSubscription, customerFetched);
            Assert.True(assetResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void AddAssetSubscription_ValidExistingCustomerNewAssetSubscriptionWithDateBoundary_SucceedsInInsert()
        {
            //Create a New Customer and New Asset
            var assetSubscription = GetMeACreateAssetSubscriptionEvent("Essentials");
            assetSubscription.StartDate = DateTime.Now.AddDays(-2).Date;
            assetSubscription.EndDate = DateTime.Now.Date;
            _subscriptionService.CreateAssetSubscription(assetSubscription);

            //Create a New Asset with same Customer
            assetSubscription.SubscriptionUID = Guid.NewGuid();
            assetSubscription.AssetUID = Guid.NewGuid();
            assetSubscription.StartDate = DateTime.Now.Date;
            assetSubscription.EndDate = DateTime.Now.AddDays(2).Date;

            _subscriptionService.CreateAssetSubscription(assetSubscription);
            var assetFetched = getAssetSubscription(assetSubscription.SubscriptionUID);
            assetFetched.AssetUID = new Guid(assetFetched.AssetUIDString);
            assetFetched.SubscriptionUID = new Guid(assetFetched.SubscriptionUIDString);
            var assetConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId", "AssetUIDString" } };
            var assetCompareLogic = new CompareLogic(assetConfig);
            ComparisonResult assetResult = assetCompareLogic.Compare(assetSubscription, assetFetched);
            Assert.True(assetResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(assetFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "AssetUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);

            //Mapping to biggest Date Boundary for comparison
            assetSubscription.StartDate = DateTime.Now.AddDays(-2).Date;
            assetSubscription.EndDate = DateTime.Now.AddDays(2).Date;
            ComparisonResult customerResult = customerCompareLogic.Compare(assetSubscription, customerFetched);
            Assert.True(assetResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void AddAssetSubscription_ExistingCustomerNewAssetSubscriptionWithSmallerDateBoundary_SucceedsInInsert()
        {
            //Create a New Customer and Asset
            var assetSubscription = GetMeACreateAssetSubscriptionEvent("Essentials");
            _subscriptionService.CreateAssetSubscription(assetSubscription);

            //Create a New Asset with same Customer
            assetSubscription.AssetUID = Guid.NewGuid();
            assetSubscription.StartDate = DateTime.Now.Date;
            assetSubscription.EndDate = DateTime.Now.Date;

            _subscriptionService.CreateAssetSubscription(assetSubscription);
            var assetFetched = getAssetSubscription(assetSubscription.SubscriptionUID);
            assetFetched.AssetUID = new Guid(assetFetched.AssetUIDString);
            assetFetched.SubscriptionUID = new Guid(assetFetched.SubscriptionUIDString);
            var assetConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId", "AssetUIDString" } };
            var assetCompareLogic = new CompareLogic(assetConfig);
            ComparisonResult assetResult = assetCompareLogic.Compare(assetSubscription, assetFetched);
            Assert.True(assetResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(assetFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "AssetUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            //Mapping to biggest Date Boundary for comparison
            assetSubscription.StartDate = DateTime.Now.AddDays(-1).Date;
            assetSubscription.EndDate = DateTime.Now.AddDays(1).Date;
            ComparisonResult customerResult = customerCompareLogic.Compare(assetSubscription, customerFetched);
            Assert.True(assetResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void AddAssetSubscription_ExistingCustomerNewAssetSubscriptionWithBiggerDateBoundary_SucceedsInInsert()
        {
            //Create a New Customer and Asset
            var assetSubscription = GetMeACreateAssetSubscriptionEvent("Essentials");
            _subscriptionService.CreateAssetSubscription(assetSubscription);

            //Create a New Asset with same Customer
            assetSubscription.AssetUID = Guid.NewGuid();
            assetSubscription.StartDate = DateTime.Now.AddDays(-1).Date;
            assetSubscription.EndDate = DateTime.Now.AddDays(1).Date;

            _subscriptionService.CreateAssetSubscription(assetSubscription);
            var assetFetched = getAssetSubscription(assetSubscription.SubscriptionUID);
            assetFetched.AssetUID = new Guid(assetFetched.AssetUIDString);
            assetFetched.SubscriptionUID = new Guid(assetFetched.SubscriptionUIDString);
            var assetConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId", "AssetUIDString" } };
            var assetCompareLogic = new CompareLogic(assetConfig);
            ComparisonResult assetResult = assetCompareLogic.Compare(assetSubscription, assetFetched);
            Assert.True(assetResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(assetFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "AssetUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            ComparisonResult customerResult = customerCompareLogic.Compare(assetSubscription, customerFetched);
            Assert.True(assetResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void UpdateAssetSubscription_WithChangedDates_SucceedsInUpdate()
        {
            //Create a New Customer and Asset
            var assetSubscription = GetMeACreateAssetSubscriptionEvent("Essentials");
            _subscriptionService.CreateAssetSubscription(assetSubscription);

            //Create a Same Asset with same Customer
            var updateAssetSubscription = GetMeAnUpdateAssetSubscriptionEvent(assetSubscription.SubscriptionUID,
                startDate: DateTime.UtcNow.AddDays(-2).Date, endDate: DateTime.UtcNow.AddDays(2).Date);
            _subscriptionService.UpdateAssetSubscription(updateAssetSubscription);
            var assetFetched = getAssetSubscription(assetSubscription.SubscriptionUID);
            Assert.NotNull(assetFetched);
            assetFetched.AssetUID = new Guid(assetFetched.AssetUIDString);
            assetFetched.SubscriptionUID = new Guid(assetFetched.SubscriptionUIDString);

            var assetConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId", "AssetUIDString" } };
            var assetCompareLogic = new CompareLogic(assetConfig);
            //Mapping to biggest Date Boundary for comparison
            assetSubscription.StartDate = updateAssetSubscription.StartDate.Value;
            assetSubscription.EndDate = updateAssetSubscription.EndDate.Value;
            ComparisonResult assetResult = assetCompareLogic.Compare(assetSubscription, assetFetched);
            Assert.True(assetResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(assetFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "AssetUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            ComparisonResult customerResult = customerCompareLogic.Compare(assetSubscription, customerFetched);
            Assert.True(assetResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void UpdateAssetSubscription_WithChangedDatesAndAsset_SucceedsInUpdate()
        {
            //Create a New Customer and Asset
            var assetSubscription = GetMeACreateAssetSubscriptionEvent("Essentials");
            _subscriptionService.CreateAssetSubscription(assetSubscription);

            //Create a Same Asset with same Customer
            var updateAssetSubscription = GetMeAnUpdateAssetSubscriptionEvent(assetSubscription.SubscriptionUID, AssetGuid: Guid.NewGuid(),
                startDate: DateTime.UtcNow.AddDays(-2).Date, endDate: DateTime.UtcNow.AddDays(2).Date);
            _subscriptionService.UpdateAssetSubscription(updateAssetSubscription);
            var assetFetched = getAssetSubscription(assetSubscription.SubscriptionUID);
            Assert.NotNull(assetFetched);
            assetFetched.AssetUID = new Guid(assetFetched.AssetUIDString);
            assetFetched.SubscriptionUID = new Guid(assetFetched.SubscriptionUIDString);

            var assetConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId", "AssetUIDString" } };
            var assetCompareLogic = new CompareLogic(assetConfig);
            //Mapping to biggest Date Boundary for comparison
            assetSubscription.AssetUID = updateAssetSubscription.AssetUID.Value;
            assetSubscription.StartDate = updateAssetSubscription.StartDate.Value;
            assetSubscription.EndDate = updateAssetSubscription.EndDate.Value;
            ComparisonResult assetResult = assetCompareLogic.Compare(assetSubscription, assetFetched);
            Assert.True(assetResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(assetFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "AssetUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            ComparisonResult customerResult = customerCompareLogic.Compare(assetSubscription, customerFetched);
            Assert.True(assetResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void UpdateAssetSubscription_WithChangedDates_Asset_Customer_SucceedsInUpdate()
        {
            //Create a New Customer and Asset
            var assetSubscription = GetMeACreateAssetSubscriptionEvent("Essentials");
            _subscriptionService.CreateAssetSubscription(assetSubscription);
            var oldAssetFetched = getAssetSubscription(assetSubscription.SubscriptionUID);

            //Create a New Asset with New Customer
            var updateAssetSubscription = GetMeAnUpdateAssetSubscriptionEvent(assetSubscription.SubscriptionUID, AssetGuid: Guid.NewGuid(), customerGuid: Guid.NewGuid(),
                startDate: DateTime.UtcNow.AddDays(-2).Date, endDate: DateTime.UtcNow.AddDays(2).Date);
            _subscriptionService.UpdateAssetSubscription(updateAssetSubscription);
            var assetFetched = getAssetSubscription(assetSubscription.SubscriptionUID);
            Assert.NotNull(assetFetched);
            assetFetched.AssetUID = new Guid(assetFetched.AssetUIDString);
            assetFetched.SubscriptionUID = new Guid(assetFetched.SubscriptionUIDString);

            var assetConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId", "AssetUIDString" } };
            var assetCompareLogic = new CompareLogic(assetConfig);
            //Mapping to biggest Date Boundary for comparison
            assetSubscription.AssetUID = updateAssetSubscription.AssetUID.Value;
            assetSubscription.StartDate = updateAssetSubscription.StartDate.Value;
            assetSubscription.EndDate = updateAssetSubscription.EndDate.Value;
            ComparisonResult assetResult = assetCompareLogic.Compare(assetSubscription, assetFetched);
            Assert.True(assetResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(assetFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "AssetUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            assetSubscription.CustomerUID = updateAssetSubscription.CustomerUID.Value;
            ComparisonResult customerResult = customerCompareLogic.Compare(assetSubscription, customerFetched);
            Assert.True(assetResult.Differences.Count == 0);
            var OldCustomerFetched = getCustomerSubscription(oldAssetFetched.CustomerSubscriptionID);
            Assert.Null(OldCustomerFetched);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void UpdateAssetSubscription_WithChangedDates_MultipleAsset_Customer_SucceedsInUpdate()
        {
            //Create a New Asset with same Customer
            var firstAssetSubscription = GetMeACreateAssetSubscriptionEvent("Essentials");
            firstAssetSubscription.StartDate = DateTime.UtcNow.AddDays(-3).Date;
            firstAssetSubscription.EndDate = DateTime.UtcNow.AddDays(3).Date;
            _subscriptionService.CreateAssetSubscription(firstAssetSubscription);

            //Create a New Customer and Asset
            var assetSubscription = GetMeACreateAssetSubscriptionEvent("Essentials");
            assetSubscription.CustomerUID = firstAssetSubscription.CustomerUID;
            _subscriptionService.CreateAssetSubscription(assetSubscription);
            var oldAssetFetched = getAssetSubscription(assetSubscription.SubscriptionUID);

            //Create a New Asset with New Customer
            var updateAssetSubscription = GetMeAnUpdateAssetSubscriptionEvent(assetSubscription.SubscriptionUID, AssetGuid: Guid.NewGuid(), customerGuid: new Guid("71d38732-8440-4c0e-a425-c42eef460e91"),
                startDate: DateTime.UtcNow.AddDays(-2).Date, endDate: DateTime.UtcNow.AddDays(2).Date);
            _subscriptionService.UpdateAssetSubscription(updateAssetSubscription);
            var assetFetched = getAssetSubscription(assetSubscription.SubscriptionUID);
            Assert.NotNull(assetFetched);
            assetFetched.AssetUID = new Guid(assetFetched.AssetUIDString);
            assetFetched.SubscriptionUID = new Guid(assetFetched.SubscriptionUIDString);

            var assetConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId", "AssetUIDString" } };
            var assetCompareLogic = new CompareLogic(assetConfig);
            //Mapping to biggest Date Boundary for comparison
            assetSubscription.AssetUID = updateAssetSubscription.AssetUID.Value;
            assetSubscription.StartDate = updateAssetSubscription.StartDate.Value;
            assetSubscription.EndDate = updateAssetSubscription.EndDate.Value;
            ComparisonResult assetResult = assetCompareLogic.Compare(assetSubscription, assetFetched);
            Assert.True(assetResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(assetFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "AssetUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            assetSubscription.CustomerUID = updateAssetSubscription.CustomerUID.Value;
            assetSubscription.StartDate = firstAssetSubscription.StartDate;
            assetSubscription.EndDate = firstAssetSubscription.EndDate;
            ComparisonResult customerResult = customerCompareLogic.Compare(assetSubscription, customerFetched);
            Assert.True(assetResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void GetCustomerSubscription_WithChangedDates_MultipleAsset_Customer_SucceedsInRead()
        {
            //Create a New Asset with same Customer
            var firstAssetSubscription = GetMeACreateAssetSubscriptionEvent("Essentials");
            firstAssetSubscription.StartDate = DateTime.UtcNow.AddDays(-3).Date;
            firstAssetSubscription.EndDate = DateTime.UtcNow.AddDays(3).Date;
            _subscriptionService.CreateAssetSubscription(firstAssetSubscription);

            //Create a New Customer and Asset
            var assetSubscription = GetMeACreateAssetSubscriptionEvent("Essentials");
            assetSubscription.CustomerUID = firstAssetSubscription.CustomerUID;
            _subscriptionService.CreateAssetSubscription(assetSubscription);

            var customerSubscriptionModelList = _subscriptionService.GetSubscriptionForCustomer(firstAssetSubscription.CustomerUID);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "AssetUID", "DeviceUID", "SubscriptionUID", "CustomerUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            ComparisonResult customerResult = customerCompareLogic.Compare(firstAssetSubscription, customerSubscriptionModelList[0]);
            Assert.True(customerResult.Differences.Count == 0);
        }

        private static AssetSubscriptionData getAssetSubscription(Guid subscriptionUID)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MySql.Subscription"].ConnectionString;
            var ReadAssetQuery = String.Format("select AssetSubscriptionUID as SubscriptionUIDString , fk_AssetUID as AssetUIDString, fk_CustomerSubscriptionID as CustomerSubscriptionID , StartDate , EndDate from AssetSubscription where AssetSubscriptionUID = '{0}'", subscriptionUID.ToString());
            AssetSubscriptionData assetSubscriptionData;
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                assetSubscriptionData = connection.Query<AssetSubscriptionData>(ReadAssetQuery).FirstOrDefault();
            }
            return assetSubscriptionData;
        }

        private static CustomerSubscriptionData getCustomerSubscription(long customerSubscritionId)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MySql.Subscription"].ConnectionString; ;
            var ReadAssetQuery = String.Format("select customer.CustomerSubscriptionID, customer.fk_CustomerUID as CustomerUIDString , serviceType.Name as ServiceType , customer.StartDate , customer.EndDate from CustomerSubscription customer inner join ServiceType serviceType on customer.fk_ServiceTypeID = serviceType.ServiceTypeID where customer.CustomerSubscriptionID = '{0}'", customerSubscritionId);
            CustomerSubscriptionData customerSubscriptionData;
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                customerSubscriptionData = connection.Query<CustomerSubscriptionData>(ReadAssetQuery).FirstOrDefault();
            }
            return customerSubscriptionData;
        }
    }

    public class AssetSubscriptionData
    {

        public string SubscriptionUIDString { get; set; }

        public Guid SubscriptionUID { get; set; }

        public string AssetUIDString { get; set; }

        public Guid AssetUID { get; set; }

        public long CustomerSubscriptionID { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }

    public class CustomerSubscriptionData
    {
        public Guid CustomerUID { get; set; }

        public string CustomerUIDString { get; set; }

        public string ServiceType { get; set; }

        public long CustomerSubscriptionID { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
