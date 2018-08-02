using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Logging;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.Webtools.Controllers
{

  [Route("api/[controller]")]
  public class GridStatusController : Controller
  {
    private static readonly ILogger Log = Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);


    [HttpGet]
    public string GridStatus()
    {
 
      try
      {
        //Log.LogInformation("About to call ActivatePersistentGridServer.Instance().SetGridActive() for Immutable TRex grid");
        bool result1 = ActivatePersistentGridServer.Instance().SetGridActive(TRexGrids.ImmutableGridName());
        //Log.LogInformation($"Activation process completed: Immutable = {result1}");

        //Log.LogInformation("About to call ActivatePersistentGridServer.Instance().SetGridActive() for Mutable TRex grid");
        bool result2 = ActivatePersistentGridServer.Instance().SetGridActive(TRexGrids.MutableGridName());
        //Log.LogInformation($"Activation process completed: Mutable = {result2}");

        return $"Activation process completed: Mutable = {result1}, Immutable = {result2}";
      }
      catch (Exception ex)
      {
        return $"Activation exception: {ex}";
      }
    }

  }
}
