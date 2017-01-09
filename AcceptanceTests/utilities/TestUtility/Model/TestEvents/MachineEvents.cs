namespace TestUtility.Model.TestEvents
{
    /// <summary>
    /// This class to to cater for the incoming events
    /// </summary>
    public class MachineEvents
    {
        public string EventType { get; set; }
        public string DayOffset { get; set; }
        public string Timestamp { get; set; }
        public string UtcOffsetHours { get; set; }
        public string SwitchNumber { get; set; }
        public string SwitchState { get; set; }
        public string OdometerKilometers { get; set; }
        //Asset details
        public string AssetName { get; set; }
        public string AssetType { get; set; }
        public string SerialNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string IconId { get; set; }

        public MachineEvents()
        {
            UtcOffsetHours = "0";
        }
    }
}
