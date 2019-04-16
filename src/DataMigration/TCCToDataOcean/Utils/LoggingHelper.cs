using System.Runtime.CompilerServices;
using System.Threading;

namespace TCCToDataOcean.Utils
{
  public class Method
  {
    public static string In([CallerMemberName] string memberName = "") => $"## In ## {LogDetails(memberName)}";
    public static string Out([CallerMemberName] string memberName = "") => $"## Out ## {LogDetails(memberName)}";
    public static string Info(string action = null, [CallerMemberName] string memberName = "") => $"{(action != null ? "## " + action + " ## " : "")}{LogDetails(memberName)}";

    private static string LogDetails(string methodName) => $"{methodName}() [{Thread.CurrentThread.ManagedThreadId}]:";
  }
}
