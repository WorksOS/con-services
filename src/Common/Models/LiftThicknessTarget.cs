
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.Common.Models
{
  /// <summary>
  /// Provides settings for thickness summary requests. Target and tolerance should be specified in meters.
  /// </summary>
  public class LiftThicknessTarget : IValidatable
  {
    /// <summary>
    /// Gets or sets the target lift thickness (in meters). This is only used with TargetThicknessSummary diplay mode and summary report.
    /// </summary>
    /// <value>
    /// The target lift thickness.
    /// </value>
    [JsonProperty(PropertyName = "TargetLiftThickness", Required = Required.Always)]
    [Required]
    public float TargetLiftThickness { get; set; }

    /// <summary>
    /// Gets or sets the above tolerance lift thickness (in meters). This value represents the thickness tolerance above the target lift thickness which is considered to meet the lift thickness target. For example, if the lift thickess is permitted to be up to 20mm thicker than the target thickness, then set this value to 0.02. This is only used with TargetThicknessSummary display mode and summary report
    /// </summary>
    /// <value>
    /// The above tolerance lift thickness.
    /// </value>
    [JsonProperty(PropertyName = "AboveToleranceLiftThickness", Required = Required.Always)]
    [Required]
    public float AboveToleranceLiftThickness { get; set; }

    /// <summary>
    /// Gets or sets the below tolerance lift thickness (in meters). This value represents the thickness tolerance below the target lift thickness which is considered to meet the lift thickness target. For example, if the lift thickess is permitted to be up to 20mm thinner than the target thickness, then set this value to 0.02. This is only used with TargetThicknessSummary display mode and summary report
    /// </summary>
    /// <value>
    /// The below tolerance lift thickness.
    /// </value>
    [JsonProperty(PropertyName = "BelowToleranceLiftThickness", Required = Required.Always)]
    [Required]
    public float BelowToleranceLiftThickness { get; set; }

    public static LiftThicknessTarget HelpSample {
      get
      {
        return new LiftThicknessTarget()
               {
                   AboveToleranceLiftThickness = (float)0.001,
                   BelowToleranceLiftThickness = (float)0.002,
                   TargetLiftThickness = (float)0.05
               };
      }
    }

    /// <summary>
    /// Private constructor
    /// </summary>
    private LiftThicknessTarget()
    { }

    public void Validate()
    {
      if ((TargetLiftThickness <= 0) || (TargetLiftThickness - BelowToleranceLiftThickness <0) || (BelowToleranceLiftThickness <0) || (AboveToleranceLiftThickness<0))
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Targte thickness settings must be positive."));      


    }
  }
}
