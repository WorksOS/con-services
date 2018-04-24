namespace RaptorSvcAcceptTestsCommon.Models
{
  /// <summary>
  /// Defines how the material thickness of a target layer thickness is to be treated in terms of it being compacted or uncompacted material
  /// This is copied from ...\RaptorServicesCommon\Models\LiftThicknessType.cs 
  /// </summary>
  public enum LiftThicknessType
  {
    /// <summary>
    /// The material thickness is uncompacted material
    /// </summary>
    Uncompacted = 0,

    /// <summary>
    /// The material thickness is compacted material
    /// </summary>
    Compacted = 1
  }
}