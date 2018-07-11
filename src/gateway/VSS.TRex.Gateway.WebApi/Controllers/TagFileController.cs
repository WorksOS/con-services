using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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

    private readonly ILogger log;


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

      log.LogDebug($"PostTagFile: ProjectID:{request.ProjectUID},File:{request.FileName}");

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

      log.LogDebug("PostTagFile: " + serializedRequest);
      TagFileRequest requestStandard = TagFileRequest.CreateTagFile(
          request.FileName,
          request.Data,
          request.ProjectUid,
          null,
          -1, false, false, request.OrgId);

      var tagfileResult = WithServiceExceptionTryExecute(() =>
                                                             RequestExecutorContainer
                                                                 .Build<TagFileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
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
      log.LogDebug($"PostTagFile: ProjectID:{request.ProjectUid},File:{request.FileName}");

      TagFileRequest requestStandard = TagFileRequest.CreateTagFile(
                                                              request.FileName,
                                                              request.Data,
                                                              request.ProjectUid,
                                                              null,
                                                              -1, false,false,request.OrgId);


      var tagfileResult = WithServiceExceptionTryExecute(() =>
                                                             RequestExecutorContainer
                                                                 .Build<TagFileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
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
