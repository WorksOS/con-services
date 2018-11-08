using System;
using System.Threading;
using VSS.Kafka.Ikvm.Client;
using java.util;
using org.apache.kafka.clients.producer;

namespace LandfillService.AcceptanceTests.LandFillKafka
{
    public class KafkaResolver : IKafkaDriver
    {
        public KafkaProducer javaProducer;
        public KafkaResolver()
        {
            IkvmKafkaInitializer.Initialize();
            log4net.Config.XmlConfigurator.Configure();
            var props = new Properties();
            props.put("bootstrap.servers", Config.KafkaEndpoint);
            props.put("acks", "all");
            props.put("retries", "0");
            props.put("batch.size", "16384");
            props.put("linger.ms", "1");
            props.put("buffer.memory", "33554432");
            props.put("key.serializer", "org.apache.kafka.common.serialization.StringSerializer");
            props.put("value.serializer", "org.apache.kafka.common.serialization.StringSerializer");

            javaProducer = new KafkaProducer(props);
        }

        /// <summary>
        /// Send the message to Kafka
        /// </summary>        
        public string SendMessage(string topic, string message)
        {
            Thread.Sleep(50);
            var producerRecord = new ProducerRecord(topic, message);
            return javaProducer.send(producerRecord, new MainCallback()).get().ToString();
        }

        class MainCallback : Callback
        {
            public void onCompletion(RecordMetadata metadata, java.lang.Exception e)
            {
                if (e != null)
                    e.printStackTrace();

                Console.WriteLine("The offset of the record we just sent is: {0}", metadata.offset());
            }
        }

        public void CloseProducer()
        {
            javaProducer.flush();
            javaProducer.close();
        }
    }
}
