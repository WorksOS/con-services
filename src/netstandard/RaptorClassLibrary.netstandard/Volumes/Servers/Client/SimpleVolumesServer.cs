using VSS.TRex.Servers.Client;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Rendering.Servers.Client
{
    /// <summary>
    /// The server used to house simpole volumes services
    /// </summary>
    public class SimpleVolumesServer : ApplicationServiceServer
    {
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SimpleVolumesServer()
        {
        }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SimpleVolumesServer(string [] roles) : base(roles)
        {
        }

        /// <summary>
        /// Creates a new instance of a simple volumes server. 
        /// </summary>
        /// <returns></returns>
        public static SimpleVolumesServer NewInstance(string [] roles)
        {
            return new SimpleVolumesServer(roles);
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
