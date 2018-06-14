using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryTests.Internal;
using System;
using System.Linq;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace RepositoryTests.ProjectRepositoryTests
{
  [TestClass]
  public class ProjectRepositoryTests : TestControllerBase
  {
    CustomerRepository _customerContext;
    ProjectRepository _projectContext;
    GeofenceRepository _geofenceContext;

    [TestInitialize]
    public void Init()
    {
      SetupLogging();

      _customerContext = new CustomerRepository(ServiceProvider.GetService<IConfigurationStore>(), ServiceProvider.GetService<ILoggerFactory>());
      _projectContext = new ProjectRepository(ServiceProvider.GetService<IConfigurationStore>(), ServiceProvider.GetService<ILoggerFactory>());
      _geofenceContext = new GeofenceRepository(ServiceProvider.GetService<IConfigurationStore>(), ServiceProvider.GetService<ILoggerFactory>());
      new SubscriptionRepository(ServiceProvider.GetService<IConfigurationStore>(), ServiceProvider.GetService<ILoggerFactory>());
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
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        Description = "the Description",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc,
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        CoordinateSystemFileContent = new byte[] { 0, 1, 2, 3, 4 },
        CoordinateSystemFileName = "thisLocation\\this.cs"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.CoordinateSystemLastActionedUTC = createProjectEvent.ActionUTC;
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project.IsDeleted = false;
      g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 1);
    }

    /// <summary>
    /// Create Project - Happy path i.e. 
    ///   customer and CustomerProject relationship also added
    ///   project doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateProjectWithCustomer_HappyPath_Unicode()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name(株)城内組　二見地区築堤護岸工事",
        Description = "the Description(株)城内組　二見地区築堤護岸工事",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc,
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        CoordinateSystemFileContent = new byte[] { 0, 1, 2, 3, 4 },
        CoordinateSystemFileName = "thisLocation\\this.cs"
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.CoordinateSystemLastActionedUTC = createProjectEvent.ActionUTC;
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project.IsDeleted = false;
      g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 1);
    }

    /// <summary>
    /// Create Project - Happy path i.e. 
    ///   customer and CustomerProject relationship also added
    ///   project doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateProjectWithCustomer_HappyPath_NoLegacyProjectId()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 0,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc,
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))"
      };


      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      var g = _projectContext.GetProjectOnly(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.IsTrue(g.Result.LegacyProjectID >= 2000000,
        "Project legacyProjectId is incorrect. Actual LegacyProjectID = {0}, should be >2m", g.Result.LegacyProjectID);
      project.LegacyProjectID = g.Result.LegacyProjectID;
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo.");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 1);
    }

    /// <summary>
    /// Create Project - Happy path i.e. 
    ///   customer and CustomerProject relationship also added
    ///   project doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateProjectWithCustomer_HappyPath_DuplicateLegacyProjectId()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectEvent1 = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc,
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))"
      };

      var createProjectEvent2 = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = createProjectEvent1.ProjectID,
        ProjectName = "The Project Name 2",
        ProjectType = createProjectEvent1.ProjectType,
        ProjectTimezone = createProjectEvent1.ProjectTimezone,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = actionUtc,
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))"
      };


      var s1 = _projectContext.StoreEvent(createProjectEvent1);
      s1.Wait();
      Assert.AreEqual(1, s1.Result, "Project event 1 not written");

      var s2 = _projectContext.StoreEvent(createProjectEvent2);
      s2.Wait();
      Assert.AreEqual(0, s2.Result, "Project event should not have been written");

      CheckProjectHistoryCount(createProjectEvent1.ProjectUID.ToString(), 1);
      CheckProjectHistoryCount(createProjectEvent2.ProjectUID.ToString(), 0);
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
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,
        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project.IsDeleted = false;
      g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 1);
    }

    /// <summary>
    /// Create Project - RelationShips not setup i.e. 
    ///   customer and CustomerProject relationship NOT added
    ///   project doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateProject_NoCustomer()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
      Assert.AreEqual(ProjectTimezones.PacificAuckland, g.Result.LandfillTimeZone,
        "Project landfill timeZone is incorrect from ProjectRepo");

      // should fail as there is no Customer or CustProject
      g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project should not be available from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 1);
    }

    /// <summary>
    /// Create Project - Project already exists
    ///   customer and CustomerProject relationship also added
    ///   project exists but is different.
    /// </summary>
    [TestMethod]
    public void CreateProjectWithCustomer_ProjectExists()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      createProjectEvent.ActionUTC = createProjectEvent.ActionUTC.AddMinutes(-2);
      _projectContext.StoreEvent(createProjectEvent).Wait();

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.LastActionedUTC = actionUtc;
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project.IsDeleted = false;
      g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 2);
    }

    /// <summary>
    /// Create Project  - Project already exists, but it's a 'dummy'?
    ///   customer and CustomerProject relationship also added
    ///   project exists but is different.
    /// </summary>
    [TestMethod]
    public void CreateProjectWithCustomer_ProjectExistsButIsDummy()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var createProjectEventEarlier = new CreateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectID = createProjectEvent.ProjectID,
        ProjectName = "has the Project Name changed",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,

        ProjectStartDate = createProjectEvent.ProjectStartDate,
        ProjectEndDate = createProjectEvent.ProjectEndDate,
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = createProjectEvent.ActionUTC.AddDays(-1)
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not created");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _projectContext.StoreEvent(createProjectEventEarlier);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Earlier Project event should have been written");

      Project project = TestHelpers.CopyModel(createProjectEventEarlier);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project.IsDeleted = false;
      project.LastActionedUTC = createProjectEvent.ActionUTC;
      project.Name = createProjectEventEarlier.ProjectName;
      var g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 2);
    }

    /// <summary>
    /// Update Project - Happy path i.e. 
    ///   customer and CustomerProject relationship also added
    ///   project exists and New ActionUTC is later than its LastActionUTC.
    /// </summary>
    [TestMethod]
    public void UpdateProject_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",

        CoordinateSystemFileContent = new byte[] { 0, 1, 2, 3, 4 },
        CoordinateSystemFileName = "thisLocation\\this.cs",
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var updateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The NEW Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,

        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        CoordinateSystemFileName = "thatLocation\\that.cs",
        ActionUTC = actionUtc.AddHours(1)
      };

      var g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _projectContext.StoreEvent(updateProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not updated");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.Name = updateProjectEvent.ProjectName;
      project.StartDate = createProjectEvent.ProjectStartDate;
      project.EndDate = updateProjectEvent.ProjectEndDate;
      project.LandfillTimeZone = ProjectTimezones.PacificAuckland;
      project.LastActionedUTC = updateProjectEvent.ActionUTC;
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project.CoordinateSystemFileName = updateProjectEvent.CoordinateSystemFileName;
      project.CoordinateSystemLastActionedUTC = updateProjectEvent.ActionUTC;
      project.GeometryWKT = createProjectEvent.ProjectBoundary;
      g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 2);
    }

    /// <summary>
    /// Null sent in Update event to be ignored
    /// </summary>
    [TestMethod]
    public void UpdateProject_IgnoreNulls()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var updateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectType = createProjectEvent.ProjectType,
        ProjectEndDate = createProjectEvent.ProjectEndDate,
        ActionUTC = actionUtc.AddHours(1)
      };

      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      _customerContext.StoreEvent(createCustomerEvent).Wait();
      _projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      s = _projectContext.StoreEvent(updateProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not updated");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LastActionedUTC = updateProjectEvent.ActionUTC;
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      var g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 2);
    }

    /// <summary>
    /// Update Project - earlier ActionUTC 
    ///   customer and CustomerProject relationship also added
    ///   project exists and New ActionUTC is earlier than its LastActionUTC.
    /// </summary>
    [TestMethod]
    public void UpdateProject_OldUpdate()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var updateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The NEW Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,

        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        ActionUTC = actionUtc.AddHours(-1)
      };

      var g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _projectContext.StoreEvent(updateProjectEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Project event not updated");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project.IsDeleted = false;
      g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 1);
    }

    /// <summary>
    /// Update Project - then the Create is applied
    /// </summary>
    [TestMethod]
    public void UpdateProject_ThenCreateEvent()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        CustomerUID = createCustomerEvent.CustomerUID,
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var updateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The NEW Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,

        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        ActionUTC = actionUtc.AddHours(1)
      };

      var s = _projectContext.StoreEvent(updateProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project update event not inserted");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not created");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project.IsDeleted = false;
      project.Name = updateProjectEvent.ProjectName;
      project.EndDate = updateProjectEvent.ProjectEndDate;
      project.LastActionedUTC = updateProjectEvent.ActionUTC;
      var g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      project.LegacyProjectID = g.Result.LegacyProjectID;
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 2);
    }

    /// <summary>
    /// Update Project - then the Create is applied
    /// </summary>
    [TestMethod]
    public void UpdateProject_ThenCreateEvent_WhereUpdateMissingTimeZone()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        CustomerUID = createCustomerEvent.CustomerUID,
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var updateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The NEW Project Name",
        ProjectType = createProjectEvent.ProjectType,
       
        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        ActionUTC = actionUtc.AddHours(1)
      };

      var s = _projectContext.StoreEvent(updateProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project update event not inserted");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not created");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project.IsDeleted = false;
      project.Name = updateProjectEvent.ProjectName;
      project.EndDate = updateProjectEvent.ProjectEndDate;
      project.LastActionedUTC = updateProjectEvent.ActionUTC;
      project.LegacyProjectID = createProjectEvent.ProjectID;
      var g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
    }

    /// <summary>
    /// Update Project - Update projectBoundary
    ///    Updates to new boundary.
    ///    Then update includes empty String - i.e. don't update. 
    /// </summary>
    [TestMethod]
    public void UpdateProject_HappyPath_UpdatesBoundary()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      string originalProjectBoundary =
        "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      string updatedProjectBoundary =
        "POLYGON((-121 38,-121 38,-121 38,-121 38,-121 38,-121 38))";
      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary = originalProjectBoundary,
        CoordinateSystemFileContent = new byte[] { 0, 1, 2, 3, 4 },
        CoordinateSystemFileName = "thisLocation\\this.cs",
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var firstUpdateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The FirstUpdated Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,
        ProjectBoundary = updatedProjectBoundary,

        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        CoordinateSystemFileName = "thatLocation\\that.cs",
        ActionUTC = actionUtc.AddHours(1)
      };

      var secondUpdateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The SecondUpdated Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,
        ProjectBoundary = string.Empty,

        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        CoordinateSystemFileName = "thatLocation\\that.cs",
        ActionUTC = actionUtc.AddHours(2)
      };

      var thirdUpdateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The ThirdUpdated Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,
        ProjectBoundary = null,

        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        CoordinateSystemFileName = "thatLocation\\that.cs",
        ActionUTC = actionUtc.AddHours(3)
      };

      var g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _projectContext.StoreEvent(firstUpdateProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not updated");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.Name = firstUpdateProjectEvent.ProjectName;
      project.EndDate = firstUpdateProjectEvent.ProjectEndDate;
      project.LastActionedUTC = firstUpdateProjectEvent.ActionUTC;
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project.CoordinateSystemFileName = firstUpdateProjectEvent.CoordinateSystemFileName;
      project.CoordinateSystemLastActionedUTC = firstUpdateProjectEvent.ActionUTC;
      project.GeometryWKT = firstUpdateProjectEvent.ProjectBoundary;
      g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
      Assert.AreEqual(firstUpdateProjectEvent.ProjectBoundary, g.Result.GeometryWKT, "Project boundary firstUpdate is incorrect from ProjectRepo");

      s = _projectContext.StoreEvent(secondUpdateProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not updated");

      g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(firstUpdateProjectEvent.ProjectBoundary, g.Result.GeometryWKT, "Project boundary secondUpdate is incorrect from ProjectRepo");

      s = _projectContext.StoreEvent(thirdUpdateProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not updated");

      g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(firstUpdateProjectEvent.ProjectBoundary, g.Result.GeometryWKT, "Project boundary thirdUpdate is incorrect from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 4);
    }


    /// <summary>
    /// Delete Project - then the Create is applied
    /// </summary>
    [TestMethod]
    public void DeleteProject_ThenCreateEvent()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        CustomerUID = createCustomerEvent.CustomerUID,
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var deleteProjectEvent = new DeleteProjectEvent()
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ActionUTC = actionUtc.AddHours(1)
      };

      var s = _projectContext.StoreEvent(deleteProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not created as a dummy");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not created");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project.IsDeleted = true;
      project.Name = createProjectEvent.ProjectName;
      project.LastActionedUTC = deleteProjectEvent.ActionUTC;
      var g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      project.LegacyProjectID = g.Result.LegacyProjectID;
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 2);
    }

    /// <summary>
    /// Delete Project - Happy path i.e. 
    ///   customer and CustomerProject relationship also added
    ///   project exists and New ActionUTC is later than its LastActionUTC.
    /// </summary>
    [TestMethod]
    public void DeleteProject_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var deleteProjectEvent = new DeleteProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ActionUTC = actionUtc.AddHours(1)
      };

      var g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _projectContext.StoreEvent(deleteProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not deleted");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.IsDeleted = true;
      project.LastActionedUTC = deleteProjectEvent.ActionUTC;
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Should not be able to retrieve Project from ProjectRepo");

      // this one ignores the IsDeleted flag in DB
      g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 2);
    }

    /// <summary>
    /// Delete Project permanentlhy 
    /// </summary>
    [TestMethod]
    public void DeleteProject_Permanently()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var deleteProjectEvent = new DeleteProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        DeletePermanently = true,
        ActionUTC = actionUtc.AddHours(1)
      };

      _projectContext.StoreEvent(createProjectEvent).Wait();
      _customerContext.StoreEvent(createCustomerEvent).Wait();
      _projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      // this one ignores the IsDeleted flag in DB
      var g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.IsFalse(g.Result.IsDeleted, "Project details are incorrect from ProjectRepo");

      var s = _projectContext.StoreEvent(deleteProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not deleted");

      g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Unable to retrieve Project from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 1);
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

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = ActionUTC
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = ActionUTC
      };

      var deleteProjectEvent = new DeleteProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ActionUTC = ActionUTC.AddHours(-1)
      };

      var g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _projectContext.StoreEvent(deleteProjectEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Project event should not be deleted");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");

      CheckProjectHistoryCount(createProjectEvent.ProjectUID.ToString(), 1);
    }

    #endregion Projects


    #region AssociateProjectWithCustomer

    /// <summary>
    /// Associate Customer Project - Happy Path
    ///   customer, CustomerProject and project added.
    ///   CustomerProject legacyCustomerID updated and ActionUTC is later
    /// </summary>
    [TestMethod]
    public void AssociateProjectWithCustomer_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var updateAssociateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 999,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc.AddDays(1)
      };

      var g = _projectContext.GetProject_UnitTest(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNull(g.Result, "Project shouldn't be there yet");

      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _projectContext.StoreEvent(updateAssociateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "CustomerProject not updated");

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LegacyCustomerID = updateAssociateCustomerProjectEvent.LegacyCustomerID;
      g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
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
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",

        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var updateAssociateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 999,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc.AddDays(-1)
      };

      _projectContext.StoreEvent(createProjectEvent).Wait();
      _customerContext.StoreEvent(createCustomerEvent).Wait();
      _projectContext.StoreEvent(associateCustomerProjectEvent).Wait();
      _projectContext.StoreEvent(updateAssociateCustomerProjectEvent).Wait();

      Project project = TestHelpers.CopyModel(createProjectEvent);
      project.CustomerUID = createCustomerEvent.CustomerUID.ToString();
      project.LegacyCustomerID = associateCustomerProjectEvent.LegacyCustomerID;
      project.IsDeleted = false;
      var g = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Project from ProjectRepo");
      Assert.AreEqual(project, g.Result, "Project details are incorrect from ProjectRepo");
    }

    /// <summary>
    /// Associate Customer Project - Happy Path
    ///   then Dissassociate - should be removed
    /// </summary>
    [TestMethod]
    public void AssociateProjectWithCustomer_ThenDissociate_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUtc
      };

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = actionUtc
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };

      var dissociateCustomerProjectEvent = new DissociateProjectCustomer
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        ActionUTC = actionUtc
      };


      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      s = _customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = _projectContext.StoreEvent(associateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "CustomerProject not associated");

      var g = _projectContext.GetProjectsForCustomer(createCustomerEvent.CustomerUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Projects for Customer");
      Assert.AreEqual(1, g.Result.Count(), "Project count is incorrect for Customer");

      s = _projectContext.StoreEvent(dissociateCustomerProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "CustomerProject not disassociated");

      g = _projectContext.GetProjectsForCustomer(createCustomerEvent.CustomerUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Projects for Customer after Dissassociation");
      Assert.AreEqual(0, g.Result.Count(), "Project count is incorrect for Customer after Dissassociation");
    }

    #endregion

    #region AssociateProjectWithGeofence

    /// <summary>
    /// Associate Project with Geofence - Happy Path
    /// </summary>
    [TestMethod]
    public void AssociateProjectWithGeofence_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();
      var boundary =
        "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))";

      var createProjectEvent = new CreateProjectEvent
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 1999999),
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = ProjectTimezones.NewZealandStandardTime,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ProjectBoundary = boundary,
        ActionUTC = actionUtc
      };

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Project.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = boundary,
        CustomerUID = customerUid,
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var associateProjectGeofence = new AssociateProjectGeofence
      {
        ProjectUID = createProjectEvent.ProjectUID,
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        ReceivedUTC = actionUtc,
        ActionUTC = actionUtc
      };

      var geo = _geofenceContext.StoreEvent(createGeofenceEvent);
      geo.Wait();
      Assert.AreEqual(1, geo.Result, "Unable to store geofence");


      var p = _projectContext.StoreEvent(createProjectEvent);
      p.Wait();
      Assert.AreEqual(1, p.Result, "Project event not written");

      var pg = _projectContext.StoreEvent(associateProjectGeofence);
      pg.Wait();
      Assert.AreEqual(1, pg.Result, "ProjectGeofence event not written");
      
      var g = _projectContext.GetAssociatedGeofences(createProjectEvent.ProjectUID.ToString());
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve ProjectGeofence from ProjectRepo");
      Assert.AreEqual(1, g.Result.Count(), "Invalid ProjectGeofence Count");
      var retrievedPg = g.Result.FirstOrDefault();
      Assert.IsNotNull(retrievedPg, "List is missing from ProjectGeofence");
      Assert.AreEqual(createProjectEvent.ProjectUID.ToString(), retrievedPg.ProjectUID, "ProjectUid is incorrect from ProjectGeofence");
      Assert.AreEqual(createGeofenceEvent.GeofenceUID.ToString(), retrievedPg.GeofenceUID, "GeofenceUID is incorrect from ProjectGeofence");
      Assert.AreEqual(createGeofenceEvent.GeofenceType, retrievedPg.GeofenceType.ToString(), "GeofenceType is incorrect from ProjectGeofence");
    }

    /// <summary>
    /// Associate Project Geofence - then dissociate it
    /// </summary>
    [TestMethod]
    public void DissociateProjectGeofence_HappyPath()
    {
      DateTime actionUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();
      var projectUid = Guid.NewGuid();

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Borrow.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
        UserUID = Guid.NewGuid(),
        AreaSqMeters = 123.456,
        ActionUTC = actionUtc
      };

      var associateProjectGeofenceEvent = new AssociateProjectGeofence()
      {
        ProjectUID = projectUid,
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        ActionUTC = actionUtc.AddDays(1)
      };

      _geofenceContext.StoreEvent(createGeofenceEvent).Wait();
      _projectContext.StoreEvent(associateProjectGeofenceEvent).Wait();

      var p = _projectContext.GetAssociatedGeofences(projectUid.ToString());
      p.Wait();
      var projectGeofences = p.Result.ToList();
      Assert.AreEqual(1, projectGeofences.Count, "Wrong number of ProjectGeofences");
      Assert.AreEqual(createGeofenceEvent.GeofenceUID.ToString(), projectGeofences[0].GeofenceUID, "Wrong project geofence returned");

      var dissociateProjectGeofenceEvent = new DissociateProjectGeofence()
      {
        ProjectUID = projectUid,
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        ActionUTC = actionUtc.AddDays(2)
      };

      _projectContext.StoreEvent(dissociateProjectGeofenceEvent).Wait();

      p = _projectContext.GetAssociatedGeofences(projectUid.ToString());
      p.Wait();
      projectGeofences = p.Result.ToList();
      Assert.AreEqual(0, projectGeofences.Count, "Wrong number of ProjectGeofences. There should not be any");
    }

    #endregion AssociateProjectWithGeofence


    #region private
    private void CheckProjectHistoryCount(string projectUid, int expectedCount)
    {
      var projectHistory = _projectContext.GetProjectHistory(projectUid);
      projectHistory.Wait();
      Assert.IsNotNull(projectHistory.Result, "Unable to retrieve ProjectHistory");
      Assert.AreEqual(expectedCount, projectHistory.Result.Count(), "ProjectHistory count incorrect");
    }
    #endregion private
  }
}