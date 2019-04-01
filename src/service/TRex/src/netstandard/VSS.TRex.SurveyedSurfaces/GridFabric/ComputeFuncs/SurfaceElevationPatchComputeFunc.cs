using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SurveyedSurfaces.Executors;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs
{
  public class SurfaceElevationPatchComputeFunc : BaseComputeFunc, IComputeFunc<ISurfaceElevationPatchArgument, byte[]>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SurfaceElevationPatchComputeFunc>();

    /// <summary>
    /// Invokes the surface elevation patch computation function on the server nodes the request has been sent to
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public byte[] Invoke(ISurfaceElevationPatchArgument arg)
    {
      byte[] resultAsBytes = null;

      try
      {
        Log.LogDebug($"CalculateDesignElevationPatchComputeFunc: Arg = {arg}");

        CalculateSurfaceElevationPatch Executor = new CalculateSurfaceElevationPatch(arg);

        IClientLeafSubGrid result = Executor.Execute();

        if (result != null)
        {
          try
          {
            resultAsBytes = result.ToBytes();
          }
          finally
          {
            DIContext.Obtain<IClientLeafSubGridFactory>().ReturnClientSubGrid(ref result);
          }
        }
      }
      catch (Exception E)
      {
        Log.LogError(E, $"{nameof(SurfaceElevationPatchComputeFunc)}.Invoke: Exception:");
      }

      return resultAsBytes;
    }
  }
}
