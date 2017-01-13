using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VSS.Project.Service.Utils;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.Project.Data;
using VSS.Project.Data.Models;
using VSS.Customer.Data;
using VSS.Subscription.Data.Models;
using VSS.Project.Service.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace RepositoryTests
{
  [TestClass]
  public class ProjectSubscriptionRepositoryTests
  {
    [TestInitialize]
    public void Init()
    {
      var serviceCollection = new ServiceCollection();
      serviceCollection.AddSingleton<ILoggerFactory>((new LoggerFactory()).AddDebug());
      new DependencyInjectionProvider(serviceCollection.BuildServiceProvider());
    }

    #region ProjectSubscriptions

    /// <summary>
    /// Create ProjectSubscription - Happy path i.e. 
    ///   customer, project, CustomerProject relationship exists
    ///   subscription doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateSubscriptionWithProject_HappyPath()
    {
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      { CustomerUID = Guid.NewGuid(), CustomerName = "The Project Name", CustomerType = CustomerType.Customer.ToString(), ActionUTC = now };

      var createProjectEvent = new CreateProjectEvent()
      { ProjectUID = Guid.NewGuid(), ProjectID = 12343, ProjectName = "The Project Name", ProjectType = ProjectType.LandFill, ProjectTimezone = projectTimeZone,
        ProjectStartDate = new DateTime(2016, 02, 01), ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = now
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      { CustomerUID = createCustomerEvent.CustomerUID, ProjectUID = createProjectEvent.ProjectUID, LegacyCustomerID = 1234, RelationType = RelationType.Customer, ActionUTC = now };

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(2017, 02, 01),
        ActionUTC = now
      };

      var associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent()
      {
        SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID,        
        ProjectUID = createProjectEvent.ProjectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = now
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());
      var projectContext = new ProjectRepository(new GenericConfiguration());
      var subscriptionContext = new SubscriptionRepository(new GenericConfiguration());
      
      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();
      
      var s = subscriptionContext.StoreEvent(createProjectSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "ProjectSubscription event not written");

      s = subscriptionContext.StoreEvent(associateProjectSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "associateProjectSubscription event not written");


      Subscription subscription = CopyModel(subscriptionContext, createProjectSubscriptionEvent);
      var g = subscriptionContext.GetSubscription(createProjectSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve subscription from subscriptionRepo");
      Assert.AreEqual(subscription, g.Result, "subscription details are incorrect from subscriptionRepo");
    }

    /// <summary>
    /// Create Subscription - Happy path out of order
    ///  same as above but out of order
    /// </summary>
    [TestMethod]
    public void CreateSubscriptionWithProject_HappyPathButOutOfOrder()
    {
      throw new NotImplementedException();
    }


    /// <summary>
    /// Create Subscription - Happy path RelationShips not setup i.e. 
    ///   customer and CustomerProject relationship NOT added
    ///   Subscription doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateSubscriptionWithProject_HappyPath_NoProject()
    {
      throw new NotImplementedException();
    }


    /// <summary>
    /// Create Subscription - Subscription already exists
    ///   customer and CustomerProject relationship also added
    ///   Subscription exists but is different.
    /// </summary>
    [TestMethod]
    public void CreateSubscriptionWithProject_SubscriptionExists()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Update Subscription - Happy path i.e. 
    ///   customer and CustomerProject relationship also added
    ///   Subscription exists and New ActionUTC is later than its LastActionUTC.
    /// </summary>
    [TestMethod]
    public void UpdateSubscription_HappyPath()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Update Subscription - earlier ActionUTC 
    ///   customer and CustomerProject relationship also added
    ///   Subscription exists and New ActionUTC is earlier than its LastActionUTC.
    /// </summary>
    [TestMethod]
    public void UpdateSubscription_OldUpdate()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Associate Customer Subscription - not supported?
    /// </summary>
    [TestMethod]
    public void AssociateCustomerSubscription_NotSupported()
    {
      throw new NotImplementedException();
    }
    #endregion
    
    #region AssociateSubscriptionWithProject

    /// <summary>
    /// AssociateProjectSubscriptionEvent - Happy Path
    ///   project and sub added.
    /// </summary>
    [TestMethod]
    public void AssociateProjectSubscriptionEvent_HappyPath()
    {      
      throw new NotImplementedException();
    }

    /// <summary>
    /// AssociateProjectSubscriptionEvent - already exists
    /// </summary>
    [TestMethod]
    public void AssociateProjectSubscriptionEvent_AlreadyExists()
    {
      throw new NotImplementedException();
    }

    #endregion

    #region private
    private CreateProjectSubscriptionEvent CopyModel(SubscriptionRepository subscriptionRepo, Subscription subscription)
    {
      return new CreateProjectSubscriptionEvent()
      {
        SubscriptionUID = Guid.Parse(subscription.SubscriptionUID),
        CustomerUID = Guid.Parse(subscription.CustomerUID),
        // todo SubscriptionType = subscriptionRepo._serviceTypes.ID(subscription.ServiceTypeID),
        StartDate = subscription.StartDate,
        EndDate = subscription.EndDate,
        ActionUTC = subscription.LastActionedUTC
      };
    }

    private Subscription CopyModel(SubscriptionRepository subscriptionRepo, CreateProjectSubscriptionEvent kafkaProjectSubscriptionEvent)
    {
      return new Subscription()
      {
        SubscriptionUID = kafkaProjectSubscriptionEvent.SubscriptionUID.ToString(),
        CustomerUID = kafkaProjectSubscriptionEvent.CustomerUID.ToString(),
        ServiceTypeID = subscriptionRepo._serviceTypes[kafkaProjectSubscriptionEvent.SubscriptionType].ID,
        StartDate = kafkaProjectSubscriptionEvent.StartDate,
        EndDate = kafkaProjectSubscriptionEvent.EndDate, 
        LastActionedUTC = kafkaProjectSubscriptionEvent.ActionUTC,
        
      };
    }
    #endregion

  }
}
 
 