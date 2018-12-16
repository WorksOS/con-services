using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.TRex.DI;

namespace VSS.TRex.Common
{
  /// <summary>
  /// Provides a context to register heart beat loggers that will provide state logging to be emitted to the 
  /// TRex log on a regular basis.
  /// </summary>
  public class TRexHeartBeatLogger : ITRexHeartBeatLogger
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<TRexHeartBeatLogger>();

    private static readonly int kDefaultIntervalInMilliseconds = DIContext.Obtain<IConfigurationStore>().GetValueInt("HEARTBEAT_LOGGER_INTERVAL", Common.Consts.HEARTBEAT_LOGGER_INTERVAL);

    private readonly List<object> loggingContexts;

    private readonly Thread contextRunner;

    public int IntervalInMilliseconds { get; }

    /// <summary>
    /// Creates a new heartbeat logger with a defined interval between heart beat epochs
    /// </summary>
    public TRexHeartBeatLogger(int intervalMS)
    {
      if (intervalMS <= 100)
        throw new ArgumentException("Heart beat logger interval cannot be <= 100 milliseconds");

      loggingContexts = new List<object>();
      IntervalInMilliseconds = intervalMS;

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

    /// <summary>
    /// Creates a new heartbeat logger with the default interval between heart beat epochs
    /// </summary>
    public TRexHeartBeatLogger() : this(kDefaultIntervalInMilliseconds)
    {
    }

    public void AddContext(object context) => loggingContexts.Add(context);

    public void RemoveContext(object context) => loggingContexts.Remove(context);
  }
}
