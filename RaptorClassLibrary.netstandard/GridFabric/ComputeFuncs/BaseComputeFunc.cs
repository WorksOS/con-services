namespace VSS.TRex.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The base class for compute funcs. This provides common aspects such as the injected Ignite instance
    /// </summary>
    public class BaseComputeFunc : BaseRaptorIgniteClass
    {
        public BaseComputeFunc()
        {
//            Debug.Assert(false, "BaseComputeFunc() may not be invoked");
        }

        /// <summary>
        /// Constructor accepting a role for the compute func that can identity a cluster group in the grid to perform the operation
        /// </summary>
        public BaseComputeFunc(string gridName, string role) : base(gridName, role)
        {
        }
    }
}
