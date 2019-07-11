using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Abstractions;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;
using ProjectDataModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;
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

    protected ITRexCompactionDataProxy TRexCompactionDataProxy;


    /// <summary>
    /// allows mapping between CG (which Raptor requires) and NG
    /// </summary>
    public ServiceTypeMappings ServiceTypeMappings = new ServiceTypeMappings();

    public DataRepository(ILogger logger, IConfigurationStore configStore, IAssetRepository assetRepository, IDeviceRepository deviceRepository,
      ICustomerRepository customerRepository, IProjectRepository projectRepository,
      ISubscriptionRepository subscriptionsRepository,
      IKafka producer, string kafkaTopicName,
      ITRexCompactionDataProxy tRexCompactionDataProxy)
    {
      Log = logger;
      ConfigStore = configStore;
      AssetRepository = assetRepository;
      DeviceRepository = deviceRepository;
      CustomerRepository = customerRepository;
      ProjectRepository = projectRepository;
      SubscriptionsRepository = subscriptionsRepository;
      Producer = producer;
      KafkaTopicName = kafkaTopicName;
      TRexCompactionDataProxy = tRexCompactionDataProxy;
    }

    public async Task<ProjectDataModel> LoadProject(long legacyProjectId)
    {
      ProjectDataModel project = null;
      try
      {
        if (legacyProjectId > 0)
          project = await ProjectRepository.GetProject(legacyProjectId);
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
          project = await ProjectRepository.GetProject(projectUid);
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
          var p = await ProjectRepository.GetProjectsForCustomer(customerUid);

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

    /// <summary>
    /// Get CSIB/s for either:
    ///   project: assumes project already found and ok so far
    ///   customer: if customer found a) via asset or b) TCCOrgId, then get that customers projects valid at that time, then get each projects CSIB
    /// Note that this is only used by v2 endpoint, which is TRex only, so can call trex, not 3dp (which could get raptor csib)
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="projectUid"></param>
    /// <param name="validAtDate"></param>
    /// <param name="northing"></param>
    /// <param name="easting"></param>
    /// <returns></returns>
    public async Task<List<ProjectDataModel>> LoadCSIBs(string customerUid, string projectUid, DateTime validAtDate, double? northing, double? easting)
    {
      if (northing == null || easting == null)
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 51));

      var projectCSIBs = new List<Tuple<string, string>>();

      if (!string.IsNullOrEmpty(projectUid))
      {
        projectCSIBs.Add(new Tuple<string, string>(projectUid, GetCSIBFromTRex(projectUid))); // todoJeannie
      }
      else
      {
        try
        {
          if (customerUid != null)
          {
            var projects = new List<ProjectDataModel>();
            var p = await ProjectRepository.GetProjectsForCustomer(customerUid);

            if (p != null)
            {
              projects = p
                .Where(x => x.StartDate <= validAtDate.Date && validAtDate.Date <= x.EndDate && !x.IsDeleted)
                .ToList();
            }

            if (projects != null)
            {
              foreach (var project in projects)
              {
                projectCSIBs.Add(new Tuple<string, string>(projectUid, GetCSIBFromTRex(project.ProjectUID))); // todoJeannie
              }
            }
          }
        }
        catch (Exception e)
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
              ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
        }
      }

      return projectCSIBs;
    }

    private string GetCSIBFromTRex(string projectUid)
    {
      try
      {
        var returnedResult = await TRexCompactionDataProxy.SendDataGetRequest<CSIBResult>(projectUid, $"/projects/{projectUid}/csib", customHeadersTodo);
        return returnedResult.CSIB;
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 999 /* todoJeannie*/, e.Message));
      }

      return string.Empty;
    }

    public async Task<List<ProjectDataModel>> GetStandardProject(string customerUid, double latitude,
      double longitude, DateTime timeOfPosition)
    {
      var projects = new List<ProjectDataModel>();

      try
      {
        if (customerUid != null)
        {
          var p = (await ProjectRepository.GetStandardProject(customerUid, latitude, longitude, timeOfPosition));
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
          var p = await ProjectRepository.GetProjectMonitoringProject(customerUid,
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
          var p = await ProjectRepository.GetIntersectingProjects(customerUid, latitude, longitude, projectTypes,
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
          assetDevice = await DeviceRepository.GetAssociatedAsset(radioSerial, deviceType);
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
          var a = await CustomerRepository.GetCustomer(new Guid(customerUid));
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
          var customerTccOrg = await CustomerRepository.GetCustomerWithTccOrg(tccOrgUid);
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
          var a = await CustomerRepository.GetCustomerWithTccOrg(new Guid(customerUid));
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
          asset = await AssetRepository.GetAsset(legacyAssetId);
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
          var s = await SubscriptionsRepository.GetProjectBasedSubscriptionsByCustomer(customerUid, validAtDate);
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
          var s = await SubscriptionsRepository.GetSubscriptionsByAsset(assetUid, validAtDate.Date);
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
