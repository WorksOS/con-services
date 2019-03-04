using System;

namespace VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels
{
    public class ProjectSubscription
    {
        public string ProjectUID { get; set; }
        public string SubscriptionUID { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime LastActionedUTC { get; set; }
    }
}