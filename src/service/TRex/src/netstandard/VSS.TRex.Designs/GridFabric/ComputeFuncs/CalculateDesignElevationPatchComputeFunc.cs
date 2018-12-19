using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using VSS.TRex.Designs.Executors;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
    /// <summary>
    /// Ignite ComputeFunc responsible for executing the elevation patch calculator
    /// </summary>
    public class CalculateDesignElevationPatchComputeFunc : BaseComputeFunc, IComputeFunc<CalculateDesignElevationPatchArgument, byte []>
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignElevationPatchComputeFunc>();

        public byte[] Invoke(CalculateDesignElevationPatchArgument args)
        {
            try
            {
                // Log.LogInformation($"CalculateDesignElevationPatchComputeFunc: Arg = {arg}");

                CalculateDesignElevationPatch Executor = new CalculateDesignElevationPatch();

                return Executor.Execute(args.ProjectID, args.ReferenceDesignUID, args.CellSize, args.OriginX, args.OriginY, 0).ToBytes();
            }
            catch (Exception E)
            {
                Log.LogError(E, "Exception:");
                return null; 
            }
        }
    }
}
