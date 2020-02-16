using System;

namespace VSS.Nighthawk.DataOut.Interfaces.Models.BlockDefinitions.MaintenanceMode
{
  [Serializable]
  public class EnableMaintenanceMode : Block
  {
    public DateTime StartUtc { get; set; }
    public int DurationHours { get; set; }
  }
}
