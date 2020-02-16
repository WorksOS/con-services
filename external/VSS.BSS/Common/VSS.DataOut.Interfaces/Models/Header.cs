using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models
{
  [Serializable]
  public class Header
  {
    public Destination Destination { get; set; }

    public DateTime TimestampUtc { get; set; }

    public bool ShouldSerializeTimestampUtc()
    {
      return TimestampUtc > DateTime.MinValue;
    }
  }
}
