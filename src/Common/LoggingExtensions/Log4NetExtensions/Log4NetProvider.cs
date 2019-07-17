using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace VSS.Log4Net.Extensions
{
  public class Log4NetProvider : ILoggerProvider
  {
    private IDictionary<string, ILogger> loggers = new Dictionary<string, ILogger>();
    private IHttpContextAccessor accessor;
    public static string RepoName { get; set; } = "";

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="accessor">Optionally sets the HTTP context accessor.</param>
    public Log4NetProvider(IHttpContextAccessor accessor = null)
    {
      this.accessor = accessor;

      if (string.IsNullOrEmpty(RepoName))
      {
        throw new ArgumentException($"You have to specify Repository name for Log4Net via {nameof(RepoName)} property");
      }
    }

    public ILogger CreateLogger(string name)
    {
      lock (loggers)
      {
        if (!loggers.ContainsKey(name))
        {
          // Have to check again since another thread may have gotten the lock first
          if (!loggers.ContainsKey(name))
          {
            loggers[name] = new Log4NetAdapter(RepoName, name, accessor);
          }
        }

        return loggers[name];
      }
    }

    public void Dispose()
    {
      loggers.Clear();
      loggers = null;
    }
  }
}
