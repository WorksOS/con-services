using System;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaConsumer
{
    public interface IKafkaConsumer<T> : IDisposable
    {
        void SetTopic(string topic);
        Task StartProcessingAsync(CancellationTokenSource token);
        void StopProcessing();
    }
}