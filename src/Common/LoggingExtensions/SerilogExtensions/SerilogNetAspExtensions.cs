using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VSS.Serilog.Extensions
{
  public static class SerilogNetAspExtensions
  {
    /// <summary>
    /// Custom AddProvider extension method that allows us more flexible dependency injection when
    /// using a custom ILoggerProvider class.
    ///
    /// One of the more useful benefits is DI'ing serivces while passing other constructor parameters.
    /// </summary>
    public static ILoggingBuilder AddProvider<T>(
      this ILoggingBuilder builder,
      Func<IServiceProvider, T> factory) where T : class, ILoggerProvider
    {
      builder.Services.AddSingleton<ILoggerProvider, T>(factory);

      return builder;
    }
  }
}
