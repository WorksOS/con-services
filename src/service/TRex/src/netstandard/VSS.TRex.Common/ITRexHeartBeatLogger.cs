using VSS.TRex.Common.Interfaces.Interfaces;

namespace VSS.TRex.Common
{
  public interface ITRexHeartBeatLogger
  {
    int IntervalInMilliseconds { get; }

    void AddContext(IHeartBeatLogger context);

    void RemoveContext(IHeartBeatLogger context);

    void Stop();
  }
}
