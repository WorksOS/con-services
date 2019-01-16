using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class UpdateSVLDesignExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// TagFileExecutor
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
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
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as DesignRequest;
      if (request == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 38);
        return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "shouldn't get here"); // to keep compiler happy
      }

      try
      {
        log.LogInformation($"#In# UpdateSVLDesignExecutor. Update design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");

        if (!DIContext.Obtain<IAlignmentManager>().Remove(request.ProjectUid, request.DesignUid))
        {
          return new ContractExecutionResult((int)RequestErrorStatus.DesignImportUnableToDeleteDesign, RequestErrorStatus.DesignImportUnableToDeleteDesign.ToString());
        }

        // load core file from s3 to local
        var localPath = DesignHelper.EstablishLocalDesignFilepath(request.ProjectUid.ToString());
        var localPathAndFileName = Path.Combine(new[] { localPath, request.FileName });

        AlignmentDesign alignmentDesign = new AlignmentDesign(SubGridTreeConsts.DefaultCellSize);
        var designLoadResult = alignmentDesign.LoadFromStorage(request.ProjectUid, request.FileName, localPath, false);
        if (designLoadResult != DesignLoadResult.Success)
        {
          log.LogError($"#Out# UpdateSVLDesignExecutor. Addition of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, designLoadResult: {designLoadResult.ToString()}");
          return new ContractExecutionResult((int)RequestErrorStatus.DesignImportUnableToRetrieveFromS3, designLoadResult.ToString());
        }

        // todo when SDK avail
        BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
        alignmentDesign.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
        alignmentDesign.GetHeightRange(out extents.MinZ, out extents.MaxZ);


        // Create the new alignment in our site model
        var designAlignment = DIContext.Obtain<IAlignmentManager>()
          .Add(request.ProjectUid,
            new Designs.Models.DesignDescriptor(request.DesignUid, localPathAndFileName, request.FileName, 0),
            extents);

        // todo when SDK avail
        /* var existanceMaps = DIContext.Obtain<IExistenceMaps>();
          existanceMaps.SetExistenceMap(request.DesignUid, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, designAlignment.ID, alignmentDesign.SubgridOverlayIndex());
          */

        log.LogInformation($"#Out# UpdateSVLDesignExecutor. Processed add design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");
      }
      catch (Exception e)
      {
        log.LogError(e, $"#Out# UpdateSVLDesignExecutor. Update of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, Exception:");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, (int)RequestErrorStatus.DesignImportUnableToUpdateDesign, e.Message);
      }

      return new ContractExecutionResult();
    }


    /// <summary>
    /// Processes the request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
