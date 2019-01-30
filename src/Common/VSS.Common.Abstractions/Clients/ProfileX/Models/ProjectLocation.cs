using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.ProfileX.Models
{
  public class ProjectLocation
  {
    /// <summary>
    /// Specifies the type of address like corporate headquarters. Any custom types are allowed. Each type that is defined must be unique.
    /// </summary>
    [JsonProperty("type", Required = Required.Always)]
    public string Type { get; set; }

    /// <summary>
    /// Flag to determine whether the address is primary or secondary
    /// </summary>
    [JsonProperty("primary")]
    public bool Primary { get; set; }

    /// <summary>
    /// Street address of the project location. New lines can be added with line separators.
    /// </summary>
    [JsonProperty("street")]
    public string Street { get; set; }

    /// <summary>
    /// The town or city of the address
    /// </summary>
    [JsonProperty("locality")]
    public string Locality { get; set; }

    /// <summary>
    /// The abbreviated province or state. The Region field will support ISO-3166 format.
    /// </summary>
    [JsonProperty("region")]
    public string Region { get; set; }

    /// <summary>
    /// PIN or ZIP Code corresponding to the region.
    /// </summary>
    [JsonProperty("postalCode")]
    public string PostalCode { get; set; }

    /// <summary>
    /// Country codes will be based on the ISO 3166-1 alpha-2 standard
    /// </summary>
    [JsonProperty("country")]
    public string Country { get; set; }

    /// <summary>
    /// Latitude of the location
    /// </summary>
    [JsonProperty("latitude")]
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude of the location
    /// </summary>
    [JsonProperty("longitude")]
    public double? Longitude { get; set; }

  }
}