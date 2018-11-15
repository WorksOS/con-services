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
    private XYZS[] Calc(CalculateDesignProfileArgument arg,
      out DesignProfilerRequestResult calcResult)
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
// Exclusive serialisation of the Design is not required in the Ignite POC
//     Design.AcquireExclusiveInterlock();
//     try
//     {
          XYZS[] result = Design.ComputeProfile(arg.ProfilePath, arg.CellSize);
          calcResult = DesignProfilerRequestResult.OK;
//     }
//     finally
//     {
//         Design.ReleaseExclusiveInterlock();
//     }

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
        try
        {
          /* Test code to force all subgrids to have 0 elevations from a design
          ClientHeightLeafSubGrid test = new ClientHeightLeafSubGrid(null, null, 6, 0.34, SubGridTreeConsts.DefaultIndexOriginOffset);
          test.SetToZeroHeight();
          return test;
          */

          // Calculate the patch of elevations and return it
          var result = Calc(args, out DesignProfilerRequestResult CalcResult);

          if (result == null)
          {
            // TODO: Handle failure to calculate a design profile result
          }

          return result;
        }
        finally
        {
          //if VLPDSvcLocations.Debug_PerformDPServiceRequestHighRateLogging then
          //Log.LogInformation($"#Out# {nameof(CalculateDesignElevationPatch)}.Execute #Result# {CaleResult}");
        }
      }
      catch (Exception E)
      {
        Log.LogError($"Execute: Exception {E}");
        return null;
      }
    }
  }
}
