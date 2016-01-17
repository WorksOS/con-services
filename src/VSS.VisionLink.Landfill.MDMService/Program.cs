using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Threading;
using log4net;
using Microsoft.Practices.Unity;
using Topshelf;
using Topshelf.Runtime;
using VSS.Interfaces.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.VisionLink.Landfill.Common.Interfaces;
using VSS.VisionLink.Landfill.Common.Utilities;
using VSS.VisionLink.Landfill.DataFeed;
using VSS.VisionLink.Landfill.Repositories;

namespace VSS.VisionLink.Landfill.MDMService
{
  internal static class Program
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private static void Main(string[] args)
    {
      var exitCode = HostFactory.Run(c =>
      {
        c.SetServiceName("_VSPLandfillMDMService");
        c.SetDisplayName("_VSPLandfillMDMService");
        c.SetDescription("Service for consuming MasterData events from Kafka topics and normalizing them.");
        c.RunAsLocalSystem();
        c.StartAutomatically();
        //Todo Specify recovery settings here
        /*        c.EnableServiceRecovery(cfg =>
                {
                  cfg.RestartService(1);
                  cfg.RestartService(1);
                  cfg.RestartService(1);
                });*/
        c.Service<ServiceController>(svc =>
        {
          svc.ConstructUsing(ServiceFactory);
          svc.WhenStarted(s => s.Start());
          svc.WhenStopped(s => s.Stop());
        });
        //TOdo Enable log4net support
        //c.UseLog4Net();
      });

      if (exitCode == TopshelfExitCode.Ok)
      {
        Log.InfoFormat("Landfill MDM Service - {0}", exitCode);
      }
      else
      {
        Log.DebugFormat("Landfill MDM Service - {0}", exitCode);
      }
    }


    private static ServiceController ServiceFactory(HostSettings settings)
    {
      return new ServiceController();
    }
  }


  public class ServiceController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly List<Thread> consumingThreads = new List<Thread>();

    private CancellationTokenSource cancelationTokenSource;
    private UnityContainer container;


    public void Start()
    {
      //Register here all dependencies for furhter consumption
      container = new UnityContainer();
 
      container.RegisterType<IKafkaQueue<IProjectEvent>, KafkaProjectEventQueue<IProjectEvent>>();

/*      container.RegisterType<IKafkaQueue<CreateSubscriptionEvent>, KafkaSubscriptionEventQueue<CreateSubscriptionEvent>>();
      container.RegisterType<IKafkaQueue<UpdateSubscriptionEvent>, KafkaSubscriptionEventQueue<UpdateSubscriptionEvent>>();*/

      var connectionString = ConfigSettings.GetConnectionString("VSPDB");
      container.RegisterType<IBookmarkRepository, BookmarkRepository>(new InjectionConstructor(connectionString));
      container.RegisterType<IProjectRepository, ProjectRepository>(new InjectionConstructor(connectionString));
     // container.RegisterType<ISubscriptionRepository, SubscriptionRepository>(new InjectionConstructor(connectionString));

      cancelationTokenSource = new CancellationTokenSource();

      Log.Debug("Landfill: Starting");
      //Build threads required to process all topics
      var kafkaTopics = ConfigurationManager.AppSettings["KafkaTopics"].Split(new[] { "," }, StringSplitOptions.None);

      //foreach (var kafkaTopic in kafkaTopics)
      foreach (var kafkaTopic in kafkaTopics)
      {
        var thread = new Thread(() => ConsumeTopic(kafkaTopic));
        consumingThreads.Add(thread);
        thread.Start();
        Log.InfoFormat("Landfill: Started topic {0}", kafkaTopic);
      }
    }

    private void ConsumeTopic(object obj)
    {
      var puller = new KafkaQueuePullerFactory().GetPuller(container, (string)obj);
      Log.DebugFormat("Landfill: Built puller for topic {0}", (string)obj);
      if (puller == null)
      {
        Log.ErrorFormat("Landfill: KafkaQueuePullerFactory failed instantiate KafkaQueuePuller class.");
        return;
      }

      while (!cancelationTokenSource.IsCancellationRequested)
      {
        Log.DebugFormat("Landfill: Pulling topic {0}", (string)obj);
        var task = puller.PullAndProcess(cancelationTokenSource.Token);
        task.Wait(cancelationTokenSource.Token);
      }
    }


    public void Stop()
    {
      cancelationTokenSource.Cancel();
    }
  }
}
