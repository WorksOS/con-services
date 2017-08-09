using System;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;

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
   
    /// <summary>
    /// Create example instance of MachineLiftDetails to display in Help documentation.
    /// </summary>
    public new static MachineLiftDetails HelpSample
    {
      get
      {
        var details = MachineDetails.HelpSample;
        return CreateMachineLiftDetails(
          details.assetID,
          details.machineName,
          details.isJohnDoe,
          new LiftDetails[]
          {
              new LiftDetails{layerId = 1L, endUtc = new DateTime(2016, 2, 5, 11, 15, 20)},
              new LiftDetails{layerId = 5L, endUtc = new DateTime(2016, 2, 5, 13, 2, 5)},
              new LiftDetails{layerId = 6L, endUtc = new DateTime(2016, 2, 5, 15, 7, 45)}
          });
      }
    }

  }

  public class LiftDetails
  {
    public long layerId { get; set; }
    public DateTime endUtc { get; set; }
  }
}