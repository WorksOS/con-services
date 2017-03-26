using System;
using System.Linq;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    #region Result
    /// <summary>
    /// The GET response body: Coordinate system settings result object.
    /// This is copied from ...\RaptorServices\CoordSvc\ResultHandling\CoordinateSystemSettings.cs
    /// </summary>
    public class CoordinateSystemSettings : RequestResult, IEquatable<CoordinateSystemSettings>
    {
        #region Members
        /// <summary>
        /// The coordinate system file name.
        /// </summary>
        /// 
        public string csName { get; set; }

        /// <summary>
        /// The name of the coordinate system group.
        /// </summary>
        /// 
        public string csGroup { get; set; }

        /// <summary>
        /// The coordinate system definition as an array of bytes.
        /// </summary>
        /// 
        public byte[] csib { get; set; }

        /// <summary>
        /// The coordinate system datum name.
        /// </summary>
        /// 
        public string datumName { get; set; }

        /// <summary>
        /// The flag indicates whether or not there are site calibration data in a coordinate system definition.
        /// </summary>
        /// 
        public bool siteCalibration { get; set; }

        /// <summary>
        /// The coordinate system geoid model file name.
        /// </summary>
        /// 
        public string geoidFileName { get; set; }

        /// <summary>
        /// The coordinate system geoid model name.
        /// </summary>
        /// 
        public string geoidName { get; set; }

        /// <summary>
        /// The flag indicates whether or not there are datum grid data in a coordinate system definition.
        /// </summary>
        /// 
        public bool isDatumGrid { get; set; }

        /// <summary>
        /// The flag indicates whether or not an assigned coordinate system projection is supported by the application.
        /// </summary>
        /// 
        public bool unsupportedProjection { get; set; }

        /// <summary>
        /// The coordinate system latitude datum grid file name.
        /// </summary>
        /// 
        public string latitudeDatumGridFileName { get; set; }

        /// <summary>
        /// The coordinate system longitude datum grid file name.
        /// </summary>
        /// 
        public string longitudeDatumGridFileName { get; set; }

        /// <summary>
        /// The coordinate system height datum grid file name.
        /// </summary>
        /// 
        public string heightDatumGridFileName { get; set; }

        /// <summary>
        /// The coordinate system shift grid file name.
        /// </summary>
        /// 
        public string shiftGridName { get; set; }

        /// <summary>
        /// The coordinate system snake grid file name.
        /// </summary>
        /// 
        public string snakeGridName { get; set; }

        /// <summary>
        /// The coordinate system vertical datum name.
        /// </summary>
        /// 
        public string verticalDatumName { get; set; }
        #endregion

        #region Constructors
        public CoordinateSystemSettings(int code, string message = "")
            : base(code, message)
        { }

        public CoordinateSystemSettings()
            : base("success")
        { }
        #endregion

        #region Equality test
        public bool Equals(CoordinateSystemSettings other)
        {
            if (other == null)
                return false;

            return this.csName == other.csName &&
                this.csGroup == other.csGroup &&
                this.csib.SequenceEqual(other.csib) &&
                this.datumName == other.datumName &&
                this.siteCalibration == other.siteCalibration &&
                this.geoidFileName == other.geoidFileName &&
                this.geoidName == other.geoidName &&
                this.isDatumGrid == other.isDatumGrid &&
                this.unsupportedProjection == other.unsupportedProjection &&
                this.latitudeDatumGridFileName == other.latitudeDatumGridFileName &&
                this.longitudeDatumGridFileName == other.longitudeDatumGridFileName &&
                this.heightDatumGridFileName == other.heightDatumGridFileName &&
                this.shiftGridName == other.shiftGridName &&
                this.snakeGridName == other.snakeGridName &&
                this.verticalDatumName == other.verticalDatumName &&
                this.Code == other.Code &&
                this.Message == other.Message;
        }

        public static bool operator ==(CoordinateSystemSettings a, CoordinateSystemSettings b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(CoordinateSystemSettings a, CoordinateSystemSettings b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is CoordinateSystemSettings && this == (CoordinateSystemSettings)obj;
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

    #region Request
    /// <summary>
    /// The POST request body: Coordinate system (CS) definition file domain object.
    /// This is copied from ...\RaptorServices\CoordSvc\Models\CoordinateSystemFile.cs
    /// </summary>
    ///
    public class CoordinateSystemFile
    {
        /// <summary>
        /// The project to process the CS definition file into.
        /// </summary>
        /// 
        public long? projectId { get; set; }

        /// <summary>
        /// The content of the CS definition file as a sequence of bytes.
        /// </summary>
        /// 
        public byte[] csFileContent { get; set; }

        /// <summary>
        /// The name of the CS definition file.
        /// </summary>
        /// 
        public string csFileName { get; set; }
    } 
    #endregion
}
