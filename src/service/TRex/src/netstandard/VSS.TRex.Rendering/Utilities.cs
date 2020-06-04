using System;
using System.Linq;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.Models;
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
        public static bool DisplayModeRequireSurveyedSurfaceInformation(DisplayMode mode)
        {
            return mode == DisplayMode.Height ||
                   mode == DisplayMode.CutFill ||
                   mode == DisplayMode.CompactionCoverage ||
                   mode == DisplayMode.VolumeCoverage ||
                   mode == DisplayMode.TargetThicknessSummary;
        }

        /// <summary>
        /// Determines if the supplied filter supports the inclusion of surveyed surface information in the request results
        /// </summary>
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
        /// Determine if the request needs to use the existence from a design to determine which sub grids to request
        /// </summary>
        public static bool RequestRequiresAccessToDesignFileExistenceMap(DisplayMode mode, DesignOffset design)
        {
            // Some requests benefit from having the existence map for a reference design on hand.
            // These are Cut Fill, Summary Volume(*) and Thickness Summary requests (*)
            // (*) Where these requests are specified with either filter-design or design-filter
            //     modes identified by the presence of a reference design

            if (design == null || design.DesignID.Equals(Guid.Empty))
              return false;

            return mode == DisplayMode.CutFill || mode == DisplayMode.VolumeCoverage || mode == DisplayMode.TargetThicknessSummary;
        }

        /// <summary>
        /// Determines if the scale relationship between pixels and the cells being rendered on them is such that the world 
        /// extent of the pixel is greater than the world extent of a sub grid (32x32 cells)
        /// </summary>
        public static bool SubGridShouldBeRenderedAsRepresentationalDueToScale(double worldTileWidth, double worldTileHeight, int nPixelsX, int nPixelsY, double subGridCellSize)
        {
            return (worldTileWidth / nPixelsX) >= subGridCellSize || (worldTileHeight / nPixelsY) >= subGridCellSize;
        }

        /// <summary>
        /// Computes the CCA palette for the specified machine
        /// </summary>
        public static IPlanViewPalette ComputeCCAPalette(ISiteModel siteModel, ICellPassAttributeFilter filter, DisplayMode mode)
        {
          var machineUid = filter.MachinesList.Length > 0 ? filter.MachinesList[0] : Guid.Empty;

          var ccaMinimumPassesValue = siteModel.GetCCAMinimumPassesValue(machineUid, filter.StartTime, filter.EndTime, filter.LayerID);

          if (ccaMinimumPassesValue == 0)
            return null;

          var ccaColorScale = CCAColorScaleManager.CreateCoverageScale(ccaMinimumPassesValue);

          var transitions = new Transition[ccaColorScale.TotalColors];

          for (var i = 0; i < transitions.Length; i++)
            transitions[i] = new Transition(i + 1, ColorUtility.UIntToColor(ccaColorScale.ColorSegments[transitions.Length - i - 1].Color));

          if (mode == DisplayMode.CCA)
          {
            return new CCAPalette {PaletteTransitions = transitions};
          }

          return new CCASummaryPalette
          {
            UndercompactedColour = transitions[0].Color, 
            CompactedColour = transitions[1].Color, 
            OvercompactedColour = transitions[2].Color
          };
        }
    }
}
