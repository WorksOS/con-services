using System.IO;
using TAGProcServiceDecls;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors
{
  /// <summary>
  /// For submitting direct submitted TAG files to Raptor.
  /// </summary>
  public class TagFileDirectSubmissionExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = item as TagFileRequestLegacy;

        var returnResult = tagProcessor.ProjectDataServerTAGProcessorClient()
                                       .SubmitTAGFileToTAGFileProcessor
                                       (request.FileName,
                                         new MemoryStream(request.Data),
                                         request.ProjectId ?? -1, 0, 0, request.MachineId ?? -1,
                                         request.Boundary != null
                                           ? RaptorConverters.convertWGS84Fence(request.Boundary)
                                           : TWGS84FenceContainer.Null(), request.TccOrgId);

        return TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(returnResult));
      }
      catch
      {
        return TagFileDirectSubmissionResult.Create(new TagFileProcessResultHelper(TTAGProcServerProcessResult.tpsprUnknown));
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }
  }
}
