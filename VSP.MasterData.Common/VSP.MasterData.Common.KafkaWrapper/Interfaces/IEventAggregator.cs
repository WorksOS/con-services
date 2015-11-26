namespace VSP.MasterData.Common.KafkaWrapper.Interfaces
{
  public interface IEventAggregator
  {
    void Subscribe<TEvent>(ISubscriber<TEvent> subscribe);
    void ProcessMessage<TEvent>(TEvent eventToPublish);
  }
}