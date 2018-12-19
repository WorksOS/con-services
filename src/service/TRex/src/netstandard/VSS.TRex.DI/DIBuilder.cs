using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;

namespace VSS.TRex.DI
{
  /// <summary>
  /// Provides a builder for Direct Injection requirements of a service
  /// </summary>
  public class DIBuilder
  {
    public static DIBuilder Instance;

    public IServiceProvider ServiceProvider { get; private set; }
    public IServiceCollection ServiceCollection = new ServiceCollection();

    /// <summary>
    /// Default constructor for DI implementation
    /// </summary>
    public DIBuilder()
    { }

    /// <summary>
    /// Constructor accepting a lambda returning a service collection to add to the DI collection
    /// </summary>
    public DIBuilder(Action<IServiceCollection> addDI)
    {
      addDI(ServiceCollection);
    }

    /// <summary>
    /// Adds a set of dependencies according to the supplied lambda
    /// </summary>
    public DIBuilder Add(Action<IServiceCollection> addDI)
    {
      addDI(ServiceCollection);
      return this;
    }

    public DefaultHttpClientBuilder AddHttpClient<TClient>(Action<HttpClient> configureClient) where TClient : class
    {
      if (ServiceCollection == null)
      {
        throw new ArgumentNullException(nameof(ServiceCollection));
      }
      if (configureClient == null)
      {
        throw new ArgumentNullException(nameof(configureClient));
      }

      ServiceCollection.AddHttpClient();

      DefaultHttpClientBuilder builder = new DefaultHttpClientBuilder(ServiceCollection, typeof(TClient).Name, Instance);
      builder.ConfigureHttpClient(configureClient);
      builder.AddTypedClient<TClient>();

      return builder;
    }

    /// <summary>
    /// Adds logging to the DI collection
    /// </summary>
    public DIBuilder AddLogging()
    {
      // Set up log4net related configuration prior to instantiating the logging service
      const string loggerRepoName = "VSS";

      //Now set actual logging name and configure logger.
      Log4NetProvider.RepoName = loggerRepoName;
      Log4NetAspExtensions.ConfigureLog4Net(loggerRepoName);

      // Create the LoggerFactory instance for the service collection
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddProvider(new Log4NetProvider());

      // Insert this immediately into the TRex.Logging namespace to get logging available as early as possible
      Logging.Logger.Inject(loggerFactory);

      // Add the logging related services to the collection
      return Add(x => { x.AddSingleton<ILoggerFactory>(loggerFactory); });
    }

    /// <summary>
    /// Builds the service provider, returning it ready for injection
    /// </summary>
    public DIBuilder Build()
    {
      ServiceProvider = ServiceCollection.BuildServiceProvider();
      Inject();

      return this;
    }

    /// <summary>
    /// Static method to create a new DIImplementation instance
    /// </summary>
    public static DIBuilder New() => Instance = new DIBuilder();

    /// <summary>
    /// Performs the Inject operation into the DIContext as a fluent operation from the DIImplementation
    /// </summary>
    public DIBuilder Inject()
    {
      DIContext.Inject(ServiceProvider);
      return this;
    }

    /// <summary>
    /// Clears out any established DI context returning a empty TRex DI builder & context.
    /// </summary>
    public DIBuilder Eject()
    {
      DIContext.Eject();

      ServiceProvider = null;
      ServiceCollection = new ServiceCollection();

      return this;
    }

    /// <summary>
    /// A handy shorthand version of .Build()
    /// </summary>
    public DIBuilder Complete() => Build();

    /// <summary>
    /// Allow continuation of building the DI context
    /// </summary>
    public static DIBuilder Continue() => Instance ?? New();
  }
}
