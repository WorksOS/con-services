using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CCSS.Geometry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.WebApi.Common;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.Models.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models
{
  /// <summary>
  /// Represents abstract container for all request executors.
  /// Uses abstract factory pattern to separate executor logic from
  ///   controller logic for testability and possible executor version.
  /// </summary>
  public class DataRepository : IDataRepository
  {
    protected ILogger _log;

    private readonly ICwsAccountClient _cwsAccountClient;

    // We need to use ProjectSvc IProjectProxy as that's where the project data is
    private readonly IProjectInternalProxy _projectProxy;

    // We need to use ProjectSvc IDeviceProxy 
    //    as when we get devices from IDeviceClient, 
    //    we need to write them into ProjectSvc local db to generate the shortRaptorAssetId
    private readonly IDeviceInternalProxy _deviceProxy;

    // convert NE to LL using the projects CSIB via TRex
    private readonly ITRexCompactionDataProxy _tRexCompactionDataProxy;

    private IHeaderDictionary _mergedCustomHeaders;


    public DataRepository(ILogger log, ITPaaSApplicationAuthentication authorization, 
      IProjectInternalProxy projectProxy, IDeviceInternalProxy deviceProxy, ITRexCompactionDataProxy tRexCompactionDataProxy,
      IHeaderDictionary requestCustomHeaders)
    {
      _log = log;
      _projectProxy = projectProxy;
      _deviceProxy = deviceProxy;
      _tRexCompactionDataProxy = tRexCompactionDataProxy;
      _mergedCustomHeaders = requestCustomHeaders;

      foreach (var header in authorization.CustomHeaders())
      {
        _mergedCustomHeaders.Add(header);
      }
    }


    #region account
    /// <summary>
    /// We could use the ProjectSvc ICustomerProxy to then call IAccountClient. For now, just go straight to client.
    /// </summary>
    [Obsolete("Not used at present. As per SP, leave in case needed in future")]
    public async Task<int> GetDeviceLicenses(string customerUid)
    {
      if (string.IsNullOrEmpty(customerUid))
        return 0;

      try
      {
        return (await _cwsAccountClient.GetDeviceLicenses(new Guid(customerUid), _mergedCustomHeaders))?.Total ?? 0;
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
    public async Task<ProjectData> GetProject(string projectUid)
    {
      if (string.IsNullOrEmpty(projectUid))
        return null;
      try
      {
        return await _projectProxy.GetProject(projectUid, _mergedCustomHeaders);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "project", e.Message));
      }
    }

    // Need to obtain 1 polygon which this device (DeviceTRN) lat/long lies within
    public ProjectDataResult GetIntersectingProjectsForDevice(GetProjectAndAssetUidsRequest request, DeviceData device, out int errorCode)
    {
      errorCode = 0;
      var deviceProjects = new ProjectDataResult();
      if (device == null || string.IsNullOrEmpty(device.CustomerUID) || string.IsNullOrEmpty(device.DeviceUID))
        return deviceProjects;

      // returns whatever the cws rules mandate, and any conditions in projectSvc e.g. non-archived and 3dp-enabled type
      try
      {
        deviceProjects = _deviceProxy.GetProjectsForDevice(device.DeviceUID, _mergedCustomHeaders).Result;
        _log.LogDebug($"{nameof(GetIntersectingProjectsForDevice)}: deviceProjects {JsonConvert.SerializeObject(deviceProjects)}");

        if (deviceProjects?.Code != 0 || !deviceProjects.ProjectDescriptors.Any())
        {
          errorCode = 48;
          return deviceProjects;
        }

        var intersectingProjects = new ProjectDataResult();
        foreach (var project in deviceProjects.ProjectDescriptors)
        {
          if (project.ProjectType.HasFlag(CwsProjectType.AcceptsTagFiles)
              && !project.IsArchived)
          {
            if (!request.HasLatLong && request.HasNE)
            {
              var convertedLL = this.ConvertNEtoLL(project.ProjectUID, request.Northing.Value, request.Easting.Value).Result;
              if (convertedLL != null)
              {
                request.Longitude = convertedLL.ConversionCoordinates[0].X;
                request.Latitude = convertedLL.ConversionCoordinates[0].Y;
              }
            }
            if (request.HasLatLong && PolygonUtils.PointInPolygon(project.ProjectGeofenceWKT, request.Latitude, request.Longitude))
              intersectingProjects.ProjectDescriptors.Add(project);
          }
        }
        if (!intersectingProjects.ProjectDescriptors.Any())
          errorCode = 44;
        return intersectingProjects;
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

    // Need to obtain cws: DeviceTRN
    public async Task<DeviceData> GetDevice(string serialNumber)
    {
      if (string.IsNullOrEmpty(serialNumber))
        return null;
      try
      {
        return await _deviceProxy.GetDevice(serialNumber, _mergedCustomHeaders);
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "device", e.Message));
      }
    }

    #endregion device

    #region coordSystem

    public async Task<CoordinateConversionResult> ConvertNEtoLL(string projectUid, double northing, double easting)
    {
      var request = new CoordinateConversionRequest(new Guid(projectUid), TwoDCoordinateConversionType.NorthEastToLatLon, new[] {new TwoDConversionCoordinate(easting, northing)});

      try
      {
        var result = await _tRexCompactionDataProxy.SendDataPostRequest<CoordinateConversionResult, CoordinateConversionRequest>(request, "/coordinateconversion", _mergedCustomHeaders);
        _log.LogDebug($"{nameof(ConvertNEtoLL)}: CoordinateConversionRequest {JsonConvert.SerializeObject(request)} CoordinateConversionResult {JsonConvert.SerializeObject(result)}");
        if (result?.ConversionCoordinates == null || result.ConversionCoordinates.Length != 1 
                                                  || result.ConversionCoordinates[0].X < -180 || result.ConversionCoordinates[0].X > 180 
                                                  || result.ConversionCoordinates[0].Y < -90 || result.ConversionCoordinates[0].Y > 90)
          return null;

        return result;
      }
      catch (Exception e)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.InternalProcessingError, 17, "tRex", e.Message));
      }
    }

    #endregion coordSystem
  }
}
