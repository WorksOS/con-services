using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Nighthawk.ExternalDataTypes.Enumerations
{
  public enum DigitalInputMonitoringConditions
  {
    Always = 0x028C,
    KeyOffEngineOff = 0x028D,
    KeyOnEngineOff = 0x028E,
    KeyOnEngineOn = 0x028F,
  }
}
