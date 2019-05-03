using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using VSS.TRex.Designs.Executors;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.GridFabric.ComputeFuncs;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
    /// <summary>
    /// Ignite ComputeFunc responsible for executing the elevation patch calculator
    /// </summary>
    public class CalculateDesignElevationPatchComputeFunc : BaseComputeFunc, IComputeFunc<CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<CalculateDesignElevationPatchComputeFunc>();

        public CalculateDesignElevationPatchResponse Invoke(CalculateDesignElevationPatchArgument args)
        {
            try
            {
                // Log.LogInformation($"CalculateDesignElevationPatchComputeFunc: Arg = {arg}");

                CalculateDesignElevationPatch Executor = new CalculateDesignElevationPatch();

                var heightsResult = Executor.Execute(args.ProjectID, args.ReferenceDesign, args.CellSize, args.OriginX, args.OriginY, out var calcResult);

                return new CalculateDesignElevationPatchResponse
                {
                    CalcResult = calcResult,
                    Heights = heightsResult
                };
            }
            catch (Exception E)
            {
                Log.LogError(E, "Exception:");
                return null; 
            }
        }
    }
}
