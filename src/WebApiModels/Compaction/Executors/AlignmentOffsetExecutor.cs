using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  public class AlignmentOffsetExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as AlignmentOffsetRequest;
      var offsets = aligmentTileService.GetAlignmentOffsets(request.projectId.Value, request.fileDescriptor);
      if (offsets.startOffset.HasValue && offsets.endOffset.HasValue)
         return AlignmentOffsetResult.CreateAlignmentOffsetResult(offsets.startOffset.Value,offsets.endOffset.Value);
      exceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
        ContractExecutionStatesEnum.FailedToGetResults, "Null results for offsets - incorrect file?");
      return null;
    }
  }
}
