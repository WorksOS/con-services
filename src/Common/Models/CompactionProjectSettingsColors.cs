using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// 3D Productivity project settings colors.
  /// </summary>
  public class CompactionProjectSettingsColors : IValidatable
  {
    #region Properties

    #region Elevation

    /// <summary>
    /// Flag to determine if default or custom Elevation colour values are used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultElevationColors", Required = Required.Default)]
    public bool? useDefaultElevationColors { get; private set; } = true;

    /// <summary>
    /// The Elevation colour values when overriding the default ones.
    /// There must be 31 values.
    /// </summary>
    [JsonProperty(PropertyName = "elevationColors", Required = Required.Default)]
    public List<uint> elevationColors { get; private set; }
    #endregion

    #region CMV

    /// <summary>
    /// Flag to determine if default or custom CMV details colour values are used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultCMVDetailsColors", Required = Required.Default)]
    public bool? useDefaultCMVDetailsColors { get; private set; } = true;

    /// <summary>
    /// The CMV details colour values when overriding the default ones.
    /// There must be 16 values.
    /// </summary>
    [JsonProperty(PropertyName = "cmvDetailsColors", Required = Required.Default)]
    public List<uint> cmvDetailsColors { get; private set; }

    /// <summary>
    /// Flag to determine if default or custom CMV summary colour values are used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultCMVSummaryColors", Required = Required.Default)]
    public bool? useDefaultCMVSummaryColors { get; private set; } = true;

    /// <summary>
    /// The colour value when the reported CMV value is on target.
    /// </summary>
    [JsonProperty(PropertyName = "cmvOnTargetColor", Required = Required.Default)]
    public uint? cmvOnTargetColor { get; private set; }

    /// <summary>
    /// The colour value when the reported CMV value is over target.
    /// </summary>
    [JsonProperty(PropertyName = "cmvOverTargetColor", Required = Required.Default)]
    public uint? cmvOverTargetColor { get; private set; }

    /// <summary>
    /// The colour value when the reported speed value is under target.
    /// </summary>
    [JsonProperty(PropertyName = "cmvUnderTargetColor", Required = Required.Default)]
    public uint? cmvUnderTargetColor { get; private set; }

    /// <summary>
    /// Flag to determine if default or custom CMV percent change colour values are used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultCMVPercentColors", Required = Required.Default)]
    public bool? useDefaultCMVPercentColors { get; private set; } = true;

    /// <summary>
    /// The CMV percent colour values when overriding the default ones.
    /// There must be 8 values.
    /// </summary>
    [JsonProperty(PropertyName = "cmvPercentColors", Required = Required.Default)]
    public List<uint> cmvPercentColors { get; private set; }
    #endregion

    #region Pass Count

    /// <summary>
    /// Flag to determine if default or custom Pass Count details colour values are used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultPassCountDetailsColors", Required = Required.Default)]
    public bool? useDefaultPassCountDetailsColors { get; private set; } = true;

    /// <summary>
    /// The Pass Count details colour values when overriding the default ones.
    /// There must be 9 values.
    /// </summary>
    [JsonProperty(PropertyName = "passCountDetailsColors", Required = Required.Default)]
    public List<uint> passCountDetailsColors { get; private set; }

    /// <summary>
    /// Flag to determine if default or custom Pass Count summary colour values are used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultPassCountSummaryColors", Required = Required.Default)]
    public bool? useDefaultPassCountSummaryColors { get; private set; } = true;

    /// <summary>
    /// The colour value when the reported pass count value is on target.
    /// </summary>
    [JsonProperty(PropertyName = "passCountOnTargetColor", Required = Required.Default)]
    public uint? passCountOnTargetColor { get; private set; }

    /// <summary>
    /// The colour value when the reported pass count value is over target.
    /// </summary>
    [JsonProperty(PropertyName = "passCountOverTargetColor", Required = Required.Default)]
    public uint? passCountOverTargetColor { get; private set; }

    /// <summary>
    /// The colour value when the reported pass count value is under target.
    /// </summary>
    [JsonProperty(PropertyName = "passCountUnderTargetColor", Required = Required.Default)]
    public uint? passCountUnderTargetColor { get; private set; }
    #endregion

    #region Cut/Fill

    /// <summary>
    /// Flag to determine if default or custom Cut/Fill colour values are used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultCutFillColors", Required = Required.Default)]
    public bool? useDefaultCutFillColors { get; private set; } = true;

    /// <summary>
    /// The Cut/Fill colour values when overriding the default ones.
    /// There must be 7 values (3 cut, on grade value and 3 fill).
    /// </summary>
    [JsonProperty(PropertyName = "cutFillColors", Required = Required.Default)]
    public List<uint> cutFillColors { get; private set; }
    #endregion

    #region Temperature

    /// <summary>
    /// Flag to determine if default or custom Temperature summary colour values are used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultTemperatureSummaryColors", Required = Required.Default)]
    public bool? useDefaultTemperatureSummaryColors { get; private set; } = true;

    /// <summary>
    /// The colour value when the reported temperature value is on target.
    /// </summary>
    [JsonProperty(PropertyName = "temperatureOnTargetColor", Required = Required.Default)]
    public uint? temperatureOnTargetColor { get; private set; }

    /// <summary>
    /// The colour value when the reported temperature value is over target.
    /// </summary>
    [JsonProperty(PropertyName = "temperatureOverTargetColor", Required = Required.Default)]
    public uint? temperatureOverTargetColor { get; private set; }

    /// <summary>
    /// The colour value when the reported temperature value is under target.
    /// </summary>
    [JsonProperty(PropertyName = "temperatureUnderTargetColor", Required = Required.Default)]
    public uint? temperatureUnderTargetColor { get; private set; }
    #endregion

    #region Speed

    /// <summary>
    /// Flag to determine if default or custom Speed summary colour values are used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultSpeedSummaryColors", Required = Required.Default)]
    public bool? useDefaultSpeedSummaryColors { get; private set; } = true;

    /// <summary>
    /// The colour value when the reported speed value is on target.
    /// </summary>
    [JsonProperty(PropertyName = "speedOnTargetColor", Required = Required.Default)]
    public uint? speedOnTargetColor { get; private set; }

    /// <summary>
    /// The colour value when the reported speed value is over target.
    /// </summary>
    [JsonProperty(PropertyName = "speedOverTargetColor", Required = Required.Default)]
    public uint? speedOverTargetColor { get; private set; }

    /// <summary>
    /// The colour value when the reported speed value is under target.
    /// </summary>
    [JsonProperty(PropertyName = "speedUnderTargetColor", Required = Required.Default)]
    public uint? speedUnderTargetColor { get; private set; }
    #endregion

    #region MDP

    /// <summary>
    /// Flag to determine if default or custom MDP summary colour values are used. Default is true.
    /// </summary>
    [JsonProperty(PropertyName = "useDefaultMDPSummaryColors", Required = Required.Default)]
    public bool? useDefaultMDPSummaryColors { get; private set; } = true;

    /// <summary>
    /// The colour value when the reported MDP value is on target.
    /// </summary>
    [JsonProperty(PropertyName = "mdpOnTargetColor", Required = Required.Default)]
    public uint? mdpOnTargetColor { get; private set; }

    /// <summary>
    /// The colour value when the reported MDP value is over target.
    /// </summary>
    [JsonProperty(PropertyName = "mdpOverTargetColor", Required = Required.Default)]
    public uint? mdpOverTargetColor { get; private set; }

    /// <summary>
    /// The colour value when the reported speed value is under target.
    /// </summary>
    [JsonProperty(PropertyName = "mdpUnderTargetColor", Required = Required.Default)]
    public uint? mdpUnderTargetColor { get; private set; }
    #endregion

    #endregion

    #region Construction
    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionProjectSettingsColors()
    {
    }

    /// <summary>
    /// Creates an instance of the CompactionProjectSettingsColors class.
    /// </summary>
    /// <returns>The instance of the CompactionProjectSettingsColors class.</returns>
    public static CompactionProjectSettingsColors Create
    (
      bool? useDefaultElevationColors = true,
      List<uint> elevationColors = null,
      bool? useDefaultCMVDetailsColors = true,
      List<uint> cmvDetailsColors = null,
      bool? useDefaultCMVSummaryColors = true,
      uint? cmvOnTargetColor = null,
      uint? cmvOverTargetColor = null,
      uint? cmvUnderTargetColor = null,
      bool? useDefaultCMVPercentColors = true,
      List<uint> cmvPercentColors = null,
      bool? useDefaultPassCountDetailsColors = true,
      List<uint> passCountDetailsColors = null,
      bool? useDefaultPassCountSummaryColors = true,
      uint? passCountOnTargetColor = null,
      uint? passCountOverTargetColor = null,
      uint? passCountUnderTargetColor = null,
      bool? useDefaultCutFillColors = true,
      List<uint> cutFillColors = null,
      bool? useDefaultTemperatureSummaryColors = true,
      uint? temperatureOnTargetColor = null,
      uint? temperatureOverTargetColor = null,
      uint? temperatureUnderTargetColor = null,
      bool? useDefaultSpeedSummaryColors = true,
      uint? speedOnTargetColor = null,
      uint? speedOverTargetColor = null,
      uint? speedUnderTargetColor = null,
      bool? useDefaultMDPSummaryColors = true,
      uint? mdpOnTargetColor = null,
      uint? mdpOverTargetColor = null,
      uint? mdpUnderTargetColor = null
    )
    {
      return new CompactionProjectSettingsColors
      {
        useDefaultElevationColors = useDefaultElevationColors,
        elevationColors = elevationColors,
        useDefaultCMVDetailsColors = useDefaultCMVDetailsColors,
        cmvDetailsColors = cmvDetailsColors,
        useDefaultCMVSummaryColors = useDefaultCMVSummaryColors,
        cmvOnTargetColor = cmvOnTargetColor,
        cmvOverTargetColor = cmvOverTargetColor,
        cmvUnderTargetColor = cmvUnderTargetColor,
        useDefaultCMVPercentColors = useDefaultCMVPercentColors,
        cmvPercentColors = cmvPercentColors,
        useDefaultPassCountDetailsColors = useDefaultPassCountDetailsColors,
        passCountDetailsColors = passCountDetailsColors,
        useDefaultPassCountSummaryColors = useDefaultPassCountSummaryColors,
        passCountOnTargetColor = passCountOnTargetColor,
        passCountOverTargetColor = passCountOverTargetColor,
        passCountUnderTargetColor = passCountUnderTargetColor,
        useDefaultCutFillColors = useDefaultCutFillColors,
        cutFillColors = cutFillColors,
        useDefaultTemperatureSummaryColors = useDefaultTemperatureSummaryColors,
        temperatureOnTargetColor = temperatureOnTargetColor,
        temperatureOverTargetColor = temperatureOverTargetColor,
        temperatureUnderTargetColor = temperatureUnderTargetColor,
        useDefaultSpeedSummaryColors = useDefaultSpeedSummaryColors,
        speedOnTargetColor = speedOnTargetColor,
        speedOverTargetColor = speedOverTargetColor,
        speedUnderTargetColor = speedUnderTargetColor,
        useDefaultMDPSummaryColors = useDefaultMDPSummaryColors,
        mdpOnTargetColor = mdpOnTargetColor,
        mdpOverTargetColor = mdpOverTargetColor,
        mdpUnderTargetColor = mdpUnderTargetColor
      };
    }

    public static readonly CompactionProjectSettingsColors DefaultSettings = new CompactionProjectSettingsColors()
    {
      useDefaultElevationColors = true,
      elevationColors = ElevationPalette,
      useDefaultCMVDetailsColors = true,
      cmvDetailsColors = CMVDetailsPalette,
      useDefaultCMVSummaryColors = true,
      cmvOnTargetColor = ON_COLOR,
      cmvOverTargetColor = OVER_COLOR,
      cmvUnderTargetColor = UNDER_COLOR,
      useDefaultCMVPercentColors = true,
      cmvPercentColors = CMVPercentChangePalette,
      useDefaultPassCountDetailsColors = true,
      passCountDetailsColors = PassCountDetailsPalette,
      useDefaultPassCountSummaryColors = true,
      passCountOnTargetColor = ON_COLOR,
      passCountOverTargetColor = OVER_COLOR,
      passCountUnderTargetColor = UNDER_COLOR,
      useDefaultCutFillColors = true,
      cutFillColors = CutFillPalette,
      useDefaultTemperatureSummaryColors = true,
      temperatureOnTargetColor = ON_COLOR,
      temperatureOverTargetColor = OVER_COLOR,
      temperatureUnderTargetColor = UNDER_COLOR,
      useDefaultSpeedSummaryColors = true,
      speedOnTargetColor = ON_COLOR,
      speedOverTargetColor = OVER_COLOR,
      speedUnderTargetColor = UNDER_COLOR,
      useDefaultMDPSummaryColors = true,
      mdpOnTargetColor = ON_COLOR,
      mdpOverTargetColor = OVER_COLOR,
      mdpUnderTargetColor = UNDER_COLOR
    };

    #endregion

    #region Validation
    /// <summary>
    /// Validates properties...
    /// </summary>
    public void Validate()
    {
      var validator = new DataAnnotationsValidator();
      validator.TryValidate(this, out ICollection<ValidationResult> results);
      if (results.Any())
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, results.FirstOrDefault().ErrorMessage));
      }

      // Elevation...
      ValidateColorValuesList(elevationColors, nameof(elevationColors), useDefaultElevationColors, NUMBER_OF_ELEVATION_COLORS);
      
      // CMV..
      ValidateColorValuesList(cmvDetailsColors, nameof(cmvDetailsColors), useDefaultCMVDetailsColors, NUMBER_OF_CMV_DETAILS_COLORS);
      ValidateColorValue(cmvOnTargetColor, nameof(cmvOnTargetColor), useDefaultCMVSummaryColors);
      ValidateColorValue(cmvOverTargetColor, nameof(cmvOverTargetColor), useDefaultCMVSummaryColors);
      ValidateColorValue(cmvUnderTargetColor, nameof(cmvUnderTargetColor), useDefaultCMVSummaryColors);
      ValidateColorValuesList(cmvPercentColors, nameof(cmvPercentColors), useDefaultCMVPercentColors, NUMBER_OF_CMV_PERCENT_COLORS);

      // Pass Count...
      ValidateColorValuesList(passCountDetailsColors, nameof(passCountDetailsColors), useDefaultPassCountDetailsColors, NUMBER_OF_PASS_COUNT_DETAILS_COLORS);
      ValidateColorValue(passCountOnTargetColor, nameof(passCountOnTargetColor), useDefaultPassCountSummaryColors);
      ValidateColorValue(passCountOverTargetColor, nameof(passCountOverTargetColor), useDefaultPassCountSummaryColors);
      ValidateColorValue(passCountUnderTargetColor, nameof(passCountUnderTargetColor), useDefaultPassCountSummaryColors);

      // Cut/Fill...
      ValidateColorValuesList(cutFillColors, nameof(cutFillColors), useDefaultCutFillColors, NUMBER_OF_CUT_FILL_COLORS);

      // Temperature...
      ValidateColorValue(temperatureOnTargetColor, nameof(temperatureOnTargetColor), useDefaultTemperatureSummaryColors);
      ValidateColorValue(temperatureOverTargetColor, nameof(temperatureOverTargetColor), useDefaultTemperatureSummaryColors);
      ValidateColorValue(temperatureUnderTargetColor, nameof(temperatureUnderTargetColor), useDefaultTemperatureSummaryColors);

      // Speed...
      ValidateColorValue(speedOnTargetColor, nameof(speedOnTargetColor), useDefaultSpeedSummaryColors);
      ValidateColorValue(speedOverTargetColor, nameof(speedOverTargetColor), useDefaultSpeedSummaryColors);
      ValidateColorValue(speedUnderTargetColor, nameof(speedUnderTargetColor), useDefaultSpeedSummaryColors);

      // MDP...
      ValidateColorValue(mdpOnTargetColor, nameof(mdpOnTargetColor), useDefaultMDPSummaryColors);
      ValidateColorValue(mdpOverTargetColor, nameof(mdpOverTargetColor), useDefaultMDPSummaryColors);
      ValidateColorValue(mdpUnderTargetColor, nameof(mdpUnderTargetColor), useDefaultMDPSummaryColors);
    }

    private void ValidateColorValue(uint? colorValue, string what, bool? useDefaultValue)
    {
      if (useDefaultValue.HasValue && !useDefaultValue.Value)
      {
        if (!colorValue.HasValue)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              $"{what} colour values must be specified"));
        }
      }
    }

    private void ValidateColorValuesList(List<uint> colorValuesList, string what, bool? useDefaultValue, byte listLength)
    {
      if (useDefaultValue.HasValue && !useDefaultValue.Value)
      {
        if (colorValuesList == null || colorValuesList.Count != listLength)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              $"{what} list should contain {listLength} element(s)"));
        }
      }
    }
    #endregion

    #region Getters
    private static uint RGBToColor(byte r, byte g, byte b)
    {
      return (uint)r << 16 | (uint)g << 8 | (uint)b << 0;
    }

    private static List<uint> ElevationPalette => new List<uint> {
      RGBToColor(200,0,0),
      RGBToColor(255,0,0),
      RGBToColor(225,60,0),
      RGBToColor(255,90,0),
      RGBToColor(255,130,0),
      RGBToColor(255,170,0),
      RGBToColor(255,200,0),
      RGBToColor(255,220,0),
      RGBToColor(250,230,0),
      RGBToColor(220,230,0),
      RGBToColor(210,230,0),
      RGBToColor(200,230,0),
      RGBToColor(180,230,0),
      RGBToColor(150,230,0),
      RGBToColor(130,230,0),
      RGBToColor(100,240,0),
      RGBToColor(0,255,0),
      RGBToColor(0,240,100),
      RGBToColor(0,230,130),
      RGBToColor(0,230,150),
      RGBToColor(0,230,180),
      RGBToColor(0,230,200),
      RGBToColor(0,230,210),
      RGBToColor(0,220,220),
      RGBToColor(0,200,230),
      RGBToColor(0,180,240),
      RGBToColor(0,150,245),
      RGBToColor(0,120,250),
      RGBToColor(0,90,255),
      RGBToColor(0,70,255),
      RGBToColor(0,0,255)
    };

    private static List<uint> CMVDetailsPalette => new List<uint>()
    {
      // Decimal values: 87963, 9423080, 6594104, 15628926, 13959168
      0x01579B, // 87963 (0)
      0x8FC8E8, // 9423080 (40)
      0x649E38, // 6594104 (80)
      0xEE7A7E, // 15628926 (120)
      0xD50000  // 13959168 (150)
    };

    private static List<uint> PassCountDetailsPalette => new List<uint>()
    {
      0x2D5783,
      0x439BDC,
      0xBEDFF1,
      0x9DCE67,
      0x6BA03E,
      0x3A6B25,
      0xF6CED3,
      0xD57A7C,
      0xC13037
    };

    private static List<uint> CutFillPalette => new List<uint>
    {
      0xD50000,
      0xE57373,
      0xFFCDD2,
      0x8BC34A,
      0xB3E5FC,
      0x039BE5,
      0x01579B
    };

    private static List<uint> CMVPercentChangePalette => new List<uint>
    {
      0x01579B,
      0x039BE5,
      0x4FC3F7,
      0xB3E5FC,
      0x8BC34A,
      0xFFCDD2,
      0xE57373,
      0xD50000
    };
    #endregion

    private const uint OVER_COLOR = 0xD50000;
    private const uint ON_COLOR = 0x8BC34A;
    private const uint UNDER_COLOR = 0x1579B;

    private const int NUMBER_OF_ELEVATION_COLORS = 31;
    private const int NUMBER_OF_CMV_DETAILS_COLORS = 5;
    private const int NUMBER_OF_CMV_PERCENT_COLORS = 8;
    private const int NUMBER_OF_PASS_COUNT_DETAILS_COLORS = 9;
    private const int NUMBER_OF_CUT_FILL_COLORS = 7;
  }
}
