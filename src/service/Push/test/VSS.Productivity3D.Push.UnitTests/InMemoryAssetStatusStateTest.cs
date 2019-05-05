using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity.Push.Models;
using VSS.Productivity3D.Push.Abstractions.AssetLocations;
using VSS.Productivity3D.Push.Hubs.AssetLocations;

namespace VSS.Productivity3D.Push.UnitTests
{
  [TestClass]
  public class InMemoryAssetStatusStateTest
  {
    [TestMethod]
    public void TestAddAndRemove()
    {
      var state = new InMemoryAssetStatusState() as IAssetStatusState;
      Assert.IsNotNull(state);

      {
        var count = state.GetSubscriptions().Result.Count;
        Assert.IsTrue(count == 0, $"Expected a zero count with a new state, got ${count} instead");
      }

      const string clientId1 = "307CA37C-2422-4AD9-A512-34DFB3BC7507";
      const string clientId2 = "B0CCDF38-AA7E-4BCB-8692-53C898D6A469";

      var model1 = new AssetUpdateSubscriptionModel()
      {
        ProjectUid = Guid.Parse("2A131E56-880E-44E8-901F-86D3CD3CBDBB")
      };
      var model2 = new AssetUpdateSubscriptionModel()
      {
        ProjectUid = Guid.Parse("D5C5A0FA-2480-4ACB-BDCE-0CDF10998BF8")
      };
      var model3 = new AssetUpdateSubscriptionModel()
      {
        ProjectUid = Guid.Parse("BDE14152-1B1B-4809-85B2-384DA1D91B43")
      };

      // Add the first model
      state.AddSubscription(clientId1, model1);

      // count should be 1
      {
        var items = state.GetSubscriptions().Result;
        Assert.IsTrue(items.Count == 1, $"Added a model, count should be one but it is ${items.Count}");
        Assert.AreEqual(model1, items[0]);
      }

      // Add the second model, with the same identifier (this should replace the existing one)
      state.AddSubscription(clientId1, model2);

      // count should be 1
      {
        var items = state.GetSubscriptions().Result;
        Assert.IsTrue(items.Count == 1,$"Added a model with the same identifier, count should be one but it is ${items.Count}");
        Assert.AreEqual(model2, items[0]);
      }

      // Add another model, with another identifier
      state.AddSubscription(clientId2, model3);

      // Count should be 2
      {
        var items = state.GetSubscriptions().Result;
        Assert.IsTrue(items.Count == 2,$"Added a model with a new identifier, count should be two but it is ${items.Count}");
        Assert.IsTrue(items.Contains(model2));
        Assert.IsTrue(items.Contains(model3));
      }

      // Remove the first subscription and an unknown subscription
      state.RemoveSubscription(clientId1);
      state.RemoveSubscription("does not exist id");

      // Count should be 1
      {
        var items = state.GetSubscriptions().Result;
        Assert.IsTrue(items.Count == 1,$"Removed a subscription, count should be one but it is ${items.Count}");
        Assert.IsTrue(items.Contains(model3));
      }

      state.RemoveSubscription(clientId2);

      // should be empty
      {
        var count = state.GetSubscriptions().Result.Count;
        Assert.IsTrue(count == 0, $"Expected a zero count with all clients removed, got ${count} instead");
      }
    }

    [TestMethod]
    public void TestClientsForProject()
    {
      // This test will ensure that when subscription is added, only events for that subscription get sent to it 
      var state = new InMemoryAssetStatusState() as IAssetStatusState;
      Assert.IsNotNull(state);

      const string clientId1 = "AF284FC0-4D38-4337-A782-564ABD65D5CC";
      const string clientId2 = "C670850B-BF18-4AA3-963F-9820C47481C8";

      var projectUid1 = Guid.Parse("59436FBE-909F-4ED9-A038-9403CAA5782D");
      var projectUid2 = Guid.Parse("860706B6-E1B9-4E33-B74A-B2691430A0D7");

      var customerUid1 = Guid.Parse("67543B02-FF2A-4AE4-96DF-826BD29E3942");
      var customerUid2 = Guid.Parse("E59522CD-C0E3-4475-AD16-9C0C3035932B");

      // First test the same customer uid, different project uid
      state.AddSubscription(clientId1, new AssetUpdateSubscriptionModel()
      {
        CustomerUid = customerUid1,
        ProjectUid = projectUid1
      });

      state.AddSubscription(clientId2, new AssetUpdateSubscriptionModel()
      {
        CustomerUid = customerUid1,
        ProjectUid = projectUid2
      });

      // Now we should only get one clientid back
      {
        var clients = state.GetClientsForProject(customerUid1, projectUid1).Result;
        Assert.IsTrue(clients.Count == 1, $"Expected a single client but got ${clients.Count}");
        Assert.AreEqual(clientId1, clients[0]);
      }

      // Now same project UID but different customer UID (also this should replace the existing subscription)
      state.AddSubscription(clientId1, new AssetUpdateSubscriptionModel()
      {
        CustomerUid = customerUid1,
        ProjectUid = projectUid2
      });

      state.AddSubscription(clientId2, new AssetUpdateSubscriptionModel()
      {
        CustomerUid = customerUid2,
        ProjectUid = projectUid2
      });

      // We should have 2 subscriptions, but only one for the project / customer
      {
        var subs = state.GetSubscriptions().Result;
        var clients = state.GetClientsForProject(customerUid2, projectUid2).Result;
        Assert.IsTrue(subs.Count == 2,$"Subs don't match, expected two got ${subs.Count}");
        Assert.IsTrue(clients.Count == 1, $"Expected a single client but got ${clients.Count}");
        Assert.AreEqual(clientId2, clients[0]);
      }

      // Now update the subscriptions to be the same
      state.AddSubscription(clientId1, new AssetUpdateSubscriptionModel()
      {
        CustomerUid = customerUid1,
        ProjectUid = projectUid1
      });

      state.AddSubscription(clientId2, new AssetUpdateSubscriptionModel()
      {
        CustomerUid = customerUid1,
        ProjectUid = projectUid1
      });

      
      // We should have 2 subscriptions, both for the same proejct / customer
      {
        var subs = state.GetSubscriptions().Result;
        var clients = state.GetClientsForProject(customerUid1, projectUid1).Result;
        Assert.IsTrue(subs.Count == 2,$"Subs don't match, expected two got ${subs.Count}");
        Assert.IsTrue(clients.Count == 2, $"Expected two clients but got ${clients.Count}");
        Assert.IsTrue(clients.Contains(clientId1));
        Assert.IsTrue(clients.Contains(clientId2));
      }

      // Now test a combo that returns no results
      {
        var clients = state.GetClientsForProject(customerUid2, projectUid2).Result;
        Assert.IsTrue(clients.Count == 0, $"Expected no clients but got ${clients.Count}");
      }
    }
  }
}
