using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Nighthawk.MTSGateway.Interfaces.Events
{
  public interface IAddressClaimUpdatedEvent
  {
    string SerialNumber { get; set; }
    int DeviceType { get; set; }
    Dictionary<byte, string> Addresses { get; set; }
  }
}
