using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models
{
  [Serializable]
  public class Destination
  {
    public Device Device { get; set; }
    public Asset Asset { get; set; }
  }
}
