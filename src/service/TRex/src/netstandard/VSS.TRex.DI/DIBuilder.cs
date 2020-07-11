using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Serilog.Extensions;

namespace VSS.TRex.DI
{
  /// <summary>
  /// Provides a builder for Direct Injection requirements of a service
  /// </summary>
  public class DIBuilder
  {
    private static DIBuilder _instance;

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
    public DIBuilder(Action<IServiceCollection> addDi)
    {
      addDi(ServiceCollection);
    }

    /// <summary>
    /// Adds a set of dependencies according to the supplied lambda
    /// </summary>
    public DIBuilder Add(Action<IServiceCollection> addDi)
    {
      addDi(ServiceCollection);
      return this;
    }

    public DefaultHttpClientBuilder AddHttpClient<TClient>(Action<HttpClient> configureClient) where TClient : class
    {
      if (ServiceCollection == null)
        throw new ArgumentNullException(nameof(ServiceCollection));

      if (configureClient == null)
        throw new ArgumentNullException(nameof(configureClient));

      ServiceCollection.AddHttpClient();

      var builder = new DefaultHttpClientBuilder(ServiceCollection, typeof(TClient).Name, _instance);
      builder.ConfigureHttpClient(configureClient);
      builder.AddTypedClient<TClient>();

      return builder;
    }

    /// <summary>
    /// Adds logging to the DI collection
    /// </summary>
    public DIBuilder AddLogging()
    {
      // Create the LoggerFactory instance for the service collection
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure());

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
    public static DIBuilder New() => _instance = new DIBuilder();

    /// <summary>
    /// Static method to create a new DIImplementation instance
    /// </summary>
    public static DIBuilder New(IServiceCollection serviceCollection) => _instance = new DIBuilder(serviceCollection);

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

      if (_instance != null)
      {
        _instance.ServiceCollection.Clear();
        _instance.ServiceCollection = null;
        _instance.ServiceProvider = null;
      }

      _instance = null;
    }

    /// <summary>
    /// A handy shorthand version of .Build()
    /// </summary>
    public DIBuilder Complete() => Build();

    /// <summary>
    /// Allow continuation of building the DI context
    /// </summary>
    public static DIBuilder Continue() => _instance ?? New();

    /// <summary>
    /// Allow continuation of building the DI context
    /// </summary>
    public static DIBuilder Continue(IServiceCollection serviceCollection) => New(serviceCollection);

    /// <summary>
    /// Removes a single instance of a registered DI service type in DIContext. The first located instance of the supplied type is removed.
    /// </summary>
    public DIBuilder RemoveSingle<T>()
    {
      var serviceDescriptor = _instance.ServiceCollection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(T));
      if (serviceDescriptor != null)
      {
        _instance.ServiceCollection.Remove(serviceDescriptor);
        
        // Validate the service was removed
        if (_instance.ServiceCollection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(T)) != null)
          throw new Exception($"Failed to remove single instance of {typeof(T).FullName} from service provider");
      }

      return _instance;
    }

    /// <summary>
    /// Removes all instances of a registered DI service type in DIContext. The first located instance of the supplied type is removed.
    /// </summary>
    public DIBuilder RemoveAll<T>()
    {
      var serviceDescriptors = _instance.ServiceCollection.Where(descriptor => descriptor.ServiceType == typeof(T));
      foreach (var service in serviceDescriptors)
        _instance.ServiceCollection.Remove(service);

      return _instance;
    }
  }
}
