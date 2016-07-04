using log4net;

namespace VSP.MasterData.Common.Logging
{
  public static class LoggerService
  {
    public static void IfDebug(this ILog Log, object message)
    {
      if (Log.IsDebugEnabled)
      {
        Log.Debug(message);
      }
    }

    public static void IfError(this ILog Log, object message)
    {
      if (Log.IsErrorEnabled)
      {
        Log.Error(message);
      }
    }

    public static void IfFatal(this ILog Log, object message)
    {
      if (Log.IsFatalEnabled)
      {
        Log.Fatal(message);
      }
    }

    public static void IfInfo(this ILog Log, object message)
    {
      if (Log.IsInfoEnabled)
      {
        Log.Info(message);
      }
    }

    public static void IfWarn(this ILog Log, object message)
    {
      if (Log.IsWarnEnabled)
      {
        Log.Warn(message);
      }
    }
  }
}