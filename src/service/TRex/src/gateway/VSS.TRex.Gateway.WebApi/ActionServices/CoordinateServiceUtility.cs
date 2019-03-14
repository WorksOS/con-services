using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common.Utilities;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.Geometry;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.WebApi.ActionServices
{
  /// <summary>
  /// Check that the site model exists, and appropriate designs exist.
  /// </summary>
  public class CoordinateServiceUtility : ICoordinateServiceUtility
  {
    /// <summary>
    /// Converts XYZ to LLH
    /// </summary>
    public int PatchLLH(string CSIB, List<MachineStatus> machines)
    {
      if (!machines.Any())
        return ContractExecutionStatesEnum.ExecutedSuccessfully;

      var coordPointer = 0;
      var NEECoords = new XYZ[machines.Count];
      foreach (var machine in machines)
      {
        if (machine.lastKnownX != null && machine.lastKnownY != null && NEECoords.Length > coordPointer)
          NEECoords[coordPointer++] = new XYZ(machine.lastKnownX.Value, machine.lastKnownY.Value);
      }

      (var errorCode, XYZ[] LLHCoords) = ConvertCoordinates.NEEToLLH(CSIB, NEECoords);
      if (errorCode == RequestErrorStatus.OK && LLHCoords.Length > 0)
      {
        coordPointer = 0;
        foreach (var machine in machines)
        {
          if (machine.lastKnownX != null && machine.lastKnownY != null)
          {
            machine.lastKnownLatitude = MathUtilities.RadiansToDegrees(LLHCoords[coordPointer].Y);
            machine.lastKnownLongitude = MathUtilities.RadiansToDegrees(LLHCoords[coordPointer++].X);
          }
        }
      }
      else
        return ContractExecutionStatesEnum.InternalProcessingError;

      return ContractExecutionStatesEnum.ExecutedSuccessfully;
    }
  }
}
