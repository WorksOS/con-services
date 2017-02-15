using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.TagFileAuth.Service.Models.RaptorServicesCommon
{
  /// <summary>
  /// The request representation used to request the boundaries of projects that are active at a specified date time and belong to the owner
  /// of the specified asset.
  /// </summary>
  public class GetProjectBoundariesAtDateRequest // : IValidatable, IServiceDomainObject, IHelpSample
  {
    /// <summary>
    /// The id of the asset owned by the customer whose active project boundaries are returned. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
    public long assetId { get; private set; }

    /// <summary>
    /// The date time from the tag file which must be within the active project date range. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "tagFileUTC", Required = Required.Always)]
    public DateTime tagFileUTC { get; private set; }

    ///// <summary>
    ///// Private constructor
    ///// </summary>
    //private GetProjectBoundariesAtDateRequest()
    //{ }

    /// <summary>
    /// Create instance of GetProjectBoundariesAtDateRequest
    /// </summary>
    public static GetProjectBoundariesAtDateRequest CreateGetProjectBoundariesAtDateRequest(
      long assetId,
      DateTime tagFileUTC
      )
    {
      return new GetProjectBoundariesAtDateRequest
      {
        assetId = assetId,
        tagFileUTC = tagFileUTC
      };
    }

    /// <summary>
    /// Example for Help
    /// </summary>
    public static GetProjectBoundariesAtDateRequest HelpSample
    {
      get
      {
        return CreateGetProjectBoundariesAtDateRequest(1892337661625085, DateTime.UtcNow.AddMinutes(-1));
      }
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
    }
  }
}