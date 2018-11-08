﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.ResultHandling;

namespace VSS.TRex.Mutable.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Process Tagfiles Controller
  /// </summary>
  public class TagFileController : BaseController
  {
    /// <summary>
    /// Controller for tagfile processing
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="configStore"></param>
    public TagFileController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler, IConfigurationStore configStore)
        : base(loggerFactory, loggerFactory.CreateLogger<TagFileController>(), exceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Test endpoint for tagfiles
    /// </summary>
    // [PostRequestVerifier]
    [Route("api/v2/tagfiles")]
    [HttpGet]
    public String Get()
    {
      // Test endpoint connection
      return "You have reached api/v2/tagfiles";
    }

    /// <summary>
    /// For accepting and loading manually or automatically submitted tag files.
    /// </summary>
    // [PostRequestVerifier]
    [Route("api/v2/tagfiles")]
    [HttpPost]
    public async Task<ContractExecutionResult> PostTagNonDirectFile([FromBody]CompactionTagFileRequest request)
    {
      var serializedRequest = SerializeObjectIgnoringProperties(request, "Data");
      Log.LogInformation("PostTagFile: " + serializedRequest);
      return await ExecuteRequest(request);
    }

    /// <summary>
    /// For the direct submission of tag files from GNSS capable machines.
    /// </summary>
   // [PostRequestVerifier]
    [Route("api/v2/tagfiles/direct")]
    [HttpPost]
    public async Task<ContractExecutionResult> PostTagFileDirectSubmission([FromBody]CompactionTagFileRequest request)
    {

      var serializedRequest = SerializeObjectIgnoringProperties(request, "Data");
      Log.LogInformation("PostTagFile (Direct): " + serializedRequest);
      return await ExecuteRequest(request);
    }

    private async Task<ContractExecutionResult> ExecuteRequest(CompactionTagFileRequest tfRequest)
    {

      var tagfileResult = WithServiceExceptionTryExecute(() => RequestExecutorContainer
                                                     .Build<TagFileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
                                                     .Process(tfRequest)) as TagFileResult;
      return tagfileResult;
    }

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

    /// <summary>
    /// Serialize the request ignoring the Data property so not to overwhelm the logs.
    /// </summary>
    private static string SerializeObjectIgnoringProperties(CompactionTagFileRequest request, params string[] properties)
    {
      return JsonConvert.SerializeObject(
          request,
          Formatting.None,
          new JsonSerializerSettings { ContractResolver = new JsonContractPropertyResolver(properties) });
    }
  }
}
