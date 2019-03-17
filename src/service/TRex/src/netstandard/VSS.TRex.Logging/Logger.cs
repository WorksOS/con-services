using Microsoft.Extensions.Logging;

namespace VSS.TRex.Logging
{
  /// <summary>
  /// The TRex Logger namespace providing CreateLogger semantics. Dependency Injection seeds the logger factory into the Logger class.
  /// </summary>
  public static class Logger
  {
    /// <summary>
    /// Get the logger factory from the DIContext
    /// </summary>
    public static ILoggerFactory Factory { get; private set; }

    /// <summary>
    /// Creates a logger based on the parameterized type from the creation context.
    /// If there is no factory instantiated this will return a null logger.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <returns></returns>
    public static ILogger CreateLogger<TState>() => Factory?.CreateLogger<TState>();

    /// <summary>
    /// Creates a logger based on the category name from the creation context.
    /// If there is no factory instantiated this will return a null logger.
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    public static ILogger CreateLogger(string categoryName) => Factory?.CreateLogger(categoryName);

    /// <summary>
    /// Provides for injection of a logger factory from a DI builder context.
    /// Note: If a factory is already injected, subsequent factory injections will be ignored as
    /// large numbers of static logger contexts may already depend on the original factory injection.
    /// </summary>
    /// <param name="factory"></param>
    public static void Inject(ILoggerFactory factory) => Factory = Factory ?? factory;
  }
}
