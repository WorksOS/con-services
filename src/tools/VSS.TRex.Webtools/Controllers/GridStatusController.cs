using System;
using System.Collections.Generic;
using System.Reflection;
using Apache.Ignite.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Logging;
using VSS.TRex.Webtools.Models;

namespace VSS.TRex.Webtools.Controllers
{

  [Route("api/grids")]
  public class IgniteGridController : Controller
  {
    private static readonly ILogger Log = Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Returns the list of grids in Trex
    /// </summary>
    [HttpGet]
    public JsonResult GetGridNames()
    {
      IList<Grid> grids = new List<Grid>();

      foreach (string name in TRexGrids.GridNames())
      {
        grids.Add(new Grid(name));
      }

      return Json(grids);
    }

    /// <summary>
    /// Returns status of either active or inactive for given grid
    /// </summary>
    /// <param name="gridName">Grid to return status for</param>
    /// <returns></returns>
    [HttpGet("status/{gridName}")]
    public string GridStatus(string gridName)
    {
      try
      {
        IIgnite ignite = TRexGridFactory.Grid(gridName);
        return ignite.GetCluster().IsActive().ToString();
      }
      catch (NullReferenceException ex)
      {
        return $"{ex.Message} \n this usually indicates are failure to get ignite";
      }
    }

    /// <summary>
    /// Set a grid active or inactive
    /// </summary>
    /// <param name="gridName">The grid we are operating on</param>
    /// <param name="status">desired state of provided grid</param>
    /// <returns></returns>
    [HttpPut("active/{gridname}/{status}")]
    public string SetGridStatus(string gridName, bool status)
    {
      try
      {
        IIgnite ignite = TRexGridFactory.Grid(gridName);
        ignite.GetCluster().SetActive(status);
      }
      catch (ArgumentException ex)
      {
        return ex.Message;
      }
      catch (NullReferenceException ex)
      {
        return $"{ex.Message} \n this usually indicates are failure to get ignite";
      }
      return $"Set grid {gridName} to {status}, this may take time to happen, use status/gridName to check status";
    }

  }
}
