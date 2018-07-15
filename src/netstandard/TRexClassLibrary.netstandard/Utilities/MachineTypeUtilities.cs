using VSS.TRex.Types;

namespace VSS.TRex.Utilities
{
  /// <summary>
  /// Utilities relating to the machine types supported
  /// </summary>
  public static class MachineTypeUtilities
  {
    /// <summary>
    /// Notes if the pass counting basis for the machine type in in temrs of 'half pasess', ie: one half pass per tracked axle of the machine
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsHalfPassCompactorMachine(MachineType type) => type == MachineType.FourDrumLandfillCompactor;

    /// <summary>
    /// Notes if the pass counting basis for the machine type in in temrs of 'half pasess', ie: one half pass per tracked axle of the machine
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsHalfPassCompactorMachine(byte type) => (MachineType)type == MachineType.FourDrumLandfillCompactor;    
  }
}
