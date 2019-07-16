using System.Linq;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.QuantizedMesh.Executors.Tasks
{

  /// <summary>
  /// A Task specialized towards rendering quantized mesh tiles
  /// </summary>
  public class QuantizedMeshTask : PipelinedSubGridTask, IQuantizedMeshTask
  {
     private static readonly ILogger Log = Logging.Logger.CreateLogger<QuantizedMeshTask>();
    /// <summary>
    /// The tile renderer responsible for processing sub grid information into tile based thematic rendering
    /// </summary>
    /// 
    // todo   public PlanViewTileRenderer TileRenderer { get; set; }

    public QuantizedMeshTask()
    { }

    public override bool TransferResponse(object response)
    {
      // Log.InfoFormat("Received a SubGrid to be processed: {0}", (response as IClientLeafSubGrid).Moniker());
      bool result = false;

      if (base.TransferResponse(response))
      {
        // todo
       // if (!(response is IClientLeafSubGrid[] subGridResponses) || subGridResponses.Length == 0)
          Log.LogWarning("No sub grid responses returned");
       // else
        ///  result = subGridResponses.Where(x => x != null).All(TileRenderer.Displayer.RenderSubGrid);
      }

      return result;
    }
  }


}
