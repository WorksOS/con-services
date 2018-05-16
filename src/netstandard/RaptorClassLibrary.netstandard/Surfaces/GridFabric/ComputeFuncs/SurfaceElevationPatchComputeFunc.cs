using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Reflection;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Surfaces.Executors;
using VSS.TRex.Surfaces.GridFabric.Arguments;

namespace VSS.TRex.Surfaces.GridFabric.ComputeFuncs
{
    [Serializable]
    public class SurfaceElevationPatchComputeFunc : /*BaseComputeFunc,*/ IComputeFunc<SurfaceElevationPatchArgument, byte[] /*ClientHeightAndTimeLeafSubGrid*/>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Local reference to the client subgrid factory
        /// </summary>
        [NonSerialized]
        private static IClientLeafSubgridFactory ClientLeafSubGridFactory = ClientLeafSubgridFactoryFactory.GetClientLeafSubGridFactory();

        /// <summary>
        /// Invokes the surface elevation patch computation function on the server nodes the request has been sent to
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public byte[] Invoke(SurfaceElevationPatchArgument arg)
        {
            try
            {
                Log.Debug($"CalculateDesignElevationPatchComputeFunc: Arg = {arg}");

                CalculateSurfaceElevationPatch Executor = new CalculateSurfaceElevationPatch(arg);

                /*ClientHeightAndTimeLeafSubGrid*/ IClientLeafSubGrid result = Executor.Execute();

                if (result != null)
                {
                    try
                    {
                        return (result as ClientHeightAndTimeLeafSubGrid).ToBytes();
                    }
                    finally
                    {
                        ClientLeafSubGridFactory.ReturnClientSubGrid(ref result);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception E)
            {
                Log.Info($"Exception: {E}");
                return null; // Todo .....
            }
        }
    }
}
