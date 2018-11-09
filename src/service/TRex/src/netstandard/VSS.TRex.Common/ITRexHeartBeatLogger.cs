namespace VSS.TRex.Common
{
  public interface ITRexHeartBeatLogger
  {
    void AddContext(object context);

    void RemoveContext(object context);
  }
}
