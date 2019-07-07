using System.IO;
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
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<QMTileRequest>(item);
//        var fileResult = trexCompactionDataProxy.SendDataPostRequestWithStreamResponse(request, "/qmtile", customHeaders).Result;
        var fileResult = trexCompactionDataProxy.SendDataPostRequestWithStreamResponse(request, "/terrain", customHeaders).Result;
        using (var ms = new MemoryStream())
        {
          fileResult.CopyTo(ms);
          return new QMTileResult(ms.ToArray());
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

  }
}
