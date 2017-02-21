using System;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.Models
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
        assetID = assetID,
        machineName = machineName,
        isJohnDoe = isJohnDoe,
        lastKnownDesignName = lastKnownDesignName,
        lastKnownLayerId = lastKnownLayerId,
        lastKnownTimeStamp = lastKnownTimeStamp,
        lastKnownLatitude = lastKnownLatitude,
        lastKnownLongitude = lastKnownLongitude,
        lastKnownX = lastKnownX,
        lastKnownY = lastKnownY

      };
    }

    /// <summary>
    /// Create example instance of MachineStatus to display in Help documentation.
    /// </summary>
    public static new MachineStatus HelpSample
    {
      get
      {
        return new MachineStatus()
        {
          assetID = 1137642418461469,
          machineName = "VOLVO G946B",
          isJohnDoe = false,
          lastKnownDesignName = "Green Park",
          lastKnownLayerId = 2,
          lastKnownTimeStamp = DateTime.UtcNow.AddHours(-2),
          lastKnownLatitude = 3.010479,
          lastKnownLongitude = -0.758809
        };
      }
    }



  }
}