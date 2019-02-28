using System.Diagnostics;
using System.Threading;

namespace TCCToDataOcean.Utils
{
  public class Method
  {
    public static string In => $"## In ## {MethodAndThreadInfo}";
    public static string Out => $"## Out ## {MethodAndThreadInfo}";
    public static string Info(string action = null) => $"{(action != null ? "## + " + action + " ##" : "")} {MethodAndThreadInfo}]";

    private static string MethodAndThreadInfo => $"{new StackFrame(1).GetMethod().Name} [{Thread.CurrentThread.ManagedThreadId}]";
  }
}
