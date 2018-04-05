using VSS.VisionLink.Raptor.GridFabric.Grids;

namespace VSS.VisionLink.Raptor.GridFabric
{
    public class BaseImmutableRaptorIgniteClass : BaseRaptorIgniteClass
    {
        /// <summary>
        /// Default no-arg constructor that sets up cluster and compute projections available for use
        /// </summary>
        public BaseImmutableRaptorIgniteClass(string role) : base(RaptorGrids.RaptorImmutableGridName(), role)
        {
        }
    }
}
