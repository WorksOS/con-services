using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Surfaces.Executors;
using VSS.TRex.Surfaces.GridFabric.Arguments;

namespace VSS.TRex.Surfaces.GridFabric.ComputeFuncs
{
  [Serializable]
  public class SurfaceElevationPatchComputeFunc : /*BaseComputeFunc,*/
    IComputeFunc<SurfaceElevationPatchArgument, byte[] /*ClientHeightAndTimeLeafSubGrid*/>
  {
    [NonSerialized]
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Local reference to the client subgrid factory
    /// </summary>
    [NonSerialized]
    private static IClientLeafSubgridFactory ClientLeafSubGridFactory = ClientLeafSubgridFactoryFactory.Factory();

    /// <summary>
    /// Invokes the surface elevation patch computation function on the server nodes the request has been sent to
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public byte[] Invoke(SurfaceElevationPatchArgument arg)
    {
      try
      {
        Log.LogDebug($"CalculateDesignElevationPatchComputeFunc: Arg = {arg}");

        CalculateSurfaceElevationPatch Executor = new CalculateSurfaceElevationPatch(arg);

        IClientLeafSubGrid result = Executor.Execute();

        if (result == null)
          return null;

        try
        {
          return result.ToBytes();
        }
        finally
        {
          ClientLeafSubGridFactory.ReturnClientSubGrid(ref result);
        }
      }
      catch (Exception E)
      {
        Log.LogInformation($"Exception: {E}");
      }

      return null;
    }
  }
}
