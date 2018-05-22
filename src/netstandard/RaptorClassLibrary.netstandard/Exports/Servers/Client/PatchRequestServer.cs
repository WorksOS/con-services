using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Requests;
using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.Servers;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.Rendering.Servers.Client
{
    /// <summary>
    /// The server used to house tile rendering services
    /// </summary>
    public class PatchRequestServer : ApplicationServiceServer
    {
        /// <summary>
        /// Default no-arg constructor that creates a server with the default Application Service role and the specialise tile rendering role.
        /// </summary>
        public PatchRequestServer() : base(new [] { ApplicationServiceServer.DEFAULT_ROLE, ServerRoles.PATCH_REQUEST_ROLE })
        {
        }

        public PatchRequestServer(string [] roles) : base(roles)
        {
        }

        /// <summary>
        /// Creates a new instance of a tile rendering server. 
        /// </summary>
        /// <returns></returns>
        public static PatchRequestServer NewInstance(string [] roles)
        {
            return new PatchRequestServer(roles);
        }

        /// <summary>
        /// Render a thematic tile bitmap according to the given arguments
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public PatchRequestResponse Execute(TileRenderRequestArgument argument)
        {
            Pat request = new TileRenderRequest();

            return request.Execute(argument);
        }
    }
}
