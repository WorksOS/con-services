using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.Executors
{
  public class CalculateDesignProfile
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignProfile>();

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

      DesignBase Design = DesignFiles.Designs.Lock(arg.DesignDescriptor, arg.ProjectID, arg.CellSize, out DesignLoadResult LockResult);

      if (Design == null)
      {
        Log.LogWarning($"Failed to read design file {arg.DesignDescriptor.FullPath}");
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
        DesignFiles.Designs.UnLock(arg.DesignDescriptor, Design);
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
          // TODO: Handle failure to calculate a design profile result
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
