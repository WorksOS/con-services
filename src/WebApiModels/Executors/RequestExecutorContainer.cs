using Microsoft.Extensions.Logging;
using Repositories;
using Repositories.DBModels;
using Repositories.ExtendedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    
    protected Project LoadProject(long legacyProjectId)
    {
      Project project = null;
      if (legacyProjectId > 0)
      {
        var projectRepo = factory.GetRepository<IProjectEvent>() as ProjectRepository;
        var p = projectRepo.GetProject(legacyProjectId);
        if (p != null) project = p.Result;
      }
      return project;
    }

    protected IEnumerable<Project> LoadProjects(string customerUid, DateTime validAtDate)
    {
      IEnumerable<Project> projects = null;
      if (customerUid != null)
      {
        var projectRepo = factory.GetRepository<IProjectEvent>() as ProjectRepository;
        var p = projectRepo.GetProjectsForCustomer(customerUid);

        if (p != null)
        {
          projects = p.Result.ToList()
            .Where(x => x.StartDate <= validAtDate.Date && validAtDate.Date <= x.EndDate);
        }
      }
      return projects;
    }

    protected AssetDeviceIds LoadAssetDevice(string radioSerial, string deviceType)
    {
      AssetDeviceIds assetDevice = null;
      if (!string.IsNullOrEmpty(radioSerial) && !string.IsNullOrEmpty(deviceType))
      {
        var deviceRepo = factory.GetRepository<IDeviceEvent>() as DeviceRepository;
        var a = deviceRepo.GetAssociatedAsset(radioSerial, deviceType);
        if (a != null)
          assetDevice = a.Result;
      }
      return assetDevice;
    }

    protected Customer LoadCustomer(string customerUid)
    {
      // TFA is only interested in customer and dealer types
      Customer customer = null;
      if (!string.IsNullOrEmpty(customerUid))
      {
        var customerRepo = factory.GetRepository<ICustomerEvent>() as CustomerRepository;
        var a = customerRepo.GetCustomer(new Guid(customerUid));
        if (a != null &&
          (a.Result.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer || a.Result.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Dealer)
          )
          customer = a.Result;
      }
      return customer;
    }

    protected CustomerTccOrg LoadCustomerByTccOrgId(string tccOrgUid)
    {
      // TFA is only interested in customer and dealer types
      CustomerTccOrg customer = null;
      if (!string.IsNullOrEmpty(tccOrgUid))
      {
        var customerRepo = factory.GetRepository<ICustomerEvent>() as CustomerRepository;
        var a = customerRepo.GetCustomerWithTccOrg(tccOrgUid);
        if (a != null &&
          (a.Result.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Customer || a.Result.CustomerType == VisionLink.Interfaces.Events.MasterData.Models.CustomerType.Dealer)
          )
          customer = a.Result;
      }
      return customer;
    }

    protected CustomerTccOrg LoadCustomerByCustomerUID(string customerUid)
    {
      // TFA is only interested in customer and dealer types
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
      return customer;
    }

    protected Asset LoadAsset(long legacyAssetId)
    {
      Asset asset = null;
      if (legacyAssetId> 0)
      {
        var assetRepo = factory.GetRepository<IAssetEvent>() as AssetRepository;
        var a = assetRepo.GetAsset(legacyAssetId);
        if ( a.Result != null) asset = a.Result;
      }
      return asset;
    }


    // customer Man3Dpm(18-15)
    // this may be from the Projects CustomerUID OR the Assets OwningCustomerUID
    protected IEnumerable<SubscriptionData> LoadManual3DCustomerBasedSubs(string customerUid, DateTime validAtDate)
    {
      IEnumerable<SubscriptionData> subs = null;
      if (!string.IsNullOrEmpty(customerUid))
      {
        var subsRepo = factory.GetRepository<ISubscriptionEvent>() as SubscriptionRepository;
        var s = subsRepo.GetSubscriptionsByCustomer(customerUid, validAtDate);
        if (s.Result != null)
        {
          subs = s.Result.ToList()
          .Where(x => x.ServiceTypeID == (int)ServiceTypeEnumNG.Manual3DProjectMonitoring)
          .Select(x => new SubscriptionData("", "", x.CustomerUID, x.ServiceTypeID, x.StartDate, x.EndDate));
        }
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
        if (s.Result != null)
        {
          subs = s.Result.ToList().Where(x => x.ServiceTypeID == (int)ServiceTypeEnumNG.e3DProjectMonitoring).Distinct()
          .Select(x => new SubscriptionData(assetUid, "", x.CustomerUID, x.ServiceTypeID, x.StartDate, x.EndDate));
        }
      }
      return subs;
    }

    protected TWGS84Point[] ParseBoundaryData(string s)
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
    #endregion

  }
}