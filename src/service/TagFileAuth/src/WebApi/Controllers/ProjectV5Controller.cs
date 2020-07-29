using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.RadioSerialMap;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Utilities;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Project controller for earthworks cutFill and tagFile validation
  /// </summary>
  public class ProjectV5Controller : BaseController<ProjectV5Controller>
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProjectV5Controller()
    { }

    /// <summary>
    /// This endpoint is used by CTCTs Earthworks endpoint in the 3dpService.
    ///   That endpoint allows an operator, once or twice a day
    ///      to obtain production data to enable it to generate a Cut/fill or other map. 
    ///   This step tries to identify a unique projectUid from knowledge of the device, also the devices customer.
    /// 
    /// EC and/or radio, location are provided.
    /// 
    /// Returns the devices customer and the ProjectUid 
    ///     which belongs to the devices Customer and 
    ///     whose boundary the location is inside
    /// 
    /// Uses the tag file executor as the logic is basically the same as an 'auto' tag file except:
    ///     this endpoint does not pass in NEE,
    ///     this endpoint requires customer in response (sent to also but probably not required)
    /// </summary>
    [Route("internal/v5/project/getUidsEarthWorks")]
    [HttpPost]
    public async Task<GetProjectUidsResult> GetProjectUidsEarthWorks([FromBody]GetProjectUidsEarthWorksRequest request)
    {
      Logger.LogDebug($"{nameof(GetProjectUidsEarthWorks)}: request: {JsonConvert.SerializeObject(request)}");
      request.Validate();
  
      var executor = RequestExecutorContainer.Build<ProjectUidsExecutor>(Logger, ConfigStore, Authorization, ProjectProxy, DeviceProxy, TRexCompactionDataProxy, RequestCustomHeaders);
      var getProjectUidsRequest = new GetProjectUidsRequest(string.Empty, request.PlatformSerial, request.Latitude, request.Longitude);
      var result = await executor.ProcessAsync(getProjectUidsRequest) as GetProjectUidsResult;

      Logger.LogResult(nameof(GetProjectUidsEarthWorks), request, result);
      return result;
    }

    /// <summary>
    /// This endpoint is used by TRex to identify a project to assign a tag file to.
    ///      It is called for each tag file, with as much information as is available e.g. device; location; projectUid
    ///
    ///      If the ProjectUid is provided, this means a user is attempting to 'manually' imported the file into this Project via the UI.
    ///
    ///      If no ProjectUid is provided, this means the tag file is coming from an 'Auto' source, either from
    ///         a) the TCC GCS endpoint, via harvester, via the 'auto' 3dp endpoint
    ///         b) or direct from a device, currently via the 'direct' 3dp endpoint
    /// 
    ///      Attempts to identify a unique project which the tag file could be applied to, also to identify the device
    ///      "Only tag files from devices which have been claimed (i.e Device has logged with appropriate account) will be considered for manual or auto tag file ingress"
    /// 
    ///     On error returns:
    ///         If validation of request fails, returns BadRequest plus a unique error code and message
    ///         If it fails to identify/verify a project, returns BadRequest plus a unique error code and message
    ///         Note that it is not an error if it fails to identify the device as the caller will determine from context if this is an issue
    ///         If something internal has gone wrong, which may be retry-able e.g. database unavailable
    ///            it returns InternalError plus a unique error code and message
    ///
    /// Workflows:  #1 Manual import (has a projectUid)
    ///             #2 TFHarvester Auto import             
    ///             #3 DirectSubmission from CTCT device (appears the same as #2)
    /// Note that for this endpoint we use Guids only to identify projects etc
    /// </summary>
    /// <param name="request">Details of the project, device, also location and its date time</param>
    /// <param name="customRadioSerialProjectMap">"temp mock data"</param>
    /// <returns>
    /// The project Uid which satisfies spatial requirements
    ///      and possibly deviceUid
    ///      otherwise errorCode etc:
    ///         Validation errors  HttpStatusCode.BadRequest:
    ///                     SerializationError (-2)
    ///                    21 "Latitude should be between -90 degrees and 90 degrees"
    ///                    22 "Longitude should be between -180 degrees and 180 degrees"
    ///                    36 "ProjectUid is present, but invalid"
    ///                    37 "Platform serial number must be provided"
    ///         Errors: HttpStatusCode.InternalServerError (retry-able?)
    ///           17  "A problem occurred accessing a service. Service: {0} Exception: {1}"
    ///            
    ///         HttpStatusCode.OK   Response Code:
    ///            18 "Manual Import: Unable to determine lat/long from northing/easting position"
    ///            38 "Manual Import: Unable to find the Project requested"
    ///            41 "Manual Import: project does not intersect the location provided"
    ///            43 "Manual Import: cannot import to an archived project"
    ///            44 "No projects found at the location provided"
    ///            47 "Unable to identify the device by this serialNumber"
    ///            48 "No projects found for this device"
    ///            49 "More than 1 project meets the location requirements"
    ///            53 "Manual Import: cannot import to a project which doesn't accept tag files"
    /// 
    ///           100 "Unable to locate device by serialNumber in cws" (3100 in TFG)
    ///           102 "Unable to locate any account for the device in cws"
    ///           103 "There is >1 active account for the device in cws"
    ///           105 "Unable to locate projects for device in cws"
    ///           124 "A problem occurred at the {0} endpoint. Exception: {1}" // this comes from ProjectSvc
    ///     
    /// </returns>
    [Route("internal/v5/project/getUids")]
    [HttpPost]
    public async Task<GetProjectUidsResult> GetProjectUids(
      [FromBody]GetProjectUidsRequest request,
      [FromServices] ICustomRadioSerialProjectMap customRadioSerialProjectMap)
    {
      Logger.LogDebug($"{nameof(GetProjectUids)}: request:{JsonConvert.SerializeObject(request)}");
      request.Validate();

      var executor = RequestExecutorContainer.Build<ProjectUidsExecutor>(Logger, ConfigStore, Authorization, ProjectProxy, DeviceProxy, TRexCompactionDataProxy, RequestCustomHeaders);

      var result = await executor.ProcessAsync(request) as GetProjectUidsResult;

      Logger.LogResult(nameof(GetProjectUids), request, result);
      return result;
    }

  }
}
