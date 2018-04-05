using VSS.VisionLink.Raptor.GridFabric;

namespace VSS.VisionLink.Raptor.Services
{
    /// <summary>
    /// The base class for services. This provides common aspects such as the injected Ignite instance
    /// </summary>
    public class BaseRaptorService : BaseRaptorIgniteClass
    {
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public BaseRaptorService(string gridName, string role) : base(gridName, role)
        {

        }
    }
}
