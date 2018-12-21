using System;
using VSS.TRex.GridFabric.Servers.Client;

namespace VSS.TRex.Servers.Client
{
    /// <summary>
    /// An application service server that can take a generic action to perform in the context of an application service service
    /// </summary>
    public class GenericApplicationServiceServer : ApplicationServiceServer
    {
        /// <summary>
        /// Static intance of the TRexApplicationServicesServer
        /// </summary>
        private static GenericApplicationServiceServer _instance;

        /// <summary>
        /// Obtains, or creates, the static singleton intances for the TRexApplicationServicesServer
        /// </summary>
        /// <returns></returns>
        public static GenericApplicationServiceServer  Instance() => _instance == null ? _instance : (_instance = new GenericApplicationServiceServer());

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public GenericApplicationServiceServer()
        {
        }

        /// <summary>
        /// Generic implenentation of a function lambda executed in the contect of the TRexApplicationServicesServer
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static TResult PerformAction<TResult>(Func<TResult> func)
        {
            Instance();
            return func();
        }
    }
}
