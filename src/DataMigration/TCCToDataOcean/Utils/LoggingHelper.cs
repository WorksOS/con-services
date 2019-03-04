using System.Diagnostics;
using System.Threading;

namespace TCCToDataOcean.Utils
{
  public class Method
  {
    public static string In => $"## In ## {new StackFrame(1).GetMethod().Name} [{Thread.CurrentThread.ManagedThreadId}]";
    public static string Out => $"## Out ## {new StackFrame(1).GetMethod().Name} [{Thread.CurrentThread.ManagedThreadId}]";
    public static string Info(string action = null) => $"{(action != null ? "## + " + action + " ##" : "")} {new StackFrame(1).GetMethod().Name} [{Thread.CurrentThread.ManagedThreadId}]";
  }
}
