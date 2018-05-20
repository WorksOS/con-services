using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.Logging
{
  /// <summary>
  /// The TRex Logger namespace providing CreateLogger semantics. Dependency Injection seeds the logger factory into the Logger class.
  /// </summary>
    public static class Logger
    {
      private static ILoggerFactory Factory {get; set;}

      public static ILogger CreateLogger<TState>() => Factory.CreateLogger<TState>();

      public static ILogger CreateLogger(string categoryName) => Factory.CreateLogger(categoryName);

      public static void Inject(ILoggerFactory factory) => Factory = factory;
    }
}
