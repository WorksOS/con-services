using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;

namespace VSS.TRex.Designs.Executors
{
  public class CalculateDesignProfile
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignProfile>();

    private static IDesignFiles designs;

    private IDesignFiles Designs => designs ?? (designs = DIContext.Obtain<IDesignFiles>());

    /// <summary>
    /// Default no-args constructor
    /// </summary>
    public CalculateDesignProfile()
    {
    }

    /// <summary>
    /// Performs the donkey work of the profile calculation
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="calcResult"></param>
    /// <returns></returns>
    private List<XYZS> Calc(CalculateDesignProfileArgument arg, out DesignProfilerRequestResult calcResult)
    {
      calcResult = DesignProfilerRequestResult.UnknownError;

      IDesignBase Design = Designs.Lock(arg.ReferenceDesign.DesignID, arg.ProjectID, arg.CellSize, out DesignLoadResult LockResult);

      if (Design == null)
      {
        Log.LogWarning($"Failed to read file for design {arg.ReferenceDesign.DesignID}");
        calcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
        return null;
      }

      try
      {
        var result = Design.ComputeProfile(arg.ProfilePath, arg.CellSize);
        //Apply any offset to the profile
        if (arg.ReferenceDesign.Offset != 0)
        {
          for (var i=0; i<result.Count; i++)
          {
            result[i] = new XYZS(result[i].X, result[i].Y, result[i].Z + arg.ReferenceDesign.Offset, result[i].Station, result[i].TriIndex);
          }
        }
        calcResult = DesignProfilerRequestResult.OK;

        return result;
      }
      finally
      {
        Designs.UnLock(arg.ReferenceDesign.DesignID, Design);
      }
    }

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    /// <returns></returns>
    public List<XYZS> Execute(CalculateDesignProfileArgument args, out DesignProfilerRequestResult calcResult)
    {
      // Perform the design profile calculation
      var result = Calc(args, out calcResult);

      if (result == null)
      {
        Log.LogInformation($"Unable to calculate a design profiler result for {args}");
        result = new List<XYZS>();
      }

      return result;
    }
  }
}
