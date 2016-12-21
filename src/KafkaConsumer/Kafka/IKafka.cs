using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.UnifiedProductivity.Service.Utils;

namespace VSS.UnifiedProductivity.Service.Interfaces
{
  public interface IKafka
  {
    string ConsumerGroup { get; set; }
    string OffsetReset { get; set;  }
    string Uri { get; set; }
    bool EnableAutoCommit { get; set; }
    int Port { get; set; }
    void Subscribe(List<string> topics);
    void Commit();
    void InitConsumer(IConfigurationStore configurationStore, string groupName = null);
    Message Consume(TimeSpan timeout);
    void Dispose();
 
  }

  public class Message
  {
    public IEnumerable<byte[]> payload { get; }
    public Error message { get; }
    public Message(IEnumerable<byte[]> payload, Error message)
    {
      this.payload = payload;
      this.message = message;
    }
  }

  public enum Error
  {
    NO_ERROR,
    NO_DATA
  }
}
