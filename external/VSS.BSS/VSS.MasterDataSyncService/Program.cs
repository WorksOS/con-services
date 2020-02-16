using System.Configuration;
using System.Reflection;
using Autofac;
using log4net;
using Topshelf;
using Topshelf.Runtime;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Common;


namespace VSS.Nighthawk.MasterDataSync
{
  public class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    protected static IContainer Container { get; set; }

    private static void Main(string[] args)
    {
      var exitCode = HostFactory.Run(host =>
      {
        host.SetServiceName("_NHMasterDataSyncService");
        host.SetDisplayName("_NHMasterDataSyncService");
        host.SetDescription("Service to sync CG and NG data");
        host.RunAsLocalSystem();
        host.Disabled();

        host.EnableServiceRecovery(cfg =>
        {
          cfg.RestartService(1);
          cfg.RestartService(1);
          cfg.RestartService(1);
        });
        host.Service<MasterDataSyncService>(s =>
        {
          s.ConstructUsing(ServiceFactory);
          s.WhenStarted(o => o.Start());
          s.WhenStopped(o => o.Stop());
        });
      });

      switch (exitCode)
      {
        case TopshelfExitCode.Ok:
          Log.IfInfoFormat("MasterDataSync Service - Exited");
          break;
        case TopshelfExitCode.AbnormalExit:
          Log.IfDebugFormat("MasterDataSync Service - AbnormalExit");
          break;
        case TopshelfExitCode.SudoRequired:
          Log.IfDebugFormat("MasterDataSync Service - SudoRequired");
          break;
        case TopshelfExitCode.ServiceAlreadyInstalled:
          Log.IfDebugFormat("MasterDataSync Service - ServiceAlreadyInstalled");
          break;
        case TopshelfExitCode.ServiceNotInstalled:
          Log.IfDebugFormat("MasterDataSync Service - ServiceNotInstalled");
          break;
        case TopshelfExitCode.StartServiceFailed:
          Log.IfDebugFormat("MasterDataSync Service - StartServiceFailed");
          break;
        case TopshelfExitCode.StopServiceFailed:
          Log.IfDebugFormat("MasterDataSync Service - StopServiceFailed");
          break;
      }
    }

    private static MasterDataSyncService ServiceFactory(HostSettings settings)
    {
      var builder = new ContainerBuilder();

      builder.RegisterType<AppConfigurationManager>()
        .As<IConfigurationManager>()
        .SingleInstance();

      builder.RegisterType<HttpRequestWrapper>()
        .As<IHttpRequestWrapper>()
        .SingleInstance();

      builder.RegisterType<CacheManager>()
        .As<ICacheManager>()
        .SingleInstance();

      builder.RegisterType<TPassAuthorizationManager>()
        .As<ITpassAuthorizationManager>()
        .SingleInstance();

      builder.RegisterType<Implementation.TaskScheduler>()
        .SingleInstance();

      builder.RegisterType<MasterDataSyncService>()
        .SingleInstance();

      Container = builder.Build();
      return Container.Resolve<MasterDataSyncService>();

    }
  }
}