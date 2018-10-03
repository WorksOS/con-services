using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  public class TemperatureDetailsRequest : ProjectID
  {
    /// <summary>
    /// The collection of temperature targets in °C. Values are in ascending order.
    /// There must be 5 values and the first value must be 0.
    /// </summary>
    public double[] Targets { get; private set; }
    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    public LiftBuildSettings LiftBuildSettings { get; private set; }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public TemperatureDetailsRequest(long projectId, double[] targets, FilterResult filter, LiftBuildSettings liftBuildSettings)
    {
      ProjectId = projectId;
      Targets = targets;
      Filter = filter;
      LiftBuildSettings = liftBuildSettings;
    }

    /// <summary>
    /// Validates the request and throws if validation fails.
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter?.Validate();

      LiftBuildSettings?.Validate();
    }

  }
}
