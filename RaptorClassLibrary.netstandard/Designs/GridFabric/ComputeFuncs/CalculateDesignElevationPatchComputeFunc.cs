using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Reflection;
using VSS.Velociraptor.DesignProfiling.Executors;
using VSS.Velociraptor.DesignProfiling.GridFabric.Arguments;

namespace VSS.Velociraptor.DesignProfiling.GridFabric.ComputeFuncs
{
    /// <summary>
    /// Ignite ComputeFunc responsible for executing the elevation patch calculator
    /// </summary>
    [Serializable]
    public class CalculateDesignElevationPatchComputeFunc : IComputeFunc<CalculateDesignElevationPatchArgument, byte [] /* ClientHeightLeafSubGrid */>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // TODO: Ignite 2.4 has a fix for the two dimensional arrya serialisation buf that requires this result to be a byte array - this can be changed back...
        public byte[] /*ClientHeightLeafSubGrid */Invoke(CalculateDesignElevationPatchArgument arg)
        {
            try
            {
                // Log.Info($"CalculateDesignElevationPatchComputeFunc: Arg = {arg}");

                CalculateDesignElevationPatch Executor = new CalculateDesignElevationPatch();

                return Executor.Execute(arg).ToBytes();
            }
            catch (Exception E)
            {
                Log.Error($"Exception: {E}");
                return null; 
            }
        }
    }
}
