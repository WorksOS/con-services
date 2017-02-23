using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Validation;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.Models
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
    /// Create example instance of ProductionDataEdit to display in Help documentation.
    /// </summary>
    public static ProductionDataEdit HelpSample
    {
      get
      {
        return new ProductionDataEdit()
               {
                   assetId = 8265735274,
                   startUTC = DateTime.UtcNow.AddDays(-5),
                   endUTC = DateTime.UtcNow.AddDays(-3),
                   onMachineDesignName = "Bob Excavator",
                   liftNumber = null
               };
      }
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
                  string.Format("Invalid override date range")));
      }

      if (string.IsNullOrEmpty(onMachineDesignName) && !liftNumber.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  string.Format("Nothing to edit")));
      }
    }
  }
}