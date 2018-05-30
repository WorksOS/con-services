using System;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  public class MachineLiftDetails : MachineDetails
  {
    public LiftDetails[] lifts { get; private set; }

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
        lifts = lifts
      };
    }
  }

  public class LiftDetails
  {
    public long layerId { get; set; }
    public DateTime endUtc { get; set; }
  }
}
