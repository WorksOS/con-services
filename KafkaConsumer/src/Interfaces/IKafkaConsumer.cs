using System;
using System.Threading;
using System.Threading.Tasks;

namespace VSS.KafkaConsumer.Interfaces
{
    public interface IAbstractKafkaConsumer
    {
        void SetTopic(string topic);
        Task<Task> StartProcessingAsync(CancellationTokenSource token);
        void StopProcessing();
        void StartProcessingSync();
    }

    public interface IKafkaConsumer<T> : IAbstractKafkaConsumer, IDisposable
    {

    }
}