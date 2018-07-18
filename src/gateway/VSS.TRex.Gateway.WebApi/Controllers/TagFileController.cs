using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Process Tagfiles Controller
  /// </summary>
  public class TagFileController : BaseController
  {

    private IMutableClientServer tagfileClientServer;

    /// <summary>
    /// Controller for tagfile processing
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="configStore"></param>
    /// <param name="tagFileClientServer"></param>
    public TagFileController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler, IConfigurationStore configStore, IMutableClientServer tagFileClientServer)
        : base(loggerFactory, loggerFactory.CreateLogger<TagFileController>(), exceptionHandler, configStore)
    {
      this.tagfileClientServer = tagFileClientServer;
    }


    /// <summary>
    /// Test endpoint for tagfiles
    /// </summary>
    // [PostRequestVerifier]
    [Route("api/v1/tagfiles")]
    [HttpGet]
    public String Get()
    {
      // Test endpoint connection
      return "You have reached api/v1/tagfiles";
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

      Log.LogDebug($"PostTagFile: ProjectID:{request.ProjectUID},File:{request.FileName}");

      var tagfileResult = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
            .Build<TagFileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler, null, tagfileClientServer)
            .Process(request)) as TagFileResult;

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



    /// <summary>
    /// For accepting and loading manually or automatically submitted tag files.
    /// </summary>
   // [PostRequestVerifier]
    [Route("api/v2/tagfiles")]
    [HttpPost]
    public IActionResult PostTagFile([FromBody]CompactionTagFileRequest request)
    {
      //request.Validate();

      // Serialize the request ignoring the Data property so not to overwhelm the logs.
      var serializedRequest = JsonConvert.SerializeObject(
          request,
          Formatting.None,
          new JsonSerializerSettings { ContractResolver = new JsonContractPropertyResolver("Data") });


      Log.LogDebug($"PostTagFile:{request.FileName}");

      TagFileRequest requestStandard = TagFileRequest.CreateTagFile(
          request.FileName,
          request.Data,
          request.ProjectUid,
          null,
          1, false, false, request.OrgId); //todo
      //      -1, false, false, request.OrgId);

      var tagfileResult = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
            .Build<TagFileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler, null, tagfileClientServer)
            .Process(requestStandard)) as TagFileResult;

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



    /// <summary>
    /// For the direct submission of tag files from GNSS capable machines.
    /// </summary>
   // [PostRequestVerifier]
    [Route("api/v2/tagfiles/direct")]
    [HttpPost]
    public IActionResult PostTagFileDirectSubmission([FromBody]CompactionTagFileRequest request)
    {

      //request.Validate();
      Log.LogDebug($"PostTagFile: ProjectID:{request.ProjectUid},File:{request.FileName}");

      TagFileRequest requestStandard = TagFileRequest.CreateTagFile(
                                                              request.FileName,
                                                              request.Data,
                                                              request.ProjectUid,
                                                              null,
                                                              -1, false, false, request.OrgId);


      var tagfileResult = WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
              .Build<TagFileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler, null, tagfileClientServer)
              .Process(requestStandard)) as TagFileResult;

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


    // todo its a common func that needs to move to package
    public class JsonContractPropertyResolver : DefaultContractResolver
    {
      private readonly string[] props;

      /// <inheritdoc />
      public JsonContractPropertyResolver(params string[] prop)
      {
        props = prop;
      }

      /// <inheritdoc />
      protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
      {
        return base.CreateProperties(type, memberSerialization)
            .Where(p => !props.Contains(p.PropertyName)).ToList();
      }
    }



  }
}
