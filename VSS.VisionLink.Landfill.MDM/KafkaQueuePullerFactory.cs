using System.Reflection;
using log4net;
using Microsoft.Practices.Unity;
using VSS.VisionLink.Utilization.DataFeed.Interfaces;

namespace VSS.VisionLink.Utilization.DataFeed
{
  public class KafkaQueuePullerFactory
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public IKafkaQueuePuller GetPuller(UnityContainer container, string kafkaTopicName)
    {
      if (string.IsNullOrEmpty(kafkaTopicName))
      {
        Log.ErrorFormat("Exception as Kafka topic name must be initialised!");
        return null;
      }

      if (kafkaTopicName.Contains("CreateProjectEvent"))
        return new KafkaQueueCreateProjectEventPuller(container, kafkaTopicName);

      if (kafkaTopicName.Contains("UpdateProjectEvent"))
        return new KafkaQueueUpdateProjectEventPuller(container, kafkaTopicName);

      if (kafkaTopicName.Contains("DeleteProjectEvent"))
        return new KafkaQueueDeleteProjectEventPuller(container, kafkaTopicName);

      Log.ErrorFormat("Exception as Kafka topic '{0}' does not exist!", kafkaTopicName);
      return null;
    }
  }
}