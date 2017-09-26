using System;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  public class MachineLiftDetails : MachineDetails
  {
    public LiftDetails[] lifts { get; private set; }

    /// <summary>
    /// Create instance of MachineLiftDetails
    /// </summary>
    public static MachineLiftDetails CreateMachineLiftDetails(
        long assetId,
        string machineName,
        bool isJohnDoe,
        LiftDetails[] lifts
        )
    {
      return new MachineLiftDetails
      {
        assetID = assetId,
        machineName = machineName,
        isJohnDoe = isJohnDoe,
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