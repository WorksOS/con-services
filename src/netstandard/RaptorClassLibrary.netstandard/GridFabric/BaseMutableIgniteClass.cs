using VSS.TRex.GridFabric.Grids;

namespace VSS.TRex.GridFabric
{
    public class BaseMutableIgniteClass : BaseIgniteClass
    {
        /// <summary>
        /// Default no-arg constructor that sets up cluster and compute projections available for use
        /// </summary>
        public BaseMutableIgniteClass(string role) : base(TRexGrids.MutableGridName(), role)
        {
        }
    }
}
