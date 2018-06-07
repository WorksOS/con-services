
using System;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  public class MachineDesignDetails : MachineDetails
  {
    public DesignName[] designs { get; private set; }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static MachineDesignDetails CreateMachineDesignDetails(long assetId, string machineName, bool isJohnDoe, DesignName[] designs)
    {
      return new MachineDesignDetails
      {
        AssetId = assetId,
        MachineName = machineName,
        IsJohnDoe = isJohnDoe,
        designs = designs
      };
    }
  }

}
