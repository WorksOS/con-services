using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Validation;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  /// <summary>
  /// A representation of an edit applied to production data.
  /// </summary>
  public class ProductionDataEdit 
  {
    /// <summary>
    /// The id of the machine whose data is overridden. Required.
    /// </summary>
    [ValidAssetID]
    [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
    [Required]
    public long assetId { get; private set; }

    /// <summary>
    /// Start of the period with overridden data. Required.
    /// </summary>
    [JsonProperty(PropertyName = "startUTC", Required = Required.Always)]
    [Required]
    public DateTime startUTC { get; private set; }

    /// <summary>
    /// End of the period with overridden data. Required.
    /// </summary>
    [JsonProperty(PropertyName = "endUTC", Required = Required.Always)]
    [Required]
    public DateTime endUTC { get; private set; }

  
    /// <summary>
    /// The design name used for the specified override period. May be null.
    /// </summary>
    [JsonProperty(PropertyName = "onMachineDesignName", Required = Required.Default)]
    public string onMachineDesignName { get; private set; }

    /// <summary>
    /// The lift number used for the specified override period. May be null.
    /// </summary>
    [JsonProperty(PropertyName = "liftNumber", Required = Required.Default)]
    public int? liftNumber { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private ProductionDataEdit()
    {
    }



    /// <summary>
    /// Create instance of ProductionDataEdit
    /// </summary>
    public static ProductionDataEdit CreateProductionDataEdit(
      long assetId,
      DateTime startUTC,
      DateTime endUTC,
      string onMachineDesignName,
      int? liftNumber
      )
    {
      return new ProductionDataEdit
             {
                 assetId = assetId,
                 startUTC = startUTC,
                 endUTC = endUTC,
                 onMachineDesignName = onMachineDesignName,
                 liftNumber = liftNumber
             };
    }
    
    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (startUTC == DateTime.MinValue || endUTC == DateTime.MinValue || startUTC >= endUTC)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "Invalid override date range"));
      }

      if (string.IsNullOrEmpty(onMachineDesignName) && !liftNumber.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "Nothing to edit"));
      }
    }
  }
}