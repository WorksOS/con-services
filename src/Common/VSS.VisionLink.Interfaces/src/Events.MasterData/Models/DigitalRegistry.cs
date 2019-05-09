using System;
using System.Collections.Generic;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public class DigitalRegistry
    {
        public List<SensorInformation> Sensors;
        public class SensorInformation
        {
            public int SensorNumber;
            
            public InputConfig? SensorConfiguration;
            
            public DateTime? SensorConfigSentUTC;
            
            public TimeSpan? DelayTime;
            
            public DateTime? DelayTimeSentUTC;
            
            public string Description;
            
            public DateTime? DescriptionSentUTC;
            
            public DigitalInputMonitoringConditions? MonitorCondition;
            
            public DateTime? MonitorConditionSentUTC;
        }
    }
}
