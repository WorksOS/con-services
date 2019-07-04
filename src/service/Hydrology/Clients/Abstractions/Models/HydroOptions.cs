using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Abstractions.Models
{
  public class HydroOptions
  {
    /// <summary>The resolution of analyzing mesh i.e. relates to points resolution </summary>
    [JsonProperty(PropertyName = "Resolution", Required = Required.Default)]
    public double Resolution { get; set; }


    public HydroOptions()
    {
      Initialize();
    }

    private void Initialize()
    {
      Resolution = double.NaN;
    }

    public HydroOptions(double resolution)
    {
      Initialize();
      Resolution = resolution;
    }

    public void Validate()
    {
      if (Resolution <= 0.005 || Resolution > 1000000) // todoJeannie what should these be?
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2004, "Resolution must be between 0.005 and < 1,000,000."));
      }
    }
  }
}

