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
    public ObjectResult PostTagFileDirectSubmission([FromBody]CompactionTagFileRequest request)
    {
      // todo
      return StatusCode((int)HttpStatusCode.BadRequest,null );
      /*

      // Serialize the request ignoring the Data property so not to overwhelm the logs.
      var serializedRequest = JsonConvert.SerializeObject(
          request,
          Formatting.None,
          new JsonSerializerSettings { ContractResolver = new JsonContractPropertyResolver("Data") });
      
      log.LogDebug("PostTagFile (Direct): " + serializedRequest);

      var projectId = GetLegacyProjectId(request.ProjectUid).Result;

      var tfRequest = TagFileRequestLegacy.CreateTagFile(request.FileName, request.Data, projectId, null, VelociraptorConstants.NO_MACHINE_ID, false, false);
      tfRequest.Validate();

      var result = RequestExecutorContainerFactory
          .Build<TagFileDirectSubmissionExecutor>(logger, raptorClient, tagProcessor)
          .Process(tfRequest) as TagFileDirectSubmissionResult;

      if (result.Code == 0)
      {
        log.LogDebug($"PostTagFile (Direct): Successfully imported TAG file '{request.FileName}'.");
        return StatusCode((int)HttpStatusCode.OK, result);
      }

      log.LogDebug($"PostTagFile (Direct): Failed to import TAG file '{request.FileName}', {result.Message}");
      return StatusCode((int)HttpStatusCode.BadRequest, result);
      */
    }








  }
}
