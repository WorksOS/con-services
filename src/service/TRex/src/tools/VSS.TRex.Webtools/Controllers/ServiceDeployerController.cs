using System;
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
      Log.LogInformation("Obtaining proxy for TAG file buffer queue service");

      try
      {
        var proxy = new TAGFileBufferQueueServiceProxy();
        Log.LogInformation("Deploying TAG file buffer queue service");
        proxy.Deploy();
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception occurred deploying service:");
        return $"Exception occurred deploying service: {e}";
      }

      Log.LogInformation("Completed service deployment for TAG file buffer queue service");
      return "Completed service deployment for TAG file buffer queue service";
    }

    /// <summary>
    /// Deploys the segment retirement service to the mutable grid
    /// </summary>
    /// <returns></returns>
    [HttpPut("segmentretirement/mutable")]
    public string DeployMutableSegmentRetirementService()
    {
      Log.LogInformation("Obtaining proxy for mutable segment retirement service");

      try
      {
        var proxy = new SegmentRetirementQueueServiceProxyMutable();
        Log.LogInformation("Deploying mutable segment retirement service");
        proxy.Deploy();
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception occurred deploying service:");
        return $"Exception occurred deploying service: {e}";
      }

      Log.LogInformation("Completed service deployment for mutable segment retirement service");
      return "Completed service deployment for mutable segment retirement service";
    }

    /// <summary>
    /// Deploys the segment retirement service to the immutable grid
    /// </summary>
    /// <returns></returns>
    [HttpPut("segmentretirement/immutable")]
    public string DeployImmutableSegmentRetirementService()
    {
      Log.LogInformation("Obtaining proxy for mutable segment retirement service");

      try
      {
        var proxy = new SegmentRetirementQueueServiceProxyImmutable();
        Log.LogInformation("Deploying mutable segment retirement service");
        proxy.Deploy();
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception occurred deploying service:");
        return $"Exception occurred deploying service: {e}";
      }

      Log.LogInformation("Completed service deployment for mutable segment retirement service");
      return "Completed service deployment for mutable segment retirement service";
    }
  }
}
