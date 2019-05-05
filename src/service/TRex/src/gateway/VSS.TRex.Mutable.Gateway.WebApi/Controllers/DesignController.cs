using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Mutable.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller to create/update/delete a design for a project.
  ///     HttpGet endpoints use the immutable endpoint (at present VSS.TRex.Gateway.WebApi)
  ///     If ProjectUid doesn't exist then it gets created
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
      GatewayHelper.ValidateAndGetSiteModel(nameof(CreateDesign), designRequest.ProjectUid, true);
     
      if (DesignExists(designRequest.ProjectUid, designRequest.FileType, designRequest.DesignUid))
      {
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Design already exists. Cannot Add.");
      }

      if (designRequest.FileType == ImportedFileType.DesignSurface || designRequest.FileType == ImportedFileType.SurveyedSurface)
      {
        return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<AddTTMDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(designRequest));
      }

      if (designRequest.FileType == ImportedFileType.Alignment)
      {
        return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<AddSVLDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(designRequest));
      }

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File type must be DesignSurface, SurveyedSurface or Alignment"));
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
      GatewayHelper.ValidateAndGetSiteModel(nameof(UpdateDesign), designRequest.ProjectUid, true);
      
      if (!DesignExists(designRequest.ProjectUid, designRequest.FileType, designRequest.DesignUid))
      {
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Design doesn't exist. Cannot update.");
      }

      if (designRequest.FileType == ImportedFileType.DesignSurface || designRequest.FileType == ImportedFileType.SurveyedSurface)
      {
        return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<UpdateTTMDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(designRequest));
      }

      if (designRequest.FileType == ImportedFileType.Alignment)
      {
        return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<UpdateSVLDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(designRequest));
      }
      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File type must be DesignSurface, SurveyedSurface or Alignment"));
    }


    /// <summary>
    /// Deletes a design from a site model.
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
      GatewayHelper.ValidateAndGetSiteModel(nameof(DeleteDesign), designRequest.ProjectUid, true);
     
      if (!DesignExists(designRequest.ProjectUid, designRequest.FileType, designRequest.DesignUid))
      {
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Design doesn't exist. Cannot delete.");
      }

      if (designRequest.FileType == ImportedFileType.DesignSurface || designRequest.FileType == ImportedFileType.SurveyedSurface)
      {
        return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<DeleteTTMDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(designRequest));
      }

      if (designRequest.FileType == ImportedFileType.Alignment)
      {
        return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<DeleteSVLDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(designRequest));
      }
      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File type must be DesignSurface, SurveyedSurface or Alignment"));
    }

    private DesignListResult GetDesignsForSiteModel(Guid projectUid, ImportedFileType fileType)
    {
      List<DesignFileDescriptor> designFileDescriptorList;
      if (fileType == ImportedFileType.DesignSurface)
      {
        var designList = DIContext.Obtain<IDesignManager>().List(projectUid);
        designFileDescriptorList = designList.Select(designSurface =>
            AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designSurface))
          .ToList();
        return new DesignListResult {DesignFileDescriptors = designFileDescriptorList};
      }

      if (fileType == ImportedFileType.SurveyedSurface)
      {
        var designSurfaceList = DIContext.Obtain<ISurveyedSurfaceManager>().List(projectUid);
        designFileDescriptorList = designSurfaceList.Select(designSurface =>
            AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designSurface))
          .ToList();
        return new DesignListResult {DesignFileDescriptors = designFileDescriptorList};
      }

      var designAlignmentList = DIContext.Obtain<IAlignmentManager>().List(projectUid);
      designFileDescriptorList = designAlignmentList.Select(designAlignment =>
          AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designAlignment))
        .ToList();
      return new DesignListResult {DesignFileDescriptors = designFileDescriptorList};
    }

    private bool DesignExists(Guid projectUid, ImportedFileType fileType, Guid designUid)
    {
      return GetDesignsForSiteModel(projectUid, fileType).DesignFileDescriptors.ToList()
        .Exists(x => (string.Compare(x.DesignUid, designUid.ToString(), StringComparison.OrdinalIgnoreCase) != 0));
    }
  }
}
