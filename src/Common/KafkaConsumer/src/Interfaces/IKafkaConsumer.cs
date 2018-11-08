using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VSS.KafkaConsumer.Interfaces
{
    public interface IAbstractKafkaConsumer
    {
        void SetTopic(string topic, string consumerGroup = null);
        Task<Task> StartProcessingAsync(CancellationTokenSource token);
        void StopProcessing();
        void StartProcessingSync();
        void OverrideLogger(ILogger logger);
        void SubscribeObserverConsumer();
    }

    public interface IKafkaConsumer<T> : IAbstractKafkaConsumer, IDisposable
    {

    }
}