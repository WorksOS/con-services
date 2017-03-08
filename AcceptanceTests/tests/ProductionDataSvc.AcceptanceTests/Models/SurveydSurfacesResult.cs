using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaptorSvcAcceptTestsCommon.Models;
using Newtonsoft.Json;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    /// <summary>
    /// Representation of Surveyed Surface in a Raptor project.
    /// </summary>
    /// 
    public class SurveyedSurfaces
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

    #region Result
    public class GetSurveydSurfacesResult : RequestResult, IEquatable<GetSurveydSurfacesResult>
    {
        #region Members
        public List<SurveyedSurfaces> SurveyedSurfaces { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor: success result by default
        /// </summary>
        public GetSurveydSurfacesResult()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(GetSurveydSurfacesResult other)
        {
            if (other == null)
                return false;

            if (this.SurveyedSurfaces.Count != other.SurveyedSurfaces.Count)
                return false;

            for (int i = 0; i < this.SurveyedSurfaces.Count; ++i)
            {
                if (!other.SurveyedSurfaces.Exists(d =>
                    d.AsAtDate == this.SurveyedSurfaces[i].AsAtDate &&
                    d.SurveyedSurface.file.fileName == this.SurveyedSurfaces[i].SurveyedSurface.file.fileName &&
                    d.SurveyedSurface.file.filespaceId == this.SurveyedSurfaces[i].SurveyedSurface.file.filespaceId &&
                    d.Id == this.SurveyedSurfaces[i].Id))
                    return false;
            }

            return this.Code == other.Code && this.Message == other.Message;
        }

        public static bool operator ==(GetSurveydSurfacesResult a, GetSurveydSurfacesResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(GetSurveydSurfacesResult a, GetSurveydSurfacesResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is GetSurveydSurfacesResult && this == (GetSurveydSurfacesResult)obj;
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
