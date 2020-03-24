using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Customer controller v1
  ///     for the UI to get customer list etc as we have no CustomerSvc yet
  /// </summary>
  public class DeviceV1Controller : ProjectBaseController
  {  

    private readonly ICwsDeviceClient cwsDeviceClient;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public DeviceV1Controller(IConfigurationStore configStore, ICwsDeviceClient cwsDeviceClient)
      : base(configStore)
    {
      this.cwsDeviceClient = cwsDeviceClient;
    }

    /// <summary>
    /// Gets device by serialNumber, including Uid and shortId 
    /// </summary>
    [Route("api/v1/device/serialnumber")]
    [HttpGet]
    public async Task<DeviceDataSingleResult> GetDeviceBySerialNumber([FromQuery]  string serialNumber)
    {
      // todoMaverick executor and validation
      var deviceResponseModel = await cwsDeviceClient.GetDeviceBySerialNumber(serialNumber);
      if (deviceResponseModel == null)
        throw new NotImplementedException();

      var deviceFromRepo = await DeviceRepo.GetDevice(deviceResponseModel.Id);

      var deviceDataResult = new DeviceDataSingleResult()
      {
        DeviceDescriptor = new DeviceData()
        {
          //CustomerUID = deviceResponseModel.AccountId, // todoMaverick
          DeviceUID = deviceResponseModel.Id,
          //DeviceName = deviceResponseModel.DeviceName,  // todoMaverick
          SerialNumber = deviceResponseModel.SerialNumber,
          //Status = deviceResponseModel.Status,  // todoMaverick
          ShortRaptorAssetId = deviceFromRepo.ShortRaptorAssetId
        }
      };
      return deviceDataResult;
    }

    /// <summary>
    /// Gets device by serialNumber, including Uid and shortId 
    /// </summary>
    [Route("api/v1/device/shortid")]
    [HttpGet]
    public async Task<DeviceDataSingleResult> GetDevice([FromQuery] int shortRaptorAssetId)
    {
      // todoMaverick executor and validation
      var deviceFromRepo = await DeviceRepo.GetDevice(shortRaptorAssetId); 
      
      var deviceResponseModel = await cwsDeviceClient.GetDeviceByDeviceUid(deviceFromRepo.DeviceUID);
      if (deviceResponseModel == null)
        throw new NotImplementedException();

      
      var deviceDataResult = new DeviceDataSingleResult()
      {
        DeviceDescriptor = new DeviceData()
        {
          //CustomerUID = deviceResponseModel.AccountId,  // todoMaverick
          DeviceUID = deviceResponseModel.Id,
          //DeviceName = deviceResponseModel.DeviceName,  // todoMaverick
          SerialNumber = deviceResponseModel.SerialNumber,
          //Status = deviceResponseModel.Status,  // todoMaverick
          ShortRaptorAssetId = deviceFromRepo.ShortRaptorAssetId
        }
      };
      return deviceDataResult;
    }

    /// <summary>
    /// Gets device by serialNumber, including Uid and shortId 
    /// </summary>
    [Route("api/v1/device/{deviceUid}/projects")]
    [HttpGet]
    public async Task<ProjectDataResult> GetProjectsForDevice(string deviceUid)
    {
      // todoMaverick executor and validation
      var projectsFromCws = await cwsDeviceClient.GetProjectsForDevice(deviceUid);
      if (cwsDeviceClient == null)
        throw new NotImplementedException();

      var projectDataResult = new ProjectDataResult();
      foreach(var projectCws in projectsFromCws.Projects)
      {
        var project = AutoMapperUtility.Automapper.Map<ProjectData>(projectCws);
        
        // todoMaverick fill in blanks from local DB ESPECIALLY shortRaptorProjectId
        //ShortRaptorProjectId
        //ProjectTimeZoneIana
        //StartDate
        //EndDate
        //GeometryWKT
        //CoordinateSystemFileName
        //CoordinateSystemLastActionedUTC
        //IsArchived

        projectDataResult.ProjectDescriptors.Add(project);
      };      

      return projectDataResult;
    }

  }
}

