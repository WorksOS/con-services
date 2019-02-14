using System;
using System.Linq;
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
    private static DIBuilder Instance;

    public IServiceProvider ServiceProvider { get; private set; }
    private IServiceCollection ServiceCollection { get; set; }
  
    /// <summary>
    /// Default constructor for DI implementation
    /// </summary>
    private DIBuilder()
    {
       ServiceCollection = new ServiceCollection();
    }

    /// <summary>
    /// Default constructor for DI implementation
    /// </summary>
    private DIBuilder(IServiceCollection serviceCollection)
    {
       ServiceCollection = serviceCollection;
    }

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
      return Add(x => { x.AddSingleton(loggerFactory); });
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
    /// Static method to create a new DIImplementation instance
    /// </summary>
    public static DIBuilder New(IServiceCollection serviceCollection) => Instance = new DIBuilder(serviceCollection);

    /// <summary>
    /// Performs the Inject operation into the DIContext as a fluent operation from the DIImplementation
    /// </summary>
    private DIBuilder Inject()
    {
      DIContext.Inject(ServiceProvider);
      return this;
    }

    /// <summary>
    /// Clears out any established DI context returning a empty TRex DI builder & context.
    /// </summary>
    public static void Eject()
    {
      DIContext.Close();

      if (Instance != null)
      {
        Instance.ServiceCollection.Clear();
        Instance.ServiceCollection = null;
        Instance.ServiceProvider = null;
      }

      Instance = null;
    }

    /// <summary>
    /// A handy shorthand version of .Build()
    /// </summary>
    public DIBuilder Complete() => Build();

    /// <summary>
    /// Allow continuation of building the DI context
    /// </summary>
    public static DIBuilder Continue() => Instance ?? New();

    /// <summary>
    /// Allow continuation of building the DI context
    /// </summary>
    public static DIBuilder Continue(IServiceCollection serviceCollection) => Instance ?? New(serviceCollection);

    /// <summary>
    /// Removes a single instance of a registered DI service type in DIContext. The first located instance of the supplied type is removed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public DIBuilder RemoveSingle<T>()
    {
      var serviceDescriptor = Instance.ServiceCollection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(T));
      if (serviceDescriptor != null)
        Instance.ServiceCollection.Remove(serviceDescriptor);

      return Instance;
    }

    /// <summary>
    /// Removes all instances of a registered DI service type in DIContext. The first located instance of the supplied type is removed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public DIBuilder RemoveAll<T>()
    {
      var serviceDescriptors = Instance.ServiceCollection.Where(descriptor => descriptor.ServiceType == typeof(T));
      foreach (var service in serviceDescriptors)
        Instance.ServiceCollection.Remove(service);

      return Instance;
    }
  }
}
