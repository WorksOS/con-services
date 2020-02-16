using VSS.Nighthawk.DataOut.Interfaces.Enums;
using VSS.Nighthawk.DataOut.Interfaces.Models;

namespace VSS.Nighthawk.DataOut.Interfaces.Commands
{
  public interface ISendOutboundMessageCommand
  {
    int RetryMax { get; set; }

    int RetryCount { get; set; }

    Message Message { get; set; }

    string MessageId { get; set; } //we need this in order to easily update the Document

    long EndpointDefinitionId { get; set; }

    string EndpointUrl { get; set; }

    string EndpointContentType { get; set; }

    string EndpointUserName { get; set; }

    byte[] EndpointEncryptedPwd { get; set; }

    MessageStatusEnum MessageStatus { get; set; }
  }
}
