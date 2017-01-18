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
using System.Linq;

namespace RepositoryTests
{
  [TestClass]
  public class ProjectSubscriptionRepositoryTests
  {
    IServiceProvider serviceProvider = null;
    SubscriptionRepository subscriptionContext = null;
    CustomerRepository customerContext = null;
    ProjectRepository projectContext = null;

    [TestInitialize]
    public void Init()
    {
      serviceProvider = new ServiceCollection()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddSingleton<ILoggerFactory>((new LoggerFactory()).AddDebug())
        .BuildServiceProvider();
      subscriptionContext = new SubscriptionRepository(serviceProvider.GetService<IConfigurationStore>());
      customerContext = new CustomerRepository(serviceProvider.GetService<IConfigurationStore>());
      projectContext = new ProjectRepository(serviceProvider.GetService<IConfigurationStore>());
    }

    #region ProjectSubscriptions

    /// <summary>
    /// Create ProjectSubscription - Happy path i.e. 
    ///   subscription doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateSubscriptionWithProject_HappyPath()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUID = Guid.NewGuid();

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };
      
      var s = subscriptionContext.StoreEvent(createProjectSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "ProjectSubscription event not written");

      Subscription subscription = CopyModel(subscriptionContext, createProjectSubscriptionEvent);
      var g = subscriptionContext.GetSubscription(createProjectSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve subscription from subscriptionRepo");
      Assert.IsTrue(CompareSubs(subscription, g.Result), "subscription details are incorrect from subscriptionRepo");
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

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var createProjectSubscriptionEvent2 = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID,
        SubscriptionType = "Landfill",
        StartDate = new DateTime(2015, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC.AddHours(1)
      };
      
      subscriptionContext.StoreEvent(createProjectSubscriptionEvent).Wait();
      var s = subscriptionContext.StoreEvent(createProjectSubscriptionEvent2);
      s.Wait();
      Assert.AreEqual(0, s.Result, "newCreate should not be written");

      Subscription subscription = CopyModel(subscriptionContext, createProjectSubscriptionEvent);
      var g = subscriptionContext.GetSubscription(createProjectSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve subscription from subscriptionRepo");
      Assert.IsTrue(CompareSubs(subscription, g.Result), "subscription details are incorrect from subscriptionRepo");
    }

    /// <summary>
    /// Create Subscription - future enddates are NOT allowed
    ///    this will result in new endDate
    /// </summary>
    [TestMethod]
    public void CreateProjectSubscription_FutureEndDate()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUID = Guid.NewGuid();

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(2110, 12, 31),
        ActionUTC = actionUTC
      };
      
      subscriptionContext.StoreEvent(createProjectSubscriptionEvent).Wait();

      Subscription subscription = CopyModel(subscriptionContext, createProjectSubscriptionEvent);
      subscription.EndDate = new DateTime(9999, 12, 31);
      var g = subscriptionContext.GetSubscription(createProjectSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve subscription from subscriptionRepo");
      Assert.IsTrue(CompareSubs(subscription, g.Result), "subscription details are incorrect from subscriptionRepo");
    }

