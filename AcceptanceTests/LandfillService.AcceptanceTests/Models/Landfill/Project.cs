using System;
using System.Collections.Generic;

namespace LandfillService.AcceptanceTests.Models.Landfill
{
    public class Project
    {
        public uint id { get; set; }
        public string name { get; set; }
        public string timeZoneName { get; set; }      // project time zone name
        public int? daysToSubscriptionExpiry { get; set; }
        public string projectUid { get; set; }
        public string legacyTimeZoneName { get; set; }
    }
}