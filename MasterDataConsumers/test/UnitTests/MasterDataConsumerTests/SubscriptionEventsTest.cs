using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System.Collections.Generic;

namespace MasterDataConsumerTests
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

      serviceTypeIDs = new Dictionary<string, int>();
      serviceTypeIDs.Add("Asset", 1);
      serviceTypeIDs.Add("Customer", 2);
      serviceTypeIDs.Add("Project", 3);
    }

    [TestMethod]
    public void CustomerEventsCopyModels()
    {
      var now = DateTime.UtcNow;

      var subscription = new Subscription()
      {
        SubscriptionUID = Guid.NewGuid().ToString(),
        CustomerUID = Guid.NewGuid().ToString(),
        ServiceTypeID = serviceTypeIDs["Customer"],
        StartDate = now,
        EndDate = now.AddYears(1),
        LastActionedUTC = now
      };

      var kafkaSubcriptionEvent = CopyCustomerSubscriptionModel(subscription);
      var copiedSubscription = CopyCustomerSubscriptionModel(kafkaSubcriptionEvent);

      Assert.AreEqual(subscription, copiedSubscription, "Subscription model conversion not completed sucessfully");
    }

    #region private
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
    #endregion
  }
}
