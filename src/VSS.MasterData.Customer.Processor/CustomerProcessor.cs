using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using VSS.Kafka.DotNetClient.Consumer;
using VSS.Kafka.DotNetClient.Interfaces;
using VSS.Kafka.DotNetClient.Model;
//using VSP.MasterData.Common.KafkaWrapper;
using VSS.MasterData.Customer.Processor.Interfaces;


namespace VSS.MasterData.Customer.Processor
{
  public class CustomerProcessor : ICustomerProcessor
  {
    private readonly IObserver<ConsumerInstanceResponse> _customerEventObserver;
    readonly BinaryConsumer _customerEventConsumer;
    IDisposable subscriber;
    CreateConsumerRequest _consumerRequest;
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public CustomerProcessor(IObserver<ConsumerInstanceResponse> customerEventObserver,
      BinaryConsumer customerEventConsumer, CreateConsumerRequest consumerRequest)
    {
      try
      {
        _customerEventObserver = customerEventObserver;
        _customerEventConsumer = customerEventConsumer;
        _consumerRequest = consumerRequest;
        subscriber = customerEventConsumer.Subscribe(customerEventObserver);
      }
      catch (Exception error)
      {
        Log.Error("Error creating the consumer" + error.Message + error.StackTrace, error);
      }
    }

    public void Process()
    {
      _customerEventConsumer.ConsumeMessages(_consumerRequest);
    }

    public void Stop()
    {
      _customerEventConsumer.Dispose();
      subscriber.Dispose();
    }
  }
}
