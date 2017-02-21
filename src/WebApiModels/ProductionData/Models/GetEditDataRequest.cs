
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;


namespace VSS.Raptor.Service.WebApiModels.ProductionData.Models
{
  /// <summary>
  /// Request to get manually edited production data changes.
  /// </summary>
  public class GetEditDataRequest : ProjectID, IValidatable
  {

    /// <summary>
    /// The id of the machine whose data is overridden. If not provided then overridden data for all machines for the specified project is returned.
    /// </summary>
    [JsonProperty(PropertyName = "assetId", Required = Required.Default)]
    public long? assetId { get; private set; }

         /// <summary>
    /// Private constructor
    /// </summary>
    private GetEditDataRequest()
    {
    }


    /// <summary>
    /// Create instance of GetEditDataRequest
    /// </summary>
    public static GetEditDataRequest CreateGetEditDataRequest(
      long projectId,
      long assetId
      )
    {
      return new GetEditDataRequest
             {
                projectId = projectId,
                assetId = assetId
             };
    }


    /// <summary>
    /// Create example instance of GetEditDataRequest to display in Help documentation.
    /// </summary>
    public new static GetEditDataRequest HelpSample
    {
      get
      {
        return new GetEditDataRequest()
               {
                   projectId = 1523,
                   assetId = 753648483
               };
      }
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
    }
  }
}