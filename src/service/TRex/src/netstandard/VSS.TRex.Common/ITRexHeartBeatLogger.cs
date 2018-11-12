namespace VSS.TRex.Common
{
  public interface ITRexHeartBeatLogger
  {
    int IntervalInMilliseconds { get; }

    void AddContext(object context);

    void RemoveContext(object context);
  }
}
