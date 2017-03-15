using System;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaConsumer.Interfaces
{
    public interface IAbstractKafkaConsumer
    {
        void SetTopic(string topic);
        Task StartProcessingAsync(CancellationTokenSource token);
        void StopProcessing();
        void StartProcessingSync();
    }

    public interface IKafkaConsumer<T> : IAbstractKafkaConsumer, IDisposable
    {

    }
}