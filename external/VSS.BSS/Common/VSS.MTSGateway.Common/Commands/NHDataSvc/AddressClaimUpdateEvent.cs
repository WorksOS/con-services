using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Nighthawk.MTSGateway.Interfaces.Events;

namespace VSS.Nighthawk.MTSGateway.Common.Commands.NHDataSvc
{
  public class AddressClaimUpdateEvent : IAddressClaimUpdatedEvent
  {
    public string SerialNumber { get; set; }

    public int DeviceType { get; set; }

    public Dictionary<byte, string> Addresses { get; set; }
  }
}
