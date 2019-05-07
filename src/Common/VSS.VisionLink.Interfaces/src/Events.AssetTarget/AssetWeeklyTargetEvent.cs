using System;
using VSS.VisionLink.Interfaces.Events.AssetTarget.Context;

namespace VSS.VisionLink.Interfaces.Events.AssetTarget
{
    /// <summary>
    /// This Event will be published when target value such as Runtime, Idling hours for a week has been created for an Asset
    /// </summary>
    public class AssetWeeklyTargetEvent
    {
        /// <summary>
        /// Asset Target Unique Id
        /// </summary>
        public Guid AssetTargetUID { get; set; }

        /// <summary>
        /// Asset Unique Identifier
        /// </summary>
        public Guid AssetUID { get; set; }

		/// <summary>
		/// Start Date for the asset target
		/// </summary>
		/// <remarks>
		/// For IdletimeHours and RuntimeHours, this will be enforced to be a Sunday by the Admin UI (for now).
		/// </remarks>
		public DateTime StartDate { get; set; }

		/// <summary>
		/// End Date for the asset target
		/// </summary>
		/// <remarks>
		/// Set as nullable to handle situations in the future where this may be null. But currenltly, this will always have a value.
		/// For IdletimeHours and RuntimeHours, this will be enforced to be 6 days after StartDate (which is always Saturday) by the Admin UI (for now).
		/// </remarks>
		public DateTime? EndDate { get; set; }

        /// <summary>
        /// UTC time that the target was created
        /// </summary>
        public DateTime InsertUTC { get; set; }

        /// <summary>
        /// UTC time that the target was updated or deleted
        /// </summary>
        public DateTime UpdateUTC { get; set; }

		/// <summary>
		/// One of value defined in AssetTargetType Enum
		/// </summary>
		/// 
		/// <remarks>
		/// Currently, only the following types are supported for Weekly target setting:
		/// IdletimeHours, RuntimeHours, PayloadinTonnes, CycleCount, and VolumeinCuMeter
		/// </remarks>
		public AssetTargetType TargetType { get; set; }

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
        /// Active state of the configuration
        /// </summary>
        /// <remarks>
		/// For IdletimeHours and RuntimeHours, this will always be set to true.
        /// </remarks>
        public bool StatusInd { get; set; }
    }
}
