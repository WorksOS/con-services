using System;

namespace VSS.TRex.Common.Exceptions
{
  public static class TRexAppDomainUnhandledExceptionHandler
  {
    public static void Handler(object sender, UnhandledExceptionEventArgs e)
    {
      if (e.ExceptionObject is Exception exception)
        Console.WriteLine($"Unhandled Exception: {exception.Message}");
      else
        Console.WriteLine($"Unhandled Exception, but not exception type. Type: {e.ExceptionObject}. {e}");

      Console.WriteLine("Unhandled Exception: " + (e.IsTerminating ? "Exiting" : "Not exiting"));
    }
  }
}
