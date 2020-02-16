using Autofac;
using Autofac.Integration.WebApi;
using log4net;
using Magnum.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Topshelf;
using Topshelf.Runtime;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.DeviceCapabilityService.Configuration;
using VSS.Nighthawk.DeviceCapabilityService.Data;
using VSS.Nighthawk.DeviceCapabilityService.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.DeviceHandlers;
using VSS.Nighthawk.DeviceCapabilityService.Helpers.Processors;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Service;
using ICacheManager = VSS.Nighthawk.DeviceCapabilityService.Interfaces.ICacheManager;
using ED = VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.Nighthawk.DeviceCapabilityService
{
  public class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static IContainer _container {get;set;}

    static void Main(string[] args)
    {
      TopshelfExitCode exitCode = HostFactory.Run(hostConfig =>
      {
        hostConfig.SetServiceName("_NHDeviceCapabilitySvc");
        hostConfig.SetDisplayName("_NHDeviceCapabilitySvc");
        hostConfig.SetDescription("Service for device capability metadata");
        hostConfig.RunAsLocalSystem();
        hostConfig.StartAutomatically();
        hostConfig.EnableServiceRecovery(svcRecoveryConfig =>
        {
          svcRecoveryConfig.RestartService(1);
          svcRecoveryConfig.RestartService(1);
          svcRecoveryConfig.RestartService(1);
        }
          );
        hostConfig.Service<RestService>(svcConfig =>
          {
            svcConfig.ConstructUsing(ServiceFactory);
            svcConfig.WhenStarted(restService =>
              {
                restService.Start();
                Log.IfInfo("DeviceCapabilityService started");
              }
            );
            svcConfig.WhenStopped(restService =>
              {
                restService.Stop();
                Log.IfInfo("DeviceCapabilityService stopped");
              }
            );
          }
        );
        hostConfig.UseLog4Net();
      });

      switch (exitCode)
      {
        case TopshelfExitCode.Ok:
          Log.IfInfoFormat("_NHDeviceCapabilitySvc: Service Exited");
          break;
        case TopshelfExitCode.AbnormalExit:
          Log.IfDebugFormat("_NHDeviceCapabilitySvc: Service AbnormalExit");
          break;
        case TopshelfExitCode.SudoRequired:
          Log.IfDebugFormat("_NHDeviceCapabilitySvc: Service SudoRequired");
          break;
        case TopshelfExitCode.ServiceAlreadyInstalled:
          Log.IfDebugFormat("_NHDeviceCapabilitySvc: ServiceAlreadyInstalled");
          break;
        case TopshelfExitCode.ServiceNotInstalled:
          Log.IfDebugFormat("_NHDeviceCapabilitySvc: ServiceNotInstalled");
          break;
        case TopshelfExitCode.StartServiceFailed:
          Log.IfDebugFormat("_NHDeviceCapabilitySvc: StartServiceFailed");
          break;
        case TopshelfExitCode.StopServiceFailed:
          Log.IfDebugFormat("_NHDeviceCapabilitySvc: StopServiceFailed");
          break;
      }
    }

    public static RestService ServiceFactory(HostSettings settings)
    {
      var deviceHandlerConfig =
           ConfigurationManager.GetSection("deviceHandlerConfig") as HandlerConfigSection;
      var NHDeviceCapabilitySvcEnvEndpointSuffix = ConfigurationManager.AppSettings["NHDeviceCapabilitySvcEnvEndpointSuffix"] ?? "local";

      var handlerCollection = (deviceHandlerConfig != null) ?
        deviceHandlerConfig.HandlerConfigs :
        new HandlerConfigCollection();

      var builder = new ContainerBuilder();

      builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

      builder.Register(c => new RestService(new Uri(Config.Default.BaseUri)))
        .As<RestService>()
        .SingleInstance();

      builder.Register(o => (NHOPFactory)(() => ObjectContextFactory.NewNHContext<INH_OP>()))
        .As<NHOPFactory>()
        .SingleInstance();

      builder.Register(c => new CacheManagerConfig("LocalCacheManager"))
        .As<ICacheManagerConfig>()
        .SingleInstance();

      builder.RegisterType<CacheManager>()
        .As<ICacheManager>()
        .SingleInstance();

      builder.RegisterType<StringEncryptor>()
        .As<IStringEncryptor>()
        .SingleInstance();

      builder.Register(context => new Storage(
          context.Resolve<NHOPFactory>(),
          context.Resolve<ICacheManager>(),
          context.Resolve<IStringEncryptor>(),
          Config.Default.DeviceTypeCacheLifetimeMinutes,
          Config.Default.EndpointCacheLifetimeMinutes)
        )
        .As<IStorage>()
        .SingleInstance();

      builder.RegisterType<DeviceQueryHelper>()
        .As<IDeviceQueryHelper>()
        .SingleInstance();

      builder.RegisterType<AssetSettingsProcessor>()
        .As<IAssetSettingsProcessor>()
        .SingleInstance();

      builder.RegisterType<DeviceConfigProcessor>()
        .As<IDeviceConfigProcessor>()
        .SingleInstance();

      builder.RegisterType<LocationUpdateRequestedProcessor>()
        .As<ILocationUpdateRequestedProcessor>()
        .SingleInstance();

      builder.RegisterType<SiteAdministrationProcessor>()
        .As<ISiteAdministrationProcessor>()
        .SingleInstance();

      builder.Register(context =>
      {
        var deviceHandlerParameters = new DeviceHandlerParameters(
            new Dictionary<ED.DeviceTypeEnum, IDeviceHandlerStrategy>(),
            new UnknownDeviceHandler()
          );

        // Populate dictionary of (device type->device type handler) pairs through reflection on assembly types
        var deviceHandlers = Assembly.GetExecutingAssembly()
          .GetTypes()
            .Where(type => (type.Implements<IDeviceHandlerStrategy>()
              && (type != typeof(IDeviceHandlerStrategy))
              && (type != typeof(UnknownDeviceHandler))));

        deviceHandlers.ToList().ForEach(type =>
        {
          if (handlerCollection.ContainsKey(type.Name))
          {
            var endpointList = (from object endpointConfig
                                in handlerCollection[type.Name].OutboundEndpoints
                                select string.Format("{0}_{1}",
                                  ((EndpointConfigElement)endpointConfig).Name,
                                  NHDeviceCapabilitySvcEnvEndpointSuffix)).ToList();

            deviceHandlerParameters.DeviceHandlers.Add(
                type.Name.Replace("DeviceHandler", "").ToEnum<ED.DeviceTypeEnum>(),
                (IDeviceHandlerStrategy)Activator.CreateInstance(type, endpointList));
          }
        });
        return deviceHandlerParameters;
      })
        .As<IDeviceHandlerParameters>()
        .SingleInstance();

      builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
        .Where(t => t.Implements<ApiController>())
        .AsSelf();

      _container = builder.Build();

      var restService = _container.Resolve<RestService>();
      restService.Configure(new AutofacWebApiDependencyResolver(_container));
      return restService;
    }
  }
}
