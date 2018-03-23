using System;
using RaptorSvcAcceptTestsCommon.Models;
using Newtonsoft.Json;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// The request to identify the cell, display information and other configuration information to determine a datum value for the cell.
    /// One of llPoint or gridPoint must be defined.
    /// </summary>
    public class CellDatumRequest
    {
        #region Members
        /// <summary>
        /// Project ID. Required.
        /// </summary>
        public long? projectId;

        /// <summary>
        /// The datum type to return (eg: height, CMV, Temperature etc). 
        /// Required.
        /// </summary>
        public DisplayMode displayMode;

        /// <summary>
        /// If defined, the WGS84 LL position to identify the cell from. 
        /// May be null.
        /// </summary>       
        public WGSPoint llPoint;

        /// <summary>
        /// If defined, the grid point in the project coordinate system to identify the cell from.
        /// May be null.
        /// </summary>
        public Point gridPoint;

        /// <summary>
        /// The filter to be used to govern selection of the cell/cell pass. 
        /// May be null.
        /// </summary>
        public Filter filter;

        /// <summary>
        /// The ID of the filter to be used.
        /// May be null.
        /// </summary>
        public long filterId;

        /// <summary>
        /// The lift/layer build settings to be used.
        /// May be null.
        /// </summary>
        public LiftBuildSettings liftBuildSettings;

        /// <summary>
        /// The descriptor identifyig the surface design to be used.
        /// May be null.
        /// </summary>
        public DesignDescriptor design; 
        #endregion

        #region Constructors
        public CellDatumRequest()
        { }

        public CellDatumRequest(long projectId, DisplayMode displayMode, WGSPoint llPoint,
            Filter filter = null, long filterId = -1, LiftBuildSettings liftBuildSettings = null, DesignDescriptor design = null)
        {
            this.projectId = projectId;
            this.displayMode = displayMode;
            this.llPoint = llPoint;
            this.gridPoint = null;
            this.filter = filter;
            this.filterId = filterId;
            this.liftBuildSettings = liftBuildSettings;
            this.design = design;
        }

        public CellDatumRequest(long projectId, DisplayMode displayMode, Point gridPoint,
            Filter filter = null, long filterId = -1, LiftBuildSettings liftBuildSettings = null, DesignDescriptor design = null)
        {
            this.projectId = projectId;
            this.displayMode = displayMode;
            this.gridPoint = gridPoint;
            this.llPoint = null;
            this.filter = filter;
            this.filterId = filterId;
            this.liftBuildSettings = liftBuildSettings;
            this.design = design;
        }
        #endregion
    } 
    #endregion

  #region Result
    public class CellDatumResult : RequestResult, IEquatable<CellDatumResult>
    {
    #region Members
    /// <summary>
    /// THe display mode used in the original request
    /// </summary>
    public DisplayMode displayMode;

    /// <summary>
    /// The internal result code resulting from the request.
    /// </summary>
    public short returnCode;

    /// <summary>
    /// The value from the request, scaled in accordance with the underlying attribute domain.
    /// </summary>
    public double value;

    /// <summary>
    /// The date and time of the value.
    /// </summary>
    public DateTime timestamp { get; set; }
    #endregion

    #region Constructor
    public CellDatumResult()
        : base("success")
    {
    }
    #endregion

    #region Equality test
    public virtual bool Equals(CellDatumResult other)
    {
        if (other == null)
            return false;

        return base.Equals(other) && 
            this.displayMode == other.displayMode &&
            this.returnCode == other.returnCode &&
            Math.Round(this.value, 3) == Math.Round(other.value, 3) &&
            this.timestamp == other.timestamp;
    }

    public static bool operator ==(CellDatumResult a, CellDatumResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CellDatumResult a, CellDatumResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
        return obj is CellDatumResult && this == (CellDatumResult)obj;
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