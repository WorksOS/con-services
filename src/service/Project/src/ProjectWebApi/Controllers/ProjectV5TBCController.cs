using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Extensions;
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

    /// todoJeannie we're not sure which endpoints TBC uses to get its projects.
    ///       Watch in testing to see what it needs
   

    /// <summary>
    /// Gets a project for a customer.
    ///    includes legacyId
    /// </summary>
    [Route("api/v5/projects/{id}")]
    [HttpGet]
    public async Task<ProjectDataTBCSingleResult> GetProjectByShortId(long id)
    {
      Logger.LogInformation("GetProjectByShortId");

      var project =  await ProjectRequestHelper.GetProjectForCustomer(new Guid(CustomerUid), new Guid(UserId), id, Logger, ServiceExceptionHandler, CwsProjectClient, customHeaders);
      var projectTbc = AutoMapperUtility.Automapper.Map<ProjectDataTBCSingleResult>(project);
      projectTbc.LegacyProjectId = (Guid.TryParse(project.ProjectId, out var g) ? g.ToLegacyId() : 0);
      if (projectTbc.LegacyProjectId == 0)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 140);

      Logger.LogDebug($"{nameof(GetProjectByShortId)}: completed successfully. projectTbc {projectTbc}");
      
      return projectTbc;
    }

    // POST: api/v5/projects
    /// <summary>
    /// TBC CreateProject. Footprint must remain the same as VSS 2nd gen: 
    ///     POST /t/trimble.com/vss-projectmonitoring/1.0/api/v5/projects HTTP/1.1
    ///     Body: {"CoordinateSystem":{"FileSpaceID":"u927f3be6-7987-4944-898f-42a088da94f2","Path":"/BC Data/Sites/Svevia Vargarda","Name":"Svevia Vargarda.dc","CreatedUTC":"0001-01-01T00:00:00Z"},"ProjectType":2,"StartDate":"2018-04-11T00:00:00Z","EndDate":"2018-05-11T00:00:00Z","ProjectName":"Svevia Vargarda","TimeZoneName":"Romance Standard Time","BoundaryLL":[{"Latitude":58.021890362243404,"Longitude":12.778613775843427},{"Latitude":58.033751276149488,"Longitude":12.783760539866186},{"Latitude":58.035972399195963,"Longitude":12.812762795456051},{"Latitude":58.032604039701752,"Longitude":12.841590546413993},{"Latitude":58.024515931878035,"Longitude":12.842137844178708},{"Latitude":58.016620613589389,"Longitude":12.831491715508857},{"Latitude":58.0128142214101,"Longitude":12.793567555971942},{"Latitude":58.021890362243404,"Longitude":12.778613775843427}],"CustomerUID":"323e4a34-56aa-11e5-a400-0050569757e0","CustomerName":"MERINO CONSTRUCTION"}
    ///     Result: HttpStatusCode.Created
    ///            {"id":6964} 
    /// 
    ///   This US only handles happy path. ServiceExceptions will be mapped in a future US. 
    /// </summary>
    [Route("api/v5/projects")]
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
            cwsDesignClient: CwsDesignClient,
            cwsProfileSettingsClient: CwsProfileSettingsClient)
          .ProcessAsync(projectValidation)) as ProjectV6DescriptorsSingleResult
        );
      
      Logger.LogDebug($"{nameof(CreateProjectTBC)}: completed successfully. ShortRaptorProjectId {result.ProjectDescriptor.ShortRaptorProjectId}");
      return ReturnLongV5Result.CreateLongV5Result(HttpStatusCode.Created, result.ProjectDescriptor.ShortRaptorProjectId);
    }

    #endregion projects


    #region TCCAuthorization

    // POST: api/v5/preferences/tcc
    /// <summary>
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
    [HttpPost]
    public ReturnSuccessV5Result ValidateTccAuthorization(
      [FromBody] ValidateTccAuthorizationRequest tccAuthorizationRequest)
    {
      if (tccAuthorizationRequest == null)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 86);

      Logger.LogInformation("ValidateTccAuthorization. completed succesfully");
      return ReturnSuccessV5Result.CreateReturnSuccessV5Result(HttpStatusCode.OK, true);
    }

    #endregion TCCAuthorization
  }
}
