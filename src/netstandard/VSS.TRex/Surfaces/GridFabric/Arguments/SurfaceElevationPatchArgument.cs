using System;
using System.Linq;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Surfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Surfaces.GridFabric.Arguments
{
    public class SurfaceElevationPatchArgument
    {
        /// <summary>
        /// The ID of the SiteModel to execute the request against
        /// </summary>
        public Guid SiteModelID { get; set; } = Guid.Empty;

        /// <summary>
        /// The bottom left on-the-ground cell origin X location for the patch of elevations to be computed from
        /// </summary>
        public uint OTGCellBottomLeftX { get; set; }

        /// <summary>
        /// The bottom left on-the-ground cell origin Y location for the patch of elevations to be computed from
        /// </summary>
        public uint OTGCellBottomLeftY { get; set; }

        /// <summary>
        /// The cell stepping size to move between points in the patch being interpolated
        /// </summary>
        public double CellSize { get; set; }

        /// <summary>
        /// Detemines which surface information shoudl be extracted: Earliest, Latest or Composite
        /// </summary>
        public SurveyedSurfacePatchType SurveyedSurfacePatchType { get; set; }

        /// <summary>
        /// A map of the cells within the subgrid patch to be computed
        /// </summary>
        public SubGridTreeBitmapSubGridBits ProcessingMap { get; set; }

        /// <summary>
        /// The list of surveyed surfaces to be included in the calculation
        /// [Note: This is fairly inefficient, the receiver of the request should be able to access surveyed surfaces locally...]
        /// </summary>
        public ISurveyedSurfaces IncludedSurveyedSurfaces { get; set; }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SurfaceElevationPatchArgument()
        {

        }

        /// <summary>
        /// Constructor taking the full state of the surface patch computation operation
        /// </summary>
        /// <param name="siteModelID"></param>
        /// <param name="oTGCellBottomLeftX"></param>
        /// <param name="oTGCellBottomLeftY"></param>
        /// <param name="cellSize"></param>
        /// <param name="surveyedSurfacePatchType"></param>
        /// <param name="processingMap"></param>
        /// <param name="includedSurveyedSurfaces"></param>
        public SurfaceElevationPatchArgument(Guid siteModelID,
                                             uint oTGCellBottomLeftX,
                                             uint oTGCellBottomLeftY,
                                             double cellSize,
                                             SurveyedSurfacePatchType surveyedSurfacePatchType,
                                             SubGridTreeBitmapSubGridBits processingMap,
                                             SurveyedSurfaces includedSurveyedSurfaces)
        {
            SiteModelID = siteModelID;
            OTGCellBottomLeftX = oTGCellBottomLeftX;
            OTGCellBottomLeftY = oTGCellBottomLeftY;
            CellSize = cellSize;
            SurveyedSurfacePatchType = surveyedSurfacePatchType;
            ProcessingMap = processingMap;
            IncludedSurveyedSurfaces = includedSurveyedSurfaces;

            // Prepare the list of surveyed surfaces for use by all invocations using this argument
            IncludedSurveyedSurfaces.SortChronologically(surveyedSurfacePatchType == SurveyedSurfacePatchType.EarliestSingleElevation);
        }

        /// <summary>
        /// Overloaded ToString to add argument properties
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString() + $" -> SiteModel:{SiteModelID}, OTGOriginBL:{OTGCellBottomLeftX}/{OTGCellBottomLeftY}, CellSize:{CellSize}, SurfacePatchType:{SurveyedSurfacePatchType}";
        }

        /// <summary>
        /// Computes a cachekey for use in a simple dictionary mapping String => SurveyedSurface elevation result request
        /// </summary>
        /// <returns></returns>
        public string CacheKey()
        {
            return $"{SiteModelID}:{OTGCellBottomLeftX}:{OTGCellBottomLeftY}:{CellSize}:{SurveyedSurfacePatchType}:{IncludedSurveyedSurfaces.Select(x => x.ID.ToString()).Aggregate((s1, s2) => s1 + s2)}";
        }
    }
}
