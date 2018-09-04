using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VSS.TRex.TAGFiles.GridFabric.Services;
using VSS.TRex.Logging;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/services")]
  [ApiController]
  public class ServiceDeployerController : ControllerBase
  {

    private static readonly ILogger Log = Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    [HttpPut("tagfilebuffer")]
    public string DeployTagFileBufferService()
    {
      Log.LogInformation($"Obtaining proxy for TAG file buffer queue service");

      // Ensure the continuous query service is installed that supports TAG file processing
      TAGFileBufferQueueServiceProxy proxy = new TAGFileBufferQueueServiceProxy();
      try
      {
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
