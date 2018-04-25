using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// Request representation for requesting project statistics
    /// </summary>
    public class StatisticsParameters
    {
        /// <summary>
        /// The project to request the statistics for using legacy project id
        /// </summary>
        public long? projectId { get; set; }
        /// <summary>
        /// The project to request the statistics for using uid
        /// </summary>
        public string projectUid { get; set; }

    /// <summary>
    /// The set of surveyed surfaces that should be excluded from the calculation of the spatial and temporal extents of the project.
    /// </summary>
    public long[] excludedSurveyedSurfaceIds { get; set; }
    }
    #endregion

    #region Result
    /// <summary>
    /// A representation of a set of spatial and temporal stastics for the project as a whole
    /// </summary>
    public class ProjectStatistics : RequestResult, IEquatable<ProjectStatistics>
    {
        #region Members
        /// <summary>
        /// Earlist time stamped data present in the project, including both production and surveyed surface data.
        /// </summary>
        public DateTime startTime { get; set; }

        /// <summary>
        /// Latest time stamped data present in the project, including both production and surveyed surface data.
        /// </summary>
        public DateTime endTime { get; set; }

        /// <summary>
        /// Size of spatial data cells in the project (the default value is 34cm)
        /// </summary>
        public Double cellSize { get; set; }

        /// <summary>
        /// The index origin offset from the absolute bottom left origin of the subgrid tree cartesian coordinate system to the centered origin of the cartesian
        /// grid coordinate system used in the project, and the centered origin cartesian coordinates of cell addresses.
        /// </summary>
        public Int32 indexOriginOffset { get; set; }

        /// <summary>
        /// The three dimensional extents of the project including both production and surveyed surface data.
        /// </summary>
        public BoundingBox3DGrid extents { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public ProjectStatistics()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(ProjectStatistics other)
        {
            if (other == null)
                return false;

            return this.startTime == other.startTime &&
                this.endTime == other.endTime &&
                this.cellSize == other.cellSize &&
                this.indexOriginOffset == other.indexOriginOffset &&
                ( Math.Round(this.extents.maxX, 2) == Math.Round(other.extents.maxX, 2) &&
                    Math.Round(this.extents.maxY, 2) == Math.Round(other.extents.maxY, 2) &&
                    Math.Round(this.extents.maxZ, 2) == Math.Round(other.extents.maxZ, 2) &&
                    Math.Round(this.extents.minX, 2) == Math.Round(other.extents.minX, 2) &&
                    Math.Round(this.extents.minY, 2) == Math.Round(other.extents.minY, 2) &&
                    Math.Round(this.extents.minZ, 2) == Math.Round(other.extents.minZ, 2) ) &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(ProjectStatistics a, ProjectStatistics b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(ProjectStatistics a, ProjectStatistics b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is ProjectStatistics && this == (ProjectStatistics)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region ToString override
        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>A string representation.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        #endregion
    } 
    #endregion
}
