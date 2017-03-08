using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaptorSvcAcceptTestsCommon.Models;
using Newtonsoft.Json;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Resquest
    /// <summary>
    /// The POST request body: A representation of a project extents request
    /// This is copied from ...\ProductionDataSvc.WebAPI\Models\ExtentRquest.cs
    /// </summary>
    public class ProjectExtentRequest
    {
        /// <summary>
        /// The project ID to request the extents for
        /// </summary>
        public long? projectId { get; set; }

        /// <summary>
        /// The set of surveyed surface IDs to be excluded from the calculation of the project extents
        /// </summary>
        public long[] excludedSurveyedSurfaceIds { get; set; }

        public ProjectExtentRequest()
        {
            projectId = -1;
            excludedSurveyedSurfaceIds = new long[] { };
        }
    } 
    #endregion

    #region Result
    /// <summary>
    /// The POST response body: ProjectExtentsResult
    /// This is copied from ...\ProductionDataSvc.WebAPI\ResultHandling\ProjectExtentsResult.cs
    /// </summary>
    public class ProjectExtentsResult : RequestResult, IEquatable<ProjectExtentsResult>
    {
        #region Members
        /// <summary>
        /// BoundingBox3DGrid
        /// </summary>
        public BoundingBox3DGrid ProjectExtents { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Default construct says "success"
        /// </summary>
        /// <param name="message"></param>
        public ProjectExtentsResult()
            : base("success")
        { }

        public ProjectExtentsResult(BoundingBox3DGrid extents, int code, string message = "")
            : base(code, message)
        {
            this.ProjectExtents = extents;
        }
        #endregion

        #region Equality test
        public bool Equals(ProjectExtentsResult other)
        {
            if (other == null)
                return false;

            return Math.Round(this.ProjectExtents.maxX, 6) == Math.Round(other.ProjectExtents.maxX, 6) &&
                Math.Round(this.ProjectExtents.maxY, 6) == Math.Round(other.ProjectExtents.maxY, 6) &&
                Math.Round(this.ProjectExtents.maxZ, 6) == Math.Round(other.ProjectExtents.maxZ, 6) &&
                Math.Round(this.ProjectExtents.minX, 6) == Math.Round(other.ProjectExtents.minX, 6) &&
                Math.Round(this.ProjectExtents.minY, 6) == Math.Round(other.ProjectExtents.minY, 6) &&
                Math.Round(this.ProjectExtents.minZ, 6) == Math.Round(other.ProjectExtents.minZ, 6) &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(ProjectExtentsResult a, ProjectExtentsResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(ProjectExtentsResult a, ProjectExtentsResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is ProjectExtentsResult && this == (ProjectExtentsResult)obj;
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
