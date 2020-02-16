using System;
using System.Collections.Generic;
using log4net;
using System.Threading;
using VSS.Nighthawk.Instrumentation;
using System.Reflection;
using System.Configuration;

namespace VSS.Hosted.VLCommon
{
  public class TimerService
  {
    #region Member Fields
    private static readonly ILog log = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly Timer _runTimer = null;
    private TimeSpan _dueTime = TimeSpan.FromMinutes(2.0);
    private TimeSpan _runInterval;
    private DateTime _dueAt = DateTime.MinValue;
    #endregion

    public TimerService(TimeSpan runInterval)
    {
      this._dueTime = runInterval;
      _runInterval = runInterval;
      _runTimer = new Timer(new TimerCallback(ProcessRunTimer));
      StartTimer();
    }

    public void StartTimer()
    {
#if DEBUG
      _runTimer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(-1));
#else
      _runTimer.Change(this._dueTime, TimeSpan.FromMilliseconds(-1));
#endif
    }

    public void StopTimer()
    {
      _runTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
    }

    public void SetTimerToTriggerAt(DateTime dueTime)
    {
      this._dueAt = dueTime;
    }

    protected virtual void TimerExpiry()
    {
      throw new NotImplementedException("You are expected to override this virtual method, or not use this base class.");
    }

    private void ProcessRunTimer(object sender)
    {
      try
      {
        log.IfDebug("TimerService.ProcessRunTimer invoked");

        DateTime begin = DateTime.UtcNow;

        TimerExpiry();

        if (ConfigurationManager.AppSettings["MetricsInstrumentation"] == "1")
        {
          ClientMetric metric = new ClientMetric()
          {
            className = this.GetType().FullName,
            startUTC = begin,
            endUTC = DateTime.UtcNow,
            methodName = "TimerExpiry",
            source = Environment.MachineName
          };
          API.Session.AddClientMetrics(new List<ClientMetric>() { metric });
        }
      }
      catch (Exception e)
      {
        log.IfError("TimerService.ProcessRunTimer encountered an exception", e);
      }
      finally
      {
        if (_dueAt > DateTime.UtcNow)
        {
          _dueTime = _dueAt - DateTime.UtcNow;
          _dueAt = DateTime.MinValue;
          if (_dueTime < TimeSpan.Zero)
            _dueTime = TimeSpan.Zero;
        }
        _runTimer.Change(_dueTime, TimeSpan.FromMilliseconds(-1));
        _dueTime = _runInterval;
      }
    }
  }
}
