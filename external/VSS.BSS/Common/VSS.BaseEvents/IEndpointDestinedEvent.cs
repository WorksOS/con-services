using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.BaseEvents
{
  public interface IEndpointDestinedEvent
  {
    EndpointDefinition[] Destinations { get; set; }
  }
}
