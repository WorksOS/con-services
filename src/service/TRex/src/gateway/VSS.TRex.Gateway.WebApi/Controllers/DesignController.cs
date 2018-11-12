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
    /// Returns surface and surveyedsurface designs
    ///    which is registered for a sitemodel.
    /// If there are no designs the result will be an empty list.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    [HttpGet]
    public DesignListResult GetDesignsForSiteModel([FromQuery] Guid projectUid)
    {
      var designList = DIContext.Obtain<IDesignManager>().List(projectUid);
      var designFileDescriptorList = designList.Select(designSurface =>
          AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designSurface))
        .ToList();

      var designSurfaceList = DIContext.Obtain<ISurveyedSurfaceManager>().List(projectUid);
      designFileDescriptorList.AddRange(designSurfaceList.Select(designSurface =>
          AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designSurface))
        .ToList());

      return new DesignListResult {DesignFileDescriptors = designFileDescriptorList.ToImmutableList()};
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
    public DesignListResult GetDesignsForSiteModel([FromQuery] Guid projectUid, [FromQuery] ImportedFileType fileType)
    {
      if (fileType == ImportedFileType.DesignSurface)
      {
        var designList = DIContext.Obtain<IDesignManager>().List(projectUid);
        var designFileDescriptorList = designList.Select(designSurface =>
            AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designSurface))
          .ToList();
        return new DesignListResult { DesignFileDescriptors = designFileDescriptorList.ToImmutableList() };
      }
      else if (fileType == ImportedFileType.SurveyedSurface)
      {
        var designSurfaceList = DIContext.Obtain<ISurveyedSurfaceManager>().List(projectUid);
        var designFileDescriptorList = designSurfaceList.Select(designSurface =>
            AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designSurface))
          .ToList();
        return new DesignListResult { DesignFileDescriptors = designFileDescriptorList.ToImmutableList() };
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
    /// <param name="designRequest"></param>
    /// <returns></returns>
    [HttpPost]
    public ContractExecutionResult CreateDesign([FromBody] DesignRequest designRequest)
    {
      Log.LogInformation($"{nameof(CreateDesign)}: {JsonConvert.SerializeObject(designRequest)}");
      designRequest.Validate();

      if (GetDesignsForSiteModel(designRequest.ProjectUid, designRequest.FileType).DesignFileDescriptors.ToList().Exists(x => x.DesignUid == designRequest.DesignUid.ToString()))
      {
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Design already exists. Cannot Add.");
      }

      if (designRequest.FileType == ImportedFileType.DesignSurface)
      {
        return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<AddDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(designRequest));
      }

      if (designRequest.FileType == ImportedFileType.SurveyedSurface)
      {
        return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<AddDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler) 
            .Process(designRequest));
      }

      return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Create of Design FileType: {designRequest.FileType.ToString()} is not YET supported by TRex.");
    }


    /// <summary>
    /// Update a design
    /// </summary>
    /// <param name="designRequest"></param>
    /// <returns></returns>
    [HttpPut]
    public ContractExecutionResult UpdateDesignSurface([FromBody] DesignRequest designRequest)
    {
      Log.LogInformation($"{nameof(UpdateDesignSurface)}: {JsonConvert.SerializeObject(designRequest)}");
      designRequest.Validate();

      if (!GetDesignsForSiteModel(designRequest.ProjectUid, designRequest.FileType).DesignFileDescriptors.ToList().Exists(x => x.DesignUid == designRequest.DesignUid.ToString()))
      {
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Design doesn't exist. Cannot update.");
      }

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<UpdateDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(designRequest));
    }


    /// <summary>
    /// Deletes a design from a sitemodel.
    ///    Files are left on S3 (as per Dmitry)
    ///    Local copies in temp are removed
    /// </summary>
    /// <param name="designRequest"></param>
    /// <returns></returns>
    [HttpDelete]
    public ContractExecutionResult DeleteDesignSurface([FromBody] DesignRequest designRequest)
    {
      Log.LogInformation($"{nameof(DeleteDesignSurface)}: {JsonConvert.SerializeObject(designRequest)}");
      designRequest.Validate();

      if (!GetDesignsForSiteModel(designRequest.ProjectUid, designRequest.FileType).DesignFileDescriptors.ToList().Exists(x => x.DesignUid == designRequest.DesignUid.ToString()))
      {
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Design doesn't exist. Cannot update.");
      }

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<DeleteDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(designRequest));
    }
  }
}
