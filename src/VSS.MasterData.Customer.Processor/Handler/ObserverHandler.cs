using VSS.MasterData.Customer.Processor.Interfaces;

namespace VSS.MasterData.Customer.Processor.Handler
{
  class ObserverHandler : IObserverHandler
  {
    public void Commit(Kafka.DotNetClient.Model.ConsumerInstanceResponse consumerInstanceResponse)
    {
      consumerInstanceResponse.Commit();
    }
  }
}
