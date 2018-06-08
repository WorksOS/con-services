namespace VSS.MasterData.Models.Utilities
{
  /// <summary>
  /// Utility class that defines constants used by data validation.
  /// </summary>
  public class ValidationConstants
  {
    /// <summary>
    /// Minimum station value.
    /// </summary>
    public const double MIN_STATION = -10000;

    /// <summary>
    /// Maximum station value.
    /// </summary>
    public const double MAX_STATION = 1000000;

    /// <summary>
    /// Minimum offset value.
    /// </summary>
    public const double MIN_OFFSET = -500;

    /// <summary>
    /// Maximum offset value.
    /// </summary>
    public const double MAX_OFFSET = 500;

    /// <summary>
    /// Minimum elevation value.
    /// </summary>
    public const double MIN_ELEVATION = -10000;

    /// <summary>
    /// Maximum elevation value.
    /// </summary>
    public const double MAX_ELEVATION = 10000;

    /// <summary>
    /// Minimum production data layer number.
    /// </summary>
    public const int MIN_LAYER_NUMBER = -1000;

    /// <summary>
    /// Maximum production data layer number.
    /// </summary>
    public const int MAX_LAYER_NUMBER = 1000;

    /// <summary>
    /// Maximum production data thickness.
    /// </summary>
    public const double MAX_THICKNESS = 100;

    /// <summary>
    /// Minimum production data thickness.
    /// </summary>
    public const double MIN_THICKNESS = 0.005;

    /// <summary>
    /// Minimum production data no change tolerance for volumes.
    /// </summary>
    public const double MIN_NO_CHANGE_TOLERANCE = 0.0;

    /// <summary>
    /// Maximum production data no change tolerance for volumes.
    /// </summary>
    public const double MAX_NO_CHANGE_TOLERANCE = 0.1;

    /// <summary>
    /// Minimum temperature range in °C for filter
    /// </summary>
    public const double MIN_TEMPERATURE = 0;

    /// <summary>
    /// Maximum temperature range in °C for filter
    /// Note: This is because Raptor 'no temperature' value in 10ths is 4096
    /// </summary>
    public const double MAX_TEMPERATURE = 409.5;

    /// <summary>
    /// Minimum pass count value for filter
    /// </summary>
    public const int MIN_PASS_COUNT = 0;

    /// <summary>
    /// Maximum pass count value for filter. Note: Raptor has this hard coded.
    /// </summary>
    public const int MAX_PASS_COUNT = 1000;
  }
}
