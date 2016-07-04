using KafkaNet;
using KafkaNet.Model;
using KafkaNet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using VSP.MasterData.Common.KafkaWrapper.Interfaces;

namespace VSP.MasterData.Common.KafkaWrapper
{
  public class ProducerWrapper : IProducerWrapper
  {
    private readonly string _topic;
    private readonly List<string> _uriList;
    private Producer _client;
    private bool _connected;
    private KafkaOptions _kafkaOptions;
    private BrokerRouter _router;

    public ProducerWrapper(string topic, List<string> uriList)
    {
      _topic = topic;
      _uriList = uriList;
    }

    public void Publish(string message, string key = "", string topicOverride = null)
    {
      if(!_connected)
        Connect();

        var msg = !string.IsNullOrEmpty(key) ? new Message(message, key) : new Message(message);
        _client.SendMessageAsync(string.IsNullOrEmpty(topicOverride) ? _topic : topicOverride, new[] { msg });
    }

    private void Connect()
    {
      _kafkaOptions = new KafkaOptions(_uriList.Select(x => new Uri(x)).ToArray());
      _router = new BrokerRouter(_kafkaOptions);
      _client = new Producer(_router);
      _connected = true;
    }
  }
}