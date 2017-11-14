using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Rendering
{
    public static class Utilities
    {
        /// <summary>
        /// Determines if a display mode requires surveyed surface information to be included within it
        /// </summary>
        /// <param name="Mode"></param>
        /// <returns></returns>
        public static bool DisplayModeRequireSurveyedSurfaceInformation(DisplayMode Mode)
        {
            return (Mode == DisplayMode.Height) ||
                   (Mode == DisplayMode.CutFill) ||
                   (Mode == DisplayMode.CompactionCoverage) ||
                   (Mode == DisplayMode.VolumeCoverage) ||
                   (Mode == DisplayMode.TargetThicknessSummary);
        }

        /// <summary>
        /// Determines if the supplied filter supports the inclusion of surveyed surface information in the request results
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static bool FilterRequireSurveyedSurfaceInformation(FilterSet filters)
        {
            if (filters == null)
            {
                return true;
            }

            return !filters.Filters.Any(x => x.AttributeFilter.HasVibeStateFilter ||
                                             x.AttributeFilter.HasDesignFilter ||
                                             x.AttributeFilter.HasMachineDirectionFilter ||
                                             x.AttributeFilter.HasMachineFilter ||
                                             x.AttributeFilter.HasGPSAccuracyFilter);
        }
    }
}
