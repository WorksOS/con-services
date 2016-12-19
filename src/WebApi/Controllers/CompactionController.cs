using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.WebApiModels.Interfaces;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace VSS.Raptor.Service.WebApi.Controllers
{
    public class CompactionController : Controller
    {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    /// Constructor with injected raptor client and logger
    /// </summary>
    /*/// <param name="raptorClient">Raptor client</param>*/
    /// <param name="logger">Logger</param>
    public CompactionController(IASNodeClient raptorClient, ILogger<CompactionController> logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
    }

    //Follow this pattern
    /// <summary>
    ///   Gets xxx 
    /// </summary>
    /// <returns>xxxResult</returns>
    /// <executor>xxxExecutor</executor>
    [Route("api/v1/compaction/xxx")]
    [HttpGet]
    public /*xxxResult*/void Getxxx()
    {
      logger.LogInformation("Getxxx");

      //1. Set up request model instance
      //2. request.Validate();
      /*
      var result =
          (RequestExecutorContainer.Build<xxxExecutor>(factory).Process(request) as xxxResult);

      return result;
      */
    }
  }
}
