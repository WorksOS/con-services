using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using VSS.MasterData.Models.FIlters;

namespace VSS.Productivity3D.Models.Models
{
    /// <summary>
    /// TAG file domain object. Model represents TAG file submitted to Raptor.
    /// </summary>
    public class CompactionTagFileRequest
    {
        /// <summary>
        /// A project unique identifier.
        /// </summary>
        public Guid? ProjectUid { get; set; }

        /// <summary>
        /// The name of the TAG file.
        /// </summary>
        /// <remarks>
        /// Shall contain only ASCII characters.
        /// </remarks>
        [JsonProperty(Required = Required.Always)]
        [ValidFilename(maxlength: 256)]
        public string FileName { get; set; }

        /// <summary>
        /// The content of the TAG file as an array of bytes.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public byte[] Data { get; set; }

        /// <summary>
        /// Defines Org ID (either from TCC or Connect) to support project-based subs
        /// </summary>
        public string OrgId { get; set; }
    }
}