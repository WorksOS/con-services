using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositories;
using Repositories.DBModels;
using Repositories.ExtendedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TestUtility;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.TagFileAuth.Service.WebApiModels.Executors
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to seperate executor logic
  ///   from
  ///   controller logic for testability and possible executor versioning.
  /// </summary>
  public abstract class RequestExecutorContainer
  {
    /// <summary>
    /// Repository factory used in ProcessEx
    /// </summary>
    protected IRepositoryFactory factory;

    /// <summary>
    /// Logger used in ProcessEx
    /// </summary>
    protected ILogger log;

    /// <summary>
    ///   Generates the errorlist for instantiated executor.
    /// </summary>
    /// <returns>List of errors with corresponding descriptions.</returns>
    public List<Tuple<int, string>> GenerateErrorlist()
    {
      return (from object enumVal in Enum.GetValues(typeof(ContractExecutionStatesEnum))
              select new Tuple<int, string>((int)enumVal, enumVal.ToString())).ToList();
    }


    /// <summary>
    /// Processes the specified item. This is the main method to execute real action.
    /// </summary>
    /// <typeparam name="T">>Generic type which should be</typeparam>
    /// <param name="item">>The item.</param>
    /// <param name="guidsWithDates">Results from applying subscription logic to the request</param>
    /// <returns></returns>
    protected abstract ContractExecutionResult ProcessEx<T>(T item); // where T : IServiceDomainObject;

    internal static object Build<T>()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ServiceException"></exception>
    public ContractExecutionResult Process<T>(T item)
    {
      if (item == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
      return ProcessEx(item);
    }

    /// <summary>
    ///   Builds this instance for specified executor type.
    /// </summary>
    /// <param name="factory">Repository factory</param>
    /// <param name="logger">Ilogger</param>
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    /// <returns></returns>
    public static TExecutor Build<TExecutor>(IRepositoryFactory factory, ILogger logger) where TExecutor : RequestExecutorContainer, new()
    {
      var executor = new TExecutor() { factory = factory, log = logger };
      return executor;
    }

    #region DataStorage // todo caching if needed

    protected class SubscriptionData
    {
      public string assetUId { get; set; }
      public string projectUid { get; set; }
      public string customerUid { get; set; }
      public int serviceTypeId { get; set; }
      public int startKeyDate { get; set; }
      public int endKeyDate { get; set; }


      public SubscriptionData(string assetUId, string projectUid, string customerUid, int serviceTypeId, DateTime? startKeyDate, DateTime? endKeyDate)
      {
        this.assetUId = assetUId;
        this.projectUid = projectUid;
        this.customerUid = customerUid;
        this.serviceTypeId = serviceTypeId;
        this.startKeyDate = startKeyDate == null ? DateTimeExtensions.KeyDate(DateTime.MinValue) : DateTimeExtensions.KeyDate(startKeyDate.Value);
        this.endKeyDate = endKeyDate == null ? DateTimeExtensions.NullKeyDate : DateTimeExtensions.KeyDate(endKeyDate.Value);

      }
    }


    protected Project LoadProject(long legacyProjectId)
    {
      Project project = null;
      if (legacyProjectId > 0)
      {
        var projectRepo = factory.GetRepository<IProjectEvent>() as ProjectRepository;
        var p = projectRepo.GetProject(legacyProjectId);
        if (p != null)
        {
          project = p.Result;
        }
      }
      return project;
    }

    protected async Task<IEnumerable<Project>> LoadProjects(string customerUid, DateTime validAtDate)
    {
      IEnumerable<Project> projects = null;
      if (customerUid != null)
      {
        var projectRepo = factory.GetRepository<IProjectEvent>() as ProjectRepository;
        var p = await projectRepo.GetProjectsForCustomer(customerUid);

        if (p != null)
        {
          log.LogDebug("Executor: Loaded projects {0} for customerUid {1}", JsonConvert.SerializeObject(p), customerUid);
          projects = p.ToList()
            .Where(x => x.StartDate <= validAtDate.Date && validAtDate.Date <= x.EndDate);
        }
      }
      return projects;
    }

    protected async Task<AssetDeviceIds> LoadAssetDevice(string radioSerial, string deviceType)
    {
      AssetDeviceIds assetDevice = null;
      if (!string.IsNullOrEmpty(radioSerial) && !string.IsNullOrEmpty(deviceType))
      {
        var deviceRepo = factory.GetRepository<IDeviceEvent>() as DeviceRepository;
        var a = await deviceRepo.GetAssociatedAsset(radioSerial, deviceType);
        assetDevice = a;
      }
      log.LogDebug("Executor: Loaded AssetDevice {0}", JsonConvert.SerializeObject(assetDevice));
      return assetDevice;
    }

    protected async Task<CustomerTccOrg> LoadCustomerByTccOrgId(string tccOrgUid)
    {
      CustomerTccOrg customer = null;
      if (!string.IsNullOrEmpty(tccOrgUid))
      {
        var customerRepo = factory.GetRepository<ICustomerEvent>() as CustomerRepository;
        var a = await customerRepo.GetCustomerWithTccOrg(tccOrgUid);
        if (a != null &&
          (a.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer || a.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Dealer)
          )
          customer = a;
      }
      log.LogDebug("Executor: Loading Customer by tccOrgUid {0} using tccOrgId {1}", JsonConvert.SerializeObject(customer), tccOrgUid);
      return customer;
    }

    protected CustomerTccOrg LoadCustomerByCustomerUID(string customerUid)
    {
      CustomerTccOrg customer = null;
      if ( customerUid != null )
      {
        var customerRepo = factory.GetRepository<ICustomerEvent>() as CustomerRepository;
        var a = customerRepo.GetCustomerWithTccOrg(new Guid(customerUid));
        if (a.Result != null &&
            (a.Result.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer || a.Result.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Dealer)
            )
        customer = a.Result;
      }
      log.LogDebug("Executor: Loading Customer by customerUid {0} using customerUid {1}", JsonConvert.SerializeObject(customer), customerUid);
      return customer;
    }

    protected Asset LoadAsset(long legacyAssetId)
    {
      Asset asset = null;
      if (legacyAssetId> 0)
      {
        var assetRepo = factory.GetRepository<IAssetEvent>() as AssetRepository;
        var a = assetRepo.GetAsset(legacyAssetId);
        asset = a.Result;
      }
      log.LogDebug("Executor: Loaded Asset {0}", JsonConvert.SerializeObject(asset));
      return asset;
    }


    protected IEnumerable<SubscriptionData> LoadProjectBasedSubs(long legacyProjectId)
    {
      IEnumerable<SubscriptionData> subs = null;
      if (legacyProjectId > 0)
      {
        var projectRepo = factory.GetRepository<IProjectEvent>() as ProjectRepository;
        var p = projectRepo.GetProjectAndSubscriptions(legacyProjectId, DateTime.UtcNow.Date);

        if (p.Result != null && p.Result.ToList().Count() > 0)
        {
          // now get any project-based subs Landfill (23--> 19) and ProjectMonitoring (24 --> 20)
          subs = p.Result.ToList()
            .Where(x => x.ServiceTypeID != (int)ServiceTypeEnumNG.Unknown)
            .Select(x => new SubscriptionData("", x.ProjectUID, x.CustomerUID, x.ServiceTypeID, x.SubscriptionStartDate, x.SubscriptionEndDate));
          log.LogDebug("Executor: Loaded projectSubs {0}", JsonConvert.SerializeObject(subs));
        }
      }
      return subs;
    }


    // customer Man3Dpm(18-15)
    // this may be from the Projects CustomerUID OR the Assets OwningCustomerUID
    protected IEnumerable<SubscriptionData> LoadManual3DCustomerBasedSubs(string customerUid)
    {
      IEnumerable<SubscriptionData> subs = null;
      if (!string.IsNullOrEmpty(customerUid))
      {
        var subsRepo = factory.GetRepository<ISubscriptionEvent>() as SubscriptionRepository;
        var s = subsRepo.GetSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date);
        subs = s.Result.ToList()
          .Where(x => x.ServiceTypeID == (int)ServiceTypeEnumNG.Manual3DProjectMonitoring)
          .Select(x => new SubscriptionData("", "", x.CustomerUID, x.ServiceTypeID, x.StartDate, x.EndDate));
      }
      return subs;
    }


    // asset:3dProjMon (16 --> 13) 
    protected IEnumerable<SubscriptionData> LoadAssetSubs(string assetUid, DateTime validAtDate)
    {
      IEnumerable<SubscriptionData> subs = null;
      if (!string.IsNullOrEmpty(assetUid))
      {
        var subsRepo = factory.GetRepository<ISubscriptionEvent>() as SubscriptionRepository;
        var s = subsRepo.GetSubscriptionsByAsset(assetUid, validAtDate.Date);
        subs = s.Result.ToList().Where(x => x.ServiceTypeID == (int)ServiceTypeEnumNG.e3DProjectMonitoring).Distinct()
          .Select(x => new SubscriptionData(assetUid, "", x.CustomerUID, x.ServiceTypeID, x.StartDate, x.EndDate));
      }
      log.LogDebug("Executor: AssetSubs {0}", JsonConvert.SerializeObject(subs));
      return subs;
    }

    protected TWGS84Point[] ParseBoundaryData(string s)
    {
      var points = new List<TWGS84Point>();
      string[] pointsArray = s.Substring(9, s.Length - 11).Split(',');

      for (int i = 0; i < pointsArray.Length; i++)
      {
        double[] coordinates = new double[2];
        coordinates = pointsArray[i].Trim().Split(' ').Select(c => double.Parse(c)).ToArray();
        points.Add(new TWGS84Point(coordinates[1], coordinates[0]));
      }
      var fencePoints = new TWGS84Point[points.Count()];
      fencePoints = points.ToArray();

      return fencePoints;
    }
    #endregion

  }
}