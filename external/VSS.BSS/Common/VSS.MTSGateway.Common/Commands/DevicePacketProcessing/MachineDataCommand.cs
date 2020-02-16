using System;
using System.Collections.Generic;
using VSS.Nighthawk.MTSGateway.Interfaces.Commands.DevicePacketProcessing;

namespace VSS.Nighthawk.MTSGateway.Common.Commands.DevicePacketProcessing
{
  public class MachineDataCommand :  IMachineDataCommand
  {
    public Guid CorrelationId { get; set; }
    public string SerialNumber { get; set; }
    public int DeviceType { get; set; }
    public byte[] Payload { get; set; }
    public DateTime ReceivedUTC { get; set; }
    public string Endpoint { get; set; }
    public long? SequenceNumber { get; set; }
    public double? OldSMH { get; set; }
    public double? SMHAfterCalibration { get; set; }
    public bool NeedToCreateDataServiceMeterAdjustment { get; set; }
    public Dictionary<byte, string> AddressClaims { get; set; }
  }
}
