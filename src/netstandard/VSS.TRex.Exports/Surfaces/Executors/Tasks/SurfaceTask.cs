using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Exports.Surfaces.Executors.Tasks
{
  /// <summary>
  /// The task responsible for receiving subgrids to be processed into a TIN surface
  /// </summary>
  public class SurfaceTask : PipelinedSubGridTask
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// The collection of subgrids being collected for a patch response
    /// </summary>
    public List<GenericLeafSubGrid<float>> SurfaceSubgrids = new List<GenericLeafSubGrid<float>>();

    /// <summary>
    /// Constructs the patch task
    /// </summary>
    /// <param name="requestDescriptor"></param>
    /// <param name="tRexNodeId"></param>
    /// <param name="gridDataType"></param>
    public SurfaceTask(Guid requestDescriptor, string tRexNodeId, GridDataType gridDataType) : base(requestDescriptor, tRexNodeId, gridDataType)
    {
    }

    /// <summary>
    /// Accept a subgrid response from the processing engine and incorporate into the result for the request.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public override bool TransferResponse(object response)
    {
      // Log.InfoFormat("Received a SubGrid to be processed: {0}", (response as IClientLeafSubGrid).Moniker());

      if (!base.TransferResponse(response))
      {
        Log.LogWarning($"Base {nameof(TransferResponse)} returned false");
        return false;
      }

      // Convert the ClientHeightLeafSubgrid into a GenericLeafSubGrid<float>...

      ClientHeightLeafSubGrid originSubGrid = (ClientHeightLeafSubGrid) (response as IClientLeafSubGrid[])[0];

      GenericLeafSubGrid<float> leaf = new GenericLeafSubGrid<float>
      {
        OriginX = originSubGrid.OriginX,
        OriginY = originSubGrid.OriginY,
        Items = originSubGrid.Clone2DArray(),
        Level = originSubGrid.Level
      };

      SurfaceSubgrids.Add(leaf);

      return true;
    }
  }
}
