using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;

namespace VSS.TRex.Volumes.Servers.Client
{
    /// <summary>
    /// The server used to house simple volumes services
    /// </summary>
    public class SimpleVolumesServer : ApplicationServiceServer
    {
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SimpleVolumesServer(string [] roles) : base(roles)
        {
        }

        /// <summary>
        /// Default no-arg constructor that creates a server with the default Application Service role and the specialize volumes calculation role.
        /// </summary>
        public SimpleVolumesServer() : this(new[] { ServerRoles.TILE_RENDERING_NODE })
        {
        }

        /*
        /// <summary>
        /// Compute a simple volume according to the parameters in the argument.
        /// This request is sent to an application service node for coordinates of the compute requirements
        /// across the main cache compute cluster
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static SimpleVolumesResponse ComputeSimpleVolumes(SimpleVolumesRequestArgument argument)
        {
            SimpleVolumesRequest_ApplicationService request = new SimpleVolumesRequest_ApplicationService();

            return request.Execute(argument);
        }
        */
    }
}
