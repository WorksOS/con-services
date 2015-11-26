namespace VSP.MasterData.Common.KafkaWrapper.Interfaces
{
  public interface ISubscriber<in T>
  {
    void Handle(T message);
  }
}