using System;
using VSS.VisionLink.Interfaces.Events.AssetTarget.Context;

namespace VSS.VisionLink.Interfaces.Events.AssetTarget
{
    /// <summary>
    /// This Event will be published when target value such as Volume, Productivity hours and Mileage for a week has been created for an Asset
    /// </summary>
    public class AssetTargetEvent
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
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End Date for the asset target
        /// </summary>
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
		/// <remarks>
		/// Currently, only the following types are supported for Weekly target setting:
		/// OdometerinKmsPerWeek, BucketVolumeinCuMeter, IdlingBurnRateinLiPerHour, and WorkingBurnRateinLiPerHour
		/// </remarks>
		public AssetTargetType TargetType { get; set; }

        /// <summary>
        /// Target Value to be configured
        /// </summary>
        public double TargetValue { get; set; }

        /// <summary>
        /// Active state of the configuration
        /// </summary>
        public bool StatusInd { get; set; }

    }
}
