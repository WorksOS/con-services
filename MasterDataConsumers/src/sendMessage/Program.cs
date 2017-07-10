using System;
using System.Text;
using Newtonsoft.Json;
using RdKafka;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.sendMessage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var random = new Random();
            using (Producer producer = new Producer("localhost:9092"))
            using (Topic topic = producer.Topic("VSS.Interfaces.Events.MasterData.ISubscriptionEvent-VUP"))
            {
                var asset = new CreateAssetSubscriptionEvent();

                var jsonHelper = new JsonHelper();
                var messagePayload = jsonHelper.SerializeObjectToJson(new { CreateProjectSubscriptionEvent = asset});
                byte[] data = Encoding.UTF8.GetBytes(messagePayload);
                DeliveryReport deliveryReport = topic.Produce(data).Result;
                Console.WriteLine($"Produced to Partition: {deliveryReport.Partition}, Offset: {deliveryReport.Offset}");
            }
        }


    }

    public class JsonHelper
    {
        public string SerializeObjectToJson<T>(T msg)
        {
            return JsonConvert.SerializeObject(msg);
        }

        public T DeserializeJsonToObject<T>(string msg)
        {
            return JsonConvert.DeserializeObject<T>(msg);
        }
    }


}
