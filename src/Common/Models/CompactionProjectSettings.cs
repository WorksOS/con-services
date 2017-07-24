using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// 3D Productivity project settings. Used in Raptor calculations.
  /// </summary>
  public class CompactionProjectSettings : IValidatable
  {
    /// <summary>
    /// Flag to determine if machine target pass count or custom target range is used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useMachineTargetPassCount", Required = Required.Default)]
    public bool? useMachineTargetPassCount { get; private set; }
    /// <summary>
    /// The minimum target pass count when overriding the machine target value
    /// </summary>
    [Range(MIN_PASS_COUNT, MAX_PASS_COUNT)]
    [JsonProperty(PropertyName = "customTargetPassCountMinimum", Required = Required.Default)]
    public int? customTargetPassCountMinimum { get; private set; }
    /// <summary>
    /// The maximum target pass count when overriding the machine target value
    /// </summary>
    [Range(MIN_PASS_COUNT, MAX_PASS_COUNT)]
    [JsonProperty(PropertyName = "customTargetPassCountMaximum", Required = Required.Default)]
    public int? customTargetPassCountMaximum { get; private set; }
    /// <summary>
    /// Flag to determine if machine target temperature or custom target range is used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useMachineTargetTemperature", Required = Required.Default)]
    public bool? useMachineTargetTemperature { get; private set; }
    /// <summary>
    /// The minimum target temperature (°C) when overriding the machine target value
    /// </summary>
    [Range(MIN_TEMPERATURE, MAX_TEMPERATURE)]
    [JsonProperty(PropertyName = "customTargetTemperatureMinimum", Required = Required.Default)]
    public double? customTargetTemperatureMinimum { get; private set; }
    /// <summary>
    /// The maximum target temperature (°C) when overriding the machine target value
    /// </summary>
    [Range(MIN_TEMPERATURE, MAX_TEMPERATURE)]
    [JsonProperty(PropertyName = "customTargetTemperatureMaximum", Required = Required.Default)]
    public double? customTargetTemperatureMaximum { get; private set; }
    /// <summary>
    /// Flag to determine if machine target CMV or custom target value is used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useMachineTargetCmv", Required = Required.Default)]
    public bool? useMachineTargetCmv { get; private set; }
    /// <summary>
    /// The target CMV value when overriding the machine target value
    /// </summary>
    [Range(MIN_RAW_CMV, MAX_RAW_CMV)]
    [JsonProperty(PropertyName = "customTargetCmv", Required = Required.Default)]
    public double? customTargetCmv { get; private set; }
    /// <summary>
    /// Flag to determine if machine target MDP or custom target value is used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useMachineTargetMdp", Required = Required.Default)]
    public bool? useMachineTargetMdp { get; private set; }
    /// <summary>
    /// The target MDP value when overriding the machine target value
    /// </summary>
    [Range(MIN_RAW_MDP, MAX_RAW_MDP)]
    [JsonProperty(PropertyName = "customTargetMdp", Required = Required.Default)]
    public double? customTargetMdp { get; private set; }
    /// <summary>
    /// Flag to determine if the default CMV % range or custom target range is used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultTargetRangeCmvPercent", Required = Required.Default)]
    public bool? useDefaultTargetRangeCmvPercent { get; private set; }
    /// <summary>
    /// The minimum target CMV % when overriding the default target range
    /// </summary>
    [Range(MIN_CMV_PERCENT, MAX_CMV_PERCENT)]
    [JsonProperty(PropertyName = "customTargetCmvPercentMinimum", Required = Required.Default)]
    public double? customTargetCmvPercentMinimum { get; private set; }
    /// <summary>
    /// The maximum target CMV % when overriding the default target range
    /// </summary>
    [Range(MIN_CMV_PERCENT, MAX_CMV_PERCENT)]
    [JsonProperty(PropertyName = "customTargetCmvPercentMaximum", Required = Required.Default)]
    public double? customTargetCmvPercentMaximum { get; private set; }
    /// <summary>
    /// Flag to determine if the default MDP % range or custom target range is used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultTargetRangeMdpPercent", Required = Required.Default)]
    public bool? useDefaultTargetRangeMdpPercent { get; private set; }
    /// <summary>
    /// The minimum target MDP % when overriding the default target range
    /// </summary>
    [Range(MIN_MDP_PERCENT, MAX_MDP_PERCENT)]
    [JsonProperty(PropertyName = "customTargetMdpPercentMinimum", Required = Required.Default)]
    public double? customTargetMdpPercentMinimum { get; private set; }
    /// <summary>
    /// The maximum target MDP % when overriding the default target range
    /// </summary>
    [Range(MIN_MDP_PERCENT, MAX_MDP_PERCENT)]
    [JsonProperty(PropertyName = "customTargetMdpPercentMaximum", Required = Required.Default)]
    public double? customTargetMdpPercentMaximum { get; private set; }
    /// <summary>
    /// Flag to determine if the default speed range or custom target range is used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultTargetRangeSpeed", Required = Required.Default)]
    public bool? useDefaultTargetRangeSpeed { get; private set; }
    /// <summary>
    /// The minimum target speed (km/h)  when overriding the default target range
    /// </summary>
    [Range(MIN_SPEED, MAX_SPEED)]
    [JsonProperty(PropertyName = "customTargetSpeedMinimum", Required = Required.Default)]
    public double? customTargetSpeedMinimum { get; private set; }
    /// <summary>
    /// The maximum target speed (km/h) when overriding the default target range
    /// </summary>
    [Range(MIN_SPEED, MAX_SPEED)]
    [JsonProperty(PropertyName = "customTargetSpeedMaximum", Required = Required.Default)]
    public double? customTargetSpeedMaximum { get; private set; }
    /// <summary>
    /// Flag to determine if default cut-fill tolerances or custom tolerances are used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultCutFillTolerances", Required = Required.Default)]
    public bool? useDefaultCutFillTolerances { get; private set; }
    /// <summary>
    /// The collection of custom cut-fill tolerances (m) when overriding the defaults. Values are in descending order, highest cut to lowest fill.
    /// </summary>
    [JsonProperty(PropertyName = "customCutFillTolerances", Required = Required.Default)]
    public List<double> customCutFillTolerances { get; private set; }
    /// <summary>
    /// Flag to determine if default shrinkage % and bulking % or custom values are used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultVolumeShrinkageBulking", Required = Required.Default)]
    public bool? useDefaultVolumeShrinkageBulking { get; private set; }
    /// <summary>
    /// The shrinkage % when overriding the default value.
    /// </summary>
    [Range(MIN_SHRINKAGE, MAX_SHRINKAGE)]
    [JsonProperty(PropertyName = "customShrinkagePercent", Required = Required.Default)]
    public double? customShrinkagePercent { get; private set; }
    /// <summary>
    /// The bulking % when overriding the default value.
    /// </summary>
    [Range(MIN_BULKING, MAX_BULKING)]
    [JsonProperty(PropertyName = "customBulkingPercent", Required = Required.Default)]
    public double? customBulkingPercent { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionProjectSettings()
    {
    }

    /// <summary>
    /// Create instance of CompactionProjectSettings
    /// </summary>
    public static CompactionProjectSettings CreateProjectSettings
      (
      bool? useMachineTargetPassCount,
      int? customTargetPassCountMinimum,
      int? customTargetPassCountMaximum,
      bool? useMachineTargetTemperature,
      double? customTargetTemperatureMinimum,
      double? customTargetTemperatureMaximum,
      bool? useMachineTargetCmv,
      double? customTargetCmv,
      bool? useMachineTargetMdp,
      double? customTargetMdp,
      bool? useDefaultTargetRangeCmvPercent,
      double? customTargetCmvPercentMinimum,
      double? customTargetCmvPercentMaximum,
      bool? useDefaultTargetRangeMdpPercent,
      double? customTargetMdpPercentMinimum,
      double? customTargetMdpPercentMaximum,
      bool? useDefaultTargetRangeSpeed,
      double? customTargetSpeedMinimum,
      double? customTargetSpeedMaximum,
      bool? useDefaultCutFillTolerances,
      List<double> customCutFillTolerances,
      bool? useDefaultVolumeShrinkageBulking,
      double? customShrinkagePercent,
      double? customBulkingPercent
      )
   
    {
      return new CompactionProjectSettings
      {
        useMachineTargetPassCount = useMachineTargetPassCount,
        customTargetPassCountMinimum = customTargetPassCountMinimum,
        customTargetPassCountMaximum = customTargetPassCountMaximum,
        useMachineTargetTemperature = useMachineTargetTemperature,
        customTargetTemperatureMinimum = customTargetTemperatureMinimum,
        customTargetTemperatureMaximum = customTargetTemperatureMaximum,
        useMachineTargetCmv = useMachineTargetCmv,
        customTargetCmv = customTargetCmv,
        useMachineTargetMdp = useMachineTargetMdp,
        customTargetMdp = customTargetMdp,
        useDefaultTargetRangeCmvPercent = useDefaultTargetRangeCmvPercent,
        customTargetCmvPercentMinimum = customTargetCmvPercentMinimum,
        customTargetCmvPercentMaximum = customTargetCmvPercentMaximum,
        useDefaultTargetRangeMdpPercent = useDefaultTargetRangeMdpPercent,
        customTargetMdpPercentMinimum = customTargetMdpPercentMinimum,
        customTargetMdpPercentMaximum = customTargetMdpPercentMaximum,
        useDefaultTargetRangeSpeed = useDefaultTargetRangeSpeed,
        customTargetSpeedMinimum = customTargetSpeedMinimum,
        customTargetSpeedMaximum = customTargetSpeedMaximum,
        useDefaultCutFillTolerances = useDefaultCutFillTolerances,
        customCutFillTolerances = customCutFillTolerances,
        useDefaultVolumeShrinkageBulking = useDefaultVolumeShrinkageBulking,
        customShrinkagePercent = customShrinkagePercent,
        customBulkingPercent = customBulkingPercent
      };
    }

    public static readonly CompactionProjectSettings DefaultSettings =
        new CompactionProjectSettings
        {
          useMachineTargetPassCount = true,
          customTargetPassCountMinimum = 6,
          customTargetPassCountMaximum = 6,
          useMachineTargetTemperature = true,
          customTargetTemperatureMinimum = 65.0,
          customTargetTemperatureMaximum = 175.0,
          useMachineTargetCmv = true,
          customTargetCmv = 70,
          useMachineTargetMdp = true,
          customTargetMdp = 70,
          useDefaultTargetRangeCmvPercent = true,
          customTargetCmvPercentMinimum = 80.0,
          customTargetCmvPercentMaximum = 130.0,
          useDefaultTargetRangeMdpPercent = true,
          customTargetMdpPercentMinimum = 80.0,
          customTargetMdpPercentMaximum = 130.0,
          useDefaultTargetRangeSpeed = true,
          customTargetSpeedMinimum = 5.0,
          customTargetSpeedMaximum = 10.0,
          useDefaultCutFillTolerances = true,
          customCutFillTolerances = new List<double> {0.2, 0.1, 0.05, 0, -0.05, -0.1, -0.2},
          useDefaultVolumeShrinkageBulking = true,
          customShrinkagePercent = 0.0,
          customBulkingPercent = 0.0
        };
      

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      var validator = new DataAnnotationsValidator();
      ICollection<ValidationResult> results;
      validator.TryValidate(this, out results);
      if (results.Any())
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, results.FirstOrDefault().ErrorMessage));
      }

      ValidateRange(useMachineTargetPassCount, customTargetPassCountMinimum, customTargetPassCountMaximum, "pass count");
      ValidateRange(useMachineTargetTemperature, customTargetTemperatureMinimum, customTargetTemperatureMaximum, "temperature");
      ValidateRange(useDefaultTargetRangeCmvPercent, customTargetCmvPercentMinimum, customTargetCmvPercentMaximum, "CMV %");
      ValidateRange(useDefaultTargetRangeMdpPercent, customTargetMdpPercentMinimum, customTargetMdpPercentMaximum, "MDP %");
      ValidateRange(useDefaultTargetRangeSpeed, customTargetSpeedMinimum, customTargetSpeedMaximum, "Speed");

      ValidateValue(useMachineTargetCmv, customTargetCmv, "CMV");
      ValidateValue(useMachineTargetMdp, customTargetMdp, "MDP");
      ValidateValue(useDefaultVolumeShrinkageBulking, customBulkingPercent, "bulking %");
      ValidateValue(useDefaultVolumeShrinkageBulking, customShrinkagePercent, "shrinkage %");

      ValidateCutFill();
    }

    /// <summary>
    /// Validates cut-fill values
    /// </summary>
    private void ValidateCutFill()
    {
      if (useDefaultCutFillTolerances.HasValue && !useDefaultCutFillTolerances.Value)
      {
        const int CUT_FILL_TOTAL = 7;
        if (customCutFillTolerances == null || customCutFillTolerances.Count != CUT_FILL_TOTAL)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              $"Exactly {CUT_FILL_TOTAL} cut-fill tolerances must be specified"));
        }

        for (int i = 0; i < CUT_FILL_TOTAL; i++)
        {   
          if (customCutFillTolerances[i] < MIN_CUT_FILL || customCutFillTolerances[i] > MAX_CUT_FILL)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                $"Cut-fill tolerances must be between {MIN_CUT_FILL} and {MAX_CUT_FILL} meters"));
          }
        }

        for (int i = 1; i < CUT_FILL_TOTAL; i++)
        {
          if (customCutFillTolerances[i - 1] < customCutFillTolerances[i])
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Cut-fill tolerances must be in order of highest cut to lowest fill"));
          }
        }
      }
    }

    /// <summary>
    /// Validates a single target value
    /// </summary>
    /// <param name="useMachineTarget">Flag to indicate if machine/default target is used</param>
    /// <param name="customTargetValue">Custom target/default value</param>
    /// <param name="what">What is being validated for error message</param>
    private void ValidateValue(bool? useMachineTarget, double? customTargetValue, string what)
    {
      if (useMachineTarget.HasValue && !useMachineTarget.Value)
      {
        if (!customTargetValue.HasValue)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              $"Custom target {what} value is required"));
        }
      }
    }

    /// <summary>
    /// Validates a target range
    /// </summary>
    /// <param name="useMachineTarget">Flag to indicate if machine/default target is used</param>
    /// <param name="customTargetMinimum">Custom target/default minimum value</param>
    /// <param name="customTargetMaxiumum">Custom target/default maximum value</param>
    /// <param name="what">What is being validated for error message</param>
    private void ValidateRange(bool? useMachineTarget, double? customTargetMinimum, double? customTargetMaxiumum, string what)
    {
      if (useMachineTarget.HasValue && !useMachineTarget.Value)
      {
        if (!customTargetMinimum.HasValue || !customTargetMaxiumum.HasValue)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              $"Both minimum and maximum target {what} must be specified"));
        }
        if (customTargetMinimum.Value > customTargetMaxiumum.Value)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              $"Minimum target {what} must be less than maximum"));
        }
      }
    }

    private const double MIN_TEMPERATURE = 1.0; //°C
    private const double MAX_TEMPERATURE = 1000.0;

    private const int MIN_PASS_COUNT = 1;
    private const int MAX_PASS_COUNT = 80;

    private const double MIN_RAW_CMV = 1.0;
    private const double MAX_RAW_CMV = 999.0;

    private const double MIN_RAW_MDP = 1.0;
    private const double MAX_RAW_MDP = 999.0;

    private const double MIN_CMV_PERCENT = 1.0;
    private const double MAX_CMV_PERCENT = 250.0;

    private const double MIN_MDP_PERCENT = 1.0;
    private const double MAX_MDP_PERCENT = 250.0;

    private const double MIN_SPEED = 1.0;   //km/h
    private const double MAX_SPEED = 100.0;

    private const double MIN_SHRINKAGE = 1.0;   //%
    private const double MAX_SHRINKAGE = 100.0;

    private const double MIN_BULKING = 1.0;   //%
    private const double MAX_BULKING = 100.0;

    private const double MIN_CUT_FILL = -400.0; //m
    private const double MAX_CUT_FILL = 400.0;  


  }
}
