using System;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Local.ResultHandling;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  public class TagFileController : BaseController
  {
    public TagFileController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler, IConfigurationStore configStore)
        : base(loggerFactory, loggerFactory.CreateLogger<TileController>(), exceptionHandler, configStore)
    {
    }


    /// <summary>
    /// Posts TAG file to Raptor. 
    /// </summary>
    // [PostRequestVerifier]
    [Route("api/v1/tagfiles")]
    [HttpPost]
    public IActionResult Post([FromBody] TagFileRequest request)
    {
      // todo
      request.Validate();

      var tagfileResult = WithServiceExceptionTryExecute(() =>
                                                             RequestExecutorContainer
                                                                 .Build<TagFileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
                                                                 .Process(request)) as TagFileResult;

      // todo we probably need to return some proper return codes to determine further course of action
      if (tagfileResult != null)
      {
        if (tagfileResult.Code == 0)
          return (IActionResult) Ok(tagfileResult);
        else
        {
          return BadRequest(tagfileResult);
        }
      }
      else
      {
        return BadRequest(TagFileResult.Create(0, "Unexpected failure")); // todo
      }

    }


    /// <summary>
    /// For the direct submission of tag files from GNSS capable machines.
    /// </summary>
   // [PostRequestVerifier]
    [Route("api/v2/tagfiles/direct")]
    [HttpPost]
    public IActionResult PostTagFileDirectSubmission([FromBody]CompactionTagFileRequest request)
    {

      request.Validate();
      TagFileRequest request2 = TagFileRequest.CreateTagFile(
                                                              request.FileName,
                                                              request.Data,
                                                              request.ProjectUid,
                                                              null,
                                                              0,false,false,request.OrgId);


      var tagfileResult = WithServiceExceptionTryExecute(() =>
                                                             RequestExecutorContainer
                                                                 .Build<TagFileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
                                                                 .Process(request2)) as TagFileResult;

      // todo we probably need to return some proper return codes to determine further course of action
      if (tagfileResult != null)
      {
        if (tagfileResult.Code == 0)
          return (IActionResult)Ok(tagfileResult);
        else
        {
          return BadRequest(tagfileResult);
        }
      }
      else
      {
        return BadRequest(TagFileResult.Create(0, "Unexpected failure")); // todo
      }


    }

  }
}
