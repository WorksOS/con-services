using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.Project.Data;
using VSS.Project.Data.Models;
using VSS.Customer.Data;
using Microsoft.Extensions.Configuration;
using log4netExtensions;
using VSS.GenericConfiguration;

namespace RepositoryTests
{
  [TestClass]
  public class ProjectRepositoryTests
  {
    IServiceProvider serviceProvider = null;
    CustomerRepository customerContext = null;
    ProjectRepository projectContext = null;

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
        .AddLogging()
        .AddSingleton<ILoggerFactory>(loggerFactory)
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .BuildServiceProvider();

      customerContext = new CustomerRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
      projectContext = new ProjectRepository(serviceProvider.GetService<IConfigurationStore>(), serviceProvider.GetService<ILoggerFactory>());
    }


    #region Projects

    /// <summary>
    /// Create Project - Happy path i.e. 
    ///   customer and CustomerProject relationship also added
    ///   project doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateProjectWithCustomer_HappyPath()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUTC,
        ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUTC
      };

      var g = projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      Project project = CopyModel(createProjectEvent);
      g = projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
    }

    /// <summary>
    /// Create Project - Happy path i.e. 
    ///   customer and CustomerProject relationship also added
    ///   project doesn't exist already.
    ///   ProjectCustomer is inserted first, then Project, then Customer
    /// </summary>
    [TestMethod]
    public void CreateProjectWithCustomer_HappyPathButOutOfOrder()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = ActionUTC
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = ActionUTC
      };

      var g = projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      Project project = CopyModel(createProjectEvent);
      g = projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
    }


    /// <summary>
    /// Create Project - RelationShips not setup i.e. 
    ///   customer and CustomerProject relationship NOT added
    ///   project doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateProject_NoCustomer()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";
     
      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = ActionUTC
      };

      var g = projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      Project project = CopyModel(createProjectEvent);
      g = projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
      Assert.AreEqual("Pacific/Auckland", g.Result.LandfillTimeZone, "Project landfill timeZone is incorrect from ProjectRepo");

      // should fail as there is no Customer or CustProject
      g = projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project should not be available from ProjectRepo");
    }

    /// <summary>
    /// Create Project - Project already exists
    ///   customer and CustomerProject relationship also added
    ///   project exists but is different.
    /// </summary>
    [TestMethod]
    public void CreateProjectWithCustomer_ProjectExists()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
          ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
          ActionUTC = ActionUTC
      };          

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = ActionUTC
      };

      var g = projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      createProjectEvent.ActionUTC = createProjectEvent.ActionUTC.AddMinutes(-2);
      projectContext.StoreEvent(createProjectEvent).Wait();

      Project project = CopyModel(createProjectEvent);
      project.LastActionedUTC = ActionUTC;
      g = projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
    }

    /// <summary>
    /// Create Project  - Project already exists, but it's a 'dummy'?
    ///   customer and CustomerProject relationship also added
    ///   project exists but is different.
    /// </summary>
    [TestMethod]
    public void CreateProjectWithCustomer_ProjectExistsButIsDummy()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
          ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
          ActionUTC = ActionUTC
      };

      var createProjectEventEarlier = new CreateProjectEvent()
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectID = createProjectEvent.ProjectID,
        ProjectName = "has the Project Name changed",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,

        ProjectStartDate = createProjectEvent.ProjectStartDate,
        ProjectEndDate = createProjectEvent.ProjectEndDate,
          ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
          ActionUTC = createProjectEvent.ActionUTC.AddDays(-1)
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = ActionUTC
      };

      var g = projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = projectContext.StoreEvent(createProjectEventEarlier);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Earlier Project event should have been written");

      Project project = CopyModel(createProjectEventEarlier);
      g = projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
    }


    /// <summary>
    /// Update Project - Happy path i.e. 
    ///   customer and CustomerProject relationship also added
    ///   project exists and New ActionUTC is later than its LastActionUTC.
    /// </summary>
    [TestMethod]
    public void UpdateProject_HappyPath()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
          ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
          ActionUTC = ActionUTC
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = ActionUTC
      };

      var updateProjectEvent = new UpdateProjectEvent()
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The NEW Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,

        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        ActionUTC = ActionUTC.AddHours(1)
      };

      var g = projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = projectContext.StoreEvent(updateProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not updated");

      Project project = CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.Name = updateProjectEvent.ProjectName;
      project.EndDate = updateProjectEvent.ProjectEndDate;
      project.LastActionedUTC = updateProjectEvent.ActionUTC;
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      g = projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
    }

    /// <summary>
    /// Update Project - earlier ActionUTC 
    ///   customer and CustomerProject relationship also added
    ///   project exists and New ActionUTC is earlier than its LastActionUTC.
    /// </summary>
    [TestMethod]
    public void UpdateProject_OldUpdate()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
          ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
          ActionUTC = ActionUTC
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = ActionUTC
      };

      var updateProjectEvent = new UpdateProjectEvent()
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The NEW Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,

        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        ActionUTC = ActionUTC.AddHours(-1)
      };

      var g = projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = projectContext.StoreEvent(updateProjectEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Project event not updated");

      Project project = CopyModel(createProjectEvent);
      g = projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
    }

    /// <summary>
    /// Delete Project - Happy path i.e. 
    ///   customer and CustomerProject relationship also added
    ///   project exists and New ActionUTC is later than its LastActionUTC.
    /// </summary>
    [TestMethod]
    public void DeleteProject_HappyPath()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
          ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
          ActionUTC = ActionUTC
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = ActionUTC
      };

      var deleteProjectEvent = new DeleteProjectEvent()
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ActionUTC = ActionUTC.AddHours(1)
      };

      var g = projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = projectContext.StoreEvent(deleteProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not deleted");

      Project project = CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.IsDeleted = true;
      project.LastActionedUTC = deleteProjectEvent.ActionUTC;
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      g = projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve Project from ProjectRepo");

      // this one ignores the IsDeleted flag in DB
      g = projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
    }

    /// <summary>
    /// Delete Project - ActionUTC too old
    ///   customer and CustomerProject relationship also added
    ///   project exists and New ActionUTC is later than its LastActionUTC.
    /// </summary>
    [TestMethod]
    public void DeleteProject_OldActionUTC()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
          ProjectBoundary = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
          ActionUTC = ActionUTC
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = ActionUTC
      };

      var deleteProjectEvent = new DeleteProjectEvent()
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ActionUTC = ActionUTC.AddHours(-1)
      };

      var g = projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = projectContext.StoreEvent(deleteProjectEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Project event should not be deleted");

      Project project = CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      g = projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
    }
    #endregion


    #region AssociateProjectWithCustomer

    /// <summary>
    /// Associate Customer Project - Happy Path
    ///   customer, CustomerProject and project added.
    ///   CustomerProject legacyCustomerID updated and ActionUTC is later
    /// </summary>
    [TestMethod]
    public void AssociateProjectWithCustomer_HappyPath()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = ActionUTC
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = ActionUTC
      };

      var updateAssociateCustomerProjectEvent = new AssociateProjectCustomer()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 999,
        RelationType = RelationType.Customer,
        ActionUTC = ActionUTC.AddDays(1)
      };

      var g = projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = projectContext.StoreEvent(updateAssociateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "CustomerProject not updated");

      Project project = CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LegacyCustomerID = updateAssociateCustomerProjectEvent.LegacyCustomerID;
      g = projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
    }

    /// <summary>
    /// Associate Customer Project - update not applied as it's late
    ///   customer, CustomerProject and project added.
    ///   CustomerProject legacyCustomerID updated but ActionUTC is earlier
    /// </summary>
    [TestMethod]
    public void AssociateProjectWithCustomer_ChangeIsEarlier()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = ActionUTC
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = ActionUTC
      };

      var updateAssociateCustomerProjectEvent = new AssociateProjectCustomer()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 999,
        RelationType = RelationType.Customer,
        ActionUTC = ActionUTC.AddDays(-1)
      };

      projectContext.StoreEvent(createProjectEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();
      projectContext.StoreEvent(associateCustomerProjectEvent).Wait();
      projectContext.StoreEvent(updateAssociateCustomerProjectEvent).Wait();

      Project project = CopyModel(createProjectEvent);
      var g = projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
    }

    #endregion


    #region timezones
    /// <summary>
    /// These Timezone conversions need to be done as acceptance tests so they are run on linux - the target platform.
    /// They should behave ok on windows (this will occur if you run a/ts against local containers).
    /// </summary>
    [TestMethod]
    public void ConvertTimezone_WindowsToIana()
    {
      var projectTimeZone = "New Zealand Standard Time";
      Assert.AreEqual("Pacific/Auckland", TimeZone.WindowsToIana(projectTimeZone), "Unable to convert WindowsToIana");
    }

    [TestMethod]
    public void ConvertTimezone_WindowsToIana_Invalid()
    {
      var projectTimeZone = "New Zealand Standard Time222";
      Assert.AreEqual("", TimeZone.WindowsToIana(projectTimeZone), "Should not be able to convert WindowsToIana");
    }

    [TestMethod]
    public void ConvertTimezone_WindowsToIana_alreadyIana()
    {
      var projectTimeZone = "Pacific/Auckland";
      Assert.AreEqual("", TimeZone.WindowsToIana(projectTimeZone), "Should not be able to convert WindowsToIana");
    }

    [TestMethod]
    public void ConvertTimezone_WindowsToIana_UTC()
    {
      var projectTimeZone = "UTC";
      Assert.AreEqual("Etc/UTC", TimeZone.WindowsToIana(projectTimeZone), "Unable to convert WindowsToIana");
    }
    #endregion


    #region private
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
        ActionUTC = project.LastActionedUTC,
        ProjectBoundary = project.GeometryWKT
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
        EndDate = kafkaProjectEvent.ProjectEndDate,
        GeometryWKT = kafkaProjectEvent.ProjectBoundary
      };
    }
    #endregion

  }
}
 
 