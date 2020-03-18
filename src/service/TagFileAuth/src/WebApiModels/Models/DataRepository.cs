using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
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
    private readonly IAccountClient _accountClient;

    // We need to use ProjectSvc IProjectProxy as thats where the project data is
    private readonly IProjectProxy _projectProxy;

    // We need to use ProjectSvc IDeviceProxy 
    //    as when we get devices from IDeviceClient, 
    //    we need to write them into ProjectSvc local db to generate the shortRaptorAssetId
    private readonly IDeviceProxy _deviceProxy;

    public DataRepository(ILogger logger, IConfigurationStore configStore,
      IAccountClient accountClient, IProjectProxy projectProxy, IDeviceProxy deviceProxy)
    {
      _log = logger;
      _configStore = configStore;
      _accountClient = accountClient; 
      _projectProxy = projectProxy;
      _deviceProxy = deviceProxy;
    }

    #region account

    public async Task<int> GetDeviceLicenses(string customerUid)
    {
      DeviceLicenseResponseModel deviceLicenseResponseModel = null;
      try
      {
        if (!string.IsNullOrEmpty(customerUid))
          deviceLicenseResponseModel = await _accountClient.GetDeviceLicenses(customerUid);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      return deviceLicenseResponseModel?.Total ?? 0;
    }

    #endregion account


    #region project

    public async Task<ProjectData> GetProject(long shortRaptorProjectId)
    {
      ProjectData project = null;
      try
      {
        if (shortRaptorProjectId > 0)
          project = await _projectProxy.GetProject(shortRaptorProjectId);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      return project;
    }

    public async Task<ProjectData> GetProject(string projectUid)
    {
      ProjectData project = null;
      try
      {
        if (!string.IsNullOrEmpty(projectUid))
          project = await _projectProxy.GetProject(projectUid);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      return project;
    }

    public async Task<List<ProjectData>> GetProjects(string customerUid, DateTime validAtDate)
    {
      var projects = new List<ProjectData>();
      try
      {
        if (!string.IsNullOrEmpty(customerUid))
        {
          var p = await _projectProxy.GetProjects(customerUid);

          if (p != null)
          {
            projects = p
              .Where(x => x.StartDate <= validAtDate.Date && validAtDate.Date <= x.EndDate && !x.IsArchived)
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

    // manual import, no time, optional device
    public async Task<List<ProjectData>> CheckManualProjectIntersection(ProjectData project, double latitude, double longitude,
      DeviceData device = null)
    {
      var accountProjects = new List<ProjectData>();
      
      try
      {
        if (project != null && !string.IsNullOrEmpty(project.ProjectUID))
          accountProjects = (await _projectProxy.GetIntersectingProjects(project.CustomerUID, latitude, longitude, project.ProjectUID));

        if (accountProjects == null || !accountProjects.Any())
          return accountProjects;

        if (device != null && !string.IsNullOrEmpty(device.DeviceUID))
        {
          var deviceAssociatedWithProjects = (await _deviceProxy.GetProjects(device.DeviceUID));
          if (deviceAssociatedWithProjects.Any())
            return deviceAssociatedWithProjects.Where(p => p.ProjectUID == accountProjects[0].ProjectUID).ToList();
        }

        return accountProjects;
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }
    }

    public async Task<List<ProjectData>> CheckDeviceProjectIntersection(DeviceData device,
      double latitude, double longitude, DateTime timeOfPosition)
    {
      var accountProjects = new List<ProjectData>();

      try
      {
        if (device != null && !string.IsNullOrEmpty(device.DeviceUID))
          accountProjects = (await _projectProxy.GetIntersectingProjects(device.CustomerUID, latitude, longitude, timeOfPosition: timeOfPosition));

        if (!accountProjects.Any())
          return accountProjects;

        var deviceAssociatedWithProjects = (await _deviceProxy.GetProjects(device.DeviceUID));

        return deviceAssociatedWithProjects.Where(p => p.ProjectUID == accountProjects[0].ProjectUID).ToList();
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }
    }

    #endregion project


    #region device

    public async Task<DeviceData> GetDevice(string serialNumber)
    {
      DeviceData device = null;
      try
      {
        if (!string.IsNullOrEmpty(serialNumber))
          device = await _deviceProxy.GetDevice(serialNumber);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      return device;
    }

    public async Task<DeviceData> GetDevice(long shortRaptorAssetId)
    {
      DeviceData device = null;
      try
      {
        if (shortRaptorAssetId > 0)
          device = await _deviceProxy.GetDevice(shortRaptorAssetId);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 28, e.Message));
      }

      return device;
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
