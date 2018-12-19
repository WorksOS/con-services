using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.Executors
{
  public class CalculateAlignmentDesignFilterBoundary
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignElevationPatch>();

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    /// <returns></returns>
    public Fence Execute(Guid projectUID, Guid referenceDesignUID, double startSTation, double endStation, double leftOffset, double rightOffset)
    {
      try
      {
        // Perform the calculation
        Fence result = null;

        // Todo: This implementation is dependent on a .Net standard version of the Symphony SDK
        throw new NotImplementedException("Alignment filter boundary implementation is dependent on a .Net standard version of the Symphony SDK");

        if (result == null)
        {
          // TODO: Handle failure to calculate the boundary
        }

        return result;
      }
      catch (Exception E)
      {
        Log.LogError(E, "Execute: Exception: ");
        return null;
      }
    }
  }
}
