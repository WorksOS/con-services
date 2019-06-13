using System;
using System.Linq;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Rendering.Palettes.CCAColorScale;
using VSS.TRex.Rendering.Palettes.Interfaces;
using VSS.TRex.SiteModels.Interfaces;

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
            return Mode == DisplayMode.Height ||
                   Mode == DisplayMode.CutFill ||
                   Mode == DisplayMode.CompactionCoverage ||
                   Mode == DisplayMode.VolumeCoverage ||
                   Mode == DisplayMode.TargetThicknessSummary;
        }

        /// <summary>
        /// Determines if the supplied filter supports the inclusion of surveyed surface information in the request results
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static bool FilterRequireSurveyedSurfaceInformation(IFilterSet filters)
        {
            return filters == null ||
                   !filters.Filters.Any(x => x.AttributeFilter.HasVibeStateFilter ||
                                             x.AttributeFilter.HasDesignFilter ||
                                             x.AttributeFilter.HasMachineDirectionFilter ||
                                             x.AttributeFilter.HasMachineFilter ||
                                             x.AttributeFilter.HasGPSAccuracyFilter);
        }

        /// <summary>
        /// Determine if the request needs to use the existence from a design to determine which subgrids to request
        /// </summary>
        /// <param name="Mode"></param>
        /// <returns></returns>
        public static bool RequestRequiresAccessToDesignFileExistenceMap(DisplayMode Mode
            // ReferenceVolumeType : TComputeICVolumesType
            )
        {
            // Some requests benefit from having the existence map for a reference design on hand.
            // These are Cut Fill, Summary Volume(*) and Thickness Summary requests (*)
            // (*) Where these requests are specified with either filter-design or design-filter
            //     volume computation modes
            return Mode == DisplayMode.CutFill ||
                     ((Mode == DisplayMode.VolumeCoverage || Mode == DisplayMode.TargetThicknessSummary) 
                     //&& (ReferenceVolumeType in [ic_cvtBetweenFilterAndDesign, ic_cvtBetweenDesignAndFilter]))
                      );
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
        public static bool SubGridShouldBeRenderedAsRepresentationalDueToScale(double WorldTileWidth, double WorldTileHeight, int NPixelsX, int NPixelsY, double SubGridCellSize)
        {
            return (WorldTileWidth / NPixelsX) >= SubGridCellSize || (WorldTileHeight / NPixelsY) >= SubGridCellSize;
        }

        /// <summary>
        /// Computes the CCA palette for the specified machine
        /// </summary>
        /// <param name="siteModel"></param>
        /// <param name="filter"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static IPlanViewPalette ComputeCCAPalette(ISiteModel siteModel, ICellPassAttributeFilter filter, DisplayMode mode)
        {
          var machineUID = filter.MachinesList.Length > 0 ? filter.MachinesList[0] : Guid.Empty;

          var ccaMinimumPassesValue = siteModel.GetCCAMinimumPassesValue(machineUID, filter.StartTime, filter.EndTime, filter.LayerID);

          if (ccaMinimumPassesValue == 0)
            return null;

          var ccaColorScale = CCAColorScaleManager.CreateCoverageScale(ccaMinimumPassesValue);

          var transitions = new Transition[ccaColorScale.TotalColors];

          for (var i = 0; i < transitions.Length; i++)
            transitions[i] = new Transition(i + 1, ColorUtility.UIntToColor(ccaColorScale.ColorSegments[transitions.Length - i - 1].Color));

          if (mode == DisplayMode.CCA)
          {
            var ccaPalette = new CCAPalette();
            ccaPalette.PaletteTransitions = transitions;

            return ccaPalette;
          }

          var ccaSummaryPalette = new CCASummaryPalette();
          ccaSummaryPalette.UndercompactedColour = transitions[0].Color;
          ccaSummaryPalette.CompactedColour = transitions[1].Color;
          ccaSummaryPalette.OvercompactedColour = transitions[2].Color;

          return ccaSummaryPalette;
        }
  }
}
