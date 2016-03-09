using log4net;
using System;
using System.Reflection;
using VSS.Customer.Data.Interfaces;
using VSS.Kafka.DotNetClient.Model;
using VSS.Customer.Processor.Consumer;
using VSS.Customer.Processor.Interfaces;

namespace VSS.Customer.Processor
{
  public class CustomerProcessor : ICustomerProcessor
  {
    private readonly IObserver<ConsumerInstanceResponse> _observer;
    private readonly IDisposable _subscriber;
    private readonly ConsumerWrapper _consumerWrapper;

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    public CustomerProcessor(ICustomerService service, IConsumerConfigurator configurator)
    {
      try
      {
        _consumerWrapper = new ConsumerWrapper(configurator);
        _subscriber = _consumerWrapper.Subscribe(new CustomerEventObserver(service));
      }
      catch (Exception error)
      {
        Log.Error("Error creating the consumer" + error.Message + error.StackTrace, error);
      }
    }

    public void Process()
    {
      _consumerWrapper.StartConsume();
    }

    public void Stop()
    {
      if (_subscriber != null)
        _subscriber.Dispose();
      _consumerWrapper.Dispose();
    }
  }
}
