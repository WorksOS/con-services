using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting site model statistics.
  /// </summary>
  [Route("api/v1/design/get")]
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
    [HttpGet]
    public DesignListResult GetDesignsForSiteModel([FromQuery] Guid projectUid, [FromQuery] ImportedFileType? fileType)
    {
      var designFileDescriptorList = new List<DesignFileDescriptor>();
      if (fileType != null && !(fileType == ImportedFileType.DesignSurface || fileType == ImportedFileType.SurveyedSurface))
      { 
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File type must be DesignSurface or SurveyedSurface"));
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

      return new DesignListResult { DesignFileDescriptors = designFileDescriptorList };
    }
  }
}
