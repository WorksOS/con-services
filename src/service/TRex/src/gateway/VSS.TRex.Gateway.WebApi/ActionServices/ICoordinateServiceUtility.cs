using System.Collections.Generic;
using VSS.Productivity3D.Models.Models;

namespace VSS.TRex.Gateway.WebApi.ActionServices
{
  public interface ICoordinateServiceUtility
  {
    /// <summary>
    /// Converts XYZ to LLH
    /// </summary>
    int PatchLLH(string CSIB, List<MachineStatus> machines);
  }
}
