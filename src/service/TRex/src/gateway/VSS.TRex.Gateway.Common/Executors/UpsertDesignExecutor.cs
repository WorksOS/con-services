using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using Consts = VSS.TRex.ExistenceMaps.Interfaces.Consts;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class UpsertDesignExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// TagFileExecutor
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
    public UpsertDesignExecutor(IConfigurationStore configStore,
        ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public UpsertDesignExecutor()
    {
    }

    /// <summary>
    /// Process add design request
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
      }

      ContractExecutionResult result = new ContractExecutionResult();

      try
      {
        log.LogInformation($"#In# UpsertDesignExecutor. Add design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");

        var localPath = Path.Combine(new[] {TRexServerConfig.PersistentCacheStoreLocation, request.ProjectUid.ToString()});
        var localPathAndFileName = Path.Combine(new[] { localPath, request.FileName});
        TTMDesign TTM = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
        var designLoadResult = TTM.LoadFromStorage(request.ProjectUid, request.FileName, localPath, false);
        if (designLoadResult != DesignLoadResult.Success)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 99 /* todojeannie */);
        }
        TTM.LoadFromFile(localPathAndFileName);

        BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
        TTM.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
        TTM.GetHeightRange(out extents.MinZ, out extents.MaxZ);

        // Create the new design for the site model
        var design = DIContext.Obtain<IDesignManager>().Add(request.ProjectUid,
          new VSS.TRex.Designs.Models.DesignDescriptor(request.DesignUid, localPathAndFileName, request.FileName, 0),
          extents);

        DIContext.Obtain<IExistenceMaps>().SetExistenceMap(request.DesignUid, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, design.ID, TTM.SubgridOverlayIndex());

        // todojeannie should these go in designmanager.add?
        S3FileTransfer.WriteFile(TRexServerConfig.PersistentCacheStoreLocation, request.ProjectUid, request.FileName + VSS.TRex.Designs.TTM.Optimised.Consts.kDesignSubgridIndexFileExt);
        S3FileTransfer.WriteFile(TRexServerConfig.PersistentCacheStoreLocation, request.ProjectUid, request.FileName + VSS.TRex.Designs.TTM.Optimised.Consts.kDesignSpatialIndexFileExt);
      }
      catch (Exception e)
      {
        result = new ContractExecutionResult(/* todojeannie */ 9999, "Unable to Add Design");
      }
      finally
      {
        log.LogInformation($"#Out# UpsertDesignExecutor. Process add design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, Result Code: {result.Code}, Message:{result.Message}");
      }

      return result;
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
