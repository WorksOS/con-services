using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VSS.Project.Service.Repositories;
using VSS.Project.Service.Utils;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.Project.Data;
using VSS.Project.Data.Models;
using VSS.Customer.Data;

namespace RepositoryTests
{
  [TestClass]
  public class GeofenceRepositoryTests
  {
    [TestInitialize]
    public void Init()
    {
      var serviceCollection = new ServiceCollection();
      serviceCollection.AddSingleton<ILoggerFactory>((new LoggerFactory()).AddDebug());
      new DependencyInjectionProvider(serviceCollection.BuildServiceProvider());
    }

    #region Geofence
   
    /// <summary>
    /// Create Geofence - Happy path i.e. 
    ///   customer, project, custProject exists, geofence doesn't exist.
    /// </summary>
    [TestMethod]
    public void CreateGeofence_HappyPath()
    {
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Project Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = now
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
        ActionUTC = now
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 1234,
        RelationType = RelationType.Customer,
        ActionUTC = now
      };

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        Description = "The Geofence Name",
        GeofenceUID = Guid.NewGuid(),
        GeofenceType = "", // todo need VSS.VisionLink.Interfaces.Events.MasterData.Models GeofenceType
        GeometryWKT = "",
        //Boundary = ?? todo I think this comes in the CreateProjectEvent????
        ActionUTC = now
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());
      var projectContext = new ProjectRepository(new GenericConfiguration());

      // todo there isn't yet a geofence repo
      //var geofenceContext = new GeofenceRepository(new GenericConfiguration());

      throw new NotImplementedException();
    }

    /// <summary>
    /// Create GeoFence - Happy path but out of order
    ///   same as happy path but inserted out of order
    /// </summary>
    [TestMethod]
    public void CreateGeofence_HappyPathButOutOfOrder()
    {
      throw new NotImplementedException();
    }


    /// <summary>
    /// Create Geofence - RelationShips not setup i.e. 
    ///   customer, project and CustomerProject relationship NOT added
    ///   geofence doesn't exist already.
    ///   insert Geofence
    /// </summary>
    [TestMethod]
    public void CreateGeofence_HappyPath_NoCustomer()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Create Geofenced - Already exists
    ///   geofence exists already.
    ///   ignore
    /// </summary>
    [TestMethod]
    public void CreateGeofence_AlreadyExists()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Create Geofenced - happyPath good Geometry
    ///   should insert
    /// </summary>
    [TestMethod]
    public void CreateGeofence_HappyPath_GoodGeometry()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Create Geofenced - invalidGeometry
    ///   should fail
    /// </summary>
    [TestMethod]
    public void CreateGeofence_InvalidGeometry()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Update Geofence - happyPath
    /// exists, just update whichever fields are allowed.
    /// </summary>
    [TestMethod]
    public void UpdateGeofence_HappyPath()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Create Geofence - invalidGeometry
    ///   should fail
    /// </summary>
    [TestMethod]
    public void UpdateGeofence_GeofenceDoesntExist()
    {
      throw new NotImplementedException();
    }

    #endregion


    #region AssociateGeofenceWithProject

    /// <summary>
    /// Associate Project Geofence - Happy Path
    ///   project and Geofence added.
    ///   Project legacyCustomerID updated and ActionUTC is later
    /// </summary>
    [TestMethod]
    public void AssociateProjectWithGeofence_HappyPath()
    {
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);
      var projectTimeZone = "New Zealand Standard Time";
      var customerUid = Guid.NewGuid();

      var createProjectEvent = new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = 12343,
        ProjectName = "The Project Name",
        ProjectType = ProjectType.LandFill,
        ProjectTimezone = projectTimeZone,

        ProjectStartDate = new DateTime(2016, 02, 01),
        ProjectEndDate = new DateTime(2017, 02, 01),
        ActionUTC = now
      };

      var createGeofenceEvent = new CreateGeofenceEvent()
      {
        CustomerUID = customerUid,
        Description = "The Geofence Name",
        GeofenceUID = Guid.NewGuid(),
        GeofenceType = "", // todo vss models needs a GeofenceType e.g. Landfill,
        GeometryWKT = "",
        //Boundary = ?? todo I think this comes in the CreateProjectEvent????
        ActionUTC = now
      };

      var associateCustomerProjectEvent = new AssociateProjectCustomer()
      {
        CustomerUID = customerUid,
        ProjectUID = createProjectEvent.ProjectUID,
        LegacyCustomerID = 999,
        RelationType = RelationType.Customer,
        ActionUTC = now.AddDays(1)
      };

      var associateProjectGeofenceEvent = new AssociateProjectGeofence()
      {
        GeofenceUID = createGeofenceEvent.GeofenceUID,
        ActionUTC = now.AddDays(1)
      };


      var customerContext = new CustomerRepository(new GenericConfiguration());
      var projectContext = new ProjectRepository(new GenericConfiguration());

      // todo
      // var geofenceContext = new GeofenceRepository(new GenericConfiguration());

      throw new NotImplementedException();
    }

    /// <summary>
    /// Associate Project Geofence - already exists
    /// </summary>
    [TestMethod]
    public void AssociateProjectWithGeofence_AlreadyExists()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Dissociate Project Geofence - not needed?
    /// </summary>
    [TestMethod]
    public void DissociateProjectWithGeofence_NotSupported()
    {
      throw new NotImplementedException();
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
 
 