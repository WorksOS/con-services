using System;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  public class MachineStatus : MachineDetails
  {
    /// <summary>
    /// The design currently loaded on the machine.
    /// </summary>
    public string lastKnownDesignName { get; private set; }

    /// <summary>
    /// The layer number currently loaded on the machine.
    /// </summary>
    public ushort? lastKnownLayerId { get; private set; }

    /// <summary>
    /// The time the machine last reported.
    /// </summary>
    public DateTime? lastKnownTimeStamp { get; private set; }

    /// <summary>
    /// The last reported position of the machine in radians.
    /// </summary>
    public double? lastKnownLatitude { get; private set; }

    /// <summary>
    /// The last reported position of the machine in radians.
    /// </summary>
    public double? lastKnownLongitude { get; private set; }

    /// <summary>
    /// The last reported position of the machine in grid coordinates.
    /// </summary>
    public double? lastKnownX { get; private set; }

    /// <summary>
    /// The last reported position of the machine in grid coordinates.
    /// </summary>
    public double? lastKnownY { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private MachineStatus()
    { }

    /// <summary>
    /// Create instance of MachineStatus
    /// </summary>
    public static MachineStatus CreateMachineStatus(
        long assetID,
        string machineName,
        bool isJohnDoe,
        string lastKnownDesignName,
        ushort? lastKnownLayerId,
        DateTime? lastKnownTimeStamp,
        double? lastKnownLatitude,
        double? lastKnownLongitude,
        double? lastKnownX,
        double? lastKnownY
        )
    {
      return new MachineStatus
      {
        AssetId = assetID,
        MachineName = machineName,
        IsJohnDoe = isJohnDoe,
        lastKnownDesignName = lastKnownDesignName,
        lastKnownLayerId = lastKnownLayerId,
        lastKnownTimeStamp = lastKnownTimeStamp,
        lastKnownLatitude = lastKnownLatitude,
        lastKnownLongitude = lastKnownLongitude,
        lastKnownX = lastKnownX,
        lastKnownY = lastKnownY
      };
    }
  }
}
