using Apache.Ignite.Core.Compute;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.GridFabric.Requests.Interfaces;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Servers.Client;

namespace VSS.VisionLink.Raptor.Rendering.Servers.Client
{
    public class AnalyticsRequest<TRequest, TArgument, TResponse>
        where TRequest : class, IComputeFunc<TArgument, TResponse>, new()
        where TResponse : class, IResponseAggregateWith<TResponse>, new()
    {
        public GenericPSNodeBroadcastRequest<TArgument, TRequest, TResponse> Request_ClusterCompute = new GenericPSNodeBroadcastRequest<TArgument, TRequest, TResponse>();
        public GenericASNodeRequest<TArgument, TRequest, TResponse> Request_ApplicationService = new GenericASNodeRequest<TArgument, TRequest, TResponse>();
    }

    /// <summary>
    /// The server used to house analytics request services
    /// </summary>
    public class AnalyticsServer<TRequest, TArgument, TResponse> : RaptorApplicationServiceServer 
        where TRequest : class, IComputeFunc<TArgument, TResponse>, new()
        where TResponse : class, IResponseAggregateWith<TResponse>, new()
    {
        private AnalyticsRequest<TRequest, TArgument, TResponse> analyticsRequest = new AnalyticsRequest<TRequest, TArgument, TResponse>();

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
        public static AnalyticsServer<TRequest, TArgument, TResponse> NewInstance()
        {
            return new AnalyticsServer<TRequest, TArgument, TResponse>();
        }

        /// <summary>
        /// Executes the request
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public TResponse Execute(TArgument argument)
        {
            GenericASNodeRequest<TArgument, TRequest, TResponse> request = new GenericASNodeRequest<TArgument, TRequest, TResponse>();
            return request.Execute(argument);

            // GenericPSNodeBroadcastRequest<TArgument, TRequest, TResponse> request = new GenericPSNodeBroadcastRequest<TArgument, TRequest, TResponse>();
            //return analyticsRequest.Request_ApplicationService.Execute(argument);
        }
    }
}
