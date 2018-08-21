using System.Linq;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering
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
        public static bool FilterRequireSurveyedSurfaceInformation(IFilterSet filters)
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

        /// <summary>
        /// Determine if the request needs to use the existance from a design to determine which subgrids to request
        /// </summary>
        /// <param name="Mode"></param>
        /// <returns></returns>
        public static bool RequestRequiresAccessToDesignFileExistanceMap(DisplayMode Mode
            // ReferenceVolumeType : TComputeICVolumesType
            )
        {
            // Some requests benefit from having the existance map for a reference design on hand.
            // These are Cut Fill, Summary Volume(*) and Thickness Summary requests (*)
            // (*) Where these requests are specified with either filter-design or design-filter
            //     volume computation modes
            return (Mode == DisplayMode.CutFill ||
                     ((Mode == DisplayMode.VolumeCoverage || Mode == DisplayMode.TargetThicknessSummary) 
                     //&& (ReferenceVolumeType in [ic_cvtBetweenFilterAndDesign, ic_cvtBetweenDesignAndFilter]))
                      ));
        }

        /// <summary>
        /// Determines if the scale relationship between pixels and the cells being rendered on them is such that the world 
        /// extent of the pixel is greater than the world extent of a subgrid (32x32 cells)
        /// </summary>
        /// <param name="WorldTileWidth"></param>
        /// <param name="WorldTileHeight"></param>
        /// <param name="NPixelsX"></param>
        /// <param name="NPixelsY"></param>
        /// <param name="SubGridCellSize"></param>
        /// <returns></returns>
        public static bool SubgridShouldBeRenderedAsRepresentationalDueToScale(double WorldTileWidth, double WorldTileHeight, int NPixelsX, int NPixelsY, double SubGridCellSize)
        {
            return ((WorldTileWidth / NPixelsX) >= SubGridCellSize || (WorldTileHeight / NPixelsY) >= SubGridCellSize);
        }
    }
}
