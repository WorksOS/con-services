using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System.Linq;
using log4netExtensions;
using VSS.GenericConfiguration;
using Repositories;
using Repositories.DBModels;

namespace RepositoryTests
{
  [TestClass]
  public class ProjectSubscriptionRepositoryTests
  {
    IServiceProvider serviceProvider = null;
    SubscriptionRepository subscriptionContext = null;
    CustomerRepository customerContext = null;
    ProjectRepository projectContext = null;
    GeofenceRepository geofenceContext = null;

    [TestInitialize]
    public void Init()
    {
      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceProvider = new ServiceCollection()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddLogging()
        .AddSingleton<ILoggerFactory>(loggerFactory)
        .BuildServiceProvider();
      subscriptionContext = new SubscriptionRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
      customerContext = new CustomerRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
      projectContext = new ProjectRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
      geofenceContext = new GeofenceRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
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
    /// Create ProjectSubscription - unhandled subtype
    /// </summary>
    [TestMethod]
    public void CreateSubscriptionWithProject_UnhandledSubscriptionType()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUID = Guid.NewGuid();

      var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = customerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring de blah",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUTC
      };

      var s = subscriptionContext.StoreEvent(createProjectSubscriptionEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "ProjectSubscription event not written");
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
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
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
      project.ServiceTypeID = subscriptionContext._serviceTypes[createProjectSubscriptionEvent.SubscriptionType].ID;
      project.SubscriptionStartDate = createProjectSubscriptionEvent.StartDate;
      project.SubscriptionEndDate = createProjectSubscriptionEvent.EndDate;
      project.SubscriptionUID = createProjectSubscriptionEvent.SubscriptionUID.ToString();
      project.IsDeleted = false;
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
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
          ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
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
      var projects = g.Result.ToList();
      Assert.IsNotNull(projects, "Unable to retrieve 1 project/sub from projectRepo");
      Assert.AreEqual(2, projects.Count, "should be 1 project/sub from projectRepo");

      g = projectContext.GetProjectsForCustomerUser(associateCustomerUser.CustomerUID.ToString(), associateCustomerUser.UserUID.ToString());
      g.Wait();
      projects = g.Result.ToList();
      Assert.IsNotNull(projects, "Unable to retrieve 1 project/sub from projectRepo");
      Assert.AreEqual(2, projects.Count, "should be 1 project/sub from projectRepo");

      Project project1 = CopyModel(createProjectEvent);
      project1.CustomerUID = associateCustomerProjectEvent.CustomerUID.ToString();
      project1.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project1.ServiceTypeID = subscriptionContext._serviceTypes[createProjectSubscriptionEvent1.SubscriptionType].ID;
      project1.SubscriptionStartDate = createProjectSubscriptionEvent1.StartDate;
      project1.SubscriptionEndDate = createProjectSubscriptionEvent1.EndDate;
      project1.SubscriptionUID = createProjectSubscriptionEvent1.SubscriptionUID.ToString();
      project1.IsDeleted = false;

      // can't gaurantee any order as none provided
      if (createProjectSubscriptionEvent1.SubscriptionUID.ToString() == projects[0].SubscriptionUID)
        Assert.AreEqual(project1, projects[0], "project details 1 are incorrect from Project-sub Repo");
      else
        Assert.AreEqual(project1, projects[1], "project details 1 are incorrect from Project-sub Repo");

      Project project2 = CopyModel(createProjectEvent);
      project2.CustomerUID = associateCustomerProjectEvent.CustomerUID.ToString();
      project2.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project2.ServiceTypeID = subscriptionContext._serviceTypes[createProjectSubscriptionEvent2.SubscriptionType].ID;
      project2.SubscriptionStartDate = createProjectSubscriptionEvent2.StartDate;
      project2.SubscriptionEndDate = createProjectSubscriptionEvent2.EndDate;
      project2.SubscriptionUID = createProjectSubscriptionEvent2.SubscriptionUID.ToString();
      project2.IsDeleted = false;

      // can't gaurantee any order as none provided
      if (createProjectSubscriptionEvent2.SubscriptionUID.ToString() == projects[0].SubscriptionUID)
        Assert.AreEqual(project2, projects[0], "project details 2 are incorrect from Project-sub Repo");
      else
        Assert.AreEqual(project2, projects[1], "project details 2 are incorrect from Project-sub Repo");
    }

