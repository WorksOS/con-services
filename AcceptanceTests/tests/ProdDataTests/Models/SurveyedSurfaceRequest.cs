using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaptorSvcAcceptTestsCommon.Models;
using Newtonsoft.Json;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// The request representation for managing of the Raptor’s list of ground/surveyed SurveyedSurfaces.
    /// </summary>
    /// 
    public class SurveyedSurfaceRequest
    {
        /// <summary>
        /// Project ID. Required.
        /// </summary>
        ///
        public long projectID { get; set; }

        /// <summary>
        /// Description to identify a surveyed surface file either by id or by its location in TCC.
        /// </summary>
        /// 
        public DesignDescriptor SurveyedSurface { get; set; }

        /// <summary>
        /// Surveyed UTC date/time.
        /// </summary>
        /// 
        public DateTime surveyedUTC { get; set; }
    } 
    #endregion

    #region Result
    /// <summary>
    /// Surveyed Surface result class.
    /// </summary>
    /// 
    public class SurveyedSurfaceResult : RequestResult, IEquatable<SurveyedSurfaceResult>
    {
        #region Members
        /// <summary>
        /// Array of Surveyed Surface details.
        /// </summary>
        /// 
        public SurveyedSurfaceDetails[] SurveyedSurfaces { get; set; } 
        #endregion

        #region Constructor
        public SurveyedSurfaceResult()
            : base("success")
        { } 
        #endregion

        #region Equality test
        public bool Equals(SurveyedSurfaceResult other)
        {
            if (other == null)
                return false;

            if (this.SurveyedSurfaces.Length != other.SurveyedSurfaces.Length)
                return false;

            for (int i = 0; i < this.SurveyedSurfaces.Length; ++i)
            {
                if (this.SurveyedSurfaces[i].Id != other.SurveyedSurfaces[i].Id ||
                        (this.SurveyedSurfaces[i].SurveyedSurface.id != other.SurveyedSurfaces[i].SurveyedSurface.id ||
                             (this.SurveyedSurfaces[i].SurveyedSurface.file.filespaceId != other.SurveyedSurfaces[i].SurveyedSurface.file.filespaceId ||
                              this.SurveyedSurfaces[i].SurveyedSurface.file.path != other.SurveyedSurfaces[i].SurveyedSurface.file.path ||
                              this.SurveyedSurfaces[i].SurveyedSurface.file.fileName != other.SurveyedSurfaces[i].SurveyedSurface.file.fileName) ||
                         this.SurveyedSurfaces[i].SurveyedSurface.offset != other.SurveyedSurfaces[i].SurveyedSurface.offset) ||
                    this.SurveyedSurfaces[i].AsAtDate != other.SurveyedSurfaces[i].AsAtDate ||
                        (Math.Round(this.SurveyedSurfaces[i].Extents.maxX, 3) != Math.Round(other.SurveyedSurfaces[i].Extents.maxX, 3) ||
                         Math.Round(this.SurveyedSurfaces[i].Extents.maxY, 3) != Math.Round(other.SurveyedSurfaces[i].Extents.maxY, 3) ||
                         Math.Round(this.SurveyedSurfaces[i].Extents.maxZ, 3) != Math.Round(other.SurveyedSurfaces[i].Extents.maxZ, 3) ||
                         Math.Round(this.SurveyedSurfaces[i].Extents.minX, 3) != Math.Round(other.SurveyedSurfaces[i].Extents.minX, 3) ||
                         Math.Round(this.SurveyedSurfaces[i].Extents.minY, 3) != Math.Round(other.SurveyedSurfaces[i].Extents.minY, 3) ||
                         Math.Round(this.SurveyedSurfaces[i].Extents.minZ, 3) != Math.Round(other.SurveyedSurfaces[i].Extents.minZ, 3)))
                {
                    return false;
                }
            }

            return this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(SurveyedSurfaceResult a, SurveyedSurfaceResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(SurveyedSurfaceResult a, SurveyedSurfaceResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is CellDatumResult && this == (SurveyedSurfaceResult)obj;
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

    /// <summary>
    /// Representation of Surveyed Surface in a Raptor project.
    /// </summary>
    /// 
    public class SurveyedSurfaceDetails
    {
        /// <summary>
        /// The ID of the Surveyed Surface file.
        /// </summary>
        /// 
        public long Id { get; set; }

        /// <summary>
        /// Description to identify a surveyed surface file either by id or by its location in TCC.
        /// </summary>
        /// 
        public DesignDescriptor SurveyedSurface { get; set; }

        /// <summary>
        /// Surveyed UTC date/time.
        /// </summary>
        /// 
        public DateTime AsAtDate { get; set; }

        /// <summary>
        /// T3DBoundingWorldExtent describes a plan extent (X and Y) covering a
        /// rectangular area of the world in world coordinates, and a height range
        /// within that extent.
        /// </summary>
        /// 
        public BoundingBox3DGrid Extents { get; set; }
    } 
    #endregion
}
