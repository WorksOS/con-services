namespace VSP.MasterData.Common.KafkaWrapper.Interfaces
{
  public interface IProducerWrapper
  {
    void Publish(string message, string key = "", string topicOverride = null);
  }
}