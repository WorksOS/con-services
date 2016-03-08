using VSS.Kafka.DotNetClient.Model;

namespace VSS.MasterData.Customer.Processor.Interfaces
{
  public interface IObserverHandler
  {
    void Commit(ConsumerInstanceResponse consumerInstanceResponse);
  }
}
