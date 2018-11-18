using System;
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

    private static IDesignFiles designs = null;

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
    private XYZS[] Calc(CalculateDesignProfileArgument arg, out DesignProfilerRequestResult calcResult)
    {
      calcResult = DesignProfilerRequestResult.UnknownError;

      IDesignBase Design = Designs.Lock(arg.DesignUid, arg.ProjectID, arg.CellSize, out DesignLoadResult LockResult);

      if (Design == null)
      {
        Log.LogWarning($"Failed to read file for design {arg.DesignUid}");
        calcResult = DesignProfilerRequestResult.FailedToLoadDesignFile;
        return null;
      }

      try
      {
        XYZS[] result = Design.ComputeProfile(arg.ProfilePath, arg.CellSize);
        calcResult = DesignProfilerRequestResult.OK;

        return result;
      }
      finally
      {
        Designs.UnLock(arg.DesignUid, Design);
      }
    }

    /// <summary>
    /// Performs execution business logic for this executor
    /// </summary>
    /// <returns></returns>
    public XYZS[] Execute(CalculateDesignProfileArgument args)
    {
      try
      {
        // Perform the design profile calculation
        var result = Calc(args, out DesignProfilerRequestResult CalcResult);

        if (result == null)
        {
          Log.LogInformation($"Unable to calculate a design profiler result for {args}");
          result = new XYZS[0];
        }

        return result;
      }
      catch (Exception E)
      {
        Log.LogError($"Execute: Exception {E}");
        return null;
      }
    }
  }
}
