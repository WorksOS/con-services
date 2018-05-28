using VSS.TRex.DesignProfiling.GridFabric.Arguments;
using VSS.TRex.DesignProfiling.GridFabric.Requests;
using VSS.TRex.Servers.Client;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.DesignProfiling.Servers.Client
{
    public class CalculateDesignElevationsServer : ImmutableClientServer
    {
        public CalculateDesignElevationsServer() : base("DesignProfiler")
        {
        }

        /// <summary>
        /// Creates a new instance of a design elevation server. 
        /// </summary>
        /// <returns></returns>
        public static CalculateDesignElevationsServer NewInstance()
        {
            return new CalculateDesignElevationsServer();
        }

        /// <summary>
        /// Compute a design elevation patch
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public ClientHeightLeafSubGrid ComputeDesignElevations(CalculateDesignElevationPatchArgument argument)
        {
            DesignElevationPatchRequest request = new DesignElevationPatchRequest();

            return request.Execute(argument);
        }
    }
}
