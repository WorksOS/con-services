using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaptorSvcAcceptTestsCommon.Models;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Utils;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    /// <summary>
    /// The request represents CCA data color palette to be used by a map legend.
    /// </summary>
    /// 
    public class CCAColorPaletteResult : RequestResult, IEquatable<CCAColorPaletteResult>
    {
        #region Members
        /// <summary>
        /// The set of colours to be used by a map legend.
        /// </summary>
        /// 
        public TColourPalette[] palettes { get; set; } 
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor: Success by default
        /// </summary>
        public CCAColorPaletteResult()
            : base("success")
        { } 
        #endregion

        #region Equality test
        public bool Equals(CCAColorPaletteResult other)
        {
            if (other == null)
                return false;

            if (this.palettes.Length != other.palettes.Length)
                return false;

            for (int i = 0; i < this.palettes.Length; ++i)
            {
                if (!(this.palettes[i].Colour == other.palettes[i].Colour && Math.Round(this.palettes[i].Value, 1) == Math.Round(other.palettes[i].Value, 1)))
                    return false;
            }
            return this.Code == other.Code && this.Message == other.Message;
        }

        public static bool operator ==(CCAColorPaletteResult a, CCAColorPaletteResult b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(CCAColorPaletteResult a, CCAColorPaletteResult b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is CCAColorPaletteResult && this == (CCAColorPaletteResult)obj;
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
}
