
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.Executors
{
  public class GetEditDataExecutor : RequestExecutorContainer
  {
 
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public GetEditDataExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GetEditDataExecutor()
    {
    }
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result = null;
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
           m.d.FMachineID, m.d.FStartDate.ToDateTime(), m.d.FEndDate.ToDateTime(), m.d.FName, (int)m.l.FLayerID)).ToList();

      var layerOnly =
        (from l in layers
         where !layerMatches.Contains(l)
         select ProductionDataEdit.CreateProductionDataEdit(
         l.FAssetID, l.FStartTime.ToDateTime(), l.FEndTime.ToDateTime(), null, (int)l.FLayerID)).ToList();

      var designOnly =
        (from d in designNames
         where !designMatches.Contains(d)
         select ProductionDataEdit.CreateProductionDataEdit(d.FMachineID, d.FStartDate.ToDateTime(), d.FEndDate.ToDateTime(), d.FName, null)).ToList();

      dataEdits.AddRange(layerOnly);
      dataEdits.AddRange(designOnly);
      return dataEdits;
    }




    protected override void ProcessErrorCodes()
    {
      RaptorResult.AddErrorMessages(ContractExecutionStates);
    }

  }
}