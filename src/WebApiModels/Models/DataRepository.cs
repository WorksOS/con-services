using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to seperate executor logic
  ///   from
  ///   controller logic for testability and possible executor versioning.
  /// </summary>
  public class DataRepository : IDataRepository
  {
    /// <summary>
    /// Logger used in ProcessEx
    /// </summary>
    public ILogger log;
    protected IConfigurationStore configStore;

    /// <summary>
    /// Repository factory used in ProcessEx
    /// </summary>
    protected IAssetRepository assetRepository;
    protected IDeviceRepository deviceRepository;
    protected ICustomerRepository customerRepository;
    protected IProjectRepository projectRepository;
    protected ISubscriptionRepository subscriptionsRepository;

    protected IKafka producer;
    protected string kafkaTopicName;


    /// <summary>
    /// allows mapping between CG (which Raptor requires) and NG
    /// </summary>
    public ServiceTypeMappings serviceTypeMappings = new ServiceTypeMappings();

    public DataRepository(ILogger logger, IConfigurationStore configStore, IAssetRepository assetRepository, IDeviceRepository deviceRepository, 
      ICustomerRepository customerRepository, IProjectRepository projectRepository,
      ISubscriptionRepository subscriptionsRepository,
      IKafka producer, string kafkaTopicName)
    {
      log = logger;
      this.configStore = configStore;
      this.assetRepository = assetRepository;
      this.deviceRepository = deviceRepository;
      this.customerRepository = customerRepository;  
      this.projectRepository = projectRepository;
      this.subscriptionsRepository = subscriptionsRepository;
      this.producer = producer;
      this.kafkaTopicName = kafkaTopicName;
    }

    public async Task<Project> LoadProject(long legacyProjectId)
    {
      Project project = null;
      if (legacyProjectId > 0)
      {
        var p = await projectRepository.GetProject(legacyProjectId).ConfigureAwait(false);
        if (p != null) project = p;
      }
      return project;
    }

    public async Task<Project> LoadProject(string projectUid)
    {
      Project project = null;

      try
      {
        if (!string.IsNullOrEmpty(projectUid))
        {
          var p = await projectRepository.GetProject(projectUid).ConfigureAwait(false);
          if (p != null) project = p;
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw; // todo re-tryable
      }

      return project;
    }

    public async Task<IEnumerable<Project>> LoadProjects(string customerUid, DateTime validAtDate)
    {
      IEnumerable<Project> projects = null;
      if (customerUid != null)
      {
        var p = await projectRepository.GetProjectsForCustomer(customerUid).ConfigureAwait(false);

        if (p != null)
        {
          projects = p.ToList()
            .Where(x => x.StartDate <= validAtDate.Date && validAtDate.Date <= x.EndDate && !x.IsDeleted);
        }
      }
      return projects;
    }

    public async Task<IEnumerable<Project>> GetStandardProject(string customerUid, double latitude,
      double longitude, DateTime timeOfPosition)
    {
      IEnumerable<Project> projects = null;
      try
      {
        if (customerUid != null)
        {
          var p = await projectRepository.GetStandardProject(customerUid, latitude, longitude, timeOfPosition)
            .ConfigureAwait(false);

          if (p != null) projects = p;
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw; // todo re-tryable
      }

      return projects;
    }

    public async Task<IEnumerable<Project>> GetProjectMonitoringProject(string customerUid, double latitude,
      double longitude, DateTime timeOfPosition,
      int projectType, int serviceType)
    {
      IEnumerable<Project> projects = null;
      try
      {
        if (customerUid != null)
        {
          var p = await projectRepository.GetProjectMonitoringProject(customerUid,
            latitude, longitude, timeOfPosition,
            projectType,
            serviceType).ConfigureAwait(false);

          if (p != null) projects = p;
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw; // todo re-tryable
      }

      return projects;
    }

    public async Task<IEnumerable<Project>> GetIntersectingProjects(string customerUid, int[] projectTypes, 
      double latitude, double longitude, DateTime? timeOfPosition = null)
    {
      IEnumerable<Project> projects = null;
      try
      {
        if (customerUid != null)
        {
          var p = await projectRepository.GetIntersectingProjects(customerUid, projectTypes,
            latitude, longitude, timeOfPosition).ConfigureAwait(false);

          // todo do time checking outside of method as allowed for ManualImport

          if (p != null) projects = p;
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw; // todo re-tryable
      }

      return projects;
    }

    public async Task<AssetDeviceIds> LoadAssetDevice(string radioSerial, string deviceType)
    {
      AssetDeviceIds assetDevice = null;
      try
      {
        if (!string.IsNullOrEmpty(radioSerial) && !string.IsNullOrEmpty(deviceType))
        {
          var a = await deviceRepository.GetAssociatedAsset(radioSerial, deviceType).ConfigureAwait(false);
          if (a != null)
            assetDevice = a;
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw; // todo re-tryable
      }

      return assetDevice;
    }

    public async Task<Customer> LoadCustomer(string customerUid)
    {
      // TFA is only interested in customer and dealer types
      Customer customer = null;
      if (!string.IsNullOrEmpty(customerUid))
      {
        var a = await customerRepository.GetCustomer(new Guid(customerUid)).ConfigureAwait(false);
        if (a != null && 
          (a.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer || a.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Dealer)
          )
          customer = a;
      }
      return customer;
    }

    public async Task<CustomerTccOrg> LoadCustomerByTccOrgId(string tccOrgUid)
    {
      // TFA is only interested in customer and dealer types
      CustomerTccOrg customer = null;
      try
      {
        if (!string.IsNullOrEmpty(tccOrgUid))
        {
          var customerTccOrg = await customerRepository.GetCustomerWithTccOrg(tccOrgUid).ConfigureAwait(false);
          if (customerTccOrg != null &&
              (customerTccOrg.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer ||
               customerTccOrg.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Dealer)
          )
            customer = customerTccOrg;
        }

      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw; // todo re-tryable
      }

      return customer;
    }

    public async Task<CustomerTccOrg> LoadCustomerByCustomerUIDAsync(string customerUid)
    {
      // TFA is only interested in customer and dealer types
      CustomerTccOrg customer = null;
      if (customerUid != null)
      {
        var a = await customerRepository.GetCustomerWithTccOrg(new Guid(customerUid)).ConfigureAwait(false);
        if (a != null &&
            (a.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer || a.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Dealer)
            )
          customer = a;
      }
      return customer;
    }

    public async Task<Asset> LoadAsset(long legacyAssetId)
    {
      Asset asset = null;
      if (legacyAssetId > 0)
      {
        var a = await assetRepository.GetAsset(legacyAssetId).ConfigureAwait(false);
        if (a != null) asset = a;
      }
      return asset;
    }

    //public async Task<Asset> LoadAsset(string assetUid)
    //{
    //  Asset asset = null;
    //  try
    //  {
    //    if (!string.IsNullOrEmpty(assetUid))
    //  {
    //    var a = await assetRepository.GetAsset(assetUid).ConfigureAwait(false);
    //    if (a != null) asset = a;
    //  }
    //  }
    //  catch (Exception e)
    //  {
    //    Console.WriteLine(e);
    //    throw; // todo re-tryable
    //  }
    //  return asset;
    //}


    // customer Man3Dpm(18-15)
    // this may be from the Projects CustomerUID OR the Assets OwningCustomerUID
    public async Task<IEnumerable<Subscriptions>> LoadManual3DCustomerBasedSubs(string customerUid, DateTime validAtDate)
    {
      IEnumerable<Subscriptions> subs = null;
      if (!string.IsNullOrEmpty(customerUid))
      {
        var s = await subscriptionsRepository.GetSubscriptionsByCustomer(customerUid, validAtDate).ConfigureAwait(false);
        if (s != null)
        {
          subs = s.ToList()
          .Where(x => x.ServiceTypeID == serviceTypeMappings.serviceTypes.Find(st => st.name == "Manual 3D Project Monitoring").NGEnum)
          .Select(x => new Subscriptions("", "", x.CustomerUID, x.ServiceTypeID, x.StartDate, x.EndDate));
        }
      }
      return subs;
    }

    // asset:3dProjMon (16 --> 13) 
    public async Task<IEnumerable<Subscriptions>> LoadAssetSubs(string assetUid, DateTime validAtDate)
    {
      IEnumerable<Subscriptions> subs = null;
      if (!string.IsNullOrEmpty(assetUid))
      {
        var s = await subscriptionsRepository.GetSubscriptionsByAsset(assetUid, validAtDate.Date).ConfigureAwait(false);
        if (s != null)
        {
          subs = s
            .Where(x => x.ServiceTypeID == (serviceTypeMappings.serviceTypes.Find(st => st.name == "3D Project Monitoring").NGEnum))
            .Select(x => new Subscriptions(assetUid, "", x.CustomerUID, x.ServiceTypeID, x.StartDate, x.EndDate))
            .Distinct()
            .ToList();
        }
      }
      return subs;
    }

    public TWGS84Point[] ParseBoundaryData(string s)
    {
      // WKT string should be in 'lon lat,' format
      // TWG84Point is 'lon, lat'
      var points = new List<TWGS84Point>();
      var pointsArray = s.Substring(9, s.Length - 11).Split(',');

      for (var i = 0; i < pointsArray.Length; i++)
      {
        var coordinates = new double[2];
        coordinates = pointsArray[i].Trim().Split(' ').Select(c => double.Parse(c)).ToArray();
        points.Add(new TWGS84Point(coordinates[0], coordinates[1]));
      }

      // is it a valid WKT polygon?
      // note that an invalid polygon can't be created via the ProjectRepo
      if (points.Count > 3 && points[0].Equals(points[points.Count - 1]))
      {
        var fencePoints = points.ToArray();
        return fencePoints;
      }
      return new TWGS84Point[0];
    }
  }
}
