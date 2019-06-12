
using System;
using System.Collections.Generic;
using System.Linq;
#if RAPTOR
using SVOICFiltersDecls;
using SVOICGridCell;
using SVOICProfileCell;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors.CellPass
{
  public abstract class CellPassesBaseExecutor<TRes> : RequestExecutorContainer 
    where TRes : ContractExecutionResult
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
#if RAPTOR
      var request = CastRequestObjectTo<CellPassesRequest>(item);

      bool isGridCoord = request.probePositionGrid != null;
      bool isLatLgCoord = request.probePositionLL != null;
      double probeX = isGridCoord ? request.probePositionGrid.x : (isLatLgCoord ? request.probePositionLL.Lon : 0);
      double probeY = isGridCoord ? request.probePositionGrid.y : (isLatLgCoord ? request.probePositionLL.Lat : 0);

      var raptorFilter = RaptorConverters.ConvertFilter(request.filter, request.ProjectId, raptorClient, overrideAssetIds: new List<long>());

      int code = raptorClient.RequestCellProfile
      (request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
        RaptorConverters.convertCellAddress(request.cellAddress ?? new CellAddress()),
        probeX, probeY,
        isGridCoord,
        RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
        request.gridDataType,
        raptorFilter,
        out var profile);


      if (code == 1)//TICServerRequestResult.icsrrNoError
        return ConvertResult(profile);

      throw CreateServiceException<CellPassesExecutor>();
#else
      throw new NotImplementedException();
#endif
    }

#if RAPTOR 
    protected abstract TRes ConvertResult(TICProfileCell profile);
#endif
    
  }
}
