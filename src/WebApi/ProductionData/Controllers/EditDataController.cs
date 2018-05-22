using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApiModels.ProductionData.Contracts;
using VSS.Productivity3D.WebApiModels.ProductionData.Executors;
using VSS.Productivity3D.WebApiModels.Report.Executors;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class EditDataController : Controller, IEditDataContract
  {
    /// <summary>
    /// Tag processor for use by executor
    /// </summary>
    private readonly ITagProcessor tagProcessor;

    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="tagProcessor">Tag processor client</param>
    /// <param name="logger">LoggerFactory</param>
    public EditDataController(IASNodeClient raptorClient, ITagProcessor tagProcessor, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.tagProcessor = tagProcessor;
      this.logger = logger;
    }

    /// <summary>
    /// Gets a list of edits or overrides of the production data for a project and machine.
    /// </summary>
    /// <param name="request">The request representation for the operation.</param>
    /// <returns>A list of the edits applied to the production data for the project and machine.</returns>
    /// <executor>GetEditDataExecutor</executor> 
    [PostRequestVerifier]
    [ProjectIdVerifier(AllowLandfillProjects = true)]
    [ProjectUidVerifier(AllowLandfillProjects = true)]
    [Route("api/v1/productiondata/getedits")]
    [HttpPost]
    public EditDataResult PostEditDataAcquire([FromBody] GetEditDataRequest request)
    {
      request.Validate();

      return RequestExecutorContainerFactory.Build<GetEditDataExecutor>(logger, raptorClient, tagProcessor).Process(request) as EditDataResult;
    }

    /// <summary>
    /// Applies an edit to production data to correct data that has been recorded wrongly in Machines by Operator.
    /// </summary>
    /// <param name="request">The request representation for the operation</param>
    /// <returns></returns>
    /// <executor>EditDataExecutor</executor>
    [PostRequestVerifier]
    [ProjectIdVerifier(AllowLandfillProjects = true, AllowArchivedState = true)]
    [ProjectUidVerifier(AllowLandfillProjects = true, AllowArchivedState = true)]
    [Route("api/v1/productiondata/edit")]
    [HttpPost]
    public ContractExecutionResult Post([FromBody]EditDataRequest request)
    {
      request.Validate();

      if (!request.undo)
      {
        //Validate against existing data edits
        GetEditDataRequest getRequest = GetEditDataRequest.CreateGetEditDataRequest(request.ProjectId ?? -1,
            request.dataEdit.assetId);
        EditDataResult editResult = PostEditDataAcquire(getRequest);
        ValidateNoOverlap(editResult.dataEdits, request.dataEdit);
        //Validate request date range within production data date range
        ValidateDates(request.ProjectId ?? -1, request.dataEdit);
      }

      return RequestExecutorContainerFactory.Build<EditDataExecutor>(logger, raptorClient, tagProcessor).Process(request);
    }

    /// <summary>
    /// Validates new edit does not overlap any existing edit of the same type for the same machine.
    /// </summary>
    private void ValidateNoOverlap(List<ProductionDataEdit> existingEdits, ProductionDataEdit newEdit)
    {
      if (existingEdits != null && existingEdits.Count > 0)
      {
        var overlapEdits = (from e in existingEdits
                            where
                                ((!string.IsNullOrEmpty(e.onMachineDesignName) &&
                                  !string.IsNullOrEmpty(newEdit.onMachineDesignName)) ||
                                 (e.liftNumber.HasValue && newEdit.liftNumber.HasValue)) &&
                                 e.assetId == newEdit.assetId &&
                                !(e.endUTC <= newEdit.startUTC || e.startUTC >= newEdit.endUTC)
                            select e).ToList();

        if (overlapEdits.Count > 0)
        {
          string message = string.Empty;
          foreach (var oe in overlapEdits)
          {
            message = $"{message}\nMachine: {oe.assetId}, Override Period: {oe.startUTC}-{oe.endUTC}, Edited Value: {(string.IsNullOrEmpty(oe.onMachineDesignName) ? oe.onMachineDesignName : (oe.liftNumber?.ToString() ?? string.Empty))}";
          }
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                $"Data edit overlaps: {message}"));
        }
      }
    }

    /// <summary>
    /// Validates new edit is within production data date range for the project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="dataEdit">New edit</param>
    private void ValidateDates(long projectId, ProductionDataEdit dataEdit)
    {
      var request = ProjectStatisticsRequest.CreateStatisticsParameters(projectId, new long[0]);
      request.Validate();
      dynamic stats = RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(logger, raptorClient, tagProcessor).Process(request) as ProjectStatisticsResult;
      if (stats == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Can not validate request - check ReportSvc configuration."));
      if (dataEdit.startUTC < stats.startTime || dataEdit.endUTC > stats.endTime)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                string.Format("Data edit outside production data date range: {0}-{1}", stats.startTime, stats.endTime)));
      }
    }
  }
}
