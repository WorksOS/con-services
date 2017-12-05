using System;
using MasterDataRepo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace LandfillDatabase.Tests
{
    public class TestBase
    {
      protected readonly CustomerRepo _customerRepo;
      protected readonly ProjectRepo _projectRepo;
      protected readonly GeofenceRepo _geofenceRepo;
      protected readonly SubscriptionRepo _subscriptionRepo;

      public TestBase()
      {
        _customerRepo = new CustomerRepo();
        _projectRepo = new ProjectRepo();
        _geofenceRepo = new GeofenceRepo();
        _subscriptionRepo = new SubscriptionRepo();
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

      public CreateProjectEvent CreateCreateProjectEvent(ProjectType projectType)
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
          ProjectBoundary =
            "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))",
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

      public CreateGeofenceEvent CreateCreateGeofenceEvent(Guid customerUid, string geofenceType)
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

      public AssociateProjectGeofence CreateAssociateProjectGeofenceEvent(Guid projectUid, Guid geofenceUid)
      {
        return new AssociateProjectGeofence
        {
          ProjectUID = projectUid,
          GeofenceUID = geofenceUid,
          ActionUTC = DateTime.UtcNow
        };
      }

      public CreateProjectSubscriptionEvent CreateCreateProjectSubscriptionEvent(Guid customerUid, string subscriptionType)
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

      var createProjectEvent = CreateCreateProjectEvent(ProjectType.LandFill);
      var associateProjectCustomer = CreateAssociateProjectCustomer(createCustomerEvent.CustomerUID, legacyCustomerId, createProjectEvent.ProjectUID);
      projectUid = createProjectEvent.ProjectUID;

      var createGeofenceEventProject = CreateCreateGeofenceEvent(createCustomerEvent.CustomerUID, "Project");
      var associateProjectGeofenceProject = CreateAssociateProjectGeofenceEvent(createProjectEvent.ProjectUID, createGeofenceEventProject.GeofenceUID);
      projectGeofenceUid = createGeofenceEventProject.GeofenceUID;

      var createGeofenceEventLandfill = CreateCreateGeofenceEvent(createCustomerEvent.CustomerUID, "Landfill");
      var associateProjectGeofenceLandfill = CreateAssociateProjectGeofenceEvent(createProjectEvent.ProjectUID, createGeofenceEventLandfill.GeofenceUID);
      landfillGeofenceUid = createGeofenceEventLandfill.GeofenceUID;

      var createProjectSubscriptionEvent = CreateCreateProjectSubscriptionEvent(createCustomerEvent.CustomerUID, "Landfill");
      var associateProjectSubscriptionEvent = CreateAssociateProjectSubscriptionEvent(createProjectEvent.ProjectUID, createProjectSubscriptionEvent.SubscriptionUID);
      subscriptionUid = createProjectSubscriptionEvent.SubscriptionUID;

      var insertCount = _customerRepo.StoreCustomer(createCustomerEvent);
      if (insertCount != 1) return false;
      insertCount = _customerRepo.StoreCustomer(associateCustomerUserEvent);
      if (insertCount != 1) return false;

      insertCount = _projectRepo.StoreProject(createProjectEvent);
      if (insertCount != 1) return false;
      insertCount = _projectRepo.StoreProject(associateProjectCustomer);
      if (insertCount != 1) return false;

      insertCount = _geofenceRepo.StoreGeofence(createGeofenceEventProject);
      if (insertCount != 1) return false;
      insertCount = _projectRepo.StoreProject(associateProjectGeofenceProject);
      if(insertCount != 1) return false;
      insertCount = _geofenceRepo.StoreGeofence(createGeofenceEventLandfill);
      if (insertCount != 1) return false;
      insertCount = _projectRepo.StoreProject(associateProjectGeofenceLandfill);
      if (insertCount != 1) return false;

      insertCount = _subscriptionRepo.StoreSubscription(createProjectSubscriptionEvent);
      if (insertCount != 1) return false;
      insertCount = _subscriptionRepo.StoreSubscription(associateProjectSubscriptionEvent);
      if (insertCount != 1) return false;

      return true;
    }
  }
  }