    /// <summary>
    /// AssociateProjectSubscriptionEvent - HappyPath multipleSubs and multiProjects
    ///   ProjectA has sub 1) dates 1-3 b) dates 2-5
    ///   ProjectB has no subs
    ///   ... should return ProjectA with SubB AND ProjectB with no sub
    ///   also to return GeometryWKT
    /// </summary>
    [TestMethod]
    public void AssociateProjectSubscriptionEvent_HappyPath_ByCustomerMultipleProjectsAndSubs()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";
      var projectUidA = Guid.NewGuid();
      var projectUidB = Guid.NewGuid();

      var createCustomerEvent = new CreateCustomerEvent()
      { CustomerUID = Guid.NewGuid(), CustomerName = "The Customer Name", CustomerType = CustomerType.Customer.ToString(), ActionUTC = actionUtc };

      var createProjectAEvent = new CreateProjectEvent()
      {
        ProjectUID = projectUidA,
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var associateCustomerProjectAEvent = new AssociateProjectCustomer()
      { CustomerUID = createCustomerEvent.CustomerUID, ProjectUID = createProjectAEvent.ProjectUID, LegacyCustomerID = 1234, RelationType = RelationType.Customer, ActionUTC = actionUtc };

      var createProjectASubscriptionEvent1 = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(2016, 11, 30),
        ActionUTC = actionUtc
      };

      var associateProjectASubscriptionEvent1 = new AssociateProjectSubscriptionEvent()
      {
        SubscriptionUID = createProjectASubscriptionEvent1.SubscriptionUID,
        ProjectUID = createProjectAEvent.ProjectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUtc
      };

