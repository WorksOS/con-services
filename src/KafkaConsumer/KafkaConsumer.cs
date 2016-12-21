using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.UnifiedProductivity.Service.Interfaces;
using VSS.UnifiedProductivity.Service.Utils;

namespace KafkaConsumer
{
    public class KafkaConsumer<T> : IKafkaConsumer<T>
    {
        private readonly IKafka kafkaDriver;
        private readonly IConfigurationStore configurationStore;
        private readonly IRepositoryFactory dbRepositoryFactory;
        private readonly IMessageTypeResolver messageResolver;

        private string topicName;
        private CancellationTokenSource stopToken;

        public KafkaConsumer(IConfigurationStore config, IKafka driver, IRepositoryFactory repositoryFactory, IMessageTypeResolver resolver)
        {
            kafkaDriver = driver;
            configurationStore = config;
            dbRepositoryFactory = repositoryFactory;
            messageResolver = resolver;
        }


        public void SetTopic(string topic)
        {
            topicName = topic;
            kafkaDriver.InitConsumer(configurationStore);
            kafkaDriver.Subscribe(new List<string>() {topic+configurationStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX") });

        }

        public Task StartProcessingAsync(CancellationTokenSource token)
        {
            stopToken = token;
            return Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    ProcessMessage();
                }
            }, TaskCreationOptions.LongRunning);

        }

        private void ProcessMessage()
        {
            var messages = kafkaDriver.Consume(TimeSpan.FromSeconds(10));
            if (messages.message == Error.NO_ERROR)
                foreach (var message in messages.payload)
                {
                    try
                    {
                        string bytesAsString = Encoding.UTF8.GetString(message, 0, message.Length);
                        var deserializedObject = JsonConvert.DeserializeObject<T>(bytesAsString,
                            messageResolver.GetConverter<T>());
                        dbRepositoryFactory.GetRepository<T>().StoreEvent(deserializedObject);
                    }
                    catch 
                    {

                    }
                    finally
                    {
                        kafkaDriver.Commit();
                    }
                }

        }

        public void StopProcessing()
        {
            stopToken.Cancel();
        }

        public void Dispose()
        {
            kafkaDriver.Dispose();
        }

    }
}