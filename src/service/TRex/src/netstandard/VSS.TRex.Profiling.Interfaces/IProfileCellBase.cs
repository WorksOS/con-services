using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.Profiling.Interfaces
{
  public interface IProfileCellBase : IFromToBinary
  {
    /// <summary>
    /// OTGCellX, OTGCellY is the on the ground index of the this particular grid cell
    /// </summary>
    uint OTGCellX { get; set; }

    /// <summary>
    /// OTGCellX, OTGCellY is the on the ground index of the this particular grid cell
    /// </summary>
    uint OTGCellY { get; set; }

    /// <summary>
    /// The real-world distance from the 'start' of the profile line drawn by the user;
    /// this is used to ensure that the client GUI correctly aligns the profile
    /// information drawn in the Long Section view with the profile line on the Plan View.
    /// </summary>
    double Station { get; set; }

    double InterceptLength { get; set; }

    float DesignElev { get; set; }

    bool IsNull();
  }
}