      var createProjectASubscriptionEvent2 = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Landfill",
        StartDate = new DateTime(2016, 10, 01),
        EndDate = new DateTime(2016, 12, 31),
        ActionUTC = actionUtc.AddMinutes(20)
      };

      var associateProjectASubscriptionEvent2 = new AssociateProjectSubscriptionEvent()
      {
        SubscriptionUID = createProjectASubscriptionEvent2.SubscriptionUID,
        ProjectUID = createProjectAEvent.ProjectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUtc.AddMinutes(22)
      };

      var createProjectAGeofenceProjectTypeEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Project.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = Guid.NewGuid(),
        ActionUTC = actionUtc
      };

      var associateProjectAGeofenceProjectTypeEvent = new AssociateProjectGeofence()
      {
        ProjectUID = createProjectAEvent.ProjectUID,
        GeofenceUID = createProjectAGeofenceProjectTypeEvent.GeofenceUID,
        ActionUTC = actionUtc.AddDays(1)
      };

      var createProjectAGeofenceNonProjectTypeEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = Guid.NewGuid(),
        ActionUTC = actionUtc
      };

      var associateProjectAGeofenceNonProjectTypeEvent = new AssociateProjectGeofence()
      {
        ProjectUID = createProjectAEvent.ProjectUID,
        GeofenceUID = createProjectAGeofenceNonProjectTypeEvent.GeofenceUID,
        ActionUTC = actionUtc.AddDays(1)
      };


      customerContext.StoreEvent(createCustomerEvent).Wait();

      projectContext.StoreEvent(createProjectAEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectAEvent).Wait();

      subscriptionContext.StoreEvent(createProjectASubscriptionEvent1).Wait();
      subscriptionContext.StoreEvent(associateProjectASubscriptionEvent1).Wait();

      subscriptionContext.StoreEvent(createProjectASubscriptionEvent2).Wait();
      subscriptionContext.StoreEvent(associateProjectASubscriptionEvent2).Wait();

      geofenceContext.StoreEvent(createProjectAGeofenceProjectTypeEvent).Wait();
      projectContext.StoreEvent(associateProjectAGeofenceProjectTypeEvent).Wait();
      geofenceContext.StoreEvent(createProjectAGeofenceNonProjectTypeEvent).Wait();
      projectContext.StoreEvent(associateProjectAGeofenceNonProjectTypeEvent).Wait();

      var createProjectBEvent = new CreateProjectEvent()
      {
        ProjectUID = projectUidB,
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154))",
        ActionUTC = actionUtc
      };

      var associateCustomerProjectBEvent = new AssociateProjectCustomer()
      { CustomerUID = createCustomerEvent.CustomerUID, ProjectUID = createProjectBEvent.ProjectUID, LegacyCustomerID = 1234, RelationType = RelationType.Customer, ActionUTC = actionUtc };

      var createProjectBGeofenceProjectTypeEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Project.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154))",
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = Guid.NewGuid(),
        ActionUTC = actionUtc
      };

      var associateProjectBGeofenceProjectTypeEvent = new AssociateProjectGeofence()
      {
        ProjectUID = createProjectBEvent.ProjectUID,
        GeofenceUID = createProjectBGeofenceProjectTypeEvent.GeofenceUID,
        ActionUTC = actionUtc.AddDays(1)
      };

      projectContext.StoreEvent(createProjectBEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectBEvent).Wait();

      geofenceContext.StoreEvent(createProjectBGeofenceProjectTypeEvent).Wait();
      projectContext.StoreEvent(associateProjectBGeofenceProjectTypeEvent).Wait();

      var g = projectContext.GetProjectsForCustomer(createCustomerEvent.CustomerUID.ToString());
      g.Wait();
      var projects = g.Result.ToList();
      Assert.IsNotNull(projects, "Unable to retrieve projects from projectRepo");
      Assert.AreEqual(2, projects.Count, "should be 2 projects from projectRepo");

      // can't gaurantee any order as none provided
      if (projectUidA.ToString() == projects[0].ProjectUID)
      {
        CompareProjects(projects[0], createProjectASubscriptionEvent2, createProjectAGeofenceProjectTypeEvent);
      }
      else
      {
        CompareProjects(projects[1], createProjectASubscriptionEvent2, createProjectAGeofenceProjectTypeEvent);
      }

      if (projectUidB.ToString() == projects[0].ProjectUID)
      {
        CompareProjects(projects[0], null, createProjectBGeofenceProjectTypeEvent);
      }
      else
      {
        CompareProjects(projects[1], null, createProjectBGeofenceProjectTypeEvent);
      }
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
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = DateTime.MaxValue,
          ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
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
      Assert.IsNull(projects[0].SubscriptionEndDate, "sub endDate should be null");
      Assert.AreEqual(new DateTime(DateTime.MaxValue.Year, DateTime.MaxValue.Month, DateTime.MaxValue.Day), projects[0].EndDate, "project endDate should be maxValue");
      Assert.IsNull(projects[0].SubscriptionUID, "project endDate should be null");
      Assert.IsNotNull(projects[0].GeometryWKT, "geofence boundar should not be null");
    }

    /// <summary>
    ///  /// <summary>
    /// get projects by legacyProjectID
    /// get subs valid at a date (3 subs here, only 2 valid)
    /// </summary>
    [TestMethod]
    public void GetByLegacyProjectIDAtSubscriptionDate()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";
      int legacyProjectID = new Random().Next(1, 1999999);
      DateTime subscriptionDateToSearch = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent()
      { CustomerUID = Guid.NewGuid(), CustomerName = "The Customer Name", CustomerType = CustomerType.Customer.ToString(), ActionUTC = actionUtc };

      var associateCustomerUser = new AssociateCustomerUserEvent()
      { CustomerUID = createCustomerEvent.CustomerUID, UserUID = Guid.NewGuid(), ActionUTC = actionUtc };

      
      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = legacyProjectID,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),       
        ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
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
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = actionUtc.AddMinutes(20)
      };

      var associateProjectSubscriptionEvent2 = new AssociateProjectSubscriptionEvent()
      {
        SubscriptionUID = createProjectSubscriptionEvent2.SubscriptionUID,
        ProjectUID = createProjectEvent.ProjectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUtc.AddMinutes(22)
      };

      var createProjectSubscriptionEvent3 = new CreateProjectSubscriptionEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = "Project Monitoring",
        StartDate = new DateTime(2016, 12, 01),
        EndDate = new DateTime(2017, 02, 13),
        ActionUTC = actionUtc
      };

      var associateProjectSubscriptionEvent3 = new AssociateProjectSubscriptionEvent()
      {
        SubscriptionUID = createProjectSubscriptionEvent3.SubscriptionUID,
        ProjectUID = createProjectEvent.ProjectUID,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = actionUtc
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      customerContext.StoreEvent(associateCustomerUser).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      subscriptionContext.StoreEvent(createProjectSubscriptionEvent1).Wait();
      subscriptionContext.StoreEvent(associateProjectSubscriptionEvent1).Wait();

      subscriptionContext.StoreEvent(createProjectSubscriptionEvent2).Wait();
      subscriptionContext.StoreEvent(associateProjectSubscriptionEvent2).Wait();

      subscriptionContext.StoreEvent(createProjectSubscriptionEvent3).Wait();
      subscriptionContext.StoreEvent(associateProjectSubscriptionEvent3).Wait();

      var g = projectContext.GetProjectAndSubscriptions(legacyProjectID, subscriptionDateToSearch); g.Wait();
      var projects = g.Result.ToList();
      Assert.IsNotNull(projects, "Unable to retrieve 1 project/sub from projectRepo");
      Assert.AreEqual(2, projects.Count, "should be 1 project/sub from projectRepo");
      if (projects[0].ServiceTypeID == 19)
      {
        Assert.AreEqual(19, projects[0].ServiceTypeID, "Landfill should be the most recent service from projectRepo");
        Assert.AreEqual(createProjectSubscriptionEvent2.EndDate, projects[0].SubscriptionEndDate, "Landfill EndDate incorrect from projectRepo");
        Assert.AreEqual(createProjectEvent.ProjectBoundary, projects[0].GeometryWKT, "Landfill Geometry incorrect from projectRepo");
        Assert.AreEqual(20, projects[1].ServiceTypeID, "PM should be the most recent service from projectRepo");
        Assert.AreEqual(createProjectSubscriptionEvent3.EndDate, projects[1].SubscriptionEndDate, "PM EndDate incorrect from projectRepo");
      }
      else        
      {
        Assert.AreEqual(20, projects[0].ServiceTypeID, "PM should be the most recent service from projectRepo");
        Assert.AreEqual(createProjectSubscriptionEvent3.EndDate, projects[0].SubscriptionEndDate, "PM EndDate incorrect from projectRepo");
        Assert.AreEqual(createProjectEvent.ProjectBoundary, projects[0].GeometryWKT, "PM Geometry incorrect from projectRepo");
        Assert.AreEqual(19, projects[1].ServiceTypeID, "Landfill should be the most recent service from projectRepo");
        Assert.AreEqual(createProjectSubscriptionEvent2.EndDate, projects[1].SubscriptionEndDate, "Landfill EndDate incorrect from projectRepo");

      }
    }


    /// </summary>
    /// <param name="subscriptionRepo"></param>
    /// <param name="subscription"></param>
    /// <returns></returns>
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
        ProjectBoundary = project.GeometryWKT,
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
        GeometryWKT = kafkaProjectEvent.ProjectBoundary,
        LastActionedUTC = kafkaProjectEvent.ActionUTC,
        StartDate = kafkaProjectEvent.ProjectStartDate,
        EndDate = kafkaProjectEvent.ProjectEndDate
      };
    }

    private void CompareProjects(Project returnedProject, CreateProjectSubscriptionEvent createProjectSubscriptionEvent, CreateGeofenceEvent createGeofenceEvent)
    {
      if (createProjectSubscriptionEvent == null)
      {
        Assert.IsNull(returnedProject.SubscriptionUID, "Incorrect subscriptionUID");
        Assert.IsNull(returnedProject.SubscriptionEndDate, "Incorrect endDate");
      }
      else
      {
        Assert.AreEqual(createProjectSubscriptionEvent.SubscriptionUID.ToString(), returnedProject.SubscriptionUID, "Incorrect subscriptionUID");
        Assert.AreEqual(createProjectSubscriptionEvent.EndDate, returnedProject.SubscriptionEndDate, "Incorrect endDate");
      }

      Assert.AreEqual(createGeofenceEvent.GeometryWKT, returnedProject.GeometryWKT, "Incorrect geometry");
    }
    #endregion

  }
}
 
 