using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaptorSvcAcceptTestsCommon.Models
{
    /// <summary>
    /// Provides settings for thickness summary requests. Target and tolerance should be specified in meters.
    /// </summary>
    public class LiftThicknessTarget
    {
        /// <summary>
        /// Gets or sets the target lift thickness (absolute value in meters). This is only used with TargetThicknessSummary diplay mode and summary report.
        /// </summary>
        /// <value>
        /// The target lift thickness.
        /// </value>
        public float TargetLiftThickness { get; set; }

        /// <summary>
        /// Gets or sets the above tolerance lift thickness (absolute value in meters). This is only used with TargetThicknessSummary diplay mode and summary report
        /// </summary>
        /// <value>
        /// The above tolerance lift thickness.
        /// </value>
        public float AboveToleranceLiftThickness { get; set; }

        /// <summary>
        /// Gets or sets the below tolerance lift thickness (absolute value in meters). This is only used with TargetThicknessSummary diplay mode and summary report
        /// </summary>
        /// <value>
        /// The below tolerance lift thickness.
        /// </value>
        public float BelowToleranceLiftThickness { get; set; }
    }
}
