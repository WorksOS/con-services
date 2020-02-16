using System;
using VSS.Nighthawk.DataOut.Interfaces.Enums;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions
{
  [Serializable]
  public class ReportingFrequency: Block
  {
    public int Frequency { get; set; }
    public ReportingFrequencyInterval Interval { get; set; }
  }
}
