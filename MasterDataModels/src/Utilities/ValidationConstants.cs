namespace VSS.MasterData.Models.Utilities
{
  /// <summary>
  /// Utility class that defines constants used by data validation.
  /// </summary>
  public class ValidationConstants
  {
    /// <summary>
    /// Minimum production data layer number.
    /// </summary>
    public const int MIN_LAYER_NUMBER = -1000;

    /// <summary>
    /// Maximum production data layer number.
    /// </summary>
    public const int MAX_LAYER_NUMBER = 1000;

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
