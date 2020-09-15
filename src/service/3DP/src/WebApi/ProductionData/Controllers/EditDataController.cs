using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Compaction.Controllers;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  [ProjectVerifier]
  public class EditDataController : BaseController<EditDataController>, IEditDataContract
  {
    private readonly ITRexCompactionDataProxy _tRexCompactionDataProxy;

    private string CustomerUid => ((RaptorPrincipal) Request.HttpContext.User).CustomerUid;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public EditDataController(
      ILoggerFactory logger,
      IConfigurationStore configStore,
      IFileImportProxy fileImportProxy,
      ITRexCompactionDataProxy tRexCompactionDataProxy,
      ICompactionSettingsManager settingsManager)
      : base(configStore, fileImportProxy, settingsManager)
    {
      this._tRexCompactionDataProxy = tRexCompactionDataProxy;
    }

    /// Called by TBC only
    /// <summary>
    /// Gets a list of edits or overrides of the production data for a project and machine.
    /// </summary>
    /// <returns>A list of the edits applied to the production data for the project and machine.</returns>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/productiondata/getedits")]
    [HttpPost]
    public async Task<EditDataResult> PostEditDataAcquireTbc([FromBody] GetEditDataRequest request)
    {
      request.Validate();
      return await RequestExecutorContainerFactory.Build<GetEditDataExecutor>(LoggerFactory, ConfigStore,
        trexCompactionDataProxy: _tRexCompactionDataProxy, userId: GetUserId(), fileImportProxy: FileImportProxy
      ).ProcessAsync(request) as EditDataResult;
    }

    /// <summary>
    /// Called by TBC only
    /// </summary>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/productiondata/edit")]
    [HttpPost]
    public async Task<ContractExecutionResult> PostEditTbc([FromBody] EditDataRequest request)
    {
      request.Validate();

      if (!request.undo)
      {
        //Validate against existing data edits
        var getRequest = GetEditDataRequest.CreateGetEditDataRequest(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
          request.dataEdit.assetId, request.ProjectUid);
        var editResult = await PostEditDataAcquireTbc(getRequest);
        ValidateNoOverlap(editResult.dataEdits, request.dataEdit);
        //Validate request date range within production data date range
        await ValidateDates(request.ProjectUid.Value, request.dataEdit);
      }

      return await RequestExecutorContainerFactory.Build<EditDataExecutor>(LoggerFactory, ConfigStore,
        trexCompactionDataProxy: _tRexCompactionDataProxy, userId: GetUserId(), fileImportProxy: FileImportProxy
      ).ProcessAsync(request);
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
          var message = string.Empty;
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
    private async Task ValidateDates(Guid projectUid, ProductionDataEdit dataEdit)
    {
      var projectExtents = await ProjectStatisticsHelper.GetProjectStatisticsWithProjectSsExclusions(projectUid, VelociraptorConstants.NO_PROJECT_ID, GetUserId(), CustomHeaders);

      if (projectExtents == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Cannot obtain ProjectStatistics."));
      if (dataEdit.startUTC < projectExtents.startTime || dataEdit.endUTC > projectExtents.endTime)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Data edit outside production data date range: {projectExtents.startTime}-{projectExtents.endTime}"));
      }
    }
  }
}
