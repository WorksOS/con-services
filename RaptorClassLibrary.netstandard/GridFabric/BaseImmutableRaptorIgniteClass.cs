using VSS.TRex.GridFabric.Grids;

namespace VSS.TRex.GridFabric
{
    public class BaseImmutableRaptorIgniteClass : BaseRaptorIgniteClass
    {
        /// <summary>
        /// Default no-arg constructor that sets up cluster and compute projections available for use
        /// </summary>
        public BaseImmutableRaptorIgniteClass(string role) : base(TRexGrids.ImmutableGridName(), role)
        {
        }
    }
}
