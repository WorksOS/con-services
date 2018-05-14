using System;

namespace VSS.VisionLink.Raptor.Servers.Client
{
    /// <summary>
    /// An application service server that can take a generic action to perform in the context of an application service service
    /// </summary>
    public class GenericApplicationServiceServer : ApplicationServiceServer
    {
        /// <summary>
        /// Static intance of the RaptorApplicationServicesServer
        /// </summary>
        private static GenericApplicationServiceServer _instance;

        /// <summary>
        /// Obtains, or creates, the static singleton intances for the RaptorApplicationServicesServer
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
        /// Generic implenentation of a function lambda executed in the contect of the RaptorApplicationServicesServer
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
