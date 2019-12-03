using System;
using System.Linq;
using VSS.Productivity.Push.Models;
using VSS.Productivity3D.Push.Abstractions.UINotifications;
using VSS.Productivity3D.Push.Hubs.UINotifications;
using Xunit;
using FluentAssertions;

namespace VSS.Productivity3D.Push.UnitTests
{
  public class InMemoryProjectEventStateTest
  {
    [Fact]
    public void TestAddAndRemove()
    {
      var state = new InMemoryProjectEventState() as IProjectEventState;
      state.Should().NotBeNull();
      state.GetSubscriptions().Result.Count.Should().Be(0);

      const string clientId1 = "307CA37C-2422-4AD9-A512-34DFB3BC7507";
      const string clientId2 = "B0CCDF38-AA7E-4BCB-8692-53C898D6A469";

      var projectUid1 = Guid.Parse("2A131E56-880E-44E8-901F-86D3CD3CBDBB");
      var projectUid2 = Guid.Parse("D5C5A0FA-2480-4ACB-BDCE-0CDF10998BF8");
      var projectUid3 = Guid.Parse("BDE14152-1B1B-4809-85B2-384DA1D91B43");

      var customerUid1 = Guid.Parse("ABC14152-1B1B-4809-85B2-384DA1D91B45");

      var model1 = new ProjectEventSubscriptionModel() { ProjectUid = projectUid1, CustomerUid = customerUid1};
      var model2 = new ProjectEventSubscriptionModel() { ProjectUid = projectUid2, CustomerUid = customerUid1 };
      var model3 = new ProjectEventSubscriptionModel() { ProjectUid = projectUid3, CustomerUid = customerUid1 };

      // Add the first model to client connection #1, should result in 1
      {
        state.AddSubscription(clientId1, model1);
        var items = state.GetSubscriptions().Result;
        items.Count.Should().Be(1);
        items[0].Should().BeEquivalentTo(model1);
      }

      // Add the first model again, for the same client connection (this should replace the existing)
      {
        state.AddSubscription(clientId1, model1);
        var items = state.GetSubscriptions().Result;
        items.Count.Should().Be(1);
        items.Contains(model1).Should().BeTrue();
        items.Contains(model2).Should().BeFalse();
      }

      // Add the second model to client connection #1, for the same client connection (this should add to the subscription list)
      {
        state.AddSubscription(clientId1, model2);
        var items = state.GetSubscriptions().Result;
        items.Count.Should().Be(2);
        items.Contains(model1).Should().BeTrue();
        items.Contains(model2).Should().BeTrue();
      }

      // Add the third model to client connection #2
      {
        state.AddSubscription(clientId2, model3);
        var items = state.GetSubscriptions().Result;
        items.Count.Should().Be(3);
        items.Contains(model1).Should().BeTrue();
        items.Contains(model2).Should().BeTrue();
        items.Contains(model3).Should().BeTrue();
      }

      // Add the third model to client connection #1
      {
        state.AddSubscription(clientId1, model3);
        var items = state.GetSubscriptions().Result;
        items.Count.Should().Be(4);
        items.Contains(model1).Should().BeTrue();
        items.Contains(model2).Should().BeTrue();
        items.Count(x => x.ProjectUid == model3.ProjectUid).Should().Be(2);
        items.Contains(model3).Should().BeTrue();
      }

      // Unsupported at present to remove 1 subscription
      state.Invoking(x => x.RemoveSubscription(clientId1)).Should().Throw<NotImplementedException>();

      // Remove the subscriptions for the first client connection and an unknown subscription
      state.RemoveSubscriptions("does not exist id");

      // Count should be 1
      {
        state.RemoveSubscriptions(clientId1);
        var items = state.GetSubscriptions().Result;
        items.Count.Should().Be(1);
        items.Contains(model3).Should().BeTrue();
      }

      // should be empty
      {
        state.RemoveSubscriptions(clientId2);
        var items = state.GetSubscriptions().Result;
        items.Count.Should().Be(0);
      }
    }

    [Fact]
    public void TestClientsForProject()
    {
      // This test will ensure that when subscription is added, only events for that subscription get sent to it 
      var state = new InMemoryProjectEventState() as IProjectEventState;
      state.Should().NotBeNull();

      const string clientId1 = "AF284FC0-4D38-4337-A782-564ABD65D5CC";
      const string clientId2 = "C670850B-BF18-4AA3-963F-9820C47481C8";

      var projectUid1 = Guid.Parse("59436FBE-909F-4ED9-A038-9403CAA5782D");
      var projectUid2 = Guid.Parse("860706B6-E1B9-4E33-B74A-B2691430A0D7");

      var customerUid1 = Guid.Parse("67543B02-FF2A-4AE4-96DF-826BD29E3942");
      var customerUid2 = Guid.Parse("E59522CD-C0E3-4475-AD16-9C0C3035932B");

      // First test the same customer uid, different project uid
      state.AddSubscription(clientId1, new ProjectEventSubscriptionModel()
      {
        CustomerUid = customerUid1,
        ProjectUid = projectUid1
      });

      state.AddSubscription(clientId2, new ProjectEventSubscriptionModel()
      {
        CustomerUid = customerUid1,
        ProjectUid = projectUid2
      });

      // Now we should only get one clientid back
      {
        var clients = state.GetClientsForProject(customerUid1, projectUid1).Result;
        clients.Count.Should().Be(1);
        clients.Contains(clientId1).Should().BeTrue();
      }

      // Now same project UID but different customer UID.
      // in reality this is an invalid scenario as a project belongs to a customer and can't be viewed under another customer
      //    for this test, this is just rubbish data. In reality, ClientHub will authenticate the customer/project relationship
      state.AddSubscription(clientId1, new ProjectEventSubscriptionModel()
      {
        CustomerUid = customerUid1,
        ProjectUid = projectUid2
      });

      state.AddSubscription(clientId2, new ProjectEventSubscriptionModel()
      {
        CustomerUid = customerUid2,
        ProjectUid = projectUid2
      });

      // We should have 3 subscriptions, but only one for the project / customer
      {
        var subs = state.GetSubscriptions().Result;
        var clients = state.GetClientsForProject(customerUid2, projectUid2).Result;
        subs.Count.Should().Be(3);
        clients.Count.Should().Be(1);
        clients.Contains(clientId2).Should().BeTrue();
      }

      // Now update the subscriptions to be the same
      state.RemoveSubscriptions(clientId1);
      state.AddSubscription(clientId1, new ProjectEventSubscriptionModel()
      {
        CustomerUid = customerUid1,
        ProjectUid = projectUid1
      });

      state.RemoveSubscriptions(clientId2);
      state.AddSubscription(clientId2, new ProjectEventSubscriptionModel()
      {
        CustomerUid = customerUid1,
        ProjectUid = projectUid1
      });


      // We should have 2 subscriptions, both for the same customer/project
      {
        var subs = state.GetSubscriptions().Result;
        var clients = state.GetClientsForProject(customerUid1, projectUid1).Result;
        subs.Count.Should().Be(2);
        clients.Count.Should().Be(2);
        clients.Contains(clientId1).Should().BeTrue();
        clients.Contains(clientId2).Should().BeTrue();
      }

      // Now test a combo that returns no results
      {
        var clients = state.GetClientsForProject(customerUid2, projectUid2).Result;
        clients.Count.Should().Be(0);
      }
    }
  }
}
