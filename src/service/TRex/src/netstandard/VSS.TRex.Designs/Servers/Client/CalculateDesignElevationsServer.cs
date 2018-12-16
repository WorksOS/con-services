using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.Servers.Client;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Designs.Servers.Client
{
    public class CalculateDesignElevationsServer : ImmutableClientServer
    {
        public CalculateDesignElevationsServer() : base(ServerRoles.DESIGN_PROFILER)
        {
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
