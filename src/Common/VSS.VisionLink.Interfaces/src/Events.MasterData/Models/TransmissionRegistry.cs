using System;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public class TransmissionRegistry
    {
        public EventReportingFrequency EventReporting;
        
        public SMUFuelReporting? SMUFuel;
        
        public DateTime? SMUFuelSentUTC;
        
        public int? NextMessageInterval;
        
        public DateTime? NextMessageIntervalSentUTC;
        
        public int? EventIntervalHours;
        
        public DateTime? EventIntervalHoursSentUTC;

        public class EventReportingFrequency
        {
            
            public EventFrequency? Level1EventFreqCode;
            
            public DateTime? Level1EventFreqSentUTC;
            
            public EventFrequency? Level2EventFreqCode;
            
            public DateTime? Level2EventFreqSentUTC;
            
            public EventFrequency? Level3EventFreqCode;
            
            public DateTime? Level3EventFreqSentUTC;
            
            public EventFrequency? DiagnosticFreqCode;
            
            public DateTime? DiagnosticFreqSentUTC;
        }
    }
}
