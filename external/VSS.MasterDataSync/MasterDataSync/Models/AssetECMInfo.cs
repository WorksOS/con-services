namespace VSS.Nighthawk.MasterDataSync.Models
{
    public class AssetECMInfo
    {
        // ECM S/N
        public string ECMSerialNumber { get; set; }
        //  Firmware P/N
        public string FirmwarePartNumber { get; set; }
        // ECM Description
        public string ECMDescription { get; set; }
        // Sync Clock Enabled
        public string SyncClockEnabled { get; set; }
        // Sync Clock Level
        public string SyncClockLevel { get; set; }
    }
}