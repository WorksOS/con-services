using System;

namespace LandfillService.AcceptanceTests.LandFillKafka
{
    public interface IKafkaDriver
    {
        string SendMessage(string topic, string message);
    }
}
