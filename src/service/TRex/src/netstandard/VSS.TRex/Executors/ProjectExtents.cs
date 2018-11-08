using System;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.Servers.Client;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Executors
{
    /// <summary>
    /// Provides the ability to query a data model for its 3D spatial extents in a number of ways
    /// </summary>
    public static class ProjectExtents
    {
        /// <summary>
        /// Returns the enclosing 3D bounding box for the production data within the project
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static BoundingWorldExtent3D ProductionDataOnly(Guid ID)
        {
            ISiteModel SiteModel = GenericApplicationServiceServer.PerformAction(() => DIContext.Obtain<ISiteModels>().GetSiteModel(ID, false));

            return SiteModel?.SiteModelExtent ?? BoundingWorldExtent3D.Inverted();
        }

        /// <summary>
        /// Returns the enclosing 3D bounding box for the production data and the surveyed surfaces within the project
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static BoundingWorldExtent3D ProductionDataAndSurveyedSurfaces(Guid ID)
        {
            ISiteModel SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(ID, false);

            return SiteModel?.GetAdjustedDataModelSpatialExtents(new Guid[0]) ?? BoundingWorldExtent3D.Inverted();
        }

        /// <summary>
        /// Returns the enclosing 3D bounding box for the production data and the surveyed surfaces within the project, excluding the
        /// surveyed surfaces contained in the provided exclusion list
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="SurveydSurfaceExclusionList"></param>
        /// <returns></returns>
        public static BoundingWorldExtent3D ProductionDataAndSurveyedSurfaces(Guid ID, Guid[] SurveydSurfaceExclusionList)
        {
            ISiteModel SiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(ID, false);

            return SiteModel?.GetAdjustedDataModelSpatialExtents(SurveydSurfaceExclusionList) ?? BoundingWorldExtent3D.Inverted();
        }
    }
}
