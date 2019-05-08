using System;
using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public class DeviceDetailsConfigInfoEvent : IDeviceDetailsConfigInfoEvent
    {
        public string ModuleCode;

        public DeviceType DeviceType;

        public bool IsGlobalGramSet = false;

        public bool? GlobalGramEnabled;

        public int? SatelliteNumber;

        public bool IsEcmListSet = false;

        public List<MTSEcmInfo> EcmList;

        public bool IsFirmwareVersionsSet = false;

        public string FirmwareVersions;

        public bool IsConfigDataSet = false;

        public MessageStatus Status;

        public List<GeneralRegistry> GeneralRegistries;

        public List<TransmissionRegistry> TransmissionRegistries;

        public List<DigitalRegistry> DigitalRegistries;

        public DateTime ActionUTC { get; set; }
        public DateTime ReceivedUTC { get; set; }
    }
}
