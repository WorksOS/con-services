using System;
using VSS.VisionLink.Interfaces.Events.AssetTarget.Context;

namespace VSS.VisionLink.Interfaces.Events.AssetTarget
{
    /// <summary>
    /// This Event will be published when target value such as Runtime, Idling hours for a week has been created for an Asset
    /// </summary>
    public class UserAssetWeeklyTargetEvent
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
        /// Sunday Target Value to be configured
        /// </summary>
        public double SundayTargetValue { get; set; }
        /// <summary>
        /// Monday Target Value to be configured
        /// </summary>
        public double MondayTargetValue { get; set; }
        /// <summary>
        /// Tuesday Target Value to be configured
        /// </summary>
        public double TuesdayTargetValue { get; set; }
        /// <summary>
        /// Wednesday Target Value to be configured
        /// </summary>
        public double WednesdayTargetValue { get; set; }
        /// <summary>
        /// Thursday Target Value to be configured
        /// </summary>
        public double ThursdayTargetValue { get; set; }
        /// <summary>
        /// Friday Target Value to be configured
        /// </summary>
        public double FridayTargetValue { get; set; }
        /// <summary>
        /// Saturday Target Value to be configured
        /// </summary>
        public double SaturdayTargetValue { get; set; }
        /// <summary>
        /// To identify whether this is a system generated record. ie., Backfill
        /// </summary>
        public bool IsSystemGenerated { get; set; }
    }
}
