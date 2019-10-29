using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using ProjectDataModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;
namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to separate executor logic
  ///   from
  ///   controller logic for testability and possible executor versioning.
  /// </summary>
  public class DataRepository : IDataRepository
  {
    /// <summary>
    /// Logger used in ProcessEx
    /// </summary>
    public ILogger Log;

    private IConfigurationStore _configStore;

    /// <summary>
    /// Repository factory used in ProcessEx
    /// </summary>
    private readonly IAssetRepository _assetRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ISubscriptionRepository _subscriptionsRepository;

    private IKafka Producer;
    private string KafkaTopicName;


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
      _configStore = configStore;
      _assetRepository = assetRepository;
      _deviceRepository = deviceRepository;
      _customerRepository = customerRepository;
      _projectRepository = projectRepository;
      _subscriptionsRepository = subscriptionsRepository;
      Producer = producer;
      KafkaTopicName = kafkaTopicName;
    }

    public async Task<ProjectDataModel> LoadProject(long legacyProjectId)
    {
      ProjectDataModel project = null;
      try
      {
        if (legacyProjectId > 0)
          project = await _projectRepository.GetProject(legacyProjectId);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      return project;
    }

    public async Task<ProjectDataModel> LoadProject(string projectUid)
    {
      ProjectDataModel project = null;
      try
      {
        if (!string.IsNullOrEmpty(projectUid))
          project = await _projectRepository.GetProject(projectUid);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      return project;
    }

    public async Task<List<ProjectDataModel>> LoadProjects(string customerUid, DateTime validAtDate)
    {
      var projects = new List<ProjectDataModel>();
      try
      {
        if (customerUid != null)
        {
          var p = await _projectRepository.GetProjectsForCustomer(customerUid);

          if (p != null)
          {
            projects = p
              .Where(x => x.StartDate <= validAtDate.Date && validAtDate.Date <= x.EndDate && !x.IsDeleted)
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

      return projects;
    }

    public async Task<List<ProjectDataModel>> GetStandardProject(string customerUid, double latitude,
      double longitude, DateTime timeOfPosition)
    {
      var projects = new List<ProjectDataModel>();

      try
      {
        if (customerUid != null)
        {
          var p = (await _projectRepository.GetStandardProject(customerUid, latitude, longitude, timeOfPosition));
          if (p != null)
            projects = p.ToList();
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

    public async Task<List<ProjectDataModel>> GetProjectMonitoringProject(string customerUid, double latitude,
      double longitude, DateTime timeOfPosition,
      int projectType, int serviceType)
    {
      var projects = new List<ProjectDataModel>();
      try
      {
        if (customerUid != null)
        {
          var p = await _projectRepository.GetProjectMonitoringProject(customerUid,
            latitude, longitude, timeOfPosition,
            projectType,
            serviceType);

          if (p != null)
            projects = p.ToList();
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

    public async Task<List<ProjectDataModel>> GetIntersectingProjects(string customerUid,
      double latitude, double longitude, int[] projectTypes, DateTime? timeOfPosition = null)
    {
      var projects = new List<ProjectDataModel>();
      try
      {
        if (customerUid != null)
        {
          var p = await _projectRepository.GetIntersectingProjects(customerUid, latitude, longitude, projectTypes,
              timeOfPosition);
          if (p != null)
            projects = p.ToList();
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
          assetDevice = await _deviceRepository.GetAssociatedAsset(radioSerial, deviceType);
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
          var a = await _customerRepository.GetCustomer(new Guid(customerUid));
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
          var customerTccOrg = await _customerRepository.GetCustomerWithTccOrg(tccOrgUid);
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
          var a = await _customerRepository.GetCustomerWithTccOrg(new Guid(customerUid));
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
          asset = await _assetRepository.GetAsset(legacyAssetId);
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
    public async Task<List<Subscriptions>> LoadManual3DCustomerBasedSubs(string customerUid,
      DateTime validAtDate)
    {
      var subs = new List<Subscriptions>();
      try
      {
        if (!string.IsNullOrEmpty(customerUid))
        {
          var s = await _subscriptionsRepository.GetProjectBasedSubscriptionsByCustomer(customerUid, validAtDate);
          if (s != null)
          {
            subs = s
              .Where(x => x.ServiceTypeID == (int)ServiceTypeEnum.Manual3DProjectMonitoring)
              .Select(x => new Subscriptions("", "", x.CustomerUID, x.ServiceTypeID, x.StartDate, x.EndDate))
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

    // asset:3dProjMon (16 --> 13) 
    public async Task<List<Subscriptions>> LoadAssetSubs(string assetUid, DateTime validAtDate)
    {
      var subs = new List<Subscriptions>();
      try
      {
        if (!string.IsNullOrEmpty(assetUid))
        {
          var s = await _subscriptionsRepository.GetSubscriptionsByAsset(assetUid, validAtDate.Date);
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
