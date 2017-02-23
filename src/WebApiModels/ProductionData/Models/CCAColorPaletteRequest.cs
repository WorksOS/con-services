using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.Models
{
  /// <summary>
  /// CCA Color Palette request domain object. The palette is to be used by a map legend. Model represents a CCA Color Palette request.
  /// </summary>
  public class CCAColorPaletteRequest : ProjectID
  {
    /// <summary>
    /// The Raptor's machine identifier.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
    [Required]
    public long assetId { get; private set; }

    /// <summary>
    /// The UTC start date of the CCA data for which to get the color palettes. 
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "startUtc", Required = Required.Default)]
    public DateTime? startUtc { get; private set; }

    /// <summary>
    /// The UTC end date of the CCA data for which to get the color palettes.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "endUtc", Required = Required.Default)]
    public DateTime? endUtc { get; private set; }

    /// <summary>
    /// The lift identifier for which to get the color palettes.
    /// </summary>
    ///
    [JsonProperty(PropertyName = "liftId", Required = Required.Default)]
    public int? liftId { get; private set; }


    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    private CCAColorPaletteRequest() 
    {
      // ...
    }
    
    /// <summary>
    /// Creates an instance of the CCAColorPaletteRequest class.
    /// </summary>
    /// <param name="projectId">Raptor's data model/project identifier.</param>    
    /// <param name="assetId">Raptor's machine identifier.</param>
    /// <param name="startUtc">The UTC start date of the CCA data for which to get the color palettes.</param>
    /// <param name="endUtc">The UTC end date of the CCA data for which to get the color palettes.</param>
    /// <param name="liftId">The lift identifier for which to get the color palettes.</param>
    /// <param name="projectUid">Raptor's data model/project unique identifier.</param>
    /// <returns>An instance of the CCAColorPaletteRequest class.</returns>    
    /// 
    public static CCAColorPaletteRequest CreateCCAColorPaletteRequest(long projectId, long assetId, DateTime? startUtc, DateTime? endUtc, int? liftId)
    {
      return new CCAColorPaletteRequest() { projectId = projectId, assetId = assetId, startUtc = startUtc, endUtc = endUtc, liftId = liftId};
    }

    /// <summary>
    /// Creates CCAColorPaletteRequest class sample instance to be displayed in Help documantation.
    /// </summary>
    /// 
    public new static CCAColorPaletteRequest HelpSample
    {
      get { return new CCAColorPaletteRequest()
        {
          projectId = 111,
          assetId = 1,
          startUtc = DateTime.UtcNow,
          endUtc = DateTime.UtcNow.AddDays(5),
          liftId = 1
        };
      }
    }

    /// <summary>
    /// Validates properties.
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (startUtc.HasValue || endUtc.HasValue)
      {
        if (startUtc.HasValue && endUtc.HasValue)
        {
          if (startUtc.Value > this.endUtc.Value)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                    "Start date must be earlier than end date!"));
          }
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "If using a date range both dates must be provided"));
        }
      }
    }

  }
}