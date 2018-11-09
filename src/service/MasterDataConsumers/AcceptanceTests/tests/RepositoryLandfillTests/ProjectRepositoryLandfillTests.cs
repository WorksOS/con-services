using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using RepositoryLandfillTests;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ProjectRepositoryLandfillTests
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
    }

    #region LandfillProjects
    
    /// <summary>
    /// Create Project on a 'Landfill-type service environment' - Happy path
    ///    environment variable is set to enable local create of Geofence and PG
    /// </summary>
    [TestMethod]
    public void CreateLandfillProject_HappyPath()
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
        ProjectTimezone = "New Zealand Standard Time",
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

      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      _customerContext.StoreEvent(createCustomerEvent).Wait();
      _projectContext.StoreEvent(associateCustomerProjectEvent).Wait();
      
      var p = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      p.Wait();
      Assert.IsNotNull(p.Result, "Unable to retrieve Project");
      Assert.AreEqual(createProjectEvent.ProjectUID.ToString(), p.Result.ProjectUID, "Project details are incorrect");

      var pg = _projectContext.GetAssociatedGeofences(createProjectEvent.ProjectUID.ToString());
      pg.Wait();
      Assert.IsNotNull(pg.Result, "Unable to retrieve ProjectGeofences");
      var projectGeofenceList = pg.Result.ToList();
      Assert.AreEqual(1, projectGeofenceList.Count(), "Geofence count is incorrect");
      Assert.AreEqual(createProjectEvent.ProjectUID.ToString(), projectGeofenceList[0].ProjectUID, "ProjectGeofence Uid is incorrect");
      Assert.AreEqual(GeofenceType.Project, projectGeofenceList[0].GeofenceType, "ProjectGeofence type is incorrect");

      var g = _geofenceContext.GetGeofence(projectGeofenceList[0].GeofenceUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Geofence");
      Assert.AreEqual(createProjectEvent.ProjectBoundary, g.Result.GeometryWKT, "GeofenceWKT is incorrect");
    }

    /// <summary>
    /// Create Project on a 'Landfill-type service environment'
    ///    ProjectGeofence and Geofence already exist, should update Geoefence only. 
    /// </summary>
    [TestMethod]
    public void CreateLandfillProject_ProjectGeofenceAlreadyExists()
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
        ProjectTimezone = "New Zealand Standard Time",
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

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = GeofenceType.Project.ToString(),
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT = createProjectEvent.ProjectBoundary,
        CustomerUID = createCustomerEvent.CustomerUID,
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

      _geofenceContext.StoreEvent(createGeofenceEvent).Wait();
      _projectContext.StoreEvent(associateProjectGeofence).Wait();

      var s = _projectContext.StoreEvent(createProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not written");

      _customerContext.StoreEvent(createCustomerEvent).Wait();
      _projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      var p = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      p.Wait();
      Assert.IsNotNull(p.Result, "Unable to retrieve Project");
      Assert.AreEqual(createProjectEvent.ProjectUID.ToString(), p.Result.ProjectUID, "Project details are incorrect");

      var pg = _projectContext.GetAssociatedGeofences(createProjectEvent.ProjectUID.ToString());
      pg.Wait();
      Assert.IsNotNull(pg.Result, "Unable to retrieve ProjectGeofences");
      var projectGeofenceList = pg.Result.ToList();
      Assert.AreEqual(1, projectGeofenceList.Count(), "Geofence count is incorrect");
      Assert.AreEqual(createProjectEvent.ProjectUID.ToString(), projectGeofenceList[0].ProjectUID, "ProjectGeofence Uid is incorrect");
      Assert.AreEqual(GeofenceType.Project, projectGeofenceList[0].GeofenceType, "ProjectGeofence type is incorrect");

      var g = _geofenceContext.GetGeofence(projectGeofenceList[0].GeofenceUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Geofence");
      Assert.AreEqual(createProjectEvent.ProjectBoundary, g.Result.GeometryWKT, "GeofenceWKT is incorrect");
    }

    /// <summary>
    /// Create Project on a 'Landfill-type service environment' - Happy path
    ///    environment variable is set to enable local create of Geofence and PG
    /// </summary>
    [TestMethod]
    public void UpdateLandfillProject_HappyPath()
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
        ProjectTimezone = "New Zealand Standard Time",
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

      _projectContext.StoreEvent(createProjectEvent).Wait();
      _customerContext.StoreEvent(createCustomerEvent).Wait();
      _projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      var updateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The NEW Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,
        ProjectBoundary = "POLYGON((94.34 3.8,95.3 3.2,95.8 3.1,94.34 3.8))",
        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        CoordinateSystemFileName = "thatLocation\\that.cs",
        ActionUTC = actionUtc.AddHours(1)
      };

      var s = _projectContext.StoreEvent(updateProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not updated");

      var p = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      p.Wait();
      Assert.IsNotNull(p.Result, "Unable to retrieve Project");
      Assert.AreEqual(createProjectEvent.ProjectUID.ToString(), p.Result.ProjectUID, "Project details are incorrect");

      var pg = _projectContext.GetAssociatedGeofences(createProjectEvent.ProjectUID.ToString());
      pg.Wait();
      Assert.IsNotNull(pg.Result, "Unable to retrieve ProjectGeofences");
      var projectGeofenceList = pg.Result.ToList();
      Assert.AreEqual(1, projectGeofenceList.Count(), "Geofence count is incorrect");
      Assert.AreEqual(createProjectEvent.ProjectUID.ToString(), projectGeofenceList[0].ProjectUID, "ProjectGeofence Uid is incorrect");
      Assert.AreEqual(GeofenceType.Project, projectGeofenceList[0].GeofenceType, "ProjectGeofence type is incorrect");

      var g = _geofenceContext.GetGeofence(projectGeofenceList[0].GeofenceUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Geofence");
      Assert.AreEqual(updateProjectEvent.ProjectBoundary, g.Result.GeometryWKT, "GeofenceWKT is incorrect");
    }

    /// <summary>
    /// Update Project on a 'Landfill-type service environment' -
    ///    Geofence doesn't exist...  create it
    /// </summary>
    [TestMethod]
    public void UpdateLandfillProject_ProjectGeofenceDoesntExist()
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
        ProjectTimezone = "New Zealand Standard Time",
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

      _projectContext.StoreEvent(createProjectEvent).Wait();
      _customerContext.StoreEvent(createCustomerEvent).Wait();
      _projectContext.StoreEvent(associateCustomerProjectEvent).Wait();

      var pg = _projectContext.GetAssociatedGeofences(createProjectEvent.ProjectUID.ToString());
      pg.Wait();
      Assert.IsNotNull(pg.Result, "Unable to retrieve ProjectGeofences");
      var projectGeofenceList = pg.Result.ToList();
      Assert.AreEqual(1, projectGeofenceList.Count(), "Geofence count is incorrect");
      var originalGeofenceUID = projectGeofenceList[0].GeofenceUID;
      var dissociateProjectGeofence = new DissociateProjectGeofence(){ProjectUID = createProjectEvent.ProjectUID, GeofenceUID = Guid.Parse(originalGeofenceUID), ActionUTC = DateTime.UtcNow};
      var d = _projectContext.StoreEvent(dissociateProjectGeofence);
      d.Wait();
      Assert.AreEqual(1, d.Result, "ProjectGeofence not dissociated");

      var updateProjectEvent = new UpdateProjectEvent
      {
        ProjectUID = createProjectEvent.ProjectUID,
        ProjectName = "The NEW Project Name",
        ProjectType = createProjectEvent.ProjectType,
        ProjectTimezone = createProjectEvent.ProjectTimezone,
        ProjectBoundary = "POLYGON((94.34 3.8,95.3 3.2,95.8 3.1,94.34 3.8))",
        ProjectEndDate = createProjectEvent.ProjectEndDate.AddDays(6),
        CoordinateSystemFileName = "thatLocation\\that.cs",
        ActionUTC = actionUtc.AddHours(1)
      };

      var s = _projectContext.StoreEvent(updateProjectEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Project event not updated");

      var p = _projectContext.GetProject(createProjectEvent.ProjectUID.ToString());
      p.Wait();
      Assert.IsNotNull(p.Result, "Unable to retrieve Project");
      Assert.AreEqual(createProjectEvent.ProjectUID.ToString(), p.Result.ProjectUID, "Project details are incorrect");

      pg = _projectContext.GetAssociatedGeofences(createProjectEvent.ProjectUID.ToString());
      pg.Wait();
      Assert.IsNotNull(pg.Result, "Unable to retrieve ProjectGeofences");
      projectGeofenceList = pg.Result.ToList();
      Assert.AreEqual(1, projectGeofenceList.Count(), "Geofence count is incorrect");
      Assert.AreEqual(createProjectEvent.ProjectUID.ToString(), projectGeofenceList[0].ProjectUID, "ProjectGeofence Uid is incorrect");
      Assert.AreEqual(GeofenceType.Project, projectGeofenceList[0].GeofenceType, "ProjectGeofence type is incorrect");
      Assert.AreNotEqual(originalGeofenceUID, projectGeofenceList[0].GeofenceUID, "Geofence Uid is incorrect, should have changed");

      var g = _geofenceContext.GetGeofence(projectGeofenceList[0].GeofenceUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Geofence");
      Assert.AreEqual(updateProjectEvent.ProjectBoundary, g.Result.GeometryWKT, "GeofenceWKT is incorrect");
    }

    #endregion LandfillProjects

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