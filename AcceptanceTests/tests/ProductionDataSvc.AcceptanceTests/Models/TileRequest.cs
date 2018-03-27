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
using XnaFan.ImageComparison;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Request
    /// <summary>
    /// The request representation for rendering a tile of thematic information such as elevation, compaction, temperature etc
    /// The bounding box of the area to be rendered may be specified in either WGS84 lat/lon or cartesian grid coordinates in the project coordinate system.
    /// </summary>
    public class TileRequest
    {
        /// <summary>
        /// Project ID. Required.
        /// </summary>
        public long? projectId { get; set; }

        /// <summary>
        /// An identifying string from the caller
        /// </summary>
        public Guid callId { get; set; }

        /// <summary>
        /// The thematic mode to be rendered; elevation, compaction, temperature etc
        /// </summary>
        public DisplayMode mode { get; set; }

        /// <summary>
        /// The set of colours to be used to map the datum values in the thematic data to colours to be rendered in the tile.
        /// </summary>
        public List<ColorPalette> palettes { get; set; }

        /// <summary>
        /// Color to be used to render subgrids representationaly when the production data is zoomed too far away.
        /// </summary>
        /// <value>
        /// The display color of the representational.
        /// </value>
        public uint representationalDisplayColor { get; set; }

        /// <summary>
        /// The settings to be used when considering compaction information being processed and analysed in preparation for rendering.
        /// </summary>
        public LiftBuildSettings liftBuildSettings { get; set; }

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
        /// The bounding box enclosing the area to be rendered. The bounding box is expressed in terms of WGS84 latitude and longitude positions, expressed in radians.
        /// Value may be null but either this or the bounding box in grid coordinates must be provided.
        /// </summary>
        public BoundingBox2DLatLon boundBoxLL { get; set; }

        /// <summary>
        /// The bounding box enclosing the area to be rendered. The bounding box is expressed in terms of cartesian grid coordinates in the project coordinate system, expressed in meters.
        /// Value may be null but either this or the bounding box in lat/lng coordinates must be provided.
        /// </summary>
        public BoundingBox2DGrid boundBoxGrid { get; set; }

        /// <summary>
        /// The width, in pixels, of the image tile to be rendered
        /// </summary>
        public ushort width { get; set; }

        /// <summary>
        /// The height, in pixels, of the image tile to be rendered
        /// </summary>
        public ushort height { get; set; }
    } 
    #endregion

    #region Result
    public class TileResult : RequestResult, IEquatable<TileResult>
    {
        #region Members
        public byte[] TileData { get; set; }
        public bool TileOutsideProjectExtents { get; set; }
        #endregion

        #region Constructor
        public TileResult() :
            base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(TileResult other)
        {
            if (other == null)
                return false;

            Image thisImage;
            Image otherImage;

            using(var ms = new MemoryStream(this.TileData))
            {
                thisImage = Image.FromStream(ms);
            }
            using(var ms = new MemoryStream(other.TileData))
            {
                otherImage = Image.FromStream(ms);
            }

            float imageDifference = ExtensionMethods.PercentageDifference(thisImage, otherImage, 0);

            // If the difference between two images is less than 3% then they are considered the same
            return imageDifference < 0.03 &&
                this.TileOutsideProjectExtents == other.TileOutsideProjectExtents &&
                this.Code == other.Code &&
                this.Message == other.Message;

            //return this.TileData.SequenceEqual(other.TileData) &&
            //    this.Code == other.Code &&
            //    this.Message == other.Message;
        }

        public static bool operator ==(TileResult a, TileResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(TileResult a, TileResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is TileResult && this == (TileResult)obj;
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
