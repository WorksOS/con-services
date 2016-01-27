using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using KafkaNet.Common;
using log4net;
using Microsoft.Practices.Unity;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Landfill.Common.Interfaces;
using VSS.VisionLink.Landfill.Common.Models;
using VSS.VisionLink.Landfill.MDM;
using VSS.VisionLink.Landfill.MDM.Interfaces;

namespace VSS.VisionLink.Landfill.DataFeed
{

  #region KafkaQueueProjectEventPuller

  public abstract class KafkaQueueSubscriptionsEventPuller : IKafkaQueuePuller
  {
    private static readonly AsyncLock Locker = new AsyncLock();

    protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly UnityContainer container;
    private readonly string kafkaTopic;

    protected KafkaQueueSubscriptionsEventPuller(UnityContainer dependencyContainer, string kafkaTopicName)
    {
      container = dependencyContainer;
      kafkaTopic = kafkaTopicName;
    }

    public async Task<bool> PullAndProcess(CancellationToken token)
    {
      var kafkaReader = new KafkaQueueReader<ISubscriptionEvent>(container, kafkaTopic);
      var ruleExecutor = new RulePipelineExecutor<ISubscriptionEvent>();
      try
      {
        RegisterRules(ruleExecutor);

        if (!await kafkaReader.ReadNextEvent())
        {
          log.DebugFormat("KafkaQueueSubscriptionEventPuller: received bad (null) event - unable to deserialize?");
          //Bad event returned here here
          return false;
        }
        log.DebugFormat("KafkaQueueSubscriptionEventPuller: executing {0} rules for message",
          ruleExecutor.RegisteredRules.Count);
        var processedEvent = ruleExecutor.ExecuteRules(kafkaReader.QueuedEvent);
        log.DebugFormat("KafkaQueueProjectEventPuller: rules executed - result is null? {0}", processedEvent == null);

        if (processedEvent != null)
        {
       //   var eventName = EventName(processedEvent);
        //  log.DebugFormat("KafkaQueueSubscriptionEventPuller: ProcessedEvent AssetUid={0} eventType={1} ActionUtc={2} ReceivedUtc={3}",
         //   processedEvent.SubscriptionUID , eventName, processedEvent.ActionUTC, processedEvent.ReceivedUTC);

          await UpdateProjectDetail(processedEvent);
          log.DebugFormat("KafkaQueueSubscriptionEventPuller: back from savedAssetDetails");

          log.DebugFormat("KafkaQueueSubscriptionEventPuller: saving offset for topic {0} : {1}", kafkaTopic,
            kafkaReader.QueuedOffset);

          //And save kafka offset
          await container.Resolve<IBookmarkRepository>().SaveBookmark(
            new Bookmark { BookmarkType = kafkaReader.ItemBookmarkType, Value = kafkaReader.QueuedOffset });
        }
      }
      catch (Exception ex)
      {
        log.Error("KafkaQueueSubscriptionEventPuller: something really bad happened here", ex);
        return false;
      }
      return true;
    }

    protected virtual void RegisterRules(RulePipelineExecutor<ISubscriptionEvent> ruleExecutor)
    {
      ruleExecutor.RegisterRule(new ValidateSubscriptionRule());
    }


    private async Task UpdateProjectDetail(ISubscriptionEvent processedEvent)
    {
      log.DebugFormat("KafkaQueueSubscriptionEventPuller Upserting AssetDetail");
      var repo = container.Resolve<ISubscriptionRepository>();
      await repo.StoreSubscription(processedEvent);
    }

  
  /*  //For logging
    private string EventName(ISubscriptionEvent evt)
    {
      if (evt is CreateSubscriptionEvent)
        return "CreateSubscriptionEvent";
      if (evt is UpdateSubscriptionEvent)
        return "UpdateSubscriptionEvent";
      return "Unknown";
    }*/
    
  }

  #endregion

  #region KafkaQueueCreateProjectEventPuller

  public class KafkaQueueCreateSubscriptionEventPuller : KafkaQueueSubscriptionsEventPuller
  {
    public KafkaQueueCreateSubscriptionEventPuller(UnityContainer dependencyContainer, string kafkaTopicName)
      : base(dependencyContainer, kafkaTopicName)
    {
    }
  }

  #endregion

  #region KafkaQueueUpdateProjectEventPuller

  public class KafkaQueueUpdateSubscriptionEventPuller : KafkaQueueSubscriptionsEventPuller
  {
    public KafkaQueueUpdateSubscriptionEventPuller(UnityContainer dependencyContainer, string kafkaTopicName)
      : base(dependencyContainer, kafkaTopicName)
    {
    }
  }

  #endregion

  #region KafkaQueueDeleteProjectEventPuller

  public class KafkaQueueDeleteSubscriptionEventPuller : KafkaQueueSubscriptionsEventPuller
  {
    public KafkaQueueDeleteSubscriptionEventPuller(UnityContainer dependencyContainer, string kafkaTopicName)
      : base(dependencyContainer, kafkaTopicName)
    {
    }
  }

  #endregion

}