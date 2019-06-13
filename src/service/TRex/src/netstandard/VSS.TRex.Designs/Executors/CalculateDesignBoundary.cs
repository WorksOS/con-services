using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.Executors
{
  public class CalculateDesignBoundary
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignBoundary>();

    public List<Fence> Execute(Guid projectUID, Guid referenceDesignUID, double tolerance)
    {
      try
      {
        // Perform the calculation
        List<Fence> result = null;

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
