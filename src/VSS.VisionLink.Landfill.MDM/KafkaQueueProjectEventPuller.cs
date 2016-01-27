using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using KafkaNet.Common;
using log4net;
using Microsoft.Practices.Unity;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.VisionLink.Landfill.Common.Interfaces;
using VSS.VisionLink.Landfill.Common.Models;
using VSS.VisionLink.Landfill.MDM;
using VSS.VisionLink.Landfill.MDM.Interfaces;

namespace VSS.VisionLink.Landfill.DataFeed
{

  public class KafkaQueueProjectEventPuller : IKafkaQueuePuller
  {
    private static readonly AsyncLock Locker = new AsyncLock();

    protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly UnityContainer container;
    private readonly string kafkaTopic;

    public KafkaQueueProjectEventPuller(UnityContainer dependencyContainer, string kafkaTopicName)
    {
      container = dependencyContainer;
      kafkaTopic = kafkaTopicName;
    }

    public async Task<bool> PullAndProcess(CancellationToken token)
    {
      var kafkaReader = new KafkaQueueReader<IProjectEvent>(container, kafkaTopic);
      var ruleExecutor = new RulePipelineExecutor<IProjectEvent>();
      try
      {
        RegisterRules(ruleExecutor);

        if (!await kafkaReader.ReadNextEvent())
        {
          log.DebugFormat("KafkaQueueProjectEventPuller: received bad (null) event - unable to deserialize?");
          //Bad event returned here here
          return false;
        }
        log.DebugFormat("KafkaQueueProjectEventPuller: executing {0} rules for message",
          ruleExecutor.RegisteredRules.Count);
        var processedEvent = ruleExecutor.ExecuteRules(kafkaReader.QueuedEvent);
        log.DebugFormat("KafkaQueueProjectEventPuller: rules executed - result is null? {0}", processedEvent == null);

        if (processedEvent != null)
        {
          var eventName = EventName(processedEvent);
          log.DebugFormat("KafkaQueueProjectEventPuller: ProcessedEvent AssetUid={0} eventType={1} ActionUtc={2} ReceivedUtc={3}",
            processedEvent.ProjectUID , eventName, processedEvent.ActionUTC, processedEvent.ReceivedUTC);

          await UpdateProjectDetail(processedEvent);
          log.DebugFormat("KafkaQueueProjectEventPuller: back from savedAssetDetails");

          log.DebugFormat("KafkaQueueProjectEventPuller: saving offset for topic {0} : {1}", kafkaTopic,
            kafkaReader.QueuedOffset);

          //And save kafka offset
          await container.Resolve<IBookmarkRepository>().SaveBookmark(
            new Bookmark { BookmarkType = kafkaReader.ItemBookmarkType, Value = kafkaReader.QueuedOffset });
        }
      }
      catch (Exception ex)
      {
        log.Error("KafkaQueueProjectEventPuller: something really bad happened here", ex);
        return false;
      }
      return true;
    }

    protected virtual void RegisterRules(RulePipelineExecutor<IProjectEvent> ruleExecutor)
    {
      ruleExecutor.RegisterRule(new ValidateProjectRule());
    }


    private async Task UpdateProjectDetail(IProjectEvent processedEvent)
    {
      log.DebugFormat("KafkaQueueProjectEventPuller Upserting AssetDetail");
      var repo = container.Resolve<IProjectRepository>();
      await repo.StoreProject(processedEvent);
    }

  
    //For logging
    private string EventName(IProjectEvent evt)
    {
      if (evt is CreateProjectEvent)
        return "CreateProjectEvent";
      if (evt is UpdateProjectEvent)
        return "UpdateProjectEvent";
      if (evt is DeleteProjectEvent)
        return "DeleteProjectEvent";
      if (evt is AssociateProjectCustomer)
        return "AssociateProjectCustomerEvent";
      if (evt is DissociateProjectCustomer)
        return "DissociateProjectCustomerEvent";

      return "Unknown";
    }
    
  }

  
}