using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositories;
using Repositories.DBModels;
using Repositories.ExtendedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TestUtility;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
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

    #region DataStorage

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

    protected Project project = null;
    protected AssetDeviceIds assetDevice = null;
    protected IEnumerable<SubscriptionData> projectSubs = null;
    protected IEnumerable<SubscriptionData> customerSubs = null;
    protected IEnumerable<SubscriptionData> assetSubs = null;


    protected void LoadProject(long legacyProjectId)
    {
      if (legacyProjectId > 0)
      {
        var projectRepo = factory.GetRepository<IProjectEvent>() as ProjectRepository;
        var p = projectRepo.GetProject(legacyProjectId);

        if (p.Result != null)
        {
          project = p.Result;
          log.LogDebug("AssetIdExecutor: Loaded project {0}", JsonConvert.SerializeObject(project));
        }
      }
    }


    protected void LoadAssetDevice(string radioSerial, string deviceType)
    {
      // todo cache and use repoFactory when complete
      if (!string.IsNullOrEmpty(radioSerial) && !string.IsNullOrEmpty(deviceType))
      {
        var deviceRepo = factory.GetRepository<IDeviceEvent>() as DeviceRepository;
        var a = deviceRepo.GetAssociatedAsset(radioSerial, deviceType);
        assetDevice = a.Result;
      }
      log.LogDebug("AssetIdExecutor: Loaded AssetDevice {0}", JsonConvert.SerializeObject(assetDevice));
    }


    protected void LoadProjectBasedSubs(long legacyProjectId)
    {
      if (legacyProjectId > 0)
      {
        var projectRepo = factory.GetRepository<IProjectEvent>() as ProjectRepository;
        var p = projectRepo.GetProjectAndSubscriptions(legacyProjectId, DateTime.UtcNow.Date);

        if (p.Result != null && p.Result.ToList().Count() > 0)
        {
          // now get any project-based subs Landfill (23--> 19) and ProjectMonitoring (24 --> 20)
          projectSubs = p.Result.ToList()
            .Where(x => x.ServiceTypeID != (int)ServiceTypeEnumNG.Unknown)
            .Select(x => new SubscriptionData("", x.ProjectUID, x.CustomerUID, x.ServiceTypeID, x.SubscriptionStartDate, x.SubscriptionEndDate));
          log.LogDebug("AssetIdExecutor: Loaded projectSubs {0}", JsonConvert.SerializeObject(projectSubs));
        }
      }
    }


    // customer Man3Dpm(18-15)
    // this may be from the Projects CustomerUID OR the Assets OwningCustomerUID
    protected void LoadManual3DCustomerBasedSubs(string customerUid)
    {
      if (!string.IsNullOrEmpty(customerUid))
      {
        var subsRepo = factory.GetRepository<ISubscriptionEvent>() as SubscriptionRepository;
        var s = subsRepo.GetSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date);
        customerSubs = s.Result.ToList()
          .Where(x => x.ServiceTypeID == (int)ServiceTypeEnumNG.Manual3DProjectMonitoring)
          .Select(x => new SubscriptionData("", "", x.CustomerUID, x.ServiceTypeID, x.StartDate, x.EndDate));
      }
    }


    // asset:3dProjMon (16 --> 13) 
    //  todo waiting for AssetSubs to be implemented in MDConsumer
    protected void LoadAssetSubs(string assetUid)
    {
      if (!string.IsNullOrEmpty(assetUid))
      {
        var subsRepo = factory.GetRepository<ISubscriptionEvent>() as SubscriptionRepository;
        //var s = subsRepo.GetSubscriptionsByAsset(assetUid, DateTime.UtcNow.Date);
        //assetSubs = s.Result.ToList().Where(x => x.ServiceTypeID == (int)ServiceTypeEnumNG.e3DProjectMonitoring)
        //  .Select(x => new SubscriptionData(x.AssetUID, "", x.CustomerUID, x.ServiceTypeID, x.SubscriptionStartDate, x.SubscriptionEndDate));
      }
      log.LogDebug("AssetIdExecutor: NOT IMPLEMENTED AssetSubs {0}", JsonConvert.SerializeObject(assetSubs));
    }
    #endregion

  }
}