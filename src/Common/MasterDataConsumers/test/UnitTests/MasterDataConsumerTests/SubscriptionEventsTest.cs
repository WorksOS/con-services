using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.MasterDataConsumer.Tests
{
  [TestClass]
  public class SubscriptionEventsTest
  {
    private Dictionary<int, string> serviceTypes = null;
    private Dictionary<string, int> serviceTypeIDs = null;

    [TestInitialize]
    public void InitTest()
    {
      serviceTypes = new Dictionary<int, string>();
      serviceTypes.Add(1, "Asset");
      serviceTypes.Add(2, "Customer");
      serviceTypes.Add(3, "Project");

      serviceTypeIDs = serviceTypes.ToDictionary(item => item.Value, item => item.Key);// To Dictionary<string, int>...
    }

    [TestMethod]
    public void AssetSubscriptionEventsCopyModels()
    {
      var subscription = CreatSubscriptionByServicetype("Asset");

      var kafkaSubcriptionEvent = CopyAssetSubscriptionModel(subscription);
      var copiedSubscription = CopyAssetSubscriptionModel(kafkaSubcriptionEvent);

      Assert.AreEqual(subscription, copiedSubscription, "Asset Subscription model conversion not completed sucessfully");
    }

    [TestMethod]
    public void CustomerSubscriptionEventsCopyModels()
    {
      var now = DateTime.UtcNow;

      var subscription = CreatSubscriptionByServicetype("Customer");

      var kafkaSubcriptionEvent = CopyCustomerSubscriptionModel(subscription);
      var copiedSubscription = CopyCustomerSubscriptionModel(kafkaSubcriptionEvent);

      Assert.AreEqual(subscription, copiedSubscription, "Customer Subscription model conversion not completed sucessfully");
    }

    [TestMethod]
    public void ProjectSubscriptionEventsCopyModels()
    {
      var now = DateTime.UtcNow;

      var subscription = CreatSubscriptionByServicetype("Project");

      var kafkaSubcriptionEvent = CopyProjectSubscriptionModel(subscription);
      var copiedSubscription = CopyProjectSubscriptionModel(kafkaSubcriptionEvent);

      Assert.AreEqual(subscription, copiedSubscription, "Customer Subscription model conversion not completed sucessfully");
    }

    #region private
    private Subscription CreatSubscriptionByServicetype(string serviceType)
    {
      var now = DateTime.UtcNow;

      return new Subscription()
      {
        SubscriptionUID = Guid.NewGuid().ToString(),
        CustomerUID = Guid.NewGuid().ToString(),
        ServiceTypeID = serviceTypeIDs[serviceType],
        StartDate = now,
        EndDate = now.AddYears(1),
        LastActionedUTC = now
      };
    }

    private CreateAssetSubscriptionEvent CopyAssetSubscriptionModel(Subscription subscription)
    {
      return new CreateAssetSubscriptionEvent()
      {
        SubscriptionUID = Guid.Parse(subscription.SubscriptionUID),
        CustomerUID = Guid.Parse(subscription.CustomerUID),
        SubscriptionType = serviceTypes[subscription.ServiceTypeID],
        StartDate = subscription.StartDate,
        EndDate = subscription.EndDate,
        ActionUTC = subscription.LastActionedUTC
      };
    }

    private Subscription CopyAssetSubscriptionModel(CreateAssetSubscriptionEvent kafkaSubcriptionEvent)
    {
      return new Subscription()
      {
        SubscriptionUID = kafkaSubcriptionEvent.SubscriptionUID.ToString(),
        CustomerUID = kafkaSubcriptionEvent.CustomerUID.ToString(),
        ServiceTypeID = serviceTypeIDs[kafkaSubcriptionEvent.SubscriptionType],
        StartDate = kafkaSubcriptionEvent.StartDate,
        EndDate = kafkaSubcriptionEvent.EndDate,
        LastActionedUTC = kafkaSubcriptionEvent.ActionUTC
      };
    }

    private CreateCustomerSubscriptionEvent CopyCustomerSubscriptionModel(Subscription subscription)
    {      
      return new CreateCustomerSubscriptionEvent()
      {
        SubscriptionUID = Guid.Parse(subscription.SubscriptionUID),
        CustomerUID = Guid.Parse(subscription.CustomerUID),
        SubscriptionType = serviceTypes[subscription.ServiceTypeID],
        StartDate = subscription.StartDate,
        EndDate = subscription.EndDate,
        ActionUTC = subscription.LastActionedUTC
      };
    }

    private Subscription CopyCustomerSubscriptionModel(CreateCustomerSubscriptionEvent kafkaSubcriptionEvent)
    {
      return new Subscription()
      {
        SubscriptionUID = kafkaSubcriptionEvent.SubscriptionUID.ToString(),
        CustomerUID = kafkaSubcriptionEvent.CustomerUID.ToString(),
        ServiceTypeID = serviceTypeIDs[kafkaSubcriptionEvent.SubscriptionType],
        StartDate = kafkaSubcriptionEvent.StartDate,
        EndDate = kafkaSubcriptionEvent.EndDate,
        LastActionedUTC = kafkaSubcriptionEvent.ActionUTC
      };
    }

    private CreateProjectSubscriptionEvent CopyProjectSubscriptionModel(Subscription subscription)
    {
      return new CreateProjectSubscriptionEvent()
      {
        SubscriptionUID = Guid.Parse(subscription.SubscriptionUID),
        CustomerUID = Guid.Parse(subscription.CustomerUID),
        SubscriptionType = serviceTypes[subscription.ServiceTypeID],
        StartDate = subscription.StartDate,
        EndDate = subscription.EndDate,
        ActionUTC = subscription.LastActionedUTC
      };
    }

    private Subscription CopyProjectSubscriptionModel(CreateProjectSubscriptionEvent kafkaSubcriptionEvent)
    {
      return new Subscription()
      {
        SubscriptionUID = kafkaSubcriptionEvent.SubscriptionUID.ToString(),
        CustomerUID = kafkaSubcriptionEvent.CustomerUID.ToString(),
        ServiceTypeID = serviceTypeIDs[kafkaSubcriptionEvent.SubscriptionType],
        StartDate = kafkaSubcriptionEvent.StartDate,
        EndDate = kafkaSubcriptionEvent.EndDate,
        LastActionedUTC = kafkaSubcriptionEvent.ActionUTC
      };
    }
    #endregion
  }
}
