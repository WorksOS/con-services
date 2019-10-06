using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Now3D.Models;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces;

namespace VSS.Productivity3D.Now3D.Controllers
{
  public class ReportController : BaseController
  {
    private readonly IProjectProxy _projectProxy;
    private readonly IFileImportProxy _fileImportProxy;
    private readonly IFilterServiceProxy _filterServiceProxy;
    private readonly IProductivity3dV2ProxyCompaction _productivity3dV2ProxyCompaction;

    public ReportController(ILoggerFactory loggerFactory, 
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy,
      IFileImportProxy fileImportProxy, 
      IFilterServiceProxy filterServiceProxy,
      IProductivity3dV2ProxyCompaction productivity3dV2ProxyCompaction) 
      : base(loggerFactory, serviceExceptionHandler)
    {
      _projectProxy = projectProxy;
      _fileImportProxy = fileImportProxy;
      _filterServiceProxy = filterServiceProxy;
      _productivity3dV2ProxyCompaction = productivity3dV2ProxyCompaction;
    }

    /// <summary>
    /// Get the pass count summary for the project extents
    /// </summary>
    [HttpGet("api/v1/reports/passcount/summary")]
    [ProducesResponseType(typeof(PassCountSummaryResult), 200)]
    public async Task<IActionResult> PasscountReport([FromQuery, BindRequired]SimpleFilter filter)
    {
      // Request projectUid, filterUid (optional) 
      // Default filter to project extents, no design
      // URL:  api/v2/passcounts/summary
      // Returns PassCountSummaryResult
      Log.LogInformation($"PasscountReport Filter: {filter}");
      var filterUid = await ConvertSimpleFilter(filter);

      var result = await ExecuteRequest<CompactionPassCountSummaryResult>("/passcounts/summary", Guid.Parse(filter.ProjectUid), filterUid);
      return Json(result);
    }

    /// <summary>
    /// Gets the CMV Summary report for the project extents
    /// </summary>
    [HttpGet("api/v1/reports/cmv/summary")]
    [ProducesResponseType(typeof(CompactionCmvSummaryResult), 200)]
    public async Task<IActionResult> CmvReport([FromQuery, BindRequired]SimpleFilter filter)
    {
      // Request: projectUid, filterUid (optional) 
      // Default filter to project extents, no design
      // URL api/v2/cmv/summary
      // Response CompactionCmvSummaryResult
      Log.LogInformation($"CmvReport Filter: {filter}");
      var filterUid = await ConvertSimpleFilter(filter);

      var result = await ExecuteRequest<CompactionCmvSummaryResult>("/cmv/summary", Guid.Parse(filter.ProjectUid), filterUid);
      return Json(result);
    }

    /// <summary>
    /// Gets the MDP Summary report for the project extents
    /// </summary>
    [HttpGet("api/v1/reports/mdp/summary")]
    [ProducesResponseType(typeof(CompactionMdpSummaryResult), 200)]
    public async Task<IActionResult> MdpReport([FromQuery, BindRequired]SimpleFilter filter)
    {
      // Request: projectUid and filterUid (optional)
      // Url api/v2/mdp/summary
      // Returns CompactionMdpSummaryResult
      Log.LogInformation($"MdpReport Filter: {filter}");

      var filterUid = await ConvertSimpleFilter(filter);

      var result = await ExecuteRequest<CompactionMdpSummaryResult>("/mdp/summary", Guid.Parse(filter.ProjectUid), filterUid);
      return Json(result);
    }

    /// <summary>
    /// Gets a Volumes (Ground to Design) summary report for the project
    /// </summary>
    [HttpGet("api/v1/reports/volumes/summary")]
    [ProducesResponseType(typeof(CompactionVolumesSummaryResult), 200)]
    public async Task<IActionResult> VolumesReport([FromQuery, BindRequired]SimpleFilter filter)
    {
      Log.LogInformation($"VolumesReport Filter: {filter}");
      // Ground to design only
      // SummaryVolumesRequest

      // Returns SummaryVolumesResult
      var filterUid = await ConvertSimpleFilter(filter);

      // Base UID needs to be filter
      // Top UID needs to be design
      var route = $"/volumes/summary?projectUid={filter.ProjectUid}&baseUid={filterUid}&topUid={filter.DesignFileUid}";
      var result = await _productivity3dV2ProxyCompaction.ExecuteGenericV2Request<CompactionVolumesSummaryResult>(route, HttpMethod.Get, null, CustomHeaders);

      if (result != null)
        return Json(result);

      ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 
        (int)Now3DExecutionStates.ErrorCodes.GeneralError, 
        null,
        "No Data");
      return null;
    }

