﻿using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.TRex.Logging;
using VSS.TRex.TAGFiles.GridFabric.Services;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/services")]
  [ApiController]
  public class ServiceDeployerController : ControllerBase
  {

    private static readonly ILogger Log = Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Deploys the TagFileBufferQueueService to the grid
    /// </summary>
    /// <returns></returns>
    [HttpPut("tagfilebuffer")]
    public string DeployTagFileBufferService()
    {
      Log.LogInformation($"Obtaining proxy for TAG file buffer queue service");

      try
      {
        // Ensure the continuous query service is installed that supports TAG file processing
        TAGFileBufferQueueServiceProxy proxy = new TAGFileBufferQueueServiceProxy();
        Log.LogInformation($"Deploying TAG file buffer queue service");
        proxy.Deploy();
      }
      catch (Exception e)
      {
        Log.LogError($"Exception occurred deploying service: {e}");
        return $"Exception occurred deploying service: {e}";
      }

      Log.LogInformation($"Complected service deployment for TAG file buffer queue service");
      return $"Complected service deployment for TAG file buffer queue service";
    }
  }
}