    /// <summary>
    /// Update Subscription - Happy path i.e. 
    ///   customer and CustomerProject relationship also added
    ///   Subscription exists and New ActionUTC is later than its LastActionUTC.
    /// </summary>
    [TestMethod]
    public void UpdateSubscription_HappyPath()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUID = Guid.NewGuid();

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = ActionUTC
      };

      var updateProjectSubscriptionEvent = new UpdateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID,
        StartDate = new DateTime(2015, 02, 01),
        EndDate = new DateTime(2016, 12, 31),
        ActionUTC = ActionUTC
      };
      
      subscriptionContext.StoreEvent(createProjectSubscriptionEvent).Wait();
      var s = subscriptionContext.StoreEvent(updateProjectSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Update ProjectSubscription event not written");

      Subscription subscription = CopyModel(subscriptionContext, createProjectSubscriptionEvent);
      subscription.StartDate = updateProjectSubscriptionEvent.StartDate.Value;
      subscription.EndDate = updateProjectSubscriptionEvent.EndDate.Value;
      subscription.LastActionedUTC = updateProjectSubscriptionEvent.ActionUTC;
      var g = subscriptionContext.GetSubscription(createProjectSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve subscription from subscriptionRepo");
      Assert.IsTrue(CompareSubs(subscription, g.Result), "subscription details are incorrect from subscriptionRepo");
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

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = ActionUTC
      };

      var updateProjectSubscriptionEvent = new UpdateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID,
        StartDate = new DateTime(2015, 02, 01),
        EndDate = new DateTime(2016, 12, 31),
        ActionUTC = ActionUTC.AddMinutes(-10)
      };
      
      subscriptionContext.StoreEvent(createProjectSubscriptionEvent).Wait();
      var s = subscriptionContext.StoreEvent(updateProjectSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Shouldn't update");

      Subscription subscription = CopyModel(subscriptionContext, createProjectSubscriptionEvent);
      var g = subscriptionContext.GetSubscription(createProjectSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve subscription from subscriptionRepo");
      Assert.IsTrue(CompareSubs(subscription, g.Result), "subscription details are incorrect from subscriptionRepo");
    }

    /// <summary>
    /// Update Subscription - startdate, endDate and customerUID in event can be null
    ///    the event in db should not be overwritten by nulls, be be retained.
    ///    only overwrite if new not null
    ///    currently the repo allows customerUID and serviceType to be changed - is this intentional?
    /// </summary>
    [TestMethod]
    public void UpdateSubscription_datesNull()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUID = Guid.NewGuid();

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = ActionUTC
      };

      var updateProjectSubscriptionEvent = new UpdateProjectSubscriptionEvent()
      {
        CustomerUID = null,
        SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID,
        StartDate = null,
        EndDate = null,
        ActionUTC = ActionUTC.AddMinutes(10)
      };
      
      subscriptionContext.StoreEvent(createProjectSubscriptionEvent).Wait();
      var s = subscriptionContext.StoreEvent(updateProjectSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Will appear to update");

      Subscription subscription = CopyModel(subscriptionContext, createProjectSubscriptionEvent);
      subscription.LastActionedUTC = updateProjectSubscriptionEvent.ActionUTC;
      var g = subscriptionContext.GetSubscription(createProjectSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve subscription from subscriptionRepo");
      Assert.IsTrue(CompareSubs(subscription, g.Result), "subscription details are incorrect from subscriptionRepo");
    }

    /// <summary>
    /// Update Subscription - future enddates are ALLOWED for an update
    /// </summary>
    [TestMethod]
    public void UpdateSubscription_FutureEndDate()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUID = Guid.NewGuid();

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(2016, 12, 31),
        ActionUTC = ActionUTC
      };

      var updateProjectSubscriptionEvent = new UpdateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID,
        StartDate = new DateTime(2015, 02, 01),
        EndDate = new DateTime(2110, 12, 31),
        ActionUTC = ActionUTC.AddHours(1)
      };
      
      subscriptionContext.StoreEvent(createProjectSubscriptionEvent).Wait();
      var s = subscriptionContext.StoreEvent(updateProjectSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Update ProjectSubscription event not written");

      Subscription subscription = CopyModel(subscriptionContext, createProjectSubscriptionEvent);
      subscription.StartDate = updateProjectSubscriptionEvent.StartDate.Value;
      subscription.EndDate = updateProjectSubscriptionEvent.EndDate.Value;
      subscription.LastActionedUTC = updateProjectSubscriptionEvent.ActionUTC;
      var g = subscriptionContext.GetSubscription(createProjectSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve subscription from subscriptionRepo");
      Assert.IsTrue(CompareSubs(subscription, g.Result), "subscription details are incorrect from subscriptionRepo");
    }

    #endregion

    #region AssociateSubscriptionWithProject

    /// <summary>
    /// AssociateProjectSubscriptionEvent - HappyPath
    /// </summary>
    [TestMethod]
    public void AssociateProjectSubscriptionEvent_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var subscriptionUID = Guid.NewGuid();
      var projectUID = Guid.NewGuid();

      var associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent()
      {
        SubscriptionUID = subscriptionUID,
        ProjectUID = projectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUtc
      };
      
      var s = subscriptionContext.StoreEvent(associateProjectSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "associateProjectSubscription event not written");

      var g = subscriptionContext.GetProjectSubscriptions_UnitTest(associateProjectSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve 1 project/sub from projectRepo");
      var projectSubscriptions = g.Result.ToList();
      Assert.AreEqual(1, projectSubscriptions.Count(), "project details are incorrect from subscriptionRepo");
      Assert.AreEqual(associateProjectSubscriptionEvent.ProjectUID.ToString(), projectSubscriptions[0].ProjectUID, "project details are incorrect from subscriptionRepo");
    }

    /// <summary>
    /// AssociateProjectSubscriptionEvent - already exists
    /// </summary>
    [TestMethod]
    public void AssociateProjectSubscriptionEvent_AlreadyExists()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var subscriptionUID = Guid.NewGuid();
      var projectUID = Guid.NewGuid();

      var associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent()
      {
        SubscriptionUID = subscriptionUID,
        ProjectUID = projectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUtc
      };
      
      subscriptionContext.StoreEvent(associateProjectSubscriptionEvent).Wait();
      var s = subscriptionContext.StoreEvent(associateProjectSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "associateProjectSubscription should not be re-written");

      var g = subscriptionContext.GetProjectSubscriptions_UnitTest(associateProjectSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve 1 project/sub from projectRepo");
      var projectSubscriptions = g.Result.ToList();
      Assert.AreEqual(1, projectSubscriptions.Count(), "project details are incorrect from subscriptionRepo");
      Assert.AreEqual(associateProjectSubscriptionEvent.ProjectUID.ToString(), projectSubscriptions[0].ProjectUID, "project details are incorrect from subscriptionRepo");
    }

    /// <summary>
    /// AssociateProjectSubscriptionEvent - Happy Path QueryBySub
    ///   project and sub added.
    /// </summary>
    [TestMethod]
    public void AssociateProjectSubscriptionEvent_HappyPathBySub()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";
      var customerUID = Guid.NewGuid();

      var createCustomerEvent = new CreateCustomerEvent()
      { CustomerUID = Guid.NewGuid(), CustomerName = "The Customer Name", CustomerType = CustomerType.Customer.ToString(), ActionUTC = actionUtc };

      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      { CustomerUID = createCustomerEvent.CustomerUID, ProjectUID = createProjectEvent.ProjectUID, LegacyCustomerID = 1234, RelationType = RelationType.Customer, ActionUTC = actionUtc };

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUtc
      };

      var associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent()
      {
        SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID,
        ProjectUID = createProjectEvent.ProjectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUtc
      };
      
      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      subscriptionContext.StoreEvent(createProjectSubscriptionEvent).Wait();
      var s = subscriptionContext.StoreEvent(associateProjectSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "associateProjectSubscription event not written");

      Project project = CopyModel(createProjectEvent);
      project.CustomerUID = associateCustomerProjectEvent.CustomerUID.ToString();
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project.SubscriptionEndDate = createProjectSubscriptionEvent.EndDate;
      project.SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID.ToString();
      var g = projectContext.GetProjectBySubcription(createProjectSubscriptionEvent.SubscriptionUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve 1 project/sub from projectRepo");
      Assert.AreEqual(createProjectSubscriptionEvent.SubscriptionUID.ToString(), g.Result.SubscriptionUID, "project details are incorrect from subscriptionRepo");
      Assert.AreEqual(project, g.Result, "project details are incorrect from subscriptionRepo");
    }


    /// <summary>
    /// AssociateProjectSubscriptionEvent - HappyPath multipleSubs
    ///   will return 2 in list
    ///    todo even though this returns 2 subs for this project, there is no way for the user
    ///      to determine what type of service type each is. Also only the endDate is available.
    /// </summary>
    [TestMethod]
    public void AssociateProjectSubscriptionEvent_HappyPath_ByUserMultipleSubs()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      { CustomerUID = Guid.NewGuid(), CustomerName = "The Customer Name", CustomerType = CustomerType.Customer.ToString(), ActionUTC = actionUtc };

      var associateCustomerUser = new AssociateCustomerUserEvent()
      { CustomerUID = createCustomerEvent.CustomerUID, UserUID = Guid.NewGuid(), ActionUTC = actionUtc };


      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      { CustomerUID = createCustomerEvent.CustomerUID, ProjectUID = createProjectEvent.ProjectUID, LegacyCustomerID = 1234, RelationType = RelationType.Customer, ActionUTC = actionUtc };

      var createProjectSubscriptionEvent1 = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(2016, 11, 30),
        ActionUTC = actionUtc
      };

      var associateProjectSubscriptionEvent1 = new AssociateProjectSubscriptionEvent()
      {
        SubscriptionUID = createProjectSubscriptionEvent1.SubscriptionUID,
        ProjectUID = createProjectEvent.ProjectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUtc
      };

      var createProjectSubscriptionEvent2 = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Landfill",
        StartDate = new DateTime(2016, 10, 01),
        EndDate = new DateTime(2016, 12, 31),
        ActionUTC = actionUtc.AddMinutes(20)
      };

      var associateProjectSubscriptionEvent2 = new AssociateProjectSubscriptionEvent()
      {
        SubscriptionUID = createProjectSubscriptionEvent2.SubscriptionUID,
        ProjectUID = createProjectEvent.ProjectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUtc.AddMinutes(22)
      };
      
      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      customerContext.StoreEvent(associateCustomerUser).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      subscriptionContext.StoreEvent(createProjectSubscriptionEvent1).Wait();
      subscriptionContext.StoreEvent(associateProjectSubscriptionEvent1).Wait();

      subscriptionContext.StoreEvent(createProjectSubscriptionEvent2).Wait();
      subscriptionContext.StoreEvent(associateProjectSubscriptionEvent2).Wait();

      var g = projectContext.GetProjectsForUser(associateCustomerUser.UserUID.ToString());
      g.Wait();
      var projects = g.Result.ToList(); //  as IList<VSS.Project.Data.Models.Project>;
      Assert.IsNotNull(projects, "Unable to retrieve 1 project/sub from projectRepo");
      Assert.AreEqual(2, projects.Count, "should be 1 project/sub from projectRepo");

      Project project1 = CopyModel(createProjectEvent);
      project1.CustomerUID = associateCustomerProjectEvent.CustomerUID.ToString();
      project1.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project1.SubscriptionEndDate = createProjectSubscriptionEvent1.EndDate;
      project1.SubscriptionUID = createProjectSubscriptionEvent1.SubscriptionUID.ToString();

      // can't gaurantee any order as none provided
      if (createProjectSubscriptionEvent1.SubscriptionUID.ToString() == projects[0].SubscriptionUID)
        Assert.AreEqual(project1, projects[0], "project details 1 are incorrect from Project-sub Repo");
      else
        Assert.AreEqual(project1, projects[1], "project details 1 are incorrect from Project-sub Repo");

      Project project2 = CopyModel(createProjectEvent);
      project2.CustomerUID = associateCustomerProjectEvent.CustomerUID.ToString();
      project2.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project2.SubscriptionEndDate = createProjectSubscriptionEvent2.EndDate;
      project2.SubscriptionUID = createProjectSubscriptionEvent2.SubscriptionUID.ToString();

      // can't gaurantee any order as none provided
      if (createProjectSubscriptionEvent2.SubscriptionUID.ToString() == projects[0].SubscriptionUID)
        Assert.AreEqual(project2, projects[0], "project details 2 are incorrect from Project-sub Repo");
      else
        Assert.AreEqual(project2, projects[1], "project details 2 are incorrect from Project-sub Repo");
    }

    /// <summary>
    /// AssociateProjectSubscriptionEvent - No subs for user
    ///   will return 0
    /// </summary>
    [TestMethod]
    public void AssociateProjectSubscriptionEvent_HappyPath_ByUserNoSubs()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      { CustomerUID = Guid.NewGuid(), CustomerName = "The Customer Name", CustomerType = CustomerType.Customer.ToString(), ActionUTC = actionUtc };

      var associateCustomerUser = new AssociateCustomerUserEvent()
      { CustomerUID = createCustomerEvent.CustomerUID, UserUID = Guid.NewGuid(), ActionUTC = actionUtc };


      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      { CustomerUID = createCustomerEvent.CustomerUID, ProjectUID = createProjectEvent.ProjectUID, LegacyCustomerID = 1234, RelationType = RelationType.Customer, ActionUTC = actionUtc };
      
      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      customerContext.StoreEvent(associateCustomerUser).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      var g = projectContext.GetProjectsForUser(associateCustomerUser.UserUID.ToString());
      g.Wait();
      var projects = g.Result.ToList(); //  as IList<VSS.Project.Data.Models.Project>;
      Assert.IsNotNull(projects, "Unable to retrieve 1 project/sub");
      Assert.AreEqual(1, projects.Count, "should be 1 project/sub");

      Assert.IsNull(projects[0].SubscriptionUID, "sub should be null");
      Assert.AreEqual(DateTime.MinValue, projects[0].SubscriptionEndDate, "sub endDate should be null");
    }

    #endregion

    #region private
    private CreateProjectSubscriptionEvent CopyModel(SubscriptionRepository subscriptionRepo, Subscription subscription)
    {
      return new CreateProjectSubscriptionEvent()
      {
        SubscriptionUID = Guid.Parse(subscription.SubscriptionUID),
        CustomerUID = Guid.Parse(subscription.CustomerUID),
        SubscriptionType = subscription.ServiceTypeID.ToString(),
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

    private bool CompareSubs(Subscription original, Subscription result)
    {
      if (original.SubscriptionUID == result.SubscriptionUID
        && original.CustomerUID == result.CustomerUID
        && original.ServiceTypeID == result.ServiceTypeID
        && original.StartDate == result.StartDate
        && original.EndDate == result.EndDate
        && original.LastActionedUTC == result.LastActionedUTC
        )
        return true;
      else
        return false;
    }

    private CreateProjectEvent CopyModel(Project project)
    {
      return new CreateProjectEvent()
      {
        ProjectUID = Guid.Parse(project.ProjectUID),
        ProjectID = project.LegacyProjectID,
        ProjectName = project.Name,
        ProjectType = project.ProjectType,
        ProjectTimezone = project.ProjectTimeZone,

        ProjectStartDate = project.StartDate,
        ProjectEndDate = project.EndDate,
        ActionUTC = project.LastActionedUTC
      };
    }

    private Project CopyModel(CreateProjectEvent kafkaProjectEvent)
    {
      return new Project()
      {
        ProjectUID = kafkaProjectEvent.ProjectUID.ToString(),
        LegacyProjectID = kafkaProjectEvent.ProjectID,
        Name = kafkaProjectEvent.ProjectName,
        ProjectType = kafkaProjectEvent.ProjectType,
        // IsDeleted =  N/A

        ProjectTimeZone = kafkaProjectEvent.ProjectTimezone,
        LandfillTimeZone = TimeZone.WindowsToIana(kafkaProjectEvent.ProjectTimezone),

        LastActionedUTC = kafkaProjectEvent.ActionUTC,
        StartDate = kafkaProjectEvent.ProjectStartDate,
        EndDate = kafkaProjectEvent.ProjectEndDate
      };
    }
    #endregion

  }
}
 
 