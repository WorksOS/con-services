using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using VSS.Geofence.Data;
using VSS.Project.Data;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Subscription.Data.Tests
{
  [TestClass]
  public class Subscriptions
  {
    private readonly MySqlSubscriptionRepository _subscriptionService;
    private readonly MySqlProjectRepository _projectService;

    public Subscriptions()
    {
      _subscriptionService = new MySqlSubscriptionRepository();
      _projectService = new MySqlProjectRepository();
      _projectService.SetInTransactionState(true);
    }

    #region Privates
    private CreateProjectSubscriptionEvent GetNewCreateProjectSubscriptionEvent(Guid subscriptionUid)
    {
      return new CreateProjectSubscriptionEvent()
      {
        SubscriptionUID = subscriptionUid,
        CustomerUID = Guid.NewGuid(),
        SubscriptionType = "Landfill",
        StartDate = DateTime.UtcNow.AddDays(-1).Date,
        EndDate = DateTime.UtcNow.AddDays(1).Date,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow.AddMilliseconds(1000)
      };
    }

    private UpdateProjectSubscriptionEvent GetNewUpdateProjectSubscriptionEvent(Guid subscriptionUID, Guid customerUID, string subscriptionType, DateTime startDate, DateTime endDate, DateTime lastActionedUTC)
    {
      return new UpdateProjectSubscriptionEvent()
      {
        SubscriptionUID = subscriptionUID,
        CustomerUID = customerUID,
        SubscriptionType = subscriptionType,
        StartDate = startDate,
        EndDate = endDate,
        ActionUTC = lastActionedUTC,
        ReceivedUTC = DateTime.UtcNow.AddMilliseconds(100)
      };
    }

    private AssociateProjectSubscriptionEvent GetNewAssociateProjectSubscriptionEvent(Guid subscriptionUID, Guid projectUID, DateTime effectiveDate, DateTime receivedUTC)
    {
      return new AssociateProjectSubscriptionEvent()
      {
        SubscriptionUID = subscriptionUID,
        ProjectUID = projectUID,
        EffectiveDate = effectiveDate,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = receivedUTC
      };
    }

    private CreateProjectEvent GetNewCreateProjectEvent(Guid projectUid)
    {
      return new CreateProjectEvent()
      {
        ProjectUID = projectUid,
        ProjectID = 123,
        ProjectName = "Test Project",
        ProjectTimezone = "New Zealand Standard Time",
        ProjectType = ProjectType.LandFill,
        ProjectStartDate = DateTime.UtcNow.AddDays(-1).Date,
        ProjectEndDate = DateTime.UtcNow.AddDays(1).Date,
        ActionUTC = DateTime.UtcNow,
        ReceivedUTC = DateTime.UtcNow.AddMilliseconds(1000)
      };
    }

   
    //This mimics SubscriptionEventObserver processing
    private void AssociateProjectSubscription(AssociateProjectSubscriptionEvent evt)
    {
      var updatedCount = 0;
      var lastActionUtc = evt.ActionUTC;
      var project = _projectService.GetProject(evt.ProjectUID.ToString());
      if (project == null)
      {
        //Create dummy project
        lastActionUtc = DateTime.MinValue;
        updatedCount = _projectService.StoreProject(
              new CreateProjectEvent
              {
                ProjectUID = evt.ProjectUID,
                ProjectName = string.Empty,
                ProjectTimezone = string.Empty,
                ActionUTC = lastActionUtc
              });

        Assert.AreEqual(1, updatedCount, "Failed to save dummy project");    
      }
      //save subscriptionUID in project
      updatedCount = _projectService.AssociateProjectSubscription(evt.ProjectUID.ToString(),
          evt.SubscriptionUID.ToString(), lastActionUtc);
      Assert.AreEqual(1, updatedCount, "Failed to save subscription UID in project repo");
   
      //now save event
      updatedCount = _subscriptionService.StoreSubscription(evt);
      Assert.AreEqual(1, updatedCount, "Failed to save associate project subscription event");
    }
    #endregion

    [TestMethod]
    public void CreateNewProjectSubscription_Succeeds()
    {
      _subscriptionService.InRollbackTransaction<object>(o =>
      {
        Guid subscriptionUid = Guid.NewGuid();
        var createProjectSubscriptionEvent = GetNewCreateProjectSubscriptionEvent(subscriptionUid);
        var upsertCount = _subscriptionService.StoreSubscription(createProjectSubscriptionEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create a Landfill project subscription!");

        var subscription = _subscriptionService.GetSubscription(subscriptionUid.ToString());
        Assert.IsNotNull(subscription, "Failed to get the created project subscription!");

        createProjectSubscriptionEvent = GetNewCreateProjectSubscriptionEvent(subscriptionUid);

        upsertCount = _subscriptionService.StoreSubscription(createProjectSubscriptionEvent);
        Assert.AreEqual(1, upsertCount, "Failed to update the existing Landfill project subscription!");

        return null;
      });
    }

    [TestMethod]
    public void CreateNewProjectSubscription_Fails()
    {
      _subscriptionService.InRollbackTransaction<object>(o =>
      {
        Guid subscriptionUid = Guid.NewGuid();
        var createProjectSubscriptionEvent = GetNewCreateProjectSubscriptionEvent(subscriptionUid);
        createProjectSubscriptionEvent.SubscriptionType = "Unified Fleet";

        var upsertCount = _subscriptionService.StoreSubscription(createProjectSubscriptionEvent);
        Assert.AreEqual(0, upsertCount, "Should fail failed to create a Landfill project subscription due to the wrong subscription's type!");

        var subscription = _subscriptionService.GetSubscription(subscriptionUid.ToString());
        Assert.IsNull(subscription, "Should fail to get a project subscription!");

        return null;
      });
    }

    [TestMethod]
    public void UpsertSubscription_Fails()
    {
      var upsertCount = _subscriptionService.StoreSubscription(null);
      Assert.AreEqual(0, upsertCount, "Should fail to upsert a subscription!");
    }

    [TestMethod]
    public void UpdateProjectSubscription_Succeeds()
    {
      _subscriptionService.InRollbackTransaction<object>(o =>
      {
        Guid subscriptionUid = Guid.NewGuid();
        var createProjectSubscriptionEvent = GetNewCreateProjectSubscriptionEvent(subscriptionUid);
        var upsertCount = _subscriptionService.StoreSubscription(createProjectSubscriptionEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create a Landfill project subscription!");

        var updatedEndDate = createProjectSubscriptionEvent.EndDate.AddDays(3);
        var updateProjectSubscriptionEvent = GetNewUpdateProjectSubscriptionEvent(subscriptionUid,
                                                                                  createProjectSubscriptionEvent.CustomerUID,
                                                                                  createProjectSubscriptionEvent.SubscriptionType,
                                                                                  createProjectSubscriptionEvent.StartDate,
                                                                                  updatedEndDate,
                                                                                  DateTime.UtcNow);

        upsertCount = _subscriptionService.StoreSubscription(updateProjectSubscriptionEvent);
        Assert.AreEqual(1, upsertCount, "Failed to update the existing Landfill project subscription!");

        var subscription = _subscriptionService.GetSubscription(subscriptionUid.ToString());
        Assert.IsNotNull(subscription, "Failed to get the updated project subscription!");

        Assert.AreEqual(subscription.SubscriptionUID, subscriptionUid.ToString(), "Project Subscription SubscriptionUID should not be changed!");
        Assert.AreEqual(subscription.CustomerUID, updateProjectSubscriptionEvent.CustomerUID.ToString(), "Project Subscription CustomerUID should not be changed!");
        Assert.AreEqual(subscription.ServiceTypeID, _subscriptionService._serviceTypes[updateProjectSubscriptionEvent.SubscriptionType].ID, "Project Subscription Type should not be changed!");
        Assert.AreEqual(subscription.StartDate, createProjectSubscriptionEvent.StartDate, "Project Subscription Start Date of should not be changed!");
        Assert.AreEqual(updatedEndDate, subscription.EndDate, "The End Date of the updated Project Subscription was incorrectly updated!");
        Assert.IsTrue(subscription.LastActionedUTC > createProjectSubscriptionEvent.ActionUTC, "Project Subscription LastActionedUtc of the updated Project Subscription was incorrectly updated!");

        return null;
      });
    }

    [TestMethod]
    public void UpdateProjectSubscription_Fails()
    {
      _subscriptionService.InRollbackTransaction<object>(o =>
      {
        Guid subscriptionUid = Guid.NewGuid();
        var createProjectSubscriptionEvent = GetNewCreateProjectSubscriptionEvent(subscriptionUid);
        var upsertCount = _subscriptionService.StoreSubscription(createProjectSubscriptionEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create a Landfill project subscription!");

        var updateProjectSubscriptionEvent = GetNewUpdateProjectSubscriptionEvent(subscriptionUid,
                                                                                  createProjectSubscriptionEvent.CustomerUID,
                                                                                  "Unified Fleet",
                                                                                  createProjectSubscriptionEvent.StartDate,
                                                                                  createProjectSubscriptionEvent.EndDate.AddMonths(3),
                                                                                  DateTime.UtcNow);

        upsertCount = _subscriptionService.StoreSubscription(updateProjectSubscriptionEvent);
        Assert.AreEqual(0, upsertCount, "Should fail to update the existing Landfill project subscription due to the wrong subscription's type!");

        return null;
      });
    }
   

    /// <summary>
    /// CreateProjectSubscription event arrives first, the AssociateProjectSubscription event is coming after
    /// </summary>
    [TestMethod]
    public void AssociateProjectSubscription_Succeeds()
    {
      _subscriptionService.InRollbackTransaction<object>(o =>
      {
        Guid subscriptionUid = Guid.NewGuid();
        var createProjectSubscriptionEvent = GetNewCreateProjectSubscriptionEvent(subscriptionUid);
        var upsertCount = _subscriptionService.StoreSubscription(createProjectSubscriptionEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create a Landfill project subscription that is to be associated with a project!");

        var subscription = _subscriptionService.GetSubscription(subscriptionUid.ToString());
        Assert.IsNotNull(subscription, "Failed to get the subscription that is to be associated with a project!");

        Guid projectUid = Guid.NewGuid();
        DateTime effectiveUtc= DateTime.UtcNow.AddDays(-7);
        var associateProjectSubscriptionEvent = GetNewAssociateProjectSubscriptionEvent(subscriptionUid,
                                                                                        projectUid,
                                                                                        effectiveUtc,
                                                                                        DateTime.UtcNow.AddMilliseconds(100));

        upsertCount = _subscriptionService.StoreSubscription(associateProjectSubscriptionEvent);
        Assert.AreEqual(1, upsertCount, "Failed to associate the existing subscription with a Landfill project!");

        subscription = _subscriptionService.GetSubscription(subscriptionUid.ToString());
        Assert.IsNotNull(subscription, "Failed to get the subscription associated with a Landfill project!");
        Assert.AreEqual(effectiveUtc, subscription.EffectiveUTC, "Wrong EffectiveUTC");

        return null;
      });
    }

    /// <summary>
    /// The events are consumed from the Kafka queue in the following order:
    /// 1. CreateProject
    /// 2. CreateProjectSubscription
    /// 3. AssociateProjectSubscription
    /// </summary>
    [TestMethod]
    public void AssociateProjectSubscription_Project_Subscription_Associate_Succeeds()
    {
      _subscriptionService.InRollbackTransaction<object>(o =>
      {
        _projectService.SetConnection((MySqlConnection)o);
        
        // CreateProject event...
        Guid projectUid = Guid.NewGuid();
        var createProjectEvent = GetNewCreateProjectEvent(projectUid);
        var upsertCount = _projectService.StoreProject(createProjectEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create a Landfill project that is to be associated with a subscription!");

        var project = _projectService.GetProject(projectUid.ToString());
        Assert.IsNotNull(project, "Failed to get the created Landfill project that is to be associated with a subscription!");

        // CreateProjectSubscription event...
        Guid subscriptionUid = Guid.NewGuid();
        var createProjectSubscriptionEvent = GetNewCreateProjectSubscriptionEvent(subscriptionUid);
        upsertCount = _subscriptionService.StoreSubscription(createProjectSubscriptionEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create a project subscription that is to be associated with a project!");

        var subscription = _subscriptionService.GetSubscription(subscriptionUid.ToString());
        Assert.IsNotNull(subscription, "Failed to get the project subscription that is to be associated with a project!");

        // AssociateProjectSubscription event...
        var associateProjectSubscriptionEvent = GetNewAssociateProjectSubscriptionEvent(subscriptionUid,
                                                                                        projectUid,
                                                                                        DateTime.UtcNow,
                                                                                        DateTime.UtcNow.AddMilliseconds(100));
        AssociateProjectSubscription(associateProjectSubscriptionEvent);

        project = _projectService.GetProject(projectUid.ToString());
        Assert.IsNotNull(project, "Failed to get the existing project associated with the subscription!");

        Assert.AreEqual(project.SubscriptionUID, subscription.SubscriptionUID, "The associated Project's SubscriptionUID does not match the existing Subscription's one!");

        return null;
      });
    }

    /// <summary>
    /// The events are consumed from the Kafka queue in the following order:
    /// 1. CreateProjectSubscription
    /// 2. CreateProject
    /// 3. AssociateProjectSubscription
    /// </summary>
    [TestMethod]
    public void AssociateProjectSubscription_Subscription_Project_Associate_Succeeds()
    {
      _subscriptionService.InRollbackTransaction<object>(o =>
      {
        _projectService.SetConnection((MySqlConnection)o);

        // CreateProjectSubscription event...
        Guid subscriptionUid = Guid.NewGuid();
        var createProjectSubscriptionEvent = GetNewCreateProjectSubscriptionEvent(subscriptionUid);
        var upsertCount = _subscriptionService.StoreSubscription(createProjectSubscriptionEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create a project subscription that is to be associated with a project!");

        var subscription = _subscriptionService.GetSubscription(subscriptionUid.ToString());
        Assert.IsNotNull(subscription, "Failed to get the project subscription that is to be associated with a project!");

        // CreateProject event...
        Guid projectUid = Guid.NewGuid();
        var createProjectEvent = GetNewCreateProjectEvent(projectUid);
        upsertCount = _projectService.StoreProject(createProjectEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create a Landfill project that is to be associated with a subscription!");

        var project = _projectService.GetProject(createProjectEvent.ProjectUID.ToString());
        Assert.IsNotNull(project, "Failed to get the created Landfill project that is to be associated with a subscription!");

        // AssociateProjectSubscription event...
        var associateProjectSubscriptionEvent = GetNewAssociateProjectSubscriptionEvent(subscriptionUid,
                                                                                        projectUid,
                                                                                        DateTime.UtcNow,
                                                                                        DateTime.UtcNow.AddMilliseconds(100));
        AssociateProjectSubscription(associateProjectSubscriptionEvent);

        project = _projectService.GetProject(projectUid.ToString());
        Assert.IsNotNull(project, "Failed to get the existing project associated with the subscription!");

        Assert.AreEqual(project.SubscriptionUID, subscriptionUid.ToString(), "The associated Project's SubscriptionUID does not match the existing Subscription's one!");

        return null;
      });
    }

    /// <summary>
    /// The events are consumed from the Kafka queue in the following order:
    /// 1. CreateProjectSubscription
    /// 2. AssociateProjectSubscription
    /// 3. CreateProject
    /// </summary>
    [TestMethod]
    public void AssociateProjectSubscription_Subscription_Associate_Project_Succeeds()
    {
      _subscriptionService.InRollbackTransaction<object>(o =>
      {
        _projectService.SetConnection((MySqlConnection)o);

        // CreateProjectSubscription event...
        Guid subscriptionUid = Guid.NewGuid();
        var createProjectSubscriptionEvent = GetNewCreateProjectSubscriptionEvent(subscriptionUid);
        var upsertCount = _subscriptionService.StoreSubscription(createProjectSubscriptionEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create a project subscription that is to be associated with the existing Landfill project!");

        var subscription = _subscriptionService.GetSubscription(subscriptionUid.ToString());
        Assert.IsNotNull(subscription, "Failed to get the project subscription that is to be associated with the existing Landfill project!");

        // AssociateProjectSubscription event...
        Guid projectUid = Guid.NewGuid();
        var associateProjectSubscriptionEvent = GetNewAssociateProjectSubscriptionEvent(subscriptionUid,
                                                                                        projectUid,
                                                                                        DateTime.UtcNow,
                                                                                        DateTime.UtcNow.AddMilliseconds(100));
        AssociateProjectSubscription(associateProjectSubscriptionEvent);

        // CreateProject event...
        var createProjectEvent = GetNewCreateProjectEvent(projectUid);
        upsertCount = _projectService.StoreProject(createProjectEvent);
        Assert.AreEqual(1, upsertCount, "Failed to create a Landfill project that is to be associated with a subscription!");

        var project = _projectService.GetProject(createProjectEvent.ProjectUID.ToString());
        Assert.IsNotNull(project, "Failed to get the created Landfill project that is to be associated with a subscription!");

        Assert.AreEqual(project.SubscriptionUID, subscriptionUid.ToString(), "The associated Project's SubscriptionUID does not match the existing Subscription's one!");
        Assert.AreEqual(createProjectEvent.ProjectName,  project.Name, "The associated Project's name did not get updated!");

        return null;
      });
    }
  }
}
