using System;
using System.Linq;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.Extensions;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.NH_DATAMockObjectSet;
using Xunit;
using Assert =Xunit.Assert;

namespace LegacyApiUserProvisioning.UserManagement.Tests
{
    [TestClass]
    public class UserManagerTests : TestsBase
    {
        #region POSITIVE TEST CASES
        
        [Fact]
        public void GetUsersByOrganization_Success()
        {
            var logger = Substitute.For<ILog>();
            var nhOpContext = new NH_OPMock();
           
            const int customerId = 1;
            const int userId = 1;

            var customer = this.RandomCustomer;
            customer.ID = customerId;

            var user = this.RandomUser;
            user.ID = userId;
            user.fk_CustomerID = customerId;

            var userFeature = this.RandomUserFeature;
            userFeature.fk_Feature = 3000;
            userFeature.fk_User = userId;

            nhOpContext.Customer.AddObject(customer);
            nhOpContext.UserFeature.AddObject(userFeature);
            nhOpContext.User.AddObject(user);

            var userManager = new UserManager(logger, nhOpContext);

            var users = userManager.GetUsersByOrganization(customer.CustomerUID.ToString()).ToList();

            Assert.True(users.Count() == 1);
            Assert.Equal(users[0].UserName, user.Name);
            Assert.Equal(users[0].FirstName, user.FirstName);
            Assert.Equal(users[0].LastName, user.LastName);
            Assert.Equal(users[0].Email, user.EmailContact);

        }

        [Fact]
        public void GetApiFeaturesByUserName_ValidUserName_Success()
        {
            var logger = Substitute.For<ILog>();
            var nhOpContext = new NH_OPMock();

            var user = this.RandomUser;
            var feature = this.RandomFeature;
            var userFeature = this.RandomUserFeature;
            var featureAccess = this.RandomFeatureAccess;

            userFeature.fk_User = user.ID;
            userFeature.Feature = feature;
            userFeature.fk_Feature = feature.ID;
            userFeature.fk_FeatureAccess = featureAccess.ID;

            featureAccess.UserFeature.Add(userFeature);

            nhOpContext.User.AddObject(user);
            nhOpContext.Feature.AddObject(feature);
            nhOpContext.UserFeature.AddObject(userFeature);
            nhOpContext.FeatureAccess.AddObject(featureAccess);

            var userName = user.Name;
            var userManager = new UserManager(logger, nhOpContext);
            var res = userManager.GetApiFeaturesByUserName(userName);
            Assert.True(res.Any());

        }

        #endregion

        #region NEGATIVE TEST CASES

        [Fact]
        public void GetUsersByOrganization_NoAPIFeature_Failure()
        {
            var logger = Substitute.For<ILog>();
            var nhOpContext = new NH_OPMock();

            const int customerId = 1;
            const int userId = 1;

            var customer = this.RandomCustomer;
            customer.ID = customerId;

            var user = this.RandomUser;
            user.ID = userId;
            user.fk_CustomerID = customerId;

            var userFeature = this.RandomUserFeature;
            userFeature.fk_Feature = 1000;
            userFeature.fk_User = userId;

            nhOpContext.Customer.AddObject(customer);
            nhOpContext.UserFeature.AddObject(userFeature);
            nhOpContext.User.AddObject(user);

            var userManager = new UserManager(logger, nhOpContext);

            var users = userManager.GetUsersByOrganization(customer.CustomerUID.ToString()).ToList();

            Assert.True(!users.Any());
        }

     
        [Fact]
        public void GetApiFeaturesByUserName_InValidUserName_Failure()
        {
            var logger = Substitute.For<ILog>();
            var nhOpContext = new NH_OPMock();

            var user = this.RandomUser;
            var feature = this.RandomFeature;
            var userFeature = this.RandomUserFeature;
            var featureAccess = this.RandomFeatureAccess;

            userFeature.fk_User = user.ID;
            userFeature.Feature = feature;
            userFeature.fk_Feature = feature.ID;
            userFeature.fk_FeatureAccess = featureAccess.ID;

            featureAccess.UserFeature.Add(userFeature);

            nhOpContext.User.AddObject(user);
            nhOpContext.Feature.AddObject(feature);
            nhOpContext.UserFeature.AddObject(userFeature);
            nhOpContext.FeatureAccess.AddObject(featureAccess);

            var userName = user.Name+"SomeSuffix";
            var userManager = new UserManager(logger, nhOpContext);
            var res = userManager.GetApiFeaturesByUserName(userName);
            Assert.False(res.Any());
        }

        #endregion
    }
}
