
namespace VSS.Nighthawk.DataOut.Interfaces.Enums
{
  public enum MachineStartStatus
  {
    NotConfigured = -0x02,
    NoPending = -0x01,
    NormalOperation = 0x00,
    Derate = 0x01,
    Disable = 0x02,
    NormalOperationPending = 0x10,
    DeratedPending = 0x11,
    DisabledPending = 0x12
  }
}
