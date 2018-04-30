using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// The class is used to schedule a tile generation job within TCC
  /// </summary>
  public class TileJobRequest
  {
    /// <summary>
    /// Gets or sets the project identifier.
    /// </summary>
    /// <value>
    /// The project identifier.
    /// </value>
    [JsonProperty(PropertyName = "projectId", Required = Required.Always)]
    public long projectId { get; set; }

    /// <summary>
    /// Gets or sets the file description.
    /// </summary>
    /// <value>
    /// The file description.
    /// </value>
    [JsonProperty(PropertyName = "fileDescr", Required = Required.Always)]
    public FileDescriptor fileDescr { get; set; }

    /// <summary>
    /// Gets or sets the suffix.
    /// </summary>
    /// <value>
    /// The suffix.
    /// </value>
    [JsonProperty(PropertyName = "suffix", Required = Required.Always)]
    public string suffix { get; set; }

    /// <summary>
    /// Gets or sets the zoom result.
    /// </summary>
    /// <value>
    /// The zoom result.
    /// </value>
    [JsonProperty(PropertyName = "zoomResult", Required = Required.Always)]
    public ZoomRangeResult zoomResult { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="TileJobRequest"/> is regenerate.
    /// </summary>
    /// <value>
    ///   <c>true</c> if regenerate; otherwise, <c>false</c>.
    /// </value>
    [JsonProperty(PropertyName = "regenerate", Required = Required.Always)]
    public bool regenerate { get; set; }
  }
}
