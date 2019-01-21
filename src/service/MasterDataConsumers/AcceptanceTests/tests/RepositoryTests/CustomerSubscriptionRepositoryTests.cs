using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryTests.Internal;
using System;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace RepositoryTests
{
  [TestClass]
  public class CustomerSubscriptionRepositoryTests : TestControllerBase
  {
    SubscriptionRepository subscriptionContext;

    [TestInitialize]
    public void Init()
    {
      SetupLogging();

      subscriptionContext = new SubscriptionRepository(ServiceProvider.GetService<IConfigurationStore>(), ServiceProvider.GetService<ILoggerFactory>());
    }

    #region CustomerSubscriptions

    /// <summary>
    /// Create CustomerSubscription - Happy path i.e. 
    ///   subscription doesn't exist already and has a customerUID
    /// </summary>
    [TestMethod]
    public void CreateCustomerSubscription_HappyPath()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent()
      {
        CustomerUID = Guid.NewGuid(),
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Manual 3D Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var s = subscriptionContext.StoreEvent(createCustomerSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "CustomerSubscription event not written");

      Subscription subscription = CopyModel(subscriptionContext, createCustomerSubscriptionEvent);
      var g = subscriptionContext.GetSubscription(createCustomerSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve subscription from subscriptionRepo");
      Assert.IsTrue(TestHelpers.CompareSubs(subscription, g.Result), "subscription details are incorrect from subscriptionRepo");
    }

    /// <summary>
    /// Create CustomerSubscription - unhandled subtype
    /// </summary>
    [TestMethod]
    public void CreateCustomerSubscription_UnknownSubscriptionType()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent()
      {
        CustomerUID = Guid.NewGuid(),
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Manual 3D Project Monitoring blah de blah",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var s = subscriptionContext.StoreEvent(createCustomerSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "CustomerSubscription event should not be written");
    }

    /// <summary>
    /// Create CustomerSubscription - unhandled subtype
    /// </summary>
    [TestMethod]
    public void CreateCustomerSubscription_UnhandledSubscriptionType()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent()
      {
        CustomerUID = Guid.NewGuid(),
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Essentials",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var s = subscriptionContext.StoreEvent(createCustomerSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "CustomerSubscription event should not be written");
    }

    /// <summary>
    /// Create CustomerSubscription - unhandled subtype
    /// </summary>
    [TestMethod]
    public void CreateCustomerSubscription_WrongSubscriptionGroup()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent()
      {
        CustomerUID = Guid.NewGuid(),
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Landfill",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var s = subscriptionContext.StoreEvent(createCustomerSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "CustomerSubscription event should not be written");
    }


    /// <summary>
    /// Create CustomerSubscription - wrong subscription family for Customer
    /// </summary>
    [TestMethod]
    public void CreateCustomerSubscription_WrongSubscriptionFamily()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent()
      {
        CustomerUID = Guid.NewGuid(),
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "3D Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var s = subscriptionContext.StoreEvent(createCustomerSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "CustomerSubscription event not written");
    }

    /// <summary>
    /// Create Subscription - Subscription already exists
    ///   Subscription exists but is different. nonsense - ignore
    /// </summary>
    [TestMethod]
    public void CreateSubscriptionWithProject_SubscriptionExistsButDifferentType()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUID = Guid.NewGuid();

      var createCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Manual 3D Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var createCustomerSubscriptionEvent2 = new CreateCustomerSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = createCustomerSubscriptionEvent.SubscriptionUID,
        SubscriptionType = "Landfill",
        StartDate = new DateTime(2015, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC.AddHours(1)
      };

      subscriptionContext.StoreEvent(createCustomerSubscriptionEvent).Wait();
      var s = subscriptionContext.StoreEvent(createCustomerSubscriptionEvent2);
      s.Wait();
      Assert.AreEqual(0, s.Result, "newCreate should not be written");

      Subscription subscription = CopyModel(subscriptionContext, createCustomerSubscriptionEvent);
      var g = subscriptionContext.GetSubscription(createCustomerSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve subscription from subscriptionRepo");
      Assert.IsTrue(TestHelpers.CompareSubs(subscription, g.Result), "subscription details are incorrect from subscriptionRepo");
    }

    /// <summary>
    /// Create Subscription - future enddates are NOT allowed
    ///    this will result in new endDate
    /// </summary>
    [TestMethod]
    public void CreateSubscription_FutureEndDate()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUID = Guid.NewGuid();

      var createCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Manual 3D Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(2110, 12, 31),
        ActionUTC = actionUTC
      };

      subscriptionContext.StoreEvent(createCustomerSubscriptionEvent).Wait();

      Subscription subscription = CopyModel(subscriptionContext, createCustomerSubscriptionEvent);
      subscription.EndDate = new DateTime(9999, 12, 31);
      var g = subscriptionContext.GetSubscription(createCustomerSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve subscription from subscriptionRepo");
      Assert.IsTrue(TestHelpers.CompareSubs(subscription, g.Result), "subscription details are incorrect from subscriptionRepo");
    }

    /// <summary>
    /// Update Subscription - Happy path i.e. 
    ///   For a customersub, there is no CustomerSubscription association object.
    ///      The CustomerUID is included in a subscription.
    ///      The sub is terminated/dis-associated by setting the EndDate to e.g. past date
    /// </summary>
    [TestMethod]
    public void UpdateSubscription_HappyPath()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUID = Guid.NewGuid();

      var createCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Manual 3D Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = ActionUTC
      };

      var updateCustomerSubscriptionEvent = new UpdateCustomerSubscriptionEvent()
      {
        SubscriptionUID = createCustomerSubscriptionEvent.SubscriptionUID,
        StartDate = new DateTime(2015, 02, 01),
        EndDate = new DateTime(2015, 12, 31),
        ActionUTC = ActionUTC
      };

      subscriptionContext.StoreEvent(createCustomerSubscriptionEvent).Wait();
      var s = subscriptionContext.StoreEvent(updateCustomerSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Update CustomerSubscription event not written");

      Subscription subscription = CopyModel(subscriptionContext, createCustomerSubscriptionEvent);
      subscription.StartDate = updateCustomerSubscriptionEvent.StartDate.Value;
      subscription.EndDate = updateCustomerSubscriptionEvent.EndDate.Value;
      subscription.LastActionedUTC = updateCustomerSubscriptionEvent.ActionUTC;
      var g = subscriptionContext.GetSubscription(createCustomerSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve subscription from subscriptionRepo");
      Assert.IsTrue(TestHelpers.CompareSubs(subscription, g.Result), "subscription details are incorrect from subscriptionRepo");
    }


    /// <summary>
    /// Update Subscription - earlier ActionUTC 
    ///   ignore it
    /// </summary>
    [TestMethod]
    public void UpdateSubscription_OldActionUTC()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUID = Guid.NewGuid();

      var createCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Manual 3D Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = ActionUTC
      };

      var updateCustomerSubscriptionEvent = new UpdateCustomerSubscriptionEvent()
      {
        SubscriptionUID = createCustomerSubscriptionEvent.SubscriptionUID,
        StartDate = new DateTime(2015, 02, 01),
        EndDate = new DateTime(2016, 12, 31),
        ActionUTC = ActionUTC.AddMinutes(-10)
      };

      subscriptionContext.StoreEvent(createCustomerSubscriptionEvent).Wait();
      var s = subscriptionContext.StoreEvent(updateCustomerSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Shouldn't update");

      Subscription subscription = CopyModel(subscriptionContext, createCustomerSubscriptionEvent);
      var g = subscriptionContext.GetSubscription(createCustomerSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve subscription from subscriptionRepo");
      Assert.IsTrue(TestHelpers.CompareSubs(subscription, g.Result), "subscription details are incorrect from subscriptionRepo");
    }

    #endregion

    #region AssetSubscriptions
    /// <summary>
    /// Create AssetSubscription 
    /// </summary>
    [TestMethod]
    public void CreateAssetSubscription_HappyPath()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createAssetSubscriptionEvent = new CreateAssetSubscriptionEvent()
      {
        CustomerUID = Guid.NewGuid(),
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "3D Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var s = subscriptionContext.StoreEvent(createAssetSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "AssetSubscription event not written");
    }

    /// <summary>
    /// Create AssetSubscription 
    /// </summary>
    [TestMethod]
    public void CreateAssetSubscription_UnsupportedType()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createAssetSubscriptionEvent = new CreateAssetSubscriptionEvent()
      {
        CustomerUID = Guid.NewGuid(),
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Essentials",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var s = subscriptionContext.StoreEvent(createAssetSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "AssetSubscription event should not have been written");
    }

    /// <summary>
    /// Create AssetSubscription 
    /// </summary>
    [TestMethod]
    public void CreateAssetSubscription_WrongGroup()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createAssetSubscriptionEvent = new CreateAssetSubscriptionEvent()
      {
        CustomerUID = Guid.NewGuid(),
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Manual 3D Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var s = subscriptionContext.StoreEvent(createAssetSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "AssetSubscription event should not have been written");
    }

    /// <summary>
    /// Create AssetSubscription - unhandled subtype
    /// </summary>
    [TestMethod]
    public void CreateAssetSubscription_UnhandledSubscriptionType()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createAssetSubscriptionEvent = new CreateAssetSubscriptionEvent()
      {
        CustomerUID = Guid.NewGuid(),
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Essentials blah",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var s = subscriptionContext.StoreEvent(createAssetSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "AssetSubscription event should not have been written");
    }

    /// <summary>
    /// Create AssetSubscription - wrong subscription family for Asset
    /// </summary>
    [TestMethod]
    public void CreateAssetSubscription_WrongSubscriptionFamily()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createAssetSubscriptionEvent = new CreateAssetSubscriptionEvent()
      {
        CustomerUID = Guid.NewGuid(),
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Manual 3D Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var s = subscriptionContext.StoreEvent(createAssetSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "AssetSubscription event should not have been written");
    }

    #endregion AssetSubscriptions

    #region private

    private static Subscription CopyModel(SubscriptionRepository subscriptionRepo, CreateCustomerSubscriptionEvent kafkaCustomerSubscriptionEvent)
    {
      return new Subscription()
      {
        SubscriptionUID = kafkaCustomerSubscriptionEvent.SubscriptionUID.ToString(),
        CustomerUID = kafkaCustomerSubscriptionEvent.CustomerUID.ToString(),
        ServiceTypeID = subscriptionRepo.ServiceTypes[kafkaCustomerSubscriptionEvent.SubscriptionType].ID,
        StartDate = kafkaCustomerSubscriptionEvent.StartDate,
        EndDate = kafkaCustomerSubscriptionEvent.EndDate,
        LastActionedUTC = kafkaCustomerSubscriptionEvent.ActionUTC,

      };
    }

    #endregion
  }
}
