using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Mutable.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller to create/update/delete a design for a project.
  ///     Get endpoints use the immutable endpoint (at present VSS.TRex.Gateway.WebApi)
  /// </summary>
  [Route("api/v1/design")]
  public class DesignController : BaseController
  {
    /// <inheritdoc />
    public DesignController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<DesignController>(), serviceExceptionHandler, configStore)
    {
    }


    /// <summary>
    /// Adds a new design to a sitemodel.
    ///   Also adds the index files to, for now, S3.
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
            .Build<AddDesignExecutor>(ConfigStore, base.LoggerFactory, ServiceExceptionHandler)
            .Process(designRequest));
      }

      if (designRequest.FileType == ImportedFileType.SurveyedSurface)
      {
        return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<AddDesignExecutor>(ConfigStore, base.LoggerFactory, ServiceExceptionHandler) 
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
    public ContractExecutionResult UpdateDesign([FromBody] DesignRequest designRequest)
    {
      Log.LogInformation($"{nameof(UpdateDesign)}: {JsonConvert.SerializeObject(designRequest)}");
      designRequest.Validate();

      if (!GetDesignsForSiteModel(designRequest.ProjectUid, designRequest.FileType).DesignFileDescriptors.ToList().Exists(x => x.DesignUid == designRequest.DesignUid.ToString()))
      {
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Design doesn't exist. Cannot update.");
      }

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<UpdateDesignExecutor>(ConfigStore, base.LoggerFactory, ServiceExceptionHandler)
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
    public ContractExecutionResult DeleteDesign([FromBody] DesignRequest designRequest)
    {
      Log.LogInformation($"{nameof(DeleteDesign)}: {JsonConvert.SerializeObject(designRequest)}");
      designRequest.Validate();

      if (!GetDesignsForSiteModel(designRequest.ProjectUid, designRequest.FileType).DesignFileDescriptors.ToList().Exists(x => x.DesignUid == designRequest.DesignUid.ToString()))
      {
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Design doesn't exist. Cannot update.");
      }

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<DeleteDesignExecutor>(ConfigStore, base.LoggerFactory, ServiceExceptionHandler)
          .Process(designRequest));
    }

    private DesignListResult GetDesignsForSiteModel(Guid projectUid, ImportedFileType fileType)
    {
      var designFileDescriptorList = new List<DesignFileDescriptor>();
      if (fileType == ImportedFileType.DesignSurface)
      {
        var designList = DIContext.Obtain<IDesignManager>().List(projectUid);
        designFileDescriptorList = designList.Select(designSurface =>
            AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designSurface))
          .ToList();
        return new DesignListResult { DesignFileDescriptors = designFileDescriptorList };
      }

      if (fileType == ImportedFileType.SurveyedSurface)
      {
        var designSurfaceList = DIContext.Obtain<ISurveyedSurfaceManager>().List(projectUid);
        designFileDescriptorList = designSurfaceList.Select(designSurface =>
            AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designSurface))
          .ToList();
        return new DesignListResult { DesignFileDescriptors = designFileDescriptorList };
      }

      return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, $"FileType: {fileType.ToString()} is not supported by TRex.") as DesignListResult;
    }
  }
}
