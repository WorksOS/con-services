using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace VSS.Log4Net.Extensions
{
  public class Log4NetProvider : ILoggerProvider
  {
    private IDictionary<string, ILogger> _loggers = new Dictionary<string, ILogger>();
    private IHttpContextAccessor _accessor;
    public static string RepoName { get; set; } = "";

    public Log4NetProvider(IHttpContextAccessor accessor)
    {
      _accessor = accessor;
      if (string.IsNullOrEmpty(RepoName))
      throw new ArgumentException($"You have to specify Repository name for Log4Net via {nameof(RepoName)} property");
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
            _loggers[name] = new Log4NetAdapter(RepoName, name, _accessor);
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
