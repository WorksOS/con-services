using RdKafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace sendMessage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var random = new Random();
            using (Producer producer = new Producer("localhost:9092"))
            using (Topic topic = producer.Topic("VSS.Interfaces.Events.MasterData.IAssetEvent-VUP"))
            {
                var asset = new CreateAssetEvent();

                var jsonHelper = new JsonHelper();
                var messagePayload = jsonHelper.SerializeObjectToJson(new {CreateAssetEvent = asset});
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
