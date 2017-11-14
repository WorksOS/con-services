using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Velociraptor.DesignProfiling.GridFabric.Arguments;
using VSS.Velociraptor.DesignProfiling.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.SubGridTrees.Client;

namespace VSS.Velociraptor.DesignProfiling.GridFabric.Requests
{
    public class DesignElevationPatchRequest : BaseRaptorRequest
    {
        public ClientHeightLeafSubGrid Execute(CalculateDesignElevationPatchArgument arg)
        {
            // Construct the function to be used
            IComputeFunc<CalculateDesignElevationPatchArgument, byte[] /*ClientHeightLeafSubGrid*/> func = new CalculateDesignElevationPatchComputeFunc();

            // Get a reference to the compute cluster group and send the request to it for processing
            // Note: Broadcast will block until all compute nodes receiving the request have responded, or
            // until the internal Ignite timeout expires

            IClusterGroup group = _ignite.GetCluster().ForRemotes().ForAttribute("Role", "DesignProfiler");
            ICompute compute = group.GetCompute();

            /*ClientHeightLeafSubGrid */ byte[] result = compute.Apply(func, arg);

            if (result == null)
            {
                return null;
            }

            ClientHeightLeafSubGrid clientResult = new ClientHeightLeafSubGrid(null, null, SubGridTree.SubGridTreeLevels, SubGridTree.DefaultCellSize, SubGridTree.DefaultIndexOriginOffset);
            clientResult.FromByteArray(result);
            return clientResult;

//            Task<ClientHeightLeafSubGrid> taskResult = compute.ApplyAsync(func, arg);

            // Send the appropriate response to the caller
//            return taskResult.Result;
        }
    }
}
