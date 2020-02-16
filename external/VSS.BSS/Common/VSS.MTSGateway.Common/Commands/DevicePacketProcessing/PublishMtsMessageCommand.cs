using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSS.Nighthawk.MTSGateway.Common.Helpers;

namespace VSS.Nighthawk.MTSGateway.Common.Commands.DevicePacketProcessing
{
  public class PublishMTSMessageCommand
  {
    public ManualResetEvent WaitEvent { get; set; }
    public long ID { get; set; }
    public PublishStatusEnum PublishStatus { get; set; }
  }
}