    /// <summary>
    /// Gets a Cut/Fill report for the given project and design
    /// </summary>
    [HttpGet("api/v1/reports/cutfill/details")]
    [ProducesResponseType(typeof(CompactionCutFillDetailedResult), 200)]
    public async Task<IActionResult> CutFillReport([FromQuery, BindRequired]SimpleFilter filter)
    {
      // Request orojectUid, filterUid (optional), cutfillDesignUid
      // Url api/v2/cutfill/details"
      // Returns CompactionCutFillDetailedResult
      Log.LogInformation($"CutFillReport Filter: {filter}");
      var filterUid = await ConvertSimpleFilter(filter);

      var additionalParams = new Dictionary<string, string>
      {
        {"cutfillDesignUid", filter.DesignFileUid}
      };
      
      var result = await ExecuteRequest<CompactionCutFillDetailedResult>("/cutfill/details", Guid.Parse(filter.ProjectUid), filterUid, additionalParams);
      return Json(result);
    }

    /// <summary>
    /// Helper method to execute a request against 3dp and throw a service exception if the request fails
    /// </summary>
    private async Task<T> ExecuteRequest<T>(string baseEndPoint, Guid projectUid, Guid filterUid, Dictionary<string, string> additionalParams = null)
      where T : class, IMasterDataModel
    {
      var route = $"{baseEndPoint}?projectUid={projectUid}&filterUid={filterUid}";

      if (additionalParams != null)
      {
        foreach (var (key, value) in additionalParams)
        {
          route += $"&{key}={value}";
        }
      }

      var result = await _productivity3dV2ProxyCompaction.ExecuteGenericV2Request<T>(route, HttpMethod.Get, null, CustomHeaders);

      if (result != null)
        return result;

      ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 
        (int)Now3DExecutionStates.ErrorCodes.GeneralError, 
        null,
        "No Data");
      return null;
    }

    /// <summary>
    /// Convert a simple filter into a Real Filter via the Filter service
    /// </summary>
    /// <returns>Filter UID</returns>
    private async Task<Guid> ConvertSimpleFilter(SimpleFilter simpleFilter)
    {
      if (simpleFilter == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest,
          (int) Now3DExecutionStates.ErrorCodes.FilterConvertFailure,
          null,
          "No Simple Filter found");
      }

      var project = await _projectProxy.GetProjectForCustomer(CustomerUid, simpleFilter.ProjectUid, CustomHeaders);
      if (project == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 
          (int) Now3DExecutionStates.ErrorCodes.FilterConvertFailure,
          null,
          $"Cannot find project {simpleFilter.ProjectUid} for Customer {CustomerUid}");
      }

      var file = await _fileImportProxy.GetFileForProject(simpleFilter.ProjectUid, UserId, simpleFilter.DesignFileUid,
        CustomHeaders);

      if (file == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 
          (int) Now3DExecutionStates.ErrorCodes.FilterConvertFailure,
          null,
          $"Cannot find file {simpleFilter.DesignFileUid} for project {simpleFilter.ProjectUid}");
      }

      var filterModel = new Filter.Abstractions.Models.Filter(simpleFilter.StartDateUtc,
        simpleFilter.EndDateUtc,
        simpleFilter.DesignFileUid,
        file.Name,
        null,
        null,
        null,
        null,
        null,
        null,
        simpleFilter.LiftNumber);

      var filterRequest = FilterRequest.Create(filterModel);

      var result = await _filterServiceProxy.CreateFilter(simpleFilter.ProjectUid, filterRequest, CustomHeaders);

      if (result.Code != 0)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 
          (int)Now3DExecutionStates.ErrorCodes.DataError, 
          result.Code.ToString(),
          result.Message);
      }

      var guid  = Guid.Parse(result.FilterDescriptor.FilterUid);

      Log.LogInformation($"Converted Simple filter '{JsonConvert.SerializeObject(simpleFilter)}' to a " +
        $"{nameof(FilterRequest)}: '{JsonConvert.SerializeObject(filterRequest)}'. FilterUID: {guid}");

      return guid;
    }
  }
}
