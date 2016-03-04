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
    public class MySqlProjectSubscriptionServiceTest
    {
        readonly MySqlSubscriptionService _subscriptionService;

        public MySqlProjectSubscriptionServiceTest()
        {
            _subscriptionService = new MySqlSubscriptionService();
        }

        private CreateProjectSubscriptionEvent GetMeACreateProjectSubscriptionEvent(string subscriptionType)
        {
            var projectSubscription = new CreateProjectSubscriptionEvent()
            {
                ActionUTC = DateTime.UtcNow,
                CustomerUID = Guid.NewGuid(),
                StartDate = DateTime.UtcNow.AddDays(-1).Date,
                EndDate = DateTime.UtcNow.AddDays(1).Date,
                SubscriptionType = subscriptionType,
                SubscriptionUID = Guid.NewGuid(),
                ReceivedUTC = DateTime.UtcNow
            };
            return projectSubscription;
        }

        private UpdateProjectSubscriptionEvent GetMeAnUpdateProjectSubscriptionEvent(Guid subscriptionGuid, Guid? customerGuid = null,
            DateTime? startDate = null, DateTime? endDate = null, string subscriptionType = null)
        {
            var projectSubscription = new UpdateProjectSubscriptionEvent()
            {
                ActionUTC = DateTime.UtcNow,
                CustomerUID = customerGuid,
                StartDate = startDate,
                EndDate = endDate,
                SubscriptionType = subscriptionType,
                SubscriptionUID = subscriptionGuid,
                ReceivedUTC = DateTime.UtcNow
            };
            return projectSubscription;
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void AddProjectSubscription_ValidNewCustomerNewProjectSubscription_SucceedsInInsert()
        {
            var projectSubscription = GetMeACreateProjectSubscriptionEvent("Project Monitoring");
            _subscriptionService.CreateProjectSubscription(projectSubscription);
            var projectFetched = getProjectSubscription(projectSubscription.SubscriptionUID);
            Assert.NotNull(projectFetched);
            projectFetched.SubscriptionUID = new Guid(projectFetched.SubscriptionUIDString);
            var projectConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId", "ProjectUIDString" } };
            var projectCompareLogic = new CompareLogic(projectConfig);
            ComparisonResult projectResult = projectCompareLogic.Compare(projectSubscription, projectFetched);
            Assert.True(projectResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(projectFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "ProjectUID","SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            ComparisonResult customerResult = customerCompareLogic.Compare(projectSubscription, customerFetched);
            Assert.True(projectResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void AddProjectSubscription_ValidExistingCustomerNewProjectSubscriptionWithDateBoundary_SucceedsInInsert()
        {
            //Create a New Customer and New Project
            var projectSubscription = GetMeACreateProjectSubscriptionEvent("Project Monitoring");
            projectSubscription.StartDate = DateTime.Now.AddDays(-2).Date;
            projectSubscription.EndDate = DateTime.Now.Date;
            _subscriptionService.CreateProjectSubscription(projectSubscription);

            //Create a New Project with same Customer
            projectSubscription.SubscriptionUID = Guid.NewGuid();
            projectSubscription.SubscriptionType = "Landfill";
            projectSubscription.StartDate = DateTime.Now.Date;
            projectSubscription.EndDate = DateTime.Now.AddDays(2).Date;

            _subscriptionService.CreateProjectSubscription(projectSubscription);
            var projectFetched = getProjectSubscription(projectSubscription.SubscriptionUID);
            projectFetched.SubscriptionUID = new Guid(projectFetched.SubscriptionUIDString);
            var projectConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId" } };
            var projectCompareLogic = new CompareLogic(projectConfig);
            ComparisonResult projectResult = projectCompareLogic.Compare(projectSubscription, projectFetched);
            Assert.True(projectResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(projectFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "ProjectUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);

            //Mapping to biggest Date Boundary for comparison
            projectSubscription.StartDate = DateTime.Now.AddDays(-2).Date;
            projectSubscription.EndDate = DateTime.Now.AddDays(2).Date;
            ComparisonResult customerResult = customerCompareLogic.Compare(projectSubscription, customerFetched);
            Assert.True(projectResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void AddProjectSubscription_ExistingCustomerNewProjectSubscriptionWithSmallerDateBoundary_SucceedsInInsert()
        {
            //Create a New Customer and Project
            var projectSubscription = GetMeACreateProjectSubscriptionEvent("Landfill");
            _subscriptionService.CreateProjectSubscription(projectSubscription);

            //Create a New Project with same Customer
            projectSubscription.SubscriptionUID = Guid.NewGuid();
            projectSubscription.SubscriptionType = "Project Monitoring";
            projectSubscription.StartDate = DateTime.Now.Date;
            projectSubscription.EndDate = DateTime.Now.Date;

            _subscriptionService.CreateProjectSubscription(projectSubscription);
            var projectFetched = getProjectSubscription(projectSubscription.SubscriptionUID);
            projectFetched.SubscriptionUID = new Guid(projectFetched.SubscriptionUIDString);
            var projectConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId" } };
            var projectCompareLogic = new CompareLogic(projectConfig);
            ComparisonResult projectResult = projectCompareLogic.Compare(projectSubscription, projectFetched);
            Assert.True(projectResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(projectFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "ProjectUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            //Mapping to biggest Date Boundary for comparison
            projectSubscription.StartDate = DateTime.Now.AddDays(-1).Date;
            projectSubscription.EndDate = DateTime.Now.AddDays(1).Date;
            ComparisonResult customerResult = customerCompareLogic.Compare(projectSubscription, customerFetched);
            Assert.True(projectResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void AddProjectSubscription_ExistingCustomerNewProjectSubscriptionWithBiggerDateBoundary_SucceedsInInsert()
        {
            //Create a New Customer and Project
            var projectSubscription = GetMeACreateProjectSubscriptionEvent("Landfill");
            _subscriptionService.CreateProjectSubscription(projectSubscription);

            //Create a New Project with same Customer
            projectSubscription.SubscriptionUID = Guid.NewGuid();
            projectSubscription.StartDate = DateTime.Now.AddDays(-1).Date;
            projectSubscription.EndDate = DateTime.Now.AddDays(1).Date;

            _subscriptionService.CreateProjectSubscription(projectSubscription);
            var projectFetched = getProjectSubscription(projectSubscription.SubscriptionUID);
            projectFetched.SubscriptionUID = new Guid(projectFetched.SubscriptionUIDString);
            var projectConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId" } };
            var projectCompareLogic = new CompareLogic(projectConfig);
            ComparisonResult projectResult = projectCompareLogic.Compare(projectSubscription, projectFetched);
            Assert.True(projectResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(projectFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "ProjectUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            ComparisonResult customerResult = customerCompareLogic.Compare(projectSubscription, customerFetched);
            Assert.True(projectResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void UpdateProjectSubscription_WithChangedDates_SucceedsInUpdate()
        {
            //Create a New Customer and Project
            var projectSubscription = GetMeACreateProjectSubscriptionEvent("Landfill");
            _subscriptionService.CreateProjectSubscription(projectSubscription);

            //Create a Same Project with same Customer
            var updateProjectSubscription = GetMeAnUpdateProjectSubscriptionEvent(projectSubscription.SubscriptionUID,
                startDate: DateTime.UtcNow.AddDays(-2).Date, endDate: DateTime.UtcNow.AddDays(2).Date);
            _subscriptionService.UpdateProjectSubscription(updateProjectSubscription);
            var projectFetched = getProjectSubscription(projectSubscription.SubscriptionUID);
            Assert.NotNull(projectFetched);
            projectFetched.SubscriptionUID = new Guid(projectFetched.SubscriptionUIDString);

            var projectConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId" } };
            var projectCompareLogic = new CompareLogic(projectConfig);
            //Mapping to biggest Date Boundary for comparison
            projectSubscription.StartDate = updateProjectSubscription.StartDate.Value;
            projectSubscription.EndDate = updateProjectSubscription.EndDate.Value;
            ComparisonResult projectResult = projectCompareLogic.Compare(projectSubscription, projectFetched);
            Assert.True(projectResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(projectFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "ProjectUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            ComparisonResult customerResult = customerCompareLogic.Compare(projectSubscription, customerFetched);
            Assert.True(projectResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void UpdateprojectSubscription_WithChangedDatesAndProject_SucceedsInUpdate()
        {
            //Create a New Customer and Project
            var projectSubscription = GetMeACreateProjectSubscriptionEvent("Landfill");
            _subscriptionService.CreateProjectSubscription(projectSubscription);

            //Create a Same Project with same Customer
            var updateProjectSubscription = GetMeAnUpdateProjectSubscriptionEvent(projectSubscription.SubscriptionUID,
                startDate: DateTime.UtcNow.AddDays(-2).Date, endDate: DateTime.UtcNow.AddDays(2).Date);
            _subscriptionService.UpdateProjectSubscription(updateProjectSubscription);
            var projectFetched = getProjectSubscription(projectSubscription.SubscriptionUID);
            Assert.NotNull(projectFetched);
            projectFetched.SubscriptionUID = new Guid(projectFetched.SubscriptionUIDString);

            var projectConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId", "ProjectUIDString" } };
            var projectCompareLogic = new CompareLogic(projectConfig);
            //Mapping to biggest Date Boundary for comparison
            projectSubscription.StartDate = updateProjectSubscription.StartDate.Value;
            projectSubscription.EndDate = updateProjectSubscription.EndDate.Value;
            ComparisonResult projectResult = projectCompareLogic.Compare(projectSubscription, projectFetched);
            Assert.True(projectResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(projectFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "ProjectUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            ComparisonResult customerResult = customerCompareLogic.Compare(projectSubscription, customerFetched);
            Assert.True(projectResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void UpdateProjectSubscription_WithChangedDates_Project_Customer_SucceedsInUpdate()
        {
            //Create a New Customer and Project
            var projectSubscription = GetMeACreateProjectSubscriptionEvent("Landfill");
            _subscriptionService.CreateProjectSubscription(projectSubscription);
            var oldProjectFetched = getProjectSubscription(projectSubscription.SubscriptionUID);

            //Create a New Project with New Customer
            var updateProjectSubscription = GetMeAnUpdateProjectSubscriptionEvent(projectSubscription.SubscriptionUID, customerGuid: Guid.NewGuid(),
                startDate: DateTime.UtcNow.AddDays(-2).Date, endDate: DateTime.UtcNow.AddDays(2).Date);
            _subscriptionService.UpdateProjectSubscription(updateProjectSubscription);
            var projectFetched = getProjectSubscription(projectSubscription.SubscriptionUID);
            Assert.NotNull(projectFetched);
            projectFetched.SubscriptionUID = new Guid(projectFetched.SubscriptionUIDString);

            var projectConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId" } };
            var projectCompareLogic = new CompareLogic(projectConfig);
            //Mapping to biggest Date Boundary for comparison
            projectSubscription.StartDate = updateProjectSubscription.StartDate.Value;
            projectSubscription.EndDate = updateProjectSubscription.EndDate.Value;
            ComparisonResult projectResult = projectCompareLogic.Compare(projectSubscription, projectFetched);
            Assert.True(projectResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(projectFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "ProjectUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            projectSubscription.CustomerUID = updateProjectSubscription.CustomerUID.Value;
            ComparisonResult customerResult = customerCompareLogic.Compare(projectSubscription, customerFetched);
            Assert.True(projectResult.Differences.Count == 0);
            var OldCustomerFetched = getCustomerSubscription(oldProjectFetched.CustomerSubscriptionID);
            Assert.Null(OldCustomerFetched);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void UpdateProjectSubscription_WithChangedDates_MultipleProject_Customer_SucceedsInUpdate()
        {
            //Create a New Customer and Project
            var firstProjectSubscription = GetMeACreateProjectSubscriptionEvent("Landfill");
            firstProjectSubscription.StartDate = DateTime.UtcNow.AddDays(-3).Date;
            firstProjectSubscription.EndDate = DateTime.UtcNow.AddDays(3).Date;
            _subscriptionService.CreateProjectSubscription(firstProjectSubscription);

            //Create a New Project with same Customer
            var projectSubscription = GetMeACreateProjectSubscriptionEvent("Project Monitoring");
            projectSubscription.CustomerUID = firstProjectSubscription.CustomerUID;
            _subscriptionService.CreateProjectSubscription(projectSubscription);
            var oldProjectFetched = getProjectSubscription(projectSubscription.SubscriptionUID);

            //Create a New Project with New Customer
            var updateProjectSubscription = GetMeAnUpdateProjectSubscriptionEvent(projectSubscription.SubscriptionUID, customerGuid: Guid.NewGuid(),
                startDate: DateTime.UtcNow.AddDays(-2).Date, endDate: DateTime.UtcNow.AddDays(2).Date);
            _subscriptionService.UpdateProjectSubscription(updateProjectSubscription);
            var projectFetched = getProjectSubscription(projectSubscription.SubscriptionUID);
            Assert.NotNull(projectFetched);
            projectFetched.SubscriptionUID = new Guid(projectFetched.SubscriptionUIDString);

            var projectConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId", "ProjectUIDString" } };
            var projectCompareLogic = new CompareLogic(projectConfig);
            //Mapping to biggest Date Boundary for comparison
            projectSubscription.StartDate = updateProjectSubscription.StartDate.Value;
            projectSubscription.EndDate = updateProjectSubscription.EndDate.Value;
            ComparisonResult projectResult = projectCompareLogic.Compare(projectSubscription, projectFetched);
            Assert.True(projectResult.Differences.Count == 0);
            var customerFetched = getCustomerSubscription(projectFetched.CustomerSubscriptionID);
            Assert.NotNull(customerFetched);
            customerFetched.CustomerUID = new Guid(customerFetched.CustomerUIDString);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "ProjectUID", "SubscriptionUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            projectSubscription.CustomerUID = updateProjectSubscription.CustomerUID.Value;
            projectSubscription.StartDate = firstProjectSubscription.StartDate;
            projectSubscription.EndDate = firstProjectSubscription.EndDate;
            ComparisonResult customerResult = customerCompareLogic.Compare(projectSubscription, customerFetched);
            Assert.True(projectResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void GetCustomerSubscription_WithChangedDates_MultipleProject_Customer_SucceedsInRead()
        {
            //Create a New Project with same Customer
            var firstProjectSubscription = GetMeACreateProjectSubscriptionEvent("Landfill");
            firstProjectSubscription.StartDate = DateTime.UtcNow.AddDays(-3).Date;
            firstProjectSubscription.EndDate = DateTime.UtcNow.AddDays(3).Date;
            _subscriptionService.CreateProjectSubscription(firstProjectSubscription);

            //Create a New Customer and Project
            var projectSubscription = GetMeACreateProjectSubscriptionEvent("Project Monitoring");
            projectSubscription.CustomerUID = firstProjectSubscription.CustomerUID;
            _subscriptionService.CreateProjectSubscription(projectSubscription);

            var customerSubscriptionModelList = _subscriptionService.GetSubscriptionForCustomer(firstProjectSubscription.CustomerUID);
            var customerConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "ProjectUID", "DeviceUID", "SubscriptionUID", "CustomerUID" } };
            var customerCompareLogic = new CompareLogic(customerConfig);
            ComparisonResult customerResult = customerCompareLogic.Compare(firstProjectSubscription, customerSubscriptionModelList[0]);
            Assert.True(customerResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void AssociateProjectSubscription_WithChangedDates_Succeeds()
        {
            //Create a New Project with same Customer
            var projectSubscription = GetMeACreateProjectSubscriptionEvent("Landfill");
            projectSubscription.StartDate = DateTime.UtcNow.AddDays(-3).Date;
            projectSubscription.EndDate = DateTime.UtcNow.AddDays(3).Date;
            _subscriptionService.CreateProjectSubscription(projectSubscription);

            //Associate New Customer and Project
            var associateProjectSubscription = new AssociateProjectSubscriptionEvent { SubscriptionUID = projectSubscription.SubscriptionUID, ProjectUID = Guid.NewGuid(), EffectiveDate = DateTime.UtcNow.AddDays(-1).Date };
            _subscriptionService.AssociateProjectSubscription(associateProjectSubscription);

            var projectFetched = getProjectSubscription(projectSubscription.SubscriptionUID);
            Assert.NotNull(projectFetched);
            projectFetched.SubscriptionUID = new Guid(projectFetched.SubscriptionUIDString);

            var projectConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId" } };
            var projectCompareLogic = new CompareLogic(projectConfig);
            //Mapping to biggest Date Boundary for comparison
            projectSubscription.StartDate = associateProjectSubscription.EffectiveDate;
            ComparisonResult projectResult = projectCompareLogic.Compare(projectSubscription, projectFetched);
            Assert.True(projectResult.Differences.Count == 0);
        }

        [Fact]
        [Trait("Category", "Database")] //needed for xunit test runner to exclude in MSBuild
        [Rollback]
        public void DissociateProjectSubscription_WithChangedDates_Succeeds()
        {
            //Create a New Project with same Customer
            var projectSubscription = GetMeACreateProjectSubscriptionEvent("Landfill");
            projectSubscription.StartDate = DateTime.UtcNow.AddDays(-3).Date;
            projectSubscription.EndDate = DateTime.UtcNow.AddDays(3).Date;
            _subscriptionService.CreateProjectSubscription(projectSubscription);

            //Associate New Customer and Project
            var dissociateProjectSubscription = new DissociateProjectSubscriptionEvent { SubscriptionUID = projectSubscription.SubscriptionUID, ProjectUID = Guid.NewGuid(), EffectiveDate = DateTime.UtcNow.AddDays(-1).Date };
            _subscriptionService.DissociateProjectSubscription(dissociateProjectSubscription);

            var projectFetched = getProjectSubscription(projectSubscription.SubscriptionUID);
            Assert.NotNull(projectFetched);
            projectFetched.SubscriptionUID = new Guid(projectFetched.SubscriptionUIDString);

            var projectConfig = new ComparisonConfig { IgnoreObjectTypes = true, MaxMillisecondsDateDifference = 500, MaxDifferences = 0, MembersToIgnore = new List<string>() { "ActionUTC", "ReceivedUTC", "SubscriptionType", "CustomerUID", "CustomerSubscriptionId" } };
            var projectCompareLogic = new CompareLogic(projectConfig);
            //Mapping to biggest Date Boundary for comparison
            projectSubscription.EndDate = dissociateProjectSubscription.EffectiveDate;
            ComparisonResult projectResult = projectCompareLogic.Compare(projectSubscription, projectFetched);
            Assert.True(projectResult.Differences.Count == 0);
        }

        private static ProjectSubscriptionData getProjectSubscription(Guid subscriptionUID)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MySql.Subscription"].ConnectionString;
            var ReadProjectQuery = String.Format("select ProjectSubscriptionUID as SubscriptionUIDString , fk_ProjectUID as ProjectUIDString, fk_CustomerSubscriptionID as CustomerSubscriptionID , StartDate , EndDate from ProjectSubscription where ProjectSubscriptionUID = '{0}'", subscriptionUID.ToString());
            ProjectSubscriptionData projectSubscriptionData;
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                projectSubscriptionData = connection.Query<ProjectSubscriptionData>(ReadProjectQuery).FirstOrDefault();
            }
            return projectSubscriptionData;
        }

        private static CustomerSubscriptionData getCustomerSubscription(long customerSubscritionId)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MySql.Subscription"].ConnectionString; ;
            var ReadProjectQuery = String.Format("select customer.CustomerSubscriptionID, customer.fk_CustomerUID as CustomerUIDString , serviceType.Name as ServiceType , customer.StartDate , customer.EndDate from CustomerSubscription customer inner join ServiceType serviceType on customer.fk_ServiceTypeID = serviceType.ServiceTypeID where customer.CustomerSubscriptionID = '{0}'", customerSubscritionId);
            CustomerSubscriptionData customerSubscriptionData;
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                customerSubscriptionData = connection.Query<CustomerSubscriptionData>(ReadProjectQuery).FirstOrDefault();
            }
            return customerSubscriptionData;
        }
    }

    public class ProjectSubscriptionData 
    {

        public string SubscriptionUIDString { get; set; }

        public Guid SubscriptionUID { get; set; }

        public string ProjectUIDString { get; set; }

        public Guid ProjectUID { get; set; }

        public long CustomerSubscriptionID { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
