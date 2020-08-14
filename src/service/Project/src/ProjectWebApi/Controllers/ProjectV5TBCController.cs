using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project controller v5TBC
  /// This is used by BusinessCenter. 
  ///     The signature must be retained.
  ///     BC is compatible with TID  
  /// </summary>
  public class ProjectV5TBCController : BaseController<ProjectV5TBCController>
  {
    /// <summary>
    /// Gets or sets the httpContextAccessor.
    /// </summary>
    protected readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProjectV5TBCController(IHttpContextAccessor httpContextAccessor)
    {
      this.httpContextAccessor = httpContextAccessor;
    }

    #region projects
    
    /// <summary>
    /// Called by TBC only.
    /// Gets list of active projects for a customer.
    ///    includes legacy short ProjectId, both archived and non-archived projects
    ///
    /// {"5844":{"isArchived":false,"name":"MZE","projectTimeZone":"W. Europe Standard Time","projectType":0,"projectTypeName":"Standard",
    ///          "startDate":"2010-10-04T00:00:00.0000000","endDate":"2019-05-03T00:00:00.0000000","projectUid":"ff6ce75d-a1dd-46fb-9ec8-843a69525eca",
    ///          "projectGeofenceWKT":"POLYGON((6.97239276473992 46.2469262095446,6.96490403717033 46.2501907126296,6.967006889038 46.2508881040108,6.97479602401726 46.2479797749553,6.97239276473992 46.2469262095446))",
    ///          "legacyProjectId":5844,"customerUID":"f584aa24-5e73-11e6-b7ed-005056831552","legacyCustomerId":"199061","coordinateSystemFileName":"Deponie Monthey 120524.dc"},
    ///  "5929":{"isArchived":true,"name":"Goliat 2017-06-29T02:31:58Z","projectTimeZone":"New Zealand Standard Time","projectType":2,"projectTypeName":"ProjectMonitoring",
    ///          "startDate":"2015-03-18T00:00:00.0000000","endDate":"2017-07-13T00:00:00.0000000","projectUid":"8bb0bb68-98f3-403d-99b6-33e3a1395559",
    ///          "projectGeofenceWKT":"POLYGON((23.801560048285 70.7101018693985,23.8047892449349 70.7102288034504,23.8068190736042 70.7094055573952,23.8032401133938 70.7088362380994,23.801912599436 70.7093792489133,23.801560048285 70.7101018693985,23.801560048285 70.7101018693985))",
    ///          "legacyProjectId":5929,"customerUID":"f584aa24-5e73-11e6-b7ed-005056831552","legacyCustomerId":"199061","coordinateSystemFileName":"Goliat.dc"},
    /// </summary>
    [Route("api/v5/project")]
    [Route("api/v2/project")] // TBC has route hardcoded
    [HttpGet]
    public async Task<Dictionary<long, ProjectDataTBCSingleResult>> GetProjects()
    {
      Logger.LogInformation($"{nameof(GetProjects)} ");

      var projects = await ProjectRequestHelper.GetProjectListForCustomer(new Guid(CustomerUid), new Guid(UserId), Logger, ServiceExceptionHandler, CwsProjectClient,
        projectType: CwsProjectType.AcceptsTagFiles, status: null, onlyAdmin: false, includeBoundaries: true, customHeaders: customHeaders);
      
      var result = new Dictionary<long, ProjectDataTBCSingleResult>();
      foreach (var project in projects)
      {
        var projectTbc = AutoMapperUtility.Automapper.Map<ProjectDataTBCSingleResult>(project);
        result[projectTbc.LegacyProjectId] = projectTbc;
      }

      Logger.LogDebug($"{nameof(GetProjects)}: completed successfully. projects {JsonConvert.SerializeObject(result)}");
      return result;
    }

    /// <summary>
    /// Called by TBC only.
    /// TBC CreateProject. Footprint must remain the same as VSS 2nd gen: 
    ///     POST /t/trimble.com/vss-projectmonitoring/1.0/api/v5/projects HTTP/1.1
    ///     Body: {"CoordinateSystem":{"FileSpaceID":"u927f3be6-7987-4944-898f-42a088da94f2","Path":"/BC Data/Sites/Svevia Vargarda","Name":"Svevia Vargarda.dc","CreatedUTC":"0001-01-01T00:00:00Z"},"ProjectType":2,"StartDate":"2018-04-11T00:00:00Z","EndDate":"2018-05-11T00:00:00Z","ProjectName":"Svevia Vargarda","TimeZoneName":"Romance Standard Time","BoundaryLL":[{"Latitude":58.021890362243404,"Longitude":12.778613775843427},{"Latitude":58.033751276149488,"Longitude":12.783760539866186},{"Latitude":58.035972399195963,"Longitude":12.812762795456051},{"Latitude":58.032604039701752,"Longitude":12.841590546413993},{"Latitude":58.024515931878035,"Longitude":12.842137844178708},{"Latitude":58.016620613589389,"Longitude":12.831491715508857},{"Latitude":58.0128142214101,"Longitude":12.793567555971942},{"Latitude":58.021890362243404,"Longitude":12.778613775843427}],"CustomerUID":"323e4a34-56aa-11e5-a400-0050569757e0","CustomerName":"MERINO CONSTRUCTION"}
    ///     Result: HttpStatusCode.Created
    ///            {"id":6964} 
    /// 
    ///   This US only handles happy path. ServiceExceptions will be mapped in a future US.
    /// Yes, this path appears to be plural from old orangeApp code 
    /// </summary>
    [Route("api/v5/projects")]
    [Route("api/v2/projects")] // TBC has route hardcoded
    [HttpPost]
    public async Task<ReturnLongV5Result> CreateProjectTBC([FromBody] CreateProjectV5Request projectRequest)
    {
      if (projectRequest == null)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 81);

      Logger.LogInformation($"{nameof(CreateProjectTBC)} projectRequest: {JsonConvert.SerializeObject(projectRequest)}");

      var projectValidation = MapV5Models.MapCreateProjectV5RequestToProjectValidation(projectRequest, CustomerUid);

      projectRequest.CoordinateSystem =
        ProjectDataValidator.ValidateBusinessCentreFile(projectRequest.CoordinateSystem);

      // Read CoordSystem file from TCC as byte[]. 
      projectValidation.CoordinateSystemFileContent =
        await TccHelper
          .GetFileContentFromTcc(projectRequest.CoordinateSystem,
            Logger, ServiceExceptionHandler, FileRepo).ConfigureAwait(false);

      var validationResult
        = await WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainerFactory
            .Build<ValidateProjectExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
              CustomerUid, UserId, null, customHeaders,
              Productivity3dV1ProxyCoord, cwsProjectClient: CwsProjectClient)
            .ProcessAsync(projectValidation)
        );
      if (validationResult.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, validationResult.Code);

      var result = (await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<CreateProjectTBCExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            CustomerUid, UserId, null, customHeaders,
            Productivity3dV1ProxyCoord, dataOceanClient: DataOceanClient, authn: Authorization,
            cwsProjectClient: CwsProjectClient, cwsDeviceClient: CwsDeviceClient,
            cwsProfileSettingsClient: CwsProfileSettingsClient)
          .ProcessAsync(projectValidation)) as ProjectV6DescriptorsSingleResult
        );
      
      Logger.LogInformation($"{nameof(CreateProjectTBC)}: completed successfully. ShortProjectId {result.ProjectDescriptor.ShortRaptorProjectId}");
      return ReturnLongV5Result.CreateLongV5Result(HttpStatusCode.Created, result.ProjectDescriptor.ShortRaptorProjectId);
    }

    #endregion projects


    #region TCCAuthorization

    /// <summary>
    /// Called by TBC only.
    /// TBC ValidateTCCAuthorization. This validates that 
    ///      a) the customer has access to the TCC organization and 
    ///      b) that the Folder structure exists in TCC.
    /// Footprint must remain the same as CGen: 
    ///     POST /t/trimble.com/vss-projectmonitoring/1.0/api/v5/preferences/tcc HTTP/1.1
    ///     Body: {"organization":"vssnz19"}     
    ///     Response: {"success":true}
    /// 
    /// Happy path only to be handled in this US. ServiceExceptions will be mapped in a future US.
    /// However this is a faillure Response:
    ///     {"status":500,"message":"invalidUser001\r\n\r\n","errorcode":1000,"link":null}
    /// 
    /// </summary>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v5/preferences/tcc")]
    [Route("api/v2/preferences/tcc")] // TBC has route hardcoded
    [HttpPost]
    public ReturnSuccessV5Result ValidateTccAuthorization(
      [FromBody] ValidateTccAuthorizationRequest tccAuthorizationRequest)
    {
      if (tccAuthorizationRequest == null)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 86);

      Logger.LogInformation($"{nameof(ValidateTccAuthorization)}: completed successfully");
      return ReturnSuccessV5Result.CreateReturnSuccessV5Result(HttpStatusCode.OK, true);
    }

    #endregion TCCAuthorization
  }
}
