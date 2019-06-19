using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Hydrology.WebApi.Abstractions.Models;
using VSS.Hydrology.WebApi.Abstractions.Models.ResultHandling;

namespace VSS.Hydrology.WebApi.Controllers
{
  /// <summary>
  /// drainage controller.
  /// </summary>
  public class DrainageController : BaseController<DrainageController>
  {
    /// <inheritdoc />
    public DrainageController(IConfigurationStore configStore) :
      base(configStore)
    { }

    /// <summary>
    /// Generates a ponding pdf from a design file (TIN) using hydro libraries
    /// </summary>
    [HttpPost("api/v1/ponding")]
    public async Task<DrainageResult> GetPondingFromDesignFile(DrainageRequest drainageRequest)
    {
      Log.LogDebug($"{nameof(GetPondingFromDesignFile)}: request {drainageRequest}");
      return new DrainageResult(string.Empty);
    }
  }
}
