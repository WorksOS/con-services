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
    public (double startStation, double endStation) Execute(Guid projectUID, Guid referenceDesignUID)
    {
      try
      {
        // Perform the calculation
        (double startStation, double endStation) result = (double.MaxValue, double.MinValue);

        // Todo: This implementation is dependent on a .Net standard version of the Symphony SDK
        throw new NotImplementedException("Alignment design station range implementation is dependent on a .Net standard version of the Symphony SDK");

        if (result.startStation == Double.MaxValue || result.endStation == Double.MinValue)
        {
          // TODO: Handle failure to calculate the station range
        }

        //return result;
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Failed to compute alignment design station station range. Site Model ID: {projectUID} design ID: {referenceDesignUID}");
        return (double.MaxValue, double.MinValue);
      }
    }
  }
}
