using Microsoft.Extensions.Logging;
using Repositories;
using Repositories.DBModels;
using Repositories.ExtendedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using WebApiModels.Enums;

namespace WebApiModels.Models
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to seperate executor logic
  ///   from
  ///   controller logic for testability and possible executor versioning.
  /// </summary>
  public class DataRepository : IDataRepository
  {
    /// <summary>
    /// Repository factory used in ProcessEx
    /// </summary>
    public IRepositoryFactory factory;

    /// <summary>
    /// Logger used in ProcessEx
    /// </summary>
    public ILogger log;

    /// <summary>
    /// allows mapping between CG (which Raptor requires) and NG
    /// </summary>
    public ServiceTypeMappings serviceTypeMappings = new ServiceTypeMappings();

    public DataRepository(IRepositoryFactory factory, ILogger logger)
    {
      this.factory = factory;
      this.log = logger;
    }

    public Project LoadProject(long legacyProjectId)
    {
      Project project = null;
      if (legacyProjectId > 0)
      {
        var projectRepo = factory.GetRepository<IProjectEvent>() as ProjectRepository;
        var p = projectRepo.GetProject(legacyProjectId);
        if (p != null && p.Result != null) project = p.Result;
      }
      return project;
    }

    public IEnumerable<Project> LoadProjects(string customerUid, DateTime validAtDate)
    {
      IEnumerable<Project> projects = null;
      if (customerUid != null)
      {
        var projectRepo = factory.GetRepository<IProjectEvent>() as ProjectRepository;
        var p = projectRepo.GetProjectsForCustomer(customerUid);

        if (p != null && p.Result != null)
        {
          projects = p.Result.ToList()
            .Where(x => x.StartDate <= validAtDate.Date && validAtDate.Date <= x.EndDate);
        }
      }
      return projects;
    }

    public AssetDeviceIds LoadAssetDevice(string radioSerial, string deviceType)
    {
      AssetDeviceIds assetDevice = null;
      if (!string.IsNullOrEmpty(radioSerial) && !string.IsNullOrEmpty(deviceType))
      {
        var deviceRepo = factory.GetRepository<IDeviceEvent>() as DeviceRepository;
        var a = deviceRepo.GetAssociatedAsset(radioSerial, deviceType);
        if (a != null && a.Result != null)
          assetDevice = a.Result;
      }
      return assetDevice;
    }

    public Customer LoadCustomer(string customerUid)
    {
      // TFA is only interested in customer and dealer types
      Customer customer = null;
      if (!string.IsNullOrEmpty(customerUid))
      {
        var customerRepo = factory.GetRepository<ICustomerEvent>() as CustomerRepository;
        var a = customerRepo.GetCustomer(new Guid(customerUid));
        if (a != null && a.Result != null &&
          (a.Result.CustomerType == VSS.VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer || a.Result.CustomerType == VSS.VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Dealer)
          )
          customer = a.Result;
      }
      return customer;
    }

    public CustomerTccOrg LoadCustomerByTccOrgId(string tccOrgUid)
    {
      // TFA is only interested in customer and dealer types
      CustomerTccOrg customer = null;
      if (!string.IsNullOrEmpty(tccOrgUid))
      {
        var customerRepo = factory.GetRepository<ICustomerEvent>() as CustomerRepository;
        var a = customerRepo.GetCustomerWithTccOrg(tccOrgUid);
        if (a != null && a.Result != null &&
          (a.Result.CustomerType == VSS.VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer || a.Result.CustomerType == VSS.VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Dealer)
          )
          customer = a.Result;
      }
      return customer;
    }

    public CustomerTccOrg LoadCustomerByCustomerUID(string customerUid)
    {
      // TFA is only interested in customer and dealer types
      CustomerTccOrg customer = null;
      if (customerUid != null)
      {
        var customerRepo = factory.GetRepository<ICustomerEvent>() as CustomerRepository;
        var a = customerRepo.GetCustomerWithTccOrg(new Guid(customerUid));
        if (a.Result != null && a.Result != null &&
            (a.Result.CustomerType == VSS.VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer || a.Result.CustomerType == VSS.VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Dealer)
            )
          customer = a.Result;
      }
      return customer;
    }

    public Asset LoadAsset(long legacyAssetId)
    {
      Asset asset = null;
      if (legacyAssetId > 0)
      {
        var assetRepo = factory.GetRepository<IAssetEvent>() as AssetRepository;
        var a = assetRepo.GetAsset(legacyAssetId);
        if (a.Result != null && a.Result != null) asset = a.Result;
      }
      return asset;
    }


    // customer Man3Dpm(18-15)
    // this may be from the Projects CustomerUID OR the Assets OwningCustomerUID
    public IEnumerable<Subscriptions> LoadManual3DCustomerBasedSubs(string customerUid, DateTime validAtDate)
    {
      IEnumerable<Subscriptions> subs = null;
      if (!string.IsNullOrEmpty(customerUid))
      {
        var subsRepo = factory.GetRepository<ISubscriptionEvent>() as SubscriptionRepository;
        var s = subsRepo.GetSubscriptionsByCustomer(customerUid, validAtDate);
        if (s.Result != null && s.Result != null)
        {
          subs = s.Result.ToList()
          .Where(x => x.ServiceTypeID == serviceTypeMappings.serviceTypes.Find(st => st.name == "Manual 3D Project Monitoring").NGEnum)
          .Select(x => new Subscriptions("", "", x.CustomerUID, x.ServiceTypeID, x.StartDate, x.EndDate));
        }
      }
      return subs;
    }

    // asset:3dProjMon (16 --> 13) 
    public IEnumerable<Subscriptions> LoadAssetSubs(string assetUid, DateTime validAtDate)
    {
      IEnumerable<Subscriptions> subs = null;
      if (!string.IsNullOrEmpty(assetUid))
      {
        var subsRepo = factory.GetRepository<ISubscriptionEvent>() as SubscriptionRepository;
        var s = subsRepo.GetSubscriptionsByAsset(assetUid, validAtDate.Date);
        if (s.Result != null && s.Result != null)
        {
          subs = s.Result.ToList()
            .Where(x => x.ServiceTypeID == (serviceTypeMappings.serviceTypes.Find(st => st.name == "3D Project Monitoring").NGEnum))
            .Distinct()
            .Select(x => new Subscriptions(assetUid, "", x.CustomerUID, x.ServiceTypeID, x.StartDate, x.EndDate));
        }
      }
      return subs;
    }

    public TWGS84Point[] ParseBoundaryData(string s)
    {
      // WKT string should be in 'lon lat,' format
      // TWG84Point is 'lon, lat'
      var points = new List<TWGS84Point>();
      string[] pointsArray = s.Substring(9, s.Length - 11).Split(',');

      for (int i = 0; i < pointsArray.Length; i++)
      {
        double[] coordinates = new double[2];
        coordinates = pointsArray[i].Trim().Split(' ').Select(c => double.Parse(c)).ToArray();
        points.Add(new TWGS84Point(coordinates[0], coordinates[1]));
      }

      // is it a valid WKT polygon?
      // note that an invalid polygon can't be created via the ProjectRepo
      if (points.Count() > 3 && points[0].Equals(points[points.Count() - 1]))
      {
        var fencePoints = new TWGS84Point[points.Count()];
        fencePoints = points.ToArray();
        return fencePoints;
      }
      return new TWGS84Point[0];
    }
  }
}