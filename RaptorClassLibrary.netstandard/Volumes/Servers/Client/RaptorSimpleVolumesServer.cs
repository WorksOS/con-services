using VSS.VisionLink.Raptor.Servers.Client;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Requests;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.Rendering.Servers.Client
{
    /// <summary>
    /// The server used to house simpole volumes services
    /// </summary>
    public class RaptorSimpleVolumesServer : RaptorApplicationServiceServer
    {
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public RaptorSimpleVolumesServer()
        {
        }

        /// <summary>
        /// Creates a new instance of a simple volumes server. 
        /// </summary>
        /// <returns></returns>
        public static RaptorSimpleVolumesServer NewInstance()
        {
            return new RaptorSimpleVolumesServer();
        }

        /// <summary>
        /// Compute a simple volume according to the parameters in the argument.
        /// This request is sent to an application service node for coordinates of the compute requirements
        /// across the main cache compute cluster
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public SimpleVolumesResponse ComputeSimpleVolues(SimpleVolumesRequestArgument argument)
        {
            SimpleVolumesRequest_ApplicationService request = new SimpleVolumesRequest_ApplicationService();

            return request.Execute(argument);
        }
    }
}
