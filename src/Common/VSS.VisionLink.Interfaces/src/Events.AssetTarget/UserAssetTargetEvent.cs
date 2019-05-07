using System;
using VSS.VisionLink.Interfaces.Events.AssetTarget.Context;

namespace VSS.VisionLink.Interfaces.Events.AssetTarget
{
    /// <summary>
    /// This Event will be published when target value such as Volume, Productivity hours and Mileage for a week has been created for an Asset
    /// </summary>
    public class UserAssetTargetEvent
    {

        /// <summary>
        /// Asset Unique Identifier
        /// </summary>
        public Guid AssetUID { get; set; }
        /// <summary>
        /// Start Date for the asset target
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// End Date for the asset target
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// EventUTC for the event
        /// </summary>
        public TimestampDetail Timestamp { get; set; }
        /// <summary>
        /// One of value defined in AssetTargetType Enum
        /// </summary>
        public AssetTargetType TargetType { get; set; }
        /// <summary>
        /// Modified By UserUID
        /// </summary>
        public Guid? UserUID { get; set; }
		/// <summary>
        /// Modified By CustomerUID
        /// </summary>
        public Guid? CustomerUID { get; set; }


        /// <summary>
        /// Target Value to be configured
        /// </summary>
        public double TargetValue { get; set; }

        /// <summary>
        /// To identify whether this is a system generated record. ie., Backfill
        /// </summary>
        public bool IsSystemGenerated { get; set; }
    }
}
