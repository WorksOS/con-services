namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public class MTSEcmInfo
    {
        public string[] engineSerialNumbers;
        
        public string[] transmissionSerialNumbers;
        
        public byte datalink;
        
        public bool actingMasterECM;
        
        public bool syncSMUClockSupported;
        
        public byte eventProtocolVersion;
        
        public byte diagnosticProtocolVersion;
        
        public string mid1;
        
        public ushort toolSupportChangeLevel1;
        
        public ushort applicationLevel1;
        
        public ushort? mid2 = null;
        
        public ushort? toolSupportChangeLevel2 = null;
        
        public ushort? applicationLevel2 = null;
        
        public string softwarePartNumber = string.Empty;
        
        public string serialNumber = string.Empty;
        
        public string SoftwareDescription = null;
        
        public string ReleaseDate = null;
        
        public string PartNumber = null;
        
        public ushort? SourceAddress = null;
        
        public bool? ArbitraryAddressCapable;
        
        public byte? IndustryGroup;
        
        public byte? VehicleSystemInstance;
        
        public byte? VehicleSystem;
        
        public byte? Function;
        
        public byte? FunctionInstance;
        
        public byte? ECUInstance;
        
        public ushort? ManufacturerCode;
        
        public int? IdentityNumber;
        
        public string J1939Name;

        public long ecmID = -1;
    }
}
