using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace log4netExtensions
{
  public class Log4NetProvider : ILoggerProvider
  {
    private IDictionary<string, ILogger> _loggers = new Dictionary<string, ILogger>();
    private readonly string _repoName;

    public Log4NetProvider(string repoName)
    {
      _repoName = repoName;
    }

    public ILogger CreateLogger(string name)
    {
      lock (_loggers)
      {
        if (!_loggers.ContainsKey(name))
        {
          // Have to check again since another thread may have gotten the lock first
          if (!_loggers.ContainsKey(name))
          {
            _loggers[name] = new Log4NetAdapter(_repoName, name);
          }
        }

        return _loggers[name];
      }
    }

    public void Dispose()
    {
      _loggers.Clear();
      _loggers = null;
    }
  }
}