using System;
using VSS.Kafka.DotNetClient.Consumer;
using VSS.Kafka.DotNetClient.Model;
using VSS.Subscription.Processor.Interfaces;

namespace VSS.Subscription.Processor.Consumer
{
	public class ConsumerWrapper
	{
		IConsumerConfigurator _consumerConfigurator;
		BinaryConsumer _consumer;

		public ConsumerWrapper(IConsumerConfigurator consumerConfigurator)
		{
			_consumerConfigurator = consumerConfigurator;

			_consumer = new BinaryConsumer(_consumerConfigurator.GetRestProxySettings());
		}
		
		
		public IDisposable Subscribe(IObserver<ConsumerInstanceResponse> observer)
		{
			return _consumer.Subscribe(observer);
		}

		public void StartConsume()
		{
			try
			{
				_consumer.ConsumeMessages(_consumerConfigurator.GetConsumerRequest());
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error - " + ex.ToString());
			}

		}

		public void Dispose()
		{
			_consumer.Dispose();
		}

	}
}
