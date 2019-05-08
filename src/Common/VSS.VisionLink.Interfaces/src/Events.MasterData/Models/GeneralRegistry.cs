using System;
using System.Collections.Generic;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
    public class GeneralRegistry
    {
        public TimeSpan? RunTimeHoursAdj;

        public DateTime? RuntimeHoursSentUTC;

        public DateTime? LastRegistrationDate;

        public string RegistrationStatus;

        public ReportingSchedule ReportSchedule;

        public bool? GlobalGramEnable;

        public DateTime? GlobalGramSentUTC;

        public string ModuleType;

        public string DataLinkType;

        public bool? BlockDataTransfer;

        public string RegDealerCode;

        public SoftwareInfo Software;

        public bool? StartStopEnable;

        public DateTime? StartStopEnableSentUTC;

        public class ReportingSchedule
        {
            public TimeSpan? ReportStartTime;
            public DateTime? ReportStartTimeSentUTC;
            public List<Report> Reports;

            public class Report
            {
                public string ReportType;
                public int? frequency;
                public DateTime? SentUTC;
            }
        }

        public class SoftwareInfo
        {
            public string HC11SoftwarePartNumber;
            
            public string ModemSoftwarePartNumber;
            
            public string HardwareSerialNumber;
            
            public string SoftwareRevision;
        }
    }
}
