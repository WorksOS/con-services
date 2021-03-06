﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.ResultHandling.Designs;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

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
    [HttpPost]
    public Task<ContractExecutionResult> CreateDesign([FromBody] DesignRequest designRequest)
    {
      Log.LogInformation($"{nameof(CreateDesign)}: {JsonConvert.SerializeObject(designRequest)}");
      designRequest.Validate();
      GatewayHelper.ValidateAndGetSiteModel(nameof(CreateDesign), designRequest.ProjectUid, true);
     
      if (DesignExists(designRequest.ProjectUid, designRequest.FileType, designRequest.DesignUid))
        return Task.FromResult(new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Design already exists. Cannot Add."));

      if (designRequest.FileType == ImportedFileType.DesignSurface || designRequest.FileType == ImportedFileType.SurveyedSurface)
      {
        return WithServiceExceptionTryExecuteAsync(() => RequestExecutorContainer
            .Build<AddTTMDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .ProcessAsync(designRequest));
      }

      if (designRequest.FileType == ImportedFileType.Alignment)
      {
        return WithServiceExceptionTryExecuteAsync(() => RequestExecutorContainer
            .Build<AddSVLDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .ProcessAsync(designRequest));
      }

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File type must be DesignSurface, SurveyedSurface or Alignment"));
    }


    /// <summary>
    /// Update a design
    /// </summary>
    [HttpPut]
    public Task<ContractExecutionResult> UpdateDesign([FromBody] DesignRequest designRequest)
    {
      Log.LogInformation($"{nameof(UpdateDesign)}: {JsonConvert.SerializeObject(designRequest)}");
      designRequest.Validate();
      GatewayHelper.ValidateAndGetSiteModel(nameof(UpdateDesign), designRequest.ProjectUid, true);
      
      if (!DesignExists(designRequest.ProjectUid, designRequest.FileType, designRequest.DesignUid))
        return Task.FromResult(new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Design doesn't exist. Cannot update."));

      if (designRequest.FileType == ImportedFileType.DesignSurface || designRequest.FileType == ImportedFileType.SurveyedSurface)
      {
        return WithServiceExceptionTryExecuteAsync(() => RequestExecutorContainer
            .Build<UpdateTTMDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .ProcessAsync(designRequest));
      }

      if (designRequest.FileType == ImportedFileType.Alignment)
      {
        return WithServiceExceptionTryExecuteAsync(() => 
          RequestExecutorContainer
            .Build<UpdateSVLDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .ProcessAsync(designRequest));
      }
      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File type must be DesignSurface, SurveyedSurface or Alignment"));
    }


    /// <summary>
    /// Deletes a design from a site model.
    ///    Files are left on S3 (as per Dmitry)
    ///    Local copies in temp are removed
    /// </summary>
    [HttpDelete]
    public Task<ContractExecutionResult> DeleteDesign([FromBody] DesignRequest designRequest)
    {
      Log.LogInformation($"{nameof(DeleteDesign)}: {JsonConvert.SerializeObject(designRequest)}");
      designRequest.Validate();
      GatewayHelper.ValidateAndGetSiteModel(nameof(DeleteDesign), designRequest.ProjectUid, true);
     
      if (!DesignExists(designRequest.ProjectUid, designRequest.FileType, designRequest.DesignUid))
      {
        return Task.FromResult(new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Design doesn't exist. Cannot delete."));
      }

      if (designRequest.FileType == ImportedFileType.DesignSurface || designRequest.FileType == ImportedFileType.SurveyedSurface)
      {
        return WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainer
            .Build<DeleteTTMDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .ProcessAsync(designRequest));
      }

      if (designRequest.FileType == ImportedFileType.Alignment)
      {
        return WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainer
            .Build<DeleteSVLDesignExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .ProcessAsync(designRequest));
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
        .Exists(x => (string.Compare(x.DesignUid, designUid.ToString(), StringComparison.OrdinalIgnoreCase) == 0));
    }
  }
}
