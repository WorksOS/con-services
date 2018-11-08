using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting site model statistics.
  /// </summary>
  [Route("api/v1/design")]
  public class DesignController : BaseController
  {
    public DesignController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<DesignController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Returns the list of designs of requested type,
    ///    which is registered for a sitemodel.
    /// If there are no designs the result will be an empty list.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="fileType"></param>
    /// <returns></returns>
    [HttpGet]
    public DesignListResult GetDesignsForSiteModel([FromQuery] Guid projectUid, ImportedFileType fileType)
    {
      if (fileType == ImportedFileType.DesignSurface)
      {
        var designList = DIContext.Obtain<IDesignManager>().List(projectUid);
        var designFileDescriptorList = designList.Select(designSurface =>
            AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designSurface))
          .ToList();
        return new DesignListResult {DesignFileDescriptors = designFileDescriptorList.ToImmutableList()};
      }
      else if (fileType == ImportedFileType.SurveyedSurface)
      {
        var designSurfaceList = DIContext.Obtain<ISurveyedSurfaceManager>().List(projectUid);
        var designFileDescriptorList = designSurfaceList.Select(designSurface =>
            AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designSurface))
          .ToList();
        return new DesignListResult {DesignFileDescriptors = designFileDescriptorList.ToImmutableList()};
      }

      return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, $"FileType: {fileType.ToString()} is not supported by TRex.") as DesignListResult;
    }

    /// <summary>
    /// Adds a new design to a sitemodel.
    ///   Also adds the index files to S3.
    ///   temporarily using s3
    ///    Bucket:   vss-project3dp-stg ( ProjectSvc writes)
    ///    Path:     projectUid
    ///    Filename: bowlfill 1290 6-5-18.ttm 
    /// </summary>
    /// <param name="designSurfaceRequest"></param>
    /// <returns></returns>
    [HttpPost]
    public ContractExecutionResult CreateDesignSurface([FromBody] DesignRequest designSurfaceRequest)
    {
      /* todojeannie move env vars from appsettings to yaml - which services? */

      Log.LogInformation($"{nameof(CreateDesignSurface)}: {JsonConvert.SerializeObject(designSurfaceRequest)}");
      designSurfaceRequest.Validate();

      if (designSurfaceRequest.FileType == ImportedFileType.DesignSurface)
      {
        var existsAlready = GetDesignsForSiteModel(designSurfaceRequest.ProjectUid, ImportedFileType.DesignSurface).DesignFileDescriptors.ToList().Exists(x => x.DesignUid == designSurfaceRequest.DesignUid.ToString());
        if (existsAlready)
        {
          return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Design already exists. Cannot Add.");
        }

        return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<UpsertDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(designSurfaceRequest));
      }

      return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Create of Design FileType: {designSurfaceRequest.FileType.ToString()} is not YET supported by TRex.");
    }


    /// <summary>
    /// Update a design
    /// </summary>
    /// <param name="designSurfaceRequest"></param>
    /// <returns></returns>
    [HttpPut]
    public ContractExecutionResult UpdateDesignSurface([FromBody] DesignRequest designSurfaceRequest)
    {
      Log.LogInformation($"{nameof(UpdateDesignSurface)}: {JsonConvert.SerializeObject(designSurfaceRequest)}");
      designSurfaceRequest.Validate();

      if (designSurfaceRequest.FileType == ImportedFileType.DesignSurface)
      {
        var existsAlready = GetDesignsForSiteModel(designSurfaceRequest.ProjectUid, ImportedFileType.DesignSurface).DesignFileDescriptors.ToList().Exists(x => x.DesignUid == designSurfaceRequest.DesignUid.ToString());
        if (!existsAlready)
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Design doesn't exist. Cannot update."));
        }

        // todojeannie rather than removing it here, should there be a DesignManager.Update() which effectively does this?
        //    how about removing the indexes from local and s3 storage?
        //    should this remove go into the Designmanager.Update?
        var isDeletedOk = DIContext.Obtain<IDesignManager>().Remove(designSurfaceRequest.ProjectUid, designSurfaceRequest.DesignUid);

        return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<UpsertDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(designSurfaceRequest));
      }

      return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Update of Design FileType: {designSurfaceRequest.FileType.ToString()} is not YET supported by TRex.");
    }


    /// <summary>
    /// Deletes a design from a sitemodel.
    ///    Also removes the index files from S3.
    ///    Removal of the design file from S3 is done by ProjectSvc.
    /// </summary>
    /// <param name="designSurfaceRequest"></param>
    /// <returns></returns>
    [HttpDelete]
    public ContractExecutionResult DeleteDesignSurface([FromBody] DesignRequest designSurfaceRequest)
    {
      Log.LogInformation($"{nameof(DeleteDesignSurface)}: {JsonConvert.SerializeObject(designSurfaceRequest)}");
      designSurfaceRequest.Validate();

      if (designSurfaceRequest.FileType == ImportedFileType.DesignSurface)
      {
        var existsAlready = GetDesignsForSiteModel(designSurfaceRequest.ProjectUid, ImportedFileType.DesignSurface).DesignFileDescriptors.ToList().Exists(x => x.DesignUid == designSurfaceRequest.DesignUid.ToString());
        if (!existsAlready)
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Design doesn't exist. Cannot delete."));
        }

        var isDeletedOk = DIContext.Obtain<IDesignManager>().Remove(designSurfaceRequest.ProjectUid, designSurfaceRequest.DesignUid);
        if (isDeletedOk)
        {
          // todojeannie should remove  index file/s from s3, however ITransferProxy has no remove at present.
          //              ProjectSvc to remove the original
          return new ContractExecutionResult();
        }
        else
        {
          return new ContractExecutionResult( /* todojeannie*/
            9999, "whatever");
        }

      }

      return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Update of Design FileType: {designSurfaceRequest.FileType.ToString()} is not YET supported by TRex.");
    }
  }
}
