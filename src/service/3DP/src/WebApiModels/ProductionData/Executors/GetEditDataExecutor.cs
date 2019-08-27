using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if RAPTOR
using VLPDDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class GetEditDataExecutor : BaseEditDataExecutor
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GetEditDataExecutor()
    {
#if RAPTOR
      ProcessErrorCodes();
#endif
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<GetEditDataRequest>(item);
#if RAPTOR
        if (UseTRexGateway("ENABLE_TREX_GATEWAY_EDIT_DATA"))
        {
#endif
          var projectIds = new ProjectIDs(request.ProjectId.Value, request.ProjectUid.Value);
          projectIds.Validate();
          var assetUid = await GetAssetUid(projectIds, request.assetId);
          var parameters = new Dictionary<string, string>
          {
            { "projectUid", request.ProjectUid.Value.ToString() }
          };
          if (assetUid.HasValue)
          {
            parameters.Add("assetUid", assetUid.ToString());
          }
          var queryParams = $"?{new System.Net.Http.FormUrlEncodedContent(parameters).ReadAsStringAsync().Result}";
          var results = await trexCompactionDataProxy.SendDataGetRequest<TRexEditDataResult>(request.ProjectUid.Value.ToString(), $"/productiondataedit{queryParams}", customHeaders);
          var assetMatches = await GetAssetIds(projectIds, results.DataEdits.Select(d => d.AssetUid).ToList());
          var convertedResults = from d in results.DataEdits select ProductionDataEdit.CreateProductionDataEdit(assetMatches[d.AssetUid], d.StartUtc, d.EndUtc, d.MachineDesignName, d.LiftNumber);
          return EditDataResult.CreateEditDataResult(convertedResults.ToList());

#if RAPTOR
        }

        return ProcessWithRaptor(request);
#endif
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

#if RAPTOR
    private ContractExecutionResult ProcessWithRaptor(GetEditDataRequest request)
    {
      TDesignName[] designNames;
      TDesignLayer[] layers;

      if (request.assetId == null)
      {
        designNames = raptorClient.GetOverriddenDesigns(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, -1);
        layers = raptorClient.GetOverriddenLayers(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, -1);
      }
      else
      {
        designNames = raptorClient.GetOverriddenDesigns(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
          request.assetId <= 0 ? -1 : (long)request.assetId);
        layers = raptorClient.GetOverriddenLayers(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
          request.assetId <= 0 ? -1 : (long)request.assetId);
      }

      return EditDataResult.CreateEditDataResult(ConvertDataEdits(designNames, layers));
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
#endif
  }
}
