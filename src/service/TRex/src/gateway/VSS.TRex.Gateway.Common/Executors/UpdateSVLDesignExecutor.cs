using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.TRex.Alignments.GridFabric.Arguments;
using VSS.TRex.Alignments.GridFabric.Requests;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class UpdateSVLDesignExecutor : BaseExecutor
  {
    /// <summary>
    /// UpdateSVLDesignExecutor
    /// </summary>
    public UpdateSVLDesignExecutor(IConfigurationStore configStore,
      ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public UpdateSVLDesignExecutor()
    {
    }

    /// <summary>
    /// Process update design request
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<DesignRequest>(item);

      try
      {
        log.LogInformation($"#In# UpdateSVLDesignExecutor. Update design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");


        // Remove the alignment
        var tRexRemoveRequest = new RemoveAlignmentRequest();
        var removeResponse = await tRexRemoveRequest.ExecuteAsync(new RemoveAlignmentArgument
        {
          ProjectID = request.ProjectUid,
          AlignmentID = request.DesignUid
        });

        if (removeResponse.RequestResult != DesignProfilerRequestResult.OK)
        {
          throw CreateServiceException<UpdateSVLDesignExecutor>
            (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
              RequestErrorStatus.DesignImportUnableToDeleteDesign);
        }

        // load core file from s3 to local
        var localPath = FilePathHelper.GetTempFolderForProject(request.ProjectUid);
        var localPathAndFileName = Path.Combine(new[] {localPath, request.FileName});

        var alignmentDesign = new SVLAlignmentDesign();
        var designLoadResult = await alignmentDesign.LoadFromStorage(request.ProjectUid, request.FileName, localPath);
        if (designLoadResult != DesignLoadResult.Success)
        {
          log.LogError($"#Out# UpdateSVLDesignExecutor. Addition of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, designLoadResult: {designLoadResult.ToString()}");
          throw CreateServiceException<UpdateSVLDesignExecutor>
            (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
              RequestErrorStatus.DesignImportUnableToRetrieveFromS3, designLoadResult.ToString());
        }

        var extents = new BoundingWorldExtent3D();
        alignmentDesign.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
        alignmentDesign.GetHeightRange(out extents.MinZ, out extents.MaxZ);

        // Create the new alignment in our site model
        var tRexAddRequest = new AddAlignmentRequest();
        var alignmentUid = await tRexAddRequest.ExecuteAsync(new AddAlignmentArgument
        {
          ProjectID = request.ProjectUid,
          DesignDescriptor = new Designs.Models.DesignDescriptor(request.DesignUid, localPathAndFileName, request.FileName),
          Extents = extents
        });

        log.LogInformation($"#Out# UpdateSVLDesignExecutor. Processed add design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");
      }
      catch (Exception e)
      {
        log.LogError(e, $"#Out# UpdateSVLDesignExecutor. Update of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, Exception:");
        throw CreateServiceException<UpdateSVLDesignExecutor>
          (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
            RequestErrorStatus.DesignImportUnableToUpdateDesign, e.Message);
      }

      return new ContractExecutionResult();
    }

    /// <summary>
    /// Processes the request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new TRexException("Use the asynchronous form of this method");
    }
  }
}
