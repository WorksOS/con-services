using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MockProjectWebApi.Controllers
{
  public class BaseController : ControllerBase
  {
    protected readonly ILogger Logger;
    protected readonly ILoggerFactory LoggerFactory;

    protected BaseController(ILoggerFactory loggerFactory)
    {
      LoggerFactory = loggerFactory;
      Logger = loggerFactory.CreateLogger(GetType());
    }
  }
}
