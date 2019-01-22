using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

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
    public ILogger Log;
    protected IConfigurationStore ConfigStore;

    /// <summary>
    /// Repository factory used in ProcessEx
    /// </summary>
    protected IAssetRepository AssetRepository;
    protected IDeviceRepository DeviceRepository;
    protected ICustomerRepository CustomerRepository;
    protected IProjectRepository ProjectRepository;
    protected ISubscriptionRepository SubscriptionsRepository;

    protected IKafka Producer;
    protected string KafkaTopicName;


    /// <summary>
    /// allows mapping between CG (which Raptor requires) and NG
    /// </summary>
    public ServiceTypeMappings ServiceTypeMappings = new ServiceTypeMappings();

    public DataRepository(ILogger logger, IConfigurationStore configStore, IAssetRepository assetRepository, IDeviceRepository deviceRepository, 
      ICustomerRepository customerRepository, IProjectRepository projectRepository,
      ISubscriptionRepository subscriptionsRepository,
      IKafka producer, string kafkaTopicName)
    {
      Log = logger;
      this.ConfigStore = configStore;
      this.AssetRepository = assetRepository;
      this.DeviceRepository = deviceRepository;
      this.CustomerRepository = customerRepository;  
      this.ProjectRepository = projectRepository;
      this.SubscriptionsRepository = subscriptionsRepository;
      this.Producer = producer;
      this.KafkaTopicName = kafkaTopicName;
    }

    public async Task<Project> LoadProject(long legacyProjectId)
    {
      Project project = null;
      try
      {
        if (legacyProjectId > 0)
        {
          project = await ProjectRepository.GetProject(legacyProjectId).ConfigureAwait(false);
        }
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
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
          project = await ProjectRepository.GetProject(projectUid).ConfigureAwait(false);
        }
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      return project;
    }

    public async Task<IEnumerable<Project>> LoadProjects(string customerUid, DateTime validAtDate)
    {
      IEnumerable<Project> projects = null;

      try
      {
        if (customerUid != null)
        {
          var p = await ProjectRepository.GetProjectsForCustomer(customerUid).ConfigureAwait(false);

          if (p != null)
          {
            projects = p.ToList()
              .Where(x => x.StartDate <= validAtDate.Date && validAtDate.Date <= x.EndDate && !x.IsDeleted);
          }
        }
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
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
          projects = await ProjectRepository.GetStandardProject(customerUid, latitude, longitude, timeOfPosition)
            .ConfigureAwait(false);
        }
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
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
          projects = await ProjectRepository.GetProjectMonitoringProject(customerUid,
            latitude, longitude, timeOfPosition,
            projectType,
            serviceType).ConfigureAwait(false);
        }
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      return projects;
    }

    public async Task<IEnumerable<Project>> GetIntersectingProjects(string customerUid, 
      double latitude, double longitude, int[] projectTypes, DateTime? timeOfPosition = null)
    {
      IEnumerable<Project> projects = null;
      try
      {
        if (customerUid != null)
        {
          projects = await ProjectRepository.GetIntersectingProjects(customerUid, latitude, longitude, projectTypes, timeOfPosition).ConfigureAwait(false);
        }
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
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
          assetDevice = await DeviceRepository.GetAssociatedAsset(radioSerial, deviceType).ConfigureAwait(false);
        }
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      return assetDevice;
    }

    public async Task<Customer> LoadCustomer(string customerUid)
    {
      // TFA is only interested in customer and dealer types
      Customer customer = null;

      try
      {
        if (!string.IsNullOrEmpty(customerUid))
        {
          var a = await CustomerRepository.GetCustomer(new Guid(customerUid)).ConfigureAwait(false);
          if (a != null &&
              (a.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer ||
               a.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Dealer)
          )
            customer = a;
        }
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
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
          var customerTccOrg = await CustomerRepository.GetCustomerWithTccOrg(tccOrgUid).ConfigureAwait(false);
          if (customerTccOrg != null &&
              (customerTccOrg.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer ||
               customerTccOrg.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Dealer)
          )
            customer = customerTccOrg;
        }
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      return customer;
    }

    public async Task<CustomerTccOrg> LoadCustomerByCustomerUIDAsync(string customerUid)
    {
      // TFA is only interested in customer and dealer types
      CustomerTccOrg customer = null;

      try
      {
        if (customerUid != null)
        {
          var a = await CustomerRepository.GetCustomerWithTccOrg(new Guid(customerUid)).ConfigureAwait(false);
          if (a != null &&
              (a.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer ||
               a.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Dealer)
          )
            customer = a;
        }
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      return customer;
    }

    public async Task<Asset> LoadAsset(long legacyAssetId)
    {
      Asset asset = null;

      try
      {

        if (legacyAssetId > 0)
        {
          asset = await AssetRepository.GetAsset(legacyAssetId).ConfigureAwait(false);
        }
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      return asset;
    }

    // customer Man3Dpm(18-15)
    // this may be from the Projects CustomerUID OR the Assets OwningCustomerUID
    public async Task<IEnumerable<Subscriptions>> LoadManual3DCustomerBasedSubs(string customerUid,
      DateTime validAtDate)
    {
      IEnumerable<Subscriptions> subs = null;

      try
      {
        if (!string.IsNullOrEmpty(customerUid))
        {
          var s = await SubscriptionsRepository.GetProjectBasedSubscriptionsByCustomer(customerUid, validAtDate)
            .ConfigureAwait(false);
          if (s != null)
          {
            subs = s.ToList()
              .Where(x => x.ServiceTypeID == (int)ServiceTypeEnum.Manual3DProjectMonitoring)
              .Select(x => new Subscriptions("", "", x.CustomerUID, x.ServiceTypeID, x.StartDate, x.EndDate));
          }
        }
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      return subs;
    }

    // asset:3dProjMon (16 --> 13) 
    public async Task<IEnumerable<Subscriptions>> LoadAssetSubs(string assetUid, DateTime validAtDate)
    {
      IEnumerable<Subscriptions> subs = null;

      try
      {
        if (!string.IsNullOrEmpty(assetUid))
        {
          var s = await SubscriptionsRepository.GetSubscriptionsByAsset(assetUid, validAtDate.Date)
            .ConfigureAwait(false);
          if (s != null)
          {
            subs = s
              .Where(x => x.ServiceTypeID == (int)ServiceTypeEnum.ThreeDProjectMonitoring)
              .Select(x => new Subscriptions(assetUid, "", x.CustomerUID, x.ServiceTypeID, x.StartDate, x.EndDate))
              .Distinct()
              .ToList();
          }
        }
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
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
