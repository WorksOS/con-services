using System.IO;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.Common.Executors
{
  public class QMTilesExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public QMTilesExecutor()
    {
      ProcessErrorCodes();
    }

    /// <summary>
    /// Processes the request for type T.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<QMTileRequest>(item);
        // Send request to TRex webapi endpoint
        var fileResult = await trexCompactionDataProxy.SendDataPostRequestWithStreamResponse(request, "/terrain", customHeaders);
        if (fileResult == null)
        {
          // No tile produced is valid. Any unexpected errors should be logged earlier
          return null;
        }
        else
        {
          using (var ms = new MemoryStream())
          {
            fileResult.CopyTo(ms); // QM tile
            return new QMTileResult(ms.ToArray());
          }
        }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    protected sealed override void ProcessErrorCodes()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new System.NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
