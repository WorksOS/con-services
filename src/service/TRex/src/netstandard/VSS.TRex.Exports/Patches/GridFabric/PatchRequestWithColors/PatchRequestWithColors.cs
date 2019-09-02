using System.Linq;
using System.Threading.Tasks;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Exports.Patches.GridFabric.PatchRequestWithColors
{
  /// <summary>
  /// Sends a request with colors to the grid for a patch of sub grids
  /// </summary>
  public class PatchRequestWithColors : GenericASNodeRequest<PatchRequestWithColorsArgument, PatchRequestWithColorsComputeFunc, PatchRequestWithColorsResponse>
  {
    public PatchRequestWithColors() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }

    public async Task<PatchResultWithColors> ExecuteAndConvertToResult(PatchRequestWithColorsArgument argument)
    {
      var response = await ExecuteAsync(argument);

      var result = new PatchResultWithColors
      {
        RenderValuesToColours = argument.RenderValuesToColours,
        TotalNumberOfPagesToCoverFilteredData = response.TotalNumberOfPagesToCoverFilteredData,
        MaxPatchSize = argument.DataPatchSize,
        PatchNumber = argument.DataPatchNumber,
        Patch = response?.SubGrids?.Select(x =>
        {
          var s = new SubgridDataPatchRecord_ElevationAndColor();
          s.Populate(x);
          return s;
        }).ToArray()
      };

      return result;
    }
  }
}
