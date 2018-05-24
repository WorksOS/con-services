using System;
using MasterDataRepo;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace LandfillDatabase.Tests
{
    public class TestBase
    {
      protected readonly CustomerRepo CustomerRepo;
      protected readonly ProjectRepo ProjectRepo;
      protected readonly GeofenceRepo GeofenceRepo;
      protected readonly SubscriptionRepo SubscriptionRepo;
      protected readonly string ProjectGeofenceWkt = 
          "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      protected readonly string LandfillGeofenceWkt =
          "POLYGON((172.68231141046 -43.6277661929154,172.692096108947 -43.6213045879588,172.701537484681 -43.6285117180247,172.698104257136 -43.6328604301996,172.689349526916 -43.6336058921214,172.682998055965 -43.6303754903428,172.68231141046 -43.6277661929154,172.68231141046 -43.6277661929154))";

    public TestBase()
      {
        CustomerRepo = new CustomerRepo();
        ProjectRepo = new ProjectRepo();
        GeofenceRepo = new GeofenceRepo();
        SubscriptionRepo = new SubscriptionRepo();
      }

      public CreateCustomerEvent CreateCustomerEvent()
      {
        return new CreateCustomerEvent()
        {
          CustomerUID = Guid.NewGuid(),
          CustomerName = "blah",
          CustomerType = CustomerType.Customer.ToString(),
          ActionUTC = DateTime.UtcNow
        };
      }

      public AssociateCustomerUserEvent CreateAssociateCustomerUserEvent(Guid customerUid)
      {
        return new AssociateCustomerUserEvent()
        {
          CustomerUID = customerUid,
          UserUID = Guid.NewGuid(),
          ActionUTC = DateTime.UtcNow
        };
      }

      public CreateProjectEvent CreateProjectEvent(ProjectType projectType)
      {
        return new CreateProjectEvent()
        {
          ProjectUID = Guid.NewGuid(),
          ProjectID = new Random().Next(1, 19999),
          ProjectName = "Test Project",
          ProjectTimezone = "New Zealand Standard Time",
          ProjectType = projectType,
          ProjectStartDate = DateTime.UtcNow.AddYears(-1).AddDays(-1).Date,
          ProjectEndDate = DateTime.UtcNow.AddYears(1).AddDays(1).Date,
          ProjectBoundary = ProjectGeofenceWkt,
          ActionUTC = DateTime.UtcNow
        };
      }

      public AssociateProjectCustomer CreateAssociateProjectCustomer(Guid customerUid, long legacyCustomerId,
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

      public CreateGeofenceEvent CreateGeofenceEvent(Guid customerUid, string geofenceType)
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
          GeometryWKT = geofenceType == "Project" ? ProjectGeofenceWkt : LandfillGeofenceWkt,
          CustomerUID = customerUid,
          AreaSqMeters = 123.456
        };
      }

      public AssociateProjectGeofence CreateAssociateProjectGeofenceEvent(Guid projectUid, Guid geofenceUid)
      {
        return new AssociateProjectGeofence
        {
          ProjectUID = projectUid,
          GeofenceUID = geofenceUid,
          ActionUTC = DateTime.UtcNow
        };
      }

      public CreateProjectSubscriptionEvent CreateProjectSubscriptionEvent(Guid customerUid, string subscriptionType)
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

      public AssociateProjectSubscriptionEvent CreateAssociateProjectSubscriptionEvent(Guid projectUid, Guid subscriptionUid)
      {
        return new AssociateProjectSubscriptionEvent()
        {
          ProjectUID = projectUid,
          SubscriptionUID = subscriptionUid,
          EffectiveDate = new DateTime(2016, 02, 03),
          ActionUTC = DateTime.UtcNow
        };
      }

    public bool CreateAProjectWithLandfill(out int legacyCustomerId,
      out Guid customerUid, out Guid userUid,
      out Guid projectUid, out Guid projectGeofenceUid, out Guid landfillGeofenceUid,
      out Guid subscriptionUid
      )
    {
      legacyCustomerId = new Random().Next(1, 1999999);
      
      var createCustomerEvent = CreateCustomerEvent();
      var associateCustomerUserEvent = CreateAssociateCustomerUserEvent(createCustomerEvent.CustomerUID);
      customerUid = createCustomerEvent.CustomerUID;
      userUid = associateCustomerUserEvent.UserUID;

      var createProjectEvent = CreateProjectEvent(ProjectType.LandFill);
      var associateProjectCustomer = CreateAssociateProjectCustomer(createCustomerEvent.CustomerUID, legacyCustomerId, createProjectEvent.ProjectUID);
      projectUid = createProjectEvent.ProjectUID;

      var createGeofenceEventProject = CreateGeofenceEvent(createCustomerEvent.CustomerUID, "Project");
      var associateProjectGeofenceProject = CreateAssociateProjectGeofenceEvent(createProjectEvent.ProjectUID, createGeofenceEventProject.GeofenceUID);
      projectGeofenceUid = createGeofenceEventProject.GeofenceUID;

      var createGeofenceEventLandfill = CreateGeofenceEvent(createCustomerEvent.CustomerUID, "Landfill");
      var associateProjectGeofenceLandfill = CreateAssociateProjectGeofenceEvent(createProjectEvent.ProjectUID, createGeofenceEventLandfill.GeofenceUID);
      landfillGeofenceUid = createGeofenceEventLandfill.GeofenceUID;

      var createProjectSubscriptionEvent = CreateProjectSubscriptionEvent(createCustomerEvent.CustomerUID, "Landfill");
      var associateProjectSubscriptionEvent = CreateAssociateProjectSubscriptionEvent(createProjectEvent.ProjectUID, createProjectSubscriptionEvent.SubscriptionUID);
      subscriptionUid = createProjectSubscriptionEvent.SubscriptionUID;

      var insertCount = CustomerRepo.StoreCustomer(createCustomerEvent);
      if (insertCount != 1) return false;
      insertCount = CustomerRepo.StoreCustomer(associateCustomerUserEvent);
      if (insertCount != 1) return false;

      insertCount = ProjectRepo.StoreProject(createProjectEvent);
      if (insertCount != 1) return false;
      insertCount = ProjectRepo.StoreProject(associateProjectCustomer);
      if (insertCount != 1) return false;

      insertCount = GeofenceRepo.StoreGeofence(createGeofenceEventProject);
      if (insertCount != 1) return false;
      insertCount = ProjectRepo.StoreProject(associateProjectGeofenceProject);
      if(insertCount != 1) return false;
      insertCount = GeofenceRepo.StoreGeofence(createGeofenceEventLandfill);
      if (insertCount != 1) return false;
      insertCount = ProjectRepo.StoreProject(associateProjectGeofenceLandfill);
      if (insertCount != 1) return false;

      insertCount = SubscriptionRepo.StoreSubscription(createProjectSubscriptionEvent);
      if (insertCount != 1) return false;
      insertCount = SubscriptionRepo.StoreSubscription(associateProjectSubscriptionEvent);
      if (insertCount != 1) return false;

      return true;
    }
  }
  }
