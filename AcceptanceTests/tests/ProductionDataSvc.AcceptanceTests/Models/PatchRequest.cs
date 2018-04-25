using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SVOICDecls;
using SVOSiteVisionDecls;
using SVOICProfileCell;
using SVOICGridCell;
using SVOICFiltersDecls;
using RaptorSvcAcceptTestsCommon.Models;
using Newtonsoft.Json;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// The representation of a Patch request. A patch defines a series of subgrids of cell data returned to the caller. patchNumber and patchSize control which patch of
    /// subgrid and cell data need to be returned within the overall set of patches that comprise the overall data set identified by the thematic dataset, filtering and
    /// analytics parameters within the request.
    /// Requesting patch number 0 will additionally return a summation of the total number of patches of the requested size that need to be requested in order to assemble the
    /// complete data set.
    /// </summary>
    public class PatchRequest
    {
        /// <summary>
        /// Project ID. Required.
        /// </summary>
        public long? projectId { get; set; }

        /// <summary>
        /// An identifying string from the caller
        /// </summary>
        public Guid? callId { get; set; }

        /// <summary>
        /// The thematic mode to be rendered; elevation, compaction, temperature etc
        /// </summary>
        public DisplayMode mode { get; set; }

        /// <summary>
        /// The set of colours to be used to map the datum values in the thematic data to colours to be rendered in the tile.
        /// </summary>
        public List<ColorPalette> palettes { get; set; }

        /// <summary>
        /// The settings to be used when considering compaction information being processed and analysed in preparation for rendering.
        /// </summary>
        public LiftBuildSettings liftBuildSettings { get; set; }

        /// <summary>
        /// Render the thematic data into colours using the supplied color palettes.
        /// </summary>
        public bool renderColorValues { get; set; }

        /// <summary>
        /// The volume computation type to use for summary volume thematic rendering
        /// </summary>
        public VolumesType computeVolType { get; set; }

        /// <summary>
        /// The tolerance to be used to indicate no change in volume for a cell. Used for summary volume thematic rendering. Value is expressed in meters.
        /// </summary>
        public double computeVolNoChangeTolerance { get; set; }

        /// <summary>
        /// The descriptor for the design to be used for volume or cut/fill based thematic renderings.
        /// </summary>
        public DesignDescriptor designDescriptor { get; set; }

        /// <summary>
        /// The base or earliest filter to be used.
        /// </summary>
        public FilterResult filter1 { get; set; }

        /// <summary>
        /// The ID of the base or earliest filter to be used.
        /// </summary>
        public long filterId1 { get; set; }

        /// <summary>
        /// The top or latest filter to be used.
        /// </summary>
        public FilterResult filter2 { get; set; }

        /// <summary>
        /// The ID of the top or latest filter to be used.
        /// </summary>
        public long filterId2 { get; set; }

        /// <summary>
        /// The method of filtering cell passes into layers to be used for thematic renderings that require layer analysis as an input into the rendered data.
        /// If this value is provided any layer method provided in a filter is ignored.
        /// </summary>
        public FilterLayerMethod filterLayerMethod { get; set; }

        /// <summary>
        /// The number of the patch of data to be requested in the overall series of patches covering the required dataset.
        /// </summary>
        public int patchNumber { get; set; }

        /// <summary>
        /// The number of subgrids to return in the patch
        /// </summary>
        public int patchSize { get; set; }
    } 
    #endregion

    #region Result
    /// <summary>
    /// A single cell of information within a subgrid in a patch
    /// </summary>
    public class PatchCellResult
    {
        #region Members
        /// <summary>
        /// Elevation at the cell center point
        /// </summary>
        public float elevation { get; set; }

        /// <summary>
        /// Requested thematic datum. Intepretation and parsing depends on the thematic domain
        /// </summary>
        public ushort datum { get; set; }

        /// <summary>
        /// The color the cell is rendered in. Only present if renderColorValues is true.
        /// </summary>
        public uint color { get; set; } 
        #endregion

        #region For object comparison
        public static bool operator ==(PatchCellResult a, PatchCellResult b)
        {
            return a.elevation == b.elevation &&
                a.datum == b.datum &&
                a.color == b.color;
        }

        public static bool operator !=(PatchCellResult a, PatchCellResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is PatchCellResult && this == (PatchCellResult)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

    /// <summary>
    /// A subgrid of information within a patch result
    /// </summary>
    public class PatchSubgridResult
    {
        #region Members
        /// <summary>
        /// The X ordinate location of the cell in the catersian coorindate space of cell address indexes centered on the catersian grid coordinate system of the project
        /// NOTE: As of the time of writing the cell origin positions returned need to be adjusted by the subgridtree IndexOriginOffset value - this should be taken into account when the subgrid is written into the patch.
        /// </summary>
        public int cellOriginX { get; set; }

        /// <summary>
        /// The X ordinate location of the cell in the catersian coorindate space of cell address indexes centered on the catersian grid coordinate system of the project
        /// NOTE: As of the time of writing the cell origin positions returned need to be adjusted by the subgridtree IndexOriginOffset value - this should be taken into account when the subgrid is written into the patch.
        /// </summary>
        public int cellOriginY { get; set; }

        /// <summary>
        /// If true there are no non-null cells of information retruned by the query for this subgrid.
        /// </summary>
        public bool isNull { get; set; }

        /// <summary>
        /// The elevation origin referenced by all cell elevations in the binary representation of the patch subgrids. Values are expressed in meters.
        /// </summary>
        public float elevationOrigin { get; set; }

        /// <summary>
        /// The grid of cells that make up this subgrid in the patch
        /// </summary>
        public PatchCellResult[,] cells { get; set; } 
        #endregion

        #region Equality test
        public static bool operator ==(PatchSubgridResult a, PatchSubgridResult b)
        {
            if (!(a.isNull || b.isNull))
            {
                if (a.cells.GetLength(0) != b.cells.GetLength(0))
                    return false;
                for (int i = 0; i < a.cells.GetLength(0); ++i)
                {
                    if (a.cells.GetLength(1) != b.cells.GetLength(1))
                        return false;
                    for (int j = 0; j < a.cells.GetLength(1); ++j)
                    {
                        if (a.cells[i, j] != b.cells[i, j])
                            return false;
                    }
                }
            }

            return a.cellOriginX == b.cellOriginX &&
                a.cellOriginY == b.cellOriginY &&
                a.isNull == b.isNull &&
                a.elevationOrigin == b.elevationOrigin;
        }

        public static bool operator !=(PatchSubgridResult a, PatchSubgridResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is PatchSubgridResult && this == (PatchSubgridResult)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

    /// <summary>
    /// A structured representation of the data retruned by the Patch request
    /// </summary>
    public class PatchResultStructured : RequestResult, IEquatable<PatchResultStructured>
    {
        #region Members
        /// <summary>
        /// All cells in the patch are of this size. All measurements relating to the cell in the patch are made at the center point of each cell.
        /// </summary>
        public double cellSize { get; set; }

        /// <summary>
        /// The number of subgrids returned in this patch request
        /// </summary>
        public int numSubgridsInPatch { get; set; }

        /// <summary>
        /// The total number of patch requests that must be made to retrieve all the information identified by the parameters of the patch query. Only returned for requests
        /// that identify patch number 0 in the set to be retrieved.
        /// </summary>
        public int totalNumPatchesRequired { get; set; }

        /// <summary>
        /// The cells in theh subgrids in the patch result have had colors rendered for the thematic data in the cells.
        /// </summary>
        public bool valuesRenderedToColors { get; set; }

        /// <summary>
        /// The collection of subgrids returned in this patch request result.
        /// </summary>
        public PatchSubgridResult[] subgrids { get; set; } 
        #endregion

        #region Constructor
        public PatchResultStructured() :
            base("success")
        { } 
        #endregion

        #region Equality test
        public bool Equals(PatchResultStructured other)
        {
            if (other == null)
                return false;

            if (this.subgrids != null && other.subgrids != null)
            {
                if (this.subgrids.Length != other.subgrids.Length)
                    return false;
                for (int i = 0; i < this.subgrids.Length; ++i)
                    if (this.subgrids[i] != other.subgrids[i]) return false;
            }
            else if (this.subgrids == null || other.subgrids == null)
                return false;

            return this.cellSize == other.cellSize &&
                this.numSubgridsInPatch == other.numSubgridsInPatch &&
                this.totalNumPatchesRequired == other.totalNumPatchesRequired &&
                this.valuesRenderedToColors == other.valuesRenderedToColors &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(PatchResultStructured a, PatchResultStructured b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(PatchResultStructured a, PatchResultStructured b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is PatchResultStructured && this == (PatchResultStructured)obj;
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
