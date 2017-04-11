using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.SiteModels;

namespace VSS.VisionLink.Raptor.Executors
{
    /// <summary>
    /// Provides the ability to query a data model for it's 3D spatial extents in a number of wayws
    /// </summary>
    public static class ProjectExtents
    {
        /// <summary>
        /// Returns the enclosing 3D bounding box for the production data within the project
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static BoundingWorldExtent3D ProductionDataOnly(long ID)
        {
            SiteModel SiteModel = SiteModels.SiteModels.Instance().GetSiteModel(ID, false);

            return SiteModel == null ? BoundingWorldExtent3D.Inverted() : SiteModel.SiteModelExtent;
        }

        /// <summary>
        /// Returns the enclosing 3D bounding box for the production data and the surveyed surfaces within the project
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static BoundingWorldExtent3D ProductionDataAndSurveyedSurfaces(long ID)
        {
            SiteModel SiteModel = SiteModels.SiteModels.Instance().GetSiteModel(ID, false);

            return SiteModel == null ? BoundingWorldExtent3D.Inverted() : SiteModel.GetAdjustedDataModelSpatialExtents(new long[0]);
        }

        /// <summary>
        /// Returns the enclosing 3D bounding box for the production data and the surveyed surfaces within the project, excluding the
        /// surveyed surfaces contained in the provided exclusion list
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="SurveydSurfaceExclusionList"></param>
        /// <returns></returns>
        public static BoundingWorldExtent3D ProductionDataAndSurveyedSurfaces(long ID, long[] SurveydSurfaceExclusionList)
        {
            SiteModel SiteModel = SiteModels.SiteModels.Instance().GetSiteModel(ID, false);

            return SiteModel == null ? BoundingWorldExtent3D.Inverted() : SiteModel.GetAdjustedDataModelSpatialExtents(SurveydSurfaceExclusionList);
        }
    }
}
