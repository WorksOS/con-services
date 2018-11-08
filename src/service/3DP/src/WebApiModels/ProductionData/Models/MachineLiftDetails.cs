using Newtonsoft.Json;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  public class MachineLiftDetails : MachineDetails
  {
    [JsonProperty(PropertyName = "lifts")]
    public LiftDetails[] Lifts { get; private set; }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static MachineLiftDetails CreateMachineLiftDetails(long assetId, string machineName, bool isJohnDoe, LiftDetails[] lifts)
    {
      return new MachineLiftDetails
      {
        AssetId = assetId,
        MachineName = machineName,
        IsJohnDoe = isJohnDoe,
        Lifts = lifts
      };
    }
  }
}
