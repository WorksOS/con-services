using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.DesignProfiling.Executors;
using VSS.TRex.DesignProfiling.GridFabric.Arguments;

namespace VSS.TRex.DesignProfiling.GridFabric.ComputeFuncs
{
    /// <summary>
    /// Ignite ComputeFunc responsible for executing the elevation patch calculator
    /// </summary>
    [Serializable]
    public class CalculateDesignElevationPatchComputeFunc : IComputeFunc<CalculateDesignElevationPatchArgument, byte [] /* ClientHeightLeafSubGrid */>
    {
        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        // TODO: Ignite 2.4 has a fix for the two dimensional arrya serialisation buf that requires this result to be a byte array - this can be changed back...
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
