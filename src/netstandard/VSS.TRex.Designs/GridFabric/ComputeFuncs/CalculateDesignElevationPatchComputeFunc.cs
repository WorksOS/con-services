using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.Designs.Executors;
using VSS.TRex.Designs.GridFabric.Arguments;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
    /// <summary>
    /// Ignite ComputeFunc responsible for executing the elevation patch calculator
    /// </summary>
    [Serializable]
    public class CalculateDesignElevationPatchComputeFunc : IComputeFunc<CalculateDesignElevationPatchArgument, byte [] /* ClientHeightLeafSubGrid */>
    {
        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    // TODO: Ignite 2.4 has a fix for the two dimensional array serialisation bug that requires this result to be a byte array - this can be changed back but...
    // Work out if our serialisation is preferred over Ignite's
    // The [Serializable] annotation means we are using the .Net serialisation. Which sucks bananas...
        public byte[] /*ClientHeightLeafSubGrid */Invoke(CalculateDesignElevationPatchArgument arg)
        {
            try
            {
                // Log.LogInformation($"CalculateDesignElevationPatchComputeFunc: Arg = {arg}");

                CalculateDesignElevationPatch Executor = new CalculateDesignElevationPatch();

                return Executor.Execute(arg).ToBytes();
            }
            catch (Exception E)
            {
                Log.LogError($"Exception: {E}");
                return null; 
            }
        }
    }
}
