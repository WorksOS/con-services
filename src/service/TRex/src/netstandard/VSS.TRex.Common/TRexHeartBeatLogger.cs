using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.Common
{
  /// <summary>
  /// Provides a context to register heart beat loggers that will provide state logging to be emitted to the 
  /// TRex log on a regular basis.
  /// </summary>
  public class TRexHeartBeatLogger : ITRexHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<TRexHeartBeatLogger>();

    private const int kDefaultIntervalInMilliseconds = 10000;

    private readonly List<object> loggingContexts;

    private readonly Thread contextRunner;

    public int IntervalInMilliseconds { get; }

    /// <summary>
    /// Creates a new heartbeat logger with the default interval between heart beat epochs
    /// </summary>
    public TRexHeartBeatLogger()
    {
      loggingContexts = new List<object>();
      IntervalInMilliseconds = kDefaultIntervalInMilliseconds;

      contextRunner = new Thread(() =>
      {
        while (contextRunner.ThreadState == ThreadState.Running)
        {
          foreach (var context in loggingContexts)
            Log.LogInformation($"Heartbeat: {context}");

          Thread.Sleep(IntervalInMilliseconds);
        }
      });

      contextRunner.Start();
    }

    public void AddContext(object context) => loggingContexts.Add(context);

    public void RemoveContext(object context) => loggingContexts.Remove(context);
  }
}
