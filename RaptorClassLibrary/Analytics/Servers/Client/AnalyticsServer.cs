using Apache.Ignite.Core.Compute;
using System.Drawing;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Requests;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Servers.Client;

namespace VSS.VisionLink.Raptor.Rendering.Servers.Client
{
    /// <summary>
    /// The server used to house tile rendering services
    /// </summary>
    public class AnalyticsServer<TRequest, TArgument, TResponse> : RaptorApplicationServiceServer where TRequest : class, IComputeFunc<TArgument, TResponse>, new()
    {
        /// <summary>
        /// Default no-arg constructor that creates a server with the default Application Service role and the specialise tile rendering role.
        /// </summary>
        public AnalyticsServer() : base(new string[] { RaptorApplicationServiceServer.DEFAULT_ROLE, ServerRoles.ANALYTICS_NODE })
        {
        }

        /// <summary>
        /// Creates a new instance of a tile rendering server. 
        /// </summary>
        /// <returns></returns>
        public static RaptorTileRenderingServer NewInstance()
        {
            return new RaptorTileRenderingServer();
        }

        /// <summary>
        /// Render a thematic tile bitmap according to the given arguments
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public TResponse RenderTile(TArgument argument)
        {
            TRequest request = new TRequest();

            return request.Execute(argument);
        }
    }
}
