namespace VSS.TRex.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The base class for compute functions. This provides common aspects such as the injected Ignite instance
    /// </summary>
    public class BaseComputeFunc : BaseIgniteClass
    {
        public BaseComputeFunc()
        {
        }

        /// <summary>
        /// Constructor accepting a role for the compute func that can identity a cluster group in the grid to perform the operation
        /// </summary>
        public BaseComputeFunc(string gridName, string role) : base(gridName, role)
        {
        }
    }
}
