using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModel.AssetSettings
{
    public class AssetSettingsListDto
    {
        public Guid AssetUID {
            get
            {
                Guid assetUid = Guid.Empty;
                Guid.TryParse(AssetUIDString, out assetUid);
                return assetUid;
            }
        }

        public string AssetUIDString { get; set; }

        public string AssetName { get; set; }

        public string SerialNumber { get; set; }

        public string MakeCode { get; set; }

		public string DeviceType { get; set; }

		public string Model { get; set; }

        public int? IconKey { get; set; }

        public string DeviceSerialNumber { get; set; }

        public int TargetStatus { get; set; }

        public long? DailyLocationReportingFrequency { get; set; }
        
        public TimeSpan? DailyReportingTime { get; set; }
        
        public string DiagnosticReportFrequency { get; set; }
        
        public long? EventDiagnosticFilterInterval { get; set; }
        
        public bool? GlobalGram { get; set; }
        
        public string HighSeverityEvents { get; set; }
        
        public string HourMeterFuelReport { get; set; }
        
        public Decimal? HoursMeter { get; set; }
        
        public string LowSeverityEvents { get; set; }
        
        public int? MaintenanceModeDuration { get; set; }
        
        public string MediumSeverityEvents { get; set; }
        
        public Decimal? MovingOrStoppedThreshold { get; set; }
        
        public int? MovingThresholdsDuration { get; set; }
        
        public Decimal? MovingThresholdsRadius { get; set; }
        
        public int? NextSentEventInHours { get; set; }
        
        public Decimal? Odometer { get; set; }
        
        public bool? ReportAssetStartStop { get; set; }
        
        public string SentUTC { get; set; }
        
        public string SMHOdometerConfig { get; set; }
        
        public int? SpeedThreshold { get; set; }
        
        public int? SpeedThresholdDuration { get; set; }
        
        public bool? SpeedThresholdEnabled { get; set; }
        
        public string StartTime { get; set; }
        
        public bool? Status { get; set; }

        public bool? SecurityMode { get; set; }

        public int? SecurityStatus { get; set; }

        public int? WorkDefinition { get; set; }
        
        public int ConfiguredSwitches { get; set; }
        
        public int TotalSwitches { get; set; }

        public DateTime? ReportingSchedulePendingUpdatedOn { get; set; }

        public DateTime? MovingThresholdPendingUpdatedOn { get; set; }

        public DateTime? MaintenanceModePendingUpdatedOn { get; set; }

        public DateTime? FaultCodeReportingPendingUpdatedOn { get; set; }

        public DateTime? SwitchesPendingUpdatedOn { get; set; }

        public DateTime? SpeedingThresholdPendingUpdatedOn { get; set; }

        public DateTime? MetersPendingUpdatedOn { get; set; }

        public DateTime? AssetSecurityPendingUpdatedOn { get; set; }
    }
}
