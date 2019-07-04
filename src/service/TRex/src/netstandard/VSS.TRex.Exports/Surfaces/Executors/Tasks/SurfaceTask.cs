using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core;

namespace VSS.TRex.Exports.Surfaces.Executors.Tasks
{
  /// <summary>
  /// The task responsible for receiving sub grids to be processed into a TIN surface
  /// </summary>
  public class SurfaceTask : PipelinedSubGridTask
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SurfaceTask>();

    /// <summary>
    /// The collection of sub grids being collected for a patch response
    /// </summary>
    public List<GenericLeafSubGrid_Float> SurfaceSubgrids = new List<GenericLeafSubGrid_Float>();

    public SurfaceTask()
    { }

    /// <summary>
    /// Accept a sub grid response from the processing engine and incorporate into the result for the request.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public override bool TransferResponse(object response)
    {
      // Log.InfoFormat("Received a SubGrid to be processed: {0}", (response as IClientLeafSubGrid).Moniker());
      bool result = false;

      if (base.TransferResponse(response))
      {
        if (!(response is IClientLeafSubGrid[] subGridResponses) || subGridResponses.Length == 0)
        {
          Log.LogWarning("No sub grid responses returned");
        }
        else
        {
          // Convert the ClientHeightLeafSubGrid into a GenericLeafSubGrid_Float...
          foreach (var subGrid in subGridResponses)
          {
            if (subGrid != null)
            {
              ClientHeightLeafSubGrid originSubGrid = (ClientHeightLeafSubGrid) subGrid;

              GenericLeafSubGrid_Float leaf = new GenericLeafSubGrid_Float
              {
                OriginX = originSubGrid.OriginX,
                OriginY = originSubGrid.OriginY,
                Items = originSubGrid.Clone2DArray(),
                Level = originSubGrid.Level
              };

              SurfaceSubgrids.Add(leaf);
            }
          }

          result = true;
        }
      }

      return result;
    }
  }
}
