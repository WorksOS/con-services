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
        private int batchSize = 100;

        public KafkaConsumer(IConfigurationStore config, IKafka driver, IRepositoryFactory repositoryFactory, IMessageTypeResolver resolver)
        {
            kafkaDriver = driver;
            configurationStore = config;
            dbRepositoryFactory = repositoryFactory;
            messageResolver = resolver;
            batchSize=configurationStore.GetValueInt("KAFKA_BATCH_SIZE");
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


        private int batchCounter = 0;
        private void ProcessMessage()
        {
            Console.WriteLine("Consuming");
            var messages = kafkaDriver.Consume(TimeSpan.FromMilliseconds(100));
            if (messages.message == Error.NO_ERROR)
                foreach (var message in messages.payload)
                {
                    try
                    {
                        string bytesAsString = Encoding.UTF8.GetString(message, 0, message.Length);
                        //Debugging only
                        Console.WriteLine(typeof(T)+" : "+bytesAsString );
                        var deserializedObject = JsonConvert.DeserializeObject<T>(bytesAsString,
                            messageResolver.GetConverter<T>());
                        Console.WriteLine("Saving");
                        dbRepositoryFactory.GetRepository<T>().StoreEvent(deserializedObject);
                    }
                    catch 
                    {

                    }
                    finally
                    {
                        Console.WriteLine("Commiting");
                        if (batchCounter > batchSize)
                        {
                            kafkaDriver.Commit();
                            batchCounter = 0;
                        }
                        else
                            batchCounter++;
                    }
                }
            if (messages.message == Error.NO_DATA&&batchCounter!=0)
            {
                kafkaDriver.Commit();
                batchCounter = 0;
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