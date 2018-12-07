namespace VSS.TRex.Profiling.Models
{
  public class ProfileCellStateBase
  {
    /// <summary>
    /// The real-world distance from the 'start' of the profile line drawn by the user;
    /// this is used to ensure that the client GUI correctly aligns the profile
    /// information drawn in the Long Section view with the profile line on the Plan View.
    /// </summary>
    public double Station;

    /// <summary>
    /// The real-world length of that part of the profile line which crosses the underlying cell;
    /// used to determine the width of the profile column as displayed in the client GUI
    /// </summary>
    public double InterceptLength;

    /// <summary>
    /// OTGCellX, OTGCellY is the on the ground index of the this particular grid cell
    /// </summary>
    public uint OTGCellX;

    /// <summary>
    /// OTGCellX, OTGCellY is the on the ground index of the this particular grid cell
    /// </summary>
    public uint OTGCellY;

    public float DesignElev;
  }
}
