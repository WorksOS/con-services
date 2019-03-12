using System;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Models.Models
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
    public double? lastKnownLatitude { get; set; }

    /// <summary>
    /// The last reported position of the machine in radians.
    /// </summary>
    public double? lastKnownLongitude { get; set; }

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
    public MachineStatus(
        long assetID,
        string machineName,
        bool isJohnDoe,
        string lastKnownDesignName,
        ushort? lastKnownLayerId,
        DateTime? lastKnownTimeStamp,
        double? lastKnownLatitude,
        double? lastKnownLongitude,
        double? lastKnownX,
        double? lastKnownY,
        Guid? assetUid = null
        )
    {
      this.AssetId = assetID;
      this.MachineName = machineName;
      this.IsJohnDoe = isJohnDoe;
      this.lastKnownDesignName = lastKnownDesignName;
      this.lastKnownLayerId = lastKnownLayerId;
      this.lastKnownTimeStamp = lastKnownTimeStamp;
      this.lastKnownLatitude = lastKnownLatitude;
      this.lastKnownLongitude = lastKnownLongitude;
      this.lastKnownX = lastKnownX;
      this.lastKnownY = lastKnownY;
      this.AssetUid = assetUid;
    }
  }
}
