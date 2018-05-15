using VSS.TRex.GridFabric.Grids;

namespace VSS.TRex.GridFabric
{
    public class BaseMutableRaptorIgniteClass : BaseRaptorIgniteClass
    {
        /// <summary>
        /// Default no-arg constructor that sets up cluster and compute projections available for use
        /// </summary>
        public BaseMutableRaptorIgniteClass(string role) : base(TRexGrids.MutableGridName(), role)
        {
        }
    }
}
