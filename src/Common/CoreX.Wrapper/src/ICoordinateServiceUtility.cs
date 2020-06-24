using System.Collections.Generic;
using VSS.Productivity3D.Productivity3D.Models.ProductionData;

namespace CoreX.Wrapper
{
  public interface ICoordinateServiceUtility
  {
    /// <summary>
    /// Converts XYZ to LLH
    /// </summary>
    bool PatchLLH(string csib, List<MachineStatus> machines);
  }
}
