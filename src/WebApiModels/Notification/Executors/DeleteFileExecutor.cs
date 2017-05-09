
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Notification.Executors
{
  /// <summary>
  /// Processes the request to delete a file.
  /// Action taken depends on the file type.
  /// </summary>
  public class DeleteFileExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public DeleteFileExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DeleteFileExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        FileDescriptor request = item as FileDescriptor;
        string ext = Path.GetExtension(request.fileName).ToUpper();
        //Only alignment files at present
        if (ext != ".SVL")
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
                "Unsupported file type"));
        }
        //Delete generated files
        //DXF (Design surface, Alignment), PRJ (DXF, DEsign, Alignment), GM_XFORM (DXF, DEsign, Alignment)

        //Delete tiles (DXF, Alignment)

        //If surveyed surface, DiscardGroundSurfaceDetails(Raptor)

        return null;

      }
      finally
      {
      }

    }
  }
}
