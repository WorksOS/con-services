using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models
{
  /// <summary>
  /// Represents abstract container for all request executors.
  /// Uses abstract factory pattern to separate executor logic from
  ///   controller logic for testability and possible executor versioning.
  /// </summary>
  public class DataRepository : IDataRepository
  {
    private ILogger _log;
    private IConfigurationStore _configStore;

    // We could use the ProjectSvc ICustomerProxy to then call IAccountClient, just go straight to client
    private readonly ICwsAccountClient _cwsAccountClient;

    // We need to use ProjectSvc IProjectProxy as thats where the project data is
    private readonly IProjectProxy _projectProxy;

    // We need to use ProjectSvc IDeviceProxy 
    //    as when we get devices from IDeviceClient, 
    //    we need to write them into ProjectSvc local db to generate the shortRaptorAssetId
    private readonly IDeviceProxy _deviceProxy;

    public DataRepository(ILogger logger, IConfigurationStore configStore,
      ICwsAccountClient cwsAccountClient, IProjectProxy projectProxy, IDeviceProxy deviceProxy)
    {
      _log = logger;
      _configStore = configStore;
      _cwsAccountClient = cwsAccountClient;
      _projectProxy = projectProxy;
      _deviceProxy = deviceProxy;
    }

    #region account

    public async Task<int> GetDeviceLicenses(string customerUid)
    {
      if (string.IsNullOrEmpty(customerUid))
        return 0;

      try
      {
        return (await _cwsAccountClient.GetDeviceLicenses(new Guid(customerUid)))?.Total ?? 0;
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "cwsAccount", e.Message));
      }
    }

    #endregion account


    #region project

    public async Task<ProjectData> GetProject(long shortRaptorProjectId)
    {
      if (shortRaptorProjectId < 1)
        return null;
      try
      {
        return await _projectProxy.GetProjectApplicationContext(shortRaptorProjectId);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "project", e.Message));
      }
    }

    public async Task<ProjectData> GetProject(string projectUid)
    {
      if (string.IsNullOrEmpty(projectUid))
        return null;
      try
      {
        return await _projectProxy.GetProjectApplicationContext(projectUid);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "project", e.Message));
      }
    }

    public async Task<List<ProjectData>> GetProjects(string customerUid, DateTime validAtDate)
    {
      if (string.IsNullOrEmpty(customerUid))
        return null;
      try
      {
        var p = await _projectProxy.GetProjects(customerUid);
        if (p != null)
        {
          // CCSSSCON-207, what should be the marketing requirements for dates here?
          return p
              .Where(x => !x.IsArchived)
              .ToList();
        }
        return null;
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "project", e.Message));
      }
    }

    public async Task<List<ProjectData>> GetProjectsAssociatedWithDevice(string customerUid, string deviceUid, DateTime validAtDate)
    {
      if (string.IsNullOrEmpty(customerUid) || string.IsNullOrEmpty(deviceUid))
        return null;
      try
      {
        var p = await _deviceProxy.GetProjectsForDevice(deviceUid);
        if (p != null)
        {
          // CCSSSCON-207, what should be the marketing requirements for dates here?
          return p
            .Where(x => (string.Compare(x.CustomerUID, customerUid, true) == 0) && !x.IsArchived)
            .ToList();
        }
        return null;
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "device", e.Message));
      }
    }

    // manual import, no time, optional device
    public async Task<List<ProjectData>> GetIntersectingProjectsForManual(ProjectData project, double latitude, double longitude,
      DeviceData device = null)
    {
      var accountProjects = new List<ProjectData>();
      if (project == null || string.IsNullOrEmpty(project.ProjectUID))
        return accountProjects;

      try
      {
        accountProjects = (await _projectProxy.GetIntersectingProjectsApplicationContext(project.CustomerUID, latitude, longitude, project.ProjectUID));
        // should not be possible to get > 1 as call was limited by the projectUid       
        if (accountProjects == null || accountProjects.Count() != 1)
          return accountProjects;
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "project", e.Message));
      }

      if (device == null || string.IsNullOrEmpty(device.DeviceUID))
        return accountProjects;

      try
      {
        var projectsAssociatedWithDevice = (await _deviceProxy.GetProjectsForDevice(device.DeviceUID));
        if (projectsAssociatedWithDevice.Any())
          return projectsAssociatedWithDevice.Where(p => p.ProjectUID == accountProjects[0].ProjectUID).ToList();

        return accountProjects;
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "device", e.Message));
      }
    }

    public async Task<List<ProjectData>> GetIntersectingProjectsForDevice(DeviceData device,
      double latitude, double longitude)
    {
      var accountProjects = new List<ProjectData>();
      if (device == null || string.IsNullOrEmpty(device.CustomerUID) || string.IsNullOrEmpty(device.DeviceUID))
        return accountProjects;

      // what projects does this customer have which intersect the lat/long?
      try
      {
        accountProjects = (await _projectProxy.GetIntersectingProjectsApplicationContext(device.CustomerUID, latitude, longitude));
        if (!accountProjects.Any())
          return accountProjects;
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "project", e.Message));
      }

      // what projects does this device have visibility to?
      try
      {
        var projectsAssociatedWithDevice = (await _deviceProxy.GetProjectsForDevice(device.DeviceUID));
        var intersection = projectsAssociatedWithDevice.Select(dp => dp.ProjectUID).Intersect(accountProjects.Select(ap => ap.ProjectUID));
        return projectsAssociatedWithDevice.Where(p => intersection.Contains(p.ProjectUID)).ToList();
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
          ContractExecutionStatesEnum.InternalProcessingError, 17, "device", e.Message));
      }
    }

    #endregion project


    #region device

    // Need to get cws: DeviceTRN, AccountTrn, DeviceType, deviceName, Status ("ACTIVE" etal?), serialNumber
    // and shortRaptorAssetId(localDB)
    public async Task<DeviceData> GetDevice(string serialNumber)
    {
      if (string.IsNullOrEmpty(serialNumber))
        return null;
      try
      {
        return await _deviceProxy.GetDevice(serialNumber);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "device", e.Message));
      }
    }

    public async Task<DeviceData> GetDevice(int shortRaptorAssetId)
    {
      if (shortRaptorAssetId < 1)
        return null;
      try
      {
        return await _deviceProxy.GetDevice(shortRaptorAssetId);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "device", e.Message));
      }
    }

    #endregion device


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
