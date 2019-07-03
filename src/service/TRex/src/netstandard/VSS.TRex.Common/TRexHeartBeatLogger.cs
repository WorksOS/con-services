using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Interfaces.Interfaces;
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

    private readonly List<IHeartBeatLogger> loggingContexts;

    private readonly Thread contextRunner;

    private bool Stopped { get; set; } 

    public int IntervalInMilliseconds { get; }

    /// <summary>
    /// Creates a new heartbeat logger with a defined interval between heart beat epochs
    /// </summary>
    public TRexHeartBeatLogger(int intervalMS)
    {
      if (intervalMS < 100)
        throw new ArgumentException("Heart beat logger interval cannot be < 100 milliseconds");

      loggingContexts = new List<IHeartBeatLogger>();
      IntervalInMilliseconds = intervalMS;

      contextRunner = new Thread(() =>
      {
        while (contextRunner.ThreadState == ThreadState.Running && !Stopped)
        {
          lock (loggingContexts)
          {
            foreach (var context in loggingContexts)
            {
              try
              {
                context.HeartBeat();
              }
              catch (Exception e)
              {
                Log.LogError(e, $"Exception in {nameof(TRexHeartBeatLogger)}");
              }
            }
          }

          Thread.Sleep(IntervalInMilliseconds);
        }
      });

      contextRunner.Start();
    }

    /// <summary>
    /// Creates a new heartbeat logger with the default interval between heart beat epochs
    /// </summary>
    public TRexHeartBeatLogger() : this(DIContext.Obtain<IConfigurationStore>().GetValueInt("HEARTBEAT_LOGGER_INTERVAL", Consts.HEARTBEAT_LOGGER_INTERVAL))
    {
    }

    public void AddContext(IHeartBeatLogger context)
    {
      lock (loggingContexts)
      {
        loggingContexts.Add(context);
      }
    }

    public void RemoveContext(IHeartBeatLogger context)
    {
      lock (loggingContexts)
      {
        loggingContexts.Remove(context);
      }
    }

    public void Stop()
    {
      Stopped = true;
    }
  }
}
