namespace VSS.Productivity3D.Common.Interfaces
{
  /// <summary>
  /// Defines if a domain object can have business validation
  /// </summary>
  public interface IValidatable
  {
    /// <summary>
    /// Validate domain object. If validation is not successful throw an exception. />
    /// </summary>
    void Validate();
  }
}
