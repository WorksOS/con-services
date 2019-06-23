using Microsoft.Extensions.Logging;
using Prism.Logging;

namespace VSS.Hydrology.WebApi
{
  public class LoggerFacade : ILoggerFacade
  {
    private readonly ILogger _logger;

    public LoggerFacade(ILogger logger)
    {
      _logger = logger;
    }

    public void Log(string message, Category category, Priority priority)
    {
      switch(category)
      {
        case Category.Debug:
          _logger.LogDebug(message);
          break;
        case Category.Warn:
          _logger.LogWarning(message);
          break;
        case Category.Exception:
          _logger.LogError(message);
          break;
        case Category.Info:
          _logger.LogInformation(message);
          break;
      } 
    }
  }
}
