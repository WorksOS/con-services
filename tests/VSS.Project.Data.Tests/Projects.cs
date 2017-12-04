using System;
using System.Linq;
using Common.Repository;
using MasterDataRepo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace VSS.Project.Data.Tests
{
  [TestClass]
  public class Projects
  {
    private readonly CustomerRepo _customerRepo;
    private readonly ProjectRepo _projectRepo;
    private readonly GeofenceRepo _geofenceRepo;
    private readonly SubscriptionRepo _subscriptionRepo;

    public Projects()
    {
      _customerRepo = new CustomerRepo();
      _projectRepo = new ProjectRepo();
      _geofenceRepo = new GeofenceRepo();
      _subscriptionRepo = new SubscriptionRepo();
    }

    private CreateCustomerEvent CreateCustomerEvent()
    {
      return new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "blah",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = DateTime.UtcNow
      };
    }
    private AssociateCustomerUserEvent CreateAssociateCustomerUserEvent(Guid customerUid)
    {
      return new AssociateCustomerUserEvent()
      {
        CustomerUID = customerUid,
        UserUID = Guid.NewGuid(),
        ActionUTC = DateTime.UtcNow
      };
    }

    private CreateProjectEvent CreateCreateProjectEvent()
    {
      return new CreateProjectEvent()
      {
        ProjectUID = Guid.NewGuid(),
        ProjectID = new Random().Next(1, 19999),
        ProjectName = "Test Project",
        ProjectTimezone = "New Zealand Standard Time",
        ProjectType = ProjectType.LandFill,
        ProjectStartDate = DateTime.UtcNow.AddYears(-1).AddDays(-1).Date,
        ProjectEndDate = DateTime.UtcNow.AddYears(1).AddDays(1).Date,
        ProjectBoundary =
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
        ActionUTC = DateTime.UtcNow
      };
    }

    private AssociateProjectCustomer CreateAssociateProjectCustomer(Guid customerUid, long legacyCustomerId,
      Guid projectUid)
    {
      return new AssociateProjectCustomer()
      {
        CustomerUID = customerUid,
        LegacyCustomerID = legacyCustomerId,
        ProjectUID = projectUid,
        ActionUTC = DateTime.UtcNow
      };
    }

    private CreateGeofenceEvent CreateCreateGeofenceEvent(Guid customerUid, string geofenceType)
    {
      // Geofence type 1 = Project, 10=Landfill
      return new CreateGeofenceEvent()
      {
        GeofenceUID = Guid.NewGuid(),
        GeofenceName = "Test Geofence",
        Description = "Testing 123",
        GeofenceType = geofenceType,
        FillColor = 16744448,
        IsTransparent = true,
        GeometryWKT =
          "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))",
        CustomerUID = customerUid,
        AreaSqMeters = 123.456
      };
    }

    private AssociateProjectGeofence CreateAssociateProjectGeofenceEvent(Guid projectUid, Guid geofenceUid)
    {
      return new AssociateProjectGeofence
      {
        ProjectUID = projectUid,
        GeofenceUID = geofenceUid,
        ActionUTC = DateTime.UtcNow
      };
    }

    private CreateProjectSubscriptionEvent CreateCreateProjectSubscriptionEvent(Guid customerUid, string subscriptionType)
    {
      return new CreateProjectSubscriptionEvent()
      {
        CustomerUID = customerUid,
        SubscriptionUID = Guid.NewGuid(),
        SubscriptionType = subscriptionType,
        StartDate = new DateTime(2016, 02, 01),
        EndDate = new DateTime(9999, 12, 31),
        ActionUTC = DateTime.UtcNow
      };
    }

    private AssociateProjectSubscriptionEvent CreateAssociateProjectSubscriptionEvent(Guid projectUid, Guid subscriptionUid)
    {
      return new AssociateProjectSubscriptionEvent()
      {
        ProjectUID = projectUid,
        SubscriptionUID = subscriptionUid,
        EffectiveDate = new DateTime(2016, 02, 03),
        ActionUTC = DateTime.UtcNow
      };
    }


    [TestMethod]
    public void GetLandfillProjectsForUser_Succeeds()
    {
      int legacyCustomerId = new Random().Next(1, 1999999);

      var createCustomerEvent = CreateCustomerEvent();
      var associateCustomerUserEvent = CreateAssociateCustomerUserEvent(createCustomerEvent.CustomerUID);

      var createProjectEvent = CreateCreateProjectEvent();
      var associateProjectCustomer = CreateAssociateProjectCustomer(createCustomerEvent.CustomerUID, legacyCustomerId, createProjectEvent.ProjectUID);

      var createGeofenceEventProject = CreateCreateGeofenceEvent(createCustomerEvent.CustomerUID, "Project");
      var associateProjectGeofenceProject = CreateAssociateProjectGeofenceEvent(createProjectEvent.ProjectUID, createGeofenceEventProject.GeofenceUID);

      var createGeofenceEventLandfill = CreateCreateGeofenceEvent(createCustomerEvent.CustomerUID, "Landfill");
      var associateProjectGeofenceLandfill = CreateAssociateProjectGeofenceEvent(createProjectEvent.ProjectUID, createGeofenceEventLandfill.GeofenceUID);

      var createProjectSubscriptionEvent = CreateCreateProjectSubscriptionEvent(createCustomerEvent.CustomerUID, "Landfill"); 
      var associateProjectSubscriptionEvent = CreateAssociateProjectSubscriptionEvent(createProjectEvent.ProjectUID, createProjectSubscriptionEvent.SubscriptionUID);

      var insertCount = _customerRepo.StoreCustomer(createCustomerEvent);
      Assert.AreEqual(1, insertCount, "Failed to create a createCustomerEvent.");
      insertCount = _customerRepo.StoreCustomer(associateCustomerUserEvent);
      Assert.AreEqual(1, insertCount, "Failed to create a associateCustomerUserEvent.");

      insertCount = _projectRepo.StoreProject(createProjectEvent);
      Assert.AreEqual(1, insertCount, "Failed to create a project.");
      insertCount = _projectRepo.StoreProject(associateProjectCustomer);
      Assert.AreEqual(1, insertCount, "Failed to create a associateProjectCustomer.");

      insertCount = _geofenceRepo.StoreGeofence(createGeofenceEventProject);
      Assert.AreEqual(1, insertCount, "Failed to create a createGeofenceEventProject.");
      insertCount = _projectRepo.StoreProject(associateProjectGeofenceProject);
      Assert.AreEqual(1, insertCount, "Failed to create a associateProjectGeofenceProject.");
      insertCount = _geofenceRepo.StoreGeofence(createGeofenceEventLandfill);
      Assert.AreEqual(1, insertCount, "Failed to create a createGeofenceEventLandfill.");
      insertCount = _projectRepo.StoreProject(associateProjectGeofenceLandfill);
      Assert.AreEqual(1, insertCount, "Failed to create a associateProjectGeofenceLandfill.");

      insertCount = _subscriptionRepo.StoreSubscription(createProjectSubscriptionEvent);
      Assert.AreEqual(1, insertCount, "Failed to create a createProjectSubscriptionEvent.");
      insertCount = _subscriptionRepo.StoreSubscription(associateProjectSubscriptionEvent);
      Assert.AreEqual(1, insertCount, "Failed to create a associateProjectSubscriptionEvent.");


      var project = LandfillDb.GetLandfillProjectsForUser(associateCustomerUserEvent.UserUID.ToString());
      Assert.IsNotNull(project, "Error trying to get the created project.");
      Assert.AreEqual(1, project.Count(), "Failed to get the created project.");
      Assert.AreEqual(createProjectEvent.ProjectUID.ToString(), project?.ToList()[0].ProjectUID, "Failed to get the correct projectUID.");
    }
  }
}
