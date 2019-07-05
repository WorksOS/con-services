using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller to get designs for a project.
  ///     Create/Update/Delete endpoints use the mutable endpoint (at present VSS.TRex.Mutable.Gateway.WebApi)
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
    /// Returns surface and surveyed surface designs
    ///    which is registered for a sitemodel.
    /// If there are no designs the result will be an empty list.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="fileType"></param>
    /// <returns></returns>
    [HttpGet("get")]
    public DesignListResult GetDesignsForProject([FromQuery] Guid projectUid, [FromQuery] ImportedFileType? fileType)
    {
      Log.LogInformation($"{nameof(GetDesignsForProject)}: projectUid{projectUid} fileType: {fileType}");

      var designFileDescriptorList = new List<DesignFileDescriptor>();
      if (fileType != null && !(fileType == ImportedFileType.DesignSurface || fileType == ImportedFileType.SurveyedSurface || fileType == ImportedFileType.Alignment))
      { 
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File type must be DesignSurface, SurveyedSurface or Alignment"));
      }

      if ((DIContext.Obtain<ISiteModels>().GetSiteModel(projectUid)) == null)
      {
        return new DesignListResult { DesignFileDescriptors = designFileDescriptorList };
      }

      if (fileType == null || fileType == ImportedFileType.DesignSurface)
      {
        var designList = DIContext.Obtain<IDesignManager>().List(projectUid);
        if (designList != null)
        {
          designFileDescriptorList = designList.Select(designSurface =>
              AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designSurface))
            .ToList();
        }
      }

      if (fileType == null || fileType == ImportedFileType.SurveyedSurface)
      {
        var designSurfaceList = DIContext.Obtain<ISurveyedSurfaceManager>().List(projectUid);
        if (designSurfaceList != null)
        {
          designFileDescriptorList.AddRange(designSurfaceList.Select(designSurface =>
              AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designSurface))
            .ToList());
        }
      }

      if (fileType == null || fileType == ImportedFileType.Alignment)
      {
        var designAlignmentList = DIContext.Obtain<IAlignmentManager>().List(projectUid);
        if (designAlignmentList != null)
        {
          designFileDescriptorList.AddRange(designAlignmentList.Select(designAlignment =>
              AutoMapperUtility.Automapper.Map<DesignFileDescriptor>(designAlignment))
            .ToList());
        }
      }

      return new DesignListResult { DesignFileDescriptors = designFileDescriptorList };
    }

    /// <summary>
    /// Gets a list of design boundaries in GeoJson format from TRex database.
    /// </summary>
    /// <param name="projectUid">The site model/project unique identifier.</param>
    /// <param name="designUid">The design file unique identidier.</param>
    /// <param name="fileName">The design file name.</param>
    /// <param name="tolerance">The spacing interval for the sampled points. Setting to 1.0 will cause points to be spaced 1.0 meters apart.</param>
    /// <returns>Execution result with a list of design boundaries.</returns>
    [HttpGet("boundaries")]
    public ContractExecutionResult GetDesignBoundaries([FromQuery] Guid projectUid, [FromQuery] Guid designUid, [FromQuery] string fileName, [FromQuery] double? tolerance)
    {
      Log.LogInformation($"{nameof(GetDesignsForProject)}: projectUid:{projectUid}, designUid:{designUid}, fileName:{fileName}, tolerance: {tolerance}");

      const double BOUNDARY_POINTS_INTERVAL = 0.0;

      var designBoundariesRequest = new TRexDesignBoundariesRequest(projectUid, designUid, fileName, tolerance ?? BOUNDARY_POINTS_INTERVAL);

      designBoundariesRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<DesignBoundariesExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(designBoundariesRequest) as DesignBoundaryResult);
    }
  }
}
