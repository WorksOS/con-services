using System.Collections.Generic;
using System.Linq;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  public class GetEditDataExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GetEditDataExecutor()
    {
      ProcessErrorCodes();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
        try
        {
          TDesignName[] designNames;
          TDesignLayer[] layers;

          GetEditDataRequest request = item as GetEditDataRequest;
          if (request.assetId == null) 
          {
            designNames = raptorClient.GetOverriddenDesigns(request.projectId ?? -1, -1);
            layers = raptorClient.GetOverriddenLayers(request.projectId ?? -1, -1);
            
          }
          else
          {
            designNames = raptorClient.GetOverriddenDesigns(request.projectId ?? -1,
                request.assetId <= 0 ? -1 : (long)request.assetId);
            layers = raptorClient.GetOverriddenLayers(request.projectId ?? -1,
                request.assetId <= 0 ? -1 : (long)request.assetId);
          }
          result = EditDataResult.CreateEditDataResult(ConvertDataEdits(designNames, layers));
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
      return result;
    }

    private List<ProductionDataEdit> ConvertDataEdits(TDesignName[] designNames, TDesignLayer[] layers)
    {
      //Check for any layer edits that go with design edits
      var matches =
           (from l in layers
           join d in designNames
               on new {assetId = l.FAssetID, startUTC = l.FStartTime, endUTC = l.FEndTime}
               equals new {assetId = d.FMachineID, startUTC = d.FStartDate, endUTC = d.FEndDate}
           select new {l, d}).ToList();

      var layerMatches = (from m in matches select m.l).ToList();
      var designMatches = (from m in matches select m.d).ToList();

      //TODO: layer number from Raptor should be an int not a long. Needs fixing in Shims.
      List<ProductionDataEdit> dataEdits =
        (from m in matches select ProductionDataEdit.CreateProductionDataEdit(
           m.d.FMachineID, m.d.FStartDate.ToDateTime(), m.d.FEndDate.ToDateTime(), m.d.FName, m.l.FLayerID)).ToList();

      var layerOnly =
        (from l in layers
         where !layerMatches.Contains(l)
         select ProductionDataEdit.CreateProductionDataEdit(
         l.FAssetID, l.FStartTime.ToDateTime(), l.FEndTime.ToDateTime(), null, l.FLayerID)).ToList();

      var designOnly =
        (from d in designNames
         where !designMatches.Contains(d)
         select ProductionDataEdit.CreateProductionDataEdit(d.FMachineID, d.FStartDate.ToDateTime(), d.FEndDate.ToDateTime(), d.FName, null)).ToList();

      dataEdits.AddRange(layerOnly);
      dataEdits.AddRange(designOnly);
      return dataEdits;
    }
    
    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }
  }
}