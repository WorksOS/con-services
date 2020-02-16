using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions
{
  [Serializable]
  public class DailyReportFrequency : Block
  {
    public int Value { get; set; }
  }
}
