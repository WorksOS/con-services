using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.Executors
{
  public class CalculateAlignmentDesignStationRange
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateAlignmentDesignStationRange>();

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    /// <param name="projectUID"></param>
    /// <param name="referenceDesignUID"></param>
    /// <returns></returns>
    public FencePoint Execute(Guid projectUID, Guid referenceDesignUID)
    {
      try
      {
        // Perform the calculation
        FencePoint result = null;

        // Todo: This implementation is dependent on a .Net standard version of the Symphony SDK
        throw new NotImplementedException("Alignment design station range implementation is dependent on a .Net standard version of the Symphony SDK");

        if (result == null)
        {
          // TODO: Handle failure to calculate the boundary
        }

        return result;
      }
      catch (Exception e)
      {
        Log.LogError(e, "Execute: Exception: ");
        return null;
      }
    }
  }
}
