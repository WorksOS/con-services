using Apache.Ignite.Core.Compute;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Designs.GridFabric.Requests
{
    public class DesignElevationPatchRequest : DesignProfilerRequest<CalculateDesignElevationPatchArgument, ClientHeightLeafSubGrid>
    {
        public override ClientHeightLeafSubGrid Execute(CalculateDesignElevationPatchArgument arg)
        {
            // Construct the function to be used
            IComputeFunc<CalculateDesignElevationPatchArgument, byte[] /*ClientHeightLeafSubGrid*/> func = new CalculateDesignElevationPatchComputeFunc();

            /*ClientHeightLeafSubGrid */ byte[] result = _Compute.Apply(func, arg);

            if (result == null)
            {
                return null;
            }

            ClientHeightLeafSubGrid clientResult = new ClientHeightLeafSubGrid(null, null, SubGridTree.SubGridTreeLevels, SubGridTree.DefaultCellSize, SubGridTree.DefaultIndexOriginOffset);
            clientResult.FromBytes(result);
            return clientResult;

//            Task<ClientHeightLeafSubGrid> taskResult = compute.ApplyAsync(func, arg);

            // Send the appropriate response to the caller
//            return taskResult.Result;
        }
    }
}
