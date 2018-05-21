using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;

namespace VSS.TRex.Logging
{
  /// <summary>
  /// The TRex Logger namespace providing CreateLogger semantics. Dependency Injection seeds the logger factory into the Logger class.
  /// </summary>
    public static class Logger
  {
    // Get the logger factory from the DIContext
    private static ILoggerFactory Factory = DIContext.Obtain<ILoggerFactory>();

      public static ILogger CreateLogger<TState>() => Factory.CreateLogger<TState>();

      public static ILogger CreateLogger(string categoryName) => Factory.CreateLogger(categoryName);

      public static void Inject(ILoggerFactory factory) => Factory = factory;
    }
}
