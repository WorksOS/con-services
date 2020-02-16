using System;
using System.Collections.Generic;

namespace VSS.Nighthawk.MTSGateway.Interfaces.Commands.DevicePacketProcessing
{
  public interface IMachineDataCommand
  {
    Guid CorrelationId { get; set; }
    string SerialNumber { get; set; }
    int DeviceType { get; set; }
    byte[] Payload { get; set; }
    DateTime ReceivedUTC { get; set; }
    string Endpoint { get; set; }
    long? SequenceNumber { get; set; }
    double? OldSMH { get; set; }
    double? SMHAfterCalibration { get; set; }
    bool NeedToCreateDataServiceMeterAdjustment { get; set; }
    Dictionary<byte, string> AddressClaims { get; set; }
  }
}